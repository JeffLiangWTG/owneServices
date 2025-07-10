using System;
using System.IO;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.CmdLine
{
	sealed class ECCNNumbersProgram
	{
		public static void ECCNNumbersProgramMain(string outputPath)
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

			var parser = new ECCNNumbersParser(ApplicationConfig.Instance.EARFilesCreatedByBISURL, outputPath);
			Console.WriteLine("Start Parse ECCN Numbers.");

			var result = parser.ConvertCodeListToXML();

			Console.WriteLine($"{result}");
			Console.WriteLine("Parse End.");
		}
	}
}
