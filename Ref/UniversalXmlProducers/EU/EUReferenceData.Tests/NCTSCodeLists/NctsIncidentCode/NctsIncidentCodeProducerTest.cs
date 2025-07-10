using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsIncidentCodeProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsIncidentCodeProducer>
	{
		protected override NctsIncidentCodeProducer Producer => new NctsIncidentCodeProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_IncidentCode.zip";

		protected override string InputFileName => "RD_NCTS-P5_IncidentCode.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL019.xml";
	}
}
