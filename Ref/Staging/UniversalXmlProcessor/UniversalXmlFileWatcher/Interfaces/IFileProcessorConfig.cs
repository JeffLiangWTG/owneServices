using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;

namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher.Interfaces
{
	public interface IFileProcessorConfig : IParserConfig
	{
		string IncomingXmlFilesDir { get; set; }
		string ArchiveXmlFilesDir { get; set; }
		string ErrorXmlFilesDir { get; set; }
		string IncomingXmlFilesForceParseDir { get; set; }
		string UniversalXmlFileExtensionName { get; set; }
		int FileWatchTimerIntervalInSeconds { get; set; }
	}
}
