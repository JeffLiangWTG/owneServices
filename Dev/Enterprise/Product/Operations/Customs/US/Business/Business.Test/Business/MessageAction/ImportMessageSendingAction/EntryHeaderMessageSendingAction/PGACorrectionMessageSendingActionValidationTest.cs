using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PGACorrectionMessageSendingActionValidationTest : TestCaseWithFactory
	{
		[TestDate(2019, 12, 30)]
		public void TestCheckUS_SendMessage_ReleaseDateMessageError()
		{
			invoiceLine.NHTSALines.AddNew();
			action.Validation.CheckUS_SendMessage();
			AssertNoMessageErrors(action.US_SendMessageInfo);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2019, 12, 19);
			action.Validation.CheckUS_SendMessage();
			AssertNoMessageErrors(action.US_SendMessageInfo);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2019, 12, 15);
			action.Validation.CheckUS_SendMessage();
			AssertNoMessageErrors(action.US_SendMessageInfo);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2019, 12, 12);
			action.Validation.CheckUS_SendMessage();
			AssertHasMessageError(action.US_SendMessageInfo, PGACorrectionMessageSendingActionValidation.ReleseDateMessageError);
		}

		[TestDate(2019, 12, 4)]
		public void TestCheckUS_SendMessage_PGAMessageError()
		{
			invoiceLine.ACE_FDALines.AddNew();
			action.Validation.CheckUS_SendMessage();
			AssertNoMessageErrors(action.US_SendMessageInfo);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2019, 12, 3);
			action.Validation.CheckUS_SendMessage();
			AssertNoMessageErrors(action.US_SendMessageInfo);
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			action.Validation.CheckUS_SendMessage();
			AssertNoMessageErrors(action.US_SendMessageInfo);
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			action.Validation.CheckUS_SendMessage();
			AssertNoMessageErrors(action.US_SendMessageInfo);
			var disposition = declaration.EntryPGACusDispositions.AddNew();
			disposition.CDI_StatusKey = ACEGovernmentAgenciesCodeList.Codes.FDA;
			action.Validation.CheckUS_SendMessage();
			AssertNoMessageErrors(action.US_SendMessageInfo);
			disposition.CDI_Status = PGADispositionCodeList.MarkAsClosedCode;
			action.Validation.CheckUS_SendMessage();
			AssertHasMessageError(action.US_SendMessageInfo, PGACorrectionMessageSendingActionValidation.GetPGAMessageError(ACEGovernmentAgenciesCodeList.Codes.FDA));
			invoiceLine.FWSHeaders.AddNew();
			disposition = declaration.EntryPGACusDispositions.AddNew();
			disposition.CDI_StatusKey = ACEGovernmentAgenciesCodeList.Codes.FWS;
			disposition.CDI_Status = PGADispositionCodeList.MarkAsClosedCode;
			action.Validation.CheckUS_SendMessage();
			AssertHasMessageError(action.US_SendMessageInfo, PGACorrectionMessageSendingActionValidation.GetPGAMessageError(ACEGovernmentAgenciesCodeList.Codes.FWS));
		}

		JobDeclaration declaration;
		PGACorrectionMessageSendingAction action;
		JobComInvoiceLine invoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "testing";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			action = new PGACorrectionMessageSendingAction(declaration.ActiveEntryHeaders.SimplifiedEntry);
		}
	}
}
