using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;
using static CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.TariffProducer;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class EUNTariffExpander
	{
		#region Constructor

		public EUNTariffExpander()
		{
		}

		public EUNTariffExpander(List<RefCusTariff> impTariffs, List<RefCusTariff> expTariffs)
		{
			this.euIMPTariffList = impTariffs;
			this.euEXPTariffList = expTariffs;
		}

		#endregion

		IEnumerable<RefCusTariff> euIMPTariffList;
		IEnumerable<RefCusTariff> euEXPTariffList;

		static IEnumerable<RefCusTariff> LoadEUNImportTariffsFromWeb(bool produceNomenclatureXML = true)
		{
			ApplicationConfig.ConfigEnvironment();
			var nomenclatureProducer = new NomenclatureProducer();
			ICompositeKeyGeneratorResult nomenclatureList = nomenclatureProducer.Run(produceNomenclatureXML);

			if (nomenclatureList.Tariffs == null || nomenclatureList.Tariffs.Count == 0)
			{
				throw new InvalidOperationException("EUN tarrifs not found.");
			}
			var eunTarrifs = nomenclatureList.Tariffs;

			var importTariffProducer = new ImportTariffProducer();
			importTariffProducer.xmlProducer = new ImportTariffXmlProducer();
			var result = importTariffProducer.LoadAllTariffs(eunTarrifs);
			return result;
		}

		static IEnumerable<RefCusTariff> LoadEUNExportTariffsFromWeb()
		{
			ApplicationConfig.ConfigEnvironment();
			// get all EU tariffs
			ICompositeKeyGeneratorResult result = ExportTariffsProducer.DownloadUXmlFileAndLoadTariffs(ApplicationConfig.ExportTariffsDownloadURL, ApplicationConfig.ExportTariffsUXmlFile, ApplicationConfig.DownloadsFolder);

			if (result.Tariffs == null || result.Tariffs.Count <= 0)
			{
				throw new InvalidOperationException("EUN tarrifs not found.");
			}
			return result.Tariffs;
		}

		static List<RefCusTariff> AddTrailingZerosForTariffCodes(List<RefCusTariff> allTariffs, int length)
		{
			foreach (RefCusTariff tariff in allTariffs)
			{
				tariff.ZZ1_TariffCode = tariff.ZZ1_TariffCode.PadRight(length, '0');
			}
			return allTariffs;
		}

		public List<RefCusTariff> MergeImportTariffsWithEUNTarrifs(List<RefCusTariff> tariffs, bool produceNomenclatureXML = true)
		{
			euIMPTariffList = euIMPTariffList ?? LoadEUNImportTariffsFromWeb(produceNomenclatureXML);
			var preferenceMapper = new PreferenceMapperNoChangesImplementation();
			var tariffCodeExtractor = new TariffCodeExtractor(euIMPTariffList);
			var tariffMerger = new TariffMerger(tariffCodeExtractor, preferenceMapper);

			return AddTrailingZerosForTariffCodes(tariffMerger.ConvertToActualTariffs(tariffs).ToList(), 10);
		}

		public List<RefCusTariff> MergeExportTariffsWithEUNTarrifs(List<RefCusTariff> tariffs)
		{
			euEXPTariffList = euEXPTariffList ?? LoadEUNExportTariffsFromWeb();
			var preferenceMapper = new PreferenceMapperNoChangesImplementation();
			var tariffCodeExtractor = new TariffCodeExtractor(euEXPTariffList);
			var tariffMerger = new TariffMerger(tariffCodeExtractor, preferenceMapper);

			return AddTrailingZerosForTariffCodes(tariffMerger.ConvertToActualTariffs(tariffs).ToList(), 8);
		}
	}
}
