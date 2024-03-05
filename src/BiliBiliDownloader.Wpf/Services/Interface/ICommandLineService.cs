namespace BiliBiliDownloader.Wpf.Services.Interface
{
	public interface ICommandLineService
	{
		public Task MergeFile(
			string output,
			string videoPath,
			string? audioPath = null,
			string? subtitlePath = null,
			bool deleteVideo = false,
			bool deleteAudio = false,
			bool deleteSubtitle = false
			);
	}
}
