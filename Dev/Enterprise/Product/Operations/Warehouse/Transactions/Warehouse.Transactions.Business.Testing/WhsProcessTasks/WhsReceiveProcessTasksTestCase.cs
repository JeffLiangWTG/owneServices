using System;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReceiveProcessTasks))]
	public class WhsReceiveProcessTasksTestCase : WhsDocketProcessTasksTestCase
	{
		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsReceive(Client, Warehouse);
		}

		protected override Type ExpectedParentType
		{
			get { return typeof(WhsReceive); }
		}

		#endregion
	}
}
