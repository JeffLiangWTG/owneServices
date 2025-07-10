using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsRoleOfRequesterParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsRoleOfRequesterParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_RoleRequester.xml",
				codeListDetail: new NctsRoleOfRequesterType(),
				resultCount: 2)
		{
		}
	}
}
