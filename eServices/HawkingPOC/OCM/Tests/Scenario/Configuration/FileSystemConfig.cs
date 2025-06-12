using System.IO;

namespace OcmPoc.Tests.Scenario.Configuration
{
	public class FileSystemConfig
	{
		public string RootPath { get; set; }
		public string SendFolder { get; set; } = "send";
		public string ReceiveFolder { get; set; } = "receive";

		public string SendPath => Path.Combine(RootPath ?? "", SendFolder);
		public string ReceivePath => Path.Combine(RootPath ?? "", ReceiveFolder);
	}
}