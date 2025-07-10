using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DtbBookingJobDatesProviderTest : TestCaseWithFactory
	{
		public void TestDepartureDate()
		{
			var now = ZDateTime.Now;
			var booking = Helper.CreateBooking();
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var picConfirmation = Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);
			var dlvConfirmation = Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.Delivery);

			var jobDatesProvider = new DtbBookingJobDatesProvider(booking);
			AssertEquals("Precondition", ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));

			picConfirmation.KK_Estimated = now.AddDays(1);
			dlvConfirmation.KK_Estimated = now.AddDays(3);
			AssertEquals("No Actuals, so use Earliest Estimated.", now.AddDays(1), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));

			picConfirmation.KK_Actual = now.AddDays(3);
			dlvConfirmation.KK_Actual = now.AddDays(2);
			AssertEquals("Has Actuals, so use Earliest Actual.", now.AddDays(2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestArrivalDate()
		{
			var now = ZDateTime.Now;
			var booking = Helper.CreateBooking();
			var picInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var picConfirmation = Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);
			var dlvConfirmation = Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.Delivery);

			var jobDatesProvider = new DtbBookingJobDatesProvider(booking);
			AssertEquals("Precondition", ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			picConfirmation.KK_Estimated = now.AddDays(1);
			dlvConfirmation.KK_Estimated = now.AddDays(3);
			AssertEquals("No Actuals, so use Lastest Estimated.", now.AddDays(3), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			picConfirmation.KK_Actual = now.AddDays(2);
			dlvConfirmation.KK_Actual = now.AddDays(1);
			AssertEquals("Has Actuals, so use Lastest Actual.", now.AddDays(2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestJobOpenDate()
		{
			var booking = Helper.CreateBooking();
			var jobDatesProvider = new DtbBookingJobDatesProvider(booking);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);

			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(booking).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
