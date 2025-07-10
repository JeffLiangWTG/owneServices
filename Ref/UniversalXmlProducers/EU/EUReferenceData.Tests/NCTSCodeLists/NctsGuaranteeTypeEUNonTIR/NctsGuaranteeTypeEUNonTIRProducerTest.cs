using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsGuaranteeTypeEUNonTIRProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsGuaranteeTypeEUNonTIRProducer>
	{
		protected override NctsGuaranteeTypeEUNonTIRProducer Producer => new NctsGuaranteeTypeEUNonTIRProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_GuaranteeTypeEUNonTIR.zip";

		protected override string InputFileName => "RD_NCTS-P5_GuaranteeTypeEUNonTIR.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL230.xml";
	}
}
