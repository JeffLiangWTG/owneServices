using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class StocktakeFilterBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		public static class Schema
		{
			public const string Product = "Product"; // Filter description
			public const string ProductCategory = "Product Category"; // Filter description
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddStocktakeNumberFilter(filters);
			AddWarehouseFilter(filters);
			AddClientFilter(filters);
			AddStatusFilter(filters);
			AddDateOpenedFilter(filters);
			AddCycleFilter(filters);
			AddProductFilter(filters);
			AddProductCategoryFilter(filters);
			AddCommodityFilter(filters);
			AddRowFilter(filters);
			AddPickAreaNameFilter(filters);
			AddExpiryDateFilter(filters);
			AddPackingDateFilter(filters);
			AddPalletIdFilter(filters);
			AddPartAttribute1Filter(filters);
			AddPartAttribute2Filter(filters);
			AddPartAttribute3Filter(filters);
			AddSerialNumberFilter(filters);
			AddDateClosedFilter(filters);
			AddLastVerifiedDateFilter(filters);
			AddLastVerifiedByFilter(filters);
			AddZeroCountOnlyFilter(filters);
			AddStocktakeTypeFilter(filters);

			AccountingFilterStrip.AddBillingFilters(filters);
			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.WhsStocktakeJobInvoicing);

			return filters;
		}

		#region StocktakeNumber Filter

		void AddStocktakeNumberFilter(ModuleFilterCollection filters)
		{
			var stocktakeNumberFilter = filters.AddFountainFilter("Stocktake No", WhsStocktakeSchema.WS_StocktakeNumber, "SK");
			stocktakeNumberFilter.MultilingualDescription = ResString.GetMultilingualString("cdc60d38-ce0c-4e01-946d-65c13f52767c", "Stocktake No");
		}

		#endregion

		#region Warehouse Filter

		void AddWarehouseFilter(ModuleFilterCollection filters)
		{
			var warehouseFilter = filters.AddGuidFilter("Warehouse", ModuleIDs.WhsConfigWarehouse, WhsStocktakeSchema.WS_WW_Whs, Warehouses);
			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("82fcc8af-cde6-4b31-8384-343ad61a90f4", "Warehouse");

			if (!Env.Security.WhsAllowedWarehouses.IsAllowed && !Globals.IsWeb)
			{
				warehouseFilter.Visibility = FilterVisibility.AlwaysVisible;
				warehouseFilter.PropertyValidation = WarehouseFilterValidation;
			}
		}

		void WarehouseFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("efd6398c-3c82-4a0c-ae8f-62e6cecbbe88", "Please select a warehouse to filter by"));
			}
		}

		#endregion

		#region Client Filter

		void AddClientFilter(ModuleFilterCollection filters)
		{
			var clientFilter = filters.AddGuidFilter("Client", ModuleIDs.Organisation, WhsStocktakeSchema.WS_OH_Client, Clients);
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("da215578-cf4d-4199-b8e4-ea1f183fabcf", "Client");
			clientFilter.IsPublishedOnWeb = false;

			if (!Env.Security.WhsAllowedClients.IsAllowed && !Globals.IsWeb)
			{
				clientFilter.Visibility = FilterVisibility.AlwaysVisible;
				clientFilter.PropertyValidation = ClientFilterValidation;
			}

			clientFilter.PropertyInfo.ValueChanged += new EventHandler(OnClient_ValueChanged);
		}

		void ClientFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("16024123-ad75-4a97-84f1-adacb32aa57f", "Please select a client to filter by"));
			}
		}

		#region OnClient_ValueChanged

		void OnClient_ValueChanged(object sender, EventArgs e)
		{
			ReValidateProductFilter();
		}

		void ReValidateProductFilter()
		{
			ProductFilterReValidator.RevalidateProductFilter(ClientFilter, ProductFilter, Factory);
		}

		#endregion

		#endregion

		#region Status Filter

		void AddStatusFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Status", GetStatusQuery, StatusList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("ae3ec606-fc85-4bd4-bafa-a3221c47efca", "Status");
		}

		#endregion

		#region DateOpened Filter

		void AddDateOpenedFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter("DateOpened", WhsStocktakeSchema.WS_StocktakeDate, false);
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("a81bbfb3-44ec-44b6-a766-692777f58308", "Date Opened");
		}

		#endregion

		#region Cycle Filter

		void AddCycleFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Cycle", GetCycleQuery, StocktakeCycleList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("1884387d-1149-4d90-beb9-ebfbcba0c2b2", "Cycle");
		}

		#endregion

		#region Product Filter

		void AddProductFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter(Schema.Product, ModuleIDs.WhsConfigProduct, GetProductQuery, GetProductsList);
			filter.Category = FilterCategories.Other;
			filter.MultilingualDescription = ResString.GetMultilingualString("3c25bab9-0df0-4c24-80f9-9d6ec119338d", "Product");
			filter.PropertyValidation = ProductFilterValidation;
		}

		void ProductFilterValidation(ZPropertyInfo info)
		{
			var clientFilter = ClientFilter;
			if (!ProductFilter.Property.IsEmpty &&
				(!clientFilter.IsActive || clientFilter.Property.IsEmpty || !clientFilter.Property.IsValid))
			{
				info.AddError(Res.GetString("153c953e-9dea-4dc7-8f16-cee3e02aafaa", "Product Code can not be entered without a Client Code."));
			}
		}

		ModuleGuidFilter ProductFilter => (ModuleGuidFilter)ModuleFilters[Schema.Product];

		#endregion

		#region Product Category Filter

		void AddProductCategoryFilter(ModuleFilterCollection filters)
		{
			var categoryfilter = filters.AddGuidFilter(Schema.ProductCategory, ModuleIDs.RefOrgPartCategory, GetProductCategoryQuery, GetProductCategoriesList);
			categoryfilter.Category = FilterCategories.Other;
			categoryfilter.MultilingualDescription = ResString.GetMultilingualString("BDED4897-6851-4327-B946-71B34E974A4B", "Product Category");
			categoryfilter.IsPublishedOnWeb = false;
		}

		#endregion

		#region Commodity Filter

		void AddCommodityFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddNkFilter("Commodity", GetCommodityCodesQuery, ModuleIDs.RefCommodityCode, CommodityCodes);
			filter.Category = FilterCategories.Other;
			filter.MultilingualDescription = ResString.GetMultilingualString("4d880e5c-9395-47a0-b81e-6b4356eff2df", "Commodity");
		}

		#endregion

		#region Row Filter

		void AddRowFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Row", GetRowsQuery);
			filter.Category = FilterCategories.Locations;
			filter.MaxLength = WhsRowSchema.WR_Name.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("4b01a4ed-0aac-4977-bdb6-13889d5bc670", "Row");
		}

		#endregion

		#region Pick AreaName Filter

		void AddPickAreaNameFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("PickAreaName", GetAreaNameQuery);
			filter.Category = FilterCategories.Locations;
			filter.MaxLength = WhsAreaSchema.WA_Name.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("StocktakeFilterBusinessObject|PickAreaName", "Pick Area Name");
		}

		#endregion

		#region ExpiryDate Filter

		void AddExpiryDateFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter("ExpiryDate",
				(comparisonOperator, fromDate, toDate) => GetStocktakeLinesDateQuery(WhsStocktakeLineSchema.WU_ExpiryDate, comparisonOperator, fromDate, toDate));
			filter.Category = FilterCategories.AttributeSearch;
			filter.MultilingualDescription = ResString.GetMultilingualString("44578f21-bc90-4aeb-9413-a10a31186a37", "Expiry Date");
		}

		#endregion

		#region PackingDate Filter

		void AddPackingDateFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter("PackingDate",
				(comparisonOperator, fromDate, toDate) => GetStocktakeLinesDateQuery(WhsStocktakeLineSchema.WU_PackingDate, comparisonOperator, fromDate, toDate));
			filter.Category = FilterCategories.AttributeSearch;
			filter.MultilingualDescription = ResString.GetMultilingualString("a1afe7fe-a845-4145-a3cb-0f5e2390823a", "Packing Date");
		}

		#endregion

		#region AddPalletIdFilters

		public void AddPalletIdFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Pallet ID", GetPalletIDQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MaxLength = WhsStocktakeLineSchema.WU_PalletID.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("b7e69653-9aaa-4bc1-a4ae-28be67c68fb1", "Pallet ID");
		}

		#endregion

		#region AddPartAttribute1Filter

		void AddPartAttribute1Filter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("PartAttribute1",
				(comparisonOperator, attribute1Value) => GetStocktakeLinesAttributeQuery(WhsStocktakeLineSchema.WU_PartAttrib1, comparisonOperator, attribute1Value));
			filter.Category = FilterCategories.AttributeSearch;
			filter.MaxLength = WhsStocktakeLineSchema.WU_PartAttrib1.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("0a56338c-5588-4d27-a8cc-3c81f8c512f4", "Part Attribute 1");
		}

		#endregion

		#region AddPartAttribute2Filter

		void AddPartAttribute2Filter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("PartAttribute2",
				(comparisonOperator, attribute2Value) => GetStocktakeLinesAttributeQuery(WhsStocktakeLineSchema.WU_PartAttrib2, comparisonOperator, attribute2Value));
			filter.Category = FilterCategories.AttributeSearch;
			filter.MaxLength = WhsStocktakeLineSchema.WU_PartAttrib2.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("a67d63b4-10d8-46cf-9e9a-b9570aae5a18", "Part Attribute 2");
		}

		#endregion

		#region AddPartAttribute3Filter

		void AddPartAttribute3Filter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("PartAttribute3",
				(comparisonOperator, attribute3Value) => GetStocktakeLinesAttributeQuery(WhsStocktakeLineSchema.WU_PartAttrib3, comparisonOperator, attribute3Value));
			filter.Category = FilterCategories.AttributeSearch;
			filter.MaxLength = WhsStocktakeLineSchema.WU_PartAttrib3.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("2b4dca38-8912-43d2-9082-e4cbfcb805fa", "Part Attribute 3");
		}

		#endregion

		#region AddSerialNumberFilter

		void AddSerialNumberFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("SerialNumber",
			(comparisonOperator, serialNumberValue) => GetStocktakeLinesAttributeQuery(WhsStocktakeLineSchema.WU_SerialNumber, comparisonOperator, serialNumberValue));
			filter.Category = FilterCategories.AttributeSearch;
			filter.MaxLength = WhsStocktakeLineSchema.WU_SerialNumber.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("37ea8390-cbfd-492d-b015-4e203c4e78d1", "Serial Number");
		}

		#endregion

		#region DateClosed Filter

		void AddDateClosedFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter("DateClosed",
				(comparisonOperator, fromDate, toDate) => GetStocktakeLinesDateQuery(WhsStocktakeLineSchema.WU_DateClosed, comparisonOperator, fromDate, toDate));
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("613be2e3-ff85-4964-b11a-08479bcdb0c2", "Date Closed");
		}

		#endregion

		#region LastVerifiedDate Filter

		void AddLastVerifiedDateFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter("LastVerifiedDate",
				(comparisonOperator, fromDate, toDate) => GetStocktakeLinesDateQuery(WhsStocktakeLineSchema.WU_DateVerified, comparisonOperator, fromDate, toDate));
			filter.Category = FilterCategories.Dates;
			filter.MultilingualDescription = ResString.GetMultilingualString("17dd3625-fa9d-4b37-8141-ee4731d79e85", "Last Verified Date");
		}

		#endregion

		#region LastVerifiedBy Filter

		void AddLastVerifiedByFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter("LastVerifiedBy", ModuleIDs.GlbStaff, GetStaffQuery, StaffList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("78f2163f-e04c-461e-b2fa-6deb1ed9e36a", "Last Verified By");
		}

		#endregion

		#region ZeroCountOnlyFilter

		void AddZeroCountOnlyFilter(ModuleFilterCollection filters)
		{
			var zeroCountDescription = new string[] { Res.GetString("abc804fd-4aeb-4601-a2eb-a1909833ea7b", "Zero Count Only – Locations with Variance") };
			var zeroCountOnlyQuery = new GetFlagsQuery[] { GetZeroCountOnlyStocktakesQuery };
			var filter = filters.AddFlagsFilter("ZeroCountOnlyFilter", zeroCountDescription, zeroCountOnlyQuery);
			filter.Category = FilterCategories.NumbersAndReferences;
			filter.MultilingualDescription = ResString.GetMultilingualString("abc804fd-4aeb-4601-a2eb-a1909833ea7b", "Zero Count Only – Locations with Variance");
		}

		#endregion

		#region StocktakeTypeFilter

		void AddStocktakeTypeFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("StocktakeType", GetStocktakeTypeQuery, StocktakeTypeList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("e436ae98-ca12-4631-a888-f510bd9ef7e3", "Stocktake Type");
		}

		#endregion

		#endregion

		#region Properties

		#region Warehouse

		public ZGuid WS_WW_Whs => WarehouseFilter.IsActive ? WarehouseFilter.Property : ZGuid.Empty;

		ModuleGuidFilter WarehouseFilter => (ModuleGuidFilter)ModuleFilters["Warehouse"];

		#endregion

		#region Client

		public ZGuid WS_OH_Client => ClientFilter.IsActive ? ClientFilter.Property : ZGuid.Empty;

		ModuleGuidFilter ClientFilter => (ModuleGuidFilter)ModuleFilters["Client"];

		OrgHeader Client => Factory.Load<OrgHeader>(WS_OH_Client);

		#endregion

		#region StatusList

		CodeDescriptionPairList StatusList => new StocktakeStatus();

		#endregion

		#region StocktakeCycleList

		ReadOnlyCodeDescriptionPairList StocktakeCycleList => WarehouseDataRegistry.Instance.StocktakeCycle.Value;

		#endregion

		#region StocktakeTypeList

		ICodeDescriptionPairListWithDefaultCode StocktakeTypeList => WarehouseDataRegistry.Instance.StocktakeTypes.Value;

		#endregion

		#endregion

		#region Lookups

		#region Warehouses

		public WhsWarehouseCollectionWithSecurityCheck Warehouses => new WhsWarehouseCollectionWithSecurityCheck(Factory);

		#endregion

		#region Clients

		public WarehouseClientCollectionWithSecurityCheck Clients => new WarehouseClientCollectionWithSecurityCheck(Factory);

		#endregion

		#region GetProductsList

		WhsOrgSupplierPartCollection GetProductsList()
		{
			if (products == null || productsClient != Client)
			{
				productsClient = Client;
				products = new WhsOrgSupplierPartCollection(Factory, null, productsClient, false);
			}
			return products;
		}
		WhsOrgSupplierPartCollection products;
		OrgHeader productsClient;

		#endregion

		#region GetProductCategoriesList

		OrgPartCategoryCollection GetProductCategoriesList => new OrgPartCategoryCollection(Factory);
		#endregion

		#region CommodityCodes

		RefCommodityCodeCollection CommodityCodes => new RefCommodityCodeCollection(Factory);

		#endregion

		#region StaffList

		public GlbStaffCollection StaffList => new GlbStaffCollection(Factory);

		#endregion

		#endregion

		#region Queries

		#region GetStatusQuery

		ZQuery GetStatusQuery(ZString status)
		{
			return new ZQuery(WhsStocktakeSchema.WS_StocktakeStatus, status);
		}

		#endregion

		#region GetCycleQuery

		ZQuery GetCycleQuery(ZString cycle)
		{
			return new ZQuery(WhsStocktakeSchema.WS_StocktakeCycle, cycle);
		}

		#endregion

		#region GetStocktakeTypeQuery

		ZQuery GetStocktakeTypeQuery(ZString stocktakeType)
		{
			return new ZQuery(WhsStocktakeSchema.WS_StocktakeType, stocktakeType);
		}

		#endregion

		#region GetCommodityCodesQuery

		ZQuery GetCommodityCodesQuery(ZString commodityCode)
		{
			// commodity code on Stocktake
			var query = new ZDBOnlyQuery(typeof(WhsStocktake));
			query.AddToFilter(WhsStocktakeSchema.WS_RH_NKCommodityCode, commodityCode);

			// commodity code on stocktake line's product
			var lineSubQuery = new ZDBOnlySubQuery(typeof(WhsStocktakeLine), WhsStocktakeLineSchema.WU_WS);
			var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsStocktakeLineSchema.WU_OP);
			orgSubQuery.AddToFilter(OrgSupplierPartSchema.OP_RH_NKCommodityCode, commodityCode);
			lineSubQuery.AddSubQuery(orgSubQuery, JoinCondition.And);

			query.AddSubQuery(lineSubQuery, JoinCondition.Or);

			return query;
		}

		#endregion

		#region GetRowsQuery

		ZQuery GetRowsQuery(SQLComparisonOperator comparisonOperator, ZString rowName)
		{
			//Row on Stocktake
			var query = new ZDBOnlyQuery(typeof(WhsStocktake));
			var rowSubQuery = new ZDBOnlySubQuery(typeof(WhsRow), WhsStocktakeSchema.WS_WR_Row);
			rowSubQuery.AddToFilter(WhsRowSchema.WR_Name, comparisonOperator, rowName);
			query.AddSubQuery(rowSubQuery, JoinCondition.And);

			//Row on Stocktake lines
			var lineSubQuery = new ZDBOnlySubQuery(typeof(WhsStocktakeLine), WhsStocktakeLineSchema.WU_WS);
			var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsStocktakeLineSchema.WU_WL);
			var rowLineSubQuery = new ZDBOnlySubQuery(typeof(WhsRow), WhsLocationViewSchema.WLV_WR);
			rowLineSubQuery.AddToFilter(WhsRowSchema.WR_Name, comparisonOperator, rowName);
			locationSubQuery.AddSubQuery(rowLineSubQuery, JoinCondition.And);
			lineSubQuery.AddSubQuery(locationSubQuery, JoinCondition.And);

			query.AddSubQuery(lineSubQuery, JoinCondition.Or);

			return query;
		}

		#endregion

		#region GetAreaNameQuery

		ZQuery GetAreaNameQuery(SQLComparisonOperator comparisonOperator, ZString areaName)
		{
			//Area name on Stocktake
			var query = new ZDBOnlyQuery(typeof(WhsStocktake));
			var areaSubQuery = new ZDBOnlySubQuery(typeof(WhsArea), WhsStocktakeSchema.WS_WA_Area);
			areaSubQuery.AddToFilter(WhsAreaSchema.WA_Name, comparisonOperator, areaName);
			query.AddSubQuery(areaSubQuery, JoinCondition.And);

			//Area name on Stocktake lines
			var lineSubQuery = new ZDBOnlySubQuery(typeof(WhsStocktakeLine), WhsStocktakeLineSchema.WU_WS);
			var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsStocktakeLineSchema.WU_WL);
			var arealineSubQuery = new ZDBOnlySubQuery(typeof(WhsArea), WhsLocationViewSchema.WLV_WA_PickingArea);
			arealineSubQuery.AddToFilter(WhsAreaSchema.WA_Name, comparisonOperator, areaName);
			locationSubQuery.AddSubQuery(arealineSubQuery, JoinCondition.And);
			lineSubQuery.AddSubQuery(locationSubQuery, JoinCondition.And);

			query.AddSubQuery(lineSubQuery, JoinCondition.Or);

			return query;
		}

		#endregion

		#region GetStaffQuery

		ZQuery GetStaffQuery(ZGuid staffGuid)
		{
			var query = new ZDBOnlyQuery(typeof(WhsStocktake));

			var glbStaffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
			glbStaffSubQuery.AddToFilter(GlbStaffSchema.PK, staffGuid);

			var stocktakeLineSubQuery = new ZDBOnlySubQuery(typeof(WhsStocktakeLine), WhsStocktakeLineSchema.WU_WS);
			stocktakeLineSubQuery.AddSubQuery(WhsStocktakeLineSchema.WU_GS_NKVerifiedBy, glbStaffSubQuery, JoinCondition.Or);
			stocktakeLineSubQuery.AddSubQuery(WhsStocktakeLineSchema.WU_Count2VerifiedBy, glbStaffSubQuery, JoinCondition.Or);
			stocktakeLineSubQuery.AddSubQuery(WhsStocktakeLineSchema.WU_Count3VerifiedBy, glbStaffSubQuery, JoinCondition.Or);

			query.AddSubQuery(stocktakeLineSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region GetProductQuery

		ZQuery GetProductQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(WhsStocktake));

			var lineQuery = new ZDBOnlySubQuery(typeof(WhsStocktakeLine), WhsStocktakeLineSchema.WU_WS);
			ProductQueryHelper.AddProductToQuery(Factory, lineQuery, WhsStocktakeLineSchema.WU_OP, value);
			query.AddSubQuery(lineQuery, JoinCondition.And);

			var productFilterQuery = new ZDBOnlySubQuery(typeof(WhsStocktakeProductFilter), WhsStocktakeProductFilterSchema.WSP_WS_Stocktake);
			ProductQueryHelper.AddProductToQuery(Factory, productFilterQuery, WhsStocktakeProductFilterSchema.WSP_OP_Product, value);
			query.AddSubQuery(productFilterQuery, JoinCondition.Or);

			return query;
		}

		#endregion

		#region GetProductCategoryQuery

		ZQuery GetProductCategoryQuery(ZGuid value)
		{
			var relationTypeSubQuery = new ZQuery();
			relationTypeSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Owner);
			relationTypeSubQuery.AddToFilter(JoinCondition.Or, OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Both);

			var relationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			relationSubQuery.AddToFilter(relationTypeSubQuery, JoinCondition.And);

			var categoryPKs = new OrgPartProductCategoryHelper(Factory).ProductCategoryAndSubCategories(value);
			relationSubQuery.AddToFilter(OrgPartRelationSchema.OU_OPC_Category, categoryPKs);

			var query = new ZDBOnlyQuery(typeof(WhsStocktake));

			var lineQuery = new ZDBOnlySubQuery(typeof(WhsStocktakeLine), WhsStocktakeLineSchema.WU_WS);
			var lineSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsStocktakeLineSchema.WU_OP);
			lineSubQuery.AddSubQuery(relationSubQuery, JoinCondition.And);
			lineQuery.AddSubQuery(lineSubQuery, JoinCondition.And);
			query.AddSubQuery(lineQuery, JoinCondition.And);

			var productFilterQuery = new ZDBOnlySubQuery(typeof(WhsStocktakeProductFilter), WhsStocktakeProductFilterSchema.WSP_WS_Stocktake);
			var productFilterSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsStocktakeProductFilterSchema.WSP_OP_Product);
			productFilterSubQuery.AddSubQuery(relationSubQuery, JoinCondition.And);
			productFilterQuery.AddSubQuery(productFilterSubQuery, JoinCondition.And);
			query.AddSubQuery(productFilterQuery, JoinCondition.Or);

			return query;
		}

		#endregion

		#region GetPalletIDQuery

		ZQuery GetPalletIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var palletSubQuery = new ZDBOnlySubQuery(typeof(WhsStocktakeLine), WhsStocktakeLineSchema.WU_WS);
			palletSubQuery.AddToFilter(WhsStocktakeLineSchema.WU_PalletID, comparisonOperator, value);

			var stocktakeQuery = new ZDBOnlyQuery(typeof(WhsStocktake));
			stocktakeQuery.AddSubQuery(palletSubQuery, JoinCondition.And);
			return stocktakeQuery;
		}

		#endregion

		#region GetZeroCountOnlyStocktakesQuery

		ZQuery GetZeroCountOnlyStocktakesQuery(ZBool value)
		{
			var query = new ZDBOnlyQuery(typeof(WhsStocktake));

			if (value)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(WhsStocktakeLine), WhsStocktakeLineSchema.WU_WS);
				subQuery.PopulateZeroCountStocktakeLinesQuery();
				subQuery.AddToFilter(WhsStocktakeLineSchema.WU_OP, SQLComparisonOperator.NotEqual, null);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		#region GetStocktakeLinesAttributeQuery

		ZQuery GetStocktakeLinesAttributeQuery(SchemaColumn column, SQLComparisonOperator comparisonOperator, ZString attributeValue)
		{
			var query = new ZDBOnlyQuery(typeof(WhsStocktake));
			var subQuery = new ZDBOnlySubQuery(typeof(WhsStocktakeLine), WhsStocktakeLineSchema.WU_WS);
			subQuery.AddToFilter(column, comparisonOperator, attributeValue);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region SetUpStocktakeLinesDateQuery

		ZQuery GetStocktakeLinesDateQuery(SchemaDateTimeColumn column, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var query = new ZDBOnlyQuery(typeof(WhsStocktake));
			var subQuery = new ZDBOnlySubQuery(typeof(WhsStocktakeLine), WhsStocktakeLineSchema.WU_WS);
			AddDateRange(subQuery, comparisonOperator, JoinCondition.And, column, fromDate.Date, toDate.Date);
			AddDateRange(subQuery, comparisonOperator, JoinCondition.Or, WhsStocktakeLineSchema.WU_Count2DateVerified, fromDate.Date, toDate.Date);
			AddDateRange(subQuery, comparisonOperator, JoinCondition.Or, WhsStocktakeLineSchema.WU_Count3DateVerified, fromDate.Date, toDate.Date);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#endregion

		#region IAccountingFilterStripHolder Members

		IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (accountingFilterStrip == null)
				{
					accountingFilterStrip = (IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this);
					accountingFilterStrip.Initialize(addProfitLossReasonFilters: true);
				}

				return accountingFilterStrip;
			}
		}

		IAccountingFilterStrip accountingFilterStrip;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			var stocktakeQuery = new ZDBOnlyQuery(typeof(WhsStocktake));
			stocktakeQuery.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return stocktakeQuery;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => null;

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories => null;

		#endregion
	}
}
