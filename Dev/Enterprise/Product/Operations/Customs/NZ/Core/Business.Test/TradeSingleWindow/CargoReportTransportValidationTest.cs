using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Customs.Universal.Testing;

	public class CargoReportTransportValidationTest : TestCaseWithFactory
	{
		public void TestCheckJW_VoyageFlight()
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			transport.JW_VoyageFlight = "AA100";
			try
			{
				transport.IsValidationSuspendedForCargoReport = true;
				transportValidation.ValidateAll();
			}
			finally
			{
				transport.IsValidationSuspendedForCargoReport = false;
			}
			AssertHasMessageErrorContaining(transport.JW_VoyageFlightInfo, CargoReportTransportValidation.FlightNotOnCustomsSupportedList);

			transport.JW_VoyageFlight = "AA220";
			try
			{
				transport.IsValidationSuspendedForCargoReport = true;
				transportValidation.ValidateAll();
			}
			finally
			{
				transport.IsValidationSuspendedForCargoReport = false;
			}
			AssertNoMessageErrorContaining(transport.JW_VoyageFlightInfo, CargoReportTransportValidation.FlightNotOnCustomsSupportedList);
		}

		public void TestCheckJW_Vessel()
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_Vessel = "AA100";
			transportValidation.ValidateAll();
			AssertHasMessageErrorContaining(transport.JW_VesselInfo, CargoReportTransportValidation.VesselNotOnCustomsSupportedList);

			transport.JW_Vessel = "AA220";
			transportValidation.ValidateAll();
			AssertNoMessageErrorContaining(transport.JW_VesselInfo, CargoReportTransportValidation.VesselNotOnCustomsSupportedList);
		}

		protected override void SetUp()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "AA220", "AA220", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			base.SetUp();

			consol = Factory.New<ForwardingConsol>();
			transport = consol.Transports[0];
			transportValidation = new CargoReportTransportValidation(transport);
		}
		ForwardingConsol consol;
		Transport transport;
		CargoReportTransportValidation transportValidation;
	}
}
