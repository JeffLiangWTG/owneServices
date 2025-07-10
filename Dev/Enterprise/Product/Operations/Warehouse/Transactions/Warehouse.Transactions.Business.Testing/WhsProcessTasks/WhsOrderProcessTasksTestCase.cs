using System;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderProcessTasks))]
	public class WhsOrderProcessTasksTestCase : WhsDocketProcessTasksTestCase
	{
		#region TestParentTypeNew

		public new void TestParentType()
		{
			var order = Helper.CreateWhsOrder(Client, Warehouse);
			var processTaskOrder = order.WorkflowItems.AddNew();
			AssertEquals(typeof(WhsOrder), processTaskOrder.Parent.GetType());
		}

		#endregion

		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsOrder(Client, Warehouse);
		}

		protected override Type ExpectedParentType
		{
			get { return typeof(WhsOrder); }
		}

		#endregion
	}
}
