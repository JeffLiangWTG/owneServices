using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		#region Putaway_AutoAllocateInventory

		[WebMethod(Description = "Auto allocate the location for inventory of a pallet id")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public AutoAllocateInventoryWebServiceResponse Putaway_AutoAllocateInventory(string palletId, string equipmentRegistration)
		{
			return AllocatePalletLocation(palletId, equipmentRegistration);
		}

		[WebMethod(Description = "Reallocate the location for inventory of a pallet id")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public AutoAllocateInventoryWebServiceResponse Putaway_ReallocateInventory(string palletId, string equipmentRegistration, Guid[] skipLocationPKs)
		{
			return AllocatePalletLocation(palletId, equipmentRegistration, skipLocationPKs, isReallocate: true);
		}

		AutoAllocateInventoryWebServiceResponse AllocatePalletLocation(
			string palletId,
			string equipmentRegistration,
			IEnumerable<Guid> skipLocationPKs = null,
			bool isReallocate = false)
		{
			return HandleWebServiceRequest<AutoAllocateInventoryWebServiceResponse>(r =>
			{
				Action<WebServiceResponse, BusinessObjectFactory, bool> allocateFunction = (response, factory, needRebuildLocationCache) =>
				{
					AllocateInventoryLocations(response, factory, palletId, equipmentRegistration, skipLocationPKs?.Select(x => new ZGuid(x)), isReallocate, needRebuildLocationCache);
				};
				PutawayHelper.AllocateInventoryLocationsWithConcurrencyHandling(allocateFunction, r, Factory);
			});
		}

		void AllocateInventoryLocations(WebServiceResponse webServiceResponse, BusinessObjectFactory factory,
			string palletId, string equipmentRegistration, IEnumerable<ZGuid> skipLocationPKs, bool isReallocate, bool needRebuildLocationCache = true)
		{
			var response = webServiceResponse as AutoAllocateInventoryWebServiceResponse;
			var equipment = string.IsNullOrWhiteSpace(equipmentRegistration) ? null : factory.LoadTop1<RefEquipment>(new ZQuery(RefEquipmentSchema.RQ_Registration, equipmentRegistration));
			var inventoryCollection = isReallocate
				? WebServiceHelper.LoadWhsInventoryByTransferFromPalletID(response, factory, SecurityHeader.WarehouseCode, palletId, true, true)
				: WebServiceHelper.LoadWhsInventoryByPalletID(response, factory, SecurityHeader.WarehouseCode, palletId, true);

			if (response.NoError() && inventoryCollection.Any()
				&& PutawayHelper.AreAllPutawayTransfersUnfinalized(response, inventoryCollection, palletId))
			{
				var receives = new HashSet<WhsReceive>();
				foreach (var inventory in inventoryCollection)
				{
					var receive = inventory.Docket as WhsReceive ?? ((WhsTransferLine)inventory.InDocketLine).AssociatedReceiveLineOfPutawayTransfer.Docket;
					receives.Add(receive);
				}

				var inventoriesForPalletId = receives
					.SelectMany(r => r.Lines.Cast<WhsReceiveLine>()
						.Where(i => i.WE_PalletID.Trim().EqualsIgnoringCase(palletId.Trim())))
					.ToArray();

				if (isReallocate)
				{
					PutawayHelper.PreparePutawayTransferLinesForReallocation(inventoriesForPalletId, factory, skipLocationPKs);
				}

				var notifications = new NotificationBuffer();
				ReceiveAllocationHelper.AllocateLocations(
					receives,
					WebServiceHelper.GetWarehouse(factory, SecurityHeader.WarehouseCode),
					inventoriesForPalletId,
					notifications,
					equipment,
					canAllocateLocationOnPutawayTransfers: true,
					skipLocationPKs,
					useLocationConcurrencyHandling: true,
					needRebuildLocationCache: needRebuildLocationCache);
				PutawayHelper.SetReceiveArrivalDate(receives);

				if (!notifications.Events.Any())
				{
					var referenceDocketLine = inventoryCollection.First().InDocketLine;
					var location = referenceDocketLine?.Location;
					if (location != null)
					{
						SetLocationResponseFields(Factory, response, location);
					}
					response.PalletID = referenceDocketLine?.WE_PalletID ?? string.Empty;
				}
				else
				{
					response.LogBusinessValidationError(
						string.Join(System.Environment.NewLine, notifications.Events.Select(n => n.Message).Distinct()));
				}
			}
		}

		#endregion
	}
}
