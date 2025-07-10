using System;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.RefDbRepo.ILReferenceData.CmdLine.ExchangeRates;
using CargoWise.RefDbRepo.ILReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.CmdLine
{
	[TestFixture]
	sealed class CustomsExchangeArgumentsParserTest
	{

		[Test]
		public void TryParse_Success()
		{
			var expectedXml = TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.ExchangeRatesRequest_347.xml");

			var result = CustomsExchangeArgumentsParser.TryParse(new string[] { "CUSTOMS_EXCHANGE_RATE_UPDATE" }, out ICurrencyRateSearchParam currencyRateSearchParam, out string messageError);
			Assert.AreEqual(true, result);
			Assert.IsNotNull(currencyRateSearchParam);

			var currencyRate = currencyRateSearchParam.CurrencyRate;
			Assert.IsNotNull(currencyRate);
			Assert.AreEqual(new DateTime(2024, 07, 16), currencyRate.fromDate);
			Assert.AreEqual(new DateTime(2024, 07, 23), currencyRate.toDate);
			Assert.IsNull(currencyRate.currencyTypeID);

			var requestContentHeader = currencyRateSearchParam.RequestContentHeader;
			Assert.IsNotNull(requestContentHeader);
			Assert.AreEqual(new DateTime(2024, 07, 16), requestContentHeader.TransmitionDateTime);
		}

		[SetUp]
		public void SetUp()
		{
			var mock = new Mock<IDateTimeProvider>();
			mock.Setup(i => i.Now).Returns(new DateTime(2024, 07, 16));
			DateTimeUtil.DateTimeProvider = mock.Object;
		}

		[TearDown]
		public void TearDown()
		{
			DateTimeUtil.DateTimeProvider = null;
		}

		void AssertArgumentError(string[] parameters, string expectedErrorMessage)
		{

			var result = CustomsExchangeArgumentsParser.TryParse(parameters, out ICurrencyRateSearchParam currencyRateSearchParam, out string messageError);
			Assert.AreEqual(false, result);
			Assert.IsNull(currencyRateSearchParam);
			Assert.That(messageError.Replace("\r\n", ""), Does.Contain(expectedErrorMessage.Replace("\r\n", "")));
		}
	}
}
