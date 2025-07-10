using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	/// <summary>
	/// This class has been created to allow current tests that use AllocateLocations for set up to continue to function, if PutawayEngine is enabled, without the need to implement a PutawayEngine mock for every test.
	/// The logic is currently very similar to PutawayManagerForReceive.cs but will be reduced to what is required in WI00343801.
	/// </summary>
	public class PutawayManagerLegacyMockForReceive : IPutawayEngineManagerForReceive
	{
		public PutawayManagerLegacyMockForReceive(WhsReceive receive)
		{
			ParentDocket = receive;
			Factory = receive.Factory;
			IsCustomsReceipt = receive.WD_DocketSubType == ReceiveType.Codes.Customs;
		}

		WhsReceive ParentDocket { get; }
		BusinessObjectFactory Factory { get; }
		bool IsCustomsReceipt { get; }

		public void Putaway(IEnumerable<WhsReceive> receives, IEnumerable<WhsReceiveLine> linesToPutaway, INotifications notifications, RefEquipment equipment = null, IEnumerable<ZGuid> skipLocationPKs = null, bool useLocationConcurrencyHandling = false, bool needRebuildLocationCache = true)
		{
			if (GetLastResortLocation() == null)
			{
				notifications.Notify(new ErrorNotification(ReceiveErrorTypes.NoLocationsDefined));
			}
			else
			{
				AutoAllocateReceive(linesToPutaway);
			}
		}

		void AutoAllocateReceive(IEnumerable<ILineToPutaway> linesToPutaway)
		{
			PrepareLocations();

			var linesToAllocate = Palletize(linesToPutaway);
			var possibleLocations = new List<WhsLocation>(10000);
			ReloadPossibleLocations(possibleLocations);

			foreach (var pallet in linesToAllocate.Pallets)
			{
				for (var i = 0; pallet.LocationPK.IsEmpty && i < possibleLocations.Count; i++)
				{
					GetLocationData(possibleLocations[i]).TryAllocate(pallet);
				}
			}
			foreach (var lineToPutaway in linesToAllocate.SingleLines)
			{
				for (var i = 0; lineToPutaway.LocationPK.IsEmpty && i < possibleLocations.Count; i++)
				{
					GetLocationData(possibleLocations[i]).TryAllocate(lineToPutaway);
				}
			}

			if (linesToAllocate.Count != 0)
			{
				LastResortAllocation(linesToAllocate);
			}
		}

		#region PrepareLocations

		void PrepareLocations()
		{
			foreach (var lineToPutaway in GetAllLinesFromDocket(ParentDocket))
			{
				if (IsValid(lineToPutaway) && lineToPutaway.LocationPK.IsValid)
				{
					GetLocationData(lineToPutaway.Location).Allocate(lineToPutaway);
				}
			}
		}

		IEnumerable<ILineToPutaway> GetAllLinesFromDocket(WhsReceive receive)
		{
			return receive.Lines.Cast<WhsReceiveLine>();
		}

		bool IsValid(ILineToPutaway lineToPutaway)
		{
			return lineToPutaway != null && lineToPutaway.Product != null && lineToPutaway.QuantityToPutaway > 0 && lineToPutaway.IsValidToPutaway;
		}

		#endregion

		LocationWithPutawayLines GetLocationData(WhsLocation location)
		{
			if (!Locations.TryGetValue(location, out var result))
			{
				// We add all locations to this cache in 1 DB hit below (ReloadPossibleLocations), but in case some location
				// is not in the cache, we add it to the cache for just that one location. This should not happen.
				PopulateLocationData(new[] { location });
				result = Locations[location];
			}
			return result;
		}

		#region ReloadPossibleLocations

		void ReloadPossibleLocations(List<WhsLocation> possibleLocations)
		{
			possibleLocations.Clear();

			var isCustomsReceive = IsCustomsReceipt;
			foreach (var location in LoadPossibleLocations())
			{
				if (IsPossibleLocationForPutaway(location, isCustomsReceive))
				{
					possibleLocations.Add(location);
				}
			}

			possibleLocations.Sort(new LocationSorter(ParentDocket.Client));

			// Factory caches query results for same query text and it does not know that changing docketline values will affect WhsInventoryView.
			// so if we execute the same query a second time the factory will give old results instead of hitting the database.
			ParentDocket.Factory.ClearQueryCache(WhsInventoryViewSchema.Constants.TableName);

			PopulateLocationData(possibleLocations);
		}

		IEnumerable<WhsLocation> LoadPossibleLocations()
		{
			foreach (WhsRow row in ParentDocket.Warehouse.Rows)
			{
				foreach (var location in row.Locations)
				{
					yield return location;
				}
			}
		}

		bool IsPossibleLocationForPutaway(WhsLocation location, bool isCustomsReceipt)
		{
			return location.WLV_LocationStatus == LocationStatus.Codes.Normal && IsLocationValidToPutawayTo(location, isCustomsReceipt);
		}

		bool IsLocationValidToPutawayTo(WhsLocation location, bool isCustomsReceipt)
		{
			var locationAreaType = location.PutawayArea.WA_AreaType;
			var areaTypeIsBonded = locationAreaType == AreaTypes.Codes.Bonded;
			return (areaTypeIsBonded == isCustomsReceipt) && !location.IsDockDoorLocation && !location.IsPackingStationLocation && !location.IsPackingConsolidationLocation;
		}

		#endregion

		#region PopulateLocationData

		void PopulateLocationData(IEnumerable<WhsLocation> locationsToPopulateDataFor)
		{
			var locationsWithNoData = GetLocationsWithoutData(locationsToPopulateDataFor);
			if (locationsWithNoData.Count > 0)
			{
				var locationData = GetLocationData(locationsWithNoData);

				foreach (var data in locationData)
				{
					Locations.Add(data.Location, data);
					locationsWithNoData.Remove(data.Location.PK); // remove the locations for which we found data
				}

				// add locations for which no data was added (mostly not saved locations in unit tests, but keep old behaviour just in case).
				foreach (var location in locationsWithNoData)
				{
					var data = new LocationWithPutawayLines(location.Value, true, ZGuid.Empty, decimal.MaxValue, location.Value.WLV_MaxWeight, location.Value.WLV_MaxCubic);
					Locations.Add(location.Value, data);
				}
			}
		}

		Dictionary<ZGuid, WhsLocation> GetLocationsWithoutData(IEnumerable<WhsLocation> allLocations)
		{
			var locationsWithNoData = new Dictionary<ZGuid, WhsLocation>();

			foreach (var location in allLocations)
			{
				if (!Locations.ContainsKey(location))
				{
					locationsWithNoData.Add(location.PK, location);
				}
			}

			return locationsWithNoData;
		}

		IEnumerable<LocationWithPutawayLines> GetLocationData(Dictionary<ZGuid, WhsLocation> locationsWithNoData)
		{
			var result = new List<LocationWithPutawayLines>();
			var rawSql = @"
select
	WL_PK,
	CASE WHEN AvailableWeight IS NULL OR AvailableWeight > 0 THEN AvailableWeight ELSE 0 END as AvailableWeight,
	CASE WHEN AvailableVolume IS NULL OR AvailableVolume > 0 THEN AvailableVolume ELSE 0 END as AvailableVolume,
	CASE WHEN AvailableUnits IS NULL OR AvailableUnits > 0 THEN AvailableUnits ELSE 0 END as AvailableUnits,
	FirstProduct,
	NumberOfProducts
from
	dbo.WhsLocation 
	cross apply
	(
		select
			AvailableWeight, 
			AvailableVolume, 
			AvailableUnits 
		from 
			dbo.WhsLocationCapacity(WL_PK, @DocketToExclude)
	) as Capacity
	cross apply
	(
		select
            FirstProduct = max(WE_OP),
            NumberOfProducts = count(distinct WE_OP)
        from
            dbo.WhsDocketLine
        where
            WE_StockOnHand > 0
            and WE_WL = WL_PK 
	) as Products
where
	WL_PK in (select value from @LocationPKs)";

			var sqlParameters = new ZSqlParameterCollection();
			sqlParameters.Add(ZSqlParameter.New("@LocationPKs", locationsWithNoData.Keys, WhsLocationSchema.PK, isTableValued: true));
			sqlParameters.Add("@DocketToExclude", ParentDocket.PK, WhsDocketSchema.WD_OH_Forwarder); // I have to use nullable GUID as a schema column to make some exiting dodgy tests to pass when PK is Empty

			var sqlResults = new DynamicBusinessObjectCollection(ParentDocket.Factory);
			sqlResults.Load(rawSql, sqlParameters);

			foreach (DynamicBusinessObject sqlResult in sqlResults)
			{
				var locationPK = (ZGuid)sqlResult[WhsLocationSchema.Constants.PK];
				var location = locationsWithNoData[locationPK];
				var numberOfProducts = (ZInt)sqlResult["NumberOfProducts"];
				var firstProductPK = (ZGuid)sqlResult["FirstProduct"];

				if (numberOfProducts <= 1)
				{
					var query = new ZQuery(WhsDocketLineSchema.WE_WL, locationPK);
					query.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);
					query.FetchOnlyFromLocalCache = true;

					var inMemoryInventories = Factory.Load<WhsDocketLine>(query);
					var uniqueProductsInMemory = inMemoryInventories.Select(i => i.WE_OP).Where(p => p != firstProductPK).Distinct().Count();
					numberOfProducts += uniqueProductsInMemory;
					firstProductPK = (firstProductPK.IsEmpty && inMemoryInventories.Length > 0) ? inMemoryInventories[0].WE_OP : firstProductPK;
				}

				var isLocationEmpty = numberOfProducts == 0;

				ZGuid productPK;
				if (numberOfProducts == 0)
				{
					productPK = ZGuid.Empty;
				}
				else
				{
					productPK = (numberOfProducts == 1) ? firstProductPK : ZGuid.Missing;
				}

				var availableWeight = (ZDecimal)sqlResult["AvailableWeight"];
				var availableVolume = (ZDecimal)sqlResult["AvailableVolume"];
				var availableUnits = (location.WLV_MaxQuantity > 0) ? (ZDecimal)sqlResult["AvailableUnits"] : (ZDecimal)decimal.MaxValue;

				var data = new LocationWithPutawayLines(location, isLocationEmpty, productPK, availableUnits, availableWeight, availableVolume);
				result.Add(data);
			}

			return result;
		}

		Dictionary<WhsLocation, LocationWithPutawayLines> Locations
		{
			get { return locations ?? (locations = new Dictionary<WhsLocation, LocationWithPutawayLines>(100)); }
		}
		Dictionary<WhsLocation, LocationWithPutawayLines> locations;

		#endregion

		#region Palletize

		PalletizedPutawayCollection Palletize(IEnumerable<ILineToPutaway> linesToPutaway)
		{
			#region Collect valid unallocated lines and sort it

			var allLinesToAllocate = new HashSet<ILineToPutaway>();
			foreach (var lineToPutaway in linesToPutaway)
			{
				if (IsValid(lineToPutaway) && lineToPutaway.LocationPK.IsEmpty)
				{
					allLinesToAllocate.Add(lineToPutaway);
				}
			}

			var allLinesToAllocateSorted = allLinesToAllocate.ToArray();
			Array.Sort(allLinesToAllocateSorted, new SortByStatusThenByHeldCodeAndThenByProduct());

			#endregion

			#region Divide into pallets

			var linesToAllocate = new PalletizedPutawayCollection(allLinesToAllocateSorted.Length);
			foreach (var lineToAllocate in allLinesToAllocateSorted)
			{
				linesToAllocate.AddLineToPutaway(lineToAllocate);
			}

			#endregion

			#region Add lines not marked to allocate but included in pallets that should be allocated

			foreach (var lineToPutaway in GetAllLinesFromDocket(ParentDocket))
			{
				if (IsValid(lineToPutaway) && lineToPutaway.LocationPK.IsEmpty && !allLinesToAllocate.Contains(lineToPutaway))
				{
					linesToAllocate.AddLineIfPalletGroupExists(lineToPutaway);
				}
			}

			return linesToAllocate;

			#endregion
		}

		#endregion

		#region LastResortAllocation

		void LastResortAllocation(PalletizedPutawayCollection linesToAllocate)
		{
			var location = GetLastResortLocation();

			foreach (var pallet in linesToAllocate.Pallets)
			{
				if (pallet.LocationPK.IsEmpty)
				{
					GetLocationData(location).Allocate(pallet);
				}
			}

			foreach (var lineToPutaway in linesToAllocate.SingleLines)
			{
				if (lineToPutaway.LocationPK.IsEmpty)
				{
					GetLocationData(location).Allocate(lineToPutaway);
				}
			}
		}

		WhsLocation GetLastResortLocation()
		{
			var isCustomsReceive = IsCustomsReceipt;
			foreach (WhsRow row in ParentDocket.Warehouse.Rows)
			{
				foreach (var location in row.Locations)
				{
					if (location.WLV_LocationStatus != LocationStatus.Codes.Damaged &&
						location.WLV_LocationStatus != LocationStatus.Codes.Void &&
						location.WLV_LocationStatus != LocationStatus.Codes.Held &&
						IsLocationValidToPutawayTo(location, isCustomsReceive))
					{
						return location;
					}
				}
			}

			return null;
		}

		#endregion

		#region LocationWithPutawayLines

		class LocationWithPutawayLines
		{
			#region Constructor

			public LocationWithPutawayLines(WhsLocation location, ZBool isLocationEmpty, ZGuid productPK, ZDecimal freeQuantity, ZDecimal freeWeight, ZDecimal freeVolume)
			{
				Location = Argument.NotNull(location, nameof(location));
				IsLocationEmpty = isLocationEmpty;
				ProductAllocated = productPK;

				FreeQuantity = freeQuantity >= 0 ? freeQuantity : throw new ArgumentException("Cannot be negative.", nameof(freeQuantity));
				FreeWeight = freeWeight >= 0 ? freeWeight : throw new ArgumentException("Cannot be negative.", nameof(freeWeight));
				FreeSpace = freeVolume >= 0 ? freeVolume : throw new ArgumentException("Cannot be negative.", nameof(freeVolume));
			}

			public readonly WhsLocation Location;

			#endregion

			#region Public

			#region ProductAllocated

			public ZGuid ProductAllocated { get; private set; }

			#endregion

			#region TryAllocate

			public void TryAllocate(PalletGroup pallet)
			{
				if ((ProductAllocated.IsEmpty || IsLocationEmpty) && GetVolume(pallet) <= FreeSpace && GetWeight(pallet) <= FreeWeight && GetQuantity(pallet) <= FreeQuantity)
				{
					Allocate(pallet);
				}
			}

			public void TryAllocate(ILineToPutaway lineToPutaway)
			{
				if (ProductAllocated.IsEmpty || (!IsLocationFull && ProductAllocated == lineToPutaway.ProductPK))
				{
					if (IsLocationsMaxWeightSpecified || IsLocationsMaxVolumeSpecified || IsLocationMaxQuantitySpecified)
					{
						var putawayVolume = IsLocationsMaxVolumeSpecified ? GetVolume(lineToPutaway) : ZDecimal.Zero;
						var putawayWeight = IsLocationsMaxWeightSpecified ? GetWeight(lineToPutaway) : ZDecimal.Zero;
						var putawayQuantity = IsLocationMaxQuantitySpecified ? GetQuantity(lineToPutaway) : ZDecimal.Zero;

						if (putawayVolume <= FreeSpace && putawayWeight <= FreeWeight && putawayQuantity <= FreeQuantity)
						{
							Allocate(lineToPutaway);
						}
						else if (lineToPutaway.QuantityToPutaway > 0)
						{
							SplitLine(lineToPutaway, putawayVolume, putawayWeight);
						}
					}
					else if (IsLocationEmpty)
					{
						Allocate(lineToPutaway);
					}
				}
			}

			void SplitLine(ILineToPutaway lineToPutaway, decimal putawayVolume, decimal putawayWeight)
			{
				decimal quantity = lineToPutaway.QuantityToPutaway;
				decimal unitVolume = putawayVolume / quantity;
				decimal unitWeight = putawayWeight / quantity;

				decimal maxQtyFitByVolume = unitVolume == 0 ? decimal.MaxValue : FreeSpace / unitVolume;
				decimal maxQtyFitByWeight = unitWeight == 0 ? decimal.MaxValue : FreeWeight / unitWeight;
				decimal maxQtyFitByQuantity = FreeQuantity;

				maxQtyFitByVolume = IsLocationsMaxVolumeSpecified ? maxQtyFitByVolume : quantity;
				maxQtyFitByWeight = IsLocationsMaxWeightSpecified ? maxQtyFitByWeight : quantity;
				maxQtyFitByQuantity = IsLocationMaxQuantitySpecified ? maxQtyFitByQuantity : quantity;

				var maxQtyToAllocate = decimal.Floor(Math.Min(Math.Min(maxQtyFitByVolume, maxQtyFitByWeight), maxQtyFitByQuantity));
				if (maxQtyToAllocate >= 1)
				{
					var newLine = lineToPutaway.Split(maxQtyToAllocate);
					Allocate(newLine);
				}
			}

			#endregion

			#region Allocate

			public void Allocate(PalletGroup pallet)
			{
				FreeSpace = 0;
				FreeWeight = 0;
				FreeQuantity = 0;
				foreach (var lineToPutaway in pallet.PutawayList)
				{
					Allocate(lineToPutaway);
				}
			}

			public void Allocate(ILineToPutaway lineToPutaway)
			{
				if (lineToPutaway.LocationPK.IsEmpty)
				{
					lineToPutaway.LocationPK = Location.PK;
					IsLocationEmpty = false;
				}

				if (lineToPutaway.PackType.IsEmpty)
				{
					lineToPutaway.PackType = lineToPutaway.Product.OP_StockKeepingUnit;
				}

				RefreshProductOnAddPutawayLine(lineToPutaway.ProductPK);
				if (IsLocationsMaxVolumeSpecified)
				{
					RefreshFreeSpaceOnAddPutawayLine(lineToPutaway);
				}

				if (IsLocationsMaxWeightSpecified)
				{
					RefreshFreeWeightOnAddPutawayLine(lineToPutaway);
				}

				if (IsLocationMaxQuantitySpecified)
				{
					RefreshFreeQuantityOnAddPutawayLine(lineToPutaway);
				}
			}

			#endregion

			#endregion

			#region Implementation

			#region fields

			bool IsLocationEmpty;
			bool IsLocationFull;
			ZDecimal FreeSpace;
			ZDecimal FreeWeight;
			ZDecimal FreeQuantity;

			#endregion

			#region Product

			void RefreshProductOnAddPutawayLine(ZGuid productPK)
			{
				if (ProductAllocated.IsEmpty)
				{
					ProductAllocated = productPK;
				}
				else if (ProductAllocated != productPK)
				{
					ProductAllocated = ZGuid.Missing;
				}
			}

			#endregion

			#region Volume

			void RefreshFreeSpaceOnAddPutawayLine(ILineToPutaway lineToPutaway)
			{
				if (FreeSpace > 0m)
				{
					if (VolumeIsDefined(lineToPutaway))
					{
						FreeSpace -= GetVolume(lineToPutaway);
					}
					else
					{
						FreeSpace = 0m;
					}
				}
				if (FreeSpace <= 0m)
				{
					IsLocationFull = true;
				}
			}

			bool VolumeIsDefined(ILineToPutaway lineToPutaway)
			{
				var part = lineToPutaway.Product;
				return part.OP_Cubic > 0m && !part.OP_CubicUQ.IsEmpty;
			}

			ZDecimal GetVolume(ILineToPutaway lineToPutaway)
			{
				ZDecimal putawayVolume = 0m;
				if (VolumeIsDefined(lineToPutaway))
				{
					var part = lineToPutaway.Product;
					putawayVolume = lineToPutaway.QuantityToPutaway * part.OP_Cubic;
					putawayVolume = Constants.Volume.Convert(putawayVolume, part.OP_CubicUQ, Location.WLV_MaxCubicUnit);
				}
				return putawayVolume;
			}

			ZDecimal GetVolume(PalletGroup pallet)
			{
				ZDecimal palletVolume = 0m;

				if (IsLocationsMaxVolumeSpecified)
				{
					foreach (var putawayLine in pallet.PutawayList)
					{
						palletVolume += GetVolume(putawayLine);
					}
				}

				return palletVolume;
			}

			bool IsLocationsMaxVolumeSpecified
			{
				get { return !Location.WLV_MaxCubicUnit.IsEmpty && Location.WLV_MaxCubic > 0m; }
			}

			#endregion

			#region Weight

			void RefreshFreeWeightOnAddPutawayLine(ILineToPutaway lineToPutaway)
			{
				if (FreeWeight > 0)
				{
					if (WeightIsDefined(lineToPutaway))
					{
						FreeWeight -= GetWeight(lineToPutaway);
					}
					else
					{
						FreeWeight = 0;
					}
				}
				if (FreeWeight <= 0)
				{
					IsLocationFull = true;
				}
			}

			bool WeightIsDefined(ILineToPutaway lineToPutaway)
			{
				var part = lineToPutaway.Product;
				return part.OP_Weight > 0m && !part.OP_WeightUQ.IsEmpty;
			}

			ZDecimal GetWeight(ILineToPutaway lineToPutaway)
			{
				ZDecimal putawayWeight = 0m;
				if (WeightIsDefined(lineToPutaway))
				{
					var part = lineToPutaway.Product;
					putawayWeight = lineToPutaway.QuantityToPutaway * part.OP_Weight;
					putawayWeight = Constants.Weight.Convert(putawayWeight, part.OP_WeightUQ, Location.WLV_MaxWeightUnit);
				}
				return putawayWeight;
			}

			ZDecimal GetWeight(PalletGroup pallet)
			{
				ZDecimal palletWeight = 0m;

				if (IsLocationsMaxWeightSpecified)
				{
					foreach (var putawayLine in pallet.PutawayList)
					{
						palletWeight += GetWeight(putawayLine);
					}
				}

				return palletWeight;
			}

			bool IsLocationsMaxWeightSpecified
			{
				get { return !Location.WLV_MaxWeightUnit.IsEmpty && Location.WLV_MaxWeight > 0m; }
			}

			#endregion

			#region Quantity

			void RefreshFreeQuantityOnAddPutawayLine(ILineToPutaway lineToPutaway)
			{
				if (FreeQuantity > 0)
				{
					FreeQuantity -= GetQuantity(lineToPutaway);
				}
				if (FreeQuantity <= 0)
				{
					IsLocationFull = true;
				}
			}

			ZDecimal GetQuantity(ILineToPutaway lineToPutaway)
			{
				return lineToPutaway.QuantityToPutaway;
			}

			ZDecimal GetQuantity(PalletGroup pallet)
			{
				ZDecimal palletQuantity = 0m;

				if (IsLocationMaxQuantitySpecified)
				{
					foreach (var putawayLine in pallet.PutawayList)
					{
						palletQuantity += GetQuantity(putawayLine);
					}
				}

				return palletQuantity;
			}

			bool IsLocationMaxQuantitySpecified
			{
				get { return Location.WLV_MaxQuantity > 0m; }
			}

			#endregion

			#endregion
		}

		#endregion

		#region PalletGroup

		class PalletGroup
		{
			#region ctor

			public PalletGroup()
			{
				putawayList = new List<ILineToPutaway>();
			}

			readonly List<ILineToPutaway> putawayList;

			#endregion

			#region Public

			public IList<ILineToPutaway> PutawayList
			{
				get { return putawayList; }
			}

			public void AddLineToPutaway(ILineToPutaway lineToPutaway)
			{
				SetCommonLocation(lineToPutaway);
				PutawayList.Add(lineToPutaway);
			}

			public ZGuid LocationPK
			{
				get { return PutawayList.Count > 0 ? PutawayList[0].LocationPK : ZGuid.Empty; }
			}

			public OrgSupplierPart Product
			{
				get { return OrgPartGuid.IsEmpty ? null : PutawayList[0].Product; }
			}

			public ZGuid OrgPartGuid
			{
				get
				{
					ZGuid result = ZGuid.Empty;
					if (PutawayList.Count != 0)
					{
						result = PutawayList[0].ProductPK;
						for (int i = 1; i < PutawayList.Count; i++)
						{
							if (PutawayList[i].ProductPK != result)
							{
								result = ZGuid.Empty;
								break;
							}
						}
					}

					return result;
				}
			}

			public ZString PalletID
			{
				get { return PutawayList.Count > 0 ? PutawayList[0].PalletID : ZString.Empty; }
			}

			#endregion

			#region SetCommonLocation

			void SetCommonLocation(ILineToPutaway lineToPutaway)
			{
				if (PutawayList.Count == 0)
				{
					SetLocationToExistingPalletID(lineToPutaway);
				}
				else
				{
					var existingLocation = PutawayList[0].LocationPK;
					if (!existingLocation.IsEmpty)
					{
						lineToPutaway.LocationPK = existingLocation;
					}
					else if (!lineToPutaway.LocationPK.IsEmpty)
					{
						foreach (var otherLines in PutawayList)
						{
							otherLines.LocationPK = lineToPutaway.LocationPK;
						}
					}
				}
			}

			static void SetLocationToExistingPalletID(ILineToPutaway lineToPutaway)
			{
				if (lineToPutaway.CheckPalletIDExists)
				{
					// Check if the pallet id is not already assigned to a location
					var query = new ZQuery(WhsInventoryViewSchema.WI_PalletID, SQLComparisonOperator.NotEqual, ZString.Empty);
					query.AddToFilter(WhsInventoryViewSchema.WI_PalletID, lineToPutaway.PalletID);
					query.AddToFilter(WhsInventoryViewSchema.WI_WL, SQLComparisonOperator.NotEqual, null);
					query.AddToFilter(WhsInventoryViewSchema.WI_WD, SQLComparisonOperator.NotEqual, lineToPutaway.DocketPK);
					query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0);

					var inventoryUsingSamePalletID = lineToPutaway.Factory.Load<WhsInventoryView>(query);
					var whsPk = lineToPutaway.WarehousePK;

					foreach (var inventory in inventoryUsingSamePalletID)
					{
						if (whsPk.Equals(inventory.WI_WW_Whs))
						{
							lineToPutaway.LocationPK = inventory.WI_WL;
							break;
						}
					}
				}
			}

			#endregion
		}

		#endregion

		#region PalletizedPutawayCollection

		class PalletizedPutawayCollection
		{
			#region ctor

			public PalletizedPutawayCollection(int capacity)
			{
				pallets = new Dictionary<string, PalletGroup>(capacity);
				singleLines = new List<ILineToPutaway>(capacity);
			}

			#endregion

			#region Public

			public void AddLineToPutaway(ILineToPutaway lineToPutaway)
			{
				if (lineToPutaway.PalletID.IsEmpty)
				{
					singleLines.Add(lineToPutaway);
				}
				else
				{
					PalletGroup groupToAdd;
					if (!pallets.TryGetValue(lineToPutaway.PalletID, out groupToAdd))
					{
						groupToAdd = new PalletGroup();
						pallets[lineToPutaway.PalletID] = groupToAdd;
					}
					groupToAdd.AddLineToPutaway(lineToPutaway);
				}
			}

			public void AddLineIfPalletGroupExists(ILineToPutaway lineToPutaway)
			{
				if (!lineToPutaway.PalletID.IsEmpty)
				{
					if (pallets.TryGetValue(lineToPutaway.PalletID, out var groupToAdd))
					{
						groupToAdd.AddLineToPutaway(lineToPutaway);
					}
				}
			}

			public IEnumerable<PalletGroup> Pallets
			{
				get { return pallets.Values; }
			}

			public IEnumerable<ILineToPutaway> SingleLines
			{
				get { return singleLines; }
			}

			public int Count
			{
				get { return GetPalletsRefreshed().Count + GetSingleLinesRefreshed().Count; }
			}

			#endregion

			#region Implementation

			#region pallets

			Dictionary<string, PalletGroup> pallets;

			Dictionary<string, PalletGroup> GetPalletsRefreshed()
			{
				if (pallets.Count > 0 && SomePalletWasAllocated())
				{
					var unallocatedPallets = new Dictionary<string, PalletGroup>(pallets.Count);
					foreach (var pair in pallets)
					{
						if (pair.Value.LocationPK.IsEmpty)
						{
							unallocatedPallets[pair.Key] = pair.Value;
						}
					}
					pallets = unallocatedPallets;
				}

				return pallets;
			}

			bool SomePalletWasAllocated()
			{
				foreach (var pallet in pallets.Values)
				{
					if (!pallet.LocationPK.IsEmpty)
					{
						return true;
					}
				}

				return false;
			}

			#endregion

			#region singleLines

			List<ILineToPutaway> singleLines;

			List<ILineToPutaway> GetSingleLinesRefreshed()
			{
				if (singleLines.Count > 0)
				{
					var unallocatedLines = new List<ILineToPutaway>(singleLines.Count);
					foreach (var line in singleLines)
					{
						if (line.LocationPK.IsEmpty)
						{
							unallocatedLines.Add(line);
						}
					}
					singleLines = unallocatedLines;
				}

				return singleLines;
			}

			#endregion

			#endregion
		}

		#endregion

		#region LocationSorter

		class LocationSorter : IComparer<WhsLocation>
		{
			public LocationSorter(OrgHeader client)
			{
				Client = Argument.NotNull(client, nameof(client));
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in LocationSorter")]
			OrgHeader Client { get; }

			#region IComparer<WhsLocation> Members

			int IComparer<WhsLocation>.Compare(WhsLocation x, WhsLocation y)
			{
				foreach (var comparison in Comparisons)
				{
					var result = comparison(x, y);
					if (result != 0)
					{
						return result;
					}
				}

				return 0;
			}

			int CompareByRow(WhsLocation x, WhsLocation y)
			{
				return x.Row.WR_Name.CompareTo(y.Row.WR_Name);
			}

			int CompareByColumn(WhsLocation x, WhsLocation y)
			{
				return x.WLV_Column.CompareTo(y.WLV_Column);
			}

			int CompareByLevel(WhsLocation x, WhsLocation y)
			{
				return x.WLV_Level.CompareTo(y.WLV_Level);
			}

			int CompareByTray(WhsLocation x, WhsLocation y)
			{
				return x.WLV_Tray.CompareTo(y.WLV_Tray);
			}

			Comparison<WhsLocation>[] Comparisons
			{
				get
				{
					if (comparisons == null)
					{
						var comparisonsList = new[]
						{
							new { Comparison = (Comparison<WhsLocation>)CompareByRow, Order = 1 },
							new { Comparison = (Comparison<WhsLocation>)CompareByColumn, Order = 2 },
							new { Comparison = (Comparison<WhsLocation>)CompareByLevel, Order = 3 },
							new { Comparison = (Comparison<WhsLocation>)CompareByTray, Order = 7 }
						};

						comparisons = comparisonsList
							.Where(c => c.Order > 0)
							.OrderBy(c => c.Order)
							.Select(c => c.Comparison)
							.ToArray();
					}

					return comparisons;
				}
			}

			Comparison<WhsLocation>[] comparisons;

			#endregion
		}

		#endregion
	}
}
