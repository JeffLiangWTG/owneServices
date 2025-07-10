using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transit.Module
{
	public abstract class WhsTransitConsignmentFilterBusinessObject<T> : WhsTransitFilterBusinessObject, IAccountingFilterStripHolder
	{
		protected static MultilingualString AllPackagesReceivedDescription => ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|AllPackagesReceived", "All Packages Received");

		protected static MultilingualString NotAllPackagesReceivedDescription => ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|NotAllPackagesReceived", "Not All Packages Received");

		protected static MultilingualString AllPackagesDepartedDescription => ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|AllPackagesDeparted", "All Packages Departed");

		protected static MultilingualString NotAllPackagesDepartedDescription => ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|NotAllPackagesDeparted", "Not All Packages Departed");

		protected static MultilingualString AllDescription => ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|All", "All");

		#region Filters

		protected abstract SchemaGuidColumn PackageStateFKSchemaColumn { get; }

		protected abstract SchemaStringColumn ConsignmentDirectionSchemaColumn { get; }

		protected abstract bool IsRCN { get; }

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			AddFiltersCore(result);
			AddAccountingFilters(result);
			AddOrganisationFilters(result);
			AddAllPackagesReceivedFilter(result);
			AddAllPackagesDepartedFilter(result);
			AddPackageStatusFilter(result);
			AddAdditionalReferenceFilter(result);
			AddStatusAndFlagsFilter(result);
			return result;
		}

		protected abstract void AddFiltersCore(ModuleFilterCollection filters);

		#region PackageFilters

		void AddAllPackagesReceivedFilter(ModuleFilterCollection filters)
		{
			var statusFilter = filters.AddTextFilter(Schema.AllPackagesReceived, GetAllPackagesReceivedQuery, GetAllPackagesReceivedList);
			statusFilter.MultilingualDescription = AllPackagesReceivedDescription;
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MaxLength = WhsItemPackageStateSchema.WPS_Status.MaxLength;
		}

		CodeDescriptionPairList GetAllPackagesReceivedList()
		{
			var options = new CodeDescriptionPairList();
			options.Add(new CodeDescriptionPair(Codes.AllPackagesReceived, AllPackagesReceivedDescription));
			options.Add(new CodeDescriptionPair(Codes.NotAllPackagesReceived, NotAllPackagesReceivedDescription));
			options.Add(new CodeDescriptionPair(Codes.All, AllDescription));
			return options;
		}

		ZQuery GetAllPackagesReceivedQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(T));

			if (value == Codes.AllPackagesReceived)
			{
				// !Packages.Any(WPS_Status == "BKD") && Packages.Any(WPS_Status != "ADJ")
				var noBookedPackageStateQuery = new ZDBOnlySubQuery(typeof(IWhsItemPackageState), PackageStateFKSchemaColumn, notIn: true);
				noBookedPackageStateQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.Equal, TransitWarehouseStatuses.Codes.Booked);
				query.AddSubQuery(PKSchemaColumn, noBookedPackageStateQuery, JoinCondition.And);

				var receivedPackageStateQuery = new ZDBOnlySubQuery(typeof(IWhsItemPackageState), PackageStateFKSchemaColumn);
				receivedPackageStateQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.NotEqual, TransitWarehouseStatuses.Codes.AdjustedOut);
				query.AddSubQuery(PKSchemaColumn, receivedPackageStateQuery, JoinCondition.And);
			}
			else if (value == Codes.NotAllPackagesReceived)
			{
				// !Packages.Any() || !Packages.Any(WPS_Status != "ADJ") || Packages.Any(WPS_Status == "BKD")
				var noPackagesQuery = new ZDBOnlySubQuery(typeof(IWhsItemPackageState), PackageStateFKSchemaColumn, notIn: true);
				query.AddSubQuery(PKSchemaColumn, noPackagesQuery, JoinCondition.And);

				var noReceivedPackagesQuery = new ZDBOnlySubQuery(typeof(IWhsItemPackageState), PackageStateFKSchemaColumn, notIn: true);
				noReceivedPackagesQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.NotEqual, TransitWarehouseStatuses.Codes.AdjustedOut);
				query.AddSubQuery(PKSchemaColumn, noReceivedPackagesQuery, JoinCondition.Or);

				var anyBookedPackagesQuery = new ZDBOnlySubQuery(typeof(IWhsItemPackageState), PackageStateFKSchemaColumn);
				anyBookedPackagesQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.Equal, TransitWarehouseStatuses.Codes.Booked);
				query.AddSubQuery(PKSchemaColumn, anyBookedPackagesQuery, JoinCondition.Or);
			}

			return query;
		}

		void AddAllPackagesDepartedFilter(ModuleFilterCollection filters)
		{
			var statusFilter = filters.AddTextFilter(Schema.AllPackagesDeparted, GetAllPackagesDepartedQuery, GetAllPackagesDepartedList);
			statusFilter.MultilingualDescription = AllPackagesDepartedDescription;
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MaxLength = WhsItemPackageStateSchema.WPS_Status.MaxLength;
		}

		CodeDescriptionPairList GetAllPackagesDepartedList()
		{
			var list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair(Codes.AllPackagesDeparted, AllPackagesDepartedDescription));
			list.Add(new CodeDescriptionPair(Codes.NotAllPackagesDeparted, NotAllPackagesDepartedDescription));
			list.Add(new CodeDescriptionPair(Codes.All, AllDescription));
			return list;
		}

		ZQuery GetAllPackagesDepartedQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(T));
			if (value == Codes.AllPackagesDeparted)
			{
				// Packages.Any(WPS_Status == "DEP" && WPS_Status == "FIN") && !Packages.Any(WPS_Status != "DEP" || WPS_Status != "FIN" || WPS_Status != "ADJ")
				var anyPackagesDepartedQuery = new ZDBOnlySubQuery(typeof(IWhsItemPackageState), PackageStateFKSchemaColumn);
				anyPackagesDepartedQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.Equal, TransitWarehouseStatuses.Codes.Departed);
				anyPackagesDepartedQuery.AddToFilter(JoinCondition.Or, WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.Equal, TransitWarehouseStatuses.Codes.Finalized);
				query.AddSubQuery(PKSchemaColumn, anyPackagesDepartedQuery, JoinCondition.And);

				var noPackagesNotDepartedQuery = new ZDBOnlySubQuery(typeof(IWhsItemPackageState), PackageStateFKSchemaColumn, notIn: true);
				noPackagesNotDepartedQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.NotEqual, TransitWarehouseStatuses.Codes.Departed);
				noPackagesNotDepartedQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.NotEqual, TransitWarehouseStatuses.Codes.Finalized);
				noPackagesNotDepartedQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.NotEqual, TransitWarehouseStatuses.Codes.AdjustedOut);
				query.AddSubQuery(PKSchemaColumn, noPackagesNotDepartedQuery, JoinCondition.And);
			}
			else if (value == Codes.NotAllPackagesDeparted)
			{
				//  !Packages.Any() || !Packages.Any(WPS_Status == "DEP" && WPS_Status == "FIN") || Packages.Any(WPS_Status != "DEP" || WPS_Status != "FIN" || WPS_Status != "ADJ")
				var noPackagesQuery = new ZDBOnlySubQuery(typeof(IWhsItemPackageState), PackageStateFKSchemaColumn, notIn: true);
				query.AddSubQuery(PKSchemaColumn, noPackagesQuery, JoinCondition.And);

				var noPackagesDepartedQuery = new ZDBOnlySubQuery(typeof(IWhsItemPackageState), PackageStateFKSchemaColumn, notIn: true);
				noPackagesDepartedQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.Equal, TransitWarehouseStatuses.Codes.Departed);
				noPackagesDepartedQuery.AddToFilter(JoinCondition.Or, WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.Equal, TransitWarehouseStatuses.Codes.Finalized);
				query.AddSubQuery(PKSchemaColumn, noPackagesDepartedQuery, JoinCondition.Or);

				var anyPackagesNotDepartedQuery = new ZDBOnlySubQuery(typeof(IWhsItemPackageState), PackageStateFKSchemaColumn);
				anyPackagesNotDepartedQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.NotEqual, TransitWarehouseStatuses.Codes.Departed);
				anyPackagesNotDepartedQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.NotEqual, TransitWarehouseStatuses.Codes.Finalized);
				anyPackagesNotDepartedQuery.AddToFilter(WhsItemPackageStateSchema.WPS_Status, SQLComparisonOperator.NotEqual, TransitWarehouseStatuses.Codes.AdjustedOut);
				query.AddSubQuery(PKSchemaColumn, anyPackagesNotDepartedQuery, JoinCondition.Or);
			}
			return query;
		}

		void AddPackageStatusFilter(ModuleFilterCollection filters)
		{
			var statusFilter = filters.AddTextFilter(Schema.PackageStatus, GetPackageStatusFilter, GetPackageStateStatuses);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|PackageStatus", "Package Status");
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.SubGroup = new PackageStateSubGroup(PackageStateFKSchemaColumn);
			statusFilter.MaxLength = WhsItemPackageStateSchema.WPS_Status.MaxLength;
		}

		ZQuery GetPackageStatusFilter(SQLComparisonOperator op, ZString status)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsItemPackageStateSchema.WPS_Status, op, status);
			return query;
		}

		class PackageStateSubGroup : ModuleFilterSubGroup
		{
			readonly SchemaColumn PackageStateFKSchemaColumn;

			public PackageStateSubGroup(SchemaGuidColumn packageStateFKSchemaColumn) : base()
			{
				PackageStateFKSchemaColumn = packageStateFKSchemaColumn;
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(T));
				var packageStateFilter = new ZDBOnlySubQuery(typeof(IWhsItemPackageState), PackageStateFKSchemaColumn);
				packageStateFilter.AddToFilter(filter);
				query.AddSubQuery(packageStateFilter, JoinCondition.And);
				return query;
			}
		}

		CodeDescriptionPairList GetPackageStateStatuses()
		{
			return new TransitWarehouseStatuses();
		}

		void AddStatusAndFlagsFilter(ModuleFilterCollection filters)
		{
			AddDirectionFilter(filters);
		}

		void AddDirectionFilter(ModuleFilterCollection filters)
		{
			var directionFilter = filters.AddTextFilter(Schema.Direction, GetDirectionFilter, new ConsignmentDirections());
			directionFilter.MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|ConsignmentFilterBusinessObject|Direction", "Direction");
			directionFilter.Category = FilterCategories.StatusAndFlags;
			directionFilter.MaxLength = ConsignmentDirectionSchemaColumn.MaxLength;
		}

		ZQuery GetDirectionFilter(SQLComparisonOperator op, ZString direction)
		{
			var query = new ZQuery();
			query.AddToFilter(ConsignmentDirectionSchemaColumn, op, direction);
			return query;
		}

		#endregion

		#region AddOrganisationFilters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var maxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);

			if (IsRCN)
			{
				var consignorCompanyName = filters.AddTextFilter(Schema.ConsignorCompanyName, (comparisonOperator, companyName) => GetAddressFilterCompanyNameQuery(comparisonOperator, companyName, DocAddressType.LocalCartageExporter));
				consignorCompanyName.MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|ConsignorCompanyName", "Consignor Company Name");
				consignorCompanyName.Category = FilterCategories.Organisations;
				consignorCompanyName.MaxLength = maxLength;
			}

			var consigneeCompanyName = filters.AddTextFilter(Schema.ConsigneeCompanyName, (comparisonOperator, companyName) => GetAddressFilterCompanyNameQuery(comparisonOperator, companyName, DocAddressType.ConsigneeDocumentaryAddress));
			consigneeCompanyName.MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|ConsigneeCompanyName", "Consignee Company Name");
			consigneeCompanyName.Category = FilterCategories.Organisations;
			consigneeCompanyName.MaxLength = maxLength;

			var bookingCompanyName = filters.AddTextFilter(Schema.BookingPartyCompanyName, (comparisonOperator, companyName) => GetAddressFilterCompanyNameQuery(comparisonOperator, companyName, DocAddressType.BookingPartyDocumentaryAddress));
			bookingCompanyName.MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|BookingPartyCompanyName", "Booking Party Company Name");
			bookingCompanyName.Category = FilterCategories.Organisations;
			bookingCompanyName.MaxLength = maxLength;

			var billingCompanyName = filters.AddTextFilter(Schema.BillingPartyCompanyName, GetBillingPartyCompanyNameQuery);
			billingCompanyName.MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|BillingPartyCompanyName", "Billing Party Company Name");
			billingCompanyName.Category = FilterCategories.Organisations;
			billingCompanyName.MaxLength = maxLength;

			var ctoCompanyName = filters.AddTextFilter(Schema.CTOCompanyName, GetCTOFilterCompanyNameQuery);
			ctoCompanyName.MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|CTOCompanyName", "CTO Company Name");
			ctoCompanyName.Category = FilterCategories.Organisations;
			ctoCompanyName.MaxLength = maxLength;

			if (!IsRCN)
			{
				var deliveryCompanyName = filters.AddTextFilter(Schema.DeliveryCompanyName, (comparisonOperator, companyName) => GetAddressFilterCompanyNameQuery(comparisonOperator, companyName, DocAddressType.ConsigneePickupDeliveryAddress));
				deliveryCompanyName.MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|DeliveryCompanyName", "Delivery Company Name");
				deliveryCompanyName.Category = FilterCategories.Organisations;
				deliveryCompanyName.MaxLength = maxLength;

				var transportCompanyName = filters.AddTextFilter(Schema.TransportCompanyName, (comparisonOperator, companyName) => GetAddressFilterCompanyNameQuery(comparisonOperator, companyName, DocAddressType.TransportCompanyDocumentaryAddress));
				transportCompanyName.MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|TransportCompanyName", "Transport Company Name");
				transportCompanyName.Category = FilterCategories.Organisations;
				transportCompanyName.MaxLength = maxLength;
			}
		}

		ZQuery GetBillingPartyCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			var query = new ZDBOnlyQuery(typeof(T));

			var jdaQuery = new ZDBOnlySubQuery(typeof(T), PKSchemaColumn);
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

		protected ZQuery GetAddressFilterCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName, DocAddressType addressType)
		{
			return GetAddressQuery<T>(comparisonOperator, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, companyName, addressType);
		}

		protected abstract ZQuery GetCTOFilterCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName);

		#endregion

		#region AddAdditionalReferenceFilter

		void AddAdditionalReferenceFilter(ModuleFilterCollection filters)
		{
			var additionalReference = filters.AddTextFilter(Schema.AdditionalReference, GetAdditionalReferenceQuery);
			additionalReference.MultilingualDescription = ResString.GetMultilingualString("TransitWarehouse|WhsTransitConsignmentFilterBusinessObject|AdditionalReference", "Additional Reference");
			additionalReference.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			additionalReference.Category = FilterCategories.NumbersAndReferences;
			additionalReference.SubGroup = new AdditionalReferenceSubGroup();
		}

		ZQuery GetAdditionalReferenceQuery(SQLComparisonOperator op, ZString number)
		{
			var query = new ZQuery();
			query.AddToFilter(CusEntryNumSchema.CE_EntryNum, op, number);
			return query;
		}

		class AdditionalReferenceSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(T));
				var entryNumFilter = new ZDBOnlySubQuery(typeof(ICusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumFilter.AddToFilter(filter);
				query.AddSubQuery(entryNumFilter, JoinCondition.And);
				return query;
			}
		}

		#endregion

		#endregion

		#region IAccountingFilterStripHolder Members

		#region Security

		protected abstract SecurityCheckpoint GetJobInvoicingSecurityCheckpoint();

		#endregion

		#region AccountingFilterStrip

		void AddAccountingFilters(ModuleFilterCollection filters)
		{
			AccountingFilterStrip.AddBillingFilters(filters);
			AccountingFilterStrip.AddJobManagementFilters(filters, GetJobInvoicingSecurityCheckpoint());
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
			{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(T) },
			{ AccountingFilterStripConfigurationKeys.InvoicedChargesFilterNameOverride, ResString.GetMultilingualString("WhsTransitConsignmentFilterBusinessObject|InvoicedChargesBilling", "Invoiced / Charges / Billing") },
			{ AccountingFilterStripConfigurationKeys.InvoicedChargesFilterOptionsSelected, InvoicedChargesFilterOptions.Default | InvoicedChargesFilterOptions.LocalBillingNotPaid },
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("WhsTransitConsignmentFilterBusinessObject|InvoiceStatus", "Invoice Status") },
		};

		IAccountingFilterStrip accountingFilterStrip;

		#endregion

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(T));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		#region Name Overides

		MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride
		{
			get { return null; }
		}

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories
		{
			get { return null; }
		}

		#endregion

		#endregion
	}
}
