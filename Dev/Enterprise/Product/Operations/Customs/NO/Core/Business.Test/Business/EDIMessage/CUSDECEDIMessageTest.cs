using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CUSDECEDIMessage))]
sealed class CUSDECEDIMessageTest : EDIMessageTest<CUSDECEDIMessage>
{
	public void TestMessageDefaults()
	{
		var message = Factory.New<CUSDECEDIMessage>();
		AssertEquals("EM_MessageType", EDIMessageConstants.MessageTypes.CUSDEC, message.EM_MessageType);
	}
}
