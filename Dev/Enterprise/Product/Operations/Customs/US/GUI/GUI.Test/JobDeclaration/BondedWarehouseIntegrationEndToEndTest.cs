using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class BondedWarehouseIntegrationEndToEndTest : Customs.Business.Testing.BondedWarehouseIntegrationEndToEndTest<JobDeclaration, JobComInvoiceLine, Business.OrgSupplierPart, CusClassification, CusClassPartPivot>
	{
		#region Change Of Ownership
		protected override void AssertAdditionalChangeOfOwnershipInwardOnlyFieldsValidation(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2)
		{
			throw new NotImplementedException();
		}

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnOriginalError(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, Enterprise.Messaging.Business.EDIMessage originalMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			throw new NotImplementedException();
		}

		protected override Enterprise.Messaging.Business.EDIMessage CreateChangeOfOwnershipOriginalResponseMessage(Enterprise.Messaging.Business.EDIMessage originalMessage, bool isSuccess)
		{
			throw new NotImplementedException();
		}

		protected override Enterprise.Messaging.Business.EDIMessage SendChangeOfOwnershipOriginalMessageViaMenu(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration)
		{
			throw new NotImplementedException();
		}

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnOriginalClear(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, Enterprise.Messaging.Business.EDIMessage originalMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			throw new NotImplementedException();
		}

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnAmendmentError(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, Enterprise.Messaging.Business.EDIMessage amendmentMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			throw new NotImplementedException();
		}

		protected override Enterprise.Messaging.Business.EDIMessage CreateChangeOfOwnershipAmendmentResponseMessage(Enterprise.Messaging.Business.EDIMessage amendmentMessage, bool isSuccess)
		{
			throw new NotImplementedException();
		}

		protected override Enterprise.Messaging.Business.EDIMessage SendChangeOfOwnershipAmendmentMessageViaMenu(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration)
		{
			throw new NotImplementedException();
		}

		protected override void SetupChangeOfOwnershipOriginalClearState(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2)
		{
			throw new NotImplementedException();
		}

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnAmendmentClear(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, Enterprise.Messaging.Business.EDIMessage amendmentMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			throw new NotImplementedException();
		}

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalError(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, Enterprise.Messaging.Business.EDIMessage amendmentMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			throw new NotImplementedException();
		}

		protected override Enterprise.Messaging.Business.EDIMessage CreateChangeOfOwnershipWithdrawalResponseMessage(Enterprise.Messaging.Business.EDIMessage withdrawalMessage, bool isSuccess)
		{
			throw new NotImplementedException();
		}

		protected override Enterprise.Messaging.Business.EDIMessage SendChangeOfOwnershipWithdrawalMessageViaMenu(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration)
		{
			throw new NotImplementedException();
		}

		protected override void ProcessResponseAndAssertChangeOfOwnershipOnWithdrawalClear(IWarehouseIntegrationSupporter changeOfOwnershipJob, JobDeclaration changeOfOwnershipDeclaration, JobComInvoiceLine changeOfOwnershipInvoiceLine, JobComInvoiceLine changeOfOwnershipInvoiceLine2, Enterprise.Messaging.Business.EDIMessage withdrawalMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			throw new NotImplementedException();
		}

		protected override IWarehouseIntegrationSupporter GetNewChangeOfOwnershipJob(ZString jobReference, ZString inwardEntryKey, ZDecimal quantity1, ZDecimal quantity2)
		{
			throw new NotImplementedException();
		}

		protected override OrgHeader GetNewOwner(IWarehouseIntegrationSupporter changeOfOwnershipJob)
		{
			throw new NotImplementedException();
		}

		#endregion
		protected override ZString GetEntryKey(IWarehouseIntegrationSupporter job)
		{
			var declaration = (JobDeclaration)job;
			return declaration.US_EntryFilerCode + "-" + declaration.ImportEntryNumber;
		}

		protected override IWarehouseIntegrationSupporter GetNewInwardJob(ZString jobReference, ZDecimal quantity, ZDecimal quantity2)
		{
			var declaration = Helper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, jobReference, "XJ5", InwardEntryNumber, quantity);
			var invoice = declaration.Invoices[0];
			var invoiceLine2 = Helper.AddInvoiceLine(invoice, Helper.Part2, quantity2, "XJ5", InwardEntryNumber, 2);
			invoice.JZ_InvoiceAmount += invoiceLine2.JI_LinePrice;
			return declaration;
		}

		protected override void SetupInwardOriginalClearState(IWarehouseIntegrationSupporter inwardjob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, bool createOriginalResponseMessage = true)
		{
			inwardDeclaration.ImportEntryNumber = InwardEntryNumber;
			var inwardEntry = inwardDeclaration.ActiveEntryHeaders[0];
			inwardEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			var message = CreateMessage(inwardDeclaration.JE_DeclarationReference, inwardDeclaration.US_EntryFilerCode, inwardDeclaration.ImportEntryNumber, InwardMessageNumber1, true);
			message.EM_Status = MQEDIMessage.Status.Received;
			message.EM_LinkedObject = inwardEntry;
		}

		protected override IWarehouseIntegrationSupporter GetNewOutwardJob(ZString jobReference, ZString inwardEntryKey, ZDecimal quantity, ZDecimal quantity2)
		{
			var inwardEntryNumber = inwardEntryKey.Replace("XJ5-", "");
			var declaration = Helper.GetNewDeclaration(EntryTypeList.Codes.WarehouseWithdrawalConsumption, jobReference, "SV9", inwardEntryNumber, quantity);
			declaration.US_WHSEntryFilerCode = "XJ5";
			var invoice = declaration.Invoices[0];
			var invoiceLine2 = Helper.AddInvoiceLine(invoice, Helper.Part2, quantity2, "SV9", inwardEntryNumber, 2);
			invoice.JZ_InvoiceAmount += invoiceLine2.JI_LinePrice;
			return declaration;
		}

		protected override void UpdateQuantity(JobComInvoiceLine invoiceLine, ZDecimal quantity)
		{
			invoiceLine.JI_InvoiceQuantity = quantity;
			if (invoiceLine.WHSPackLines.Count == 1)
			{
				invoiceLine.WHSPackLines[0].US_PackedQty = quantity;
			}
		}

		protected override void SetupOutwardOriginalClearState(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, bool createOriginalResponseMessage = true)
		{
			outwardDeclaration.ImportEntryNumber = OutwardEntryNumber;
			var outwardEntry = outwardDeclaration.ActiveEntryHeaders[0];
			outwardEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			var message = CreateMessage(outwardDeclaration.JE_DeclarationReference, outwardDeclaration.US_EntryFilerCode, outwardDeclaration.ImportEntryNumber, OutwardMessageNumber1, true);
			message.EM_Status = MQEDIMessage.Status.Received;
			message.EM_LinkedObject = outwardEntry;
		}

		protected override void GetInvoiceLines(IWarehouseIntegrationSupporter job, out JobComInvoiceLine invoiceLine, out JobComInvoiceLine invoiceLine2)
		{
			var declaration = (JobDeclaration)job;
			var invoiceLines = declaration.InvoiceLines.OfType<JobComInvoiceLine>().OrderBy(x => x.JI_LineNo).ToArray();
			invoiceLine = invoiceLines[0];
			invoiceLine2 = invoiceLines[1];
		}

		protected override Customs.Business.Testing.WhsDataTestHelper<JobDeclaration, Business.OrgSupplierPart, CusClassification, CusClassPartPivot> CreateNewHelper()
		{
			return new WhsDataTestHelper(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetupForSendMessage();
		}

		protected override Enterprise.Messaging.Business.EDIMessage SendInwardOriginalMessageViaMenu(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDclaration)
		{
			return SendMessageViaMenu(inwardDclaration, "Send Original Messages");
		}

		protected override void AssertAdditionalInwardFieldsValidation(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2)
		{
			inwardInvoiceLine.JI_InvoiceQuantity = 0m;
			Factory.Save();
			var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)inwardDeclaration.MessageInitiator;
			SendInwardAmendmentMessageViaMenu(inwardJob, inwardDeclaration);
			AssertContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), messageInitiator.InvalidOperationText);
		}

		protected override void ProcessResponseAndAssertInwardOnOriginalError(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, Enterprise.Messaging.Business.EDIMessage originalMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			ProcessResponseAndAssertEntryStatus(responseMessage, ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal);
		}

		protected override Enterprise.Messaging.Business.EDIMessage CreateInwardOriginalResponseMessage(Enterprise.Messaging.Business.EDIMessage originalMessage, bool isSuccess)
		{
			var entry = (CusEntryHeader)originalMessage.EM_LinkedObject;
			var declaration = entry.Declaration;
			return CreateMessage(declaration.JE_DeclarationReference, declaration.US_EntryFilerCode, declaration.ImportEntryNumber, originalMessage.EM_MessageNum, isSuccess);
		}

		protected override void ProcessResponseAndAssertInwardOnOriginalClear(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, Enterprise.Messaging.Business.EDIMessage originalMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			ProcessResponseAndAssertEntryStatus(responseMessage, ImportMessageStatusList.Codes.ClearEntrySummaryOriginal);
		}

		protected override void ProcessResponseAndAssertInwardOnAmendmentError(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, Enterprise.Messaging.Business.EDIMessage amendmentMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			ProcessResponseAndAssertEntryStatus(responseMessage, ImportMessageStatusList.Codes.ErrorEntrySummaryReplace);
		}

		protected override Enterprise.Messaging.Business.EDIMessage CreateInwardAmendmentResponseMessage(Enterprise.Messaging.Business.EDIMessage amendmentMessage, bool isSuccess)
		{
			var entry = (CusEntryHeader)amendmentMessage.EM_LinkedObject;
			var declaration = entry.Declaration;
			return CreateMessage(declaration.JE_DeclarationReference, declaration.US_EntryFilerCode, declaration.ImportEntryNumber, amendmentMessage.EM_MessageNum, isSuccess);
		}

		protected override ZString ExpectedInwardAmendmentErrorWHSStatus => WarehouseTransactionStatusList.Codes.InwardCanceled;
		protected override ZDecimal ExpectedInwardAmendmentErrorInventoryQuantity => ZDecimal.Zero;
		protected override ZDecimal ExpectedInwardAmendmentErrorInventoryQuantity2 => ZDecimal.Zero;
		protected override Enterprise.Messaging.Business.EDIMessage SendInwardAmendmentMessageViaMenu(IWarehouseIntegrationSupporter job, JobDeclaration inwardDeclaration)
		{
			return SendMessageViaMenu(inwardDeclaration, "Send Replacement / Update Messages");
		}

		protected override void ProcessResponseAndAssertInwardOnAmendmentClear(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, Enterprise.Messaging.Business.EDIMessage amendmentMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			ProcessResponseAndAssertEntryStatus(responseMessage, ImportMessageStatusList.Codes.ClearEntrySummaryReplace);
		}

		protected override void ProcessResponseAndAssertInwardOnWithdrawalError(IWarehouseIntegrationSupporter iwnardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, Enterprise.Messaging.Business.EDIMessage withdrawalMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			ProcessResponseAndAssertEntryStatus(responseMessage, ImportMessageStatusList.Codes.ErrorEntrySummaryDelete);
		}

		protected override Enterprise.Messaging.Business.EDIMessage CreateInwardWithdrawalResponseMessage(Enterprise.Messaging.Business.EDIMessage withdrawalMessage, bool isSuccess)
		{
			var entry = (CusEntryHeader)withdrawalMessage.EM_LinkedObject;
			var declaration = entry.Declaration;
			return CreateMessage(declaration.JE_DeclarationReference, declaration.US_EntryFilerCode, declaration.ImportEntryNumber, withdrawalMessage.EM_MessageNum, isSuccess);
		}

		protected override Enterprise.Messaging.Business.EDIMessage SendInwardWithdrawalMessageViaMenu(IWarehouseIntegrationSupporter job, JobDeclaration inwardDeclaration)
		{
			return SendMessageViaMenu(inwardDeclaration, "Send Deletion Messages");
		}

		protected override void ProcessResponseAndAssertInwardOnWithdrawalClear(IWarehouseIntegrationSupporter inwardJob, JobDeclaration inwardDeclaration, JobComInvoiceLine inwardInvoiceLine, JobComInvoiceLine inwardInvoiceLine2, Enterprise.Messaging.Business.EDIMessage withdrawalMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			ProcessResponseAndAssertEntryStatus(responseMessage, ImportMessageStatusList.Codes.ClearEntrySummaryDelete);
		}

		protected override Enterprise.Messaging.Business.EDIMessage SendOutwardOriginalMessageViaMenu(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration)
		{
			return SendMessageViaMenu(outwardDeclaration, "Send Original Messages");
		}

		protected override void AssertAdditionalOutwardFieldsValidation(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2)
		{
			outwardInvoiceLine.JI_InvoiceQuantity = 0m;
			Factory.Save();
			var messageInitiator = (Customs.Business.SendsMessagesToCustomsShutterUpperer)outwardDeclaration.MessageInitiator;
			SendInwardAmendmentMessageViaMenu(outwardJob, outwardDeclaration);
			AssertContains(JobDeclaration.InvoiceLineMarkedForBondedWarehousingRequiresAnInvoiceQuantityAndUnit("Inventory Management"), messageInitiator.InvalidOperationText);
		}

		protected override Enterprise.Messaging.Business.EDIMessage CreateOutwardOriginalResponseMessage(Enterprise.Messaging.Business.EDIMessage originalMessage, bool isSuccess)
		{
			var entry = (CusEntryHeader)originalMessage.EM_LinkedObject;
			var declaration = entry.Declaration;
			return CreateMessage(declaration.JE_DeclarationReference, declaration.US_EntryFilerCode, declaration.ImportEntryNumber, originalMessage.EM_MessageNum, isSuccess);
		}

		protected override void ProcessResponseAndAssertOutwardOnOriginalError(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, Enterprise.Messaging.Business.EDIMessage originalMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			ProcessResponseAndAssertEntryStatus(responseMessage, ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal);
		}

		protected override void ProcessResponseAndAssertOutwardOnOriginalClear(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, Enterprise.Messaging.Business.EDIMessage originalMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			ProcessResponseAndAssertEntryStatus(responseMessage, ImportMessageStatusList.Codes.ClearEntrySummaryOriginal);
		}

		protected override void ProcessResponseAndAssertOutwardOnAmendmentError(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, Enterprise.Messaging.Business.EDIMessage amendmentMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			ProcessResponseAndAssertEntryStatus(responseMessage, ImportMessageStatusList.Codes.ErrorEntrySummaryReplace);
		}

		protected override ZString ExpectedOutwardAmendmentErrorWHSStatus => WarehouseTransactionStatusList.Codes.OutwardCanceled;

		protected override ZDecimal ExpectedOutwardAmendmentErrorInventoryQuantity => 100m;

		protected override ZDecimal ExpectedOutwardAmendmentErrorInventoryQuantity2 => 300m;

		protected override Enterprise.Messaging.Business.EDIMessage CreateOutwardAmendmentResponseMessage(Enterprise.Messaging.Business.EDIMessage amendmentMessage, bool isSuccess)
		{
			var entry = (CusEntryHeader)amendmentMessage.EM_LinkedObject;
			var declaration = entry.Declaration;
			return CreateMessage(declaration.JE_DeclarationReference, declaration.US_EntryFilerCode, declaration.ImportEntryNumber, amendmentMessage.EM_MessageNum, isSuccess);
		}

		protected override Enterprise.Messaging.Business.EDIMessage SendOutwardAmendmentMessageViaMenu(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration)
		{
			return SendMessageViaMenu(outwardDeclaration, "Send Replacement / Update Messages");
		}

		protected override void ProcessResponseAndAssertOutwardOnAmendmentClear(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, Enterprise.Messaging.Business.EDIMessage amendmentMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			ProcessResponseAndAssertEntryStatus(responseMessage, ImportMessageStatusList.Codes.ClearEntrySummaryReplace);
		}

		protected override void ProcessResponseAndAssertOutwardOnWithdrawalError(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, Enterprise.Messaging.Business.EDIMessage withdrawalMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			ProcessResponseAndAssertEntryStatus(responseMessage, ImportMessageStatusList.Codes.ErrorEntrySummaryDelete);
		}

		protected override Enterprise.Messaging.Business.EDIMessage CreateOutwardWithdrawalResponseMessage(Enterprise.Messaging.Business.EDIMessage withdrawalMessage, bool isSuccess)
		{
			var entry = (CusEntryHeader)withdrawalMessage.EM_LinkedObject;
			var declaration = entry.Declaration;
			return CreateMessage(declaration.JE_DeclarationReference, declaration.US_EntryFilerCode, declaration.ImportEntryNumber, withdrawalMessage.EM_MessageNum, isSuccess);
		}

		protected override Enterprise.Messaging.Business.EDIMessage SendOutwardWithdrawalMessageViaMenu(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration)
		{
			return SendMessageViaMenu(outwardDeclaration, "Send Deletion Messages");
		}

		protected override void ProcessResponseAndAssertOutwardOnWithdrawalClear(IWarehouseIntegrationSupporter outwardJob, JobDeclaration outwardDeclaration, JobComInvoiceLine outwardInvoiceLine, JobComInvoiceLine outwardInvoiceLine2, Enterprise.Messaging.Business.EDIMessage withdrawalMessage, Enterprise.Messaging.Business.EDIMessage responseMessage)
		{
			ProcessResponseAndAssertEntryStatus(responseMessage, ImportMessageStatusList.Codes.ClearEntrySummaryDelete);
		}

		protected override void ProcessResponse(IWarehouseIntegrationSupporter job, JobDeclaration declaration, JobComInvoiceLine invoiceLine, JobComInvoiceLine invoiceLine2, Enterprise.Messaging.Business.EDIMessage outgoingMessage, Enterprise.Messaging.Business.EDIMessage incomingMessage)
		{
			new ABIIncomingMessageProcessor().ExecuteBatch();
		}

		MQEDIMessage CreateMessage(ZString jobReference, ZString entryFiler, ZString entryNumber, ZString messageNumber, bool isSuccess)
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			message.EM_Status = MQEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageNum = messageNumber;
			message.EM_MessageText = string.Format(isSuccess ? ClearMessage : ErrorMessage, jobReference.Left(12).PadRight(12), messageNumber.Left(21).PadRight(21), entryFiler.Left(3).PadRight(3), entryNumber.Left(8).PadRight(8));
			return message;
		}

		MQEDIMessage SendMessageViaMenu(JobDeclaration declaration, string menuItem)
		{
			declaration.US_QtyBeingWithdrawn = declaration.InvoiceLines.OfType<JobComInvoiceLine>().Sum(x => x.JI_InvoiceQuantity);
			Factory.Save();
			var mockMenu = new Mock<EDIMenu>();
			mockMenu.CallBase = true;
			mockMenu
				.Protected()
				.Setup<bool>("ShowImportMessageSendingActionForm", ItExpr.IsAny<ImportMessageSendingActionCollection>())
				.Returns((ImportMessageSendingActionCollection actions) =>
				{
					actions.IsCancelled = false;
					actions[0].US_SendMessage = true;
					return true;
				});

			using (var form = new ZForm(declaration))
			using (var menu = mockMenu.Object)
			{
				form.Menu.MenuItems.Add(menu);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.Declaration = declaration;
				menu.MenuItems.FindByText(menuItem).PerformClick();
			}

			return declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages.OfType<MQEDIMessage>().Where(x => x.IsTransmitMessage).OrderByDescending(x => x.EM_SystemCreateTimeUtc + x.EM_MessageNum).FirstOrDefault();
		}

		void ProcessResponseAndAssertEntryStatus(Enterprise.Messaging.Business.EDIMessage responseMessage, ZString status)
		{
			new ABIIncomingMessageProcessor().ExecuteBatch();
			responseMessage.Reload();
			AssertEquals(MQEDIMessage.Status.Received, responseMessage.EM_Status);
			var entry = (CusEntryHeader)responseMessage.EM_LinkedObject;
			entry.Reload();
			AssertEquals("Status changed", status, entry.CH_Status);
		}

		WhsDataTestHelper Helper => (WhsDataTestHelper)helper;

		const string InwardEntryNumber = "71026528";
		const string OutwardEntryNumber = "71026601";
		const string InwardMessageNumber1 = "HYEDUSCMT_188180";
		const string OutwardMessageNumber1 = "HYEDUSCMT_188238";
		const string ClearMessage = "B001101{2}AX                                               {1}" + "E0 SUMMRY 000001 REF ID: {2} {3} {0} 171                          " + "E1A 995   SUMMARY HAS BEEN ADDED                  {2}  {3}00100{0}" + "Y  1101{2}AX00002";
		const string ErrorMessage = "B001101{2}AX                                               {1}" + "E0 SUMMRY 000001 REF ID: {2} {3} {0}                              " + "E1RF998   TRANSACTION DATA REJECTED               {2}  {3}     {0}" + "Y  1101{2}AX00002";
	}
}
