using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSCodeLists.CountryCodesCTC.DocumentTypeExcise
{
	sealed class NctsCountryOutsideCustomsSecurityAgreementAreaParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsCountryOutsideCustomsSecurityAgreementAreaParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_NCTSCountryOutsideCustomsSecurityAgreementArea.xml",
				codeListDetail: new NctsCountryOutsideCustomsSecurityAgreementArea(),
				resultCount: 5
			)
		{
		}
	}
}
