using System.IO;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public interface IConcessionFileReader
	{
		string[] ReadAllLines(string path);
	}

	internal class ConcessionFileReader : IConcessionFileReader
	{
		public string[] ReadAllLines(string path)
		{
			return File.ReadAllLines(path);
		}
	}
}
