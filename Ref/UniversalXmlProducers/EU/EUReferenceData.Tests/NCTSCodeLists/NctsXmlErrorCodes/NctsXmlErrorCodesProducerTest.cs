using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsXmlErrorCodesProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsXmlErrorCodesProducer>
	{
		protected override NctsXmlErrorCodesProducer Producer => new NctsXmlErrorCodesProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_XmlErrorCodes.zip";

		protected override string InputFileName => "RD_NCTS-P5_XmlErrorCodes.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL030.xml";
	}
}
