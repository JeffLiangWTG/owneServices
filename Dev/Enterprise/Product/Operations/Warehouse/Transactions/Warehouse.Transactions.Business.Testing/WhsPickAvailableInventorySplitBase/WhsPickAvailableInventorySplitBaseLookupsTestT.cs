using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsPickAvailableInventorySplitBaseLookupsTest<T> : WhsBusinessObjectLookupsTestCase where T : WhsPickAvailableInventorySplitBase
	{
		#region TestAssignedTos

		public virtual void TestAssignedTos()
		{
			AssertType<GlbStaffCollection>(Inventory.Lookups.AssignedTos);
		}

		#endregion

		protected T Inventory => inventory ?? (inventory = CreateInventory());

		protected abstract T CreateInventory();

		T inventory;
	}
}
