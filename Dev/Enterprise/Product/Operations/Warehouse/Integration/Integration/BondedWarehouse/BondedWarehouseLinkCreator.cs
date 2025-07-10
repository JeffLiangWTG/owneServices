using System;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Integration.BondedWarehouse
{
	public class BondedWarehouseLinkCreator
	{
		public BondedWarehouseLinkCreator()
		{
		}

		public IBondedWarehouseLink GetNewBondedWarehouseLink(BusinessObjectFactory factory)
		{
			return (IBondedWarehouseLink)Activator.CreateInstance(ObjectFactory.GetType<IWhsBondedWarehouseLink>(), new object[] { factory });
		}
	}
}

