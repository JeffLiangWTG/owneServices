using System;
using System.IO;

namespace CargoWise.RefDbRepo.MXReferenceData.Services
{
	public abstract class BaseDownloader
	{
		public static string OutputFolderPath => outputFolderPath ?? (outputFolderPath = GetOutputFolderPath());

		static string outputFolderPath;

		static string GetOutputFolderPath()
		{
			var outputFolder = ConfigurationProvider.Configuration.GetSection("OutputFolder").Value;
			if (string.IsNullOrEmpty(outputFolder))
			{
				throw new InvalidOperationException("AppSetting OutputFolder need to be set in config file");
			}

			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, outputFolder);
		}
	}
}
