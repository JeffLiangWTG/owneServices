using System;

using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class ExchangeRateDataJSONTest
	{
		[Test]
		public void TestExchangeRateDataRow()
		{
			var exchangeRateDataRow = new ExchangeRateDataJSON("AUD", new DateTime(2020, 3, 11), new DateTime(2020, 3, 20, 23, 59, 00), 19.71m, 19.94m);
			Assert.AreEqual("AUD", exchangeRateDataRow.Currency);
			Assert.AreEqual(new DateTime(2020, 3, 11), exchangeRateDataRow.StartDate);
			Assert.AreEqual(new DateTime(2020, 3, 20, 23, 59, 00), exchangeRateDataRow.EndDate);
			Assert.AreEqual(19.71m, exchangeRateDataRow.InRate);
			Assert.AreEqual(19.94m, exchangeRateDataRow.ExRate);
		}

		[Test]
		public void TestRates()
		{
			var exchangeRateDataRow = new ExchangeRateDataJSON("AUD", new DateTime(2020, 3, 11), new DateTime(2020, 3, 20, 23, 59, 00), 19.71m, 19.94m);
			Assert.AreEqual(19.71m, exchangeRateDataRow.InRate);
			Assert.AreEqual(19.94m, exchangeRateDataRow.ExRate);
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
