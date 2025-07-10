using System;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsTransferProcessTasks))]
	public class WhsTransferProcessTasksTestCase : WhsDocketProcessTasksTestCase
	{
		#region TestParentControllerID

		public void TestParentControllerID()
		{
			var processTask = Factory.New<WhsTransferProcessTasks>();
			AssertEquals(ControllerIDs.WhsTransfer, processTask.ParentControllerID);
		}

		#endregion

		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsTransfer(Client, Warehouse);
		}

		protected override Type ExpectedParentType
		{
			get { return typeof(WhsTransfer); }
		}

		#endregion
	}
}
