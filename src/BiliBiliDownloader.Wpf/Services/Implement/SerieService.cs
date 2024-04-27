using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using BiliBiliDownloader.Wpf.Models.Response;
using BiliBiliDownloader.Wpf.Services.Interface;

namespace BiliBiliDownloader.Wpf.Services.Implement
{
    internal class SerieService(HttpClient httpClient, CookieContainer cookie) : ISerieService
    {
        public async Task<SerieResponse?> GetSerie(long serieId)
        {
            try
            {
                var response =
                    await httpClient.GetAsync($"v2/ogv/play/episodes?s_locale=vi_VN&platform=web&season_id={serieId}");
                await using var stream = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<SerieResponse>(stream);
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GetSerieName(long serieId)
        {
            var serieHttpClient = new HttpClient(new HttpClientHandler()
            {
                CookieContainer = cookie,
                UseCookies = true
            })
            {
                BaseAddress = new Uri("https://www.bilibili.tv/vi/media/")
            };

            var response = await serieHttpClient.GetAsync(serieId.ToString());

            var responseString = await response.Content.ReadAsStringAsync();
            string pattern = @"<h1[^>]*>(.*?)<\/h1>";

            var regex = new Regex(pattern);

            var match = regex.Match(responseString);

            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
