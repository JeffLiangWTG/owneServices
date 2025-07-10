using System;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.CmdLine
{
	class A99TariffsProgram
	{
		public static void TariffsMain(string outputPath)
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

			Console.WriteLine("Processing start...");
			var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ApplicationConfig.Instance.A99JsonFileName);
			var result = new A99TariffsParser(outputPath).ParseToXMLFile(filePath);
			Console.WriteLine(result);
			Console.WriteLine("Processing end...");
		}
	}
}
