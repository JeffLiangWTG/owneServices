using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(N5168MessageHelper))]
	sealed class N5168MessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageText = TestMessageText;
			testMessage.EM_MessageType = "UHC";
			return TWMessageHelper.NewIncomingHelper(testMessage);
		}

		string TestMessageText => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00225584.N5168.xml");
		string TestHtml => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00225584.N5168.html");
		[ExpectNoExceptions]
		public void TestToHtml()
		{
			var helper = (N5168MessageHelper)GetNewBusinessObject();
			NUnit.Framework.Assert.That(helper.ToHtml(), NUnit.Framework.Is.EqualTo(TestHtml));
		}
	}
}
