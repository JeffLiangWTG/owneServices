using System.Linq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Services.Testing
{
	[TestFixture]
	sealed class CustomsExchangeRatesSOAPResponseParserTest
	{
		[Test]
		public void Parse()
		{
			var testXmlText = @"<Q:Response xmlns:Q=""urn:wco:datamodel:WCO:ExchangeRateInformation:1"" xmlns:P=""urn:wco:datamodel:WCO:DS:1"">
							  <Q:FunctionCode>94</Q:FunctionCode>
							  <Q:Declaration>
								<Q:AdditionalInformation>
								  <Q:RequestedInspectionDateTime>2023-11-29T07:14:11</Q:RequestedInspectionDateTime>
								</Q:AdditionalInformation>
								<Q:CurrencyExchange>
								  <Q:RateNumeric>4.3353964</Q:RateNumeric>
								  <Q:CurrencyTypeCode>PLN</Q:CurrencyTypeCode>
								</Q:CurrencyExchange>
								<Q:CurrencyExchange>
								  <Q:RateNumeric>4.9728388</Q:RateNumeric>
								  <Q:CurrencyTypeCode>RON</Q:CurrencyTypeCode>
								</Q:CurrencyExchange>
								<Q:CurrencyExchange>
								  <Q:RateNumeric>24.259433</Q:RateNumeric>
								  <Q:CurrencyTypeCode>CZK</Q:CurrencyTypeCode>
								</Q:CurrencyExchange>
							  </Q:Declaration>
							</Q:Response>";

			var parser = new CustomsExchangeRatesSOAPResponseParser(testXmlText);
			var response = parser.Parse();

			Assert.That(response.Count, Is.EqualTo(3), "Number of exchange rates");

			Assert.That(response.ElementAt(0).FunctionCode, Is.EqualTo("94"), "Element 1 Function Code");
			Assert.That(response.ElementAt(0).RateNumeric, Is.EqualTo(4.3353964m), "Element 1 Rate Numeric");
			Assert.That(response.ElementAt(0).CurrencyTypeCode, Is.EqualTo("PLN"), "Element 1 CurrencyTypeCode");

			Assert.That(response.ElementAt(1).FunctionCode, Is.EqualTo("94"), "Element 2 Function Code");
			Assert.That(response.ElementAt(1).RateNumeric, Is.EqualTo(4.9728388m), "Element 2 Rate Numeric");
			Assert.That(response.ElementAt(1).CurrencyTypeCode, Is.EqualTo("RON"), "Element 2 CurrencyTypeCode");

			Assert.That(response.ElementAt(2).FunctionCode, Is.EqualTo("94"), "Element 3 Function Code");
			Assert.That(response.ElementAt(2).RateNumeric, Is.EqualTo(24.259433m), "Element 3 Rate Numeric");
			Assert.That(response.ElementAt(2).CurrencyTypeCode, Is.EqualTo("CZK"), "Element 3 CurrencyTypeCode");
		}
	}
}
