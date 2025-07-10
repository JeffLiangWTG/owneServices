using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketProcessTasksTest : ProcessTaskTest
	{
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

		protected abstract WhsDocket GetNewDocket();

		protected virtual WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		protected virtual OrgHeader GetNewClient()
		{
			return Helper.CreateClient("TST CLT", "Test Client");
		}

		protected virtual WhsWarehouse GetNewWarehouse()
		{
			return Helper.CreateWarehouse("TST WHS", "ROW", 2, 2);
		}

		WhsDocket docket;
		OrgHeader client;
		WhsWarehouse warehouse;
		WhsTestHelperFunctions helper;

		#endregion
	}
}
