using System.IO;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public interface IFileReader
	{
		string[] ReadAllLines(string path);
	}

	public class FileReader : IFileReader
	{
		public string[] ReadAllLines(string path)
		{
			return File.ReadAllLines(path);
		}
	}
}
