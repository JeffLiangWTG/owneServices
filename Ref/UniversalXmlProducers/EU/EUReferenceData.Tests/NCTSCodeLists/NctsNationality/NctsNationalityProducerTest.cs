using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsNationalityProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsNationalityProducer>
	{
		protected override NctsNationalityProducer Producer => new NctsNationalityProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_Nationality.zip";

		protected override string InputFileName => "RD_NCTS-P5_Nationality.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_NCNAT.xml";
	}
}
