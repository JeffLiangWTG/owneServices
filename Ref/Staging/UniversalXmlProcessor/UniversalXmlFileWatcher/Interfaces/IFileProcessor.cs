namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher.Interfaces
{
	public interface IFileProcessor
	{
		string ArchiveFolder { get; set; }
		string ErrorFolder { get; set; }
		string[] FoldersToScan { get; set; }
		string FileExtensionToMonitor { get; set; }
		int OverdueMonths { get; set; }
		string LastRunTimeFile { get; set; }
		Task ScanFolders();
		void DeleteOverdueFiles();
	}
}
