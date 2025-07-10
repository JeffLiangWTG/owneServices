using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(InvoiceLineCharge))]
	sealed class InvoiceLineChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFieldsReadonly()
		{
			var testCharge = Factory.NewWithValidTestData<InvoiceLineCharge>();
			AssertEquals(true, testCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			AssertEquals(true, testCharge.J7_IsDutiableInfo.ReadOnly);
			AssertEquals(true, testCharge.J7_IsGSTApplicableInfo.ReadOnly);
			AssertEquals(true, testCharge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);
		}

		public void TestShouldSetCurrencyToLinePriceRefCurrency()
		{
			var testCharge = Factory.NewWithValidTestData<InvoiceLineCharge>();
			testCharge.J7_Percentage = 0;
			testCharge.J7_Amount = 0;
			AssertEquals(true, testCharge.ShouldSetCurrencyFromParent);
			testCharge.J7_Percentage = 1;
			AssertEquals(true, testCharge.ShouldSetCurrencyFromParent);
			testCharge.J7_Percentage = 0;
			testCharge.J7_Amount = 1;
			testCharge.J7_RX_NKCurrency = "USD";
			AssertEquals(true, testCharge.ShouldSetCurrencyFromParent);
			testCharge.J7_Percentage = 0;
			testCharge.J7_Amount = 1;
			testCharge.J7_RX_NKCurrency = "";
			AssertEquals(true, testCharge.ShouldSetCurrencyFromParent);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			return invoiceLine.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.New<InvoiceLineCharge>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}
	}
}
