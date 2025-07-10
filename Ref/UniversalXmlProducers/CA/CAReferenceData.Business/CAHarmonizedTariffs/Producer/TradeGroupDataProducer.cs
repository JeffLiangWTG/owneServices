using System;
using System.Globalization;
using System.Text;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs
{
	public class TradeGroupDataProducer : IProducer
	{
		public TradeGroupDataProducer(StringBuilder errorBuilder, IFileDownloader downloader)
		{
			this.downloader = downloader;
			_errorBuilder = errorBuilder;
			_workingFolder = System.IO.Path.GetTempFileName();
		}
		readonly StringBuilder _errorBuilder;
		readonly IFileDownloader downloader;

		public string OutPutFilePath => _outPutFilePath ?? (_outPutFilePath = System.IO.Path.Combine(ApplicationConfig.OutputPath, PublicationTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + string.Format(CultureInfo.InvariantCulture, ApplicationConfig.CustomsHarmonizedFilename, FunctionCode)));
		string _outPutFilePath;

		public DateTime PublicationTime => publicationTime;
		DateTime publicationTime;

		public string FunctionCode => Constants.ProgramFunctions.CATradeGroup;

		public string WorkingFileOrDirectory => _workingFolder;
		readonly string _workingFolder;

		public void QueryDataAndParseToXMLFile()
		{
			Console.WriteLine("Start processing CA Trade Group Data");
			var preProcessChecker = new PreProcessChecker(FunctionCode);
			try
			{
				var latestUpdateDateAndPDFUrl = downloader.GetTradeGroupEffectiveDateAndUrl();
				if (preProcessChecker.UpdateLastPublishDate(latestUpdateDateAndPDFUrl.Item1))
				{
					publicationTime = latestUpdateDateAndPDFUrl.Item1;
					Console.WriteLine("CA Trade Group: Downloading files from webservice...");
					if (downloader.DownloadFile(latestUpdateDateAndPDFUrl.Item2, WorkingFileOrDirectory))
					{
						new TradeGroupDataParser(publicationTime, OutPutFilePath).ParseTradeGroupData(WorkingFileOrDirectory);
					}
				}
				else
				{
					Console.WriteLine($"CA Trade Group: Nothing new published since last process. Skip processing this time.");
				}
			}
			catch (Exception ex)
			{
				_errorBuilder.AppendLine($"Error processing in Trade Group Data :{System.Environment.NewLine}{ex.Message}");
				preProcessChecker.MarkAsProcessRequired();
			}

			Console.WriteLine("End processing CA Trade Group Data");
		}
	}
}
