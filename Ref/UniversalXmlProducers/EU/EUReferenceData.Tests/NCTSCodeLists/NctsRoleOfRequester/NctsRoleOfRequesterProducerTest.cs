using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsRoleOfRequesterProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsRoleOfRequesterProducer>
	{
		protected override NctsRoleOfRequesterProducer Producer => new NctsRoleOfRequesterProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_RoleRequester.zip";

		protected override string InputFileName => "RD_NCTS-P5_RoleRequester.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL156.xml";

		protected override string RefCusCodeTypeOutputFileName => "RefCusCodeTypeZZ_EUN_CL156.xml";
	}
}
