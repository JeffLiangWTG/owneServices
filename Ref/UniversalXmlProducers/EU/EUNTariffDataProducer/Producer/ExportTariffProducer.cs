using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ExportTariffProducer : TariffProducer
	{
		public override IEnumerable<string> FilesToLocate => new[] { "Duties Export", "Measure exclusions" };

		protected override ITariffMerger GetNewTariffMerger(ITariffCodeExtractor tariffCodeExtractor, IPreferenceMapper preferenceMapper) => new TariffMerger(tariffCodeExtractor, preferenceMapper, isExport: true, expireTariffBasedOnMaxRateDate: true);

		protected override TariffXmlProducer GetNewTariffXmlProducer() => new ExportTariffXmlProducer();

		protected override TariffXmlProducer GetNewTariffXmlProducerIncludingCompositeKey(bool includeCompositeKey) => new ExportTariffXmlProducer(includeCompositeKey);

		protected override IEnumerable<RefCusTariff> LoadTariffs(IEnumerable<RefCusTariff> allTariffs, IEnumerable<RefCusTariff> processedTariffs)
		{
			foreach (var processedTariff in processedTariffs)
			{
				var affectedTariffs = allTariffs.Where(x => AffectedByTariff(x, processedTariff));

				foreach (var tariff in affectedTariffs)
				{
					tariff.RefCusConditions = (tariff.RefCusConditions ?? new RefCusCondition[] { }).Union(processedTariff.RefCusConditions ?? new RefCusCondition[] { }).ToArray();
					tariff.RefCusTariffUOMs = UnionAndDistinctUOM(tariff.RefCusTariffUOMs, processedTariff.RefCusTariffUOMs);
				}
			}
			foreach (var tariff in allTariffs.Where(x => x.RefCusTariffUOMs == null).ToArray())
			{
				tariff.RefCusTariffUOMs = new RefCusTariffUOM[] { TariffUOMGenerator.DefaultTariffUom };
			}
			return allTariffs;
		}

		protected override RawRecord GetNewRawRecord() => new ExportRawRecord();

		static bool AffectedByTariff(RefCusTariff tariff, RefCusTariff processedTariff)
		{
			var processedTrimmedTariffCode = Utils.RemoveTrailingZeros(processedTariff.ZZ1_TariffCode);
			return tariff.ZZ1_TariffCode.StartsWith(processedTrimmedTariffCode, System.StringComparison.Ordinal);
		}

		static RefCusTariffUOM[] UnionAndDistinctUOM(RefCusTariffUOM[] exsistingTariffUoms, RefCusTariffUOM[] newTariffUoms)
		{
			return (exsistingTariffUoms ?? new RefCusTariffUOM[] { })
								   .Union(newTariffUoms ?? new RefCusTariffUOM[] { })
								   .GroupBy(x => x.ZZ8_UOM)
								   .Select(x => x.First())
								   .ToArray();
		}

		public override IEnumerable<IWebFileInfo> LocateWebFiles()
		{
			if (ApplicationConfig.SkipExportDutyDownloadIfExists)
			{
				return ApplicationConfig.GetExistingFileIfExists(ApplicationConfig.DownloadsFolder, FilesToLocate, null);
			}
			return base.LocateWebFiles();
		}
	}
}
