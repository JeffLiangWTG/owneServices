using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.MessageProcessor.Testing
{
	[TestedType(typeof(REQDOCMessageHelper))]
	sealed class REQDOCMessageHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageSender()
		{
			var helper = (REQDOCMessageHelper)GetNewBusinessObject();
			AssertEquals("51051342TST", helper.MessageSender);
		}

		public void TestLocalReferenceNumber()
		{
			var helper = (REQDOCMessageHelper)GetNewBusinessObject();
			AssertEquals("TestReference1", helper.LocalReferenceNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var testMessage = Factory.NewWithValidTestData<REQDOCEDIMessage>();
			testMessage.EM_MessageText = REQDOCTestMessage.Replace("\r\n", "");
			return REQDOCMessageHelper.New(testMessage);
		}

		const string REQDOCTestMessage = @"UNH+202+REQDOC:D:99B:UN:ZZZ01'
BGM+929+TestReference1+9'
DOC+929'
NAD+MS+51051342TST'
LIN+1'
UNT+6+202'";
	}
}
