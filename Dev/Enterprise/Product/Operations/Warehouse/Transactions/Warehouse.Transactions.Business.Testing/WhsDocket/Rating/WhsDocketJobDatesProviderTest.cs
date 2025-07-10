using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsDocketJobDatesProviderTest : TestCaseWithFactory
	{
		[TestDate(2014, 5, 6)]
		public void TestArrivalDate()
		{
			var whsDocket = Factory.New<WhsReceive>();
			var jobDatesProvider = new WhsDocketJobDatesProvider(whsDocket);

			AssertEquals(ZDateTime.Today, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			var finalisedDate = ZDateTimeOffset.Today.AddDays(1);
			whsDocket.WD_FinalisedDate = finalisedDate;

			AssertEquals(finalisedDate.ToDateTime(), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		[TestDate(2014, 5, 6)]
		public void TestDepartureDate()
		{
			var whsDocket = Factory.New<WhsReceive>();
			var jobDatesProvider = new WhsDocketJobDatesProvider(whsDocket);

			AssertEquals(ZDateTime.Today, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));

			var finalisedDate = ZDateTimeOffset.Today.AddDays(1);
			whsDocket.WD_FinalisedDate = finalisedDate;

			AssertEquals(finalisedDate.ToDateTime(), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestJobOpenDate()
		{
			var whsDocket = Factory.New<WhsReceive>();
			var jobDatesProvider = new WhsDocketJobDatesProvider(whsDocket);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);

			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(whsDocket).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}
	}
}
