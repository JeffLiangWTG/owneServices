using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsAdHocServiceJobJobDatesProviderTest : TestCaseWithFactory
	{
		[TestDate(2016, 3, 14)]
		public void TestArrivalDate()
		{
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			var jobDatesProvider = new WhsAdHocServiceJobJobDatesProvider(adHocServiceJob);

			AssertEquals(ZDateTime.Today, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			var billingDate = ZDateTime.Today.AddDays(1);
			adHocServiceJob.BillingDate = billingDate;

			AssertEquals(billingDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		[TestDate(2016, 3, 14)]
		public void TestDepartureDate()
		{
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			var jobDatesProvider = new WhsAdHocServiceJobJobDatesProvider(adHocServiceJob);

			AssertEquals(ZDateTime.Today, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));

			var billingDate = ZDateTime.Today.AddDays(1);
			adHocServiceJob.BillingDate = billingDate;

			AssertEquals(billingDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestJobOpenDate()
		{
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			var jobDatesProvider = new WhsAdHocServiceJobJobDatesProvider(adHocServiceJob);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);

			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(adHocServiceJob).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}
	}
}
