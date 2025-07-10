using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(MessageSendingActionParent))]
sealed class MessageSendingActionParentTest : EU.NCTS.Business.Testing.NctsHeaderMessageSendingObjectParentTest
{
	public new void TestTopLevelBusinessObject()
	{
		AssertType<NctsHeader>(messageSendingActionParent.TopLevelBusinessObject);
	}

	public void TestMessageSendingActions()
	{
		var messageSendingActionParentForTest = new MessageSendingActionParentForTest(nctsHeader);
		CombineAssertions(() =>
		{
			AssertType<MessageSendingActionCollection>(messageSendingActionParentForTest.GetSendingObjectsCollectionCoreExposed);
			AssertType<MessageSendingActionCollection>(messageSendingActionParentForTest.SendingObjectsCollection);
		});
	}

	public new void TestSecurityCheckpointToSendWithMessageError()
	{
		AssertEquals(messageSendingActionParent.SecurityCheckpointToSendWithMessageError, Env.Security.CustomsDeclarationSendWithMessageErrors);
	}

	public void TestMessageSendingObjectProperties()
	{
		var testItem = messageSendingActionParent.MessageSendingObjectProperties.ToList();

		CombineAssertions(() =>
		{
			AssertEquals("Count", 3, testItem.Count);
			AssertEquals("MessageSendingObjectProperties should contain \"LRN\"", true, testItem.Exists(item => item.PropertyName == "LRN"));
			AssertEquals("MessageSendingObjectProperties should contain \"MRN\"", true, testItem.Exists(item => item.PropertyName == "MRN"));
			AssertEquals("MessageSendingObjectProperties should contain \"MessageType\"", true, testItem.Exists(item => item.PropertyName == "MessageType"));
		});
	}

	public void TestGetBizObjValidationMessageErrors()
	{
		var sendingObjectParent = new MessageSendingActionParent(nctsHeader);
		var action = sendingObjectParent.SendingObjectsCollection[0];
		action.MessageType = NctsMessageTypeListNL.Codes.Declaration;
		action.ShouldSend = true;
		CombineAssertions(() =>
		{
			AssertNotNullOrEmpty("MessageType is DEC, BizObjValidationMessageErrors is not empty", sendingObjectParent.BizObjValidationMessageErrors);

			action.MessageType = NctsMessageTypeListNL.Codes.InvalidationCancellation;
			action.ShouldSend = true;
			AssertEquals("MessageType is INV, BizObjValidationMessageErrors is empty", ZString.Empty, sendingObjectParent.BizObjValidationMessageErrors);

			action.MessageType = NctsMessageTypeListNL.Codes.ResponseOnRequestForNonArrivedMovement;
			action.ShouldSend = true;
			AssertEquals("MessageType is RNM, BizObjValidationMessageErrors is empty", ZString.Empty, sendingObjectParent.BizObjValidationMessageErrors);

			action.MessageType = NctsMessageTypeListNL.Codes.RequestARelease;
			action.ShouldSend = true;
			AssertEquals("MessageType is RRL, BizObjValidationMessageErrors is empty", ZString.Empty, sendingObjectParent.BizObjValidationMessageErrors);
		});
	}

	public void TestShowValidationErrors()
	{
		var sendingObjectParent = new MessageSendingActionParent(nctsHeader);
		var action = sendingObjectParent.SendingObjectsCollection[0];
		action.MessageType = NctsMessageTypeListNL.Codes.Declaration;
		action.ShouldSend = true;
		CombineAssertions(() =>
		{
			AssertEquals("MessageType is DEC, ShowValidationErrors is true", true, sendingObjectParent.ShowValidationErrors);

			action.MessageType = NctsMessageTypeListNL.Codes.InvalidationCancellation;
			action.ShouldSend = true;
			AssertEquals("MessageType is INV, ShowValidationErrors is false", false, sendingObjectParent.ShowValidationErrors);

			action.MessageType = NctsMessageTypeListNL.Codes.ResponseOnRequestForNonArrivedMovement;
			action.ShouldSend = true;
			AssertEquals("MessageType is RNM, ShowValidationErrors is false", false, sendingObjectParent.ShowValidationErrors);

			action.MessageType = NctsMessageTypeListNL.Codes.RequestARelease;
			action.ShouldSend = true;
			AssertEquals("MessageType is RRL, ShowValidationErrors is false", false, sendingObjectParent.ShowValidationErrors);
		});
	}

