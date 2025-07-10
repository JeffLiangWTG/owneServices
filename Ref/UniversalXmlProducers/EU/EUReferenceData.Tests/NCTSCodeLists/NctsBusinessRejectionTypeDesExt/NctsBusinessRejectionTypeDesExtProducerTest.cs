using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsBusinessRejectionTypeDesExtProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsBusinessRejectionTypeDesExtProducer>
	{
		protected override NctsBusinessRejectionTypeDesExtProducer Producer => new NctsBusinessRejectionTypeDesExtProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_BusinessRejectionTypeDesExt.zip";

		protected override string InputFileName => "RD_NCTS-P5_BusinessRejectionTypeDesExt.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL570.xml";
	}
}
