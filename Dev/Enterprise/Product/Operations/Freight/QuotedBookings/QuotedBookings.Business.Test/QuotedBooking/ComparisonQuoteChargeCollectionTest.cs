using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(ComparisonQuoteChargeCollection))]
	public class ComparisonQuoteChargeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ComparisonQuoteChargeCollection>
	{
		#region Test Cases

		public void TestCurrencyCharges()
		{
			TestCollection.Add(new ComparisonQuoteCharge("Test1", 10, "USD", 20));
			TestCollection.Add(new ComparisonQuoteCharge("Test2", 15, "AUD", 25));
			TestCollection.Add(new ComparisonQuoteCharge("Test3", 20, "USD", 30));
			TestCollection.Add(new ComparisonQuoteCharge("Test4", 10, "RUB", 20));
			TestCollection.Add(new ComparisonQuoteCharge("Test5", 50, "USD", 60));

			AssertEquals(3, TestCollection.GetChargesBasedOnCurrency("USD").Count);
			AssertEquals(1, TestCollection.GetChargesBasedOnCurrency("AUD").Count);
			AssertEquals(1, TestCollection.GetChargesBasedOnCurrency("RUB").Count);

			ComparisonQuoteChargeCollection uSDCharges = TestCollection.GetChargesBasedOnCurrency("USD");
			AssertCharge("Test1", 10, "USD", 20, uSDCharges[0]);
			AssertCharge("Test3", 20, "USD", 30, uSDCharges[1]);
			AssertCharge("Test5", 50, "USD", 60, uSDCharges[2]);
		}

		protected void AssertCharge(ZString description, ZDecimal oSSellAmount, ZString oSCurrency, ZDecimal localSellAmount, ComparisonQuoteCharge charge)
		{
			AssertEquals(description, charge.Description);
			AssertEquals(oSSellAmount, charge.OSSellAmount);
			AssertEquals(oSCurrency, charge.OSSellCurrency);
			AssertEquals(localSellAmount, charge.LocalSellAmount);
		}

		public void TestCurrencies()
		{
			TestCollection.Add(new ComparisonQuoteCharge("Test1", 10, "USD", 20));
			TestCollection.Add(new ComparisonQuoteCharge("Test2", 15, "AUD", 25));
			TestCollection.Add(new ComparisonQuoteCharge("Test3", 20, "USD", 30));
			TestCollection.Add(new ComparisonQuoteCharge("Test4", 10, "RUB", 20));
			TestCollection.Add(new ComparisonQuoteCharge("Test5", 50, "USD", 60));

			AssertEquals(3, TestCollection.Currencies.Count);
			TestCollection.Currencies.Contains("USD");
			TestCollection.Currencies.Contains("AUD");
			TestCollection.Currencies.Contains("RUB");
		}

		public void TestTotalOSSellAmount()
		{
			TestCollection.Add(new ComparisonQuoteCharge("Test1", 10, "USD", 20));
			TestCollection.Add(new ComparisonQuoteCharge("Test2", 15, "USD", 25));
			TestCollection.Add(new ComparisonQuoteCharge("Test3", 20, "USD", 30));

			AssertEquals(45m, TestCollection.TotalOSSellAmount);
		}

		public void TestTotalLocalSellAmount()
		{
			TestCollection.Add(new ComparisonQuoteCharge("Test1", 10, "USD", 20));
			TestCollection.Add(new ComparisonQuoteCharge("Test2", 15, "USD", 25));
			TestCollection.Add(new ComparisonQuoteCharge("Test3", 20, "USD", 30));

			AssertEquals(75m, TestCollection.TotalLocalSellAmount);
		}

		public void TestCleanZeroCharges()
		{
			TestCollection.Add(new ComparisonQuoteCharge("Test1", 10, "USD", 20));
			TestCollection.Add(new ComparisonQuoteCharge("Test2", 15, "AUD", 25));
			TestCollection.Add(new ComparisonQuoteCharge("Test3", 0, "USD", 30));
			TestCollection.Add(new ComparisonQuoteCharge("Test4", 10, "RUB", 20));
			TestCollection.Add(new ComparisonQuoteCharge("Test5", 0, "USD", 60));

			TestCollection.RemoveZeroValueCharges();

			AssertEquals(3, TestCollection.Count);
			AssertCharge("Test1", 10, "USD", 20, TestCollection[0]);
			AssertCharge("Test2", 15, "AUD", 25, TestCollection[1]);
			AssertCharge("Test4", 10, "RUB", 20, TestCollection[2]);
		}

		public void TestAddCharge()
		{
			Action<JobCharge, decimal> setSellExchangeRateProperly = (c, r) =>
			{
				var exRateWrap = c.GetType().GetProperty("RevenueExchangeRate").GetValue(c);
				var method = exRateWrap?.GetType().GetMethod("SetBuyRate_ForTestOnly").Invoke(exRateWrap, new object[] { r });
			};

			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_Desc = "Test1";
			charge.JR_InvoiceType = "CUR";
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 10m;
			setSellExchangeRateProperly(charge, 0.5m);
			charge.JR_LocalSellAmt = 20m;
			TestCollection.Add(charge);

			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_Desc = "Test2";
			charge.JR_InvoiceType = "CUR";
			charge.JR_RX_NKSellCurrency = "EUR";
			charge.JR_OSSellAmt = 15;
			setSellExchangeRateProperly(charge, 0.6m);
			charge.JR_LocalSellAmt = 25;
			TestCollection.Add(charge);

			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_Desc = "Test3";
			charge.JR_InvoiceType = "CUR";
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 20;
			setSellExchangeRateProperly(charge, 0.66666m);
			charge.JR_LocalSellAmt = 30;
			TestCollection.Add(charge);

			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_Desc = "Test4";
			charge.JR_InvoiceType = "CUR";
			charge.JR_RX_NKSellCurrency = ZString.Empty;
			charge.JR_OSSellAmt = 10;
			setSellExchangeRateProperly(charge, 0.5m);
			charge.JR_LocalSellAmt = 20;
			TestCollection.Add(charge);

			charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_Desc = "Test5";
			charge.JR_InvoiceType = "CUR";
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 50;
			setSellExchangeRateProperly(charge, 0.8333m);
			charge.JR_LocalSellAmt = 60;
			TestCollection.Add(charge);

			AssertEquals(5, TestCollection.Count);
			AssertCharge("Test1", 10m, "USD", 20, TestCollection[0]);
			AssertCharge("Test2", 15m, "EUR", 25, TestCollection[1]);
			AssertCharge("Test3", 20m, "USD", 30, TestCollection[2]);
			AssertCharge("Test4", 10m, "", 20, TestCollection[3]);
			AssertCharge("Test5", 50m, "USD", 60, TestCollection[4]);
		}

		public void AddComparisonQuoteCharge()
		{
			TestCollection.Add(new ComparisonQuoteCharge("Test1", 10, "USD", 20));
			TestCollection.Add(new ComparisonQuoteCharge("Test2", 15, "AUD", 25));
			TestCollection.Add(new ComparisonQuoteCharge("Test3", 20, "USD", 30));
			TestCollection.Add(new ComparisonQuoteCharge("Test4", 10, "RUB", 20));
			TestCollection.Add(new ComparisonQuoteCharge("Test5", 50, "USD", 60));

			AssertEquals(5, TestCollection.Count);
			AssertCharge("Test1", 10m, "USD", 20, TestCollection[0]);
			AssertCharge("Test2", 15m, "AUD", 25, TestCollection[1]);
			AssertCharge("Test3", 20m, "USD", 30, TestCollection[2]);
			AssertCharge("Test4", 10m, "RUB", 20, TestCollection[3]);
			AssertCharge("Test5", 50m, "USD", 60, TestCollection[4]);
		}

		#endregion

		#region Implementation

		protected override ComparisonQuoteChargeCollection GetCollectionToTest()
		{
			return new ComparisonQuoteChargeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComparisonQuoteCharge();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new ComparisonQuoteChargeCollection();
		}

		protected ComparisonQuoteChargeCollection TestCollection;

		#endregion
	}
}
