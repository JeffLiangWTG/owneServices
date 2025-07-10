using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class ComponentLineForAssembly : NonPersistentBusinessObject
	{
		public ComponentLineForAssembly(WhsDocketLine inventoryLine, ZDecimal unitsToPick)
		{
			InventoryLine = Argument.NotNull(inventoryLine, nameof(inventoryLine));
			UnitsToPick = unitsToPick;
		}

		public WhsDocketLine InventoryLine { get; }

		[ResourceStringData("ComponentLineForAssembly|UnitsToPick", Caption = "Units to Pick")]
		public ZDecimal UnitsToPick { get; }
	}
}
