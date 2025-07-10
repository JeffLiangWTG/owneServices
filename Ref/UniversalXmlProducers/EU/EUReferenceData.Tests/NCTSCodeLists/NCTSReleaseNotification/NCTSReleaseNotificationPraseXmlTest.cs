using CargoWise.RefDbRepo.EUReferenceData.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Tests
{
	sealed class NCTSReleaseNotificationPraseXmlTest : NCTSCodeListParserAbstractTest
	{
		public NCTSReleaseNotificationPraseXmlTest()
			: base(
				inputFileName: "RD_NCTS-P5_ReleaseNotification.xml",
				codeListDetail: new NCTSReleaseNotification(),
				resultCount: 4)
		{
		}
	}
}
