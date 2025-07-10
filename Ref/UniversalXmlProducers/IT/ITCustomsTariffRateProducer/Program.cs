using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Configuration;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.DataLoader;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.Resources;
using CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer.WebHandler;
using CargoWise.RefDbRepo.XmlProducer.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer
{
	class Program
	{
		static int Main(string[] args)
		{
			ProduceXml(args);
			return (int)ProducerStatus.Success;
		}

		static void ProduceXml(string[] args)
		{
			ApplicationConfig.ConfigEnvironment();
			// TODO: Enhance XMLWriter to support batch SaveXml and apply to this project
			ILogger logger = new ConsoleErrorLogger();
			var xmlService = new XmlService.XmlService(logger);
			var dateTimeProvider = new DateTimeProvider();

			using (var httpHandler = new HttpClientHandler())
			{
				httpHandler.SetSecureConnection(ApplicationConfig.ITCustomsWebUrlRequireSecureConnection);
				var publicationDateResponseHandler = new WebResponseHandler(httpHandler, dateTimeProvider);
				var publicationDateParser = new WebContentParser(publicationDateResponseHandler, ApplicationConfig.CustomsWebUrlPublicationDate);
				var publicationDate = publicationDateParser.GetPublicationDate();
				xmlService.UpdatePublicationDate(publicationDate);

				httpHandler.SetSecureConnection(ApplicationConfig.SafeDbServiceUrlRequireSecureConnection);

				var skipLastSyncCheck = false;
				List<string> cusTariffCodes;
				if (args.Length > 0)
				{
					cusTariffCodes = args.ToList();
					skipLastSyncCheck = true;
				}
				else
				{
					var cusTariffLoader = new CusTariffLoader(logger, httpHandler);
					if (!cusTariffLoader.Load(ApplicationConfig.SafeDbServiceUrl))
					{
						return;
					}
					cusTariffCodes = cusTariffLoader.AllCodes?.Distinct().ToList();
				}

				var totalNumberOfRecords = cusTariffCodes.Count;
				int numberOfRecordsProcessed = 0;

				if (!skipLastSyncCheck && publicationDate <= ApplicationConfig.GetLastSyncTime())
				{
					return;
				}

				var tradeGroupLookup = new TradeGroupLookup(logger, httpHandler);
				if (!tradeGroupLookup.Load(ApplicationConfig.SafeDbServiceUrl))
				{
					return;
				}

				var taxOrFeeCodeLookup = new TaxOrFeeCodeLookup(logger, httpHandler);
				if (!taxOrFeeCodeLookup.Load(ApplicationConfig.SafeDbServiceUrl))
				{
					return;
				}

				var rateCodeDataLookup = new RateCodeDataLookup(logger, httpHandler);
				if (!rateCodeDataLookup.Load(ApplicationConfig.SafeDbServiceUrl))
				{
					return;
				}

				var preferenceDataLookup = new NoteA148PreferenceDataLoader(logger, httpHandler);
				if (!preferenceDataLookup.Load(ApplicationConfig.SafeDbServiceUrl))
				{
					return;
				}

				httpHandler.SetSecureConnection(ApplicationConfig.ITCustomsWebUrlRequireSecureConnection);
				var handler = new WebResponseHandler(httpHandler, dateTimeProvider);

				var scrappedRecordParser = new ScrappedRecordParser(tradeGroupLookup, taxOrFeeCodeLookup, rateCodeDataLookup, publicationDate);
				var webContentParser = new WebContentParser(xmlService, handler, ApplicationConfig.CustomsWebUrl, scrappedRecordParser, preferenceDataLookup, logger);

				if (File.Exists(ApplicationConfig.PartialUrdXmlStoragePath))
				{
					File.Delete(ApplicationConfig.PartialUrdXmlStoragePath);
				}

				foreach (var tariffCodeBatch in cusTariffCodes.Batch(ApplicationConfig.BatchSize))
				{
					var currentBatch = tariffCodeBatch.ToArray();
					webContentParser.GenerateXmlFromWebContent(currentBatch)?.Wait();
					numberOfRecordsProcessed += currentBatch.Length;
					CheckAndDisplayProgress(numberOfRecordsProcessed, totalNumberOfRecords);
					xmlService.SaveXml(ApplicationConfig.PartialUrdXmlStoragePath);
				}

				SaveXml();

				DisplayProgress(numberOfRecordsProcessed, totalNumberOfRecords);

				if (!skipLastSyncCheck)
				{
					ApplicationConfig.SetLastSyncTime(publicationDate);
				}
			}
		}

		static void SaveXml()
		{
			var destination = ApplicationConfig.UrdXmlStoragePath;
			if (File.Exists(destination))
			{
				var fileName = Path.GetFileNameWithoutExtension(destination);
				var newFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}";

				destination = ApplicationConfig.UrdXmlStoragePath.Replace(fileName, newFileName);
			}

			File.Move(ApplicationConfig.PartialUrdXmlStoragePath, destination);
		}

		static void CheckAndDisplayProgress(int numberOfRecordsProcessed, int totalNumberOfRecords)
		{
			if (numberOfRecordsProcessed % ApplicationConfig.NoOfRecordsProcessed == 0)
			{
				DisplayProgress(numberOfRecordsProcessed, totalNumberOfRecords);
			}
		}

		static void DisplayProgress(int numberOfRecordsProcessed, int totalNumberOfRecords)
		{
			Console.WriteLine($"{numberOfRecordsProcessed} of {totalNumberOfRecords} records processed");
		}
	}
}
