using System;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.SE.NCTS.Business.Testing;

sealed class MessageSendingConfigurationTest : EU.NCTS.Business.Testing.MessageSendingConfigurationAbstractTest<MessageSendingConfiguration>
{
	protected override Type ExpectedNctsHeaderMessageSendingObjectValidationDeciderType => typeof(EU.NCTS.Business.NctsHeaderMessageSendingObjectValidationDecider);

	public override void TestGetNewNctsMessageSendingObjectParent()
	{
		AssertType<NctsHeaderMessageSendingObjectParent>(
			"GetNewNctsHeaderMessageSendingObjectParent should return SE NctsHeaderMessageSendingObjectParent",
			configuration.GetNewNctsHeaderMessageSendingObjectParent(arrivalNctsHeader)
		);
	}

	public override void TestGetShouldSendDefault()
	{
		Assert("GetShouldSendDefault not changed.", true);
	}

	public override void TestMessageTypeList()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(arrivalNctsHeader);
		AssertType<NCTSArrivalOutgoingMessageTypeList>("Arrival MessageTypeList", configuration.MessageTypeList(arrivalNctsHeader));

		var departureNctsHeader = Factory.New<EU.NCTS.Business.NctsHeader>();
		departureNctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var departureSendingObject = new NctsHeaderMessageSendingObject(departureNctsHeader);
		AssertType<NCTSDepartureOutgoingMessageTypeList>("Departure MessageTypeList", configuration.MessageTypeList(departureNctsHeader));
	}

	public override void TestSetDefaultMessageType()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(arrivalNctsHeader);
		configuration.SetDefaultMessageType(sendingObject);
		AssertEquals("Default MessageType 007 by default.", NCTSArrivalOutgoingMessageTypeList.Codes.ArrivalNotification, sendingObject.MessageType);

		arrivalNctsHeader.ArrivalMovementHeader.BM_CustomsStatus = EU.NCTS.Business.CodeDescriptionPairLists.NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks;
		configuration.SetDefaultMessageType(sendingObject);
		AssertEquals("Default MessageType 044 when BM_CustomsStatus is ULR.", NCTSArrivalOutgoingMessageTypeList.Codes.UnloadingRemarks, sendingObject.MessageType);

		var departureNctsHeader = Factory.New<EU.NCTS.Business.NctsHeader>();
		departureNctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		sendingObject = new NctsHeaderMessageSendingObject(departureNctsHeader);
		configuration.SetDefaultMessageType(sendingObject);
		AssertEquals("Default MessageType for Departure with no MRN.", "015", sendingObject.MessageType);

		departureNctsHeader.MovementReferenceEntryNumber.CE_EntryNum = "SE25MRN00909238";
		configuration.SetDefaultMessageType(sendingObject);
		AssertEquals("Default MessageType for Departure with MRN.", "014", sendingObject.MessageType);

		departureNctsHeader.MovementHeader.BM_AdditionalDeclarationType = "D";
		configuration.SetDefaultMessageType(sendingObject);
		AssertEquals("Default MessageType for Departure with MRN and CusInBondMoveHeader.BM_AdditionalDeclarationType == 'D'.", "170", sendingObject.MessageType);
	}

	public override void TestShowJustification()
	{
		Assert("ShowJustification not changed.", true);
	}

	protected override void SetUp()
	{
		base.SetUp();
		arrivalNctsHeader = Factory.New<EU.NCTS.Business.NctsHeader>();
		arrivalNctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
	}
	EU.NCTS.Business.NctsHeader arrivalNctsHeader;
}

