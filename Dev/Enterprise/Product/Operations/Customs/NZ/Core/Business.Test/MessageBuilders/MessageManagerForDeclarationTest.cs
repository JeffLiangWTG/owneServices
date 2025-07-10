using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.MasterFiles.Business;
using MessageInitiatorForTesting = Enterprise.Customs.Business.SendsMessagesToCustomsReturningResultsAsProperties;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.Testing
{
	public abstract class MessageManagerForDeclarationTest : MessageManagerForClearanceTest
	{
		public void TestEnteredRemarks()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			manager.EnteredRemarks = "Why Not";
			AssertEquals("Manager.EnteredRemarks", "Why Not", manager.EnteredRemarks);
			AssertEquals("Manager.EntryHeader.CH_CustomsMessageRemarks", "Why Not", manager.EntryHeader.CH_CustomsMessageRemarks);
		}

		public void TestIsOKToSendWithMessagingErrorsOnCancel()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "PK");
			Factory.Save();
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.DeclarationNumber = "12345678";
			Declaration.JE_MasterBill = "081122345678";
			if (Declaration.JE_MessageSubType == JobMessageSubTypeList.Codes.WriteOff)
			{
				Declaration.JE_PaymentMethod = FreightPaymentMethodList.Codes.DF;
				Declaration.JE_HouseBill = "T0753Y";
			}

			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			Declaration.MessageInitiator = new MessageInitiatorForTesting(false);
			Assert(!manager.IsOKToSendWithMessagingErrors());
			AssertMultilineEquals("Should have messaging errors", ExpectedAlphabeticallyOrderedMessageErrorsForTSW, GetSortedMessageErrors(Declaration.Notifications.GetMessageErrors()), '\r');
		}

		public void TestIsOKToSendWithMessagingErrorsContinues()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "PK");
			Factory.Save();
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.DeclarationNumber = "12345678";
			Declaration.JE_MasterBill = "081122345678";
			if (Declaration.JE_MessageSubType == JobMessageSubTypeList.Codes.WriteOff)
			{
				Declaration.JE_PaymentMethod = FreightPaymentMethodList.Codes.AC;
				Declaration.JE_HouseBill = "HB00321";
			}

			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			Declaration.MessageInitiator = new MessageInitiatorForTesting(true);
			Assert(manager.IsOKToSendWithMessagingErrors());
			AssertMultilineEquals("Should have messaging errors", ExpectedAlphabeticallyOrderedMessageErrorsForTSW, GetSortedMessageErrors(Declaration.Notifications.GetMessageErrors()), '\r');
		}

		public void TestMessageTypeToBeSentSendOriginal()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("Manager.MessageTypeToBeSent", MessageManagerForDeclaration.MessageType.Original, manager.MessageTypeToBeSent);
		}

		public virtual void TestMessageTypeToBeSentRejection()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			SetDeclarationToRejected();
			Declaration.DeclarationNumber = "";
			AssertEquals("Manager.MessageTypeToBeSent - rejection when Entry number has not been received needs to send again as an Original", MessageManagerForDeclaration.MessageType.Original, manager.MessageTypeToBeSent);
			Declaration.DeclarationNumber = "12345678";
			AssertEquals("Manager.MessageTypeToBeSent", MessageManagerForDeclaration.MessageType.Replacement, manager.MessageTypeToBeSent);
		}

		public void TestMessageTypeToBeSentInspectionsToRHL()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			SetDeclarationToInspectionsAuditRequirements();
			Declaration.DeclarationNumber = "";
			AssertEquals("Manager.MessageTypeToBeSent", MessageManagerForDeclaration.MessageType.Original, manager.MessageTypeToBeSent);
			Declaration.DeclarationNumber = "12345678";
			AssertEquals("Manager.MessageTypeToBeSent", MessageTypeForCompleteReplacement, manager.MessageTypeToBeSent);
		}

		public void TestMessageTypeToBeSentClearanceOKToRHL()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			AssertEquals("Manager.MessageTypeToBeSent", MessageTypeForCompleteReplacement, manager.MessageTypeToBeSent);
		}

		public void TestMessageTypeToBeSentErrorInLineToRHL()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			SetDeclarationToEntryInError();
			Declaration.DeclarationNumber = "12345678";
			AssertEquals("Manager.MessageTypeToBeSent", MessageTypeForCompleteReplacement, manager.MessageTypeToBeSent);
		}

		public void TestMessageTypeToBeSentErrorInHeaderToRHL()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			SetDeclarationToEntryInError();
			Declaration.DeclarationNumber = "12345678";
			AssertEquals("Manager.MessageTypeToBeSent", MessageTypeForCompleteReplacement, manager.MessageTypeToBeSent);
		}

		public void TestMessageTypeToBeSentErrorInBothToRHL()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			SetDeclarationToEntryInError();
			Declaration.DeclarationNumber = "12345678";
			AssertEquals("Manager.MessageTypeToBeSent", MessageTypeForCompleteReplacement, manager.MessageTypeToBeSent);
		}

		public void TestMessageTypeToBeSentToCustomsToNone()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			SetDeclarationToSentToCustoms();
			AssertEquals("Manager.MessageTypeToBeSent", MessageManagerForDeclaration.MessageType.None, manager.MessageTypeToBeSent);
		}

		public void TestMessageTypeToBeSentCancelEntry()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.CancelMessage);
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			AssertEquals("Manager.MessageTypeToBeSent", MessageManagerForDeclaration.MessageType.CancelEntry, manager.MessageTypeToBeSent);
		}

		public virtual void TestMessageTypeToBeSentResetToOriginal()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.ResetToOriginal);
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			AssertEquals("Manager.MessageTypeToBeSent", MessageManagerForDeclaration.MessageType.ResetToOriginal, manager.MessageTypeToBeSent);
		}

		public void TestIsOkToExecuteReadyToSendOriginal()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("Manager.IsOkToExecute", true, manager.IsOkToExecute);
			AssertEquals("Manager.LastHumanReadableStatus", MessageManagerForDeclaration.MessageReadyToSendMessage, manager.LastHumanReadableStatus);
		}

		public void TestIsOkToExecuteCannotSendIsWaitingResponse()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			SetDeclarationToSentToCustoms();
			AssertEquals("Manager.IsOkToExecute", false, manager.IsOkToExecute);
			AssertEquals("Manager.LastHumanReadableStatus", "Cannot Send Message - Job is Waiting for a Response", manager.LastHumanReadableStatus);
		}

		public void TestIsOkToExecuteCannotSendWithoutClearanceNum()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.CancelMessage);
			AssertEquals("Manager.IsOkToExecute", false, manager.IsOkToExecute);
			AssertEquals("Manager.LastHumanReadableStatus", "Cannot Send Message - Cannot Cancel when a Clearance Number has never been received From Customs.", manager.LastHumanReadableStatus);
		}

		public void TestIsOkToExecuteReadyToSendCancellation()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.CancelMessage);
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			AssertEquals("Manager.IsOkToExecute", true, manager.IsOkToExecute);
			AssertEquals("Manager.LastHumanReadableStatus", MessageManagerForDeclaration.MessageReadyToSendMessage, manager.LastHumanReadableStatus);
		}

		public virtual void TestIsOkToExecuteCannotResetToOriginal()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.ResetToOriginal);
			AssertEquals("Manager.IsOkToExecute: Should be able to reset to original even when there are no messages", true, manager.IsOkToExecute);
		}

		public virtual void TestIsOkToExecuteResetToOriginal()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.ResetToOriginal);
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			AssertEquals("Manager.IsOkToExecute", true, manager.IsOkToExecute);
			AssertEquals("Manager.LastHumanReadableStatus", "Ready to Reset to Original", manager.LastHumanReadableStatus);
		}

		public void TestExecuteOriginalEntry()
		{
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("Manager.Execute()", true, manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Original Entry Message " + MessageManagerForDeclaration.MessageReportingImmediateSend, manager.LastHumanReadableStatus);
		}

		public abstract void TestExecuteReplaceRejectedEntry();

		public void TestExecuteReplaceHeaderAndLines()
		{
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("Manager.Execute()", true, manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Replacement Entry Message " + MessageManagerForDeclaration.MessageReportingImmediateSend, manager.LastHumanReadableStatus);
		}

		public void TestExecuteCancelEntry()
		{
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.CancelMessage);
			AssertEquals("Manager.Execute()", true, manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Cancel Entry Message " + MessageManagerForDeclaration.MessageReportingImmediateSend, manager.LastHumanReadableStatus);
		}

		public virtual void TestExecuteResetToOriginal()
		{
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.ResetToOriginal);
			AssertEquals("Manager.Execute()", true, manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Job has been 'Reset to Original'", manager.LastHumanReadableStatus);
		}

		public virtual void TestExecuteResetToOriginalWhenNoResponseReceived()
		{
			SetDeclarationToSentToCustoms();
			Declaration.DeclarationNumber = "";
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.ResetToOriginal);
			AssertEquals("Manager.Execute()", true, manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Job has been 'Reset to Original'", manager.LastHumanReadableStatus);
		}

		public void TestMessageTypeAndStatusForCancellationOk()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.CancelMessage);
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			AssertEquals("Manager.IsOkToExecute", true, manager.IsOkToExecute);
			AssertEquals("Manager.MessageTypeAndStatus", "Ready to Send Cancellation Message", manager.MessageTypeAndStatus);
		}

		public void TestMessageTypeAndStatusForCancellationNotOk()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.CancelMessage);
			Declaration.DeclarationNumber = "";
			AssertEquals("Manager.IsOkToExecute", false, manager.IsOkToExecute);
			AssertEquals("Manager.MessageTypeAndStatus", "Cannot Send Message - Cannot Cancel when a Clearance Number has never been received From Customs.", manager.MessageTypeAndStatus);
		}

		public void TestMiscellaneousDeclarationErrors()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals("Manager.IsOkToExecute", false, manager.IsOkToExecute);
			AssertEquals("Manager.LastHumanReadableStatus", MessageManagerForDeclaration.PreconditionCannotSendMiscDeclaration, manager.LastHumanReadableStatus);
		}

		public void TestEntryCancelledErrors()
		{
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			Declaration.CusEntryHeader.CH_IsEntryCancelled = true;
			AssertEquals("Manager.IsOkToExecute", false, manager.IsOkToExecute);
			AssertEquals("Manager.LastHumanReadableStatus", MessageManagerForDeclaration.PreconditionCannotSendWhenEntryHasBeenCancelled, manager.LastHumanReadableStatus);
		}

		#region Implementation
		protected abstract MessageManager.MessageType MessageTypeForCompleteReplacement { get; }
		protected abstract string ExpectedAlphabeticallyOrderedMessageErrors { get; }
		protected abstract string ExpectedAlphabeticallyOrderedMessageErrorsForTSW { get; }

		protected abstract void SetDeclarationToSentToCustoms();
		protected abstract void SetDeclarationToClearanceOK();
		protected abstract void SetDeclarationToRejected();
		protected abstract void SetDeclarationToInspectionsAuditRequirements();
		protected abstract void SetDeclarationToEntryInError();

		JobDeclaration fDeclaration;
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = GetNewJobDeclaration();
					fDeclaration.DisableDefaultPackingInformation = true;
					fDeclaration.JE_EDITransmitDate = Declaration.CachedTodaysDate;
					fDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
					fDeclaration.Invoices.AddNew();
					fDeclaration.InvoiceLines.AddNew();
				}
				return fDeclaration;
			}
		}

		MessageManagerForDeclaration GetNewManager(MessageManager.OperationType operationType)
		{
			return (MessageManagerForDeclaration)GetNewMessageManager(operationType);
		}

		protected abstract JobDeclaration GetNewJobDeclaration();

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "00009917B");
		}
		#endregion
	}
}
