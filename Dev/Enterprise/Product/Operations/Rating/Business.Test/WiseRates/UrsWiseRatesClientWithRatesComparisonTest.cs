using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business.WiseRates;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Tools.Exceptions;

namespace Enterprise.Rating.Business.Testing;

public class UrsWiseRatesClientWithRatesComparisonTest : TestCaseWithFactory
{
	public void TestSearchAsync_ShouldReturnResultFromUrs()
	{
		var ratesServiceResponse = new RatesSearchResponse();
		ratesServiceResponse.Rates = [new Rate()];
		var ratesServiceClient = new Mock<IWiseRatesClient>();
		ratesServiceClient
			.Setup(c => c.SearchAsync(
				It.IsAny<RatesSearchRequest>(),
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult(ratesServiceResponse))
			.Verifiable();

		var ursResponse = new RatesSearchResponse();
		ursResponse.Rates = [new Rate()];
		var ursClient = new Mock<IWiseRatesClient>();
		ursClient
			.Setup(c => c.SearchAsync(
				It.IsAny<RatesSearchRequest>(),
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult(ursResponse))
			.Verifiable();

		var ratesServiceSwitcher = new UrsWiseRatesClientWithRatesComparison(ratesServiceClient.Object, ursClient.Object);
		var response = ratesServiceSwitcher.SearchAsync(new RatesSearchRequest()).GetAwaiter().GetResult();
		AssertEquals(ursResponse, response);

		ratesServiceClient.Verify();
		ursClient.Verify();
	}

	public void TestSearchAsync_WhenRatesServiceFails_ShouldStillReturnResultFromUrs()
	{
		var ratesServiceResponse = new RatesSearchResponse();
		ratesServiceResponse.Rates = [new Rate()];
		var ratesServiceClient = new Mock<IWiseRatesClient>();
		ratesServiceClient
			.Setup(c => c.SearchAsync(
				It.IsAny<RatesSearchRequest>(),
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.ThrowsAsync(new HttpResponseException(HttpStatusCode.InternalServerError, "It happens"))
			.Verifiable();

		var ursResponse = new RatesSearchResponse();
		ursResponse.Rates = [new Rate()];
		var ursClient = new Mock<IWiseRatesClient>();
		ursClient
			.Setup(c => c.SearchAsync(
				It.IsAny<RatesSearchRequest>(),
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult(ursResponse))
			.Verifiable();

		var ratesServiceSwitcher = new UrsWiseRatesClientWithRatesComparison(ratesServiceClient.Object, ursClient.Object);
		var response = ratesServiceSwitcher.SearchAsync(new RatesSearchRequest()).GetAwaiter().GetResult();
		AssertEquals(ursResponse, response);

		ratesServiceClient.Verify();
		ursClient.Verify();
	}

	public void TestSearchAsync_WhenRatesAreDifferent_SendCorrelationReport()
	{
		IEnumerable<string> actualMessages = null;

		var ratesServiceResponse = new RatesSearchResponse();
		ratesServiceResponse.Rates =
		[
			CreateRate("1", "UAIEV", "AUSYD", "Emirates", "Standard", [
				CreateCharge("FRT", ">", 0, 10),
				CreateCharge("WAR", ">", 0, 100)]),
			CreateRate("2", "UAIEV", "AUSYD", "Qantas", "Standard", [CreateCharge("FRT", ">", 100, 20)]),	// Missing rate
			CreateRate("3", "UAIEV", "AUSYD", "Qantas", "Express", [
				CreateCharge("FRT", ">", 100, 30),
				CreateCharge("FRT", ">", 200, 25),
				CreateCharge("FRT", ">", 300, 20)]),
			CreateRate("4", "UAIEV", "AUSYD", "Etihad", "Express", [
				CreateCharge("FRT", ">", 0, 10),
				CreateCharge("BAF", ">", 0, 15),
				CreateCharge("CAF", ">", 0, 20)]),
			CreateRate("5", "UAIEV", "AUSYD", "Lufthansa", "Standard", [CreateCharge("FRT", ">", 0, 10)]),
			CreateRate("6", "UAIEV", "AUSYD", "Singapore Airlines", "Standard", [
				CreateCharge("FRT", ">", 1000, 1),
				CreateCharge("FRT", ">", 2000, 2)]),
		];

		var ratesServiceClient = new Mock<IWiseRatesClient>();
		ratesServiceClient.SetupGet(c => c.ServiceURL).Returns("http://rates-service");
		ratesServiceClient
			.Setup(c => c.SearchAsync(
				It.IsAny<RatesSearchRequest>(),
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult(ratesServiceResponse));
		ratesServiceClient
			.Setup(c => c.SendKafkaMessageAsync(It.IsAny<IEnumerable<string>>()))
			.Callback((IEnumerable<string> message) => actualMessages = message);

		var ursResponse = new RatesSearchResponse();
		ursResponse.Rates =
		[
			CreateRate("1", "UAIEV", "AUSYD", "Emirates", "Standard", [
				CreateCharge("FRT", ">", 0, 10),
				CreateCharge("WAR", ">", 0, 100)]),															// Full Match
			CreateRate("2", "UAIEV", "AUSYD", "Etihad", "Standard", [CreateCharge("FRT", ">", 100, 20)]),	// Extra rate
			CreateRate("3", "UAIEV", "AUSYD", "Qantas", "Express", [
				CreateCharge("FRT", ">", 100, 30),
				CreateCharge("FRT", ">", 300, 20)]),													// Missing break (>200)
			CreateRate("4", "UAIEV", "AUSYD", "Etihad", "Express", [
				CreateCharge("FRT", ">", 0, 10),
				CreateCharge("CAF", ">", 0, 20)]),														// Missing charge (BAF)
			CreateRate("5", "UAIEV", "AUSYD", "Lufthansa", "Standard", [
				CreateCharge("FRT", ">", 0, 10),
				CreateCharge("CAF", ">", 0, 20)]),														// Extra charge (CAF)
			CreateRate("6", "UAIEV", "AUSYD", "Singapore Airlines", "Standard", [
				CreateCharge("FRT", ">", 1000, 1),
				CreateCharge("FRT", ">", 2000, 3)]),													// Invalid rate (per unit rate 3 instead of 2)
		];

		var ursClient = new Mock<IWiseRatesClient>();
		ursClient.SetupGet(c => c.ServiceURL).Returns("http://urs-service");
		ursClient
			.Setup(c => c.SearchAsync(
				It.IsAny<RatesSearchRequest>(),
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.Returns(Task.FromResult(ursResponse));

		var ratesServiceSwitcher = new UrsWiseRatesClientWithRatesComparison(ratesServiceClient.Object, ursClient.Object);
		ratesServiceSwitcher.SearchAsync(new RatesSearchRequest { RatesQuery = new RatesQuery { Origin = ["AUSYD"] } }, "mclaren").GetAwaiter().GetResult();

		var expectedNotes = @"URS Rate 3 doesn't match 3 Rates Service rate

Rates Service Charges:
FRT|None||20|||||||>||300|False
FRT|None||25|||||||>||200|False
FRT|None||30|||||||>||100|False

URS Charges:
FRT|None||20|||||||>||300|False
FRT|None||30|||||||>||100|False

============================
URS Rate 4 doesn't match 4 Rates Service rate

Rates Service Charges:
BAF|None||15|||||||>||0|False
CAF|None||20|||||||>||0|False
FRT|None||10|||||||>||0|False

URS Charges:
CAF|None||20|||||||>||0|False
FRT|None||10|||||||>||0|False

============================
URS Rate 5 doesn't match 5 Rates Service rate

Rates Service Charges:
FRT|None||10|||||||>||0|False

URS Charges:
CAF|None||20|||||||>||0|False
FRT|None||10|||||||>||0|False

============================
URS Rate 6 doesn't match 6 Rates Service rate

Rates Service Charges:
FRT|None||1|||||||>||1000|False
FRT|None||2|||||||>||2000|False

URS Charges:
FRT|None||1|||||||>||1000|False
FRT|None||3|||||||>||2000|False
";

		var actualObject = (JObject)JsonConvert.DeserializeObject(actualMessages.Single());
		AssertEquals("mclaren", actualObject["fields"]["TraceId"].Value<string>());
		AssertEquals("UrsIntegration", actualObject["fields"]["SourceContext"].Value<string>());
		AssertEquals("EDI", actualObject["fields"]["UserSystem"].Value<string>());
		AssertEquals("failed", actualObject["fields"]["Result"].Value<string>());
		AssertEquals("http://rates-service", actualObject["fields"]["RatesServiceUrl"].Value<string>());
		AssertEquals("http://urs-service", actualObject["fields"]["UrsUrl"].Value<string>());
		AssertEquals(6, actualObject["fields"]["Correlation"]["RatesServiceRatesCount"].Value<int>());
		AssertEquals(6, actualObject["fields"]["Correlation"]["UrsRatesCount"].Value<int>());
		AssertEquals(1, actualObject["fields"]["Correlation"]["MissingRatesCount"].Value<int>());
		AssertEquals("||UAIEV|AUSYD|Qantas|Standard|1/01/0001 12:00:00 AM||||||||||", actualObject["fields"]["Correlation"]["MissingRates"].Value<string>());
		AssertEquals(1, actualObject["fields"]["Correlation"]["NonExpectedRatesCount"].Value<int>());
		AssertEquals("||UAIEV|AUSYD|Etihad|Standard|1/01/0001 12:00:00 AM||||||||||", actualObject["fields"]["Correlation"]["NonExpectedRates"].Value<string>());
		AssertEquals(4, actualObject["fields"]["Correlation"]["InvalidRatesCount"].Value<int>());
		AssertEquals(expectedNotes, actualObject["fields"]["Correlation"]["Notes"].Value<string>());
	}

	Rate CreateRate(string id, string origin, string destination, string carrier, string serviceLevel, Charge[] charges)
	{
		return new Rate
		{
			Id = id,
			Origin = origin,
			Destination = destination,
			Carrier = carrier,
			ServiceLevel = serviceLevel,
			Charges = charges
		};
	}

	Charge CreateCharge(string chargeCode, string breakOperator, int breakValue, decimal perUnitRate)
	{
		return new Charge
		{
			ChargeCode = chargeCode,
			BreakOperator = breakOperator,
			Break = breakValue,
			PerUnitRate = perUnitRate
		};
	}
}
