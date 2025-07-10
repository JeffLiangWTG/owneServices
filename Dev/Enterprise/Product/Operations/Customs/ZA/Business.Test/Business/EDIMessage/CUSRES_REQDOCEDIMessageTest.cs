using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CUSRES_REQDOCEDIMessage))]
	sealed class CUSRES_REQDOCEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCUSRESHelper()
		{
			var cusresMessage = Factory.New<CUSRESEDIMessage>();
			cusresMessage.EM_MessageText = ZAMessageTest.CUSRESTestMessage.Replace("\r\n", "");
			var helper = cusresMessage.CUSRESHelper;
			AssertNotNull(helper);
			AssertType<CUSRESMessageHelper>(helper);
		}

		public void TestDefaultValues()
		{
			var testMessage = Factory.New<CUSRES_REQDOCEDIMessage>();
			AssertEquals("RSQ", testMessage.EM_MessageType);
		}
	}
}
