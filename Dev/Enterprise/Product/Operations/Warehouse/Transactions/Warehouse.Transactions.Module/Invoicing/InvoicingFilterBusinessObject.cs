using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class InvoicingFilterBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		public InvoicingFilterBusinessObject()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter description")]
		const string MissingJobHeaderFilterName = "Missing Job Header";

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddClientFilter(filters);
			AddWarehouseFilter(filters);
			AddMissingJobHeaderFilter(filters);
			AddOffBandProcessingStatusFilter(filters);

			filters.AddFountainFilter("Invoice Number", JobStorageSchema.ET_StorageJobNumber, "I").MultilingualDescription = ResString.GetMultilingualString("a79536fd-30ee-4aab-ad15-db12f7a73dcb", "Invoice Number");
			filters.AddDateFilter("Billing Date", JobStorageSchema.ET_BillingDate).MultilingualDescription = ResString.GetMultilingualString("e8452ebc-b753-4b6b-bed2-601d8204ca4e", "Billing Date");
			filters.AddDateFilter("From Date", JobStorageSchema.ET_StorageFromDate).MultilingualDescription = ResString.GetMultilingualString("791eec86-f6e8-45e5-923d-eaf2646d4459", "From Date");
			filters.AddDateFilter("To Date", JobStorageSchema.ET_StorageToDate).MultilingualDescription = ResString.GetMultilingualString("c031eb90-5515-4cc2-9683-9e13b4b99d7d", "To Date");

			AccountingFilterStrip.AddBillingFilters(filters);
			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.WhsInvoicingJobInvoicing);

			return filters;
		}

		#region AddMissingJobHeaderFilter

		void AddMissingJobHeaderFilter(ModuleFilterCollection filters)
		{
			var missingJobHeaderFilter = filters.AddFlagsFilter(MissingJobHeaderFilterName, new string[] { Res.GetString("75542915-f78b-44df-a26d-ca4533072a1b", "Missing Job Header") }, new GetFlagsQuery[] { GetNoJobHeaderQuery });
			missingJobHeaderFilter.MultilingualDescription = ResString.GetMultilingualString("76c5e44f-02fc-4d13-9fa0-8cb8122f413a", "Missing Job Header");
			missingJobHeaderFilter.Property0 = true;
		}

		#endregion

		#region Warehouse Filter

		void AddWarehouseFilter(ModuleFilterCollection filters)
		{
			ModuleGuidFilter warehouseFilter = filters.AddGuidFilter("Warehouse", ModuleIDs.WhsConfigWarehouse, JobStorageSchema.ET_WW, Warehouses);
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
				info.AddError(Res.GetString("a28e8931-deb9-45ba-82c0-0af276f89060", "Please select a warehouse to filter by"));
			}
		}

		#endregion

		#region Client Filter

		void AddClientFilter(ModuleFilterCollection filters)
		{
			var clientFilter = filters.AddGuidFilter("Client", ModuleIDs.Organisation, JobStorageSchema.ET_OH_Client, Clients);
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("b206433a-ba4a-4da6-8725-a91d0bfb90fe", "Client");

			if (!Env.Security.WhsAllowedClients.IsAllowed && !Globals.IsWeb)
			{
				clientFilter.Visibility = FilterVisibility.AlwaysVisible;
				clientFilter.PropertyValidation = ClientFilterValidation;
			}
		}

		void ClientFilterValidation(ZPropertyInfo info)
		{
			if (info.Value.IsEmpty)
			{
				info.AddError(Res.GetString("1632f9b7-dedd-4057-ba44-0a73ce210526", "Please select a client to filter by"));
			}
		}

		#endregion

		#region AddOffBandProcessingStatusFilter

		void AddOffBandProcessingStatusFilter(ModuleFilterCollection filters)
		{
			var filter = new TextFilterExactOrNotEqual(JobStorageSchema.Constants.ET_OffBandProcessingStatus, JobStorageSchema.ET_OffBandProcessingStatus, StorageOffBandProcessingStatuses);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("InvoicingFilterBusinessObject|ET_OffBandProcessingStatus", "Off Band Processing Status");
			filter.DefaultProperty = StorageOffBandProcessingStatus.Codes.NIQ;
			filter.Visibility = filter.Visibility = FilterVisibility.AlwaysApplied | FilterVisibility.AlwaysVisible;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filters.AddFilter(filter);
		}

		#region StorageOffBandProcessingStatuses

		public StorageOffBandProcessingStatus StorageOffBandProcessingStatuses => Factory.GetCachedValue("InvoicingFilterBusinessObject|OffBandProcessingStatus", () => new StorageOffBandProcessingStatus());

		#endregion

		#endregion

		#region GetNoJobHeaderQuery

		protected ZQuery GetNoJobHeaderQuery(ZBool value)
		{
			var jobStorageQuery = new ZDBOnlyQuery(typeof(JobStorage));
			if (value)
			{
				var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, notIn: true);
				jobStorageQuery.AddSubQuery(subQuery, JoinCondition.And);
			}

			return jobStorageQuery;
		}
		#endregion

		#endregion

		#region Properties

		#region Filter

		public override ZQuery Filter
		{
			get
			{
				var query = new ZDBOnlyQuery(typeof(WhsInvoice));
				query.AddToFilter(base.Filter);

				var filter = (ModuleFlagsFilter)this[MissingJobHeaderFilterName];
				if (filter == null || !filter.IsActive || !filter.Property0)
				{
					var subQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
					subQuery.AddToFilter(JobHeaderSchema.JH_ParentTableCode, JobStorageSchema.Constants.Prefix);
					subQuery.AddToFilter(JobHeaderSchema.JH_GC, Env.CurrentCompany.PK);

					query.AddSubQuery(subQuery, JoinCondition.And);
				}

				return query;
			}
		}

		#endregion

		#region Warehouse

		public ZGuid ET_WW
		{
			get { return WarehouseFilter.IsActive ? WarehouseFilter.Property : ZGuid.Empty; }
#if DEBUG
			set
			{
				if (!ActiveModuleFilters.Contains(WarehouseFilter))
				{
					WarehouseFilter.IsActive = true;
				}
				WarehouseFilter.Property = value;
			}
#endif
		}

		ModuleGuidFilter WarehouseFilter
		{
			get { return (ModuleGuidFilter)ModuleFilters["Warehouse"]; }
		}

		#endregion

		#region Client

		public ZGuid ET_OH_Client
		{
			get { return ClientFilter.IsActive ? ClientFilter.Property : ZGuid.Empty; }
#if DEBUG
			set
			{
				if (!ActiveModuleFilters.Contains(ClientFilter))
				{
					ClientFilter.IsActive = true;
				}
				ClientFilter.Property = value;
			}
#endif
		}

		ModuleGuidFilter ClientFilter
		{
			get { return (ModuleGuidFilter)ModuleFilters["Client"]; }
		}

		#endregion

		#endregion

		#region Lookups

		public WhsWarehouseCollection Warehouses
		{
			get { return new WhsWarehouseCollectionWithSecurityCheck(Factory); }
		}

		public WarehouseClientCollection Clients
		{
			get { return new WarehouseClientCollectionWithSecurityCheck(Factory); }
		}

		#endregion

		#region IAccountingFilterStripHolder Members

		IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (AccountingFilterStrip_innerValue == null)
				{
					AccountingFilterStrip_innerValue = (IAccountingFilterStrip)Activator.CreateInstance(ObjectFactory.GetType<IAccountingFilterStrip>(), this);
					AccountingFilterStrip_innerValue.Initialize(addProfitLossReasonFilters: true);
				}

				return AccountingFilterStrip_innerValue;
			}
		}
		IAccountingFilterStrip AccountingFilterStrip_innerValue;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery mawbQuery = new ZDBOnlyQuery(typeof(WhsInvoice));
			mawbQuery.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return mawbQuery;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("55bdb3f1-3d16-4ea0-97e9-2e3fc3b1984e", "Status") }
		};

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories => null;

		#endregion
	}
}
