using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NCTSReleaseNotificationProducerTest : NCTSCodeListXmlProducerAbstractTest<NCTSReleaseNotificationProducer>
	{
		protected override NCTSReleaseNotificationProducer Producer => new NCTSReleaseNotificationProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_ReleaseNotification.zip";

		protected override string InputFileName => "RD_NCTS-P5_ReleaseNotification.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL164.xml";
	}
}
