using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(BondedWarehouseCollection))]
	sealed class BondedWarehouseCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BondedWarehouseCollection(Factory);
		}

		public void TestNewChildDefaults()
		{
			OrgHeader org1 = BondedWarehouses.AddNew();
			AssertEquals("WarehouseClient is selected", true, org1.OH_IsWarehouseClient);

			Enterprise.Environment.Env.Security.OrgDetailsNewOrgTypeFlagWH.IsAllowed = false;

			OrgHeader org2 = BondedWarehouses.AddNew();
			AssertEquals("WarehouseClient is not selected", false, org2.OH_IsWarehouseClient);
		}

		public void TestValidateEntityOnSaving()
		{
			OrgHeader organisation = BondedWarehouses.AddNew();
			organisation.OH_IsWarehouseClient = false;
			BondedWarehouses.ValidateEntityOnSaving(organisation);
			Assert("OH_IsWarehouseClient has error", organisation.OH_IsWarehouseClientInfo.HasErrors());

			organisation.OH_IsWarehouseClient = true;
			BondedWarehouses.ValidateEntityOnSaving(organisation);
			Assert("OH_IsWarehouseClient does not have error", !organisation.OH_IsWarehouseClientInfo.HasErrors());
		}

		#region Implementation

		BondedWarehouseCollection BondedWarehouses;

		protected override void SetUp()
		{
			base.SetUp();
			BondedWarehouses = new BondedWarehouseCollection(Factory);
		}

		#endregion
	}
}
