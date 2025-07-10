using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class AdHocServiceJobFilterBusinessObject : FilterStripBusinessObject
	{
		#region FilterConstants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constant")]
		public static class FilterConstants
		{
			public const string Client = "Client";
			public const string Warehouse = "Warehouse";
			public const string BillingDate = "Billing Date";
			public const string WSJ_CustomerReference = "WSJ_CustomerReference";
			public const string AdHocJobNumber = "AdHocJobNumber";
		}

		#endregion

		#region GetModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddCustomerReferenceFilter(filters);
			AddClientFilter(filters);
			AddWarehouseFilter(filters);
			AddBillingDateFilter(filters);
			AddServiceTypeDateFilter(filters);

			return filters;
		}

		#region AddServiceTypeDateFilter

		void AddServiceTypeDateFilter(ModuleFilterCollection filters)
		{
			var serviceTypeDateBookedFilter = new ServiceTypeDateFilter(this, typeof(WhsAdHocServiceJob), true);
			filters.AddCustomFilter(serviceTypeDateBookedFilter);

			var serviceTypeDateCompletedFilter = new ServiceTypeDateFilter(this, typeof(WhsAdHocServiceJob), false);
			filters.AddCustomFilter(serviceTypeDateCompletedFilter);
		}

		#endregion

		#region AddCustomerReferenceFilter

		void AddCustomerReferenceFilter(ModuleFilterCollection filters)
		{
			var customerReferenceNoFilter = filters.AddTextFilter(FilterConstants.WSJ_CustomerReference, WhsAdHocServiceJobSchema.WSJ_CustomerReference);
			customerReferenceNoFilter.Category = FilterCategories.NumbersAndReferences;
			customerReferenceNoFilter.MultilingualDescription = ResString.GetMultilingualString("229ba3c6-1804-49aa-8b34-533a030e1d1d", "Customer Reference No.");
			customerReferenceNoFilter.MaxLength = WhsAdHocServiceJobSchema.WSJ_CustomerReference.MaxLength;
		}

		#endregion

		#region AddClientFilters

		void AddClientFilter(ModuleFilterCollection filters)
		{
			var clientFilter = filters.AddGuidFilter(FilterConstants.Client, ModuleIDs.Organisation, WhsAdHocServiceJobSchema.WSJ_OH_Client, new WarehouseClientCollectionWithSecurityCheck(Factory));
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("376496e0-6eb5-4c03-bc5c-d10058b927a8", "Client");
			clientFilter.Category = FilterCategories.Organisations;

			if (!Env.Security.WhsAllowedClients.IsAllowed)
			{
				clientFilter.Visibility = FilterVisibility.AlwaysVisible;
				clientFilter.PropertyValidation = ClientFilterValidation;
			}
		}

		void ClientFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("dd5fcd8e-d1f0-4ac0-a9e0-03f18e193fd3", "Please select a client to filter by"));
			}
		}

		#endregion

		#region AddWarehouseFilter

		void AddWarehouseFilter(ModuleFilterCollection filters)
		{
			var warehouseFilter = filters.AddGuidFilter(FilterConstants.Warehouse, ModuleIDs.WhsConfigWarehouse, WhsAdHocServiceJobSchema.WSJ_WW_Whs, new WhsWarehouseCollectionWithSecurityCheck(Factory));
			warehouseFilter.MultilingualDescription = ResString.GetMultilingualString("5117214f-132e-46a4-8352-5f807f81c48e", "Warehouse");
			warehouseFilter.Category = FilterCategories.Organisations;

			if (!Env.Security.WhsAllowedWarehouses.IsAllowed)
			{
				warehouseFilter.Visibility = FilterVisibility.AlwaysVisible;
				warehouseFilter.PropertyValidation = WarehouseFilterValidation;
			}
		}

		void WarehouseFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("f2240a0a-3640-4dd1-ab3c-1e9c0fa35514", "Please select a warehouse to filter by"));
			}
		}

		#endregion

		#region AddBillingDateFilter

		void AddBillingDateFilter(ModuleFilterCollection filters)
		{
			var billingDateFilter = filters.AddDateFilter(FilterConstants.BillingDate, WhsAdHocServiceJobSchema.WSJ_BillingDate);
			billingDateFilter.MultilingualDescription = ResString.GetMultilingualString("b7bf1f79-8ee9-452a-a889-db1820be85c9", "Billing Date");
		}

		#endregion

		#region GetModuleFilterThatOverridesAllOtherFilters

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleFountainFilter(FilterConstants.AdHocJobNumber, WhsAdHocServiceJobSchema.WSJ_JobNumber, "WI") { MultilingualDescription = ResString.GetMultilingualString("4A11B85B-3069-4293-8378-2AEAA2186E24", "Ad Hoc Job Number") };
		}

		#endregion

		#region ClientPK

		public ZGuid ClientPK => ClientFilter.IsActive ? ClientFilter.Property : ZGuid.Empty;

		ModuleGuidFilter ClientFilter => (ModuleGuidFilter)ModuleFilters[FilterConstants.Client];

		#endregion

		#region WarehousePK

		public ZGuid WarehousePK => WarehouseFilter.IsActive ? WarehouseFilter.Property : ZGuid.Empty;

		ModuleGuidFilter WarehouseFilter => (ModuleGuidFilter)ModuleFilters[FilterConstants.Warehouse];

		#endregion

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				var query = base.Filter;

				var adHocServiceJobQuery = new ZDBOnlyQuery(typeof(WhsAdHocServiceJob));
				var jobHeaderSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
				jobHeaderSubQuery.AddToFilter(JobHeaderSchema.JH_GC, SQLComparisonOperator.Equal, Env.CurrentCompanyPK);
				adHocServiceJobQuery.AddSubQuery(jobHeaderSubQuery, JoinCondition.And);
				query.AddToFilter(adHocServiceJobQuery);

				return query;
			}
		}

		#endregion

		#endregion
	}
}
