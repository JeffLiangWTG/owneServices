using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Invoicing.Module
{
	public abstract class PeriodicInvoicingFilterBusinessObject(WarehouseCollectionType warehouseType) : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter description")]
		const string MissingJobHeaderFilterName = "Missing Job Header";

		protected abstract SecurityCheckpoint CheckPointForJobManagement { get; }

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddClientFilter(filters);
			AddWarehouseFilter(filters);
			AddMissingJobHeaderFilter(filters);
			AddOffBandProcessingStatusFilter(filters);

			filters.AddFountainFilter("Invoice Number", JobStorageSchema.ET_StorageJobNumber, "I").MultilingualDescription = ResString.GetMultilingualString("d2b09683-3886-4dd5-a8fa-4e5917799035", "Invoice Number");
			filters.AddDateFilter("Billing Date", JobStorageSchema.ET_BillingDate).MultilingualDescription = ResString.GetMultilingualString("cd9d5440-e595-42f8-9645-97d256ebe79b", "Billing Date");
			filters.AddDateFilter("From Date", JobStorageSchema.ET_StorageFromDate).MultilingualDescription = ResString.GetMultilingualString("540b7c38-74ae-4d83-8a8a-ef422381b4be", "From Date");
			filters.AddDateFilter("To Date", JobStorageSchema.ET_StorageToDate).MultilingualDescription = ResString.GetMultilingualString("79a74b00-164b-43a8-af01-eb174148cc1f", "To Date");

			AccountingFilterStrip.AddBillingFilters(filters);
			AccountingFilterStrip.AddJobManagementFilters(filters, CheckPointForJobManagement);

			return filters;
		}

		#region AddMissingJobHeaderFilter

		void AddMissingJobHeaderFilter(ModuleFilterCollection filters)
		{
			var missingJobHeaderFilter = filters.AddFlagsFilter(MissingJobHeaderFilterName, new string[] { Res.GetString("e390dc6a-270c-41f9-bf2a-c9269fad30ec", "Missing Job Header") }, new GetFlagsQuery[] { GetNoJobHeaderQuery });
			missingJobHeaderFilter.MultilingualDescription = ResString.GetMultilingualString("f8755b6e-d193-48d9-b679-09089ab5740b", "Missing Job Header");
			missingJobHeaderFilter.Property0 = true;
		}

		#endregion

		#region Warehouse Filter

		void AddWarehouseFilter(ModuleFilterCollection filters)
		{
			ModuleGuidFilter warehouseFilter = filters.AddGuidFilter("Warehouse", ModuleIDs.WhsConfigWarehouse, JobStorageSchema.ET_WW, Warehouses);
			warehouseFilter.MultilingualDescription = WarehouseFilterDescription;

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
				info.AddError(WarehouseFilterValidationError);
			}
		}

		public virtual ResourceString WarehouseFilterDescription => ResString.GetMultilingualString("f851a3f5-fe27-4ee7-b54d-d7310e2acee0", "Warehouse");

		protected virtual string WarehouseFilterValidationError => Res.GetString("90ac3057-e49f-4b30-a992-de7ed05368a0", "Please select a warehouse to filter by");

		#endregion

		#region Client Filter

		void AddClientFilter(ModuleFilterCollection filters)
		{
			var clientFilter = filters.AddGuidFilter("Client", ModuleIDs.Organisation, JobStorageSchema.ET_OH_Client, Clients);
			clientFilter.MultilingualDescription = ResString.GetMultilingualString("a9d879fd-7af6-45d1-b558-4479f7146a19", "Client");

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
				info.AddError(Res.GetString("9ef122ac-c091-4c3b-91f4-2490a00f752b", "Please select a client to filter by"));
			}
		}

		#endregion

		#region AddOffBandProcessingStatusFilter

		void AddOffBandProcessingStatusFilter(ModuleFilterCollection filters)
		{
			var filter = new TextFilterExactOrNotEqual(JobStorageSchema.Constants.ET_OffBandProcessingStatus, JobStorageSchema.ET_OffBandProcessingStatus, StorageOffBandProcessingStatuses);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("PeriodicInvoicingFilterBusinessObject|ET_OffBandProcessingStatus", "Off Band Processing Status");
			filter.DefaultProperty = StorageOffBandProcessingStatus.Codes.NIQ;
			filter.Visibility = filter.Visibility = FilterVisibility.AlwaysApplied | FilterVisibility.AlwaysVisible;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filters.AddFilter(filter);
		}

		#region StorageOffBandProcessingStatuses

		public StorageOffBandProcessingStatus StorageOffBandProcessingStatuses => Factory.GetCachedValue("PeriodicInvoicingFilterBusinessObject|OffBandProcessingStatus", () => new StorageOffBandProcessingStatus());

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
				var query = new ZDBOnlyQuery(typeof(PeriodicInvoicing));
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
			get { return new WhsWarehouseCollectionWithSecurityCheck(Factory, warehouseType); }
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
			ZDBOnlyQuery mawbQuery = new ZDBOnlyQuery(typeof(PeriodicInvoicing));
			mawbQuery.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return mawbQuery;
		}

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("a7082472-0cf4-4f5f-b7a6-e8a0695ff620", "Status") }
		};

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories => null;

		#endregion
	}
}
