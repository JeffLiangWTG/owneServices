using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	public class StoragePeriodJobDatesProviderTest : TestCaseWithFactory
	{
		public void TestArrivalDate()
		{
			var whsInvoice = Factory.NewWithValidTestData<WhsInvoice>();
			var jobDatesProvider = new StoragePeriodJobDatesProvider(whsInvoice);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			var date = new ZDateTime(2014, 5, 12);
			jobDatesProvider.SetDate(JobDateTypes.Codes.ArrivalDate, date);

			AssertEquals(date, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestDepartureDate()
		{
			var whsInvoice = Factory.NewWithValidTestData<WhsInvoice>();
			var jobDatesProvider = new StoragePeriodJobDatesProvider(whsInvoice);
			AssertEquals(ZDateTime.Empty, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));

			var date = new ZDateTime(2014, 5, 12);
			jobDatesProvider.SetDate(JobDateTypes.Codes.DepartureDate, date);

			AssertEquals(date, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestJobOpenDate()
		{
			var whsInvoice = Factory.NewWithValidTestData<WhsInvoice>();
			var jobDatesProvider = new StoragePeriodJobDatesProvider(whsInvoice);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);

			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(whsInvoice).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}
	}
}