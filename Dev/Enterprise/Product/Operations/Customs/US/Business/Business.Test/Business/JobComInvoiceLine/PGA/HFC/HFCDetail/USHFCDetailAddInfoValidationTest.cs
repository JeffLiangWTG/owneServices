using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USHFCDetailAddInfoValidationTest : BusinessObjectLookupsTestCase
	{
		public void TestAtLeastDetailMeetsReq()
		{
			hfcDetail.AddInfoValidation.ValidateUS_LPCONumber();
			hfcDetail.AddInfoValidation.ValidateUS_ActiveIngredientPercentage();
			AssertHasMessageError(hfcDetail.US_LPCONumberInfo, USHFCDetailAddInfoValidation.AtLeastDetailMeetsReq);
			AssertHasMessageError(hfcDetail.US_ActiveIngredientPercentageInfo, USHFCDetailAddInfoValidation.AtLeastDetailMeetsReq);
			hfcDetail.US_LPCONumber = "1";
			hfcDetail.US_ActiveIngredientPercentage = 1;
			hfcDetail.AddInfoValidation.ValidateUS_LPCONumber();
			AssertNoMessageError(hfcDetail.US_LPCONumberInfo, USHFCDetailAddInfoValidation.AtLeastDetailMeetsReq);
			AssertNoMessageError(hfcDetail.US_ActiveIngredientPercentageInfo, USHFCDetailAddInfoValidation.AtLeastDetailMeetsReq);
		}

		public void TestCheckUS_ActiveIngredientPercentage()
		{
			hfcDetail.US_ActiveIngredientPercentage = 100;
			var hfcDetail1 = hfcHeader.USHFCDetails.AddNew();
			AssertNoMessageError(hfcDetail.US_ActiveIngredientPercentageInfo, USHFCDetailAddInfoValidation.MustNotExceed100);
			AssertNoMessageError(hfcDetail1.US_ActiveIngredientPercentageInfo, USHFCDetailAddInfoValidation.MustNotExceed100);
			hfcDetail1.US_ActiveIngredientPercentage = 20;
			hfcDetail.AddInfoValidation.ValidateUS_ActiveIngredientPercentage();
			AssertHasMessageError(hfcDetail.US_ActiveIngredientPercentageInfo, USHFCDetailAddInfoValidation.MustNotExceed100);
			AssertHasMessageError(hfcDetail1.US_ActiveIngredientPercentageInfo, USHFCDetailAddInfoValidation.MustNotExceed100);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.US_HFCInd = OGAIndicatorList.Codes.Declared;
			hfcHeader = invoiceLine.USHFCHeaders.AddNew();
			hfcDetail = hfcHeader.USHFCDetails.AddNew();
		}
		JobComInvoiceLine invoiceLine;
		USHFCHeader hfcHeader;
		USHFCDetail hfcDetail;
	}
}
