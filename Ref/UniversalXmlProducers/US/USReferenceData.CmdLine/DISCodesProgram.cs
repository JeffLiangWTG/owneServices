using System;
using CargoWise.RefDbRepo.USReferenceData.Business.DISCodes;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.CmdLine
{
	class DISCodesProgram
	{
		public static void DISCodesMain(string outputPath)
		{
			if (string.IsNullOrEmpty(outputPath))
			{
				Console.WriteLine("AppSetting OutputFolder needs to be set in config file");
				return;
			}
			var serviceClient = new DownLoadService();
			var parser = new DISCodeParser(ApplicationConfig.Instance.CustomsBorderProtectionGoverment, ApplicationConfig.Instance.ACEDISImplementationGuideURLForPDF, outputPath, serviceClient);

			Console.WriteLine("Start Parse DIS Code(s).");
			
			var result = parser.DownloadPDFAndExportXML();

			Console.WriteLine($"Parse End. Result: {result}");
			
		}
	}
}
