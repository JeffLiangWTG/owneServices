using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class VASOrderFilterBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		#region FilterConstants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constant")]
		public static class FilterConstants
		{
			public const string Client = "Client";
			public const string CustomerReferenceNo = "CustomerReferenceNo";
			public const string Product = "Product";
			public const string ProductCategory = "Product Category";
			public const string ProductPartAttribute1 = "Product Part Attribute 1";
			public const string ProductPartAttribute2 = "Product Part Attribute 2";
			public const string ProductPartAttribute3 = "Product Part Attribute 3";
			public const string ProductSerialNumber = "Product Serial Number";
			public const string ProductPackingDate = "Product Packing Date";
			public const string ProductExpiryDate = "Product Expiry Date";
			public const string ServiceArea = "Service Area";
			public const string Status = "Status";
			public const string VASOrderJobID = "VAS Order Job ID";
			public const string Warehouse = "Warehouse";
		}

		#endregion

		#region GetModuleFilterThatOverridesAllOtherFilters

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter(FilterConstants.VASOrderJobID, WhsVASOrderSchema.WVO_JobID, "WV") { MultilingualDescription = ResString.GetMultilingualString("b15de1cb-dd45-4405-802d-df5470706ec1", "VAS Order Job ID") };
		}

		#endregion

		#region GetModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddTextFilters(filters);
			AddClientFilter(filters);
			AddWarehouseFilter(filters);
			AddDateFilters(filters);
			AddPartAttributeFilters(filters);
			AddProductFilter(filters);
			AddProductCategoryFilter(filters);
			AddServiceAreaFilter(filters);
			AddStatusFilter(filters);
			AddActiveStatusFilter(filters);
			AccountingFilterStrip.AddBillingFilters(filters);
			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.WhsVASOrderJobInvoicing);
			AddServiceTypeDateFilter(filters);

			return filters;
		}

		#endregion

		#region AddTextFilters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var customerReferenceNoFilter = filters.AddTextFilter(FilterConstants.CustomerReferenceNo, WhsVASOrderSchema.WVO_CustomerReferenceNo);
			customerReferenceNoFilter.Category = FilterCategories.NumbersAndReferences;
			customerReferenceNoFilter.MultilingualDescription = ResString.GetMultilingualString("229ba3c6-1804-49aa-8b34-533a030e1d1d", "Customer Reference No.");
		}

		#endregion

		#region AddServiceTypeDateFilter

		void AddServiceTypeDateFilter(ModuleFilterCollection filters)
		{
			var serviceTypeDateBookedFilter = new ServiceTypeDateFilter(this, typeof(WhsVASOrder), true);
			filters.AddCustomFilter(serviceTypeDateBookedFilter);

			var serviceTypeDateCompletedFilter = new ServiceTypeDateFilter(this, typeof(WhsVASOrder), false);
			filters.AddCustomFilter(serviceTypeDateCompletedFilter);
		}

		#endregion

		#region AddDateFilters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			AddDateOnLinesFilter(filters, FilterConstants.ProductPackingDate, ResString.GetMultilingualString("695885e7-152b-4512-8b04-810eab2fca59", "Product Packing Date"), WhsVASOrderLineSchema.WVL_PackingDate);
			AddDateOnLinesFilter(filters, FilterConstants.ProductExpiryDate, ResString.GetMultilingualString("f794dc55-5bde-4d6f-b20c-9edbbe77e0bb", "Product Expiry Date"), WhsVASOrderLineSchema.WVL_ExpiryDate);
		}

		void AddDateOnLinesFilter(ModuleFilterCollection filters, string filterConstant, MultilingualString description, SchemaDateTimeColumn date)
		{
			var dateFilter = filters.AddDateFilter(filterConstant, date);
			dateFilter.Category = FilterCategories.Dates;
			dateFilter.MultilingualDescription = description;
			dateFilter.SubGroup = LinesSubGroup;
		}

		#endregion

		#region AddClientFilters

		void AddClientFilter(ModuleFilterCollection filters)
		{
			var clientFilter = filters.AddGuidFilter(FilterConstants.Client, ModuleIDs.Organisation, WhsVASOrderSchema.WVO_OH_Client, new WarehouseClientCollectionWithSecurityCheck(Factory));
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("376496e0-6eb5-4c03-bc5c-d10058b927a8", "Client");
			clientFilter.Category = FilterCategories.Organisations;
		}

		#endregion

		#region AddWarehouseFilters

		void AddWarehouseFilter(ModuleFilterCollection filters)
		{
			var warehouseFilter = filters.AddGuidFilter(FilterConstants.Warehouse, ModuleIDs.WhsConfigWarehouse, GetWarehouseQuery, new WhsWarehouseCollectionWithSecurityCheck(Factory));
			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("3B5A20B4-0759-488E-97D5-D8C815A55BB6", "Warehouse");
		}

		ZQuery GetWarehouseQuery(ZGuid value)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(WhsArea), WhsVASOrderSchema.WVO_WA_ServiceArea);
			subQuery.AddToFilter(WhsAreaSchema.WA_WW_Whs, value);

			var result = new ZDBOnlyQuery(typeof(WhsVASOrder));
			result.AddSubQuery(subQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#region AddPartAttributeFilters

		void AddPartAttributeFilters(ModuleFilterCollection filters)
		{
			AddPartAttributeFilter(filters, FilterConstants.ProductPartAttribute1, ResString.GetMultilingualString("e4e88208-25b6-4d2c-a135-f7340d355148", "Product Part Attribute 1"), WhsVASOrderLineSchema.WVL_PartAttrib1);
			AddPartAttributeFilter(filters, FilterConstants.ProductPartAttribute2, ResString.GetMultilingualString("708f3cb1-9a1b-4a7b-830a-890f7b35b545", "Product Part Attribute 2"), WhsVASOrderLineSchema.WVL_PartAttrib2);
			AddPartAttributeFilter(filters, FilterConstants.ProductPartAttribute3, ResString.GetMultilingualString("a406c0ae-c427-4738-9219-98bb05f89d49", "Product Part Attribute 3"), WhsVASOrderLineSchema.WVL_PartAttrib3);
			AddPartAttributeFilter(filters, FilterConstants.ProductSerialNumber, ResString.GetMultilingualString("733a0be3-d83f-4e8b-a3cd-7ee5eea559f3", "Product Serial Number"), WhsVASOrderLineSchema.WVL_SerialNumber);
		}

		void AddPartAttributeFilter(ModuleFilterCollection filters, string filterConstant, MultilingualString description, SchemaStringColumn partAttribute)
		{
			var partAttributeFilter = filters.AddTextFilter(filterConstant, partAttribute);
			partAttributeFilter.Category = FilterCategories.AttributeSearch;
			partAttributeFilter.MultilingualDescription = description;
			partAttributeFilter.SubGroup = LinesSubGroup;
		}

		#endregion

		#region AddProductFilters

		void AddProductFilter(ModuleFilterCollection filters)
		{
			var productFilter = filters.AddGuidFilter(FilterConstants.Product, ModuleIDs.WhsConfigProduct, WhsVASOrderLineSchema.WVL_OP_Product, new WhsOrgSupplierPartCollection(Factory));
			productFilter.MultilingualDescription = ResString.GetMultilingualString("a0ea317d-f857-403e-8549-936c22bdc089", "Product");
			productFilter.SubGroup = LinesSubGroup;
		}

		#endregion

		#region AddProductCategoryFilter

		void AddProductCategoryFilter(ModuleFilterCollection filters)
		{
			var categoryFilter = filters.AddGuidFilter(FilterConstants.ProductCategory, ModuleIDs.RefOrgPartCategory, GetProdutCategoryQuery, new OrgPartCategoryCollection(Factory));
			categoryFilter.MultilingualDescription = ResString.GetMultilingualString("D3AAAABD-CE9D-44E3-B894-E93669451649", "Product Category");
			categoryFilter.SubGroup = LinesSubGroup;
			categoryFilter.IsPublishedOnWeb = false;
		}

		ZQuery GetProdutCategoryQuery(ZGuid value)
		{
			var relationTypeSubQuery = new ZQuery();
			relationTypeSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Owner);
			relationTypeSubQuery.AddToFilter(JoinCondition.Or, OrgPartRelationSchema.OU_Relationship, SQLComparisonOperator.Equal, OrgPartRelation.RelationshipTypes.Both);

			var relationSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
			relationSubQuery.AddToFilter(relationTypeSubQuery, JoinCondition.And);

			var categoryPKs = new OrgPartProductCategoryHelper(Factory).ProductCategoryAndSubCategories(value);
			relationSubQuery.AddToFilter(OrgPartRelationSchema.OU_OPC_Category, categoryPKs);

			var productSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), WhsVASOrderLineSchema.WVL_OP_Product);
			productSubQuery.AddSubQuery(relationSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsVASOrderLine));
			query.AddSubQuery(productSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region AddServiceAreaFilter

		void AddServiceAreaFilter(ModuleFilterCollection filters)
		{
			var warehouseFilter = filters.AddGuidFilter(FilterConstants.ServiceArea, ModuleIDs.WhsConfigArea, WhsVASOrderSchema.WVO_WA_ServiceArea, new WhsAreaCollection(Factory));
			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("a2777f05-e17e-47f0-bfdf-aaa1c7af2285", "Service Area");
		}

		#endregion

		#region AddStatusFilter

		void AddStatusFilter(ModuleFilterCollection filters)
		{
			var statusList = new WhsVASOrderStatuses();
			statusList.RemoveCode(WhsVASOrderStatuses.Codes.Cancelled);

			var statusFilter = filters.AddTextFilter(FilterConstants.Status, GetStatusQuery, statusList);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("02655b1a-636d-4989-924c-5e66c2774a25", "Status");
			statusFilter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery GetStatusQuery(ZString value)
		{
			switch (value)
			{
				case CodeLists.WhsVASOrderStatuses.Codes.Entered:
					var enteredQuery = new ZQuery(WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea, null);
					enteredQuery.AddToFilter(WhsVASOrderSchema.WVO_WD_TransferOutOfServiceArea, null);
					enteredQuery.AddToFilter(WhsVASOrderSchema.WVO_CancelledTimeUtc, SQLComparisonOperator.Equal, ZDateTime.Empty);
					return enteredQuery;

				case CodeLists.WhsVASOrderStatuses.Codes.TransferringIn:
					return GetVASOrderBasedOnTransferStatus(WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea, SQLComparisonOperator.NotEqual);

				case CodeLists.WhsVASOrderStatuses.Codes.Working:
					var workingQuery = GetVASOrderBasedOnTransferStatus(WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea, SQLComparisonOperator.Equal);
					workingQuery.AddToFilter(WhsVASOrderSchema.WVO_WorkCompletedTimeUtc, SQLComparisonOperator.Equal, ZDateTime.Empty);
					return workingQuery;

				case CodeLists.WhsVASOrderStatuses.Codes.WorkCompleted:
					var workCompletedQuery = new ZQuery(WhsVASOrderSchema.WVO_WorkCompletedTimeUtc, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
					workCompletedQuery.AddToFilter(WhsVASOrderSchema.WVO_FinalizedTimeUtc, SQLComparisonOperator.Equal, ZDateTime.Empty);
					workCompletedQuery.AddToFilter(WhsVASOrderSchema.WVO_WD_TransferOutOfServiceArea, null);
					return workCompletedQuery;

				case CodeLists.WhsVASOrderStatuses.Codes.TransferringOut:
					return GetVASOrderBasedOnTransferStatus(WhsVASOrderSchema.WVO_WD_TransferOutOfServiceArea, SQLComparisonOperator.NotEqual);

				case CodeLists.WhsVASOrderStatuses.Codes.Finalized:
					return new ZQuery(WhsVASOrderSchema.WVO_FinalizedTimeUtc, SQLComparisonOperator.NotEqual, ZDateTime.Empty);

				default:
					return ZQuery.NoResultQuery;
			}
		}

		ZDBOnlyQuery GetVASOrderBasedOnTransferStatus(SchemaGuidColumn transferColumn, SQLComparisonOperator finalised)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(WhsTransfer), transferColumn);
			subQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, finalised, DocketStatus.Codes.Finalised);

			var query = new ZDBOnlyQuery(typeof(WhsVASOrder));
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region AddActiveStatusFilter

		protected override ZQuery GetActiveStatusQueryCore(ZString status)
		{
			var zQuery = new ZQuery();
			var trimmedStatus = status.Trim();
			if (StatusInactive.EqualsUnresolvedOrLocalized(trimmedStatus, ignoreCase: true))
			{
				zQuery.AddToFilter(WhsVASOrderSchema.WVO_CancelledTimeUtc, SQLComparisonOperator.NotEqual, null);
			}
			else if (StatusActive.EqualsUnresolvedOrLocalized(trimmedStatus, ignoreCase: true))
			{
				zQuery.AddToFilter(WhsVASOrderSchema.WVO_CancelledTimeUtc, SQLComparisonOperator.Equal, null);
			}

			return zQuery;
		}

		#endregion

		#region LinesSubGroup

		ModuleFilterSubGroup LinesSubGroup
		{
			get { return linesSubGroup ?? (linesSubGroup = new LinesFilterSubGroup()); }
		}
		ModuleFilterSubGroup linesSubGroup;

		class LinesFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var subFilter = new ZDBOnlySubQuery(typeof(WhsVASOrderLine), WhsVASOrderLineSchema.WVL_WVO_VASOrder);
				subFilter.AddToFilter(filter);

				var vasOrderFilter = new ZDBOnlyQuery(typeof(WhsVASOrder));
				vasOrderFilter.AddSubQuery(subFilter, JoinCondition.And);

				return vasOrderFilter;
			}
		}

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
			var vasOrderQuery = new ZDBOnlyQuery(typeof(WhsVASOrder));
			vasOrderQuery.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return vasOrderQuery;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration =>
				new Dictionary<string, object>() { { AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("VASOrderFilterBusinessObject|InvoiceStatus", "Invoice Status") } };

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories => null;

		#endregion	
	}
}
