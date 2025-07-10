using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsQueryIdentifierProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsQueryIdentifierProducer>
	{
		protected override NctsQueryIdentifierProducer Producer => new NctsQueryIdentifierProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_QueryIdentifier.zip";

		protected override string InputFileName => "RD_NCTS-P5_QueryIdentifier.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL054.xml";

		protected override string RefCusCodeTypeOutputFileName => "RefCusCodeTypeZZ_EUN_CL054.xml";
	}
}
