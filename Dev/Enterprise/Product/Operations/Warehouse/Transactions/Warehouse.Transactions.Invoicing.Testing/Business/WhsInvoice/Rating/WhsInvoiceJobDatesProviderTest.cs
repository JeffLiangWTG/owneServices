using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	public class WhsInvoiceJobDatesProviderTest : TestCaseWithFactory
	{
		[TestDate(2014, 5, 6)]
		public void TestArrivalDate()
		{
			var whsInvoice = Factory.New<WhsInvoice>();
			var jobDatesProvider = new WhsInvoiceJobDatesProvider(whsInvoice);
			whsInvoice.ET_StorageFromDate = ZDateTime.Today.AddDays(-1);
			whsInvoice.ET_StorageToDate = ZDateTime.Today;

			AssertEquals(ZDateTime.Today, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		[TestDate(2014, 5, 6)]
		public void TestDepartureDate()
		{
			var whsInvoice = Factory.New<WhsInvoice>();
			var jobDatesProvider = new WhsInvoiceJobDatesProvider(whsInvoice);
			whsInvoice.ET_StorageFromDate = ZDateTime.Today;

			AssertEquals(ZDateTime.Today, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestJobOpenDate()
		{
			var whsInvoice = Factory.New<WhsInvoice>();
			var jobDatesProvider = new WhsInvoiceJobDatesProvider(whsInvoice);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);

			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(whsInvoice).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}
	}
}
