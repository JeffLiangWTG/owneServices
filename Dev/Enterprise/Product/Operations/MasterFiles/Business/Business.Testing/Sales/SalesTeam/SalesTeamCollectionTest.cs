using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesTeamCollection))]
	sealed class SalesTeamCollectionTest : ActiveBusinessObjectCollectionTestCase<SalesTeamCollection>
	{
		public void TestRelationshipFilter()
		{
			var salesTeam1 = Factory.NewWithValidTestData<SalesTeam>();
			var salesTeam2 = Factory.NewWithValidTestData<SalesTeam>();
			salesTeam1.GG_IsSales = true;
			salesTeam2.GG_IsSales = true;
			//			Factory.Save();

			var collection = GetCollectionToTest();
			AssertCollectionContains(salesTeam1, collection);
			AssertCollectionContains(salesTeam2, collection);

			salesTeam1.GG_IsSales = false;
			salesTeam2.GG_IsSales = false;
			var salesTeam3 = Factory.NewWithValidTestData<SalesTeam>();
			salesTeam3.GG_IsSales = true;

			//			Factory.Save();
			AssertCollectionNotContains(salesTeam1, collection);
			AssertCollectionNotContains(salesTeam2, collection);
			AssertCollectionContains(salesTeam3, collection);
		}
	}
}
