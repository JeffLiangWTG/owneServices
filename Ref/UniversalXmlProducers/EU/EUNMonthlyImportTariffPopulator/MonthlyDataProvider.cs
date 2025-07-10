using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNMonthlyImportTariffPopulator
{
	public class MonthlyDataProvider : IMonthlyDataProvider
	{
		readonly IConfigProvider configProvider;
		readonly IHttpClientHelper httpClientHelper;
		readonly ILinkFinder linkFinder;
		readonly string tempPathForDownload;
		DateTime _publicationTime;

		public MonthlyDataProvider(IConfigProvider configProvider, IHttpClientHelper httpClientHelper,string tempDownloadFolder)
		{
			this.configProvider = configProvider;
			this.httpClientHelper = httpClientHelper;

			tempPathForDownload = string.IsNullOrEmpty(tempDownloadFolder) ? Path.GetTempPath() : tempDownloadFolder;
			if (!Directory.Exists(tempPathForDownload))
			{
				Directory.CreateDirectory(tempPathForDownload);
			}
			linkFinder = new LinkFinder(this.httpClientHelper, this.configProvider);
		}

		static ITraderObjectLoader GetTraderObjectLoader(string link, string path)
		{
			return new TraderObjectLoader(link, path);
		}

		public SEReferenceData.Services.goodsNomenclature[] GetGoodsNomenclatures(DateTime publicationDate)
		{
			if (goodsNomenclatures != null)
			{
				return goodsNomenclatures;
			}
			var declarableLoader = GetTraderObjectLoader(linkFinder.GetGoodsNomenclatureLink(), tempPathForDownload);
			return goodsNomenclatures = declarableLoader.Get(new GoodsNomenclatureFilter(configProvider.Filter_GoodsNomenclatures), publicationDate).ToArray();
		}
		SEReferenceData.Services.goodsNomenclature[] goodsNomenclatures;

		public IEnumerable<declarableGoodsNomenclature> GetDeclarableGoodsNomenclature(DateTime publicationDate)
		{
			if (declarableGoodsNomenclatures != null)
			{
				return declarableGoodsNomenclatures;
			}
			var declarableLoader = GetTraderObjectLoader(linkFinder.GetDeclarableGoodsNomenclatureLink(), tempPathForDownload);
			return declarableGoodsNomenclatures = declarableLoader.Get(new DeclarableGoodsNomenclatureFilter(configProvider.Filter_GoodsNomenclatures), publicationDate);
		}
		IEnumerable<declarableGoodsNomenclature> declarableGoodsNomenclatures;

		public IEnumerable<measureType1> GetMeasureTypes(DateTime publicationDate)
		{
			if (measureTypes != null)
			{
				return measureTypes;
			}
			var measureTypeLoader = GetTraderObjectLoader(linkFinder.GetMeasureTypeLink(), tempPathForDownload);
			return measureTypes = measureTypeLoader.Get(new MeasureTypeFilter(), publicationDate);
		}
		IEnumerable<measureType1> measureTypes;

		public string[] GetRegulationIds(DateTime publicationDate)
		{
			if (regulationIds != null)
			{
				return regulationIds;
			}
			var regulationLoader = GetTraderObjectLoader(linkFinder.GetRegulationLink(), tempPathForDownload);
			var baseRegulations = regulationLoader.Get(new BaseRegulationFilter(), publicationDate);
			var modificationRegulations = regulationLoader.Get(new ModificationRegulationFilter(), publicationDate);
			return regulationIds = baseRegulations.Select(x => x.regulationId)
				.Concat(modificationRegulations.Select(x => x.modificationRegulationId)).ToArray();
		}
		string[] regulationIds;

		public IEnumerable<SEReferenceData.Services.measureConditionCode> GetMeasureConditionCodes(DateTime publicationDate)
		{
			if (measureConditionCodes != null)
			{
				return measureConditionCodes;
			}
			var conditionCodeLoader = GetTraderObjectLoader(linkFinder.GetMeasureConditionCodeLink(), tempPathForDownload);
			return measureConditionCodes = conditionCodeLoader.Get(new MeasureConditionCodeFilter(), publicationDate);
		}
		IEnumerable<SEReferenceData.Services.measureConditionCode> measureConditionCodes;

		public IEnumerable<RefCusTariff> GetImportTariffs()
		{
			if (!Directory.Exists(tempPathForDownload))
			{
				Directory.CreateDirectory(tempPathForDownload);
			}

			_publicationTime = linkFinder.GetPublicationTime();

			// Tariff Creation
			var importTariffs = new List<RefCusTariff>();
			var tariffCreator = new TariffCreator();
			foreach (var declarable in GetDeclarableGoodsNomenclature(_publicationTime))
			{
				var tariff = tariffCreator.Create(declarable, GetGoodsNomenclatures(_publicationTime));
				if (tariff != null && tariff.ZZ1_ZZI_NKTariffType == "IMP")
				{
					importTariffs.Add(tariff);
				}
			}

			SetRelatedDataForImportTariffs(importTariffs, _publicationTime);

			return importTariffs;
		}

		public void SetRelatedDataForImportTariffs(IEnumerable<RefCusTariff> importTariffs, DateTime publicationDate)
		{
			// Measure Type
			var measureTypes = GetMeasureTypes(publicationDate);
			// MeasureConditionCode
			var conditionCodes = GetMeasureConditionCodes(publicationDate);
			// Regulation
			var regulationIds = GetRegulationIds(publicationDate);

			// Measure
			var measureLoader = GetTraderObjectLoader(linkFinder.GetMeasureLink(), tempPathForDownload);
			var goodsNomenclatureFilter = configProvider.Filter_GoodsNomenclatures;
			if (!goodsNomenclatureFilter.Any())
			{
				goodsNomenclatureFilter = importTariffs.Select(x => x.ZZ1_TariffCode).ToArray();
			}
			var measures = measureLoader.Get(new MeasureFilter(goodsNomenclatureFilter, configProvider.Filter_MeasureTypes, configProvider.Filter_GeographicalAreaIds, regulationIds), publicationDate);
			var supplementaryUnitMeasures = measures.Where(x => x.measureComponent.Any(y => y.dutyExpressionId == "99")).ToArray();

			foreach (var (measure, rates, tariffUoms) in XMLObjectCreationHelper.CreateRatesAndTariffUOMs(measureTypes, measures))
			{
				foreach (var tariff in importTariffs)
				{
					if (measure.IsApplicable(tariff.ZZ1_TariffCode))
					{
						if (tariff.RefCusRates == null)
						{
							tariff.RefCusRates = new RefCusRate[0];
						}
						tariff.RefCusRates = tariff.RefCusRates.Concat(rates).ToArray();

						if (tariff.RefCusTariffUOMs == null)
						{
							tariff.RefCusTariffUOMs = new RefCusTariffUOM[] { TariffUOMCreator.GetDefaultTariffUOM };
						}
						tariff.RefCusTariffUOMs = tariff.RefCusTariffUOMs.Concat(tariffUoms).ToArray();
					}
				}
			}

			foreach (var condition in XMLObjectCreationHelper.CreateConditions(conditionCodes, measures))
			{
				foreach (var tariff in importTariffs)
				{
					if (condition.Item1.IsApplicable(tariff.ZZ1_TariffCode))
					{
						var supplementaryUnit = XMLObjectCreationHelper.GetSupplementaryUnit(supplementaryUnitMeasures, tariff.ZZ1_TariffCode);
						if (tariff.RefCusConditions == null)
						{
							tariff.RefCusConditions = new RefCusCondition[0];
						}
						tariff.RefCusConditions = tariff.RefCusConditions.Concat(condition.Item2.Invoke(supplementaryUnit)).ToArray();
					}
				}
			}
		}

		public DateTime GetPublicationTime()
		{
			return _publicationTime;
		}
	}
}
