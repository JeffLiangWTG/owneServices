using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IPutawayLocationCacheUpdater
	{
		void UpdatePutawayLocationCache(BusinessObjectFactory factory, CancellationToken token);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		bool UpdateWarehousePutawayLocationCacheWithProgressForm(BusinessObjectFactory factory, ZGuid warehousePK, INotifications notifications, int maxWaitInMinutes = 30);
	}
}
