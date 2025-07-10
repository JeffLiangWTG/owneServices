using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NctsIncidentCodeParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsIncidentCodeParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_IncidentCode.xml",
				codeListDetail: new NctsIncidentCodeType(),
				resultCount: 6
			)
		{
		}
	}
}
