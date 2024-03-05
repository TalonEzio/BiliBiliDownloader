namespace BiliBiliDownloader.Wpf.Models.Services
{
	public class EpisodeSaveInfo
	{
		public DateTime From { get; set; }
		public DateTime To { get; set; }
		public List<AudioSaveInfo> AudioFiles { get; set; } = [];
		public List<VideoSaveInfo> VideoFiles { get; set; } = [];
	}
}
