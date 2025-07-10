using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module
{
	public class LocationFilterBusinessObject : FilterStripBusinessObject
	{
		#region ModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			// warehouse
			var warehouseFilter = filters.AddGuidFilter(WhsLocationCollection.FilterSchema.Warehouse, ModuleIDs.WhsConfigWarehouse, GetWarehouseQuery, () => GetWarehouses());
			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("1e01cdb5-599d-4d0c-b8ff-64dab037194a", "Warehouse");
			warehouseFilter.PropertyValidation += ValidateWarehouse;
			warehouseFilter.Visibility = FilterVisibility.AlwaysVisible;

			// empty / in-use locations
			var textEmptyStatusfilter = filters.AddTextFilter(WhsLocationCollection.FilterSchema.LocationEmptyStatus, GetEmptyLocationQuery, LocationEmptyStatus);
			textEmptyStatusfilter.DefaultProperty = EmptyLocationFilterOption.All;
			textEmptyStatusfilter.MultilingualDescription = ResString.GetMultilingualString("8e470b23-2b0c-4e02-a0c3-07a90367b7a5", "Location Empty Status");

			// DockDoor Locations Filter
			var dockDoorLocationFilter = filters.AddFlagsFilter(WhsLocationCollection.FilterSchema.DockDoorLocation, new string[] { Res.GetString("0976ba05-e132-40dd-8dba-027a495bfcdd", "Dock Door Locations") }, new GetFlagsQuery[] { GetDockDoorLocationsQuery });
			dockDoorLocationFilter.MultilingualDescription = ResString.GetMultilingualString("f6ee4049-cd95-4800-80dc-a8e7be6963bc", "Dock Door Locations");

			var packingStationLocationFilter = filters.AddFlagsFilter(WhsLocationCollection.FilterSchema.PackingStationLocation, new string[] { Res.GetString("58a9a700-09f2-423a-a369-57e08db1ece2", "Packing Station Locations") }, new GetFlagsQuery[] { GetPackingStationLocationsQuery });
			packingStationLocationFilter.MultilingualDescription = ResString.GetMultilingualString("9b8c59fa-bf82-4fa9-a4a6-23e9fd97fd97", "Packing Station Locations");

			// Bonded Locations Filter
			var bondedLocationFilter = filters.AddFlagsFilter(WhsLocationCollection.FilterSchema.BondedLocation, new string[] { Res.GetString("17B73494-3C2A-4798-A3E8-7A8CC5A0923E", "Bonded Pick Locations") }, new GetFlagsQuery[] { GetBondedLocationsQuery });
			bondedLocationFilter.MultilingualDescription = ResString.GetMultilingualString("15103CD7-21E2-40F6-BE03-A9572AEAC1FE", "Bonded Pick Locations");

			var lastInventoryChangeDateFilter = filters.AddDateFilter(WhsLocationCollection.FilterSchema.LastInventoryChangeDate, WhsLocationViewSchema.WLV_LastInventoryChangeDate);
			lastInventoryChangeDateFilter.MultilingualDescription = ResString.GetMultilingualString("b255ea9e-6a0f-4309-a4e8-08699e508609", "Last Inventory Changed Date");

			// location status
			var textStatusFilter = filters.AddTextFilter(WhsLocationCollection.FilterSchema.LocationStatus, WhsLocationViewSchema.WLV_LocationStatus, new LocationStatus());
			textStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			textStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			textStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			textStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			textStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			textStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			textStatusFilter.MultilingualDescription = ResString.GetMultilingualString("LocationFilterBusinessObject|LocationStatus", "Location Status");
			textStatusFilter.DefaultProperty = LocationStatus.Codes.Normal;
			textStatusFilter.Visibility = FilterVisibility.AlwaysVisible;

			AddPickSequenceAndPutawaySequenceFilters(filters);
			AddCycleCountFilters(filters);

			// Area / Row / Level / Column / Tray
			AddAreaFilters(filters);

			var rowFilter = filters.AddGuidFilter(WhsLocationCollection.FilterSchema.Row, ModuleIDs.WhsConfigRow, WhsLocationViewSchema.WLV_WR, GetRows);
			rowFilter.Category = FilterCategories.NumbersAndReferences;
			rowFilter.MultilingualDescription = ResString.GetMultilingualString("6725b31a-020a-4b7f-b7e3-3bb28369b5d2", "Row");

			filters.AddNumberRangeFilter(WhsLocationCollection.FilterSchema.Column, WhsLocationViewSchema.WLV_Column).MultilingualDescription = ResString.GetMultilingualString("9d9bab50-6637-4859-8d7f-33a430c3467b", "Column");
			filters.AddNumberRangeFilter(WhsLocationCollection.FilterSchema.Level, WhsLocationViewSchema.WLV_Level).MultilingualDescription = ResString.GetMultilingualString("7665ddb2-5c2e-49a3-9e24-82e1fd846277", "Level");
			filters.AddNumberRangeFilter(WhsLocationCollection.FilterSchema.Tray, WhsLocationViewSchema.WLV_Tray).MultilingualDescription = ResString.GetMultilingualString("cb8fdc29-7159-4c21-9503-24c617615fe4", "Tray");

			return filters;
		}

		void AddAreaFilters(ModuleFilterCollection filters)
		{
			var pickingAreaFilter = filters.AddGuidFilter(
				WhsLocationCollection.FilterSchema.PickingArea, // Temporary constant will be removed by a later work item
				ModuleIDs.WhsConfigArea, WhsLocationViewSchema.WLV_WA_PickingArea, GetPickingAreas);
			pickingAreaFilter.Category = FilterCategories.NumbersAndReferences;
			pickingAreaFilter.MultilingualDescription =
				ResString.GetMultilingualString("LocationFilterBusinessObject|PickArea", "Pick Area");

			var putawayAreaFilter = filters.AddGuidFilter(WhsLocationCollection.FilterSchema.PutawayArea,
				ModuleIDs.WhsConfigArea, WhsLocationViewSchema.WLV_WA_PutawayArea, GetPutawayAreas);
			putawayAreaFilter.Category = FilterCategories.NumbersAndReferences;
			putawayAreaFilter.MultilingualDescription =
				ResString.GetMultilingualString("LocationFilterBusinessObject|PutawayArea", "Putaway Area");
		}

		ZQuery GetDockDoorLocationsQuery(ZBool value)
		{
			return GetLocationClassQuery(LocationClasses.Codes.DDL, value);
		}

		ZQuery GetPackingStationLocationsQuery(ZBool value)
		{
			return GetLocationClassQuery(LocationClasses.Codes.PST, value);
		}

		ZQuery GetLocationClassQuery(string locationClass, ZBool value)
		{
			var comparisonOperator = value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;

			var locationTypeSubQuery = new ZDBOnlySubQuery(typeof(WhsLocationType), WhsLocationViewSchema.WLV_WLT_LocationType);
			locationTypeSubQuery.AddToFilter(WhsLocationTypeSchema.WLT_LocationClass, comparisonOperator, locationClass);

			var result = new ZDBOnlyQuery(typeof(WhsLocation));
			result.AddSubQuery(locationTypeSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetBondedLocationsQuery(ZBool value)
		{
			var comparisonOperator = value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;

			var locationPickAreaTypeSubQuery = new ZDBOnlySubQuery(typeof(WhsArea), WhsLocationViewSchema.WLV_WA_PickingArea);
			locationPickAreaTypeSubQuery.AddToFilter(WhsAreaSchema.WA_AreaType, comparisonOperator, AreaTypes.Codes.Bonded);

			var result = new ZDBOnlyQuery(typeof(WhsLocation));
			result.AddSubQuery(locationPickAreaTypeSubQuery, JoinCondition.And);

			return result;
		}

		void ValidateWarehouse(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("8cefc742-86db-414b-908f-9446da62ab78", "Enter a Warehouse."));
			}
		}

		#endregion

		#region EmptyLocationQuery

		protected ZQuery GetEmptyLocationQuery(ZString text)
		{
			ZQuery result;

			if (text == EmptyLocationFilterOption.All)
			{
				result = new ZQuery();
			}
			else
			{
				result = new ZDBOnlyQuery(typeof(WhsLocation));
				ZDBOnlySubQuery inventoryQuery;

				if (text == EmptyLocationFilterOption.Empty)
				{
					inventoryQuery = new ZDBOnlySubQuery(typeof(IWhsInventoryView), WhsInventoryViewSchema.WI_WL, true); // NOT IN
					inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_WL, SQLComparisonOperator.NotEqual, DBNull.Value);
				}
				else // LocationStatus.InUse
				{
					inventoryQuery = new ZDBOnlySubQuery(typeof(IWhsInventoryView), WhsInventoryViewSchema.WI_WL); // IN
				}

				inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);
				((ZDBOnlyQuery)result).AddSubQuery(inventoryQuery, JoinCondition.And);
			}

			return result;
		}

		CodeDescriptionPairList LocationEmptyStatus
		{
			get
			{
				if (locationEmptyStatus == null)
				{
					locationEmptyStatus = new CodeDescriptionPairList();
					locationEmptyStatus.AddPair(EmptyLocationFilterOption.All, Res.GetString("2f9909ce-c2a5-427a-bc71-cff3cbd34831", "All Locations"));
					locationEmptyStatus.AddPair(EmptyLocationFilterOption.Empty, Res.GetString("df478a6a-7e30-4f34-93c8-1dadfd8376ae", "Empty Locations"));
					locationEmptyStatus.AddPair(EmptyLocationFilterOption.InUse, Res.GetString("68b87893-b2b1-4024-b48c-30243d912c1c", "Non-Empty Locations"));
				}
				return locationEmptyStatus;
			}
		}

		CodeDescriptionPairList locationEmptyStatus;

		public static class EmptyLocationFilterOption
		{
			public const string All = "ALL";
			public const string Empty = "EMP";
			public const string InUse = "USE";
		}

		#endregion

		#region WarehouseQuery

		protected ZQuery GetWarehouseQuery(ZGuid warehousePK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(WhsLocation));
			ZDBOnlySubQuery rowQuery = new ZDBOnlySubQuery(typeof(WhsRow), WhsLocationViewSchema.WLV_WR);
			rowQuery.AddToFilter(WhsRowSchema.WR_WW_Whs, warehousePK);
			result.AddSubQuery(rowQuery, JoinCondition.And);

			return result;
		}

		protected override void SetExternalDefaultsCore(FilterBusinessObjectDefault filterDefault, IEnumerable<ZString> skippedOrCategory = null)
		{
			if (filterDefault.FilterName == WhsLocationCollection.FilterSchema.Warehouse)
			{
				warehouseCollectionType = GetWarehouseCollectionType((ZGuid)filterDefault.Value);
			}

			base.SetExternalDefaultsCore(filterDefault, skippedOrCategory);
		}

		WarehouseCollectionType GetWarehouseCollectionType(ZGuid warehousePK)
		{
			if (!warehousePK.IsEmpty)
			{
				var warehouse = Factory.Load<WhsWarehouse>(warehousePK);
				if (warehouse != null && warehouse.WW_WarehouseType == WarehouseTypes.Codes.Transit)
				{
					return WarehouseCollectionType.TransitWarehouse;
				}
			}

			return WarehouseCollectionType.ProductWarehouse;
		}

		WarehouseCollectionType warehouseCollectionType = WarehouseCollectionType.ProductWarehouse;

		WhsWarehouseCollectionWithSecurityCheck GetWarehouses()
		{
			return new WhsWarehouseCollectionWithSecurityCheck(Factory, warehouseCollectionType);
		}

		#endregion

		#region Areas + Rows

		WhsAreaCollection GetPickingAreas()
		{
			return WhsAreaCollection.GetPickingAreas(Factory, WarehouseFilter.Property);
		}

		WhsAreaCollection GetPutawayAreas()
		{
			return WhsAreaCollection.GetPutawayAreas(Factory, WarehouseFilter.Property);
		}

		WhsRowCollection GetRows()
		{
			var selectedWarehouse = GetSelectedWarehouse();
			return new WhsRowCollection(selectedWarehouse, Factory);
		}

		WhsWarehouse GetSelectedWarehouse()
		{
			var warehouseFilter = WarehouseFilter;
			return Factory.Load<WhsWarehouse>(warehouseFilter.Property);
		}

		#endregion

		#region PickAndPutawaySequenceFilters

		void AddPickSequenceAndPutawaySequenceFilters(ModuleFilterCollection filters)
		{
			var pickPathSequenceFilter = filters.AddNumberRangeFilter(WhsLocationCollection.FilterSchema.PickPathSequence, WhsLocationViewSchema.WLV_PickPathSequence);
			pickPathSequenceFilter.MultilingualDescription = ResString.GetMultilingualString("8E3A534B-8496-4B90-99BF-9D296DBA5E47", "Pick Path Sequence");

			var putawayPathSequenceFilter = filters.AddNumberRangeFilter(WhsLocationCollection.FilterSchema.PutawayPathSequence, WhsLocationViewSchema.WLV_PutawayPathSequence);
			putawayPathSequenceFilter.MultilingualDescription = ResString.GetMultilingualString("C22BA49E-0259-4CE5-950B-6210C4975CDB", "Putaway Path Sequence");
		}

		#endregion

		#region Cycle Count

		void AddCycleCountFilters(ModuleFilterCollection filters)
		{
			var cycleCountLastPerformedFilter = filters.AddDateFilter(WhsLocationCollection.FilterSchema.CycleCountLastPerformed, WhsLocationViewSchema.WLV_CycleCountLastPerformed);
			cycleCountLastPerformedFilter.MultilingualDescription = ResString.GetMultilingualString("CE7F8F3C-6D1D-48E9-8CE6-BA1987FAA5C2", "Cycle Count Last Performed Date");

			var cycleCountPathSequenceFilter = filters.AddNumberRangeFilter(WhsLocationCollection.FilterSchema.CycleCountPathSequence, WhsLocationViewSchema.WLV_CycleCountPathSequence);
			cycleCountPathSequenceFilter.MultilingualDescription = ResString.GetMultilingualString("C577A8B1-01B3-4190-949B-117E34737C9A", "Cycle Count Path Sequence");
		}

		#endregion

		#region SetInitialCodeForSearch

		public override void SetInitialCodeForSearch(ZString code, Type typeOfElementsToFind)
		{
			base.SetInitialCodeForSearch(code, typeOfElementsToFind);
			SetInitialCodeForSearch(code);
		}

		protected override void SetInitialCodeForSearchCore(ZString code, string propertyName)
		{
			base.SetInitialCodeForSearchCore(code, propertyName);
			SetInitialCodeForSearch(code);
		}

		void SetInitialCodeForSearch(ZString code)
		{
			if (!code.IsEmpty)
			{
				SetInitialCodeForSearchCore(code);
			}
		}

		void SetInitialCodeForSearchCore(ZString code)
		{
			if (WarehouseFilter != null)
			{
				var warehouse = Factory.Load<WhsWarehouse>(WarehouseFilter.Property);
				if (warehouse != null)
				{
					var parts = warehouse.FindLocationParts(code);
					if (parts != null)
					{
						if (parts.Row != null && RowFilter != null)
						{
							RowFilter.Property = parts.Row.PK;
							RowFilter.Visibility = FilterVisibility.AlwaysVisible;
						}

						if (parts.Column.HasValue && ColumnFilter != null)
						{
							ColumnFilter.Property1 = parts.Column.Value;
							ColumnFilter.Property2 = parts.Column.Value;
							ColumnFilter.Visibility = FilterVisibility.AlwaysVisible;
						}

						if (parts.Level.HasValue && LevelFilter != null)
						{
							LevelFilter.Property1 = parts.Level.Value;
							LevelFilter.Property2 = parts.Level.Value;
							LevelFilter.Visibility = FilterVisibility.AlwaysVisible;
						}

						if (parts.Tray.HasValue && TrayFilter != null)
						{
							TrayFilter.Property1 = parts.Tray.Value;
							TrayFilter.Property2 = parts.Tray.Value;
							TrayFilter.Visibility = FilterVisibility.AlwaysVisible;
						}
					}
				}
			}
		}

		ModuleGuidFilter WarehouseFilter => (ModuleGuidFilter)ModuleFilters[WhsLocationCollection.FilterSchema.Warehouse];

		ModuleGuidFilter RowFilter => (ModuleGuidFilter)ModuleFilters[WhsLocationCollection.FilterSchema.Row];

		ModuleNumberRangeFilter ColumnFilter => (ModuleNumberRangeFilter)ModuleFilters[WhsLocationCollection.FilterSchema.Column];

		ModuleNumberRangeFilter LevelFilter => (ModuleNumberRangeFilter)ModuleFilters[WhsLocationCollection.FilterSchema.Level];

		ModuleNumberRangeFilter TrayFilter => (ModuleNumberRangeFilter)ModuleFilters[WhsLocationCollection.FilterSchema.Tray];

		#endregion
	}
}
