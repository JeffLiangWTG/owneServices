using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(OutboundEDIMessage))]
sealed class OutboundEDIMessageTest : Messaging.Testing.EDIMessageTest
{
	public void TestEM_ReceiveTransmit_DefaultValue()
	{
		var outboundMessage = Factory.New<OutboundEDIMessage>();
		AssertEquals("EM_ReceiveTransmit initial value", "TRX", outboundMessage.EM_ReceiveTransmit);
	}
}
