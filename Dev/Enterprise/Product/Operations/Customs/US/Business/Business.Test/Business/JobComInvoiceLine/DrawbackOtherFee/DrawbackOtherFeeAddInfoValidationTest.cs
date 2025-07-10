using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class DrawbackOtherFeeAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_99ClaimAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var otherFee = invoiceLine.DrawbackOtherFees.AddNew();
			otherFee.US_FeeType = DrawbackOtherFeeTypesList.Codes.BeefFee;
			otherFee.CalculatedAmount = 100m;
			otherFee._99ClaimedAmount = 99m;
			AssertNoMessageError(otherFee.US_99ClaimAmountInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
			otherFee._99ClaimedAmount = 101m;
			AssertHasMessageError(otherFee.US_99ClaimAmountInfo, DrawbackJobUSComInvoiceLineValidation._99ClaimedAmountShouldNotBeGreaterThanCalculatedAmount);
		}
	}
}
