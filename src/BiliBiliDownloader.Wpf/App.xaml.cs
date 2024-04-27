using BiliBiliDownloader.Wpf.Common;
using BiliBiliDownloader.Wpf.Services.Implement;
using BiliBiliDownloader.Wpf.Services.Interface;
using BiliBiliDownloader.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Windows;


namespace BiliBiliDownloader.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            var serviceProvider = ConfigureService();

            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();

            try
            {
                mainWindow.Show();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private IServiceProvider ConfigureService()
        {
            IServiceCollection service = new ServiceCollection();
            service.AddScoped<MainWindow>();
            service.AddScoped(_ => new HttpClient()
            {
                BaseAddress = new Uri(ApiConstants.BaseUri)
            });


            service.AddScoped<ISubtitleService, SubtitleService>();
            service.AddScoped<IEpisodeService, EpisodeService>();
            service.AddScoped<ICommandLineService, CommandLineService>();
            service.AddScoped<ISerieService, SerieService>();

            using var stream = new FileStream("Cookies/cookies.txt", FileMode.Open);

            using var reader = new StreamReader(stream, Encoding.ASCII);
            var cookies = reader.ReadToEnd();

            service.AddScoped(_ => InitCookieContainer(cookies));

            service.AddScoped(serviceProvider =>
            {
                var cookieContainer = serviceProvider.GetService<CookieContainer>();
                if (cookieContainer == null)
                    return new HttpClient()
                    {
                        BaseAddress = new Uri(ApiConstants.BaseUri)
                    };
                return InitHttpClient(cookieContainer);
            });

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/downloads.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();


            service.AddLogging(x =>
            {
                x.ClearProviders();
            });
            service.AddSingleton(Log.Logger);

            return service.BuildServiceProvider();
        }

        private static HttpClient InitHttpClient(CookieContainer cookieContainer)
        {

            var httpClient = new HttpClient(new HttpClientHandler()
            {
                CookieContainer = cookieContainer,
                UseCookies = true,

            });
            httpClient.BaseAddress = new Uri(ApiConstants.BaseUri);
            return httpClient;
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
    }

}
