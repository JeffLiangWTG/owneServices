using System;
using System.IO;
using CargoWise.RefDbRepo.USReferenceData.Business.ExchangeRate;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.CmdLine
{
	sealed class ExchangeRateProgram
	{
		public static void ExchangeRateProgramMain(string outputPath)
		{
			if (string.IsNullOrEmpty(outputPath))
			{
				Console.WriteLine("AppSetting OutputFolder needs to be set in config file");
				return;
			}
			else if (!Directory.Exists(outputPath))
			{
				Directory.CreateDirectory(outputPath);
			}

			var serviceClient = new DownLoadService();
			var parser = new ExchangeRateParser(ApplicationConfig.Instance.ExchangeRateFilePageURL, ApplicationConfig.Instance.ExchangeRateFileBaseURL, outputPath, serviceClient);

			Console.WriteLine("Start Parse Exchange Rate(s).");

			var result = parser.ParseToXml();

			Console.WriteLine($"Parse End. Result: {result}");
		}
	}
}
