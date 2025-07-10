using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.USReferenceData.Business;

namespace CargoWise.RefDbRepo.USReferenceData.CmdLine
{
	class NewWatchTariffProgram
	{
		public static void NewWatchTariffMain(string outputPath, string[] args)
		{
			if (string.IsNullOrEmpty(outputPath))
			{
				Console.WriteLine("AppSetting OutputFolder needs to be set in config file");
				return;
			}

			var outputFolder = Path.GetDirectoryName(outputPath);
			if (!Directory.Exists(outputFolder))
			{
				Directory.CreateDirectory(outputFolder);
			}

			DateTime publicationTime;
			if (!DateTime.TryParse(args[1], out publicationTime))
			{
				Console.WriteLine("Publication Time: Please enter the correct time format. Ex: yyyy-MM-ddTHH:mm:ss");
				return;
			}

			Console.WriteLine("Processing start...");
			var outPutFilePath = Path.Combine(outputPath, "USNewWatchTariff_" + publicationTime.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture) + ".xml");
			var inputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RefCusCodeList", "US New Watch Rules.txt");
			var result = new NewWatchTariffParser(inputFile, outPutFilePath, publicationTime).ParseToXMLFile();
			if (string.IsNullOrEmpty(result))
			{
				Console.WriteLine("Processing end...");
				Console.WriteLine($"Processed result generated: {outPutFilePath}");
			}
			else
			{
				Console.Error.WriteLine("Exception happended during processing. Please see following messages:");
				Console.Error.WriteLine(result);
			}
		}
	}
}
