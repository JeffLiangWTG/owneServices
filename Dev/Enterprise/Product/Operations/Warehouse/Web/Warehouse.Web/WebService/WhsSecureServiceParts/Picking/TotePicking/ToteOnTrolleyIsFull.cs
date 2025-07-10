using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		[WebMethod(Description = "Tote On Trolley Is Full")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse ToteOnTrolleyIsFull(Guid trolleyJobPK, string toteID)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => ToteOnTrolleyIsFullCore(r, trolleyJobPK, toteID));
		}

		void ToteOnTrolleyIsFullCore(WebServiceResponse response, Guid trolleyJobPK, string toteID)
		{
			var trolley = Factory.Load<WhsPickTrolleyJob>(trolleyJobPK);
			var isTrolleyValid = CheckTrolleyJob(response, trolley, PickTrolleyStatus.Codes.Picking, ActionErrorMessage);
			if (isTrolleyValid)
			{
				var tote = trolley.Slots.Select(s => s.Package).SingleOrDefault(p => p != null && p.KP_PackageID == toteID);
				var isToteValid = CheckTote(response, tote, ActionErrorMessage);
				if (isToteValid)
				{
					var (isToteHasContents, componentPickLines, kitQtyCache) = CheckToteHasContents(response, tote, ActionErrorMessage);
					if (isToteHasContents)
					{
						foreach (var itemDivot in tote.PackedItemDivots.OrderBy(di => di.KI_PackedQty).ToArray())
						{
							UnpackNotPickedLines(itemDivot, kitQtyCache, tote);
						}

						UnAssignPickLines(componentPickLines.Where(l => !l.IsPickedFromPutawayLocation));

						Factory.Save();
					}
				}
			}
		}

		static ZString ActionErrorMessage => Res.GetString("73A41312-A3DB-4E37-B212-C5AD27D4A758", "Cannot set Tote as Full.");

		bool CheckTrolleyJob(WebServiceResponse response, WhsPickTrolleyJob trolleyJob, ZString validStatus, ZString actionErrorMessage)
		{
			if (trolleyJob == null)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("CheckTrolleyJob|NotFound", "Trolley job was not found. {0}", actionErrorMessage);
			}
			else if (trolleyJob.WTJ_Status != validStatus)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("CheckTrolleyJob|WrongState", "This Trolley job is in {0} state. {1}", trolleyJob.WTJ_Status, actionErrorMessage);
			}

			return string.IsNullOrEmpty(response.ErrorMessage);
		}

		bool CheckTote(WebServiceResponse response, PkgPackage tote, ZString actionErrorMessage)
		{
			if (tote == null)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("CheckTote|NotFound", "Tote was not found. {0}", actionErrorMessage);
			}
			else if (!tote.GetIsTote())
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("CheckTote|WrongPackType", "Package has a Pack Type of '{0}'. {1}", tote.KP_F3_NKPackType, actionErrorMessage);
			}

			return string.IsNullOrEmpty(response.ErrorMessage);
		}

		(bool HasContents, WhsPickLine[] ComponentPickLines, Dictionary<ZGuid, decimal> KitQtyCache) CheckToteHasContents(WebServiceResponse response, PkgPackage tote, ZString actionErrorMessage)
		{
			var kitOrderLines = Array.Empty<WhsOrderLine>();
			var componentPickLines = Array.Empty<WhsPickLine>();
			var inTransitComponentQtyCache = new Dictionary<ZGuid, decimal>();
			var inTransitKitQtyCache = new Dictionary<ZGuid, decimal>();
			var itemsPacked = tote.PackedItemDivots.Select(d => (WhsPickLine)d.PackedItem).Where(i => i != null).ToArray();

			var hasPickedComponents = false;
			var kitPickLines = itemsPacked.Where(pl => pl.IsPickByBOMKitPickLine()).ToArray();
			if (kitPickLines.Length > 0)
			{
				kitOrderLines = kitPickLines.DistinctBy(pl => pl.WZ_WE_TransactionLine).Select(pl => pl.DocketLine).Cast<WhsOrderLine>().ToArray();
				componentPickLines = kitOrderLines.SelectMany(l => l.ChildComponentLines.SelectMany(cl => cl.PickLines)).ToArray();

				foreach (var componentPickLine in componentPickLines)
				{
					if (componentPickLine.IsPickedFromPutawayLocation && componentPickLine.InventoryLine.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit)
					{
						hasPickedComponents = true;

						var cacheKey = componentPickLine.WZ_WE_TransactionLine;
						if (inTransitComponentQtyCache.TryGetValue(cacheKey, out var inTransitComponentQty))
						{
							inTransitComponentQtyCache[cacheKey] = inTransitComponentQty + componentPickLine.WZ_Units;
						}
						else
						{
							inTransitComponentQtyCache[cacheKey] = componentPickLine.WZ_Units;
						}
					}
				}
			}

			if (!hasPickedComponents && !itemsPacked.Any(pickLine => pickLine.IsPickedFromPutawayLocation))
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("CheckToteHasContents|NotFound", "Tote does not contain any items. {0}", actionErrorMessage);
			}

			if (hasPickedComponents)
			{
				(var isPickedComponentsMatchingKits, inTransitKitQtyCache) = CacheInTransitKitQty(kitOrderLines, inTransitComponentQtyCache);
				if (!isPickedComponentsMatchingKits)
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("CheckToteHasContents|ComponentsMismatchKits", "Tote contains Components that are insufficient to assemble a Kit. {0}", actionErrorMessage);
				}
			}

			return (string.IsNullOrEmpty(response.ErrorMessage), componentPickLines, inTransitKitQtyCache);
		}

		(bool IsPickedComponentsMatchingKits, Dictionary<ZGuid, decimal> KitQtyCache) CacheInTransitKitQty(IEnumerable<WhsOrderLine> kitOrderLines, Dictionary<ZGuid, decimal> inTransitComponentQtyCache)
		{
			var isPickedComponentsMatchingKits = true;
			var kitQtyMap = new Dictionary<ZGuid, decimal>();
			var bomParts = WhsPickByBOMHelper.CacheBOMParts(kitOrderLines);

			foreach (var kitOrderLine in kitOrderLines)
			{
				var kitQtyFromInTransitComponents = kitOrderLine.GetQuantityFromComponents(componentOrderLine =>
				{
					inTransitComponentQtyCache.TryGetValue(componentOrderLine.PK, out var qty);
					return qty;
				});

				if (kitQtyFromInTransitComponents == 0)
				{
					isPickedComponentsMatchingKits = false;
					break;
				}

				foreach (var componentOrderLine in kitOrderLine.ChildComponentLines)
				{
					var bomPart = bomParts[new BOMPartCacheKey(componentOrderLine.WE_OP, componentOrderLine.WE_F3_NKPackType)];
					if (BOMComponentQuantityHelper.GetComponentsQuantityToBuildKits(bomPart, kitQtyFromInTransitComponents) != inTransitComponentQtyCache[componentOrderLine.PK])
					{
						isPickedComponentsMatchingKits = false;
						break;
					}
				}

				kitQtyMap[kitOrderLine.PK] = kitQtyFromInTransitComponents;
			}
			return (isPickedComponentsMatchingKits, kitQtyMap);
		}

		void UnpackNotPickedLines(PkgPackageItemDivot itemDivot, Dictionary<ZGuid, decimal> kitQtyMap, PkgPackage tote)
		{
			var pickLine = (WhsPickLine)itemDivot.PackedItem;
			if (pickLine != null && !pickLine.IsPickedFromPutawayLocation)
			{
				pickLine.WZ_IsPicking = false;
				pickLine.WZ_GS_NKAssignedTo = "";

				var orderLine = (WhsPickableDocketLine)pickLine.DocketLine;
				var releaseLine = orderLine.ReleaseLines.Cast<WhsReleaseLine>().Single(rl => rl.KeyForPacking == itemDivot.PackedItem.Key);

				if (pickLine.IsPickByBOMKitPickLine())
				{
					if (kitQtyMap.TryGetValue(pickLine.WZ_WE_TransactionLine, out var qty) && qty > 0)
					{
						if (pickLine.WZ_Units > qty)
						{
							pickLine.Split(pickLine.WZ_Units - qty);
							itemDivot.DeleteForRepacking(releaseLine);
							tote.Pack(pickLine, releaseLine);
						}
						kitQtyMap[pickLine.WZ_WE_TransactionLine] = qty - pickLine.WZ_Units;
					}
					else
					{
						itemDivot.DeleteForRepacking(releaseLine);
					}
				}
				else
				{
					itemDivot.DeleteForRepacking(releaseLine);
				}
			}
		}
	}
}
