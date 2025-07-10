using System.IO;
using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	static class TestHelper
	{
		public static string CopyToDownloadPath(string sourcePath, string destinationFileName, DateTime lastWriteTime)
		{
			if (!Directory.Exists(ApplicationConfig.DownloadsFolder))
			{
				Directory.CreateDirectory(ApplicationConfig.DownloadsFolder);
			}

			var destinationPath = Path.Combine(ApplicationConfig.DownloadsFolder, destinationFileName);
			File.Copy(sourcePath, destinationPath, overwrite: true);
			File.SetLastWriteTime(destinationPath, lastWriteTime);
			return Path.GetFullPath(destinationPath);
		}

		public static string BasePath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestFiles");
	}
}
