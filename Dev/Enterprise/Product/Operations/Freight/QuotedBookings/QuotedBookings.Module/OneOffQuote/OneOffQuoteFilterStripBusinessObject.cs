using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Module
{
	public class OneOffQuoteFilterStripBusinessObject : BaseQuotedBookingFilterStripBusinessObject
	{
		public override ZQuery Filter
		{
			get
			{
				if (IsInWebQuoteMode)
				{
					ZDBOnlyQuery quoteQuery = new ZDBOnlyQuery(typeof(Quote));

					ZDBOnlySubQuery viewQuotedBookingSubQuery = new ZDBOnlySubQuery(typeof(ViewQuotedBooking), ViewQuotedBookingSchema.VB_TH);
					viewQuotedBookingSubQuery.AddToFilter(JoinCondition.And, ViewQuotedBookingSchema.VB_JS, SQLComparisonOperator.Equal, null);
					viewQuotedBookingSubQuery.AddToFilter(base.Filter);

					quoteQuery.AddSubQuery(viewQuotedBookingSubQuery, JoinCondition.And);
					return quoteQuery;
				}

				ZQuery query = base.Filter;
				query.AddToFilter(ViewQuotedBookingSchema.VB_GC, Env.CurrentCompany.PK);

				return query;
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			if (IsInWebQuoteMode)
			{
				AddWebQuoteFilters(filters);
			}
			else
			{
				AddNumbersAndReferencesFilters(filters);
				AddStatusAndFlagsFilters(filters);
				AddTextFilters(filters);
				AddOneOffQuoteDatesFilters(filters);
				AddLocationsFilters(filters);
				AddOrganisationsStaffFilters(filters);
				AddModesAndTypesFilters(filters);
				AddOneOffQuoteApprovalStatusFilters(filters);
				AddOneOffQuoteKPIFilters(filters);
				AddOneOffQuoteSourceFilters(filters);
				AddOneOffQuoteRevisionReasonFilters(filters);
				AddOneOffQuotePotentialCarriersFilters(filters);
				AccountingFilterStrip.AddBillingFilters(filters);
				AddOneOffQuoteHBLDeliveryModeFilter(filters);

				if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
				{
					AddCO2Filters(filters);
				}
			}

			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);

			return filters;
		}

		#region Text

		protected void AddTextFilters(ModuleFilterCollection filters)
		{
			var statusFilter = filters.AddTextFilter(Descriptions.StatusAndFlags.Status, GetQuoteStatusQuery, QuoteStatuses);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("7A70E0E0-80BF-4CD3-9315-8DA5E454AE9A", "Status");
			statusFilter.Category = FilterCategories.StatusAndFlags;

			var usedStatusFilter = filters.AddTextFilter(Descriptions.StatusAndFlags.Used, GetQuoteUsedQuery, YesNoBothStatusList);
			usedStatusFilter.MultilingualDescription = ResString.GetMultilingualString("6164725D-1C59-494D-8DB5-97AEC67F2D5D", "Used");
			usedStatusFilter.Category = FilterCategories.StatusAndFlags;
			usedStatusFilter.IsPublishedOnWeb = false;

			var finalPrintFilter = filters.AddTextFilter(Descriptions.StatusAndFlags.FinalPrint, GetQuoteFinalPrintQuery, YesNoBothStatusList);
			finalPrintFilter.MultilingualDescription = ResString.GetMultilingualString("1C3D495C-3216-4405-9E30-5B6182EA2126", "Final Print");
			finalPrintFilter.Category = FilterCategories.StatusAndFlags;
			finalPrintFilter.IsPublishedOnWeb = false;
		}

		ZQuery GetQuoteStatusQuery(ZString status)
		{
			var viewQuotedBookingQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));

			var quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);
			QuoteStatus.SetQueryStatusQuery(quoteSubQuery, status);

			viewQuotedBookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);
			return viewQuotedBookingQuery;
		}

		ZQuery GetQuoteUsedQuery(ZString status) => GetQuoteBoolStatusQuery(RatingHeaderSchema.TH_IsOneOffQuoteConsumed, status);

		ZQuery GetQuoteFinalPrintQuery(ZString status) => GetQuoteBoolStatusQuery(RatingHeaderSchema.TH_IsLocked, status);

		ZQuery GetQuoteBoolStatusQuery(SchemaBoolColumn column, ZString status)
		{
			var viewQuotedBookingQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));

			var quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);
			if (status == "Yes")
			{
				quoteSubQuery.AddToFilter(JoinCondition.And, column, SQLComparisonOperator.Equal, ZBool.True);
			}
			else if (status == "No")
			{
				quoteSubQuery.AddToFilter(JoinCondition.And, column, SQLComparisonOperator.Equal, ZBool.False);
			}

			viewQuotedBookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);
			return viewQuotedBookingQuery;
		}

		CodeDescriptionPairList QuoteStatuses => quoteStatuses ?? (quoteStatuses = QuoteStatus.GetStatuses(isFromOneOffQuoteModule: true));
		CodeDescriptionPairList quoteStatuses;

		public CodeDescriptionPairList YesNoBothStatusList
		{
			get
			{
				return new CodeDescriptionPairList
				{
					new CodeDescriptionPair(YesNoBothStatus.Yes, YesNoBothStatus.Yes),
					new CodeDescriptionPair(YesNoBothStatus.No, YesNoBothStatus.No),
					new CodeDescriptionPair(YesNoBothStatus.Both, YesNoBothStatus.Both)
				};
			}
		}

		public sealed class YesNoBothStatus
		{
			public static string Yes => Res.GetString("7d0a1a38-d68f-4bc8-83f3-fe591dbcb38c", "Yes");
			public static string No => Res.GetString("b5a1c069-0fb2-40b8-beed-8f239b7becb8", "No");
			public static string Both => Res.GetString("4a0e68d0-a9df-474f-925a-5018ab70ffad", "Both");
		}

		#endregion

		#region One Off Quote Dates

		void AddOneOffQuoteDatesFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(Descriptions.Dates.ClientAcceptedDate, GetClientAcceptedDateQuery).MultilingualDescription = ResString.GetMultilingualString("CFFD121C-ABD7-4C94-9406-C5CE51072C1F", "Client Accepted Date");

			var startDateFilter = filters.AddDateFilter(Descriptions.Dates.StartDate, GetStartDateQuery);
			startDateFilter.MultilingualDescription = ResString.GetMultilingualString("71F53E36-DF22-4693-AC0E-9E65243C0936", "Start Date");
			startDateFilter.IsPublishedOnWeb = false;

			var endDateFilter = filters.AddDateFilter(Descriptions.Dates.EndDate, GetEndDateQuery);
			endDateFilter.MultilingualDescription = ResString.GetMultilingualString("97345703-0356-4BC5-91A7-3B4D2AF9AE52", "End Date");
			endDateFilter.IsPublishedOnWeb = false;
		}

		ZQuery GetClientAcceptedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var viewQuotedBookingQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));

			var quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);
			AddDateTimeRange(quoteSubQuery, comparisonOperator, JoinCondition.And, RatingHeaderSchema.TH_ClientAccepted, fromDate, toDate, false, true);

			viewQuotedBookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);
			return viewQuotedBookingQuery;
		}

		ZQuery GetStartDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var viewQuotedBookingQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));

			var quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);
			AddDateTimeRange(quoteSubQuery, comparisonOperator, JoinCondition.And, RatingHeaderSchema.TH_QuoteDate, fromDate, toDate, false, true);

			viewQuotedBookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);
			return viewQuotedBookingQuery;
		}

		ZQuery GetEndDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var viewQuotedBookingQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));

			var quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);
			AddDateTimeRange(quoteSubQuery, comparisonOperator, JoinCondition.And, RatingHeaderSchema.TH_QuoteEndDate, fromDate, toDate, false, true);

			viewQuotedBookingQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);
			return viewQuotedBookingQuery;
		}

		#endregion

		#region Numbers And References

		protected override void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			base.AddNumbersAndReferencesFilters(filters);
			new FilterBuilder(filters, FilterCategories.NumbersAndReferences)
				//Quote
				.AddFountainFilter(Descriptions.NumbersAndReferences.QuoteNumber, GetQuoteNoQuery, "")
				.WithMaxLengthOf(RatingHeaderSchema.TH_QuoteNumber)
				.MultilingualDescription(ResString.GetMultilingualString("QuotedBookings|OneOffQuoteFilterControl|QuoteNum", "Quote #"))
				//Booking
				.AddFountainFilter(Descriptions.NumbersAndReferences.BookingNumber, GetBookingNoQuery, "")
				.WithMaxLengthOf(JobShipmentSchema.JS_UniqueConsignRef)
				.MultilingualDescription(ResString.GetMultilingualString("QuotedBookings|OneOffQuoteFilterControl|BookingNum", "Booking #"));
		}

		protected override ZQuery GetCompanyTariffLevelOverride(ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty)
			{
				ZByte.TryParse(value, out var companyTariffLevelOverride);
				var oneOffQuery = GetWithOneOffQuery(RateOneOffShipmentSchema.TT_CompanyTariffLevelOverride, SQLComparisonOperator.Equal, companyTariffLevelOverride);
				result.AddToFilter(oneOffQuery);
			}
			return result;
		}

		protected override void AddFMCTariffIDFilter(ModuleFilterCollection filters)
		{
			var fmcTariffIDFilter = filters.AddTextFilter(Descriptions.NumbersAndReferences.FMCTariffID, GetFMCTariffIDQuery);
			fmcTariffIDFilter.MaxLength = RateOneOffShipmentSchema.TT_FMCTariffID.MaxLength;
			fmcTariffIDFilter.Category = FilterCategories.NumbersAndReferences;
			fmcTariffIDFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|FMCTariffID", "FMC Tariff ID");
		}

		ZQuery GetFMCTariffIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZQuery();
			result.AddToFilter(GetWithOneOffQuery(RateOneOffShipmentSchema.TT_FMCTariffID, comparisonOperator, value));
			return result;
		}

		protected override void AddCommodityCodeFilter(ModuleFilterCollection filters)
		{
			var rateCommodityFilter = filters.AddNkFilter(Descriptions.NumbersAndReferences.CommodityCode, GetCommodityCodeQuery, ModuleIDs.RefCommodityCode, BindingLists.RefCommodityCode_List);
			rateCommodityFilter.Category = FilterCategories.NumbersAndReferences;
			rateCommodityFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|CommodityCode", "Commodity Code");
		}

		ZQuery GetCommodityCodeQuery(ZString value)
		{
			var result = new ZQuery();
			result.AddToFilter(GetWithOneOffQuery(RateOneOffShipmentSchema.TT_RH_NKCommodity, SQLComparisonOperator.Equal, value));
			return result;
		}

		#region GetQuoteNoQuery

		protected override ZQuery GetQuoteNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery result = base.GetQuoteNoQuery(comparisonOperator, value);

			if (value.IsEmpty)
			{
				if (comparisonOperator == SpecialComparisonOperator.IsBlank)
				{
					result.IsNoResultQuery = true;
				}
				else
				{
					return new ZQuery();
				}
			}
			return result;
		}

		#endregion

		#region GetBookingNoQuery

		ZQuery GetBookingNoQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery result = new ZQuery();

			if (!value.IsEmpty)
			{
				ZDBOnlyQuery viewFilter = GetWithBookingQuery(JobShipmentSchema.JS_UniqueConsignRef, comparisonOperator, value);
				result.AddToFilter(viewFilter, JoinCondition.And);
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(ViewQuotedBookingSchema.VB_JS, comparisonOperator, null);
			}

			return result;
		}

		#endregion

		#endregion

		#region WebQuoteMode

		public bool IsInWebQuoteMode
		{
			get { return isInWebQuoteMode && Globals.IsWeb; }
			set { isInWebQuoteMode = value; }
		}
		bool isInWebQuoteMode;

		protected void AddWebQuoteFilters(ModuleFilterCollection filters)
		{
			new FilterBuilder(filters, FilterCategories.NumbersAndReferences)
				.AddFountainFilter(Descriptions.WebQuote.QuoteNumber, GetQuoteNoQuery, "")
				.WithMaxLengthOf(RatingHeaderSchema.TH_QuoteNumber)
				.MultilingualDescription(ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|QuoteNum", "Quote #"));

			new FilterBuilder(filters, FilterCategories.StatusAndFlags)
				.AddStatusFilter(Descriptions.WebQuote.Status, GetWebQuoteStatusFilter, WebStatusList, AllQuotesStatus).MultilingualDescription(ResString.GetMultilingualString("2DED07FE-5004-4D61-85D4-35CAFC797849", "Status"));

			new FilterBuilder(filters, FilterCategories.Dates)
				.AddDateFilter(Descriptions.WebQuote.QuotationDate, GetWebQuoteDateFilter).MultilingualDescription(ResString.GetMultilingualString("51BC5BA0-995E-4E06-ADA7-92997D6DEC90", "Quotation Date"))
				.AddDateFilter(Descriptions.WebQuote.ExpiryDate, GetWebQuoteExpiryDateFilter).MultilingualDescription(ResString.GetMultilingualString("5E84845A-259D-4747-9E65-DB1D92C1F773", "Expiry Date"))
				.AddDateFilter(Descriptions.WebQuote.AcceptanceDate, GetWebQuoteAcceptanceDateFilter).MultilingualDescription(ResString.GetMultilingualString("0A116E4C-6ED2-4243-9920-B17F6A49453A", "Acceptance Date"));

			AddModeFilter(filters);
			AddOriginDestinationFilter(filters);
		}

		public CodeDescriptionPairList WebStatusList
		{
			get
			{
				return new CodeDescriptionPairList
						{
							new CodeDescriptionPair(ActiveStatus, Res.GetString("5b9f1bec-9c5d-4ce7-b033-dc64bcc57c69", "Active")),
							new CodeDescriptionPair(FinalisedStatus, Res.GetString("da303f84-e040-4fc9-a0ea-9188dc5d9dde", "Finalized")),
							new CodeDescriptionPair(AllQuotesStatus, Res.GetString("b8ea31d9-9575-4da0-a321-a92718ccb6b6", "All Quotes"))
						};
			}
		}

		protected const string ActiveStatus = "ACT";
		protected const string FinalisedStatus = "FIN";
		protected const string AllQuotesStatus = "ALL";

		protected ZQuery GetWebQuoteStatusFilter(ZString value)
		{
			ZQuery result = new ZQuery();

			switch (value)
			{
				case FinalisedStatus:
					result.AddToFilter(GetWithQuoteHeaderQuery(RatingHeaderSchema.TH_IsLocked, SQLComparisonOperator.Equal, ZBool.True));
					break;
				case ActiveStatus:
					result.AddToFilter(GetWithQuoteHeaderQuery(RatingHeaderSchema.TH_IsLocked, SQLComparisonOperator.Equal, ZBool.False));
					break;
			}

			return result;
		}

		protected ZQuery GetWebQuoteDateFilter(DateComparisonOperator comparisonOperator, ZDateTime minValue, ZDateTime maxValue)
		{
			return GetQuoteDateFilter(comparisonOperator, minValue, maxValue, RatingHeaderSchema.TH_QuoteDate);
		}

		protected ZQuery GetWebQuoteExpiryDateFilter(DateComparisonOperator comparisonOperator, ZDateTime minValue, ZDateTime maxValue)
		{
			return GetQuoteDateFilter(comparisonOperator, minValue, maxValue, RatingHeaderSchema.TH_QuoteEndDate);
		}

		protected ZQuery GetWebQuoteAcceptanceDateFilter(DateComparisonOperator comparisonOperator, ZDateTime minValue, ZDateTime maxValue)
		{
			return GetQuoteDateFilter(comparisonOperator, minValue, maxValue, RatingHeaderSchema.TH_Accepted);
		}

		protected ZQuery GetQuoteDateFilter(DateComparisonOperator comparisonOperator, ZDateTime minValue, ZDateTime maxValue, SchemaDateTimeColumn column)
		{
			ZQuery result = new ZQuery();
			AddDateRange(result, comparisonOperator, JoinCondition.And, column, minValue.Date, maxValue.Date);
			return result;
		}

		#endregion

		#region Locations

		#region GetOriginDestinationQuery

		protected override ZQuery GetOriginDestinationQuery(ZString originNK, ZString destinationNK)
		{
			ZQuery result = new ZQuery();

			if (!originNK.IsEmpty || !destinationNK.IsEmpty)
			{
				ZDBOnlyQuery quoteQuery = GetOriginDestinationQuoteQuery(originNK, destinationNK);
				result.AddToFilter(quoteQuery);
			}

			return result;
		}

		ZDBOnlyQuery GetOriginDestinationQuoteQuery(ZString originNK, ZString destinationNK)
		{
			ZDBOnlyQuery quoteQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			ZDBOnlySubQuery quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);
			ZDBOnlySubQuery entrySubQuery = new ZDBOnlySubQuery(typeof(RateOneOffShipment), RateOneOffShipmentSchema.TT_TH);

			//oneoffshipment
			ZDBOnlyQuery rateOneOffShipmentQuery = new ZDBOnlyQuery(typeof(RateOneOffShipment));
			if (!originNK.IsEmpty)
			{
				rateOneOffShipmentQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, originNK, RateOneOffShipmentSchema.TT_RL_NKReceivalLocation,
					typeof(RateOneOffShipment)));
			}
			if (!destinationNK.IsEmpty)
			{
				rateOneOffShipmentQuery.AddToFilter(LocationHelper.GetLocationFilter(Factory, destinationNK, RateOneOffShipmentSchema.TT_RL_NKDeliveryLocation,
					typeof(RateOneOffShipment)));
			}
			entrySubQuery.AddToFilter(rateOneOffShipmentQuery);

			quoteSubQuery.AddSubQuery(entrySubQuery, JoinCondition.And);
			quoteQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);
			return quoteQuery;
		}

		#endregion

		#endregion

		#region Organisations Staff

		protected override void AddOrganisationsStaffFilters(ModuleFilterCollection filters)
		{
			base.AddOrganisationsStaffFilters(filters);

			var consignee = Res.GetData("QuotedBookings|QuotedBookingFilterControl|Consignee", "Consignee");

			filters.AddGuidFilter(Descriptions.OrganisationsStaff.Carrier, ModuleIDs.Organisation, GetCarrierQuery, BindingLists.OrgHeader_List).MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|Carrier", "Carrier");
			filters.AddGuidFilter(Descriptions.OrganisationsStaff.Client, ModuleIDs.Organisation, GetClientQuery, BindingLists.OrgHeader_List).MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|Client", "Client");
			filters.AddGuidFilter(Descriptions.OrganisationsStaff.Creditor, ModuleIDs.Organisation, GetCreditorQuery, BindingLists.OrgHeader_List).MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|Creditor", "Creditor");

			var branchFilter = filters.AddGuidFilter(Descriptions.OrganisationsStaff.Branch, ModuleIDs.GlbBranch, GetBranchQuery, BindingLists.GlbBranch_List);
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.IsPublishedOnWeb = false;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|Branch", "Branch");

			var operationsRepFilter = filters.AddNkFilter(Descriptions.OrganisationsStaff.OperationsRepresentative, GetOperationsRepQuery, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory));
			operationsRepFilter.Category = FilterCategories.Organisations;
			operationsRepFilter.IsPublishedOnWeb = false;
			operationsRepFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|OperationsRepresentative", "Operations Representative");

			var consignorShipperTerminologyRegistryItem = FreightDataRegistry.Instance.ConsignorShipperTerminology;
			var consignorTerminology = consignorShipperTerminologyRegistryItem.Value.IsEmpty ?
					(ZString)consignorShipperTerminologyRegistryItem.DefaultValue :
					((ZString)consignorShipperTerminologyRegistryItem.Value).SubstringSafe(0, 15);
			var consignorConsigneeFilter = new ModuleGuidsFilterForOrg(Descriptions.OrganisationsStaff.ConsignorConsignee, ModuleIDs.Organisation, GetConsignorConsigneeQuery, BindingLists.OrgConsignor_FilterList, BindingLists.OrgConsignee_FilterList);
			consignorConsigneeFilter.SetItemDescriptions(new ResourceStringData("", consignorTerminology), consignee);
			consignorConsigneeFilter.MultilingualDescription = ResString.GetMultilingualString("E1E6D7CC-D3DC-4878-88A0-BAC9F7FE6BC4", "{0} / Consignee", consignorTerminology);
			filters.AddFilter(consignorConsigneeFilter);

			var consignorContactFilter = filters.AddTextFilter(Descriptions.OrganisationsStaff.ConsignorContact, GetConsignorContactQuery);
			consignorContactFilter.Category = FilterCategories.Organisations;
			consignorContactFilter.IsPublishedOnWeb = false;
			consignorContactFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ConsignorContact", "Consignor Contact");

			var consigneeContactFilter = filters.AddTextFilter(Descriptions.OrganisationsStaff.ConsigneeContact, GetConsigneeContactQuery);
			consigneeContactFilter.Category = FilterCategories.Organisations;
			consigneeContactFilter.IsPublishedOnWeb = false;
			consigneeContactFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ConsigneeContact", "Consignee Contact");

			var clientContactFilter = filters.AddTextFilter(Descriptions.OrganisationsStaff.ClientContact, GetClientContactQuery);
			clientContactFilter.Category = FilterCategories.Organisations;
			clientContactFilter.IsPublishedOnWeb = false;
			clientContactFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|ClientContact", "Client Contact");
		}

		#region GetCarrierQuery

		protected ZQuery GetCarrierQuery(ZGuid value)
		{
			ZQuery result = new ZQuery();

			if (!value.IsEmpty)
			{
				ZDBOnlyQuery oneOffQuery = GetWithOneOffQuery(RateOneOffShipmentSchema.TT_OH_Carrier, SQLComparisonOperator.Equal, value);
				result.AddToFilter(oneOffQuery);
			}

			return result;
		}

		#endregion

		#region GetCreditorQuery

		protected ZQuery GetCreditorQuery(ZGuid value)
		{
			var result = new ZQuery();

			if (!value.IsEmpty)
			{
				var oneOffQuery = GetWithOneOffQuery(RateOneOffShipmentSchema.TT_OH_Creditor, SQLComparisonOperator.Equal, value);
				result.AddToFilter(oneOffQuery);
			}

			return result;
		}

		#endregion

		#region GetClientQuery

		protected override ZQuery GetClientNameQuery(SQLComparisonOperator comparisonOperator, ZString companyName)
		{
			var addressCode = DocAddressTypes.GetCode(Factory, DocAddressType.QuotationClientAddress);

			var isBlankOrNot = comparisonOperator == SpecialComparisonOperator.IsBlank
				|| comparisonOperator == SQLComparisonOperator.DoesNotStartWith
				|| comparisonOperator == SQLComparisonOperator.NotContains
				|| comparisonOperator == SQLComparisonOperator.NotEqual;

			var condtion = isBlankOrNot ? JoinCondition.Or : JoinCondition.And;
			var operatorForJob = isBlankOrNot ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;

			// DocAddress

			var orgHeaders = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			orgHeaders.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, companyName);

			var orgAddresses = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			orgAddresses.AddSubQuery(orgHeaders, JoinCondition.And);

			var docAddresses1 = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddresses1.AddSubQuery(orgAddresses, JoinCondition.And);
			docAddresses1.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, addressCode);
			docAddresses1.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressOverride, SQLComparisonOperator.Equal, false);

			var docAddresses2 = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddresses2.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_CompanyName, comparisonOperator, companyName);
			docAddresses2.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, addressCode);
			docAddresses2.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressOverride, SQLComparisonOperator.Equal, true);

			// JobHeader

			var orgHeaderBookingSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			orgHeaderBookingSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, companyName);

			var addressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
			addressQuery.AddSubQuery(orgHeaderBookingSubQuery, JoinCondition.And);

			var jobSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobSubQuery.AddSubQuery(addressQuery, JoinCondition.And);
			jobSubQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			jobSubQuery.AddToFilter(condtion, JobHeaderSchema.JH_OA_LocalChargesAddr, operatorForJob, null);

			//Quote

			var quoteJobSubQuery = new ZDBOnlySubQuery(typeof(ViewQuotedBooking), ViewQuotedBookingSchema.VB_TH, false, true, true);
			quoteJobSubQuery.AddToFilter(ViewQuotedBookingSchema.VB_JS, SQLComparisonOperator.NotEqual, null);
			quoteJobSubQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, jobSubQuery, JoinCondition.And);

			var quoteSubQuery = new ZDBOnlySubQuery(typeof(ViewQuotedBooking), ViewQuotedBookingSchema.VB_TH, false, true, true);
			quoteSubQuery.AddToFilter(ViewQuotedBookingSchema.VB_JS, null);
			quoteSubQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, docAddresses1, JoinCondition.And);

			var quoteDocAddressSubQuery = new ZDBOnlySubQuery(typeof(ViewQuotedBooking), ViewQuotedBookingSchema.VB_TH, false, true, true);
			quoteDocAddressSubQuery.AddToFilter(ViewQuotedBookingSchema.VB_JS, null);
			quoteDocAddressSubQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, docAddresses2, JoinCondition.And);

			var quoteQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			quoteQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteJobSubQuery, JoinCondition.Or);
			quoteQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.Or);
			quoteQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteDocAddressSubQuery, JoinCondition.Or);

			return quoteQuery;
		}

		protected ZQuery GetClientQuery(ZGuid value)
		{
			ZQuery result = new ZQuery();
			ZString addressCode = DocAddressTypes.GetCode(Factory, DocAddressType.QuotationClientAddress);

			if (value.IsValid)
			{
				// DocAddress

				ZDBOnlySubQuery docAddresses = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				ZDBOnlySubQuery orgAddresses = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

				orgAddresses.AddToFilter(OrgAddressSchema.OA_OH, value);
				docAddresses.AddSubQuery(orgAddresses, JoinCondition.And);
				docAddresses.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, addressCode);

				//Quote

				ZDBOnlyQuery quoteQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
				ZDBOnlySubQuery quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);

				quoteSubQuery.AddSubQuery(RatingHeaderSchema.PK, docAddresses, JoinCondition.And);
				quoteQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.Or);

				result.AddToFilter(quoteQuery);
			}

			return result;
		}

		#endregion

		#region GetConsignorConsigneeQuery

		protected ZQuery GetConsignorConsigneeQuery(ZGuid consignorPK, ZGuid consigneePK)
		{
			ZQuery result = new ZQuery();

			if (consignorPK.IsValid || consigneePK.IsValid)
			{
				ZDBOnlyQuery quoteQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
				ZDBOnlySubQuery quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);

				if (consignorPK.IsValid)
				{
					quoteSubQuery.AddToFilter(GetQuoteOrgDocAddressQuery(DocAddressType.OneOffQuotePickupAddress, consignorPK));
				}
				if (consigneePK.IsValid)
				{
					quoteSubQuery.AddToFilter(GetQuoteOrgDocAddressQuery(DocAddressType.OneOffQuoteDeliveryAddress, consigneePK));
				}

				quoteQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);

				result.AddToFilter(quoteQuery);
			}

			return result;
		}

		#endregion

		#region GetRelatedPartiesQuery

		protected override ZQuery GetClientRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			ZQuery docAddress = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(orgHeaderFilter, new ZQuery(), false, false);
			docAddress.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.QuotationClientAddress);
			ZQuery quoteQuery = GetQuoteFromDocAddressFilter(docAddress);

			ZQuery result = new ZQuery();
			result.AddToFilter(quoteQuery);

			return result;
		}

		protected override ZQuery GetConsignorRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			ZQuery docAddress = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(orgHeaderFilter, new ZQuery(), false, false);
			docAddress.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.OneOffQuotePickupAddress);

			ZQuery quoteQuery = GetRateOneOffShipmentFromDocAddressFilter(docAddress);

			ZQuery result = new ZQuery();
			result.AddToFilter(quoteQuery);

			return result;
		}

		protected override ZQuery GetConsigneeRelatedPartiesQuery(ZQuery orgHeaderFilter)
		{
			ZQuery docAddress = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(orgHeaderFilter, new ZQuery(), false, false);
			docAddress.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.Codes.OneOffQuoteDeliveryAddress);
			ZQuery quoteQuery = GetRateOneOffShipmentFromDocAddressFilter(docAddress);

			ZQuery result = new ZQuery();
			result.AddToFilter(quoteQuery);

			return result;
		}

		#endregion

		#region GetBranchQuery

		protected ZQuery GetBranchQuery(ZGuid branchGuid)
		{
			var branchQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			var jobSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);

			jobSubQuery.AddToFilter(JobHeaderSchema.JH_GB, branchGuid);
			jobSubQuery.AddToFilter(JobHeaderSchema.JH_ParentTableCode, RatingHeaderSchema.Constants.Prefix);

			branchQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, jobSubQuery, JoinCondition.And);

			return branchQuery;
		}

		#endregion

		#region GetOperationsRepQuery

		protected ZQuery GetOperationsRepQuery(ZString operationsRepNK)
		{
			var operationsRepQuery = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
			var jobSubQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);

			jobSubQuery.AddToFilter(JobHeaderSchema.JH_GS_NKRepOps, operationsRepNK);
			jobSubQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			jobSubQuery.AddToFilter(JobHeaderSchema.JH_ParentTableCode, RatingHeaderSchema.Constants.Prefix);

			operationsRepQuery.AddSubQuery(ViewQuotedBookingSchema.VB_TH, jobSubQuery, JoinCondition.And);

			return operationsRepQuery;
		}

		#endregion

		#region GetContactQuery

		protected ZQuery GetConsignorContactQuery(SQLComparisonOperator comparisonOperator, ZString consignorContact)
		{
			return GetContactQuery(DocAddressType.OneOffQuotePickupAddress, comparisonOperator, consignorContact);
		}

		protected ZQuery GetConsigneeContactQuery(SQLComparisonOperator comparisonOperator, ZString consigneeContact)
		{
			return GetContactQuery(DocAddressType.OneOffQuoteDeliveryAddress, comparisonOperator, consigneeContact);
		}

		protected ZQuery GetClientContactQuery(SQLComparisonOperator comparisonOperator, ZString clientContact)
		{
			return GetContactQuery(DocAddressType.QuotationClientAddress, comparisonOperator, clientContact);
		}

		protected ZQuery GetContactQuery(DocAddressType docAddressType, SQLComparisonOperator comparisonOperator, ZString contact)
		{
			var docAddressesSubQuery = new ZQuery()
				.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_Contact, comparisonOperator, contact)
				.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, DocAddressTypes.GetCode(Factory, docAddressType));

			return docAddressType switch
			{
				DocAddressType.OneOffQuotePickupAddress or DocAddressType.OneOffQuoteDeliveryAddress => GetRateOneOffShipmentFromDocAddressFilter(docAddressesSubQuery),
				DocAddressType.QuotationClientAddress => GetQuoteFromDocAddressFilter(docAddressesSubQuery),
				_ => throw new NotSupportedException($"docAddressType '{docAddressType}' is not supported.")
			};
		}

		#endregion

		#endregion

		#region Modes And Types

		#region GetModeQuery

		protected override ZQuery GetModeQuery(ZString value)
		{
			var oneOffViewQuery = new ZQuery();

			if (!value.IsEmpty)
			{
				oneOffViewQuery.AddToFilter(GetWithOneOffQuery(RateOneOffShipmentSchema.TT_TransportMode, SQLComparisonOperator.Equal, RatingConstants.GetTransportModeFromMode(value)), JoinCondition.And);
				oneOffViewQuery.AddToFilter(GetWithOneOffQuery(RateOneOffShipmentSchema.TT_ContainerMode, SQLComparisonOperator.Equal, RatingConstants.GetOneOffQuoteContainerModeFromMode(value)), JoinCondition.And);
			}

			return oneOffViewQuery;
		}

		#endregion

		protected override ZQuery GetTransportModeQuery(ZString value)
		{
			var oneOffViewQuery = new ZQuery();
			if (!value.IsEmpty)
			{
				oneOffViewQuery.AddToFilter(GetWithOneOffQuery(RateOneOffShipmentSchema.TT_TransportMode, SQLComparisonOperator.Equal, value), JoinCondition.And);
			}

			return oneOffViewQuery;
		}

		protected override ZQuery GetContainerModeQuery(ZString value)
		{
			var oneOffViewQuery = new ZQuery();
			if (!value.IsEmpty)
			{
				oneOffViewQuery.AddToFilter(GetWithOneOffQuery(RateOneOffShipmentSchema.TT_ContainerMode, SQLComparisonOperator.Equal, value), JoinCondition.And);
			}

			return oneOffViewQuery;
		}

		protected void AddOneOffQuoteHBLDeliveryModeFilter(ModuleFilterCollection filters)
		{
			var hBLDeliveryModeList = RateEntryLookups.GetHBLDeliveryModeList(string.Empty, string.Empty);
			var modeFilter = filters.AddTextFilter(Descriptions.ModesAndTypes.HBLDeliveryMode, GetOneOffQuoteHBLDeliveryModeQuery, hBLDeliveryModeList);
			modeFilter.Category = FilterCategories.ModesAndTypes;
			modeFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|OneOffQuoteFilterControl|HBLDeliveryMode", "HBL Delivery Mode");
		}

		ZQuery GetOneOffQuoteHBLDeliveryModeQuery(ZString value)
		{
			var oneOffViewQuery = new ZQuery();
			if (!value.IsEmpty)
			{
				oneOffViewQuery.AddToFilter(GetWithOneOffQuery(RateOneOffShipmentSchema.TT_HBLDeliveryMode, SQLComparisonOperator.Equal, value), JoinCondition.And);
			}

			return oneOffViewQuery;
		}

		#region GetServiceLevelQuery

		protected override ZQuery GetServiceLevelQuery(ZString value)
		{
			ZQuery result = new ZQuery();

			if (!value.IsEmpty)
			{
				ZQuery oneOffViewQuery = GetWithOneOffQuery(RateOneOffShipmentSchema.TT_RS_NKServiceLevel, SQLComparisonOperator.Equal, value);
				result.AddToFilter(oneOffViewQuery, JoinCondition.Or);
			}

			return result;
		}

		#endregion

		#endregion

		#region Quote Approval Status
		protected virtual void AddOneOffQuoteApprovalStatusFilters(ModuleFilterCollection filters)
		{
			var oneOffQuoteApprovalStatusFilter = filters.AddTextFilter(Descriptions.StatusAndFlags.OneOffQuoteApprovalStatus, GetOneOffQuoteApprovalStatusQuery, OneOffQuoteApprovalStatusList);
			oneOffQuoteApprovalStatusFilter.Category = FilterCategories.StatusAndFlags;
			oneOffQuoteApprovalStatusFilter.DefaultProperty = OrgConstants.FilterControl.ActiveStatus.Code.AllClients;
			oneOffQuoteApprovalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|OneOffQuoteApprovalStatus", Descriptions.StatusAndFlags.OneOffQuoteApprovalStatus);
		}

		#region GetQuoteActiveStatusQuery
		ZQuery GetOneOffQuoteApprovalStatusQuery(ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty && value != OrgConstants.FilterControl.ActiveStatus.Code.AllClients)
			{
				var isApproved = value == OrgConstants.FilterControl.ActiveStatus.Code.ActiveClients;
				var oneOffQuery = GetWithOneOffQuery(RateOneOffShipmentSchema.TT_QuoteApprovedByManager, SQLComparisonOperator.Equal, isApproved);
				result.AddToFilter(oneOffQuery);
			}
			return result;
		}
		#endregion
		#endregion

		protected virtual void AddOneOffQuoteKPIFilters(ModuleFilterCollection filters)
		{
			var oneOffQuoteApprovalStatusFilter = filters.AddTextFilter(Descriptions.StatusAndFlags.OneOffQuoteKPI, GetOneOffQuoteKPIQuery, OneOffQuoteKPIList);
			oneOffQuoteApprovalStatusFilter.Category = FilterCategories.StatusAndFlags;
			oneOffQuoteApprovalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|OneOffQuoteKPI", Descriptions.StatusAndFlags.OneOffQuoteKPI);
		}

		ZQuery GetOneOffQuoteKPIQuery(ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty)
			{
				var oneOffQuery = GetWithOneOffQuery(RateOneOffShipmentSchema.TT_QuoteKPI, SQLComparisonOperator.Equal, value);
				result.AddToFilter(oneOffQuery);
			}
			return result;
		}

		protected virtual void AddOneOffQuoteSourceFilters(ModuleFilterCollection filters)
		{
			var oneOffQuoteApprovalStatusFilter = filters.AddTextFilter(Descriptions.StatusAndFlags.OneOffQuoteSource, GetOneOffQuoteSourceQuery, OneOffQuoteSourceList);
			oneOffQuoteApprovalStatusFilter.Category = FilterCategories.StatusAndFlags;
			oneOffQuoteApprovalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|OneOffQuoteSource", Descriptions.StatusAndFlags.OneOffQuoteSource);
		}

		ZQuery GetOneOffQuoteSourceQuery(ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty)
			{
				var oneOffQuery = GetWithOneOffQuery(RateOneOffShipmentSchema.TT_QuoteSource, SQLComparisonOperator.Equal, value);
				result.AddToFilter(oneOffQuery);
			}
			return result;
		}

		protected virtual void AddOneOffQuoteRevisionReasonFilters(ModuleFilterCollection filters)
		{
			var oneOffQuoteApprovalStatusFilter = filters.AddTextFilter(Descriptions.StatusAndFlags.OneOffQuoteRevisionReason, GetOneOffQuoteRevisionReasonQuery, OneOffQuoteRevisionReasonList);
			oneOffQuoteApprovalStatusFilter.Category = FilterCategories.StatusAndFlags;
			oneOffQuoteApprovalStatusFilter.MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|OneOffQuoteRevisionReason", Descriptions.StatusAndFlags.OneOffQuoteRevisionReason);
		}

		protected virtual void AddOneOffQuotePotentialCarriersFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidInSubCollectionFilter(
				Descriptions.PotentialCarriers.PotentialCarrier,
				FilterCategories.Organisations,
				ModuleIDs.Organisation,
				GetPotentialCarriersFilterQuery(RateOneOffCarrierSchema.TTC_OH_Carrier),
				new OrgHeaderCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|OneOffQuotePotentialCarrier", Descriptions.PotentialCarriers.PotentialCarrier);

			filters.AddGuidInSubCollectionFilter(
				Descriptions.PotentialCarriers.PotentialCreditor,
				FilterCategories.Organisations,
				ModuleIDs.Organisation,
				GetPotentialCarriersFilterQuery(RateOneOffCarrierSchema.TTC_OH_Creditor),
				new OrgHeaderCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("QuotedBookings|QuotedBookingFilterControl|OneOffQuotePotentialCreditor", Descriptions.PotentialCarriers.PotentialCreditor);
		}

		GetGuidQueryWithNotIn GetPotentialCarriersFilterQuery(SchemaColumn rateOneOffCarrierColumn) =>
			(ZGuid value, bool notIn) =>
			{
				var oneOffCarrierSubQuery = new ZDBOnlySubQuery(typeof(RateOneOffCarrier), RateOneOffCarrierSchema.TTC_TT);
				oneOffCarrierSubQuery.AddToFilter(rateOneOffCarrierColumn, value);

				var oneOffShipmentSubQuery = new ZDBOnlySubQuery(typeof(RateOneOffShipment), RateOneOffShipmentSchema.TT_TH, notIn);
				oneOffShipmentSubQuery.AddSubQuery(oneOffCarrierSubQuery, JoinCondition.And);

				var quoteSubQuery = new ZDBOnlySubQuery(typeof(Quote), RatingHeaderSchema.PK);
				quoteSubQuery.AddSubQuery(oneOffShipmentSubQuery, JoinCondition.And);

				var viewFilter = new ZDBOnlyQuery(typeof(ViewQuotedBooking));
				viewFilter.AddSubQuery(ViewQuotedBookingSchema.VB_TH, quoteSubQuery, JoinCondition.And);
				return viewFilter;
			};

		ZQuery GetOneOffQuoteRevisionReasonQuery(ZString value)
		{
			var result = new ZQuery();
			if (!value.IsEmpty)
			{
				var oneOffQuery = GetWithOneOffQuery(RateOneOffShipmentSchema.TT_RevisionReason, SQLComparisonOperator.Equal, value);
				result.AddToFilter(oneOffQuery);
			}
			return result;
		}

		protected override bool IsBookingOnly => false;

		readonly OneOffQuoteCRMSecurityProvider SecurityProvider = new OneOffQuoteCRMSecurityProvider();
	}
}
