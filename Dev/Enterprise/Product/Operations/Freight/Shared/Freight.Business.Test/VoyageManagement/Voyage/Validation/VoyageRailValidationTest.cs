using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageRailValidationTest : TestBaseJobVoyageValidation
	{
		#region JV_VoyageFlight

		public void TestValidateJV_VoyageFlight()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "GABRIELA";

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "MARCIONETTI";

			Voyage.JV_RV_NKVessel = vessel1.RV_FK;
			Voyage.JV_VoyageFlight = ZString.Empty;

			Voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "HKHKG";

			AssertHasError(Voyage.JV_VoyageFlightInfo, "Please enter a Voyage Number.");

			Voyage.JV_VoyageFlight = "1234567";
			AssertNoWarnings("Journey number not empty, no warning expected.", Voyage.JV_VoyageFlightInfo);

			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
			voyage2.JV_RV_NKVessel = vessel1.RV_FK;
			voyage2.JV_VoyageFlight = "1234567";
			AssertHasErrors("Journey number same as journey number for different journey with same vessel, error expected", voyage2.JV_VoyageFlightInfo);

			voyage2.JV_VoyageFlight = "12346";
			AssertNoNotifications("Voyage number correct, no error expected", voyage2.JV_VoyageFlightInfo);

			voyage2.JV_VoyageFlight = "1234567";
			AssertHasErrors("Voyage number same as voyage number for different voyage with same vessel, error expected", voyage2.JV_VoyageFlightInfo);

			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage2.Validation.ValidateJV_VoyageFlight();
			AssertNoNotifications("Voyage number correct, no error expected", voyage2.JV_VoyageFlightInfo);
		}

		#endregion

		#region Implementation

		protected override BaseJobVoyageValidation GetValidationObject()
		{
			return new VoyageRailValidation(Voyage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
		}

		#endregion
	}
}
