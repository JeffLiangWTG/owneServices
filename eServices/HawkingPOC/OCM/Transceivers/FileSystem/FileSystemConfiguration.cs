using System.IO;

namespace OcmPoc.Transceivers.FileSystem
{
	public class FileSystemConfiguration : ConnectionConfigBase
	{
		public string FileNamePattern { get; set; }
		public string SendDirectory { get; set; } = "/transfers/send";
		public string ReceiveDirectory { get; set; } = "/transfers/receive";

		public DirectoryInfo SendDirectoryInfo => new DirectoryInfo(SendDirectory);
		public DirectoryInfo ReceiveDirectoryInfo => new DirectoryInfo(ReceiveDirectory);
	}
}
