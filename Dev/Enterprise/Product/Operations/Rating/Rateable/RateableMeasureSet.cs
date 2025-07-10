using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// The measurable quantities and attributes for a RatingAdapter.
	/// </summary>
	public class RateableMeasureSet : IRateableMeasureSet, IDistinctPartRateDimensions
	{
		public RateableMeasureSet()
			: this(AdapterType.Shipment)
		{
		}

		public RateableMeasureSet(AdapterType adapterType)
		{
			measureTypeParts = new IRateablePartList[MeasureTypeHelper.MeasureTypeMaxValue + 1];
			AdapterType = adapterType;
		}

		// Sparse array of the parts for all the MeasureTypes. Elements are null if not present.
		IRateablePartList[] measureTypeParts;

		public IRateablePartList GetPartList(MeasureType measureType) => measureTypeParts[(int)measureType];

		/// <summary>
		/// Restore a measure that was temporarily changed.
		/// </summary>
		public void RestorePartList(MeasureType measureType, IRateablePartList parts)
		{
			AddPartList(measureType, parts);
		}

		public void AddPartList(MeasureType measureType, IRateablePartList parts)
		{
			measureTypeParts[(int)measureType] = parts;
			ClearCache();
		}

		public AdapterType AdapterType { get; }

		public void SetWeightAndVolumeWithCommodity(ZDecimal weightInKG, ZDecimal volumeInM3, ZString commodity)
		{
			SetWeightWithCommodity(weightInKG, Core.Constants.Weight.Kilograms, commodity);
			SetVolumeWithCommodity(volumeInM3, Core.Constants.Volume.CubicMetres, commodity);
		}

		/// <summary>
		/// Given the parameter, it will update all commodities to be `commodityCode`.
		///
		/// All containers and packs will have their commodityCode changed.
		/// </summary>
		public void UpdateCommodities(string commodityCode)
		{
			var allWeights = GetPartList(MeasureType.Weight);
			var allVolumes = GetPartList(MeasureType.Volume);

			for (int i = 0; i < this.measureTypeParts.Length; i++)
			{
				var partlist = GetPartList((MeasureType)i);
				if (partlist is RateablePartList rateablePartList)
				{
					rateablePartList.HasCommodity = true;
					foreach (var part in rateablePartList.OfType<RateablePart>())
					{
						part.CommodityCode = commodityCode;
					}
				}
				else if (partlist is JobLevelPart jobPart)
				{
					if (jobPart.HasCommodity)
					{
						jobPart.CommodityCode = commodityCode;
					}
				}
			}

			ClearCache();
		}

		public void SetChargeableWithCommodity(ZString commodity, ZString chargeableUnit, decimal actual, decimal forClient, decimal forProvider)
		{
			var part = new JobLevelPart();
			part.CommodityCode = commodity;
			part.ChargeableUnit = chargeableUnit;
			part.ChargeableMeasure = new ClientProviderValues(actual, forClient, forProvider);
			AddPartList(MeasureType.Chargeable, part);
		}

		public ZString GetChargeableCommodity()
		{
			var parts = GetPartList(MeasureType.Chargeable);
			if (parts != null && parts.HasCommodity)
			{
				return parts[0].CommodityCode;
			}

			return ZString.Empty;
		}

		// Generally applies to non-containerized jobs
		public void SetWeightWithCommodity(ZDecimal weightAmount, ZString weightUnit, ZString commodity)
		{
			var part = new JobLevelPart();
			part.CommodityCode = commodity;
			part.Weight = weightAmount;
			part.WeightUnit = weightUnit;
			AddPartList(MeasureType.Weight, part);
		}

		// Generally applies to non-containerized jobs
		public void SetVolumeWithCommodity(ZDecimal volumeAmount, ZString volumeUnit, ZString commodity)
		{
			var part = new JobLevelPart();
			part.CommodityCode = commodity;
			part.Volume = volumeAmount;
			part.VolumeUnit = volumeUnit;
			AddPartList(MeasureType.Volume, part);
		}

		public void CreateWeightAndVolumeListWithCommodityAndPackageType()
		{
			var partList = new RateablePartList();
			weightAndVolumeWithCommodityAndPackageType = partList;
			AddPartList(MeasureType.Weight, partList);
			AddPartList(MeasureType.Volume, partList);
			partList.WeightUnit = Constants.Weight.Kilograms;
			partList.VolumeUnit = Constants.Volume.CubicMetres;
			partList.HasCommodity = true;
			partList.HasPackageType = true;
		}
		RateablePartList weightAndVolumeWithCommodityAndPackageType;

		public bool HasWeightAndVolumeListWithCommodityAndPackageType
			=> weightAndVolumeWithCommodityAndPackageType != null;

		/// <summary>
		/// Add weight and volume of package.
		/// Volume is nullable. If null then only the weight is added.
		/// </summary>
		public void AddWeightAndVolumeWithCommodityAndPackageType(decimal weightInKG, decimal? volumeinM3, ZString commodity, ZString packType)
		{
			if (weightAndVolumeWithCommodityAndPackageType == null)
			{
				CreateWeightAndVolumeListWithCommodityAndPackageType();
			}
			var part = new RateablePart();
			part.Weight = weightInKG;
			part.CommodityCode = commodity;
			part.PackageType = packType;

			if (volumeinM3.HasValue)
			{
				part.Volume = volumeinM3.Value;
			}
			weightAndVolumeWithCommodityAndPackageType.AddPart(part);
		}

		#region Warehouse

		/// <summary>
		/// Create a warehouse package list with weight, volume, unit count (MeasureType.Unit) and package count (MeasureType.Package)
		/// and attributes of warehouse and package type.
		/// </summary>
		public void CreateWarehousePackageList(Action<RateableMeasureSet> lazyPopulate)
		{
			// Does it need one time specification of dimensions and measures?
			var parts = new RateablePartList();
			warehousePackages = parts;
			parts.WeightUnit = Constants.Weight.Kilograms;
			parts.VolumeUnit = Constants.Volume.CubicMetres;
			parts.HasWarehouse = true;
			parts.HasPackageType = true;
			parts.HasCommodity = true;

			lazyPopulateWarehousePackage = lazyPopulate;
			parts.SetLazyPopulate(LazyPopulateWarehousePackageInfo);

			AddPartList(MeasureType.Weight, parts);
			AddPartList(MeasureType.Volume, parts);
			AddPartList(MeasureType.Unit, parts);
			AddPartList(MeasureType.Package, parts);
		}
		RateablePartList warehousePackages;

		public void CreateYardWorkOrders(Action<RateableMeasureSet> lazyPopulate)
		{
			var parts = new RateablePartList();
			yardWorkOrders = parts;
			parts.AreaUnit = Constants.Area.SquareCentimetre;
			parts.LengthUnit = Constants.Length.Centimetres;
			parts.HasWarehouse = true;
			parts.HasMaintainanceAndRepairFields = true;
			parts.HasPackageType = true;

			lazyPopulateWarehousePackage = lazyPopulate;
			parts.SetLazyPopulate(LazyPopulateWarehousePackageInfo);

			AddPartList(MeasureType.Length, parts);
			AddPartList(MeasureType.Area, parts);
			AddPartList(MeasureType.Unit, parts);
		}
		RateablePartList yardWorkOrders;

		Action<RateableMeasureSet> lazyPopulateWarehousePackage;

		void LazyPopulateWarehousePackageInfo(RateablePartList info)
			=> CallOnce(ref lazyPopulateWarehousePackage);

		public void AddWarehousePackage(ZDecimal weightInKG, ZDecimal volumeInM3, ZDecimal packageQty, ZGuid warehousePK, ZString packtype, ZString commodityCode)
		{
			AddWarehousePackageCore(weightInKG, volumeInM3, packageQty, warehousePK, packtype, commodityCode, false);
		}

		public void AddWarehousePackage(ZDecimal weightInKG, ZDecimal volumeInM3, ZDecimal packageQty, ZGuid warehousePK, ZString packtype, ZString commodityCode, bool isPackageLoaded)
		{
			AddWarehousePackageCore(weightInKG, volumeInM3, packageQty, warehousePK, packtype, commodityCode, isPackageLoaded);
		}

		void AddWarehousePackageCore(ZDecimal weightInKG, ZDecimal volumeInM3, ZDecimal packageQty, ZGuid warehousePK, ZString packtype, ZString commodityCode, bool isPackageLoaded)
		{
			var part = new RateablePart();
			warehousePackages.AddPart(part);
			part.Weight = weightInKG;
			part.Volume = volumeInM3;
			part.PackageCount = packageQty;
			part.WarehousePk = ToNullable(warehousePK);
			part.PackageType = packtype;
			part.UnitCount = packageQty;
			part.CommodityCode = commodityCode;
			part.IsPackageLoaded = isPackageLoaded;
		}

		public void AddYardWorkOrder(ZGuid workOrderLinePK, ZDecimal area, ZDecimal perimeter, ZInt quantity, TimeSpan? labourHour, ZGuid warehousePK, ZGuid refContainerComponent, ZString refUnitSection, ZGuid refRepairCode, ZGuid refMaterialCode)
		{
			var part = new RateablePart();
			yardWorkOrders.AddPart(part);
			part.Area = area;
			part.Length = perimeter;
			part.UnitCount = quantity;
			if (labourHour.HasValue)
			{
				part.Time = new TimeInfo(labourHour.Value);
			}
			part.WarehousePk = ToNullable(warehousePK);
			part.RefUnitSection = refUnitSection;
			part.RefContainerComponent = refContainerComponent.ToGuid();
			part.RefMaterialCode = refMaterialCode.ToGuid();
			part.RefRepairCode = refRepairCode.ToGuid();
			part.WorkOrderLinePK = workOrderLinePK.ToGuid();
		}

		/// <summary>
		/// Set attributes for calculators that don't depend on measures.
		/// <see cref="CreateUnidentifiedCountWithWarehouseDocketBasedOnLineCount"/> for more details
		/// </summary>
		public void SetUnidentifiedQuantityForWarehouse(int quantity, ZGuid warehousePK)
		{
			var part = new JobLevelPart();
			part.WarehousePk = ToNullable(warehousePK);
			part.UnidentifiedCount = quantity;
			AddPartList(MeasureType.Unidentified, part);
		}

		/// <summary>
		/// Set attributes for calculators that don't depend on measures.
		/// </summary>
		public void SetUnidentifiedQuantityForWarehouseDocket(int quantity, ZGuid warehousePK, ZString docketReference)
		{
			var part = new JobLevelPart();
			part.WarehousePk = ToNullable(warehousePK);
			part.DocketReference = docketReference;
			part.UnidentifiedCount = quantity;
			AddPartList(MeasureType.Unidentified, part);
		}

		public void SetQuantityWithWarehouseDocket(MeasureType measureType, ZDecimal amount, ZString unit, ZGuid warehousePK, ZString docketReference)
		{
			if (warehouseDocketJob == null)
			{
				warehouseDocketJob = new JobLevelPart();
				warehouseDocketJob.WarehousePk = ToNullable(warehousePK);
				warehouseDocketJob.DocketReference = docketReference;
			}
			else
			{
				if (warehouseDocketJob.WarehousePk != ToNullable(warehousePK))
				{
					throw new ArgumentException("value must be the same for each call", nameof(warehousePK));
				}
				if (warehouseDocketJob.DocketReference != (string)docketReference)
				{
					throw new ArgumentException("value must be the same for each call", nameof(docketReference));
				}
			}
			var part = warehouseDocketJob;
			AddPartList(measureType, part);
			if (measureType == MeasureType.JobWeight)
			{
				part.Weight = amount;
				part.WeightUnit = unit;
			}
			else if (measureType == MeasureType.JobVolume)
			{
				part.Volume = amount;
				part.VolumeUnit = unit;
			}
			else if (measureType == MeasureType.JobUnit)
			{
				part.UnitCount = amount;
			}
			else if (measureType == MeasureType.Shipment)
			{
				part.ShipmentCount = amount;
			}
			else if (measureType == MeasureType.ChargeablePallet)
			{
				part.ChargeablePalletCount = amount;
			}
			else
			{
				throw new ArgumentException("Unsupported value " + measureType.ToString(), nameof(measureType));
			}
		}
		JobLevelPart warehouseDocketJob;

		public void CreateContainerList(Action<RateableMeasureSet> lazyPopulate, bool includeCommodity = true,
			bool includePalletized = false,
			bool includeOwnership = false,
			bool includeNumber = false,
			bool includeWeightVolume = false,
			bool includeCartageLegPK = false,
			bool includeDocketReference = false)
		{
			Argument.NotNull(lazyPopulate, nameof(lazyPopulate));
			lazyPopulateContainers = lazyPopulate;
			containerListForBuilding = BuildContainerList(includeCommodity, includePalletized, includeOwnership, includeNumber, includeCartageLegPK, includeDocketReference);
			containerListForBuilding.SetLazyPopulate(LazyPopulateContainerParts);
			AddPartList(MeasureType.ContainerCount, containerListForBuilding);
		}

		Action<RateableMeasureSet> lazyPopulateContainers;

		void LazyPopulateContainerParts(RateablePartList info)
			=> CallOnce(ref lazyPopulateContainers);

		#region Warehouse product

		[Flags]
		public enum WarehouseProductOptionalAttributes
		{
			None = 0,
			ProductAttributes = 1,
			ChargeGroupToUse = 2,
			PackageType = 4
		}

		/// <summary>
		/// Create product list with weight/volume/unit and attributes warehousePK, productPK, commodity
		/// </summary>
		/// <param name="lazyPopulate"></param>
		public void CreateWarehouseProductList(Action<RateableMeasureSet> lazyPopulate, bool includeProductAttributes)
		{
			warehouseProductNormal = new RateablePartList();
			warehouseProductNormal.HasWarehouse = true;
			warehouseProductNormal.HasProduct = true;
			warehouseProductNormal.HasCommodity = true;
			if (includeProductAttributes)
			{
				warehouseProductNormal.HasProductAttributes = true;
			}

			SetWarehouseProductNormalMeasures(lazyPopulate);
		}

		/// <summary>
		/// Product with warehousePK, productPK, commodityCode
		/// </summary>
		public void AddWarehouseProduct(
			(decimal Amount, string ErrorMessage) weightInKG,
			(decimal Amount, string ErrorMessage) volumeInM3,
			decimal units,
			ZGuid warehousePK, ZGuid productPK, ZString commodityCode)
		{
			var part = new RateablePart();
			part.WarehousePk = ToNullable(warehousePK);
			part.ProductPk = ToNullable(productPK);
			part.CommodityCode = commodityCode;
			AddWarehouseProductNormal(weightInKG, volumeInM3, units, part);
		}

		/// <summary>
		/// Product with warehousePK, productPK, commodityCode, ProductAttributesMeasure
		/// </summary>
		public void AddWarehouseProduct(
			(decimal Amount, string ErrorMessage) weightInKG,
			(decimal Amount, string ErrorMessage) volumeInM3,
			decimal units,
			ZGuid warehousePK, ZGuid productPK, ProductAttributesMeasure productAttributes, ZString commodityCode)
		{
			var part = new RateablePart();
			part.WarehousePk = ToNullable(warehousePK);
			part.ProductPk = ToNullable(productPK);
			part.CommodityCode = commodityCode;
			part.ProductAttributes = productAttributes;
			AddWarehouseProductNormal(weightInKG, volumeInM3, units, part);
		}

		public void CreateWarehouseDocketLines(
			Action<RateableMeasureSet> lazyPopulate,
			bool useNormalMeasures,
			bool useStorageMeasures,
			WarehouseProductOptionalAttributes optionalAttributes)
		{
			if (useNormalMeasures)
			{
				warehouseProductNormal = new RateablePartList();
				SetWarehouseDocketLineAttributes(warehouseProductNormal, optionalAttributes);
				SetWarehouseProductNormalMeasures(lazyPopulate);
			}

			if (useStorageMeasures)
			{
				warehouseProductStorage = new RateablePartList();
				SetWarehouseDocketLineAttributes(warehouseProductStorage, optionalAttributes);
				SetWarehouseProductStorageMeasures(lazyPopulate, useNormalMeasures);
			}
		}

		static void SetWarehouseDocketLineAttributes(RateablePartList parts, WarehouseProductOptionalAttributes optionalAttributes)
		{
			parts.HasWarehouse = true;
			parts.HasProduct = true;
			parts.HasCommodity = true;
			parts.HasDocketReference = true;
			if (optionalAttributes.HasFlag(WarehouseProductOptionalAttributes.ProductAttributes))
			{
				parts.HasProductAttributes = true;
			}
			if (optionalAttributes.HasFlag(WarehouseProductOptionalAttributes.ChargeGroupToUse))
			{
				parts.HasChargeGroupToUse = true;
			}
			if (optionalAttributes.HasFlag(WarehouseProductOptionalAttributes.PackageType))
			{
				parts.HasPackageType = true;
			}

			parts.HasWarehouseLine = true;
		}

		/// <summary>
		/// Filter out (remove) all containers that don't match the given type and commodity.
		/// Creates a new list of containers. The old list is not modified so the caller can first take a copy and restore it later if needed.
		/// </summary>
		public void FilterContainers(Guid? filterContainerTypePk, string filterContainerQuality, string filterCommodityCode)
		{
			var oldList = GetPartList(MeasureType.ContainerCount);
			if (oldList == null)
			{
				return;
			}
			var newList = (RateablePartList)oldList.NewEmptyClone();
			foreach (IRateableContainer container in oldList)
			{
				if (container.ContainerTypePk == filterContainerTypePk &&
					(container.ContainerQuality == filterContainerQuality || filterContainerQuality == null) &&
					container.CommodityCode == filterCommodityCode)
				{
					newList.AddPart(container);
				}
			}
			AddPartList(MeasureType.ContainerCount, newList);
			containerListForBuilding = newList;
		}

		/// <summary>
		/// Filter out (remove) all other container measures that don't match the given type and commodity.
		/// Creates a new list of containers. The old list is not modified so the caller can first take a copy and restore it later if needed.
		/// </summary>
		public void FilterContainerParts(MeasureType measureType, Guid? filterContainerTypePk, ZString filterCommodityCode)
		{
			var oldList = GetPartList(measureType);
			if (oldList == null)
			{
				return;
			}
			var newList = (RateablePartList)oldList.NewEmptyClone();
			foreach (IRateablePart container in oldList)
			{
				if (container.ContainerTypePk == filterContainerTypePk && (string.IsNullOrEmpty(filterCommodityCode) || container.CommodityCode == filterCommodityCode))
				{
					newList.AddPart(container);
				}
			}
			AddPartList(measureType, newList);
		}

		#region Warehouse product normal measures for weight/volume/unit

		void SetWarehouseProductNormalMeasures(Action<RateableMeasureSet> lazyPopulate)
		{
			lazyPopulateWarehouseProductNormal = lazyPopulate;
			warehouseProductNormal.SetLazyPopulate(LazyPopulateWarehouseProductNormalLineInfo);
			warehouseProductNormal.WeightUnit = Constants.Weight.Kilograms;
			warehouseProductNormal.VolumeUnit = Constants.Volume.CubicMetres;
			AddPartList(MeasureType.Weight, warehouseProductNormal);
			AddPartList(MeasureType.Volume, warehouseProductNormal);
			AddPartList(MeasureType.Unit, warehouseProductNormal);
		}
		RateablePartList warehouseProductNormal;
		Action<RateableMeasureSet> lazyPopulateWarehouseProductNormal;

		void LazyPopulateWarehouseProductNormalLineInfo(RateablePartList parts)
			=> CallOnce(ref lazyPopulateWarehouseProductNormal);

		public void AddWarehouseDocketNormalLine(
			(decimal Amount, string ErrorMessage) weightInKG,
			(decimal Amount, string ErrorMessage) volumeInM3,
			decimal units,
			ZGuid warehousePK, ZGuid productPK, ProductAttributesMeasure attributes, ZString commodityCode, ZString docketReference, ZString packType)
		{
			var part = new RateablePart();
			part.WarehousePk = ToNullable(warehousePK);
			part.ProductPk = ToNullable(productPK);
			part.ProductAttributes = attributes;
			part.CommodityCode = commodityCode;
			part.DocketReference = docketReference;
			part.PackageType = packType;

			AddWarehouseProductNormal(weightInKG, volumeInM3, units, part);
		}

		void AddWarehouseProductNormal(
			(decimal Amount, string ErrorMessage) weightInKG,
			(decimal Amount, string ErrorMessage) volumeInM3,
			decimal units,
			RateablePart part)
		{
			part.Weight = weightInKG.Amount;
			part.Volume = volumeInM3.Amount;
			part.UnitCount = units;
			warehouseProductNormal.AddPart(part);
			AddErrorIfNotBlank(MeasureType.Weight, weightInKG.ErrorMessage);
			AddErrorIfNotBlank(MeasureType.Volume, volumeInM3.ErrorMessage);
		}

		#endregion

		#region Warehouse product storage measures for weight/volume/unit

		void SetWarehouseProductStorageMeasures(Action<RateableMeasureSet> lazyPopulate, bool isSameAsNormalMeasures)
		{
			Action<RateablePartList> lazyPopulatePoints;
			if (isSameAsNormalMeasures)
			{
				// Need to use same lazy function to ensure it is called only once
				lazyPopulatePoints = LazyPopulateWarehouseProductNormalLineInfo;
			}
			else
			{
				lazyPopulateWarehouseProductStorage = lazyPopulate;
				lazyPopulatePoints = LazyPopulateWarehouseProductStorageInfo;
			}
			warehouseProductStorage.WeightUnit = Constants.Weight.Kilograms;
			warehouseProductStorage.VolumeUnit = Constants.Volume.CubicMetres;
			warehouseProductStorage.SetLazyPopulate(lazyPopulatePoints);
			AddPartList(MeasureType.StorageWeight, warehouseProductStorage);
			AddPartList(MeasureType.StorageVolume, warehouseProductStorage);
			AddPartList(MeasureType.StorageUnit, warehouseProductStorage);
		}
		RateablePartList warehouseProductStorage;

		Action<RateableMeasureSet> lazyPopulateWarehouseProductStorage;

		void LazyPopulateWarehouseProductStorageInfo(RateablePartList info)
			=> CallOnce(ref lazyPopulateWarehouseProductStorage);

		void CallOnce(ref Action<RateableMeasureSet> lazyPopulate)
		{
			if (lazyPopulate != null)
			{
				var action = lazyPopulate;
				lazyPopulate = null;
				action(this);
			}
		}

		/// <summary>
		/// Add warehouse docket quantities to MeasureType.StorageWeight/Volume/Unit
		/// Attributes of warehouse, product, product key attributes, commodity, docket and pack type.
		/// </summary>
		public void AddWarehouseDocketStorageLine(
			(decimal Amount, string ErrorMessage) weightInKG,
			(decimal Amount, string ErrorMessage) volumeInM3,
			decimal units,
			ZGuid warehousePK, ZGuid productPK, ProductAttributesMeasure attributes, ZString commodityCode, ZString docketReference, ZString packType)
		{
			var part = new RateablePart();
			part.WarehousePk = ToNullable(warehousePK);
			part.ProductPk = ToNullable(productPK);
			part.ProductAttributes = attributes;
			part.CommodityCode = commodityCode;
			part.DocketReference = docketReference;
			part.PackageType = packType;
			AddWarehouseProductStorage(weightInKG, volumeInM3, units, part);
		}

		/// <summary>
		/// Add warehouse docket quantities to MeasureType.StorageWeight/Volume/Unit.
		/// Attributes of warehouse, docket, product, commodity.
		/// </summary>
		public void AddWarehouseDocketStorageLine(
			(decimal Amount, string ErrorMessage) weightInKG,
			(decimal Amount, string ErrorMessage) volumeInM3,
			decimal units,
			ZGuid warehousePK,
			ZString docketReference,
			ZGuid productPK,
			ZString commodityCode)
		{
			var part = new RateablePart();
			part.WarehousePk = ToNullable(warehousePK);
			part.DocketReference = docketReference;
			part.ProductPk = ToNullable(productPK);
			part.CommodityCode = commodityCode;
			AddWarehouseProductStorage(weightInKG, volumeInM3, units, part);
		}

		/// <summary>
		/// Add warehouse docket adjustment quantities to MeasureType.StorageWeight/Volume/Unit
		/// </summary>
		public void AddWarehouseDocketStorageLineAdjustment(
			(decimal Amount, string ErrorMessage) weightInKG,
			(decimal Amount, string ErrorMessage) volumeInM3,
			decimal units,
			ZGuid warehousePK, ZGuid productPK, ZString commodityCode, ZString docketReference, ZString chargeGroupToUse)
		{
			var part = new RateablePart();
			part.WarehousePk = ToNullable(warehousePK);
			part.ProductPk = ToNullable(productPK);
			part.CommodityCode = commodityCode;
			part.DocketReference = docketReference;
			part.ChargeGroupToUse = chargeGroupToUse;
			AddWarehouseProductStorage(weightInKG, volumeInM3, units, part);
		}

		void AddWarehouseProductStorage(
			(decimal Amount, string ErrorMessage) weightInKG,
			(decimal Amount, string ErrorMessage) volumeInM3,
			decimal units,
			RateablePart part)
		{
			part.Weight = weightInKG.Amount;
			part.Volume = volumeInM3.Amount;
			part.UnitCount = units;
			warehouseProductStorage.AddPart(part);
			AddErrorIfNotBlank(MeasureType.StorageWeight, weightInKG.ErrorMessage);
			AddErrorIfNotBlank(MeasureType.StorageVolume, volumeInM3.ErrorMessage);
		}

		#endregion

#if DEBUG
		/// <summary>
		/// Find all parts for the given MeasureType that have a product that matches the given productPK and attributes
		/// and return their total count.
		/// </summary>
		public decimal UnitsByProduct_ForTest(MeasureType measureType, ZGuid productPK, params string[] attributes)
		{
			decimal result = 0;
			var partList = GetPartList(measureType);

			if (partList != null && partList.HasProduct)
			{
				var info = PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType);
				var pkToFind = ToNullable(productPK);
				foreach (var part in partList)
				{
					if (part.ProductPk == pkToFind)
					{
						var productAttributes = part.ProductAttributes;
						if (productAttributes != null)
						{
							if (DifferentAttributes(attributes, productAttributes))
							{
								continue;
							}
						}

						result += info.GetActualValue(part);
					}
				}
			}

			return result;

			bool DifferentAttributes(string[] partAttributes, ProductAttributesMeasure productAttributes)
			{
				var differentAttributes = false;
				for (var attributeIndex = 0; attributeIndex < partAttributes.Length; attributeIndex++)
				{
					differentAttributes = productAttributes.Length < attributeIndex + 1 || productAttributes[attributeIndex] != attributes[attributeIndex];
					if (differentAttributes)
					{
						break;
					}
				}
				return differentAttributes;
			}
		}

		public decimal UnitsByCommodity_ForTest(MeasureType measureType, ZString commodity)
		{
			decimal result = 0;

			var partList = GetPartList(measureType);
			if (partList != null && partList.HasCommodity)
			{
				var info = PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType);
				foreach (var part in partList)
				{
					if (part.CommodityCode == commodity)
					{
						result += info.GetActualValue(part);
					}
				}
			}

			return result;
		}

		public decimal UnitsByLocation_ForTest(MeasureType measureType, ZString locationDescription, int roundingScale = 5)
		{
			decimal result = 0;

			var partList = GetPartList(measureType);

			if (partList != null && partList.HasLocation)
			{
				var info = PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType);
				foreach (var part in partList)
				{
					var location = part.Location;
					if (location != null && location.Description == locationDescription)
					{
						result += info.GetActualValue(part);
					}
				}
			}

			return Utilities.Round(result, roundingScale);
		}

		public decimal UnitsByWarehouseAndProduct_ForTest(MeasureType measureType, ZGuid warehousePK, ZGuid productPK)
		{
			decimal result = 0;

			var partList = GetPartList(measureType);
			if (partList != null && partList.HasWarehouse && partList.HasProduct)
			{
				var info = PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType);
				foreach (var part in partList)
				{
					if (part.WarehousePk == ToNullable(warehousePK) &&
						part.ProductPk == ToNullable(productPK))
					{
						result += info.GetActualValue(part);
					}
				}
			}

			return result;
		}

		public bool ContainsProduct_ForTest(MeasureType measureType, ZGuid productPK)
		{
			var partList = GetPartList(measureType);
			if (partList != null && partList.HasProduct)
			{
				foreach (var part in partList)
				{
					if (part.ProductPk == ToNullable(productPK))
					{
						return true;
					}
				}
			}

			return false;
		}

		public IEnumerable<string> GetUniqueUnitDockets_ForTest()
			=> GetPartList(MeasureType.Unit)?.Select(x => x.DocketReference).Distinct() ?? Enumerable.Empty<string>();
		public IEnumerable<ProductAttributesMeasure> GetUnitProductAttributes_ForTest()
			=> GetPartList(MeasureType.Unit)?.Select(x => x.ProductAttributes).Distinct() ?? Enumerable.Empty<ProductAttributesMeasure>();
