using System;
using System.Linq;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Services.ExchangeRates;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.Common;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.ExchangeRates
{
	[TestFixture]
	sealed class MessageConversionHelperTests
	{
		[Test]
		public void Convert_Valid()
		{
			var effDate = new DateTime(2016, 4, 3);
			var msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.ExchangeRates.TestFiles.Input.Gesmes_Valid_01.txt");
			var logger = new TestLogger();

			var msg = CreateMessage(msgContent);

			var exchange = MessageConversionHelper.Convert(msg, logger);

			Assert.That(exchange, Is.Not.Null);
			Assert.That(exchange.PublishDate, Is.EqualTo(msg.CreatedDate));
			Assert.That(logger.ErrorString, Is.EqualTo(string.Empty));
			Assert.That(exchange.ExchangeRates, Is.Not.Null.And.Not.Empty);
			Assert.That(exchange.ExchangeRates.Count, Is.EqualTo(18 * 4));

			var rate = exchange.ExchangeRates.First(x => x.Currency == "CHF");
			Assert.That(rate.CountryCode, Is.Not.Empty);
			Assert.That(rate.RateType, Is.EqualTo("CUS"));
			Assert.That(rate.StartDate, Is.EqualTo(effDate));
			Assert.That(rate.StartDate, Is.EqualTo(effDate));
			Assert.That(rate.Rate, Is.EqualTo(0.064m));

			var countryCodes = exchange.ExchangeRates.Select(x => x.CountryCode).Distinct().OrderBy(x => x).ToList();

			Assert.That(countryCodes.Count, Is.EqualTo(4));
			Assert.That(countryCodes[0], Is.EqualTo("LS"));
			Assert.That(countryCodes[1], Is.EqualTo("NA"));
			Assert.That(countryCodes[2], Is.EqualTo("SZ"));
			Assert.That(countryCodes[3], Is.EqualTo("ZA"));
		}

		[Test]
		public void Convert_Invalid()
		{
			var msgContent = TestHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.ZAReferenceData.Tests.Services.ExchangeRates.TestFiles.Input.Gesmes_Invalid.txt");
			var logger = new TestLogger();

			var msg = CreateMessage(msgContent);

			var exchange = MessageConversionHelper.Convert(msg, logger);

			Assert.That(exchange, Is.Null);
			Assert.That(logger.ErrorString, Contains.Substring("Expected 1 occurrence(s) of 'DTM' message section"));
		}

		[TestCase("Any", "Any")]
		[TestCase("USD", "USD")]
		[TestCase("ZWD", "ZWL")]
		public void TestCurrencyConversion(string ccy, string expectedccy)
		{
			var actual = MessageConversionHelper.ConvertCurrency(ccy);
			Assert.AreEqual(expectedccy, actual, $"{ccy} should be converted to {expectedccy}");
		}

		SourceDataMessage CreateMessage(string content)
		{
			return new SourceDataMessage
			{
				Content = content,
				CreatedDate = DateTime.Now,
				ID = Guid.NewGuid(),
				PublishDate = DateTime.Today,
				Status = "QUE"
			};
		}
	}
}