	public void TestSendAndSaveMessages_ShouldCall_AssignUnassignedDeclarationGoodsItemNumbers()
	{
		var sendingObject = messageSendingActionParent.SendingObjectsCollection.Cast<MessageSendingAction>().First();
		sendingObject.MessageType = NctsMessageTypeListNL.Codes.Declaration;
		var bill1 = nctsHeader.Bills.AddNew();
		var goodsItem1 = bill1.GoodsItems.AddNew();

		AssertEquals("The number is not assigned", 0, goodsItem1.BY_DeclarationGoodsItemNumber);
		messageSendingActionParent.SendAndSaveMessages();
		AssertEquals("The number is assigned", 1, goodsItem1.BY_DeclarationGoodsItemNumber);
	}

	public void TestDoSendAction_Departure()
	{
		var sendingObject = messageSendingActionParent.SendingObjectsCollection.Cast<MessageSendingAction>().First();
		CombineAssertions(() =>
		{
			sendingObject.MessageType = "test";
			AssertEquals(messageSendingActionParent.SendAndSaveMessages(), false);
			sendingObject.MessageType = NctsMessageTypeListNL.Codes.ArrivalNotification;
			AssertEquals(messageSendingActionParent.SendAndSaveMessages(), true);
			sendingObject.MessageType = NctsMessageTypeListNL.Codes.Declaration;
			AssertEquals(messageSendingActionParent.SendAndSaveMessages(), true);
			sendingObject.MessageType = NctsMessageTypeListNL.Codes.InvalidationCancellation;
			AssertEquals(messageSendingActionParent.SendAndSaveMessages(), true);
			sendingObject.MessageType = NctsMessageTypeListNL.Codes.ResponseOnRequestForNonArrivedMovement;
			AssertEquals(messageSendingActionParent.SendAndSaveMessages(), true);
			sendingObject.MessageType = NctsMessageTypeListNL.Codes.PresentationNotification;
			AssertEquals(messageSendingActionParent.SendAndSaveMessages(), true);
		});
	}

	public void TestDoSendAction_Arrival()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		messageSendingActionParent = new MessageSendingActionParentForTest(nctsHeader);
		var sendingObject = messageSendingActionParent.SendingObjectsCollection.Cast<MessageSendingAction>().First();
		CombineAssertions(() =>
		{
			sendingObject.MessageType = "test";
			AssertEquals(messageSendingActionParent.SendAndSaveMessages(), false);
			sendingObject.MessageType = NctsMessageTypeListNL.Codes.ArrivalNotification;
			AssertEquals(messageSendingActionParent.SendAndSaveMessages(), true);
			sendingObject.MessageType = NctsMessageTypeListNL.Codes.UnloadingRemarks;
			AssertEquals(messageSendingActionParent.SendAndSaveMessages(), true);
		});
	}

	protected override BusinessObject GetNewBusinessObject() => messageSendingActionParent;

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		messageSendingActionParent = new MessageSendingActionParentForTest(nctsHeader);
	}
	NctsHeader nctsHeader;
	MessageSendingActionParent messageSendingActionParent;
}

class MessageSendingActionParentForTest : MessageSendingActionParent
{
	public MessageSendingActionParentForTest(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	public NonPersistentBusinessObjectCollection<NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCoreExposed => GetSendingObjectsCollectionCore();
}
