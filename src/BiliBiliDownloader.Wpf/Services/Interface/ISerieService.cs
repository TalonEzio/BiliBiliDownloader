using BiliBiliDownloader.Wpf.Models.Response;

namespace BiliBiliDownloader.Wpf.Services.Interface
{
	public interface ISerieService
	{
		Task<SerieResponse?> GetSerie(long serieId);
		Task<string> GetSerieName(long serieId);
	}
}
