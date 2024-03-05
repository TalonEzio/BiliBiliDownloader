using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using BiliBiliDownloader.Wpf.Models.Response;
using BiliBiliDownloader.Wpf.Models.Services;
using BiliBiliDownloader.Wpf.Services.Interface;
using SlugGenerator;

namespace BiliBiliDownloader.Wpf.Services.Implement
{
	public class SubtitleService(HttpClient httpClient, CookieContainer cookieContainer) : ISubtitleService
	{
		public async Task<SubtitleSaveInfo> ConvertJsonToSrt(string outputPath, string fileName, SubtitleJsonResponse subtitle)
		{
			var body = subtitle.Body;
			var savePath = Path.Combine(outputPath, fileName);
			await using var fileStream = new FileStream(savePath, FileMode.Create);

			await using var writer = new StreamWriter(fileStream, Encoding.Unicode);
			var count = 0;
			foreach (var line in body)
			{
				await writer.WriteLineAsync(GenerateLine(count, line.From, line.To, line.Content));
				count++;
			}

			return new SubtitleSaveInfo()
			{
				SavePath = savePath,
				Name = fileName
			};
		}

		private static string GenerateLine(int index, double from, double to, string content)
		{
			var builder = new StringBuilder();

			builder.AppendLine(index.ToString());

			builder.Append($"{FormatTime(from)}");
			builder.Append(" --> ");
			builder.Append($"{FormatTime(to)}\n");

			builder.AppendLine(content);

			return builder.ToString();
		}

		private static string FormatTime(double seconds)
		{
			var time = TimeSpan.FromSeconds(seconds);
			return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2},{time.Milliseconds:D3}";
		}

		public async Task<SubtitleResponse?> GetSubtitle(long episodeId)
		{
			var responseSubtitle =
				await httpClient.GetAsync($"v2/subtitle?s_locale=vi_VN&platform=web&episode_id={episodeId}");

			if (responseSubtitle.StatusCode != HttpStatusCode.OK) return null;

			var streamSubtitle = await responseSubtitle.Content.ReadAsStreamAsync();

			var responseObject = await JsonSerializer.DeserializeAsync<SubtitleResponse>(streamSubtitle);
			return responseObject;
		}

		public async Task<SubtitleSaveInfo?> SaveSubtitleFile(string outputPath, string fileName, SubtitleResponse subtitleResponse)
		{
			try
			{
				var subtitleUrl = subtitleResponse.SubtitleData.VideoSubtitles
					.FirstOrDefault(x => x.Srt.Url.Length > 0 || x.Ass.Url.Length > 0 && x.LangKey == "vi");

				if (subtitleUrl == null) return null;

				var url = subtitleUrl.Srt.Url;

				var isAss = false;

				if (string.IsNullOrEmpty(url))
				{
					url = subtitleUrl.Ass.Url;
					isAss = true;
				}

				var subtitleUri = new Uri(subtitleUrl.Srt.Url);

				var handler = new HttpClientHandler()
				{
					CookieContainer = cookieContainer,
					UseCookies = true
				};
				using var subtitleHttpClient = new HttpClient(handler);

				subtitleHttpClient.BaseAddress = subtitleUri;

				var subtitleSourceResponse = await subtitleHttpClient.GetAsync("");
				var subtitleJsonStreamAsync = await subtitleSourceResponse.Content.ReadAsStreamAsync();

				if (isAss)
				{
					var assFilePath = Path.Combine(outputPath, (fileName + ".ass"));
					await using var fileStream =
						new FileStream(assFilePath, FileMode.Create);
					await subtitleJsonStreamAsync.CopyToAsync(fileStream);
					return new SubtitleSaveInfo()
					{
						SavePath = assFilePath,
						Name = fileName
					};
				}

				var subtitleJson = await JsonSerializer.DeserializeAsync<SubtitleJsonResponse>(subtitleJsonStreamAsync);

				if (subtitleJson == null || !subtitleJson.Body.Any()) return null;

				return await ConvertJsonToSrt(outputPath, fileName +".srt", subtitleJson);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

		}
	}
}
