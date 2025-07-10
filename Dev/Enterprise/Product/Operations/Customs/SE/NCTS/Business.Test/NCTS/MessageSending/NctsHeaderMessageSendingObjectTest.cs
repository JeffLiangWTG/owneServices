using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.SE.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObject))]
sealed class NctsHeaderMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestMessageTypeDescription()
	{
		var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);

		sendingObject.MessageType = NCTSArrivalOutgoingMessageTypeList.Codes.ArrivalNotification;
		AssertEquals(
			"MessageTypeDescription should match the code description.",
			(ZString)NCTSArrivalOutgoingMessageTypeList.Descriptions.ArrivalNotification,
			sendingObject.MessageTypeDescription
		);

		sendingObject.MessageType = NCTSArrivalOutgoingMessageTypeList.Codes.UnloadingRemarks;
		AssertEquals(
			"MessageTypeDescription should match the code description.",
			(ZString)NCTSArrivalOutgoingMessageTypeList.Descriptions.UnloadingRemarks,
			sendingObject.MessageTypeDescription
		);
	}

	protected override BusinessObject GetNewBusinessObject() => new NctsHeaderMessageSendingObject(nctsHeader);

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<EU.NCTS.Business.NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
	}

	EU.NCTS.Business.NctsHeader nctsHeader;
}
