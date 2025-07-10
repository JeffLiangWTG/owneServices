using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMFlightHelperTest : TestCaseWithFactory
	{
		public void TestCalculateArrivalDateFromFlightDetail()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.EstDateAtFirstArrival = ZDate.Empty;
			header.AMA_E_ARV = new ZDate(2020, 08, 04);
			var flight = new FlightDetail(Factory);
			flight.IsArrival = true;
			flight.FlightArrivalDate = ZDate.Empty;
			AssertEquals(flight.FlightArrivalDate, AIMFlightHelper.CalculateArrivalDate(header, flight));
			flight.FlightArrivalDate = new ZDate(2020, 08, 06);
			AssertEquals(flight.FlightArrivalDate, AIMFlightHelper.CalculateArrivalDate(header, flight));
			flight.IsArrival = false;
			AssertEquals(header.AMA_E_ARV, AIMFlightHelper.CalculateArrivalDate(header, flight));
			header.EstDateAtFirstArrival = new ZDate(2020, 08, 09);
			AssertEquals(header.EstDateAtFirstArrival, AIMFlightHelper.CalculateArrivalDate(header, flight));
		}

		public void TestCheckCombinedCarrierFlight()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Flight number empty, error expected", "You have not entered a Flight.", AIMFlightHelper.CheckCombinedCarrierFlight(ZString.Empty, ZString.Empty));
				AssertEquals("Flight number has less than 2 characters, error expected.", ValidationConstants.FlightNumberIsWrongLength, AIMFlightHelper.CheckCombinedCarrierFlight("1", ZString.Empty));
				AssertEquals("Flight number has more than 8 characters, error expected.", ValidationConstants.FlightNumberIsWrongLength, AIMFlightHelper.CheckCombinedCarrierFlight("123456789", ZString.Empty));
				AssertEquals("Flight number contains non-alphanumeric character, error expected.", ValidationConstants.FlightNumberIsNonCompliant, AIMFlightHelper.CheckCombinedCarrierFlight("A234!6", ZString.Empty));
				AssertEquals("The 8th character of the Flight number is a number, error expected.", ValidationConstants.FlightNumberIsNonCompliant, AIMFlightHelper.CheckCombinedCarrierFlight("12345678", ZString.Empty));
				AssertEquals("flight number too short, error expected.", ValidationConstants.FlightNumberIsNonCompliant, AIMFlightHelper.CheckCombinedCarrierFlight("QF1", ZString.Empty));
				AssertEquals("Carrier part matches carrier, not expecting errors.", ZString.Empty, AIMFlightHelper.CheckCombinedCarrierFlight("QF1", "QF"));
				AssertEquals("flight number too short, error expected.", ValidationConstants.FlightNumberIsNonCompliant, AIMFlightHelper.CheckCombinedCarrierFlight("QF12", ZString.Empty));
				AssertEquals("Carrier part matches carrier, not expecting errors.", ZString.Empty, AIMFlightHelper.CheckCombinedCarrierFlight("QF12", "QF"));
				AssertEquals("Carrier part does not match carrier, error expected.", ValidationConstants.FlightNumberIsNonCompliant, AIMFlightHelper.CheckCombinedCarrierFlight("QF12", "QU"));
				AssertEquals("carrier too long, ignored. error expected.", ValidationConstants.FlightNumberIsNonCompliant, AIMFlightHelper.CheckCombinedCarrierFlight("QUAN1", "QUAN"));
				AssertEquals("Carrier part matches carrier, not expecting errors.", ZString.Empty, AIMFlightHelper.CheckCombinedCarrierFlight("QF1A", "QF"));
				AssertEquals("Carrier part matches carrier, not expecting errors.", ZString.Empty, AIMFlightHelper.CheckCombinedCarrierFlight("QF12A", "QF"));
				AssertEquals("flight number has invalid characters, error expected.", ValidationConstants.FlightNumberIsNonCompliant, AIMFlightHelper.CheckCombinedCarrierFlight("QF1AB", "QF"));
				AssertEquals("flight number has invalid characters, error expected.", ValidationConstants.FlightNumberIsNonCompliant, AIMFlightHelper.CheckCombinedCarrierFlight("QF1A2B", "QF"));
				AssertEquals("flight number has invalid characters, error expected.", ValidationConstants.FlightNumberIsNonCompliant, AIMFlightHelper.CheckCombinedCarrierFlight("QF12AB", "QF"));
				AssertEquals("flight number has invalid characters, error expected.", ValidationConstants.FlightNumberIsNonCompliant, AIMFlightHelper.CheckCombinedCarrierFlight("QF123AB", "QF"));
				AssertEquals("flight number has invalid characters, error expected.", ValidationConstants.FlightNumberIsNonCompliant, AIMFlightHelper.CheckCombinedCarrierFlight("QF12A4", "QF"));
				AssertEquals("flight number has invalid characters, error expected.", ValidationConstants.FlightNumberIsNonCompliant, AIMFlightHelper.CheckCombinedCarrierFlight("QF12A4B", "QF"));
				AssertEquals("Valid combined flight number, not expecting errors.", ZString.Empty, AIMFlightHelper.CheckCombinedCarrierFlight("QF123", ZString.Empty));
				AssertEquals("Valid combined flight number, not expecting errors.", ZString.Empty, AIMFlightHelper.CheckCombinedCarrierFlight("QF123V", ZString.Empty));
				AssertEquals("Valid combined flight number, not expecting errors.", ZString.Empty, AIMFlightHelper.CheckCombinedCarrierFlight("QF1234", ZString.Empty));
				AssertEquals("Valid combined flight number, not expecting errors.", ZString.Empty, AIMFlightHelper.CheckCombinedCarrierFlight("QF1234V", ZString.Empty));
				AssertEquals("Valid combined flight number, not expecting errors.", ZString.Empty, AIMFlightHelper.CheckCombinedCarrierFlight("1234567A", ZString.Empty));
				AssertEquals("Valid combined flight number, not expecting errors.", ZString.Empty, AIMFlightHelper.CheckCombinedCarrierFlight("KLM325", ZString.Empty));
			});
		}

		public void TestCreatePaddedFlightNumberForMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var arrival = header.ArrivalHeaders.AddNew();
			header.AMA_CarrierCode = "";
			arrival.ATH_VoyageFlightNo = "";
			AssertEquals("", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			arrival.ATH_VoyageFlightNo = "QF1";
			AssertEquals("QF1", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			arrival.ATH_VoyageFlightNo = "QF01";
			AssertEquals("QF01", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			arrival.ATH_VoyageFlightNo = "QF001";
			AssertEquals("QF001", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			header.AMA_CarrierCode = "QF";
			arrival.ATH_VoyageFlightNo = "QF1";
			AssertEquals("QF001", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			arrival.ATH_VoyageFlightNo = "QF01";
			AssertEquals("QF001", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			arrival.ATH_VoyageFlightNo = "QF001";
			AssertEquals("QF001", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			arrival.ATH_VoyageFlightNo = "QF1A";
			AssertEquals("QF001A", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			arrival.ATH_VoyageFlightNo = "QF01A";
			AssertEquals("QF001A", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			arrival.ATH_VoyageFlightNo = "QF001A";
			AssertEquals("QF001A", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			arrival.ATH_VoyageFlightNo = "QF1AB";
			AssertEquals("QF1AB", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			header.AMA_CarrierCode = "QFN";
			arrival.ATH_VoyageFlightNo = "QF1";
			AssertEquals("QF1", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			header.AMA_CarrierCode = "KLM";
			arrival.ATH_VoyageFlightNo = "KL1";
			AssertEquals("KL1", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			header.AMA_CarrierCode = "KLM";
			arrival.ATH_VoyageFlightNo = "KLM1";
			AssertEquals("KLM001", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
			header.AMA_CarrierCode = "KLM1";
			arrival.ATH_VoyageFlightNo = "KLM1";
			AssertEquals("KLM1", AIMFlightHelper.CreatePaddedFlightNumberForMessage(arrival.ATH_VoyageFlightNo, header.AMA_CarrierCode));
		}
	}
}
