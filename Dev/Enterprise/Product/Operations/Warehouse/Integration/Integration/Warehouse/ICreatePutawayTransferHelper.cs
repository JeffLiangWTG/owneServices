using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Integration
{
	public interface ICreatePutawayTransferHelper
	{
		public string CreateTransfer(BusinessObjectFactory factory, IWhsWarehouse warehouse, IEnumerable<IWhsInventoryView> inventory, bool isMultiPalletPutaway, string staffCode);
	}
}
