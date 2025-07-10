using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsCountryCodesCommunityTypeProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsCountryCodesCommunityProducer>
	{
		protected override NctsCountryCodesCommunityProducer Producer => new NctsCountryCodesCommunityProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_CountryCodesCommunity.zip";

		protected override string InputFileName => "RD_NCTS-P5_CountryCodesCommunity.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL010.xml";
	}
}
