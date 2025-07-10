using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageJobDatesProviderTest : TestCaseWithFactory
	{
		public void TestArrivalDate()
		{
			var year = ZDateTime.Now.Year;
			var move = Factory.New<CommonBookedCtgMove>();
			var adapter = new CartageMoveRatingAdapter(move, Factory.New<JobDocAddress>(), Factory.New<JobDocAddress>());
			var jobDatesProvider = ((IAutoRating)adapter).JobDatesProvider;
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
			move.CartageLegs.AddNew();
			var leg = move.CartageLegs.AddNew();
			var deliverTimeOut = new ZDateTime(year, 5, 12);
			leg.JU_PickupTimeIn = new ZDateTime(year, 3, 1);
			leg.JU_DeliverTimeOut = deliverTimeOut;
			adapter = new CartageMoveRatingAdapter(move, Factory.New<JobDocAddress>(), Factory.New<JobDocAddress>());
			jobDatesProvider = ((IAutoRating)adapter).JobDatesProvider;
			AssertEquals(deliverTimeOut, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestDepartureDate()
		{
			var year = ZDateTime.Now.Year;
			var move = Factory.New<CommonBookedCtgMove>();
			var adapter = new CartageMoveRatingAdapter(move, Factory.New<JobDocAddress>(), Factory.New<JobDocAddress>());
			var jobDatesProvider = ((IAutoRating)adapter).JobDatesProvider;
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
			var leg = move.CartageLegs.AddNew();
			var pickupTimeIn = new ZDateTime(year, 5, 12);
			leg.JU_PickupTimeIn = pickupTimeIn;
			leg.JU_DeliverTimeOut = new ZDateTime(year, 3, 1);
			move.CartageLegs.AddNew();
			adapter = new CartageMoveRatingAdapter(move, Factory.New<JobDocAddress>(), Factory.New<JobDocAddress>());
			jobDatesProvider = ((IAutoRating)adapter).JobDatesProvider;
			AssertEquals(pickupTimeIn, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestJobOpenDate()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ShippingTransportMode = Core.Constants.TransportModes.All;
			var move = Factory.New<CommonBookedCtgMove>();
			move.EW_JJ = cartage.PK;
			var adapter = new CartageMoveRatingAdapter(move, Factory.New<JobDocAddress>(), Factory.New<JobDocAddress>());
			var jobDatesProvider = ((IAutoRating)adapter).JobDatesProvider;
			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);
			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(move.Cartage).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;
			adapter = new CartageMoveRatingAdapter(move, Factory.New<JobDocAddress>(), Factory.New<JobDocAddress>());
			jobDatesProvider = ((IAutoRating)adapter).JobDatesProvider;
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}
	}
}
