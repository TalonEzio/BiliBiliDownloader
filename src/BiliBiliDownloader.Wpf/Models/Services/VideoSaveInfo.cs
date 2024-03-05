using BiliBiliDownloader.Wpf.Common;

namespace BiliBiliDownloader.Wpf.Models.Services
{
	public class VideoSaveInfo
	{
		public string Name { get; set; } = string.Empty;
		public string SavePath { get; set; } = string.Empty;
		public VideoQuality Quality { get; set; }

	}
}
