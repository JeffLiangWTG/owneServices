using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(MessageSendingAction))]
	class MessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestResetValidationModesAndUnRegisterBillAsEditableChildObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00032432";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.ValidationModes = ValidationModes.PermitToTransfer;
			var bill2 = header.Bills.AddNew();
			bill2.ValidationModes = ValidationModes.SubsequentInBond;
			var moveHeader = header.MovementHeader;
			var amsMoveDetail1 = bill1.MovementDetail;
			var amsMoveDetail2 = bill2.MovementDetail;
			AssertEquals(2, header.Bills.Count);

			var sendingAction = new MessageSendingAction(header, ActionCode.Creating);
			AssertEquals(2, sendingAction.MessageSendingObjects.Count);

			header.ValidationModes = ValidationModes.PermitToTransfer;
			AssertEquals(ValidationModes.PermitToTransfer, bill1.ValidationModes);
			AssertEquals(ValidationModes.PermitToTransfer, bill2.ValidationModes);

			header.ValidationModes = ValidationModes.InventoryRecord;
			AssertEquals(ValidationModes.InventoryRecord, bill1.ValidationModes);
			AssertEquals(ValidationModes.InventoryRecord, bill2.ValidationModes);

			sendingAction.ResetValidationModesAndUnRegisterBillAsEditableChildObject();
			AssertEquals(ValidationModes.PermitToTransfer, bill1.ValidationModes);
			AssertEquals(ValidationModes.SubsequentInBond, bill2.ValidationModes);
		}

		public void TestMovements()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00032432";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var moveHeader = header.MovementHeader;
			var inBondMoveHeader1 = header.InBondMovementHeaders.AddNew();
			inBondMoveHeader1.InBondNumber = "INB21132";
			inBondMoveHeader1.BM_InBondEntryType = "61";
			var inBondMoveHeader2 = header.InBondMovementHeaders.AddNew();
			inBondMoveHeader2.InBondNumber = "INB86545";
			inBondMoveHeader2.BM_InBondEntryType = "62";

			var sendingAction = new MessageSendingAction(header, ActionCode.Creating);
			AssertEquals(1, sendingAction.Movements.Count);
			var movement = sendingAction.Movements[0];
			AssertEquals("C00032432", movement.MM_RelatedDetails);
			AssertEquals(true, movement.MM_Send);

			foreach (var actionCode in new[] {
				ActionCode.InBondArrival,
				ActionCode.SubsequentInBondOriginal,
				ActionCode.SubsequentInBondDelete,
				ActionCode.SubsequentInBondAmendment,
				ActionCode.InBondExportation,
				ActionCode.InBondTransferOfLiability })
			{
				sendingAction = new MessageSendingAction(header, actionCode);
				AssertEquals(2, sendingAction.Movements.Count);
				var movement1 = sendingAction.Movements[0];
				var movement2 = sendingAction.Movements[1];
				if (movement2.MM_RelatedDetails == "In-Bond Number: INB21132")
				{
					movement1 = sendingAction.Movements[1];
					movement2 = sendingAction.Movements[0];
				}
				AssertEquals("In-Bond Number: INB21132, Entry Type: 61", movement1.MM_RelatedDetails);
				AssertEquals(false, movement1.MM_Send);
				AssertEquals("In-Bond Number: INB86545, Entry Type: 62", movement2.MM_RelatedDetails);
				AssertEquals(false, movement2.MM_Send);
			}
		}

		public void TestMessageSendingObjects()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00032432";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "MB2";
			var moveHeader = header.MovementHeader;
			var amsMoveDetail1 = bill1.MovementDetail;
			var amsMoveDetail2 = bill2.MovementDetail;
			var inBondMoveHeader1 = header.InBondMovementHeaders.AddNew();
			inBondMoveHeader1.InBondNumber = "INB21132";
			var inBondMoveHeader1Detail1 = inBondMoveHeader1.MovementDetails.AddNew(bill1.PK);
			var inBondMoveHeader1Detail2 = inBondMoveHeader1.MovementDetails.AddNew(bill2.PK);
			var inBondMoveHeader2 = header.InBondMovementHeaders.AddNew();
			inBondMoveHeader2.InBondNumber = "INB86545";
			var inBondMoveHeader2Detail1 = inBondMoveHeader2.MovementDetails.AddNew(bill1.PK);
			var inBondMoveHeader2Detail2 = inBondMoveHeader2.MovementDetails.AddNew(bill2.PK);

			var sendingAction = new MessageSendingAction(header, ActionCode.SubsequentInBondOriginal);
			AssertEquals("No In-Bond movement has been marked for sending", 0, sendingAction.MessageSendingObjects.Count);
			AssertEquals(2, sendingAction.Movements.Count);
			var movement1 = sendingAction.Movements[0];
			var movement2 = sendingAction.Movements[1];
			if (movement2.MM_RelatedDetails == "In-Bond Number: INB21132")
			{
				movement1 = sendingAction.Movements[1];
				movement2 = sendingAction.Movements[0];
			}
			movement1.MM_Send = false;
			movement2.MM_Send = true;
			AssertEquals(2, sendingAction.MessageSendingObjects.Count);
			var moveDetail1 = (MessageSendingObject)sendingAction.MessageSendingObjects.FindByPK(inBondMoveHeader2Detail1.PK);
			var moveDetail2 = (MessageSendingObject)sendingAction.MessageSendingObjects.FindByPK(inBondMoveHeader2Detail2.PK);
			AssertEquals(ActionCode.SubsequentInBondOriginal, moveDetail1.ActionCode);
			AssertEquals(ActionCode.SubsequentInBondOriginal, moveDetail2.ActionCode);

			sendingAction = new MessageSendingAction(header, ActionCode.SubsequentInBondAmendment);
			AssertEquals(2, sendingAction.Movements.Count);
			movement1 = sendingAction.Movements[0];
			movement2 = sendingAction.Movements[1];
			if (movement2.MM_RelatedDetails == "In-Bond Number: INB21132")
			{
				movement1 = sendingAction.Movements[1];
				movement2 = sendingAction.Movements[0];
			}
			movement1.MM_Send = false;
			movement2.MM_Send = true;
			AssertEquals(2, sendingAction.MessageSendingObjects.Count);
			AssertEquals(moveDetail1, (MessageSendingObject)sendingAction.MessageSendingObjects.FindByPK(inBondMoveHeader2Detail1.PK));
			AssertEquals(moveDetail2, (MessageSendingObject)sendingAction.MessageSendingObjects.FindByPK(inBondMoveHeader2Detail2.PK));
			AssertEquals(ActionCode.SubsequentInBondAmendment, moveDetail1.ActionCode);
			AssertEquals(ActionCode.SubsequentInBondAmendment, moveDetail2.ActionCode);

			sendingAction = new MessageSendingAction(header, ActionCode.SubsequentInBondDelete);
			AssertEquals(2, sendingAction.Movements.Count);
			movement1 = sendingAction.Movements[0];
			movement2 = sendingAction.Movements[1];
			if (movement2.MM_RelatedDetails == "In-Bond Number: INB21132")
			{
				movement1 = sendingAction.Movements[1];
				movement2 = sendingAction.Movements[0];
			}
			movement1.MM_Send = false;
			movement2.MM_Send = true;
			AssertEquals(2, sendingAction.MessageSendingObjects.Count);
			AssertEquals(moveDetail1, (MessageSendingObject)sendingAction.MessageSendingObjects.FindByPK(inBondMoveHeader2Detail1.PK));
			AssertEquals(moveDetail2, (MessageSendingObject)sendingAction.MessageSendingObjects.FindByPK(inBondMoveHeader2Detail2.PK));
			AssertEquals(ActionCode.SubsequentInBondDelete, moveDetail1.ActionCode);
			AssertEquals(ActionCode.SubsequentInBondDelete, moveDetail2.ActionCode);
		}

		public void TestCreateAMSMessagesWhenMessageErrorsAllowed()
		{
			var header = Factory.New<CusInBondHeader>();
			header.Bills.AddNew().RunPreSaveValidation();

			var action = new MessageSendingAction(header, ActionCode.Creating);
			action.RunPreSaveValidation();
			AssertEquals(1, action.CreateAMSMessages());
			Assert("pre condition", action.HasMessageErrors);
			var messages = header.MovementHeader.Messages;
			AssertEquals(1, messages.Count);

			var message = messages[0];
			Assert(message.EM_SendWithMessageErrors);
		}

		public void TestCreateAMSMessagesWhenMessageErrorsDisallowed()
		{
			var header = Factory.New<CusInBondHeader>();
			header.Bills.AddNew();

			var action = new MessageSendingAction(header, ActionCode.Creating, false);
			AssertEquals(1, action.CreateAMSMessages());

			var messages = header.MovementHeader.Messages;
			AssertEquals(1, messages.Count);

			var message = messages[0];
			Assert(!message.EM_SendWithMessageErrors);
		}

		[TestDate(2012, 04, 05)]
		public void TestCreateAMS_ACEMessages()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill1 = header.Bills.AddNew();
			var moveDetail1 = bill1.MovementDetail;
			var bill2 = header.Bills.AddNew();
			var moveDetail2 = bill2.MovementDetail;
			var action = new MessageSendingAction(header, ActionCode.Creating);
			AssertEquals(2, action.CreateAMSMessages());
			AssertEquals(2, moveHeader.Messages.Count);
		}

		public void TestCreateAMS_ActionReplaceEntireBillDeleteAndAdd()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var action = new MessageSendingAction(header, ActionCode.AmendingAdd);
			var message = action.MessageSendingObjects[0];
			message.MB_BillActionCode = AMSBillSendingActionCodeList.Codes.ReplaceEntireBillDeleteAndAdd;
			AssertEquals(1, action.CreateAMSMessages());
		}

		public void TestGetWarningForBillsToSendThatAreWaitingForResponse()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill1 = header.Bills.AddNew();
			var moveDetail1 = bill1.MovementDetail;
			var bill2 = header.Bills.AddNew();
			var moveDetail2 = bill2.MovementDetail;
			var bill3 = header.Bills.AddNew();
			var moveDetail3 = bill3.MovementDetail;

			var action = new MessageSendingAction(header, ActionCode.AmendingUpdate);
			AssertEquals("Precondition", 3, action.MessageSendingObjects.Count);
			var billToSend1 = action.MessageSendingObjects[0];
			billToSend1.MB_Send = ZBool.True;
			billToSend1.MB_MessageStatus = ZString.Empty;
			billToSend1.MB_IssuerCode = "OTT1";
			billToSend1.MB_BillOfLadingSequenceNumber = "BOL123";
			var billToSend2 = action.MessageSendingObjects[1];
			billToSend2.MB_Send = ZBool.True;
			billToSend2.MB_MessageStatus = ZString.Empty;
			billToSend2.MB_IssuerCode = ZString.Empty;
			billToSend2.MB_BillOfLadingSequenceNumber = "BOL456";
			var billToSend3 = action.MessageSendingObjects[2];
			billToSend3.MB_Send = ZBool.True;
			billToSend3.MB_MessageStatus = ZString.Empty;
			billToSend3.MB_IssuerCode = "OTT4";
			billToSend3.MB_BillOfLadingSequenceNumber = ZString.Empty;
			AssertEquals("Precondition", 3, action.ObjectsToSend.Count);
			AssertEquals(ZString.Empty, action.GetWarningForEntitiesToSendThatAreWaitingForResponse());
			billToSend1.MB_MessageStatus = AMSBillMessageStatusList.Codes.Adding;
			billToSend2.MB_MessageStatus = AMSBillMessageStatusList.Codes.Updating;
			AssertEquals(MessageSendingAction.EntitiesToSendThatAreWaitingForResponseMessage("OTT1 BOL123\r\nBOL456"), action.GetWarningForEntitiesToSendThatAreWaitingForResponse());
			billToSend2.MB_MessageStatus = AMSBillMessageStatusList.Codes.Updated;
			billToSend3.MB_MessageStatus = AMSBillMessageStatusList.Codes.Deleting;
			AssertEquals(MessageSendingAction.EntitiesToSendThatAreWaitingForResponseMessage("OTT1 BOL123\r\nOTT4"), action.GetWarningForEntitiesToSendThatAreWaitingForResponse());

			billToSend1.MB_Send = false;
			AssertEquals(MessageSendingAction.EntitiesToSendThatAreWaitingForResponseMessage("OTT4"), action.GetWarningForEntitiesToSendThatAreWaitingForResponse());
		}

		public void TestBillsToSend()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill1 = header.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			var moveDetail1 = bill1.MovementDetail;
			var bill2 = header.Bills.AddNew();
			bill2.B0_MasterBillNumber = "MB2";
			var moveDetail2 = bill2.MovementDetail;
			var bill3 = header.Bills.AddNew();
			bill3.B0_MasterBillNumber = "MB3";
			var moveDetail3 = bill3.MovementDetail;
			var action = new MessageSendingAction(header, ActionCode.AmendingAdd);
			AssertEquals(3, action.MessageSendingObjects.Count);
			action.MessageSendingObjects[1].MB_Send = false;
			action.MessageSendingObjects[2].MB_Send = false;
			AssertEquals(1, action.ObjectsToSend.Count);
			AssertEquals(action.MessageSendingObjects[0].MB_BillOfLadingSequenceNumber, action.ObjectsToSend[0].MB_BillOfLadingSequenceNumber);
			action.MessageSendingObjects[2].MB_Send = true;
			AssertEquals(2, action.ObjectsToSend.Count);
			AssertEquals(action.MessageSendingObjects[0].MB_BillOfLadingSequenceNumber, action.ObjectsToSend[0].MB_BillOfLadingSequenceNumber);
			AssertEquals(action.MessageSendingObjects[2].MB_BillOfLadingSequenceNumber, action.ObjectsToSend[1].MB_BillOfLadingSequenceNumber);
		}

		public void TestMessageSendObjectsForVesselArrival()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_TransitDirection = DirectionTypeList.Codes.MVOCC;
			var moveHeader = (CusInBondMoveHeader)header.MovementHeaders.AddNew();
			var bill0 = header.Bills.AddNew();
			bill0.MovementDetail.B9_CustomsStatus = "FIL";
			bill0.B0_InBondPortOfDestDCode = "1101";
			var bill1 = header.Bills.AddNew();
			bill1.MovementDetail.B9_CustomsStatus = "FIL";
			bill1.B0_InBondPortOfDestDCode = "1001";
			var action = new MessageSendingAction(header, ActionCode.VesselArrival);
			AssertEquals(2, action.MessageSendingObjects.Count);
			AssertEquals("1101", action.MessageSendingObjects[0].MB_PortOfUnlading);
			AssertEquals("1001", action.MessageSendingObjects[1].MB_PortOfUnlading);

			action = new MessageSendingAction(header, ActionCode.ChangeEstDateOfArrival);
			AssertEquals(2, action.MessageSendingObjects.Count);
			AssertEquals("1101", action.MessageSendingObjects[0].MB_PortOfUnlading);
			AssertEquals("1001", action.MessageSendingObjects[1].MB_PortOfUnlading);

			action = new MessageSendingAction(header, ActionCode.VesselDeparture);
			AssertEquals(1, action.MessageSendingObjects.Count);
			AssertEquals(ZString.Empty, action.MessageSendingObjects[0].MB_PortOfUnlading);

			header.PortArrivalDetails.AddNewIfNotExist("1001", ZDateTime.Today);
			action = new MessageSendingAction(header, ActionCode.VesselArrival);
			AssertEquals(1, action.MessageSendingObjects.Count);

			header.PortArrivalDetails.AddNewIfNotExist("1101", ZDateTime.Today);
			action = new MessageSendingAction(header, ActionCode.VesselArrival);
			AssertEquals(0, action.MessageSendingObjects.Count);

			action = new MessageSendingAction(header, ActionCode.ChangeEstDateOfArrival);
			AssertEquals(2, action.MessageSendingObjects.Count);

			action = new MessageSendingAction(header, ActionCode.VesselDeparture);
			AssertEquals(1, action.MessageSendingObjects.Count);
		}

		public void TestSend1000MessagesWith10MinuteInterval()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var helper = new AutoSendUSAMSMessageProcessor(header).AMSMessageHelper;
			var batchSize = helper.BatchSize;
			var delay = helper.Delay;
			for (int i = 0; i < batchSize * 4; i++)
			{
				var bill = header.Bills.AddNew();
			}

			var action = new MessageSendingAction(header, ActionCode.AmendingAdd);
			AssertEquals(batchSize * 4, action.MessageSendingObjects.Count);
			for (int i = 3 * batchSize; i < batchSize * 4; i++)
			{
				action.MessageSendingObjects[i].MB_BillActionCode = AMSBillSendingActionCodeList.Codes.DeleteBill;
			}
			AssertEquals(batchSize * 4, action.CreateAMSMessages());
			moveHeader.Messages.Sort(AMSEDIMessage.Schema.EM_HeldUntilDate);

			for (int i = 0; i < batchSize; i++)
			{
				AssertEquals(moveHeader.Messages[i].EM_HeldUntilDate, ZDateTime.Empty);
				AssertEquals(moveHeader.Messages[i + 2 * batchSize].EM_HeldUntilDate, moveHeader.Messages[i + batchSize].EM_HeldUntilDate.AddMinutes(delay));
				AssertEquals(moveHeader.Messages[i + 3 * batchSize].EM_HeldUntilDate, moveHeader.Messages[i + 2 * batchSize].EM_HeldUntilDate.AddMinutes(delay));
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			return new MessageSendingAction(header, ActionCode.Creating);
		}
	}
}
