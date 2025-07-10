using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusLineTariffDetail))]
	sealed class CusLineTariffDetailTest : Customs.Business.Testing.CusLineTariffDetailTest
	{
		public void TestCusLineTariffDetail()
		{
			var cusLineTariffDetail = Factory.New<CusLineTariffDetail>();

			AssertType<CusLineTariffDetailLookups>("Lookups", cusLineTariffDetail.Lookups);
			AssertType<CusLineTariffDetailValidation>("Validation", cusLineTariffDetail.Validation);
		}

		[TestDate(2025, 03, 01)]
		public void TestDefaultUQFromTariff()
		{
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030120", "7", 0.068m, "KG");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038815", "7", 0.075m, "NO");

			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_StandAloneInvoiceDirection = JobMessageTypeList.Codes.Import;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.01.20";
			AssertEquals("UQ default to 'KG'", "KG", invoiceLine.US_SupAdditionalTariff1UQ);

			invoiceLine.US_SupAdditionalTariff1Qty = 100m;
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.15";
			AssertEquals("UQ default to 'NO'", "NO", invoiceLine.US_SupAdditionalTariff1UQ);
			AssertEquals("Quantity cleared because UQ is different", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff1Qty);

			invoiceLine.US_SupAdditionalTariff1GoodsValue = 500m;
			invoiceLine.US_SupAdditionalTariff1Qty = 200m;
			invoiceLine.SupFormattedAdditionalTariff1 = ZString.Empty;
			AssertEquals("UQ is empty", ZString.Empty, invoiceLine.US_SupAdditionalTariff1UQ);
			AssertEquals("Quantity is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff1Qty);
			AssertEquals("Goods Value is zero", ZDecimal.Zero, invoiceLine.US_SupAdditionalTariff1GoodsValue);
		}
	}
}
