using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class DtbBookingConsignmentJobDatesProviderTest : TestCaseWithFactory
	{
		public void TestDepartureDate()
		{
			var now = ZDateTime.Now;
			var consignment = Helper.CreateBookingConsignment();
			var picInstruction = Helper.CreateInstruction(consignment, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(consignment, InstructionTypes.Codes.Delivery);
			var picConfirmation1 = Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);
			var picConfirmation2 = Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);
			var dlvConfirmation = Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			var jobDatesProvider = new DtbBookingConsignmentJobDatesProvider(consignment, picInstruction, dlvInstruction);
			AssertEquals("Precondition", ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));

			picConfirmation1.KK_Estimated = now.AddDays(3);
			picConfirmation2.KK_Estimated = now.AddDays(2);
			dlvConfirmation.KK_Estimated = now.AddDays(1);
			AssertEquals("No Actuals, so use Earliest Estimated from PickUp instruction.", now.AddDays(2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));

			picConfirmation1.KK_Actual = now.AddDays(3);
			picConfirmation2.KK_Actual = now.AddDays(4);
			dlvConfirmation.KK_Actual = now.AddDays(2);
			AssertEquals("Has Actuals, so use Earliest Actual from PickUp instruction.", now.AddDays(3), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestArrivalDate()
		{
			var now = ZDateTime.Now;
			var consignment = Helper.CreateBookingConsignment();
			var picInstruction = Helper.CreateInstruction(consignment, InstructionTypes.Codes.PickUp);
			var dlvInstruction = Helper.CreateInstruction(consignment, InstructionTypes.Codes.Delivery);
			var picConfirmation = Helper.CreateConfirmation(picInstruction, ConfirmationTypes.Codes.PickUp);
			var dlvConfirmation1 = Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);
			var dlvConfirmation2 = Helper.CreateConfirmation(dlvInstruction, ConfirmationTypes.Codes.Delivery);

			var jobDatesProvider = new DtbBookingConsignmentJobDatesProvider(consignment, picInstruction, dlvInstruction);
			AssertEquals("Precondition", ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			picConfirmation.KK_Estimated = now.AddDays(3);
			dlvConfirmation1.KK_Estimated = now.AddDays(2);
			dlvConfirmation2.KK_Estimated = now.AddDays(1);
			AssertEquals("No Actuals, so use Lastest Estimated from Delivery instruction.", now.AddDays(2), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			picConfirmation.KK_Actual = now.AddDays(4);
			dlvConfirmation1.KK_Actual = now.AddDays(2);
			dlvConfirmation2.KK_Actual = now.AddDays(3);
			AssertEquals("Has Actuals, so use Lastest Actual from Delivery instruction.", now.AddDays(3), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestJobOpenDate()
		{
			var consignment = Helper.CreateBookingConsignment();
			var fromInstruction = Helper.CreateInstruction(consignment, InstructionTypes.Codes.PickUp);
			var toInstruction = Helper.CreateInstruction(consignment, InstructionTypes.Codes.Delivery);
			var jobDatesProvider = new DtbBookingConsignmentJobDatesProvider(consignment, fromInstruction, toInstruction);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);

			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(consignment).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}

		#region Implementation

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
