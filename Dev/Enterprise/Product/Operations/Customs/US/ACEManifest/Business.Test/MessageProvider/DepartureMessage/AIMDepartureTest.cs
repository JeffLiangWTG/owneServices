using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMDepartureTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_CarrierCode = "SHA2";
			header.AMA_Voyage = "KLM0569E";
			header.EstDateAtFirstArrival = new ZDateTime(2019, 2, 4, 12, 0, 0);
			header.AMA_E_ARV = new ZDateTime(2019, 3, 6, 12, 0, 0);
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "H0035";
			var departure = CreateAIMDeparture(header, new ZDateTime(2020, 12, 24, 14, 55, 00));
			AssertEquals("KLM0569E", departure.FlightNumber);
			AssertEquals(new ZDate(2019, 2, 4), departure.DateOfScheduledArrival);
			AssertEquals(new ZDate(2020, 12, 24), departure.LiftoffDate);
			AssertEquals("14:55", departure.LiftoffTime);
			AssertEquals(ZString.Empty, departure.ActualImportingCarrier);
			AssertEquals(ZString.Empty, departure.ActualFlightNumber);
			header.EstDateAtFirstArrival = ZDateTime.Empty;
			departure = CreateAIMDeparture(header, new ZDateTime(2020, 12, 24, 14, 55, 00));
			AssertEquals(new ZDate(2019, 3, 6), departure.DateOfScheduledArrival);
			var arrHeader = header.ArrivalHeaders.AddNew();
			arrHeader.ATH_ETAAtDischargePort = new ZDateTime(2019, 4, 8, 12, 0, 0);
			arrHeader.ATH_VoyageFlightNo = "V0669";
			departure = CreateAIMDeparture(header, ZDateTime.Empty);
			AssertEquals("V0669", departure.FlightNumber);
			AssertEquals(new ZDate(2019, 4, 8), departure.DateOfScheduledArrival);
			AssertEquals(ZDate.Empty, departure.LiftoffDate);
			AssertEquals("", departure.LiftoffTime);
		}

		public void TestFlightNumberFromArrival()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var arrival = header.ArrivalHeaders.AddNew();
			arrival.ATH_VoyageFlightNo = "";
			AssertEquals("", CreateAIMDeparture(header).FlightNumber);
			arrival.ATH_VoyageFlightNo = "Q";
			AssertEquals("Q", CreateAIMDeparture(header).FlightNumber);
			header.AMA_CarrierCode = "Q1";
			arrival.ATH_VoyageFlightNo = "Q123";
			AssertEquals("Q1023", CreateAIMDeparture(header).FlightNumber);
			header.AMA_CarrierCode = "QF";
			arrival.ATH_VoyageFlightNo = "QF1";
			AssertEquals("QF001", CreateAIMDeparture(header).FlightNumber);
			arrival.ATH_VoyageFlightNo = "QF01";
			AssertEquals("QF001", CreateAIMDeparture(header).FlightNumber);
			header.AMA_CarrierCode = "";
			arrival.ATH_VoyageFlightNo = "QF001";
			AssertEquals("QF001", CreateAIMDeparture(header).FlightNumber);
			arrival.ATH_VoyageFlightNo = "081001";
			AssertEquals("081001", CreateAIMDeparture(header).FlightNumber);
			arrival.ATH_VoyageFlightNo = "08101";
			AssertEquals("08101", CreateAIMDeparture(header).FlightNumber);
			arrival.ATH_VoyageFlightNo = "08101";
			AssertEquals("08101", CreateAIMDeparture(header).FlightNumber);
			arrival.ATH_VoyageFlightNo = "081";
			AssertEquals("081", CreateAIMDeparture(header).FlightNumber);
			header.AMA_CarrierCode = "081";
			arrival.ATH_VoyageFlightNo = "08101";
			AssertEquals("081001", CreateAIMDeparture(header).FlightNumber);
			arrival.ATH_VoyageFlightNo = "0811";
			AssertEquals("081001", CreateAIMDeparture(header).FlightNumber);
			arrival.ATH_VoyageFlightNo = "081";
			AssertEquals("081", CreateAIMDeparture(header).FlightNumber);
			header.AMA_CarrierCode = "QUAN";
			arrival.ATH_VoyageFlightNo = "QUAN01";
			AssertEquals("QUAN01", CreateAIMDeparture(header).FlightNumber);
		}

		public void TestFlightNumberFromManifest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			header.AMA_Voyage = "";
			AssertEquals("", CreateAIMDeparture(header).FlightNumber);
			header.AMA_Voyage = "Q";
			AssertEquals("Q", CreateAIMDeparture(header).FlightNumber);
			header.AMA_CarrierCode = "Q1";
			header.AMA_Voyage = "Q123";
			AssertEquals("Q1023", CreateAIMDeparture(header).FlightNumber);
			header.AMA_CarrierCode = "QF";
			header.AMA_Voyage = "QF1";
			AssertEquals("QF001", CreateAIMDeparture(header).FlightNumber);
			header.AMA_Voyage = "QF01";
			AssertEquals("QF001", CreateAIMDeparture(header).FlightNumber);
			header.AMA_CarrierCode = "";
			header.AMA_Voyage = "QF001";
			AssertEquals("QF001", CreateAIMDeparture(header).FlightNumber);
			header.AMA_Voyage = "081001";
			AssertEquals("081001", CreateAIMDeparture(header).FlightNumber);
			header.AMA_Voyage = "08101";
			AssertEquals("08101", CreateAIMDeparture(header).FlightNumber);
			header.AMA_Voyage = "08101";
			AssertEquals("08101", CreateAIMDeparture(header).FlightNumber);
			header.AMA_Voyage = "081";
			AssertEquals("081", CreateAIMDeparture(header).FlightNumber);
			header.AMA_CarrierCode = "081";
			header.AMA_Voyage = "08101";
			AssertEquals("081001", CreateAIMDeparture(header).FlightNumber);
			header.AMA_Voyage = "0811";
			AssertEquals("081001", CreateAIMDeparture(header).FlightNumber);
			header.AMA_Voyage = "081";
			AssertEquals("081", CreateAIMDeparture(header).FlightNumber);
			header.AMA_CarrierCode = "QUAN";
			header.AMA_Voyage = "QUAN01";
			AssertEquals("QUAN01", CreateAIMDeparture(header).FlightNumber);
		}

		AIMDeparture CreateAIMDeparture(AsycudaManifestHeader header, ZDateTime? departureTime = null)
		{
			var additionalMessageInformation = new AdditionalMessageInformation(header);
			if (departureTime.HasValue)
			{
				additionalMessageInformation.AM_FlightDepartureTime = departureTime.Value;
			}

			return new AIMDeparture(additionalMessageInformation);
		}
	}
}
