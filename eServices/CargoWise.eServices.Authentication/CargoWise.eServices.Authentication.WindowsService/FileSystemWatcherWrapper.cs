using System.IO;

namespace CargoWise.eServices.Authentication.WindowsService
{
	public interface IFileSystemWatcherWrapper
	{
		event FileSystemEventHandler Changed;
		event RenamedEventHandler Renamed;
		bool EnableRaisingEvents { get; set; }
		string Filter { get; set; }
		string Path { get; set; }
	}

	public class FileSystemWatcherWrapper : FileSystemWatcher, IFileSystemWatcherWrapper
	{
	}
}