#endif

		public bool MeasureHasWarehouse(MeasureType measureType) => GetPartList(measureType)?.HasWarehouse ?? false;
		public bool MeasureHasDocketReference(MeasureType measureType) => GetPartList(measureType)?.HasDocketReference ?? false;
		public bool MeasureHasProduct(MeasureType measureType) => GetPartList(measureType)?.HasProduct ?? false;
		public bool MeasureHasProductAttributes(MeasureType measureType) => GetPartList(measureType)?.HasProductAttributes ?? false;
		public bool MeasureHasCommodity(MeasureType measureType) => GetPartList(measureType)?.HasCommodity ?? false;
		public bool MeasureHasChargeGroupToUse(MeasureType measureType) => GetPartList(measureType)?.HasChargeGroupToUse ?? false;
		public bool MeasureHasLocation(MeasureType measureType) => GetPartList(measureType)?.HasLocation ?? false;

		#endregion

		#region Warehouse Pallet IDs

		public void CreateWarehousePalletIdList(Action<RateableMeasureSet> lazyPopulate, bool includeDocketReference)
		{
			lazyPopulateWarehousePalletIdList = lazyPopulate;
			warehousePalletIdList = new RateablePartList();
			warehousePalletIdList.HasWarehouse = true;
			warehousePalletIdList.HasPalletID = true;
			if (includeDocketReference)
			{
				warehousePalletIdList.HasDocketReference = true;
			}

			warehousePalletIdList.SetLazyPopulate(LazyPopulateWarehousePalletIdInfo);
			AddPartList(MeasureType.PalletID, warehousePalletIdList);
		}
		RateablePartList warehousePalletIdList;
		Action<RateableMeasureSet> lazyPopulateWarehousePalletIdList;

		void LazyPopulateWarehousePalletIdInfo(RateablePartList info)
			=> CallOnce(ref lazyPopulateWarehousePalletIdList);

		public void AddWarehousePalletId(ZGuid warehousePK, ZString palletId)
		{
			var part = new RateablePart();
			part.WarehousePk = ToNullable(warehousePK);
			part.PalletID = palletId;
			warehousePalletIdList.AddPart(part);
		}

		public void AddWarehousePalletId(ZGuid warehousePK, ZString palletId, ZString docketReference)
		{
			var part = new RateablePart();
			part.WarehousePk = ToNullable(warehousePK);
			part.PalletID = palletId;
			part.DocketReference = docketReference;
			warehousePalletIdList.AddPart(part);
		}

