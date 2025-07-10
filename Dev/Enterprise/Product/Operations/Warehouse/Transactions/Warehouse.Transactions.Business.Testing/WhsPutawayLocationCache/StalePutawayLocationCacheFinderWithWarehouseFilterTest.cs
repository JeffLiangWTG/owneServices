using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class StalePutawayLocationCacheFinderWithWarehouseFilterTest : StalePutawayLocationCacheFinderTest
	{
		protected override void TestFindCacheEntriesThatNeedUpdating_MultipleWarehousesCore()
		{
			var now = ZDateTime.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse1 = data.Whs1;
			var location1 = warehouse1.FindLocation("A-1");
			location1.LocationType.WLT_SystemLastEditTimeUtc = now;
			SetLocationWeightAndCubicConstraints(location1, 5, 5);

			var warehouse2 = Helper.CreateWarehouse("WHS2");
			var location2 = Helper.CreateRowAndGenerateLocations(warehouse2, "AA", 1, 1, 1).Locations.Single();
			location2.LocationType.WLT_SystemLastEditTimeUtc = now;
			SetLocationWeightAndCubicConstraints(location2, 5, 5);

			Factory.Save();

			TestDateAttribute.Date = now.AddMinutes(5).ToDateTime();
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location1.PK);
			new WhsPutawayLocationCacheManager().CreateCache(Factory, location2.PK);
			var staleLocationInfos = ObjectFactory.Get<IStalePutawayLocationCacheFinder>()
				.FindCacheEntriesThatNeedUpdating(Factory);
			AssertEquals("Returned collection of stale locations should not include both locations.", false,
				staleLocationInfos.Any(staleLocation => staleLocation.LocationPK == location1.PK));
			AssertEquals("Returned collection of stale locations should not include both locations.", false,
				staleLocationInfos.Any(staleLocation => staleLocation.LocationPK == location2.PK));

			SetLocationWeightAndCubicConstraints(location1, 10, 10);
			SetLocationWeightAndCubicConstraints(location2, 10, 10);
			Factory.Save();

			var staleLocations2 = FindCacheEntriesThatNeedUpdating(Factory, warehouse1.PK);
			AssertCollectionContains("Returned collection of stale locations should only include warehouse1 location.",
				location1.PK, staleLocations2);
			AssertCollectionNotContains(
				"Returned collection of stale locations should not include warehouse2 location.", location2.PK,
				staleLocations2);

			var staleLocations3 = FindCacheEntriesThatNeedUpdating(Factory, warehouse2.PK);
			AssertCollectionContains("Returned collection of stale locations should only include warehouse2 location.",
				location2.PK, staleLocations3);
			AssertCollectionNotContains(
				"Returned collection of stale locations should not include warehouse1 location.", location1.PK,
				staleLocations3);
		}

		protected override IEnumerable<ZGuid> FindCacheEntriesThatNeedUpdating(BusinessObjectFactory factory,
			ZGuid warehousePK)
			=> ObjectFactory.Get<IStalePutawayLocationCacheFinder>()
				.FindCacheEntriesThatNeedUpdating(factory, warehousePK).ToArray();
	}
}
