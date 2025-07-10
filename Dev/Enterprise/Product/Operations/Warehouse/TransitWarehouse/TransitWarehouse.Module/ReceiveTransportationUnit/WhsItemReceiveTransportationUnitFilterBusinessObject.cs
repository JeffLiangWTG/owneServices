using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Module
{
	public class WhsItemReceiveTransportationUnitFilterBusinessObject : WhsTransitFilterBusinessObject, IAccountingFilterStripHolder
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddNumberFilters(filters);
			AddDateFilters(filters);
			AddOrganisationFilters(filters);
			AddModeFilters(filters);
			AddAccountingFilters(filters);

			return filters;
		}

		protected override SchemaGuidColumn WarehouseFKSchemaColumn => WhsItemReceiveTransportationUnitSchema.WRH_WW_Warehouse;

		public override SchemaGuidColumn PKSchemaColumn => WhsItemReceiveTransportationUnitSchema.PK;

		#region Filters

		#region GetModuleFilterThatOverridesAllOtherFilters

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var jobIDFilter = new ModuleFountainFilter(Schema.JobID, WhsItemReceiveTransportationUnitSchema.WRH_ReferenceNumber, "TR");
			jobIDFilter.MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsItemReceiveTransportationUnitFilterBusinessObject|RTUID", "RTU ID");

			return jobIDFilter;
		}

		#endregion

		#region AccountingFilterStrip

		void AddAccountingFilters(ModuleFilterCollection filters)
		{
			AccountingFilterStrip.AddBillingFilters(filters);
			AccountingFilterStrip.AddJobManagementFilters(filters, Env.Security.WhsItemReceiveTransportationUnitJobInvoicing);
		}

		protected IAccountingFilterStrip AccountingFilterStrip
		{
			get
			{
				if (accountingFilterStrip == null)
				{
					accountingFilterStrip = ObjectFactory.New<IAccountingFilterStrip>(this);
					accountingFilterStrip.Initialize(addProfitLossReasonFilters: true);
				}

				return accountingFilterStrip;
			}
		}

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new Dictionary<string, object>()
		{
			{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(WhsItemReceiveTransportationUnit) },
			{ AccountingFilterStripConfigurationKeys.InvoicedChargesFilterNameOverride, ResString.GetMultilingualString("7a3fce55-6cd6-4a3c-ba1b-3d3dc72cd0ef", "Invoiced / Charges / Billing") },
			{ AccountingFilterStripConfigurationKeys.InvoicedChargesFilterOptionsSelected, InvoicedChargesFilterOptions.Default | InvoicedChargesFilterOptions.LocalBillingNotPaid },
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("176bbc5e-01a6-44fd-a352-5ddf7343ecb4", "Invoice Status") },
		};

		IAccountingFilterStrip accountingFilterStrip;

		#endregion

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(WhsItemReceiveTransportationUnit));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		#region Name Overides

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories => null;

		#endregion

		#region AddOrganisationFilters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var maxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);

			var transportCompanyName = filters.AddTextFilter(Schema.TransportCompanyName, GetTransportCompanyFilterCompanyNameQuery);
			transportCompanyName.MultilingualDescription = ResString.GetMultilingualString("18ff07de-5f0d-42a2-916a-93e64e4874c0", "Transport Company Name");
			transportCompanyName.Category = FilterCategories.Organisations;
			transportCompanyName.MaxLength = maxLength;

			var billingCompanyName = filters.AddTextFilter(Schema.BillingPartyCompanyName, GetBillingPartyCompanyNameQuery<WhsItemReceiveTransportationUnit>);
			billingCompanyName.MultilingualDescription = ResString.GetMultilingualString("7137a3be-fa42-45d0-b129-821d2baa5017", "Billing Party Company Name");
			billingCompanyName.Category = FilterCategories.Organisations;
			billingCompanyName.MaxLength = maxLength;
		}

		ZQuery GetTransportCompanyFilterCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			return GetAddressQuery<WhsItemReceiveTransportationUnit>(comparisonOperator, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, companyName, DocAddressType.TransportCompanyDocumentaryAddress);
		}

		#endregion

		#region Modes Filters

		void AddModeFilters(ModuleFilterCollection filters)
		{
			var containerTypeFilter = filters.AddGuidFilter(Schema.ContainerType, ModuleIDs.RefContainer, (pk) => new ZQuery(RefContainerSchema.PK, pk), ContainerTypeList);
			containerTypeFilter.Category = FilterCategories.ModesAndTypes;
			containerTypeFilter.MultilingualDescription = ResString.GetMultilingualString("8ecf0f3a-c721-4f0f-8d5f-7d90949e94c6", "Container Type");
			containerTypeFilter.SubGroup = ContainerTypeFilterProcessor;
		}

		ModuleFilterSubGroup ContainerTypeFilterProcessor => containerTypeFilterProcessor ?? (containerTypeFilterProcessor = new ContainerTypeSubGroup());
		ContainerTypeSubGroup containerTypeFilterProcessor;

		class ContainerTypeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var refContainerQuery = new ZDBOnlySubQuery(typeof(RefContainer), PkgPackageContainerSchema.K0_RC_ContainerType);
				refContainerQuery.AddToFilter(filter);

				var pkgContainersQuery = new ZDBOnlySubQuery(typeof(PkgPackageContainer), PkgPackageContainerSchema.K0_KP_Package);
				pkgContainersQuery.AddSubQuery(refContainerQuery, JoinCondition.And);

				var packageQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
				packageQuery.AddToFilter(PkgPackageSchema.KP_F3_NKPackType, Core.Constants.PkgUnit.Container);
				packageQuery.AddSubQuery(pkgContainersQuery, JoinCondition.And);

				var packageJobQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);
				packageJobQuery.AddSubQuery(packageQuery, JoinCondition.And);

				var result = new ZDBOnlyQuery(typeof(WhsItemReceiveTransportationUnit));
				result.AddSubQuery(packageJobQuery, JoinCondition.And);

				return result;
			}
		}

		RefContainerCollection ContainerTypeList => containerTypeList ?? (containerTypeList = new RefContainerCollection(Factory));

		RefContainerCollection containerTypeList;

		#endregion

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberFilter(Schema.ReferenceNumber, WhsItemReceiveTransportationUnitSchema.WRH_VehicleReference).MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsItemReceiveTransportationUnitFilterBusinessObject|VehicleReference", "RTU Reference");
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(Schema.UnloadComplete, WhsItemReceiveTransportationUnitSchema.WRH_UnloadCompleteTime).MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsItemReceiveTransportationUnitFilterBusinessObject|UnloadComplete", "Unload Complete");
		}

		#endregion
	}
}
