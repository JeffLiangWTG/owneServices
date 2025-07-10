using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module
{
	[TestedType(typeof(GlbCompanyCampaignFilterBusinessObject))]
	public class GlbCompanyCampaignFilterBusinessObject_Test : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestCategoryFilter_Description()
		{
			var filterBizo1 = new GlbCompanyCampaignFilterBusinessObject();
			AssertEquals(OrganisationsDataRegistry.Instance.CampaignCategory1Label.DefaultValue, filterBizo1["Category 1"].MultilingualDescription);
			AssertEquals(OrganisationsDataRegistry.Instance.CampaignCategory2Label.DefaultValue, filterBizo1["Category 2"].MultilingualDescription);

			OrganisationsDataRegistry.Instance.CampaignCategory1Label.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My CRM Campaign Category 1");
			OrganisationsDataRegistry.Instance.CampaignCategory2Label.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My CRM Campaign Category 2");

			var filterBizo2 = new GlbCompanyCampaignFilterBusinessObject();
			AssertEquals("My CRM Campaign Category 1", filterBizo2["Category 1"].MultilingualDescription);
			AssertEquals("My CRM Campaign Category 2", filterBizo2["Category 2"].MultilingualDescription);
		}

		public void TestCampaignCoordinatorFilter()
		{
			GlbCompanyCampaign campaign1 = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaign campaign2 = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaign campaign3 = Factory.New<GlbCompanyCampaign>();

			campaign1.G0_GS_NKCampaignCoordinator = "BK";
			campaign2.G0_GS_NKCampaignCoordinator = "H.B";
			campaign3.G0_GS_NKCampaignCoordinator = "PE";

			ModuleNkFilter managerFilter = (ModuleNkFilter)CampaignFilter["Campaign Coordinator"];
			managerFilter.Property = "PE";
			managerFilter.IsActive = true;

			Campaigns.Load(CampaignFilter.Filter);
			AssertCollectionNotContains(campaign1, Campaigns);
			AssertCollectionNotContains(campaign2, Campaigns);
			AssertCollectionContains(campaign3, Campaigns);

			managerFilter.Property = "H.B";
			managerFilter.IsActive = true;

			Campaigns.Load(CampaignFilter.Filter);
			AssertCollectionNotContains(campaign1, Campaigns);
			AssertCollectionContains(campaign2, Campaigns);
			AssertCollectionNotContains(campaign3, Campaigns);
		}

		public void TestCampaignNameFilter()
		{
			GlbCompanyCampaign campaign1 = Factory.New<GlbCompanyCampaign>();
			campaign1.G0_CampaignName = "CAT";

			GlbCompanyCampaign campaign2 = Factory.New<GlbCompanyCampaign>();
			campaign2.G0_CampaignName = "ACAT";

			GlbCompanyCampaign campaign3 = Factory.New<GlbCompanyCampaign>();
			campaign3.G0_CampaignName = "CATS";

			((ModuleTextFilter)CampaignFilter["Campaign Name"]).Property = "CAT";
			((ModuleTextFilter)CampaignFilter["Campaign Name"]).IsActive = true;
			((ModuleTextFilter)CampaignFilter["Campaign Name"]).SqlComparisonOperator = SQLComparisonOperator.Contains;

			Campaigns.Load(CampaignFilter.Filter);

			AssertCollectionContains("Contains operator, should contain Campaign 1", campaign1, Campaigns);
			AssertCollectionContains("Contains operator, should contain Campaign 2", campaign2, Campaigns);
			AssertCollectionContains("Contains operator, should contain Campaign 3", campaign3, Campaigns);

			((ModuleTextFilter)CampaignFilter["Campaign Name"]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			Campaigns.Load(CampaignFilter.Filter);

			AssertCollectionContains("Starts with operator, should contain Campaign 1", campaign1, Campaigns);
			AssertCollectionNotContains("Starts with operator, should NOT contain Campaign 2", campaign2, Campaigns);
			AssertCollectionContains("Starts with operator, should contain Campaign 3", campaign3, Campaigns);

			((ModuleTextFilter)CampaignFilter["Campaign Name"]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			Campaigns.Load(CampaignFilter.Filter);

			AssertCollectionContains("Equals operator, should contain Campaign 1", campaign1, Campaigns);
			AssertCollectionNotContains("Equals operator, should NOT contain Campaign 2", campaign2, Campaigns);
			AssertCollectionNotContains("Equals operator, should NOT contain Campaign 3", campaign3, Campaigns);
		}

		public void TestCampaignManagerFilter()
		{
			GlbCompanyCampaign campaign1 = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaign campaign2 = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaign campaign3 = Factory.New<GlbCompanyCampaign>();

			campaign1.G0_GS_NKCampaignManager = "BK";
			campaign2.G0_GS_NKCampaignManager = "H.B";
			campaign3.G0_GS_NKCampaignManager = "PE";

			ModuleNkFilter managerFilter = (ModuleNkFilter)CampaignFilter["Campaign Manager"];
			managerFilter.Property = "BK";
			managerFilter.IsActive = true;

			Campaigns.Load(CampaignFilter.Filter);
			AssertCollectionContains(campaign1, Campaigns);
			AssertCollectionNotContains(campaign2, Campaigns);
			AssertCollectionNotContains(campaign3, Campaigns);

			managerFilter.Property = "H.B";
			managerFilter.IsActive = true;

			Campaigns.Load(CampaignFilter.Filter);
			AssertCollectionNotContains(campaign1, Campaigns);
			AssertCollectionContains(campaign2, Campaigns);
			AssertCollectionNotContains(campaign3, Campaigns);
		}

		public void TestCompanyFilter()
		{
			ModuleGuidFilter companyFilter = (ModuleGuidFilter)CampaignFilter["Company"];
			AssertEquals(FilterVisibility.AlwaysVisible, companyFilter.Visibility);
			AssertEquals(GlbCompany.CurrentCompany.PK, companyFilter.DefaultProperty);

			GlbCompany company1 = Factory.New<GlbCompany>();
			GlbCompany company2 = Factory.New<GlbCompany>();

			GlbCompanyCampaign campaign1 = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaign campaign2 = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaign campaign3 = Factory.New<GlbCompanyCampaign>();

			campaign1.G0_GC = company1.PK;
			campaign2.G0_GC = company2.PK;
			campaign3.G0_GC = GlbCompany.CurrentCompany.PK;

			((ModuleGuidFilter)CampaignFilter["Company"]).IsActive = true;
			Campaigns.Load(CampaignFilter.Filter);

			AssertCollectionNotContains("should NOT contain Campaign 1", campaign1, Campaigns);
			AssertCollectionNotContains("should NOT contain Campaign 2", campaign2, Campaigns);
			AssertCollectionContains("should contain Campaign 3", campaign3, Campaigns);
		}

		public void TestCompanyFilter_Validation()
		{
			ModuleGuidFilter companyFilter = (ModuleGuidFilter)CampaignFilter["Company"];
			AssertNull("If allowed, PropertyValidation should be null", companyFilter.PropertyValidation);

			Env.Security.CampaignAllowSearchOutsideLoginCompany.IsAllowed = false;
			FilterStripBusinessObject newCampaignFilter = GetNewFilterStripBusinessObject();
			ModuleGuidFilter newCompanyFilter = (ModuleGuidFilter)newCampaignFilter["Company"];
			AssertNotNull("If not allowed, validation should not be null", newCompanyFilter.PropertyValidation);

			newCompanyFilter.PropertyValidation(newCompanyFilter.PropertyInfo);
			AssertNoErrors(newCompanyFilter.PropertyInfo);

			ZQuery otherCompanyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			GlbCompany otherCompany = Factory.LoadTop1<GlbCompany>(otherCompanyQuery);
			newCompanyFilter.Property = otherCompany.PK;
			string expectedErrorMessage = @"Your current security rights only allow you to view campaigns relevant to your current login company (EDI).
If you think this is incorrect, please contact your system administrator.";
			AssertHasError(newCompanyFilter.PropertyInfo, expectedErrorMessage);
		}

		public void TestDateTimeFilterControlFilter()
		{
			AssertDateFieldFilterIsCorrect(GlbCompanyCampaignSchema.G0_ActualStartedDate.Name, "Actual Start Date");
			AssertDateFieldFilterIsCorrect(GlbCompanyCampaignSchema.G0_ActualCompletedDate.Name, "Actual Complete Date");
			AssertDateFieldFilterIsCorrect(GlbCompanyCampaignSchema.G0_EstimatedStartedDate.Name, "Estimated Start Date");
			AssertDateFieldFilterIsCorrect(GlbCompanyCampaignSchema.G0_EstimatedCompletedDate.Name, "Estimated Complete Date");

			ZDateTime refDate = new ZDateTime(2003, 4, 4);

			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaign campaign2 = Factory.New<GlbCompanyCampaign>();

			Campaigns.Load(CampaignFilter.Filter);

			Assert("Collection should contain Campaign", Campaigns.Contains(campaign));
			Assert("Collection should contain Campaign2", Campaigns.Contains(campaign2));
		}

		void AssertDateFieldFilterIsCorrect(ZString dateField, ZString dateTypeForFilter)
		{
			ZDateTime refDate = new ZDateTime(2003, 4, 4);
			GlbCompanyCampaignFilterBusinessObject campaignFilter = new GlbCompanyCampaignFilterBusinessObject();

			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			GlbCompanyCampaign campaign2 = Factory.New<GlbCompanyCampaign>();
			campaign[dateField] = refDate.AddDays(2);
			campaign2[dateField] = refDate.AddDays(1);

			((ModuleDateFilter)campaignFilter[dateTypeForFilter]).IsActive = true;
			((ModuleDateFilter)campaignFilter[dateTypeForFilter]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)campaignFilter[dateTypeForFilter]).Property1 = refDate;
			GlbCompanyCampaignCollection collection = new GlbCompanyCampaignCollection(Factory, campaignFilter.Filter);
			collection.Load();

			Assert("Collection should contain Campaign", collection.Contains(campaign));
			Assert("Collection should contain Campaign2", collection.Contains(campaign2));

			((ModuleDateFilter)campaignFilter[dateTypeForFilter]).Property2 = refDate.AddDays(1);
			GlbCompanyCampaignCollection collection2 = new GlbCompanyCampaignCollection(Factory, campaignFilter.Filter);
			collection2.Load();
			Assert("Collection should NOT contain Campaign", !collection2.Contains(campaign));
			Assert("Collection should contain Campaign2", collection.Contains(campaign2));
		}

		public void TestCampaignIDFilter()
		{
			ModuleTextFilter campaignIDFilter = (ModuleTextFilter)CampaignFilter["Campaign ID"];
			campaignIDFilter.IsActive = true;

			GlbCompanyCampaign campaign1 = Factory.New<GlbCompanyCampaign>();
			campaign1.G0_CampaignID = "ABC00001000";
			campaign1.G0_Category = "EMAIL";
			campaign1.G0_Type = "EXIST";
			campaign1.G0_CampaignName = "Campaign Name1";
			GlbCompanyCampaign campaign2 = Factory.New<GlbCompanyCampaign>();
			campaign2.G0_CampaignID = "XYZ00001001";
			campaign2.G0_Category = "DLIST";
			campaign2.G0_Type = "EXIST";
			campaign2.G0_CampaignName = "Campaign Name2";

			Factory.Save();

			campaignIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			campaignIDFilter.Property = campaign1.G0_CampaignID;
			GlbCompanyCampaignCollection collection = new GlbCompanyCampaignCollection(Factory, CampaignFilter.Filter);
			collection.Load();
			AssertCollectionContains(campaign1, collection);
			AssertCollectionNotContains(campaign2, collection);

			campaignIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			campaignIDFilter.Property = "0000100";
			collection = new GlbCompanyCampaignCollection(Factory, CampaignFilter.Filter);
			collection.Load();
			AssertCollectionContains(campaign1, collection);
			AssertCollectionContains(campaign2, collection);

			campaignIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			campaignIDFilter.Property = "ABC";
			collection = new GlbCompanyCampaignCollection(Factory, CampaignFilter.Filter);
			collection.Load();
			AssertCollectionContains(campaign1, collection);
			AssertCollectionNotContains(campaign2, collection);

			campaignIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			campaignIDFilter.Property = "1001";
			collection = new GlbCompanyCampaignCollection(Factory, CampaignFilter.Filter);
			collection.Load();
			AssertCollectionContains(campaign1, collection);
			AssertCollectionNotContains(campaign2, collection);

			campaignIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			campaignIDFilter.Property = "ABC";
			collection = new GlbCompanyCampaignCollection(Factory, CampaignFilter.Filter);
			collection.Load();
			AssertCollectionNotContains(campaign1, collection);
			AssertCollectionContains(campaign2, collection);

			campaignIDFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			campaignIDFilter.Property = string.Empty;
			collection = new GlbCompanyCampaignCollection(Factory, CampaignFilter.Filter);
			collection.Load();
			AssertCollectionContains(campaign1, collection);
			AssertCollectionContains(campaign2, collection);
		}

		public void TestAddWorkflowCustomFieldsFilters()
		{
			var collection = new GlbCompanyCampaignFilterBusinessObject().ModuleFilters;

			AssertNull(collection["Custom1 String"]);
			AssertNull(collection["Custom1 Integer"]);
			AssertNull(collection["Workflow Flags"]);
			AssertNull(collection["Custom2 Boolean"]);
			AssertNull(collection["Custom2 Datetime"]);
			AssertNull(collection["Unrelated Custom String"]);

			PrepareTemplatesWithCustomFields();

			collection = new GlbCompanyCampaignFilterBusinessObject().ModuleFilters;

			AssertEquals(typeof(ModuleTextFilter), collection["Custom1 String"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), collection["Custom1 Integer"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), collection["Workflow Flags"].GetType());
			AssertEquals(typeof(ModuleTextFilter), collection["Custom2 Boolean"].GetType());
			AssertEquals(typeof(ModuleDateFilter), collection["Custom2 Datetime"].GetType());
			AssertNull(collection["Unrelated Custom String"]);
		}

		void PrepareTemplatesWithCustomFields()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = CRMCampaignWorkflowDescriptor.WorkflowTypeCode;

			var template1Definition1 = template1.GenCustomColumnDefinitions.AddNew();
			template1Definition1.XC_Name = "Custom1 String";
			template1Definition1.XC_Type = AddOnColumnDataType.Codes.String;

			var template1Definition2 = template1.GenCustomColumnDefinitions.AddNew();
			template1Definition2.XC_Name = "Custom1 Integer";
			template1Definition2.XC_Type = AddOnColumnDataType.Codes.Integer;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = CRMCampaignWorkflowDescriptor.WorkflowTypeCode;

			var template2Definition1 = template2.GenCustomColumnDefinitions.AddNew();
			template2Definition1.XC_Name = "Custom2 Boolean";
			template2Definition1.XC_Type = AddOnColumnDataType.Codes.Boolean;

			var template2Definition2 = template2.GenCustomColumnDefinitions.AddNew();
			template2Definition2.XC_Name = "Custom2 Datetime";
			template2Definition2.XC_Type = AddOnColumnDataType.Codes.Datetime;

			var unrelatedTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			unrelatedTemplate.P0_ProcessType = "ZZZ";

			var unrelatedTemplateDefinition = unrelatedTemplate.GenCustomColumnDefinitions.AddNew();
			unrelatedTemplateDefinition.XC_Name = "Unrelated Custom String";
			unrelatedTemplateDefinition.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			WorkflowCustomFieldsFilter.ClearCache();
		}

		#endregion

		#region CRM Security

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<GlbCompanyCampaign>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.CampaignManagementCRMSecurity);
		}

		#endregion

		#region Recipient Contacts Filters

		#region JobCategory

		public void TestJobCategoryFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var contact_EMA_1 = Factory.NewWithValidTestData<OrgContact>();
			contact_EMA_1.OC_JobCategory = OrgContactJobCategories.Codes.EMA;
			contact_EMA_1.OC_OH = orgHeader.PK;

			var contact_EMA_2 = Factory.NewWithValidTestData<OrgContact>();
			contact_EMA_2.OC_JobCategory = OrgContactJobCategories.Codes.EMA;
			contact_EMA_2.OC_OH = orgHeader.PK;

			var contact_LEA = Factory.NewWithValidTestData<OrgContact>();
			contact_LEA.OC_JobCategory = OrgContactJobCategories.Codes.LEA;
			contact_LEA.OC_OH = orgHeader.PK;

			var contact_MAA = Factory.NewWithValidTestData<OrgContact>();
			contact_MAA.OC_JobCategory = OrgContactJobCategories.Codes.MAA;
			contact_MAA.OC_OH = orgHeader.PK;

			var campaign1 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign1.G0_CampaignName = "Test Campaign";
			var campaignItem1_1 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem1_1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1_1.G8_RecipientID = contact_EMA_1.PK;
			campaignItem1_1.G8_G0 = campaign1.PK;

			var campaignItem1_2 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem1_2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem1_2.G8_RecipientID = contact_EMA_2.PK;
			campaignItem1_2.G8_G0 = campaign1.PK;

			var campaign2 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign2.G0_CampaignName = "Test Campaign";
			var campaignItem2_1 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem2_1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2_1.G8_RecipientID = contact_EMA_1.PK;
			campaignItem2_1.G8_G0 = campaign2.PK;

			var campaignItem2_2 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem2_2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem2_2.G8_RecipientID = contact_LEA.PK;
			campaignItem2_2.G8_G0 = campaign2.PK;

			var campaign3 = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign3.G0_CampaignName = "Test Campaign";
			var campaignItem3_1 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem3_1.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3_1.G8_RecipientID = contact_EMA_1.PK;
			campaignItem3_1.G8_G0 = campaign3.PK;

			var campaignItem3_2 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem3_2.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3_2.G8_RecipientID = contact_LEA.PK;
			campaignItem3_2.G8_G0 = campaign3.PK;

			var campaignItem3_3 = Factory.NewWithValidTestData<GlbCompanyCampaignItem>();
			campaignItem3_3.G8_RecipientTableCode = OrgContactSchema.Constants.Prefix;
			campaignItem3_3.G8_RecipientID = contact_MAA.PK;
			campaignItem3_3.G8_G0 = campaign3.PK;
			Factory.Save();

			var jobCategoryFilter = (JobCategoryModuleTextFilter)CampaignFilter["Job Category"];
			jobCategoryFilter.IsActive = true;

			var campaignName = (ModuleTextFilter)CampaignFilter["Campaign Name"];
			campaignName.IsActive = true;
			campaignName.Property = "Test Campaign";

			AssertEquals("Recipient Contacts", jobCategoryFilter.Category.Description);
			AssertEquals("Job Category", jobCategoryFilter.MultilingualDescription.ToString());
			AssertEquals(3, jobCategoryFilter.ComparisonOperator_List.Count);
			AssertEquals(jobCategoryFilter.OperatorAnyMatch, jobCategoryFilter.ComparisonOperator_List[0].Code);
			AssertEquals(jobCategoryFilter.OperatorAllMatch, jobCategoryFilter.ComparisonOperator_List[1].Code);
			AssertEquals(jobCategoryFilter.OperatorNoneMatch, jobCategoryFilter.ComparisonOperator_List[2].Code);

			AssertJobCategoryFilterResult(jobCategoryFilter, jobCategoryFilter.OperatorAnyMatch, OrgContactJobCategories.Codes.EMA, new ZGuid[] { campaign1.PK, campaign2.PK, campaign3.PK });
			AssertJobCategoryFilterResult(jobCategoryFilter, jobCategoryFilter.OperatorAnyMatch, OrgContactJobCategories.Codes.LEA, new ZGuid[] { campaign2.PK, campaign3.PK });
			AssertJobCategoryFilterResult(jobCategoryFilter, jobCategoryFilter.OperatorAnyMatch, OrgContactJobCategories.Codes.MAA, new ZGuid[] { campaign3.PK });
			AssertJobCategoryFilterResult(jobCategoryFilter, jobCategoryFilter.OperatorNoneMatch, OrgContactJobCategories.Codes.EMA, Array.Empty<ZGuid>());
			AssertJobCategoryFilterResult(jobCategoryFilter, jobCategoryFilter.OperatorNoneMatch, OrgContactJobCategories.Codes.LEA, new ZGuid[] { campaign1.PK });
			AssertJobCategoryFilterResult(jobCategoryFilter, jobCategoryFilter.OperatorNoneMatch, OrgContactJobCategories.Codes.MAA, new ZGuid[] { campaign1.PK, campaign2.PK });
			AssertJobCategoryFilterResult(jobCategoryFilter, jobCategoryFilter.OperatorAllMatch, OrgContactJobCategories.Codes.EMA, new ZGuid[] { campaign1.PK });
			AssertJobCategoryFilterResult(jobCategoryFilter, jobCategoryFilter.OperatorAllMatch, OrgContactJobCategories.Codes.LEA, Array.Empty<ZGuid>());
			AssertJobCategoryFilterResult(jobCategoryFilter, jobCategoryFilter.OperatorAllMatch, OrgContactJobCategories.Codes.MAA, Array.Empty<ZGuid>());
		}

		void AssertJobCategoryFilterResult(ModuleTextFilter filter, string comparisonOperator, string property, ZGuid[] expectedPks)
		{
			filter.ComparisonOperator = comparisonOperator;
			filter.Property = property;

			var collection = new GlbCompanyCampaignCollection(Factory, CampaignFilter.Filter);
			collection.Load();

			AssertEquals(expectedPks.Length, collection.Count);
			foreach (var pk in expectedPks)
			{
				collection.Contains(pk);
			}
		}

		#endregion

		#endregion

		#region Implementation

		GlbCompanyCampaignFilterBusinessObject campaignFilter;
		GlbCompanyCampaignCollection campaigns;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbCompanyCampaignFilterBusinessObject();
		}

		GlbCompanyCampaignFilterBusinessObject CampaignFilter
		{
			get { return campaignFilter ?? (campaignFilter = new GlbCompanyCampaignFilterBusinessObject()); }
		}

		GlbCompanyCampaignCollection Campaigns
		{
			get { return campaigns ?? (campaigns = new GlbCompanyCampaignCollection(Factory)); }
		}

		#endregion
	}
}
