using System;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsAdjustmentProcessTasks))]
	public class WhsAdjustmentProcessTasksTestCase : WhsDocketProcessTasksTestCase
	{
		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsAdjustment(Client, Warehouse);
		}

		protected override Type ExpectedParentType
		{
			get { return typeof(WhsAdjustment); }
		}

		#endregion
	}
}
