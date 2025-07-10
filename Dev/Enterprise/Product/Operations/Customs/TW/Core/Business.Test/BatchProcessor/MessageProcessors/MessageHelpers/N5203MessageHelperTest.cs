using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5203MessageHelper))]
	sealed class N5203MessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageType = "ECD";
			testMessage.EM_MessageText = TestXml;
			return new N5203MessageHelper(testMessage);
		}

		public static string TestXml => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00333496.N5203.xml");
		public static string TestHtml => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00333496.N5203.html");
		public void TestToHtml()
		{
			var helper = (N5203MessageHelper)GetNewBusinessObject();
			var html = helper.ToHtml();
			AssertXMLEquals(TestHtml, html);
		}

		public void TestNoExceptionThrownWhenGoodsShipmentAdditionalDocumentIDIsNull()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageType = "ECD";
			testMessage.EM_MessageText = TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00567634.N5203.xml");
			var messageHelper = new N5203MessageHelper(testMessage);
			AssertNoExceptionThrown("additionalDocument.ID is null", () => messageHelper.ToHtml());
		}
	}
}
