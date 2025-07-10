using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsWorkOrderProcessTasks))]
	public class WhsWorkOrderProcessTasksTestCase : WhsDocketProcessTasksTestCase
	{
		#region TestWorkOrderProcessTasksCollectionCorrectlyAddsWorkOrderProcessTasks

		public void TestWorkOrderProcessTasksCollectionCorrectlyAddsWorkOrderProcessTasks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var workOrder = Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			var processTask = workOrder.WorkflowItems.AddNew();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var reloadedProcessTask = otherFactory.Load<WhsWorkOrderProcessTasks>(processTask.PK);

			ProcessTask newProcessTask = null;
			AssertNoExceptionThrown(() => newProcessTask = new ProcessTaskCollectionView(reloadedProcessTask.ParentTaskCollection).AddNew());
			AssertEquals(typeof(WhsWorkOrderProcessTasks), newProcessTask.GetType());
		}

		#endregion

		#region TestParentControllerID

		public void TestParentControllerID()
		{
			var processTask = Factory.New<WhsWorkOrderProcessTasks>();
			AssertEquals(ControllerIDs.WhsWorkOrder, processTask.ParentControllerID);
		}

		#endregion

		#region Implementation

		protected override WhsDocket GetNewDocket()
		{
			return Helper.CreateWhsWorkOrder(Client, Warehouse);
		}

		protected override Type ExpectedParentType
		{
			get { return typeof(WhsWorkOrder); }
		}

		#endregion
	}
}