#if DEBUG
		public IEnumerable<ZGuid> GetPalletIdWarehouses_ForTest()
			=> warehousePalletIdList?.Select(x => NullableHelper.ToZGuid(x.WarehousePk)) ?? Enumerable.Empty<ZGuid>();
		public IEnumerable<string> GetPalletIds_ForTest()
			=> warehousePalletIdList?.Select(x => x.PalletID) ?? Enumerable.Empty<string>();
		public IEnumerable<string> GetPalletIdDockets_ForTest()
		{
			if (warehousePalletIdList != null && warehousePalletIdList.HasDocketReference)
			{
				return warehousePalletIdList.Select(x => x.DocketReference);
			}
			return Enumerable.Empty<string>();
		}
#endif

		#endregion

		#region JobUnit

		public void CreateJobUnitList(Action<RateableMeasureSet> lazyPopulate)
		{
			lazyPopulateJobUnitList = lazyPopulate;
			jobUnitList = new RateablePartList();
			jobUnitList.SetLazyPopulate(LazyPopulateJobUnitInfo);
			AddPartList(MeasureType.JobUnit, jobUnitList);
		}

		RateablePartList jobUnitList;
		Action<RateableMeasureSet> lazyPopulateJobUnitList;

		void LazyPopulateJobUnitInfo(RateablePartList info)
			=> CallOnce(ref lazyPopulateJobUnitList);

		public void AddJobUnit(decimal quantity)
		{
			var part = new RateablePart();
			part.UnitCount = quantity;
			jobUnitList.AddPart(part);
		}

		#endregion

		#region Location pallet

		public void CreateLocationPalletList(Action<RateableMeasureSet> lazyPopulate)
		{
			lazyPopulateLocationPalletList = lazyPopulate;
			locationPalletList = new RateablePartList();
			locationPalletList.HasWarehouse = true;
			locationPalletList.HasLocation = true;
			locationPalletList.HasProduct = true;
			locationPalletList.HasProductAttributes = true;
			locationPalletList.HasCommodity = true;
			locationPalletList.SetLazyPopulate(LazyPopulateLocationPalletInfo);
			AddPartList(MeasureType.LocationPallet, locationPalletList);
		}

		RateablePartList locationPalletList;
		Action<RateableMeasureSet> lazyPopulateLocationPalletList;

		void LazyPopulateLocationPalletInfo(RateablePartList info)
			=> CallOnce(ref lazyPopulateLocationPalletList);

		public void AddLocationPallet(decimal quantity,
				ZGuid warehousePK,
				LocationMeasure locationMeasure,
				ZGuid productPK,
				ProductAttributesMeasure attributes,
				ZString commodity)
		{
			var part = new RateablePart();
			part.LocationPalletCount = quantity;
			part.WarehousePk = ToNullable(warehousePK);
			part.Location = locationMeasure;
			part.ProductPk = ToNullable(productPK);
			part.ProductAttributes = attributes;
			part.CommodityCode = commodity;
			locationPalletList.AddPart(part);
		}

		#endregion

		#region ChargeablePallet

		class ChargeablePalletPart : BaseRateablePart
		{
			public ChargeablePalletPart(decimal quantity) { ChargeablePalletCount = quantity; }
			public override decimal? ChargeablePalletCount { get; }
		}

		public void CreateChargeablePalletList(Action<RateableMeasureSet> lazyPopulate)
		{
			lazyPopulateChargeablePalletList = lazyPopulate;
			chargeablePalletList = new RateablePartList();
			chargeablePalletList.SetLazyPopulate(LazyPopulateChargeablePalletInfo);
			AddPartList(MeasureType.ChargeablePallet, chargeablePalletList);
		}
		RateablePartList chargeablePalletList;
		Action<RateableMeasureSet> lazyPopulateChargeablePalletList;

		void LazyPopulateChargeablePalletInfo(RateablePartList info)
			=> CallOnce(ref lazyPopulateChargeablePalletList);

		public void AddChargeablePallet(decimal quantity)
		{
			var part = new ChargeablePalletPart(quantity);
			chargeablePalletList.AddPart(part);
		}

		#endregion

		#region Errors

		readonly Dictionary<MeasureType, List<string>> invalidMeasuresWithErrors = new Dictionary<MeasureType, List<string>>();

		void AddErrorIfNotBlank(MeasureType measureType, string errorMessage)
		{
			if (!string.IsNullOrEmpty(errorMessage))
			{
				AddError(measureType, errorMessage);
			}
		}

		public void AddError(MeasureType measureType, string errorMessage)
		{
			var errors = invalidMeasuresWithErrors.GetOrAdd(measureType, () => new List<string>());
			errors.Add(errorMessage);
		}

		/// <summary>
		/// Return errors for the given MeasureType.
		/// </summary>
		public IEnumerable<string> GetMeasureErrors(MeasureType measureType)
		{
			var partList = GetPartList(measureType);
			EnsurePopulated(partList);

			return invalidMeasuresWithErrors.TryGetValue(measureType, out var errors)
				? errors
				: Enumerable.Empty<string>();
		}

		/// <summary>
		/// Return all the errors. Only used by RatingObjectSerializer.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public Dictionary<MeasureType, List<string>> InvalidMeasuresWithErrors
		{
			get
			{
				foreach (var partList in measureTypeParts)
				{
					EnsurePopulated(partList);
				}
				return invalidMeasuresWithErrors;
			}
		}

		static void EnsurePopulated(IRateablePartList partList)
		{
			if (partList != null && partList.HasPendingLazyPopulate)
			{ _ = partList.Count; }
		}

		#endregion

		#region Warehouse docket packages

		/// <summary>
		/// Create a warehouse docket package list with count, and attributes of warehouse, docket and package type
		/// </summary>
		public void CreateWarehouseDocketPackageCountList(Action<RateableMeasureSet> lazyPopulate)
		{
			lazyPopulateWarehouseDocketPackageCountList = lazyPopulate;
			warehouseDocketPackageCountList = new RateablePartList();
			warehouseDocketPackageCountList.HasWarehouse = true;
			warehouseDocketPackageCountList.HasDocketReference = true;
			warehouseDocketPackageCountList.HasPackageType = true;
			warehouseDocketPackageCountList.SetLazyPopulate(LazyPopulateWarehouseDocketPackageCountInfo);
			AddPartList(MeasureType.Package, warehouseDocketPackageCountList);
		}
		RateablePartList warehouseDocketPackageCountList;
		Action<RateableMeasureSet> lazyPopulateWarehouseDocketPackageCountList;

		void LazyPopulateWarehouseDocketPackageCountInfo(RateablePartList info)
			=> CallOnce(ref lazyPopulateWarehouseDocketPackageCountList);

		public void AddWarehouseDocketPackageCount(decimal packageCount, ZGuid warehousePK, ZString docketReference, ZString packType, bool isPackageLoaded)
		{
			var part = new RateablePart();
			part.PackageCount = packageCount;
			part.WarehousePk = ToNullable(warehousePK);
			part.DocketReference = docketReference;
			part.PackageType = packType;
			part.IsPackageLoaded = isPackageLoaded;
			warehouseDocketPackageCountList.AddPart(part);
		}

		#endregion

		/// <summary>
		/// Create empty MeasureType.Line measure. Given Action should call AddLineCountWithWarehouseDocket to populate the measure.
		/// </summary>
		public void CreateLineCountWithWarehouseDocket(Action<RateableMeasureSet> lazyPopulate)
		{
			lazyPopulateWarehouseDocketLineCount = lazyPopulate;
			warehouseDocketLineCount = new RateablePartList();
			warehouseDocketLineCount.HasWarehouse = true;
			warehouseDocketLineCount.HasDocketReference = true;
			warehouseDocketLineCount.SetLazyPopulate(LazyPopulateWarehouseDocketLineCountInfo);
			AddPartList(MeasureType.Line, warehouseDocketLineCount);
		}
		RateablePartList warehouseDocketLineCount;
		Action<RateableMeasureSet> lazyPopulateWarehouseDocketLineCount;

		/// <summary>
		/// Creates MeasureType.Unidentified measure to store dimensions such as the docket reference to pass to calculators that don't otherwise depend on measures.
		/// The dimension must be part of a filter. See IPartFilter and LineMeasureMatcher for details
		/// Must be called after calling CreateLineCountWithWarehouseDocket().
		/// The value will populated by AddLineCountWithWarehouseDocket().
		/// Note, it's not obvious what the line count has to do with the Unidentified/Service count.
		/// </summary>
		public void CreateUnidentifiedCountWithWarehouseDocketBasedOnLineCount()
		{
			AddPartList(MeasureType.Unidentified, warehouseDocketLineCount);
		}

		void LazyPopulateWarehouseDocketLineCountInfo(RateablePartList info)
			=> CallOnce(ref lazyPopulateWarehouseDocketLineCount);

		/// <summary>
		/// Add line count to MeasureType.Line and MeasureType.Unidentified
		/// Must only be called once. Throws InvalidOperationException if called again.
		/// </summary>
		public void AddLineCountWithWarehouseDocket(decimal lineCount, ZGuid warehousePK, ZString docketReference)
		{
			if (warehouseDocketLineCount.Count != 0)
			{
				throw new InvalidOperationException("Must only be called once");
			}
			var part = new WarehouseDocketLinePart(lineCount, warehousePK, docketReference);
			warehouseDocketLineCount.AddPart(part);
		}

		class WarehouseDocketLinePart : BaseRateablePart
		{
			public WarehouseDocketLinePart(decimal lineCount, ZGuid warehousePK, ZString docketReference)
			{
				LineCount = lineCount;
				WarehousePk = ToNullable(warehousePK);
				DocketReference = docketReference;
			}
			public override Guid? WarehousePk { get; }
			public override string DocketReference { get; }
			public override decimal? LineCount { get; }
			public override int? UnidentifiedCount => (int)LineCount;
		}

		public void AddWarehouseOuterPackage(decimal volume, decimal weight, decimal packageCount, string packageType, string docketReference, Guid? warehousePK = default)
		{
			var part = new RateablePart
			{
				Weight = weight,
				Volume = volume,
				PackageCount = packageCount,
				PackageType = packageType,
				WarehousePk = warehousePK,
				DocketReference = docketReference
			};

			warehousePackage.AddPart(part);
		}

		public void CreateWarehousePackages(Action<RateableMeasureSet> lazyPopulate)
		{
			lazyPopulateWarehousePackageLineMeasure = lazyPopulate;
			warehousePackage = new RateablePartList();
			warehousePackage.HasWarehouse = true;
			warehousePackage.SetLazyPopulate(LazyPopulateWarehousePackageLineMeasureInfo);
			warehousePackage.WeightUnit = Constants.Weight.Kilograms;
			warehousePackage.VolumeUnit = Constants.Volume.CubicMetres;
			AddPartList(MeasureType.WarehousePackage, warehousePackage);
			AddPartList(MeasureType.WarehousePackageWeight, warehousePackage);
			AddPartList(MeasureType.WarehousePackageVolume, warehousePackage);
		}
		Action<RateableMeasureSet> lazyPopulateWarehousePackageLineMeasure;

		void LazyPopulateWarehousePackageLineMeasureInfo(RateablePartList info)
			=> CallOnce(ref lazyPopulateWarehousePackageLineMeasure);

		RateablePartList warehousePackage;

		public void AddWarehouseBOMKitCount(Guid productPK, decimal unitCount)
		{
			var part = new RateablePart
			{
				ProductPk = productPK,
				UnitCount = unitCount,
			};
			warehouseBOMKit.AddPart(part);
		}

		public void CreateWarehouseBOMKitPartList(Action<RateableMeasureSet> lazyPopulate)
		{
			lazyPopulateWarehouseBOMKitMeasure = lazyPopulate;
			warehouseBOMKit = new RateablePartList();
			warehouseBOMKit.SetLazyPopulate(LazyPopulateWarehouseBOMKitMeasureInfo);
			AddPartList(MeasureType.BOMKit, warehouseBOMKit);
		}
		Action<RateableMeasureSet> lazyPopulateWarehouseBOMKitMeasure;

		void LazyPopulateWarehouseBOMKitMeasureInfo(RateablePartList info)
			=> CallOnce(ref lazyPopulateWarehouseBOMKitMeasure);

		RateablePartList warehouseBOMKit;

		#endregion

		public IEnumerable<string> GetContainerUniqueCommodities() => GetPartList(MeasureType.ContainerCount)?.GetDistinctCommodities() ?? Enumerable.Empty<string>();
		public IEnumerable<string> GetWeightUniqueCommodities() => GetPartList(MeasureType.Weight)?.GetDistinctCommodities() ?? Enumerable.Empty<string>();
		public IEnumerable<string> GetWeightUniquePackageTypes() => GetPartList(MeasureType.Weight)?.GetDistinctPackageTypes() ?? Enumerable.Empty<string>();
		public IEnumerable<string> GetVolumeUniqueCommodities() => GetPartList(MeasureType.Volume)?.GetDistinctCommodities() ?? Enumerable.Empty<string>();
		public IEnumerable<string> GetVolumeUniquePackageTypes() => GetPartList(MeasureType.Volume)?.GetDistinctPackageTypes() ?? Enumerable.Empty<string>();
		public IEnumerable<string> GetPackageUniqueCommodities() => GetPartList(MeasureType.Package)?.GetDistinctCommodities() ?? Enumerable.Empty<string>();
		public IEnumerable<string> GetPackageUniquePackageTypes() => GetPartList(MeasureType.Package)?.GetDistinctPackageTypes() ?? Enumerable.Empty<string>();

		/// <summary>
		/// Returns all the unique commodity codes across all measure types.
		/// Result will not contain a null string.
		/// It may contain an empty string.
		/// </summary>
		public IReadOnlyCollection<string> GetDistinctCommodities()
			=> GetOrCreateDistinctValues(ref cachedInfo.distinctCommodities, (IRateablePartList parts) => parts.GetDistinctCommodities());

		public IReadOnlyCollection<string> GetDistinctRefUnitSection()
			=> GetOrCreateDistinctValues(ref cachedInfo.distinctRefUnitSections, (IRateablePartList parts) => parts.GetDistinctRefUnitSection());

		public IReadOnlyCollection<Guid?> GetDistinctJobRefContainerComponents()
			=> GetOrCreateDistinctValues(ref cachedInfo.distinctRefContainerComponents, (IRateablePartList parts) => parts.GetDistinctJobRefContainerComponents());

		public IReadOnlyCollection<Guid?> GetDistinctRefContainerMaterials()
			=> GetOrCreateDistinctValues(ref cachedInfo.distinctRefContainerMaterials, (IRateablePartList parts) => parts.GetDistinctRefContainerMaterials());

		public IReadOnlyCollection<Guid?> GetDistinctRefContainerRepairs()
			=> GetOrCreateDistinctValues(ref cachedInfo.distinctRefContainerRepairs, (IRateablePartList parts) => parts.GetDistinctRefContainerRepairs());

		public IReadOnlyCollection<RefContainerInfoParts> GetDistinctRefContainerInfo()
		{
			if (cachedInfo.distinctRefContainerInfo == null)
			{
				cachedInfo.distinctRefContainerInfo = new HashSet<RefContainerInfoParts>();
				foreach (var partList in measureTypeParts)
				{
					if (partList != null)
					{
						var vals = partList.GetDistinctRefContainerInfo();
						foreach (var val in vals)
						{
							cachedInfo.distinctRefContainerInfo.Add(val);
						}
					}
				}
			}

			return cachedInfo.distinctRefContainerInfo;
		}

		/// <summary>
		/// Alternative name for GetDistinctCommodities for compatibility with some external code.
		/// </summary>
		public IEnumerable<string> GetCommodities() => GetDistinctCommodities();

		/// <summary>
		/// Returns all distinct, non-null warehouse PKs
		/// </summary>
		public IReadOnlyCollection<Guid?> GetDistinctWarehousePKs() => GetHashSetWarehousePKs();
		HashSet<Guid?> GetHashSetWarehousePKs()
			=> GetOrCreateDistinctValues(ref cachedInfo.distinctWarehousePKs, parts => parts.GetDistinctWarehousePKs());

		/// <summary>
		/// Returns all distinct product PKs
		/// </summary>
		public IReadOnlyCollection<Guid?> GetDistinctProductPKs() => GetHashSetProductPKs();
		HashSet<Guid?> GetHashSetProductPKs()
			=> GetOrCreateDistinctValues(ref cachedInfo.distinctProductPKs, parts => parts.GetDistinctProductPKs());
		public bool HasProductPk(Guid warehousePK) => GetHashSetProductPKs().Contains(warehousePK);

		/// <summary>
		/// Returns all distinct, non-empty container ownership attributes
		/// </summary>
		public IReadOnlyCollection<string> GetDistinctContainerOwnerships()
			=> GetOrCreateDistinctValues(ref cachedInfo.distinctContainerOwnerships, (IRateablePartList parts) => parts.GetDistinctContainerOwnerships());

		HashSet<T> GetOrCreateDistinctValues<T>(ref HashSet<T> uniqueValues, Func<IRateablePartList, IEnumerable<T>> valueFunc)
		{
			if (uniqueValues == null)
			{
				uniqueValues = new HashSet<T>();
				foreach (var partList in measureTypeParts)
				{
					if (partList != null)
					{
						var vals = valueFunc(partList);
						foreach (var val in vals)
						{
							uniqueValues.Add(val);
						}
					}
				}
			}

			return uniqueValues;
		}

		/// <summary>
		/// Set package measures (weight, volume and package details) for a cartage (local transport) move or leg.
		/// Container information should only be set for containerised jobs.
		/// </summary>
		/// <param name="cartageLegPk">Cartage Leg PK. If not null, will be added to charge attributes under JobChargeAttribTypeList.Codes.CartageLegPK.</param>
		public void SetCartagePackage(
			Guid? cartageLegPk,
			Guid? containerType,
			string containerNumber,
			decimal weight, string weightUnit,
			decimal volume, string volumeUnit,
			decimal packageCount, string packageUnit)
		{
			var part = new JobLevelPart();

			part.CartageLegPK = cartageLegPk;

			if (containerType.HasValue)
			{
				part.HasContainerType = true;
				part.ContainerTypePk = containerType;
			}

			if (!string.IsNullOrEmpty(containerNumber))
			{
				part.HasContainerNumber = true;
				part.ContainerNumber = containerNumber;
			}

			part.Weight = weight;
			part.WeightUnit = weightUnit;

			part.Volume = volume;
			part.VolumeUnit = volumeUnit;

			part.HasPackageType = true;
			part.PackageCount = packageCount;
			part.UnitCount = packageCount;
			part.PackageUnit = packageUnit;
			part.PackageType = packageUnit;

			AddPartList(MeasureType.Weight, part);
			AddPartList(MeasureType.Volume, part);
			AddPartList(MeasureType.Package, part);
			AddPartList(MeasureType.Unit, part);
			AddPartList(MeasureType.Time, part);
		}

		RateablePartList packUnitList;

		// Jobs with no packages still need a part since even zero packages can be rated, at least in tests.
		// For example, a rate per KM has MeasureType.Unit, but uses the DistanceChargeableCalculationStrategy
		// which can get the distance from origin and destination.
		// Possibly this is not realistic in production, since if there are no packages nothing has gone any distance.
		// For now, to keep the tests passing, add a part with zero units.
		RateableContainer CreateZeroPackageContainer(string packType)
		{
			var part = new RateableContainer();
			part.PackageType = packType;
			part.UnitCount = 0;
			part.PackageCount = 0;
			return part;
		}

		static void SetPacklineMeasures(RateablePart part,
			string commodity,
			Guid? containerPK,
			Guid? containerTypePK,
			IClientProviderValues weight,
			IClientProviderValues volume,
			IClientProviderValues loadingMeters,
			decimal? packageCount,
			string packageRef,
			decimal pivotBreak)
		{
			part.WeightMeasure = weight;
			part.VolumeMeasure = volume;
			SetPacklineMeasuresWithoutWeightVolume(part, commodity, containerPK, containerTypePK, loadingMeters, packageCount, packageRef, pivotBreak);
		}

		static void SetPacklineMeasuresWithoutWeightVolume(RateablePart part,
			string commodity,
			Guid? containerPK,
			Guid? containerTypePK,
			IClientProviderValues loadingMeters,
			decimal? packageCount,
			string packageRef,
			decimal pivotBreak)
		{
			part.CommodityCode = commodity;
			part.ContainerPK = containerPK;
			part.ContainerTypePk = containerTypePK;
			part.LoadingMeterMeasure = loadingMeters;
			part.PackageCount = packageCount;
			part.PackageCountReference = packageRef;
			part.PivotBreak = pivotBreak;
		}

