using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsLoadProcessTask))]
	public class WhsLoadProcessTaskTest : ProcessTaskTest
	{
		#region TestWhsLoadProcessTaskCollectionCorrectlyAddsWhsLoadProcessTask

		public void TestWhsLoadProcessTaskCollectionCorrectlyAddsWhsLoadProcessTask()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL00000001", "CDS", startTime: DateTimeOffset.Now);
			var processTask = load.WorkflowItems.AddNew();
			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var reloadedProcessTask = otherFactory.Load<WhsLoadProcessTask>(processTask.PK);

			ProcessTask newProcessTask = null;
			AssertNoExceptionThrown(() => newProcessTask = new ProcessTaskCollectionView(reloadedProcessTask.ParentTaskCollection).AddNew());
			AssertEquals(typeof(WhsLoadProcessTask), newProcessTask.GetType());
		}

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, "WL00000001", "CDS", startTime: DateTimeOffset.Now);
			return load.WorkflowItems.AddNew();
		}

		#region TestParentControllerID

		public void TestParentControllerID()
		{
			var processTask = Factory.New<WhsLoadProcessTask>();
			AssertEquals(ControllerIDs.WhsLoad, processTask.ParentControllerID);
		}

		#endregion

		protected virtual WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));

		WhsTestHelperFunctions helper;
	}
}
