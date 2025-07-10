using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class InventoryFilterBusinessObject : FilterStripBusinessObject
	{
		#region Schema

		public static class Schema
		{
			public const string Status = "Status"; // Filter description
			public const string HeldCode = "HeldCode"; // Filter description
			public const string Client = "Client"; // Filter description
			public const string Column = "Column"; // Filter description
			public const string Level = "Level"; // Filter description
			public const string Tray = "Tray"; // Filter description
			public const string LastTouchedDate = "LastTouchedDate"; // Filter description
			public const string Product = "Product"; // Filter description
			public const string ProductStyle = "Product Style"; // Filter description
			public const string ColourCode = "Colour Code"; // Filter description
			public const string ClassificationCode = "Classification Code"; // Filter description
			public const string StyleSize = "Style Size"; // Filter description
			public const string Commodity = "Commodity"; // Filter description
			public const string ProductCategory = "Product Category"; // Filter description
			public const string Warehouse = "Warehouse"; // Filter description
			public const string LocationStatus = "LocationStatus"; // Filter description
			public const string LocationType = "LocationType"; // Filter description
			public const string LocationClass = "LocationClass"; // Filter description
			public const string PickMethod = "PickMethod"; // Filter description
			public const string TouchesTillStocktake = "TouchesTillStocktake"; // Filter description
			public const string WeightMax = "WeightMax"; // Filter description
			public const string VolumeMax = "VolumeMax"; // Filter description
			public const string QuantityMax = "QuantityMax"; // Filter description
			public const string WeightUQ = "WeightUQ"; // Filter description
			public const string VolumeUQ = "VolumeUQ"; // Filter description
			public const string ArrivalDate = "Arrival Date"; // Filter description
			public const string ExpiryDate = "Expiry Date"; // Filter description
			public const string PackingDate = "Packing Date"; // Filter description
			public const string ReceiveReference = "Receive Reference"; // Filter description
			public const string PalletID = "Pallet ID"; // Filter description
			public const string PackageID = "Package/Tote ID"; // Filter description
			public const string PickAreaName = "Pick Area Name"; // Filter description
			public const string PickAreaType = "Pick Area Type"; // Filter description
			public const string Row = "Row"; // Filter description
			public const string CustomsEntryKey = "Customs Entry Key"; // Filter description
			public const string CountryOfOrigin = "Country of Origin"; // Filter description
			public const string Manufacturer = "Manufacturer"; // Filter description
			public const string Committed = "Committed"; // Filter description
			public const string Location = "Location"; // Filter description
			public const string DeclarationReference = "Declaration Reference"; // Filter description
			public const string CustomsDeadline = "Customs Deadline"; // Filter description
			public const string InwardStyle = "Customs Inward Style"; // Filter description
			public const string InwardProcedure = "Customs Inward Procedure"; // Filter description
			public const string IsExpired = "Is Expired"; // Filter description
			public const string AllocationKey = "Allocation Key"; // Filter description
			public const string ComponentProduct = "Component Product"; // Filter description
			public const string ComponentPartAttrib1 = "Component Part Attribute 1"; // Filter description
			public const string ComponentPartAttrib2 = "Component Part Attribute 2"; // Filter description
			public const string ComponentPartAttrib3 = "Component Part Attribute 3"; // Filter description
			public const string ComponentSerial = "Component Serial Number"; // Filter description
			public const string ComponentPackingDate = "Component Packing Date"; // Filter description
			public const string ComponentExpiryDate = "Component Expiry Date"; // Filter description
			public const string ComponentEntryKey = "Component Inwards Entry Key"; // Filter description

			public static class IsExpiredTypeCodes
			{
				public const string Expired = "EXP"; // Filter Constant
				public const string NotExpired = "NOT"; // Filter Constant
				public const string All = "ALL"; // Filter Constant
			}
		}

		#endregion

		public InventoryFilterBusinessObject()
		{
		}

		public InventoryFilterBusinessObject(OrgHeader loggedInWebUsersOrg)
			: this()
		{
			this.loggedInWebUsersOrg = loggedInWebUsersOrg;
		}
		readonly OrgHeader loggedInWebUsersOrg;

		public InventoryFilterBusinessObject(WhsDocket docket)
			: this()
		{
			this.docket = docket;
			((IFilterStripBusinessObjectInternals)this).LayoutContext = ModuleIDs.WhsInventory.Name;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in InventoryFilterBusinessObject")]
		readonly WhsDocket docket;

		protected override void OnModuleFiltersCreated()
		{
			base.OnModuleFiltersCreated();
			var filters = GetAttributeFilters();
			foreach (ModuleFilter filter in filters)
			{
				ModuleFilters.AddFilter(filter);
			}
		}

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				var query = base.Filter;

				var inventoryToSeeQuery = new ZQuery();
				inventoryToSeeQuery.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);
				query.AddToFilter(inventoryToSeeQuery);
				return query;
			}
		}

		#endregion

		#region Filters

		#region GetModuleFiltersCore

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddStatusFilter(filters);
			AddClientFilter(filters);
			AddWarehouseFilter(filters);
			AddProductFilter(filters);
			AddProductCategoryFilter(filters);
			AddLocationFilter(filters);

			AddManufacturerFilter(filters);
			AddCountryOfOriginFilter(filters);

			AddProductStyleFilter(filters);
			AddColourCodeFilter(filters);
			AddClassificationCodeFilter(filters);
			AddStyleSizeFilter(filters);

			AddLastTouchedDateFilter(filters);
			AddPickAreaFilter(filters);
			AddPickAreaTypeFilter(filters);
			AddLocationClassFilter(filters);

			AddComponentFilter(filters);

			var rowFilter = filters.AddTextFilter(Schema.Row, delegate (SQLComparisonOperator comparisonOperator, ZString value)
			{ return GetLocationSubQuery(typeof(WhsRow), WhsLocationViewSchema.WLV_WR, WhsRowSchema.WR_Name, comparisonOperator, value); });
			rowFilter.MultilingualDescription = ResString.GetMultilingualString("b687553d-5bac-42bd-9365-159400e54192", "Row");
			rowFilter.MaxLength = WhsRowSchema.WR_Name.MaxLength;
			rowFilter.IsPublishedOnWeb = false;

			var columnFilter = filters.AddTextFilterForExactComparison(Schema.Column, delegate (SQLComparisonOperator comparisonOperator, ZString value)
			{ return GetLocationParserSubQuery(WhsLocationViewSchema.WLV_Column, WhsWarehouseSchema.WW_LocationColumnsAlpha, WhsWarehouseSchema.WW_LocationColumnsZeroBased, value); });
			columnFilter.MultilingualDescription = ResString.GetMultilingualString("0e55fd42-83e9-435d-8c1a-2f11b64e89d8", "Column");
			columnFilter.Category = FilterCategories.Locations;
			columnFilter.IsPublishedOnWeb = false;

			var levelFilter = filters.AddTextFilterForExactComparison(Schema.Level, delegate (SQLComparisonOperator comparisonOperator, ZString value)
			{ return GetLocationParserSubQuery(WhsLocationViewSchema.WLV_Level, WhsWarehouseSchema.WW_LocationLevelsAlpha, WhsWarehouseSchema.WW_LocationLevelsZeroBased, value); });
			levelFilter.MultilingualDescription = ResString.GetMultilingualString("889B393C-9D56-42AE-9345-2553A48C250A", "Level");
			levelFilter.Category = FilterCategories.Locations;
			levelFilter.IsPublishedOnWeb = false;

			var trayFilter = filters.AddTextFilterForExactComparison(Schema.Tray, delegate (SQLComparisonOperator comparisonOperator, ZString value)
			{ return GetLocationParserSubQuery(WhsLocationViewSchema.WLV_Tray, WhsWarehouseSchema.WW_LocationTraysAlpha, WhsWarehouseSchema.WW_LocationTraysZeroBased, value); });
			trayFilter.MultilingualDescription = ResString.GetMultilingualString("d4707453-ffee-4974-ac95-01c3d25cad7a", "Tray");
			trayFilter.Category = FilterCategories.Locations;
			trayFilter.IsPublishedOnWeb = false;

			var customsEntryFilter = filters.AddTextFilter(Schema.CustomsEntryKey, WhsInventoryViewSchema.WI_BondedEntryKey);
			customsEntryFilter.MultilingualDescription = ResString.GetMultilingualString("e641525c-9fbb-4a9b-8c9a-5f7894b2afd6", "Customs Entry Key");
			customsEntryFilter.Category = FilterCategories.TextSearch;
			customsEntryFilter.IsPublishedOnWeb = false;

			var receiveReferenceFilter = filters.AddTextFilter(Schema.ReceiveReference, ReceiptReferenceFilter);
			receiveReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("ff67c715-c517-4733-b675-621a325ce474", "Receive Reference");
			receiveReferenceFilter.MaxLength = WhsDocketSchema.WD_ExternalReference.MaxLength;

			filters.AddTextFilter(Schema.PalletID, WhsInventoryViewSchema.WI_PalletID).MultilingualDescription = ResString.GetMultilingualString("b7e69653-9aaa-4bc1-a4ae-28be67c68fb1", "Pallet ID");
			filters.AddTextFilter(Schema.AllocationKey, WhsInventoryViewSchema.WI_AllocationKey).MultilingualDescription = ResString.GetMultilingualString("ddfd0b95-b4bf-43a5-9d14-4561c79504ea", "Allocation Key");

			var packageIDFilter = filters.AddTextFilter(Schema.PackageID, PackageIDFilter);
			packageIDFilter.MultilingualDescription = ResString.GetMultilingualString("e51f003a-3e60-4e8c-bc79-89f779024ea9", "Package/Tote ID");
			packageIDFilter.SubGroup = DocketLineSubGroup;
			packageIDFilter.MaxLength = PkgPackageHeaderSchema.KPH_PackageID.MaxLength;
			packageIDFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			packageIDFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			filters.AddNkFilter(Schema.Commodity, GetCommodityCodeQuery, ModuleIDs.RefCommodityCode, CommodityCodes).MultilingualDescription = ResString.GetMultilingualString("83f7c1b2-c285-4778-acf2-f03fa74ab4b7", "Commodity");

			filters.AddDateFilter(Schema.ArrivalDate, WhsInventoryViewSchema.WI_ArrivalDate).MultilingualDescription = ResString.GetMultilingualString("569897ef-5219-4390-bf14-0743544208ae", "Arrival Date");
			filters.AddDateFilter(Schema.ExpiryDate, WhsInventoryViewSchema.WI_ExpiryDate).MultilingualDescription = ResString.GetMultilingualString("13e996c0-6ab7-4c51-aa88-d29162bd9fcf", "Expiry Date");
			filters.AddDateFilter(Schema.PackingDate, WhsInventoryViewSchema.WI_PackingDate).MultilingualDescription = ResString.GetMultilingualString("b99a1f76-b0da-486a-ba25-60d704cdc1c3", "Packing Date");
			AddLocationNumericFilterFromColumn(filters, Schema.WeightMax, WhsLocationViewSchema.WLV_MaxWeight, ResString.GetMultilingualString("InventoryFilterBusinessObject|WeightMax", "Weight (Max)"));
			AddLocationNumericFilterFromColumn(filters, Schema.VolumeMax, WhsLocationViewSchema.WLV_MaxCubic, ResString.GetMultilingualString("InventoryFilterBusinessObject|VolumeMax", "Volume (Max)"));
			AddLocationNumericFilterFromColumn(filters, Schema.QuantityMax, WhsLocationViewSchema.WLV_MaxQuantity, ResString.GetMultilingualString("InventoryFilterBusinessObject|QuantityMax", "Quantity (Max)"));
			AddLocationStockTakeFilter(filters, Schema.TouchesTillStocktake, ResString.GetMultilingualString("InventoryFilterBusinessObject|TouchesTillStocktake", "Remaining Touch Count until Stocktake Required"));
			AddLocationSubFilter(filters, Schema.LocationStatus, WhsLocationViewSchema.WLV_LocationStatus, new LocationStatus(), ResString.GetMultilingualString("InventoryFilterBusinessObject|LocationStatus", "Location Status"));
			AddLocationSubFilter(filters, Schema.PickMethod, WhsLocationViewSchema.WLV_PickMethod, WarehouseDataRegistry.Instance.PickMethod.Value, ResString.GetMultilingualString("InventoryFilterBusinessObject|PickMethod", "Pick Method"));
			AddLocationSubFilter(filters, Schema.WeightUQ, WhsLocationViewSchema.WLV_MaxWeightUnit, new CodeDescriptionPairList(OLookUpEditType.Weight), ResString.GetMultilingualString("InventoryFilterBusinessObject|WeightUQ", "Weight UQ"));
			AddLocationSubFilter(filters, Schema.VolumeUQ, WhsLocationViewSchema.WLV_MaxCubicUnit, new CodeDescriptionPairList(OLookUpEditType.Volume), ResString.GetMultilingualString("InventoryFilterBusinessObject|VolumeUQ", "Volume UQ"));

			var locationTypeFilter = filters.AddGuidFilter(Schema.LocationType, ModuleIDs.WhsConfigLocationType, GetLocationTypeFilter, GetLocationTypes);
			locationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("InventoryFilterBusinessObject|LocationType", "Location Type");
			locationTypeFilter.IsPublishedOnWeb = false;

			var filter = filters.AddTextFilter(Schema.Committed, GetCommittedQuery, CommittedTypes);
			filter.MultilingualDescription = ResString.GetMultilingualString("6a41c62b-2ef9-4cf5-b621-0c6e1a5c5d74", "Committed");
			filter.Category = FilterCategories.StatusAndFlags;

			var heldCodeFilter = filters.AddTextFilter(Schema.HeldCode, WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, new WhsInventoryHeldCodeCollection(Factory));
			heldCodeFilter.Category = FilterCategories.StatusAndFlags;
			heldCodeFilter.MultilingualDescription = ResString.GetMultilingualString("6ff96fa1-c02d-4bdb-87cb-489a8c3ca91a", "Hold Code");
			heldCodeFilter.SubGroup = DocketLineSubGroup;
			heldCodeFilter.IsPublishedOnWeb = false;

			var isExpiredFilter = filters.AddTextFilter(Schema.IsExpired, GetIsExpiredQuery, IsExpiredTypes);
			isExpiredFilter.MultilingualDescription = ResString.GetMultilingualString("InventoryFilterBusinessObject|IsExpired", "Is Expired");
			isExpiredFilter.Category = FilterCategories.StatusAndFlags;

			AddWBSimpleQuerys(filters);

			return filters;
		}

		#endregion

		#region AddPickAreaFilter

		void AddPickAreaFilter(ModuleFilterCollection filters)
		{
			var areaFilter = filters.AddTextFilter(Schema.PickAreaName, delegate (SQLComparisonOperator comparisonOperator, ZString value)
			{ return GetLocationSubQuery(typeof(WhsArea), WhsLocationViewSchema.WLV_WA_PickingArea, WhsAreaSchema.WA_Name, comparisonOperator, value); });
			areaFilter.MultilingualDescription = ResString.GetMultilingualString("InventoryFilterBusinessObject|PickArea", "Pick Area Name");
			areaFilter.Category = FilterCategories.Locations;
			areaFilter.IsPublishedOnWeb = false;
			areaFilter.MaxLength = WhsAreaSchema.WA_Name.MaxLength;
		}

		#endregion

		#region AddPickAreaTypeFilter

		void AddPickAreaTypeFilter(ModuleFilterCollection filters)
		{
			var list = new AreaTypes();
			var areaFilter = filters.AddTextFilter(Schema.PickAreaType, delegate (ZString value)
			{ return GetLocationSubQuery(typeof(WhsArea), WhsLocationViewSchema.WLV_WA_PickingArea, WhsAreaSchema.WA_AreaType, SQLComparisonOperator.Equal, value); }, list);
			areaFilter.MultilingualDescription = ResString.GetMultilingualString("InventoryFilterBusinessObject|PickAreaType", "Pick Area Type");
			areaFilter.Category = FilterCategories.Locations;
			areaFilter.IsPublishedOnWeb = false;
			areaFilter.MaxLength = WhsAreaSchema.WA_AreaType.MaxLength;
		}

		#endregion

		#region AddLastTouchedDateFilter

		void AddLastTouchedDateFilter(ModuleFilterCollection filters)
		{
			var lastTouchedDateFilter = filters.AddDateFilter(Schema.LastTouchedDate, WhsLocationViewSchema.WLV_LastInventoryChangeDate);
			lastTouchedDateFilter.MultilingualDescription = ResString.GetMultilingualString("InventoryFilterBusinessObject|LastTouchedDate", "Date Last Touched");
			lastTouchedDateFilter.Category = FilterCategories.Locations;
			lastTouchedDateFilter.SubGroup = LocationSubGroup;
			lastTouchedDateFilter.IsPublishedOnWeb = false;
		}

		#endregion

		#region AddLocationClassFilter

		void AddLocationClassFilter(ModuleFilterCollection filters)
		{
			var locationClassFilter = filters.AddTextFilter(Schema.LocationClass, delegate (SQLComparisonOperator comparisonOperator, ZString value)
			{ return GetLocationSubQuery(typeof(WhsLocationType), WhsLocationViewSchema.WLV_WLT_LocationType, WhsLocationTypeSchema.WLT_LocationClass, comparisonOperator, value); }, new LocationClasses());
			locationClassFilter.MultilingualDescription = ResString.GetMultilingualString("18F29711-A03D-4396-86AE-570EC369B3EB", "Location Class");
			locationClassFilter.Category = FilterCategories.Locations;
			locationClassFilter.MaxLength = WhsLocationTypeSchema.WLT_LocationClass.MaxLength;
			locationClassFilter.IsPublishedOnWeb = false;
			locationClassFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			locationClassFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			locationClassFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			locationClassFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			locationClassFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			locationClassFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
		}

		#endregion

		#region AddComponentFilter

		void AddComponentFilter(ModuleFilterCollection filters)
		{
			var componentFilter = filters.AddGuidFilter(Schema.ComponentProduct, ModuleIDs.WhsConfigProduct, WhsDocketLineSchema.WE_OP, GetProducts);
			componentFilter.MultilingualDescription = ResString.GetMultilingualString("e3cd94ed-9295-4a04-b51a-3a220506d764", "Component Product");
			componentFilter.PropertyValidation = ComponentProductFilterValidation;
			componentFilter.SubGroup = ComponentSubGroup;

			var partAttrib1Filter = filters.AddTextFilter(Schema.ComponentPartAttrib1, WhsDocketLineSchema.WE_PartAttrib1);
			partAttrib1Filter.Category = FilterCategories.AttributeSearch;
			partAttrib1Filter.MaxLength = WhsDocketLineSchema.WE_PartAttrib1.MaxLength;
			partAttrib1Filter.SubGroup = ComponentSubGroup;
			partAttrib1Filter.MultilingualDescription = ResString.GetMultilingualString("7a729c03-ee33-4e41-bdf8-79fcf6ceace7", "Component Part Attribute 1");

			var partAttrib2Filter = filters.AddTextFilter(Schema.ComponentPartAttrib2, WhsDocketLineSchema.WE_PartAttrib2);
			partAttrib2Filter.Category = FilterCategories.AttributeSearch;
			partAttrib2Filter.MaxLength = WhsDocketLineSchema.WE_PartAttrib2.MaxLength;
			partAttrib2Filter.SubGroup = ComponentSubGroup;
			partAttrib2Filter.MultilingualDescription = ResString.GetMultilingualString("da4858c7-f061-4a8b-977c-f4248e2addfa", "Component Part Attribute 2");

			var partAttrib3Filter = filters.AddTextFilter(Schema.ComponentPartAttrib3, WhsDocketLineSchema.WE_PartAttrib3);
			partAttrib3Filter.Category = FilterCategories.AttributeSearch;
			partAttrib3Filter.MaxLength = WhsDocketLineSchema.WE_PartAttrib3.MaxLength;
			partAttrib3Filter.SubGroup = ComponentSubGroup;
			partAttrib3Filter.MultilingualDescription = ResString.GetMultilingualString("84e8b7a5-b256-4100-bf9e-dbfb03969dd9", "Component Part Attribute 3");

			ModuleTextFilter serialFilter;
			if (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value)
			{
				serialFilter = filters.AddTextFilter(Schema.ComponentSerial, FilterByComponentSerialNumber);
				serialFilter.MaxLength = WhsSerialNumberSchema.WSN_SerialNumber.MaxLength;
			}
			else
			{
				serialFilter = filters.AddTextFilter(Schema.ComponentSerial, WhsDocketLineSchema.WE_SerialNumber);
				serialFilter.MaxLength = WhsDocketLineSchema.WE_SerialNumber.MaxLength;
				serialFilter.SubGroup = ComponentSubGroup;
			}
			serialFilter.Category = FilterCategories.AttributeSearch;
			serialFilter.MultilingualDescription = ResString.GetMultilingualString("e10b3304-e2f4-4a59-b3d1-4b939539a5a4", "Component Serial Number");

			var expiryFilter = filters.AddDateFilter(Schema.ComponentExpiryDate, WhsDocketLineSchema.WE_ExpiryDate);
			expiryFilter.Category = FilterCategories.AttributeSearch;
			expiryFilter.SubGroup = ComponentSubGroup;
			expiryFilter.MultilingualDescription = ResString.GetMultilingualString("399c7c78-cd66-4b5f-ac21-bbf79333e6a0", "Component Expiry Date");

			var packingFilter = filters.AddDateFilter(Schema.ComponentPackingDate, WhsDocketLineSchema.WE_PackingDate);
			packingFilter.Category = FilterCategories.AttributeSearch;
			packingFilter.SubGroup = ComponentSubGroup;
			packingFilter.MultilingualDescription = ResString.GetMultilingualString("77373e04-5ef8-4750-b409-57c737c83562", "Component Packing Date");

			var customsInwardsEntryKeyFilter = filters.AddTextFilter(Schema.ComponentEntryKey, WhsDocketLineSchema.WE_BondedEntryKey);
			customsInwardsEntryKeyFilter.Category = FilterCategories.AttributeSearch;
			customsInwardsEntryKeyFilter.MaxLength = WhsDocketLineSchema.WE_BondedEntryKey.MaxLength;
			customsInwardsEntryKeyFilter.SubGroup = ComponentSubGroup;
			customsInwardsEntryKeyFilter.MultilingualDescription = ResString.GetMultilingualString("d5fc68e5-732e-4bdb-8196-0ad786a0c5c9", "Component Inwards Entry Key");
		}

		protected ModuleFilterSubGroup ComponentSubGroup
		{
			get { return componentSubGroup ?? (componentSubGroup = new ComponentFiltersSubGroup()); }
		}
		ModuleFilterSubGroup componentSubGroup;

		static ZQuery GetInventoryQueryViaPickline(SchemaColumn fieldNameToQuery, ZDBOnlySubQuery subQueryToFilterPickLine)
		{
			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddSubQuery(fieldNameToQuery, subQueryToFilterPickLine, JoinCondition.And);

			var pivotSubQuery = new ZDBOnlySubQuery(typeof(WhsBOMInventoryPivot), WhsBOMInventoryPivotSchema.WIP_WE_InventoryLine);
			pivotSubQuery.AddSubQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, pickLineSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			query.AddSubQuery(WhsInventoryViewSchema.WI_WE_OriginalInDocketLineForRating, pivotSubQuery, JoinCondition.And);

			return query;
		}

		class ComponentFiltersSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var componentLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.PK);
				componentLineSubQuery.AddToFilter(filter);

				return GetInventoryQueryViaPickline(WhsPickLineSchema.WZ_WE_InventoryLine, componentLineSubQuery);
			}
		}

		#endregion

		#region DocketLineSubGroup

		protected ModuleFilterSubGroup DocketLineSubGroup
		{
			get { return docketLineSubGroup ?? (docketLineSubGroup = new DocketLineFilterSubGroup()); }
		}
		ModuleFilterSubGroup docketLineSubGroup;

		class DocketLineFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subFilter = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsInventoryViewSchema.WI_WE_InDocketLine);
				subFilter.AddToFilter(filter);

				var inventoryFilter = new ZDBOnlyQuery(typeof(WhsInventoryView));
				inventoryFilter.AddSubQuery(subFilter, JoinCondition.And);

				return inventoryFilter;
			}
		}

		#endregion

		#region GetIsExpiredQuery

		ZQuery GetIsExpiredQuery(ZString value)
		{
			var result = new ZQuery();

			if (value == Schema.IsExpiredTypeCodes.Expired)
			{
				result.AddToFilter(WhsInventoryViewSchema.WI_ExpiryDate, SQLComparisonOperator.LessThanOrEqualTo, ZDate.Today);
			}
			else if (value == Schema.IsExpiredTypeCodes.NotExpired)
			{
				result.AddToFilter(WhsInventoryViewSchema.WI_ExpiryDate, SQLComparisonOperator.GreaterThan, ZDate.Today);
				result.AddToFilter(JoinCondition.Or, WhsInventoryViewSchema.WI_ExpiryDate, SQLComparisonOperator.Equal, null);
			}

			return result;
		}

		#endregion

		#region GetCommittedQuery

		ZQuery GetCommittedQuery(ZString value)
		{
			ZQuery result;

			const bool IN = true;
			if (value == "COM")
			{
				var pickLineSubQuery = GetPickLineSubQuery(IN);

				result = new ZDBOnlyQuery(typeof(WhsInventoryView));
				((ZDBOnlyQuery)result).AddSubQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, pickLineSubQuery, JoinCondition.And);
			}
			else if (value == "UNC")
			{
				var pickLineSubQuery = GetPickLineSubQuery(!IN);

				result = new ZDBOnlyQuery(typeof(WhsInventoryView));
				((ZDBOnlyQuery)result).AddSubQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, pickLineSubQuery, JoinCondition.And);
			}
			else
			{
				result = new ZQuery();
			}

			return result;
		}

		ZDBOnlySubQuery GetPickLineSubQuery(bool iN)
		{
			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_InventoryLine, !iN);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_Units, GreaterThanComparisonOperator.GreaterThan, 0m);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_PickedDateTime, null);

			return pickLineSubQuery;
		}

		#endregion

		#region AddStatusFilter

		ModuleTextFilter AddStatusFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(Schema.Status, GetInventoryStatusQuery, InventoryStatuses);
			filter.MultilingualDescription = ResString.GetMultilingualString("F4D9A706-8C8C-4074-A0F0-3CDF18AD634C", "Status");
			filter.Category = FilterCategories.StatusAndFlags;
			return filter;
		}

		#region GetInventoryStatusQuery

		ZQuery GetInventoryStatusQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			AddIfNotEmpty(query, WhsInventoryViewSchema.WI_InventoryStatus, SQLComparisonOperator.Equal, value);

			if (value == InventoryStatus.Codes.Held)
			{
				var locationQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsInventoryViewSchema.WI_WE_InDocketLine);
				locationQuery.AddToFilter(WhsLocationViewSchema.WLV_LocationStatus, new[] { LocationStatus.Codes.Held, LocationStatus.Codes.Damaged });
				query.AddSubQuery(locationQuery, JoinCondition.Or);
			}
			return query;
		}

		#endregion

		#endregion

		#region AddClientFilter

		protected void AddClientFilter(ModuleFilterCollection filters)
		{
			var clientFilter = filters.AddGuidFilter(Schema.Client, ModuleIDs.Organisation, GetClientFilter, Clients);
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("b206433a-ba4a-4da6-8725-a91d0bfb90fe", "Client");
			clientFilter.IsPublishedOnWeb = false;

			if (!Env.Security.WhsAllowedClients.IsAllowed && !Globals.IsWeb)
			{
				clientFilter.Visibility = FilterVisibility.AlwaysVisible;
				clientFilter.PropertyValidation = ClientFilterValidation;
			}
			clientFilter.PropertyInfo.ValueChanged += OnClientChanged;
		}

		ZQuery GetClientFilter(ZGuid value)
		{
			var clientQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			clientQuery.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, value);

			return clientQuery;
		}

		void ClientFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("9b0def8a-9dfb-464e-b786-20c84f90216c", "Please select a client to filter by"));
			}
		}

		#region OnClientChanged

		void OnClientChanged(object sender, EventArgs e)
		{
			ClientChanged?.Invoke(this, EventArgs.Empty);

			RevalidateProductFilter();
		}

		public event EventHandler ClientChanged;

		#endregion

		#endregion

		#region AddWarehouseFilter

		void AddWarehouseFilter(ModuleFilterCollection filters)
		{
			var warehouseFilter = filters.AddGuidFilter(Schema.Warehouse, ModuleIDs.WhsConfigWarehouse, GetWarehouseQuery, Warehouses);
			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("82fcc8af-cde6-4b31-8384-343ad61a90f4", "Warehouse");
			if (!Env.Security.WhsAllowedWarehouses.IsAllowed && !Globals.IsWeb)
			{
				warehouseFilter.Visibility = FilterVisibility.AlwaysVisible;
				warehouseFilter.PropertyValidation = WarehouseFilterValidation;
			}
			else if (Globals.IsWeb)
			{
				warehouseFilter.PropertyValidation = WarehouseWebFilterValidation;
			}
			warehouseFilter.PropertyInfo.ValueChanged += OnWarehouseChanged;
		}

		void WarehouseFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("daea1cd1-3352-431c-af26-7a42e02fd357", "Please select a warehouse to filter by"));
			}
		}

		protected virtual void WarehouseWebFilterValidation(ZPropertyInfo info)
		{
		}

		#endregion

		#region OnWarehouseChanged

		void OnWarehouseChanged(object sender, EventArgs e)
		{
			if (WarehouseChanged != null)
			{
				WarehouseChanged(this, EventArgs.Empty);
			}

			OnWarehouseChangeUpdateLocationCache();
		}

		public event EventHandler WarehouseChanged;

		#endregion

		#region AddProductFilter

		void AddProductFilter(ModuleFilterCollection filters)
		{
			var productFilter = filters.AddGuidFilter(Schema.Product, ModuleIDs.WhsConfigProduct, GetProductFilter, GetProducts);
			productFilter.MultilingualDescription = ResString.GetMultilingualString("cae29191-e6d9-4a0a-8f05-71f130f95dbb", "Product");
			productFilter.PropertyValidation = ProductFilterValidation;
		}

		ZQuery GetProductFilter(ZGuid value)
		{
			ZQuery query;

			var product = Factory.Load<OrgSupplierPart>(value);
			if (product == null)
			{
				query = ZQuery.NoResultQuery;
			}
			else
			{
				query = new ZDBOnlyQuery(typeof(WhsInventoryView));
				var subQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsInventoryViewSchema.WI_OP);
				subQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, product.OP_PartNum);
				((ZDBOnlyQuery)query).AddSubQuery(subQuery, JoinCondition.And);
			}

			return query;
		}

		ZQuery GetLocationTypeFilter(ZGuid value)
		{
			var locationQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsInventoryViewSchema.WI_WL);
			locationQuery.AddToFilter(WhsLocationViewSchema.WLV_WLT_LocationType, value);

			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			query.AddSubQuery(locationQuery, JoinCondition.Or);

			return query;
		}

		void ProductFilterValidation(ZPropertyInfo info) => ProductFilterValidation(ProductFilter.Property, info);

		void ComponentProductFilterValidation(ZPropertyInfo info) => ProductFilterValidation(ComponentProductFilter.Property, info);

		void ProductFilterValidation(ZGuid property, ZPropertyInfo info)
		{
			if (!Globals.IsWeb)
			{
				var clientFilter = ClientFilter;
				if (!property.IsEmpty &&
					(!clientFilter.IsActive || clientFilter.Property.IsEmpty || !clientFilter.Property.IsValid))
				{
					info.AddError(Res.GetString("70275478-0b1d-45c3-81d8-475733c4da60", "Product Code can not be entered without a Client Code."));
				}
			}
		}

		WhsOrgSupplierPartCollection GetProducts()
		{
			return GetProductsCore();
		}

		protected virtual WhsOrgSupplierPartCollection GetProductsCore()
		{
			var client = Client;
			if (products == null || productsClient != client)
			{
				productsClient = client;
				products = new WhsOrgSupplierPartCollection(Factory, null, productsClient, false);
			}

			return products;
		}

		WhsOrgSupplierPartCollection products;
		OrgHeader productsClient;

		#endregion

		#region AddProductCategoryFilter

		void AddProductCategoryFilter(ModuleFilterCollection filters)
		{
			var productCategoryFilter = filters.AddGuidFilter(Schema.ProductCategory, ModuleIDs.RefOrgPartCategory, GetProductCategoryFilter, GetProductCategories);
			productCategoryFilter.MultilingualDescription = ResString.GetMultilingualString("609EAF02-E0AC-414C-BD47-80382413D7AB", "Product Category");
			productCategoryFilter.IsPublishedOnWeb = false;
		}

		ZQuery GetProductCategoryFilter(ZGuid value)
		{
			var relationTypeSubQuery = new ZQuery();
			relationTypeSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Owner);
			relationTypeSubQuery.AddToFilter(JoinCondition.Or, OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Both);

			var relationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			relationSubQuery.AddToFilter(relationTypeSubQuery, JoinCondition.And);

			var categoryPKs = new OrgPartProductCategoryHelper(Factory).ProductCategoryAndSubCategories(value);
			relationSubQuery.AddToFilter(OrgPartRelationSchema.OU_OPC_Category, categoryPKs);

			var productSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsInventoryViewSchema.WI_OP);
			productSubQuery.AddSubQuery(relationSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			query.AddSubQuery(productSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region WB Filters

		#region AddManufacturerFilter

		void AddManufacturerFilter(ModuleFilterCollection filters)
		{
			var manufacturerFilter = filters.AddGuidFilter(Schema.Manufacturer, ModuleIDs.Organisation, GetManufacturerFilter, GetManufacturers);
			manufacturerFilter.MultilingualDescription = ResString.GetMultilingualString("InventoryFilterBusinessObject|Manufacturer", "Manufacturer");
			manufacturerFilter.Category = FilterCategories.Other;
		}

		ZQuery GetManufacturerFilter(ZGuid value)
		{
			var orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderSubQuery.AddToFilter(OrgHeaderSchema.PK, value);

			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressSubQuery.AddSubQuery(OrgAddressSchema.OA_OH, OrgHeaderSchema.PK, orgHeaderSubQuery, JoinCondition.And);

			var bondedAttributesSubQuery = new ZDBOnlySubQuery(typeof(WhsBondedWarehouseAttribute), WhsBondedWarehouseAttributeSchema.PK);
			bondedAttributesSubQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentTableCode, WhsDocketLineSchema.Constants.Prefix);
			bondedAttributesSubQuery.AddSubQuery(WhsBondedWarehouseAttributeSchema.WB_OA_ManufacturerAddress, OrgAddressSchema.PK, orgAddressSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			query.AddSubQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, WhsBondedWarehouseAttributeSchema.WB_ParentID, bondedAttributesSubQuery, JoinCondition.And);
			return query;
		}

		OrgHeaderCollection GetManufacturers => manufacturers ?? (manufacturers = new OrgHeaderCollection(Factory));
		OrgHeaderCollection manufacturers;

		#endregion

		#region AddCountryOfOriginFilter

		void AddCountryOfOriginFilter(ModuleFilterCollection filters)
		{
			var countryOfOriginFilter = filters.AddNkFilter(Schema.CountryOfOrigin, GetCountryOfOriginFilter, ModuleIDs.RefCountry, GetCountries);
			countryOfOriginFilter.MultilingualDescription = ResString.GetMultilingualString("InventoryFilterBusinessObject|CountryOfOrigin", "Country/Region of Origin");
			countryOfOriginFilter.Category = FilterCategories.Other;
		}

		ZQuery GetCountryOfOriginFilter(ZString value)
		{
			var bondedWarehouseAttributeSubQuery = new ZDBOnlySubQuery(typeof(WhsBondedWarehouseAttribute), WhsBondedWarehouseAttributeSchema.WB_ParentID);
			bondedWarehouseAttributeSubQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentTableCode, WhsDocketLineSchema.Constants.Prefix);
			bondedWarehouseAttributeSubQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_RN_NKCountryOfOrigin, value);

			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			query.AddSubQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, WhsBondedWarehouseAttributeSchema.WB_ParentID, bondedWarehouseAttributeSubQuery, JoinCondition.And);

			return query;
		}

		RefCountryCollection GetCountries => countries ?? (countries = new RefCountryCollection(Factory));
		RefCountryCollection countries;

		#endregion

		void AddWBSimpleQuerys(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(
				Schema.DeclarationReference,
				(@operator, value) => GetInventoryViewWithWBTextQuery(WhsBondedWarehouseAttributeSchema.WB_DeclarationReference, @operator, value)
			).MultilingualDescription = ResString.GetMultilingualString("0FF506F8-C080-4777-B2F0-985A8F8D0240", "Declaration Reference");

			filters.AddDateFilter(
				Schema.CustomsDeadline,
				(@operator, value1, value2) => GetInventoryViewWithWBDateQuery(WhsBondedWarehouseAttributeSchema.WB_CustomsDeadline, @operator, value1, value2)
			).MultilingualDescription = ResString.GetMultilingualString("7EE816FF-40DA-4279-AC4D-B5BF13C20537", "Customs Deadline");

			filters.AddTextFilter(
				Schema.InwardStyle,
				(@operator, value) => GetInventoryViewWithWBTextQuery(WhsBondedWarehouseAttributeSchema.WB_InwardStyle, @operator, value)
			).MultilingualDescription = ResString.GetMultilingualString("205E3085-34EB-4495-ACF0-DFB34430B224", "Customs Inward Style");

			filters.AddTextFilter(
				Schema.InwardProcedure,
				(@operator, value) => GetInventoryViewWithWBTextQuery(WhsBondedWarehouseAttributeSchema.WB_InwardProcedure, @operator, value)
			).MultilingualDescription = ResString.GetMultilingualString("0B0AA6A5-139B-4DF3-8F9F-8753DD876B8C", "Customs Inward Procedure");
		}

		ZQuery GetInventoryViewWithWBTextQuery(SchemaColumn wbColumn, SQLComparisonOperator @operator, IZType value)
		{
			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));

			var wbSubQuery = new ZDBOnlySubQuery(typeof(WhsBondedWarehouseAttribute), WhsBondedWarehouseAttributeSchema.WB_ParentID);
			wbSubQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentTableCode, WhsDocketLineSchema.Constants.Prefix);
			wbSubQuery.AddToFilter(wbColumn, @operator, value);
			query.AddSubQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, WhsBondedWarehouseAttributeSchema.WB_ParentID, wbSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetInventoryViewWithWBDateQuery(SchemaDateTimeColumn wbColumn, DateComparisonOperator @operator, ZDateTime value1, ZDateTime value2)
		{
			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));

			var wbSubQuery = new ZDBOnlySubQuery(typeof(WhsBondedWarehouseAttribute), WhsBondedWarehouseAttributeSchema.WB_ParentID);
			wbSubQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentTableCode, WhsDocketLineSchema.Constants.Prefix);
			AddDateTimeRange(wbSubQuery, @operator, JoinCondition.And, wbColumn, value1, value2);
			query.AddSubQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, WhsBondedWarehouseAttributeSchema.WB_ParentID, wbSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region AddProductStyleFilter

		void AddProductStyleFilter(ModuleFilterCollection filters)
		{
			var productStyleFilter = filters.AddGuidFilter(Schema.ProductStyle, ModuleIDs.WhsConfigProductStyle, GetProductStyleFilter, GetProductStyles);
			productStyleFilter.MultilingualDescription = ResString.GetMultilingualString("11E38220-A23F-4e64-93E5-EE429CB1BDC1", "Product Style");
			productStyleFilter.PropertyInfo.ValueChanged += OnProductStyleChanged;
			productStyleFilter.IsPublishedOnWeb = false;
		}

		ZQuery GetProductStyleFilter(ZGuid value)
		{
			var lineQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			var partSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsInventoryViewSchema.WI_OP);

			var colourSubQuery = new ZDBOnlySubQuery(typeof(WhsProductStyleColour), OrgSupplierPartSchema.OP_WSC_WhsProductStyleColour);
			colourSubQuery.AddToFilter(WhsProductStyleColourSchema.WSC_WST_ProductStyle, value);

			var classificationSubQuery = new ZDBOnlySubQuery(typeof(WhsProductStyleClassification), OrgSupplierPartSchema.OP_WSS_WhsProductStyleClassification);
			classificationSubQuery.AddToFilter(WhsProductStyleClassificationSchema.WSS_WST_ProductStyle, value);

			var sizeSubQuery = new ZDBOnlySubQuery(typeof(WhsProductStyleSize), OrgSupplierPartSchema.OP_WSZ_WhsProductStyleSize);
			sizeSubQuery.AddToFilter(WhsProductStyleSizeSchema.WSZ_WST_ProductStyle, value);

			partSubQuery.AddSubQuery(colourSubQuery, JoinCondition.And);
			partSubQuery.AddSubQuery(sizeSubQuery, JoinCondition.And);
			lineQuery.AddSubQuery(partSubQuery, JoinCondition.And);

			return lineQuery;
		}

		WhsProductStyleCollection GetProductStyles() => productStyles ?? (productStyles = new WhsProductStyleCollection(Factory));
		WhsProductStyleCollection productStyles;
		ZGuid ProductStylePK => ProductStyleFilter != null && ProductStyleFilter.IsActive ? ProductStyleFilter.Property : ZGuid.Empty;

		#endregion

		#region Product Style

		void OnProductStyleChanged(object sender, EventArgs e)
		{
			ColourCodeFilter.Property = "";
			ClassificationCodeFilter.Property = "";
			StyleSizeFilter.Property = "";
		}
		ModuleTextFilter ColourCodeFilter => (ModuleTextFilter)ModuleFilters[Schema.ColourCode];

		ModuleTextFilter ClassificationCodeFilter => (ModuleTextFilter)ModuleFilters[Schema.ClassificationCode];

		#endregion

		#region StyleSizeFilter

		void AddStyleSizeFilter(ModuleFilterCollection filters)
		{
			var styleSizeFilter = filters.AddTextFilter(Schema.StyleSize, GetProductStyleSizeQuery, GetProductStyleSizes);
			styleSizeFilter.Category = FilterCategories.Other;
			styleSizeFilter.MultilingualDescription = ResString.GetMultilingualString("1871EF5B-868A-456f-91A3-51815D1713DC", "Product Style Size");
			styleSizeFilter.IsPublishedOnWeb = false;
		}

		ZQuery GetProductStyleSizeQuery(ZString value)
		{
			var lineQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			var partSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsInventoryViewSchema.WI_OP);

			var sizeSubQuery = new ZDBOnlySubQuery(typeof(WhsProductStyleSize), OrgSupplierPartSchema.OP_WSZ_WhsProductStyleSize);
			sizeSubQuery.AddToFilter(WhsProductStyleSizeSchema.WSZ_Size, value);

			partSubQuery.AddSubQuery(sizeSubQuery, JoinCondition.And);
			lineQuery.AddSubQuery(partSubQuery, JoinCondition.And);

			return lineQuery;
		}

		IList GetProductStyleSizes()
		{
			var productStyle = ProductStyle;
			return productStyle != null
				? Factory.GetCachedValue("InventoryFilterBusinessObject|GetProductStyleSize|" + ProductStylePK, () => new WhsProductStyleSizeCollection(ProductStyle))
				: new List<WhsProductStyleSize>().AsReadOnly();
		}

		ModuleTextFilter StyleSizeFilter => (ModuleTextFilter)ModuleFilters[Schema.StyleSize];

		#endregion

		#region ColorCodeFilter

		void AddColourCodeFilter(ModuleFilterCollection filters)
		{
			var colourCodeFilter = filters.AddTextFilter(Schema.ColourCode, GetProductStyleColourQuery, GetProductStyleColours);
			colourCodeFilter.Category = FilterCategories.Other;
			colourCodeFilter.MultilingualDescription = ResString.GetMultilingualString("CE03E351-9011-445b-9FD3-81B4BFC806A4", "Product Style Color");
			colourCodeFilter.IsPublishedOnWeb = false;
		}

		ZQuery GetProductStyleColourQuery(ZString value)
		{
			var lineQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			var partSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsInventoryViewSchema.WI_OP);

			var colourSubQuery = new ZDBOnlySubQuery(typeof(WhsProductStyleColour), OrgSupplierPartSchema.OP_WSC_WhsProductStyleColour);
			colourSubQuery.AddToFilter(WhsProductStyleColourSchema.WSC_Code, value);

			partSubQuery.AddSubQuery(colourSubQuery, JoinCondition.And);
			lineQuery.AddSubQuery(partSubQuery, JoinCondition.And);

			return lineQuery;
		}

		IList GetProductStyleColours()
		{
			var productStyle = ProductStyle;
			return productStyle != null
				? Factory.GetCachedValue("InventoryFilterBusinessObject|GetProductStyleColours|" + ProductStylePK, () => new WhsProductStyleColourCollection(ProductStyle))
				: new List<WhsProductStyleColour>().AsReadOnly();
		}

		#endregion

		#region ClassificationCodeFilter

		void AddClassificationCodeFilter(ModuleFilterCollection filters)
		{
			var classificationCodeFilter = filters.AddTextFilter(Schema.ClassificationCode, GetProductStyleClassificationQuery, GetProductStyleClassifications);
			classificationCodeFilter.Category = FilterCategories.Other;
			classificationCodeFilter.MultilingualDescription = ResString.GetMultilingualString("3206bcae-4011-4ba3-8b30-fc5664917dc0", "Product Style Classification");
			classificationCodeFilter.IsPublishedOnWeb = false;
		}

		ZQuery GetProductStyleClassificationQuery(ZString value)
		{
			var lineQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			var partSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsInventoryViewSchema.WI_OP);

			var classificationSubQuery = new ZDBOnlySubQuery(typeof(WhsProductStyleClassification), OrgSupplierPartSchema.OP_WSS_WhsProductStyleClassification);
			classificationSubQuery.AddToFilter(WhsProductStyleClassificationSchema.WSS_Code, value);

			partSubQuery.AddSubQuery(classificationSubQuery, JoinCondition.And);
			lineQuery.AddSubQuery(partSubQuery, JoinCondition.And);

			return lineQuery;
		}

		IList GetProductStyleClassifications()
		{
			var productStyle = ProductStyle;
			return productStyle != null
				? Factory.GetCachedValue("InventoryFilterBusinessObject|GetProductStyleClassifications|" + ProductStylePK, () => new WhsProductStyleClassificationCollection(ProductStyle))
				: new List<WhsProductStyleClassification>().AsReadOnly();
		}

		#endregion

		#region GetAttributeQuery

		public ModuleFilterCollection GetAttributeFilters()
		{
			var filters = new ModuleFilterCollection();

			var attributes = new AttributeManager()
				.GetAllAttributes(
					AttributeManager.AttributeModules.WhsInventory,
					Client,
					loggedInWebUsersOrg)
				.ToArray();

			filters.AddAttributeFilters(attributes.Where(a => !IsSerialAttribute(a)), GetAttributeFilter);

			var serialAttribute = attributes.SingleOrDefault(IsSerialAttribute);
			// No need to check null after serial number column has been removed from the docket line.
			if (serialAttribute != null)
			{
				var serialFilter = filters.AddTextFilter(serialAttribute.Key, FilterBySerialNumber);

				serialFilter.MaxLength = WhsSerialNumberSchema.WSN_SerialNumber.MaxLength;
				serialFilter.Category = FilterCategories.AttributeSearch;
				serialFilter.MultilingualDescription = serialAttribute.Caption;
			}

			return filters;

			static bool IsSerialAttribute(CustomAttribute customAttribute)
				=> customAttribute.Column.Name.Equals(WhsSerialNumberSchema.WSN_SerialNumber.Name);
		}

		ZDBOnlyQuery GetAttributeFilter(ZQuery filter, SchemaColumn column)
		{
			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));

			if (column.TableSchema == WhsDocketSchema.Instance)
			{
				var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsInventoryViewSchema.WI_WD);
				docketSubQuery.AddToFilter(filter);
				query.AddSubQuery(docketSubQuery, JoinCondition.And);
			}
			else if (column.TableSchema == WhsDocketLineSchema.Instance)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsInventoryViewSchema.WI_WE_InDocketLine);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}
			else if (column.TableSchema == WhsInventoryViewSchema.Instance)
			{
				query.AddToFilter(filter);
			}
			else if (column.TableSchema == WhsSerialNumberSchema.Instance)
			{
				// Manually implemented in FilterBySerialNumber; no need to add it to the query.
			}
			else
			{
				throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "{0} schema is not supported", column.TableSchema));
			}

			return query;
		}

		#endregion

		#region AddLocationFilter

		void AddLocationFilter(ModuleFilterCollection filters)
		{
			var locationFilter = filters.AddGuidFilter(Schema.Location, ModuleIDs.WhsConfigLocation, WhsInventoryViewSchema.WI_WL, GetLocations);
			locationFilter.MultilingualDescription = ResString.GetMultilingualString("90B9F179-AA7B-4F06-BACA-41FA27DF8955", "Location");
			locationFilter.PropertyValidation = LocationFilterValidation;
			locationFilter.Category = FilterCategories.Locations;
			locationFilter.IsPublishedOnWeb = false;
		}

		void LocationFilterValidation(ZPropertyInfo info)
		{
			var warehouseFilter = WarehouseFilter;
			if (!LocationFilter.Property.IsEmpty &&
			   (!warehouseFilter.IsActive || warehouseFilter.Property.IsEmpty || !warehouseFilter.Property.IsValid))
			{
				info.AddError(Res.GetString("48E6459E-74BD-4911-99FD-2977EA5D22BE", "Location can not be entered without a warehouse."));
			}
		}

		WhsLocationCollection GetLocations()
		{
			if (whsLocations == null)
			{
				var warehouse = WI_WW_Whs.IsValid ? Warehouse : null;
				whsLocations = warehouse == null ? new WhsLocationCollection(Factory) : new WhsLocationCollection(warehouse);
			}

			return whsLocations;
		}

		void OnWarehouseChangeUpdateLocationCache()
		{
			var locationFilter = LocationFilter;
			if (locationFilter.IsActive && locationFilter.Property.IsValid)
			{
				var location = Factory.Load<WhsLocation>(locationFilter.Property);
				if (location == null || WI_WW_Whs != location.WLV_WW_Whs)
				{
					ClearLocationCache();
				}
				else
				{
					locationFilter.Validation.ValidateProperty();
				}
			}
			else
			{
				ClearLocationCache();
			}

			void ClearLocationCache()
			{
				locationFilter.Property = ZGuid.Empty;
				whsLocations = null;
			}
		}

		WhsLocationCollection whsLocations;

		#endregion

		#endregion

		#region GetCommodityCodeQuery

		ZQuery GetCommodityCodeQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			var productSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsInventoryViewSchema.WI_OP);
			productSubQuery.AddToFilter(OrgSupplierPartSchema.OP_RH_NKCommodityCode, value);
			query.AddSubQuery(productSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region AddLocationNumericFilterFromParams

		ModuleNumberRangeFilter AddLocationNumericFilterFromColumn(ModuleFilterCollection filters, ZString descritption, SchemaNumericColumn column, MultilingualString mLingualString)
		{
			var filter = filters.AddNumberRangeFilter(descritption, column);
			filter.MultilingualDescription = mLingualString;
			filter.Category = FilterCategories.Locations;
			filter.SubGroup = LocationSubGroup;
			filter.IsPublishedOnWeb = false;

			return filter;
		}

		#endregion

		#region AddLocationStockTakeFilter

		ModuleNumberRangeFilter AddLocationStockTakeFilter(ModuleFilterCollection filters, ZString descritption, MultilingualString mLingualString)
		{
			var filter = filters.AddNumberRangeFilter(descritption, GetLocationStockTakeCountQuery);
			filter.MultilingualDescription = mLingualString;
			filter.Category = FilterCategories.Locations;
			filter.SubGroup = LocationSubGroup;
			filter.IsPublishedOnWeb = false;
			filter.PropertyType = ZCalcEditPropertyType.Int;
			return filter;
		}

		#endregion

		#region AddLocationSubFilter

		void AddLocationSubFilter(ModuleFilterCollection filters, string description, SchemaStringColumn column, ICodeDescriptionPairList list, MultilingualString multilingualDescription)
		{
			var filter = filters.AddTextFilter(description, value => new ZQuery(column, value), list);
			filter.MultilingualDescription = multilingualDescription;
			filter.Category = FilterCategories.Locations;
			filter.SubGroup = LocationSubGroup;
			filter.IsPublishedOnWeb = false;
		}

		#endregion

		#region GetLocationStockTakeCountQuery

		ZQuery GetLocationStockTakeCountQuery(INumericZType min, INumericZType max)
		{
			var query = new ZQuery();
			var parameters = new ZSqlParameterCollection(); // Values passed in are Decimals so we change to Int since column is an int.
			parameters.Add("@TouchesMin", ((ZDecimal)min).ToZInt(), WhsLocationViewSchema.WLV_MaximumPickCountBeforeAutomatedStocktake); // Filter string
			parameters.Add("@TouchesMax", ((ZDecimal)max).ToZInt(), WhsLocationViewSchema.WLV_FinalisedPickCount); // Filter string
			var comparison = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} - {1}  >= @TouchesMin  AND {0} - {1} <= @TouchesMax", WhsLocationViewSchema.WLV_MaximumPickCountBeforeAutomatedStocktake.Name, WhsLocationViewSchema.WLV_FinalisedPickCount.Name); // Filter string
			query.AddFilterAndZSQLParameterCollection(comparison, parameters);

			return query;
		}

		#endregion

		#region LocationFiltersSubGroup

		LocationFiltersSubGroup LocationSubGroup => locationSubGroup ?? (locationSubGroup = new LocationFiltersSubGroup());
		LocationFiltersSubGroup locationSubGroup;

		class LocationFiltersSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return WhsInventoryView.GetInventoryQueryByLocation(s => s.AddToFilter(filter));
			}
		}

		#endregion

		#region GetLocationSubQuery

		readonly IList<SQLComparisonOperator> operatorsNeedIncludeInventoryWithoutLocation = new List<SQLComparisonOperator> {
			SQLComparisonOperator.DoesNotEndWith,
			SQLComparisonOperator.DoesNotStartWith,
			SQLComparisonOperator.DoesNotEndWith,
			SQLComparisonOperator.NotContains,
			SQLComparisonOperator.NotEqual,
			SpecialComparisonOperator.IsBlank
		};

		ZQuery GetLocationSubQuery(Type typeOfBusinessObjectToSubQuery, SchemaColumn foreignKey, SchemaColumn column, SQLComparisonOperator comparisonOperator, object value)
		{
			var query = WhsInventoryView.GetInventoryQueryByLocation(locationQuery =>
			{
				var subQuery = new ZDBOnlySubQuery(typeOfBusinessObjectToSubQuery, foreignKey);
				subQuery.AddToFilter(column, comparisonOperator, value);
				locationQuery.AddSubQuery(subQuery, JoinCondition.And);
			});

			if (operatorsNeedIncludeInventoryWithoutLocation.Contains(comparisonOperator))
			{
				var noLocationQuery = new ZQuery(WhsInventoryViewSchema.WI_WL, null);
				query.AddToFilter(noLocationQuery, JoinCondition.Or);
			}

			return query;
		}

		#endregion

		#region GetLocationParserSubQuery

		ZQuery GetLocationParserSubQuery(SchemaColumn locationComponent, SchemaColumn warehouseComponentAlpha, SchemaColumn warehouseComponentZeroBased, ZString value)
		{
			return LocationComponentQueryHelper.GetLocationParserSubQueryForInventory(locationComponent, warehouseComponentAlpha, warehouseComponentZeroBased, value);
		}

		#endregion

		#region ReceiptReferenceFilter

		ZQuery ReceiptReferenceFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var receiptSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsInventoryViewSchema.WI_WD);
			receiptSubQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, comparisonOperator, value);

			var receiptLineSubQuery = new ZDBOnlySubQuery(typeof(WhsInventoryView), WhsInventoryViewSchema.WI_WE_InDocketLine);
			receiptLineSubQuery.AddSubQuery(receiptSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			query.AddSubQuery(WhsInventoryViewSchema.WI_WE_OriginalInDocketLineForRating, receiptLineSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region PackageIDFilter

		ZQuery PackageIDFilter(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var packageIDSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
			packageIDSubQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, comparisonOperator, value);

			var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageItemDivotSchema.KI_KP_Package);
			packageSubQuery.AddSubQuery(packageIDSubQuery, JoinCondition.And);

			var packageItemDivotPickLineSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageItemDivot), PkgPackageItemDivotSchema.KI_ParentID);
			packageItemDivotPickLineSubQuery.AddToFilter(PkgPackageItemDivotSchema.KI_ParentTableCode, WhsPickLineSchema.Constants.Prefix);
			packageItemDivotPickLineSubQuery.AddSubQuery(packageSubQuery, JoinCondition.And);

			var pickLineQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_InventoryLine);
			pickLineQuery.AddSubQuery(packageItemDivotPickLineSubQuery, JoinCondition.Or);

			var query = new ZDBOnlyQuery(typeof(WhsDocketLine));
			query.AddSubQuery(pickLineQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region SerialNumberFilter

		ZDBOnlySubQuery BuildSerialPivotQuery(SQLComparisonOperator comparisonOperator, ZString serialNumber)
		{
			var isBlank = comparisonOperator == SpecialComparisonOperator.IsBlank;
			var notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);
			var pivotSubQuery = new ZDBOnlySubQuery(typeof(WhsSerialNumberPivot), WhsSerialNumberPivotSchema.WSV_ParentID, notIn || isBlank);
			if (!isBlank)
			{
				var filter = new ZQuery(WhsSerialNumberSchema.WSN_SerialNumber, comparisonOperator, serialNumber);
				var subQuerySerialNumber = new ZDBOnlySubQuery(typeof(WhsSerialNumber), WhsSerialNumberSchema.PK);
				subQuerySerialNumber.AddToFilter(filter);
				pivotSubQuery.AddSubQuery(WhsSerialNumberPivotSchema.WSV_WSN_SerialNumber, subQuerySerialNumber, JoinCondition.And);
			}
			return pivotSubQuery;
		}

		ZQuery FilterBySerialNumber(SQLComparisonOperator comparisonOperator, ZString serialNumber)
		{
			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			query.AddSubQuery(BuildSerialPivotQuery(comparisonOperator, serialNumber), JoinCondition.And);
			return query;
		}

		ZQuery FilterByComponentSerialNumber(SQLComparisonOperator comparisonOperator, ZString serialNumber)
		{
			var pivot = BuildSerialPivotQuery(comparisonOperator, serialNumber);
			return GetInventoryQueryViaPickline(WhsPickLineSchema.WZ_WE_InventoryLine, pivot);
		}

		#endregion

		#region GetLocationTypes

		WhsLocationTypeCollection GetLocationTypes => new WhsLocationTypeCollection(Factory);

		#endregion

		#region Warehouse

		#endregion

		#region Fields

		public OrgHeader Client => Factory.Load<OrgHeader>(ClientPK);
		public ZGuid ClientPK => ClientFilter != null && ClientFilter.IsActive ? ClientFilter.Property : ZGuid.Empty;
		public WarehouseClientCollectionWithSecurityCheck Clients => new WarehouseClientCollectionWithSecurityCheck(Factory);
		public RefCommodityCodeCollection CommodityCodes => new RefCommodityCodeCollection(Factory);
		public WhsProductStyle ProductStyle => !ProductStylePK.IsEmpty ? Factory.Load<WhsProductStyle>(ProductStylePK) : null;
		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(WI_WW_Whs);

		ModuleGuidFilter ClientFilter => (ModuleGuidFilter)ModuleFilters[Schema.Client];
		ZGuid WI_WW_Whs => WarehouseFilter != null && WarehouseFilter.IsActive ? WarehouseFilter.Property : ZGuid.Empty;
		ModuleGuidFilter ProductStyleFilter => (ModuleGuidFilter)ModuleFilters[Schema.ProductStyle];
		ModuleGuidFilter WarehouseFilter => (ModuleGuidFilter)ModuleFilters[Schema.Warehouse];
		OrgPartCategoryCollection GetProductCategories => new OrgPartCategoryCollection(Factory);
		ModuleGuidFilter ProductFilter => (ModuleGuidFilter)ModuleFilters[Schema.Product];
		ModuleGuidFilter ComponentProductFilter => (ModuleGuidFilter)ModuleFilters[Schema.ComponentProduct];
		ModuleGuidFilter LocationFilter => (ModuleGuidFilter)ModuleFilters[Schema.Location];
		WhsWarehouseCollectionWithSecurityCheck Warehouses => new WhsWarehouseCollectionWithSecurityCheck(Factory);
		void RevalidateProductFilter() => ProductFilterReValidator.RevalidateProductFilter(ClientFilter, ProductFilter, Factory);

		#endregion

		#region Lookups

		public CodeDescriptionPairList CommittedTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair("ALL", Res.GetString("3d5db0ab-3c97-4827-ad71-f9d83084e5d6", "All"));
				result.AddPair("COM", Res.GetString("6a41c62b-2ef9-4cf5-b621-0c6e1a5c5d74", "Committed"));
				result.AddPair("UNC", Res.GetString("b9241036-7041-42a3-a9a5-cfe5e0223f1c", "Uncommitted"));
				return result;
			}
		}

		public CodeDescriptionPairList IsExpiredTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Schema.IsExpiredTypeCodes.All, Res.GetString("a4fc5a9f-fbb5-4e0d-99ab-72710ab586f1", "All"));
				result.AddPair(Schema.IsExpiredTypeCodes.Expired, Res.GetString("805dc048-7bbe-4af5-aa25-788ce335b50c", "Expired"));
				result.AddPair(Schema.IsExpiredTypeCodes.NotExpired, Res.GetString("e2d8d41a-f926-43c2-b5fb-698c4c33ced0", "Not Expired"));
				return result;
			}
		}

		#region InventoryStatuses

		public InventoryStatus InventoryStatuses
		{
			get
			{
				return Factory.GetCachedValue("InventoryFilterBusinessObject|InventoryStatuses", () =>
				{
					var statuses = new InventoryStatus();
					statuses.Sort();
					return statuses;
				});
			}
		}

		#endregion

		#endregion

		#region Queries

		#region GetWarehouseQuery

		ZQuery GetWarehouseQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			query.AddToFilter(WhsInventoryViewSchema.WI_WW_Whs, value);

			return query;
		}

		#endregion

		#region DocketLineLocationSubGroup

		protected ModuleFilterSubGroup DocketLineLocationSubGroup => docketLineLocationSubGroup ?? (docketLineLocationSubGroup = new DocketLineLocationFilterSubGroup());
		ModuleFilterSubGroup docketLineLocationSubGroup;

		class DocketLineLocationFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subsubfilter = new ZDBOnlySubQuery(typeof(WhsLocation), WhsInventoryViewSchema.WI_WL);
				subsubfilter.AddToFilter(filter);

				var inventoryFilter = new ZDBOnlyQuery(typeof(WhsInventoryView));
				inventoryFilter.AddSubQuery(subsubfilter, JoinCondition.And);

				return inventoryFilter;
			}
		}

		#endregion

		#endregion
	}
}
