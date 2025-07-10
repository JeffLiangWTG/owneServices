namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher.Interfaces
{
	public interface IFileCompressor
	{
		bool CompressFile(string fileName, string destinationFileName);
	}
}
