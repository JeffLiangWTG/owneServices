using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketProcessTasksTestCase : WhsBusinessObjectTestCase
	{
		#region TestParentType

		public void TestParentType()
		{
			var docket = GetNewDocket();
			var processTask = docket.WorkflowItems.AddNew();
			AssertEquals(ExpectedParentType, processTask.Parent.GetType());
		}

		protected abstract Type ExpectedParentType { get; }

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Docket.WorkflowItems.AddNew();
		}

		protected WhsDocket Docket
		{
			get { return docket ?? (docket = GetNewDocket()); }
		}

		protected OrgHeader Client
		{
			get { return client ?? (client = GetNewClient()); }
		}

		protected WhsWarehouse Warehouse
		{
			get { return warehouse ?? (warehouse = GetNewWarehouse()); }
		}

		protected virtual OrgHeader GetNewClient()
		{
			return Helper.CreateClient("TST CLT", "Test Client");
		}

		protected virtual WhsWarehouse GetNewWarehouse()
		{
			return Helper.CreateWarehouse("TST WHS", "ROW", 2, 2);
		}

		protected abstract WhsDocket GetNewDocket();

		WhsDocket docket;
		OrgHeader client;
		WhsWarehouse warehouse;

		#endregion
	}
}
