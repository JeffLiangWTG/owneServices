using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderProcessTasks))]
	public class OrderProcessTasksTest : ProcessTaskTest
	{
		#region Defaulting Estimate Date

		[TestDate(2025, 1, 1)]
		public void TestEstimateDefaultedFromOrderDate()
		{
			ProcessTask milestone = Order.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimatedDefaultedFrom = OrderMilestoneEstimateDefaultedFromList.Codes.OrderDate;

			Factory.Save();
			AssertEquals("JD_OrderDate set for the test", true, Order.JD_OrderDate.IsValid);
			AssertEquals("Estimated date defaulted from Order Created date", Order.JD_OrderDate, milestone.P9_ScheduledDate.ToZDateTime());
		}

		[TestDate(2010, 1, 1)]
		public void TestEstimateDefaultedFromOrderLineRequiredDate()
		{
			OrderLine orderLine1 = Order.OrderLines.AddNew();
			OrderLine orderLine2 = Order.OrderLines.AddNew();
			orderLine1.JO_LineDropDate = new ZDateTime(2005, 1, 1);
			orderLine2.JO_LineDropDate = new ZDateTime(2005, 2, 2);

			ProcessTask milestone = Order.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimatedDefaultedFrom = OrderMilestoneEstimateDefaultedFromList.Codes.OrderLineRequiredDate;
			Factory.Save();
			AssertEquals("Estimated date defaulted from Order Created date", orderLine1.JO_LineDropDate, milestone.P9_ScheduledDate.ToZDateTime());
		}

		#endregion

		#region Implementation

		Order Order
		{
			get { return order ?? (order = Factory.NewWithValidTestData<Order>()); }
		}
		Order order;

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var result = (OrderProcessTasks)GetNewBusinessObject();
			result.Parent.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;

			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			Order order = Factory.New<Order>();
			return order.WorkflowItems.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var buyer = factory.NewWithValidTestData<OrgHeader>();

			var orderData = factory.NewWithValidTestData<Order>();
			orderData.BuyerPK = buyer.PK;

			return orderData.WorkflowItems.AddNew();
		}

		#endregion
	}
}
