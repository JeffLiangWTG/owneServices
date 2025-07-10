using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsAdditionalInformationProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsAdditionalInformationProducer>
	{
		protected override NctsAdditionalInformationProducer Producer => new NctsAdditionalInformationProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_AdditionalInformation.zip";

		protected override string InputFileName => "RD_NCTS-P5_AdditionalInformation.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_AI44N.xml";
	}
}
