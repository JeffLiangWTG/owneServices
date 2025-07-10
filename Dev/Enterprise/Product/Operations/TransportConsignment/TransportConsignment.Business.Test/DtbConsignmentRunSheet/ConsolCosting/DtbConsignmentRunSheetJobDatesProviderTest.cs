using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRunSheetJobDatesProviderTest : TestCaseWithFactory
	{
		public void TestArrivalDate()
		{
			var year = ZDateTime.Now.Year;

			var consignmentRunSheet = Factory.New<DtbConsignmentRunSheet>();
			var jobDatesProvider = new DtbConsignmentRunSheetRatingAdapter.DtbConsignmentRunSheetJobDatesProvider(consignmentRunSheet);

			var dateToTest = new ZDateTime(year, 5, 7);
			var dateToTestDTO = new ZDateTimeOffset(dateToTest, DateTimeKind.Local);
			consignmentRunSheet.KG_EndTime = dateToTestDTO;
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestDepartureDate()
		{
			var year = ZDateTime.Now.Year;

			var consignmentRunSheet = Factory.New<DtbConsignmentRunSheet>();
			var jobDatesProvider = new DtbConsignmentRunSheetRatingAdapter.DtbConsignmentRunSheetJobDatesProvider(consignmentRunSheet);

			var dateToTest = new ZDateTime(year, 5, 7);
			var dateToTestDTO = new ZDateTimeOffset(dateToTest, DateTimeKind.Local);
			consignmentRunSheet.KG_StartTime = dateToTestDTO;
			AssertEquals(dateToTest, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}
	}
}
