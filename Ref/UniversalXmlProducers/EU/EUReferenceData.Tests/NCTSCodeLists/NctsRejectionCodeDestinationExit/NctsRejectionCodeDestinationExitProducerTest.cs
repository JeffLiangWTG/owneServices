using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsRejectionCodeDestinationExitProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsRejectionCodeDestinationExitProducer>
	{
		protected override NctsRejectionCodeDestinationExitProducer Producer => new NctsRejectionCodeDestinationExitProducer();
		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_RejectionCodeDestinationExit.zip";
		protected override string InputFileName => "RD_NCTS-P5_RejectionCodeDestinationExit.zip";
		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL227.xml";
	}
}
