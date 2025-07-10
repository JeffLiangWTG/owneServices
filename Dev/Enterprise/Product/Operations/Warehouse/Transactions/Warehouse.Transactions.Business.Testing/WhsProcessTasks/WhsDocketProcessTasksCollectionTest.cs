using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketProcessTasksCollectionTest : WhsBusinessObjectCollectionTestCase
	{
		#region TestHasNewConditionBeenMetSinceLastSave

		public void TestHasNewConditionBeenMetSinceLastSave()
		{
			var docket = GetNewDocket();
			var processTask = docket.WorkflowItems.AddNew();

			docket.CancelReactivateDocket();
			AssertEquals(false, processTask.Parent.WorkflowItems.HasNewConditionBeenMetSinceLastSave());

			Factory.Save();
			AssertEquals(false, processTask.Parent.WorkflowItems.HasNewConditionBeenMetSinceLastSave());

			docket.CancelReactivateDocket();
			AssertEquals(true, processTask.Parent.WorkflowItems.HasNewConditionBeenMetSinceLastSave());
		}

		#endregion

		#region Overrides

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return GetNewCollection();
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			disposable = ProcessTaskCollection.CanCreateTaskCollection();
		}

		protected override void TearDown()
		{
			disposable.Dispose();
			base.TearDown();
		}

		IDisposable disposable;

		protected abstract WhsDocketProcessTasksCollection GetNewCollection();

		protected WhsDocket Docket
		{
			get { return docket ?? (docket = GetNewDocket()); }
		}

		protected WhsWarehouse Warehouse
		{
			get { return warehouse ?? (warehouse = GetNewWarehouse()); }
		}

		protected OrgHeader Client
		{
			get { return client ?? (client = GetNewClient()); }
		}

		protected abstract WhsDocket GetNewDocket();

		protected virtual WhsWarehouse GetNewWarehouse()
		{
			return Helper.CreateWarehouse("TST");
		}

		protected virtual OrgHeader GetNewClient()
		{
			return Helper.CreateClient();
		}

		OrgHeader client;
		WhsWarehouse warehouse;
		WhsDocket docket;

		#endregion
	}
}
