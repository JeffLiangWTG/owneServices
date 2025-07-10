using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsGuaranteeTypeCTCProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsGuaranteeTypeCTCProducer>
	{
		protected override NctsGuaranteeTypeCTCProducer Producer => new NctsGuaranteeTypeCTCProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_GuaranteeTypeCTC.zip";

		protected override string InputFileName => "RD_NCTS-P5_GuaranteeTypeCTC.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL229.xml";
	}
}
