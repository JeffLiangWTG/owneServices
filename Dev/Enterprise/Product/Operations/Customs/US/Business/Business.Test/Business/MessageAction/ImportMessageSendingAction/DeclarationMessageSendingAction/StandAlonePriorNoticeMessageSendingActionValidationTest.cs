using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class StandAlonePriorNoticeMessageSendingActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_SendMessage()
		{
			Declaration.ImportEntryNumber = "12345678";
			Declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.ENT;
			var collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			var sendingAction = collection.FindFirstElement<StandAlonePriorNoticeMessageSendingAction>(ImportMessageStatusList.MessageType.Undefined);
			sendingAction.US_SendMessage = true;
			AssertHasMessageErrorContaining(sendingAction.US_SendMessageInfo, StandAlonePriorNoticeMessageSendingActionValidation.NoPriorNoticeLinesToSend);
			var fda = InvoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_FDAForcePN = true;
			collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			sendingAction = collection.FindFirstElement<StandAlonePriorNoticeMessageSendingAction>(ImportMessageStatusList.MessageType.Undefined);
			sendingAction.US_SendMessage = true;
			AssertNoMessageErrorContaining(sendingAction.US_SendMessageInfo, StandAlonePriorNoticeMessageSendingActionValidation.NoPriorNoticeLinesToSend);
			InvoiceLine.ACE_FDALines.RemoveAndDeleteAll();
			Declaration.JE_MasterBill = "MB1234567";
			Declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			sendingAction = collection.FindFirstElement<StandAlonePriorNoticeMessageSendingAction>(ImportMessageStatusList.MessageType.Undefined);
			sendingAction.US_SendMessage = true;
			AssertHasMessageErrorContaining(sendingAction.US_SendMessageInfo, StandAlonePriorNoticeMessageSendingActionValidation.NoPriorNoticeLinesToSend);
			fda = InvoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_FDAForcePN = true;
			collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			sendingAction = collection.FindFirstElement<StandAlonePriorNoticeMessageSendingAction>(ImportMessageStatusList.MessageType.Undefined);
			sendingAction.US_SendMessage = true;
			AssertNoMessageErrorContaining(sendingAction.US_SendMessageInfo, StandAlonePriorNoticeMessageSendingActionValidation.NoPriorNoticeLinesToSend);
			var newMasterBill = Declaration.Bills.CreatePrimaryBill(Customs.Business.BillTypeList.Codes.MasterBill);
			newMasterBill.CU_BillNum = "MB22222222";
			collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			sendingAction = collection.FindFirstElement<StandAlonePriorNoticeMessageSendingAction>(ImportMessageStatusList.MessageType.Undefined);
			sendingAction.US_SendMessage = true;
			AssertHasMessageErrorContaining(sendingAction.US_SendMessageInfo, StandAlonePriorNoticeMessageSendingActionValidation.NoPriorNoticeLinesForMultipleMasterBills);
			InvoiceLine.InvoiceHeader.JZ_CU_RelatedHouseBill = Declaration.PrimaryMasterBill.PK;
			collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			sendingAction = collection.FindFirstElement<StandAlonePriorNoticeMessageSendingAction>(ImportMessageStatusList.MessageType.Undefined);
			sendingAction.US_SendMessage = true;
			AssertNoMessageErrorContaining(sendingAction.US_SendMessageInfo, StandAlonePriorNoticeMessageSendingActionValidation.NoPriorNoticeLinesForMultipleMasterBills);
		}

		public void TestCheckUS_PNActionCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			var action = actions[0];
			action.US_PNActionCode = "!";
			AssertHasMessageErrorContaining(action.US_PNActionCodeInfo, ListValidation.InvalidCodeMessageError);
			action.US_PNActionCode = ACEPNActionCodeList.Codes.D;
			AssertNoMessageErrorContaining(action.US_PNActionCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(action.US_PNActionCodeInfo, StandAlonePriorNoticeMessageSendingActionValidation.CodeForFutureUse);
			action.US_PNActionCode = ACEPNActionCodeList.Codes.A;
			AssertNoMessageError(action.US_PNActionCodeInfo, StandAlonePriorNoticeMessageSendingActionValidation.CodeForFutureUse);
		}

		public void TestCheckUS_PNActionCodeForFTZ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = US.Business.JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.US_EnableSPN = true;
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
			declaration.FTZAdmissionNumber = "2140000|17|00000001";
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.FTZ;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			var action = actions[0];
			action.US_PNActionCode = "!";
			AssertHasMessageErrorContaining(action.US_PNActionCodeInfo, ListValidation.InvalidCodeMessageError);
			action.US_PNActionCode = ACEPNActionCodeList.Codes.D;
			AssertNoMessageErrorContaining(action.US_PNActionCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(action.US_PNActionCodeInfo, StandAlonePriorNoticeMessageSendingActionValidation.CodeForFutureUse);
			action.US_PNActionCode = ACEPNActionCodeList.Codes.A;
			AssertNoMessageError(action.US_PNActionCodeInfo, StandAlonePriorNoticeMessageSendingActionValidation.CodeForFutureUse);
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableSPN = true;
				}

				return declaration;
			}
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var fInvoice = Declaration.Invoices.AddNew();
					invoiceLine = fInvoice.JobComInvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}
	}
}
