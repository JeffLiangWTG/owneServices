using System;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class ExchangeRateDataRowTest
	{
		[Test]
		public void TestExchangeRateDataRow()
		{
			var exchangeRateDataRow = new ExchangeRateDataRow("AUD201807322.45     22.68     ");
			Assert.AreEqual("AUD", exchangeRateDataRow.Currency);
			Assert.AreEqual(2018, exchangeRateDataRow.Year);
			Assert.AreEqual(7, exchangeRateDataRow.Month);
			Assert.AreEqual(3, exchangeRateDataRow.TenDay);
			Assert.AreEqual(new DateTime(2018, 7, 21), exchangeRateDataRow.StartDate);
			Assert.AreEqual(new DateTime(2018, 7, 31, 23, 59, 00), exchangeRateDataRow.EndDate);
			Assert.AreEqual(22.45, exchangeRateDataRow.InRate);
			Assert.AreEqual(22.68, exchangeRateDataRow.ExRate);
		}

		[Test]
		public void TestExRate()
		{
			var row = new ExchangeRateDataRow("ARS20191110.51806   0.51976   ");
			Assert.AreEqual(0.51806m, row.InRate);
			Assert.AreEqual(0.51976m, row.ExRate);
		}

		[Test]
		[SetCulture("en-US")]
		public void TestExRateWhenUS()
		{
			TestExRate();
		}

		[Test]
		[SetCulture("fr-FR")]
		public void TestExRateWhenFR()
		{
			TestExRate();
		}
	}
}
