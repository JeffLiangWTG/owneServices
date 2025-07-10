using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(SalesTeamFilterBusinessObject))]
	sealed class SalesTeamFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Text

		public void TestSearchBySalesRepName()
		{
			SalesTeam group1 = Factory.NewWithValidTestData<SalesTeam>();
			SalesTeam group2 = Factory.NewWithValidTestData<SalesTeam>();

			GlbStaff salesRep1 = Factory.NewWithValidTestData<GlbStaff>();
			salesRep1.GS_FullName = "Lucy Hehir";
			salesRep1.GS_IsSalesRep = true;
			GlbStaff salesRep2 = Factory.NewWithValidTestData<GlbStaff>();
			salesRep2.GS_FullName = "Richard John";
			salesRep2.GS_IsSalesRep = true;
			GlbStaff salesRep3 = Factory.NewWithValidTestData<GlbStaff>();
			salesRep3.GS_FullName = "Lisa Tree";
			salesRep3.GS_IsSalesRep = true;

			group1.Staff.Add(salesRep1);
			group2.Staff.Add(salesRep2);

			Factory.Save();

			SalesTeamFilterBusinessObject salesRep1Filter = new SalesTeamFilterBusinessObject();
			((ModuleGuidFilter)salesRep1Filter["Sales Rep"]).Property = salesRep1.PK;
			((ModuleGuidFilter)salesRep1Filter["Sales Rep"]).IsActive = true;

			SalesTeamCollection collection = new SalesTeamCollection(Factory, salesRep1Filter.Filter);

			Assert("Filtered for sales rep's teams, collection should contain group1", collection.Contains(group1));
			Assert("Filtered for sales rep's teams, collection should NOT contain group2", !collection.Contains(group2));

			SalesTeamFilterBusinessObject salesRep2Filter = new SalesTeamFilterBusinessObject();
			((ModuleGuidFilter)salesRep2Filter["Sales Rep"]).Property = salesRep2.PK;
			((ModuleGuidFilter)salesRep2Filter["Sales Rep"]).IsActive = true;

			collection = new SalesTeamCollection(Factory, salesRep2Filter.Filter);

			Assert("Filtered for sales rep's teams, collection should NOT contain group1", !collection.Contains(group1));
			Assert("Filtered for sales rep's teams, collection should contain group2", collection.Contains(group2));

			SalesTeamFilterBusinessObject salesRep3Filter = new SalesTeamFilterBusinessObject();
			((ModuleGuidFilter)salesRep3Filter["Sales Rep"]).Property = salesRep3.PK;
			((ModuleGuidFilter)salesRep3Filter["Sales Rep"]).IsActive = true;

			collection = new SalesTeamCollection(Factory, salesRep3Filter.Filter);

			Assert("Filtered for sales rep's teams, collection should NOT contain group1", !collection.Contains(group1));
			Assert("Filtered for sales rep's teams, collection should NOT contain group2", !collection.Contains(group2));
		}

		#endregion

		#region Company

		public void TestCompanyFilter()
		{
			var companyA = Factory.NewWithValidTestData<GlbCompany>();
			var companyB = Factory.NewWithValidTestData<GlbCompany>();

			var globalTeam = Factory.NewWithValidTestData<SalesTeam>();
			globalTeam.GG_GC = ZGuid.Empty;
			var teamA = Factory.NewWithValidTestData<SalesTeam>();
			teamA.GG_GC = companyA.PK;
			var teamB = Factory.NewWithValidTestData<SalesTeam>();
			teamB.GG_GC = companyB.PK;

			Factory.Save();

			Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.IsAllowed = true;

			var filterBizObj = new SalesTeamFilterBusinessObject();
			var companyFilter = (ModuleGuidFilter)filterBizObj["Company"];

			companyFilter.IsActive = true;
			companyFilter.Property = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder(new[] { globalTeam, teamA, teamB }, Factory.Load<SalesTeam>(filterBizObj.Filter));

			companyFilter.Property = companyA.PK;
			AssertContainsExactElementsInAnyOrder(new[] { teamA }, Factory.Load<SalesTeam>(filterBizObj.Filter));

			companyFilter.Property = companyB.PK;
			AssertContainsExactElementsInAnyOrder(new[] { teamB }, Factory.Load<SalesTeam>(filterBizObj.Filter));

			companyFilter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.IsBlank;
			AssertContainsExactElementsInAnyOrder(new[] { globalTeam }, Factory.Load<SalesTeam>(filterBizObj.Filter));

			Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.IsAllowed = false;
			filterBizObj = new SalesTeamFilterBusinessObject();
			companyFilter = (ModuleGuidFilter)filterBizObj["Company"];
			AssertContainsExactElementsInAnyOrder(new[] { ModuleGuidFilter.ComparisonConstants.Exact, ModuleGuidFilter.ComparisonConstants.IsBlank }, companyFilter.AllowedComparisonOperators);

			string expectedSecurityErrorMessage = string.Format(@"You do not have the appropriate security rights to view or edit Sales Teams outside your current login company ({0}).

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: 

{1}", GlbCompany.CurrentCompany.GC_Code, Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.DisplayTextPathToSecurityRight);

			companyFilter.IsActive = true;
			companyFilter.Property = companyA.PK;
			AssertHasError(companyFilter.PropertyInfo, expectedSecurityErrorMessage);

			companyFilter.Property = companyB.PK;
			AssertHasError(companyFilter.PropertyInfo, expectedSecurityErrorMessage);

			companyFilter.Property = ZGuid.Empty;
			AssertNoErrors(companyFilter.PropertyInfo);

			companyFilter.Property = GlbCompany.CurrentCompany.PK;
			AssertNoErrors(companyFilter.PropertyInfo);
		}

		public void TestGlobalOrCurrentLoginCompanyFilter()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();

			var globalTeam = Factory.NewWithValidTestData<SalesTeam>();
			globalTeam.GG_GC = ZGuid.Empty;
			var otherCompanyTeam = Factory.NewWithValidTestData<SalesTeam>();
			otherCompanyTeam.GG_GC = otherCompany.PK;
			var currentCompanyTeam = Factory.NewWithValidTestData<SalesTeam>();
			currentCompanyTeam.GG_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.IsAllowed = true;

			var filterBizObj = new SalesTeamFilterBusinessObject();
			var globalOrCurrentLoginCompanyFilter = (ModuleFlagsFilter)filterBizObj["GlobalOrCurrentLoginCompany"];
			AssertEquals(FilterVisibility.AlwaysVisible, globalOrCurrentLoginCompanyFilter.Visibility);
			AssertEquals(true, globalOrCurrentLoginCompanyFilter.DefaultProperties["Show Global or for Current Login Company Only"]);

			globalOrCurrentLoginCompanyFilter.IsActive = true;
			globalOrCurrentLoginCompanyFilter.Property0 = ZBool.True;
			AssertNoErrors(globalOrCurrentLoginCompanyFilter.Property0Info);
			AssertContainsExactElementsInAnyOrder(new[] { globalTeam, currentCompanyTeam }, Factory.Load<SalesTeam>(filterBizObj.Filter));

			globalOrCurrentLoginCompanyFilter.Property0 = ZBool.False;
			AssertNoErrors(globalOrCurrentLoginCompanyFilter.Property0Info);
			AssertContainsExactElementsInAnyOrder(new[] { globalTeam, currentCompanyTeam, otherCompanyTeam }, Factory.Load<SalesTeam>(filterBizObj.Filter));

			Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.IsAllowed = false;
			filterBizObj = new SalesTeamFilterBusinessObject();
			globalOrCurrentLoginCompanyFilter = (ModuleFlagsFilter)filterBizObj["GlobalOrCurrentLoginCompany"];

			globalOrCurrentLoginCompanyFilter.IsActive = true;
			globalOrCurrentLoginCompanyFilter.Property0 = ZBool.True;
			AssertNoErrors(globalOrCurrentLoginCompanyFilter.Property0Info);

			globalOrCurrentLoginCompanyFilter.Property0 = ZBool.False;
			AssertHasError(globalOrCurrentLoginCompanyFilter.Property0Info, string.Format(@"You do not have the appropriate security rights to view or edit Sales Teams outside your current login company ({0}).

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: 

{1}", GlbCompany.CurrentCompany.GC_Code, Env.Security.SalesTeamsAllowSearchOutsideLoginCompany.DisplayTextPathToSecurityRight));
		}

		#endregion

		#region Parent Group

		public void TestParentTeam()
		{
			var grandparentTeam = MasterFilesTestHelper.CreateSalesTeam(Factory, "A", "Team A");
			var parentTeam = MasterFilesTestHelper.CreateSalesTeam(Factory, "B", "Team B");
			var childTeam = MasterFilesTestHelper.CreateSalesTeam(Factory, "C", "Team C");

			childTeam.GG_GG_ParentGroup = parentTeam.PK;
			parentTeam.GG_GG_ParentGroup = grandparentTeam.PK;

			Factory.Save();

			var bizo = new SalesTeamFilterBusinessObject();
			var filter = (ModuleGuidFilter)bizo["ParentTeamPk"];

			filter.IsActive = true;

			AssertResults(ModuleGuidFilter.ComparisonConstants.Exact, grandparentTeam.PK, parentTeam);
			AssertResults(ModuleGuidFilter.ComparisonConstants.Exact, parentTeam.PK, childTeam);
			AssertResults(ModuleGuidFilter.ComparisonConstants.Exact, childTeam.PK);

			AssertResults(ModuleGuidFilter.ComparisonConstants.NotEqual, grandparentTeam.PK, grandparentTeam, childTeam);
			AssertResults(ModuleGuidFilter.ComparisonConstants.IsNotBlank, ZGuid.Empty, parentTeam, childTeam);
			AssertResults(ModuleGuidFilter.ComparisonConstants.IsBlank, ZGuid.Empty, grandparentTeam);

			void AssertResults(string comparisonOperator, ZGuid value, params GlbGroup[] expectedGroups)
			{
				filter.ComparisonOperator = comparisonOperator;
				filter.Property = value;
				var query = new ZQuery(GlbGroupSchema.GG_Type, SQLComparisonOperator.Equal, GlbGroupTypeList.Codes.Staff);
				query.AddToFilter(bizo.Filter);
				var groups = Factory.Load<SalesTeam>(query);
				AssertContainsExactElementsInAnyOrder(expectedGroups, groups);
			}
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new SalesTeamFilterBusinessObject();
		}

		#endregion
	}
}
