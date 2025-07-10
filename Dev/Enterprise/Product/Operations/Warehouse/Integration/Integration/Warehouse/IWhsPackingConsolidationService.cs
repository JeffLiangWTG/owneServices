using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsPackingConsolidationService
	{
		string CreateDockDoorTransfer(ZGuid handlingUnitPackagePK);

		string PutawayStockInDockDoor(ZGuid handlingUnitPackagePK, ZGuid dockDoorLocationPK, ZGuid warehousePK);

		string GenerateHandlingUnit(ZGuid handlingUnitPK, ZGuid warehousePK, ZGuid? loadPK);
	}
}
