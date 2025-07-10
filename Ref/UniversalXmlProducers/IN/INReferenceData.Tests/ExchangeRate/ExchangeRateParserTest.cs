using System;
using System.Linq;
using CargoWise.RefDbRepo.INReferenceData.Business;
using CargoWise.RefDbRepo.INReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.INReferenceData.Tests
{
	[TestFixture]
	public class ExchangeRateParserTest
	{
		[Test]
		public void TestParseResponse()
		{
			var goodResponse = @"
{
  ""status"": 1,
  ""notStartDate"": ""06-09-2024"",
  ""notEndDate"": ""05-09-2024"",
  ""currencyDetail"": [
    {
      ""currencyCode"": ""AED"",
      ""currencyDesc"": ""UAE Dirham"",
      ""cbicImport"": 23.6,
      ""cbicExport"": 22.2,
      ""units"": ""1.0""
    }]
}
";
			var exchangeRates = ExchangeRateParser.ParseResponse(goodResponse);

			Assert.AreEqual(2, exchangeRates.Count);

			var importRate = exchangeRates.FirstOrDefault(x => x.ZZN_ExRateType == Constants.ExchangeRate.Types.Customs);
			Assert.NotNull(importRate);
			Assert.AreEqual(importRate.ZZN_Rate, 23.6m);
			Assert.AreEqual(importRate.ZZN_RX_NKExCurrency, "AED");
			Assert.AreEqual(importRate.ZZN_AsPublished, "1 AED = 23.6 INR");

			var exportRate = exchangeRates.FirstOrDefault(x => x.ZZN_ExRateType == Constants.ExchangeRate.Types.CustomsExport);
			Assert.NotNull(exportRate);
			Assert.AreEqual(exportRate.ZZN_Rate, 22.2m);
			Assert.AreEqual(exportRate.ZZN_RX_NKExCurrency, "AED");
			Assert.AreEqual(exportRate.ZZN_AsPublished, "1 AED = 22.2 INR");

		}

		[Test]
		public void TestParseResponse_BadResponse()
		{
			var badResponse = "{\"status\": 0}";
			Assert.Throws<UnhandledApplicationException>(() => ExchangeRateParser.ParseResponse(badResponse));
		}
	}
}
