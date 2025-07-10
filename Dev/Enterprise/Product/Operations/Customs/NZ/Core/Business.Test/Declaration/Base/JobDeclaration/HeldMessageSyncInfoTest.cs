namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.Integration;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(HeldMessageSyncInfo))]
	public class HeldMessageSyncInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldRecreateMessage()
		{
			var testHeldMessageInfo = new HeldMessageSyncInfo(null);
			AssertEquals("ShouldRecreateMessage intial value?", false, testHeldMessageInfo.ShouldRecreateMessage);
			testHeldMessageInfo.ShouldRecreateMessage = true;
			AssertEquals("ShouldRecreateMessage?", true, testHeldMessageInfo.ShouldRecreateMessage);
		}

		public void TestRemarksForLeavingMessageUnchanged()
		{
			var testHeldMessageInfo = new HeldMessageSyncInfo(null);
			AssertEquals("RemarksForLeavingMessageUnchanged intialy empty", true, testHeldMessageInfo.RemarksForLeavingMessageUnchanged.IsEmpty);
			testHeldMessageInfo.RemarksForLeavingMessageUnchanged = ZString.Replicate('X', 128);
			AssertEquals("RemarksForLeavingMessageUnchanged", ZString.Replicate('X', 128), testHeldMessageInfo.RemarksForLeavingMessageUnchanged);
			testHeldMessageInfo.RemarksForLeavingMessageUnchanged = "Test Remarks";
			AssertEquals("RemarksForLeavingMessageUnchanged", "Test Remarks", testHeldMessageInfo.RemarksForLeavingMessageUnchanged);

			try
			{
				testHeldMessageInfo.RemarksForLeavingMessageUnchanged = ZString.Replicate('X', 129);
				Fail("Should throw MaxLengthExceededException");
			}
			catch (MaxLengthExceededException)
			{
				AssertEquals("RemarksForLeavingMessageUnchanged", "Test Remarks", testHeldMessageInfo.RemarksForLeavingMessageUnchanged);
			}
			finally
			{
				Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestHasChangesToHeldMessages()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "65432198B");

			// Dec1
			var dec1 = CreateDeclarationQueuedForSending();
			var heldMessageChangeInfo1 = new HeldMessageSyncInfo(dec1.CusEntryHeader);
			AssertEquals("Dec1 HasChangesToHeldMessages?", false, heldMessageChangeInfo1.SetHasChangesToHeldMessages());
			dec1.JE_DateOfArrival = dec1.JE_DateOfArrival.AddDays(2);
			AssertEquals("Dec1 HasChangesToHeldMessages?", true, heldMessageChangeInfo1.SetHasChangesToHeldMessages());
			// Change Message Held until date to the past.
			dec1.CusEntryHeader.Messages[0].EM_HeldUntilDate = ZDate.Today.AddDays(-2);
			AssertEquals("Dec1 HasChangesToHeldMessages?", false, heldMessageChangeInfo1.SetHasChangesToHeldMessages());

			// Dec 2
			var dec2 = CreateDeclarationQueuedForSending();
			var heldMessageChangeInfo2 = new HeldMessageSyncInfo(dec2.CusEntryHeader);
			AssertEquals("Dec2 HasChangesToHeldMessages?", false, heldMessageChangeInfo2.SetHasChangesToHeldMessages());
			dec2.JE_DateOfArrival = dec2.JE_DateOfArrival.AddDays(2);
			AssertEquals("Dec2 HasChangesToHeldMessages?", true, heldMessageChangeInfo2.SetHasChangesToHeldMessages());
			// Mark Message as sent (other then queued)
			dec2.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			AssertEquals("Dec2 HasChangesToHeldMessages?", false, heldMessageChangeInfo2.SetHasChangesToHeldMessages());

			// Dec 3
			var dec3 = CreateDeclarationQueuedForSending();
			var heldMessageChangeInfo3 = new HeldMessageSyncInfo(dec3.CusEntryHeader);
			AssertEquals("Dec3 HasChangesToHeldMessages?", false, heldMessageChangeInfo3.SetHasChangesToHeldMessages());
			dec3.JE_DateOfArrival = dec3.JE_DateOfArrival.AddDays(2);
			AssertEquals("Dec3 HasChangesToHeldMessages?", true, heldMessageChangeInfo3.SetHasChangesToHeldMessages());
			// Add DAC event (acknowledgement)
			var dacLog = dec3.CusEntryHeader.Logs.AddNew(Events.DeclarationAmendedPermitApproved, "Test Remarks");
			AssertEquals("Dec3 HasChangesToHeldMessages?", true, heldMessageChangeInfo3.SetHasChangesToHeldMessages());
			// Cancel DAC event
			dacLog.Cancel();
			AssertEquals("Dec3 HasChangesToHeldMessages?", true, heldMessageChangeInfo3.SetHasChangesToHeldMessages());

			// Dec 4
			var dec4 = CreateDeclarationQueuedForSending();
			var heldMessageChangeInfo4 = new HeldMessageSyncInfo(dec4.CusEntryHeader);
			AssertEquals("Dec4 HasChangesToHeldMessages?", false, heldMessageChangeInfo4.SetHasChangesToHeldMessages());
			dec4.JE_Folio = "Irrelevant";
			AssertEquals("Dec4 HasChangesToHeldMessages?", false, heldMessageChangeInfo4.SetHasChangesToHeldMessages());
		}

		public void TestPerformHeldMessageActionsAndSave()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "65432198B");

			var declaration = CreateDeclarationQueuedForSending();

			var heldMessageChangeInfo = new HeldMessageSyncInfo(declaration.CusEntryHeader);
			AssertEquals("HasChangesToHeldMessages?", false, heldMessageChangeInfo.SetHasChangesToHeldMessages());

			declaration.JE_MasterBill = "123456789";
			AssertEquals("HasChangesToHeldMessages?", true, heldMessageChangeInfo.SetHasChangesToHeldMessages());
			AssertEquals("HeldMessageInfo.ShouldRecreateMessage?", true, heldMessageChangeInfo.ShouldRecreateMessage);
			AssertEquals("HeldMessageInfo.ShouldLeaveMessageUnchanged?", false, heldMessageChangeInfo.ShouldLeaveMessageUnchanged);
			AssertEquals("HeldMessageInfo.RemarksForLeavingMessageUnchanged", "", heldMessageChangeInfo.RemarksForLeavingMessageUnchanged);
			AssertEquals("Held Message Status", EDIMessage.Status.Queued, declaration.CusEntryHeader.Messages[0].EM_Status);

			// Option 3: Cancel -> No actions
			heldMessageChangeInfo.ShouldRecreateMessage = false;
			heldMessageChangeInfo.PerformHeldMessageActions();
			AssertEquals("Entry Status", FormalEntryStatusList.Codes.QueuedForSending, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Held Message Status", EDIMessage.Status.Queued, declaration.CusEntryHeader.Messages[0].EM_Status);
			var ackLogs = declaration.CusEntryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeclarationAmendedPermitApproved.Code));
			AssertEquals("No DAC event logged", 0, ackLogs.Length);

			// Option 1: Recreate message -> DoHeldMessageActions should cancel held message
			heldMessageChangeInfo.ShouldRecreateMessage = true;
			heldMessageChangeInfo.PerformHeldMessageActions();
			AssertEquals("Entry Status should be not sent", FormalEntryStatusList.Codes.NotSentToCustoms, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Held Message should be cancelled", EDIMessage.Status.Cancelled, declaration.CusEntryHeader.Messages[0].EM_Status);
			ackLogs = declaration.CusEntryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeclarationAmendedPermitApproved.Code));
			AssertEquals("No DAC event logged", 0, ackLogs.Length);

			// Option 2: Leave message unchanged -> DoHeldMessageActions should log a DAC event with acknowledgment remarks
			heldMessageChangeInfo.ShouldRecreateMessage = false;
			heldMessageChangeInfo.ShouldLeaveMessageUnchanged = true;
			heldMessageChangeInfo.RemarksForLeavingMessageUnchanged = "TestDoHeldMessageActions_Remarks";
			heldMessageChangeInfo.PerformHeldMessageActions();
			ackLogs = declaration.CusEntryHeader.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeclarationAmendedPermitApproved.Code));
			AssertEquals("DAC event logged with remarcks", "TestDoHeldMessageActions_Remarks", ackLogs[0].SL_Reference);

			// After cancel message or create ack event -> HasChangesToHeldMessages should be false.
			AssertEquals("After cancel message and create ack event -> HasChangesToHeldMessages?", false, heldMessageChangeInfo.SetHasChangesToHeldMessages());
		}

		public void TestChangeRelevantPropertiesUpToDate()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "65432198B");
			var declaration = CreateDeclarationQueuedForSending();
			var initialMessage = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original).GetMessageText();

			var msgSyncInfo = new HeldMessageSyncInfo(declaration.CusEntryHeader);
			AssertEquals("After initial generation: HasChangesToHeldMessages?", false, msgSyncInfo.SetHasChangesToHeldMessages());

			declaration.JE_MasterBill = "3333";
			AssertMessageChanges("JE_MasterBill", msgSyncInfo);
			declaration.JE_RL_NKPortOfLoading = "3";
			AssertMessageChanges("JE_RL_NKPortOfLoading", msgSyncInfo);
			declaration.JE_RL_NKFinalDestination = "3";
			AssertMessageChanges("JE_RL_NKFinalDestination", msgSyncInfo);
			declaration.JE_DateOfArrival = DateTime.UtcNow.AddMonths(1);
			AssertMessageChanges("JE_DateOfArrival", msgSyncInfo);
			declaration.JE_VoyageFlightNo = "3333";
			AssertMessageChanges("JE_VoyageFlightNo", msgSyncInfo);
			declaration.JE_VesselName = "3333";
			AssertMessageChanges("JE_VesselName", msgSyncInfo);

			var changedMessage = new MessageBuilder(declaration.CusEntryHeader, MessageBuilder.MessageTypes.Original).GetMessageText();
			AssertNotEquals("Message after changes", initialMessage, changedMessage);
		}

		void AssertMessageChanges(string ptyName, HeldMessageSyncInfo msgSyncInfo)
		{
			AssertEquals("After modifying [" + ptyName + "]: HasChangesToHeldMessages?", true, msgSyncInfo.SetHasChangesToHeldMessages());
			Factory.Save();
			AssertEquals("After Save: HasChangesToHeldMessages?", false, msgSyncInfo.SetHasChangesToHeldMessages());
		}

		JobDeclaration CreateDeclarationQueuedForSending()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			declaration.CusEntryHeader.CH_EDITransmitDate = ZDate.Today.AddDays(7);
			declaration.JE_DateOfArrival = ZDate.Today.AddDays(10);

			var message = declaration.CusEntryHeader.Messages.AddNew();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_HeldUntilDate = declaration.CusEntryHeader.CH_EDITransmitDate;
			message.EM_MessageText += "<<MSGNO PLACEHOLDER>>";

			Factory.Save();
			return declaration;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new HeldMessageSyncInfo(null);
		}
	}
}
