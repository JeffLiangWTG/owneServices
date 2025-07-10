using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Interfaces;

namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher.Configuration
{
	public class FileProcessorConfig : IFileProcessorConfig
	{
		public required string LogDir { get; set; }
		public required int BulkInsertSize { get; set; }
		public required string IncomingXmlFilesDir { get; set; }
		public required string ArchiveXmlFilesDir { get; set; }
		public required string ErrorXmlFilesDir { get; set; }
		public required string IncomingXmlFilesForceParseDir { get; set; }
		public required string UniversalXmlFileExtensionName { get; set; }
		public int FileWatchTimerIntervalInSeconds { get; set; }
	}
}
