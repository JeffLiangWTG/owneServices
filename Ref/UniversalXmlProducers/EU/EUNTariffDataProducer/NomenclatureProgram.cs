using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	static class NomenclatureProgram
	{
		public static void Run()
		{
			var nomenclatures = RunNomenclatureProducer();
			RunImportTariffCompositeKeyProducer(nomenclatures);
			RunImportTariffProducer(nomenclatures);
			TariffCodeExtractor.ClearCache();
			RunExportTariffProducer();
		}

		#region Implementation

		static ICompositeKeyGeneratorResult RunNomenclatureProducer()
		{
			var nomenclatureProducer = new NomenclatureProducer();
			var result = nomenclatureProducer.Run();
			return result;
		}

		public static void RunImportTariffCompositeKeyProducer(ICompositeKeyGeneratorResult result)
		{
			Argument.NotNull(result, nameof(result));
			var tariffs = result.Tariffs;
			foreach (var tariff in tariffs)
			{
				tariff.ZZ1_TariffCode = EUNUtils.NormalizeTariffCode(tariff.ZZ1_TariffCode);
			}

			var compositeKeyProducer = new ImportTariffCompositeKeyProducer();
			compositeKeyProducer.Run(tariffs);
		}

		static void RunImportTariffProducer(ICompositeKeyGeneratorResult result)
		{
			Argument.NotNull(result, nameof(result));

			if (TariffAreEmpty(result))
			{
				return;
			}

			var importTariffProducer = new ImportTariffProducer();
			var tariffs = result.Tariffs;
			foreach (var tariff in tariffs)
			{
				tariff.ZZ1_TariffCode = EUNUtils.NormalizeTariffCode(tariff.ZZ1_TariffCode);
			}
			importTariffProducer.Run(tariffs);
		}

		public static void RunExportTariffProducer()
		{
			var eightDigitNomenclatureProducer = new ExportNomenclatureProducer();
			var eightDigitResult = eightDigitNomenclatureProducer.Run(produceXml: false);

			if (TariffAreEmpty(eightDigitResult))
			{
				return;
			}

			var exportTariffProducer = new ExportTariffProducer();
			var tariffs = eightDigitResult.Tariffs;
			foreach (var tariff in tariffs)
			{
				tariff.ZZ1_TariffCode = EUNUtils.NormalizeExportTariffCode(tariff.ZZ1_TariffCode);
			}

			exportTariffProducer.Run(tariffs);
		}

		public static bool TariffAreEmpty(ICompositeKeyGeneratorResult compositeKeyGeneratorResult)
		{
			Argument.NotNull(compositeKeyGeneratorResult, nameof(compositeKeyGeneratorResult));

			return compositeKeyGeneratorResult.Tariffs == null || compositeKeyGeneratorResult.Tariffs.Count <= 0;
		}

		#endregion
	}
}
