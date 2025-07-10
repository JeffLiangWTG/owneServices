using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Manifest.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class N5101HMessageHelperTest : TransactionedTestCase
	{
		public void TestToHtml()
		{
			var testXml = TWManifestXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Manifest.Business.Testing.BatchProcessor.TestFile.WI00533326.N5101H.xml");
			var testHtml = TWManifestXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Manifest.Business.Testing.BatchProcessor.TestFile.WI00533326.N5101H.html");
			var helper = CareateN5101HessageHelper(testXml);
			var html = helper.ToHtml();
			AssertXMLEquals("xml must be the same", testHtml, html);
		}

		N5101HMessageHelper CareateN5101HessageHelper(string testXml)
		{
			var factory = new BusinessObjectFactory();
			var testMessage = factory.NewWithValidTestData<AsycudaMessage>();
			testMessage.EM_MessageType = "FHM";
			testMessage.EM_MessageText = testXml;
			return new N5101HMessageHelper(testMessage);
		}
	}
}
