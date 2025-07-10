using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class ReceiveAllocationHelper
	{
		public static void AllocateLocations(WhsReceive receive, WhsWarehouse warehouse, bool needRebuildLocationCache = true)
		{
			AllocateLocations(new[] { receive }, warehouse, receive.Lines.Cast<WhsReceiveLine>(), receive.NotificationSubscriber, needRebuildLocationCache: needRebuildLocationCache);
		}

		public static void AllocateLocations(
			IEnumerable<WhsReceive> receives,
			WhsWarehouse warehouse,
			IEnumerable<WhsReceiveLine> selectedInventory,
			INotifications notifications,
			RefEquipment equipment = null,
			bool canAllocateLocationOnPutawayTransfers = false,
			IEnumerable<ZGuid> skipLocationPKs = null,
			bool useLocationConcurrencyHandling = false,
			bool needRebuildLocationCache = true)
		{
			Argument.NotNull(receives, nameof(receives));
			Argument.NotNull(selectedInventory, nameof(selectedInventory));

			if (receives.Any(r => r.WD_WW_Whs != warehouse.PK))
			{
				throw new ArgumentException("Warehouse must be same for all dockets.");
			}

			if (warehouse == null)
			{
				notifications.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseNoWarehouseOrClient));
			}
			else
			{
				var receiveLookup = receives.ToDictionary(r => r.PK);
				var groupedLines = selectedInventory.GroupBy(inv => inv.WE_WD);

				var receivesToPutaway = new List<WhsReceive>();
				var linesToPutaway = new List<WhsReceiveLine>();

				foreach (var docketLines in groupedLines)
				{
					var receive = receiveLookup[docketLines.Key];
					var localNotifications = notifications ?? receive.NotificationSubscriber;

					if (ShouldPerformActionOnSelectedLines(receive, docketLines, localNotifications, canAllocateLocationOnPutawayTransfers))
					{
						if (warehouse.WW_IsVirtualWarehouse)
						{
							AllocateLocationsForVirtualWarehouse(receive, docketLines, warehouse, localNotifications);
						}
						else
						{
							receivesToPutaway.Add(receive);
							linesToPutaway.AddRange(docketLines);
						}
					}
				}

				if (receivesToPutaway.Count > 0)
				{
					var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForReceive>();
					var putawayNotifications = notifications ?? receivesToPutaway[0].NotificationSubscriber;
					putawayManager.Putaway(receivesToPutaway, linesToPutaway, putawayNotifications, equipment, skipLocationPKs, useLocationConcurrencyHandling, needRebuildLocationCache);
				}
			}
		}

		static void AllocateLocationsForVirtualWarehouse(WhsReceive docket, IEnumerable<WhsReceiveLine> lines, WhsWarehouse warehouse, INotifications notifications)
		{
			var location = GetDefaultLocation(docket, warehouse);
			if (location != null)
			{
				var locationPk = location.PK;
				foreach (var line in lines.Where(l => l.WE_WL.IsEmpty))
				{
					line.WE_WL = locationPk;
				}
			}
			else
			{
				notifications.Notify(new ErrorNotification(ReceiveErrorTypes.NoLocationsDefined));
			}
		}

		static WhsLocation GetDefaultLocation(WhsReceive docket, WhsWarehouse warehouse)
		{
			WhsLocation location;

			if (docket.IsCustomsTransaction)
			{
				location = docket.WD_IsInwardsProcessingJob ? warehouse.DefaultLocationInInwardProcessingArea : warehouse.DefaultLocationInBondedArea;
			}
			else
			{
				location = warehouse.DefaultLocationInNonBondedArea;
			}

			return location;
		}

		static bool ShouldPerformActionOnSelectedLines(WhsReceive receive, IEnumerable<WhsReceiveLine> selectedInventory, INotifications notifications, bool canAllocateLocationOnPutawayTransfers)
		{
			var result = false;

			receive.PerformActionOnSelectedLines(
				lines =>
				{
					if (receive.Client == null)
					{
						notifications.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseNoWarehouseOrClient));
					}
					else
					{
						result = true;
					}
				},
				selectedInventory, notifications, canAllocateLocationOnPutawayTransfers);

			return result;
		}
	}
}
