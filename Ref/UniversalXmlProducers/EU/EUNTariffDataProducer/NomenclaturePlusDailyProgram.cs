using System.Linq;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	static class NomenclaturePlusDailyProgram
	{
		public static void Run()
		{
			var dailyTariffUpdatesFileProvider = new DailyTariffUpdatesFileProvider();
			dailyTariffUpdatesFileProvider.CleanAll();

			RunImport(dailyTariffUpdatesFileProvider);
			TariffCodeExtractor.ClearCache();
			RunExport(dailyTariffUpdatesFileProvider);
		}

		#region Import

		static void RunImport(DailyTariffUpdatesFileProvider dailyTariffUpdatesFileProvider)
		{
			var nomenclatures = new NomenclaturePlusDailyProducer(dailyTariffUpdatesFileProvider).Run();
			NomenclatureProgram.RunImportTariffCompositeKeyProducer(nomenclatures);

			if (NomenclatureProgram.TariffAreEmpty(nomenclatures))
			{
				return;
			}

			var importTariffProducer = new ImportTariffPlusDailyProducer(ApplicationConfig.PublishDate, dailyTariffUpdatesFileProvider);
			var tariffs = nomenclatures.Tariffs;
			foreach (var tariff in tariffs)
			{
				tariff.ZZ1_TariffCode = EUNUtils.NormalizeTariffCode(tariff.ZZ1_TariffCode);
				tariff.RefCusTariffLanguages = tariff.RefCusTariffLanguages.OrderBy(x => x.ZX7_ZX6_NKLanguage).ToArray();
			}
			importTariffProducer.Run(tariffs);
		}

		#endregion

		#region Export

		static void RunExport(DailyTariffUpdatesFileProvider dailyTariffUpdatesFileProvider)
		{
			var nomenclatures = new ExportNomenclaturePlusDailyProducer(dailyTariffUpdatesFileProvider)
				.Run(produceXml: false);

			if (NomenclatureProgram.TariffAreEmpty(nomenclatures))
			{
				return;
			}

			var exportTariffProducer = new ExportTariffPlusDailyProducer(ApplicationConfig.PublishDate, dailyTariffUpdatesFileProvider);
			var tariffs = nomenclatures.Tariffs;
			foreach (var tariff in tariffs)
			{
				tariff.ZZ1_TariffCode = EUNUtils.NormalizeExportTariffCode(tariff.ZZ1_TariffCode);
				tariff.RefCusTariffLanguages = tariff.RefCusTariffLanguages.OrderBy(x => x.ZX7_ZX6_NKLanguage).ToArray();
			}

			exportTariffProducer.Run(tariffs);
		}

		#endregion
	}
}
