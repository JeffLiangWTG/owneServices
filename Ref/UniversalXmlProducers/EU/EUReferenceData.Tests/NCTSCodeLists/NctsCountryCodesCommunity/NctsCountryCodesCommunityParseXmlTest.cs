using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsCountryCodesCommunityParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsCountryCodesCommunityParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_CountryCodesCommunity.xml",
				codeListDetail: new NctsCountryCodesCommunityType(),
				resultCount: 36
			)
		{
		}
	}
}
