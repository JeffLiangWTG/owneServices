using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsRejectionCodeDepartureExportProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsRejectionCodeDepartureExportProducer>
	{
		protected override NctsRejectionCodeDepartureExportProducer Producer => new NctsRejectionCodeDepartureExportProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_RejectionCodeDepartureExport.zip";

		protected override string InputFileName => "RD_NCTS-P5_RejectionCodeDepartureExport.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL226.xml";
	}
}
