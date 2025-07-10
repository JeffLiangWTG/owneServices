using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsCountryCodesCTCParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsCountryCodesCTCParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_CountryCodesCTC.xml",
				codeListDetail: new NctsCountryCodesCTC(),
				resultCount: 8
			)
		{
		}
	}
}
