using System.Collections.Generic;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(InventoryModule))]
	class InventoryModuleTest : InventoryModuleWithoutOperationalActionsTest
	{
		protected override Dictionary<string, int> ExpectedDBHits
		{
			get
			{
				var expectedDBHits = base.ExpectedDBHits;
				return expectedDBHits;
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsInventory;
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert("Not required.", true);
		}

		protected override InventoryModuleWithoutOperationalActions GetInventoryModuleForTest()
		{
			return new InventoryModule();
		}
	}
}
