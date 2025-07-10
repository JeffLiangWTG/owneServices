using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing.Accounting
{
	sealed class AccPOSChargeCodeGroupPivotViewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGroups()
		{
			var pivot = Factory.New<AccPOSChargeCodeGroupPivot>();
			AssertNull("Pre-condition: ChargeCodeGroup is not set", pivot.ChargeCodeGroup);

			var lookups = pivot.Lookups;
			AssertNotNull(lookups);

			AssertNotNull("Groups", lookups.Groups);
			AssertNotNull("Groups.Master", lookups.Groups.Master);
			AssertEquals("Groups Master is Current Company", Env.CurrentCompanyPK, lookups.Groups.Master.PK);

			var group = Factory.New<AccPOSChargeCodeGroup>();
			AssertEquals("Pre-condition: Set to the Current Company by default", Env.CurrentCompanyPK, group.GRO_GC);
			pivot.GRP_GRO_Group = group.PK;
			AssertEquals("Groups Master is Current Company", Env.CurrentCompanyPK, lookups.Groups.Master.PK);

			var company = Factory.New<GlbCompany>();
			group.GRO_GC = company.PK;
			AssertEquals("Groups Master is Group Company", company.PK, lookups.Groups.Master.PK);
		}

		public void TestChargeCodes()
		{
			var pivot = Factory.New<AccPOSChargeCodeGroupPivot>();
			AssertNull("Pre-condition: ChargeCodeGroup is not set", pivot.ChargeCodeGroup);

			var lookups = pivot.Lookups;
			AssertNotNull(lookups);

			AssertNotNull("ChargeCodes", lookups.ChargeCodes);
			Assert("IsNoResultQuery", lookups.ChargeCodes.CompleteFilter.IsNoResultQuery);

			var group = Factory.New<AccPOSChargeCodeGroup>();
			AssertEquals("Pre-condition: Set to the Current Company by default", Env.CurrentCompanyPK, group.GRO_GC);
			pivot.GRP_GRO_Group = group.PK;
			AssertEquals("IsNoResultQuery", false, lookups.ChargeCodes.CompleteFilter.IsNoResultQuery);

			var filterText = lookups.ChargeCodes.CompleteFilter.GetAsWhereClause(true);
			AssertContains("Charge Codes for Current Company", Env.CurrentCompanyPK.ToString(), filterText);

			var company = Factory.New<GlbCompany>();
			group.GRO_GC = company.PK;
			AssertEquals("IsNoResultQuery", false, lookups.ChargeCodes.CompleteFilter.IsNoResultQuery);

			filterText = lookups.ChargeCodes.CompleteFilter.GetAsWhereClause(true);
			AssertContains("Chargew Codes for Group Company", company.PK.ToString(), filterText);
		}
	}
}
