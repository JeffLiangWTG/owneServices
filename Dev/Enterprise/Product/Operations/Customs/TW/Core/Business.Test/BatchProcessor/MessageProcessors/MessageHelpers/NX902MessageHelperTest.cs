using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX902MessageHelper))]
	sealed class NX902MessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<CusTWControllingMessageHeader>();
			header.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX201_01;
			var testMessage = Factory.NewWithValidTestData<TWMessage>();
			testMessage.EM_MessageText = TestMessageText;
			testMessage.EM_MessageType = MessageTypeList.Codes._902;
			return TWMessageHelper.NewIncomingHelper(testMessage, header);
		}

		string TestMessageText => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00655217.NX902.xml");
		string TestHtml => TWXmlTestCaseWithFactory.GetExpectedMessageXML(@"Enterprise.Customs.TW.Business.Testing.BatchProcessor.TestFile.WI00655217.NX902.html");

		[ExpectNoExceptions]
		public void TestToHtml()
		{
			var helper = (NX902MessageHelper)GetNewBusinessObject();
			NUnit.Framework.Assert.That(helper.ToHtml(), NUnit.Framework.Is.EqualTo(TestHtml));
		}
	}
}
