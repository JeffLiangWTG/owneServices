using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business.Test
{
	public class WorkProjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestActiveTypes()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var project = Factory.New<Project>();

			AssertContainsExactElementsInAnyOrder(tree.GetParents(true), project.Lookups.ActiveTypes);
			AssertContainsExactElementsInAnyOrder(tree.GetParents(false), project.Lookups.AllTypes);
		}

		public void TestActiveSubtypes()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var project = Factory.New<Project>();
			project.WKP_Type = "";
			var actual1 = project.Lookups.ActiveSubtypes;
			project.WKP_Type = "AAA";
			var actual2 = project.Lookups.ActiveSubtypes;

			AssertContainsExactElementsInAnyOrder(tree.GetChildren("", true), actual1);
			AssertContainsExactElementsInAnyOrder(tree.GetChildren("AAA", true), actual2);
		}

		public void TestActiveModules()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var project = Factory.New<Project>();
			var actual1 = project.Lookups.ActiveModules;

			project.WKP_Type = "AAA";
			project.WKP_SubType = "AAA";
			var actual2 = project.Lookups.ActiveModules;

			project.WKP_Type = "BBB";
			project.WKP_SubType = "BB1";
			var actual3 = project.Lookups.ActiveModules;

			AssertContainsExactElementsInAnyOrder(tree.GetChildren("", "", true), actual1);
			AssertContainsExactElementsInAnyOrder(tree.GetChildren("AAA", "AAA", true), actual2);
			AssertContainsExactElementsInAnyOrder(tree.GetChildren("BBB", "BB1", true), actual3);
		}

		public void TestActivePriorities()
		{
			var tree = CodeDescriptionBoolTreeTestHelper.CreateTestTree4();
			ProcessManagementRegistry.Instance.ProjectTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var project = Factory.New<Project>();
			var actual1 = project.Lookups.ActivePriorities;

			project.WKP_Type = "1AA";
			project.WKP_Module = "3AA";
			var actual2 = project.Lookups.ActivePriorities;

			project.WKP_SubType = "2AA";
			var actual3 = project.Lookups.ActivePriorities;

			project.WKP_Type = "1BB";
			project.WKP_SubType = "2B1";
			project.WKP_Module = "";
			var actual4 = project.Lookups.ActivePriorities;

			project.WKP_Module = "3S1";
			var actual5 = project.Lookups.ActivePriorities;

			AssertContainsExactElementsInAnyOrder(tree.GetChildren(true, "", "", ""), actual1);
			AssertContainsExactElementsInAnyOrder(tree.GetChildren(true, "1AA", "", "3AA"), actual2);
			AssertContainsExactElementsInAnyOrder(tree.GetChildren(true, "1AA", "2AA", "3AA"), actual3);
			AssertContainsExactElementsInAnyOrder(tree.GetChildren(true, "1BB", "2B1", ""), actual4);
			AssertContainsExactElementsInAnyOrder(tree.GetChildren(true, "1BB", "2B1", "3S1"), actual5);
		}

		public void TestClients()
		{
			var project = Factory.New<Project>();
			string orgName = "Such an unique org name";
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			client1.OH_FullName = orgName;
			client2.OH_FullName = orgName;
			client1.OH_IsActive = true;
			client2.OH_IsActive = false;

			OrganisationsFindBoxCollection collection = project.Lookups.Clients;
			collection.LoadWithMoreFiltering(new ZQuery(OrgHeaderSchema.OH_FullName, orgName));
			AssertEquals(true, collection.Contains(client1));
			AssertEquals(false, collection.Contains(client2));
		}

		public void TestProjectManagers()
		{
			var project = Factory.New<Project>();
			var staff1 = Factory.New<GlbStaff>();
			var staff2 = Factory.New<GlbStaff>();
			staff1.GS_IsActive = true;
			staff2.GS_IsActive = false;

			GlbStaffCollection collection = project.Lookups.ProjectManagers;
			AssertEquals(true, collection.Contains(staff1));
			AssertEquals(false, collection.Contains(staff2));
		}

		public void TestStaff()
		{
			var project = Factory.New<Project>();
			var staff1 = Factory.New<GlbStaff>();
			var staff2 = Factory.New<GlbStaff>();
			staff1.GS_IsActive = true;
			staff2.GS_IsActive = false;

			GlbStaffCollection collection = project.Lookups.Staff;
			AssertEquals(true, collection.Contains(staff1));
			AssertEquals(true, collection.Contains(staff2));
		}

		public void TestOpportunities()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_OH = client.PK;

			var project = Factory.New<Project>();
			project.ClientOrganisationPK = ZGuid.Empty;
			OrgOpportunityCollection collection = project.Lookups.Opportunities;
			collection.Load();
			AssertEquals("None because a client has not been selected on the project", 0, collection.Count);
			AssertEquals(false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor("Organisation" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));

			project.ClientOrganisationPK = client.PK;
			collection = project.Lookups.Opportunities;
			collection.Load();
			AssertEquals(1, collection.Count);
			AssertEquals(client.PK, collection.FilterBusinessObjectDefaults["Organisation" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"].Value);
		}

		public void TestStatusList()
		{
			CodeDescriptionPairList expected = new ProcessTaskStatusCodeList();

			var workItem = Factory.New<WorkItem>();
			var actual = workItem.Lookups.StatusList;
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionComparer(), expected.ToArray(), actual.ToArray());
		}

		void AssertContainsExactElementsInAnyOrder(CodeDescriptionPairList expected, CodeDescriptionPairList actual)
		{
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionComparer(), expected.ToArray(), actual.ToArray());
		}

		public class CodeDescriptionComparer : IEqualityComparer<ICodeDescription>
		{
			public int GetHashCode(ICodeDescription type)
			{
				return type.GetHashCode();
			}

			public bool Equals(ICodeDescription first, ICodeDescription second)
			{
				return first.Code == second.Code && first.Description == second.Description;
			}
		}
	}
}