#if DEBUG
		public IEnumerable<ZGuid> GetPacklineUniqueContainerTypePKs_ForTest()
			=> GetPartList(MeasureType.Package).GetDistinctContainerTypePKs().Select(x => NullableHelper.ToZGuid(x));
		public IEnumerable<decimal> GetPacklineWeights_ForTest()
			=> GetPartList(MeasureType.Package).Select(x => x.WeightMeasure?.Actual ?? 0);
		public IEnumerable<string> GetPacklineCommodities_ForTest()
			=> GetPartList(MeasureType.Package).Select(x => x.CommodityCode);
		public IEnumerable<ZGuid> GetPacklineContainerTypes_ForTest()
			=> GetPartList(MeasureType.Package).Select(x => NullableHelper.ToZGuid(x.ContainerTypePk));
#endif

		public void SetPackageCountWithCommodity(ZDecimal packageCount, ZString commodity)
		{
			var part = new JobLevelPart();
			part.CommodityCode = commodity;
			part.PackageCount = packageCount;
			AddPartList(MeasureType.Package, part);
		}

		public void SetPackageCountWithEmptyContainerType(ZDecimal packageCount)
		{
			var part = new JobLevelPart();
			// An empty container type seems redundant. Unless it would match a rate with an actual container type, which seems unlikely.
			part.HasContainerType = true;
			part.PackageCount = packageCount;
			AddPartList(MeasureType.Package, part);
		}

		public bool PackageCountHasContainerType
			=> GetPartList(MeasureType.Package)?.HasContainerType ?? false;

		/// <summary>
		/// Return the Package Types and their count for MeasureType.Package.
		/// Result is not null. Will be empty if no such measure.
		/// </summary>
		public Dictionary<string, decimal> BuildPackageTypeAndCountMap(bool loadedPackagesOnly)
		{
			var partList = GetPartList(MeasureType.Package);
			var result = new Dictionary<string, decimal>();
			if (partList != null && partList.HasPackageType)
			{
				foreach (var part in partList)
				{
					var packageType = part.PackageType;
					var packageCount = part.PackageCount ?? 0;

					if (loadedPackagesOnly && !part.IsPackageLoaded)
					{
						packageCount = 0;
					}

					if (!result.ContainsKey(packageType))
					{
						result.Add(packageType, 0m);
					}
					result[packageType] += packageCount;
				}
			}

			return result;
		}

		/// <summary>
		/// Create an empty MeasureType.Unit list of packages, if not already created.
		/// Add to the list using AddPackageUnit().
		/// </summary>
		public void CreatePackageUnitList(bool includeCommodity = false)
		{
			if (packUnitList == null)
			{
				packUnitList = new RateablePartList();
				if (includeCommodity)
				{
					packUnitList.HasCommodity = true;
				}
				packUnitList.HasPackageType = true;
				AddPartList(MeasureType.Unit, packUnitList);
			}
			else
			{
				if (!packUnitList.HasPackageType)
				{
					throw new InvalidOperationException("MeasureType.Unit was not created using CreatePackageUnitList");
				}
				if (includeCommodity && !packUnitList.HasCommodity)
				{
					throw new InvalidOperationException("Attempt to add a PackageUnit with commodity attribute when list was created without option.");
				}
			}
		}

		/// <summary>
		/// Copy MeasureType.Unit to MeasureType.Package
		/// </summary>
		public void CopyUnitMeasureToPackage()
		{
			AddPartList(MeasureType.Package, GetPartList(MeasureType.Unit));
		}

		/// <summary>
		/// Add to MeasureType.Unit a list of packages represented as container infos, all with the same package type.
		/// PackType is a code from RefPackType.F3_Code.
		/// Seems only used in combined calculators where the unit returns true from UnitHelper.IsTopPack (i.e., "CN", "PK", or package unit).
		/// </summary>
		public void AddPackageUnit(MeasureInfo.ContainerInfo package, ZString packType)
		{
			CreatePackageUnitList();
			if (package == null)
			{
				var part = CreateZeroPackageContainer(packType);
				packUnitList.AddPart(part);
			}
			else
			{
				var part = new RateableContainer();
				PopulateFromContainerInfo(part, package);
				part.PackageType = packType;
				packUnitList.AddPart(part);
			}
		}

		public void AddPackageUnitWithCommodity(MeasureInfo.ContainerInfo package, ZString packType, ZString commodity)
		{
			CreatePackageUnitList(includeCommodity: true);
			if (package == null)
			{
				var part = CreateZeroPackageContainer(packType);
				part.CommodityCode = commodity;
				packUnitList.AddPart(part);
			}
			else
			{
				var part = new RateableContainer();
				PopulateFromContainerInfo(part, package);
				part.PackageType = packType;
				part.PackageCount = part.ContainerCount;
				part.CommodityCode = commodity;

				packUnitList.AddPart(part);
			}
		}

		/// <summary>
		/// All distinct package types
		/// </summary>
		public IReadOnlyCollection<string> GetDistinctPackageTypes()
			=> GetOrCreateDistinctValues(ref cachedInfo.distinctPackageTypes, parts => parts.GetDistinctPackageTypes());

		/// <summary>
		/// Remove the current container list, if any.
		/// </summary>
		public void RemoveContainerList()
		{
			RemoveMeasureType(MeasureType.ContainerCount);
			containerListForBuilding = null;
		}

		public void RemoveMeasureType(MeasureType measureType)
		{
			if (GetPartList(measureType) != null)
			{
				AddPartList(measureType, null);
				invalidMeasuresWithErrors.Remove(measureType);
			}
		}

		/// <summary>
		/// Create a container list.
		/// If one is already created, throws an exception if it has different optional attributes, otherwise does nothing.
		/// Containers will always have container type.
		/// Containers with contents will also always have a commodity.
		/// However, for container detention the contents are not relevant and it may be empty, so only container type applies.
		/// Optional other attributes:
		/// - palletized (to match TL_IsOnPallets)
		/// - ownership (to match TL_ContainerOwnership)
		/// - container number (not used for matching, only used for LocalTransport to ensure one charge per container and to populate JobChargeAttribTypeList.Codes.ContainerNumber)
		/// - weight and volume - seems only used for Quick Calculate
		/// </summary>
		public void CreateContainerList(
			bool includeCommodity = true,
			bool includePalletized = false,
			bool includeOwnership = false,
			bool includeContainerNumber = false,
			bool includeCartageLegPK = false,
			bool includeDocketReference = false)
		{
			if (containerListForBuilding == null)
			{
				containerListForBuilding = BuildContainerList(includeCommodity, includePalletized, includeOwnership, includeContainerNumber, includeCartageLegPK, includeDocketReference);
				AddPartList(MeasureType.ContainerCount, containerListForBuilding);
			}
			else
			{
				ValidateContainerDimensions(includeCommodity, includePalletized, includeOwnership, includeContainerNumber, includeCartageLegPK, includeDocketReference);
			}
		}

		// Used while building container measures. Don't use afterwards for calculations since there can be a different list for temporary changes.
		RateablePartList containerListForBuilding;

		RateablePartList BuildContainerList(bool includeCommodity,
			bool includePalletized,
			bool includeOwnership,
			bool includeContainerNumber,
			bool includeCartageLegPK,
			bool includeDocketReference)
		{
			var result = new RateablePartList();
			result.HasContainerType = true;
			if (includeCommodity)
			{
				result.HasCommodity = true;
			}
			if (includePalletized)
			{
				result.HasPalletized = true;
			}
			if (includeOwnership)
			{
				result.HasContainerOwnership = true;
			}
			if (includeContainerNumber)
			{
				result.HasContainerNumber = true;
			}

			if (includeCartageLegPK)
			{
				result.HasCartageLegPK = true;
			}
			if (includeDocketReference)
			{
				result.HasDocketReference = true;
			}

			return result;
		}

		void ValidateContainerDimensions(bool includeCommodity = false,
			bool includePalletized = false,
			bool includeOwnership = false,
			bool includeContainerNumber = false,
			bool includeCartageLegPK = false,
			bool includeDocketReference = false)
		{
			if (includeCommodity && !containerListForBuilding.HasCommodity)
			{
				throw new InvalidOperationException("Attempt to add a container with commodity attribute when container list was created without option.");
			}
			if (includePalletized && !containerListForBuilding.HasPalletized)
			{
				throw new InvalidOperationException("Attempt to add a container with Palletized attribute when container list was created without option.");
			}
			if (includeOwnership && !containerListForBuilding.HasContainerOwnership)
			{
				throw new InvalidOperationException("Attempt to add a container with Ownership attribute when container list was created without option.");
			}
			if (includeContainerNumber && !containerListForBuilding.HasContainerNumber)
			{
				throw new InvalidOperationException("Attempt to add a container with ContainerNumber attribute when container list was created without option.");
			}
			if (includeCartageLegPK && !containerListForBuilding.HasCartageLegPK)
			{
				throw new InvalidOperationException("Attempt to add a container with CartageLegPK attribute when container list was created without option.");
			}
			if (includeDocketReference && !containerListForBuilding.HasDocketReference)
			{
				throw new InvalidOperationException("Attempt to add a container with DocketReference attribute when container list was created without option.");
			}
		}

		public bool ContainerListHasPalletized => GetPartList(MeasureType.ContainerCount)?.HasPalletized ?? false;
		public bool ContainerListHasContainerOwnership => GetPartList(MeasureType.ContainerCount)?.HasContainerOwnership ?? false;
		public bool ContainerListHasContainerNumber => GetPartList(MeasureType.ContainerCount)?.HasContainerNumber ?? false;
		public bool ContainerListHasCommodity => GetPartList(MeasureType.ContainerCount)?.HasCommodity ?? false;

		/// <summary>
		/// Add a container where only the type is relevant (e.g., detention)
		/// </summary>
		public void AddContainer(ZGuid containerTypePK)
		{
			CreateContainerList(includeCommodity: false);

			var containerPart = new RateableContainer();
			containerPart.ContainerTypePk = ToNullable(containerTypePK);
			containerListForBuilding.AddPart(containerPart);
		}

		/// <summary>
		/// Add a set of containers all with the same container type.
		/// For jobs that only care about container count (and possibly TEU count).
		/// Used by OrderRatingAdapter (doesn't set TEU) and LinehaulAndRunSheetRatingAdapter (does set TEU, only has one container).
		/// </summary>
		public void AddContainerGroup(ZGuid containerTypePK, IEnumerable<MeasureInfo.ContainerInfo> containerInfos)
		{
			CreateContainerList(includeCommodity: false);
			foreach (var info in containerInfos)
			{
				var containerPart = new RateableContainer();
				containerPart.ContainerTypePk = ToNullable(containerTypePK);
				containerPart.TEU = info.TEU;
				PopulateFromContainerInfo(containerPart, info);
				containerListForBuilding.AddPart(containerPart);
			}
		}

		/// <summary>
		/// Add a set of containers all with the same container type and commodity.
		/// </summary>
		public void AddContainerGroup(ZGuid containerTypePK, ZString commodity, IEnumerable<MeasureInfo.ContainerInfo> containerInfos)
		{
			CreateContainerList();
			foreach (var info in containerInfos)
			{
				var containerPart = new RateableContainer();
				containerPart.ContainerTypePk = ToNullable(containerTypePK);
				containerPart.CommodityCode = commodity;
				PopulateFromContainerInfo(containerPart, info);
				containerListForBuilding.AddPart(containerPart);
			}
		}

		static void PopulateFromContainerInfo(RateableContainer container, MeasureInfo.ContainerInfo info)
		{
			container.TEU = info.TEU;
			if (info.HasWeightVolume)
			{
				container.ContainerWeightInKG = info.Weight;
				container.ContainerVolumeInM3 = info.Volume;
				container.ContainerCalculatedWeightInKG = info.Weight;
				container.ContainerCalculatedVolumeInM3 = info.Volume;
				container.ContainerGrossWeight = info.GrossWeight;
			}
			container.ContainerSpotRates = info.ContainerSpotRates;
			if (string.IsNullOrEmpty(container.ContainerNumber))
			{
				container.ContainerNumber = info.ContainerNumber;
			}
			container.PivotBreak = info.PivotBreak;
			container.ShipmentShare = info.ShipmentShare;
			container.ContainerPK = ToNullable(info.ContainerPK);
			container.ContainerQuality = info.ContainerQuality;
			container.ContainerPackages = info.ContainerInfoPackages;
			container.RefNumber = info.RefNumber;
			container.ContainerCount = info.ContainerCount;
			container.UnitCount = info.ContainerCount;
			container.ContainerIsNonOperatingReefer = info.IsNonOperatingReefer;
		}

		/// <summary>
		/// Add a container given container type and number. Blank number is allowed.
		/// </summary>
		public void AddContainerWithNumber(ZGuid containerTypePK, ZString containerNumber, MeasureInfo.ContainerInfo containerInfo)
		{
			CreateContainerList(includeCommodity: false, includeContainerNumber: true);
			var containerPart = new RateableContainer();
			containerPart.ContainerTypePk = ToNullable(containerTypePK);
			containerPart.ContainerNumber = containerNumber;
			PopulateFromContainerInfo(containerPart, containerInfo);
			containerListForBuilding.AddPart(containerPart);
		}

		public void AddContainers(IEnumerable<IRateableContainer> containers)
		{
			foreach (var container in containers)
			{
				containerListForBuilding.AddPart(container);
			}
		}

		/// <summary>
		/// Add a container given container type, commodity and number. Blank number is allowed.
		/// </summary>
		public void AddContainerWithCommodityAndNumber(ZGuid containerTypePK, ZString commodity, ZString containerNumber, MeasureInfo.ContainerInfo containerInfo)
		{
			CreateContainerList(includeContainerNumber: true);
			var containerPart = new RateableContainer();
			containerPart.ContainerTypePk = ToNullable(containerTypePK);
			containerPart.CommodityCode = commodity;
			containerPart.ContainerNumber = containerNumber;
			PopulateFromContainerInfo(containerPart, containerInfo);
			containerListForBuilding.AddPart(containerPart);
		}

		/// <summary>
		/// Add a container given container type, number and cartage leg PK. Blank number is allowed.
		/// </summary>
		public void AddContainerWithNumberAndCartageLeg(ZGuid containerTypePK, ZString containerNumber, ZGuid cartageLegPK, MeasureInfo.ContainerInfo containerInfo)
		{
			CreateContainerList(includeCommodity: false, includeContainerNumber: true, includeCartageLegPK: true);
			var containerPart = new RateableContainer();
			containerPart.ContainerTypePk = ToNullable(containerTypePK);
			containerPart.ContainerNumber = containerNumber;
			containerPart.CartageLegPK = ToNullable(cartageLegPK);
			PopulateFromContainerInfo(containerPart, containerInfo);
			containerListForBuilding.AddPart(containerPart);
		}

		/// <summary>
		/// Add a set of containers all with the same container type, commodity, palletized and ownership.
		/// The number of containers is determined by the number of elements in containerInfos.
		/// </summary>
		public void AddContainerGroup(ZGuid containerTypePK, ZString commodity, bool isPalletized, ZString containerOwnership,
			IEnumerable<MeasureInfo.ContainerInfo> containerInfos)
		{
			CreateContainerList(includePalletized: true, includeOwnership: true);
			foreach (var info in containerInfos)
			{
				var containerPart = new RateableContainer();
				containerPart.ContainerTypePk = ToNullable(containerTypePK);
				containerPart.CommodityCode = commodity;
				containerPart.IsOnPallets = isPalletized;
				containerPart.ContainerOwnership = containerOwnership;
				PopulateFromContainerInfo(containerPart, info);
				containerListForBuilding.AddPart(containerPart);
			}
		}

		/// <summary>
		/// Add a set of containers all with the same container type, commodity, ownership, and number. Blank number is allowed.
		///
		/// If container number is provided there will be only one ContainerInfo
		/// If container number is empty, there can more than one ContainerInfo and the weight and volume is for the first.
		/// The weight and volume are the container calculated values, not the gross amounts.
		/// </summary>
		public void AddContainerGroup(ZGuid containerTypePK, string commodity, string containerOwnership, string containerNumber,
			decimal weight, decimal volume,
			IEnumerable<MeasureInfo.ContainerInfo> containerInfos)
		{
			CreateContainerList(includeOwnership: true, includeContainerNumber: true);
			foreach (var info in containerInfos)
			{
				var containerPart = new RateableContainer();
				containerPart.ContainerTypePk = ToNullable(containerTypePK);
				containerPart.CommodityCode = commodity;
				containerPart.ContainerOwnership = containerOwnership;
				containerPart.ContainerNumber = containerNumber;
				containerPart.ContainerCalculatedWeightInKG = weight;
				containerPart.ContainerCalculatedVolumeInM3 = volume;
				PopulateFromContainerInfo(containerPart, info);
				containerListForBuilding.AddPart(containerPart);
			}
		}

		public void AddContainerGroupWithPalletizedAndDocket(ZGuid containerTypePK, bool isPalletized, string docketReference, IEnumerable<MeasureInfo.ContainerInfo> containerInfos)
		{
			ValidateContainerDimensions(includeCommodity: false, includePalletized: true, includeDocketReference: true);
			foreach (var info in containerInfos)
			{
				var containerPart = new RateableContainer();
				containerPart.ContainerTypePk = ToNullable(containerTypePK);
				containerPart.IsOnPallets = isPalletized;
				containerPart.DocketReference = docketReference;
				PopulateFromContainerInfo(containerPart, info);
				containerListForBuilding.AddPart(containerPart);
			}
		}

		/// <summary>
		/// Add an LCL container to the container list.
		/// Assumes palletized and ownership option are not present.
		/// </summary>
		public void AddLCL(ZString commodity, ZDecimal weight, ZString weightUnit, ZDecimal volume, ZString volumeUnit, int packageCount = 0)
		{
			CreateContainerList();
			var containerPart = new RateableContainer();
			containerPart.ContainerTypePk = MeasureInfo.ContainerInfo.LCL.ToGuid();
			containerPart.CommodityCode = commodity;
			containerPart.SetContainerWeight(weight, weightUnit);
			containerPart.SetContainerVolume(volume, volumeUnit);
			// Note, this is the base class PackageCount, which normally corresponds to MeasureType.Package.
			// The old system allowed different counts in MeasureType.Package vs ContainerInfo.Packages.
			// May need another property if different counts are really needed.
			containerPart.PackageCount = packageCount;
			containerPart.ContainerCount = 0;

			containerListForBuilding.AddPart(containerPart);
		}

		/// <summary>
		/// Distinct, non-null container type PKs (using system Guid not ZGuid) from the container list (MeasureType.ContainerType)
		/// </summary>
		/// <returns>not null</returns>
		public IReadOnlyCollection<Guid?> GetContainerListDistinctContainerTypePKs()
			=> GetPartList(MeasureType.ContainerCount)?.GetDistinctContainerTypePKs() ?? emptyGuidCollection;

		readonly IReadOnlyCollection<Guid?> emptyGuidCollection = Array.Empty<Guid?>();

		/// <summary>
		/// Distinct, non-null container type PKs (using system Guid not ZGuid) from all parts (not just the container list).
		/// </summary>
		/// <returns>not null</returns>
		public IReadOnlyCollection<Guid?> GetDistinctContainerTypePKs()
			=> GetHashSetContainerTypePKs();

		public IReadOnlyCollection<string> GetDistinctContainerQualities() => GetAllContainers()
			.Select(x => x?.ContainerQuality)
			.Where(x => x != null)
			.ToHashSet();

		public IReadOnlyCollection<bool> GetDistinctContainerIsNonOperatingReefers()
			=> GetPartList(MeasureType.ContainerCount)?.GetDistinctContainerIsNonOperatingReefers() ?? Array.Empty<bool>();

		/// <summary>
		/// Get container list distinct container type PKs, but using ZGuid for compatibility with a bunch of old code.
		/// </summary>
		public IEnumerable<ZGuid> GetContainerTypePKs() => GetContainerListDistinctContainerTypePKs().Select(x => NullableHelper.ToZGuid(x));

		HashSet<Guid?> GetHashSetContainerTypePKs()
			=> GetOrCreateDistinctValues(ref cachedInfo.distinctContainerTypePKs, (IRateablePartList parts) => parts.GetDistinctContainerTypePKs());

		public IEnumerable<IRateableContainer> GetAllContainers()
		{
			return GetPartList(MeasureType.ContainerCount)?.OfType<IRateableContainer>()
				   ?? Enumerable.Empty<IRateableContainer>();
		}

		public IEnumerable<(ZGuid ContainerTypePk, string ContainerQuality, ZString CommodityCode, int ContainerCount)> GetContainerTypeAndCommodityList()
			=> GetContainerGroups().Select(x => (x.ContainerTypePK, x.ContainerQuality, x.CommodityCode, x.ContainerCount));

		public IEnumerable<ZString> GetContainerQualities() => GetAllContainers()
			.Select(x => new ZString(x?.ContainerQuality))
			.Where(x => !x.IsEmpty)
			.Distinct();

		/// <summary>
		/// Returns all distinct values of the "Palletized" attribute on the container list.
		/// Result is empty if there is no container list, or the containers don't have the palletized attribute defined.
		/// </summary>
		public IReadOnlyCollection<bool> GetDistinctPalletized()
			=> distinctPalletized ?? (distinctPalletized = CreateDistinctPalletized());
		IReadOnlyCollection<bool> distinctPalletized;

		IReadOnlyCollection<bool> CreateDistinctPalletized()
		{
			bool hasPalletized = false;
			bool hasNotPalletized = false;
			var partList = GetPartList(MeasureType.ContainerCount);
			if (partList != null && partList.HasPalletized)
			{
				foreach (var part in partList)
				{
					if (part.IsOnPallets)
					{
						hasPalletized = true;
					}
					else
					{
						hasNotPalletized = true;
					}
				}
			}
			var result = new List<bool>();
			if (hasPalletized)
			{
				result.Add(true);
			}
			if (hasNotPalletized)
			{
				result.Add(false);
			}
			return result;
		}

		/// <summary>
		/// Just for unit tests so far...
		/// </summary>
		public class ContainerGroup
		{
			public ZGuid ContainerTypePK { get; set; }
			public string ContainerQuality { get; set; }
			public ZString CommodityCode { get; set; }
			public bool? IsPalletized { get; set; }
			public string Ownership { get; set; }
			public int ContainerCount { get; set; }
			public string ContainerNumber { get; set; }
			public string DocketReference { get; set; }
			public IEnumerable<IRateableContainer> Containers { get; set; }
		}

		public IEnumerable<ContainerGroup> GetContainerGroups()
		{
			var partList = GetPartList(MeasureType.ContainerCount);
			if (partList != null)
			{
				var containers = partList.Cast<IRateableContainer>();

				var result = new List<ContainerGroup>(containers.Count());

				// Note, IRateableContainer.ContainerNumber can be set even if it is not enabled as a dimension
				// when it comes via ContainerInfo.
				// The old MeasureInfo class could store a ContainerNumber as a dimension and also as ContainerInfo.ContainerNumber.
				// This method only returns ContainerNumber if it is enabled as a dimension, for backwards compatibility.
				foreach (var partGroup in containers.GroupBy(x => new
				{
					ContainerTypePk = x.ContainerTypePk,
					ContainerQuality = x.ContainerQuality,
					CommodityCode = x.CommodityCode,
					IsOnPallets = x.IsOnPallets,
					ContainerOwnership = x.ContainerOwnership ?? string.Empty,
					ContainerNumber = partList.HasContainerNumber ? x.ContainerNumber : null,
					DocketReference = x.DocketReference
				}))
				{
					var key = partGroup.Key;
					result.Add(new ContainerGroup()
					{
						ContainerTypePK = NullableHelper.ToZGuid(key.ContainerTypePk),
						ContainerQuality = key.ContainerQuality,
						CommodityCode = key.CommodityCode,
						IsPalletized = partList.HasPalletized ? key.IsOnPallets : null,
						Ownership = key.ContainerOwnership,
						ContainerCount = partGroup.Sum(g => g.ContainerCount > 0 ? g.ContainerCount : 1),
						ContainerNumber = key.ContainerNumber,
						DocketReference = key.DocketReference,
						Containers = partGroup
					});
				}
				return result;
			}
			else
			{
				return Enumerable.Empty<ContainerGroup>();
			}
		}

		public IEnumerable<(ZGuid ContainerTypePk, string CommodityCode, string ContainerNumber, int ContainerCount, decimal Weight, decimal Volume)> GetContainerListWithCalculatedWeightVolume()
		{
			var partList = GetPartList(MeasureType.ContainerCount);
			if (partList != null)
			{
				var containers = partList.Cast<IRateableContainer>();

				var result = new List<(ZGuid ContainerTypePk, string CommodityCode, string ContainerNumber, int ContainerCount, decimal Weight, decimal Volume)>(partList.Count);
				foreach (var container in containers)
				{
					result.Add((
						ContainerTypePk: NullableHelper.ToZGuid(container.ContainerTypePk),
						CommodityCode: container.CommodityCode,
						ContainerNumber: container.ContainerNumber,
						ContainerCount: container.ContainerCount,
						Weight: container.ContainerCalculatedWeightInKG,
						Volume: container.ContainerCalculatedVolumeInM3
					));
				}
				return result;
			}
			else
			{
				return Enumerable.Empty<(ZGuid ContainerTypePk, string CommodityCode, string ContainerNumber, int ContainerCount, decimal Weight, decimal Volume)>();
			}
		}

		/// <summary>
		/// Return the count of containers with given container type PK.
		/// Returns zero if there is no container list, or the list does not contain the type PK.
		/// </summary>
		public decimal GetContainerCountForContainerType(Guid? containerTypePK)
			=> GetRateableContainersOrNull(containerTypePK)?
				.Sum(c => c.ContainerCount) ?? 0;

		public decimal GetTotalTEUForContainerType(Guid? containerTypePK)
			=> GetRateableContainersOrNull(containerTypePK)?
				.Sum(x => x.TEU) ?? 0;

		/// <summary>
		/// Get all containers in the container list. Returns null if there is no container list.
		/// </summary>
		IEnumerable<IRateableContainer> GetRateableContainersOrNull()
			=> GetPartList(MeasureType.ContainerCount)?
				.Cast<IRateableContainer>();

		/// <summary>
		/// Get all containers in the container list that match the given container type PK.
		/// Returns null if there is no container list.
		/// Returns an empty list if there is a container list, but with no such containers.
		/// </summary>
		IEnumerable<IRateableContainer> GetRateableContainersOrNull(Guid? containerTypePK)
			=> GetRateableContainersOrNull()?
				.Where(x => x.ContainerTypePk == containerTypePK);

		public IEnumerable<ContainerSpotRates> GetContainerSpotRates()
			=> GetPartList(MeasureType.ContainerCount)?
				.Cast<IRateableContainer>()
				.Where(x => x.ContainerSpotRates != null)
				.Select(x => x.ContainerSpotRates)
				.GroupBy(x => x.ContainerPK)
				.Select(g => g.First())
			?? Enumerable.Empty<ContainerSpotRates>();

