using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Module
{
	public class QuotationsFilterBusinessObject : RatingFilterBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);
			return filters;
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery standardSpotQuoteQuery = GetStandardSpotQuoteQuery();
				ZQuery salesRepSecurityQuery = GetSalesRepSecurityQuery();

				ZQuery query = new ZQuery();
				query.AddToFilter(base.Filter);
				query.AddToFilter(standardSpotQuoteQuery);
				query.AddToFilter(salesRepSecurityQuery);

				return query;
			}
		}

		ZQuery GetStandardSpotQuoteQuery()
		{
			return new ZQuery(RatingHeaderSchema.TH_OneTimeQuote, false);
		}

		ZQuery GetSalesRepSecurityQuery()
		{
			ZQuery result = new ZQuery();

			bool showAllQuotes = Env.Security.QuotationShowAllQuotes.IsAllowed;
			bool showOnlyForCurrentBranch = (Env.Security.QuotationShowBranchQuotes.IsAllowed && !showAllQuotes);
			bool showOnlyForCurrentUser = !(showAllQuotes || showOnlyForCurrentBranch);

			if (showOnlyForCurrentBranch)
			{
				ZDBOnlySubQuery glbStaffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
				glbStaffSubQuery.AddToFilter(GlbStaffSchema.GS_GB_HomeBranch, GlbStaff.CurrentUser.GS_GB_HomeBranch);

				ZDBOnlyQuery quotationsQuery = new ZDBOnlyQuery(typeof(RatingHeader));
				quotationsQuery.AddSubQuery(RatingHeaderSchema.TH_GS_NKFirstSignatory, glbStaffSubQuery, JoinCondition.And);
				quotationsQuery.AddSubQuery(RatingHeaderSchema.TH_GS_NKSecondSignatory, glbStaffSubQuery, JoinCondition.Or);

				result.AddToFilter(quotationsQuery);
			}
			else if (showOnlyForCurrentUser)
			{
				ZQuery userFilter = new ZQuery();
				userFilter.AddToFilter(RatingHeaderSchema.TH_GS_NKFirstSignatory, GlbStaff.CurrentUser.GS_Code);
				userFilter.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_GS_NKSecondSignatory, GlbStaff.CurrentUser.GS_Code);

				result.AddToFilter(userFilter);
			}

			return result;
		}

		protected override void AddTextFilters(ModuleFilterCollection filters)
		{
			base.AddTextFilters(filters);

			filters.AddFountainFilter(RateFilterHelper.Constants.QuoteNumber, RatingHeaderSchema.TH_QuoteNumber, "").MultilingualDescription = ResString.GetMultilingualString("a06660bb-25ee-4aa4-9020-07795a2da337", "Quote Number");

			ModuleTextFilter statusFilter = filters.AddTextFilter("Status", GetQuoteStatusQuery, QuoteStatuses);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("47b51649-0aa4-4192-8041-b5c53fce6126", "Status");
			statusFilter.Category = FilterCategories.StatusAndFlags;

			ModuleTextFilter cancellationReasonFilter = filters.AddTextFilter("Cancellation Reason", RatingHeaderSchema.TH_QuoteCancellationReason, CancellationReasonList);
			cancellationReasonFilter.MultilingualDescription = ResString.GetMultilingualString("56ead56d-bde7-47a5-9659-13e8c37a3bd7", "Cancellation Reason");
			cancellationReasonFilter.Category = FilterCategories.StatusAndFlags;
		}

		protected override ZQuery GetOrganizationNameQuery(SQLComparisonOperator comparisonOperator, ZString organizationName)
		{
			var rateQueryHelper = new RateQueryHelper(Factory);
			return rateQueryHelper.GetOrganisationQueryForQuotation(comparisonOperator, organizationName);
		}

		protected override void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(RateFilterHelper.Constants.QuoteDate, RatingHeaderSchema.TH_QuoteDate).MultilingualDescription = ResString.GetMultilingualString("6ff95897-4703-4afd-93e3-8a86ad4aac04", "Quote Date");
			filters.AddDateFilter(RateFilterHelper.Constants.ExpiryDate, RatingHeaderSchema.TH_QuoteEndDate).MultilingualDescription = ResString.GetMultilingualString("78db3a37-c3b7-4194-9bb1-86bc1dd054a6", "Expiry Date");
			filters.AddDateFilter(RateFilterHelper.Constants.AcceptedDate, RatingHeaderSchema.TH_Accepted).MultilingualDescription = ResString.GetMultilingualString("466f63c8-0462-4dd4-bdea-ddd33259fa35", "Accepted Date");
			filters.AddDateFilter(RateFilterHelper.Constants.ClientAcceptedDate, RatingHeaderSchema.TH_ClientAccepted).MultilingualDescription = ResString.GetMultilingualString("f02c8c48-4267-4853-ace2-8e5473fc8f42", "Client Accepted Date");
			filters.AddDateFilter(RateFilterHelper.Constants.FollowUpDate, RatingHeaderSchema.TH_FollowUpDate).MultilingualDescription = ResString.GetMultilingualString("23ccaa27-5ee5-49b2-be8e-2a6ef4bf37cc", "Follow Up Date");
		}

		#region Status Filter

		ZQuery GetQuoteStatusQuery(ZString status)
		{
			var ratingHeaderQuery = new ZDBOnlyQuery(typeof(RatingHeader));

			QuoteStatus.SetQueryStatusQuery(ratingHeaderQuery, status);

			return ratingHeaderQuery;
		}

		#endregion

		protected override void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			base.AddOrganisationFilters(filters);

			var filter = filters.AddNkFilter(RateFilterHelper.Constants.Signatory, GetSignatoryQuery, ModuleIDs.GlbStaff, Staff);
			filter.MultilingualDescription = ResString.GetMultilingualString("0760b0fd-bd77-478c-a62b-0a19cafa1358", "Signatory");
			filter.Category = FilterCategories.Organisations;
		}

		ZQuery GetSignatoryQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(RatingHeader));
			query.AddToFilter(RatingHeaderSchema.TH_GS_NKFirstSignatory, value);
			query.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_GS_NKSecondSignatory, value);

			return query;
		}

		protected override ModuleNkFilter CreateSalesRepFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddNkFilter(RateFilterHelper.Constants.StaffFilterSalesRep, OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible, ModuleIDs.GlbStaff, Staff);
			filter.SubGroup = new SalesRepSubGroupQuotation(RateFilterHelper);
			filter.MultilingualDescription = ResString.GetMultilingualString("50b3747a-d811-4a84-bf41-a46616a18065", "Sales Rep");
			return filter;
		}

		class SalesRepSubGroupQuotation : ModuleFilterSubGroup
		{
			public SalesRepSubGroupQuotation(RateFilterHelper helper)
			{
				this.helper = helper;
			}

			readonly RateFilterHelper helper;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				return helper.GetSalesRepQueryForQuotation(filter);
			}
		}

		protected override ModuleGuidFilter CreateOrganisationFilter(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter(RatingHeaderOrganisationName, ModuleIDs.Organisation, RateFilterHelper.GetOrganisationQueryForQuotation, TH_OH_List);
			filter.MultilingualDescription = RatingHeaderOrganisationCaption;
			return filter;
		}

		#endregion

		#region Lookups

		#region Status

		CodeDescriptionPairList QuoteStatuses => quoteStatuses ?? (quoteStatuses = QuoteStatus.GetStatuses());
		CodeDescriptionPairList quoteStatuses;

		#endregion

		#region Cancellation Reason

		ICodeDescriptionPairList CancellationReasonList
		{
			get { return RatingDataRegistry.Instance.QuoteCancellationReasonCodes.Value.GetCodeDescriptionPairList(); }
		}

		#endregion

		#endregion

		readonly QuotationsCRMSecurityProvider SecurityProvider = new QuotationsCRMSecurityProvider();

		protected override string GetRateType() => RatingConstants.RatingHeaderTypes.Quote;

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var helper = new WorkflowFilterStripsHelperWithRoutingSupport(typeof(Quote), WorkflowDescriptors.QuotationWorkflowDescriptorCode, Factory);
			helper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(helper);

			return helpers;
		}
	}
}

