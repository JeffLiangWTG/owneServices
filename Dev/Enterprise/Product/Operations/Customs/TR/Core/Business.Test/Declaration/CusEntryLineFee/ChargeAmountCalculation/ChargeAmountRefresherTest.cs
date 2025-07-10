using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class ChargeAmountRefresherTest : TestCaseWithFactory
	{
		public void TestChargeAmountIsRecalculatedIfNotEmptyAndInPercentage()
		{
			var lineFee = Factory.New<CusEntryLineFee>();

			lineFee.CF_BaseValue = 20.145m;
			lineFee.CF_Rate = 15m;
			AssertEquals("Charge Amount should be divided by 100", 3.0218m, lineFee.CF_ChargeAmount);

			lineFee.CF_BaseValue = 30.324m;
			lineFee.CF_Rate = 20m;
			AssertEquals("Charge Amount should be 6000 even if it was not zero before", 6.0648m, lineFee.CF_ChargeAmount);
		}

		public void TestGetNewChargeAmountCalculator()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			var chargeAmountRefresher = new ChargeAmountRefresher(lineFee);
			AssertType<PercentageChargeAmountCalculator>("Charge Amount Calculator Type", chargeAmountRefresher.GetNewChargeAmountCalculator());
		}

		public void TestShouldRefreshChargeAmount()
		{
			var lineFee = Factory.New<CusEntryLineFee>();
			var chargeAmountRefresherForTest = new ChargeAmountRefresherForTest(lineFee);
			AssertEquals("ShouldRefreshChargeAmount", true, chargeAmountRefresherForTest.ShouldRefreshChargeAmountExposed);
		}

		class ChargeAmountRefresherForTest : ChargeAmountRefresher
		{
			public ChargeAmountRefresherForTest(CusEntryLineFee lineFee) : base(lineFee)
			{
			}

			public bool ShouldRefreshChargeAmountExposed => ShouldRefreshChargeAmount;
		}
	}
}
