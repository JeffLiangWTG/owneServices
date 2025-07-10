using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CUSRESEDIMessage))]
sealed class CUSRESEDIMessageTest : EDIMessageTest<CUSRESEDIMessage>
{
	protected override bool ExpectedIsMessageInterpretationSetterSupported => false;

	public void TestMessageDefaults()
	{
		AssertEquals("EM_MessageType", EDIMessageConstants.MessageTypes.CUSRES, CreateNewMessage().EM_MessageType);
	}

	public void TestLookups() => AssertType<CUSRESEDIMessageLookups>(CreateNewMessage().Lookups);

	public void TestPrettier() => AssertType<CUSRESEDIMessagePrettier>(CreateNewMessage().Prettier);

	CUSRESEDIMessage CreateNewMessage() => Factory.New<CUSRESEDIMessage>();
}
