using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsDynamicWorkOrderProcessTasks))]
	public class WhsDynamicWorkOrderProcessTasksTest : WhsDocketProcessTasksTest
	{
		#region TestDynamicWorkOrderProcessTasksCollectionCorrectlyAddsWorkOrderProcessTasks

		public void TestDynamicWorkOrderProcessTasksCollectionCorrectlyAddsWorkOrderProcessTasks()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var workOrder = Helper.CreateWhsDynamicWorkOrder(data.Org1, data.Whs1, "D1");
			var processTask = workOrder.WorkflowItems.AddNew();
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var reloadedProcessTask = otherFactory.Load<WhsDynamicWorkOrderProcessTasks>(processTask.PK);

			ProcessTask newProcessTask = null;
			AssertNoExceptionThrown(() => newProcessTask = new ProcessTaskCollectionView(reloadedProcessTask.ParentTaskCollection).AddNew());
			AssertEquals(typeof(WhsDynamicWorkOrderProcessTasks), newProcessTask.GetType());
		}

		#endregion

		#region TestParentControllerID

		public void TestParentControllerID()
		{
			var processTask = Factory.New<WhsDynamicWorkOrderProcessTasks>();
			AssertEquals(ControllerIDs.WhsDynamicWorkOrder, processTask.ParentControllerID);
		}

		#endregion

		protected override WhsDocket GetNewDocket()
		{
			var workOrder = Factory.New<WhsDynamicWorkOrder>();
			workOrder.WD_OH_Client = GetNewClient().PK;
			workOrder.WD_WW_Whs = GetNewWarehouse().PK;
			workOrder.WD_RequiredDate = ZDateTimeOffset.Now;
			return workOrder;
		}
	}
}
