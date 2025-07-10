using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CustomsValuationCalculator))]
sealed class CustomsValuationCalculatorTest : TestCaseWithFactory
{
	public void TestType()
	{
		var calculator = new CustomsValuationCalculator(invoiceLine);
		AssertType<CustomsValuationCalculator>(calculator);
	}

	public void TestGetAmountToAddToITOTForStatistical()
	{
		var calculator = new CustomsValuationCalculator(invoiceLine);

		CombineAssertions(() =>
		{
			var chargesCode1 = invoiceLine.Charges.AddNew("CD1", 1m, CurrencyCodes.Norway);
			chargesCode1.J7_IsStatisticalValueApplicable = true;
			var chargesCode2 = invoiceLine.Charges.AddNew("CD2", 2m, CurrencyCodes.Norway);
			chargesCode2.J7_IsStatisticalValueApplicable = false;
			AssertEquals("Amount to add to ITOT without VGE", 1m, calculator.GetAmountToAddToITOTForStatistical(BaseJobComInvoiceHeader.GetLocalCurrencyFor(invoiceLine.InvoiceHeader)));

			var chargesVge = invoiceLine.Charges.AddNew(NOInvoiceChargeTypesImport.Codes.ValueOfGoodsExported, 4m, CurrencyCodes.Norway);
			AssertEquals("Amount to add to ITOT with VGE", 5m, calculator.GetAmountToAddToITOTForStatistical(BaseJobComInvoiceHeader.GetLocalCurrencyFor(invoiceLine.InvoiceHeader)));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
}

