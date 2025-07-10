using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(ProjectFilterBusinessObject))]
	class ProjectFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ProjectFilterBusinessObject();
		}

		public void TestStatusFilter()
		{
			var ignore = new List<string>() { "NCM", "A+S", "W+S" };

			var list = new ProcessTaskFilterBusinessObject().Statuses;
			for (var i = 0; i < list.Count; i++)
			{
				if (!ignore.Contains(list[i].Code))
				{
					var proj = Factory.NewWithValidTestData<Project>();
					proj.WKP_Status = list[i].Code;
				}
			}

			Factory.Save();

			var filterBizo = new ProjectFilterBusinessObject();
			var collection = new ProjectCollection(Factory);

			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(6, collection.Count);

			ModuleTextFilter filter = ((ModuleTextFilter)filterBizo["Status"]);
			filter.Property = "OPN";
			filter.IsActive = true;
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(1, collection.Count);

			filter.Property = "ASN";
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(1, collection.Count);

			filter.Property = "WRK";
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(1, collection.Count);

			filter.Property = "SUS";
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(1, collection.Count);

			filter.Property = "CLS";
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(1, collection.Count);

			filter.Property = "CAN";
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(1, collection.Count);

			filter.Property = "NCM";
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(4, collection.Count);

			filter.Property = "A+S";
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(2, collection.Count);

			filter.Property = "W+S";
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(2, collection.Count);
		}

		public void TestSummaryFilter()
		{
			string summary = "A helpful summary of the project which is extremely descriptive and helpful";
			Project project1 = Factory.NewWithValidTestData<Project>();
			project1.WKP_Summary = summary;

			string summary2 = "Another summary of the project which is also helpful";
			Project project2 = Factory.NewWithValidTestData<Project>();
			project2.WKP_Summary = summary2;

			string summary3 = "Another not summary of a project which is not descriptive nor helpful!";
			Project project3 = Factory.NewWithValidTestData<Project>();
			project3.WKP_Summary = summary3;

			Factory.Save();

			ProjectFilterBusinessObject filterBizO = new ProjectFilterBusinessObject();
			ProjectCollection collection = new ProjectCollection(Factory);
			ModuleTextFilter summaryFilter = ((ModuleTextFilter)filterBizO["Summary"]);
			summaryFilter.IsActive = true;

			summaryFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			summaryFilter.Property = summary;
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(project1));

			summaryFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			summaryFilter.Property = summary;
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(project2));
			Assert(collection.Contains(project3));

			summaryFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			summaryFilter.Property = "Another";
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(2, collection.Count);
			Assert(collection.Contains(project2));
			Assert(collection.Contains(project3));

			summaryFilter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			summaryFilter.Property = "Another";
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(project1));

			summaryFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			summaryFilter.Property = "Frog";
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(0, collection.Count);

			summaryFilter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			summaryFilter.Property = "Frog";
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(3, collection.Count);
			Assert(collection.Contains(project1));
			Assert(collection.Contains(project2));
			Assert(collection.Contains(project3));

			AssertEquals("Summary filter does not have IsBlank operator", false, summaryFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			AssertEquals("Summary filter does not have IsNotBlank operator", false, summaryFilter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
		}

		public void TestClientFilter()
		{
			var project1 = Factory.NewWithValidTestData<Project>();
			project1.WKP_Summary = "Project 1";
			project1.ClientOrganisation.OH_Code = "AML";

			var project2 = Factory.NewWithValidTestData<Project>();
			project2.WKP_Summary = "Project 2";
			project2.ClientOrganisation.OH_Code = "TST";

			// when changing clients, need to re-fill in the client contact
			var project3 = Factory.NewWithValidTestData<Project>();
			project3.WKP_Summary = "Project 3";
			project3.WKP_OA_ClientAddress = project2.ClientOrganisation.MainAddress.PK;
			project3.WKP_OC_Contact = project2.ClientOrganisation.Contacts[0].PK;

			var project4 = Factory.NewWithValidTestData<Project>();
			project4.WKP_Summary = "Project 4";
			project4.ClientOrganisation.OH_Code = "JNG";

			var project5 = Factory.New<Project>();
			project5.WKP_Summary = "Project 5";
			AssertNull(project5.ClientOrganisation);
			AssertNull(project5.ClientAddress);

			Factory.Save();

			var filterBizo = new ProjectFilterBusinessObject();
			var results = Factory.Load<Project>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("No filters were specified so all projects should be returned.", new[] { "Project 1", "Project 2", "Project 3", "Project 4", "Project 5" }, results.Select(x => x.WKP_Summary));

			var filter = filterBizo.AddGuidFilterStrip("Client", project1.ClientOrganisation.PK);
			results = Factory.Load<Project>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Project 1" }, results.Select(x => x.WKP_Summary));

			filter.Property = project2.ClientOrganisation.PK;
			results = Factory.Load<Project>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Project 2", "Project 3" }, results.Select(x => x.WKP_Summary));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = project2.ClientOrganisation.PK;
			results = Factory.Load<Project>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Project 1", "Project 4", "Project 5" }, results.Select(x => x.WKP_Summary));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			results = Factory.Load<Project>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Project 1", "Project 2", "Project 3", "Project 4" }, results.Select(x => x.WKP_Summary));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			results = Factory.Load<Project>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "Project 5" }, results.Select(x => x.WKP_Summary));
		}

		public void TestClientNameFilter()
		{
			Project proj1 = Factory.NewWithValidTestData<Project>();
			proj1.ClientOrganisation.OH_FullName = "AML";

			Project proj2 = Factory.NewWithValidTestData<Project>();
			proj2.ClientOrganisation.OH_FullName = "TST";

			// when changing clients, need to re-fill in the client contact
			Project proj3 = Factory.NewWithValidTestData<Project>();
			proj3.WKP_OA_ClientAddress = proj2.ClientOrganisation.MainAddress.PK;
			proj3.WKP_OC_Contact = proj2.ClientOrganisation.Contacts[0].PK;

			Project proj4 = Factory.NewWithValidTestData<Project>();
			proj4.ClientOrganisation.OH_FullName = "JNG";

			Factory.Save();

			ProjectFilterBusinessObject filterBizo = new ProjectFilterBusinessObject();
			ProjectCollection collection = new ProjectCollection(Factory);

			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals("Total", 4, collection.Count);

			ModuleTextFilter filter = ((ModuleTextFilter)filterBizo["Client Name"]);
			filter.Property = proj1.ClientOrganisation.OH_FullName;
			filter.IsActive = true;
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals("AML filter", 1, collection.Count);
			AssertCollectionContains("AML client", proj1, collection);

			filter.Property = proj2.ClientOrganisation.OH_FullName;
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals("TST filter", 2, collection.Count);
			AssertCollectionContains("TST client", proj2, collection);
			AssertCollectionContains("TST client", proj3, collection);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = proj2.ClientOrganisation.OH_FullName;
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals("'Not equal' to TST filter", 2, collection.Count);
			AssertCollectionContains("AML client", proj1, collection);
			AssertCollectionContains("JNG client", proj4, collection);

			AssertEquals("Client Name filter does not have IsBlank operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			AssertEquals("Client Name filter does not have IsNotBlank operator", false, filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));
		}

		public void TestLocationFilter()
		{
			Project proj1 = Factory.NewWithValidTestData<Project>();
			OrgHeader client = proj1.ClientOrganisation;
			client.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			OrgAddress clientAddress = client.Addresses.AddNew();
			clientAddress.OA_Address1 = "Jones Street";
			clientAddress.OA_City = "Ultimo";
			clientAddress.OA_PostCode = "2007";
			clientAddress.OA_State = "NSW";
			clientAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			proj1.WKP_OA_ClientAddress = clientAddress.PK;

			Project proj2 = Factory.NewWithValidTestData<Project>();
			proj2.ClientOrganisation.MainAddress.OA_RL_NKRelatedPortCode = "USCHI";

			// make proj1 valid so it can be saved (when setting a different client address, the contact blanks out)
			OrgContact contact = client.Contacts.AddNew();
			contact.OC_ContactName = "Alex";
			proj1.WKP_OC_Contact = contact.PK;
			Factory.Save();

			ProjectFilterBusinessObject filterBizo = new ProjectFilterBusinessObject();
			ProjectCollection collection = new ProjectCollection(Factory);

			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(2, collection.Count);

			ModuleNkFilter filter = ((ModuleNkFilter)filterBizo["Location"]);
			filter.Property = "AUSYD";
			filter.IsActive = true;
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(proj1, collection);

			filter.Property = "US";
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains(proj2, collection);

			filter.Property = "NOPE";
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(0, collection.Count);
		}

		public void TestProjectManagerFilter()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "AAA";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "BBB";
			GlbStaff staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "CCC";
			GlbStaff staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_Code = "DDD";

			Project project1 = Factory.NewWithValidTestData<Project>();
			project1.WKP_GS_NKProjectManager = staff1.GS_Code;
			Project project2 = Factory.NewWithValidTestData<Project>();
			project2.WKP_GS_NKProjectManager = staff1.GS_Code;
			Project project3 = Factory.NewWithValidTestData<Project>();
			project3.WKP_GS_NKProjectManager = staff2.GS_Code;
			Project project4 = Factory.NewWithValidTestData<Project>();
			project4.WKP_GS_NKProjectManager = staff3.GS_Code;
			Project project5 = Factory.NewWithValidTestData<Project>();
			project5.WKP_GS_NKProjectManager = staff3.GS_Code;
			Factory.Save();

			ProjectFilterBusinessObject filterBizO = new ProjectFilterBusinessObject();
			ProjectCollection collection = new ProjectCollection(Factory);
			ModuleNkFilter projectManagerFilter = ((ModuleNkFilter)filterBizO["Project Manager"]);
			projectManagerFilter.IsActive = true;

			projectManagerFilter.Property = staff1.GS_Code;
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(2, collection.Count);

			projectManagerFilter.Property = staff2.GS_Code;
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(1, collection.Count);

			projectManagerFilter.Property = staff3.GS_Code;
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(2, collection.Count);

			projectManagerFilter.Property = staff4.GS_Code;
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(0, collection.Count);
		}

		public void TestOpportunityNumber()
		{
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity4 = Factory.NewWithValidTestData<OrgOpportunity>();

			var project1 = Factory.NewWithValidTestData<Project>();
			project1.WKP_P8_Opportunity = opportunity1.PK;
			var project2 = Factory.NewWithValidTestData<Project>();
			project2.WKP_P8_Opportunity = opportunity2.PK;
			var project3 = Factory.NewWithValidTestData<Project>();
			project3.WKP_P8_Opportunity = opportunity3.PK;
			var project4 = Factory.NewWithValidTestData<Project>();
			project4.WKP_P8_Opportunity = opportunity3.PK;
			Factory.Save();

			var filterBizO = new ProjectFilterBusinessObject();
			var opportunityNumber = ((ModuleGuidFilter)filterBizO["Opportunity #"]);
			opportunityNumber.IsActive = true;

			opportunityNumber.Property = opportunity1.PK;
			var collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals(project1, collection[0]);

			opportunityNumber.Property = opportunity2.PK;
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals(project2, collection[0]);

			opportunityNumber.Property = opportunity3.PK;
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { project3, project4 }, collection);

			opportunityNumber.Property = opportunity4.PK;
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(0, collection.Count);
		}

		public void TestOpportunitySalesPerson()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "AAA";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "BBB";
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "CCC";
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();
			staff4.GS_Code = "DDD";

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity1.P8_GS_NKPrimarySalesPerson = staff1.GS_Code;
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity2.P8_GS_NKPrimarySalesPerson = staff2.GS_Code;
			var opportunity3 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity3.P8_GS_NKPrimarySalesPerson = staff3.GS_Code;
			var opportunity4 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity4.P8_GS_NKPrimarySalesPerson = staff1.GS_Code;
			var opportunity5 = Factory.NewWithValidTestData<OrgOpportunity>();

			var project1 = Factory.NewWithValidTestData<Project>();
			project1.WKP_P8_Opportunity = opportunity1.PK;
			var project2 = Factory.NewWithValidTestData<Project>();
			project2.WKP_P8_Opportunity = opportunity2.PK;
			var project3 = Factory.NewWithValidTestData<Project>();
			project3.WKP_P8_Opportunity = opportunity3.PK;
			var project4 = Factory.NewWithValidTestData<Project>();
			project4.WKP_P8_Opportunity = opportunity4.PK;
			var project5 = Factory.NewWithValidTestData<Project>();
			project5.WKP_P8_Opportunity = opportunity5.PK;
			_ = Factory.NewWithValidTestData<Project>();
			Factory.Save();

			var filterBizO = new ProjectFilterBusinessObject();
			var opportunitySalesPersonFilter = ((ModuleNkFilter)filterBizO["Opportunity Sales Person"]);
			opportunitySalesPersonFilter.IsActive = true;

			opportunitySalesPersonFilter.Property = staff1.GS_Code;
			var collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { project1, project4 }, collection);

			opportunitySalesPersonFilter.Property = staff2.GS_Code;
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals(project2, collection[0]);

			opportunitySalesPersonFilter.Property = staff3.GS_Code;
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(1, collection.Count);
			AssertEquals(project3, collection[0]);

			opportunitySalesPersonFilter.Property = staff4.GS_Code;
			collection = new ProjectCollection(Factory, filterBizO.Filter);
			AssertEquals(0, collection.Count);
		}

		[TestDate(2014, 04, 01)]
		public void TestDeferredFilter()
		{
			ProjectFilterBusinessObject filterBizo = new ProjectFilterBusinessObject();
			ProjectCollection collection = new ProjectCollection(Factory);

			collection = new ProjectCollection(Factory, filterBizo.Filter);

			Project proj1 = Factory.NewWithValidTestData<Project>();
			var proj1_jobHeader = ProcessJobHeaderProvider.GetForParent(proj1, Factory);
			proj1_jobHeader.DoNotStartBeforeDateLocal = ZDateTime.Today;

			Project proj2 = Factory.NewWithValidTestData<Project>();
			var proj2_jobHeader = ProcessJobHeaderProvider.GetForParent(proj2, Factory);
			proj2_jobHeader.DoNotStartBeforeDateLocal = ZDateTime.Today.AddDays(2);

			Factory.Save();

			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals("Total", 2, collection.Count);

			ModuleDateFilter filter = ((ModuleDateFilter)filterBizo["Project Deferred"]);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = proj1.DeferredDateLocal.AddHours(-1);
			filter.Property2 = proj1.DeferredDateLocal.AddHours(1);
			filter.IsActive = true;
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains("Project 1", proj1, collection);

			filter.Property1 = proj2.DeferredDateLocal.AddHours(-1);
			filter.Property2 = proj2.DeferredDateLocal.AddHours(1);
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains("Project 2", proj2, collection);

			filter.Property1 = ZDateTime.Today.AddDays(5).AddHours(1);
			filter.Property2 = ZDateTime.Today.AddDays(5).AddHours(1);
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(0, collection.Count);
		}

		[TestDate(2014, 04, 01)]
		public void TestDeferredDateMetFilter()
		{
			ProjectFilterBusinessObject filterBizo = new ProjectFilterBusinessObject();
			ProjectCollection collection = new ProjectCollection(Factory);

			collection = new ProjectCollection(Factory, filterBizo.Filter);

			Project proj1 = Factory.NewWithValidTestData<Project>();
			var proj1_jobHeader = ProcessJobHeaderProvider.GetForParent(proj1, Factory);
			proj1_jobHeader.DoNotStartBeforeDateLocal = ZDateTime.Today.AddDays(-5);

			Project proj2 = Factory.NewWithValidTestData<Project>();
			var proj2_jobHeader = ProcessJobHeaderProvider.GetForParent(proj2, Factory);
			proj2_jobHeader.DoNotStartBeforeDateLocal = ZDateTime.Today.AddDays(2);

			Factory.Save();

			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals("Total", 2, collection.Count);

			ModuleFlagsFilter filter = ((ModuleFlagsFilter)filterBizo["Def. Date Met"]);
			filter.Property0 = true;
			filter.IsActive = true;
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains("Project 1", proj1, collection);

			filter.Property0 = false;
			collection = new ProjectCollection(Factory, filterBizo.Filter);
			AssertEquals(1, collection.Count);
			AssertCollectionContains("Project 2", proj2, collection);
		}

		public void TestProjectStartedIsUtc()
		{
			var filterBizO = new ProjectFilterBusinessObject();
			AssertNotNull(filterBizO);
			var filter = filterBizO["Project Started"] as ModuleDateFilter;
			AssertNotNull(filter);
			Assert("Should be UTC convertible", filter.ConvertFromLocalToUTC);
		}

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<Project>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.ProjectCRMSecurity);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;
			ObjectFactory.Get<IBMTestHelper>().CreateSystem(Factory, "WKP");
		}
	}
}
