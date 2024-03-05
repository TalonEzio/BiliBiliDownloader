using BiliBiliDownloader.Wpf.Models.Response;
using BiliBiliDownloader.Wpf.Services.Interface;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using BiliBiliDownloader.Wpf.Models.Services;
using SlugGenerator;

namespace BiliBiliDownloader.Wpf.Services.Implement
{
	public class EpisodeService(HttpClient httpClient) : IEpisodeService
	{
		public async Task<EpisodeSaveInfo> SaveEpisode(EpisodeResponse episode, string savePath, Predicate<Video>? videoPredicate = null,
			Predicate<Audio>? audioPredicate = null)
		{
			var result = new EpisodeSaveInfo
			{
				From = DateTime.Now,
				AudioFiles = await SaveAudioEpisode(episode, savePath, audioPredicate),
				VideoFiles = await SaveVideoEpisode(episode, savePath, videoPredicate),
				To = DateTime.Now
			};

			return result;
		}
		public async Task<EpisodeResponse?> GetEpisode(long episodeId)
		{
			var responseVideo =
				await httpClient.GetAsync($"playurl?ep_id={episodeId}&platform=web&device=wap&qn=64&tf=0&type=0");

			var streamVideo = await responseVideo.Content.ReadAsStreamAsync();
			var videoResponse = await JsonSerializer.DeserializeAsync<EpisodeResponse>(streamVideo);

			return videoResponse;
		}


		public async Task<string> GetEpisodeName(long episodeId, long serieId)
		{
			var response =
				await httpClient.GetAsync($"v2/ogv/play/episodes?s_locale=vi_VN&platform=web&season_id={serieId}");

			await using var stream = await response.Content.ReadAsStreamAsync();

			var result = JsonSerializer.Deserialize<SerieResponse>(stream);

			var name = string.Empty;
			if (result == null || !result.Data.Sections.Any()) return string.Empty;
			foreach (var section in result.Data.Sections)
			{
				var find = section.Episodes.FirstOrDefault(x => x.EpisodeId == episodeId.ToString());
				if (find == null) continue;

				name = find.TitleDisplay;
				break;
			}

			return name;
		}

		public async Task<List<VideoSaveInfo>> SaveVideoEpisode(EpisodeResponse episode, string savePath, Predicate<Video>? predicate = null)
		{
			var videoName = episode.Name ?? Guid.NewGuid().ToString();

			var saveVideos = episode.EpisodeData.Playurl.Videos.Where(x => !string.IsNullOrEmpty(x.VideoResource.Url));

			if (predicate != null)
			{
				saveVideos = saveVideos.Where(x => predicate(x)).ToList();
			}

			var videoSaveFiles = new List<VideoSaveInfo>();


			foreach (var video in saveVideos)
			{
				var videoFileName = videoName + "-" + video.StreamInfo.DescWords + ".mp4";
				var videoFilePath = Path.Combine(savePath, videoFileName);

				using var httpClientVideo = new HttpClient();
				httpClientVideo.BaseAddress = new Uri(video.VideoResource.Url);

				await using var fileStream = new FileStream(videoFilePath, FileMode.Create);

				var videoStream = await httpClientVideo.GetAsync("");

				var sourceStream = await videoStream.Content.ReadAsStreamAsync();

				await sourceStream.CopyToAsync(fileStream);

				videoSaveFiles.Add(new VideoSaveInfo()
				{
					Name = videoFileName,
					Quality = video.StreamInfo.Quality,
					SavePath = videoFilePath
				});
			}

			return videoSaveFiles;
		}
		public async Task<List<AudioSaveInfo>> SaveAudioEpisode(EpisodeResponse episode, string savePath, Predicate<Audio>? predicate = null)
		{
			var audios = episode.EpisodeData.Playurl.Audios.OrderByDescending(x => x.Quality).ToList();
			var audioName = episode.Name ?? Guid.NewGuid().ToString();
			if (predicate != null)
			{
				audios = audios.Where(x => predicate(x)).ToList();
			}

			var audioSaveFiles = new List<AudioSaveInfo>();
			var count = 0;
			foreach (var audio in audios)
			{

				var fileName = $"{audioName}-{count++}.aac";
				var audioFilePath = Path.Combine(savePath, fileName);

				using var handlerAudio = new HttpClientHandler();

				using var clientAudio = new HttpClient(handlerAudio);

				clientAudio.BaseAddress = new Uri(audio.Url);

				await using var fileStreamA = new FileStream(audioFilePath, FileMode.Create);

				var audioStream = await clientAudio.GetAsync("");

				var audioSourceStream = await audioStream.Content.ReadAsStreamAsync();

				await audioSourceStream.CopyToAsync(fileStreamA);

				audioSaveFiles.Add(new AudioSaveInfo()
				{
					Name = fileName,
					Quality = audio.Quality,
					SavePath = audioFilePath
				});

			}

			return audioSaveFiles;
		}


	}
}
