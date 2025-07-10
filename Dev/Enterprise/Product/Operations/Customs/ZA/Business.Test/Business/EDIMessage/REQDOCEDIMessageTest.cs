using Enterprise.Customs.Universal.Messaging;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(REQDOCEDIMessage))]
	sealed class REQDOCEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLocalReferenceNumber()
		{
			var reqdocMessage = Factory.New<REQDOCEDIMessage>();
			reqdocMessage.EM_MessageText = REQDOCTestMessage.Replace("\r\n", "");
			AssertEquals("TestReference1", reqdocMessage.LocalReferenceNumber);
		}

		public void TestParentMessageNumber()
		{
			var reqdocMessage = Factory.New<REQDOCEDIMessage>();
			reqdocMessage.EM_MessageText = REQDOCTestMessage.Replace("\r\n", "");
			reqdocMessage.EM_MessageNum = "202";
			AssertEquals("202", reqdocMessage.ParentMessageNumber);
		}

		public void TestSenderID()
		{
			var reqdocMessage = Factory.New<REQDOCEDIMessage>();
			reqdocMessage.EM_MessageText = REQDOCTestMessage.Replace("\r\n", "");
			AssertEquals("51051342TST", ((IInterchangeSenderIdProvider)reqdocMessage).SenderID);
		}

		public void TestDefaultValues()
		{
			var testMessage = Factory.New<REQDOCEDIMessage>();
			AssertEquals("REQ", testMessage.EM_MessageType);
		}

		const string REQDOCTestMessage = @"UNH+202+REQDOC:D:99B:UN:ZZZ01'
BGM+929+TestReference1+9'
DOC+929'
NAD+MS+51051342TST'
LIN+1'
UNT+6+202'";
	}
}
