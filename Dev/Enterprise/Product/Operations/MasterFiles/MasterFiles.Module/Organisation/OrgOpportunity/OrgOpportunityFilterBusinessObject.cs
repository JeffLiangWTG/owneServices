using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Organisation.OpportunityManagement.OrgOpportunity;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module.Organisation.CommissionAgreement;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class OrgOpportunityFilterBusinessObject : FilterStripBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddLocationFilter(filters);

			filters.AddGuidFilter("Organisation", ModuleIDs.Organisation, OrgOpportunitySchema.P8_OH, SalesOrganisations).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|Organisation", "Organization");
			filters.AddGuidFilter("Company", ModuleIDs.GlbCompany, OrgOpportunitySchema.P8_GC, Companies).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|Company", "Company");
			filters.AddNkFilter("Sales Person", OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson, ModuleIDs.GlbStaff, SalesPersons).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|SalesPerson", "Sales Person");
			filters.AddGuidFilter("Referring Organization", ModuleIDs.Organisation, OrgOpportunitySchema.P8_OH_ReferringOrganisation, Organisations).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|ReferringOrganization", "Referring Organization");

			filters.AddTextFilter("Description", OrgOpportunitySchema.P8_OpportunityDescription).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|Description", "Description");
			filters.AddTextFilter("Status", OrgOpportunitySchema.P8_Status, Statuses).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|Status", "Status");
			filters.AddTextFilter("Source Details", OrgOpportunitySchema.P8_SourceDetails).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|SourceDetails", "Source Details");

			filters.AddNumberRangeFilter("Close Certainty", OrgOpportunitySchema.P8_CloseCertainty).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|CloseCertainty", "Close Certainty");
			filters.AddTextFilter("Product Type", OrgOpportunitySchema.P8_PackageType, ProductTypes).MultilingualDescription = OrganisationsDataRegistry.Instance.ProductTypeLabel.Value;
			filters.AddNumberRangeFilter("Total Estimated Value (p.a)", OrgOpportunitySchema.P8_EstimatedValue).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|TotalEstimatedValuePA", "Total Estimated Value (p.a)");
			var currentFilter = filters.AddNumberRangeFilter("Current", OrgOpportunitySchema.P8_DiscountAmount);
			currentFilter.MultilingualDescription = OrganisationsDataRegistry.Instance.CurrentLabel.Value;
			currentFilter.Decimals = 0;
			var potentialFilter = filters.AddNumberRangeFilter("Potential", OrgOpportunitySchema.P8_RentalMultiplier);
			potentialFilter.MultilingualDescription = OrganisationsDataRegistry.Instance.PotentialLabel.Value;
			potentialFilter.Decimals = 0;
			var filter = (ModuleFilter)filters.AddGuidFilter("Assigned Office", ModuleIDs.Organisation, GetAssignedOfficeQueryWithOperator, Organisations, OrgOpportunitySchema.P8_OA_AssignedOffice);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|AssignedOffice", "Assigned Office");
			filter = filters.AddTextFilter("Value Type", OrgOpportunityValueSchema.PV_RevenueType, ValueTypes);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|ValueType", "Value Type");
			filter.SubGroup = new ValueTypeSubGroup();
			filter = filters.AddTextFilter("Client Size", OrgMiscServSchema.OM_CMClientSize, ClientSize);
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|ClientSize", "Client Size");
			filter.SubGroup = new ClientSizeSubGroup();

			filters.AddTextFilter("Sales Type", OrgOpportunitySchema.P8_OpportunityType, Types).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|Sales Type", "Sales Type");
			filters.AddTextFilter("Outcome", OrgOpportunitySchema.P8_Outcome, Outcomes).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|Outcome", "Outcome");
			filters.AddTextFilter("Stage", OrgOpportunitySchema.P8_Stage, Stages).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|Stage", "Stage");
			filters.AddTextFilter("Source", OrgOpportunitySchema.P8_Source, Sources).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|Source", "Source");
			filters.AddTextFilter("Close Reason", OrgOpportunitySchema.P8_LostReason, ClosedReasons).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|CloseReason", "Close Reason");
			filters.AddTextFilter("Overall Disposition", GetOverallDispositionQuery, OverallDisposition).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|OverallDisposition", "Overall Disposition");

			filters.AddDateFilter("Close Date", OrgOpportunitySchema.P8_ClosedDate, true).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|CloseDate", "Close Date");
			filters.AddDateFilter("Estimated Close Date", OrgOpportunitySchema.P8_EstimatedCloseDate, true).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|EstimatedCloseDate", "Estimated Close Date");
			filters.AddDateFilter("Recall Date", OrgOpportunitySchema.P8_RecallDate, true).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|RecallDate", "Recall Date");
			filters.AddDateFilter("Last Quoted Date", GetLastQuotedDateQuery).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|LastQuotedDate", "Last Quoted Date");
			filters.AddNkFilter("Sales Team", GetSalesTeamsQueryWithOperator, ModuleIDs.SalesTeam, SalesTeams).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|SalesTeam", "Sales Team");

			var flagFilter = filters.AddFlagsFilter("Created From Inquiry", new string[] { ResString.GetMultilingualString("ce434b68-a013-4760-b0e2-a14c2db25bbe", "Created From Inquiry") }, new GetFlagsQuery[] { GetCreatedFromInquiryQuery });
			flagFilter.MultilingualDescription = ResString.GetMultilingualString("ce434b68-a013-4760-b0e2-a14c2db25bbe", "Created From Inquiry");

			AddCommissionAgreementFilters(filters);
			AddRelatedTradeLanesFilter(filters);

			filters.AddWorkflowCustomFieldsFilters(Factory, OpportunityWorkflowDescriptor.WorkflowTypeCode, typeof(OrgOpportunity));

			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);
			ObjectFactory.Get<IValueAnalysisModuleHelper>()?.AddValueAnalysisModuleFilterStrips(OrgOpportunitySchema.PK, filters, Factory, typeof(OrgOpportunity));

			return filters;
		}

		#region OverallDispositionQuery
		ZQuery GetOverallDispositionQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			if (!value.IsEmpty)
			{
				var closedStatuses = new List<string>();
				foreach (CodeDescriptionBool status in OrganisationsDataRegistry.Instance.OpportunityStatus.Value)
				{
					if (status.Bool)
					{
						closedStatuses.Add(status.Code);
					}
				}

				if (value == OrgOpportunityOverallDispositionList.Codes.Closed)
				{
					result.AddToFilter(OrgOpportunitySchema.P8_Status, closedStatuses);
				}
				else
				{
					result.AddToFilter(OrgOpportunitySchema.P8_Status, SQLComparisonOperator.NotEqual, closedStatuses);
				}
			}
			return result;
		}
		#endregion

		#region LastQuotedDate

		ZQuery GetLastQuotedDateQuery(DateComparisonOperator comparisonOperator, ZDateTime dateFrom, ZDateTime dateTo)
		{
			#region SuppressResourceStringsCheckRegion

			string dateCondition = string.Empty;
			string inCondition = string.Empty;
			ZSqlParameterCollection sqlParams = null;
			if (comparisonOperator == DateComparisonOperator.HasDateInRange)
			{
				inCondition = "IN";
				dateCondition = "BETWEEN @FromDate AND @ToDate";
				sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@FromDate", dateFrom, RatingHeaderSchema.TH_QuoteDate);
				sqlParams.Add("@ToDate", dateTo, RatingHeaderSchema.TH_QuoteDate);
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				inCondition = "IN";
				dateCondition = "IS NOT NULL";
			}
			else if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				inCondition = "NOT IN";
				dateCondition = "IS NOT NULL";
			}

			string filter = string.Format(
			@"{0} {1} (SELECT {2} 
				FROM 
					dbo.RatingHeader
					JOIN dbo.JobDocAddress ON {3} = {4}
					JOIN dbo.OrgAddress ON {5} = {6}
					JOIN dbo.OrgHeader ON {7} = {2}
				GROUP BY {2}
				HAVING MAX({8}) {9})",
				OrgOpportunitySchema.Constants.P8_OH,           //0
				inCondition,                                    //1
				OrgHeaderSchema.Constants.PK,                   //2
				JobDocAddressSchema.Constants.E2_ParentID,      //3
				RatingHeaderSchema.Constants.PK,                //4
				JobDocAddressSchema.Constants.E2_OA_Address,    //5
				OrgAddressSchema.Constants.PK,                  //6
				OrgAddressSchema.Constants.OA_OH,               //7
				RatingHeaderSchema.Constants.TH_QuoteDate,      //8
				dateCondition);                                 //9

			#endregion

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgOpportunity));
			query.AddFilterAndZSQLParameterCollection(filter, sqlParams);
			return query;
		}

		#endregion

		#region Assigned Office

		ZQuery GetAssignedOfficeQueryWithOperator(ZDBOnlySubQuery filterQuery, SQLComparisonOperator comparisonOperator, object value)
		{
			var orgOpportunityQuery = new ZDBOnlyQuery(typeof(OrgOpportunity));

			if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				orgOpportunityQuery.AddToFilter(OrgOpportunitySchema.P8_OA_AssignedOffice, comparisonOperator, DBNull.Value);
			}
			else
			{
				var subQueryAddress = new ZDBOnlySubQuery(typeof(OrgAddress), OrgOpportunitySchema.P8_OA_AssignedOffice);
				if (filterQuery != null)
				{
					subQueryAddress.AddSubQuery(OrgAddressSchema.OA_OH, OrgHeaderSchema.PK, filterQuery, JoinCondition.And);
				}
				else
				{
					subQueryAddress.AddToFilter(OrgAddressSchema.OA_OH, comparisonOperator, value);
				}
				orgOpportunityQuery.AddSubQuery(OrgOpportunitySchema.P8_OA_AssignedOffice, OrgAddressSchema.PK, subQueryAddress, JoinCondition.And);
			}

			return orgOpportunityQuery;
		}

		#endregion

		#region Value Type

		class ValueTypeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgOpportunity));
				ZDBOnlySubQuery valueSubQuery = new ZDBOnlySubQuery(typeof(OrgOpportunityValue), OrgOpportunityValueSchema.PV_P8);
				valueSubQuery.AddToFilter(filter);
				result.AddSubQuery(valueSubQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion

		#region Client Size

		class ClientSizeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgOpportunity));
				ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgOpportunitySchema.P8_OH);
				ZDBOnlySubQuery orgMiscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
				orgMiscServSubQuery.AddToFilter(filter);
				orgSubQuery.AddSubQuery(orgMiscServSubQuery, JoinCondition.And);
				result.AddSubQuery(orgSubQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion

		#region Location

		void AddLocationFilter(ModuleFilterCollection filters)
		{
			ModuleNkFilter filter = filters.AddNkFilter("Location", GetLocationQuery, ModuleIDs.Location, Locations);
			filter.MaxLength = OrgHeaderSchema.OH_RL_NKClosestPort.MaxLength;
			filter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|Organization Related City/Port", "Organization Related City/Port");
			filter.SubGroup = new LocationSubGroup();

			if (!Env.Security.OrganisationAllowSearchOutsideLoginCountry.IsAllowed)
			{
				filter.Visibility = FilterVisibility.AlwaysVisible;
				filter.DefaultProperty = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				filter.PropertyValidation = LocationCountryFilterValidation;
			}
		}

		void LocationCountryFilterValidation(ZPropertyInfo info)
		{
			if (!((ZString)info.Value).StartsWith(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				string errorMessage = Res.GetString("f5b155b0-f405-48f3-b82c-8d625a54b46a", @"Your current security rights only allow you to view opportunities relating to organizations based in your current login country/region ({0}).
If you think this is incorrect, please contact your system administrator.", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				info.AddError(errorMessage);
			}
		}

		class LocationSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZQuery query = new ZQuery();

				ZDBOnlyQuery oppQuery = new ZDBOnlyQuery(typeof(OrgOpportunity));
				ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgOpportunitySchema.P8_OH);
				orgSubQuery.AddToFilter(filter);
				oppQuery.AddSubQuery(orgSubQuery, JoinCondition.And);

				query.AddToFilter(oppQuery);

				return query;
			}
		}

		ZQuery GetLocationQuery(ZString value)
		{
			ZQuery query = new ZQuery();
			if (!value.IsEmpty)
			{
				query.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, value);
			}
			return query;
		}

		#endregion

		#region Commission Agreement

		FilterCategory CommissionAgreementCategory
		{
			get { return commissionAgreementCategory ?? (commissionAgreementCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("2fd8a6de-f0d8-4e0a-98b0-6ae81151c628", "Commission Agreement"))); }
		}
		FilterCategory commissionAgreementCategory;

		void AddCommissionAgreementFilters(ModuleFilterCollection filters)
		{
			var commissionAgreementsModuleFilter = new CommissionAgreementsModuleFilter("Commission Agreements", OrgOpportunitySchema.PK, OrgCommissionAgreementSchema.CA0_P8, new OrgCommissionAgreementCollection(Factory), typeof(OrgOpportunity));
			commissionAgreementsModuleFilter.MultilingualDescription = ResString.GetMultilingualString("BB8F8972-9972-4545-90CD-07F4C321E2C5", "Commission Agreements");
			commissionAgreementsModuleFilter.Category = CommissionAgreementCategory;
			filters.AddFilter(commissionAgreementsModuleFilter);
		}

		#endregion

		#region Related Trade Lanes

		void AddRelatedTradeLanesFilter(ModuleFilterCollection filters)
		{
			var relatedTradeLanesCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("33E88EAA-DD56-4547-9D19-B33D1331A177", "Related Trade Lanes"));
			var tradeLaneSubGroup = new TradeLaneSubGroup();
			var productSubGroup = new TradeLaneProductSubGroup(tradeLaneSubGroup);
			var detailSubGroup = new TradeLaneDetailSubGroup(tradeLaneSubGroup);
			var prospectSubGroup = new TradeLaneProspectSubGroup(detailSubGroup);

			var directionFilter = filters.AddTextFilter("Sales Monthly Trade Mode", GetDirectionQuery, RelatedTradeLanes_DirectionsList);
			directionFilter.Category = relatedTradeLanesCategory;
			directionFilter.SubGroup = tradeLaneSubGroup;
			directionFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|RelatedTradeLanesDirection", "Direction");

			var locationFilter = filters.AddLocationFilter("Sales Trade Lane Origin / Destination", GetLocationQuery, Locations, Locations);
			locationFilter.MaxLength = ViewLocationSchema.VLO_Code.MaxLength;
			locationFilter.SetItemDescriptions(Res.GetData("MasterFiles|OrgOpportunityFilter|RelatedTradeLanesDirection|Origin", "Origin"), Res.GetData("MasterFiles|OrgOpportunityFilter|RelatedTradeLanesDirection|Destination", "Destination"));
			locationFilter.Category = relatedTradeLanesCategory;
			locationFilter.SubGroup = tradeLaneSubGroup;
			locationFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|RelatedTradeLanesLocation", "Origin / Destination");

			var salesProductFilter = filters.AddTextFilter("Sales Trade Lane Product", GetSalesProductQuery, SalesProductTypes);
			salesProductFilter.Category = relatedTradeLanesCategory;
			salesProductFilter.SubGroup = productSubGroup;
			salesProductFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|RelatedTradeLanesDetailTradeType", "Sales Product");

			var tradeStatusFilter = filters.AddTextFilter("Sales Trade Lane Status", GetTradeStatusQuery, TradeStatus);
			tradeStatusFilter.Category = relatedTradeLanesCategory;
			tradeStatusFilter.SubGroup = detailSubGroup;
			tradeStatusFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|RelatedTradeLanesDetailStatus", "Trade Status");

			var commodityfilter = filters.AddNkFilter("Sales Trade Lane Commodity", OrgTradeProspectSchema.PAP_RH_NKCommodityCode, ModuleIDs.RefCommodityCode, Commodities);
			commodityfilter.Category = relatedTradeLanesCategory;
			commodityfilter.SubGroup = prospectSubGroup;
			commodityfilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|RelatedTradeLanesDetailCommodity", "Trade Commodity");
		}

		ZQuery GetDirectionQuery(ZString direction)
		{
			ZQuery result = new ZQuery();
			if (!direction.IsEmpty)
			{
				if (direction == Core.Constants.Sales.Mode.Import)
				{
					result.AddToFilter(OrgSalesSchema.OW_OH_Buyer, SQLComparisonOperator.Equal, OrgOpportunitySchema.P8_OH);
				}
				else if (direction == Core.Constants.Sales.Mode.Export)
				{
					result.AddToFilter(OrgSalesSchema.OW_OH_Supplier, SQLComparisonOperator.Equal, OrgOpportunitySchema.P8_OH);
				}
			}
			return result;
		}

		ZQuery GetLocationQuery(ZString originCode, ZString destinationCode)
		{
			var result = new ZDBOnlyQuery(typeof(OrgSales));

			if (!originCode.IsEmpty)
			{
				var originSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_OriginID);
				originSubQuery.AddToFilter(ViewLocationSchema.VLO_Code, SQLComparisonOperator.StartsWith, originCode);
				result.AddSubQuery(originSubQuery, JoinCondition.And);
			}
			if (!destinationCode.IsEmpty)
			{
				var destinationSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_DestinationID);
				destinationSubQuery.AddToFilter(ViewLocationSchema.VLO_Code, SQLComparisonOperator.StartsWith, destinationCode);
				result.AddSubQuery(destinationSubQuery, JoinCondition.And);
			}

			return result;
		}

		ZQuery GetSalesProductQuery(ZString value)
		{
			return new ZQuery(OrgSalesProductSchema.MP_Code, value);
		}

		ZQuery GetTradeStatusQuery(ZString value)
		{
			return new ZQuery(OrgTradeDetailSchema.PA_Status, value);
		}

		class TradeLaneSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(OrgOpportunity));

				var pivotSubQuery = new ZDBOnlySubQuery(typeof(OrgSalesValueAssociationPivot), OrgSalesValueAssociationPivotSchema.SVP_ActivityId);
				var tradeLanesSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesValueAssociationPivotSchema.SVP_TradeId);

				tradeLanesSubQuery.AddToFilter(filter);

				pivotSubQuery.AddSubQuery(tradeLanesSubQuery, JoinCondition.And);
				result.AddSubQuery(pivotSubQuery, JoinCondition.And);

				return result;
			}
		}

		class TradeLaneProspectSubGroup : ModuleFilterSubGroup
		{
			public TradeLaneProspectSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgTradeDetail));
				ZDBOnlySubQuery detailSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeProspect), OrgTradeProspectSchema.PAP_PA);
				detailSubQuery.AddToFilter(filter);
				result.AddSubQuery(detailSubQuery, JoinCondition.And);
				return result;
			}
		}

		class TradeLaneDetailSubGroup : ModuleFilterSubGroup
		{
			public TradeLaneDetailSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgSales));
				ZDBOnlySubQuery detailSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeDetail), OrgTradeDetailSchema.PA_OW);
				detailSubQuery.AddToFilter(filter);
				result.AddSubQuery(detailSubQuery, JoinCondition.And);
				return result;
			}
		}

		class TradeLaneProductSubGroup : ModuleFilterSubGroup
		{
			public TradeLaneProductSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{ }

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(OrgSales));
				ZDBOnlySubQuery productSubQuery = new ZDBOnlySubQuery(typeof(IOrgSalesProduct), OrgSalesSchema.OW_MP_Product);
				productSubQuery.AddToFilter(filter);
				result.AddSubQuery(productSubQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion

		#region Sales Team

		ZQuery GetSalesTeamsQueryWithOperator(SQLComparisonOperator comparisonOperator, ZString salesTeam)
		{
			ZQuery zQuery = new ZQuery();

			ZDBOnlyQuery orgOpportunityQuery = new ZDBOnlyQuery(typeof(OrgOpportunity));
			ZDBOnlySubQuery subQueryGlbStaff = new ZDBOnlySubQuery(typeof(GlbStaff), OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson);
			ZDBOnlySubQuery subQueryGroupLink = new ZDBOnlySubQuery(typeof(GlbGroupLink), GlbGroupLinkSchema.GK_GS, comparisonOperator == SpecialComparisonOperator.IsBlank);
			ZDBOnlySubQuery subQueryGroup = new ZDBOnlySubQuery(typeof(GlbGroup), GlbGroupLinkSchema.GK_GG);
			subQueryGroup.AddToFilter(GlbGroupSchema.GG_IsSales, true);

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				subQueryGroupLink.AddSubQuery(subQueryGroup, JoinCondition.And);
				subQueryGlbStaff.AddSubQuery(subQueryGroupLink, JoinCondition.And);
				orgOpportunityQuery.AddSubQuery(OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson, GlbStaffSchema.GS_Code, subQueryGlbStaff, JoinCondition.And);

				ZDBOnlySubQuery orgOpportunitySalesPersonUnAssigned = new ZDBOnlySubQuery(typeof(OrgOpportunity), OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson);
				orgOpportunitySalesPersonUnAssigned.AddToFilter(OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson, ZString.Empty);
				orgOpportunityQuery.AddSubQuery(OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson, OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson, orgOpportunitySalesPersonUnAssigned, JoinCondition.Or);
			}
			else
			{
				subQueryGroup.AddToFilter(GlbGroupSchema.GG_Code, comparisonOperator, salesTeam.SubstringSafe(0, GlbGroupSchema.GG_Code.MaxLength));
				subQueryGroupLink.AddSubQuery(subQueryGroup, JoinCondition.And);
				subQueryGlbStaff.AddSubQuery(subQueryGroupLink, JoinCondition.And);
				orgOpportunityQuery.AddSubQuery(OrgOpportunitySchema.P8_GS_NKPrimarySalesPerson, GlbStaffSchema.GS_Code, subQueryGlbStaff, JoinCondition.And);
			}

			zQuery.AddToFilter(orgOpportunityQuery);
			return zQuery;
		}

		#endregion

		#region Created From Inquiry

		ZQuery GetCreatedFromInquiryQuery(ZBool value)
		{
			if (value)
			{
				return new ZQuery(OrgOpportunitySchema.P8_O1_Enquiry, SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				return new ZQuery();
			}
		}

		#endregion

		#region GetModuleFilterThatOverridesAllOtherFilters

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			var opportunityIDFilter = new ModuleFountainFilter("Opportunity ID", OrgOpportunitySchema.P8_OpportunityID, "O");
			opportunityIDFilter.IsCommon = true;
			opportunityIDFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|OrgOpportunityFilter|OpportunityID", "Opportunity ID");
			return opportunityIDFilter;
		}

		#endregion

		#endregion

		#region Lookups

		#region Organisations

		SalesOrganisationCollection SalesOrganisations
		{
			get { return salesOrganisations ?? (salesOrganisations = new SalesOrganisationCollection(Factory)); }
		}
		SalesOrganisationCollection salesOrganisations;

		OrgHeaderCollection Organisations
		{
			get { return organisations ?? (organisations = new OrgHeaderCollection(Factory)); }
		}
		OrgHeaderCollection organisations;

		#endregion

		#region Companies

		GlbCompanyCollection Companies
		{
			get { return companies ?? (companies = new GlbCompanyCollection(Factory)); }
		}
		GlbCompanyCollection companies;

		#endregion

		#region Sales Persons

		GlbStaffCollection SalesPersons
		{
			get
			{
				if (fSalesPersons == null)
				{
					fSalesPersons = new GlbStaffCollection(Factory);
				}
				return fSalesPersons;
			}
		}

		GlbStaffCollection fSalesPersons;

		#endregion

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		#endregion

		#region Types

		ReadOnlyCodeDescriptionPairList Types
		{
			get { return OrganisationsDataRegistry.Instance.OpportunitySalesTypes.Value.GetCodeDescriptionPairList(); }
		}

		#endregion

		#region Stages

		ReadOnlyCodeDescriptionPairList Stages
		{
			get { return OrganisationsDataRegistry.Instance.OpportunityStages.Value.GetCodeDescriptionPairList(); }
		}

		#endregion

		#region Statuses

		ICodeDescriptionBoolList Statuses
		{
			get
			{
				if (fStatuses == null)
				{
					fStatuses = OrganisationsDataRegistry.Instance.OpportunityStatus.Value;
				}
				return fStatuses;
			}
		}

		ICodeDescriptionBoolList fStatuses;

		#endregion

		#region Outcomes

		ReadOnlyCodeDescriptionPairList Outcomes
		{
			get { return OrganisationsDataRegistry.Instance.OpportunityOutcome.Value.GetCodeDescriptionPairList(); }
		}

		#endregion

		#region Sources

		ReadOnlyCodeDescriptionPairList Sources
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityFilterLookups.Sources", OrganisationsDataRegistry.Instance.OpportunitySource.Value.GetCodeDescriptionPairList);
			}
		}

		#endregion

		#region Closed Reasons

		ReadOnlyCodeDescriptionPairList ClosedReasons
		{
			get { return OrganisationsDataRegistry.Instance.ClosedOpportunityReasons.Value.GetCodeDescriptionPairList(); }
		}

		#endregion

		#region Overall Disposition
		ReadOnlyCodeDescriptionPairList OverallDisposition
		{
			get
			{
				return fOverallDispositionList ?? (fOverallDispositionList = new OrgOpportunityOverallDispositionList());
			}
		}
		OrgOpportunityOverallDispositionList fOverallDispositionList;
		#endregion

		#region Related Trade Lanes

		public CodeDescriptionPairList RelatedTradeLanes_DirectionsList
		{
			get
			{
				return fRelatedTradeLanes_DirectionsList ?? (fRelatedTradeLanes_DirectionsList = new CodeDescriptionPairList(OLookUpEditType.SalesMode));
			}
		}
		CodeDescriptionPairList fRelatedTradeLanes_DirectionsList;

		#endregion

		#region Product Types

		public ReadOnlyCodeDescriptionPairList ProductTypes
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityFilterBusinessObject.ProductTypes", OrganisationsDataRegistry.Instance.ProductTypeList.Value.GetCodeDescriptionPairList);
			}
		}

		#endregion

		#region Value Types

		public virtual CodeDescriptionPairList ValueTypes
		{
			get { return valueTypes ?? (valueTypes = new CodeDescriptionPairList(new OrgOpportunityValueLookups(null).ValueTypes)); }
		}
		CodeDescriptionPairList valueTypes;

		#endregion

		#region Client Size

		public ReadOnlyCodeDescriptionPairList ClientSize
		{
			get { return OrganisationsDataRegistry.Instance.ClientSizeList.Value; }
		}

		#endregion

		#region Sales Product Types

		public CodeDescriptionPairList SalesProductTypes
		{
			get
			{
				return Factory.GetCachedValue("OrgOpportunityFilterBusinessObject|SalesProductTypes", () =>
				{
					var result = new CodeDescriptionPairList();
					var products = Factory.Load<IOrgSalesProduct>(new ZQuery());
					foreach (var product in products)
					{
						result.AddPair(product.MP_Code, product.MP_Name);
					}
					result.Sort();

					return result;
				});
			}
		}

		#endregion

		#region Trade Status

		public CodeDescriptionPairList TradeStatus
		{
			get { return new OrgTradeDetail.TradeLaneStatus(); }
		}

		#endregion

		#region Commodities

		public RefCommodityCodeCollection Commodities
		{
			get { return commodities ?? (commodities = new RefCommodityCodeCollection(Factory)); }
		}
		protected RefCommodityCodeCollection commodities;

		#endregion

		#region Sales Teams

		SalesTeamCollection SalesTeams
		{
			get
			{
				if (salesTeams == null)
				{
					salesTeams = new SalesTeamCollection(Factory);
				}
				return salesTeams;
			}
		}

		SalesTeamCollection salesTeams;

		#endregion

		#endregion

		protected override void OnModuleFiltersCreated()
		{
			base.OnModuleFiltersCreated();
			moduleFiltersCreated = true;
		}

		protected override void OnModuleFiltersReset()
		{
			base.OnModuleFiltersReset();
			moduleFiltersCreated = false;
		}

		bool moduleFiltersCreated;

		protected IEnumerable<ModuleFilter> GetActiveFiltersByDescription(string description)
		{
			if (moduleFiltersCreated)
			{
				Regex descriptionRegExp = new Regex(@"^" + description + @"(\s\([\d]\))?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
				foreach (ModuleFilter filter in ActiveModuleFilters)
				{
					if (descriptionRegExp.IsMatch(filter.Description))
					{
						yield return filter;
					}
				}
			}
		}

		readonly OrgOpportunityCRMSecurityProvider SecurityProvider = new OrgOpportunityCRMSecurityProvider();
	}
}
