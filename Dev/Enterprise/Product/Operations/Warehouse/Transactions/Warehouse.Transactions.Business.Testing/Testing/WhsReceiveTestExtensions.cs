using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public static class WhsReceiveTestExtensions
	{
		public static void AllocateLocationsWithMock(this WhsReceive whsReceive)
		{
			using (ObjectFactory.Substitute<IPutawayEngineManagerForReceive>(new PutawayManagerLegacyMockForReceive(whsReceive)))
			{
				ReceiveAllocationHelper.AllocateLocations(whsReceive, whsReceive.Warehouse);
			}
		}

		public static void AllocateLocationsWithMock(this WhsReceive whsReceive, IEnumerable<WhsReceiveLine> selectedInventory, INotifications notifications = null, RefEquipment equipment = null, bool canAllocateLocationOnPutawayTransfers = false)
		{
			using (ObjectFactory.Substitute<IPutawayEngineManagerForReceive>(new PutawayManagerLegacyMockForReceive(whsReceive)))
			{
				ReceiveAllocationHelper.AllocateLocations(new[] { whsReceive }, whsReceive.Warehouse, selectedInventory, notifications, equipment, canAllocateLocationOnPutawayTransfers);
			}
		}
	}
}
