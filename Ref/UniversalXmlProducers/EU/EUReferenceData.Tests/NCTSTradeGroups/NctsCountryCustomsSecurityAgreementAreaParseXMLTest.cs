using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSTradeGroups;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsCountryCustomsSecurityAgreementAreaParseXMLTest : NctsTradeGroupXMLProducerAbstractTest
	{
		protected override NctsTradeGroupXMLProducer Producer => new NctsCountryCustomsSecurityAgreementAreaProducer();

		protected override string DownloadUrl => "https://ec.europa.eu/taxation_customs/dds2/rd/compressed_file/data_download/RD_NCTS-P5_CountryCustomsSecurityAgreementArea.zip";

		protected override string InputFileName => "RD_NCTS-P5_CountryCustomsSecurityAgreementArea.zip";

		protected override string OutputFileName => "RefCusTradeGroupZZ_EUN_EUSEC.xml";
	}
}
