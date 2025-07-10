using System;
using System.IO;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CFIAAIRSRegistrationTypes;

namespace CargoWise.RefDbRepo.CAReferenceData.CmdLine
{
	public static class CFIAAIRSRegistrationTypes
	{
		public static void Run()
		{
			var preProcessChecker = new PreProcessChecker(Constants.ProgramFunctions.CFIAAIRSRegistrationTypes);
			var downLoader = new CFIAAIRSRegistrationTypeFileDownloader(preProcessChecker);
			var errorLogs = downLoader.ErrorBuilder;
			try
			{
				if (downLoader.ReadPages())
				{
					var englishFileName = Path.GetTempFileName();
					if (downLoader.DownloadFile(downLoader.EnglishDownloadPageURL, englishFileName, downloadPdf: false))
					{
						var frenchFileName = Path.GetTempFileName();
						if (downLoader.DownloadFile(downLoader.FrenchDownloadPageURL, frenchFileName, passCheck: true, downloadPdf: false))
						{
							var exportFilePath = Path.Combine(ApplicationConfig.OutputPath, ApplicationConfig.CFIAAIRSRegistrationTypesFilename);
							var reader = new DataReader(englishFileName, frenchFileName, exportFilePath);
							if (!reader.ReadHtmlAndExportXML())
							{
								errorLogs.AppendLine("Fail to read data or create XML");
							}
						}
					}
				}
				else
				{
					errorLogs.AppendLine("Read web pages failed.");
				}
			}
			catch (Exception)
			{
				preProcessChecker.MarkAsProcessRequired();
				throw;
			}

			if (!string.IsNullOrEmpty(errorLogs.ToString()))
			{
				preProcessChecker.MarkAsProcessRequired();
				Program.PrintErrorMessage(errorLogs.ToString());
			}
		}
	}
}
