using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Rating.Rateable
{
	/// <summary>
	/// List of RateableParts with the same measure types and dimensions
	/// E.g., all the containers.
	/// </summary>
	public class RateablePartList : IRateablePartList
	{
		public RateablePartList()
		{
		}

		public void AddPart(IRateablePart part)
		{
			PartList.Add(part);
		}

		public void AddParts(IEnumerable<IRateablePart> parts)
		{
			PartList.AddRange(parts);
		}

		#region Parts

		List<IRateablePart> PartList
		{
			get
			{
				if (lazyPopulate != null)
				{
					var action = lazyPopulate;
					lazyPopulate = null;
					action(this);
				}

				return partListDoNotUseDirectlySinceLazyPopulated;
			}
		}
		List<IRateablePart> partListDoNotUseDirectlySinceLazyPopulated = new List<IRateablePart>();
		Action<RateablePartList> lazyPopulate;

		internal void SetLazyPopulate(Action<RateablePartList> lazyPopulate) => this.lazyPopulate = lazyPopulate;
		public bool HasPendingLazyPopulate => lazyPopulate != null;

		public IEnumerator<IRateablePart> GetEnumerator() => PartList.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public int Count => PartList.Count;
		public IRateablePart this[int index] => PartList[index];

		#endregion

		public string WeightUnit { get; set; }

		public string VolumeUnit { get; set; }

		public string ChargeableUnit { get; set; }

		public string PackageUnit { get; set; }

		public bool HasProductAttributes { get; set; }

		public bool HasLocation { get; set; }

		public bool HasDocketReference { get; set; }

		public bool HasPackageType { get; set; }

		public bool HasWarehouseLine { get; set; }

		public bool HasCartageLegPK { get; set; }

		public bool HasPalletID { get; set; }

		public bool HasContainerNumber { get; set; }

		public bool HasContainerType { get; set; }

		public bool HasCommodity { get; set; }

		public bool HasMaintainanceAndRepairFields { get; set; }

		public bool HasWarehouse { get; set; }

		public bool HasProduct { get; set; }

		public bool HasPalletized { get; set; }

		public bool HasContainerOwnership { get; set; }

		public bool HasChargeGroupToUse { get; set; }

		public bool HasYardUnitType { get; set; }

		public bool HasYardUnitLoad { get; set; }

		public bool HasYardUnitClient { get; set; }

		public string PickupDistanceUnit { get; set; }

		public string DeliveryDistanceUnit { get; set; }

		public string AreaUnit { get; set; }

		public string LengthUnit { get; set; }

		public IReadOnlyCollection<Guid?> GetDistinctContainerTypePKs()
			=> GetOrCreateDistinctPKs(HasContainerType, ref cachedInfo.distinctContainerTypes, GetPartContainerTypePk);
		static Guid? GetPartContainerTypePk(IRateablePart p) => p.ContainerTypePk;

		public IReadOnlyCollection<string> GetDistinctContainerQualities()
			=> GetOrCreateDistinct(HasContainerType, ref cachedInfo.distinctContainerQualities, GetPartContainerQuality);
		static string GetPartContainerQuality(IRateablePart p) => (p as IRateableContainer)?.ContainerQuality ?? string.Empty;

		public IReadOnlyCollection<bool> GetDistinctContainerIsNonOperatingReefers()
			=> GetOrCreateDistinct(HasContainerType, ref cachedInfo.distinctContainerIsNonOperatedReefer, GetPartIsNonOperatedReefer);
		static bool GetPartIsNonOperatedReefer(IRateablePart p) => (p as IRateableContainer)?.ContainerIsNonOperatingReefer ?? false;

		public IReadOnlyCollection<string> GetDistinctCommodities()
			=> GetOrCreateDistinct(HasCommodity, ref cachedInfo.distinctCommodities, GetPartCommodityCode);
		static string GetPartCommodityCode(IRateablePart p) => p.CommodityCode;

		public IReadOnlyCollection<string> GetDistinctRefUnitSection()
			=> GetOrCreateDistinct(HasMaintainanceAndRepairFields, ref cachedInfo.distinctRefUnitSections, GetPartRefUnitSection);
		static string GetPartRefUnitSection(IRateablePart p) => p.RefUnitSection;

		public IReadOnlyCollection<Guid?> GetDistinctJobRefContainerComponents()
			=> GetOrCreateDistinct(HasMaintainanceAndRepairFields, ref cachedInfo.distinctRefComponents, GetPartRefContainerComponents);
		static Guid? GetPartRefContainerComponents(IRateablePart p) => p.RefContainerComponent;

		public IReadOnlyCollection<Guid?> GetDistinctRefContainerMaterials()
			=> GetOrCreateDistinct(HasMaintainanceAndRepairFields, ref cachedInfo.distinctRefMaterials, GetPartRefContainerMaterials);
		static Guid? GetPartRefContainerMaterials(IRateablePart p) => p.RefMaterialCode;

		public IReadOnlyCollection<Guid?> GetDistinctRefContainerRepairs()
			=> GetOrCreateDistinct(HasMaintainanceAndRepairFields, ref cachedInfo.distinctRefRepairs, GetPartRefContainerRepairs);
		static Guid? GetPartRefContainerRepairs(IRateablePart p) => p.RefRepairCode;

		public IReadOnlyCollection<RefContainerInfoParts> GetDistinctRefContainerInfo()
		{
			if (cachedInfo.distinctRefContainerInfo == null)
			{
				cachedInfo.distinctRefContainerInfo = PartList
					.Where(p => p != null)
					.Select(p => new RefContainerInfoParts
					{
						RefContainerComponent = p.RefContainerComponent,
						RefContainerMaterial = p.RefMaterialCode,
						RefContainerRepair = p.RefRepairCode,
						RefUnitSection = p.RefUnitSection,
						WorkOrderLinePK = p.WorkOrderLinePK ?? Guid.Empty,
					})
					.ToHashSet();
			}

			return cachedInfo.distinctRefContainerInfo;
		}

		public IReadOnlyCollection<string> GetDistinctPackageTypes()
			=> GetOrCreateDistinct(HasPackageType, ref cachedInfo.distinctPackageTypes, GetPartPackageType);
		static string GetPartPackageType(IRateablePart p) => p.PackageType;

		public IReadOnlyCollection<Guid?> GetDistinctWarehousePKs()
			=> GetOrCreateDistinctPKs(HasWarehouse, ref cachedInfo.distinctWarehousePKs, GetPartWarehousePk);
		static Guid? GetPartWarehousePk(IRateablePart p) => p.WarehousePk;

		public IReadOnlyCollection<Guid?> GetDistinctProductPKs()
			=> GetOrCreateDistinctPKs(HasProduct, ref cachedInfo.distinctProductPKs, GetPartProductPk);
		static Guid? GetPartProductPk(IRateablePart p) => p.ProductPk;

		public IReadOnlyCollection<string> GetDistinctContainerOwnerships()
			=> GetOrCreateDistinct(HasContainerOwnership, ref cachedInfo.distinctContainerOwnerships, GetPartContainerOwnership);
		static string GetPartContainerOwnership(IRateablePart p) => p.ContainerOwnership;

		public IReadOnlyCollection<bool> GetDistinctPalletized()
			=> cachedInfo.distinctPalletized ?? (cachedInfo.distinctPalletized = CreateDistinctPalletized());

		/// <summary>
		/// Put all cached information in here so the NewEmptyClone method:
		/// - can invalidate them all at once without having to set each one
		/// - doesn't need changing if more cached information is added
		/// </summary>
		struct CachedInfo
		{
			public HashSet<Guid?> distinctContainerTypes;
			public HashSet<string> distinctContainerQualities;
			public HashSet<bool> distinctContainerIsNonOperatedReefer;
			public HashSet<string> distinctCommodities;
			public HashSet<string> distinctRefUnitSections;
			public HashSet<Guid?> distinctRefComponents;
			public HashSet<Guid?> distinctRefRepairs;
			public HashSet<Guid?> distinctRefMaterials;
			public HashSet<RefContainerInfoParts> distinctRefContainerInfo;
			public HashSet<string> distinctPackageTypes;
			public HashSet<Guid?> distinctWarehousePKs;
			public HashSet<Guid?> distinctProductPKs;
			public IReadOnlyCollection<bool> distinctPalletized;
			public HashSet<string> distinctContainerOwnerships;
		}
		CachedInfo cachedInfo;

		internal void ClearCache() => cachedInfo = default;

		IReadOnlyCollection<bool> CreateDistinctPalletized()
		{
			bool hasPalletized = false;
			bool hasNotPalletized = false;
			foreach (var part in PartList)
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

		IReadOnlyCollection<T> GetOrCreateDistinct<T>(bool hasValues, ref HashSet<T> distinctValues, Func<IRateablePart, T> valueFunc)
		{
			if (!hasValues)
			{
				return Array.Empty<T>();
			}

			if (distinctValues == null)
			{
				distinctValues = PartList
					.Select(valueFunc)
					.Where(x => x != null)
					.ToHashSet();
			}

			return distinctValues;
		}

		IReadOnlyCollection<Guid?> GetOrCreateDistinctPKs(bool hasValues, ref HashSet<Guid?> distinctValues, Func<IRateablePart, Guid?> valueFunc)
		{
			if (!hasValues)
			{
				return Array.Empty<Guid?>();
			}

			if (distinctValues == null)
			{
				distinctValues = PartList
					.Select(valueFunc)
					.ToHashSet();
			}

			return distinctValues;
		}

		public IRateablePartList NewEmptyClone()
		{
			var clone = (RateablePartList)MemberwiseClone();
			clone.partListDoNotUseDirectlySinceLazyPopulated = new List<IRateablePart>();
			clone.lazyPopulate = null;
			clone.cachedInfo = default;
			return clone;
		}

		/// <summary>
		/// Populate this empty clone from the given source
		/// </summary>
		public void PopulateEmptyClone(IRateablePartList source)
		{
			// Units
			ChargeableUnit = source.ChargeableUnit;
			WeightUnit = source.WeightUnit;
			VolumeUnit = source.VolumeUnit;
			PackageUnit = source.PackageUnit;
			PickupDistanceUnit = source.PickupDistanceUnit;
			DeliveryDistanceUnit = source.DeliveryDistanceUnit;

			// Dimensions
			HasCartageLegPK = source.HasCartageLegPK;
			HasChargeGroupToUse = source.HasChargeGroupToUse;
			HasCommodity = source.HasCommodity;
			HasContainerNumber = source.HasContainerNumber;
			HasContainerOwnership = source.HasContainerOwnership;
			HasContainerType = source.HasContainerType;
			HasDocketReference = source.HasDocketReference;
			HasLocation = source.HasLocation;
			HasPackageType = source.HasPackageType;
			HasPalletID = source.HasPalletID;
			HasPalletized = source.HasPalletized;
			HasProduct = source.HasProduct;
			HasProductAttributes = source.HasProductAttributes;
			HasWarehouse = source.HasWarehouse;
		}
	}
}
