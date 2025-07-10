using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSShipmentJobDatesProviderTests : TestCaseWithFactory
	{
		[TestDate(2014, 5, 8)]
		public void TestArrivalDate()
		{
			var cfsShipment = Factory.New<CFSShipment>();
			var jobDatesProvider = new CFSShipmentJobDatesProvider(cfsShipment);

			AssertEquals(new ZDateTime(2014, 5, 8), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			cfsShipment.JS_A_RCV = new ZDateTime(2014, 5, 10);
			AssertEquals(new ZDateTime(2014, 5, 10), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		[TestDate(2014, 5, 9)]
		public void TestDepartureDate()
		{
			var cfsShipment = Factory.New<CFSShipment>();
			var jobDatesProvider = new CFSShipmentJobDatesProvider(cfsShipment);

			AssertEquals(new ZDateTime(2014, 5, 9), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));

			cfsShipment.JS_A_RCV = new ZDateTime(2014, 5, 10);
			AssertEquals(new ZDateTime(2014, 5, 10), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestJobOpenDate()
		{
			var cfsShipment = Factory.New<CFSShipment>();
			var jobDatesProvider = new CFSShipmentJobDatesProvider(cfsShipment);

			AssertEquals(false, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate).IsValid);

			var dateToTest = ZDateTime.Today;
			var jobHeader = new JobHeader.Loader(cfsShipment).TryLoadOrCreate();
			jobHeader.JH_A_JOP = dateToTest;

			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.JobOpenDate));
		}
	}
}
