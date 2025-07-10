using System.Globalization;
using CargoWise.RefDbRepo.USReferenceData.Business;
using CargoWise.RefDbRepo.USReferenceData.Business.ExchangeRate;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.USReferenceData.Tests
{
	[TestFixture]
	sealed class ExchangeRateSchemaTest
	{
		[Test]
		public void TestToRefExchangeRate()
		{
			var schema = new ExchangeRateSchema
			{
				ISOCode = "AT",
				CountryUnion = "AUSTRIA",
				Rate = "0.087811",
				IND = "D",
				CURCode = "ATS",
			};

			var rate = schema.ToRefExchangeRate();

			Assert.AreEqual(Constants.USCountryCode, rate.ZZN_RN_NKCountry);
			Assert.AreEqual(Constants.ExchangeRate.DefaultRateType, rate.ZZN_ExRateType);

			Assert.AreEqual(decimal.Parse(schema.Rate, CultureInfo.InvariantCulture), rate.ZZN_Rate);
			Assert.AreEqual(schema.CURCode, rate.ZZN_RX_NKExCurrency);
		}

		[Test]
		public void TestToRefExchangeRateWithInvalidRate()
		{
			var schema = new ExchangeRateSchema
			{
				ISOCode = "AT",
				CountryUnion = "AUSTRIA",
				Rate = Constants.ExchangeRate.InvalidRate,
				IND = "D",
				CURCode = "ATS",
			};

			var rate = schema.ToRefExchangeRate();
			Assert.Null(rate);
		}
	}
}
