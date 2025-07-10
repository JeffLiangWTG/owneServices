using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DairyFeeCalculatorTest : TestCaseWithFactory
	{
		[TestDate(2009, 1, 1)]
		public void TestCalculateDairyFee()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.DairyFeeApplicable;
			AssertNotNull(invoiceLine.ImportTariff);

			invoiceLine.ImportTariff.SetUpTestDataForDairyFeeWithXComputationCode();
			invoiceLine.JI_CustomsThirdUnitQty = "CKG";
			invoiceLine.JI_CustomsThirdQuantity = 1000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Dairy Fee calculated", 13.27m, invoiceLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee));
		}

		[TestDate(2009, 1, 1)]
		public void TestCalculateDairyFee2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.DairyFeeApplicable;
			AssertNotNull(invoiceLine.ImportTariff);

			invoiceLine.ImportTariff.SetUpTestDataForDairyFeeWith2ComputationCode();

			invoiceLine.JI_CustomsSecondQuantity = 1000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Dairy Fee calculated", 13.27m, invoiceLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.DairyFee));
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
