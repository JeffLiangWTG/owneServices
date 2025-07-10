using System;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.ILReferenceData.Tests.Business
{
	[TestFixture]
	sealed class ExchangeRatesRequestProviderTest
	{
		[Test]
		public void TestConstructor()
		{
			Assert.Catch<ArgumentNullException>(() => new ExchangeRatesRequestProvider(null), "ArgumentNullException raise when currencyRateSearchParam is null ");
		}

		[Test]
		public void TestGetExchangeRates_ByTableName()
		{
			var expectedXml = TestHelper.GetManifestResourceStream("CargoWise.RefDbRepo.ILReferenceData.Tests.TestFiles.ExchangeRatesRequest_347.xml");

			var customsCodesRequest = new ExchangeRatesRequestProvider(new CurrencyRateSearchParamWrapper(new DateTime(2024, 7, 16), new DateTime(2024, 6, 2), new DateTime(2024, 6, 16), "USD"));
			var request = customsCodesRequest.GetRequestXml();
			Assert.AreEqual(request, expectedXml);
		}
	}
}
