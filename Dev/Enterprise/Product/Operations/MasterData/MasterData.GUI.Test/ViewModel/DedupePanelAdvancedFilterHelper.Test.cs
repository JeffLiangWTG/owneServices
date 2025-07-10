using System.Collections.Generic;
using System.Runtime.Caching;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.MasterData.GUI.Tests
{
	class DedupePanelAdvancedFilterHelperTest : TestCaseWithFactory
	{
		public void TestSetGetThenClearCache()
		{
			var memoryCache = MemoryCache.Default;
			var previousCacheCount = memoryCache.GetCount();

			var testCacheName = "TestCacheName";
			var testCacheKey = DedupePanelAdvancedFilterHelper.RegionName + testCacheName;
			var testCacheValue = "TestCacheValue";

			DedupePanelAdvancedFilterHelper.SetCache(testCacheName, testCacheValue, false);
			AssertEquals(testCacheValue, memoryCache.Get(testCacheKey).ToString());

			var currentCacheCount = memoryCache.GetCount();
			AssertEquals(previousCacheCount + 1, currentCacheCount);
			AssertEquals(testCacheValue, DedupePanelAdvancedFilterHelper.GetCache(testCacheName).ToString());

			DedupePanelAdvancedFilterHelper.ClearCache();
			AssertEquals(previousCacheCount, memoryCache.GetCount());
			AssertEquals(false, memoryCache.Contains(testCacheKey));
		}

		public void TestUpdatePersistenceFiltersValue()
		{
			var findQuery = new ZQuery(StmDataSchema.SD_Owner, Env.CurrentUser.PK);
			findQuery.AddToFilter(StmDataSchema.SD_Name, DedupePanelAdvancedFilterHelper.MDMPersonDeduplicationPanelFiltersValueKey);
			findQuery.AddToFilter(StmDataSchema.SD_Type, RegistryDataTypes.Codes.Binary);

			DedupePanelAdvancedFilterHelper.SetCache("DUMMY", new DummyFilterCache { DummyValue = "AA" }, true);
			DedupePanelAdvancedFilterHelper.PersistencePersonFiltersValue(Factory);

			var stmData = Factory.LoadTop1<StmData>(findQuery);
			AssertNotNull("Precondition: Cached Dummy Filter should be saved to StmData table", stmData);

			var result = JsonConvert.DeserializeObject<List<object>>(stmData.SD_BinaryValue.ToUTF8(), new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			});
			AssertEquals("Precondition: 1 filter value was found", 1, result.Count);
			AssertEquals("Precondition: StmData should be serialized correctly", "AA", (result[0] as DummyFilterCache).DummyValue);

			DedupePanelAdvancedFilterHelper.SetCache("DUMMY", new DummyFilterCache { DummyValue = "BB" }, true);
			DedupePanelAdvancedFilterHelper.PersistencePersonFiltersValue(Factory);

			result = JsonConvert.DeserializeObject<List<object>>(stmData.SD_BinaryValue.ToUTF8(), new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			});

			AssertEquals("Precondition: still only 1 filter value was found", 1, result.Count);
			AssertEquals("StmData updated correctly", "BB", (result[0] as DummyFilterCache).DummyValue);
		}

		protected override void SetUp()
		{
			DedupePanelAdvancedFilterHelper.ClearCache();
		}

		protected override void TearDown()
		{
			DedupePanelAdvancedFilterHelper.ClearCache();
		}

		class DummyFilterCache
		{
			public string DummyValue { get; set; }
		}
	}
}
