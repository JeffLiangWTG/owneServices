using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.INReferenceData.Services;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public class ExchangeRateXmlProducer
	{
		readonly string DataSource;

		public ExchangeRateXmlProducer(string dataSource)
		{
			DataSource = dataSource;
		}

		public List<string> ProduceXml()
		{
			var errors = new List<string>();
			var currencyDates = new HashSet<DateTime>() { new DateTimeProvider().GetIndiaToday() };
			FailDates.ForEach(x => currencyDates.Add(x));
			var currentFailDates = new List<DateTime>();
			foreach (var currencyDate in currencyDates)
			{
				try
				{
					ProduceXmlForOneDay(currencyDate);
				}
				catch (UnhandledApplicationException e)
				{
					errors.Add(e.Message);
					currentFailDates.Add(currencyDate);
				}
			}

			SaveFailDates(currentFailDates);
			return errors;
		}

		protected virtual void ProduceXmlForOneDay(DateTime currencyDate)
		{
			try
			{
				var responseContent = Downloader.DownloadData(currencyDate);
				var exchangeRates = ExchangeRateParser.ParseResponse(responseContent);
				ExportToXml(currencyDate, exchangeRates);
			}
			catch (Exception ex)
			{
				throw new UnhandledApplicationException(
					$"Error while processing currency date {currencyDate:dd-MM-yyyy}", ex);
			}
		}

		ExchangeRateDownloader Downloader => downloader ?? (downloader = new ExchangeRateDownloader());
		ExchangeRateDownloader downloader;

		protected LocalFileStorage FailDateStorage => failDateStorage ?? (failDateStorage = new LocalFileStorage("ExchangeRateFailDates"));
		LocalFileStorage failDateStorage;

		List<DateTime> FailDates => FailDateStorage.Load<List<DateTime>>() ?? new List<DateTime>();

		void SaveFailDates(List<DateTime> retryDates)
		{
			if (retryDates.Count == 0)
			{
				FailDateStorage.ClearData();
				return;
			}

			FailDateStorage.Save(retryDates);
		}

		#region Output Files

		protected void ExportToXml(DateTime currencyDate, List<RefExchangeRateZZ> exchangeRates)
		{
			if (exchangeRates == null || exchangeRates.Count == 0)
			{
				return;
			}

			var rateTypes = new[] { Constants.ExchangeRate.Types.Customs, Constants.ExchangeRate.Types.CustomsExport };
			foreach (var rateType in rateTypes)
			{
				var xmlWriterConfig = XMLWriterHelper.GetRefExchangeRateWriterConfiguration(currencyDate.AddDays(1), rateType);
				XMLWriterHelper.ExportToXMLFile(
					xmlWriterConfig,
					exchangeRates.Where(x => x.ZZN_ExRateType == rateType).ToList(),
					$"{DataSource} {rateType}",
					currencyDate.Add(DateTime.Now.TimeOfDay),
					UpdateType.Partial,
					GetOutputFilePath(currencyDate, rateType));
			}
		}

		protected string GetOutputFilePath(DateTime currencyDate, string rateType)
		{
			var fileName = $"RefExchangeRateZZ_IN_{rateType}_{currencyDate:ddMMyyyy}.xml";
			var outputPath = AppConfig.Shared.OutputDirectory;
			var filePath = Path.Combine(outputPath, fileName);
			if (!outputFilePaths.Contains(filePath))
			{
				outputFilePaths.Add(filePath);
			}

			return filePath;
		}

		protected void DeleteOutputFiles()
		{
			outputFilePaths.Where(File.Exists).ToList().ForEach(File.Delete);
		}

		readonly List<string> outputFilePaths = new List<string>();

		#endregion
	}
}
