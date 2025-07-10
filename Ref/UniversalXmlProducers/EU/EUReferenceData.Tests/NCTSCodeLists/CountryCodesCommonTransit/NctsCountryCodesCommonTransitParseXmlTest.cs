using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests.NCTSCodeLists.CountryCodesCTC.CountryCodesCommonTransit
{
	sealed class NctsCountryCodesCommonTransitParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsCountryCodesCommonTransitParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_CountryCodesCommonTransit.xml",
				codeListDetail: new NctsCountryCodesCommonTransit(),
				resultCount: 52
			)
		{
		}
	}
}
