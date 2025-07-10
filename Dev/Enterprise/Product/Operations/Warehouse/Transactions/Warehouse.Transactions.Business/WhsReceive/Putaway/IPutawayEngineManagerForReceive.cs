using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPutawayEngineManagerForReceive
	{
		void Putaway(IEnumerable<WhsReceive> receives, IEnumerable<WhsReceiveLine> linesToPutaway, INotifications notifications, RefEquipment equipment = null, IEnumerable<ZGuid> skipLocationPKs = null, bool useLocationConcurrencyHandling = false, bool needRebuildLocationCache = true);
	}
}
