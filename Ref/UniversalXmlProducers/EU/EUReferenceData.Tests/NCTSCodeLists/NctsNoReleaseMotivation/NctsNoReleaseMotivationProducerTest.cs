using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsNoReleaseMotivationProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsNoReleaseMotivationProducer>
	{
		protected override NctsNoReleaseMotivationProducer Producer => new NctsNoReleaseMotivationProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_NoReleaseMotivation.zip";

		protected override string InputFileName => "RD_NCTS-P5_NoReleaseMotivation.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL211.xml";
	}
}
