using System;
using System.IO;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CFIAAIRSMiscData;

namespace CargoWise.RefDbRepo.CAReferenceData.CmdLine
{
	class CFIAAIRSMiscData
	{
		public static void Run()
		{
			var preProcessChecker = new PreProcessChecker(Constants.ProgramFunctions.CFIAAIRSMiscData);
			var downLoader = new CFIAAIRSMiscDataFileDownloader(preProcessChecker);
			var errorLogs = downLoader.ErrorBuilder;
			try
			{
				var fileName = Path.GetTempFileName();

				if (downLoader.DownloadFile(ApplicationConfig.AIRSMiscellaneousCodes, fileName))
				{
					var exportFilePath = Path.Combine(ApplicationConfig.OutputPath, ApplicationConfig.CFIAAIRSMiscCodesFilename);
					var reader = new DataReader(fileName, exportFilePath);

					if (!reader.ReadPDFAndExportXML())
					{
						errorLogs.AppendLine("Fail to read data or create XML");
					}
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
