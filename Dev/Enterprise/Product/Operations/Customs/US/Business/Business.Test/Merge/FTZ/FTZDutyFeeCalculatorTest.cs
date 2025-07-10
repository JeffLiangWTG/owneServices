using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZDutyFeeCalculatorTest : TestCaseWithFactory
	{
		public void TestHMFDeMinimusRuleDoesNotApplyJobByJob()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 500m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.FTZEntry;
			AssertEquals("Payable HMF", 0.63m, entry.HMFAmountForEntry);

			AssertEquals("Line has HMF which should be rounded", 0.63m, invoiceLine.CusEntryLine.HMFAmount);
			AssertEquals("Payable HMF", 0.63m, invoiceLine.HMFAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
