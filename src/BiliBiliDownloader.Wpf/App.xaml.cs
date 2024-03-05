using System.Net;
using System.Net.Http;
using BiliBiliDownloader.Wpf.Common;
using BiliBiliDownloader.Wpf.Services.Implement;
using BiliBiliDownloader.Wpf.Services.Interface;
using BiliBiliDownloader.Wpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;


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

			var mainWindow = serviceProvider.GetService<MainWindow>();

			mainWindow!.Show();
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

			service.AddScoped(_ => InitCookieContainer());

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

		private static CookieContainer InitCookieContainer()
		{
			CookieContainer cookieContainer = new();
			var jwt =
				"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJSZXEiOnsiaXAiOiIxMDQuMjguMjU0Ljc0IiwiYXBwX3JlZ2lvbiI6IlZOIiwiaG9zdCI6ImFwaS5iaWxpYmlsaS50diIsImFwcF92ZXJzaW9uIjoiMCIsInBsYXRmb3JtIjoid2ViIn0sIlJlZ2lvbkluZm8iOnsicmVnaW9uX2lkIjo1ODcyMDI1NiwicmVnaW9uX2VuIjoiVk4iLCJMYXRpdHVkZSI6MjEuMDI0NDEwMjQ3ODAyNzM0LCJMb25naXR1ZGUiOjEwNS44NDE0NjExODE2NDA2MiwiaXBfY291bnRyeSI6Iui2iuWNlyIsImNvdW50cnlfY29kZSI6ODQsInByb3ZpbmNlIjoi5rKz5YaFIn0sImV4cCI6MTcwOTA0MTAxNX0.0nimwC1_hCYmKqNS0xIAht-tE0KLFGW2B-hI-Qcytzc";


			var uri = new Uri(ApiConstants.BaseUri);
			cookieContainer.Add(uri, new Cookie("DedeUserID", "1409227328"));
			cookieContainer.Add(uri, new Cookie("DedeUserID__ckMd5", "da9859b1cbd67d407b47ffbea09cd5d9"));
			cookieContainer.Add(uri, new Cookie("SESSDATA", "c930c788%2C1717927037%2C4dd08%2Ac10040"));
			cookieContainer.Add(uri, new Cookie("bili_jct", "7adffb1192b7e05ab1d540887fcda7de"));
			cookieContainer.Add(uri, new Cookie("bstar-bstar_c_locale-lang", "vi"));
			cookieContainer.Add(uri, new Cookie("bstar_c_locale", "vi"));
			cookieContainer.Add(uri, new Cookie("bstar_s_locale", "vi"));
			cookieContainer.Add(uri, new Cookie("buvid3", "c174cef1-6791-4ff4-a46f-65f14590faa022966infoc"));
			cookieContainer.Add(uri, new Cookie("joy_jct", "7adffb1192b7e05ab1d540887fcda7de"));
			cookieContainer.Add(uri, new Cookie("mid", "1409227328"));
			cookieContainer.Add(uri, new Cookie("regionforbid", jwt));

			return cookieContainer;
		}
	}

}
