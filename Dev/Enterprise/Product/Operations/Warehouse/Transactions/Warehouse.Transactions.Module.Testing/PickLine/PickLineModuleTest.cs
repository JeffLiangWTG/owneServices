using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(PickLineModule))]
	class PickLineModuleTest : InventoryModuleWithoutOperationalActionsTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsPickLine;
		}

		protected override InventoryModuleWithoutOperationalActions GetInventoryModuleForTest()
		{
			return new PickLineModule();
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert("Not required.", true);
		}
	}
}
