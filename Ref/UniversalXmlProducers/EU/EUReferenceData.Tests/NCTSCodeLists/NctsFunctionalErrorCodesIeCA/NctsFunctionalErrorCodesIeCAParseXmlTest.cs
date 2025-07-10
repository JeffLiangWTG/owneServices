using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsFunctionalErrorCodesIeCAParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsFunctionalErrorCodesIeCAParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_FunctionalErrorCodesIeCA.xml",
				codeListDetail: new NctsFunctionalErrorCodesIeCA(),
				resultCount: 11)
		{
		}
	}
}
