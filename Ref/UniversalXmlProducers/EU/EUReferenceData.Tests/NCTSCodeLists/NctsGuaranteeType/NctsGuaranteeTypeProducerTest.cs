using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsGuaranteeTypeProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsGuaranteeTypeProducer>
	{
		protected override NctsGuaranteeTypeProducer Producer => new NctsGuaranteeTypeProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_GuaranteeType.zip";

		protected override string InputFileName => "RD_NCTS-P5_GuaranteeType.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL251.xml";
	}
}
