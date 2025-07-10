using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal abstract class AgencyShipmentJobDatesProviderTest : ShipmentJobDatesProviderBaseTest
	{
		public void TestJobArrivalDate()
		{
			var shipment = GetAgencyShipment();
			AssertEquals(ZDateTime.Empty, shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
			var voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "DEBER";
			JobSailing sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(origin.JA_RL_NKPortOfLoading, destination.JB_RL_NKPortOfDischarge);
			shipment.JS_JX = sailing.PK;
			sailing.Destination.JB_E_ARV = new ZDateTime(2010, 10, 10);
			AssertEquals(new ZDateTime(2010, 10, 10), shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
			sailing.Destination.JB_A_ARV = new ZDateTime(2011, 11, 11);
			AssertEquals(new ZDateTime(2011, 11, 11), shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
			shipment.JS_E_ARV = new ZDateTime(2012, 12, 12);
			AssertEquals(new ZDateTime(2012, 12, 12), shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestJobDepartureDate()
		{
			var shipment = GetAgencyShipment();
			AssertEquals(ZDateTime.Empty, shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			var voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "DEBER";
			JobSailing sailing = voyage.Sailings.GetSailingFromLoadAndDischarge(origin.JA_RL_NKPortOfLoading, destination.JB_RL_NKPortOfDischarge);
			shipment.JS_JX = sailing.PK;
			sailing.Origin.JA_E_DEP = new ZDateTime(2010, 10, 10);
			AssertEquals(new ZDateTime(2010, 10, 10), shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			sailing.Origin.JA_A_DEP = new ZDateTime(2011, 11, 11);
			AssertEquals(new ZDateTime(2011, 11, 11), shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			shipment.JS_E_DEP = new ZDateTime(2012, 12, 12);
			AssertEquals(new ZDateTime(2012, 12, 12), shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestJobOpenDate()
		{
			var shipment = GetAgencyShipment();
			AssertEquals(false, shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);
			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(shipment).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;
			AssertEquals(dateToTest, shipment.RatingAdapter.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}

		protected abstract AgencyShipment GetAgencyShipment();
	}
}
