using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsAdditionalReferenceProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsAdditionalReferenceProducer>
	{
		protected override NctsAdditionalReferenceProducer Producer => new NctsAdditionalReferenceProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_AdditionalReference.zip";

		protected override string InputFileName => "RD_NCTS-P5_AdditionalReference.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_AR44N.xml";
	}
}
