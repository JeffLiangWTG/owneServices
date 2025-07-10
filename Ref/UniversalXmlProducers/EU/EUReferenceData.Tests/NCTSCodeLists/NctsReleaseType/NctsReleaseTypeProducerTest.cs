using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsReleaseTypeProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsReleaseTypeProducer>
	{
		protected override NctsReleaseTypeProducer Producer => new NctsReleaseTypeProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_ReleaseType.zip";

		protected override string InputFileName => "RD_NCTS-P5_ReleaseType.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL163.xml";
	}
}
