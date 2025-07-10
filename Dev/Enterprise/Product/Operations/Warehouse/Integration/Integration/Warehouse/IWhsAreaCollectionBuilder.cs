using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsAreaCollectionBuilder
	{
		IWhsAreaCollection GetPickingAreas(BusinessObjectFactory factory, IWhsWarehouse master);
		IWhsAreaCollection GetPutawayAreas(BusinessObjectFactory factory, IWhsWarehouse master);
	}
}
