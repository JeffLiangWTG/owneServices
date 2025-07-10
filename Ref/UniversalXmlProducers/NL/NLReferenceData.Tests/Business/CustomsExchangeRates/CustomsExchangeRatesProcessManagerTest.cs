using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CargoWise.RefDbRepo.NLReferenceData.Services;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NLReferenceData.Business.Testing
{
	[TestFixture]
	sealed class CustomsExchangeRatesProcessManagerTest
	{
		[Test]
		public void CreateRequest()
		{
			var expectedString = @"<Envelope xmlns=""http://schemas.xmlsoap.org/soap/envelope/"">
	<Header/>
	<Body>
		<ExchangeRate xmlns=""https://exchangerate.douane.nl/ExchangeRateInformation/2014/01/30/"">
			<Declaration xmlns=""urn:wco:datamodel:WCO:RequestForExchangeRateInformation:1"">
				<FunctionCode>91</FunctionCode>
				<AdditionalInformation>
					<RequestedInspectionDateTime>2023-03-01T12:00:00</RequestedInspectionDateTime>
				</AdditionalInformation>
			</Declaration>
		</ExchangeRate>
	</Body>
</Envelope>";

			var requestDate = new System.DateTime(2023, 3, 01, 12, 0, 0);
			var requestString = CustomsExchangeRatesProcessManager.CreateRequest(requestDate);
			var expectedDocument = XDocument.Parse(expectedString);
			var requestDocument = XDocument.Parse(requestString);

			Assert.That(expectedDocument.ToString(), Is.EqualTo(requestDocument.ToString()));
		}

		[Test]
		public void GetSOAPResult()
		{
			var httpClientMock = new Mock<IHttpClientHelper>();
			var exchangeRateBuilderMock = new Mock<IExchangeRatesBuilder<ExchangeRate>>();
			var processManager = new CustomsExchangeRatesProcessManager(exchangeRateBuilderMock.Object) { HttpClientHelper = httpClientMock.Object };
			var requestDate = new System.DateTime(2023, 3, 01, 12, 0, 0);
			var requestContent = CustomsExchangeRatesProcessManager.CreateRequest(requestDate);
			var resultContent = @"<Q:Response xmlns:Q=""urn:wco:datamodel:WCO:ExchangeRateInformation:1"" xmlns:P=""urn:wco:datamodel:WCO:DS:1"">
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
			httpClientMock.Setup(x => x.PostAndReadAsAsyncString("https://exchangerate.douane.nl/ExchangeRateInformation", requestContent, "text/xml")).Returns(Task.FromResult(resultContent));

			var errorCollector = new StringBuilder();
			var result = processManager.GetSOAPResult(requestDate, errorCollector);

			Assert.That(result.Count, Is.EqualTo(3), "Number of elements read");
			Assert.That(result.ElementAt(0), Is.TypeOf<ExchangeRate>(), "Type");
		}
	}
}
