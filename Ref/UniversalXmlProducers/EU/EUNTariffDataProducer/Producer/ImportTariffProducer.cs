using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class ImportTariffProducer : TariffProducer
	{
		public override IEnumerable<string> FilesToLocate => new[] { "Duties Import", "Duties Export", "Measure exclusions", "Measure conditions" };

		protected override ITariffMerger GetNewTariffMerger(ITariffCodeExtractor tariffCodeExtractor, IPreferenceMapper preferenceMapper) => new TariffMerger(tariffCodeExtractor, preferenceMapper);

		protected override TariffXmlProducer GetNewTariffXmlProducer() => new ImportTariffXmlProducer();

		protected override TariffXmlProducer GetNewTariffXmlProducerIncludingCompositeKey(bool includeCompositeKey) => new ImportTariffXmlProducer(includeCompositeKey);

		protected override IEnumerable<RefCusTariff> LoadTariffs(IEnumerable<RefCusTariff> allTariffs, IEnumerable<RefCusTariff> processedTariffs)
		{
			var allTariffDictionary = allTariffs.ToDictionary(x => GetKey(x));
			foreach (var tariff in processedTariffs)
			{
				allTariffDictionary.TryGetValue(GetKey(tariff), out var tariffFromNomenclature);
				if (tariffFromNomenclature is null)
				{
					continue;
				}

				if (tariffFromNomenclature.ZZ1_EndDate < MaxDate)
				{
					tariff.ZZ1_EndDate = tariffFromNomenclature.ZZ1_EndDate;
				}
				tariff.RefCusTariffLanguages = tariffFromNomenclature.RefCusTariffLanguages;
			}
			return processedTariffs;

			string GetKey(RefCusTariff tariff)
				=> tariff.ZZ1_TariffCode + tariff.ZZ1_Description;
		}

		public override IEnumerable<IWebFileInfo> LocateWebFiles()
		{
			if (ApplicationConfig.SkipImportDutyDownloadIfExists)
			{
				return ApplicationConfig.GetExistingFileIfExists(ApplicationConfig.DownloadsFolder, FilesToLocate, null);
			}

			var locatedFiles = base.LocateWebFiles().ToList();

			var missingConditionsFile = ApplicationConfig.GetMissingConditionsFile();
			locatedFiles.Add(missingConditionsFile);
			return locatedFiles;
		}

		protected override RawRecord GetNewRawRecord() => new ImportRawRecord();

		readonly DateTime MaxDate = new DateTime(2079, 06, 06, 23, 59, 00);
	}
}
