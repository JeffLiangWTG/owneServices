using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class PickLineModule : InventoryModuleWithoutOperationalActions
	{
		public PickLineModule()
			: base()
		{
		}

		public override ModuleIdentifier ID => ModuleIDs.WhsPickLine;
	}
}
