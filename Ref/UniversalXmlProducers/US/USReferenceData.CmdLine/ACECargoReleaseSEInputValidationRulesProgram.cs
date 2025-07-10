using System;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.CmdLine
{
	class ACECargoReleaseSEInputValidationRulesProgram
	{
		public static void ACECargoReleaseSEInputValidationRulesMain(string outputPath)
		{
			var serviceClient = new DownLoadService();
			var processDate = new DateTimeProvider().GetUTCNow();

			var action = new Action(() =>
			{
				Program.PrintLogMessage(new ACECargoReleaseSEInputValidationRulesParser(serviceClient, processDate).DownloadAndConvertCodesToXMLFile(outputPath));
			});
			action.Invoke();
		}
	}
}
