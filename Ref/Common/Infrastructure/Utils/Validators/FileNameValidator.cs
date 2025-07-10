using System.IO;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class FileNameValidator
	{
		public static bool IsValid(string fileName)
		{
			return fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
		}
	}
}
