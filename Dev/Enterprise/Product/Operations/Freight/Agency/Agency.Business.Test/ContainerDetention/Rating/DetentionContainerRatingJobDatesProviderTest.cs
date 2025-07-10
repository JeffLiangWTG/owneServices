using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business
{
	internal class DetentionContainerRatingJobDatesProviderTest : TestCaseWithFactory
	{
		public void TestArrivalDate()
		{
			var dateToTest = new ZDateTime(2014, 5, 7);
			var movement = Factory.New<ContainerMovement>();
			movement.E9_MovementDate = dateToTest;
			var jobDatesProvider = new DetentionContainerRatingJobDatesProvider(movement);
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestDepartureDate()
		{
			var dateToTest = new ZDateTime(2014, 5, 9);
			var movement = Factory.New<ContainerMovement>();
			movement.E9_MovementDate = dateToTest;
			var jobDatesProvider = new DetentionContainerRatingJobDatesProvider(movement);
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestJobOpenDate()
		{
			var detention = Factory.New<ContainerDetention>();
			var movement = Factory.New<ContainerMovement>();
			detention.Movements.Add(movement);
			var jobDatesProvider = new DetentionContainerRatingJobDatesProvider(movement);
			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);
			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(detention).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}
	}
}
