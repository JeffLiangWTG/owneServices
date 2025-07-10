using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsVASOrder
	{
		ZGuid PK { get; }
		ZString WVO_CustomerReferenceNo { get; }
		ZGuid WarehousePK { get; }
	}
}
