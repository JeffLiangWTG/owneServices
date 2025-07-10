using System.IO;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
{
	public static class SystemIOHelper
	{
		public static string GetOutputFilePath(string fileName)
		{
			return Path.Combine(ApplicationConfig.OutputPath, fileName);
		}

		public static string GetResFilePath(string fileName)
		{
			return Path.Combine(ApplicationConfig.ResPath, fileName);
		}
	}
}
