using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DrawbackJobUSComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void Test99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.US_DRWClaimAmountOverriden_New = true;
			invoiceLine.CalculatedDuty = 100m;
			invoiceLine._99ClaimedDuty = 100m;
			AssertNoMessageError(invoiceLine._99ClaimedDutyInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			invoiceLine._99ClaimedDuty = 101m;
			AssertHasMessageError(invoiceLine._99ClaimedDutyInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			invoiceLine.CalculatedTax = 100m;
			invoiceLine._99ClaimedTax = 100m;
			AssertNoMessageError(invoiceLine._99ClaimedTaxInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			invoiceLine._99ClaimedTax = 101m;
			AssertHasMessageError(invoiceLine._99ClaimedTaxInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			invoiceLine.CalculatedHMF = 100m;
			invoiceLine._99ClaimedHMF = 100m;
			AssertNoMessageError(invoiceLine._99ClaimedHMFInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			invoiceLine._99ClaimedHMF = 101m;
			AssertHasMessageError(invoiceLine._99ClaimedHMFInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			invoiceLine.CalculatedMPF = 100m;
			invoiceLine._99ClaimedMPF = 100m;
			AssertNoMessageError(invoiceLine._99ClaimedMPFInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			invoiceLine._99ClaimedMPF = 101m;
			AssertHasMessageError(invoiceLine._99ClaimedMPFInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
		}
	}
}
