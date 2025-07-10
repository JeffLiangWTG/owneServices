using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSContainerStorageJobDateProviderTest : TestCaseWithFactory
	{
		public void TestArrivalDate()
		{
			var container = Factory.NewWithValidTestData<CFSContainer>();
			var jobDatesProvider = new CFSContainerStorageJobDateProvider(container);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate).IsValid);

			var dateToTest = new ZDateTime(2006, 5, 8);
			container.JC_ArrivalTime = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestDepartureDate()
		{
			var container = Factory.NewWithValidTestData<CFSContainer>();
			var jobDatesProvider = new CFSContainerStorageJobDateProvider(container);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate).IsValid);

			var dateToTest = new ZDateTime(2006, 5, 8);
			container.JC_DepartureTime = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestJobOpenDate()
		{
			var container = Factory.NewWithValidTestData<CFSContainer>();
			var jobDatesProvider = new CFSContainerStorageJobDateProvider(container);

			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));

			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(container).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}
	}
}
