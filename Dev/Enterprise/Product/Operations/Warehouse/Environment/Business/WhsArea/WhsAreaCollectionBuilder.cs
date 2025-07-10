using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsAreaCollectionBuilder : IWhsAreaCollectionBuilder
	{
		public IWhsAreaCollection GetPickingAreas(BusinessObjectFactory factory, IWhsWarehouse master)
		{
			return WhsAreaCollection.GetPickingAreas(factory, master.PK);
		}

		public IWhsAreaCollection GetPutawayAreas(BusinessObjectFactory factory, IWhsWarehouse master)
		{
			return WhsAreaCollection.GetPutawayAreas(factory, master.PK);
		}
	}
}
