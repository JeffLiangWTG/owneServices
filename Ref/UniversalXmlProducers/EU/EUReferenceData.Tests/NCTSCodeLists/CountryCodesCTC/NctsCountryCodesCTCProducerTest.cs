using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsCountryCodesCTCProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsCountryCodesCTCProducer>
	{
		protected override NctsCountryCodesCTCProducer Producer => new NctsCountryCodesCTCProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_CountryCodesCTC.zip";

		protected override string InputFileName => "RD_NCTS-P5_CountryCodesCTC.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL112.xml";
	}
}
