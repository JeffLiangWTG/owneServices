using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsSupportingDocumentProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsSupportingDocumentProducer>
	{
		protected override NctsSupportingDocumentProducer Producer => new NctsSupportingDocumentProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_SupportingDocumentType.zip";

		protected override string InputFileName => "RD_NCTS-P5_SupportingDocumentType.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_DC44N.xml";
	}
}
