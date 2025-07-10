using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNDailyImportTariffPopulator
{
	public class DailyHelper : IDailyHelper
	{
		public IEnumerable<measure> GetMeasures(IDailyConfigProvider dailyConfigProvider, ITraderObjectLoader dailyMeasureLoader, string[] regulationIds, DateTime publicationDate)
		{
			var measureFilter = new MeasureFilter(dailyConfigProvider.Filter_GoodsNomenclatures, dailyConfigProvider.Filter_MeasureTypes, dailyConfigProvider.Filter_GeographicalAreaIds, regulationIds);
			var goodsNomenclatureFilter = new GoodsNomenclatureFilter(dailyConfigProvider.Filter_GoodsNomenclatures);
			var recordFilter = new RecordFilter().SetMeasureFilter(measureFilter).SetGoodsNomenclatureFilter(goodsNomenclatureFilter);
			var dailyMeasureRecords = dailyMeasureLoader.Get(recordFilter, publicationDate);
			return dailyMeasureRecords.Where(x => recordFilter.IsMeasure(x) && recordFilter.IsValid(x, publicationDate)).Select(x => recordFilter.GetValidValueFromRecord(x)).Cast<measure>();
		}

		public string[] GetRegulations(IEnumerable<ITraderObjectLoader> measureLoaders, string[] regulationsIds, DateTime publicationDate)
		{
			var baseRegulationsFilter = new BaseRegulationFilter();
			var modificationRegulationsFilter = new ModificationRegulationFilter();
			var recordFilter = new RecordFilter().SetBaseRegulationFilter(baseRegulationsFilter).SetModificationRegulationFilter(modificationRegulationsFilter);
			foreach (var measureLoader in measureLoaders)
			{
				var recordRecords = measureLoader.Get(recordFilter, publicationDate);
				var baseRegulations = recordRecords
					.Where(x => recordFilter.IsBaseRegulation(x) && recordFilter.IsValid(x, publicationDate))
					.Select(x => recordFilter.GetValidValueFromRecord(x)).Cast<baseRegulation>().Select(x => x.regulationId).ToArray();

				var modificationRegulations = recordRecords
					.Where(x => recordFilter.IsModificationRegulation(x) && recordFilter.IsValid(x, publicationDate))
					.Select(x => recordFilter.GetValidValueFromRecord(x)).Cast<modificationRegulation>().Select(x => x.modificationRegulationId).ToArray();

				var newBaseRegulations = baseRegulations.Except(regulationsIds);
				var newModificationRegulations = modificationRegulations.Except(regulationsIds).Except(newBaseRegulations);

				regulationsIds = regulationsIds.Concat(newBaseRegulations).Concat(newModificationRegulations).ToArray();
			}

			return regulationsIds;
		}

		public IEnumerable<RefCusTariff> GetTariffsFromMeasureLoaders(IEnumerable<ITraderObjectLoader> measureLoaders, IEnumerable<EUNCommonImportTariffPopulator.measureType1> measureTypes, goodsNomenclature[] goodsNomenclatures, IEnumerable<EUNCommonImportTariffPopulator.declarableGoodsNomenclature> monthlyDeclarableGoodsNomenclature, IDailyConfigProvider dailyConfigProvider, string[] regulationIds, DateTime publicationDate, List<RefCusTariff> dailyTariffList)
		{
			var tariffCreator = new TariffCreator();
			var result = new List<RefCusTariff>();
			foreach (var measureLoader in measureLoaders)
			{
				var measures = GetMeasures(dailyConfigProvider, measureLoader, regulationIds, publicationDate);

				foreach (var dailyMeasure in measures)
				{
					var tariffs = tariffCreator.Create(dailyMeasure, monthlyDeclarableGoodsNomenclature, goodsNomenclatures);
					foreach (var tariff in tariffs)
					{
						if (tariff != null && tariff.ZZ1_ZZI_NKTariffType == "IMP")
						{
							if (!dailyTariffList.Any(x => x.ZZ1_TariffCode == tariff.ZZ1_TariffCode)
								&& !result.Any(x => x.ZZ1_TariffCode == tariff.ZZ1_TariffCode))
							{
								result.Add(tariff);
							}
						}
					}
				}
			}
			return result;
		}

		public IEnumerable<RefCusTariff> GetTariffsFromDeclarableGoodsNomenclatureLinks(IEnumerable<string> declarableGoodsNomenclatureLinks, goodsNomenclature[] goodsNomenclatures, IDeclarableLoader declarableLoader, IDailyConfigProvider dailyConfigProvider, DateTime publicationDate, string temporaryDownloadPath, List<RefCusTariff> dailyTariffList)
		{
			var tariffCreator = new TariffCreator();
			var result = new List<RefCusTariff>();
			foreach (var link in declarableGoodsNomenclatureLinks)
			{
				var dailyDeclarableLoader = declarableLoader.GetTraderObjectLoader(link, temporaryDownloadPath);
				foreach (var declarable in dailyDeclarableLoader.Get(new DeclarableGoodsNomenclatureFilter(dailyConfigProvider.Filter_GoodsNomenclatures), publicationDate))
				{
					var tariff = tariffCreator.Create(declarable, goodsNomenclatures);
					if (tariff != null && tariff.ZZ1_ZZI_NKTariffType == "IMP")
					{
						if (!dailyTariffList.Any(x => x.ZZ1_TariffCode == tariff.ZZ1_TariffCode)
							&& !result.Any(x => x.ZZ1_TariffCode == tariff.ZZ1_TariffCode))
						{
							result.Add(tariff);
						}
					}
				}
			}
			return result;
		}

		public IEnumerable<goodsNomenclature> GetGoodsNomenclatures(IEnumerable<ITraderObjectLoader> measureLoaders, IDailyConfigProvider dailyConfigProvider, goodsNomenclature[] monthlyGoodsNomenclatures, DateTime publicationDate)
		{
			var result = new List<goodsNomenclature>(monthlyGoodsNomenclatures);
			var goodsNomenclatureFilter = new GoodsNomenclatureFilter(dailyConfigProvider.Filter_GoodsNomenclatures);
			var recordFilter = new RecordFilter().SetGoodsNomenclatureFilter(goodsNomenclatureFilter);
			foreach (var measureLoader in measureLoaders)
			{
				var recordRecords = measureLoader.Get(recordFilter, publicationDate);

				var goodsNomenclatures = recordRecords.Where(x => recordFilter.IsGoodsNomenclature(x) && recordFilter.IsValid(x, publicationDate))
					.Select(x => recordFilter.GetValidValueFromRecord(x)).Cast<goodsNomenclature>();
				foreach (var goodsNomenclature in goodsNomenclatures)
				{
					if (!monthlyGoodsNomenclatures.Any(x => x.goodsNomenclatureCode == goodsNomenclature.goodsNomenclatureCode && x.productLineSuffix == goodsNomenclature.productLineSuffix)
						&& !result.Any(x => x.goodsNomenclatureCode == goodsNomenclature.goodsNomenclatureCode && x.productLineSuffix == goodsNomenclature.productLineSuffix))
					{
						result.Add(goodsNomenclature);
					}
				}
			}
			return result;
		}
	}
}
