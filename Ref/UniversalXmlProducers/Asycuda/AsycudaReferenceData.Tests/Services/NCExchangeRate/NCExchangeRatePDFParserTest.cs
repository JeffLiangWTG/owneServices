using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.AsycudaReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Tests.Services;

[TestFixture]
sealed class NCExchangeRatePDFParserTest
{
	[Test]
	public void TestParsePDF()
	{
		var inputFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"AsycudaTestFiles\NCExchangeRate.pdf");
		var exchangeRates = NCExchangeRatePDFParser.ParsePDF(inputFilePath);
		Assert.That(exchangeRates.StartDate, Is.EqualTo(new DateTime(2025, 03, 01)));
		Assert.That(exchangeRates.EndDate, Is.EqualTo(new DateTime(2025, 03, 31)));
		Assert.That(exchangeRates.ExchangeRateDetails.Count, Is.EqualTo(29));
		var gbpRate = exchangeRates.ExchangeRateDetails.FirstOrDefault(x => x.Code == "GBP");
		Assert.That(gbpRate, !Is.Null);
		Assert.That(gbpRate.Rate, Is.EqualTo(145.44966));
	}
}
