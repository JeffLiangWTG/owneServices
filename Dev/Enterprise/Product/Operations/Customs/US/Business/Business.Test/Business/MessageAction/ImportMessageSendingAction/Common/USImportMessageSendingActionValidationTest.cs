using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USImportMessageSendingActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateUS_SE_ContactPhoneForEntrySummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var action = actions[0];
			action.US_CertifyCargoRelease = true;
			action.US_SE_ContactPhone = "+11025553535";
			AssertHasMessageError(action.US_SE_ContactPhoneInfo, USImportMessageSendingActionValidation.InValidContactPhoneMessage);
			action.US_SE_ContactPhone = "1025553535";
			AssertNoMessageError(action.US_SE_ContactPhoneInfo, USImportMessageSendingActionValidation.InValidContactPhoneMessage);
			action.US_CertifyCargoRelease = false;
			action.US_SE_ContactPhone = "+11025553535";
			AssertNoMessageError(action.US_SE_ContactPhoneInfo, USImportMessageSendingActionValidation.InValidContactPhoneMessage);
		}

		public void TestValidateUS_SE_ContactPhoneForCargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var action = actions[0];
			action.US_SE_ContactPhone = "+11025553535";
			AssertHasMessageError(action.US_SE_ContactPhoneInfo, USImportMessageSendingActionValidation.InValidContactPhoneMessage);
			action.US_SE_ContactPhone = "1025553535";
			AssertNoMessageError(action.US_SE_ContactPhoneInfo, USImportMessageSendingActionValidation.InValidContactPhoneMessage);
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			action = actions[0];
			action.US_SE_ContactPhone = "+11025553535";
			AssertHasMessageError(action.US_SE_ContactPhoneInfo, USImportMessageSendingActionValidation.InValidContactPhoneMessage);
			action.US_SE_ContactPhone = "1025553535";
			AssertNoMessageError(action.US_SE_ContactPhoneInfo, USImportMessageSendingActionValidation.InValidContactPhoneMessage);
		}

		public void TestValidateUS_SendMessageOriginal()
		{
			var declaration = JobDeclarationTest.CreateFTZWeeklyEstimateDeclaration(Factory, "40000007");
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 10, 14);
			Factory.Save();
			var permit = declaration.FindRelatedPermits()[0];
			var lineTransaction1 = permit.CusPermitLineTransactions.AddNew();
			lineTransaction1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			var lineTransaction2 = permit.CusPermitLineTransactions.AddNew();
			declaration.US_EnableENS = true;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = actions.First(x => (x as ImportMessageSendingAction)?.IsEntrySummary ?? false) as ImportMessageSendingAction;
			Assert("Should have an Entry Summary message", action != null);
			action.US_SendMessage = true;
			AssertHasError(action.US_SendMessageInfo, USImportMessageSendingActionValidation.WeeklyEstimatesEntrySummaryMustWaitForAllFinalizedOrders);
			lineTransaction1.CPL_TransactionStatus = ZString.Empty;
			action.US_SendMessage = true;
			AssertNoError(action.US_SendMessageInfo, USImportMessageSendingActionValidation.WeeklyEstimatesEntrySummaryMustWaitForAllFinalizedOrders);
		}

		public void TestValidateUS_SendMessageReplacement()
		{
			var declaration = JobDeclarationTest.CreateFTZWeeklyEstimateDeclaration(Factory, "40000007");
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 10, 14);
			Factory.Save();
			var permit = declaration.FindRelatedPermits()[0];
			var lineTransaction1 = permit.CusPermitLineTransactions.AddNew();
			lineTransaction1.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			var lineTransaction2 = permit.CusPermitLineTransactions.AddNew();
			declaration.US_EnableENS = true;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var crlEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			crlEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var action = actions.First(x => (x as ImportMessageSendingAction)?.IsEntrySummary ?? false) as ImportMessageSendingAction;
			Assert("Should have an Entry Summary message", action != null);
			action.US_SendMessage = true;
			AssertHasError(action.US_SendMessageInfo, USImportMessageSendingActionValidation.WeeklyEstimatesEntrySummaryMustWaitForAllFinalizedOrders);
			lineTransaction1.CPL_TransactionStatus = ZString.Empty;
			action.US_SendMessage = true;
			AssertNoError(action.US_SendMessageInfo, USImportMessageSendingActionValidation.WeeklyEstimatesEntrySummaryMustWaitForAllFinalizedOrders);
		}
	}
}
