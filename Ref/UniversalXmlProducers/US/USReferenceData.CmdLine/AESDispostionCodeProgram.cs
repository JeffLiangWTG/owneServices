using System;
using CargoWise.RefDbRepo.USReferenceData.Business.AESDispostionCode;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.CmdLine
{
	class AESDispostionCodeProgram
	{
		public static void AESDispostionCodesMain(string outputPath)
		{
			if (string.IsNullOrEmpty(outputPath))
			{
				Console.WriteLine("AppSetting OutputFolder needs to be set in config file");
				return;
			}

			var serviceClient = new DownLoadService();
			var parser = new AESDispostionCodeParser(ApplicationConfig.Instance.CustomsBorderProtectionGoverment, ApplicationConfig.Instance.AESDispostionCodeURL, outputPath, serviceClient);

			Console.WriteLine("Start Parse AES Dispostion Code(s).");

			var result = parser.DownloadPDFAndExportXML();

			Console.WriteLine($"Parse End. Result: {result}");
		}
	}
}
