using System;
using System.IO;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAOfficeCode;

namespace CargoWise.RefDbRepo.CAReferenceData.CmdLine
{
	class CAOfficeCode
	{
		public static void Run()
		{
			Console.WriteLine("Start processing CA Office Code data");
			var exportFilePath = Path.Combine(ApplicationConfig.OutputPath, ApplicationConfig.CAOfficeCodesFilename);
			var parser = new CustomsOfficeCodeParser(exportFilePath, ApplicationConfig.USPortOfExitMappingFileName, ApplicationConfig.CAOfficeCodePublicationTime);
			parser.ProduceData(ApplicationConfig.CAOfficeCodeUrl);
			Console.WriteLine("End processing CA Office Code data");
		}
	}
}
