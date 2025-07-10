using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.US.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Warehouse.Transactions.Business.US.Testing
{
	//  IMPORTANT: CHANGING THE DATA SETUPS IN THESE HELPERS IS EXTREMELY LIKELY TO INVALIDATE DEPENDANT AUTO TESTS
	//  DO NOT CHANGE THESE HELPERS WITHOUT DOING A FULL ANALYSIS OF ALL DEPENDANT TESTS.
	public class WhsTestHelperFunctionsUS : WhsTestHelperFunctions
	{
		public WhsTestHelperFunctionsUS(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Warehouse

		public override WhsWarehouse CreateWarehouse(ZString name, bool shouldPrePopulateDDL = true)
		{
			return HelperEnv.CreateWarehouse(name, shouldPrePopulateDDL);
		}

		#endregion

		#region TSA Status

		public void SetOrgAddressTSAStatus(OrgAddress address, ZString tSAStatus)
		{
			HelperEnv.SetOrgAddressTSAStatus(address, tSAStatus);
		}

		public void SetWarehouseTSAStatus(WhsWarehouse warehouse, ZString tSAStatus)
		{
			SetOrgAddressTSAStatus(warehouse.WarehouseAddress, tSAStatus);
		}

		#endregion

		#region Client

		public override OrgHeader CreateClient(string code, string name)
		{
			OrgHeader org = base.CreateClient(code, name);
			HelperEnv.SetupUSOrganization(org);
			return org;
		}

		#endregion

		#region Implementation

		protected WhsTestHelperFunctionsEnvUS HelperEnv
		{
			get
			{
				if (helperEnv == null)
				{
					helperEnv = new WhsTestHelperFunctionsEnvUS(Factory);
				}
				return helperEnv;
			}
		}

		WhsTestHelperFunctionsEnvUS helperEnv;

		#endregion
	}
}
