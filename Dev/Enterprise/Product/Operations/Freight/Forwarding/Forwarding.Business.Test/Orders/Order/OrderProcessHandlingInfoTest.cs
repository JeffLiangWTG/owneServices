using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestPopulateCascadingTargets_FromOrderToOrderLine()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var orderLine1 = order.OrderLines.AddNew();
			var orderLine2 = order.OrderLines.AddNew();

			var orderTrigger = CreateTrigger(order);
			var orderLineTrigger1 = CreateTrigger(orderLine1);
			var orderLineTrigger2 = CreateTrigger(orderLine2);

			Factory.Save();

			var eventLog = Factory.New<StmALog>();
			using (eventLog.LockForUpdatingKeyFieldsForTesting())
			{
				eventLog.SL_SE_NKEvent = Events.ItemDocumentJobFinalisedCode;
			}

			var processHandlingInfo = new OrderProcessHandlingInfo(order);
			var targets = processHandlingInfo.GetCascadingTargets(eventLog);
			AssertContainsExactElementsInAnyOrder(new[] { orderLine1.PK, orderLine2.PK }, targets.Select(x => ((OrderLine)x.Parent).PK));

			var target1 = targets.FirstOrDefault(t => (OrderLine)t.Parent == orderLine1);
			AssertNotNull(target1);
			AssertEquals(orderLineTrigger1, target1.Triggers.FirstOrDefault());

			var target2 = targets.FirstOrDefault(t => (OrderLine)t.Parent == orderLine2);
			AssertNotNull(target2);
			AssertEquals(orderLineTrigger2, target2.Triggers.FirstOrDefault());
		}

		#region Implementation

		ProcessTask CreateTrigger(IWorkflowProvider parent)
		{
			var trigger = parent.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ItemDocumentJobFinalisedCode;
			trigger.P9_RespondToCascadedEvents = true;

			return trigger;
		}

		#endregion
	}
}
