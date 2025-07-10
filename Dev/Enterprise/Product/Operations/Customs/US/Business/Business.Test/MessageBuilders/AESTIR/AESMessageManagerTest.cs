using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs.US;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AESMessageManagerTest : DeclarationTestHelper
	{
		public void TestLightValidationIsValid()
		{
			var helper = new DeclarationTestHelper(Factory);
			var declaration = helper.CreateAirExportDeclaration();
			var aesDeclaration = new AESDeclarationForTesting(declaration);
			Factory.Save();
			AssertNotEquals("PreCondition: declaration.JE_DeclarationReference", "", declaration.JE_DeclarationReference);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("PreCondition: declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertNotEquals("entryHeader.CH_BGMReference", "", entryHeader.CH_BGMReference);
			AssertEquals("entryHeader.CH_Status", AESDirectCustomsEntryStatus.Codes.NotSent, entryHeader.CH_Status);
			AssertEquals("entryHeader.CH_EntryStatus", AESDirectCustomsEntryStatus.Codes.NotSent, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.Messages.Count", 0, entryHeader.Messages.Count);

			entryHeader.US_ShouldBeReportToCustoms = true;
			AESMessageManager.SubmitToCustoms(aesDeclaration);
			AssertEquals("entryHeader.CH_Status", AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, entryHeader.CH_Status);
			AssertEquals("entryHeader.CH_EntryStatus", AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.Messages.Count", 1, entryHeader.Messages.Count);
			AESTIREDIMessage message = (AESTIREDIMessage)entryHeader.Messages[0];
			AssertAESMessage(message);

			Assert(entryHeader.LightValidationIsValid);
		}

		public void TestMergeAndCheckForAESTIR()
		{
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var filer = new ExportEntryFilerID();
			filer.EntryFilerID = "123456789";
			filer.EntryFilerIDType = "D";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			var filerOnBranchLevel = new ExportEntryFilerID();
			filerOnBranchLevel.EntryFilerID = "123-00-4567";
			filerOnBranchLevel.EntryFilerIDType = "S";
			USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, filerOnBranchLevel);

			AssertNotNull(ExportForwarderContact);
			JobDeclaration declaration = CreateSeaExportDeclaration();
			declaration.US_TransportReference = "";
			declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = ZString.Empty;
			OrgContact exportForwarderContact = ExportForwarderContact;
			JobComInvoiceHeader invoice1 = declaration.Invoices[0];
			invoice1.US_ImportEntryNo = "INV1543";
			AssertNotEquals("US_ImportEntryNo", declaration.US_ImportEntryNo, invoice1.US_ImportEntryNo);
			AssertHasMessageErrors(declaration.US_TransportReferenceInfo);
			ZString transportReferenceErrorMessage = declaration.US_TransportReferenceInfo.GetMessageErrors().GetFirstMessage();
			var manager = new AESMessageManager(declaration);
			manager.OnSave += Manager_OnSave;

			SendsMessagesToCustomsShutterUpperer sendsMessages = new SendsMessagesToCustomsShutterUpperer(false);
			sendsMessages.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = sendsMessages;
			sendsMessages.AnswerToContinueWithAction = false;
			AssertEquals("PreCondition: IsInDatabase", false, declaration.IsInDatabase);
			AssertEquals("PreCondition: ActiveEntryHeaders.Count", 0, declaration.ActiveEntryHeaders.Count);
			sendsMessages.ContinueWithActionMessage = "";
			sendsMessages.LastErrors = null;
			onSaveCount = 0;
			AssertEquals(false, manager.MergeAndCheck());
			AssertEquals("IsInDatabase", false, declaration.IsInDatabase);
			AssertEquals(0, onSaveCount);

			sendsMessages.AnswerToContinueWithAction = true;
			AssertEquals(true, manager.MergeAndCheck());
			AssertEquals("IsInDatabase", true, declaration.IsInDatabase);
			AssertEquals(1, onSaveCount);
			ZString lastMessage = sendsMessages.ContinueWithActionMessage + sendsMessages.LastErrorsAsString + sendsMessages.InvalidOperationText;
			AssertContains("Error message should contain", transportReferenceErrorMessage, lastMessage);
			AssertNotContains("Error message should not contain", AESMessageManager.EntryFilerIDForThisNotSetup, lastMessage);
			AssertEquals("ActiveEntryHeaders.Count", 2, declaration.ActiveEntryHeaders.Count);

			declaration.US_TransportReference = "BKF33234";
			sendsMessages.ContinueWithActionMessage = "";
			sendsMessages.LastErrors = null;
			AssertEquals(true, manager.MergeAndCheck());
			AssertEquals("IsInDatabase", true, declaration.IsInDatabase);
			AssertEquals(2, onSaveCount);
			lastMessage = sendsMessages.ContinueWithActionMessage + sendsMessages.LastErrorsAsString + sendsMessages.InvalidOperationText;
			AssertNotContains("Error message should not contain", transportReferenceErrorMessage, lastMessage);
			AssertEquals("ActiveEntryHeaders.Count", 2, declaration.ActiveEntryHeaders.Count);

			((IRegistryItemInternals)USCustomsDataRegistry.Instance.ExportEntryFilerID).DeleteValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			sendsMessages.ContinueWithActionMessage = "";
			sendsMessages.LastErrors = null;
			AssertEquals(true, manager.MergeAndCheck());
			AssertEquals("IsInDatabase", true, declaration.IsInDatabase);
			AssertEquals(2, onSaveCount);
			lastMessage = sendsMessages.ContinueWithActionMessage + sendsMessages.LastErrorsAsString + sendsMessages.InvalidOperationText;
			AssertNotContains("Error message should not contain", transportReferenceErrorMessage, lastMessage);
			AssertNotContains("Error message should not contain", AESMessageManager.EntryFilerIDForThisNotSetup, lastMessage);
			AssertEquals("ActiveEntryHeaders.Count", 2, declaration.ActiveEntryHeaders.Count);

			sendsMessages.ContinueWithActionMessage = "";
			sendsMessages.LastErrors = null;
			foreach (Customs.Business.CusEntryHeader entryHeader in declaration.ActiveEntryHeaders)
			{
				entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
				entryHeader.EntryNumber = "ITN1234";
			}
			onSaveCount = 0;
			AssertEquals(true, manager.MergeAndCheck());
			AssertEquals(1, onSaveCount);
			lastMessage = sendsMessages.ContinueWithActionMessage + sendsMessages.LastErrorsAsString + sendsMessages.InvalidOperationText;
			AssertNotContains("Error message should not contain", AESMessageManager.EntryFilerIDForThisNotSetup, lastMessage);
			AssertNotContains("Error message should not contain", transportReferenceErrorMessage, lastMessage);
			AssertEquals("ActiveEntryHeaders.Count", 2, declaration.ActiveEntryHeaders.Count);
		}

		public void TestCanSendAESTIR()
		{
			CustomsDataRegistry.Instance.AutoAllocateContainerToInvoiceLines.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var filer = new ExportEntryFilerID();
			filer.EntryFilerID = "36-123456700";
			filer.EntryFilerIDType = "E";

			using (USCustomsDataRegistry.Instance.ExportEntryFilerID.DataType.SuspendValidation())
			{
				USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
			}

			var declaration = CreateSeaExportDeclaration();
			var manager = new AESMessageManager(declaration);
			manager.OnSave += Manager_OnSave;

			var sendsMessages = new SendsMessagesToCustomsShutterUpperer(false);
			sendsMessages.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = sendsMessages;
			AssertEquals(true, manager.MergeAndCheck());

			var lastMessage = sendsMessages.ContinueWithActionMessage + sendsMessages.LastErrorsAsString + sendsMessages.InvalidOperationText;
			AssertNotContains("Error message should not contain", AESMessageManager.IDFormat, lastMessage);

			filer = new ExportEntryFilerID();
			filer.EntryFilerID = "36-1234567";
			filer.EntryFilerIDType = "E";

			using (USCustomsDataRegistry.Instance.ExportEntryFilerID.DataType.SuspendValidation())
			{
				USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
			}

			sendsMessages = new SendsMessagesToCustomsShutterUpperer(false);
			sendsMessages.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = sendsMessages;
			AssertEquals(true, manager.MergeAndCheck());

			lastMessage = sendsMessages.ContinueWithActionMessage + sendsMessages.LastErrorsAsString + sendsMessages.InvalidOperationText;
			AssertNotContains("Error message should not contain", AESMessageManager.IDFormat, lastMessage);

			filer = new ExportEntryFilerID();
			filer.EntryFilerID = "361-23-45670";
			filer.EntryFilerIDType = "S";

			using (USCustomsDataRegistry.Instance.ExportEntryFilerID.DataType.SuspendValidation())
			{
				USCustomsDataRegistry.Instance.ExportEntryFilerID.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);
			}

			sendsMessages = new SendsMessagesToCustomsShutterUpperer(false);
			sendsMessages.AnswerToContinueWithAction = true;
			declaration.MessageInitiator = sendsMessages;
			AssertEquals(false, manager.MergeAndCheck());

			lastMessage = sendsMessages.ContinueWithActionMessage + sendsMessages.LastErrorsAsString + sendsMessages.InvalidOperationText;
			AssertContains("Error message should contains", AESMessageManager.IDFormat, lastMessage);
		}

		public void TestEntrySubmittedDateAndCustomsCommencedIsSet()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			JobDeclaration declaration = helper.CreateAirExportDeclaration();
			var aesDeclaration = new AESDeclarationForTesting(declaration);
			Factory.Save();
			AssertNotEquals("PreCondition: declaration.JE_DeclarationReference", "", declaration.JE_DeclarationReference);
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			AssertEquals("PreCondition: JE_EntrySubmittedDate", ZDateTime.Empty, declaration.JE_EntrySubmittedDate);
			AssertEquals("PreCondition: declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			AssertNull(declaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced));
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.US_ShouldBeReportToCustoms = false;
			AESMessageManager.SubmitToCustoms(aesDeclaration);
			AssertEquals(ZDateTime.Empty, declaration.JE_EntrySubmittedDate);

			entryHeader.US_ShouldBeReportToCustoms = true;
			AESMessageManager.SubmitToCustoms(aesDeclaration);
			Assert("JE_EntrySubmittedDate", declaration.JE_EntrySubmittedDate.IsValid);
			Assert("CH_EntrySubmittedDate", entryHeader.CH_EntrySubmittedDate.IsValid);

			AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.ExportCustomsCommenced));
		}

		public void TestSubmitToCustoms()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			JobDeclaration declaration = helper.CreateAirExportDeclaration();
			var aesDeclaration = new AESDeclarationForTesting(declaration);
			Factory.Save();
			AssertNotEquals("PreCondition: declaration.JE_DeclarationReference", "", declaration.JE_DeclarationReference);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("PreCondition: declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertNotEquals("entryHeader.CH_BGMReference", "", entryHeader.CH_BGMReference);
			AssertEquals("entryHeader.CH_Status", AESDirectCustomsEntryStatus.Codes.NotSent, entryHeader.CH_Status);
			AssertEquals("entryHeader.CH_EntryStatus", AESDirectCustomsEntryStatus.Codes.NotSent, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.Messages.Count", 0, entryHeader.Messages.Count);

			entryHeader.US_ShouldBeReportToCustoms = false;
			AESMessageManager.SubmitToCustoms(aesDeclaration);
			AssertEquals("entryHeader.CH_Status", AESDirectCustomsEntryStatus.Codes.NotSent, entryHeader.CH_Status);
			AssertEquals("entryHeader.CH_EntryStatus", AESDirectCustomsEntryStatus.Codes.NotSent, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.Messages.Count", 0, entryHeader.Messages.Count);

			entryHeader.US_ShouldBeReportToCustoms = true;
			AESMessageManager.SubmitToCustoms(aesDeclaration);
			AssertEquals("entryHeader.CH_Status", AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, entryHeader.CH_Status);
			AssertEquals("entryHeader.CH_EntryStatus", AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.Messages.Count", 1, entryHeader.Messages.Count);
			AESTIREDIMessage message = (AESTIREDIMessage)entryHeader.Messages[0];
			AssertAESMessage(message);

			ZGuid oldMessagePK = message.PK;
			entryHeader.US_ShouldBeReportToCustoms = true;
			AESMessageManager.SubmitToCustoms(aesDeclaration);
			AssertEquals("entryHeader.CH_Status", AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, entryHeader.CH_Status);
			AssertEquals("entryHeader.CH_EntryStatus", AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.Messages.Count", 2, entryHeader.Messages.Count);

			message = (AESTIREDIMessage)entryHeader.Messages[0];
			AESTIREDIMessage message2 = (AESTIREDIMessage)entryHeader.Messages[1];
			if (message.PK != oldMessagePK)
			{
				message = (AESTIREDIMessage)entryHeader.Messages[1];
				message2 = (AESTIREDIMessage)entryHeader.Messages[0];
			}
			AssertAESMessage(message);
			AssertAESMessage(message2);

			AESTIREDIMessage responseMessage = Factory.New<AESTIREDIMessage>();
			responseMessage.EM_ReceiveTransmit = AESTIREDIMessage.Direction.Receive;
			entryHeader.Messages.Add(responseMessage);
			entryHeader.CH_EntryStatus = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entryHeader.LogManager.AddAClearLogIfNecessary(AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, AESDirectCustomsEntryStatus.Codes.OriginalSEDClear, entryHeader.StatusList);
			Factory.Save();
			AESMessageManager.SubmitToCustoms(aesDeclaration);
			AssertEquals("entryHeader.CH_Status", AESDirectCustomsEntryStatus.Codes.AwaitingReplacementResponse, entryHeader.CH_Status);
			AssertEquals("entryHeader.CH_EntryStatus", AESDirectCustomsEntryStatus.Codes.OriginalSEDClear, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.Messages.Count", 4, entryHeader.Messages.Count);
		}

		[TestDate(2010, 07, 19, 07, 36, 50)]
		public void TestSubmitToCustomsAfterWithdrawalGeneratesAnAddMessage()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);
			JobDeclaration declaration = helper.CreateAirExportDeclaration();
			var aesDeclaration = new AESDeclarationForTesting(declaration);
			Factory.Save();
			AssertNotEquals("PreCondition: declaration.JE_DeclarationReference", "", declaration.JE_DeclarationReference);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			declaration.RunPreSaveValidation();
			Factory.Save();
			AssertEquals("PreCondition: declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			AssertNotEquals("entryHeader.CH_BGMReference", "", entryHeader.CH_BGMReference);
			AssertEquals("entryHeader.CH_Status", AESDirectCustomsEntryStatus.Codes.NotSent, entryHeader.CH_Status);
			AssertEquals("entryHeader.CH_EntryStatus", AESDirectCustomsEntryStatus.Codes.NotSent, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.Messages.Count", 0, entryHeader.Messages.Count);

			entryHeader.US_ShouldBeReportToCustoms = true;
			AESMessageManager.SubmitToCustoms(aesDeclaration);
			AssertEquals("entryHeader.CH_Status", AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, entryHeader.CH_Status);
			AssertEquals("entryHeader.CH_EntryStatus", AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.CH_BGMReference", "B00001000", entryHeader.CH_BGMReference);
			AssertEquals("entryHeader.Messages.Count", 1, entryHeader.Messages.Count);
			AESTIREDIMessage message = (AESTIREDIMessage)entryHeader.Messages[0];
			AssertAESMessage(message);

			ZGuid oldMessagePK = message.PK;
			entryHeader.US_ShouldBeReportToCustoms = true;
			AESMessageManager.SubmitToCustoms(aesDeclaration);
			AssertEquals("entryHeader.CH_Status", AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, entryHeader.CH_Status);
			AssertEquals("entryHeader.CH_EntryStatus", AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.Messages.Count", 2, entryHeader.Messages.Count);

			message = (AESTIREDIMessage)entryHeader.Messages[0];
			AESTIREDIMessage message2 = (AESTIREDIMessage)entryHeader.Messages[1];
			if (message.PK != oldMessagePK)
			{
				message = (AESTIREDIMessage)entryHeader.Messages[1];
				message2 = (AESTIREDIMessage)entryHeader.Messages[0];
			}
			AssertAESMessage(message);
			AssertAESMessage(message2);

			AESTIREDIMessage responseMessage = Factory.New<AESTIREDIMessage>();
			responseMessage.EM_ReceiveTransmit = AESTIREDIMessage.Direction.Receive;
			entryHeader.Messages.Add(responseMessage);
			entryHeader.CH_EntryStatus = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entryHeader.LogManager.AddAClearLogIfNecessary(AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, AESDirectCustomsEntryStatus.Codes.OriginalSEDClear, entryHeader.StatusList);
			entryHeader.US_SendWithdrawn = true;
			Factory.Save();
			AESMessageManager.SubmitToCustoms(aesDeclaration);
			AssertEquals("entryHeader.CH_Status", AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse, entryHeader.CH_Status);
			AssertEquals("entryHeader.CH_EntryStatus", AESDirectCustomsEntryStatus.Codes.OriginalSEDClear, entryHeader.CH_EntryStatus);
			AssertEquals("entryHeader.Messages.Count", 4, entryHeader.Messages.Count);

			AESTIREDIMessage responseMessage2 = Factory.New<AESTIREDIMessage>();
			responseMessage.EM_ReceiveTransmit = AESTIREDIMessage.Direction.Receive;
			entryHeader.Messages.Add(responseMessage);
			entryHeader.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;
			//entryHeader.GetNewBGMReferenceIfEntryPreviouslyWithdrawn();
			entryHeader.US_ShouldBeReportToCustoms = true;
			Factory.Save();
			declaration.DoMerge();
			//AssertEquals("entryHeader.CH_BGMReference", "E0010071907365001", entryHeader.CH_BGMReference);
			AESMessageManager.SubmitToCustoms(aesDeclaration);
			AssertEquals("entryHeader.CH_Status - submission after an entry has been deleted should be a new ADD (Original) message with updated BGM reference", AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, entryHeader.CH_Status);
		}

		int onSaveCount;
		void Manager_OnSave()
		{
			onSaveCount++;
			Factory.Save();
		}

		void AssertAESMessage(AESTIREDIMessage message)
		{
			AssertEquals("EM_MessageType", ApplicationIdentifierCodeList.AES.CommodityShipment, message.EM_MessageType);
			AssertEquals("EM_ApplicationCode", AESTIREDIMessage.ApplicationCodes.USCustomsExport, message.EM_ApplicationCode);
			AssertEquals("EM_Status", AESTIREDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("EM_ReceiveTransmit", AESTIREDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
		}

		sealed class AESDeclarationForTesting : IAESDeclaration
		{
			public AESDeclarationForTesting(JobDeclaration declaration)
			{
				this.declaration = declaration;
			}
			readonly JobDeclaration declaration;

			void IAESDeclaration.LogCustomsCommencedIfNeeded() => declaration.LogCustomsCommencedIfNeeded();

			BusinessObjectFactory IAESDeclaration.Factory => declaration.Factory;

			ISendsMessagesToCustoms IAESDeclaration.MessageInitiator => declaration.MessageInitiator;

			IEnumerable<IAESEntry> IAESDeclaration.ActiveEntryHeaders
			{
				get
				{
					foreach (IAESEntry entry in declaration.ActiveEntryHeaders)
					{
						yield return entry;
					}
				}
			}
		}
	}
}
