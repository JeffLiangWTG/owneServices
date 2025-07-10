using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(SARSEDIMessage))]
	abstract class SARSEDIMessageAbstractTest : Messaging.Testing.EDIMessageTest
	{
		public void TestReceiveTransmit()
		{
			ZAMessage message = (ZAMessage)GetNewBusinessObject();
			AssertEquals(ExpectedReceiveTransmit, message.EM_ReceiveTransmit);
		}

		public virtual void TestIsTestMessage()
		{
			ZAMessage message = (ZAMessage)GetNewBusinessObject();
			Env.Registry.ZACustoms.SetIsTestMode(message.Branch, true);
			message = (ZAMessage)GetNewBusinessObject();
			AssertEquals("Test Mode", true, message.EM_IsTestMessage);
			Env.Registry.ZACustoms.SetIsTestMode(message.Branch, false);
			message = (ZAMessage)GetNewBusinessObject();
			AssertEquals("Test Mode", false, message.EM_IsTestMessage);
		}

		protected virtual string ExpectedReceiveTransmit => SARSEDIMessage.Direction.Transmit;
	}
}
