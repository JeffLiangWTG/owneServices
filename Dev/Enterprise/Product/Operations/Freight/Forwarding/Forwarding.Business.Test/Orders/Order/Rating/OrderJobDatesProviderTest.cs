using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderJobDatesProviderTest : TestCaseWithFactory
	{
		public void TestDepartureDate()
		{
			var order = Factory.New<Order>();
			var milestone = order.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.DepartureCode;
			var actualDate = new ZDateTime(2014, 5, 7);
			var estimatedDate = new ZDateTimeOffset(new ZDateTime(2014, 5, 8));
			milestone.SetMilestoneActualDateForTest(actualDate);
			milestone.SetMilestoneScheduledDateForTest(estimatedDate);

			var jobDatesProvider = new OrderJobDatesProvider(order);
			AssertEquals(actualDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));

			milestone.SetMilestoneActualDateForTest(ZDateTime.Empty);
			AssertEquals(estimatedDate.ToZDateTime(), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.DepartureDate));
		}

		public void TestArrivalDate()
		{
			var order = Factory.New<Order>();
			var milestone = order.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			var actualDate = new ZDateTime(2014, 5, 7);
			var estimatedDate = new ZDateTimeOffset(new ZDateTime(2014, 5, 8));
			milestone.SetMilestoneActualDateForTest(actualDate);
			milestone.SetMilestoneScheduledDateForTest(estimatedDate);

			var jobDatesProvider = new OrderJobDatesProvider(order);
			AssertEquals(actualDate, jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));

			milestone.SetMilestoneActualDateForTest(ZDateTime.Empty);
			AssertEquals(estimatedDate.ToZDateTime(), jobDatesProvider.GetJobDateByType(JobDateTypes.Codes.ArrivalDate));
		}
	}
}
