using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsTransportDocumentProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsTransportDocumentProducer>
	{
		protected override NctsTransportDocumentProducer Producer => new NctsTransportDocumentProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_TransportDocumentType.zip";

		protected override string InputFileName => "RD_NCTS-P5_TransportDocumentType.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_TD44N.xml";
	}
}
