using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class VOCValueInterfactExtensionsTests : TestCaseWithFactory
	{
		public void TestGetAmountDue()
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory);
			Factory.Save();
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var testInst = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst.CEI_Style = "40";
			var invHeader = testDeclaration.Invoices.AddNew();
			var invLine1 = invHeader.InvoiceLines.AddNew();
			invLine1.JI_CEI = testInst.PK;
			invLine1.JI_Procedure = invLine1.EntryInstruction.CEI_Style + "43";
			invLine1.JI_Tariff = "00001000";
			invLine1.JI_ZZF_NKTaxType = "VAT";
			var invLine2 = invHeader.InvoiceLines.AddNew();
			invLine2.JI_CEI = testInst.PK;
			invLine2.JI_Procedure = invLine2.EntryInstruction.CEI_Style + "43";
			invLine2.JI_Tariff = "00001000";
			invLine2.JI_ZZF_NKTaxType = "VAT";
			var testHeader = testDeclaration.CustomsEntryHeaders.AddNew();
			testHeader.CustomsDutyExcluding12BBefore = 90m;
			testHeader.S1P2BDutyBefore = 30m;
			testHeader.ValueAddedTaxBefore = 20m;
			testHeader.ProvisionalPaymentAmountBefore = 9m;
			testHeader.PenaltyAmountBefore = 3m;
			var testLine1 = testHeader.MergedLines.AddNew();
			testLine1.CL_CustomsValue = 20m;
			testLine1.Fees.AddOrUpdate("1P1", 500m);
			testLine1.Fees.AddOrUpdate("12A", 390m);
			testLine1.Fees.AddOrUpdate("12B", 250m);
			testLine1.Fees.AddOrUpdate("VAT", 200m);
			testLine1.ProvisionalPayments.AddNew("PPA", 50m);
			testLine1.ProvisionalPayments.AddNew("PEN", 20m);
			testLine1.InvoiceLines.Add(invLine1);
			var testLine2 = testHeader.MergedLines.AddNew();
			testLine2.CL_CustomsValue = 20m;
			testLine2.Fees.AddOrUpdate("12A", 10m);
			testLine2.Fees.AddOrUpdate("12B", 50m);
			testLine2.ProvisionalPayments.AddNew("PPR", 40m);
			testLine2.ProvisionalPayments.AddNew("PEN", 10m);
			testLine2.InvoiceLines.Add(invLine2);
			AssertEquals(1400m, (testHeader as IVOCAfterValues).GetAmountDue());
			AssertEquals(152m, (testHeader as IVOCBeforeValues).GetAmountDue());
		}
	}
}
