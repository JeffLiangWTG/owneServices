using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageRoadValidationTest : TestBaseJobVoyageValidation
	{
		#region JV_VoyageFlight

		public void TestValidateJV_VoyageFlight()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "GABRIELA";

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "MARCIONETTI";

			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Road;
			Voyage.JV_RV_NKVessel = vessel1.RV_FK;
			Voyage.JV_VoyageFlight = ZString.Empty;
			AssertHasError(Voyage.JV_VoyageFlightInfo, "Please enter a Truck Ref.");

			Voyage.JV_VoyageFlight = "1234567";
			AssertNoErrors(Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = ZString.Empty;

			JobVoyage voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Core.Constants.TransportModes.Rail;
			voyage2.JV_RV_NKVessel = vessel1.RV_FK;
			voyage2.JV_VoyageFlight = "1234567";
			AssertNoErrors("Journey name the same, different Journey number, no error expected", voyage2.JV_VoyageFlightInfo);

			voyage2.JV_VoyageFlight = ZString.Empty;
			AssertHasNotifications("Journey name/number same as other journey, error expected", voyage2.JV_VoyageFlightInfo);

			voyage2.JV_RV_NKVessel = vessel2.RV_FK;
			voyage2.JV_VoyageFlight = "ABC";
			voyage2.Validation.ValidateJV_VoyageFlight();
			AssertNoErrors("Voyage number correct, no error expected", voyage2.JV_VoyageFlightInfo);
		}

		#endregion

		#region Implementation

		protected override BaseJobVoyageValidation GetValidationObject()
		{
			return new VoyageRoadValidation(Voyage);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Voyage.JV_AirSeaRoad = Constants.TransportModes.Road;
		}

		#endregion
	}
}
