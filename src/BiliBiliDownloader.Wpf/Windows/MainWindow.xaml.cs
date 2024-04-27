using BiliBiliDownloader.Wpf.Common;
using BiliBiliDownloader.Wpf.Models.Response;
using BiliBiliDownloader.Wpf.Models.Services;
using BiliBiliDownloader.Wpf.Services.Implement;
using BiliBiliDownloader.Wpf.Services.Interface;
using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Text;
using System.Text.RegularExpressions;

namespace BiliBiliDownloader.Wpf.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private static readonly string BasePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!;

        private readonly ISerieService _serieService;

        public MainWindow(ISerieService serieService)
        {
            _serieService = serieService;

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
                    outputPath = Path.Combine(outputPath, serieId + " - " + serieName);
                    Directory.CreateDirectory(outputPath);
                }
            }

            await using var stream = new FileStream(Path.Combine(BasePath, "Cookies,cookies.txt"), FileMode.Open);

            using var reader = new StreamReader(stream, Encoding.ASCII);
            var cookies = await reader.ReadToEndAsync();

            var cookieContainer = InitCookieContainer(cookies);

            if (!string.IsNullOrEmpty(TxtEpisodeId.Text))
            {

                var episodeList = TxtEpisodeId.Text.Split(',');
                using var httpClient = GetHttpClient(cookieContainer);

                foreach (var episodeIdStr in episodeList)
                {
                    var episodeId = long.Parse(episodeIdStr);

                    var findEpisode = FindEpisodeInSerie(serie, episodeId);
                    if (findEpisode == null)
                    {
                        MessageBox.Show(this, "Không tồn tại tập trong bộ này", "Lỗi");
                        EnableControls = true;
                        return;
                    }

                    try
                    {
                        await DownloadEpisode(httpClient, cookieContainer, episodeId, findEpisode.TitleDisplay,
                            outputPath, serieName);
                        AddMessage($"Tải thành công {findEpisode.TitleDisplay}");
                    }
                    catch (Exception exception)
                    {
                        AddMessage(exception.Message);
                    }
                }
                EnableControls = true;

                return;
            }

            var taskList = new List<Task>();
            TxtResult.Text += "Chuẩn bị xử lý\n";
            var i = 1;
            foreach (var section in serie.Data.Sections)
            {
                string newOutputPath = outputPath;

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

                var task = ActionAsync();
                taskList.Add(task);
                continue;

                async Task ActionAsync()
                {
                    using var httpClient = GetHttpClient(cookieContainer);


                    foreach (var episode in section.Episodes)
                    {
                        var episodeId = long.Parse(episode.EpisodeId);

                        var findEpisode = FindEpisodeInSerie(serie, episodeId);
                        if (findEpisode == null)
                        {
                            MessageBox.Show(this, "Không tồn tại tập trong bộ này", "Lỗi");
                            return;
                        }
                        try
                        {
                            var result = await DownloadEpisode(httpClient, cookieContainer, episodeId,
                                findEpisode.TitleDisplay, newOutputPath, serieName);
                            if (File.Exists(result))
                            {
                                AddMessage($"Tải thành công {episode.TitleDisplay}");
                            }
                        }
                        catch (Exception exception)
                        {
                            AddMessage(exception.Message);
                        }
                    }
                }
            }

            await Task.WhenAll(taskList);

            AddMessage("Xử lý hoàn tất\n");

            EnableControls = true;
        }

        private static HttpClient GetHttpClient(CookieContainer cookieContainer)
        {

            var httpClient = new HttpClient(new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
                UseCookies = true,

            });
            httpClient.BaseAddress = new Uri(ApiConstants.BaseUri);
            return httpClient;
        }

        private Episode? FindEpisodeInSerie(SerieResponse serie, long episodeId)
        {
            return serie.Data.Sections
                .Select(serieSection =>
                    serieSection.Episodes.FirstOrDefault(x =>
                        x.EpisodeId.Equals(episodeId.ToString())))
                .OfType<Episode>().FirstOrDefault();
        }
        private static CookieContainer InitCookieContainer(string cookies)
        {
            CookieContainer cookieContainer = new();

            var uri = new Uri(ApiConstants.BaseUri);

            var cookieItems = cookies.Split(';');
            foreach (var item in cookieItems)
            {
                var itemSplit = item.Split("=", 2);
                cookieContainer.Add(uri, new Cookie(itemSplit[0].Trim(), itemSplit[1].Trim()));

            }

            return cookieContainer;
        }

        private static string SanitizeFolderName(string inputFolder)
        {
            var invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var regexString = $"[{Regex.Escape(invalidChars)}]";
            var sanitizedFolderName = Regex.Replace(inputFolder, regexString, " ");
            return sanitizedFolderName;
        }
        private static async Task<string> DownloadEpisode(HttpClient httpClient, CookieContainer cookieContainer,
            long episodeId, string episodeName, string outputPath, string serieName)
        {
            episodeName = SanitizeFolderName(episodeName);

            serieName = SanitizeFolderName(serieName);

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            var fileName = SanitizeFolderName(episodeName.Split(" - ")[0]);

            var output = Path.Combine(outputPath, serieName + " - " + fileName + ".mkv");


            var episodeService = new EpisodeService(httpClient);
            var subtitleService = new SubtitleService(httpClient, cookieContainer);
            var commandLineService = new CommandLineService();

            var episode = await episodeService.GetEpisode(episodeId);

            if (episode is { EpisodeData: null })
            {
                throw new Exception($"Không đọc được thông tin {episodeName}, xem lại cookie");
            }

            episode!.Name = fileName;

            var subtitle = await subtitleService.GetSubtitle(episodeId);

            if (subtitle == null || !subtitle.SubtitleData.VideoSubtitles.Any() || !subtitle.SubtitleData.Subtitles.Any())
            {
                throw new Exception($"{serieName} - {fileName}: Không có phụ đề tiếng Việt");
            }
            var subtitleFile =
                await subtitleService.SaveSubtitleFile(outputPath, episode.Name, subtitle)
                    ?? throw new Exception($"{serieName} - {fileName}: Lỗi lưu phụ đề!");

            episode.EpisodeData.Playurl.Audios = [episode.EpisodeData.Playurl.Audios.OrderByDescending(x => x.Quality).First()];

            episode.EpisodeData.Playurl.Videos = episode.EpisodeData.Playurl.Videos
                .Where(x => x.StreamInfo.Quality >= VideoQuality._480P && !string.IsNullOrEmpty(x.VideoResource.Url))
                .OrderByDescending(x => x.StreamInfo.Quality == VideoQuality._1080P)
                .ThenByDescending(x => x.StreamInfo.Quality == VideoQuality._1080P_HD)
                .ThenByDescending(x => x.StreamInfo.Quality == VideoQuality._720P)
                .ThenByDescending(x => x.StreamInfo.Quality == VideoQuality._480P)
                .ThenByDescending(x => x.VideoResource.Bandwidth)
                .ThenByDescending(x => x.StreamInfo.Quality)
                .ToList();

            var savedVideo = false;
            var result = new EpisodeSaveInfo();
            var videos = episode.EpisodeData.Playurl.Videos;

            do
            {
                try
                {
                    result = await episodeService
                        .SaveEpisode(episode, outputPath);
                    savedVideo = true;
                }
                catch (Exception)
                {
                    await Task.Delay(100);
                    if (videos.Any())
                    {
                        var first = videos.First();
                        if (!string.IsNullOrEmpty(first.VideoResource.BackupUrl))
                        {
                            first.VideoResource.Url = first.VideoResource.BackupUrl;
                        }
                        else
                        {
                            videos.Remove(videos.First());
                        }
                    }
                    else
                    {
                        throw new Exception($"Không tải được tất cả link từ {episodeName}");
                    }
                }
            } while (!savedVideo);


            var videoPath = result.VideoFiles.First().SavePath;
            var audioPath = result.AudioFiles.First().SavePath;
            var subtitlePath = subtitleFile.SavePath;

            await commandLineService.MergeFile(output, videoPath, audioPath, subtitlePath,
                deleteVideo: true,
                deleteAudio: true,
                deleteSubtitle: true);

            File.Delete(videoPath);
            File.Delete(audioPath);
            File.Delete(subtitlePath);

            return output;
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
                if (!text.EndsWith("\n")) text += '\n';
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {

            var processStart = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = Path.Combine(BasePath, "Cookies", "cookies.txt")
            };

            var process = new Process()
            {
                StartInfo = processStart
            };
            process.Start();
        }
    }
}