#if DEBUG
		public List<ContainerSpotRates> GetContainerSpotRates_ForTest()
			=> GetContainerSpotRates().ToList();
#endif

		/// <summary>
		/// Determine if job is containerized. Returns:
		/// - true if at least one container is FCL
		/// - false if all containers are LCL
		/// - null if there is no container list or no containers
		/// </summary>
		public bool? IsContainerized()
		{
			var parts = GetPartList(MeasureType.ContainerCount);
			if (parts == null)
			{
				return null;
			}

			var containerTypes = parts.GetDistinctContainerTypePKs();
			if (!containerTypes.Any() ||
				(!containerTypes.Skip(1).Any() && containerTypes.First() == MeasureInfo.ContainerInfo.LCL))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		/// <summary>
		/// The number of shipments on the job.
		/// Corresponds to MeasureType.Shipment.
		/// Used only for rating when the unit is HB - House Bill
		/// Null means the job is not of a kind that has shipments.
		/// Zero means the house bill is not charged to this job, e.g., it is charged to a co-load master shipment.
		/// </summary>
		public decimal? Shipments
		{
			get => GetActualOrNull(MeasureType.Shipment);
			set
			{
				var partList = value.HasValue
					? new JobLevelPart() { ShipmentCount = value }
					: null;
				AddPartList(MeasureType.Shipment, partList);
			}
		}

		public void SetShipmentsWithCartageLegPK(decimal shipments, ZGuid cartageLegPK)
		{
			var part = new JobLevelPart();
			part.ShipmentCount = shipments;
			part.CartageLegPK = ToNullable(cartageLegPK);
			AddPartList(MeasureType.Shipment, part);
		}

		/// <summary>
		/// Lowest Level Bill count. Some customs jobs are rated on the number of Lowest Level Bills
		/// Lowest Level Bills are Master, House and Sub-house bills that have no children.
		/// </summary>
		public decimal? LowestBill
		{
			get => GetActualOrNull(MeasureType.LowestBill);
			set
			{
				var partList = value.HasValue
					? new JobLevelPart() { LowestBillCount = value }
					: null;
				AddPartList(MeasureType.LowestBill, partList);
			}
		}

		public void SetPickupDistance(decimal distance, string distanceUnit)
		{
			var part = new JobLevelPart();
			part.PickupDistance = distance;
			part.PickupDistanceUnit = distanceUnit;
			AddPartList(MeasureType.PickupDistance, part);
		}
		public Quantity PickupDistance => GetQuantity(MeasureType.PickupDistance);

		public void SetDeliveryDistance(decimal distance, string distanceUnit)
		{
			var part = new JobLevelPart();
			part.DeliveryDistance = distance;
			part.DeliveryDistanceUnit = distanceUnit;
			AddPartList(MeasureType.DeliveryDistance, part);
		}
		public Quantity DeliveryDistance => GetQuantity(MeasureType.DeliveryDistance);

		/// <summary>
		/// Set MeasureTypes by combining the values from all the given RateableMeasureSets.
		/// Weights and Volumes are converted to the unit of the first RateableMeasureSet.
		/// Will throw an exception for other MeasureTypes if the units don't match.
		/// Add more conversions if other types are needed (like PickupDistance).
		///
		/// As of 2021-07, called with MeasureType.Weight, MeasureType.Volume, MeasureType.Chargeable, MeasureType.Package, MeasureType.LoadingMeters from various Consols
		/// Chargeable is not actually supported, why?
		/// All those measures other than MeasureType.Chargeable are the pack lines.
		/// Buyers consol doesn't set MeasureType.LoadingMeters, neither from shipments or directly.
		///
		/// For standard consol, MeasureTypes are Weight, Volume, Package, LoadingMeters.
		/// A standard consol has its own chargeable fields and doesn't need to summarize shipment chargeables.
		/// Package and LoadingMeters do not have units (well LoadingMeters always has an implied unit of meters) and do not get any conversions.
		///
		/// For a buyers consol, MeasureTypes are Weight, Volume, Package, Chargeable.
		/// Why doesn't it summarize LoadingMeters? Good question. It's a trucking measure and maybe customers have never needed it.
		/// Maybe buyers consols are only used for air/sea freight.
		/// </summary>
		public void SetCombinedMeasureFrom(IEnumerable<RateableMeasureSet> measuresToCombine, params MeasureType[] measureTypes)
		{
			if (!measuresToCombine.Any())
			{
				return;
			}

			if (!measureTypes.Contains(MeasureType.Weight) ||
				!measureTypes.Contains(MeasureType.Volume) ||
				!measureTypes.Contains(MeasureType.Package))
			{
				throw new NotImplementedException("this only supports pack lines so far");
			}

			// We use cache because usually adapters share the same Rateable Parts for different measure types. For example,
			// if a shipment has the following pack lines:
			// 10 Boxes, 100 KG, 1 M3, Packed in 20GP Container
			// 20 Pallets, 200 KG, 2 M3, Packed in 40GP Container
			//
			// The adapter will create 2 parts:
			//	Part 1: Count 10, Weight 100 KG, Volume 1 M3, Container 20GP
			//	Part 2: Count 20, Weight 200 KG, Volume 2 M3, Container 40GP
			//
			// And then share between Weight and Volume measure parts:
			// Measures[MeasureType.Weight] = [Part 1, Part 2]
			// Measures[MeasureType.Volume] = [Part 1, Part 2]
			//
			// So, after converting units for a part once (lets say for weight) we don't need to convert it second time (for volume)
			var cache = new Dictionary<IRateablePart, IRateablePart>();

			foreach (var measureType in measureTypes)
			{
				// When summarizing parts we need to make sure they are in the same weight/volume units, so, we create a new
				// part list which will have units from the first part and then other parts will convert units to units of this part
				RateablePartList newPartList = null;

				foreach (var measures in measuresToCombine)
				{
					var partList = measures.GetPartList(measureType);
					if (partList == null || partList.Count == 0)
					{
						continue;
					}

					if (newPartList == null)
					{
						newPartList = (RateablePartList)partList.NewEmptyClone();
						AddPartList(measureType, newPartList);
					}

					var needsUnitsConversion = newPartList.WeightUnit != partList.WeightUnit ||
											   newPartList.VolumeUnit != partList.VolumeUnit;

					foreach (var part in partList)
					{
						if (!needsUnitsConversion)
						{
							// No need to convert as this part is already in desired units
							newPartList.AddPart(part);
							continue;
						}

						if (cache.TryGetValue(part, out var cachedNewPart))
						{
							// We already converted this part instance, so, adding it from the cache
							newPartList.AddPart(cachedNewPart);
							continue;
						}

						var newPart = new RateablePart();

						SetPacklineMeasures(newPart,
							part.CommodityCode,
							part.ContainerPK,
							part.ContainerTypePk,
							Convert(MeasureType.Weight, part.WeightMeasure, partList.WeightUnit, newPartList.WeightUnit),
							Convert(MeasureType.Volume, part.VolumeMeasure, partList.VolumeUnit, newPartList.VolumeUnit),
							part.LoadingMeterMeasure,
							part.PackageCount,
							part.PackageCountReference,
							part.PivotBreak);

						cache[part] = newPart;
						newPartList.AddPart(newPart);
					}
				}
			}
		}

		IClientProviderValues Convert(MeasureType measureType, IClientProviderValues val, string sourceUnit, string targetUnit)
		{
			try
			{
				return
					val != null
					? new ClientProviderValues(
						Convert(val.Actual, sourceUnit, targetUnit),
						Convert(val.ForClient, sourceUnit, targetUnit),
						Convert(val.ForProvider, sourceUnit, targetUnit))
					: null;
			}
			catch (NotSupportedException ex)
			{
				AddError(measureType, ex.Message);
				return null;
			}
		}

		public static decimal Convert(decimal sourceValue, string sourceUnit, string targetUnit)
		{
			sourceUnit = sourceUnit.ToUpper();
			targetUnit = targetUnit.ToUpper();

			if (sourceUnit == targetUnit)
			{
				return sourceValue;
			}
			if (Constants.Weight.ContainsCode(sourceUnit) && Constants.Weight.ContainsCode(targetUnit))
			{
				return Constants.Weight.Convert(sourceValue, sourceUnit, targetUnit);
			}
			if (Constants.Volume.ContainsCode(sourceUnit) && Constants.Volume.ContainsCode(targetUnit))
			{
				return Constants.Volume.Convert(sourceValue, sourceUnit, targetUnit);
			}

			throw new NotSupportedException(string.Format("SourceUnit ({0}) and TargetUnit ({1}) must both be weight units or volume units.", sourceUnit, targetUnit));
		}

		public bool HasMeasureType(MeasureType measureType)
			=> GetPartList(measureType) != null;

		/// <summary>
		/// The MeasureTypes present. Empty list if no measures have been added.
		/// </summary>
		public IEnumerable<MeasureType> GetMeasureTypes()
			=> Enum.GetValues(typeof(MeasureType))
				.Cast<MeasureType>()
				.Where(x => GetPartList(x) != null);

		public static MultilingualString GetDescription(MeasureType measureType) => MeasureTypeDescriptions.GetDescription(measureType);

		public int MeasureTypeCount => measureTypeParts.Count(x => x != null);

		public ZDecimal GetActual(MeasureType measureType)
			=> GetActualOrNull(measureType) ?? 0;

		decimal? GetActualOrNull(MeasureType measureType)
		{
			var parts = GetPartList(measureType);
			if (parts == null)
			{
				return null;
			}

			var info = PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType);
			decimal result = 0;
			foreach (var part in parts)
			{
				result += info.GetActualValue(part);
			}

			return result;
		}

		public ZString GetUnit(MeasureType measureType)
		{
			var partList = GetPartList(measureType);
			if (partList != null)
			{
				return PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType)
					.GetUnit(partList);
			}
			else
			{
				return ZString.Empty;
			}
		}

		public int GetPartCount(MeasureType measureType)
		{
			var parts = GetPartList(measureType);
			if (parts != null)
			{
				return parts.Count;
			}
			else
			{
				return 0;
			}
		}

		public ZDecimal GetForClient(MeasureType measureType)
		{
			var partList = GetPartList(measureType);
			if (partList != null)
			{
				decimal result = 0;
				var measureInfo = PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType);
				foreach (var part in partList)
				{
					result += measureInfo.GetClientValue(part);
				}
				return result;
			}
			else
			{
				return 0;
			}
		}

		public ZDecimal GetForProvider(MeasureType measureType)
		{
			var partList = GetPartList(measureType);
			if (partList != null)
			{
				decimal result = 0;
				var measureInfo = PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType);
				foreach (var part in partList)
				{
					result += measureInfo.GetProviderValue(part);
				}
				return result;
			}
			else
			{
				return 0;
			}
		}

		public void SetQuantity(MeasureType measureType, decimal amount, string unit)
		{
			if (measureType == MeasureType.ContainerCount)
			{
				// Some unit tests set this as a number.
				// In production it should always be a list of containers
				CreateContainerList(includeCommodity: false);
				for (int i = 0; i < (int)amount; ++i)
				{
					AddContainer(ZGuid.Empty);
				}
			}
			else
			{
				var part = new JobLevelPart();
				part.SetQuantity(measureType, amount, unit);
				AddPartList(measureType, part);
			}
		}

		decimal SumActualValues(IRateablePartList partList, IPartMeasureInfo measureInfo)
		{
			decimal result = 0;
			foreach (var part in partList)
			{
				result += measureInfo.GetActualValue(part);
			}

			return result;
		}

		/// <summary>
		/// Return value as a Quantity struct.
		/// Quantity does not allow blank units except as a default, where amount as zero.
		/// So this is useless for values with no units.
		/// It will return default Quantity if units are blank.
		/// Does not set reference.
		/// </summary>
		public Quantity GetQuantity(MeasureType measureType)
		{
			var partList = GetPartList(measureType);
			if (partList != null)
			{
				var measureInfo = PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType);
				var unit = measureInfo.GetUnit(partList);
				if (!string.IsNullOrWhiteSpace(unit))
				{
					var actual = SumActualValues(partList, measureInfo);
					return new Quantity(actual, unit);
				}
			}

			return default;
		}

		/// <summary>
		/// Measure TimeInfo.
		/// Returns null if MeasureType.Time has no TimeInfo or there is no MeasureType.Time.
		/// </summary>
		public TimeInfo Time
		{
			get
			{
				var partList = GetPartList(MeasureType.Time);
				return partList?.FirstOrDefault()?.Time;
			}
			set
			{
				if (value != null)
				{
					AddPartList(MeasureType.Time, new JobLevelPart { Time = value });
				}
				else
				{
					AddPartList(MeasureType.Time, null);
				}
			}
		}

		public string ValueString(MeasureType measureType)
		{
			var partList = GetPartList(measureType);
			if (partList == null)
			{
				return string.Empty;
			}

			var firstPart = partList.FirstOrDefault();
			var time = firstPart?.Time;
			if (time != null)
			{
				return time.ToString();
			}

			if (firstPart is IRateableContainer)
			{
				return partList.Count.ToString();
			}
			else
			{
				var measureInfo = PartMeasureInfoProvider.Instance.GetPartMeasureInfo(measureType);
				decimal actual = 0;
				decimal forProvider = 0;
				decimal forClient = 0;
				foreach (var part in partList)
				{
					actual += measureInfo.GetActualValue(part);
					forProvider += measureInfo.GetProviderValue(part);
					forClient += measureInfo.GetClientValue(part);
				}
				return string.Format("({0}, {1}, {2})", actual, forClient, forProvider);
			}
		}

		/// <summary>
		/// ValueString appended with unit string, e.g., 12 KM (or just ValueString if there is no unit).
		/// </summary>
		public string ValueAndUnitString(MeasureType measureType)
		{
			var unit = GetUnit(measureType);
			return ValueString(measureType) + (unit.IsEmpty ? string.Empty : " " + unit);
		}

		static Guid? ToNullable(ZGuid pk) => NullableHelper.ToNullable(pk);

		public RateableMeasureSet SplitOutLCLMeasuresIfPresent(IRateableMeasureSet otherMeasures)
		{
			var containerParts = GetPartList(MeasureType.ContainerCount);
			if (containerParts == null)
			{
				return null;
			}

			bool hasLCL = false;
			decimal lclWeightInKG = 0;
			decimal lclVolumeInM3 = 0;
			int lclPackages = 0;
			foreach (IRateableContainer container in containerParts)
			{
				if (container.ContainerTypePk == MeasureInfo.ContainerInfo.LCL.ToGuid())
				{
					hasLCL = true;
					lclWeightInKG += container.ContainerWeightInKG;
					lclVolumeInM3 += container.ContainerVolumeInM3;
					lclPackages += container.ContainerPackages;
				}
			}

			if (hasLCL)
			{
				var newMeasures = ((RateableMeasureSet)otherMeasures).ShallowishClone();
				newMeasures.SetQuantity(MeasureType.Weight, lclWeightInKG, Constants.Weight.Kilograms);
				newMeasures.SetQuantity(MeasureType.Volume, lclVolumeInM3, Constants.Volume.CubicMetres);
				newMeasures.SetQuantity(MeasureType.Package, lclPackages, string.Empty);
				newMeasures.RemoveMeasureType(MeasureType.Chargeable);
				newMeasures.RemoveContainerList();
				return newMeasures;
			}

			return null;
		}

		/// <summary>
		/// Put all cached information in here so it can easily be cleared.
		/// </summary>
		struct CachedInfo
		{
			public HashSet<string> distinctCommodities;
			public HashSet<string> distinctRefUnitSections;
			public HashSet<Guid?> distinctRefContainerComponents;
			public HashSet<Guid?> distinctRefContainerRepairs;
			public HashSet<Guid?> distinctRefContainerMaterials;
			public HashSet<RefContainerInfoParts> distinctRefContainerInfo;
			public HashSet<Guid?> distinctWarehousePKs;
			public HashSet<Guid?> distinctProductPKs;
			public HashSet<string> distinctContainerOwnerships;
			public HashSet<string> distinctPackageTypes;
			public HashSet<Guid?> distinctContainerTypePKs;
		}
		CachedInfo cachedInfo;

		void ClearCache()
		{
			cachedInfo = default;
			foreach (var part in measureTypeParts.OfType<RateablePartList>())
			{
				part.ClearCache();
			}
		}

		/// <summary>
		/// Makes a memberwise clone of everything except the part list for each MeasureType.
		/// That is itself shallow cloned so the new class can remove entire measures without affecting the original.
		/// However individual measures are not cloned.
		/// Thus the clone or original shouldn't be subsequently used to modify parts, quantities and attributes.
		/// </summary>
		RateableMeasureSet ShallowishClone()
		{
			var clone = (RateableMeasureSet)MemberwiseClone();
			clone.measureTypeParts = new IRateablePartList[measureTypeParts.Length];
			clone.ClearCache();
			Array.Copy(measureTypeParts, clone.measureTypeParts, measureTypeParts.Length);
			return clone;
		}
	}
}
