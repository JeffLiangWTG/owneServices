using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class OrderLineFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Client = "Client";
			public const string Warehouse = "Warehouse";
			public const string SubType = "Type";
			public const string Consignee = "Consignee";
			public const string OrderNo = "Order No.";
			public const string Status = "Status";
			public const string Product = "Product";
			public const string DeclarationReference = "Declaration Reference";
			public const string InwardStyle = "Inward Style";
			public const string InwardProcedure = "Inward Procedure";
			public const string CustomsEntryKey = "Customs Entry Key";
		}

		public OrderLineFilterBusinessObject()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var clientFilter = filters.AddGuidFilter(Schema.Client, ModuleIDs.Organisation, GetClientQuery, Clients);
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("F1502A12-0AC9-4372-AE5D-0F50156D4C05", "Client");

			var warehouseFilter = filters.AddGuidFilter(Schema.Warehouse, ModuleIDs.WhsConfigWarehouse, GetWarehouseQuery, Warehouses);
			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("E92C80D0-E953-4D45-BB01-61BED8718067", "Warehouse");

			var subTypeFilter = filters.AddTextFilter(Schema.SubType, GetSubTypeQuery, SubTypes);
			subTypeFilter.MultilingualDescription = ResString.GetMultilingualString("483C59B0-EC4B-42F0-8E82-8DA2638A497F", "Type");

			var consigneeFilter = filters.AddGuidFilter(Schema.Consignee, ModuleIDs.Organisation, GetConsigneeQuery, Consignees);
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("77913803-FA9C-4229-B415-6555BA356938", "Consignee");

			var orderNoFilter = filters.AddTextFilter(Schema.OrderNo, GetOrderNoQuery);
			orderNoFilter.MultilingualDescription = ResString.GetMultilingualString("F0D21F60-760D-4DB9-BE37-B5A4473B52D2", "Order No.");
			orderNoFilter.MaxLength = WhsDocketSchema.WD_ExternalReference.MaxLength;

			var statusFilter = filters.AddTextFilter(Schema.Status, GetStatusQuery, Statuses);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("19AE8616-6644-4BC2-9A0C-E30D2992113D", "Status");

			var productFilter = filters.AddGuidFilter(Schema.Product, ModuleIDs.WhsConfigProduct, GetProductQuery, Products);
			productFilter.MultilingualDescription = ResString.GetMultilingualString("A20296F2-A80B-4455-A9B1-92CF8AA79A96", "Product");

			var declarationReferenceFilter = filters.AddTextFilter(Schema.DeclarationReference, GetDeclarationReferenceQuery);
			declarationReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("99539B18-43BA-41D1-957F-45CA92236C69", "Declaration Reference");
			declarationReferenceFilter.MaxLength = WhsBondedWarehouseAttributeSchema.WB_DeclarationReference.MaxLength;

			var inwardStyleFilter = filters.AddTextFilter(Schema.InwardStyle, GetInwardStyleQuery);
			inwardStyleFilter.MultilingualDescription = ResString.GetMultilingualString("F97DDCA5-DF38-4E35-B9F2-D783F6478E7A", "Inward Style");
			inwardStyleFilter.MaxLength = WhsBondedWarehouseAttributeSchema.WB_InwardStyle.MaxLength;

			var inwardProcedureFilter = filters.AddTextFilter(Schema.InwardProcedure, GetInwardProcedureQuery);
			inwardProcedureFilter.MultilingualDescription = ResString.GetMultilingualString("2B8B6C15-3214-4670-A4C6-C1AC59B7A428", "Inward Procedure");
			inwardProcedureFilter.MaxLength = WhsBondedWarehouseAttributeSchema.WB_InwardProcedure.MaxLength;

			var customsEntryKeyFilter = filters.AddTextFilter(Schema.CustomsEntryKey, GetCustomsEntryKeyQuery);
			customsEntryKeyFilter.MultilingualDescription = ResString.GetMultilingualString("CD9289D5-9EBF-400E-9AD4-4260726AB024", "Customs Entry Key");
			customsEntryKeyFilter.MaxLength = WhsDocketLineSchema.WE_BondedEntryKey.MaxLength;

			return filters;
		}

		WarehouseClientCollectionWithSecurityCheck Clients => clients ?? (clients = new WarehouseClientCollectionWithSecurityCheck(Factory));
		WarehouseClientCollectionWithSecurityCheck clients;

		WhsWarehouseCollectionWithSecurityCheck Warehouses => warehouses ?? (warehouses = new WhsWarehouseCollectionWithSecurityCheck(Factory));
		WhsWarehouseCollectionWithSecurityCheck warehouses;

		ConsigneeCollection Consignees => consignees ?? (consignees = new ConsigneeCollection(Factory));
		ConsigneeCollection consignees;

		CodeDescriptionPairList SubTypes => subTypes ?? (subTypes = new OrderType());
		CodeDescriptionPairList subTypes;

		CodeDescriptionPairList Statuses => statuses ?? (statuses = GetStatusQuery());
		CodeDescriptionPairList statuses;

		CodeDescriptionPairList GetStatusQuery()
		{
			var statuses = WhsOrderHelper.OrderStatuses;
			statuses.RemoveAt(statuses.IndexOfCode(DocketStatus.Codes.New));
			return statuses;
		}

		WhsOrgSupplierPartCollection Products => products ?? (products = new WhsOrgSupplierPartCollection(Factory));
		WhsOrgSupplierPartCollection products;

		ZQuery GetClientQuery(ZGuid value)
		{
			return GetQueryWithOrderColumn(WhsDocketSchema.WD_OH_Client, value);
		}

		ZQuery GetWarehouseQuery(ZGuid value)
		{
			return GetQueryWithOrderColumn(WhsDocketSchema.WD_WW_Whs, value);
		}

		ZQuery GetSubTypeQuery(ZString value)
		{
			return GetQueryWithOrderColumn(WhsDocketSchema.WD_DocketSubType, value);
		}

		ZQuery GetConsigneeQuery(ZGuid value)
		{
			var query = GetOrderLineQuery();
			var orderSubQuery = GetOrderSubQuery();
			var docAddressQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(DocAddressTypes.Codes.ConsigneeAddress, value);
			orderSubQuery.AddSubQuery(docAddressQuery, JoinCondition.And);
			query.AddSubQuery(orderSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetOrderNoQuery(SQLComparisonOperator @operator, ZString value)
		{
			var query = GetOrderLineQuery();
			var orderSubQuery = GetOrderSubQuery();
			orderSubQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, @operator, value);
			query.AddSubQuery(orderSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetStatusQuery(ZString value)
		{
			var query = GetOrderLineQuery();
			var orderSubQuery = GetOrderSubQuery();

			var orderStatusSubQuery = new ZDBOnlySubQuery(typeof(WhsOrderStatusView), WhsOrderStatusViewSchema.PK);
			orderStatusSubQuery.AddToFilter(WhsOrderStatusViewSchema.WOS_OrderStatus, value);
			orderSubQuery.AddSubQuery(WhsDocketSchema.PK, orderStatusSubQuery, JoinCondition.And);

			query.AddSubQuery(orderSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetProductQuery(ZGuid value)
		{
			var query = GetOrderLineQuery();
			ProductQueryHelper.AddProductToQuery(Factory, query, WhsDocketLineSchema.WE_OP, value);
			return query;
		}

		ZQuery GetDeclarationReferenceQuery(SQLComparisonOperator @operator, ZString value)
		{
			return GetQueryWithWhsAttributeColumn(WhsBondedWarehouseAttributeSchema.WB_DeclarationReference, @operator, value);
		}

		ZQuery GetInwardStyleQuery(SQLComparisonOperator @operator, ZString value)
		{
			return GetQueryWithWhsAttributeColumn(WhsBondedWarehouseAttributeSchema.WB_InwardStyle, @operator, value);
		}

		ZQuery GetInwardProcedureQuery(SQLComparisonOperator @operator, ZString value)
		{
			return GetQueryWithWhsAttributeColumn(WhsBondedWarehouseAttributeSchema.WB_InwardProcedure, @operator, value);
		}

		ZQuery GetCustomsEntryKeyQuery(SQLComparisonOperator @operator, ZString value)
		{
			return GetQueryWithColumn(WhsDocketLineSchema.WE_BondedEntryKey, @operator, value);
		}

		ZQuery GetQueryWithColumn(SchemaColumn column, SQLComparisonOperator @operator, object value)
		{
			var query = GetOrderLineQuery();
			query.AddToFilter(column, @operator, value);
			return query;
		}

		ZQuery GetQueryWithOrderColumn(SchemaColumn orderColumn, object value)
		{
			var query = GetOrderLineQuery();
			var orderSubQuery = GetOrderSubQuery();
			orderSubQuery.AddToFilter(orderColumn, value);
			query.AddSubQuery(orderSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetQueryWithWhsAttributeColumn(SchemaColumn whsAttrColumn, SQLComparisonOperator @operator, object value)
		{
			var query = GetOrderLineQuery();
			var whsAttrSubQuery = new ZDBOnlySubQuery(typeof(WhsBondedWarehouseAttribute), WhsBondedWarehouseAttributeSchema.WB_ParentID);
			whsAttrSubQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentTableCode, WhsDocketLineSchema.Constants.Prefix);
			whsAttrSubQuery.AddToFilter(whsAttrColumn, @operator, value);
			query.AddSubQuery(whsAttrSubQuery, JoinCondition.And);
			return query;
		}

		ZDBOnlyQuery GetOrderLineQuery()
		{
			var query = new ZDBOnlyQuery(typeof(WhsOrderLine));
			query.AddToFilter(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Order);
			return query;
		}

		ZDBOnlySubQuery GetOrderSubQuery()
		{
			var orderSubQuery = new ZDBOnlySubQuery(typeof(WhsOrder), WhsDocketLineSchema.WE_WD);
			orderSubQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
			return orderSubQuery;
		}
	}
}
