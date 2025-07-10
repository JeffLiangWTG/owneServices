using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class ExchangeRateDataTXTTest
	{
		[Test]
		public void TestExchangeRateDataRow()
		{
			var exchangeRateDataRow = new ExchangeRateDataTXT("AUD	109	03	2	19.71   	19.94   ");
			Assert.AreEqual("AUD", exchangeRateDataRow.Currency);
			Assert.AreEqual(new DateTime(2020, 3, 11), exchangeRateDataRow.StartDate);
			Assert.AreEqual(new DateTime(2020, 3, 20, 23, 59, 00), exchangeRateDataRow.EndDate);
			Assert.AreEqual(19.71, exchangeRateDataRow.InRate);
			Assert.AreEqual(19.94, exchangeRateDataRow.ExRate);
		}

		[Test]
		public void TestRates()
		{
			var exchangeRateDataRow = new ExchangeRateDataTXT("AUD	109	03	2	19.71   	19.94   ");
			Assert.AreEqual(19.71, exchangeRateDataRow.InRate);
			Assert.AreEqual(19.94, exchangeRateDataRow.ExRate);
		}

		[Test]
		[SetCulture("en-US")]
		public void TestRatesWhenUS()
		{
			TestRates();
		}

		[Test]
		[SetCulture("fr-FR")]
		public void TestRatesWhenFR()
		{
			TestRates();
		}
	}
}
