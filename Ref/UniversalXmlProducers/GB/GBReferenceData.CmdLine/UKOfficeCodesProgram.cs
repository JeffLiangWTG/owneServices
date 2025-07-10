using System;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Business.UKOfficeCodes;
using CargoWise.RefDbRepo.GBReferenceData.Services;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;
using CargoWise.RefDbRepo.GBReferenceData.Services.UKOfficeCodes;

namespace CargoWise.RefDbRepo.GBReferenceData.CmdLine
{
	static class UKOfficeCodesProgram
	{
		public static void Run()
		{
			var errorCollector = new StringBuilder();
			var webClient = new WebClientWrapper();
			var downloader = new UKOfficeCodesDownloader(webClient);
			var odsData = downloader.GetSpreadsheetData(ConfigurationProvider.UKOfficeCodesLandingPageUrl);
			new UKOfficeCodesParser(errorCollector, odsData).Parse();
			if (errorCollector.Length > 0)
			{
				var errorMessage = $"The following processing notifications occurred:\r\n{errorCollector}";
				Console.Error.WriteLine(errorMessage);
			}
		}
	}
}
