using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.Module
{
	public abstract class WhsTransitFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string JobID = "JobID"; // Filter description
			public const string ReferenceNumber = "ReferenceNumber"; // Filter description
			public const string Warehouse = "Warehouse"; // Filter description
			public const string AdditionalReference = "AdditionalReference"; // Filter description
			public const string PackageStatus = "PackageStatus"; // Filter description
			public const string AllPackagesReceived = "AllPackagesReceived"; // Filter description
			public const string AllPackagesDeparted = "AllPackagesDeparted"; // Filter description
			public const string ConsignorCompanyName = "ConsignorCompanyName"; // SupressCodeSmell Reason = Filter description
			public const string ConsigneeCompanyName = "ConsigneeCompanyName"; // Filter description
			public const string TransportCompanyName = "TransportCompanyName"; // Filter description
			public const string BillingPartyCompanyName = "BillingPartyCompanyName"; // Filter description
			public const string CTOCompanyName = "CTOCompanyName"; // Filter description
			public const string DeliveryCompanyName = "DeliveryCompanyName"; // Filter description
			public const string BookingPartyCompanyName = "BookingPartyCompanyName"; // Filter description
			public const string InvoicedChargesBilling = "Invoiced / Charges / Billing"; // Filter description
			public const string InvoiceStatus = "Invoice Status"; // Filter description
			public const string ContainerType = "ContainerType"; // Filter description
			public const string Direction = "Direction"; // Filter description
			public const string Creditor = "Creditor"; // Filter description

			public const string UnloadComplete = "UnloadComplete"; // Filter description
			public const string LoadComplete = "LoadComplete"; // Filter description
		}

		public static class Codes
		{
			public const string AllPackagesReceived = "RCV"; // Filter description
			public const string NotAllPackagesReceived = "NRC"; // Filter description
			public const string AllPackagesDeparted = "DEP"; // Filter description
			public const string NotAllPackagesDeparted = "NDE"; // Filter description
			public const string All = "ALL"; // Filter description
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddWarehouseFilter(result);
			return result;
		}

		#region Warehouse Filter

		protected abstract SchemaGuidColumn WarehouseFKSchemaColumn { get; }

		protected virtual ModuleFilterSubGroup GetWarehouseSubGroupQuery() => ModuleFilterSubGroup.Default;

		void AddWarehouseFilter(ModuleFilterCollection filters)
		{
			var warehouseCollection = new WhsWarehouseCollection(Factory, WarehouseCollectionType.TransitWarehouse);

			ModuleGuidFilter filter;
			if (WarehouseFKSchemaColumn != null)
			{
				filter = filters.AddGuidFilter(Schema.Warehouse, ModuleIDs.WhsConfigWarehouse, WarehouseFKSchemaColumn, warehouseCollection);
			}
			else
			{
				filter = filters.AddGuidFilter(Schema.Warehouse, ModuleIDs.WhsConfigWarehouse, (warehousePK) => new ZQuery(WhsWarehouseSchema.PK, warehousePK), warehouseCollection);
				filter.SubGroup = GetWarehouseSubGroupQuery();
			}

			filter.MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitFilterBusinessObject|Warehouse", "Warehouse");
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			filter.DefaultProperty = TransitWarehouseHelper.GetTransitWarehouseInCurrentBranch(Factory)?.PK ?? ZGuid.Empty;
			filter.Category = FilterCategories.Other;
		}

		public bool HasTransitWarehouseInCurrentBranch()
		{
			var result = true;
			var currentWarehouse = TransitWarehouseHelper.GetTransitWarehouseInCurrentBranch(Factory);

			if (currentWarehouse == null)
			{
				Globals.Message.ShowWarning(Res.GetString("b7d02abe-88e8-4b66-af4c-f3ca40719daa", "You are logged into branch '{0} - {1}' that doesn't have a Transit Warehouse setup. \r\nPlease log into a branch that is linked to a Transit Warehouse.", GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName));
				result = false;
			}

			return result;
		}

		#endregion

		protected ZDBOnlySubQuery GetJobHeaderSubQuery(bool notIn)
		{
			var jobHeaderFilter = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, notIn);
			jobHeaderFilter.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			return jobHeaderFilter;
		}

		protected ZQuery GetAddressQuery<BusinessObjectType>(SQLComparisonOperator comparisonOperator, SchemaStringColumn orgColumn, SchemaStringColumn docAddColumnm, ZString paramValue, DocAddressType addressType)
		{
			var query = new ZDBOnlyQuery(typeof(BusinessObjectType));
			AddAddressSubQuery(query, comparisonOperator, orgColumn, docAddColumnm, paramValue, addressType);
			return query;
		}

		protected void AddAddressSubQuery(ZDBOnlyQuery query, SQLComparisonOperator comparisonOperator, SchemaStringColumn orgColumn, SchemaStringColumn docAddColumnm, ZString paramValue, DocAddressType addressType)
		{
			var addressTypeCode = DocAddressTypes.GetCode(Factory, addressType);

			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				var jobDocAddressNotInSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn: true);
				AddDocAddressTypeFilter(jobDocAddressNotInSubQuery, addressTypeCode);
				query.AddSubQuery(PKSchemaColumn, jobDocAddressNotInSubQuery, JoinCondition.And);

				var jobDocAddressOverrideSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn: false);
				AddDocAddressTypeFilter(jobDocAddressOverrideSubQuery, addressTypeCode);
				jobDocAddressOverrideSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, true);
				jobDocAddressOverrideSubQuery.AddToFilter(JobDocAddressSchema.E2_CompanyName, SQLComparisonOperator.IsBlank, ZString.Empty);
				query.AddSubQuery(PKSchemaColumn, jobDocAddressOverrideSubQuery, JoinCondition.Or);

				var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn: false);
				AddDocAddressTypeFilter(jobDocAddressSubQuery, addressTypeCode);
				jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, false);
				jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_OA_Address, SQLComparisonOperator.IsBlank, null);
				query.AddSubQuery(PKSchemaColumn, jobDocAddressSubQuery, JoinCondition.Or);
			}
			else
			{
				var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressFieldSearchDBSubQuery(addressTypeCode, comparisonOperator, orgColumn, docAddColumnm, paramValue);
				query.AddSubQuery(PKSchemaColumn, jobDocAddressSubQuery, JoinCondition.And);
			}
		}

		protected void AddDocAddressTypeFilter(ZQuery query, ZString docAddressTypeCode)
			=> query.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, docAddressTypeCode);

		protected ZQuery GetBillingPartyCompanyNameQuery<BusinessObjectType>(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			var query = new ZDBOnlyQuery(typeof(BusinessObjectType));

			var jdaQuery = new ZDBOnlySubQuery(typeof(BusinessObjectType), PKSchemaColumn);
			AddAddressSubQuery(jdaQuery, comparisonOperator, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, companyName, DocAddressType.ClientRequestedBillingParty);
			var jobHeaderNotInFilter = GetJobHeaderSubQuery(notIn: true);
			jdaQuery.AddSubQuery(PKSchemaColumn, jobHeaderNotInFilter, JoinCondition.And);
			query.AddSubQuery(PKSchemaColumn, jdaQuery, JoinCondition.And);

			var jobHeaderSubQuery = GetJobHeaderSubQuery(notIn: false);

			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				jobHeaderSubQuery.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_OA_LocalChargesAddr, SQLComparisonOperator.IsBlank, null);
			}
			else
			{
				var localClientCompanyNameFilter = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
				localClientCompanyNameFilter.AddToFilter(new ZQuery(OrgHeaderSchema.OH_FullName, comparisonOperator, companyName));
				var localClientAddressFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
				localClientAddressFilter.AddSubQuery(localClientCompanyNameFilter, JoinCondition.And);
				jobHeaderSubQuery.AddSubQuery(localClientAddressFilter, JoinCondition.And);
			}

			query.AddSubQuery(PKSchemaColumn, jobHeaderSubQuery, JoinCondition.Or);

			return query;
		}
	}
}
