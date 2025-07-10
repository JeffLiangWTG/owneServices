using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgAgentRelationshipFilterBusinessObject))]
	sealed class OrgAgentRelationshipFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestAgencyOfficeFilter()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship3 = Factory.NewWithValidTestData<OrgAgentRelationship>();

			orgAgentRelationship1.O3_OH_SendingAgent = orgHeader1.PK;
			orgAgentRelationship2.O3_OH_SendingAgent = orgHeader2.PK;
			orgAgentRelationship2.O3_OH_ReceivingAgent = orgHeader1.PK;
			orgAgentRelationship3.O3_OH_SendingAgent = orgHeader2.PK;
			orgAgentRelationship3.O3_ProfitShareType = "AGY";

			Factory.Save();

			var filter = new OrgAgentRelationshipFilterBusinessObject();

			var agencyOfficeFilter = (ModuleGuidFilter)filter[OrgAgentRelationshipFilterBusinessObject.Descriptions.AgencyOffice];
			agencyOfficeFilter.IsActive = true;

			var collection = new OrgAgentRelationshipCollection(Factory);

			agencyOfficeFilter.Property = orgHeader1.PK;
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);
			AssertCollectionNotContains(orgAgentRelationship3, collection);

			agencyOfficeFilter.Property = orgHeader2.PK;
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);
			AssertCollectionContains(orgAgentRelationship3, collection);
		}

		public void TestStartDateFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();

			orgAgentRelationship1.O3_OH_SendingAgent = orgHeader.PK;

			var orgProfitShareDetails1 = orgAgentRelationship1.ProfitShareDetails.AddNew();
			var orgProfitShareDetails2 = orgAgentRelationship2.ProfitShareDetails.AddNew();

			orgProfitShareDetails1.O4_StartDate = ZDateTime.Today;
			orgProfitShareDetails2.O4_StartDate = ZDateTime.Today.AddDays(3);

			Factory.Save();

			var filter = new OrgAgentRelationshipFilterBusinessObject();

			var startDateFilter = (ModuleDateFilter)filter[OrgAgentRelationshipFilterBusinessObject.Descriptions.StartDate];
			startDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			startDateFilter.IsActive = true;

			var collection = new OrgAgentRelationshipCollection(Factory);

			startDateFilter.Property1 = ZDateTime.Today;
			startDateFilter.Property2 = ZDateTime.Today;
			collection.Load(filter.Filter);
			AssertCollectionContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			startDateFilter.Property1 = ZDateTime.Today.AddDays(-1);
			startDateFilter.Property2 = ZDateTime.Today.AddDays(-1);
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			startDateFilter.Property1 = ZDateTime.Today.AddDays(3);
			startDateFilter.Property2 = ZDateTime.Today.AddDays(3);
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionContains(orgAgentRelationship2, collection);

			startDateFilter.Property1 = ZDateTime.Today.AddDays(2);
			startDateFilter.Property2 = ZDateTime.Today.AddDays(2);
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			startDateFilter.Property1 = ZDateTime.Today.AddDays(4);
			startDateFilter.Property2 = ZDateTime.Today.AddDays(4);
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);
		}

		public void TestEndDateFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();

			orgAgentRelationship1.O3_OH_SendingAgent = orgHeader.PK;

			var orgProfitShareDetails1 = orgAgentRelationship1.ProfitShareDetails.AddNew();
			var orgProfitShareDetails2 = orgAgentRelationship2.ProfitShareDetails.AddNew();

			orgProfitShareDetails1.O4_EndDate = ZDateTime.Today;
			orgProfitShareDetails2.O4_EndDate = ZDateTime.Today.AddDays(3);

			Factory.Save();

			var filter = new OrgAgentRelationshipFilterBusinessObject();

			var endDateFilter = (ModuleDateFilter)filter[OrgAgentRelationshipFilterBusinessObject.Descriptions.EndDate];
			endDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			endDateFilter.IsActive = true;

			var collection = new OrgAgentRelationshipCollection(Factory);

			endDateFilter.Property1 = ZDateTime.Today;
			endDateFilter.Property2 = ZDateTime.Today;
			collection.Load(filter.Filter);
			AssertCollectionContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			endDateFilter.Property1 = ZDateTime.Today.AddDays(-1);
			endDateFilter.Property2 = ZDateTime.Today.AddDays(-1);
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			endDateFilter.Property1 = ZDateTime.Today.AddDays(3);
			endDateFilter.Property2 = ZDateTime.Today.AddDays(3);
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionContains(orgAgentRelationship2, collection);

			endDateFilter.Property1 = ZDateTime.Today.AddDays(2);
			endDateFilter.Property2 = ZDateTime.Today.AddDays(2);
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			endDateFilter.Property1 = ZDateTime.Today.AddDays(4);
			endDateFilter.Property2 = ZDateTime.Today.AddDays(4);
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);
		}

		public void TestJobTypeFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();

			orgAgentRelationship1.O3_OH_SendingAgent = orgHeader.PK;

			var orgProfitShareDetails1 = orgAgentRelationship1.ProfitShareDetails.AddNew();
			var orgProfitShareDetails2 = orgAgentRelationship2.ProfitShareDetails.AddNew();

			orgProfitShareDetails1.O4_JobType = "SHP";
			orgProfitShareDetails2.O4_JobType = "GCN";

			Factory.Save();

			var filter = new OrgAgentRelationshipFilterBusinessObject();

			var jobTypeFilter = (ModuleTextFilter)filter[OrgAgentRelationshipFilterBusinessObject.Descriptions.JobType];
			jobTypeFilter.IsActive = true;

			var collection = new OrgAgentRelationshipCollection(Factory);

			jobTypeFilter.Property = "SHP";
			collection.Load(filter.Filter);
			AssertCollectionContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			jobTypeFilter.Property = "GCN";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionContains(orgAgentRelationship2, collection);

			jobTypeFilter.Property = "ABC";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);
		}

		public void TestFreightModeFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();

			orgAgentRelationship1.O3_OH_SendingAgent = orgHeader.PK;

			var orgProfitShareDetails1 = orgAgentRelationship1.ProfitShareDetails.AddNew();
			var orgProfitShareDetails2 = orgAgentRelationship2.ProfitShareDetails.AddNew();

			orgProfitShareDetails1.O4_FreightMode = "ABC";
			orgProfitShareDetails2.O4_FreightMode = "DEF";

			Factory.Save();

			var filter = new OrgAgentRelationshipFilterBusinessObject();

			var freightModeFilter = (ModuleTextFilter)filter[OrgAgentRelationshipFilterBusinessObject.Descriptions.FreightMode];
			freightModeFilter.IsActive = true;

			var collection = new OrgAgentRelationshipCollection(Factory);

			freightModeFilter.Property = "ABC";
			collection.Load(filter.Filter);
			AssertCollectionContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			freightModeFilter.Property = "DEF";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionContains(orgAgentRelationship2, collection);

			freightModeFilter.Property = "GHI";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);
		}

		public void TestGatewayAgentTypeFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();

			orgAgentRelationship1.O3_OH_SendingAgent = orgHeader.PK;

			var orgProfitShareDetails1 = orgAgentRelationship1.ProfitShareDetails.AddNew();
			var orgProfitShareDetails2 = orgAgentRelationship2.ProfitShareDetails.AddNew();

			orgProfitShareDetails1.O4_JobType = "GCN";
			orgProfitShareDetails2.O4_JobType = "GCN";

			orgProfitShareDetails1.O4_GatewayAgentType = "SGW";
			orgProfitShareDetails2.O4_GatewayAgentType = "RGW";

			Factory.Save();

			var filter = new OrgAgentRelationshipFilterBusinessObject();

			var gatewayAgentTypeFilter = (ModuleTextFilter)filter[OrgAgentRelationshipFilterBusinessObject.Descriptions.GatewayAgentType];
			gatewayAgentTypeFilter.IsActive = true;

			var collection = new OrgAgentRelationshipCollection(Factory);

			gatewayAgentTypeFilter.Property = "SGW";
			collection.Load(filter.Filter);
			AssertCollectionContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			gatewayAgentTypeFilter.Property = "RGW";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionContains(orgAgentRelationship2, collection);

			gatewayAgentTypeFilter.Property = "ABC";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);
		}

		public void TestShareLossesFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();

			orgAgentRelationship1.O3_OH_SendingAgent = orgHeader.PK;

			var orgProfitShareDetails1 = orgAgentRelationship1.ProfitShareDetails.AddNew();
			var orgProfitShareDetails2 = orgAgentRelationship2.ProfitShareDetails.AddNew();

			orgProfitShareDetails1.O4_ShareLosses = true;
			orgProfitShareDetails2.O4_ShareLosses = false;

			Factory.Save();

			var filter = new OrgAgentRelationshipFilterBusinessObject();

			var shareLossesFilter = (ModuleFlagsFilter)filter[OrgAgentRelationshipFilterBusinessObject.Descriptions.ShareLosses];
			shareLossesFilter.IsActive = true;

			var collection = new OrgAgentRelationshipCollection(Factory);

			shareLossesFilter.Property0 = true;
			collection.Load(filter.Filter);
			AssertCollectionContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			shareLossesFilter.Property0 = false;
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionContains(orgAgentRelationship2, collection);
		}

		public void TestAgreementTypeFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();

			orgAgentRelationship1.O3_OH_SendingAgent = orgHeader.PK;

			var orgProfitShareDetails1 = orgAgentRelationship1.ProfitShareDetails.AddNew();
			var orgProfitShareDetails2 = orgAgentRelationship2.ProfitShareDetails.AddNew();

			orgProfitShareDetails1.O4_AgreementType = "ABC";
			orgProfitShareDetails2.O4_AgreementType = "DEF";

			Factory.Save();

			var filter = new OrgAgentRelationshipFilterBusinessObject();

			var agreementTypeFilter = (ModuleTextFilter)filter[OrgAgentRelationshipFilterBusinessObject.Descriptions.AgreementType];
			agreementTypeFilter.IsActive = true;

			var collection = new OrgAgentRelationshipCollection(Factory);

			agreementTypeFilter.Property = "ABC";
			collection.Load(filter.Filter);
			AssertCollectionContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			agreementTypeFilter.Property = "DEF";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionContains(orgAgentRelationship2, collection);

			agreementTypeFilter.Property = "GHI";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);
		}

		public void TestReceivingPortOrCountryFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();

			orgAgentRelationship1.O3_OH_SendingAgent = orgHeader.PK;

			var orgProfitShareDetails1 = orgAgentRelationship1.ProfitShareDetails.AddNew();
			var orgProfitShareDetails2 = orgAgentRelationship2.ProfitShareDetails.AddNew();

			orgProfitShareDetails1.O4_ReceivingPortOrCountry = "ABCD";
			orgProfitShareDetails2.O4_ReceivingPortOrCountry = "DEFG";

			Factory.Save();

			var filter = new OrgAgentRelationshipFilterBusinessObject();

			var receivingPortOrCountryFilter = (ModuleNkFilter)filter[OrgAgentRelationshipFilterBusinessObject.Descriptions.ReceivingPortOrCountry];
			receivingPortOrCountryFilter.IsActive = true;

			var collection = new OrgAgentRelationshipCollection(Factory);

			receivingPortOrCountryFilter.Property = "ABCD";
			collection.Load(filter.Filter);
			AssertCollectionContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			receivingPortOrCountryFilter.Property = "DEFG";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionContains(orgAgentRelationship2, collection);

			receivingPortOrCountryFilter.Property = "GHIJ";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);
		}

		public void TestSendingPortOrCountryFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();

			orgAgentRelationship1.O3_OH_SendingAgent = orgHeader.PK;

			var orgProfitShareDetails1 = orgAgentRelationship1.ProfitShareDetails.AddNew();
			var orgProfitShareDetails2 = orgAgentRelationship2.ProfitShareDetails.AddNew();

			orgProfitShareDetails1.O4_SendingPortOrCountry = "ABCD";
			orgProfitShareDetails2.O4_SendingPortOrCountry = "DEFG";

			Factory.Save();

			var filter = new OrgAgentRelationshipFilterBusinessObject();

			var sendingPortOrCountryFilter = (ModuleNkFilter)filter[OrgAgentRelationshipFilterBusinessObject.Descriptions.SendingPortOrCountry];
			sendingPortOrCountryFilter.IsActive = true;

			var collection = new OrgAgentRelationshipCollection(Factory);

			sendingPortOrCountryFilter.Property = "ABCD";
			collection.Load(filter.Filter);
			AssertCollectionContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			sendingPortOrCountryFilter.Property = "DEFG";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionContains(orgAgentRelationship2, collection);

			sendingPortOrCountryFilter.Property = "GHIJ";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);
		}

		public void TestGatewayProfitApportionmentMethodFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();

			orgAgentRelationship1.O3_OH_SendingAgent = orgHeader.PK;

			var orgProfitShareDetails1 = orgAgentRelationship1.ProfitShareDetails.AddNew();
			var orgProfitShareDetails2 = orgAgentRelationship2.ProfitShareDetails.AddNew();

			orgProfitShareDetails1.O4_JobType = "GCN";
			orgProfitShareDetails2.O4_JobType = "GCN";

			orgProfitShareDetails1.O4_GatewayProfitApportionmentMethod = "GVT";
			orgProfitShareDetails2.O4_GatewayProfitApportionmentMethod = "GWT";

			Factory.Save();

			var filter = new OrgAgentRelationshipFilterBusinessObject();

			var gatewayProfitApportionmentMethodFilter = (ModuleTextFilter)filter[OrgAgentRelationshipFilterBusinessObject.Descriptions.GatewayProfitApportionmentMethod];
			gatewayProfitApportionmentMethodFilter.IsActive = true;

			var collection = new OrgAgentRelationshipCollection(Factory);

			gatewayProfitApportionmentMethodFilter.Property = "GVT";
			collection.Load(filter.Filter);
			AssertCollectionContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			gatewayProfitApportionmentMethodFilter.Property = "GWT";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionContains(orgAgentRelationship2, collection);

			gatewayProfitApportionmentMethodFilter.Property = "ABC";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);
		}

		public void TestOrgOverrideTypeFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();

			orgAgentRelationship1.O3_OH_SendingAgent = orgHeader.PK;

			var orgProfitShareDetails1 = orgAgentRelationship1.ProfitShareDetails.AddNew();
			var orgProfitShareDetails2 = orgAgentRelationship2.ProfitShareDetails.AddNew();

			orgProfitShareDetails1.O4_OrgOverrideType = "IBR";
			orgProfitShareDetails2.O4_OrgOverrideType = "DEA";

			Factory.Save();

			var filter = new OrgAgentRelationshipFilterBusinessObject();

			var orgOverrideTypeFilter = (ModuleTextFilter)filter[OrgAgentRelationshipFilterBusinessObject.Descriptions.OrganizationOverrideType];
			orgOverrideTypeFilter.IsActive = true;

			var collection = new OrgAgentRelationshipCollection(Factory);

			orgOverrideTypeFilter.Property = "IBR";
			collection.Load(filter.Filter);
			AssertCollectionContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			orgOverrideTypeFilter.Property = "DEA";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionContains(orgAgentRelationship2, collection);

			orgOverrideTypeFilter.Property = "ABC";
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);
		}

		public void TestOrgOverrideFilter()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderForOverride1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderForOverride2 = Factory.NewWithValidTestData<OrgHeader>();

			var orgAgentRelationship1 = Factory.NewWithValidTestData<OrgAgentRelationship>();
			var orgAgentRelationship2 = Factory.NewWithValidTestData<OrgAgentRelationship>();

			orgAgentRelationship1.O3_OH_SendingAgent = orgHeader.PK;

			var orgProfitShareDetails1 = orgAgentRelationship1.ProfitShareDetails.AddNew();
			var orgProfitShareDetails2 = orgAgentRelationship2.ProfitShareDetails.AddNew();

			orgProfitShareDetails1.O4_OH_OrgOverride = orgHeaderForOverride1.PK;
			orgProfitShareDetails2.O4_OH_OrgOverride = orgHeaderForOverride2.PK;

			Factory.Save();

			var filter = new OrgAgentRelationshipFilterBusinessObject();

			var orgOverrideTypeFilter = (ModuleGuidFilter)filter[OrgAgentRelationshipFilterBusinessObject.Descriptions.OrganizationOverride];
			orgOverrideTypeFilter.IsActive = true;

			var collection = new OrgAgentRelationshipCollection(Factory);

			orgOverrideTypeFilter.Property = orgHeaderForOverride1.PK;
			collection.Load(filter.Filter);
			AssertCollectionContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);

			orgOverrideTypeFilter.Property = orgHeaderForOverride2.PK;
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionContains(orgAgentRelationship2, collection);

			orgOverrideTypeFilter.Property = orgHeader.PK;
			collection.Load(filter.Filter);
			AssertCollectionNotContains(orgAgentRelationship1, collection);
			AssertCollectionNotContains(orgAgentRelationship2, collection);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrgAgentRelationshipFilterBusinessObject();
		}

		#endregion
	}
}
