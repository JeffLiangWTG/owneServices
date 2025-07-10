using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsNationalityParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsNationalityParseXmlTest()
			: base(
			"RD_NCTS-P5_Nationality.xml",
			new NctsNationality(),
			3
			)
		{
		}
	}
}
