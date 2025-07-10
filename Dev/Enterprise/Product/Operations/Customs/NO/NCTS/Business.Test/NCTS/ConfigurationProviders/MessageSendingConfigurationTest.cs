using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.Business.MessageSending.Testing;

[TestedType(typeof(MessageSendingConfiguration))]
sealed class MessageSendingConfigurationTest : MessageSendingConfigurationAbstractTest<MessageSendingConfiguration>
{
	protected override Type ExpectedNctsHeaderMessageSendingObjectValidationDeciderType => typeof(NctsHeaderMessageSendingObjectValidationDecider);

	public override void TestGetNewNctsMessageSendingObjectParent()
	{
		AssertType<NctsHeaderMessageSendingObjectParent>(configuration.GetNewNctsHeaderMessageSendingObjectParent(header));
	}

	public override void TestGetShouldSendDefault()
	{
		var sendingObj = new NctsHeaderMessageSendingObject(header);
		Assert(configuration.GetShouldSendDefault(sendingObj));
	}

	public override void TestMessageTypeList()
	{
		AssertType<NctsArrivalMessageTypeCodeList>("Arrival MessageTypes", configuration.MessageTypeList(GetNewNctsHeader(NctsMovementType.Codes.Arrival)));
		AssertType<NctsDepartureMessageTypeCodeList>("Departure MessageTypes", configuration.MessageTypeList(GetNewNctsHeader(NctsMovementType.Codes.Departure)));
	}

	public override void TestSetDefaultMessageType()
	{
		CombineAssertions("Arrival Movement", () =>
		{
			var arrivalHeader = GetNewNctsHeader(NctsMovementType.Codes.Arrival);
			var arrivalMovementHeader = arrivalHeader.ArrivalMovementHeader;

			var arrivalMessageSendingObject = new NctsHeaderMessageSendingObject(arrivalHeader);
			AssertEquals("When BM_CustomsStatus is empty", NctsArrivalMessageTypeCodeList.Codes.ArrivalNotification, arrivalMessageSendingObject.MessageType);

			arrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;
			arrivalMessageSendingObject = new NctsHeaderMessageSendingObject(arrivalHeader);
			AssertEquals("When BM_CustomsStatus is UAP", NctsArrivalMessageTypeCodeList.Codes.UnloadingRemarks, arrivalMessageSendingObject.MessageType);

			arrivalMovementHeader.BM_CustomsStatus = NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
			arrivalMessageSendingObject = new NctsHeaderMessageSendingObject(arrivalHeader);
			AssertEquals("When BM_CustomsStatus is other than UAP", NctsArrivalMessageTypeCodeList.Codes.ArrivalNotification, arrivalMessageSendingObject.MessageType);
		});

		var sendingObject = new NctsHeaderMessageSendingObject(header);
		configuration.SetDefaultMessageType(sendingObject);
		AssertEquals("Departure Movement", NctsDepartureMessageTypeCodeList.Codes.HelpMeDecide, sendingObject.MessageType);
	}

	public override void TestShowJustification()
	{
		_ = new NctsHeaderMessageSendingObject(header);
		AssertEquals(expected: true, configuration.ShowJustification(header));
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = GetNewNctsHeader(NctsMovementType.Codes.Departure);
	}

	NctsHeader GetNewNctsHeader(string movementType)
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(movementType);
		return header;
	}

	NctsHeader header;
}
