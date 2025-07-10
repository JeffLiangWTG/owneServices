using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsDeclarationTypeProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsDeclarationTypeProducer>
	{
		protected override NctsDeclarationTypeProducer Producer => new NctsDeclarationTypeProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_DeclarationType.zip";

		protected override string InputFileName => "RD_NCTS-P5_DeclarationType.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_NCTDT.xml";
	}
}
