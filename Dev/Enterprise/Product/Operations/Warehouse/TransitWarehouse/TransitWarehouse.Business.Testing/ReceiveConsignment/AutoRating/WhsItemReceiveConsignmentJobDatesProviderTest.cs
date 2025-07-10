using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class WhsItemReceiveConsignmentJobDatesProviderTest : TestCaseWithFactory
	{
		public void TestArrivalDate()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			receiveConsignment.WRC_CompleteTime = DateTime.Now;
			Factory.Save();

			var completeTime = receiveConsignment.WRC_CompleteTime.ToLocalZDateTime();
			var jobDatesProvider = new WhsItemReceiveConsignmentJobDatesProvider(receiveConsignment);
			jobDatesProvider.SetDate(JobDateTypes.Codes.ArrivalDate, completeTime);
			AssertEquals(completeTime, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}

		public void TestDepartureDate()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var receiveConsignment = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			receiveConsignment.WRC_CompleteTime = DateTime.Now;
			Factory.Save();

			var completeTime = receiveConsignment.WRC_CompleteTime.ToLocalZDateTime();
			var jobDatesProvider = new WhsItemReceiveConsignmentJobDatesProvider(receiveConsignment);
			jobDatesProvider.SetDate(JobDateTypes.Codes.DepartureDate, completeTime);
			AssertEquals(completeTime, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
