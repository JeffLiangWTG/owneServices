using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class CYDYardCollection(BusinessObjectFactory factory)
	{
		public BusinessObjectCollection AllYards => allYards ??= GetAllYards();

		BusinessObjectCollection allYards;

		BusinessObjectCollection GetAllYards()
		{
			var whsWarehouseCollectionType = ObjectFactory.GetType<IWhsWarehouseCollection>();
			var result = (BusinessObjectCollection)Activator.CreateInstance(whsWarehouseCollectionType, factory);
			((IWhsWarehouseCollection)result).WarehouseCollectionType = WarehouseCollectionType.CYDWarehouse;
			result.Load();
			return result;
		}
	}
}
