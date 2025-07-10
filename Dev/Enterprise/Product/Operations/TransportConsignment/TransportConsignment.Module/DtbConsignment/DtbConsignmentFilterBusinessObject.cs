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
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbConsignmentFilterBusinessObject : FilterStripBusinessObject, IAccountingFilterStripHolder
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddAccountingFilters(result);
			AddConsignmentNumber(result);
			AddOrganisationFilters(result);
			AddStatusAndFlagsFilters(result);
			return result;
		}

		#region IAccountingFilterStripHolder Members

		#region Security

		protected SecurityCheckpoint GetJobInvoicingSecurityCheckpoint() => Env.Security.DtbConsignmentJobInvoicing;

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
					accountingFilterStrip.Initialize();
				}

				return accountingFilterStrip;
			}
		}

		IAccountingFilterStrip accountingFilterStrip;

		#endregion

		ZQuery IAccountingFilterStripHolder.TopLevelBusinessObjectQuery(ZDBOnlySubQuery billingPKSubQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(DtbConsignment));
			result.AddSubQuery(billingPKSubQuery, JoinCondition.And);

			return result;
		}

		#region Name Overides

		ZBool IAccountingFilterStripHolder.IsFilterStripForParentTable => true;

		Dictionary<string, object> IAccountingFilterStripHolder.AccountingFilterStripConfiguration => new ()
		{
			{ AccountingFilterStripConfigurationKeys.BusinessObjectType, typeof(DtbConsignment) },
			{ AccountingFilterStripConfigurationKeys.InvoicedChargesFilterNameOverride, ResString.GetMultilingualString("LandTransport|DtbConsignmentFilterBusinessObject|InvoicedChargesBilling", "Invoiced / Charges / Billing") },
			{ AccountingFilterStripConfigurationKeys.InvoicedChargesFilterOptionsSelected, InvoicedChargesFilterOptions.Default | InvoicedChargesFilterOptions.LocalBillingNotPaid },
			{ AccountingFilterStripConfigurationKeys.InvoicingJobStatusFilterNameOverride, ResString.GetMultilingualString("LandTransport|DtbConsignmentFilterBusinessObject|InvoiceStatus", "Invoice Status") },
		};

