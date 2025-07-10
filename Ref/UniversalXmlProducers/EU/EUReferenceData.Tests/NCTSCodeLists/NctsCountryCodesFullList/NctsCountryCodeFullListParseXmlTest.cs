using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsCountryCodeFullListParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsCountryCodeFullListParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_CountryCodesFullList.xml",
				codeListDetail: new NctsCountryCodesFullList(),
				resultCount: 265)
		{
		}
	}
}
