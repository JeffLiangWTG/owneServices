using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsCountryOutsideCustomsSecurityAgreementAreaProducerTest : NCTSCodeListXmlProducerAbstractTest<NctsCountryOutsideCustomsSecurityAgreementAreaProducer>
	{
		protected override NctsCountryOutsideCustomsSecurityAgreementAreaProducer Producer => new NctsCountryOutsideCustomsSecurityAgreementAreaProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_NCTSCountryOutsideCustomsSecurityAgreementArea.zip";

		protected override string InputFileName => "RD_NCTS-P5_NCTSCountryOutsideCustomsSecurityAgreementArea.zip";

		protected override string RefCusCodeListOutputFileName => "RefCusCodeListZZ_EUN_CL247.xml";
	}
}
