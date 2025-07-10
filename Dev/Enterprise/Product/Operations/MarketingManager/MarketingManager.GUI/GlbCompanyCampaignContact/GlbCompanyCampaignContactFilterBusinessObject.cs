using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Recruiter;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class GlbCompanyCampaignContactFilterBusinessObject : FilterStripBusinessObject, IRelatedModuleFilterBusinessObject, ISetCampaignFilterLayoutContext
	{
		public GlbCompanyCampaignContactFilterBusinessObject(GlbCompanyCampaign campaign)
		{
			Campaign = campaign;
		}

		/// <summary>
		/// Parameterless constructor for color scheme and filter rule support.
		/// </summary>
		public GlbCompanyCampaignContactFilterBusinessObject()
		{
			QueryObjectType = typeof(CampaignContact);
		}

		protected GlbCompanyCampaign Campaign;

		protected virtual ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.DripMarketingFilterRule;
		}

		ZGuid Owner
		{
			get { return Campaign.G0_GCG_Group.IsEmpty ? Campaign.PK : Campaign.G0_GCG_Group; }
		}

		protected override FilterStripLayoutsHelper GetNewLayoutsHelper()
		{
			if (layoutsHelper == null)
			{
				layoutsHelper = new GlbCompanyCampaignFilterStripLayoutsHelper();
			}
			layoutsHelper.BizObjPK = Env.CurrentUser.PK;
			if (Campaign != null)
			{
				layoutsHelper.AdditionalObjPK = Owner;
			}

			if (setLayoutContextForHelper)
			{
				SetCampaignFilterLayoutContext();
			}

			return layoutsHelper;
		}

		internal GlbCompanyCampaignFilterStripLayoutsHelper layoutsHelper;

		public override StmModuleFilter FindLayout(ZString layoutName, ZBool isPublished)
		{
			var query = new ZQuery(StmModuleFilterSchema.S9_FilterName, layoutName);
			query.AddToFilter(StmModuleFilterSchema.S9_IsPublished, isPublished);
			query.AddToFilter(StmModuleFilterSchema.S9_ModuleID, CurrentLayoutContext);

			var nonSystemDefaultSubQuery = new ZQuery(StmModuleFilterSchema.S9_IsSystem, 0);
			if (layoutName.IsEmpty)
			{
				nonSystemDefaultSubQuery.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, Owner);
			}

			var systemDefaultSubQuery = new ZQuery(StmModuleFilterSchema.S9_IsSystem, 1);

			var conditionalQuery = new ZQuery(nonSystemDefaultSubQuery, JoinCondition.Or, systemDefaultSubQuery);
			query.AddToFilter(conditionalQuery);
			return Layouts.Find(query).FirstOrDefault();
		}

		public string CurrentLayoutContext
		{
			get
			{
				if (Campaign == null)
				{
					return ModuleIDs.GlbCompanyCampaignContact.Name;
				}
				var array = new[] { ModuleIDs.GlbCompanyCampaignContact.Name, Campaign.ContactDataSourceAsFilterModuleIDSuffix };
				return string.Join("_", array.Where(x => !string.IsNullOrEmpty(x)));
			}
		}

		public bool IsContactDataSourceChanging;

		internal void SetCampaignFilterLayoutContext()
		{
			if (Campaign == null)
			{
				return;
			}
			if (layoutsHelper != null)
			{
				layoutsHelper.BizObjPK = Owner;
			}
			((IFilterStripBusinessObjectInternals)this).LayoutContext = CurrentLayoutContext;
		}

		bool setLayoutContextForHelper;
		void ISetCampaignFilterLayoutContext.SetContext()
		{
			SetCampaignFilterLayoutContext();
			setLayoutContextForHelper = true;
		}

		public new StmModuleFilter GetLastUsedLayout()
		{
			var query = new ZQuery { ReLoadExistingRows = true };
			query.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, Owner);
			query.AddToFilter(StmModuleFilterSchema.S9_ModuleID, CurrentLayoutContext);
			var layout = Campaign.IsUsingCloneFactory ? Campaign.Factory.LoadTop1<StmModuleFilter>(query) : LastUsedCampaignContactLayoutFactory.LoadTop1<StmModuleFilter>(query);
			if (layout == null)
			{
				layout = base.GetLastUsedLayout();
			}
			else
			{
				SetCampaignFilterLayoutContext();
			}
			return layout;
		}

		protected override void SetLastUsedLayoutValue(StmData lastUsedFilterStmData)
		{
			if (!lastUsedLayout.IsRowDeletedOrDetachedOrNull)
			{
				if (!lastUsedLayout.S9_FilterNameMultilingual.IsEmpty || (lastUsedLayout.S9_FilterNameMultilingual.IsEmpty && Campaign != null && lastUsedFilterStmData.SD_Owner == Owner))
				{
					base.SetLastUsedLayoutValue(lastUsedFilterStmData);
				}
			}

			CampaignContactFilterDataSourceHelper.SetCampaignLastUsedLayoutValue(lastUsedFilterStmData, LastUsedCampaignContactLayoutFactory);
		}

		protected override ZQuery LastUsedLayoutNameQuery()
		{
			ZQuery query = base.LastUsedLayoutNameQuery();
			query.AddToFilter(CampaignContactFilterDataSourceHelper.CampaignLastUsedLayoutNameQuery());
			return query;
		}

		protected override System.Collections.ObjectModel.ReadOnlyCollection<StmModuleFilter> GetLayouts(bool isPublished)
		{
			List<StmModuleFilter> result = new List<StmModuleFilter>();

			foreach (StmModuleFilter layout in Layouts)
			{
				if (layout.S9_IsPublished == isPublished && !layout.S9_FilterNameMultilingual.IsEmpty)
				{
					result.Add(layout);
				}
			}

			return result.AsReadOnly();
		}

		public BusinessObjectFactory LastUsedCampaignContactLayoutFactory
		{
			get { return lastUsedCampaignContactLayoutFactory ?? (lastUsedCampaignContactLayoutFactory = new BusinessObjectFactory() { NameForDebugging = "FilterStripBusinessObject.LastUsedPreferredLayoutFactory" }); }
		}
		BusinessObjectFactory lastUsedCampaignContactLayoutFactory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public ZGuid[] SourceCampaignPKs
		{
			get
			{
				if (!Campaign.IsMasterCampaign && !Campaign.IsTouchCampaign)
				{
					return new[]
					{
						Campaign.SourceCampaignPK
					};
				}
				else
				{
					return Campaign.TouchSourceCampaignPKs ?? Array.Empty<ZGuid>();
				}
			}
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new GlbCompanyCampaignModuleFilterCollection(Campaign);

			AddActiveOrgContactQuery(filters);
			AddSubscribedRecipientsQuery(filters);
			GetDataFromTableDeciderFilters(filters);
			AddNumbersAndReferencesFilters(filters);
			AddStatusAndFlagsFilters(filters);
			AddRelationshipOrgAndStaffFilters(filters);
			AddDateFilters(filters);
			AddStaffAssignmentPersonAndRoleModuleFilters(filters);
			AddTextFilters(filters);
			AddCampaignTrackerFilters(filters);
			AddRelatedItemFilters(filters);
			AddUtcOffsetFilter(filters);
			AddInquiryFilters(filters);

			filters.AddDateFilter(FilterDescription.ContactDetailsVerifiedDate, ViewCampaignContactSchema.VCC_DetailsVerified, true).MultilingualDescription = ResString.GetMultilingualString("42c71d1e-11c0-4916-ac3d-9981a4094925", FilterDescription.ContactDetailsVerifiedDate);

			ObjectFactory.Get<IValueAnalysisModuleHelper>()?.AddValueAnalysisModuleFilterStrips(ViewCampaignContactSchema.VCC_OH, filters, Factory, typeof(CampaignContact));

			return filters;
		}

		protected internal virtual void ResetCampaignModuleFilters()
		{
			var filters = ModuleFilters as GlbCompanyCampaignModuleFilterCollection;
			if (filters != null)
			{
				filters.ResetCampaignFilterList();
			}
		}

		#region NumbersAndReferences Filters

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it's an identifier")]
		protected const string AchievableBusinessDescription = "Sales - Achievable Business";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it's an identifier")]
		protected const string PercentageWonDescription = "Sales - Percentage Won";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it's an identifier")]
		protected const string TotalRevenueDescription = "Sales - Total Revenue";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it's an identifier")]
		protected const string WarehouseRevenueDescription = "Sales - Warehouse Revenue";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it's an identifier")]
		protected const string ConsultingRevenueDescription = "Sales - Consulting Revenue";

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			var orgMiscServSubGroup = OrgMiscServSubGroupInstance;

			CampaignContactNumberFilter achieveableBusinessFilter = new CampaignContactNumberFilter(AchievableBusinessDescription, GetSalesAchieveableBusiness);
			achieveableBusinessFilter.MultilingualDescription = ResString.GetMultilingualString("f5563451-2cea-4044-9014-18f1bd4da525", FilterDescription.SalesAchievableBusiness);
			achieveableBusinessFilter.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(achieveableBusinessFilter);

			var salesClientSize = filters.AddTextFilter(FilterDescription.SalesClientSize, OrgMiscServSchema.OM_CMClientSize, ClientSizes);
			salesClientSize.MultilingualDescription = ResString.GetMultilingualString("d2c5d3cc-e7dd-4a69-b672-d76d922a0931", FilterDescription.SalesClientSize);
			salesClientSize.Category = FilterCategories.NumbersAndReferences;
			salesClientSize.SubGroup = orgMiscServSubGroup;

			CampaignContactTextRangeFilter salesClientRelationship = new CampaignContactTextRangeFilter(FilterDescription.SalesClientRelationship, GetSalesClientRelationship, ZeroToTenList);
			salesClientRelationship.MultilingualDescription = ResString.GetMultilingualString("45e847ed-0112-4f1e-b28e-a002b9f5ad04", FilterDescription.SalesClientRelationship);
			salesClientRelationship.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(salesClientRelationship);

			CampaignContactTextRangeFilter salesClientDesireToRemain = new CampaignContactTextRangeFilter(FilterDescription.SalesClientDesireToRemainWithCompany, GetSalesClientDesireToRemainWithComapny, ZeroToTenList);
			salesClientDesireToRemain.MultilingualDescription = ResString.GetMultilingualString("a44e2f62-72f5-4241-9ca2-40f7bf5143f7", FilterDescription.SalesClientDesireToRemainWithCompany);
			salesClientDesireToRemain.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(salesClientDesireToRemain);

			CampaignContactTextRangeFilter salesDifficultyWhichClientCanBePoached = new CampaignContactTextRangeFilter(FilterDescription.SalesDifficultyWhichClientCanBePoached, GetSalesDifficultyWhichClientCanBePoached, ZeroToTenList);
			salesDifficultyWhichClientCanBePoached.MultilingualDescription = ResString.GetMultilingualString("7fba1e8d-b870-4622-99d8-5d665d1b3b6e", FilterDescription.SalesDifficultyWhichClientCanBePoached);
			salesDifficultyWhichClientCanBePoached.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(salesDifficultyWhichClientCanBePoached);

			CampaignContactTextRangeFilter salesAmountOfClientElectronicIntegration = new CampaignContactTextRangeFilter(FilterDescription.SalesAmountOfClientElectronicIntegration, GetSalesAmountOfClientElectronicIntegration, ZeroToTenList);
			salesAmountOfClientElectronicIntegration.MultilingualDescription = ResString.GetMultilingualString("2ed25d44-a6a8-4e4c-8071-90496b7735f7", FilterDescription.SalesAmountOfClientElectronicIntegration);
			salesAmountOfClientElectronicIntegration.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(salesAmountOfClientElectronicIntegration);

			CampaignContactNumberFilter salesConsultingRevenue = new CampaignContactNumberFilter(ConsultingRevenueDescription, GetSalesConsultingRevenue);
			salesConsultingRevenue.MultilingualDescription = ResString.GetMultilingualString("64c06411-b4b6-43c6-9e60-51e8cfe03a67", FilterDescription.SalesConsultingRevenue);
			salesConsultingRevenue.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(salesConsultingRevenue);

			CampaignContactNumberFilter salesConversionCertainty = new CampaignContactNumberFilter("Sales - Conversion Certainty", GetSalesConversionCertainty);
			salesConversionCertainty.MultilingualDescription = ResString.GetMultilingualString("820fa8aa-98a6-4d0d-8b0a-a777efeaae14", FilterDescription.SalesConversionCertainty);
			salesConversionCertainty.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(salesConversionCertainty);

			CampaignContactNumberFilter salesEstimatedProfit = new CampaignContactNumberFilter(FilterDescription.SalesEstimatedProfit, GetSalesEstimatedProfit);
			salesEstimatedProfit.MultilingualDescription = ResString.GetMultilingualString("7d08a79a-db84-4c36-b735-abf841ef8586", FilterDescription.SalesEstimatedProfit);
			salesEstimatedProfit.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(salesEstimatedProfit);

			var salesGrowthOutlook = filters.AddTextFilter(FilterDescription.SalesGrowthOutlook, OrgMiscServSchema.OM_CMGrowthOutlook, SalesGrowthOutlooks);
			salesGrowthOutlook.MultilingualDescription = ResString.GetMultilingualString("51167520-27f9-44b9-b335-3567810dd1bb", FilterDescription.SalesGrowthOutlook);
			salesGrowthOutlook.Category = FilterCategories.NumbersAndReferences;
			salesGrowthOutlook.SubGroup = orgMiscServSubGroup;

			CampaignContactNumberFilter salesPercentageWon = new CampaignContactNumberFilter(PercentageWonDescription, GetSalesPercentageWon);
			salesPercentageWon.MultilingualDescription = ResString.GetMultilingualString("fea04cce-7fea-47e0-b822-c4544a9ebd55", FilterDescription.SalesPercentageWon);
			salesPercentageWon.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(salesPercentageWon);

			CampaignContactNumberFilter salesRelatedStaff = new CampaignContactNumberFilter(FilterDescription.SalesRelatedStaff, GetSalesRelatedStaff);
			salesRelatedStaff.MultilingualDescription = ResString.GetMultilingualString("01894730-36b1-463e-83a9-0eda804c950f", FilterDescription.SalesRelatedStaff);
			salesRelatedStaff.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(salesRelatedStaff);

			var salesCategory = filters.AddTextFilter(FilterDescription.SalesCategory, OrgMiscServSchema.OM_CMSalesCategory, SalesCategories);
			salesCategory.MultilingualDescription = ResString.GetMultilingualString("b593070b-1c45-4fce-afa9-91f88863f814", FilterDescription.SalesCategory);
			salesCategory.Category = FilterCategories.StatusAndFlags;
			salesCategory.SubGroup = orgMiscServSubGroup;

			var verticalMarket = filters.AddTextFilter(FilterDescription.VerticalMarket, OrgMiscServSchema.OM_CMIndustryVertical, VerticalMarketTypes);
			verticalMarket.MultilingualDescription = ResString.GetMultilingualString("4141E3E5-2F81-4352-A27D-ED0C450712A8", FilterDescription.VerticalMarket);
			verticalMarket.Category = FilterCategories.StatusAndFlags;
			verticalMarket.SubGroup = orgMiscServSubGroup;

			var salesTerritory = filters.AddTextFilter(FilterDescription.SalesTerritory, OrgMiscServSchema.OM_CMSalesTerritory, SalesTerritories);
			salesTerritory.MultilingualDescription = ResString.GetMultilingualString("847d221c-6c6b-401b-bc8b-92f6b32ff184", FilterDescription.SalesTerritory);
			salesTerritory.Category = FilterCategories.StatusAndFlags;
			salesTerritory.SubGroup = orgMiscServSubGroup;

			CampaignContactNumberFilter salesTotalRevenue = new CampaignContactNumberFilter(TotalRevenueDescription, GetSalesTotalRevenue);
			salesTotalRevenue.MultilingualDescription = ResString.GetMultilingualString("12bce7dc-021f-41cd-820a-6d4ee6dac209", FilterDescription.SalesTotalRevenue);
			salesTotalRevenue.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(salesTotalRevenue);

			CampaignContactNumberFilter salesWarehouseRevenue = new CampaignContactNumberFilter(WarehouseRevenueDescription, GetSalesWarehouseRevenue);
			salesWarehouseRevenue.MultilingualDescription = ResString.GetMultilingualString("cb6caff9-70d6-4934-9fbd-f1516d4603b4", FilterDescription.SalesWarehouseRevenue);
			salesWarehouseRevenue.Category = FilterCategories.NumbersAndReferences;
			filters.AddCustomFilter(salesWarehouseRevenue);
		}

		protected ZQuery OrgMiscServ<T>(SQLComparisonOperator comparisonOperator, T value, SchemaColumn filterColumn)
		{
			ZDBOnlySubQuery orgMiscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
			if (filterColumn is SchemaByteColumn && value.GetType() == typeof(ZString))
			{
				ZByte valueToUse;
				if (ZByte.TryParse(value.ToString(), out valueToUse))
				{
					orgMiscServSubQuery.AddToFilter(filterColumn, comparisonOperator, valueToUse);
				}
			}
			else
			{
				orgMiscServSubQuery.AddToFilter(filterColumn, comparisonOperator, value);
			}

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

			query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgMiscServSubQuery, JoinCondition.And);
			return query;
		}

		readonly OrgMiscServSubGroup OrgMiscServSubGroupInstance = new OrgMiscServSubGroup();

		class OrgMiscServSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var orgMiscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
				orgMiscServSubQuery.AddToFilter(filter);
				var query = new ZDBOnlyQuery(typeof(CampaignContact));
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
				query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgMiscServSubQuery, JoinCondition.And);
				return query;
			}
		}

		ZQuery GetSalesAchieveableBusiness(SQLComparisonOperator comparisonOperator, ZInt value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgMiscServ(comparisonOperator, value, OrgMiscServSchema.OM_CMAcheivableClientRevenue), JoinCondition.And);
			return query;
		}

		ZQuery GetSalesClientRelationship(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return OrgMiscServ(comparisonOperator, value, OrgMiscServSchema.OM_CMOverallClientRelation);
		}

		ZQuery GetSalesClientDesireToRemainWithComapny(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return OrgMiscServ(comparisonOperator, value, OrgMiscServSchema.OM_CMClientsDesireToRemain);
		}

		ZQuery GetSalesDifficultyWhichClientCanBePoached(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return OrgMiscServ(comparisonOperator, value, OrgMiscServSchema.OM_CMEaseClientCanBePoached);
		}

		ZQuery GetSalesAmountOfClientElectronicIntegration(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return OrgMiscServ(comparisonOperator, value, OrgMiscServSchema.OM_CMAmountOfElectronicIntegration);
		}

		ZQuery GetSalesConsultingRevenue(SQLComparisonOperator comparisonOperator, ZInt value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgMiscServ(comparisonOperator, value, OrgMiscServSchema.OM_CMConsultingRevenue), JoinCondition.And);
			return query;
		}

		ZQuery GetSalesConversionCertainty(SQLComparisonOperator comparisonOperator, ZInt value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgMiscServ(comparisonOperator, value, OrgMiscServSchema.OM_CMPercentage), JoinCondition.And);
			return query;
		}

		ZQuery GetSalesEstimatedProfit(SQLComparisonOperator comparisonOperator, ZInt value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgMiscServ(comparisonOperator, value, OrgMiscServSchema.OM_CMEstimatedProfit), JoinCondition.And);
			return query;
		}

		ZQuery GetSalesPercentageWon(SQLComparisonOperator comparisonOperator, ZInt value)
		{
			ZByte val = (byte)value;

			ZQuery query = new ZQuery();
			query.AddToFilter(OrgMiscServ(comparisonOperator, val, OrgMiscServSchema.OM_CMAmountOfBusinessWon), JoinCondition.And);
			return query;
		}

		ZQuery GetSalesRelatedStaff(SQLComparisonOperator comparisonOperator, ZInt value)
		{
			ZInt val = value;

			ZQuery query = new ZQuery();
			query.AddToFilter(OrgMiscServ(comparisonOperator, val, OrgMiscServSchema.OM_CMNoOfEmployees), JoinCondition.And);
			return query;
		}

		ZQuery GetSalesTotalRevenue(SQLComparisonOperator comparisonOperator, ZInt value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgMiscServ(comparisonOperator, value, OrgMiscServSchema.OM_CMTotalClientRevenue), JoinCondition.And);
			return query;
		}

		ZQuery GetSalesWarehouseRevenue(SQLComparisonOperator comparisonOperator, ZInt value)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(OrgMiscServ(comparisonOperator, value, OrgMiscServSchema.OM_CMWarehouseRevenue), JoinCondition.And);
			return query;
		}

		#endregion

		#region StatusAndFlags Filters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var contactSourceFilters = filters.AddTextFilter(FilterDescription.ContactSource, ViewCampaignContactSchema.VCC_ContactSource, ContactSources);
			contactSourceFilters.MultilingualDescription = ResString.GetMultilingualString("6b95783e-2b2e-4bdc-bf6c-e0b2a70d1e93", FilterDescription.ContactSource);
			contactSourceFilters.Category = FilterCategories.StatusAndFlags;

			var jobCategoryFilter = filters.AddTextFilter(FilterDescription.JobCategory, ViewCampaignContactSchema.VCC_JobCategory, JobCategoryList);
			jobCategoryFilter.MultilingualDescription = ResString.GetMultilingualString("95d578e9-75bb-4da7-9a0d-351fd8a9ad76", FilterDescription.JobCategory);
			jobCategoryFilter.Category = CampaignContactFilterCategories.ClientIntelligenceAndInquiries;

			var contactAttributesFilter = new ModuleDependentItemTextFilter("Contact Attribute", GetContactAttributeQuery, ContactAttributes);
			contactAttributesFilter.MultilingualDescription = ResString.GetMultilingualString("fe0a04f4-597b-4557-8213-2818671f02a7", FilterDescription.ContactAttributes);
			contactAttributesFilter.Category = FilterCategories.StatusAndFlags;
			filters.AddFilter(contactAttributesFilter);

			var documentGroupsFilter = new ModuleDependentItemTextFilter("Document Group", GetDocumentGroupQuery, DocumentGroups);
			documentGroupsFilter.MultilingualDescription = ResString.GetMultilingualString("6ac5a073-7829-4044-b959-eacd5e836a72", FilterDescription.DocumentGroups);
			documentGroupsFilter.Category = FilterCategories.StatusAndFlags;
			filters.AddFilter(documentGroupsFilter);

			var officialContactForDocumentGroup = filters.AddTextFilter(FilterDescription.OfficialContactForDocumentGroup, GetOfficialContactForDocumentGroup, DocumentGroups);
			officialContactForDocumentGroup.MultilingualDescription = ResString.GetMultilingualString("24800d3d-9f05-4504-bbbc-1f76fac4085a", FilterDescription.OfficialContactForDocumentGroup);
			officialContactForDocumentGroup.Category = FilterCategories.StatusAndFlags;

			var organisationTypeFilter = filters.AddTextFilter(FilterDescription.OrganizationType, GetOrganisationTypes, OrganisationFlags);
			organisationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("3035d3a8-909f-400b-b7b2-05f61a7ffe50", FilterDescription.OrganizationType);
			SetupComparisonOperators(organisationTypeFilter);

			var orgMarketingModuleFilter = AddMarketingOptionsFilter(filters);
			orgMarketingModuleFilter.MultilingualDescription = ResString.GetMultilingualString("BBA25331-7CE0-4062-98E0-86CC696170AF", "Marketing Options");
			SetupComparisonOperators(orgMarketingModuleFilter);

			var secondaryOrgTypeFilter = AddSecondaryOrgTypeFilter(filters);
			secondaryOrgTypeFilter.MultilingualDescription = ResString.GetMultilingualString("6DAAC827-E02E-495A-9301-E5F033DBA8A4", "Secondary Type");
			SetupComparisonOperators(secondaryOrgTypeFilter);

			var mainCompetitorActivityFilter = filters.AddTextFilter(FilterDescription.MainCompetitorActivity, OrgMiscServSchema.OM_CMCompetitorActivity, MainCompetitorActivity_List);
			mainCompetitorActivityFilter.MultilingualDescription = ResString.GetMultilingualString("950188d5-a870-42db-8b71-3dcd468ca9fa", FilterDescription.MainCompetitorActivity);
			mainCompetitorActivityFilter.Category = FilterCategories.StatusAndFlags;
			mainCompetitorActivityFilter.SubGroup = OrgMiscServSubGroupInstance;

			var salesTradeLaneStatusFilter = filters.AddTextFilter(FilterDescription.SalesTradeLaneStatus, OrgTradeDetailSchema.PA_Status, TradeLaneStatuses);
			salesTradeLaneStatusFilter.MultilingualDescription = ResString.GetMultilingualString("27d941b6-7370-400d-be4a-5054b4b3a18c", FilterDescription.SalesTradeLaneStatus);
			salesTradeLaneStatusFilter.Category = FilterCategories.StatusAndFlags;
			salesTradeLaneStatusFilter.SubGroup = new SalesTradeLaneStatusSubGroup();

			var salesTradeLaneProductFilter = filters.AddGuidFilter(FilterDescription.SalesTradeLaneProduct, ModuleIDs.SalesProduct, OrgSalesSchema.OW_MP_Product, SalesProductTypes);
			salesTradeLaneProductFilter.MultilingualDescription = ResString.GetMultilingualString("d9101764-b225-45f1-a321-a5b6f9dbdd5f", FilterDescription.SalesTradeLaneProduct);
			salesTradeLaneProductFilter.Category = FilterCategories.StatusAndFlags;
			salesTradeLaneProductFilter.SubGroup = new SalesTradeLaneProductSubGroup();

			var closeReasonFilter = filters.AddTextFilter(FilterDescription.CloseReason, OrgColdCallRegisterSchema.O1_CloseReason, InquiryCloseReasons);
			closeReasonFilter.MultilingualDescription = ResString.GetMultilingualString("21F82FEA-38E0-48AD-8B76-AA0EF7600E54", FilterDescription.CloseReason);
			closeReasonFilter.Category = CampaignContactFilterCategories.Inquiries;
			closeReasonFilter.SubGroup = OrgColdCallRegisterSubGroupInstance;
		}

		void SetupComparisonOperators(ModuleTextFilter filter)
		{
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.ShowDescription = false;
		}

		ModuleTextFilter AddMarketingOptionsFilter(ModuleFilterCollection filters)
		{
			return filters.AddTextFilter("Marketing Options", (comparisonOperator, value) =>
			{
				bool notIn = comparisonOperator.IsNegativeSQLOperator();
				var query = OrgFilterHelper.GetMarketingOptionsFilter(value);
				var marketingQuery = new ZDBOnlyQuery(typeof(CampaignContact));
				marketingQuery.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
				var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK, notIn);
				orgSubQuery.AddToFilter(query);
				marketingQuery.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);
				return marketingQuery;
			}, OrgFilterHelper.GetMarketingOptions());
		}

		ModuleTextFilter AddSecondaryOrgTypeFilter(ModuleFilterCollection filters)
		{
			return filters.AddTextFilter("Secondary Type", (comparisonOperator, value) =>
			{
				bool notIn = comparisonOperator.IsNegativeSQLOperator();
				var query = OrgFilterHelper.GetSecondaryOrgTypeFilter(value);
				var secondaryQuery = new ZDBOnlyQuery(typeof(CampaignContact));
				secondaryQuery.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
				var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK, notIn);
				orgSubQuery.AddToFilter(query);
				secondaryQuery.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);
				return secondaryQuery;
			}, OrgFilterHelper.GetSecondaryOrgTypes(OrgModuleType.CompanyCampaignContact));
		}

		readonly OrganisationFilterHelper OrgFilterHelper = new OrganisationFilterHelper();

		readonly OrgColdCallRegisterSubGroup OrgColdCallRegisterSubGroupInstance = new OrgColdCallRegisterSubGroup();

		class OrgColdCallRegisterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(CampaignContact));
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgColdCallRegisterSchema.Constants.Prefix);
				var leadInterestQuery = new ZDBOnlySubQuery(typeof(SalesEnquiry), OrgColdCallRegisterSchema.PK);
				leadInterestQuery.AddToFilter(filter);
				query.AddSubQuery(leadInterestQuery, JoinCondition.And);
				return query;
			}
		}

		ZQuery GetContactAttributeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

			bool notIn = comparisonOperator.IsNegativeSQLOperator();
			ZDBOnlySubQuery attributeSubQuery = new ZDBOnlySubQuery(typeof(OrgContactAttribute), OrgContactAttributeSchema.PC_OC, notIn);
			attributeSubQuery.AddToFilter(OrgContactAttributeSchema.PC_Type, value);
			query.AddSubQuery(attributeSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetDocumentGroupQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

			bool notIn = comparisonOperator.IsNegativeSQLOperator();
			ZDBOnlySubQuery docSubQuery = new ZDBOnlySubQuery(typeof(OrgDocument), OrgDocumentSchema.OD_OC, notIn);
			docSubQuery.AddToFilter(OrgDocumentSchema.OD_DocumentGroup, value);
			if (!notIn)
			{
				docSubQuery.AddToFilter(OrgDocumentSchema.OD_DeliverBy, SQLComparisonOperator.NotEqual, Core.Constants.ContactNotifyModes.DoNotDeliver);
			}

			query.AddSubQuery(ViewCampaignContactSchema.PK, docSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetOfficialContactForDocumentGroup(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			@params.Add("@DOCGROUP", value, OrgDocumentSchema.OD_DocumentGroup);

			string sQL = string.Format(@"{0} in	
(
	SELECT
	convert(uniqueidentifier, isnull(DefaultCsvContact.OC_PK, CsvContact.OC_PK)) CsvContactPk FROM
	dbo.OrgHeader
	LEFT JOIN
	(
		SELECT OC_OH, min(convert(char(36), OC_PK)) OC_PK
		FROM dbo.OrgContact
		WHERE
			OC_PK in
			(
				SELECT OD_OC
				FROM dbo.OrgDocument 
				WHERE OD_DocumentGroup = @DOCGROUP
				AND OD_DefaultContact = '1'
				AND OD_DeliverBy <> 'DND'
			)
		GROUP BY OC_OH
	) DefaultCsvContact ON DefaultCsvContact.OC_OH = OH_PK
	LEFT JOIN
	(
		SELECT OC_OH, min(convert(char(36), OC_PK)) OC_PK
		FROM dbo.OrgContact
		WHERE
			OC_PK in
			(
				SELECT OD_OC
				FROM dbo.OrgDocument 
				WHERE OD_DocumentGroup = @DOCGROUP
				AND OD_DeliverBy <> 'DND'
			)
		GROUP BY OC_OH
	) CsvContact ON CsvContact.OC_OH = OH_PK WHERE
	(
		DefaultCsvContact.OC_PK is not null
		OR
		(
			CsvContact.OC_PK is not null
			AND DefaultCsvContact.OC_PK is null
		)
	)
)
", ViewCampaignContactSchema.PK.Name);

			query.AddFilterAndZSQLParameterCollection(sQL, @params);
			return query;
		}

		ZQuery GetOrganisationTypes(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

			SchemaColumn orgColumn = OrgColumnMappingList.GetColumnFromDescription(value);
			if (orgColumn != null)
			{
				ZBool operatorValue = comparisonOperator == SQLComparisonOperator.Equal ? ZBool.True : ZBool.False;

				if (orgColumn == OrgCompanyDataSchema.OB_IsCreditor || orgColumn == OrgCompanyDataSchema.OB_IsDebtor)
				{
					ZDBOnlySubQuery debtorCreditorSubquery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					debtorCreditorSubquery.AddToFilter(orgColumn, SQLComparisonOperator.Equal, operatorValue);
					debtorCreditorSubquery.AddToFilter(JoinCondition.And, OrgCompanyDataSchema.OB_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
					query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, debtorCreditorSubquery, JoinCondition.And);
				}
				else
				{
					ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
					orgSubQuery.AddToFilter(orgColumn, SQLComparisonOperator.Equal, operatorValue);
					query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);
				}
			}

			return query;
		}

		class SalesTradeLaneStatusSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

				ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				ZDBOnlySubQuery tradeLaneImportQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Buyer);
				ZDBOnlySubQuery tradeLaneExportQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Supplier);
				ZDBOnlySubQuery tradeDetailQuery = new ZDBOnlySubQuery(typeof(OrgTradeDetail), OrgTradeDetailSchema.PA_OW);

				tradeDetailQuery.AddToFilter(filter);

				tradeLaneImportQuery.AddSubQuery(tradeDetailQuery, JoinCondition.And);
				tradeLaneExportQuery.AddSubQuery(tradeDetailQuery, JoinCondition.And);

				orgSubQuery.AddSubQuery(tradeLaneImportQuery, JoinCondition.Or);
				orgSubQuery.AddSubQuery(tradeLaneExportQuery, JoinCondition.Or);

				query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);
				return query;
			}
		}

		class SalesTradeLaneProductSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

				ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				ZDBOnlySubQuery tradeLanePrimaryQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Primary);
				tradeLanePrimaryQuery.AddToFilter(filter);

				ZDBOnlySubQuery tradeLaneImportQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Buyer);
				tradeLaneImportQuery.AddToFilter(filter);

				ZDBOnlySubQuery tradeLaneExportQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Supplier);
				tradeLaneExportQuery.AddToFilter(filter);

				orgSubQuery.AddSubQuery(tradeLanePrimaryQuery, JoinCondition.Or);
				orgSubQuery.AddSubQuery(tradeLaneImportQuery, JoinCondition.Or);
				orgSubQuery.AddSubQuery(tradeLaneExportQuery, JoinCondition.Or);

				query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion

		#region RelationshipOrgAndStaffFilters

		void AddRelationshipOrgAndStaffFilters(ModuleFilterCollection filters)
		{
			var verifiedByFilter = filters.AddNkFilter(FilterDescription.ContactDetailsVerifiedBy, StmALogSchema.SL_GS_NKUser, ModuleIDs.GlbStaff, StaffCollection);
			verifiedByFilter.MultilingualDescription = ResString.GetMultilingualString("bbadc481-9b5f-4784-9192-800a65737657", FilterDescription.ContactDetailsVerifiedBy);
			verifiedByFilter.Category = FilterCategories.RelationshipOrgAndStaff;
			verifiedByFilter.SubGroup = new VerifiedByStaffSubGroup();

			var organisationFilter = filters.AddNkFilter(FilterDescription.Organization, GetOrganisation, ModuleIDs.Organisation, OrgCollection);
			organisationFilter.MaxLength = ViewCampaignContactSchema.VCC_OrgCode.MaxLength;
			organisationFilter.MultilingualDescription = ResString.GetMultilingualString("54095094-c73e-4f05-87d9-28d54dbbc7e4", FilterDescription.Organization);
			organisationFilter.Category = CampaignContactFilterCategories.CommonTypes;

			var countryPortFilter = filters.AddNkFilter("Country / Port", GetCountryOrPort, ModuleIDs.Location, Locations);
			countryPortFilter.MultilingualDescription = ResString.GetMultilingualString("28347160-5D6A-44B9-B9BB-277AA8562CE1", FilterDescription.CountryOrPort);
			countryPortFilter.Category = CampaignContactFilterCategories.CommonTypes;

			var isPrimaryWorkplaceContactFilter = filters.AddTextFilter(FilterDescription.IsPrimaryWorkplaceContact, GetContactIsPrimaryWorkplaceQuery, ContactIsPrimaryWorkplaceList);
			isPrimaryWorkplaceContactFilter.MultilingualDescription = ResString.GetMultilingualString("0da6642c-59a3-4c1d-80b6-15689f241883", FilterDescription.IsPrimaryWorkplaceContact);
			isPrimaryWorkplaceContactFilter.Category = CampaignContactFilterCategories.CommonTypes;

			var branchFilter = filters.AddNkFilter(FilterDescription.Branch, GetBranches, ModuleIDs.GlbBranch, BranchFilters);
			branchFilter.MaxLength = GlbBranchSchema.GB_Code.MaxLength;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("dd62270b-d554-4f4b-b8a0-7733616fd6b2", FilterDescription.Branch);
			branchFilter.Category = FilterCategories.RelationshipOrgAndStaff;

			var salesMainCompetitorFilter = new OrgSalesMainCompetitorModuleFilter("Sales Main Competitor On", GetSalesMainCompetitorOnQuery);
			salesMainCompetitorFilter.MultilingualDescription = ResString.GetMultilingualString("MarketingManager|CampaignContactFilter|SalesMainCompetitor", "Sales Main Competitor On");
			salesMainCompetitorFilter.Category = FilterCategories.RelationshipOrgAndStaff;
			filters.AddCustomFilter(salesMainCompetitorFilter);

			var hasMainCompetitorFilter = new OrgHasMainCompetitorModuleFilter("Has Main Competitor On", GetHasMainCompetitorOnQuery);
			hasMainCompetitorFilter.MultilingualDescription = ResString.GetMultilingualString("MarketingManager|CampaignContactFilter|HasMainCompetitor", "Has Main Competitor On");
			hasMainCompetitorFilter.Category = FilterCategories.RelationshipOrgAndStaff;
			filters.AddCustomFilter(hasMainCompetitorFilter);

			var salesTradeLaneFilterMaxLength = new[] { RefUNLOCOSchema.RL_Code.MaxLength, RefCountrySchema.RN_Code.MaxLength, RefZoneHeaderSchema.FZ_Code.MaxLength }.Max();

			var salesTradeLaneOriginPortFilter = filters.AddNkFilter(FilterDescription.SalesTradeLaneOriginPort, GetSalesTradeLaneOriginPort, ModuleIDs.Location, Locations);
			salesTradeLaneOriginPortFilter.MaxLength = salesTradeLaneFilterMaxLength;
			salesTradeLaneOriginPortFilter.MultilingualDescription = ResString.GetMultilingualString("e0f0fb67-47fd-4c88-a63b-d6380ef6c02a", FilterDescription.SalesTradeLaneOriginPort);
			salesTradeLaneOriginPortFilter.Category = FilterCategories.RelationshipOrgAndStaff;

			var salesTradeLaneDestinationPortFilter = filters.AddNkFilter(FilterDescription.SalesTradeLaneDestinationPort, GetSalesTradeLaneDestinationPort, ModuleIDs.Location, Locations);
			salesTradeLaneDestinationPortFilter.MaxLength = salesTradeLaneFilterMaxLength;
			salesTradeLaneDestinationPortFilter.MultilingualDescription = ResString.GetMultilingualString("36501b42-b849-470d-9387-c7d9a38760ac", FilterDescription.SalesTradeLaneDestinationPort);
			salesTradeLaneDestinationPortFilter.Category = FilterCategories.RelationshipOrgAndStaff;

			var salesMainExportImportCommoditySubGroup = new SalesMainExportImportCommoditySubGroup();
			var salesMainExportCommodityFilter = filters.AddNkFilter(FilterDescription.SalesMainExportCommodity, OrgMiscServSchema.OM_RH_NKCMMainExportCmdty, ModuleIDs.RefCommodityCode, Commodities);
			salesMainExportCommodityFilter.MultilingualDescription = ResString.GetMultilingualString("a3f8d1c6-51b0-4f93-a338-80c37af8bf45", FilterDescription.SalesMainExportCommodity);
			salesMainExportCommodityFilter.Category = FilterCategories.RelationshipOrgAndStaff;
			salesMainExportCommodityFilter.SubGroup = salesMainExportImportCommoditySubGroup;

			var salesMainImportCommodityFilter = filters.AddNkFilter(FilterDescription.SalesMainImportCommodity, OrgMiscServSchema.OM_RH_NKCMMainImportCmdty, ModuleIDs.RefCommodityCode, Commodities);
			salesMainImportCommodityFilter.MultilingualDescription = ResString.GetMultilingualString("1fdca286-cb29-4d57-9a7e-1d81670e8af7", FilterDescription.SalesMainImportCommodity);
			salesMainImportCommodityFilter.Category = FilterCategories.RelationshipOrgAndStaff;
			salesMainImportCommodityFilter.SubGroup = salesMainExportImportCommoditySubGroup;

			var salesTradeLaneCommodityFilter = filters.AddNkFilter(FilterDescription.SalesTradeLaneCommodity, OrgTradeProspectSchema.PAP_RH_NKCommodityCode, ModuleIDs.RefCommodityCode, Commodities);
			salesTradeLaneCommodityFilter.MultilingualDescription = ResString.GetMultilingualString("10a77cdd-d13f-41e9-ab75-e0e4343d76fd", FilterDescription.SalesTradeLaneCommodity);
			salesTradeLaneCommodityFilter.Category = FilterCategories.RelationshipOrgAndStaff;
			salesTradeLaneCommodityFilter.SubGroup = new SalesTradeLaneCommoditySubGroup();

			var stateFilter = filters.AddGuidFilter(FilterDescription.State, ModuleIDs.RefCountryStates, GetState, GetStatesCollection);
			stateFilter.MultilingualDescription = ResString.GetMultilingualString("7fbd50e6-251c-4fcc-8b10-ffe494fdc504", FilterDescription.State);
			stateFilter.Category = FilterCategories.RelationshipOrgAndStaff;

			var referringOrgFilter = filters.AddGuidFilter(FilterDescription.ReferringOrganization, ModuleIDs.Organisation, OrgColdCallRegisterSchema.O1_OH_SourceOfLead, OrgCollection);
			referringOrgFilter.MultilingualDescription = ResString.GetMultilingualString("036F08A1-7E3D-426A-82A9-8AD69EFB5C52", FilterDescription.ReferringOrganization);
			referringOrgFilter.Category = CampaignContactFilterCategories.Inquiries;
			referringOrgFilter.SubGroup = OrgColdCallRegisterSubGroupInstance;
		}

		class VerifiedByStaffSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

				ZDBOnlySubQuery logsSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
				logsSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.RecordAudited.Code);
				logsSubQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, OrgContact.DetailsVerifiedLogReference);
				logsSubQuery.AddToFilter(filter);

				query.AddSubQuery(ViewCampaignContactSchema.PK, logsSubQuery, JoinCondition.And);
				return query;
			}
		}

		ZQuery GetOrganisation(SQLComparisonOperator comparisonOperator, ZString nk)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_OrgCode, comparisonOperator, nk);
			if (Campaign.IsUsingClientIntelligenceDataSource)
			{
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
			}
			else if (Campaign.IsUsingInquiryDataSource)
			{
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgColdCallRegisterSchema.Constants.Prefix);
			}
			return query;
		}

		ZQuery GetCountryOrPort(ZString nk)
		{
			bool isCountryCode = (nk.Length == RefCountrySchema.RN_Code.MaxLength);
			var portCodeComparisonOperator = isCountryCode ? SQLComparisonOperator.StartsWith : SQLComparisonOperator.Equal;

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));

			ZDBOnlySubQuery addressCapabilityQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
			addressCapabilityQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, OrgConstants.AddressType.Office);
			addressCapabilityQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, ZBool.True);

			ZDBOnlySubQuery nullContactAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
			nullContactAddressQuery.AddToFilter(JoinCondition.And, OrgAddressSchema.OA_RL_NKRelatedPortCode, portCodeComparisonOperator, nk);
			nullContactAddressQuery.AddSubQuery(addressCapabilityQuery, JoinCondition.And);

			ZDBOnlySubQuery closestPortQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			closestPortQuery.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, portCodeComparisonOperator, nk);

			ZDBOnlyQuery innerContactQuery = new ZDBOnlyQuery(typeof(OrgContact));
			innerContactQuery.AddSubQuery(OrgContactSchema.OC_OH, nullContactAddressQuery, JoinCondition.And);
			innerContactQuery.AddSubQuery(OrgContactSchema.OC_OH, closestPortQuery, JoinCondition.Or);

			ZQuery innerQuery = new ZQuery();
			innerQuery.AddToFilter(OrgContactSchema.OC_OA_OrgAddress, SQLComparisonOperator.Equal, null);
			innerQuery.AddToFilter(innerContactQuery, JoinCondition.And);

			ZDBOnlySubQuery addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, portCodeComparisonOperator, nk);

			if (isCountryCode)
			{
				addressSubQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_RN_NKCountryCode, nk);
			}

			ZDBOnlySubQuery relatedPortQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			relatedPortQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, "");

			ZDBOnlyQuery relatedAndClosestPortQuery = new ZDBOnlyQuery(typeof(OrgContact));
			relatedAndClosestPortQuery.AddSubQuery(OrgContactSchema.OC_OA_OrgAddress, relatedPortQuery, JoinCondition.And);
			relatedAndClosestPortQuery.AddSubQuery(OrgContactSchema.OC_OH, closestPortQuery, JoinCondition.And);

			ZDBOnlyQuery contactPortQuery = new ZDBOnlyQuery(typeof(OrgContact));
			contactPortQuery.AddSubQuery(OrgContactSchema.OC_OA_OrgAddress, addressSubQuery, JoinCondition.And);
			contactPortQuery.AddToFilter(relatedAndClosestPortQuery, JoinCondition.Or);

			ZDBOnlyQuery contactWithOrgAddressQuery = new ZDBOnlyQuery(typeof(OrgContact));
			contactWithOrgAddressQuery.AddToFilter(OrgContactSchema.OC_OA_OrgAddress, SQLComparisonOperator.NotEqual, null);
			contactWithOrgAddressQuery.AddToFilter(contactPortQuery, JoinCondition.And);

			ZDBOnlySubQuery contactQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.PK);
			contactQuery.AddToFilter(contactWithOrgAddressQuery, JoinCondition.And);
			contactQuery.AddToFilter(innerQuery, JoinCondition.Or);

			ZQuery inquiryQuery = new ZQuery();
			inquiryQuery.AddToFilter(ViewCampaignContactSchema.VCC_RelatedPortCode, portCodeComparisonOperator, nk);
			inquiryQuery.AddToFilter(JoinCondition.And, ViewCampaignContactSchema.VCC_TableCode, OrgColdCallRegisterSchema.Constants.Prefix);

			query.AddSubQuery(contactQuery, JoinCondition.And);
			query.AddToFilter(inquiryQuery, JoinCondition.Or);

			return query;
		}

		ZQuery GetBranches(SQLComparisonOperator comparisonOperator, ZString nk)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			ZDBOnlySubQuery companyDataSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			ZDBOnlySubQuery branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), OrgCompanyDataSchema.OB_GB_ControllingBranch);

			branchSubQuery.AddToFilter(GlbBranchSchema.GB_Code, comparisonOperator, nk);
			companyDataSubQuery.AddSubQuery(branchSubQuery, JoinCondition.And);
			orgSubQuery.AddSubQuery(companyDataSubQuery, JoinCondition.And);

			query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);
			return query;
		}

		protected ZQuery GetHasMainCompetitorOnQuery(ZBool hasCompetitor, ZString competitorType)
		{
			var orgCompetitorSubQuery = new ZDBOnlySubQuery(typeof(OrgCompetitor), OrgCompetitorSchema.OCP_OH_Parent, notIn: !hasCompetitor);
			orgCompetitorSubQuery.AddToFilter(OrgCompetitorSchema.OCP_Type, competitorType);

			var resultQuery = new ZDBOnlyQuery(typeof(CampaignContact));
			resultQuery.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
			resultQuery.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgCompetitorSubQuery, JoinCondition.And);

			return resultQuery;
		}

		protected ZQuery GetSalesMainCompetitorOnQuery(ZGuid competitor, ZString competitorType)
		{
			var orgCompetitorSubQuery = new ZDBOnlySubQuery(typeof(OrgCompetitor), OrgCompetitorSchema.OCP_OH_Parent);
			orgCompetitorSubQuery.AddToFilter(OrgCompetitorSchema.OCP_OH_Competitor, competitor);
			orgCompetitorSubQuery.AddToFilter(OrgCompetitorSchema.OCP_Type, competitorType);

			var resultQuery = new ZDBOnlyQuery(typeof(CampaignContact));
			resultQuery.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
			resultQuery.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgCompetitorSubQuery, JoinCondition.And);

			return resultQuery;
		}

		ZQuery GetSalesTradeLaneOriginPort(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

			var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), ViewCampaignContactSchema.VCC_OH);

			var primaryOrgSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Primary);
			var primaryOriginSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_OriginID);
			primaryOriginSubQuery.AddToFilter(ViewLocationSchema.VLO_Code, SQLComparisonOperator.StartsWith, value);
			primaryOrgSubQuery.AddSubQuery(primaryOriginSubQuery, JoinCondition.And);

			var supplierOrgSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Supplier);
			var supplierOriginSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_OriginID);
			supplierOriginSubQuery.AddToFilter(ViewLocationSchema.VLO_Code, SQLComparisonOperator.StartsWith, value);
			supplierOrgSubQuery.AddSubQuery(supplierOriginSubQuery, JoinCondition.And);

			var buyerOrgSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Buyer);
			var buyerOriginSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_OriginID);
			buyerOriginSubQuery.AddToFilter(ViewLocationSchema.VLO_Code, SQLComparisonOperator.StartsWith, value);
			buyerOrgSubQuery.AddSubQuery(buyerOriginSubQuery, JoinCondition.And);

			orgSubQuery.AddSubQuery(primaryOrgSubQuery, JoinCondition.Or);
			orgSubQuery.AddSubQuery(supplierOrgSubQuery, JoinCondition.Or);
			orgSubQuery.AddSubQuery(buyerOrgSubQuery, JoinCondition.Or);

			query.AddSubQuery(orgSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetSalesTradeLaneDestinationPort(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

			var orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), ViewCampaignContactSchema.VCC_OH);

			var primaryOrgSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Primary);
			var primaryDestinationSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_DestinationID);
			primaryDestinationSubQuery.AddToFilter(ViewLocationSchema.VLO_Code, SQLComparisonOperator.StartsWith, value);
			primaryOrgSubQuery.AddSubQuery(primaryDestinationSubQuery, JoinCondition.And);

			var supplierOrgSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Supplier);
			var supplierDestinationSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_DestinationID);
			supplierDestinationSubQuery.AddToFilter(ViewLocationSchema.VLO_Code, SQLComparisonOperator.StartsWith, value);
			supplierOrgSubQuery.AddSubQuery(supplierDestinationSubQuery, JoinCondition.And);

			var buyerOrgSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Buyer);
			var buyerDestinationSubQuery = new ZDBOnlySubQuery(typeof(ViewLocation), OrgSalesSchema.OW_DestinationID);
			buyerDestinationSubQuery.AddToFilter(ViewLocationSchema.VLO_Code, SQLComparisonOperator.StartsWith, value);
			buyerOrgSubQuery.AddSubQuery(buyerDestinationSubQuery, JoinCondition.And);

			orgSubQuery.AddSubQuery(primaryOrgSubQuery, JoinCondition.Or);
			orgSubQuery.AddSubQuery(supplierOrgSubQuery, JoinCondition.Or);
			orgSubQuery.AddSubQuery(buyerOrgSubQuery, JoinCondition.Or);

			query.AddSubQuery(orgSubQuery, JoinCondition.And);
			return query;
		}

		class SalesMainExportImportCommoditySubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

				ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				orgSubQuery.AddToFilter(OrgHeaderSchema.OH_IsSalesLead, ZBool.True);

				ZDBOnlySubQuery miscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
				miscServSubQuery.AddToFilter(filter);

				orgSubQuery.AddSubQuery(miscServSubQuery, JoinCondition.And);

				query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);
				return query;
			}
		}

		class SalesTradeLaneCommoditySubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

				ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				orgSubQuery.AddToFilter(OrgHeaderSchema.OH_IsSalesLead, ZBool.True);

				ZDBOnlySubQuery tradeDetailSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeDetail), OrgTradeDetailSchema.PA_OW);
				ZDBOnlySubQuery tradeProspectSubQuery = new ZDBOnlySubQuery(typeof(OrgTradeProspect), OrgTradeProspectSchema.PAP_PA);
				tradeProspectSubQuery.AddToFilter(filter);
				tradeDetailSubQuery.AddSubQuery(tradeProspectSubQuery, JoinCondition.And);

				ZDBOnlySubQuery salesBuyerSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Buyer);
				ZDBOnlySubQuery salesSupplierSubQuery = new ZDBOnlySubQuery(typeof(OrgSales), OrgSalesSchema.OW_OH_Supplier);

				salesBuyerSubQuery.AddSubQuery(tradeDetailSubQuery, JoinCondition.And);
				salesSupplierSubQuery.AddSubQuery(tradeDetailSubQuery, JoinCondition.And);

				ZDBOnlyQuery combinedSubquery = new ZDBOnlyQuery(typeof(OrgHeader));
				combinedSubquery.AddSubQuery(salesBuyerSubQuery, JoinCondition.Or);
				combinedSubquery.AddSubQuery(salesSupplierSubQuery, JoinCondition.Or);

				orgSubQuery.AddToFilter(combinedSubquery);

				query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.And);
				return query;
			}
		}

		protected class CampaignsSubGroup : ModuleFilterSubGroup
		{
			public CampaignsSubGroup(bool isExcludingSentCampaign)
			{
				IsExcludingSentCampaign = isExcludingSentCampaign;
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));

				ZDBOnlySubQuery campaignItemSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID, IsExcludingSentCampaign);
				campaignItemSubQuery.AddToFilter(filter);
				query.AddSubQuery(ViewCampaignContactSchema.PK, campaignItemSubQuery, JoinCondition.And);

				return query;
			}

			readonly bool IsExcludingSentCampaign;
		}

		ZQuery GetState(SQLComparisonOperator comparisonOperator, object value)
		{
			var query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				query.AddToFilter(ViewCampaignContactSchema.VCC_State, SQLComparisonOperator.Equal, ZString.Empty);
			}
			else if (comparisonOperator == SQLComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(ViewCampaignContactSchema.VCC_State, SQLComparisonOperator.NotEqual, ZString.Empty);
			}
			else if (comparisonOperator == SQLComparisonOperator.Equal || comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				var isNotIn = comparisonOperator == SQLComparisonOperator.NotEqual;
				var stateDescriptionQuery = new ZDBOnlySubQuery(typeof(RefCountryStates), RefCountryStatesSchema.RW_Description, isNotIn);
				stateDescriptionQuery.AddToFilter(RefCountryStatesSchema.PK, value);

				var stateCodeQuery = new ZDBOnlySubQuery(typeof(RefCountryStates), RefCountryStatesSchema.RW_Code, isNotIn);
				stateCodeQuery.AddToFilter(RefCountryStatesSchema.PK, value);

				var stateForCountryQuery = new ZDBOnlySubQuery(typeof(RefCountryStates), RefCountryStatesSchema.RW_RN_NKCountryCode, RefCountrySchema.RN_Code);
				stateForCountryQuery.AddToFilter(RefCountryStatesSchema.PK, value);
				var countryQuery = new ZDBOnlySubQuery(typeof(RefCountry), RefCountrySchema.RN_Code);
				countryQuery.AddSubQuery(RefCountrySchema.RN_Code, stateForCountryQuery, JoinCondition.And);
				var unlocoQuery = new ZDBOnlySubQuery(typeof(RefUNLOCO), RefUNLOCOSchema.RL_Code, isNotIn);
				unlocoQuery.AddSubQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, countryQuery, JoinCondition.And);

				var stateQuery = new ZDBOnlyQuery(typeof(CampaignContact));

				if (comparisonOperator == SQLComparisonOperator.Equal)
				{
					stateQuery.AddSubQuery(ViewCampaignContactSchema.VCC_State, stateDescriptionQuery, JoinCondition.And);
					stateQuery.AddSubQuery(ViewCampaignContactSchema.VCC_State, stateCodeQuery, JoinCondition.Or);
					stateQuery.AddSubQuery(ViewCampaignContactSchema.VCC_RelatedPortCode, unlocoQuery, JoinCondition.And);
				}
				else if (comparisonOperator == SQLComparisonOperator.NotEqual)
				{
					stateQuery.AddSubQuery(ViewCampaignContactSchema.VCC_State, stateDescriptionQuery, JoinCondition.And);
					stateQuery.AddSubQuery(ViewCampaignContactSchema.VCC_State, stateCodeQuery, JoinCondition.And);
					stateQuery.AddSubQuery(ViewCampaignContactSchema.VCC_RelatedPortCode, unlocoQuery, JoinCondition.Or);
				}

				query.AddToFilter(stateQuery);
			}

			return query;
		}

		#endregion

		#region Text Filters

		void AddTextFilters(ModuleFilterCollection filters)
		{
			var contactNameFilter = filters.AddTextFilter(FilterDescription.ContactName, GetContactName);
			contactNameFilter.MultilingualDescription = ResString.GetMultilingualString("a974ceeb-f0d0-4b91-977e-6d617c9c02d7", FilterDescription.ContactName);
			contactNameFilter.Category = CampaignContactFilterCategories.CommonTypes;

			var emailAddressFilter = filters.AddTextFilter(FilterDescription.EmailAddress, GetEmailAddress);
			emailAddressFilter.MaxLength = ViewCampaignContactSchema.VCC_Email.MaxLength;
			emailAddressFilter.MultilingualDescription = ResString.GetMultilingualString("5b3ee371-6bd9-42fd-81c8-f2515f5675d2", FilterDescription.EmailAddress);
			emailAddressFilter.Category = CampaignContactFilterCategories.CommonTypes;

			var cityFilter = filters.AddTextFilter(FilterDescription.City, GetCity);
			cityFilter.MaxLength = OrgAddressSchema.OA_City.MaxLength;
			cityFilter.MultilingualDescription = ResString.GetMultilingualString("bdfe0ceb-75da-4148-a046-d5cef3fe8963", FilterDescription.City);
			cityFilter.Category = CampaignContactFilterCategories.ClientIntelligenceAndInquiries;

			var notReceiveCampaignFilter = filters.AddGuidFilter(FilterDescription.HasNotReceivedCampaign, ModuleIDs.GlbCompanyCampaign, GlbCompanyCampaignItemSchema.G8_G0, Campaigns);
			notReceiveCampaignFilter.MultilingualDescription = ResString.GetMultilingualString("11bc4d82-91cb-4278-a58e-68dedf651ac0", FilterDescription.HasNotReceivedCampaign);
			notReceiveCampaignFilter.Category = CampaignContactFilterCategories.ClientIntelligenceAndInquiries;
			notReceiveCampaignFilter.SubGroup = new CampaignsSubGroup(isExcludingSentCampaign: true);

			var hasReceivedCampaignFilter = filters.AddGuidFilter(FilterDescription.HasReceivedCampaign, ModuleIDs.GlbCompanyCampaign, GlbCompanyCampaignItemSchema.G8_G0, Campaigns);
			hasReceivedCampaignFilter.MultilingualDescription = ResString.GetMultilingualString("75217044-ef70-4ba2-b434-c3ab1d798f59", FilterDescription.HasReceivedCampaign);
			hasReceivedCampaignFilter.Category = CampaignContactFilterCategories.ClientIntelligenceAndInquiries;
			hasReceivedCampaignFilter.SubGroup = new CampaignsSubGroup(isExcludingSentCampaign: false);

			var leadInterestFilter = filters.AddTextFilter(FilterDescription.LeadInterest, OrgColdCallRegisterSchema.O1_InterestLevel, InquiryLeadInterestList);
			leadInterestFilter.MultilingualDescription = ResString.GetMultilingualString("A2F25272-5320-46C0-9EAE-5DC58B7D5A14", FilterDescription.LeadInterest);
			leadInterestFilter.Category = CampaignContactFilterCategories.Inquiries;
			leadInterestFilter.SubGroup = OrgColdCallRegisterSubGroupInstance;

			var organizationNameFilter = filters.AddTextFilter(FilterDescription.OrganizationName, GetOrganizationName);
			organizationNameFilter.MaxLength = ViewCampaignContactSchema.VCC_OrgFullName.MaxLength;
			organizationNameFilter.MultilingualDescription = ResString.GetMultilingualString("9D8DFCCB-B571-4534-9058-D2BE9BB9D8BA", FilterDescription.OrganizationName);
			organizationNameFilter.Category = CampaignContactFilterCategories.CommonTypes;
		}

		ZQuery GetCity(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));

			if (Campaign != null && Campaign.IsUsingClientIntelligenceDataSource)
			{
				ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.OA_OH);
				ZDBOnlySubQuery orgAddressCapSubQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
				orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_City, comparisonOperator, value);
				orgAddressCapSubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, OrgConstants.AddressType.Office);
				orgAddressCapSubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, ZBool.True);
				orgAddressSubQuery.AddSubQuery(orgAddressCapSubQuery, JoinCondition.And);
				orgSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
				query.AddSubQuery(ViewCampaignContactSchema.VCC_OH, orgSubQuery, JoinCondition.Or);

				ZDBOnlySubQuery contactAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
				ZDBOnlySubQuery contactAddressCapSubQuery = new ZDBOnlySubQuery(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA);
				contactAddressSubQuery.AddToFilter(OrgAddressSchema.OA_City, comparisonOperator, value);
				contactAddressCapSubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_AddressType, OrgAddressType.Office.Code);
				contactAddressCapSubQuery.AddToFilter(OrgAddressCapabilitySchema.PZ_IsMainAddress, ZBool.True);
				contactAddressSubQuery.AddSubQuery(contactAddressCapSubQuery, JoinCondition.And);
				query.AddSubQuery(ViewCampaignContactSchema.VCC_OA, contactAddressSubQuery, JoinCondition.Or);
			}

			if (Campaign != null && Campaign.IsUsingInquiryDataSource)
			{
				query.AddToFilter(JoinCondition.Or, ViewCampaignContactSchema.VCC_City, comparisonOperator, value);
			}

			return query;
		}

		ZQuery GetContactName(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_ContactName, comparisonOperator, value);
			return query;
		}

		ZQuery GetEmailAddress(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_Email, comparisonOperator, value);
			return query;
		}

		ZQuery GetOrganizationName(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_OrgFullName, comparisonOperator, value);
			return query;
		}

		#endregion

		#region Campaign Tracker Filters

		ModuleTextFilter AddCreatedOnWebFilter(ModuleFilterCollection filters, SchemaStringColumn creatingUserColumn)
		{
			ModuleTextFilter filter = new ModuleTextFilter(FilterDescription.CreatedOnWeb, (createdOn) =>
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));

				ZDBOnlySubQuery campaignItemSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID);

				campaignItemSubQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, SourceCampaignPKs);

				if (createdOn == CreatedOnCodes.WebTracker)
				{
					campaignItemSubQuery.AddToFilter(creatingUserColumn, User.WebUserCode);
				}
				else if (createdOn == CreatedOnCodes.Enterprise)
				{
					campaignItemSubQuery.AddToFilter(creatingUserColumn, SQLComparisonOperator.NotEqual, User.WebUserCode);
				}
				query.AddSubQuery(ViewCampaignContactSchema.PK, campaignItemSubQuery, JoinCondition.And);
				return query;
			},
				CreatedOnWebList);

			filter.Category = CampaignContactFilterCategories.CampaignTracking;
			filter.DefaultProperty = CreatedOnCodes.All;
			filter.MultilingualDescription = ResString.GetMultilingualString("bbd23e3c-ca3f-4553-82bf-487e78fa385f", FilterDescription.CreatedOnWeb);
			filters.AddFilter(filter);
			return filter;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		static CodeDescriptionPairList CreatedOnWebList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(CreatedOnCodes.All, Res.GetString("fadedfe3-203d-401e-b26e-2502dd9917e2", "All Records"));
				result.AddPair(CreatedOnCodes.WebTracker, Res.GetString("48b76615-b85b-4f18-92e1-2a20ee6b8355", "Created using WebTracker"));
				result.AddPair(CreatedOnCodes.Enterprise, Res.GetString("4e305da2-a753-44d1-944a-94e8bbc15813", "Created using {0}", "CargoWise"));

				return result;
			}
		}

		static class CreatedOnCodes
		{
			public const string All = "ALL";
			public const string WebTracker = "WEB";
			public const string Enterprise = "ENT";
		}

		public void AddCampaignTrackerFilters(ModuleFilterCollection filters)
		{
			if (Campaign != null && !Campaign.IsTargetList)
			{
				CampaignContactContextLinkActivityModuleFilter linkActivityContextFilter = new CampaignContactContextLinkActivityModuleFilter(FilterDescription.HasContextActivity, this, Campaign);
				linkActivityContextFilter.Category = GlbCompanyCampaignItemFilterBusinessObject.LinkActivityCategories.LinkActivity;
				linkActivityContextFilter.MultilingualDescription = ResString.GetMultilingualString("c9f54b31-b576-4c55-80ae-75837577a93a", FilterDescription.HasContextActivity);
				filters.AddCustomFilter(linkActivityContextFilter);

				CampaignContactDestinationURLLinkActivityModuleFilter linkActivityURLFilter = new CampaignContactDestinationURLLinkActivityModuleFilter(FilterDescription.HasDestinationURLActivity, this, Campaign);
				linkActivityURLFilter.Category = GlbCompanyCampaignItemFilterBusinessObject.LinkActivityCategories.LinkActivity;
				linkActivityURLFilter.MultilingualDescription = ResString.GetMultilingualString("8d4c60e2-61d1-4759-b130-af0ec4c723c3", FilterDescription.HasDestinationURLActivity);
				filters.AddCustomFilter(linkActivityURLFilter);

				UniqueDaysActivityCountFilter distinctDaysCountFilter = new UniqueDaysActivityCountFilter(FilterDescription.UniqueDayActivityCount, GetDistinctDaysCountQuery);
				distinctDaysCountFilter.MultilingualDescription = ResString.GetMultilingualString("cfbb4fc1-9d03-495d-bcf0-50dd0778a30d", FilterDescription.UniqueDayActivityCount);
				distinctDaysCountFilter.DefaultComparisonOperator = Enterprise.MarketingManager.GUI.CampaignContactNumberFilter.ComparisonConstants.GreaterThanOrEqualTo;
				distinctDaysCountFilter.Category = GlbCompanyCampaignItemFilterBusinessObject.LinkActivityCategories.LinkActivity;
				filters.AddCustomFilter(distinctDaysCountFilter);
			}

			AddCreatedOnWebFilter(filters, GlbCompanyCampaignItemSchema.G8_SystemCreateUser);

			var sourceCampaignFilter = filters.AddGuidFilter(FilterDescription.SourceCampaign, ModuleIDs.GlbCompanyCampaign, GetSourceCampaignPK, Campaigns);
			sourceCampaignFilter.MultilingualDescription = ResString.GetMultilingualString("08719EDD-C804-41CF-A363-CF2C71A0FB2A", FilterDescription.SourceCampaign);
			sourceCampaignFilter.Category = CampaignContactFilterCategories.CampaignTracking;
			sourceCampaignFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			sourceCampaignFilter.FilterPriority = FilterPriority.First;

			var sentByPersonFilter = filters.AddNkFilter("Sent By Person", GetCreatingUser, ModuleIDs.GlbStaff, StaffCollection);
			sentByPersonFilter.MultilingualDescription = ResString.GetMultilingualString("bd200e94-2a73-46fe-892c-410377a9477b", FilterDescription.CreatingUser);
			sentByPersonFilter.Category = CampaignContactFilterCategories.CampaignTracking;

			var lastEditUserFilter = filters.AddNkFilter(FilterDescription.LastEditUser, GetLastEditUserQuery, ModuleIDs.GlbStaff, StaffCollection);
			lastEditUserFilter.MultilingualDescription = ResString.GetMultilingualString("a1eaa3af-fd23-4cd6-9457-e9700803ba3a", FilterDescription.LastEditUser);
			lastEditUserFilter.Category = CampaignContactFilterCategories.CampaignTracking;

			var trackingStatusFilter = filters.AddTextFilter("Tracking Status", GetTrackingStatus, TrackingStatusList);
			trackingStatusFilter.MaxLength = GlbCompanyCampaignItemSchema.G8_TrackingStatus.MaxLength;
			trackingStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ba1ef480-4489-45c4-8763-c992de4f7fbf", FilterDescription.DeliveryStatus);
			trackingStatusFilter.Category = CampaignContactFilterCategories.CampaignTracking;

			var createdTimeFilter = filters.AddDateFilter(FilterDescription.CreatedTime, GetCreateTimeQuery, true, false);
			createdTimeFilter.MultilingualDescription = ResString.GetMultilingualString("96D461CA-0FB9-47B4-B65C-6EF2C5787425", FilterDescription.CreatedTime);
			createdTimeFilter.Category = CampaignContactFilterCategories.CampaignTracking;

			var lastEditTimeFilter = filters.AddDateFilter(FilterDescription.LastEditTime, GetLastEditTimeQuery, true, false);
			lastEditTimeFilter.MultilingualDescription = ResString.GetMultilingualString("5327614e-0829-471c-ab57-f3e92e4d5655", FilterDescription.LastEditTime);
			lastEditTimeFilter.Category = CampaignContactFilterCategories.CampaignTracking;

			var lastSentTimeFilter = filters.AddDateFilter(FilterDescription.LastSentTime, GetLastSentTimeQuery, true, false);
			lastSentTimeFilter.MultilingualDescription = ResString.GetMultilingualString("124f8284-d07f-49e8-b1f5-16496d6e7c41", FilterDescription.LastSentTime);
			lastSentTimeFilter.Category = CampaignContactFilterCategories.CampaignTracking;
		}

		ZQuery GetDistinctDaysCountQuery(SQLComparisonOperator comparisonOperator, ZInt value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			ZDBOnlySubQuery campaignItemSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID);

			string sQL = GetDistinctDaysCountQueryHelper.GetDistinctDaysActivitySQL(comparisonOperator, value, SourceCampaignPKs);

			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			campaignItemSubQuery.AddFilterAndZSQLParameterCollection(sQL, @params);

			query.AddSubQuery(ViewCampaignContactSchema.PK, campaignItemSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetSourceCampaignPK(ZGuid value)
		{
			if (Campaign != null && Campaign.IsUsingCampaignTrackingDataSource)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));

				ZDBOnlySubQuery campaignItemSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID);
				campaignItemSubQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, SourceCampaignPKs);

				query.AddSubQuery(ViewCampaignContactSchema.PK, campaignItemSubQuery, JoinCondition.And);
				return query;
			}
			else
			{
				return new ZQuery();
			}
		}

		ZQuery GetCreatingUser(SQLComparisonOperator comparisonOperator, ZString nk)
		{
			return GetUserQuery(nk, GlbCompanyCampaignItemSchema.G8_SystemCreateUser);
		}

		ZQuery GetLastEditUserQuery(SQLComparisonOperator comparisonOperator, ZString nk)
		{
			return GetUserQuery(nk, GlbCompanyCampaignItemSchema.G8_SystemLastEditUser);
		}

		ZQuery GetUserQuery(ZString nk, SchemaStringColumn column)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));

			ZDBOnlySubQuery campaignItemSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID);
			campaignItemSubQuery.AddToFilter(column, nk);
			campaignItemSubQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, SourceCampaignPKs);

			query.AddSubQuery(ViewCampaignContactSchema.PK, campaignItemSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetTrackingStatus(SQLComparisonOperator comparisonOperator, ZString nk)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));

			ZDBOnlySubQuery campaignItemSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID);
			campaignItemSubQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_TrackingStatus, comparisonOperator, nk);
			campaignItemSubQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, SourceCampaignPKs);

			query.AddSubQuery(ViewCampaignContactSchema.PK, campaignItemSubQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetContactIsPrimaryWorkplaceQuery(ZString contactSelectValue)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));

			contactSelectValue = contactSelectValue.Trim().ToUpper();
			if (contactSelectValue == OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.PrimaryWorkplaceContactsOnly)
			{
				var relationshipQuery = new ZDBOnlySubQuery(typeof(GlbPersonPrimaryRelationship), GlbPersonPrimaryRelationshipSchema.PPR_PrimaryId);

				query.AddSubQuery(ViewCampaignContactSchema.PK, relationshipQuery, JoinCondition.And);
			}
			else if (contactSelectValue == OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.NonPrimaryWorkplaceContacts)
			{
				var nonRelationshipQuery = new ZDBOnlySubQuery(typeof(GlbPersonPrimaryRelationship), GlbPersonPrimaryRelationshipSchema.PPR_PrimaryId, true);

				query.AddSubQuery(ViewCampaignContactSchema.PK, nonRelationshipQuery, JoinCondition.And);
			}

			return query;
		}

		ZQuery GetCreateTimeQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			return GetTimeQuery(value1, value2, GlbCompanyCampaignItemSchema.G8_SystemCreateTimeUtc);
		}

		ZQuery GetLastEditTimeQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			return GetTimeQuery(value1, value2, GlbCompanyCampaignItemSchema.G8_SystemLastEditTimeUtc);
		}

		ZQuery GetLastSentTimeQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			return GetTimeQuery(value1, value2, GlbCompanyCampaignItemSchema.G8_LastSentTimeUtc);
		}

		ZQuery GetTimeQuery(ZDateTime value1, ZDateTime value2, SchemaDateTimeColumn column)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));

			ZDBOnlySubQuery campaignItemSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID);
			campaignItemSubQuery.AddToFilter(GlbCompanyCampaignItemSchema.G8_G0, SourceCampaignPKs);

			if (value1.IsValid)
			{
				campaignItemSubQuery.AddToFilter(column, SQLComparisonOperator.GreaterThanOrEqualTo, value1);
			}

			if (value2.IsValid)
			{
				campaignItemSubQuery.AddToFilter(column, SQLComparisonOperator.LessThan, value2);
			}

			query.AddSubQuery(ViewCampaignContactSchema.PK, campaignItemSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Date Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var notReceiveCampaignsByDateFilter = filters.AddDateFilter(FilterDescription.HasNotReceivedCampaignsByDate, GetNotReceiveCampaignsByDate);
			notReceiveCampaignsByDateFilter.MultilingualDescription = ResString.GetMultilingualString("7d569fa9-09f7-458e-b1bb-23a80c6aa47b", FilterDescription.HasNotReceivedCampaignsByDate);

			var hasReceivedCampaignsByDateFilter = filters.AddDateFilter(FilterDescription.HasReceivedCampaignsByDate, GetHasReceivedCampaignsByDate);
			hasReceivedCampaignsByDateFilter.MultilingualDescription = ResString.GetMultilingualString("74cc0708-0819-4253-a8e8-fdee7e3c169d", FilterDescription.HasReceivedCampaignsByDate);
		}

		protected virtual ZQuery GetCampaignsByDate(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate, bool isExcludingSentCampaign)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);

			ZDBOnlySubQuery campaignItemSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignItem), GlbCompanyCampaignItemSchema.G8_RecipientID, isExcludingSentCampaign);
			AddDateRange(campaignItemSubQuery, comparisonOperator, JoinCondition.And, GlbCompanyCampaignItemSchema.G8_SystemCreateTimeUtc, fromDate.Date, toDate.Date);

			query.AddSubQuery(ViewCampaignContactSchema.PK, campaignItemSubQuery, JoinCondition.And);
			return query;
		}

		protected ZQuery GetNotReceiveCampaignsByDate(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			return GetCampaignsByDate(comparisonOperator, value1, value2, true);
		}

		protected ZQuery GetHasReceivedCampaignsByDate(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			return GetCampaignsByDate(comparisonOperator, value1, value2, false);
		}

		#endregion

		#region Staff Assignment Person and Role

		void AddStaffAssignmentPersonAndRoleModuleFilters(ModuleFilterCollection filters)
		{
			StaffAssignmentPersonAndRoleModuleFilter staffAssignmentPersonAndRoleFilter = new StaffAssignmentPersonAndRoleModuleFilter(FilterDescription.StaffAssignmentPersonAndRole, this);
			staffAssignmentPersonAndRoleFilter.Category = FilterCategories.RelationshipOrgAndStaff;
			staffAssignmentPersonAndRoleFilter.MultilingualDescription = ResString.GetMultilingualString("4712e0c3-3678-4e69-aaf4-d16be07a9d31", FilterDescription.StaffAssignmentPersonAndRole);
			filters.AddCustomFilter(staffAssignmentPersonAndRoleFilter);
		}

		#endregion

		#region Active Organisation and Active Contact Filter - Hidden

		void AddActiveOrgContactQuery(ModuleFilterCollection filters)
		{
			if (Campaign != null)
			{
				var activeQueryFilters = filters.AddTextFilter("ActiveQuery", GetActiveQuery);
				activeQueryFilters.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			}
		}

		ZQuery GetActiveQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery activeOrgContactQuery = Campaign.GetContactFilter;
			Campaign.AddOrgRestrictionFilterIfApplicable(activeOrgContactQuery);
			return activeOrgContactQuery;
		}

		#endregion

		#region Subscribed Filter - Always applied

		protected static string SubscribedCode => "SUB";
		protected static string UnsubscribedCode => "UNS";
		protected internal static string SubscriptionFilterCode => "SubscriptionStatus";

		protected void AddSubscribedRecipientsQuery(ModuleFilterCollection filters)
		{
			if (Campaign == null || filters[SubscriptionFilterCode] != null)
			{
				return;
			}

			var valuesList = GetSubscriptionStatusList();
			var filter = filters.AddTextFilter(SubscriptionFilterCode, GetSubscribedRecipientsQuery, valuesList);
			filter.MultilingualDescription = ResString.GetMultilingualString("563fb120-0093-4f7a-93d4-892e3c68a3c5", FilterDescription.ContactSubscriptionStatus);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			filter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			filter.Property = valuesList[UnsubscribedCode, StringComparison.CurrentCultureIgnoreCase].Code;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Category = CampaignContactFilterCategories.CommonTypes;
			filter.Visibility = FilterVisibility.AlwaysVisible;
			filter.FilterPriority = FilterPriority.Last;
		}

		protected static CodeDescriptionPairList GetSubscriptionStatusList()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(SubscribedCode, Res.GetString("28fa18ca-3123-4d00-bad9-f1ac5e343aef", "Subscribed contacts"));
			result.AddPair(UnsubscribedCode, Res.GetString("c1f55ef4-26ce-4e6d-8b95-edb4c12e159d", "Unsubscribed contacts"));

			return result;
		}

		ZQuery GetSubscribedRecipientsQuery(SQLComparisonOperator comparisonOperator, ZString status)
		{
			status = status.Trim().ToUpper();

			if (status != UnsubscribedCode && status != SubscribedCode)
			{
				return ZQuery.NoResultQuery;
			}

			var subscribedStatus = status != UnsubscribedCode.ToUpperInvariant();
			var notInFlag = comparisonOperator != SQLComparisonOperator.Equal;

			var viewQuery = new ZDBOnlyQuery(typeof(CampaignContact));
			var unsubscribeTypesQuery = Campaign.GetUnsubscribeFilter();

			AddSubscribedContactsAndOrganisations(viewQuery, subscribedStatus, notInFlag, unsubscribeTypesQuery);

			return viewQuery;
		}

		protected virtual void AddSubscribedContactsAndOrganisations(ZDBOnlyQuery viewQuery, bool subscribedStatus, bool notInFlag, ZQuery unsubscribeTypesQuery)
		{
			AddSubscribedContacts(viewQuery, subscribedStatus, notInFlag, unsubscribeTypesQuery);
			AddSubscribedOrganisations(viewQuery, subscribedStatus, notInFlag, unsubscribeTypesQuery);
		}

		protected static void AddSubscribedContacts(ZDBOnlyQuery viewQuery, bool subscribedStatus, bool notInFlag, ZQuery unsubscribeTypesQuery)
		{
			var contactsSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignSubscription), GlbCompanyCampaignSubscriptionSchema.GCS_Email, notInFlag);
			contactsSubQuery.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_IsSubscribed, subscribedStatus);
			contactsSubQuery.AddToFilter(unsubscribeTypesQuery, JoinCondition.And);

			viewQuery.AddToFilter(ViewCampaignContactSchema.VCC_Email, SQLComparisonOperator.NotEqual, "");
			viewQuery.AddSubQuery(ViewCampaignContactSchema.VCC_Email, contactsSubQuery, JoinCondition.And);
		}

		static void AddSubscribedOrganisations(ZDBOnlyQuery viewQuery, bool subscribedStatus, bool notInFlag, ZQuery unsubscribeTypesQuery)
		{
			var subscribedOrgsSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignSubscription), GlbCompanyCampaignSubscriptionSchema.GCS_OH);
			subscribedOrgsSubQuery.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_IsSubscribed, subscribedStatus);
			subscribedOrgsSubQuery.AddToFilter(unsubscribeTypesQuery, JoinCondition.And);

			var allContactsFromSubscribedOrgsSubQueryContact = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.PK, notInFlag);
			allContactsFromSubscribedOrgsSubQueryContact.AddSubQuery(OrgContactSchema.OC_OH, subscribedOrgsSubQuery, JoinCondition.And);

			var allContactsFromSubscribedOrgsSubQueryInquiry = new ZDBOnlySubQuery(typeof(OrgColdCallRegister), OrgColdCallRegisterSchema.O1_Email, notInFlag);
			allContactsFromSubscribedOrgsSubQueryInquiry.AddSubQuery(OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead, subscribedOrgsSubQuery, JoinCondition.And);

			ExcludeUnsubscribedContacts(allContactsFromSubscribedOrgsSubQueryContact, subscribedStatus, unsubscribeTypesQuery, OrgContactSchema.OC_Email);
			ExcludeUnsubscribedContacts(allContactsFromSubscribedOrgsSubQueryInquiry, subscribedStatus, unsubscribeTypesQuery, OrgColdCallRegisterSchema.O1_Email);

			var allContactsFromSubscribedOrgsQueryContact = new ZDBOnlyQuery(typeof(CampaignContact));
			allContactsFromSubscribedOrgsQueryContact.AddToFilter(ViewCampaignContactSchema.VCC_Email, SQLComparisonOperator.NotEqual, "");
			allContactsFromSubscribedOrgsQueryContact.AddSubQuery(ViewCampaignContactSchema.PK, allContactsFromSubscribedOrgsSubQueryContact, JoinCondition.And);

			var allContactsFromSubscribedOrgsQueryInquiry = new ZDBOnlyQuery(typeof(CampaignContact));
			allContactsFromSubscribedOrgsQueryInquiry.AddToFilter(ViewCampaignContactSchema.VCC_Email, SQLComparisonOperator.NotEqual, "");
			allContactsFromSubscribedOrgsQueryInquiry.AddSubQuery(ViewCampaignContactSchema.VCC_Email, allContactsFromSubscribedOrgsSubQueryInquiry, JoinCondition.And);

			var finalQuery = new ZQuery();
			finalQuery.AddToFilter(allContactsFromSubscribedOrgsQueryContact);
			finalQuery.AddToFilter(allContactsFromSubscribedOrgsQueryInquiry, notInFlag ? JoinCondition.And : JoinCondition.Or);

			viewQuery.AddToFilter(finalQuery, notInFlag ? JoinCondition.And : JoinCondition.Or);
		}

		static void ExcludeUnsubscribedContacts(ZDBOnlySubQuery allContactsFromSubscribedOrgsSubQuery, bool subscribedStatus, ZQuery unsubscribeTypesQuery, SchemaStringColumn emailColumn)
		{
			var unsubscribedContactsSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignSubscription), GlbCompanyCampaignSubscriptionSchema.GCS_Email, true);
			unsubscribedContactsSubQuery.AddToFilter(GlbCompanyCampaignSubscriptionSchema.GCS_IsSubscribed, !subscribedStatus);
			unsubscribedContactsSubQuery.AddToFilter(unsubscribeTypesQuery, JoinCondition.And);

			allContactsFromSubscribedOrgsSubQuery.AddSubQuery(emailColumn, unsubscribedContactsSubQuery, JoinCondition.And);
		}

		#endregion

		#region Inquiry Filters

		void AddInquiryFilters(ModuleFilterCollection filters)
		{
			var inquiryPKFilter = filters.AddGuidFilter("Inquiry PK", ModuleIDs.SalesEnquiry, GetInquiryPKQuery, new SalesEnquiryCollection(Factory));
			inquiryPKFilter.MultilingualDescription = ResString.GetMultilingualString("810e1d1b-c873-4f58-83c8-e8640678ec8c", FilterDescription.InquiryID);
			inquiryPKFilter.Category = CampaignContactFilterCategories.Inquiries;

			var inquiryTypeFilter = filters.AddTextFilter("Inquiry Type", ViewCampaignContactSchema.VCC_EnquiryType, InquiryTypes);
			inquiryTypeFilter.MultilingualDescription = ResString.GetMultilingualString("4e8c7a72-e854-4261-a638-39d35aad33ff", FilterDescription.Type);
			inquiryTypeFilter.Category = CampaignContactFilterCategories.Inquiries;

			var inquiryLeadSourceFilter = filters.AddTextFilter("Inquiry Source Type", ViewCampaignContactSchema.VCC_LeadSource, InquiryLeadSources);
			inquiryLeadSourceFilter.MultilingualDescription = ResString.GetMultilingualString("80f6a384-cf47-4767-a92c-415c5fac0068", FilterDescription.SourceType);
			inquiryLeadSourceFilter.Category = CampaignContactFilterCategories.Inquiries;

			var inquirySourceDetailsFilter = filters.AddTextFilter("Inquiry Source Details", ViewCampaignContactSchema.VCC_OpportunitySourceDetails);
			inquirySourceDetailsFilter.MultilingualDescription = ResString.GetMultilingualString("dc460549-5868-4b13-a060-c10cea9d127b", FilterDescription.SourceDetails);
			inquirySourceDetailsFilter.Category = CampaignContactFilterCategories.Inquiries;

			var inquiryOriginalCallDateFilter = filters.AddDateFilter("Inquiry Original Call Date", ViewCampaignContactSchema.VCC_LeadCalledDate, true);
			inquiryOriginalCallDateFilter.MultilingualDescription = ResString.GetMultilingualString("211e6293-b407-420d-a5cd-9a8f034daae8", FilterDescription.OriginalCallDate);
			inquiryOriginalCallDateFilter.Category = CampaignContactFilterCategories.Inquiries;

			var inquiryLeadStatusFilter = filters.AddTextFilter("Inquiry Status", ViewCampaignContactSchema.VCC_LeadStatus, InquiryStatusLists);
			inquiryLeadStatusFilter.MultilingualDescription = ResString.GetMultilingualString("ae153211-36ab-44eb-ad97-61c48aef6641", FilterDescription.Status);
			inquiryLeadStatusFilter.Category = CampaignContactFilterCategories.Inquiries;

			var inquiryAssignedSalesRepFilter = filters.AddNkFilter("Inquiry Assigned Sales Rep", GlbStaffSchema.GS_Code, ModuleIDs.GlbStaff, StaffCollection);
			inquiryAssignedSalesRepFilter.MultilingualDescription = ResString.GetMultilingualString("e53a20ca-27bc-4ed4-870f-f1c94b19597f", FilterDescription.AssignedSalesRep);
			inquiryAssignedSalesRepFilter.Category = CampaignContactFilterCategories.Inquiries;
			inquiryAssignedSalesRepFilter.SubGroup = new GlbStaffSubGroup();

			var inquiryHasLinkedToOrganisationFilter = filters.AddTextFilter("Inquiry Has Linked to Organisation", GetInquiryHasLinkedToOrganisation, InquiryLinksToOrganisationList);
			inquiryHasLinkedToOrganisationFilter.MultilingualDescription = ResString.GetMultilingualString("6de9b03c-2441-496a-ac03-fd884eef2a7a", FilterDescription.HasLinkedToOrganization);
			inquiryHasLinkedToOrganisationFilter.Category = CampaignContactFilterCategories.Inquiries;
		}

		ZQuery GetInquiryPKQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.PK, value);
			return query;
		}

		class GlbStaffSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(CampaignContact));
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgColdCallRegisterSchema.Constants.Prefix);
				var staffSubQuery = new ZDBOnlySubQuery(typeof(GlbStaff), GlbStaffSchema.GS_Code);
				staffSubQuery.AddToFilter(filter);
				query.AddSubQuery(ViewCampaignContactSchema.VCC_GS_NKRepAssigned, staffSubQuery, JoinCondition.And);
				return query;
			}
		}

		ZQuery GetInquiryHasLinkedToOrganisation(ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgColdCallRegisterSchema.Constants.Prefix);

			bool isLinked = value.EqualsIgnoringCase(Res.GetString("3e4b5c41-374c-4a7b-a44b-5788172791dd", "Yes"));
			if (isLinked)
			{
				query.AddToFilter(ViewCampaignContactSchema.VCC_OH, SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				query.AddToFilter(ViewCampaignContactSchema.VCC_OH, SQLComparisonOperator.Equal, null);
			}

			return query;
		}

		#endregion

		#region Table Decider

		void GetDataFromTableDeciderFilters(ModuleFilterCollection filters)
		{
			if (Campaign != null)
			{
				var tableDeciderFilter = filters.AddTextFilter("TableDecider", GetDataFromTableDeciderQuery);
				tableDeciderFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			}
		}

		ZQuery GetDataFromTableDeciderQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CampaignContact));
			if (Campaign != null && Campaign.IsUsingClientIntelligenceDataSource)
			{
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
			}
			if (Campaign != null && Campaign.IsUsingInquiryDataSource)
			{
				query.AddToFilter(JoinCondition.Or, ViewCampaignContactSchema.VCC_TableCode, OrgColdCallRegisterSchema.Constants.Prefix);
			}
			return query;
		}

		#endregion

		#region Related Item Filters

		public void AddRelatedItemFilters(ModuleFilterCollection filters)
		{
			var contactsRelatedAccreditationAttemptsFilter = new PersonAccreditationAttemptsFilter(FilterDescription.AccreditationAttemptsOrgContact, GlbPersonSchema.PK, GlbAccreditationAttemptSchema.HAA_PER, ObjectFactory.Get<IGlbAccreditationAttemptCollection>("IGlbAccreditationAttemptCollection", Factory), typeof(GlbPerson));
			contactsRelatedAccreditationAttemptsFilter.MultilingualDescription = ResString.GetMultilingualString("cfb94748-e3f1-4958-9078-f4c00d2294ac", FilterDescription.AccreditationAttemptsOrgContact);
			contactsRelatedAccreditationAttemptsFilter.SubGroup = new GlbPersonContactSubGroup();
			filters.AddFilter(contactsRelatedAccreditationAttemptsFilter);

			var contactsFilter = new ContactsModuleFilter(FilterDescription.Contacts, ViewCampaignContactSchema.PK, OrgContactSchema.PK, new OrgContactCollection(Factory), typeof(CampaignContact));
			contactsFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|GlbCompanyCampaignContactFilter|Contacts", "Contacts");
			filters.AddFilter(contactsFilter);
		}

		class GlbPersonContactSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(CampaignContact));
				query.AddToFilter(ViewCampaignContactSchema.VCC_TableCode, OrgContactSchema.Constants.Prefix);
				var contactSubQuery = new ZDBOnlySubQuery(typeof(OrgContact), OrgContactSchema.PK);
				var personSubQuery = new ZDBOnlySubQuery(typeof(GlbPerson), GlbPersonSchema.PK);
				personSubQuery.AddToFilter(filter);
				contactSubQuery.AddSubQuery(OrgContactSchema.OC_PER, personSubQuery, JoinCondition.And);
				query.AddSubQuery(ViewCampaignContactSchema.PK, contactSubQuery, JoinCondition.And);
				return query;
			}
		}

		#endregion

		#region Overridden Methods

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();

			var bmHelper = (IBMFilterStripsHelper)ObjectFactory.Get("IBMFilterStripsHelper");

			Type businessObjectType;

			if (Campaign != null && Campaign.IsUsingInquiryDataSource)
			{
				bmHelper.SubColumnOverride = ViewCampaignContactSchema.PK;
				businessObjectType = typeof(SalesEnquiry);
			}
			else
			{
				bmHelper.SubColumnOverride = ViewCampaignContactSchema.VCC_OH;
				businessObjectType = typeof(OrgHeader);
			}

			bmHelper.Initialise(businessObjectType, Factory);
			bmHelper.BusinessObjectTypeOverride = typeof(CampaignContact);

			helpers.Add(bmHelper);
			return helpers;
		}

		#endregion

		#region UTC Offset Filters

		void AddUtcOffsetFilter(ModuleFilterCollection filters)
		{
			var lookup = new RefTimeZoneLookups(Factory);

			var utcOffsetFilter = new UtcOffsetFilter(FilterDescription.UTCOffset, GetUtcOffsetQuery, lookup.OffsetFromUtcList)
			{
				MultilingualDescription = ResString.GetMultilingualString("C8D67CB3-6EB2-406A-83AC-6A6730FB5E52", FilterDescription.UTCOffset)
			};

			filters.AddCustomFilter(utcOffsetFilter);
		}

		ZQuery GetUtcOffsetQuery(ZString from, ZString to)
		{
			var query = new ZQuery();
			var lookup = new RefTimeZoneLookups(Factory);
			var utcOffsetUtils = new UtcOffsetUtils(lookup.OffsetFromUtcList);
			query.AddToFilter(ViewCampaignContactSchema.VCC_OffsetMinutesFromUtc, SQLComparisonOperator.Equal, utcOffsetUtils.GetUtcOffsets(ZShort.Parse(from), ZShort.Parse(to)));
			return query;
		}

		#endregion

		public override ZQuery Filter
		{
			get
			{
				if (Campaign != null && IsPreview)
				{
					Campaign.DisableContactsNotSentToQueryCheck = true;
				}

				ZQuery query = base.Filter;
				if (Campaign != null && !query.IsNoResultQuery)
				{
					if (Campaign.G0_DeDuplicateContacts && !IsPreview)
					{
						query = Campaign.ExcludeDuplicateQuery(query);
						query.AddToFilter(GetDataFromTableDeciderQuery(SQLComparisonOperator.StartsWith, string.Empty));
					}

					if (Campaign.G0_BatchCountDefault < Campaign.MaxDisplayRecords)
					{
						query.MaximumRows = Campaign.G0_BatchCountDefault;
					}
					query.FetchOnlyFromLocalCache = true;

					if (IsPreview)
					{
						Campaign.DisableContactsNotSentToQueryCheck = false;
					}
				}
				return query;
			}
		}

		#endregion

		#region Lookups

		ReadOnlyCodeDescriptionPairList contactSources;

		ReadOnlyCodeDescriptionPairList ContactSources
		{
			get { return contactSources ?? (contactSources = OrganisationsDataRegistry.Instance.ContactSourceTypes.Value); }
		}

		GlbStaffCollection staffCollection;
		GlbStaffCollection StaffCollection
		{
			get { return staffCollection ?? (staffCollection = new GlbStaffCollection(Factory)); }
		}

		OrgHeaderCollection orgCollection;
		OrgHeaderCollection OrgCollection
		{
			get { return orgCollection ?? (orgCollection = Factory.GetCachedValue("GlbCompanyCampaignContactFilterBusinessObject.OrgCollection", delegate { return new OrgHeaderCollection(Factory); })); }
		}

		ReadOnlyCodeDescriptionPairList jobCategoryList;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Untranslatable reason")]
		ReadOnlyCodeDescriptionPairList JobCategoryList
		{
			get
			{
				return jobCategoryList ??
				(jobCategoryList = Factory.GetCachedValue<CodeDescriptionPairList>("GlbCompanyCampaignContactFilterBusinessObject.JobCategoryList", () =>
				{
					UntranslatableCodeDescriptionPairList list = new UntranslatableCodeDescriptionPairList("Description values are stored directly in the database");
					list.AddRange(OrganisationsDataRegistry.Instance.ContactJobCategories.Value.GetActiveCodeDescriptionPairList());
					return list;
				}));
			}
		}

		ReadOnlyCodeDescriptionPairList contactAttributes;
		ReadOnlyCodeDescriptionPairList ContactAttributes
		{
			get { return contactAttributes ?? (contactAttributes = Env.Registry.OrgListOfInterests); }
		}

		RefCountryCollection countries;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Testing")]
		RefCountryCollection Countries
		{
			get
			{
				if (countries == null)
				{
					countries = new RefCountryCollection(Factory);
				}
				return countries;
			}
		}

		RefUNLOCOCollection fUNLOCOs;
		public RefUNLOCOCollection UNLOCOs
		{
			get
			{
				if (fUNLOCOs == null)
				{
					fUNLOCOs = new RefUNLOCOCollection(Factory);
				}
				return fUNLOCOs;
			}
		}

		public CodeDescriptionPairList DocumentGroups
		{
			get { return OrgCodeLists.ContactType_List; }
		}

		public GlbBranchCollection BranchFilters
		{
			get { return new GlbBranchCollection(Factory); }
		}

		public OrgFlagsMappingList OrganisationFlags
		{
			get { return Factory.GetCachedValue("GlbCompanyCampaignContactFilterBusinessObject.OrganisationFlags", delegate { return new OrgFlagsMappingList(); }); }
		}

		OrgFlagsMappingList OrgColumnMappingList
		{
			get
			{
				if (fOrgColumnMappingList == null)
				{
					fOrgColumnMappingList = new OrgFlagsMappingList();
				}
				return fOrgColumnMappingList;
			}
		}
		OrgFlagsMappingList fOrgColumnMappingList;

		public ReadOnlyCodeDescriptionPairList ClientSizes
		{
			get { return OrganisationsDataRegistry.Instance.ClientSizeList.Value; }
		}

		public CodeDescriptionPairList ZeroToTenList
		{
			get { return OrgCodeLists.ZeroToTen_List; }
		}

		public ReadOnlyCodeDescriptionPairList SalesGrowthOutlooks
		{
			get { return Env.Registry.SalesGrowthOutlookList; }
		}

		public ReadOnlyCodeDescriptionPairList SalesCategories
		{
			get { return Env.Registry.SalesCategoryList; }
		}

		public CodeDescriptionBoolCollection VerticalMarketTypes
		{
			get { return OrganisationsDataRegistry.Instance.IndustryVerticalTypes.Value; }
		}

		public ReadOnlyCodeDescriptionPairList SalesTerritories
		{
			get { return Env.Registry.SalesTerritoryList; }
		}

		public ReadOnlyCodeDescriptionPairList MainCompetitorActivity_List
		{
			get { return Env.Registry.CompetitorActivityList; }
		}

		public OrgHeaderCollection Brokers
		{
			get { return new BrokerCollection(Factory); }
		}

		public CodeDescriptionPairList TradeLaneStatuses
		{
			get { return new OrgTradeDetail.TradeLaneStatus(); }
		}

		#region ContactIsPrimaryWorkplace List

		public CodeDescriptionPairList ContactIsPrimaryWorkplaceList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.AllContacts, Res.GetString("MasterFiles|GlbCompanyCampaignContactFilter|ContactIsPrimaryWorkplace|AllContacts", "All contacts"));
				list.AddPair(OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.PrimaryWorkplaceContactsOnly, Res.GetString("MasterFiles|GlbCompanyCampaignContactFilter|ContactIsPrimaryWorkplace|PrimaryWorkplaceContactsOnly", "Primary Workplace Contacts Only"));
				list.AddPair(OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.NonPrimaryWorkplaceContacts, Res.GetString("MasterFiles|GlbCompanyCampaignContactFilter|ContactIsPrimaryWorkplace|NonPrimaryWorkplaceContacts", "Non Primary Workplace Contacts"));

				return list;
			}
		}

		#endregion

		public LocationCollection Locations
		{
			get
			{
				if (locations == null)
				{
					locations = new LocationCollection(Factory);
				}
				return locations;
			}
		}
		LocationCollection locations;

		public OrgSalesProductCollection SalesProductTypes
		{
			get { return new OrgSalesProductCollection(Factory); }
		}

		public RefCommodityCodeCollection Commodities
		{
			get { return commodities ?? (commodities = new RefCommodityCodeCollection(Factory)); }
		}
		protected RefCommodityCodeCollection commodities;

		public GlbCompanyCampaignCollection Campaigns
		{
			get
			{
				if (campaigns == null)
				{
					ZQuery filter = new ZQuery(GlbCompanyCampaignSchema.G0_BroadcastVoteSurveyExam, SQLComparisonOperator.NotEqual, Core.Constants.Recruiter.LearningCentreCampaignType);
					campaigns = new GlbCompanyCampaignCollection(Factory, filter);
				}
				return campaigns;
			}
		}
		protected GlbCompanyCampaignCollection campaigns;

		public CampaignRefCountryStatesCollection GetStatesCollection
		{
			get { return getStatesCollection ?? (getStatesCollection = new CampaignRefCountryStatesCollection(Factory)); }
		}
		CampaignRefCountryStatesCollection getStatesCollection;

		public class OrgFlagsMappingList : CodeDescriptionPairList
		{
			public OrgFlagsMappingList()
				: base()
			{
				AddColumn(OrgCompanyDataSchema.OB_IsDebtor);
				AddColumn(OrgCompanyDataSchema.OB_IsCreditor);
				AddColumn(OrgHeaderSchema.OH_IsConsignee);
				AddColumn(OrgHeaderSchema.OH_IsConsignor);
				AddColumn(OrgHeaderSchema.OH_IsShippingProvider);
				AddColumn(OrgHeaderSchema.OH_IsForwarder);
				AddColumn(OrgHeaderSchema.OH_IsTransportClient);
				AddColumn(OrgHeaderSchema.OH_IsWarehouseClient);
				AddColumn(OrgHeaderSchema.OH_IsBroker);
				AddColumn(OrgHeaderSchema.OH_IsMiscFreightServices);
				AddColumn(OrgHeaderSchema.OH_IsCompetitor);
				AddColumn(OrgHeaderSchema.OH_IsSalesLead);

				if (OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value)
				{
					AddColumn(OrgHeaderSchema.OH_IsControllingAgent);
				}

				if (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value)
				{
					AddColumn(OrgHeaderSchema.OH_IsControllingCustomer);
				}
			}

			void AddColumn(SchemaColumn column)
			{
				string code;
				using (Res.TemporarilySwitchLanguage(Enterprise.Core.Constants.Languages.English))
				{
					code = DataBoundResourceStrings.GetColumnDescriptiveName(column.TableName, column.Name);
				}
				string description = DataBoundResourceStrings.GetColumnDescriptiveName(column.TableName, column.Name);
				Add(new OrgFlagMapping(code, description, column));
			}

			public SchemaColumn GetColumnFromDescription(string description)
			{
				foreach (OrgFlagMapping mapping in this)
				{
					if (mapping.Code.ToUpper() == description.ToUpper())
					{
						return mapping.Column;
					}
				}

				return null;
			}

			class OrgFlagMapping : CodeDescriptionPair
			{
				public OrgFlagMapping(string code, string description, SchemaColumn column)
					: base(code, description)
				{
					this.Column = column;
				}

				public readonly SchemaColumn Column;
			}
		}

		public CodeDescriptionPairList InquiryLinksToOrganisationList
		{
			get
			{
				if (inquiryLinksToOrganisationList == null)
				{
					inquiryLinksToOrganisationList = new CodeDescriptionPairList();
					inquiryLinksToOrganisationList.AddPair(Res.GetString("095507b7-e7aa-4bad-915f-a4fc82a6113e", "Yes"), Res.GetString("816a1aff-ef43-4a22-ae1c-5b4d47f9c1dc", "Yes"));
					inquiryLinksToOrganisationList.AddPair(Res.GetString("31779dd9-ab11-4c3f-bb60-2d88cdcd25ee", "No"), Res.GetString("7cd7bd78-60af-4e5b-aa44-5a9ed7a05359", "No"));
				}

				return inquiryLinksToOrganisationList;
			}
		}
		CodeDescriptionPairList inquiryLinksToOrganisationList;

		public ReadOnlyCodeDescriptionPairList InquiryTypes
		{
			get { return SalesEnquiryLookups.GetAllEnquiryTypes(); }
		}

		public ReadOnlyCodeDescriptionPairList InquiryLeadSources
		{
			get { return new SalesEnquiryLookups(null).Source_List; }
		}

		public CodeDescriptionPairList InquiryStatusLists
		{
			get { return new SalesEnquiryLookups(null).StatusList; }
		}

		CodeDescriptionPairList InquiryLeadInterestList
		{
			get { return new SalesEnquiryLookups(null).LeadInterest_List; }
		}

		CodeDescriptionPairList InquiryCloseReasons
		{
			get { return SalesEnquiryLookups.CreateEnquiryCloseReasonList(); }
		}

		CodeDescriptionPairList TrackingStatusList
		{
			get
			{
				if (trackingStatusList == null)
				{
					trackingStatusList = new TrackingStatusCodes();
				}
				return trackingStatusList;
			}
		}

		public bool IsPreview { get; set; }

		CodeDescriptionPairList trackingStatusList;

		#endregion

		#region FilterCategory

		public static class CampaignContactFilterCategories
		{
			public static FilterCategory Inquiries
			{
				get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.Inquiries", FilterDescription.Inquiries)); }
			}

			public static FilterCategory CommonTypes
			{
				get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.Common", FilterDescription.CommonTypes)); }
			}

			public static FilterCategory ClientIntelligenceAndInquiries
			{
				get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.ClientIntelligenceAndInquiries", FilterDescription.OrganizationManager)); }
			}

			public static FilterCategory CampaignTracking
			{
				get { return FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("FilterCategory.CampaignTracking", FilterDescription.CampaignTracking)); }
			}
		}

		#endregion

		#region Filter Descriptions

		public static class FilterDescription
		{
			#region SuppressResourceStringsCheckRegion

			public const string AccreditationAttemptsOrgContact = "Accreditation Attempts (by Contact)";
			public const string CampaignTracking = "Campaign Tracking";
			public const string OrganizationManager = "Organization/Inquiry Manager";
			public const string CommonTypes = "Common Types";
			public const string Inquiries = "Inquiries";
			public const string HasLinkedToOrganization = "Has Linked to Organization";
			public const string AssignedSalesRep = "Assigned Sales Rep";
			public const string Status = "Status";
			public const string OriginalCallDate = "Original Call Date";
			public const string SourceDetails = "Source Details";
			public const string SourceType = "Source Type";
			public const string Type = "Type";
			public const string InquiryID = "Inquiry ID";
			public const string ContactSubscriptionStatus = "Contact Subscription Status";
			public const string StaffAssignmentPersonAndRole = "Staff Assignment Person And Role";
			public const string HasReceivedCampaignsByDate = "Has Received Campaigns By Date";
			public const string HasNotReceivedCampaignsByDate = "Has Not Received Campaigns By Date";
			public const string LastSentTime = "Last Sent Time";
			public const string LastEditTime = "Last Edit Time";
			public const string CreatedTime = "Created Time";
			public const string DeliveryStatus = "Delivery Status";
			public const string IsPrimaryWorkplaceContact = "Is Primary Workplace Contact";
			public const string LastEditUser = "Last Edit User";
			public const string CreatingUser = "Creating User";
			public const string SourceCampaign = "Source Campaign";
			public const string UniqueDayActivityCount = "Unique Day(s) Activity Count";
			public const string HasDestinationURLActivity = "Has Destination URL Activity";
			public const string HasContextActivity = "Has Context Activity";
			public const string CreatedOnWeb = "Created On Web/Internal";
			public const string OrganizationName = "Organization Name";
			public const string LeadInterest = "Lead Interest";
			public const string HasReceivedCampaign = "Has Received Campaign";
			public const string HasNotReceivedCampaign = "Has Not Received Campaign";
			public const string City = "City";
			public const string EmailAddress = "Email Address";
			public const string ContactName = "Contact Name";
			public const string ReferringOrganization = "Referring Organization";
			public const string State = "State";
			public const string SalesTradeLaneCommodity = "Sales Trade Lane - Commodity";
			public const string SalesMainImportCommodity = "Sales Main Import Commodity";
			public const string SalesMainExportCommodity = "Sales Main Export Commodity";
			public const string SalesTradeLaneDestinationPort = "Sales Trade Lane - Destination Port";
			public const string SalesTradeLaneOriginPort = "Sales Trade Lane - Origin Port";
			public const string Branch = "Branch";
			public const string CountryOrPort = "Country(Region) / Port";
			public const string Organization = "Organization";
			public const string ContactDetailsVerifiedBy = "Contact Details Verified By";
			public const string CloseReason = "Close Reason";
			public const string SalesTradeLaneProduct = "Sales Trade Lane - Product";
			public const string SalesTradeLaneStatus = "Sales Trade Lane - Status";
			public const string MainCompetitorActivity = "Main Competitor Activity";
			public const string OrganizationType = "Organization Type";
			public const string OfficialContactForDocumentGroup = "Official Contact for Document Group";
			public const string DocumentGroups = "Document Groups";
			public const string ContactAttributes = "Contact Attributes";
			public const string JobCategory = "Job Category";
			public const string ContactSource = "Contact Source";
			public const string SalesWarehouseRevenue = "Sales - Warehouse Revenue";
			public const string SalesTotalRevenue = "Sales - Total Revenue";
			public const string SalesTerritory = "Sales - Sales Territory";
			public const string SalesCategory = "Sales - Sales Category";
			public const string SalesRelatedStaff = "Sales - Related Staff";
			public const string SalesPercentageWon = "Sales - Percentage Won";
			public const string SalesGrowthOutlook = "Sales - Growth Outlook";
			public const string SalesEstimatedProfit = "Sales - Estimated Profit";
			public const string SalesConversionCertainty = "Sales - Conversion Certainty";
			public const string SalesConsultingRevenue = "Sales - Consulting Revenue";
			public const string SalesAmountOfClientElectronicIntegration = "Sales - Amount of Client Electronic Integration";
			public const string SalesDifficultyWhichClientCanBePoached = "Sales - Difficulty which Client can be poached";
			public const string SalesClientDesireToRemainWithCompany = "Sales - Client Desire to Remain with Company";
			public const string SalesClientRelationship = "Sales - Client Relationship";
			public const string SalesClientSize = "Sales - Client Size";
			public const string SalesAchievableBusiness = "Sales - Achievable Business";
			public const string ContactDetailsVerifiedDate = "Contact Details Verified Date";
			public const string Contacts = "Contacts";
			public const string VerticalMarket = "Vertical Market";
			public const string UTCOffset = "UTC Offset";

			#endregion
		}

		#endregion
	}
}
