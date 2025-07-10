using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Integration
{
	public interface IFTZWarehouseDataProvider
	{
		bool IsFTZWarehouseDetailedTrackingEnabled(IOrgAddress warehouseAddress);
	}
}
