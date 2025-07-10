using System;
using CargoWise.RefDbRepo.IEReferenceData.CmdLine;
using CargoWise.RefDbRepo.IEReferenceData.ROSErrors.Business;
using CargoWise.RefDbRepo.IEReferenceData.ROSErrors.Services;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.ROSErrors.CmdLine
{
	class ROSErrorsProgram
	{
		public static void Run(string outputPath)
		{
			var now = DateTime.UtcNow;
			var lastRun = RuntimeDataRecorder.Read(RuntimeDataRecorder.Keys.DownloadROSErrorList) is DateTime dateTime ? dateTime : DateTime.MinValue;
			if (lastRun.Date < now.Date)
			{
				var downloader = new DownloadROSErrorList();
				var errorCodeList = DownloadROSErrorList.Download(ApplicationConfig.Instance.ROSErrorsListUrl);
				Console.Error.WriteLine(new ROSErrorsListProducer().ConvertToXmlFile(errorCodeList.ExtractedErrorList, new DateTimeProvider().CurrentLocalDate, outputPath));
				RuntimeDataRecorder.Write(RuntimeDataRecorder.Keys.DownloadROSErrorList, now);
			}
		}
	}
}
