using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZLineDutyFeeCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculateForFTZ()
		{
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.JI_LinePrice = 5000m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2.JI_LinePrice = 3000m;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine3.JI_LinePrice = 1500m;

			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine4.JI_LinePrice = 500m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("HMF", 6.25m, declaration.ActiveEntryHeaders.FTZEntry.HMFAmountForEntry);
			AssertEquals("Total Payable", 6.25m, declaration.ActiveEntryHeaders.FTZEntry.TotalAmountPayable);
			AssertEquals(6.25m, invoiceLine.HMFAmount);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
