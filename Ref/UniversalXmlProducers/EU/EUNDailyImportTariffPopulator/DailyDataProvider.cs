using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	public class DailyDataProvider : IDailyDataProvider
	{
		readonly IHttpClientHelper httpClientHelper;
		readonly IDailyConfigProvider dailyConfigProvider;
		readonly IDailyIntegrationHelper dailyIntegrationHelper;
		readonly IDailyHelper dailyHelper;
		readonly IDeclarableLoader declarableLoader;
		DateTime _publicationTime;

		public DailyDataProvider(IDailyIntegrationHelper dailyIntegrationHelper, IDailyConfigProvider dailyConfigProvider, IHttpClientHelper httpClientHelper, IDailyHelper dailyHelper, IDeclarableLoader declarableLoader)
		{
			this.dailyIntegrationHelper = dailyIntegrationHelper;
			this.dailyConfigProvider = dailyConfigProvider;
			this.httpClientHelper = httpClientHelper;
			this.dailyHelper = dailyHelper;
			this.declarableLoader = declarableLoader;
		}

		public IEnumerable<RefCusTariff> GetImportTariffs()
		{
			var linkFinder = new DailyLinkFinder(httpClientHelper, dailyConfigProvider);
			return GetImportTariffs(linkFinder);
		}

		public IEnumerable<RefCusTariff> GetImportTariffs(IDailyLinkFinder linkFinder)
		{
			var tmpPath = Path.GetTempPath();
			if (!string.IsNullOrEmpty(dailyConfigProvider.DownloadFolder_alternative))
			{
				tmpPath = dailyConfigProvider.DownloadFolder_alternative;
			}
			if (!Directory.Exists(tmpPath))
			{
				Directory.CreateDirectory(tmpPath);
			}

			_publicationTime = linkFinder.GetPublicationTime();

			//monthly tariffs
			var monthlyDataProvider = dailyIntegrationHelper.GetMonthlyDataProvider();
			var goodsNomenclatures = monthlyDataProvider.GetGoodsNomenclatures(_publicationTime);
			var measureTypes = monthlyDataProvider.GetMeasureTypes(_publicationTime);
			var regulationIds = monthlyDataProvider.GetRegulationIds(_publicationTime);
			var monthlyDeclarableGoodsNomenclature = monthlyDataProvider.GetDeclarableGoodsNomenclature(_publicationTime);

			var measureLinks = linkFinder.GetIncrementalObjectTraderExportLinkOrderByPublishDateAscending();
			var measureLoaders = measureLinks.Select(x => declarableLoader.GetTraderObjectLoader(x, tmpPath)).ToArray();

			//combine monthly data with daily data.
			goodsNomenclatures = dailyHelper.GetGoodsNomenclatures(measureLoaders, dailyConfigProvider, goodsNomenclatures, _publicationTime).ToArray();
			regulationIds = dailyHelper.GetRegulations(measureLoaders, regulationIds, _publicationTime);

			//load daily tariffs from IncrementalObjectTraderExport_DeclarableGoodsNomenclature files
			var dailyTariffList = new List<RefCusTariff>();
			var declarableGoodsNomenclatureLinks = linkFinder.GetIncrementalObjectTraderExport_DeclarableGoodsNomenclatureLinkOrderByPublishDateAscending();
			dailyTariffList.AddRange(dailyHelper.GetTariffsFromDeclarableGoodsNomenclatureLinks(declarableGoodsNomenclatureLinks, goodsNomenclatures,
				declarableLoader, dailyConfigProvider, _publicationTime, tmpPath, dailyTariffList));

			//tariffs from IncrementalObjectTraderExport files.
			dailyTariffList.AddRange(dailyHelper.GetTariffsFromMeasureLoaders(measureLoaders, measureTypes, goodsNomenclatures,
				monthlyDeclarableGoodsNomenclature, dailyConfigProvider,
				regulationIds, _publicationTime, dailyTariffList));

			//Set monthly Rates/Conditions/UOM/etc. on empty tariffs.
			monthlyDataProvider.SetRelatedDataForImportTariffs(dailyTariffList, _publicationTime);

			var conditionCodes = monthlyDataProvider.GetMeasureConditionCodes(_publicationTime);
			foreach (var measureLoader in measureLoaders)
			{
				MergeChildrenData(measureLoader, measureTypes, conditionCodes, dailyTariffList, regulationIds);
			}

			return dailyTariffList;
		}

		void MergeChildrenData(ITraderObjectLoader measureLoader, IEnumerable<measureType1> measureTypes, IEnumerable<SEReferenceData.Services.measureConditionCode> conditionCodes, List<RefCusTariff> dailyTariffList, string[] regulationIds)
		{
			var currentMeasures = dailyHelper.GetMeasures(dailyConfigProvider, measureLoader, regulationIds, _publicationTime);
			foreach (var (measure, rates, tariffUoms) in XMLObjectCreationHelper.CreateRatesAndTariffUOMs(measureTypes, currentMeasures))
			{
				foreach (var tariff in dailyTariffList)
				{
					if (measure.IsApplicable(tariff.ZZ1_TariffCode))
					{
						if (tariff.RefCusRates == null)
						{
							tariff.RefCusRates = new RefCusRate[0];
						}

						MergeHelper.MergeRates(tariff, rates);

						if (tariff.RefCusTariffUOMs == null)
						{
							tariff.RefCusTariffUOMs = new RefCusTariffUOM[] { TariffUOMCreator.GetDefaultTariffUOM };
						}

						MergeHelper.InsertTariffUOMIfNotExists(tariff, tariffUoms);
					}
				}
			}

			var supplementaryUnitMeasures = currentMeasures.Where(x => x.measureComponent.Any(y => y.dutyExpressionId == "99")).ToArray();
			foreach (var condition in XMLObjectCreationHelper.CreateConditions(conditionCodes, currentMeasures))
			{
				foreach (var tariff in dailyTariffList)
				{
					if (condition.Item1.IsApplicable(tariff.ZZ1_TariffCode))
					{
						var supplementaryUnit = XMLObjectCreationHelper.GetSupplementaryUnit(supplementaryUnitMeasures, tariff.ZZ1_TariffCode);
						if (tariff.RefCusConditions == null)
						{
							tariff.RefCusConditions = new RefCusCondition[0];
						}
						MergeHelper.MergeConditions(tariff, condition.Item2.Invoke(supplementaryUnit));
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
