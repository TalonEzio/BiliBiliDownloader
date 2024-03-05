using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using BiliBiliDownloader.Wpf.Common;
using BiliBiliDownloader.Wpf.Models.Response;
using BiliBiliDownloader.Wpf.Services.Interface;
using Microsoft.Win32;
using SlugGenerator;

namespace BiliBiliDownloader.Wpf.Windows
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : INotifyPropertyChanged
	{

		private readonly ISubtitleService _subtitleService;
		private readonly IEpisodeService _episodeService;
		private readonly ICommandLineService _commandLineService;
		private readonly ISerieService _serieService;
		private readonly Serilog.ILogger _logger;

		public MainWindow(ISubtitleService subtitleService, IEpisodeService episodeService,
			ICommandLineService commandLineService, ISerieService serieService, Serilog.ILogger logger)
		{
			_subtitleService = subtitleService;
			_episodeService = episodeService;
			_commandLineService = commandLineService;
			_serieService = serieService;
			_logger = logger;

			InitializeComponent();

			DataContext = this;

		}

		private bool _enableControls = true;

		public bool EnableControls
		{
			get => _enableControls;
			set => SetField(ref _enableControls, value);
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value)) return;
			field = value;
			OnPropertyChanged(propertyName);
		}

		private async void BtnDownload_OnClick(object sender, RoutedEventArgs e)
		{

			var outputPath = TxtOutputPath.Text;

			if (string.IsNullOrEmpty(outputPath))
			{
				MessageBox.Show(this, "Mời nhập đường dẫn trước", "Lỗi");
				return;
			}
			if (string.IsNullOrEmpty(outputPath))
			{
				MessageBox.Show(this, "Mời nhập đường dẫn trước", "Lỗi");
				return;
			}


			if (!long.TryParse(TxtSerieId.Text, out var serieId))
			{
				MessageBox.Show("SerieId lỗi, nhập lại");
				return;
			}

			var serie = await _serieService.GetSerie(serieId);

			if (serie == null || serie.Code < 0)
			{
				MessageBox.Show(this, "Không tồn tại bộ này", "Lỗi");
				return;
			}

			EnableControls = false;
			TxtResult.Text = "";
			var autoCreateFolder = CkbAutoCreateFolder.IsChecked ?? false;
			var autoCrateSeason = CkbAutoCreateSeason.IsChecked ?? false;

			if (!Directory.Exists(outputPath))
			{
				Directory.CreateDirectory(outputPath);
			}

			var serieName = string.Empty;
			if (autoCreateFolder)
			{
				serieName = await _serieService.GetSerieName(serieId);

				if (!Directory.Exists(serieName))
				{
					serieName = serieName.GenerateSlug();
					outputPath = Path.Combine(outputPath, serieId + " - " + serieName);
					Directory.CreateDirectory(outputPath);
				}
			}



			if (!string.IsNullOrEmpty(TxtEpisodeId.Text))
			{
				var episodeId = long.Parse(TxtEpisodeId.Text);

				var findEpisode = FindEpisodeInSerie(serie, episodeId);
				if (findEpisode == null)
				{
					MessageBox.Show(this, "Không tồn tại tập trong bộ này", "Lỗi");
					return;
				}

				await DownloadEpisode(episodeId, findEpisode.TitleDisplay, outputPath, serieId, serieName);
				EnableControls = true;
				return;
			}

			var taskList = new List<Task>();
			TxtResult.Text += "Chuẩn bị xử lý\n";
			var i = 1;
			foreach (var section in serie.Data.Sections)
			{
				var newOutputPath = outputPath;

				if (autoCrateSeason)
				{
					var folderName = $"{i++.ToString().PadLeft(3, '0')}";
					if (!string.IsNullOrEmpty(section.Title))
					{
						folderName += $" - {section.Title}";
					}
					newOutputPath = Path.Combine(outputPath, folderName);

					if (!Directory.Exists(newOutputPath))
					{
						Directory.CreateDirectory(newOutputPath);
					}
				}

				async Task ActionAsync()
				{

					foreach (var episode in section.Episodes)
					{
						await DownloadEpisode(long.Parse(episode.EpisodeId), episode.TitleDisplay.GenerateSlug(), newOutputPath, serieId, serieName);
					}
				}

				Task task = ActionAsync();
				taskList.Add(task);
			}

			await Task.WhenAll(taskList);

			AddMessage("Xử lý bộ hoàn tất\n");

			EnableControls = true;
		}

		private Episode? FindEpisodeInSerie(SerieResponse serie, long episodeId)
		{
			return serie.Data.Sections
				.Select(serieSection =>
					serieSection.Episodes.FirstOrDefault(x =>
						x.EpisodeId.Equals(episodeId.ToString())))
				.OfType<Episode>().FirstOrDefault();
		}

		private async Task DownloadEpisode(long episodeId, string episodeName, string outputPath, long serieId, string serieName)
		{
			var episode = await _episodeService.GetEpisode(episodeId);

			if (episode == null)
			{
				MessageBox.Show(this, "Không tìm được tập phim", "Lỗi");
				return;
			}

			AddMessage($"Bắt đầu xử lý: {episodeName}\n");

			episode.Name = episodeName;

			var subtitle = await _subtitleService.GetSubtitle(episodeId);
			if (subtitle?.SubtitleData.Subtitles == null || subtitle.Code < 0)
			{
				_logger.Error($"{serieId} - {serieName}: {episodeId} - {episodeName} không tải được sub!");
				AddMessage($"{serieId} - {serieName}: {episodeId} - {episodeName} không tải được sub!");
				return;
			}


			var subtitleFile = await _subtitleService.SaveSubtitleFile(outputPath, episodeName, subtitle);

			if (subtitleFile == null || string.IsNullOrEmpty(subtitleFile.SavePath))
			{
				AddMessage($"{serieId} - {serieName}: {episodeId} - {episodeName} không có phụ đề tiếng Việt, bỏ qua!");
				_logger.Error($"{serieId} - {serieName}: {episodeId} - {episodeName} không có phụ đề tiếng Việt, bỏ qua!");
				return;
			}


			episode.EpisodeData.Playurl.Audios = [episode.EpisodeData.Playurl.Audios.First()];

			try
			{
				var result = await _episodeService
					.SaveEpisode(episode, outputPath,
						video => video.StreamInfo.Quality == VideoQuality._720P
					);
				var output = Path.Combine(outputPath, episode.Name + ".mkv");

				var videoPath = result.VideoFiles.First().SavePath;
				var audioPath = result.AudioFiles.FirstOrDefault()?.SavePath ?? string.Empty;
				var subtitlePath = subtitleFile.SavePath;

				await _commandLineService.MergeFile(output, videoPath, audioPath, subtitlePath,
					deleteVideo: CkbDeleteVideo.IsChecked ?? false,
					deleteAudio: CkbDeleteAudio.IsChecked ?? false,
					deleteSubtitle: CkbDeleteSub.IsChecked ?? false);

				AddMessage($"Xử lý xong {episodeName}\n");

				_logger.Information($"{serieId} - {serieName}: {episodeId} - {episodeName} xử lý hoàn tất!");
			}
			catch (Exception e)
			{
				_logger.Error($"{serieId} - {serieName}: Có lỗi khi tải {episodeId} - {episodeName}: {e.Message}");
			}
		}
		private void BtnSelectOutputPath_OnClick(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFolderDialog();

			dialog.ShowDialog();

			TxtOutputPath.Text = dialog.FolderName;
		}

		private void AddMessage(string text)
		{
			Dispatcher.Invoke(() =>
			{
				TxtResult.Text += text;
				TxtResult.Focus();
				TxtResult.CaretIndex = TxtResult.Text.Length;
				TxtResult.ScrollToEnd();
			});
		}


		private void MenuItem_OnClick(object sender, RoutedEventArgs e)
		{
			var path = Path.Combine(Environment.CurrentDirectory, "logs");

			if (File.Exists(path))
			{
				Process.Start("explorer", path);

			}
		}
	}
}
