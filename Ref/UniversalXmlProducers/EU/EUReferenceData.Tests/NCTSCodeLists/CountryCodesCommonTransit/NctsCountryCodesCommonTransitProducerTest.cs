using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsCountryCodesCommonTransitProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsCountryCodesCommonTransitProducer>
	{
		protected override NctsCountryCodesCommonTransitProducer Producer => new NctsCountryCodesCommonTransitProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_CountryCodesCommonTransit.zip";

		protected override string InputFileName => "RD_NCTS-P5_CountryCodesCommonTransit.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_C0009.xml";
	}
}
