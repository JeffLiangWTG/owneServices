using System;
using System.IO;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAFacilityData;
using CargoWise.RefDbRepo.CAReferenceData.Services;

namespace CargoWise.RefDbRepo.CAReferenceData.CmdLine
{
	class CAFacilityData
	{
		public static void Run(string[] args)
		{
			var validArgs = ValidateArguments(args);

			if (validArgs)
			{
				var exportFilePath = Path.Combine(Business.ApplicationConfig.OutputPath, Business.ApplicationConfig.CustomsOfficeGenericSublocationCodeFilename);
				var customsOfficeGenericSublocationCode = new CustomsOfficeGenericSublocationCodeParser(new WebpageTableToDataTable(WebScraper.GetHtmlWithWebDriver(Business.ApplicationConfig.CustomsOfficeGenericSublocationCodeUrl)), exportFilePath);
				customsOfficeGenericSublocationCode.ExportXml();

				exportFilePath = Path.Combine(Business.ApplicationConfig.OutputPath, Business.ApplicationConfig.SufferanceWarehouseOperatorAndSublocationCodeFilename);
				var sufferanceWarehouseOperatorAndSublocationCode = new SufferanceWarehouseOperatorAndSublocationCodeParser(new WebpageTableToDataTable(WebScraper.GetHtmlWithWebDriver(Business.ApplicationConfig.SufferanceWarehouseOperatorAndSublocationCodeUrl)), exportFilePath);
				sufferanceWarehouseOperatorAndSublocationCode.ExportXml();
			}
			else
			{
				Console.WriteLine($"Too many arguments");
			}
		}

		static bool ValidateArguments(string[] args)
		{
			if (args == null || args.Length == 0)
			{
				return true;
			}
			return false;
		}
	}
}
