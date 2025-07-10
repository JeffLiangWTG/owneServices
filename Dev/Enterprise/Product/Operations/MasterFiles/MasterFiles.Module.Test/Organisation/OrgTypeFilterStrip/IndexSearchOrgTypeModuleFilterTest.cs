using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(IndexSearchOrgTypeModuleFilter))]
	sealed class IndexSearchOrgTypeModuleFilterTest : ModuleFilterTestCase<IndexSearchOrgTypeModuleFilter>
	{
		IndexSearchOrgTypeModuleFilter TestOrgTypeModuleFilter => testOrgTypeModuleFilter ?? (testOrgTypeModuleFilter = new IndexSearchOrgTypeModuleFilter("Organisation Type"));

		IndexSearchOrgTypeModuleFilter testOrgTypeModuleFilter;

		public void TestGetGlowIndexQuery()
		{
			TestOrgTypeModuleFilter.Property0 = true;
			TestOrgTypeModuleFilter.Property1 = true;
			TestOrgTypeModuleFilter.Property2 = true;
			TestOrgTypeModuleFilter.Property3 = true;
			TestOrgTypeModuleFilter.Property4 = true;
			TestOrgTypeModuleFilter.Property5 = true;
			TestOrgTypeModuleFilter.Property6 = true;
			TestOrgTypeModuleFilter.Property7 = true;
			TestOrgTypeModuleFilter.Property8 = true;
			TestOrgTypeModuleFilter.Property9 = true;
			TestOrgTypeModuleFilter.Property10 = true;
			TestOrgTypeModuleFilter.Property11 = true;
			TestOrgTypeModuleFilter.Property12 = true;
			TestOrgTypeModuleFilter.Property13 = true;

			Assert(TestOrgTypeModuleFilter.AndJoinCondition && !TestOrgTypeModuleFilter.OrJoinCondition);
			AssertEquals($"((CWDefaultHiddenOrgCompanyPKWithIsDebtor eq {GlbCompany.CurrentCompany.PK}) " +
				$"and (CWDefaultHiddenOrgCompanyPKWithIsCreditor eq {GlbCompany.CurrentCompany.PK}) " +
				$"and (OrganizationType eq '\"IsConsignee\"') " +
				$"and (OrganizationType eq '\"IsConsignor\"') " +
				$"and (OrganizationType eq '\"IsCarrier\"') " +
				$"and (OrganizationType eq '\"IsForwarder\"') " +
				$"and (OrganizationType eq '\"IsTransportClient\"') " +
				$"and (OrganizationType eq '\"IsWarehouseClient\"') " +
				$"and (OrganizationType eq '\"IsBroker\"') " +
				$"and (OrganizationType eq '\"IsServices\"') " +
				$"and (OrganizationType eq '\"IsCompetitor\"') " +
				$"and (OrganizationType eq '\"IsSales\"') " +
				$"and (OrganizationType eq '\"IsControllingAgent\"') " +
				$"and (OrganizationType eq '\"IsControllingCustomer\"'))",
				TestOrgTypeModuleFilter.GetGlowIndexQuery().ToUrlComponent());

			TestOrgTypeModuleFilter.AndJoinCondition = false;
			TestOrgTypeModuleFilter.OrJoinCondition = true;

			Assert(!TestOrgTypeModuleFilter.AndJoinCondition && TestOrgTypeModuleFilter.OrJoinCondition);
			AssertEquals($"((CWDefaultHiddenOrgCompanyPKWithIsDebtor eq {GlbCompany.CurrentCompany.PK}) " +
				$"or (CWDefaultHiddenOrgCompanyPKWithIsCreditor eq {GlbCompany.CurrentCompany.PK}) " +
				$"or (OrganizationType eq '\"IsConsignee\"') " +
				$"or (OrganizationType eq '\"IsConsignor\"') " +
				$"or (OrganizationType eq '\"IsCarrier\"') " +
				$"or (OrganizationType eq '\"IsForwarder\"') " +
				$"or (OrganizationType eq '\"IsTransportClient\"') " +
				$"or (OrganizationType eq '\"IsWarehouseClient\"') " +
				$"or (OrganizationType eq '\"IsBroker\"') " +
				$"or (OrganizationType eq '\"IsServices\"') " +
				$"or (OrganizationType eq '\"IsCompetitor\"') " +
				$"or (OrganizationType eq '\"IsSales\"') " +
				$"or (OrganizationType eq '\"IsControllingAgent\"') " +
				$"or (OrganizationType eq '\"IsControllingCustomer\"'))",
				TestOrgTypeModuleFilter.GetGlowIndexQuery().ToUrlComponent());
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.StatusAndFlags;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, TestOrgTypeModuleFilter.IsExpensiveQuery);
		}

		protected override IndexSearchOrgTypeModuleFilter GetNewModuleFilter()
		{
			return new IndexSearchOrgTypeModuleFilter("moo");
		}

		protected override string[] GetPropertiesExcludedFromCacheInvalidationTest(IndexSearchOrgTypeModuleFilter filter)
		{
			return base.GetPropertiesExcludedFromCacheInvalidationTest(filter).Concat(new[] { "Item" }).ToArray(); // This will cause the test to skip the indexer, which is problematic and sets values for properties that are tested independently anyway.
		}
	}
}
