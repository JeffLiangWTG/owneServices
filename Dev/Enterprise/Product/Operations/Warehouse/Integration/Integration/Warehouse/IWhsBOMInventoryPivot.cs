using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsBOMInventoryPivot
	{
		ZGuid WIP_WE_ComponentLine { get; set; }
		ZGuid WIP_WE_InventoryLine { get; set; }
		IWhsDocketLine ComponentLine { get; }
		ZDecimal WIP_ComponentQuantity { get; set; }
	}
}
