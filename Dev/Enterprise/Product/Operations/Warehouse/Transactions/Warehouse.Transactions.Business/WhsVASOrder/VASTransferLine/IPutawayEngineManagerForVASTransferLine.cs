using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPutawayEngineManagerForVASTransferLine
	{
		void Putaway(IEnumerable<WhsVASOrder> vasOrders, IEnumerable<VASReturnTransferLine> linesToPutaway, INotifications notifications, RefEquipment equipment = null);
	}
}
