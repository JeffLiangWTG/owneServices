using System;
using CargoWise.RefDbRepo.USReferenceData.Business.ExemptionCode;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.CmdLine
{
	class ExemptionCodeProgram
	{
		public static void ExemptionCodeMain(string outputPath)
		{
			if (string.IsNullOrEmpty(outputPath))
			{
				Console.WriteLine("AppSetting OutputFolder needs to be set in config file");
				return;
			}
			var serviceClient = new DownLoadService();
			var parser = new ExemptionCodeParser(ApplicationConfig.Instance.CustomsBorderProtectionGoverment, ApplicationConfig.Instance.ACEDDTCITARExemptionCodesURLForPDF, outputPath, serviceClient);

			Console.WriteLine("Start Parse Exemption Code(s).");

			var result = parser.DownloadPDFAndExportXML();

			Console.WriteLine($"Parse End. Result: {result}");
		}
	}
}
