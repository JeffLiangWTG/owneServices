using CargoWise.RefDbRepo.EUReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	[TestFixture]
	sealed class NctsBusinessRejectionTypeDepExpParseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NctsBusinessRejectionTypeDepExpParseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_BusinessRejectionTypeDepExp.xml",
				codeListDetail: new NctsBusinessRejectionTypeDepExp(),
				resultCount: 6)
		{
		}
	}
}
