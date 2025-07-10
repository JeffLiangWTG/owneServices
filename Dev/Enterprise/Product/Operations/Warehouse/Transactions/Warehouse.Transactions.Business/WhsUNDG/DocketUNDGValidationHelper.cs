using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class DocketUNDGValidationHelper
	{
		static readonly string WeightUQ = Constants.Weight.Kilograms;
		static readonly string VolumeUQ = Constants.Volume.CubicMetres;

		public static DocketUNDGValidationCache BuildDocketUNDGValidationCache(WhsReceive receive)
		{
			var factory = receive.Factory;
			var warehouse = receive.Warehouse;

			IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> overLimitUNDGs = null;
			IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> overThresholdUNDGs = null;
			Dictionary<ZGuid, WhsProductUNDGInfo[]> productUNDGs = null;
			Dictionary<ZGuid, ZString> dgCodes = null;
			Dictionary<ZGuid, ZString> dcrCodes = null;

			if (warehouse != null && warehouse.WW_IsDangerousGoodsManagementEnabled)
			{
				var receiveLinesToConsider = receive.Lines.Where(l => l.WE_WL.IsValid && !l.Location.IsDockDoorLocation && l.WE_CurrentInventoryStatus == InventoryStatus.Codes.Putaway).ToArray();

				var productsUNDGInfos = WhsWarehouseUNDGTotalsHelper.LoadProductsUNDGInfo(factory, receiveLinesToConsider.Select(x => x.WE_OP).ToArray(), WeightUQ, VolumeUQ);
				var docketUNDGTotals = GetDocketUNDGTotals(receiveLinesToConsider.ToArray(), productsUNDGInfos);
				var warehouseUNDGTotals = GetWarehouseUNDGTotals(factory, warehouse.PK, receive.PK, warehouse.UNDGLimits.ToArray()).ToArray();

				overLimitUNDGs = FindDocketOverLimitUNDGs(warehouseUNDGTotals, docketUNDGTotals).ToArray();
				overThresholdUNDGs = FindDocketOverLimitUNDGs(warehouseUNDGTotals, docketUNDGTotals, warehouse.WW_DGThresholdPercentage).ToArray();

				productUNDGs = productsUNDGInfos.GroupBy(g => g.ProductPK).ToDictionary(g => g.Key, g => g.ToArray());
				dgCodes = productsUNDGInfos.GroupBy(g => g.DG_PK).ToDictionary(g => g.Key, g => g.Min(x => x.DG_Code));
				dcrCodes = productsUNDGInfos.GroupBy(g => g.DCR_PK).ToDictionary(g => g.Key, g => g.Min(x => x.DCR_Code));
			}

			return new DocketUNDGValidationCache(
				overLimitUNDGs ?? Array.Empty<WhsWarehouseUNDGTotalsInfo>(),
				overThresholdUNDGs ?? Array.Empty<WhsWarehouseUNDGTotalsInfo>(),
				productUNDGs ?? new Dictionary<ZGuid, WhsProductUNDGInfo[]>(),
				dgCodes ?? new Dictionary<ZGuid, ZString>(),
				dcrCodes ?? new Dictionary<ZGuid, ZString>());
		}

		public static WarehouseUNDGValidationResult CheckUNDGTotalsForWarehouse(
			BusinessObjectFactory factory,
			WhsWarehouse warehouse,
			ZGuid docketToExcludePK,
			IReadOnlyCollection<WhsDocketLine> docketLines)
		{
			var validationMessage = string.Empty;
			if (warehouse != null && warehouse.WW_IsDangerousGoodsManagementEnabled)
			{
				var productsUNDGInfos = WhsWarehouseUNDGTotalsHelper.LoadProductsUNDGInfo(factory, docketLines.Select(x => x.WE_OP).ToArray(), WeightUQ, VolumeUQ);
				var docketUNDGTotals = GetDocketUNDGTotals(docketLines.ToArray(), productsUNDGInfos);
				var warehouseUNDGTotals = GetWarehouseUNDGTotals(factory, warehouse.PK, docketToExcludePK, warehouse.UNDGLimits);
				var overLimitUNDGs = FindDocketOverLimitUNDGs(warehouseUNDGTotals, docketUNDGTotals);

				validationMessage = GetValidationMessage(factory, overLimitUNDGs.ToArray());
			}

			return new WarehouseUNDGValidationResult(validationMessage);
		}

		public static WarehouseUNDGValidationResult CheckUNDGTotalsForWarehouse(DocketUNDGValidationCache docketUNDGValidationCache)
		{
			var validationMessage = GetValidationMessage(
				docketUNDGValidationCache.DocketOverLimitUNDGs,
				docketUNDGValidationCache.DGCodes,
				docketUNDGValidationCache.DCRCodes);

			return new WarehouseUNDGValidationResult(validationMessage);
		}

		public static WarehouseUNDGValidationResult CheckUNDGTotalsForProductsInWarehouse(
			DocketUNDGValidationCache docketUNDGValidationCache,
			IReadOnlyCollection<WhsProductUNDGInfo> docketLineProductUNDGInfos)
		{
			var productOverLimitUNDGs = FindProductOverLimitUNDGs(docketUNDGValidationCache.DocketOverLimitUNDGs, docketLineProductUNDGInfos);
			var overLimitMessage = GetValidationMessage(productOverLimitUNDGs, docketUNDGValidationCache.DGCodes, docketUNDGValidationCache.DCRCodes);

			var productOverThresholdUNDGs = FindProductOverLimitUNDGs(docketUNDGValidationCache.DocketOverThresholdUNDGs, docketLineProductUNDGInfos);
			var overThresholdMessage = GetValidationMessage(productOverThresholdUNDGs, docketUNDGValidationCache.DGCodes, docketUNDGValidationCache.DCRCodes);

			return new WarehouseUNDGValidationResult(overLimitMessage, overThresholdMessage);
		}

		static IEnumerable<WhsWarehouseUNDGTotalsInfo> GetWarehouseUNDGTotals(
			BusinessObjectFactory factory,
			ZGuid warehousePK,
			ZGuid docketToExcludePK,
			IEnumerable<WhsUNDGLimit> warehouseUNDGLimits)
		{
			var inventoryUNDGTotals = WhsWarehouseUNDGTotalsHelper.LoadUNDGTotalsForWarehouseInventory(factory, warehousePK, docketToExcludePK);
			var otherUNDGTotals = GetUNDGTotalsWithNoInventory(inventoryUNDGTotals, warehouseUNDGLimits);

			var warehouseUNDGTotals = inventoryUNDGTotals.Union(otherUNDGTotals);
			return warehouseUNDGTotals.Select(x =>
				new WhsWarehouseUNDGTotalsInfo(
					x.UNDGSubStance,
					x.UNDGCountryReference,
					x.UNDGClass,
					Constants.Weight.Convert(x.TotalWeight, x.TotalWeightLimitUQ, WeightUQ),
					Constants.Weight.Convert(x.TotalWeightLimit, x.TotalWeightLimitUQ, WeightUQ),
					WeightUQ,
					Constants.Volume.Convert(x.TotalVolume, x.TotalVolumeLimitUQ, VolumeUQ),
					Constants.Volume.Convert(x.TotalVolumeLimit, x.TotalVolumeLimitUQ, VolumeUQ),
					VolumeUQ
				));
		}

		static IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> GetUNDGTotalsWithNoInventory(
			IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> inventoryUNDGTotals,
			IEnumerable<WhsUNDGLimit> warehouseUNDGLimits)
		{
			var existingLimitsBySubstance = inventoryUNDGTotals.Where(x => x.UNDGSubStance.IsValid).Select(x => x.UNDGSubStance).ToHashSet();
			var existingLimitsByCountryReference = inventoryUNDGTotals.Where(x => x.UNDGCountryReference.IsValid).Select(x => x.UNDGCountryReference).ToHashSet();
			var existingLimitsByClass = inventoryUNDGTotals.Where(x => !x.UNDGClass.IsEmpty).Select(x => x.UNDGClass).ToHashSet();

			return warehouseUNDGLimits
				.Where(IsUNDGExcludedInExistingLimits)
				.Select(x => new WhsWarehouseUNDGTotalsInfo(
					x.WWD_DG.IsValid ? x.WWD_DG : ZGuid.Empty,
					x.WWD_DCR_UNDGCountryReference.IsValid ? x.WWD_DCR_UNDGCountryReference : ZGuid.Empty,
					!x.WWD_UNDGClass.IsEmpty ? x.WWD_UNDGClass : string.Empty,
					0,
					Constants.Weight.ConvertSafe(x.WWD_TotalWeightLimit, x.WWD_TotalWeightLimitUQ, WeightUQ),
					WeightUQ,
					0,
					Constants.Volume.ConvertSafe(x.WWD_TotalVolumeLimit, x.WWD_TotalVolumeLimitUQ, VolumeUQ),
					VolumeUQ
				)).ToArray();

			bool IsUNDGExcludedInExistingLimits(WhsUNDGLimit limit)
				=> (limit.WWD_DG.IsValid && !existingLimitsBySubstance.Contains(limit.WWD_DG))
				   || (limit.WWD_DCR_UNDGCountryReference.IsValid && !existingLimitsByCountryReference.Contains(limit.WWD_DCR_UNDGCountryReference))
				   || (!limit.WWD_UNDGClass.IsEmpty && !existingLimitsByClass.Contains(limit.WWD_UNDGClass));
		}

		static IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> GetDocketUNDGTotals(
			IReadOnlyCollection<WhsDocketLine> docketLines,
			IReadOnlyCollection<WhsProductUNDGInfo> productsUNDGInfos)
		{
			var docketUNDGInfo = GetDocketUNDGInfos(docketLines, productsUNDGInfos);
			return docketUNDGInfo.Where(d => d.DG_PK.IsValid)
				.GroupBy(g => g.DG_PK)
				.Select(g => new WhsWarehouseUNDGTotalsInfo(
					g.Key,
					ZGuid.Empty,
					ZString.Empty,
					g.Sum(result => result.DG_Weight),
					0,
					WeightUQ,
					g.Sum(result => result.DG_Volume),
					0,
					VolumeUQ
				)).Union(docketUNDGInfo.Where(d => d.DCR_PK.IsValid)
					.GroupBy(g => g.DCR_PK)
					.Select(g => new WhsWarehouseUNDGTotalsInfo(
						ZGuid.Empty,
						g.Key,
						string.Empty,
						g.Sum(result => result.DG_Weight),
						0,
						WeightUQ,
						g.Sum(result => result.DG_Volume),
						0,
						VolumeUQ
				))).Union(docketUNDGInfo.Where(d => !d.UNDGClass.IsEmpty)
					.GroupBy(g => g.UNDGClass)
					.Select(g => new WhsWarehouseUNDGTotalsInfo(
						ZGuid.Empty,
						ZGuid.Empty,
						g.Key,
						g.Sum(result => result.DG_Weight),
						0,
						WeightUQ,
						g.Sum(result => result.DG_Volume),
						0,
						VolumeUQ
				))).ToArray();
		}

		static IEnumerable<WhsProductUNDGInfo> GetDocketUNDGInfos(IReadOnlyCollection<WhsDocketLine> docketLines, IReadOnlyCollection<WhsProductUNDGInfo> productUNDGInfos)
		{
			var lineQuantityTotals = docketLines.GroupBy(g => g.WE_OP)
				.ToDictionary(g => g.Key, g => g.Sum(s => s.WE_TransactionQuantity));

			return productUNDGInfos.Select(p => new WhsProductUNDGInfo(
				p.ProductPK,
				p.DG_PK,
				p.DG_Code,
				p.DCR_PK,
				p.DCR_Code,
				p.UNDGClass,
				p.DG_Weight * lineQuantityTotals[p.ProductPK],
				WeightUQ,
				p.DG_Volume * lineQuantityTotals[p.ProductPK],
				VolumeUQ)
			).ToArray();
		}

		static IEnumerable<WhsWarehouseUNDGTotalsInfo> FindDocketOverLimitUNDGs(
			IEnumerable<WhsWarehouseUNDGTotalsInfo> warehouseUNDGTotals,
			IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> docketUNDGTotals,
			int thresholdPercentageToCheck = 100)
		{
			var substanceTotals = docketUNDGTotals.Where(d => d.UNDGSubStance.IsValid).ToDictionary(d => d.UNDGSubStance);
			var countryTotals = docketUNDGTotals.Where(d => d.UNDGCountryReference.IsValid).ToDictionary(d => d.UNDGCountryReference);
			var classTotals = docketUNDGTotals.Where(d => !d.UNDGClass.IsEmpty).ToDictionary(d => d.UNDGClass);

			return warehouseUNDGTotals.Where(IsOverLimitUNDG);

			bool IsOverLimitUNDG(WhsWarehouseUNDGTotalsInfo warehouseUNDG)
			{
				WhsWarehouseUNDGTotalsInfo docketUNDG;
				if (warehouseUNDG.UNDGSubStance.IsValid)
				{
					substanceTotals.TryGetValue(warehouseUNDG.UNDGSubStance, out docketUNDG);
				}
				else if (warehouseUNDG.UNDGCountryReference.IsValid)
				{
					countryTotals.TryGetValue(warehouseUNDG.UNDGCountryReference, out docketUNDG);
				}
				else
				{
					classTotals.TryGetValue(warehouseUNDG.UNDGClass, out docketUNDG);
				}

				return docketUNDG != null
					&& (warehouseUNDG.TotalWeight + docketUNDG.TotalWeight > warehouseUNDG.TotalWeightLimit * thresholdPercentageToCheck / 100
						|| warehouseUNDG.TotalVolume + docketUNDG.TotalVolume > warehouseUNDG.TotalVolumeLimit * thresholdPercentageToCheck / 100);
			}
		}

		static IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> FindProductOverLimitUNDGs(
			IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> overLimitUNDGs,
			IReadOnlyCollection<WhsProductUNDGInfo> productUNDGInfos)
		{
			var undgSubstances = productUNDGInfos.Where(x => x.DG_PK.IsValid).Select(x => x.DG_PK).ToHashSet();
			var undgCountryReferences = productUNDGInfos.Where(x => x.DCR_PK.IsValid).Select(x => x.DCR_PK).ToHashSet();
			var undgClasses = productUNDGInfos.Where(x => !x.UNDGClass.IsEmpty).Select(x => x.UNDGClass).ToHashSet();

			return overLimitUNDGs.Where(x => undgSubstances.Contains(x.UNDGSubStance) || undgCountryReferences.Contains(x.UNDGCountryReference) || undgClasses.Contains(x.UNDGClass)).ToArray();
		}

		static ZString GetValidationMessage(BusinessObjectFactory factory, IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> overLimitUNDGs)
		{
			Dictionary<ZGuid, ZString> dgCodes = null;
			Dictionary<ZGuid, ZString> dcrCodes = null;
			var substancePKs = overLimitUNDGs.Where(x => x.UNDGSubStance.IsValid).Select(x => x.UNDGSubStance).Distinct().ToArray();
			if (substancePKs.Length > 0)
			{
				dgCodes = WhsWarehouseUNDGTotalsHelper.LoadUNDGSubstanceCodes(factory, substancePKs);
			}

			var countryReferencePKs = overLimitUNDGs.Where(x => x.UNDGCountryReference.IsValid).Select(x => x.UNDGCountryReference).Distinct().ToArray();
			if (countryReferencePKs.Length > 0)
			{
				dcrCodes = WhsWarehouseUNDGTotalsHelper.LoadUNDGCountryReferenceCodes(factory, countryReferencePKs);
			}

			return GetValidationMessage(overLimitUNDGs, dgCodes ?? new Dictionary<ZGuid, ZString>(), dcrCodes ?? new Dictionary<ZGuid, ZString>());
		}

		static ZString GetValidationMessage(
			IReadOnlyCollection<WhsWarehouseUNDGTotalsInfo> overLimitUNDGs,
			Dictionary<ZGuid, ZString> dgCodes,
			Dictionary<ZGuid, ZString> dcrCodes)
		{
			var stringBuilder = new ZStringBuilder();
			if (overLimitUNDGs != null)
			{
				overLimitUNDGs.Where(x => x.UNDGSubStance.IsValid)
					.Select(x => x.UNDGSubStance)
					.Distinct()
					.Select(s => dgCodes[s])
					.OrderBy(s => s)
					.ForEach(s => stringBuilder.AppendLine(Res.GetString("fe088a48-89c8-45e7-8c8b-bfc4d67a9663", "Substance Code '{0}'", s)));

				overLimitUNDGs.Where(x => x.UNDGCountryReference.IsValid)
					.Select(x => x.UNDGCountryReference)
					.Distinct()
					.Select(s => dcrCodes[s])
					.OrderBy(s => s)
					.ForEach(s => stringBuilder.AppendLine(Res.GetString("a8a1c0f0-f3e2-4076-a83f-580c0b041c00", "Country Reference '{0}'", s)));

				overLimitUNDGs.Where(x => !x.UNDGClass.IsEmpty)
					.Select(x => x.UNDGClass)
					.Distinct()
					.OrderBy(s => s)
					.ForEach(s => stringBuilder.AppendLine(Res.GetString("15fd3bce-7e81-4ff8-b683-489016d90883", "Class Code '{0}'", s)));
			}

			return stringBuilder.ToString();
		}
	}
}
