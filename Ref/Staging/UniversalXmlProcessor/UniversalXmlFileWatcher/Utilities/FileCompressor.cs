using System.IO.Compression;
using CargoWise.RefDbRepo.UniversalXmlFileWatcher.Interfaces;

namespace CargoWise.RefDbRepo.UniversalXmlFileWatcher.Utilities
{
	public class FileCompressor : IFileCompressor
	{
		public bool CompressFile(string fileName, string destinationFileName)
		{
			if (File.Exists(destinationFileName))
			{
				File.Delete(destinationFileName);
			}
			using (ZipArchive zip = ZipFile.Open(destinationFileName, ZipArchiveMode.Create))
			{
				FileInfo fileInfo = new FileInfo(fileName);
				var entryName = fileInfo.Name;
				zip.CreateEntryFromFile(fileName, entryName);
			}
			return true;
		}
	}
}
