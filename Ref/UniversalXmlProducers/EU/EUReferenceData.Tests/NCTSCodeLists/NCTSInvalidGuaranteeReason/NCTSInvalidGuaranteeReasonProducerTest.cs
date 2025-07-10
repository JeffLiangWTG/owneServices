using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NCTSInvalidGuaranteeReasonProducerTest : NCTSCodeListXmlProducerAbstractTest<NCTSInvalidGuaranteeReasonProducer>
	{
		protected override NCTSInvalidGuaranteeReasonProducer Producer => new NCTSInvalidGuaranteeReasonProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_InvalidGuaranteeReason.zip";

		protected override string InputFileName => "RD_NCTS-P5_InvalidGuaranteeReason.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL252.xml";
	}
}
