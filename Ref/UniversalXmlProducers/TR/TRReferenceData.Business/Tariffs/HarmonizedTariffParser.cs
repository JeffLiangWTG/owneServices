using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.TRReferenceData.Business
{
	public class HarmonizedTariffParser
	{
		readonly NomenclatureTariffParser nomenclatureTariffParser;
		readonly HsnTariffBanDataParser hsnTariffBanDataParser;

		public HarmonizedTariffParser() : this(new NomenclatureTariffParser(), new HsnTariffBanDataParser()) { }

		public HarmonizedTariffParser(NomenclatureTariffParser nomenclatureTariffParser, HsnTariffBanDataParser hsnTariffBanDataParser)
		{
			this.nomenclatureTariffParser = nomenclatureTariffParser;
			this.hsnTariffBanDataParser = hsnTariffBanDataParser;
		}

		public string ConvertToXMLFile(string outputDirectory)
		{
			var tariffs = nomenclatureTariffParser.GetTariffs();
			var populatedTariffs = hsnTariffBanDataParser.PopulateTariffs(tariffs);

			RunAdditionalProcessors(populatedTariffs);

			nomenclatureTariffParser.BuildNomenclatureXMLFile(outputDirectory);
			nomenclatureTariffParser.BuildTariffXMLFile(outputDirectory, populatedTariffs);
			return nomenclatureTariffParser.ErrorMessage + Environment.NewLine + hsnTariffBanDataParser.ErrorMessage;
		}

		public virtual void RunAdditionalProcessors(IEnumerable<RefCusTariff> tariffs)
		{
			HsnTariffSCDListIVDutyRatesProcessor.AttachSCDListIVDutyRates(tariffs);
			HsnTariffEXCListDutyRatesProcessor.AttachEXCListDutyRates(tariffs);
			// TODO: uncomment below (AttachDTYRates) line to activate rates functionality for WI00379492 -M12
			//HsnTariffDTYRatesProcessor.AttachDTYRates(tariffs);
		}
	}
}
