using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class InvoiceLineChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckJ7_ChargeTypeInfo()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testDeclaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvoiceLine = testInvoice.InvoiceLines.AddNew();
			var testCharge = testInvoiceLine.Charges.AddNew();
			CombineAssertions("EXP", () =>
			{
				testDeclaration.JE_MessageType = "EXP";
				testCharge.J7_ChargeType = "ADD";
				AssertHasMessageErrorContaining(testCharge.J7_ChargeTypeInfo, "The code you have selected is not in the list.");
				testCharge.J7_ChargeType = "XXX";
				AssertHasMessageErrorContaining(testCharge.J7_ChargeTypeInfo, "Please enter a valid Charge code");
				testCharge.J7_ChargeType = "DIS";
				AssertNoMessageErrors(testCharge.J7_ChargeTypeInfo);
				testCharge.J7_ChargeType = "INT";
				AssertHasMessageErrors(testCharge.J7_ChargeTypeInfo);
			});
			CombineAssertions("IMP", () =>
			{
				testDeclaration.JE_MessageType = "IMP";
				testCharge.J7_ChargeType = "ADD";
				AssertHasMessageErrorContaining(testCharge.J7_ChargeTypeInfo, "The code you have selected is not in the list.");
				testCharge.J7_ChargeType = "XXX";
				AssertHasMessageErrorContaining(testCharge.J7_ChargeTypeInfo, "Please enter a valid Charge code");
				testCharge.J7_ChargeType = "DIS";
				AssertNoMessageErrors(testCharge.J7_ChargeTypeInfo);
				testCharge.J7_ChargeType = "INT";
				AssertNoMessageErrors(testCharge.J7_ChargeTypeInfo);
			});
		}
	}
}
