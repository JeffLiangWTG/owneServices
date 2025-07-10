using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsReceiveLine : IWhsDocketLine
	{
		ZGuid ConsigneePK { get; }
		IWhsInventoryView Inventory { get; }
		ZDateTimeOffset WE_RequiredByDate { get; }
	}
}
