using System;
using System.IO;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CBSAErrorCode;

namespace CargoWise.RefDbRepo.CAReferenceData.CmdLine
{
	class CBSAErrorCode
	{
		public static void Run()
		{
			var preProcessChecker = new PreProcessChecker(Constants.ProgramFunctions.CBSAErrorCode);
			var downLoader = new CBSAErrorCodeFileDownloader(preProcessChecker);
			var errorLogs = downLoader.ErrorBuilder;
			try
			{
				var fileName = Path.GetTempFileName();
				if (downLoader.DownloadFile(ApplicationConfig.CBSAErrorCodes, fileName))
				{
					var reader = new DataReader(fileName, ApplicationConfig.OutputPath, downLoader.PublicationTime);

					if (!reader.ReadCSVAndExportXML())
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
