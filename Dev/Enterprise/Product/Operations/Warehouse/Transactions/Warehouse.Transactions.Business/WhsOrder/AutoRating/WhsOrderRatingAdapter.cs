using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsOrderRatingAdapter : WhsDocketRatingAdapter<WhsOrder>
	{
		public WhsOrderRatingAdapter(WhsOrder order)
			: base(order)
		{
		}

		public override IDocAddress DeliveryAddress
		{
			get { return Parent.LoadJobDocAddressQuickly(Parent.ConsigneeDocAddressRequirement.DefaultDocAddressType); }
		}

		public override FreightMode FreightMode
		{
			get { return Parent.Containers.Any(c => c.WC_IsChargeable) ? FreightMode.Containerised : base.FreightMode; }
		}

		public override AdapterType AdapterType => AdapterType.WarehouseOrder;

		protected override IEnumerable<string> UsedChargeCodeGroups
		{
			get { return new[] { ChargeCodeGroupList.Codes.WHSOutwards }; }
		}

		protected override ZPropertyInfo GetTransportCoOrganisationPropertyInfo() => Parent.GetTransportCoOrganisationPropertyInfo();

		protected override decimal JobWeight
		{
			get { return Parent.WD_WeightSent; }
		}

		protected override decimal JobVolume
		{
			get { return Parent.WD_CubicSent; }
		}

		protected override decimal JobLevelUnits
		{
			get { return Parent.WD_UnitsSent; }
		}

		protected override void AddMeasuresCore(RateableMeasureSet result, ClosureData data)
		{
			CreateBOMKitMeasure(result);
		}

		#region CalculateChargeablePallets

		protected override int CalculateChargeablePalletsCore(ClosureData data)
		{
			int totalPallets;
			var outerPackages = data.OuterPackages;
			if (outerPackages != null && outerPackages.Length > 0)
			{
				totalPallets = outerPackages.Where(p => p.IsPalletPackType).Sum(p => p.Package.KP_PackageQty);
			}
			else
			{
				totalPallets = Parent.WD_PalletsSent;
			}

			return totalPallets;
		}

		#endregion

		#region PackageMeasure

		protected override void LazyPopulatePackageMeasure(RateableMeasureSet measures, ClosureData closureData)
		{
			var packages = closureData.OuterPackages;
			if (packages != null && packages.Length > 0)
			{
				var packagesThatArentPallets = packages.Where(p => !p.IsPalletPackType).ToArray();

				if (packagesThatArentPallets.Length > 0)
				{
					var whsPK = Parent.WD_WW_Whs;
					var reference = GetDocketReference(Parent);

					foreach (var p in packagesThatArentPallets)
					{
						Parent.Factory.AddFetchHint(WhsLoadPkgPackagePivotSchema.Instance, GetPivotFromPkgQuery(p.Package));
					}

					foreach (var packageGroup in packagesThatArentPallets.Select(p => p.Package).GroupBy(p => new { p.KP_F3_NKPackType, IsLoaded = p.HasLoadedPivot() }))
					{
						measures.AddWarehouseDocketPackageCount(packageGroup.Sum(p => p.KP_PackageQty), whsPK, reference, packageGroup.Key.KP_F3_NKPackType, packageGroup.Key.IsLoaded);
					}
				}
			}
			else
			{
				base.LazyPopulatePackageMeasure(measures, closureData);
			}
		}

		static ZQuery GetPivotFromPkgQuery(PkgPackage p)
		{
			var pivotQuery = new ZQuery(WhsLoadPkgPackagePivotSchema.WLP_KP_Package, p.PK);
			pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_UnloadedTime, SQLComparisonOperator.Equal, null);
			return pivotQuery;
		}

		#endregion

		#region WarehousePackageMeasure

		protected override void LazyPopulateWarehousePackageLineMeasure(RateableMeasureSet measures, ClosureData closureData)
		{
			var packages = closureData.OuterPackages;
			if (packages != null && packages.Length > 0)
			{
				foreach (var outerPackage in packages.Select(p => p.Package))
				{
					measures.AddWarehouseOuterPackage
					(
						volume: GetVolumeInM3(outerPackage.KP_Volume, outerPackage.KP_VolumeUQ),
						weight: GetWeightInKG(outerPackage.KP_Weight, outerPackage.KP_WeightUQ),
						packageCount: outerPackage.KP_PackageQty,
						packageType: outerPackage.KP_F3_NKPackType,
						docketReference: GetDocketReference(Parent),
						warehousePK: Parent.WD_WW_Whs.ToGuid()
					);
				}
			}
			else
			{
				base.LazyPopulateWarehousePackageLineMeasure(measures, closureData);
			}
		}

		static decimal GetWeightInKG(decimal weight, string unit)
		{
			var upperCaseUnit = unit.ToUpper();
			return upperCaseUnit != Constants.Weight.Kilograms
				? Constants.Weight.Convert(weight, upperCaseUnit, Constants.Weight.Kilograms)
				: weight;
		}

		static decimal GetVolumeInM3(decimal volume, string unit)
		{
			var upperCaseUnit = unit.ToUpper();
			return upperCaseUnit != Constants.Volume.CubicMetres
				? Constants.Volume.Convert(volume, upperCaseUnit, Constants.Volume.CubicMetres)
				: volume;
		}

		#endregion

		#region SplitMonthBilling

		protected override void CreateWeightVolumeUnitMeasure(RateableMeasureSet rateableMeasures, ClosureData closureData)
		{
			if (closureData.IsSplitMonthBilling)
			{
				CreateDocketLineMeasure(rateableMeasures, closureData, useNormalMeasures: true, useStorageMeasures: false);
				SetSplitMonthBillingLineMeasure(rateableMeasures, closureData);
			}
			else
			{
				base.CreateWeightVolumeUnitMeasure(rateableMeasures, closureData);
			}
		}

		void SetSplitMonthBillingLineMeasure(RateableMeasureSet rateableMeasures, ClosureData closureData)
		{
			Action<RateableMeasureSet> lazyPopulateSplitMonthBillingLines = measures =>
			{
				if (Parent.IsFinalised && !closureData.ShouldChargeStorageInAdvance)
				{
					foreach (var group in closureData.Lines.GroupBy(l => l.ProductPK))
					{
						if (closureData.Products.TryGetValue(group.Key, out var product) && product != null)
						{
							var total = group.Sum(l => l.Quantity);
							var quantity = GetOutwardsStorageQuantityForProduct(closureData.QuantitiesByProduct, product.Parent, total);
							var weight = GetWeightForRating(product, quantity);
							var volume = GetVolumeForRating(product, quantity);
							var units = GetUnitsForRating(product, quantity);
							measures.AddWarehouseDocketStorageLine(weight, volume, units,
								// Warehouse
								Parent.WD_WW_Whs,
								closureData.DocketReference,
								// Product
								group.Key,
								// Commodity
								product.Parent.OP_RH_NKCommodityCode);
						}
					}
				}
			};

			rateableMeasures.CreateWarehouseDocketLines(lazyPopulateSplitMonthBillingLines, useNormalMeasures: false, useStorageMeasures: true,
				RateableMeasureSet.WarehouseProductOptionalAttributes.None);
		}

		#endregion

		#region BomKitMeasure

		void CreateBOMKitMeasure(RateableMeasureSet measures)
		{
			measures.CreateWarehouseBOMKitPartList(LazyPopulateBOMKitMeasure);
		}

		void LazyPopulateBOMKitMeasure(RateableMeasureSet measures)
		{
			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsOrderLine), WhsDocketLineSchema.PK);
			docketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_WD, Parent.PK);

			var bomLinkSubQuery = new ZDBOnlySubQuery(typeof(WhsBOMInventoryPivot), WhsBOMInventoryPivotSchema.WIP_WE_InventoryLine);
			bomLinkSubQuery.AddSubQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, docketLineSubQuery, JoinCondition.And);

			var receiveLineQuery = new ZDBOnlyQuery(typeof(WhsReceiveLine));
			receiveLineQuery.AddSubQuery(WhsDocketLineSchema.PK, bomLinkSubQuery, JoinCondition.And);

			var receiveLineGroup = Parent.Factory.Load<WhsReceiveLine>(receiveLineQuery).GroupBy(l => l.WE_OP);

			foreach (var lineGroup in receiveLineGroup)
			{
				measures.AddWarehouseBOMKitCount(lineGroup.Key.ToGuid(), lineGroup.Sum(l => l.WE_TransactionQuantity));
			}
		}

		#endregion
	}
}
