using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPutawayExistingPalletManager
	{
		IEnumerable<WhsInventoryView> AllocateExistingPalletLocations(BusinessObjectFactory factory, IEnumerable<WhsReceive> receives, IEnumerable<WhsReceiveLine> lines, WhsWarehouse warehouse);
	}
}
