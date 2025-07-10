using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class InvoiceChargeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJ7_ChargeTypeInfo()
		{
			var testInvoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var dec = testInvoice.JobDeclaration;
			if (dec != null)
			{
				dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			}

			testInvoice.JobDeclaration.JE_MessageType = "IMP";
			var testCharge = testInvoice.Charges.AddNew();
			testCharge.J7_ChargeType = "ADD";
			AssertNoMessageErrors(testCharge.J7_ChargeTypeInfo);
			testCharge.J7_ChargeType = "XXX";
			AssertHasMessageErrorContaining(testCharge.J7_ChargeTypeInfo, "Please enter a valid Charge code");
			testCharge.J7_ChargeType = "DIS";
			AssertNoMessageErrors(testCharge.J7_ChargeTypeInfo);
			testCharge.J7_ChargeType = "INT";
			AssertHasMessageErrorContaining(testCharge.J7_ChargeTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestStandAloneCommercialInvoiceJ7_ChargeTypeInfo()
		{
			var testInvoice = Factory.New<JobComInvoiceHeader>();
			new FakeDeclarationCreatorForInvoice(testInvoice);
			var dec = testInvoice.JobDeclaration;
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testInvoice.JZ_MessageType = "IMP";
			var testCharge = testInvoice.Charges.AddNew();
			testCharge.J7_ChargeType = "ADD";
			AssertNoMessageErrors(testCharge.J7_ChargeTypeInfo);
			testCharge.J7_ChargeType = "XXX";
			AssertHasMessageErrorContaining(testCharge.J7_ChargeTypeInfo, "Please enter a valid Charge code");
			testCharge.J7_ChargeType = "DIS";
			AssertNoMessageErrors(testCharge.J7_ChargeTypeInfo);
			testCharge.J7_ChargeType = "INT";
			AssertHasMessageErrorContaining(testCharge.J7_ChargeTypeInfo, "The code you have selected is not in the list.");
		}
	}
}
