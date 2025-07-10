using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ProvisionalPaymentAmountCodeDataValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckCY_Value()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryLine = dec.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var provisionalPayment = entryLine.ProvisionalPayments.AddNew();
			provisionalPayment.CY_Code = "TST";
			provisionalPayment.CY_Value = 1m;
			provisionalPayment.Validation.ValidateAll();
			AssertNoNotifications(provisionalPayment.CY_ValueInfo);
			CombineAssertions(() =>
			{
				provisionalPayment.CY_Value = 0m;
				provisionalPayment.Validation.ValidateAll();
				AssertNoErrors(provisionalPayment.CY_ValueInfo);
				AssertHasMessageErrorContaining(provisionalPayment.CY_ValueInfo, MandatoryValidation.ValueCannotBeZero);
				AssertNoWarnings(provisionalPayment.CY_ValueInfo);
			});
			provisionalPayment.CY_Code = ZString.Empty;
			provisionalPayment.CY_Value = 1m;
			provisionalPayment.Validation.ValidateAll();
			AssertNoNotifications(provisionalPayment.CY_ValueInfo);
			provisionalPayment.CY_Value = 0m;
			provisionalPayment.Validation.ValidateAll();
			AssertNoNotifications(provisionalPayment.CY_ValueInfo);
		}

		public void TestCheckCY_Code_CaseClosed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "11";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = instruction.PK;
			var pp1 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PEN, 0, "1", "REF1", true);
			var pp2 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPC, 0, "1", "REF2", true);
			var pp3 = entryHeader.EntryPayInfos.AddNewEntryPayInfo(ZDateTime.Now, "C", LineLevelProvisionalPayments.Codes.PPA, 0, "1", "REF3", false);
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_LineNumber = 1;
			entryLine.InvoiceLines.Add(invoiceLine);
			var tester = entryLine.ProvisionalPayments.AddNew();
			CombineAssertions(() =>
			{
				tester.CY_Code = LineLevelProvisionalPayments.Codes.FOR;
				AssertNoNotifications(tester.CY_CodeInfo);
				tester.CY_Code = LineLevelProvisionalPayments.Codes.PEN;
				AssertHasWarningContaining(tester.CY_CodeInfo, "Provisional Payment Case for type:PEN has already been closed for this Entry Line, the entered amount won't be used for Fee Calculation or message sending.");
				tester.CY_Code = LineLevelProvisionalPayments.Codes.PPA;
				AssertNoNotifications(tester.CY_CodeInfo);
				tester.CY_Code = LineLevelProvisionalPayments.Codes.PPC;
				AssertHasWarningContaining(tester.CY_CodeInfo, "Provisional Payment Case for type:PPC has already been closed for this Entry Line, the entered amount won't be used for Fee Calculation or message sending.");
			});
		}

		public new void TestCheckCY_Code()
		{
			base.TestCheckCY_Code();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			var testEntryLine = dec.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var tester = testEntryLine.ProvisionalPayments.AddNew();
			tester.CY_Code = LineLevelProvisionalPayments.Codes.FOR;
			AssertNoNotifications(tester.CY_CodeInfo);
			tester.CY_Code = LineLevelProvisionalPayments.Codes.PEN;
			AssertNoNotifications(tester.CY_CodeInfo);
			tester.CY_Code = LineLevelProvisionalPayments.Codes.PPA;
			AssertNoNotifications(tester.CY_CodeInfo);
			tester.CY_Code = LineLevelProvisionalPayments.Codes.PPC;
			AssertNoNotifications(tester.CY_CodeInfo);
			tester.CY_Code = LineLevelProvisionalPayments.Codes.PPG;
			AssertNoNotifications(tester.CY_CodeInfo);
			tester.CY_Code = LineLevelProvisionalPayments.Codes.PPR;
			AssertNoNotifications(tester.CY_CodeInfo);
			tester.CY_Code = LineLevelProvisionalPayments.Codes.PPT;
			AssertNoNotifications(tester.CY_CodeInfo);
			tester.CY_Code = "XXX";
			AssertNoErrors(tester.CY_CodeInfo);
			AssertHasMessageErrorContaining(tester.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoWarnings(tester.CY_CodeInfo);
			tester.CY_Code = ZString.Empty;
			AssertHasErrorContaining(tester.CY_CodeInfo, MandatoryValidation.MustBeEntered);
			AssertNoMessageErrors(tester.CY_CodeInfo);
			AssertNoWarnings(tester.CY_CodeInfo);
			tester.CY_Code = LineLevelProvisionalPayments.Codes.PEN;
			var duplicateTester = testEntryLine.ProvisionalPayments.AddNew();
			duplicateTester.CY_Code = LineLevelProvisionalPayments.Codes.FOR;
			AssertNoNotifications(duplicateTester.CY_CodeInfo);
			duplicateTester.CY_Code = LineLevelProvisionalPayments.Codes.PEN;
			AssertNoErrors(duplicateTester.CY_CodeInfo);
			AssertHasMessageErrorContaining(duplicateTester.CY_CodeInfo, "Provisional Payment Code:PEN has already been specified for this Entry Line.");
			AssertNoWarnings(duplicateTester.CY_CodeInfo);
			duplicateTester.CY_Code = LineLevelProvisionalPayments.Codes.PPA;
			AssertNoNotifications(duplicateTester.CY_CodeInfo);
			duplicateTester.CY_Code = LineLevelProvisionalPayments.Codes.PPC;
			AssertNoNotifications(duplicateTester.CY_CodeInfo);
			duplicateTester.CY_Code = LineLevelProvisionalPayments.Codes.PPG;
			AssertNoNotifications(duplicateTester.CY_CodeInfo);
			duplicateTester.CY_Code = LineLevelProvisionalPayments.Codes.PPR;
			AssertNoNotifications(duplicateTester.CY_CodeInfo);
			duplicateTester.CY_Code = LineLevelProvisionalPayments.Codes.PPT;
			AssertNoNotifications(duplicateTester.CY_CodeInfo);
			tester.CY_Code = HeaderLevelProvisionalPayments.Codes.PPE;
			AssertNoNotifications(duplicateTester.CY_CodeInfo);
			dec.JE_MessageType = "EXP";
			tester.CY_Code = "DLA";
			AssertNoNotifications(tester.CY_CodeInfo);
			tester.CY_Code = "XXX";
			AssertNoErrors(tester.CY_CodeInfo);
			AssertHasMessageErrorContaining(tester.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestAdditionalInfoValidation()
		{
			var diamondLevyError = CusEntryLineValidation.DiamondLevyValueAndAmountRequiredError;
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "EXP";
			var entryLine = dec.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var provisionalPayment = entryLine.ProvisionalPayments.AddNew();
			var additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			AssertNoRowMessageErrorContaining(entryLine, diamondLevyError);
			provisionalPayment.CY_Code = "DLA";
			AssertHasRowMessageErrorContaining(entryLine, diamondLevyError);
			provisionalPayment.CY_Code = "ABC";
			AssertNoRowMessageErrorContaining(entryLine, diamondLevyError);
			additionalInfo.CY_Code = "DLV";
			provisionalPayment.CY_Code = "DLA";
			AssertNoRowMessageErrorContaining(entryLine, diamondLevyError);
		}
	}
}
