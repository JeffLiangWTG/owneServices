using System;
using System.Globalization;
using System.Text;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class TariffDataProducer : IProducer
	{
		public TariffDataProducer(StringBuilder errorBuilder, IFileDownloader downloader)
		{
			_errorBuilder = errorBuilder;
			this.downloader = downloader;
			_workingFolder = System.IO.Path.GetTempFileName();
			_excelFolder = System.IO.Path.GetTempFileName();
		}
		readonly StringBuilder _errorBuilder;
		readonly IFileDownloader downloader;
		readonly string _excelFolder;

		public string OutPutFilePath => _outPutFilePath ?? (_outPutFilePath = System.IO.Path.Combine(ApplicationConfig.OutputPath, PublicationTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + string.Format(CultureInfo.InvariantCulture, ApplicationConfig.CustomsHarmonizedFilename, FunctionCode)));
		string _outPutFilePath;

		public DateTime PublicationTime => publicationTime;
		DateTime publicationTime;

		public string FunctionCode => Constants.ProgramFunctions.CATariff;

		public string WorkingFileOrDirectory => _workingFolder;
		readonly string _workingFolder;

		public void QueryDataAndParseToXMLFile()
		{
			Console.WriteLine("Start processing CA Harmonized Tariff data");
			var preProcessChecker = new PreProcessChecker(FunctionCode);
			try
			{
				var latestUpdateDateAndZipUrl = downloader.GetLastEditDateAndAccessDbUrl();
				var excelModifiedDateAndUrl = downloader.GetConditionExcelModifiedDateAndUrl();
				publicationTime = DateTime.Compare(latestUpdateDateAndZipUrl.Item1, excelModifiedDateAndUrl.Item1) < 0 ? excelModifiedDateAndUrl.Item1 : latestUpdateDateAndZipUrl.Item1;

				if (preProcessChecker.UpdateLastPublishDate(publicationTime))
				{
					if (downloader.DownloadFile(latestUpdateDateAndZipUrl.Item2, WorkingFileOrDirectory) && downloader.DownloadFile(excelModifiedDateAndUrl.Item2, _excelFolder))
					{
						new TariffDataParser(WorkingFileOrDirectory, _excelFolder).ParseRefCusTariffsIntoXML(OutPutFilePath, PublicationTime);
					}
				}
				else
				{
					Console.WriteLine($"CA Harmonized Tariff: Nothing new published since last process. Skip processing this time.");
				}
			}
			catch (Exception ex)
			{
				_errorBuilder.AppendLine($"Error processing in Harmonized Tariff data :{System.Environment.NewLine}{ex.Message}");
				preProcessChecker.MarkAsProcessRequired();
			}
			Console.WriteLine("End processing CA Harmonized Tariff data");
		}
	}
}
