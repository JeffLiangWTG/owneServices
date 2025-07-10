using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(ComparisonQuoteCharge))]
	public class ComparisonQuoteChargeTest : NonPersistentBusinessObjectTestCase
	{
		#region Test Cases
		public void TestConstructors()
		{
			TestComparisonQuoteCharge = new ComparisonQuoteCharge();
			AssertEquals(ZString.Empty, TestComparisonQuoteCharge.OSSellCurrency);
			AssertEquals(ZDecimal.Zero, TestComparisonQuoteCharge.OSSellAmount);
			AssertEquals(ZDecimal.Zero, TestComparisonQuoteCharge.LocalSellAmount);
			AssertEquals(ZString.Empty, TestComparisonQuoteCharge.Description);
			TestComparisonQuoteCharge = new ComparisonQuoteCharge("TEST", 10.1, "AUD", 20.1);
			AssertEquals("TEST", TestComparisonQuoteCharge.Description);
			AssertEquals(10.1m, TestComparisonQuoteCharge.OSSellAmount);
			AssertEquals("AUD", TestComparisonQuoteCharge.OSSellCurrency);
			AssertEquals(20.1m, TestComparisonQuoteCharge.LocalSellAmount);
		}

		public void TestLocalSellAmount()
		{
			AssertEquals(ZDecimal.Zero, TestComparisonQuoteCharge.LocalSellAmount);
			TestComparisonQuoteCharge.LocalSellAmount = 20.4;
			AssertEquals(20.4m, TestComparisonQuoteCharge.LocalSellAmount);
		}

		public void TestOSSellCurrency()
		{
			AssertEquals(ZString.Empty, TestComparisonQuoteCharge.OSSellCurrency);
			TestComparisonQuoteCharge.OSSellCurrency = "RUB";
			AssertEquals("RUB", TestComparisonQuoteCharge.OSSellCurrency);
		}

		public void TestOSSellAmount()
		{
			AssertEquals(ZDecimal.Zero, TestComparisonQuoteCharge.OSSellAmount);
			TestComparisonQuoteCharge.OSSellAmount = 21.2;
			AssertEquals(21.2m, TestComparisonQuoteCharge.OSSellAmount);
		}

		public void TestDescription()
		{
			AssertEquals(ZString.Empty, TestComparisonQuoteCharge.Description);
			TestComparisonQuoteCharge.Description = "Description";
			AssertEquals("Description", TestComparisonQuoteCharge.Description);
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			TestComparisonQuoteCharge = new ComparisonQuoteCharge();
		}

		protected ComparisonQuoteCharge TestComparisonQuoteCharge;
		#endregion
	}
}
