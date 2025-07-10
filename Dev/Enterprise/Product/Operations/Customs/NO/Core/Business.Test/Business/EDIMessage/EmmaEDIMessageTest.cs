using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EmmaEDIMessage))]
sealed class EmmaEDIMessageTest : EDIMessageTest<EmmaEDIMessage>
{
	public void TestSetDefaultValues() => CombineAssertions(() =>
	{
		var message = Factory.New<EmmaEDIMessage>();
		AssertEquals("EM_MessageType", EDIMessageConstants.MessageTypes.EMMA, message.EM_MessageType);
		AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
	});
}
