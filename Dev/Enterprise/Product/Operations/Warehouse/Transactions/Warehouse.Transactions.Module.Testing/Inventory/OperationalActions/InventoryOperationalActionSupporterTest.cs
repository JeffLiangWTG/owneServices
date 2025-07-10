using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(InventoryOperationalActionSupporter))]
	public class InventoryOperationalActionSupporterTest : WhsOperationalActionSupporterTest<InventoryOperationalActionSupporter>
	{
		protected override ModuleIdentifier ModuleID => ModuleIDs.WhsInventory;
	}
}
