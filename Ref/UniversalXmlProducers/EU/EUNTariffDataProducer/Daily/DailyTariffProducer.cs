using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class DailyTariffProducer
	{
		ITariffDataProducer tariffDataProducer;
		IXmlProducer<RefCusTariff> xmlProducer;
		ITariffDataProducer seTariffDataProducer;
		readonly IDailyTariffFactory _dailyTariffFactory;

		public DailyTariffProducer(IDailyTariffFactory dailyTariffFactory)
		{
			Argument.NotNull(dailyTariffFactory, nameof(dailyTariffFactory));
			_dailyTariffFactory = dailyTariffFactory;
		}

		public void Run()
		{
			IEnumerable<IWebTariffHeader> tariffCodeHeaders;
			using (var webDriverHelper = _dailyTariffFactory.GetWebDriverHelper())
			{
				var tariffData = LoadTariffData(webDriverHelper);
				if (!tariffData.Any())
				{
					return;
				}
				tariffCodeHeaders = EUNUtils.GenerateWebTariffHeadersFromRefCusTariff(tariffData, _dailyTariffFactory.TariffCollectionFromNomenclature);
			}
			if (tariffCodeHeaders != null)
			{
				ExportMonthlyXml(tariffCodeHeaders);
				ExportTaricDailyAndSEXml(tariffCodeHeaders);
			}
		}

		void ExportTaricDailyAndSEXml(IEnumerable<IWebTariffHeader> tariffCodeHeaders)
		{
			var seXmlProducer = _dailyTariffFactory.GetSEXmlProducer();
			var tariffs = seTariffDataProducer.LoadTariffRates(tariffCodeHeaders);
			if (!tariffs.Any())
			{
				return;
			}
			seXmlProducer.InitializeWriter(ApplicationConfig.PublishDate, null, UpdateType.Partial);
			seXmlProducer.ExportToXml(tariffs);
		}

		void ExportMonthlyXml(IEnumerable<IWebTariffHeader> tariffCodeHeaders)
		{
			var tariffs = tariffDataProducer.LoadTariffRates(tariffCodeHeaders);
			if (!tariffs.Any())
			{
				return;
			}
			xmlProducer.InitializeWriter(ApplicationConfig.PublishDate, null, UpdateType.Partial);
			xmlProducer.ExportToXml(tariffs);
		}

		List<string> LoadTariffData(IWebDriverHelper webDriverHelper)
		{
			xmlProducer = _dailyTariffFactory.GetXmlProducer();
			var files = _dailyTariffFactory.LocateWebFiles(webDriverHelper);
			if (files.First().Exception != null)
			{
				xmlProducer.InitializeWriter(DateTime.Today);
				var errorMessage = files.First().Exception.Message;
				xmlProducer.ReportDataSourceError(errorMessage);
				throw files.First().Exception;
			}

			var seMeasureParser = _dailyTariffFactory.GetSEMeasureParser();
			var dailyTariffParser = new DailyTariffRateParser();
			var rawRecord = _dailyTariffFactory.GetRawRecord(dailyTariffParser, seMeasureParser);
			ParseDailyTariffDataProducer(files, rawRecord);

			var seRawRecord = _dailyTariffFactory.GetRawRecord(dailyTariffParser, seMeasureParser);
			ParseSETariffDataProducer(seRawRecord);
			return seRawRecord.GetDailyTariffHeadersToProcess();
		}

		void ParseDailyTariffDataProducer(IEnumerable<IWebFileInfo> files, IRawRecord rawRecord)
		{
			rawRecord.Parse(files);
			tariffDataProducer = GetTariffDataProducerFromRawRecord(rawRecord);
		}

		void ParseSETariffDataProducer(IRawRecord rawRecord)
		{
			rawRecord.ParseSEFileAndAttachTaricDailyRecords();
			seTariffDataProducer = GetTariffDataProducerFromRawRecord(rawRecord);
		}

		ITariffDataProducer GetTariffDataProducerFromRawRecord(IRawRecord rawRecord)
		{
			var measuringUnitTransformer = new MeasuringUnitTransformer();
			var agriculturalComponentFormulaExtractor = new AgriculturalComponentFormulaExtractor(measuringUnitTransformer);
			var conditionFormulaExtractor = new ConditionFormulaExtractor(measuringUnitTransformer);
			var formulaExtractor = new FormulaExtractor(measuringUnitTransformer, agriculturalComponentFormulaExtractor, conditionFormulaExtractor);
			var rateGenerator = new RateGenerator(formulaExtractor);
			var conditionValueDescriptionExtractor = new ConditionValueDescriptionExtractor();
			var conditionGenerator = new ConditionGenerator(conditionValueDescriptionExtractor);
			var tariffGenerator = new TariffGenerator(rateGenerator, conditionGenerator);
			var preferenceMapper = new PreferenceMapper();
			var tariffCodeExtractor = new TariffCodeExtractor(null);
			var tariffMerger = new TariffMerger(tariffCodeExtractor, preferenceMapper, expireTariffBasedOnMaxRateDate: false);

			return _dailyTariffFactory.GetDailyTariffDataProducer(rawRecord, tariffGenerator, tariffMerger);
		}
	}
}