MultilingualString IAccountingFilterStripHolder.AmountFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.BillingFiltersCategoryNameOveride => null;

		MultilingualString IAccountingFilterStripHolder.FilterNameSuffixInOtherCategories => null;

		#endregion

		#endregion

		#region ConsignmentNumber

		void AddConsignmentNumber(ModuleFilterCollection filters)
		{
			filters.AddFountainFilter("ConsignmentNumber", DtbConsignmentSchema.LTC_JobID, "CN").MultilingualDescription = ResString.GetMultilingualString("LandTransport|DtbConsignmentFilterBusinessObject|LTC_JobID", "Consignment Number");
		}

		#endregion

		#region OrganisationFilter

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var maxLength = Math.Min(OrgHeaderSchema.OH_FullName.MaxLength, JobDocAddressSchema.E2_CompanyName.MaxLength);

			var bookingCompanyName = filters.AddTextFilter("BookingPartyCompanyName", (comparisonOperator, companyName) => GetAddressFilterCompanyNameQuery(comparisonOperator, companyName, DocAddressType.BookingPartyDocumentaryAddress));
			bookingCompanyName.MultilingualDescription = ResString.GetMultilingualString("LandTransport|DtbConsignmentFilterBusinessObject|BookingPartyCompanyName", "Booking Party Company Name");
			bookingCompanyName.Category = FilterCategories.Organisations;
			bookingCompanyName.MaxLength = maxLength;

			var billingCompanyName = filters.AddTextFilter("BillingPartyCompanyName", (comparisonOperator, companyName) => GetBillingPartyCompanyNameQuery(comparisonOperator, companyName, OrgHeaderSchema.OH_FullName));
			billingCompanyName.MultilingualDescription = ResString.GetMultilingualString("LandTransport|DtbConsignmentFilterBusinessObject|BillingPartyCompanyName", "Billing Party Company Name");
			billingCompanyName.Category = FilterCategories.Organisations;
			billingCompanyName.MaxLength = maxLength;

			var billingPartyOrgCode = filters.AddTextFilter("BillingPartyOrgCode", (comparisonOperator, companyName) => GetBillingPartyCompanyNameQuery(comparisonOperator, companyName, OrgHeaderSchema.OH_Code));
			billingPartyOrgCode.MultilingualDescription = ResString.GetMultilingualString("LandTransport|DtbConsignmentFilterBusinessObject|BillingPartyOrgCode", "Billing Party Org. Code");
			billingPartyOrgCode.Category = FilterCategories.Organisations;
			billingPartyOrgCode.MaxLength = maxLength;

			var pickupCompanyName = filters.AddTextFilter("PickupCompanyName", (comparisonOperator, companyName) => GetAddressNameQueryForDtbConsignmentAddress(comparisonOperator, companyName, ConsignmentAddressTypes.Codes.PickUp, DocAddressType.LocalCartageExporter));
			pickupCompanyName.MultilingualDescription = ResString.GetMultilingualString("LandTransport|DtbConsignmentFilterBusinessObject|PickupCompanyName", "Pickup Company Name");
			pickupCompanyName.Category = FilterCategories.Organisations;
			pickupCompanyName.MaxLength = maxLength;

			var deliveryCompanyName = filters.AddTextFilter("DeliveryCompanyName", (comparisonOperator, companyName) => GetAddressNameQueryForDtbConsignmentAddress(comparisonOperator, companyName, ConsignmentAddressTypes.Codes.Delivery, DocAddressType.LocalCartageImporter));
			deliveryCompanyName.MultilingualDescription = ResString.GetMultilingualString("LandTransport|DtbConsignmentFilterBusinessObject|DeliveryCompanyName", "Delivery Company Name");
			deliveryCompanyName.Category = FilterCategories.Organisations;
			deliveryCompanyName.MaxLength = maxLength;
		}

		ZQuery GetAddressNameQueryForDtbConsignmentAddress(SQLComparisonOperator comparisonOperator, ZString companyName, string consignmentAddressType, DocAddressType docAddressType)
		{
			var consignmentAddressSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentAddress), DtbConsignmentAddressSchema.LTS_LTC_Consignment);
			consignmentAddressSubQuery.AddToFilter(DtbConsignmentAddressSchema.LTS_InstructionType, consignmentAddressType);
			AddAddressSubQuery(consignmentAddressSubQuery, DtbConsignmentAddressSchema.PK, comparisonOperator, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, companyName, docAddressType);

			var query = new ZDBOnlyQuery(typeof(DtbConsignment));
			query.AddSubQuery(consignmentAddressSubQuery, JoinCondition.And);

			return query;
		}

		protected ZQuery GetAddressFilterCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName, DocAddressType addressType)
		{
			return GetAddressQuery<DtbConsignment>(comparisonOperator, OrgHeaderSchema.OH_FullName, JobDocAddressSchema.E2_CompanyName, companyName, addressType);
		}

		protected ZQuery GetAddressQuery<BusinessObjectType>(SQLComparisonOperator comparisonOperator, SchemaStringColumn orgColumn, SchemaStringColumn docAddColumnm, ZString paramValue, DocAddressType addressType)
		{
			var query = new ZDBOnlyQuery(typeof(BusinessObjectType));
			AddAddressSubQuery(query, DtbConsignmentSchema.PK, comparisonOperator, orgColumn, docAddColumnm, paramValue, addressType);
			return query;
		}

		protected void AddAddressSubQuery(ZDBOnlyQuery query, SchemaGuidColumn parentPkColumn, SQLComparisonOperator comparisonOperator, SchemaStringColumn orgColumn, SchemaStringColumn docAddColumnm, ZString paramValue, DocAddressType addressType)
		{
			var addressTypeCode = DocAddressTypes.GetCode(Factory, addressType);

			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				var jobDocAddressNotInSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn: true);
				AddDocAddressTypeFilter(jobDocAddressNotInSubQuery, addressTypeCode);
				query.AddSubQuery(parentPkColumn, jobDocAddressNotInSubQuery, JoinCondition.And);

				var jobDocAddressOverrideSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn: false);
				AddDocAddressTypeFilter(jobDocAddressOverrideSubQuery, addressTypeCode);
				jobDocAddressOverrideSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, true);
				jobDocAddressOverrideSubQuery.AddToFilter(JobDocAddressSchema.E2_CompanyName, SQLComparisonOperator.IsBlank, ZString.Empty);
				query.AddSubQuery(parentPkColumn, jobDocAddressOverrideSubQuery, JoinCondition.Or);

				var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn: false);
				AddDocAddressTypeFilter(jobDocAddressSubQuery, addressTypeCode);
				jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressOverride, false);
				jobDocAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_OA_Address, SQLComparisonOperator.IsBlank, null);
				query.AddSubQuery(parentPkColumn, jobDocAddressSubQuery, JoinCondition.Or);
			}
			else
			{
				var jobDocAddressSubQuery = JobDocAddressQueryHelper.JobDocAddressFieldSearchDBSubQuery(addressTypeCode, comparisonOperator, orgColumn, docAddColumnm, paramValue);
				query.AddSubQuery(parentPkColumn, jobDocAddressSubQuery, JoinCondition.And);
			}
		}

		protected void AddDocAddressTypeFilter(ZQuery query, ZString docAddressTypeCode)
			=> query.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, docAddressTypeCode);

		ZQuery GetBillingPartyCompanyNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName, SchemaStringColumn columnName)
		{
			var query = new ZDBOnlyQuery(typeof(DtbConsignment));

			var jdaQuery = new ZDBOnlySubQuery(typeof(DtbConsignment), DtbConsignmentSchema.PK);
			AddAddressSubQuery(jdaQuery, DtbConsignmentSchema.PK, comparisonOperator, columnName, JobDocAddressSchema.E2_CompanyName, companyName, DocAddressType.ClientRequestedBillingParty);
			var jobHeaderNotInFilter = GetJobHeaderSubQuery(notIn: true);
			jdaQuery.AddSubQuery(DtbConsignmentSchema.PK, jobHeaderNotInFilter, JoinCondition.And);
			query.AddSubQuery(DtbConsignmentSchema.PK, jdaQuery, JoinCondition.And);

			var jobHeaderSubQuery = GetJobHeaderSubQuery(notIn: false);

			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				jobHeaderSubQuery.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_OA_LocalChargesAddr, SQLComparisonOperator.IsBlank, null);
			}
			else
			{
				var localClientCompanyNameFilter = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
				localClientCompanyNameFilter.AddToFilter(new ZQuery(columnName, comparisonOperator, companyName));
				var localClientAddressFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
				localClientAddressFilter.AddSubQuery(localClientCompanyNameFilter, JoinCondition.And);
				jobHeaderSubQuery.AddSubQuery(localClientAddressFilter, JoinCondition.And);
			}

			query.AddSubQuery(DtbConsignmentSchema.PK, jobHeaderSubQuery, JoinCondition.Or);

			return query;
		}

		protected ZDBOnlySubQuery GetJobHeaderSubQuery(bool notIn)
		{
			var jobHeaderFilter = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, notIn);
			jobHeaderFilter.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			return jobHeaderFilter;
		}

		#endregion

		#region StatusAndFlagsFilters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			ModuleFilter incoTermFilter = filters.AddTextFilter("Incoterm", DtbConsignmentSchema.LTC_Incoterm, IncoTermList);
			incoTermFilter.Category = FilterCategories.StatusAndFlags;
			incoTermFilter.MultilingualDescription = ResString.GetMultilingualString("LandTransport|DtbConsignmentFilterBusinessObject|LTC_Incoterm", "Incoterm");
		}

		CodeDescriptionPairList IncoTermList
		{
			get { return incoTermList ?? (incoTermList = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms)); }
		}

		CodeDescriptionPairList incoTermList;

		#endregion
	}
}
