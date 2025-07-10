using System;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class MessageSendingConfigurationTest : EU.NCTS.Business.Testing.MessageSendingConfigurationAbstractTest<MessageSendingConfiguration>
{
	public override void TestGetNewNctsMessageSendingObjectParent()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType<MessageSendingObjectParent>(configuration.GetNewNctsHeaderMessageSendingObjectParent(header));
	}

	public override void TestGetShouldSendDefault()
	{
		Assert("There is no changes to be tested here", true);
	}

	public override void TestMessageTypeList()
	{
		var departureHeader = Factory.New<NctsHeader>();
		departureHeader.SetMovementType(NctsMovementType.Codes.Departure);

		var arrivalHeader = Factory.New<NctsHeader>();
		arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		CombineAssertions(() =>
		{
			AssertEquals("Departure", "AMD, DEC, INV, PRN, RNM, RRL", configuration.MessageTypeList(departureHeader).CodesAsString);
			AssertEquals("Arrival", "ARN, RNM, URM", configuration.MessageTypeList(arrivalHeader).CodesAsString);
		});
	}

	public override void TestShowJustification()
	{
		var departureHeader = Factory.New<NctsHeader>();
		departureHeader.SetMovementType(NctsMovementType.Codes.Departure);

		AssertEquals(expected: false, configuration.ShowJustification(departureHeader));
	}

	public override void TestSetDefaultMessageType()
	{
		Assert("There is no changes to be tested here", true);
	}

	public void TestReleaseRequestCode() => AssertEquals(DepartureMessageSendingObjectTypeList.Codes.RRL, configuration.ReleaseRequestCode);

	protected override Type ExpectedNctsHeaderMessageSendingObjectValidationDeciderType => typeof(NctsHeaderMessageSendingObjectValidationDecider);
}
