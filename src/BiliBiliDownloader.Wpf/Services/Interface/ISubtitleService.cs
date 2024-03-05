using BiliBiliDownloader.Wpf.Models.Response;
using BiliBiliDownloader.Wpf.Models.Services;

namespace BiliBiliDownloader.Wpf.Services.Interface
{
	public interface ISubtitleService
	{
		public Task<SubtitleSaveInfo> ConvertJsonToSrt(string outputPath,string fileName, SubtitleJsonResponse subtitle);
		public Task<SubtitleResponse?> GetSubtitle(long episodeId);
		public Task<SubtitleSaveInfo?> SaveSubtitleFile(string outputPath, string fileName,SubtitleResponse subtitleResponse);

	}
}
