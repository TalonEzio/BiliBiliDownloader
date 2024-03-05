using BiliBiliDownloader.Wpf.Services.Interface;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BiliBiliDownloader.Wpf.Services.Implement
{
	public class CommandLineService : ICommandLineService
	{
		public async Task MergeFile(string output, string videoPath, string? audioPath = null, string? subtitlePath = null,
			bool deleteVideo = false, bool deleteAudio = false, bool deleteSubtitle = false)
		{
			var cmdBuilder = new StringBuilder();

			cmdBuilder.Append($"-o \"{output}\"");

			cmdBuilder.Append($" \"{videoPath}\"");

			if (!string.IsNullOrEmpty(audioPath)) cmdBuilder.Append($" \"{audioPath}\"");
			if (!string.IsNullOrEmpty(subtitlePath)) cmdBuilder.Append($" \"{subtitlePath}\"");

			var processStart = new ProcessStartInfo
			{
				FileName = "Tools/mkvmerge",
				RedirectStandardInput = true,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				StandardInputEncoding = Encoding.Unicode,
				Arguments = cmdBuilder.ToString(),
				CreateNoWindow = true
			};

			var process = new Process();

			process.StartInfo = processStart;
			process.Start();
			await process.WaitForExitAsync();

			if (deleteVideo && !string.IsNullOrEmpty(videoPath)) File.Delete(videoPath);
			if (deleteAudio && !string.IsNullOrEmpty(audioPath)) File.Delete(audioPath);
			if (deleteSubtitle && !string.IsNullOrEmpty(subtitlePath)) File.Delete(subtitlePath);

			//var newName = Path.GetFileNameWithoutExtension(output) +"-saved" + Path.GetExtension(output);
			//var newNamePath = Path.Combine(Path.GetDirectoryName(output)!, newName);
			//if (output != newNamePath)
			//{
			//	File.Move(output, newNamePath);
			//}
		}
	}
}
