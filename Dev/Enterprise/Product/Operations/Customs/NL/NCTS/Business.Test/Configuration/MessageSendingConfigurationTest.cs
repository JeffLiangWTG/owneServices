using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

sealed class MessageSendingConfigurationTest : EU.NCTS.Business.Testing.MessageSendingConfigurationAbstractTest<MessageSendingConfiguration>
{
	public override void TestGetNewNctsMessageSendingObjectParent()
	{
		AssertType<MessageSendingActionParent>(configuration.GetNewNctsHeaderMessageSendingObjectParent(header));
	}

	public override void TestMessageTypeList()
	{
		Assert("There is no changes to be tested here", true);
	}

	public override void TestSetDefaultMessageType()
	{
		Assert("There is no changes to be tested here", true);
	}

	public override void TestShowJustification()
	{
		Assert("There is no changes to be tested here", true);
	}

	public override void TestGetShouldSendDefault()
	{
		Assert("There is no changes to be tested here", true);
	}

	protected override Type ExpectedNctsHeaderMessageSendingObjectValidationDeciderType => typeof(NctsHeaderMessageSendingObjectValidationDecider);

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
	}
	NctsHeader header;
}
