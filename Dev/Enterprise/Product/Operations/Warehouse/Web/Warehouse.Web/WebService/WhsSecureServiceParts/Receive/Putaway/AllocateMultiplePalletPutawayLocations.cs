using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Business.Putaway;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Allocate Locations for Multiple Pallet Putaway")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PutawayMultiplePalletsWebServiceResponse AllocateMultiplePalletPutawayLocations(string[] palletIDs, string equipmentRegistration)
		{
			return HandleWebServiceRequest<PutawayMultiplePalletsWebServiceResponse>(r => AllocateMultiplePalletsLocations(r, palletIDs, equipmentRegistration));
		}

		[WebMethod(Description = "Reallocate Locations for Multiple Pallet Putaway")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PutawayMultiplePalletsWebServiceResponse ReallocateMultiplePalletPutawayLocations(string[] palletIDs, PutawayPalletInfo[] allocatedPallets, string equipmentRegistration, Guid[] skipLocationPKs)
		{
			return HandleWebServiceRequest<PutawayMultiplePalletsWebServiceResponse>(
				r => AllocateMultiplePalletsLocations(r, palletIDs, equipmentRegistration, skipLocationPKs, allocatedPallets, isReallocate: true));
		}

		void AllocateMultiplePalletsLocations(
			PutawayMultiplePalletsWebServiceResponse serviceResponse,
			string[] palletIDs,
			string equipmentRegistration,
			IEnumerable<Guid> skipLocationPKs = null,
			PutawayPalletInfo[] allocatedPallets = null,
			bool isReallocate = false)
		{
			Action<WebServiceResponse, BusinessObjectFactory, bool> allocateFunction = (response, factory, needRebuildLocationCache) =>
			{
				AllocateMultipleInventoryLocations(response, factory, palletIDs, equipmentRegistration, skipLocationPKs?.Select(x => new ZGuid(x)), allocatedPallets, isReallocate, needRebuildLocationCache);
			};
			PutawayHelper.AllocateInventoryLocationsWithConcurrencyHandling(allocateFunction, serviceResponse, Factory);
		}

		void AllocateMultipleInventoryLocations(
			WebServiceResponse webServiceResponse,
			BusinessObjectFactory factory,
			string[] palletIDs,
			string equipmentRegistration,
			IEnumerable<ZGuid> skipLocationPKs,
			PutawayPalletInfo[] allocatedPallets,
			bool isReallocate,
			bool needRebuildLocationCache)
		{
			var response = webServiceResponse as PutawayMultiplePalletsWebServiceResponse;
			var staff = WebServiceHelper.GetStaff(factory, SecurityHeader.UserName);
			var warehouse = WebServiceHelper.GetWarehouse(factory, SecurityHeader.WarehouseCode);

			if (staff != null && warehouse != null)
			{
				var palletInfos = new List<PutawayPalletInfo>();
				var job = PutawayHelper.LoadUnfinalisedPutawayJob(factory, warehouse, staff);
				if (job != null)
				{
					var transferInvs = WebServiceHelper
						.LoadWhsInventoryByTransferFromPalletIDs(response, factory, SecurityHeader.WarehouseCode, palletIDs, logErrorIfEmpty: true, includeFinalized: true)
						.Where(i => i.WI_InDocketLineType == DocketType.Codes.Transfer);

					var putawayTransferLines =
								transferInvs
									.Select(i => i.InDocketLine)
									.Cast<WhsTransferLine>()
									.Where(t => t.IsPutawayTransferLine);

					if (response.NoError())
					{
						if (PutawayHelper.AreAllPutawayTransfersUnfinalized(response, putawayTransferLines))
						{
							var loadedPalletIDs = putawayTransferLines.Select(i => i.WE_TransferFromPalletId.ToString()).Distinct();
							var palletsWithoutUnfinalisedTransfers = palletIDs.Except(loadedPalletIDs, StringComparer.OrdinalIgnoreCase);

							if (!palletsWithoutUnfinalisedTransfers.Any())
							{
								var receiveLines = putawayTransferLines.Select(l => l.AssociatedReceiveLineOfPutawayTransfer);
								palletInfos = AllocateReceives(response, factory, equipmentRegistration, receiveLines, warehouse, skipLocationPKs, allocatedPallets, isReallocate, needRebuildLocationCache);
							}
							else
							{
								response.LogBusinessValidationError(Res.GetString("d6b6a1aa-ad63-42be-97a6-aadf9cf043f7", "Pallet ID(s) '{0}' are missing an un-finalized putaway transfer.", string.Join(", ", palletsWithoutUnfinalisedTransfers)));
							}
						}
					}
				}
				else
				{
					response.LogBusinessValidationError(Res.GetString("1fe16b70-5b8c-40ee-96c7-311bac2bb941", "Putaway Job cannot be found."));
				}

				if (response.Error == ErrorTypes.None || response.Error == ErrorTypes.WarningOnly)
				{
					response.PalletInfos = palletInfos.ToArray();
				}
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("7f020eee-dca0-4421-9fa8-776f35c91f4f", "Please provide login credentials to use this service."));
			}
		}

		List<PutawayPalletInfo> AllocateReceives(
			PutawayMultiplePalletsWebServiceResponse response,
			BusinessObjectFactory factory,
			string equipmentRegistration,
			IEnumerable<WhsReceiveLine> receiveLines,
			WhsWarehouse warehouse,
			IEnumerable<ZGuid> skipLocationPKs,
			PutawayPalletInfo[] allocatedPallets,
			bool isReallocate,
			bool needRebuildLocationCache)
		{
			var notifications = new NotificationBuffer();
			List<PutawayPalletInfo> palletInfos = null;
			var equipment =
				string.IsNullOrWhiteSpace(equipmentRegistration)
					? null
					: factory.LoadTop1<RefEquipment>(new ZQuery(RefEquipmentSchema.RQ_Registration, equipmentRegistration));

			var sortedReceiveLines = receiveLines.GroupBy(r => r.WE_WD);
			var receivesToPutaway = new Dictionary<ZGuid, WhsReceive>();

			foreach (var whsReceiveLines in sortedReceiveLines)
			{
				var receive = whsReceiveLines.First().Docket;
				if (receive != null)
				{
					receivesToPutaway.Add(receive.PK, receive);
				}
			}

			var locationPKs = skipLocationPKs?.ToArray();
			if (isReallocate)
			{
				PutawayHelper.PreparePutawayTransferLinesForReallocation(receiveLines.ToArray(), factory, locationPKs);
			}

			if (receivesToPutaway.Count > 0)
			{
				ReceiveAllocationHelper.AllocateLocations(
					receivesToPutaway.Values,
					warehouse,
					receiveLines,
					notifications,
					equipment,
					canAllocateLocationOnPutawayTransfers: true,
					locationPKs,
					useLocationConcurrencyHandling: true,
					needRebuildLocationCache: needRebuildLocationCache);
				PutawayHelper.SetReceiveArrivalDate(receivesToPutaway.Values);
			}

			if (!notifications.HasErrors)
			{
				if (notifications.HasWarnings)
				{
					response.Error = ErrorTypes.WarningOnly;
					response.ErrorMessage = GetEventsFromBuffer(notifications);
				}

				palletInfos = PopulatePalletInfos(receivesToPutaway, sortedReceiveLines, allocatedPallets);
			}
			else
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = GetEventsFromBuffer(notifications);
			}

			return palletInfos ?? new List<PutawayPalletInfo>();
		}

		string GetEventsFromBuffer(NotificationBuffer notifications)
		{
			return string.Join(System.Environment.NewLine, notifications.Events.Select(n => n.Message).Distinct());
		}

		List<PutawayPalletInfo> PopulatePalletInfos(
			Dictionary<ZGuid, WhsReceive> receivesByPK,
			IEnumerable<IGrouping<ZGuid, WhsReceiveLine>> groupedWhsReceiveLines,
			PutawayPalletInfo[] allocatedPallets)
		{
			var palletInfoWithLocation = new List<(PutawayPalletInfo PalletInfo, WhsLocation Location)>();
			foreach (var whsReceiveLines in groupedWhsReceiveLines)
			{
				var linesPerPalletID = whsReceiveLines.GroupBy(l => l.WE_PalletID);
				var receive = receivesByPK[whsReceiveLines.Key];

				foreach (var lines in linesPerPalletID)
				{
					var referenceDocketLine = lines.First().PutawayTransferLine;
					var location = referenceDocketLine?.Location;
					var client = receive.Client;

					var palletInfo = new PutawayPalletInfo(
						lines.Key,
						location?.WLV_LocationString ?? string.Empty,
						location?.WLV_LocationString_UserFriendly ?? string.Empty,
						location?.FormattedCheckDigit ?? string.Empty,
						location?.PK.ToGuid() ?? Guid.Empty,
						client.OH_Code,
						receive.PK.ToGuid(),
						location?.WLV_PutawayPathSequence.ToString() ?? string.Empty,
						referenceDocketLine.WE_PalletID);

					palletInfo.ConsolidateProductInfos(
						lines.Select(line => line.ProductCode.ToString()),
						client.PartAttributeManager.PartAttributeName1,
						lines.Select(line => line.WE_PartAttrib1.ToString()),
						client.PartAttributeManager.PartAttributeName2,
						lines.Select(line => line.WE_PartAttrib2.ToString()),
						client.PartAttributeManager.PartAttributeName3,
						lines.Select(line => line.WE_PartAttrib3.ToString()),
						lines.Select(line => line.WE_SerialNumber.ToString()),
						lines.Select(line => line.WE_ExpiryDate.IsValid ? line.WE_ExpiryDate.ToDateTime() : DateTime.MinValue),
						lines.Select(line => line.WE_PackingDate.IsValid ? line.WE_PackingDate.ToDateTime() : DateTime.MinValue));

					palletInfoWithLocation.Add((palletInfo, location));
				}
			}

			if (allocatedPallets != null)
			{
				foreach (var pk in allocatedPallets.Select(x => new ZGuid(x.LocationPK)).Distinct())
				{
					Factory.AddFetchHint(WhsLocationViewSchema.PK, pk);
				}

				foreach (var pallet in allocatedPallets)
				{
					var allocatedLocation = Factory.Load<WhsLocation>(pallet.LocationPK);
					palletInfoWithLocation.Add((pallet, allocatedLocation));
				}
			}

			palletInfoWithLocation.Sort(new SortPutwayPalletInfoWithLocation());
			return palletInfoWithLocation.Select(p => p.PalletInfo).ToList();
		}
	}
}
