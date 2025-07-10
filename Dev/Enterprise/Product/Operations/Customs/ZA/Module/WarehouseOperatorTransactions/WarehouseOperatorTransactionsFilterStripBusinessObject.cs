using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Module
{
	public class WarehouseOperatorTransactionsFilterStripBusinessObject
		: FilterStripBusinessObject
	{
		public static class FilterConstants
		{
			public const string ProductCode = "Product Code";
			public const string Status = "Status";
			public const string TransactionType = "Transaction Type";
			public const string OwnerReference = "Owner Reference";
			public const string ExportType = "Export Type";
			public const string IsCustomsControlled = "Is Customs Controlled?";
			public const string TransactionDate = "Transaction Date";
			public const string BatchNumber = "Batch Number";
			public const string LineReference = "Line Reference";
			public const string CountryOfOrigin = "Country/Region Of Origin";
			public const string CustomsEntryNumber = "Customs Reference Number";
			public const string IsSystemCreated = "Is System Created?";
			public const string Warehouse = "Warehouse";
			public const string ProductOwner = "Product Owner";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			var batchSubGroupForCusWHSOperatorTransactionBatch = new BatchSubGroupForCusWHSOperatorTransactionBatch();
			var batchSubGroupForOrgSupplierPart = new BatchSubGroupForOrgSupplierPart();
			var singleFlagText = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|TickedForSetUntickedForUnset", "Ticked for set, unticked for unset");

			var productCodeFilter = result.AddTextFilter(FilterConstants.ProductCode, OrgSupplierPartSchema.OP_PartNum);
			productCodeFilter.Category = FilterCategories.NumbersAndReferences;
			productCodeFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|ProductCode", FilterConstants.ProductCode);
			productCodeFilter.SubGroup = batchSubGroupForOrgSupplierPart;

			var statusFilter = result.AddTextFilter(FilterConstants.Status, CusWHSOperatorTransactionSchema.WOT_Status, () => new WarehouseOperatorTransactionStatusList());
			statusFilter.Category = FilterCategories.NumbersAndReferences;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|Status", FilterConstants.Status);

			var transactionTypeFilter = result.AddTextFilter(FilterConstants.TransactionType, CusWHSOperatorTransactionSchema.WOT_TransactionType, () => new WarehouseOperatorTransactionTypeList());
			transactionTypeFilter.Category = FilterCategories.NumbersAndReferences;
			transactionTypeFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|TransactionType", FilterConstants.TransactionType);

			var ownerReferenceFilter = result.AddTextFilter(FilterConstants.OwnerReference, CusWHSOperatorTransactionSchema.WOT_OwnerReference);
			ownerReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			ownerReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|OwnerReference", FilterConstants.OwnerReference);

			var exportTypeFilter = result.AddTextFilter(FilterConstants.ExportType, CusWHSOperatorTransactionSchema.WOT_ExportType, () => new WarehouseOperatorTransactionExportTypeList());
			exportTypeFilter.Category = FilterCategories.NumbersAndReferences;
			exportTypeFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|ExportType", FilterConstants.ExportType);

			var batchNumberFilter = result.AddTextFilter(FilterConstants.BatchNumber, CusWHSOperatorTransactionBatchSchema.WOB_Batch);
			batchNumberFilter.Category = FilterCategories.NumbersAndReferences;
			batchNumberFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|BatchNumber", FilterConstants.BatchNumber);
			batchNumberFilter.SubGroup = batchSubGroupForCusWHSOperatorTransactionBatch;

			var isCustomsControlledFilter = result.AddFlagsFilter(FilterConstants.IsCustomsControlled, new string[] { singleFlagText }, new GetFlagsQuery[] { GetIsCustomsControlledQuery });
			isCustomsControlledFilter.Category = FilterCategories.StatusAndFlags;
			isCustomsControlledFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|IsCustomsControlled", FilterConstants.IsCustomsControlled);

			var transactionDateFilter = result.AddDateFilter(FilterConstants.TransactionDate, CusWHSOperatorTransactionSchema.WOT_TransactionDate);
			transactionDateFilter.Category = FilterCategories.Dates;
			transactionDateFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|TransactionDate", FilterConstants.TransactionDate);

			var lineReferenceFilter = result.AddTextFilter(FilterConstants.LineReference, CusWHSOperatorTransactionSchema.WOT_LineReference);
			lineReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			lineReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|LineReference", FilterConstants.LineReference);

			var countryOfOriginFilter = result.AddNkFilter(FilterConstants.CountryOfOrigin, CusWHSOperatorTransactionSchema.WOT_RN_NKOrigin, ModuleIDs.RefCountry, new RefCountryCollection(Factory));
			countryOfOriginFilter.Category = FilterCategories.NumbersAndReferences;
			countryOfOriginFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|CountryOfOrigin", FilterConstants.CountryOfOrigin);

			var customsEntryNumberFilter = result.AddTextFilter(FilterConstants.CustomsEntryNumber, CusWHSOperatorTransactionSchema.WOT_CustomsEntryNumber);
			customsEntryNumberFilter.Category = FilterCategories.NumbersAndReferences;
			customsEntryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|CustomsEntryNumber", FilterConstants.CustomsEntryNumber);

			var isSystemCreatedFilter = result.AddFlagsFilter(FilterConstants.IsSystemCreated, new string[] { singleFlagText }, new GetFlagsQuery[] { GetIsSystemCreatedQuery });
			isSystemCreatedFilter.Category = FilterCategories.StatusAndFlags;
			isSystemCreatedFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|IsSystemCreated", FilterConstants.IsSystemCreated);
			isSystemCreatedFilter.SubGroup = batchSubGroupForCusWHSOperatorTransactionBatch;

			var warehouseFilter = result.AddGuidFilter(FilterConstants.Warehouse, ModuleIDs.WhsConfigWarehouse, whsPk => GetWarehouseQuery(whsPk), Warehouses);
			warehouseFilter.Category = FilterCategories.Organisations;
			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|Warehouse", FilterConstants.Warehouse);
			warehouseFilter.SubGroup = batchSubGroupForCusWHSOperatorTransactionBatch;
			warehouseFilter.Visibility = FilterVisibility.AlwaysVisible;
			warehouseFilter.PropertyValidation = WarehouseValidation;

			var productOwnerFilter = result.AddGuidFilter(FilterConstants.ProductOwner, ModuleIDs.Organisation, orgPK => GetProductOwnerQuery(orgPK), ProductOwners);
			productOwnerFilter.Category = FilterCategories.Organisations;
			productOwnerFilter.MultilingualDescription = ResString.GetMultilingualString("WarehouseOperatorTransactionsFilter|ProductOwner", FilterConstants.ProductOwner);
			productOwnerFilter.Visibility = FilterVisibility.AlwaysVisible;
			productOwnerFilter.PropertyValidation = ProductOwnerValidation;

			return result;
		}

		WhsWarehouseCollection Warehouses => warehouses ?? (warehouses = new WhsWarehouseCollection(Factory, WarehouseCollectionType.All));
		WhsWarehouseCollection warehouses;

		OrgHeaderCollection ProductOwners => productOwners ?? (productOwners = new OrgHeaderCollection(Factory));
		OrgHeaderCollection productOwners;

		ZQuery GetWarehouseQuery(ZGuid value)
		{
			var query = new ZDBOnlyQuery(typeof(CusWHSOperatorTransactionBatch));
			var warehouseSubQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsWarehouseSchema.WW_OA_WarehouseAddress);
			warehouseSubQuery.AddToFilter(WhsWarehouseSchema.PK, value);
			query.AddSubQuery(CusWHSOperatorTransactionBatchSchema.WOB_OA_Warehouse, warehouseSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetProductOwnerQuery(ZGuid value)
		{
			var query = new ZQuery();
			query.AddToFilter(CusWHSOperatorTransactionSchema.WOT_OH_ProductOwner, value);

			return query;
		}

		ZQuery GetIsSystemCreatedQuery(ZBool value)
		{
			var query = new ZQuery();
			query.AddToFilter(CusWHSOperatorTransactionBatchSchema.WOB_IsSystemCreated, value);
			return query;
		}

		ZQuery GetIsCustomsControlledQuery(ZBool value)
		{
			var query = new ZQuery();
			query.AddToFilter(CusWHSOperatorTransactionSchema.WOT_IsCustomsControlled, value ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, true);
			return query;
		}

		void WarehouseValidation(ZPropertyInfo info)
		{
			var value = (ZGuid)info.Value;
			if (!value.IsValid)
			{
				info.AddError(Business.ValidationConstants.WarehouseOperatorTransactionsFilterStripBusinessObject.WarehouseFilter);
			}
		}

		void ProductOwnerValidation(ZPropertyInfo info)
		{
			var value = (ZGuid)info.Value;
			if (!value.IsValid)
			{
				info.AddError(Business.ValidationConstants.WarehouseOperatorTransactionsFilterStripBusinessObject.ProductOwnerFilter);
			}
		}

		class BatchSubGroupForCusWHSOperatorTransactionBatch : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(CusWHSOperatorTransaction));
				var subQuery = new ZDBOnlySubQuery(typeof(CusWHSOperatorTransactionBatch), CusWHSOperatorTransactionSchema.WOT_WOB_CusWHSTransactionBatch);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}

		class BatchSubGroupForOrgSupplierPart : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(CusWHSOperatorTransaction));
				var subQuery = new ZDBOnlySubQuery(typeof(Customs.Business.OrgSupplierPart), CusWHSOperatorTransactionSchema.WOT_OP_Product);
				subQuery.AddToFilter(filter);
				query.AddSubQuery(subQuery, JoinCondition.And);
				return query;
			}
		}
	}
}
