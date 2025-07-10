using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.US.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[UseSnapshotProtection]
	class WhsPutawayLocationCacheHoldLockTest : TestCase
	{
		#region TestCreateCache_HoldLock

		public void TestCreateCache_HoldLock()
		{
			var factory = new BusinessObjectFactory();
			var data = new TestDataSimpleEnvironment(factory, 3, 3);
			var warehouse = data.Whs1;
			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			var location3 = warehouse.FindLocation("A-3");
			factory.Save();

			((IDbConnected)factory).Connection.BeginTransaction();
			new WhsPutawayLocationCacheManager().CreateCache(factory, new[] { location1.PK, location2.PK });
			factory.Save();

			var isSecondUserLocked = false;
			var task = Task.Factory.StartNew(() =>
			{
				using (Db.DisposableActionForDbConnection())
				using (var connectionForSecondUser = Db.NewExtraConnectionToMainDb())
				{
					var secondFactory = new BusinessObjectFactory(connectionForSecondUser) { RefreshEnabled = false };
					((IDbConnected)secondFactory).Connection.BeginTransaction();
					isSecondUserLocked = true;
					new WhsPutawayLocationCacheManager().CreateCache(secondFactory,
						new[] { location1.PK, location3.PK });
					isSecondUserLocked = false;
					((IDbConnected)secondFactory).Connection.CommitTransaction();
				}
			});

			Thread.Sleep(5000);
			AssertEquals("Second user should be currently locked.", true, isSecondUserLocked);
			AssertEquals(true, (factory as IDbConnected).Connection.IsInTransaction);

			((IDbConnected)factory).Connection.CommitTransaction();

			task.Wait();
			AssertEquals("Second user should not be locked anymore.", false, isSecondUserLocked);
			AssertEquals(false, (factory as IDbConnected).Connection.IsInTransaction);

			var locationPksHash = new HashSet<ZGuid>(new[] { location1.PK, location2.PK, location3.PK });
			var cache = new WhsPutawayLocationCacheManager()
				.GetCache(factory, warehouse.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).Where(c =>
					locationPksHash.Contains(((Guid)c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name])))
				.ToArray();
			AssertEquals(3, cache.Length);
			var locationPks = cache.Select(row => row[WhsPutawayLocationCacheSchema.PK.Name])
				.Select(value => value == DBNull.Value ? ZGuid.Empty : new ZGuid((Guid)value));
			AssertCollectionContains(location1.PK, locationPks);
			AssertCollectionContains(location2.PK, locationPks);
			AssertCollectionContains(location3.PK, locationPks);
		}

		#endregion
	}

	class WhsPutawayLocationCacheManagerTest : WhsTestCaseWithFactory
	{
		public void TestGetCache()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse1 = data.Whs1;
			var warehouse2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			Factory.Save();

			var cacheManager = new WhsPutawayLocationCacheManager();
			var locationPks = new[]
			{
				warehouse1.FindLocation("A-1").PK, warehouse1.FindLocation("A-2").PK,
				warehouse2.FindLocation("B-1").PK, warehouse2.FindLocation("B-2").PK
			};
			cacheManager.CreateCache(Factory, locationPks);

			var cache1 = cacheManager.GetCache(Factory, warehouse1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK });
			AssertEquals("Should return 2 WhsPutawayLocationCache.", 2, cache1.Count());
			AssertEquals("Should not contain any other table's columns.", 0,
				cache1.ElementAt(0).Table.Columns.Cast<DataColumn>().Count(pn => !pn.ColumnName.StartsWith("WPC_")));

			AssertContainsExactElementsInAnyOrder("Should include appropriate locations.",
				new[] { warehouse1.FindLocation("A-1").PK, warehouse1.FindLocation("A-2").PK },
				cache1.Select(c => c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]));

			var cache1_FromInterface =
				cacheManager.GetCache(Factory, warehouse1.PK, new[] { data.Org1.PK },
					new[] { data.Part1.PK });
			AssertEquals("Should return 2 WhsPutawayLocationCache.", 2, cache1_FromInterface.Count());
			AssertContainsExactElementsInAnyOrder("Should include appropriate locations.",
				new[] { warehouse1.FindLocation("A-1").PK, warehouse1.FindLocation("A-2").PK },
				cache1_FromInterface.Select(c => c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]));

			var cache2 = cacheManager.GetCache(Factory, warehouse2.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK });
			AssertEquals("Should return 2 WhsPutawayLocationCache.", 2, cache2.Count());
			AssertContainsExactElementsInAnyOrder("Should include appropriate locations.",
				new[] { warehouse2.FindLocation("B-1").PK, warehouse2.FindLocation("B-2").PK },
				cache2.Select(c => c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]));
		}

		public void TestGetCache_ThrowsWithNullFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			Factory.Save();

			var locationPks = new[] { location1.PK, location2.PK };
			new WhsPutawayLocationCacheManager().CreateCache(Factory, locationPks);

			AssertExceptionThrown<ArgumentNullException>(() =>
				new WhsPutawayLocationCacheManager().GetCache(null, warehouse.PK, new[] { data.Org1.PK },
					new[] { data.Part1.PK }));
		}

		public void TestGetCache_UsesSmartParameterisation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse1 = data.Whs1;
			var warehouse2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			Factory.Save();

			var cacheManager = new WhsPutawayLocationCacheManager();
			var locationPks = new[]
			{
				warehouse1.FindLocation("A-1").PK, warehouse1.FindLocation("A-2").PK,
				warehouse2.FindLocation("B-1").PK, warehouse2.FindLocation("B-2").PK
			};

			cacheManager.CreateCache(Factory, locationPks);

			using (TestConnection.TrackExecutedCommands())
			{
				var cache1 = cacheManager.GetCache(Factory, warehouse1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).ToArray();
				AssertEquals("Should return 2 WhsPutawayLocationCache.", 2, cache1.Length);
				AssertEquals("Should not contain any other table's columns.", 0,
					cache1[0].Table.Columns.Cast<DataColumn>().Count(pn => !pn.ColumnName.StartsWith("WPC_")));

				AssertContainsExactElementsInAnyOrder("Should include appropriate locations.",
					new[] { warehouse1.FindLocation("A-1").PK, warehouse1.FindLocation("A-2").PK },
					cache1.Select(c => c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]));

				var selectCommand = TestConnection.ExecutedCommands.SingleOrDefault(c =>
					c.Contains("WPC_WW_Warehouse = @"));
				AssertStartsWith("Smart parameterisation should be used.", @"
SELECT 
	*
FROM
	dbo.WhsPutawayLocationCache
WHERE
	WPC_WW_Warehouse = @WhsPK_NOHISTOGRAM AND
	(WPC_OH_Client IS NULL OR (WPC_OH_Client IN (SELECT Value FROM @ClientPKs))) AND
	(WPC_OP_Product IS NULL OR (WPC_OP_Product IN (SELECT Value FROM @ProductPKs))) AND
	
	(WPC_MaxQuantity = 0 OR WPC_AvailableQuantity > 0)
", selectCommand);
			}
		}

		public void TestWPC_PartialPalletID_MaxLength()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A");
			location.WLV_MaxQuantity = 10m;
			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var maxLengthPalletID = new string('1', WhsDocketLineSchema.WE_PalletID.MaxLength);
			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1_1 =
				Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, location, maxLengthPalletID);
			Factory.Save();
			IReadOnlyList<DataRow> cache1 = null;
			AssertNoExceptionThrown(() => cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK));

			var partialPalletRecord = cache1.Single(c =>
				((string)c[WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name]) ==
				WhsPutawayLocationCacheManager.LocationCacheType.PLT);
			AssertEquals("Should return correct PalletID", maxLengthPalletID,
				partialPalletRecord[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name]);
		}

		public void TestGetCache_IncludesNonPersistedDetails()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var area1 = Helper.CreateArea(data.Whs1, "A", AreaTypes.Codes.FreeStore);
			var area2 = Helper.CreateArea(data.Whs1, "B", AreaTypes.Codes.Excise);
			var location1 = data.Whs1.FindLocation("A");
			location1.WLV_WA_PutawayArea = area1.PK;
			location1.WLV_WA_PickingArea = area2.PK;
			location1.WLV_PutawayPathSequence = 7;
			Factory.Save();

			var cacheManager = new WhsPutawayLocationCacheManager();
			var locationPks = new[] { location1.PK };
			cacheManager.CreateCache(Factory, locationPks);

			var cache = cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK });
			AssertEquals("Should return 1 WhsPutawayLocationCache.", 1, cache.Count());

			var cacheRecord = cache.ElementAt(0);
			AssertEquals(WhsPutawayLocationCacheSchema.WPC_WA_Area.Name, area1.PK,
				cacheRecord[WhsPutawayLocationCacheSchema.WPC_WA_Area.Name]);
			AssertEquals(WhsPutawayLocationCacheSchema.WPC_AreaType.Name, AreaTypes.Codes.FreeStore,
				cacheRecord[WhsPutawayLocationCacheSchema.WPC_AreaType.Name]);
			AssertEquals(WhsPutawayLocationCacheSchema.WPC_PutawaySequence.Name, 7,
				cacheRecord[WhsPutawayLocationCacheSchema.WPC_PutawaySequence.Name]);
		}

		public void TestGetCache_IncludesLocationsWithNoCapacityLimits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cacheManager = new WhsPutawayLocationCacheManager();
			var locationPks = new[] { location.PK };
			cacheManager.CreateCache(Factory, locationPks);

			var cache = cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).ToArray();
			AssertEquals("Should return 1 WhsPutawayLocationCache.", 1, cache.Length);

			AssertEquals("Should return one WhsPutawayLocationCache with a max quantity equal to that of the location.",
				0m, cache[0][WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with a available quantity equal to that of the location.",
				0m, cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);

			AssertEquals("Should return one WhsPutawayLocationCache with a max volume equal to that of the location.",
				0m, cache[0][WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with a available volume equal to that of the location.", 0m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);

			AssertEquals("Should return one WhsPutawayLocationCache with a max weight equal to that of the location.",
				0m, cache[0][WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with a available weight equal to that of the location.", 0m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);

			AssertEquals(
				"Should return one WhsPutawayLocationCache with a pallet count equal to that of the location.", (short)0,
				cache[0][WhsPutawayLocationCacheSchema.WPC_PalletQuantity.Name]);
		}

		public void TestGetCache_FiltersEmptyLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A");
			location.WLV_MaxQuantity = 10m;
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			AssertEquals("Precondition: Should create one WhsPutawayLocationCache.", 1,
				CreateAndGetCache(Factory, data.Whs1.PK, location.PK).Count);

			var cacheManager = new WhsPutawayLocationCacheManager();
			var cache = cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).ToArray();
			AssertEquals("Should return 0 WhsPutawayLocationCache records.", 0, cache.Length);
		}

		public void TestGetCache_FiltersEmptyLocations_PartialPallets()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A");
			location.WLV_MaxQuantity = 10m;
			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1_1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, location, "PLT-123");
			Factory.Save();

			AssertEquals("Precondition: Should create two WhsPutawayLocationCache records.", 2,
				CreateAndGetCache(Factory, data.Whs1.PK, location.PK).Count);

			var cacheManager = new WhsPutawayLocationCacheManager();
			var cache = cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).ToArray();
			AssertEquals("Should return 0 WhsPutawayLocationCache records.", 0, cache.Length);
		}

		public void TestGetCache_FiltersEmptyLocations_PickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, location, replenishMax: 10m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			ConditionalFinaliseWithAssertion(receive, true);
			Factory.Save();

			AssertEquals("Precondition: Should create one WhsPutawayLocationCache.", 1,
				CreateAndGetCache(Factory, data.Whs1.PK, location.PK).Count);

			var cacheManager = new WhsPutawayLocationCacheManager();
			var cache = cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).ToArray();
			AssertEquals("Should return 0 WhsPutawayLocationCache records.", 0, cache.Length);
		}

		public void TestGetCache_DifferentClientsAndProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cacheManager = new WhsPutawayLocationCacheManager();
			var locationPks = new[] { location.PK };
			cacheManager.CreateCache(Factory, locationPks);

			AssertEquals("Should return 1 WhsPutawayLocationCache irrespective of client and product.", 1,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).Count());
			AssertEquals("Should return 1 WhsPutawayLocationCache irrespective of client and product.", 1,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part2.PK }).Count());
			AssertEquals("Should return 1 WhsPutawayLocationCache irrespective of client and product.", 1,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK, data.Part2.PK })
					.Count());
			AssertEquals("Should return 1 WhsPutawayLocationCache irrespective of client and product.", 1,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { ZGuid.NewZGuid() }, new[] { ZGuid.NewZGuid() }).Count());
		}

		public void TestGetCache_DifferentClientsAndProducts_PartialPallet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A");
			location.WLV_MaxQuantity = 10m;
			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1_1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, location, "PLT-123");
			Factory.Save();

			AssertEquals("Precondition: Should create two WhsPutawayLocationCache records.", 2,
				CreateAndGetCache(Factory, data.Whs1.PK, location.PK).Count);

			var cacheManager = new WhsPutawayLocationCacheManager();
			AssertEquals("Should return 2 WhsPutawayLocationCache records.", 2,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).Count());
			AssertEquals("Should return 2 WhsPutawayLocationCache records.", 2,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK, data.Part2.PK })
					.Count());

			var cacheDifferentClient = cacheManager
				.GetCache(Factory, data.Whs1.PK, new[] { ZGuid.NewZGuid() }, new[] { data.Part1.PK }).ToArray();
			AssertEquals("Should filter the partial pallet if the product/client do not match.", 1,
				cacheDifferentClient.Length);
			AssertNotNull("Should filter the partial pallet if the product/client do not match.",
				GetCacheRow(cacheDifferentClient, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
					ZGuid.Empty, ZGuid.Empty));

			var cacheDifferentProduct = cacheManager
				.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { ZGuid.NewZGuid() }).ToArray();
			AssertEquals("Should filter the partial pallet if the product/client do not match.", 1,
				cacheDifferentProduct.Length);
			AssertNotNull("Should filter the partial pallet if the product/client do not match.",
				GetCacheRow(cacheDifferentProduct, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC,
					"", ZGuid.Empty, ZGuid.Empty));
		}

		public void TestGetCache_MultiClientsAndProducts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A");

			var client2 = Helper.CreateClient("C2");
			Helper.CreateProductClientRelationShip(client2, data.Part2);

			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);
			AddUnitConversionForPart(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1_1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, location, "PLT-123");

			var receive2 = Helper.CreateWhsReceive(client2, warehouse);
			var receiveLine2_1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 5m, location, "PLT-456");
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Precondition: Should create three WhsPutawayLocationCache records.", 3,
				CreateAndGetCache(Factory, data.Whs1.PK, location.PK).Count);

			var cacheManager = new WhsPutawayLocationCacheManager();

			AssertEquals("Should return 1 WhsPutawayLocationCache record with no matching client.", 1,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { ZGuid.NewZGuid() }, new[] { data.Part1.PK, data.Part2.PK }).Count());

			AssertEquals("Should return 2 WhsPutawayLocationCache records.", 2,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).Count());

			AssertEquals("Should return 1 WhsPutawayLocationCache record with no matching product.", 1,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part2.PK }).Count());

			AssertEquals("Should return 1 WhsPutawayLocationCache record with no matching product.", 1,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { client2.PK }, new[] { data.Part1.PK }).Count());

			AssertEquals("Should return 2 WhsPutawayLocationCache records.", 2,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { client2.PK }, new[] { data.Part2.PK }).Count());

			AssertEquals("Should return 2 WhsPutawayLocationCache records.", 2,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK, client2.PK }, new[] { data.Part1.PK })
					.Count());

			AssertEquals("Should return 2 WhsPutawayLocationCache records.", 2,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK, client2.PK }, new[] { data.Part2.PK })
					.Count());

			AssertEquals("Should return 3 WhsPutawayLocationCache records.", 3,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK, client2.PK }, new[] { data.Part1.PK, data.Part2.PK })
					.Count());
		}

		public void TestGetCache_DifferentClientsAndProducts_PickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, location, replenishMax: 10m);
			Factory.Save();

			AssertEquals("Precondition: Should create one WhsPutawayLocationCache.", 1,
				CreateAndGetCache(Factory, data.Whs1.PK, location.PK).Count);

			var cacheManager = new WhsPutawayLocationCacheManager();
			AssertEquals("Should return 1 WhsPutawayLocationCache record.", 1,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).Count());
			AssertEquals("Should return 1 WhsPutawayLocationCache record.", 1,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK, data.Part2.PK })
					.Count());

			var cacheDifferentClient = cacheManager
				.GetCache(Factory, data.Whs1.PK, new[] { ZGuid.NewZGuid() }, new[] { data.Part1.PK }).ToArray();
			AssertEquals("Should filter the pick face if the product/client do not match.", 0,
				cacheDifferentClient.Length);

			var cacheDifferentProduct = cacheManager
				.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { ZGuid.NewZGuid() }).ToArray();
			AssertEquals("Should filter the pick face if the product/client do not match.", 0,
				cacheDifferentProduct.Length);
		}

		public void TestGetCache_DifferentClientsAndProducts_DynamicPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var dynamicArea = Helper.CreateArea(warehouse, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DGD", LocationClasses.Codes.DPF);
			location.WLV_WLT_LocationType = dynamicLocationType.PK;
			location.WLV_WA_PickingArea = dynamicArea.PK;
			var whsProduct = WhsProduct.GetWhsProduct(data.Part1);
			AddParamsByWhsAndClient(whsProduct, data.Org1, warehouse, dynamicArea);
			Factory.Save();

			AssertEquals("Precondition: Should create one WhsPutawayLocationCache.", 1,
				CreateAndGetCache(Factory, data.Whs1.PK, location.PK).Count);

			var cacheManager = new WhsPutawayLocationCacheManager();
			AssertEquals("Should return 1 WhsPutawayLocationCache record.", 1,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }).Count());
			AssertEquals("Should return 1 WhsPutawayLocationCache record.", 1,
				cacheManager.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK, data.Part2.PK })
					.Count());

			var cacheDifferentClient = cacheManager
				.GetCache(Factory, data.Whs1.PK, new[] { ZGuid.NewZGuid() }, new[] { data.Part1.PK }).ToArray();
			AssertEquals("Should filter the dynamic pick face if the product/client do not match.", 0,
				cacheDifferentClient.Length);

			var cacheDifferentProduct = cacheManager
				.GetCache(Factory, data.Whs1.PK, new[] { data.Org1.PK }, new[] { ZGuid.NewZGuid() }).ToArray();
			AssertEquals("Should filter the dynamic pick face if the product/client do not match.", 0,
				cacheDifferentProduct.Length);
		}

		public void TestCreateCache_ThrowsWithNullFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			Factory.Save();

			var locationPks = new[] { location1.PK, location2.PK };
			AssertExceptionThrown<ArgumentNullException>(() =>
				new WhsPutawayLocationCacheManager().CreateCache(null, locationPks));
		}

		public void TestCreateCache_SingleLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache.", 1, cache.Count);
		}

		public void TestCreateCache_MultipleLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, new[] { location1.PK, location2.PK });
			AssertEquals("Should return two WhsPutawayLocationCaches.", 2, cache.Count);
		}

		public void TestCreateCache_VoidedLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");

			location.WLV_LocationStatus = LocationStatus.Codes.Void;
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no results.", 0, cache1.Count);
		}

		public void TestCreateCache_DockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.DefaultInboundDockDoorLocation;
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no results.", 0, cache1.Count);
		}

		public void TestCreateCache_PackingStationLocation()
		{
			var packingStationLocationType = Helper.CreateLocationType("PAK", "Packing", false, 0, LocationClasses.Codes.PST);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var packingStation = data.Whs1.FindLocation("A-2");
			packingStation.WLV_WLT_LocationType = packingStationLocationType.PK;
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, packingStation.PK);
			AssertEquals("Should return no results.", 0, cache1.Count);
		}

		public void TestCreateCache_PackingConsolidationLocation()
		{
			var packingConsolidationLocationType = Helper.CreateLocationType("CON", "Consolidation", false, 0, LocationClasses.Codes.CON);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var packingConsolidationLocation = data.Whs1.FindLocation("A-2");
			packingConsolidationLocation.WLV_WLT_LocationType = packingConsolidationLocationType.PK;
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, packingConsolidationLocation.PK);
			AssertEquals("Should return no results.", 0, cache1.Count);
		}

		public void TestCreateCache_FixedLocationWithNoPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;

			var locationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "PFC"));
			var location = warehouse.FindLocation("A-1");
			location.WLV_WLT_LocationType = locationType.PK;
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no results.", 0, cache1.Count);
		}

		public void TestCreateCache_DynamicLocationWithNoPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;

			var location = warehouse.FindLocation("A-1");
			var dynamicArea = Helper.CreateArea(warehouse, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DGD", LocationClasses.Codes.DPF);
			location.WLV_WLT_LocationType = dynamicLocationType.PK;
			location.WLV_WA_PickingArea = dynamicArea.PK;
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no results.", 0, cache1.Count);
		}

		public void TestCreateCache_TransitWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no results.", 0, cache1.Count);
		}

		public void TestCreateCache_ContainerYardWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no results.", 0, cache1.Count);
		}

		public void TestCreateCache_FTZWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return 1 result.", 1, cache1.Count);
		}

		public void TestCreateCache_VirtualWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");

			warehouse.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no results.", 0, cache.Count);
		}

		#region TestCreateCache_CascadeDelete

		public void TestCreateCache_CascadeDelete_Location()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, new[] { location1.PK, location2.PK });
			AssertEquals("Should return two WhsPutawayLocationCaches.", 2, cache1.Count);

			DeleteTableRow(WhsLocation.Schema.TableName, WhsLocation.Schema.PK, location1.PK);

			var cache2 = GetCacheForLocations(Factory, warehouse.PK, new[] { location1.PK, location2.PK });
			AssertEquals("WhsPutawayLocationCache should only return 1 entry.", 1, cache2.Count);
			AssertEquals("Returned entry for cache should be for location2.", location2.PK,
				cache2[0][WhsPutawayLocationCacheSchema.PK.Name]);
		}

		public void TestCreateCache_CascadeDelete_PickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location1 = warehouse.FindLocation("A-1");
			var pickface1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			var location2 = warehouse.FindLocation("A-2");
			var pickface2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location2);
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, new[] { location1.PK, location2.PK });
			AssertEquals("Should return two WhsPutawayLocationCaches.", 2, cache1.Count);

			DeleteTableRow(WhsPickFace.Schema.TableName, WhsPickFace.Schema.PK, pickface1.PK);

			var cache2 = GetCacheForLocations(Factory, warehouse.PK, new[] { location1.PK, location2.PK });
			AssertEquals("WhsPutawayLocationCache should only return 1 entry.", 1, cache2.Count);
			AssertEquals("Returned entry for cache should be for location2.", pickface2.PK,
				cache2[0][WhsPutawayLocationCacheSchema.PK.Name]);
		}

		#endregion

		#region TestCreateCache_ClearsUpStaleData

		public void TestCreateCache_ClearsUpStaleData_PickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, new[] { location1.PK, location2.PK });
			AssertEquals("Should return two WhsPutawayLocationCache.", 2, cache1.Count);
			AssertNotNull("Returned entry for pick face cache.",
				GetCacheRow(cache1, location1.PK, WhsPutawayLocationCacheManager.LocationCacheType.FIX, "",
					data.Part1.PK, data.Org1.PK));
			AssertNotNull("Returned entry for location2.",
				GetCacheRow(cache1, location2.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "", ZGuid.Empty,
					ZGuid.Empty));

			pickFace1.WF_OP = data.Part2.PK;
			Factory.Save();

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, new[] { location1.PK, location2.PK });
			AssertEquals("Should return two WhsPutawayLocationCache.", 2, cache2.Count);
			AssertNotNull("Returned entry for pick face cache.",
				GetCacheRow(cache2, location1.PK, WhsPutawayLocationCacheManager.LocationCacheType.FIX, "",
					data.Part2.PK, data.Org1.PK));
			AssertNotNull("Returned entry for location2.",
				GetCacheRow(cache2, location2.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "", ZGuid.Empty,
					ZGuid.Empty));
		}

		public void TestCreateCache_ClearsUpStaleData_PartialPallet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			SetLocationWeightAndCubicConstraints(location, 30, 60);
			SetPartWeightAndCubicAttributes(data.Part1, 1, 1.5);
			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1_1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, location, "PLT-123");
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return two WhsPutawayLocationCaches.", 2, cache1.Count);

			var pltRow1 = GetCacheRow(cache1, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-123", data.Part1.PK, data.Org1.PK);
			var locRow1 = GetCacheRow(cache1, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);
			AssertNotNull("Cache should include PLT Row", pltRow1);
			AssertNotNull("Cache should include LOC Row", locRow1);

			receiveLine1_1.WI_PalletID = "PLT-456";
			Factory.Save();

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return two WhsPutawayLocationCaches.", 2, cache2.Count);
			var pltRow2 = GetCacheRow(cache2, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-456", data.Part1.PK, data.Org1.PK);
			var locRow2 = GetCacheRow(cache2, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);
			AssertNotNull("Cache should include PLT Row", pltRow2);
			AssertNotNull("Cache should include LOC Row", locRow2);
		}

		#endregion

		#region TestCreateCache_InsertAndUpdateTable

		public void TestCreateCache_UpdatesExistingRow_NormalLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache.", 1, cache1.Count);
			AssertEquals("First row's WPC_PK should be the row the PK that we wanted to add to the cache.", location.PK,
				cache1[0][WhsPutawayLocationCacheSchema.PK.Name]);
			AssertEquals("First row's Location FK should be the row the Location that we wanted to add to the cache.",
				location.PK, cache1[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return still only one WhsPutawayLocationCache.", 1, cache2.Count);
			AssertEquals("First row's WPC_PK should still be the row the PK that we wanted to add to the cache.",
				location.PK, cache2[0][WhsPutawayLocationCacheSchema.PK.Name]);
			AssertEquals("First row's Location FK should be the row the Location that we wanted to add to the cache.",
				location.PK, cache2[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]);
		}

		public void TestCreateCache_UpdatesExistingRow_VoidedLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache.", 1, cache1.Count);
			AssertEquals("First row's WPC_PK should be the row the PK that we wanted to add to the cache.", location.PK,
				cache1[0][WhsPutawayLocationCacheSchema.PK.Name]);
			AssertEquals("First row's Location FK should be the row the Location that we wanted to add to the cache.",
				location.PK, cache1[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]);

			location.WLV_LocationStatus = LocationStatus.Codes.Void;
			Factory.Save();

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no results.", 0, cache2.Count);
		}

		public void TestCreateCache_UpdatesExistingRow_DockDoorLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache.", 1, cache1.Count);
			AssertEquals("First row's WPC_PK should be the row the PK that we wanted to add to the cache.", location.PK,
				cache1[0][WhsPutawayLocationCacheSchema.PK.Name]);
			AssertEquals("First row's Location FK should be the row the Location that we wanted to add to the cache.",
				location.PK, cache1[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]);

			var dockDoorLocationType = Helper.CreateLocationType("XYZ", "Test", false, 0, LocationClasses.Codes.DDL);
			location.WLV_WLT_LocationType = dockDoorLocationType.PK;
			Factory.Save();

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no results.", 0, cache2.Count);
		}

		public void TestCreateCache_UpdatesExistingRow_FixedPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache.", 1, cache1.Count);
			AssertEquals("First row's WPC_PK should be the row the PK that we wanted to add to the cache.", pickface.PK,
				cache1[0][WhsPutawayLocationCacheSchema.PK.Name]);
			AssertEquals("First row's Location FK should be the row the Location that we wanted to add to the cache.",
				location.PK, cache1[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return still only one WhsPutawayLocationCache.", 1, cache2.Count);
			AssertEquals("First row's WPC_PK should still be the row the PK that we wanted to add to the cache.",
				pickface.PK, cache2[0][WhsPutawayLocationCacheSchema.PK.Name]);
			AssertEquals("First row's Location FK should be the row the Location that we wanted to add to the cache.",
				location.PK, cache2[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]);
		}

		public void TestCreateCache_UpdatesExistingRow_FixedPickFace_PickFaceRemoved()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache.", 1, cache1.Count);
			AssertEquals("First row's WPC_PK should be the row the PK that we wanted to add to the cache.", pickface.PK,
				cache1[0][WhsPutawayLocationCacheSchema.PK.Name]);
			AssertEquals("First row's Location FK should be the row the Location that we wanted to add to the cache.",
				location.PK, cache1[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]);

			pickface.Delete();
			Factory.Save();

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no cache records.", 0, cache2.Count);
		}

		public void TestCreateCache_UpdatesExistingRow_DynamicPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var dynamicArea = Helper.CreateArea(warehouse, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DGD", LocationClasses.Codes.DPF);
			location.WLV_WLT_LocationType = dynamicLocationType.PK;
			location.WLV_WA_PickingArea = dynamicArea.PK;
			var whsProduct = WhsProduct.GetWhsProduct(data.Part1);
			AddParamsByWhsAndClient(whsProduct, data.Org1, warehouse, dynamicArea);
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache.", 1, cache1.Count);
			AssertEquals("First row's Location FK should be the row the Location that we wanted to add to the cache.",
				location.PK, cache1[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return still only one WhsPutawayLocationCache.", 1, cache2.Count);
			AssertEquals("First row's Location FK should be the row the Location that we wanted to add to the cache.",
				location.PK, cache2[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]);
		}

		public void TestCreateCache_UpdatesExistingRow_DynamicPickFace_ParamsRemoved()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var dynamicArea = Helper.CreateArea(warehouse, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DGD", LocationClasses.Codes.DPF);
			location.WLV_WLT_LocationType = dynamicLocationType.PK;
			location.WLV_WA_PickingArea = dynamicArea.PK;
			var whsProduct = WhsProduct.GetWhsProduct(data.Part1);
			var productParams = AddParamsByWhsAndClient(whsProduct, data.Org1, warehouse, dynamicArea);
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache.", 1, cache1.Count);
			AssertEquals("First row's Location FK should be the row the Location that we wanted to add to the cache.",
				location.PK, cache1[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]);

			productParams.Delete();
			Factory.Save();

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no WhsPutawayLocationCache.", 0, cache2.Count);
		}

		public void TestCreateCache_UpdatesExistingRow_ChangedToTransitWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache.", 1, cache1.Count);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			Factory.Save();

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no results.", 0, cache2.Count);
		}

		public void TestCreateCache_UpdatesExistingRow_ChangedToContainerYardWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache.", 1, cache1.Count);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			Factory.Save();

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no results.", 0, cache2.Count);
		}

		public void TestCreateCache_UpdatesExistingRow_ChangedToVirtualWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache.", 1, cache1.Count);

			warehouse.WW_IsVirtualWarehouse = true;
			Factory.Save();

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return no results.", 0, cache2.Count);
		}

		public void TestCreateCache_UpdateExistingRow_PartialPallet_Finalised() =>
			TestCreateCache_UpdateExistingRow_PartialPallet(true);

		public void TestCreateCache_UpdateExistingRow_PartialPallet_NotFinalised() =>
			TestCreateCache_UpdateExistingRow_PartialPallet(false);

		void TestCreateCache_UpdateExistingRow_PartialPallet(bool finalised)
		{
			var palletId = "JL2";
			var pa1 = "I";
			var pa2 = "W";
			var pa3 = "T";
			var packingDate = ZDateTime.Now.Date;
			var expiryDate = packingDate.AddDays(-1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.WLV_MaxQuantity = 30m;
			SetLocationWeightAndCubicConstraints(location, 30, 60);
			SetPartWeightAndCubicAttributes(data.Part1, 1, 1.5);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location, palletId, expiryDate, packingDate,
				pa1, pa2, pa3, "");
			ConditionalFinaliseWithAssertion(receive1, finalised);

			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache.", 2, cache1.Count);
			var pltRow1 = GetCacheRow(cache1, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				palletId, data.Part1.PK, data.Org1.PK);
			var locRow1 = GetCacheRow(cache1, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);
			AssertNotNull("Cache should include PLT Row", pltRow1);
			AssertNotNull("Cache should include LOC Row", locRow1);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return still only one WhsPutawayLocationCache.", 2, cache2.Count);
			var pltRow2 = GetCacheRow(cache2, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				palletId, data.Part1.PK, data.Org1.PK);
			var locRow2 = GetCacheRow(cache2, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);
			AssertNotNull("Cache should include PLT Row", pltRow2);
			AssertNotNull("Cache should include LOC Row", locRow2);
		}

		public void TestCreateCache_UpdateExistingRow_PartialPallet_Cancelled()
		{
			var palletId = "JL2";
			var pa1 = "I";
			var pa2 = "W";
			var pa3 = "T";
			var packingDate = ZDateTime.Now.Date;
			var expiryDate = packingDate.AddDays(-1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.WLV_MaxQuantity = 30m;
			SetLocationWeightAndCubicConstraints(location, 30, 60);
			SetPartWeightAndCubicAttributes(data.Part1, 1, 1.5);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, location, palletId, expiryDate, packingDate,
				pa1, pa2, pa3, "");
			Factory.Save();

			receive1.WD_DocketStatus = DocketStatus.Codes.Entered; // hack
			receive1.CancelReactivateDocket();
			AssertEquals("Receive should be successfully cancelled.", true, receive1.IsCancelled);
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache, with no partial pallet.", 1, cache1.Count);

			var locRow1 = GetCacheRow(cache1, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);
			AssertNotNull("Cache should include LOC Row", locRow1);
		}

		public void TestCreateCache_AddLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK);
			AssertEquals("Should return only one WhsPutawayLocationCache.", 1, cache1.Count);
			AssertEquals("First row's WPC_PK should be the row the PK that we wanted to add to the cache.",
				location1.PK, cache1[0][WhsPutawayLocationCacheSchema.PK.Name]);

			new WhsPutawayLocationCacheManager().CreateCache(Factory, location2.PK);
			var cache2 = GetCacheForLocations(Factory, warehouse.PK, new[] { location1.PK, location2.PK });
			AssertEquals("Should return two WhsPutawayLocationCaches.", 2, cache2.Count);
			AssertContainsExactElementsInAnyOrder(new[] { location1.PK, location2.PK },
				new[]
				{
					cache2[0][WhsPutawayLocationCacheSchema.PK.Name],
					cache2[1][WhsPutawayLocationCacheSchema.PK.Name]
				});
		}

		public void TestCreateCache_UpdateStaleDataWithoutRemovingOtherData()
		{
			var expiryDate = ZDateTime.Now.Date.AddDays(-1);
			var packingDate = expiryDate.AddDays(-1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var location1 = data.Whs1.FindLocation("A-1");
			SetLocationWeightAndCubicConstraints(location1, 150, 175);
			var location2 = data.Whs1.FindLocation("A-2");
			SetLocationWeightAndCubicConstraints(location2, 200, 225);
			SetPartWeightAndCubicAttributes(data.Part1, 10, 10);

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 12.5, location1, expiryDate, packingDate, "A",
				"B", "C", "");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 8, location2, expiryDate, packingDate, "A", "B",
				"C", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);

			CreateAndGetCache(Factory, data.Whs1.PK, new ZGuid[] { location1.PK, location2.PK });
			var cache1_1 = GetCacheForLocation(Factory, data.Whs1.PK, location1.PK);
			AssertAllComputedColumns(cache1_1, 12.5, relation.PK, 10, 150, 10, 175, "A", "B", "C", packingDate,
				expiryDate);

			var cache2_1 = GetCacheForLocation(Factory, data.Whs1.PK, location2.PK);
			AssertAllComputedColumns(cache2_1, 8, relation.PK, 10, 200, 10, 225, "A", "B", "C", packingDate,
				expiryDate);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "2");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 0.5, location1, expiryDate,
				packingDate, "A", "B", "C", "");
			receive2.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive2);
			Factory.Save();

			var cache1_2 = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK).ToList();
			AssertAllComputedColumns(cache1_2, 13m, relation.PK, 10, 150, 10, 175, "A", "B", "C", packingDate,
				expiryDate);

			var cache2_2 = GetCacheForLocation(Factory, data.Whs1.PK, location2.PK); // Does not clobber location2
			AssertAllComputedColumns(cache2_2, 8, relation.PK, 10, 200, 10, 225, "A", "B", "C", packingDate,
				expiryDate);
		}

		#endregion

		public void TestCreateCache_DoesNotAggregateOrgPartRelationsBetweenLocations()
		{
			var expiryDate1 = ZDateTime.Now.Date.AddDays(-1);
			var expiryDate2 = expiryDate1.AddDays(-3);
			var packingDate1 = expiryDate1.AddDays(-1);
			var packingDate2 = expiryDate2.AddDays(-1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var location1 = data.Whs1.FindLocation("A-1");
			SetLocationWeightAndCubicConstraints(location1, 150, 175);
			var location2 = data.Whs1.FindLocation("A-2");
			SetLocationWeightAndCubicConstraints(location2, 200, 225);
			SetPartWeightAndCubicAttributes(data.Part1, 10, 10);

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 12.5, location1, expiryDate1, packingDate1, "A",
				"B", "C", "");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 8, location2, expiryDate2, packingDate2, "D",
				"E", "F", "");
			receive1.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive1);
			Factory.Save();

			var relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);

			CreateAndGetCache(Factory, data.Whs1.PK, new ZGuid[] { location1.PK, location2.PK });
			var cache1 = GetCacheForLocation(Factory, data.Whs1.PK, location1.PK);
			AssertAllComputedColumns(cache1, 12.5, relation.PK, 10, 150, 10, 175, "A", "B", "C", packingDate1,
				expiryDate1);

			var cache2 = GetCacheForLocation(Factory, data.Whs1.PK, location2.PK);
			AssertAllComputedColumns(cache2, 8, relation.PK, 10, 200, 10, 225, "D", "E", "F", packingDate2,
				expiryDate2);
		}

		public void TestCreateCache_PK()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with a PK equal to that of the location.",
				location.PK, cache[0][WhsPutawayLocationCacheSchema.PK.Name]);
		}

		public void TestCreateCache_WarehousePK()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with a warehouse pk equal to that of the location.",
				warehouse.PK, cache[0][WhsPutawayLocationCacheSchema.WPC_WW_Warehouse.Name]);
		}

		#region TestCreateCache_LocationAttributes

		public void TestCreateCache_LocationTypeCode_Normal() => TestCreateCache_LocationTypeCode("RNO");

		public void TestCreateCache_LocationTypeCode_NotNormal() => TestCreateCache_LocationTypeCode("ZAK");

		void TestCreateCache_LocationTypeCode(ZString locationTypeCode)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			if (locationTypeCode != "RNO")
			{
				var locationType = Helper.CreateLocationType(locationTypeCode);
				location.WLV_WLT_LocationType = locationType.PK;
			}

			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with a location type equal to that of the location.",
				locationTypeCode, cache[0][WhsPutawayLocationCacheSchema.WPC_LocationTypeCode.Name]);
		}

		public void TestCreateCache_LocationClass_Normal() => TestCreateCache_LocationClass(LocationClasses.Codes.NOR);

		public void TestCreateCache_LocationClass_NotNormal() =>
			TestCreateCache_LocationClass(LocationClasses.Codes.HPL);

		void TestCreateCache_LocationClass(ZString locationClass)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.LocationType.WLT_LocationClass = locationClass;
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with a LocationClass equal to that of the location.",
				locationClass, cache[0][WhsPutawayLocationCacheSchema.WPC_LocationClass.Name]);
		}

		public void TestCreateCache_LocationStatus_Normal() =>
			TestCreateCache_LocationStatus(LocationStatus.Codes.Normal);

		public void TestCreateCache_LocationStatus_NotNormal() =>
			TestCreateCache_LocationStatus(LocationStatus.Codes.Damaged);

		void TestCreateCache_LocationStatus(ZString locationStatus)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.WLV_LocationStatus = locationStatus;
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with the LocationStatus equal to that of the location.",
				locationStatus, cache[0][WhsPutawayLocationCacheSchema.WPC_LocationStatus.Name]);
		}

		public void TestCreateCache_AreaName()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with the putaway area name equal to that of the location.",
				location.PutawayArea.WA_Name, cache[0][WhsPutawayLocationCacheSchema.WPC_AreaName.Name]);
		}

		public void TestCreateCache_RowName()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the row name equal to that of the location.",
				location.Row.WR_Name, cache[0][WhsPutawayLocationCacheSchema.WPC_RowName.Name]);
		}

		public void TestCreateCache_Column_Default() => TestCreateCache_Column("A-1");

		public void TestCreateCache_Column_NotDefault() => TestCreateCache_Column("A-2");

		void TestCreateCache_Column(string locStr)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation(locStr);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				location.WLV_Column, cache[0][WhsPutawayLocationCacheSchema.WPC_Column.Name]);
		}

		public void TestCreateCache_Column_ZeroBased()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var warehouse = data.Whs1;
			warehouse.WW_LocationColumnsZeroBased = true;
			Factory.Save();

			var location1 = warehouse.FindLocation("A-0");
			var location2 = warehouse.FindLocation("A-1");

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)0, cache1[0][WhsPutawayLocationCacheSchema.WPC_Column.Name]);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location2.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)1, cache2[0][WhsPutawayLocationCacheSchema.WPC_Column.Name]);
		}

		public void TestCreateCache_Column_Alphabetical()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var warehouse = data.Whs1;
			warehouse.WW_LocationColumnsAlpha = true;
			Factory.Save();

			var location1 = warehouse.FindLocation("A-A");
			var location2 = warehouse.FindLocation("A-B");

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)1, cache1[0][WhsPutawayLocationCacheSchema.WPC_Column.Name]);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location2.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)2, cache2[0][WhsPutawayLocationCacheSchema.WPC_Column.Name]);
		}

		public void TestCreateCache_Level_Default() => TestCreateCache_Level("A-1");

		public void TestCreateCache_Level_NotDefault() => TestCreateCache_Level("A-2-2");

		void TestCreateCache_Level(string locStr)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation(locStr);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the Level equal to that of the location.",
				location.WLV_Level, cache[0][WhsPutawayLocationCacheSchema.WPC_Level.Name]);
		}

		public void TestCreateCache_Level_ZeroBased()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var warehouse = data.Whs1;
			warehouse.WW_LocationLevelsZeroBased = true;
			Factory.Save();

			var location1 = warehouse.FindLocation("A-1-0");
			var location2 = warehouse.FindLocation("A-1-1");

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)0, cache1[0][WhsPutawayLocationCacheSchema.WPC_Level.Name]);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location2.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)1, cache2[0][WhsPutawayLocationCacheSchema.WPC_Level.Name]);
		}

		public void TestCreateCache_Level_Alphabetical()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var warehouse = data.Whs1;
			warehouse.WW_LocationLevelsAlpha = true;
			Factory.Save();

			var location1 = warehouse.FindLocation("A-1-A");
			var location2 = warehouse.FindLocation("A-1-B");

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)1, cache1[0][WhsPutawayLocationCacheSchema.WPC_Level.Name]);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location2.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)2, cache2[0][WhsPutawayLocationCacheSchema.WPC_Level.Name]);
		}

		public void TestCreateCache_Tray_Default() => TestCreateCache_Tray("A-1");

		public void TestCreateCache_Tray_NotDefault() => TestCreateCache_Tray("B-5-5-2");

		void TestCreateCache_Tray(string locStr)
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var warehouse = data.Whs1;
			Helper.CreateRow(warehouse, "B", 5, 5, 5);
			((IWhsWarehouseInternals)warehouse).GenerateLocations();
			Factory.Save();
			var location = warehouse.FindLocation(locStr);

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the Tray equal to that of the location.",
				location.WLV_Tray, cache[0][WhsPutawayLocationCacheSchema.WPC_Tray.Name]);
		}

		public void TestCreateCache_Tray_ZeroBased()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			warehouse.WW_LocationTraysZeroBased = true;
			Helper.CreateRow(warehouse, "B", 5, 5, 5);
			((IWhsWarehouseInternals)warehouse).GenerateLocations();
			Factory.Save();

			var location1 = warehouse.FindLocation("B-1-1-0");
			var location2 = warehouse.FindLocation("B-1-1-1");

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)0, cache1[0][WhsPutawayLocationCacheSchema.WPC_Tray.Name]);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location2.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)1, cache2[0][WhsPutawayLocationCacheSchema.WPC_Tray.Name]);
		}

		public void TestCreateCache_Tray_Alphabetical()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			warehouse.WW_LocationTraysAlpha = true;
			Helper.CreateRow(warehouse, "B", 5, 5, 5);
			((IWhsWarehouseInternals)warehouse).GenerateLocations();
			Factory.Save();

			var location1 = warehouse.FindLocation("B-1-1-A");
			var location2 = warehouse.FindLocation("B-1-1-B");

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)1, cache1[0][WhsPutawayLocationCacheSchema.WPC_Tray.Name]);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location2.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)2, cache2[0][WhsPutawayLocationCacheSchema.WPC_Tray.Name]);
		}

		public void TestCreateCache_WL_Location_Single() => TestCreateCache_WL_Location(false);

		public void TestCreateCache_WL_Location_Multiple() => TestCreateCache_WL_Location(true);

		void TestCreateCache_WL_Location(bool many)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			Factory.Save();

			var locationPks = many ? new[] { location1.PK, location2.PK } : new[] { location1.PK };

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, locationPks);
			var gotPks = many
				? new[]
				{
					cache[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name],
					cache[1][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]
				}
				: new[] { cache[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name] };
			AssertContainsExactElementsInAnyOrder(
				"Should return WhsPutawayLocationCache with the WL_Location(s) set to the correct location pk(s).",
				locationPks, gotPks);
		}

		public void TestCreateCache_WF_PickFace_NotPickFace() => TestCreateCache_WF_PickFace(false, false);

		public void TestCreateCache_WF_PickFace_Single() => TestCreateCache_WF_PickFace(true, false);

		public void TestCreateCache_WF_PickFace_Multiple() => TestCreateCache_WF_PickFace(true, true);

		void TestCreateCache_WF_PickFace(bool isPickFace, bool many)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			var pickFace1Pk = ZGuid.Empty;
			var pickFace2Pk = ZGuid.Empty;
			if (isPickFace)
			{
				var pickFace1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
				var pickFace2 = Helper.CreateProductPickFace(data.Part1, data.Org1, location2);
				pickFace1Pk = pickFace1.PK;
				pickFace2Pk = pickFace2.PK;
			}

			Factory.Save();

			var locationPks = many ? new[] { location1.PK, location2.PK } : new[] { location1.PK };
			var pickFacePks = many ? new[] { pickFace1Pk, pickFace2Pk } : new[] { pickFace1Pk };

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, locationPks);
			var gotPks = many
				? new[]
				{
					cache[0][WhsPutawayLocationCacheSchema.WPC_WF_PickFace.Name],
					cache[1][WhsPutawayLocationCacheSchema.WPC_WF_PickFace.Name]
				}
				: new[] { cache[0][WhsPutawayLocationCacheSchema.WPC_WF_PickFace.Name] };
			AssertContainsExactElementsInAnyOrder(
				"Should return WhsPutawayLocationCache with the WF_PickFace(s) set to the correct pickface pk(s).",
				pickFacePks, gotPks.Select(GetZGuid));
		}

		public void TestCreateCache_NoCapacityContraints()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with a max quantity equal to that of the location.",
				0m, cache[0][WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with a available quantity equal to that of the location.",
				0m, cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);

			AssertEquals("Should return one WhsPutawayLocationCache with a max volume equal to that of the location.",
				0m, cache[0][WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with a available volume equal to that of the location.", 0m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);

			AssertEquals("Should return one WhsPutawayLocationCache with a max weight equal to that of the location.",
				0m, cache[0][WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with a available weight equal to that of the location.", 0m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);

			AssertEquals(
				"Should return one WhsPutawayLocationCache with a pallet count equal to that of the location.", (short)0,
				cache[0][WhsPutawayLocationCacheSchema.WPC_PalletQuantity.Name]);
		}

		public void TestCreateCache_MaxQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.WLV_MaxQuantity = 10m;
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with a max quantity equal to that of the location.",
				10m, cache[0][WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
		}

		public void TestCreateCache_MaxWeight()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			SetLocationWeightConstraints(location, 10);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with a max weight equal to that of the location.",
				10m, cache[0][WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
		}

		public void TestCreateCache_WeightUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.WLV_MaxWeightUnit = "g";
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with a max weight unit equal to that of the location.", "g",
				cache[0][WhsPutawayLocationCacheSchema.WPC_WeightUnit.Name]);
		}

		public void TestCreateCache_MaxVolume()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			SetLocationCubicConstraints(location, 10);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with a max volume equal to that of the location.",
				10m, cache[0][WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
		}

		public void TestCreateCache_PalletSpaces()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.WLV_PalletFloorSpaces = 5;
			location.WLV_PalletStackHeight = 6;
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with a pallet spaces equal to that of the location.", (short)30,
				cache[0][WhsPutawayLocationCacheSchema.WPC_PalletSpaces.Name]);
		}

		public void TestCreateCache_PalletSpacesExceedSmallInt()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.WLV_PalletFloorSpaces = 200;
			location.WLV_PalletStackHeight = 200;
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with 32767", (short)32767,
				cache[0][WhsPutawayLocationCacheSchema.WPC_PalletSpaces.Name]);
		}

		public void TestCreateCache_PalletQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_PalletFloorSpaces = 5;
			location.WLV_PalletStackHeight = 6;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8, location, "PLT0001");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the pallet count equal to 1.", (short)1,
				cache[0][WhsPutawayLocationCacheSchema.WPC_PalletQuantity.Name]);
		}

		public void TestCreateCache_VolumeUnit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.WLV_MaxCubicUnit = "cc";
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with a max volume unit equal to that of the location.", "cc",
				cache[0][WhsPutawayLocationCacheSchema.WPC_VolumeUnit.Name]);
		}

		public void TestCreateCache_PutawaySequence()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A");
			location.WLV_PutawayPathSequence = 42;
			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, location, "PLT-123");
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Precondition: Should create two WhsPutawayLocationCache records.", 2, cache.Count);
			AssertEquals("Should return correct putaway sequence.", 42,
				cache[0][WhsPutawayLocationCacheSchema.WPC_PutawaySequence.Name]);
			AssertEquals("Should return correct putaway sequence.", 42,
				cache[1][WhsPutawayLocationCacheSchema.WPC_PutawaySequence.Name]);
		}

		public void TestCreateCache_Area()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var warehouse = data.Whs1;
			var bondedArea = Helper.CreateArea(warehouse, "B", AreaTypes.Codes.Bonded);
			var exciseArea = Helper.CreateArea(warehouse, "E", AreaTypes.Codes.Excise);

			var location = warehouse.FindLocation("A");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			location.WLV_WA_PickingArea = exciseArea.PK;
			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, location, "PLT-123");
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Precondition: Should create two WhsPutawayLocationCache records.", 2, cache.Count);
			AssertEquals("Should return correct area FK.", bondedArea.PK,
				cache[0][WhsPutawayLocationCacheSchema.WPC_WA_Area.Name]);
			AssertEquals("Should return correct area FK.", bondedArea.PK,
				cache[1][WhsPutawayLocationCacheSchema.WPC_WA_Area.Name]);
		}

		public void TestCreateCache_AreaType()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var warehouse = data.Whs1;
			var bondedArea = Helper.CreateArea(warehouse, "B", AreaTypes.Codes.Bonded);
			var exciseArea = Helper.CreateArea(warehouse, "E", AreaTypes.Codes.Excise);

			var location = warehouse.FindLocation("A");
			location.WLV_WA_PutawayArea = bondedArea.PK;
			location.WLV_WA_PickingArea = exciseArea.PK;
			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, location, "PLT-123");
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Precondition: Should create two WhsPutawayLocationCache records.", 2, cache.Count);
			AssertEquals("Should return correct area type.", AreaTypes.Codes.Bonded,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AreaType.Name]);
			AssertEquals("Should return correct area type.", AreaTypes.Codes.Bonded,
				cache[1][WhsPutawayLocationCacheSchema.WPC_AreaType.Name]);
		}

		#endregion

		#region TestCreateCache_LocationCacheType

		public void TestCreateCache_LocationCacheType_LOC()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Cache must return location that is just a location.",
				WhsPutawayLocationCacheManager.LocationCacheType.LOC,
				cache[0][WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name]);
		}

		public void TestCreateCache_LocationCacheType_FIX()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Cache must return location that is a fixed pick face.",
				WhsPutawayLocationCacheManager.LocationCacheType.FIX,
				cache[0][WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name]);
		}

		public void TestCreateCache_LocationCacheType_FIX_UserDefined()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var fixedLocationType = Helper.CreateLocationType("UFI", "User Fixed", false, 1, LocationClasses.Codes.FIX);
			var location = warehouse.FindLocation("A-1");
			location.WLV_WLT_LocationType = fixedLocationType.PK;
			var pickFace = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Cache must return location that is a fixed pick face.",
				WhsPutawayLocationCacheManager.LocationCacheType.FIX,
				cache[0][WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name]);
		}

		enum LocationCacheTypeDYNFlags
		{
			HasDynamicProduct = 0b_001,
			HasDynamicArea = 0b_010,
			LocationTypeIsDynamic = 0b_100
		}

		public void TestCreateCache_LocationCacheType_DYN_DPFLocationType_HasDynamicArea_HasDynamicProduct() =>
			TestCreateCache_LocationCacheType_DYN(LocationCacheTypeDYNFlags.LocationTypeIsDynamic |
													LocationCacheTypeDYNFlags.HasDynamicArea |
													LocationCacheTypeDYNFlags.HasDynamicProduct);

		public void TestCreateCache_LocationCacheType_DYN_DPFLocationType_HasDynamicArea_NotHasDynamicProduct() =>
			TestCreateCache_LocationCacheType_DYN(LocationCacheTypeDYNFlags.LocationTypeIsDynamic |
													LocationCacheTypeDYNFlags.HasDynamicArea);

		public void TestCreateCache_LocationCacheType_DYN_DPFLocationType_NotHasDynamicArea_HasDynamicProduct() =>
			TestCreateCache_LocationCacheType_DYN(LocationCacheTypeDYNFlags.LocationTypeIsDynamic |
													LocationCacheTypeDYNFlags.HasDynamicProduct);

		void TestCreateCache_LocationCacheType_DYN(LocationCacheTypeDYNFlags flags)
		{
			bool isLocationTypeDynamic = (flags & LocationCacheTypeDYNFlags.LocationTypeIsDynamic) ==
										 LocationCacheTypeDYNFlags.LocationTypeIsDynamic;
			bool hasDynamicArea = (flags & LocationCacheTypeDYNFlags.HasDynamicArea) ==
									LocationCacheTypeDYNFlags.HasDynamicArea;
			bool hasDynamicProduct = (flags & LocationCacheTypeDYNFlags.HasDynamicProduct) ==
									 LocationCacheTypeDYNFlags.HasDynamicProduct;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var dynamicArea = Helper.CreateArea(warehouse, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var normalLocationType = Helper.CreateLocationType("JVH", LocationClasses.Codes.NOR);
			var dynamicLocationType = Helper.CreateLocationType("DGD", LocationClasses.Codes.DPF);

			var location = warehouse.FindLocation("A-1");
			location.WLV_WLT_LocationType = isLocationTypeDynamic ? dynamicLocationType.PK : normalLocationType.PK;
			if (hasDynamicArea)
			{
				location.WLV_WA_PickingArea = dynamicArea.PK;
			}

			if (hasDynamicProduct)
			{
				var whsProduct = WhsProduct.GetWhsProduct(data.Part1);
				AddParamsByWhsAndClient(whsProduct, data.Org1, warehouse, dynamicArea);
			}

			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			if (isLocationTypeDynamic && hasDynamicArea && hasDynamicProduct)
			{
				AssertEquals($"Cache should return a row that is a DynamicPickFace.",
					WhsPutawayLocationCacheManager.LocationCacheType.DYN,
					cache[0][WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name]);
			}
			else if (!isLocationTypeDynamic)
			{
				AssertEquals("Cache should return single row for location that is not a Dynamic Pick Face.", 1,
					cache.Count);
				AssertEquals("Returned cache row should not be a Dynamic Pick Face.",
					WhsPutawayLocationCacheManager.LocationCacheType.LOC,
					cache[0][WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name]);
			}
			else
			{
				AssertEquals("Cache should not return rows for invalid DynamicPickFaces.", 0, cache.Count);
			}
		}

		enum LocationCacheTypePLTFlags
		{
			Finalised = 0b_0000_0001,
			MultipleSameRelationDocketLines = 0b_0000_0010,
			RegularLocation = 0b_0000_0100,
			NotFull = 0b_0000_1000,
			HasConversion = 0b_0001_0000,
			HomogenousClients = 0b_0010_0000,
			HomogenousParts = 0b_0100_0000,
			HasPalletID = 0b_1000_0000,
			Valid = 0b_1111_1100
		}

		public void TestCreateCache_LocationCacheType_PLT_NotFinalised_NotHomogenousParts() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Valid ^
													LocationCacheTypePLTFlags.HomogenousParts);

		public void TestCreateCache_LocationCacheType_PLT_NotFinalised_NoPalletID() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Valid ^
													LocationCacheTypePLTFlags.HasPalletID);

		public void TestCreateCache_LocationCacheType_PLT_NotFinalised_NotHomogenousClients() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Valid ^
													LocationCacheTypePLTFlags.HomogenousClients);

		public void TestCreateCache_LocationCacheType_PLT_NotFinalised_NoConversion() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Valid ^
													LocationCacheTypePLTFlags.HasConversion);

		public void TestCreateCache_LocationCacheType_PLT_NotFinalised_IsFull() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Valid ^ LocationCacheTypePLTFlags.NotFull);

		public void TestCreateCache_LocationCacheType_PLT_NotFinalised_IsFull_MultipleSameRelationDocketLines() =>
			TestCreateCache_LocationCacheType_PLT(
				(LocationCacheTypePLTFlags.Valid ^ LocationCacheTypePLTFlags.NotFull) |
				LocationCacheTypePLTFlags.HomogenousParts);

		public void TestCreateCache_LocationCacheType_PLT_NotFinalised_NotRegularLocations() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Valid ^
													LocationCacheTypePLTFlags.RegularLocation);

		public void TestCreateCache_LocationCacheType_PLT_NotFinalised_Valid() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Valid);

		public void TestCreateCache_LocationCacheType_PLT_NotFinalised_Valid_MultipleSameRelationDocketLines() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Valid |
													LocationCacheTypePLTFlags.MultipleSameRelationDocketLines);

		public void TestCreateCache_LocationCacheType_PLT_Finalised_NotHomogenousParts() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Finalised |
													(LocationCacheTypePLTFlags.Valid ^
													 LocationCacheTypePLTFlags.HomogenousParts));

		public void TestCreateCache_LocationCacheType_PLT_Finalised_NoPalletID() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Finalised |
													(LocationCacheTypePLTFlags.Valid ^
													 LocationCacheTypePLTFlags.HasPalletID));

		public void TestCreateCache_LocationCacheType_PLT_Finalised_NotHomogenousClients() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Finalised |
													(LocationCacheTypePLTFlags.Valid ^
													 LocationCacheTypePLTFlags.HomogenousClients));

		public void TestCreateCache_LocationCacheType_PLT_Finalised_NoConversion() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Finalised |
													(LocationCacheTypePLTFlags.Valid ^
													 LocationCacheTypePLTFlags.HasConversion));

		public void TestCreateCache_LocationCacheType_PLT_Finalised_IsFull() => TestCreateCache_LocationCacheType_PLT(
			LocationCacheTypePLTFlags.Finalised |
			(LocationCacheTypePLTFlags.Valid ^ LocationCacheTypePLTFlags.NotFull));

		public void TestCreateCache_LocationCacheType_PLT_Finalised_IsFull_MultipleSameRelationDocketLines() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Finalised |
													(LocationCacheTypePLTFlags.Valid ^
													 LocationCacheTypePLTFlags.NotFull) |
													LocationCacheTypePLTFlags.MultipleSameRelationDocketLines);

		public void TestCreateCache_LocationCacheType_PLT_Finalised_Valid() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Finalised |
													LocationCacheTypePLTFlags.Valid);

		public void TestCreateCache_LocationCacheType_PLT_Finalised_Valid_MultipleSameRelationDocketLines() =>
			TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags.Finalised |
													LocationCacheTypePLTFlags.Valid | LocationCacheTypePLTFlags
														.MultipleSameRelationDocketLines);

		void TestCreateCache_LocationCacheType_PLT(LocationCacheTypePLTFlags flags)
		{
			bool hasPalletId = (flags & LocationCacheTypePLTFlags.HasPalletID) == LocationCacheTypePLTFlags.HasPalletID;
			bool homogenousParts = (flags & LocationCacheTypePLTFlags.HomogenousParts) ==
									 LocationCacheTypePLTFlags.HomogenousParts;
			bool homogenousClients = (flags & LocationCacheTypePLTFlags.HomogenousClients) ==
									 LocationCacheTypePLTFlags.HomogenousClients;
			bool hasConversion = (flags & LocationCacheTypePLTFlags.HasConversion) ==
								 LocationCacheTypePLTFlags.HasConversion;
			bool notFull = (flags & LocationCacheTypePLTFlags.NotFull) == LocationCacheTypePLTFlags.NotFull;
			bool isRegularLocation = (flags & LocationCacheTypePLTFlags.RegularLocation) ==
									 LocationCacheTypePLTFlags.RegularLocation;
			bool multipleSameRelationDocketLines =
				(flags & LocationCacheTypePLTFlags.MultipleSameRelationDocketLines) ==
				LocationCacheTypePLTFlags.MultipleSameRelationDocketLines;
			bool finalised = (flags & LocationCacheTypePLTFlags.Finalised) == LocationCacheTypePLTFlags.Finalised;
			var validPLT = (flags & LocationCacheTypePLTFlags.Valid) == LocationCacheTypePLTFlags.Valid;

			var palletId = "JL2";
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			SetLocationWeightAndCubicConstraints(location, 30, 60);
			SetPartWeightAndCubicAttributes(data.Part1, 1, 1.5);

			if (!isRegularLocation)
			{
				Helper.CreateProductPickFace(data.Part1, data.Org1, location, replenishMax: 1000m);
			}

			if (hasConversion)
			{
				AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);
			}

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1_1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1,
				(!notFull && !multipleSameRelationDocketLines) ? 15 : 5, location);
			receiveLine1_1.WI_WL = location.PK;
			if (hasPalletId)
			{
				receiveLine1_1.WI_PalletID = palletId;
			}

			if (multipleSameRelationDocketLines)
			{
				var receiveLine1_1_2 =
					Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, notFull ? 5 : 10, location, palletId);
			}

			if (!homogenousParts)
			{
				var receiveLine1_2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 5, location, palletId);
				AddUnitConversionForPart(data.Part2, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);
			}

			ConditionalFinaliseWithAssertion(receive1, finalised);

			if (!homogenousClients)
			{
				var org2 = Helper.CreateClient("REM");
				var relation =
					Helper.CreateProductClientRelationShip(org2, data.Part1, OrgPartRelation.RelationshipTypes.Owner);
				var receive2 = Helper.CreateWhsReceive(org2, warehouse);
				var receiveLine2_1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5, location, palletId);
				ConditionalFinaliseWithAssertion(receive2, finalised);
			}

			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			var locRow = GetCacheRow(cache, location.PK,
				isRegularLocation
					? WhsPutawayLocationCacheManager.LocationCacheType.LOC
					: WhsPutawayLocationCacheManager.LocationCacheType.FIX, "",
				isRegularLocation ? ZGuid.Empty : data.Part1.PK, isRegularLocation ? ZGuid.Empty : data.Org1.PK);
			AssertEquals("Putaway Location Cache should return the right number of rows.", 1 + validPLT.ToInt(),
				cache.Count);
			AssertEquals("Putaway Location Cache should return the right type of row.",
				isRegularLocation
					? WhsPutawayLocationCacheManager.LocationCacheType.LOC
					: WhsPutawayLocationCacheManager.LocationCacheType.FIX,
				locRow[WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name]);
			if (validPLT)
			{
				var pltRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
					palletId, data.Part1.PK, data.Org1.PK);
				AssertEquals("Putaway Location Cache should return the right type of row.",
					WhsPutawayLocationCacheManager.LocationCacheType.PLT,
					pltRow[WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name]);
				AssertEquals(
					"Putaway Location Cache should return a row with a Partial Pallet ID for a valid Partial Pallet, otherwise nothing.",
					palletId, pltRow[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name]);
			}
		}

		#endregion

		#region TestCreateCache_PartialPallet

		enum PartialPalletSingleFlags
		{
			Finalised = 0b_0001,
			SamePartAttributes = 0b_0010,
			MultipleSameRelationDocketLines = 0b_0100,
			MultiLegConversions = 0b_1000
		}

		public void TestCreateCache_PartialPallet_SinglePallet_SingleDocketLine_Finalised() =>
			TestCreateCache_PartialPallet_SinglePallet(PartialPalletSingleFlags.SamePartAttributes |
														 PartialPalletSingleFlags.Finalised);

		public void TestCreateCache_PartialPallet_SinglePallet_ManyDocketLines_SameAttributes_Finalised() =>
			TestCreateCache_PartialPallet_SinglePallet(PartialPalletSingleFlags.MultipleSameRelationDocketLines |
														 PartialPalletSingleFlags.SamePartAttributes |
														 PartialPalletSingleFlags.Finalised);

		public void TestCreateCache_PartialPallet_SinglePallet_ManyDocketLines_DifferentAttributes_Finalised() =>
			TestCreateCache_PartialPallet_SinglePallet(PartialPalletSingleFlags.MultipleSameRelationDocketLines |
														 PartialPalletSingleFlags.Finalised);

		public void
			TestCreateCache_PartialPallet_SinglePallet_SingleDocketLine_SameAttributes_MultiLegConversions_Finalised() =>
			TestCreateCache_PartialPallet_SinglePallet(PartialPalletSingleFlags.MultiLegConversions |
														 PartialPalletSingleFlags.Finalised |
														 PartialPalletSingleFlags.SamePartAttributes);

		public void TestCreateCache_PartialPallet_SinglePallet_SingleDocketLine_NotFinalised() =>
			TestCreateCache_PartialPallet_SinglePallet(PartialPalletSingleFlags.SamePartAttributes);

		public void TestCreateCache_PartialPallet_SinglePallet_ManyDocketLines_SameAttributes_NotFinalised() =>
			TestCreateCache_PartialPallet_SinglePallet(PartialPalletSingleFlags.MultipleSameRelationDocketLines |
														 PartialPalletSingleFlags.SamePartAttributes);

		public void TestCreateCache_PartialPallet_SinglePallet_ManyDocketLines_DifferentAttributes_NotFinalised() =>
			TestCreateCache_PartialPallet_SinglePallet(PartialPalletSingleFlags.MultipleSameRelationDocketLines);

		public void
			TestCreateCache_PartialPallet_SinglePallet_SingleDocketLine_SameAttributes_MultiLegConversions_NotFinalised() =>
			TestCreateCache_PartialPallet_SinglePallet(PartialPalletSingleFlags.MultiLegConversions |
														 PartialPalletSingleFlags.SamePartAttributes);

		void TestCreateCache_PartialPallet_SinglePallet(PartialPalletSingleFlags flags)
		{
			bool multiLegConversions = (flags & PartialPalletSingleFlags.MultiLegConversions) ==
										 PartialPalletSingleFlags.MultiLegConversions;
			bool multipleSameRelationDocketLines = (flags & PartialPalletSingleFlags.MultipleSameRelationDocketLines) ==
													 PartialPalletSingleFlags.MultipleSameRelationDocketLines;
			bool samePartAttributes = (flags & PartialPalletSingleFlags.SamePartAttributes) ==
										PartialPalletSingleFlags.SamePartAttributes;
			bool finalised = (flags & PartialPalletSingleFlags.Finalised) == PartialPalletSingleFlags.Finalised;
			var palletId = "JL2";
			var pa1_1 = "I";
			var pa2_1 = "W";
			var pa3_1 = "T";
			var packingDate1 = ZDateTime.Now.Date;
			var expiryDate1 = packingDate1.AddDays(-1);
			var pa1_2 = samePartAttributes ? pa1_1 : "K";
			var pa2_2 = samePartAttributes ? pa2_1 : "M";
			var pa3_2 = samePartAttributes ? pa3_1 : "S";
			var packingDate2 = samePartAttributes ? packingDate1 : expiryDate1.AddDays(-1);
			var expiryDate2 = samePartAttributes ? expiryDate1 : packingDate2.AddDays(-1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.WLV_MaxQuantity = 30m;
			SetLocationWeightAndCubicConstraints(location, 30, 60);
			SetPartWeightAndCubicAttributes(data.Part1, 1, 1.5);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Factory.Save();

			if (multiLegConversions)
			{
				AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Box, 5m);
				AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Box, Constants.PkgUnit.Pallet, 3m);
			}
			else
			{
				AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);
			}

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location, palletId, expiryDate1, packingDate1,
				pa1_1, pa2_1, pa3_1, "");
			if (multipleSameRelationDocketLines)
			{
				Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location, palletId, expiryDate2,
					packingDate2, pa1_2, pa2_2, pa3_2, "");
			}

			ConditionalFinaliseWithAssertion(receive1, finalised);

			Factory.Save();

			var relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Putaway Location Cache should return the right number of rows.", 2, cache.Count);
			AssertContainsExactElementsInAnyOrder("Putaway Location Cache should return the right row values.",
				new ZString[]
				{
					WhsPutawayLocationCacheManager.LocationCacheType.LOC,
					WhsPutawayLocationCacheManager.LocationCacheType.PLT
				},
				GetCacheValuesInColumn<string>(cache, WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name));
			var pltRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT, palletId,
				data.Part1.PK, data.Org1.PK);
			var locRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);
			AssertEquals(
				"Putaway Location Cache should return a row with a Partial Pallet ID for a valid Partial Pallet, otherwise nothing.",
				palletId, pltRow[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name]);
			AssertEquals("Putaway Location Cache should return a row with type LOC and no Partial Pallet ID.", "",
				locRow[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name]);

			var expectedPa1 = samePartAttributes ? pa1_1 : "";
			var expectedPa2 = samePartAttributes ? pa2_1 : "";
			var expectedPa3 = samePartAttributes ? pa3_1 : "";
			ZDate? expectedExpiryDate = null;
			ZDate? expectedPackingDate = null;
			if (samePartAttributes)
			{
				expectedExpiryDate = expiryDate1;
				expectedPackingDate = packingDate1;
			}

			AssertEquals("Putaway Location Cache should return a PLT row with correct available weight.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max weight.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max weight unit.", string.Empty,
				pltRow[WhsPutawayLocationCacheSchema.WPC_WeightUnit.Name]);

			AssertEquals("Putaway Location Cache should return a PLT row with correct available volume.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max volume.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max volume unit.", string.Empty,
				pltRow[WhsPutawayLocationCacheSchema.WPC_VolumeUnit.Name]);

			var units = multipleSameRelationDocketLines ? 10 : 5;
			AssertEquals("Putaway Location Cache should return a PLT row with correct available quantity.", 15m - units,
				pltRow[WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max quantity.", 1 * 15m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
			AssertAllComputedColumns(pltRow, units, relation.PK, 1m, units, 1.5m, 1.5m * units, expectedPa1,
				expectedPa2, expectedPa3, expectedPackingDate, expectedExpiryDate,
				expectedNumberOfAttributes: 2 - samePartAttributes.ToInt(), expectedNumberOfRelations: 1);

			AssertEquals("Putaway Location Cache should return a LOC row with correct available quantity.", 30m - units,
				locRow[WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max quantity.", 30m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight.", 30m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max volume.", 60m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
			AssertAllComputedColumns(locRow, units, relation.PK, 30m - units, 60m - (units * 1.5m), expectedPa1,
				expectedPa2, expectedPa3, expectedPackingDate, expectedExpiryDate, 2 - samePartAttributes.ToInt());
		}

		enum PartialPalletMultiplePalletFlags
		{
			None = 0b_0000,
			Finalised = 0b_0001,
			SamePartAttributes = 0b_0010,
			SecondPalletIsValid = 0b_0100,
			SecondPalletHasNoId = 0b_1000
		}

		public void TestCreateCache_PartialPallet_MultiplePallet_SecondIsValid_SameAttributes_Finalised() =>
			TestCreateCache_PartialPallet_MultiplePallet(PartialPalletMultiplePalletFlags.SecondPalletIsValid |
														 PartialPalletMultiplePalletFlags.SamePartAttributes |
														 PartialPalletMultiplePalletFlags.Finalised);

		public void TestCreateCache_PartialPallet_MultiplePallet_SecondIsValid_DifferentAttributes_Finalised() =>
			TestCreateCache_PartialPallet_MultiplePallet(PartialPalletMultiplePalletFlags.SecondPalletIsValid |
														 PartialPalletMultiplePalletFlags.Finalised);

		public void TestCreateCache_PartialPallet_MultiplePallet_SecondIsValid_SameAttributes_NotFinalised() =>
			TestCreateCache_PartialPallet_MultiplePallet(PartialPalletMultiplePalletFlags.SecondPalletIsValid |
														 PartialPalletMultiplePalletFlags.SamePartAttributes);

		public void TestCreateCache_PartialPallet_MultiplePallet_SecondIsValid_DifferentAttributes_NotFinalised() =>
			TestCreateCache_PartialPallet_MultiplePallet(PartialPalletMultiplePalletFlags.SecondPalletIsValid);

		public void TestCreateCache_PartialPallet_MultiplePallet_SecondIsNotValid_SameAttributes_Finalised() =>
			TestCreateCache_PartialPallet_MultiplePallet(PartialPalletMultiplePalletFlags.SamePartAttributes |
														 PartialPalletMultiplePalletFlags.Finalised);

		public void TestCreateCache_PartialPallet_MultiplePallet_SecondIsNotValid_DifferentAttributes_Finalised() =>
			TestCreateCache_PartialPallet_MultiplePallet(PartialPalletMultiplePalletFlags.Finalised);

		public void TestCreateCache_PartialPallet_MultiplePallet_SecondIsNotValid_SameAttributes_NotFinalised() =>
			TestCreateCache_PartialPallet_MultiplePallet(PartialPalletMultiplePalletFlags.SamePartAttributes);

		public void TestCreateCache_PartialPallet_MultiplePallet_SecondIsNotValid_DifferentAttributes_NotFinalised() =>
			TestCreateCache_PartialPallet_MultiplePallet(PartialPalletMultiplePalletFlags.None);

		public void TestCreateCache_PartialPallet_MultiplePallet_SecondHasNoPalletId_SameAttributes_Finalised() =>
			TestCreateCache_PartialPallet_MultiplePallet(PartialPalletMultiplePalletFlags.SecondPalletHasNoId |
														 PartialPalletMultiplePalletFlags.SamePartAttributes |
														 PartialPalletMultiplePalletFlags.Finalised);

		public void TestCreateCache_PartialPallet_MultiplePallet_SecondHasNoPalletId_DifferentAttributes_Finalised() =>
			TestCreateCache_PartialPallet_MultiplePallet(PartialPalletMultiplePalletFlags.SecondPalletHasNoId |
														 PartialPalletMultiplePalletFlags.Finalised);

		public void TestCreateCache_PartialPallet_MultiplePallet_SecondHasNoPalletId_SameAttributes_NotFinalised() =>
			TestCreateCache_PartialPallet_MultiplePallet(PartialPalletMultiplePalletFlags.SecondPalletHasNoId |
														 PartialPalletMultiplePalletFlags.SamePartAttributes);

		public void
			TestCreateCache_PartialPallet_MultiplePallet_SecondHasNoPalletId_DifferentAttributes_NotFinalised() =>
			TestCreateCache_PartialPallet_MultiplePallet(PartialPalletMultiplePalletFlags.SecondPalletHasNoId);

		void TestCreateCache_PartialPallet_MultiplePallet(PartialPalletMultiplePalletFlags flags)
		{
			bool samePartAttributes = (flags & PartialPalletMultiplePalletFlags.SamePartAttributes) ==
										PartialPalletMultiplePalletFlags.SamePartAttributes;
			bool secondPalletIsValid = (flags & PartialPalletMultiplePalletFlags.SecondPalletIsValid) ==
										 PartialPalletMultiplePalletFlags.SecondPalletIsValid;
			bool finalised = (flags & PartialPalletMultiplePalletFlags.Finalised) ==
							 PartialPalletMultiplePalletFlags.Finalised;
			var palletId1 = "JOR";
			var palletId2 = (flags & PartialPalletMultiplePalletFlags.SecondPalletHasNoId) ==
							PartialPalletMultiplePalletFlags.SecondPalletHasNoId
				? ""
				: "DAN";
			var pa1_1 = "J";
			var pa2_1 = "L";
			var pa3_1 = "E";
			var packingDate1 = ZDateTime.Now.Date;
			var expiryDate1 = packingDate1.AddDays(-1);
			var pa1_2 = samePartAttributes ? pa1_1 : "W";
			var pa2_2 = samePartAttributes ? pa2_1 : "I";
			var pa3_2 = samePartAttributes ? pa3_1 : "S";
			var packingDate2 = samePartAttributes ? packingDate1 : expiryDate1.AddDays(-1);
			var expiryDate2 = samePartAttributes ? expiryDate1 : packingDate2.AddDays(-1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.WLV_MaxQuantity = 30m;
			SetLocationWeightAndCubicConstraints(location, 30, 60);
			SetPartWeightAndCubicAttributes(data.Part1, 1, 1.5);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 10m);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location, palletId1, expiryDate1,
				packingDate1, pa1_1, pa2_1, pa3_1, "");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, secondPalletIsValid ? 3 : 10, location,
				palletId2, expiryDate2, packingDate2, pa1_2, pa2_2, pa3_2, "");
			ConditionalFinaliseWithAssertion(receive1, finalised);

			Factory.Save();

			var relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Putaway Location Cache should return the right number of rows.",
				2 + secondPalletIsValid.ToInt(), cache.Count);
			var expectedRowTypes = secondPalletIsValid
				? new ZString[]
				{
					WhsPutawayLocationCacheManager.LocationCacheType.LOC,
					WhsPutawayLocationCacheManager.LocationCacheType.PLT,
					WhsPutawayLocationCacheManager.LocationCacheType.PLT
				}
				: new ZString[]
				{
					WhsPutawayLocationCacheManager.LocationCacheType.LOC,
					WhsPutawayLocationCacheManager.LocationCacheType.PLT
				};
			AssertContainsExactElementsInAnyOrder("Putaway Location Cache should return the right row values.",
				expectedRowTypes,
				GetCacheValuesInColumn<string>(cache, WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name));
			var pltRow1 = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				palletId1, data.Part1.PK, data.Org1.PK);
			var locRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);
			AssertEquals(
				"Putaway Location Cache should return a row with a Partial Pallet ID for a valid Partial Pallet, otherwise nothing.",
				palletId1, pltRow1[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name]);
			AssertEquals("Putaway Location Cache should return a row with type LOC and no Partial Pallet ID.", "",
				locRow[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name]);

			var expectedPa1 = samePartAttributes ? pa1_1 : "";
			var expectedPa2 = samePartAttributes ? pa2_1 : "";
			var expectedPa3 = samePartAttributes ? pa3_1 : "";
			ZDate? expectedExpiryDate = null;
			ZDate? expectedPackingDate = null;
			if (samePartAttributes)
			{
				expectedExpiryDate = expiryDate1;
				expectedPackingDate = packingDate1;
			}

			AssertEquals("Putaway Location Cache should return a PLT row with correct available quantity.", 5m,
				pltRow1[WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max quantity.", 1 * 10m,
				pltRow1[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);

			AssertEquals("Putaway Location Cache should return a PLT row with correct available weight.", 0m,
				pltRow1[WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max weight.", 0m,
				pltRow1[WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max weight unit.", string.Empty,
				pltRow1[WhsPutawayLocationCacheSchema.WPC_WeightUnit.Name]);

			AssertEquals("Putaway Location Cache should return a PLT row with correct available volume.", 0m,
				pltRow1[WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max volume.", 0m,
				pltRow1[WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max volume unit.", string.Empty,
				pltRow1[WhsPutawayLocationCacheSchema.WPC_VolumeUnit.Name]);

			AssertAllComputedColumns(pltRow1, 5, relation.PK, 1m, 5 * 1m, 1.5m, 5 * 1.5m, pa1_1, pa2_1, pa3_1,
				packingDate1, expiryDate1, 1);
			if (secondPalletIsValid)
			{
				var pltRow2 = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
					palletId2, data.Part1.PK, data.Org1.PK);
				AssertEquals(
					"Putaway Location Cache should return a row with a Partial Pallet ID for a valid Partial Pallet, otherwise nothing.",
					palletId2, pltRow2[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name]);

				AssertEquals("Putaway Location Cache should return a PLT row with correct available quantity.", 7m,
					pltRow2[WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
				AssertEquals("Putaway Location Cache should return a PLT row with correct max quantity.", 1 * 10m,
					pltRow2[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);

				AssertEquals("Putaway Location Cache should return a PLT row with correct available weight.", 0m,
					pltRow2[WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
				AssertEquals("Putaway Location Cache should return a PLT row with correct max weight.", 0m,
					pltRow2[WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
				AssertEquals("Putaway Location Cache should return a PLT row with correct max weight unit.",
					string.Empty, pltRow2[WhsPutawayLocationCacheSchema.WPC_WeightUnit.Name]);

				AssertEquals("Putaway Location Cache should return a PLT row with correct available volume.", 0m,
					pltRow2[WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);
				AssertEquals("Putaway Location Cache should return a PLT row with correct max volume.", 0m,
					pltRow2[WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
				AssertEquals("Putaway Location Cache should return a PLT row with correct max volume unit.",
					string.Empty, pltRow2[WhsPutawayLocationCacheSchema.WPC_VolumeUnit.Name]);

				AssertAllComputedColumns(pltRow2, 3, relation.PK, 10m, 30m, 15m, 45m, pa1_2, pa2_2, pa3_2, packingDate2,
					expiryDate2, 1);
			}

			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight.", 30m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max volume.", 60m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max quantity.", 30m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);

			var units = 8 + (7 * (!secondPalletIsValid).ToInt());
			AssertAllComputedColumns(locRow, units, relation.PK, 30m - units, 60m - (units * 1.5m), expectedPa1,
				expectedPa2, expectedPa3, expectedPackingDate, expectedExpiryDate,
				expectedNumberOfAttributes: 2 - samePartAttributes.ToInt(), expectedNumberOfRelations: 1);
		}

		#endregion

		#region TestCreateCache_PartialPallet_ExceedsLocationCapacity

		public void TestCreateCache_PartialPallet_ExceedsLocationCapacity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.WLV_MaxQuantity = 10m;
			Factory.Save();

			SetLocationWeightAndCubicConstraints(location, 10, 10, Constants.Weight.Kilograms,
				Constants.Volume.CubicDecimetres);
			SetPartWeightAndCubicAttributes(data.Part1, 1000, 1000, Constants.Weight.Grams,
				Constants.Volume.CubicCentimeters);

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);
			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location, "PLT-123");
			ConditionalFinaliseWithAssertion(receive1, true);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Putaway Location Cache should return the right number of rows.", 2, cache.Count);
			AssertContainsExactElementsInAnyOrder("Putaway Location Cache should return the right row values.",
				new ZString[]
				{
					WhsPutawayLocationCacheManager.LocationCacheType.LOC,
					WhsPutawayLocationCacheManager.LocationCacheType.PLT
				},
				GetCacheValuesInColumn<string>(cache, WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name));
			var pltRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-123", data.Part1.PK, data.Org1.PK);
			var locRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);

			AssertEquals(
				"Putaway Location Cache should return a row with a Partial Pallet ID for a valid Partial Pallet.",
				"PLT-123", pltRow[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name]);
			AssertEquals("Putaway Location Cache should return a row with type LOC and no Partial Pallet ID.", "",
				locRow[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name]);

			AssertEquals("Putaway Location Cache should return a PLT row with correct available weight.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max weight.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max weight units.", string.Empty,
				pltRow[WhsPutawayLocationCacheSchema.WPC_WeightUnit.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct available volume.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max volume.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max volume units.", string.Empty,
				pltRow[WhsPutawayLocationCacheSchema.WPC_VolumeUnit.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct available quantity.", 5m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max quantity.", 15m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);

			AssertEquals("Putaway Location Cache should return a LOC row with correct available weight.", 5m,
				locRow[WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight.", 10m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight unit.",
				Constants.Weight.Kilograms, locRow[WhsPutawayLocationCacheSchema.WPC_WeightUnit.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct available volume.", 5m,
				locRow[WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max volume.", 10m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight unit.",
				Constants.Volume.CubicDecimetres, locRow[WhsPutawayLocationCacheSchema.WPC_VolumeUnit.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct available quantity.", 5m,
				locRow[WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max quantity.", 10m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
		}

		#endregion

		#region TestCreateCache_PartialPallet_ExceedsLocationCapacity_WithLooseInventory

		public void TestCreateCache_PartialPallet_ExceedsLocationCapacity_WithLooseInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.WLV_MaxQuantity = 10m;
			Factory.Save();

			SetLocationWeightAndCubicConstraints(location, 10, 10, Constants.Weight.Kilograms,
				Constants.Volume.CubicDecimetres);
			SetPartWeightAndCubicAttributes(data.Part1, 1000, 1000, Constants.Weight.Grams,
				Constants.Volume.CubicCentimeters);

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);
			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 2, location, "PLT-123");
			ConditionalFinaliseWithAssertion(receive1, true);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Putaway Location Cache should return the right number of rows.", 2, cache.Count);
			AssertContainsExactElementsInAnyOrder("Putaway Location Cache should return the right row values.",
				new ZString[]
				{
					WhsPutawayLocationCacheManager.LocationCacheType.LOC,
					WhsPutawayLocationCacheManager.LocationCacheType.PLT
				},
				GetCacheValuesInColumn<string>(cache, WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name));
			var pltRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-123", data.Part1.PK, data.Org1.PK);
			var locRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);

			AssertEquals("Putaway Location Cache should return a PLT row with correct available weight.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max weight.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max weight units.", string.Empty,
				pltRow[WhsPutawayLocationCacheSchema.WPC_WeightUnit.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct available volume.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max volume.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max volume units.", string.Empty,
				pltRow[WhsPutawayLocationCacheSchema.WPC_VolumeUnit.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct available quantity.", 3m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max quantity.", 15m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);

			AssertEquals("Putaway Location Cache should return a LOC row with correct available weight.", 3m,
				locRow[WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight.", 10m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight unit.",
				Constants.Weight.Kilograms, locRow[WhsPutawayLocationCacheSchema.WPC_WeightUnit.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct available volume.", 3m,
				locRow[WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max volume.", 10m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight unit.",
				Constants.Volume.CubicDecimetres, locRow[WhsPutawayLocationCacheSchema.WPC_VolumeUnit.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct available quantity.", 3m,
				locRow[WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max quantity.", 10m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
		}

		#endregion

		#region TestCreateCache_PartialPallet_QuantityPrecisionOverflows

		public void TestCreateCache_PartialPallet_QuantityPrecisionOverflows()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			SetPartWeightAndCubicAttributes(data.Part1, 1m, 1, Constants.Weight.Pounds, Constants.Volume.CubicFeet);
			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet,
				123456789123.123456m);
			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1_1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location, "PLT-123");
			ConditionalFinaliseWithAssertion(receive1, true);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Putaway Location Cache should return the right number of rows.", 2, cache.Count);
			AssertContainsExactElementsInAnyOrder("Putaway Location Cache should return the right row values.",
				new ZString[]
				{
					WhsPutawayLocationCacheManager.LocationCacheType.LOC,
					WhsPutawayLocationCacheManager.LocationCacheType.PLT
				},
				GetCacheValuesInColumn<string>(cache, WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name));
			var pltRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-123", data.Part1.PK, data.Org1.PK);
			var locRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);

			AssertEquals(
				"Putaway Location Cache should return a row with a Partial Pallet ID for a valid Partial Pallet.",
				"PLT-123", pltRow[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name]);
			AssertEquals("Putaway Location Cache should return a row with type LOC and no Partial Pallet ID.", "",
				locRow[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name]);
			AssertEquals("Putaway Location Cache should truncate max quantity.", 123456789123.123m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
		}

		#endregion

		#region TestCreateCache_PartialPallets_NoCapacityLimits

		public void TestCreateCache_PartialPallets_NoCapacityLimits()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			SetPartWeightAndCubicAttributes(data.Part1, 1000, 1000, Constants.Weight.Grams,
				Constants.Volume.CubicCentimeters);
			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location, "PLT-123");
			ConditionalFinaliseWithAssertion(receive1, true);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Putaway Location Cache should return the right number of rows.", 2, cache.Count);
			AssertContainsExactElementsInAnyOrder("Putaway Location Cache should return the right row values.",
				new ZString[]
				{
					WhsPutawayLocationCacheManager.LocationCacheType.LOC,
					WhsPutawayLocationCacheManager.LocationCacheType.PLT
				},
				GetCacheValuesInColumn<string>(cache, WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name));
			var pltRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-123", data.Part1.PK, data.Org1.PK);
			var locRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);

			AssertEquals(
				"Putaway Location Cache should return a row with a Partial Pallet ID for a valid Partial Pallet.",
				"PLT-123", pltRow[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name]);
			AssertEquals("Putaway Location Cache should return a row with type LOC and no Partial Pallet ID.", "",
				locRow[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name]);

			AssertEquals("Putaway Location Cache should return a PLT row with correct available weight.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max weight.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max weight units.", string.Empty,
				pltRow[WhsPutawayLocationCacheSchema.WPC_WeightUnit.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct available volume.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max volume.", 0m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max volume units.", string.Empty,
				pltRow[WhsPutawayLocationCacheSchema.WPC_VolumeUnit.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct available quantity.", 10m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
			AssertEquals("Putaway Location Cache should return a PLT row with correct max quantity.", 15m,
				pltRow[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);

			AssertEquals("Putaway Location Cache should return a LOC row with correct available weight.", 0m,
				locRow[WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight.", 0m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight unit.", string.Empty,
				locRow[WhsPutawayLocationCacheSchema.WPC_WeightUnit.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct available volume.", 0m,
				locRow[WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max volume.", 0m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight unit.", string.Empty,
				locRow[WhsPutawayLocationCacheSchema.WPC_VolumeUnit.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct available quantity.", 0m,
				locRow[WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max quantity.", 0m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
		}

		#endregion

		public void TestCreateCache_PartialPallet_Column_ZeroBased()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var warehouse = data.Whs1;
			warehouse.WW_LocationColumnsZeroBased = true;
			Factory.Save();

			SetPartWeightAndCubicAttributes(data.Part1, 1000, 1000, Constants.Weight.Grams,
				Constants.Volume.CubicCentimeters);
			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var location1 = warehouse.FindLocation("A-0");
			var location2 = warehouse.FindLocation("A-1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location1, "PLT-123");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location2, "PLT-456");
			ConditionalFinaliseWithAssertion(receive1, true);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, new[] { location1.PK, location2.PK });
			var plt1Row = GetCacheRow(cache, location1.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-123", data.Part1.PK, data.Org1.PK);
			var plt2Row = GetCacheRow(cache, location2.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-456", data.Part1.PK, data.Org1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)0, plt1Row[WhsPutawayLocationCacheSchema.WPC_Column.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)1, plt2Row[WhsPutawayLocationCacheSchema.WPC_Column.Name]);
		}

		public void TestCreateCache_PartialPallet_Column_Alphabetical()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var warehouse = data.Whs1;
			warehouse.WW_LocationColumnsAlpha = true;
			Factory.Save();

			SetPartWeightAndCubicAttributes(data.Part1, 1000, 1000, Constants.Weight.Grams,
				Constants.Volume.CubicCentimeters);
			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var location1 = warehouse.FindLocation("A-A");
			var location2 = warehouse.FindLocation("A-B");

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location1, "PLT-123");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location2, "PLT-456");
			ConditionalFinaliseWithAssertion(receive1, true);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, new[] { location1.PK, location2.PK });
			var plt1Row = GetCacheRow(cache, location1.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-123", data.Part1.PK, data.Org1.PK);
			var plt2Row = GetCacheRow(cache, location2.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-456", data.Part1.PK, data.Org1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)1, plt1Row[WhsPutawayLocationCacheSchema.WPC_Column.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)2, plt2Row[WhsPutawayLocationCacheSchema.WPC_Column.Name]);
		}

		public void TestCreateCache_PartialPallet_Level_ZeroBased()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var warehouse = data.Whs1;
			warehouse.WW_LocationLevelsZeroBased = true;
			Factory.Save();

			SetPartWeightAndCubicAttributes(data.Part1, 1000, 1000, Constants.Weight.Grams,
				Constants.Volume.CubicCentimeters);
			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var location1 = warehouse.FindLocation("A-1-0");
			var location2 = warehouse.FindLocation("A-1-1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location1, "PLT-123");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location2, "PLT-456");
			ConditionalFinaliseWithAssertion(receive1, true);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, new[] { location1.PK, location2.PK });
			var plt1Row = GetCacheRow(cache, location1.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-123", data.Part1.PK, data.Org1.PK);
			var plt2Row = GetCacheRow(cache, location2.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-456", data.Part1.PK, data.Org1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)0, plt1Row[WhsPutawayLocationCacheSchema.WPC_Level.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)1, plt2Row[WhsPutawayLocationCacheSchema.WPC_Level.Name]);
		}

		public void TestCreateCache_PartialPallet_Level_Alphabetical()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 2);
			var warehouse = data.Whs1;
			warehouse.WW_LocationLevelsAlpha = true;
			Factory.Save();

			SetPartWeightAndCubicAttributes(data.Part1, 1000, 1000, Constants.Weight.Grams,
				Constants.Volume.CubicCentimeters);
			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var location1 = warehouse.FindLocation("A-1-A");
			var location2 = warehouse.FindLocation("A-1-B");

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location1, "PLT-123");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location2, "PLT-456");
			ConditionalFinaliseWithAssertion(receive1, true);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, new[] { location1.PK, location2.PK });
			var plt1Row = GetCacheRow(cache, location1.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-123", data.Part1.PK, data.Org1.PK);
			var plt2Row = GetCacheRow(cache, location2.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-456", data.Part1.PK, data.Org1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)1, plt1Row[WhsPutawayLocationCacheSchema.WPC_Level.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)2, plt2Row[WhsPutawayLocationCacheSchema.WPC_Level.Name]);
		}

		public void TestCreateCache_PartialPallet_Tray_ZeroBased()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			warehouse.WW_LocationTraysZeroBased = true;
			Helper.CreateRow(warehouse, "B", 5, 5, 5);
			((IWhsWarehouseInternals)warehouse).GenerateLocations();
			Factory.Save();

			SetPartWeightAndCubicAttributes(data.Part1, 1000, 1000, Constants.Weight.Grams,
				Constants.Volume.CubicCentimeters);
			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var location1 = warehouse.FindLocation("B-1-1-0");
			var location2 = warehouse.FindLocation("B-1-1-1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location1, "PLT-123");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location2, "PLT-456");
			ConditionalFinaliseWithAssertion(receive1, true);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, new[] { location1.PK, location2.PK });
			var plt1Row = GetCacheRow(cache, location1.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-123", data.Part1.PK, data.Org1.PK);
			var plt2Row = GetCacheRow(cache, location2.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-456", data.Part1.PK, data.Org1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)0, plt1Row[WhsPutawayLocationCacheSchema.WPC_Tray.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)1, plt2Row[WhsPutawayLocationCacheSchema.WPC_Tray.Name]);
		}

		public void TestCreateCache_PartialPallet_Tray_Alphabetical()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			warehouse.WW_LocationTraysAlpha = true;
			Helper.CreateRow(warehouse, "B", 5, 5, 5);
			((IWhsWarehouseInternals)warehouse).GenerateLocations();
			Factory.Save();

			SetPartWeightAndCubicAttributes(data.Part1, 1000, 1000, Constants.Weight.Grams,
				Constants.Volume.CubicCentimeters);
			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var location1 = warehouse.FindLocation("B-1-1-A");
			var location2 = warehouse.FindLocation("B-1-1-B");

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location1, "PLT-123");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5, location2, "PLT-456");
			ConditionalFinaliseWithAssertion(receive1, true);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, new[] { location1.PK, location2.PK });
			var plt1Row = GetCacheRow(cache, location1.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-123", data.Part1.PK, data.Org1.PK);
			var plt2Row = GetCacheRow(cache, location2.PK, WhsPutawayLocationCacheManager.LocationCacheType.PLT,
				"PLT-456", data.Part1.PK, data.Org1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)1, plt1Row[WhsPutawayLocationCacheSchema.WPC_Tray.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)2, plt2Row[WhsPutawayLocationCacheSchema.WPC_Tray.Name]);
		}

		#region TestCreateCache_LargeMaxQuantity

		public void TestCreateCache_LargeMaxQuantity()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			location.WLV_MaxQuantity = 12345678912345.123m;
			SetPartWeightAndCubicAttributes(data.Part1, 1, 1, Constants.Weight.Kilograms,
				Constants.Volume.CubicDecimetres);
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 15m, location);
			ConditionalFinaliseWithAssertion(receive1, true);
			Factory.Save();
			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Putaway Location Cache should return the right number of rows.", 1, cache.Count);
			var locRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max quantity.",
				12345678912345.123m, locRow[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct available quantity.",
				12345678912330.123m, locRow[WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
		}

		#endregion

		#region TestCreateCache_CapacitiesExceeded

		public void TestCreateCache_CapacitiesExceeded()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 15, location);
			ConditionalFinaliseWithAssertion(receive1, true);
			Factory.Save();

			using (WhsTestHelperFunctions.SuspendTrigger("TG_PreventOverReduceLocationUnitsCapacity",
						 WhsLocationSchema.Constants.TableName)) // Existing bad data is possible
			{
				location.WLV_MaxQuantity = 10m;
				SetLocationWeightAndCubicConstraints(location, 10, 10, Constants.Weight.Kilograms,
					Constants.Volume.CubicDecimetres);
				SetPartWeightAndCubicAttributes(data.Part1, 1, 1, Constants.Weight.Kilograms,
					Constants.Volume.CubicDecimetres);
				Factory.Save();
			}

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Putaway Location Cache should return the right number of rows.", 1, cache.Count);
			var locRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);

			AssertEquals("Putaway Location Cache should return a LOC row with correct available weight.", 0m,
				locRow[WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight.", 10m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxWeight.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight unit.",
				Constants.Weight.Kilograms, locRow[WhsPutawayLocationCacheSchema.WPC_WeightUnit.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct available volume.", 0m,
				locRow[WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max volume.", 10m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxVolume.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max weight unit.",
				Constants.Volume.CubicDecimetres, locRow[WhsPutawayLocationCacheSchema.WPC_VolumeUnit.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct max quantity.", 10m,
				locRow[WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
			AssertEquals("Putaway Location Cache should return a LOC row with correct available quantity.", 0m,
				locRow[WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
		}

		#endregion

		#region TestCreateCache_WPC_StockOnHandExceeded_DoesNotOverflow

		public void TestCreateCache_WPC_StockOnHandExceeded_DoesNotOverflow()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			// Avoid overflow of volume/weight
			data.Part1.OP_Cubic = 0m;
			data.Part1.OP_Weight = 0m;

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 999999999999999m, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 123456m, location);
			ConditionalFinaliseWithAssertion(receive1, true);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Putaway Location Cache should return the right number of rows.", 1, cache.Count);
			var locRow = GetCacheRow(cache, location.PK, WhsPutawayLocationCacheManager.LocationCacheType.LOC, "",
				ZGuid.Empty, ZGuid.Empty);

			// This overflow case can only happen if MaxQuantity is not used, in this case StockOnHand isnt as important so truncating will be fine.
			// Additionally, this edge case relies on an insane amount of units in a locations so is unlikely to be seen outside of test rigs
			AssertEquals("Putaway Location Cache should return a LOC row with capped stock on hand.",
				999999999999999.999m, locRow[WhsPutawayLocationCacheSchema.WPC_StockOnHand.Name]);
		}

		#endregion

		#region TestCreateCache_IsTSAApprovedKnown

		public void TestCreateCache_IsTSAApprovedKnown_USWarehouse_TSAStatusKnown_ApprovedKnownLocation() =>
			TestCreateCache_IsTSAApprovedKnown(true, true, true, 1);

		public void TestCreateCache_IsTSAApprovedKnown_USWarehouse_TSAStatusUnknown_ApprovedKnownLocation() =>
			TestCreateCache_IsTSAApprovedKnown(true, false, true, 0);

		public void TestCreateCache_IsTSAApprovedKnown_USWarehouse_TSAStatusKnown_NotApprovedKnownLocation() =>
			TestCreateCache_IsTSAApprovedKnown(true, true, false, 0);

		public void TestCreateCache_IsTSAApprovedKnown_USWarehouse_TSAStatusUnknown_NotApprovedKnownLocation() =>
			TestCreateCache_IsTSAApprovedKnown(true, false, false, 0);

		public void TestCreateCache_IsTSAApprovedKnown_NotUSWarehouse_TSAStatusKnown_ApprovedKnownLocation() =>
			TestCreateCache_IsTSAApprovedKnown(false, true, true, 0);

		public void TestCreateCache_IsTSAApprovedKnown_NotUSWarehouse_TSAStatusUnknown_ApprovedKnownLocation() =>
			TestCreateCache_IsTSAApprovedKnown(false, false, true, 0);

		public void TestCreateCache_IsTSAApprovedKnown_NotUSWarehouse_TSAStatusKnown_NotApprovedKnownLocation() =>
			TestCreateCache_IsTSAApprovedKnown(false, true, false, 0);

		public void TestCreateCache_IsTSAApprovedKnown_NotUSWarehouse_TSAStatusUnknown_NotApprovedKnownLocation() =>
			TestCreateCache_IsTSAApprovedKnown(false, false, false, 0);

		void TestCreateCache_IsTSAApprovedKnown(bool usWhs, bool orgAddressTSAStatusKnown, bool approvedKnownLocation,
			int result)
		{
			var usHelper = new WhsTestHelperFunctionsEnvUS(Factory);
			var whs = usWhs
				? usHelper.CreateWarehouse("TST WHS", "A", 2, 1)
				: Helper.CreateWarehouse("TST WHS", "A", 2, 1);
			Factory.Save();

			var location = whs.FindLocation("A-1");
			usHelper.SetOrgAddressTSAStatus(whs.WarehouseAddress,
				orgAddressTSAStatusKnown
					? Environment.CodeLists.US.TSAStatus.Codes.Known
					: Environment.CodeLists.US.TSAStatus.Codes.Unknown);
			location.WLV_ApprovedKnownLocation = approvedKnownLocation
				? Environment.CodeLists.US.TSAStatus.Codes.Known
				: Environment.CodeLists.US.TSAStatus.Codes.Unknown;
			Factory.Save();

			var cache = CreateAndGetCache(Factory, whs.PK, location.PK);
			AssertEquals(result == 1, cache[0][WhsPutawayLocationCacheSchema.WPC_IsTSAApprovedKnown.Name]);
		}

		public void TestCreateCache_MultipleOrgCountryDataRecords() =>
			TestCreateCache_IsTSAApprovedKnown_MultipleOrgCountryDataRecords(isTSAApproved: false,
				afterCreation: false);

		public void TestCreateCache_MultipleOrgCountryDataRecords_AfterCreation() =>
			TestCreateCache_IsTSAApprovedKnown_MultipleOrgCountryDataRecords(isTSAApproved: false, afterCreation: true);

		public void TestCreateCache_IsTSAApprovedKnown_MultipleOrgCountryDataRecords() =>
			TestCreateCache_IsTSAApprovedKnown_MultipleOrgCountryDataRecords(isTSAApproved: true, afterCreation: false);

		public void TestCreateCache_IsTSAApprovedKnown_MultipleOrgCountryDataRecords_AfterCreation() =>
			TestCreateCache_IsTSAApprovedKnown_MultipleOrgCountryDataRecords(isTSAApproved: true, afterCreation: true);

		void TestCreateCache_IsTSAApprovedKnown_MultipleOrgCountryDataRecords(bool isTSAApproved, bool afterCreation)
		{
			var usHelper = new WhsTestHelperFunctionsEnvUS(Factory);
			var whs = isTSAApproved
				? usHelper.CreateWarehouse("TST WHS", "A", 2, 1)
				: Helper.CreateWarehouse("TST WHS", "A", 2, 1);
			Factory.Save();

			var location = whs.FindLocation("A-1");

			if (isTSAApproved)
			{
				usHelper.SetOrgAddressTSAStatus(whs.WarehouseAddress, Environment.CodeLists.US.TSAStatus.Codes.Known);
				location.WLV_ApprovedKnownLocation = Environment.CodeLists.US.TSAStatus.Codes.Known;
			}
			else
			{
				CreateOrgCountryData(Constants.CountryCodes.Australia);
			}

			if (!afterCreation)
			{
				CreateOrgCountryData(Constants.CountryCodes.Venezuela);
			}

			Factory.Save();

			var cache1 = CreateAndGetCache(Factory, whs.PK, location.PK);
			AssertEquals(isTSAApproved, cache1[0][WhsPutawayLocationCacheSchema.WPC_IsTSAApprovedKnown.Name]);

			if (afterCreation)
			{
				CreateOrgCountryData(Constants.CountryCodes.Jordan);
				Factory.Save();

				var cache2 = CreateAndGetCache(Factory, whs.PK, location.PK);
				AssertEquals(isTSAApproved, cache2[0][WhsPutawayLocationCacheSchema.WPC_IsTSAApprovedKnown.Name]);
			}

			void CreateOrgCountryData(string countryCode)
			{
				var countryData = Factory.New<OrgCountryData>();
				countryData.OV_OA_ApprovedLocation = whs.WarehouseAddress.PK;
				countryData.OV_OH_OrgHeader = whs.WarehouseAddress.OA_OH;
				countryData.OV_RN_NKClientCountryRelation = countryCode;
			}
		}

		#endregion

		#region TestCreateCache_PickFace

		public void TestCreateCache_IsFixedPickFaceFull_Finalised() =>
			TestCreateCache_IsFixedPickFaceFullCore(true, true);

		public void TestCreateCache_IsFixedPickFaceNotFull_Finalised() =>
			TestCreateCache_IsFixedPickFaceFullCore(false, true);

		public void TestCreateCache_IsFixedPickFaceFull_NotFinalised() =>
			TestCreateCache_IsFixedPickFaceFullCore(true, false);

		public void TestCreateCache_IsFixedPickFaceNotFull_NotFinalised() =>
			TestCreateCache_IsFixedPickFaceFullCore(false, false);

		void TestCreateCache_IsFixedPickFaceFullCore(bool isFull, bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
			pickface.WF_ReplenishMaximum = 15;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, location);
			if (isFull)
			{
				var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, location);
			}

			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals($"Cache must return location that is {(isFull ? "" : "not")} full.", isFull,
				cache[0][WhsPutawayLocationCacheSchema.WPC_IsFixedPickFaceFull.Name]);
		}

		public void TestCreateCache_OP_PickFaceProduct_Fixed_Single() =>
			TestCreateCache_OP_PickFaceProduct(false, false);

		public void TestCreateCache_OP_PickFaceProduct_Fixed_Multiple() =>
			TestCreateCache_OP_PickFaceProduct(false, true);

		public void TestCreateCache_OP_PickFaceProduct_Dynamic_Single() =>
			TestCreateCache_OP_PickFaceProduct(true, false);

		public void TestCreateCache_OP_PickFaceProduct_Dynamic_Multiple() =>
			TestCreateCache_OP_PickFaceProduct(true, true);

		void TestCreateCache_OP_PickFaceProduct(bool isDynamic, bool multipleProducts)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			if (isDynamic)
			{
				var dynamicArea = Helper.CreateArea(warehouse, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
				var dynamicLocationType = Helper.CreateLocationType("JCL", LocationClasses.Codes.DPF);

				location.WLV_WLT_LocationType = dynamicLocationType.PK;
				location.WLV_WA_PickingArea = dynamicArea.PK;

				var whsProduct1 = WhsProduct.GetWhsProduct(data.Part1);
				AddParamsByWhsAndClient(whsProduct1, data.Org1, warehouse, dynamicArea);

				if (multipleProducts)
				{
					var whsProduct2 = WhsProduct.GetWhsProduct(data.Part2);
					AddParamsByWhsAndClient(whsProduct2, data.Org1, warehouse, dynamicArea);
				}
			}
			else
			{
				var pickface1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
				if (multipleProducts)
				{
					var pickface2 = Helper.CreateProductPickFace(data.Part2, data.Org1, location);
				}
			}

			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Cache should return the correct number of rows.", multipleProducts.ToInt() + 1, cache.Count);
			if (multipleProducts)
			{
				AssertContainsExactElementsInAnyOrder("Returned cache should have both product keys on separate rows.",
					new[] { data.Part1.PK, data.Part2.PK },
					new[]
					{
						cache[0][WhsPutawayLocationCacheSchema.WPC_OP_Product.Name],
						cache[1][WhsPutawayLocationCacheSchema.WPC_OP_Product.Name]
					});
			}
			else
			{
				AssertEquals("Cache must return row that has the assigned product pk.", data.Part1.PK,
					cache[0][WhsPutawayLocationCacheSchema.WPC_OP_Product.Name]);
			}
		}

		public void TestCreateCache_OH_PickFaceClient_Fixed_Single() => TestCreateCache_OH_PickFaceClient(false, false);

		public void TestCreateCache_OH_PickFaceClient_Fixed_Multiple() =>
			TestCreateCache_OH_PickFaceClient(false, true);

		public void TestCreateCache_OH_PickFaceClient_Dynamic_Single() =>
			TestCreateCache_OH_PickFaceClient(true, false);

		public void TestCreateCache_OH_PickFaceClient_Dynamic_Multiple() =>
			TestCreateCache_OH_PickFaceClient(true, true);

		void TestCreateCache_OH_PickFaceClient(bool isDynamic, bool multipleClients)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var org2 = Helper.CreateClient("JJL");
			if (isDynamic)
			{
				var dynamicArea = Helper.CreateArea(warehouse, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
				var dynamicLocationType = Helper.CreateLocationType("JCL", LocationClasses.Codes.DPF);

				location.WLV_WLT_LocationType = dynamicLocationType.PK;
				location.WLV_WA_PickingArea = dynamicArea.PK;

				var whsProduct1 = WhsProduct.GetWhsProduct(data.Part1);
				AddParamsByWhsAndClient(whsProduct1, data.Org1, warehouse, dynamicArea);

				if (multipleClients)
				{
					AddParamsByWhsAndClient(whsProduct1, org2, warehouse, dynamicArea);
				}
			}
			else
			{
				var pickface1 = Helper.CreateProductPickFace(data.Part1, data.Org1, location);
				if (multipleClients)
				{
					var pickface2 = Helper.CreateProductPickFace(data.Part1, org2, location);
				}
			}

			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Cache should return the correct number of rows.", multipleClients.ToInt() + 1, cache.Count);
			if (multipleClients)
			{
				AssertContainsExactElementsInAnyOrder("Returned cache should have both client keys on separate rows.",
					new[] { data.Org1.PK, org2.PK },
					new[]
					{
						cache[0][WhsPutawayLocationCacheSchema.WPC_OH_Client.Name],
						cache[1][WhsPutawayLocationCacheSchema.WPC_OH_Client.Name]
					});
			}
			else
			{
				AssertEquals("Cache must return row that has the assigned client pk.", data.Org1.PK,
					cache[0][WhsPutawayLocationCacheSchema.WPC_OH_Client.Name]);
			}
		}

		public void TestCreateCache_OnlyContainsPickFaceOrgPartRelationData_UserDefinedFixed_Finalised() =>
			TestCreateCache_OnlyContainsPickFaceOrgPartRelationData(SetupUserDefinedFixedPickFace,
				GetOrderedFixedPickFaces, true);

		public void TestCreateCache_OnlyContainsPickFaceOrgPartRelationData_UserDefinedFixed_NotFinalised() =>
			TestCreateCache_OnlyContainsPickFaceOrgPartRelationData(SetupUserDefinedFixedPickFace,
				GetOrderedFixedPickFaces, false);

		void SetupUserDefinedFixedPickFace(TestDataSimpleEnvironment data, OrgHeader org2, WhsLocation location)
		{
			var fixedLocationType = Helper.CreateLocationType("UFI", "User Fixed", false, 1, LocationClasses.Codes.FIX);
			location.WLV_WLT_LocationType = fixedLocationType.PK;
			SetupFixedPickFace(data, org2, location);
		}

		public void TestCreateCache_OnlyContainsPickFaceOrgPartRelationData_Fixed_Finalised() =>
			TestCreateCache_OnlyContainsPickFaceOrgPartRelationData(SetupFixedPickFace, GetOrderedFixedPickFaces, true);

		public void TestCreateCache_OnlyContainsPickFaceOrgPartRelationData_Fixed_NotFinalised() =>
			TestCreateCache_OnlyContainsPickFaceOrgPartRelationData(SetupFixedPickFace, GetOrderedFixedPickFaces,
				false);

		void SetupFixedPickFace(TestDataSimpleEnvironment data, OrgHeader org2, WhsLocation location)
		{
			Helper.CreateProductPickFace(data.Part1, data.Org1, location, replenishMax: 1000m);
			Helper.CreateProductPickFace(data.Part2, data.Org1, location, replenishMax: 1000m);
			Helper.CreateProductPickFace(data.Part1, org2, location, replenishMax: 1000m);
			Helper.CreateProductPickFace(data.Part2, org2, location, replenishMax: 1000m);
		}

		DataRow[] GetOrderedFixedPickFaces(IReadOnlyList<DataRow> cache, TestDataSimpleEnvironment data, OrgHeader org2,
			WhsLocation location)
		{
			var result = new DataRow[cache.Count];
			var cachePks = new[]
			{
				GetZGuid(cache[0][WhsPutawayLocationCacheSchema.PK.Name]),
				GetZGuid(cache[1][WhsPutawayLocationCacheSchema.PK.Name]),
				GetZGuid(cache[2][WhsPutawayLocationCacheSchema.PK.Name]),
				GetZGuid(cache[3][WhsPutawayLocationCacheSchema.PK.Name])
			};
			var pickFace1 = WhsProduct.GetWhsProduct(data.Part1).PickFaces.FindByLocation(data.Org1.PK, location.PK);
			var pickFace2 = WhsProduct.GetWhsProduct(data.Part2).PickFaces.FindByLocation(data.Org1.PK, location.PK);
			var pickFace3 = WhsProduct.GetWhsProduct(data.Part1).PickFaces.FindByLocation(org2.PK, location.PK);
			var pickFace4 = WhsProduct.GetWhsProduct(data.Part2).PickFaces.FindByLocation(org2.PK, location.PK);
			result[0] = cache[Array.IndexOf(cachePks, pickFace1.PK)];
			result[1] = cache[Array.IndexOf(cachePks, pickFace2.PK)];
			result[2] = cache[Array.IndexOf(cachePks, pickFace3.PK)];
			result[3] = cache[Array.IndexOf(cachePks, pickFace4.PK)];

			return result;
		}

		public void TestCreateCache_OnlyContainsPickFaceOrgPartRelationData_Dynamic_Finalised() =>
			TestCreateCache_OnlyContainsPickFaceOrgPartRelationData(SetupDynamicPickFace, GetOrderedDynamicPickFaces,
				true);

		public void TestCreateCache_OnlyContainsPickFaceOrgPartRelationData_Dynamic_NotFinalised() =>
			TestCreateCache_OnlyContainsPickFaceOrgPartRelationData(SetupDynamicPickFace, GetOrderedDynamicPickFaces,
				false);

		void SetupDynamicPickFace(TestDataSimpleEnvironment data, OrgHeader org2, WhsLocation location)
		{
			var warehouse = data.Whs1;
			var dynamicArea = Helper.CreateArea(warehouse, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("RTB", LocationClasses.Codes.DPF);

			location.WLV_WLT_LocationType = dynamicLocationType.PK;
			location.WLV_WA_PickingArea = dynamicArea.PK;

			var whsProduct1 = WhsProduct.GetWhsProduct(data.Part1);
			var whsProduct2 = WhsProduct.GetWhsProduct(data.Part2);
			AddParamsByWhsAndClient(whsProduct1, data.Org1, warehouse, dynamicArea);
			AddParamsByWhsAndClient(whsProduct1, org2, warehouse, dynamicArea);
			AddParamsByWhsAndClient(whsProduct2, data.Org1, warehouse, dynamicArea);
			AddParamsByWhsAndClient(whsProduct2, org2, warehouse, dynamicArea);
		}

		DataRow[] GetOrderedDynamicPickFaces(IReadOnlyList<DataRow> cache, TestDataSimpleEnvironment data,
			OrgHeader org2, WhsLocation location)
		{
			var result = new DataRow[cache.Count];
			result[0] = GetCacheRow(cache, location, WhsPutawayLocationCacheManager.LocationCacheType.DYN, "",
				data.Part1, data.Org1);
			result[1] = GetCacheRow(cache, location, WhsPutawayLocationCacheManager.LocationCacheType.DYN, "",
				data.Part2, data.Org1);
			result[2] = GetCacheRow(cache, location, WhsPutawayLocationCacheManager.LocationCacheType.DYN, "",
				data.Part1, org2);
			result[3] = GetCacheRow(cache, location, WhsPutawayLocationCacheManager.LocationCacheType.DYN, "",
				data.Part2, org2);

			return result;
		}

		delegate void SetupLocationDelegate(TestDataSimpleEnvironment data, OrgHeader org2, WhsLocation location);

		delegate DataRow[] GetOrderedLocationCacheDelegate(IReadOnlyList<DataRow> cache, TestDataSimpleEnvironment data,
			OrgHeader org2, WhsLocation location);

		void TestCreateCache_OnlyContainsPickFaceOrgPartRelationData(SetupLocationDelegate setupLocation,
			GetOrderedLocationCacheDelegate getOrderedLocationCache, bool finalised)
		{
			var pa1_1 = "A";
			var pa2_1 = "N";
			var pa3_1 = "G";
			var expiryDate_1 = ZDateTime.Now.Date;
			var packingDate_1 = expiryDate_1.AddDays(-1);
			var pa1_2 = "U";
			var pa2_2 = "S";
			var pa3_2 = "P";
			var expiryDate_2 = packingDate_1.AddDays(-1);
			var packingDate_2 = expiryDate_2.AddDays(-1);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var org2 = Helper.CreateClient("RLC");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Helper.CreateProductClientRelationShip(org2, data.Part2);

			setupLocation(data, org2, location);

			Helper.SetClientAllAttributeType(data.Org1, mandatoryAttributeType: true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Helper.SetClientAllAttributeType(org2, mandatoryAttributeType: true);
			Helper.SetProductAllAttributeUse(org2, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Helper.SetProductAllAttributeUse(org2, data.Part2, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 1, location, expiryDate_1, packingDate_1, pa1_1,
				pa2_1, pa3_1, "");
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part2, 2, location, expiryDate_1, packingDate_1, pa1_1,
				pa2_1, pa3_1, "");
			ConditionalFinaliseWithAssertion(receive1, finalised);

			var receive2 = Helper.CreateWhsReceive(org2, warehouse);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 4, location, expiryDate_2, packingDate_2, pa1_2,
				pa2_2, pa3_2, "");
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part2, 8, location, expiryDate_2, packingDate_2, pa1_2,
				pa2_2, pa3_2, "");
			ConditionalFinaliseWithAssertion(receive2, finalised);

			Factory.Save();

			var relation1_1 =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var relation1_2 =
				data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var relation2_1 =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(org2,
					OrgPartRelation.RelationshipTypes.Owner);
			var relation2_2 =
				data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(org2,
					OrgPartRelation.RelationshipTypes.Owner);

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Cache should return the correct number of rows.", 4, cache.Count);
			AssertContainsExactElementsInAnyOrder("Returned cache should have both product keys on seperate rows.",
				new[] { data.Part1.PK, data.Part2.PK, data.Part1.PK, data.Part2.PK },
				new[]
				{
					cache[0][WhsPutawayLocationCacheSchema.WPC_OP_Product.Name],
					cache[1][WhsPutawayLocationCacheSchema.WPC_OP_Product.Name],
					cache[2][WhsPutawayLocationCacheSchema.WPC_OP_Product.Name],
					cache[3][WhsPutawayLocationCacheSchema.WPC_OP_Product.Name]
				});
			AssertContainsExactElementsInAnyOrder("Returned cache should have both client keys on seperate rows.",
				new[] { data.Org1.PK, org2.PK, data.Org1.PK, org2.PK },
				new[]
				{
					cache[0][WhsPutawayLocationCacheSchema.WPC_OH_Client.Name],
					cache[1][WhsPutawayLocationCacheSchema.WPC_OH_Client.Name],
					cache[2][WhsPutawayLocationCacheSchema.WPC_OH_Client.Name],
					cache[3][WhsPutawayLocationCacheSchema.WPC_OH_Client.Name]
				});

			var orderedCache = getOrderedLocationCache(cache, data, org2, location);
			AssertAllComputedColumns(orderedCache[0], 1, relation1_1.PK, 0, 0, 0, 0, pa1_1, pa2_1, pa3_1, packingDate_1,
				expiryDate_1, 1);
			AssertAllComputedColumns(orderedCache[1], 2, relation1_2.PK, 0, 0, 0, 0, pa1_1, pa2_1, pa3_1, packingDate_1,
				expiryDate_1, 1);
			AssertAllComputedColumns(orderedCache[2], 4, relation2_1.PK, 0, 0, 0, 0, pa1_2, pa2_2, pa3_2, packingDate_2,
				expiryDate_2, 1);
			AssertAllComputedColumns(orderedCache[3], 8, relation2_2.PK, 0, 0, 0, 0, pa1_2, pa2_2, pa3_2, packingDate_2,
				expiryDate_2, 1);
		}

		public void TestCreateCache_PickFace_Quantities_Finalised() => TestCreateCache_PickFace_Quantities(true);

		public void TestCreateCache_PickFace_Quantities_NotFinalised() => TestCreateCache_PickFace_Quantities(false);

		void TestCreateCache_PickFace_Quantities(bool finalisedTransactionLines)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, location, replenishMax: 15m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			ConditionalFinaliseWithAssertion(receive, finalisedTransactionLines);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals($"Cache must return location with correct stock on hand.", 10m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_StockOnHand.Name]);
			AssertEquals($"Cache must return location with available quantity.", 5m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
			AssertEquals($"Cache must return location with correct max quantity.", 15m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
			AssertEquals($"Cache must return non-full pick face location.", false,
				cache[0][WhsPutawayLocationCacheSchema.WPC_IsFixedPickFaceFull.Name]);
		}

		public void TestCreateCache_PickFace_Quantities_NoStockOnHand()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, location, replenishMax: 15m);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals($"Cache must return location with correct stock on hand.", 0m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_StockOnHand.Name]);
			AssertEquals($"Cache must return location with available quantity.", 15m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
			AssertEquals($"Cache must return location with correct max quantity.", 15m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
			AssertEquals($"Cache must return empty pick face location.", false,
				cache[0][WhsPutawayLocationCacheSchema.WPC_IsFixedPickFaceFull.Name]);
		}

		public void TestCreateCache_PickFace_Quantities_Overfilled()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var pickface = Helper.CreateProductPickFace(data.Part1, data.Org1, location, replenishMax: 5m);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5m, location);
			ConditionalFinaliseWithAssertion(receive, true);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals($"Cache must return location with correct stock on hand.", 10m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_StockOnHand.Name]);
			AssertEquals($"Cache must return location with available quantity.", 0m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
			AssertEquals($"Cache must return location with correct max quantity.", 5m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_MaxQuantity.Name]);
			AssertEquals($"Cache must return full pick face location.", true,
				cache[0][WhsPutawayLocationCacheSchema.WPC_IsFixedPickFaceFull.Name]);
		}

		public void TestCreateCache_PickFace_LocationZeroBased()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			warehouse.WW_LocationColumnsZeroBased = true;
			warehouse.WW_LocationLevelsZeroBased = true;
			warehouse.WW_LocationTraysZeroBased = true;
			Helper.CreateRow(warehouse, "B", 5, 5, 5);
			((IWhsWarehouseInternals)warehouse).GenerateLocations();
			Factory.Save();

			var location1 = warehouse.FindLocation("B-0-0-0");
			var location2 = warehouse.FindLocation("B-1-1-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			Helper.CreateProductPickFace(data.Part1, data.Org1, location2);

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)0, cache1[0][WhsPutawayLocationCacheSchema.WPC_Column.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)0, cache1[0][WhsPutawayLocationCacheSchema.WPC_Level.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)0, cache1[0][WhsPutawayLocationCacheSchema.WPC_Tray.Name]);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location2.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)1, cache2[0][WhsPutawayLocationCacheSchema.WPC_Column.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)1, cache2[0][WhsPutawayLocationCacheSchema.WPC_Level.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)1, cache2[0][WhsPutawayLocationCacheSchema.WPC_Tray.Name]);
		}

		public void TestCreateCache_PickFace_LocationAlphabetical()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			warehouse.WW_LocationColumnsAlpha = true;
			warehouse.WW_LocationLevelsAlpha = true;
			warehouse.WW_LocationTraysAlpha = true;
			Helper.CreateRow(warehouse, "B", 5, 5, 5);
			((IWhsWarehouseInternals)warehouse).GenerateLocations();
			Factory.Save();

			var location1 = warehouse.FindLocation("B-A-A-A");
			var location2 = warehouse.FindLocation("B-B-B-B");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location1);
			Helper.CreateProductPickFace(data.Part1, data.Org1, location2);

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)1, cache1[0][WhsPutawayLocationCacheSchema.WPC_Column.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)1, cache1[0][WhsPutawayLocationCacheSchema.WPC_Level.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)1, cache1[0][WhsPutawayLocationCacheSchema.WPC_Tray.Name]);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location2.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)2, cache2[0][WhsPutawayLocationCacheSchema.WPC_Column.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)2, cache2[0][WhsPutawayLocationCacheSchema.WPC_Level.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)2, cache2[0][WhsPutawayLocationCacheSchema.WPC_Tray.Name]);
		}

		#endregion

		public void TestCreateCache_DynamicPickFace_LocationZeroBased()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			warehouse.WW_LocationColumnsZeroBased = true;
			warehouse.WW_LocationLevelsZeroBased = true;
			warehouse.WW_LocationTraysZeroBased = true;
			Helper.CreateRow(warehouse, "B", 5, 5, 5);
			((IWhsWarehouseInternals)warehouse).GenerateLocations();
			Factory.Save();

			var location1 = warehouse.FindLocation("B-0-0-0");
			var location2 = warehouse.FindLocation("B-1-1-1");
			var dynamicArea = Helper.CreateArea(warehouse, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DGD", LocationClasses.Codes.DPF);
			location1.WLV_WLT_LocationType = dynamicLocationType.PK;
			location1.WLV_WA_PickingArea = dynamicArea.PK;
			location2.WLV_WLT_LocationType = dynamicLocationType.PK;
			location2.WLV_WA_PickingArea = dynamicArea.PK;
			AddParamsByWhsAndClient(WhsProduct.GetWhsProduct(data.Part1), data.Org1, warehouse, dynamicArea);

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)0, cache1[0][WhsPutawayLocationCacheSchema.WPC_Column.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)0, cache1[0][WhsPutawayLocationCacheSchema.WPC_Level.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)0, cache1[0][WhsPutawayLocationCacheSchema.WPC_Tray.Name]);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location2.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)1, cache2[0][WhsPutawayLocationCacheSchema.WPC_Column.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)1, cache2[0][WhsPutawayLocationCacheSchema.WPC_Level.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)1, cache2[0][WhsPutawayLocationCacheSchema.WPC_Tray.Name]);
		}

		public void TestCreateCache_DynamicPickFace_LocationAlphabetical()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var warehouse = data.Whs1;
			warehouse.WW_LocationColumnsAlpha = true;
			warehouse.WW_LocationLevelsAlpha = true;
			warehouse.WW_LocationTraysAlpha = true;
			Helper.CreateRow(warehouse, "B", 5, 5, 5);
			((IWhsWarehouseInternals)warehouse).GenerateLocations();
			Factory.Save();

			var location1 = warehouse.FindLocation("B-A-A-A");
			var location2 = warehouse.FindLocation("B-B-B-B");
			var dynamicArea = Helper.CreateArea(warehouse, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DGD", LocationClasses.Codes.DPF);
			location1.WLV_WLT_LocationType = dynamicLocationType.PK;
			location1.WLV_WA_PickingArea = dynamicArea.PK;
			location2.WLV_WLT_LocationType = dynamicLocationType.PK;
			location2.WLV_WA_PickingArea = dynamicArea.PK;
			AddParamsByWhsAndClient(WhsProduct.GetWhsProduct(data.Part1), data.Org1, warehouse, dynamicArea);

			var cache1 = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)1, cache1[0][WhsPutawayLocationCacheSchema.WPC_Column.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)1, cache1[0][WhsPutawayLocationCacheSchema.WPC_Level.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)1, cache1[0][WhsPutawayLocationCacheSchema.WPC_Tray.Name]);

			var cache2 = CreateAndGetCache(Factory, data.Whs1.PK, location2.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the column equal to that of the location.",
				(short)2, cache2[0][WhsPutawayLocationCacheSchema.WPC_Column.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the level equal to that of the location.",
				(short)2, cache2[0][WhsPutawayLocationCacheSchema.WPC_Level.Name]);
			AssertEquals("Should return one WhsPutawayLocationCache with the tray equal to that of the location.",
				(short)2, cache2[0][WhsPutawayLocationCacheSchema.WPC_Tray.Name]);
		}

		public void TestCreateCache_WithAdjustment_Positive() => TestCreateCache_WithAdjustment(0.5);

		public void TestCreateCache_WithAdjustment_Negative() => TestCreateCache_WithAdjustment(-0.5);

		void TestCreateCache_WithAdjustment(ZDecimal amount)
		{
			var expiryDate = ZDateTime.Now.Date.AddDays(-1);
			var packingDate = expiryDate.AddDays(-1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var location1 = data.Whs1.FindLocation("A-1");
			SetLocationWeightAndCubicConstraints(location1, 150, 175);
			SetPartWeightAndCubicAttributes(data.Part1, 10, 10);

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, location1, expiryDate, packingDate, "A", "B",
				"C", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2.5, location1, expiryDate, packingDate, "A", "B",
				"C", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, amount, location1.ToLocationString(), "A", "B",
				"C", "", expiryDate, packingDate);
			adjustment.RunPreSaveValidation();
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK);
			var relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var expectedAdjustment =
				amount > 0
					? amount
					: 0; // We only care about adjustments in, out should not be included... to be consistent with WhsLocationCapacity.
			AssertAllComputedColumns(cache, 12.5m + expectedAdjustment, relation.PK, 10, 150, 10, 175, "A", "B", "C",
				packingDate, expiryDate);
		}

		public void TestCreateCache_InTransitStock()
		{
			var expiryDate = ZDateTime.Now.Date.AddDays(-1);
			var packingDate = expiryDate.AddDays(-1);
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var location1 = data.Whs1.FindLocation("A-1");
			SetLocationWeightAndCubicConstraints(location1, 150, 175);
			var location2 = data.Whs1.FindLocation("A-2");
			SetLocationWeightAndCubicConstraints(location2, 200, 225);
			SetPartWeightAndCubicAttributes(data.Part1, 10, 10);

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, location1, expiryDate, packingDate, "A", "B",
				"C", "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2.5, location1, expiryDate, packingDate, "A", "B",
				"C", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 12.5, location1, "", location2,
				expiryDate, packingDate, "A", "B", "C");
			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals(InventoryStatus.Codes.InTransit, transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location1.PK);
			AssertAllComputedColumns(cache, 0, ZGuid.Empty, 10, 150, 10, 175, null, null, null, null, null,
				expectedNumberOfRelations: 0);

			var relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			cache = CreateAndGetCache(Factory, data.Whs1.PK, location2.PK);
			AssertAllComputedColumns(cache, 12.5, relation.PK, 10, 200, 10, 225, "A", "B", "C", packingDate,
				expiryDate);
		}

		#region TestCreateCache_Weight

		public void TestCreateCache_AvailableWeight_SingleDocketLine_NotFinalised() =>
			TestCreateCache_AvailableWeight_SingleDocketLine(false);

		public void TestCreateCache_AvailableWeight_SingleDocketLine_Finalised() =>
			TestCreateCache_AvailableWeight_SingleDocketLine(true);

		void TestCreateCache_AvailableWeight_SingleDocketLine(bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			SetLocationWeightConstraints(location, 100);
			SetPartWeightAttributes(data.Part1, 10);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8, location);
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the available weight equal to 20.", 20m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
		}

		public void TestCreateCache_AvailableWeight_MultipleDocketLine_NotFinalised() =>
			TestCreateCache_AvailableWeight_MultipleDocketLine(false);

		public void TestCreateCache_AvailableWeight_MultipleDocketLine_Finalised() =>
			TestCreateCache_AvailableWeight_MultipleDocketLine(true);

		void TestCreateCache_AvailableWeight_MultipleDocketLine(bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			SetLocationWeightConstraints(location, 100);
			SetPartWeightAttributes(data.Part1, 10);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, location);
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the available weight equal to 20.", 20m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
		}

		public void TestCreateCache_AvailableWeight_MultipleProducts_NotFinalised() =>
			TestCreateCache_AvailableWeight_MultipleProducts(false);

		public void TestCreateCache_AvailableWeight_MultipleProducts_Finalised() =>
			TestCreateCache_AvailableWeight_MultipleProducts(true);

		void TestCreateCache_AvailableWeight_MultipleProducts(bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			SetLocationWeightConstraints(location, 100);
			SetPartWeightAttributes(data.Part1, 10);
			SetPartWeightAttributes(data.Part2, 5);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 8, location);
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the available weight equal to 20.", 20m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
		}

		#endregion

		#region TestCreateCache_Volume

		public void TestCreateCache_AvailableVolume_SingleDocketLine_NotFinalised() =>
			TestCreateCache_AvailableVolume_SingleDocketLine(false);

		public void TestCreateCache_AvailableVolume_SingleDocketLine_Finalised() =>
			TestCreateCache_AvailableVolume_SingleDocketLine(true);

		void TestCreateCache_AvailableVolume_SingleDocketLine(bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			SetLocationCubicConstraints(location, 100);
			SetPartCubicAttributes(data.Part1, 10);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8, location);
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the available Volume equal to 20.", 20m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);
		}

		public void TestCreateCache_AvailableVolume_MultipleDocketLine_NotFinalised() =>
			TestCreateCache_AvailableVolume_MultipleDocketLine(false);

		public void TestCreateCache_AvailableVolume_MultipleDocketLine_Finalised() =>
			TestCreateCache_AvailableVolume_MultipleDocketLine(true);

		void TestCreateCache_AvailableVolume_MultipleDocketLine(bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			SetLocationCubicConstraints(location, 100);
			SetPartCubicAttributes(data.Part1, 10);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 3, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, location);
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the available Volume equal to 20.", 20m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);
		}

		public void TestCreateCache_AvailableVolume_MultipleProducts_NotFinalised() =>
			TestCreateCache_AvailableVolume_MultipleProducts(false);

		public void TestCreateCache_AvailableVolume_MultipleProducts_Finalised() =>
			TestCreateCache_AvailableVolume_MultipleProducts(true);

		void TestCreateCache_AvailableVolume_MultipleProducts(bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			SetLocationCubicConstraints(location, 100);
			SetPartCubicAttributes(data.Part1, 10);
			SetPartCubicAttributes(data.Part2, 5);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 4, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 8, location);
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the available Volume equal to 20.", 20m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);
		}

		#endregion

		#region TestCreateCache_StockOnHand

		public void TestCreateCache_StockOnHand_SingleDocketLine_NotFinalised() =>
			TestCreateCache_StockOnHand_SingleDocketLine(false, null, false);

		public void TestCreateCache_StockOnHand_SingleDocketLine_Finalised() =>
			TestCreateCache_StockOnHand_SingleDocketLine(true, null, false);

		public void TestCreateCache_StockOnHand_SingleDocketLine_NotFinalised_AdditionalOwner() =>
			TestCreateCache_StockOnHand_SingleDocketLine(false, OrgPartRelation.RelationshipTypes.Owner, false);

		public void TestCreateCache_StockOnHand_SingleDocketLine_Finalised_AdditionalOwner() =>
			TestCreateCache_StockOnHand_SingleDocketLine(true, OrgPartRelation.RelationshipTypes.Owner, false);

		public void TestCreateCache_StockOnHand_SingleDocketLine_NotFinalised_AdditionalBoth() =>
			TestCreateCache_StockOnHand_SingleDocketLine(false, OrgPartRelation.RelationshipTypes.Both, false);

		public void TestCreateCache_StockOnHand_SingleDocketLine_Finalised_AdditionalBoth() =>
			TestCreateCache_StockOnHand_SingleDocketLine(true, OrgPartRelation.RelationshipTypes.Both, false);

		public void TestCreateCache_StockOnHand_SingleDocketLine_NotFinalised_ManyLocations() =>
			TestCreateCache_StockOnHand_SingleDocketLine(false, null, true);

		public void TestCreateCache_StockOnHand_SingleDocketLine_Finalised_ManyLocations() =>
			TestCreateCache_StockOnHand_SingleDocketLine(true, null, true);

		public void TestCreateCache_StockOnHand_SingleDocketLine_NotFinalised_AdditionalOwner_ManyLocations() =>
			TestCreateCache_StockOnHand_SingleDocketLine(false, OrgPartRelation.RelationshipTypes.Owner, true);

		public void TestCreateCache_StockOnHand_SingleDocketLine_Finalised_AdditionalOwner_ManyLocations() =>
			TestCreateCache_StockOnHand_SingleDocketLine(true, OrgPartRelation.RelationshipTypes.Owner, true);

		public void TestCreateCache_StockOnHand_SingleDocketLine_NotFinalised_AdditionalBoth_ManyLocations() =>
			TestCreateCache_StockOnHand_SingleDocketLine(false, OrgPartRelation.RelationshipTypes.Both, true);

		public void TestCreateCache_StockOnHand_SingleDocketLine_Finalised_AdditionalBoth_ManyLocations() =>
			TestCreateCache_StockOnHand_SingleDocketLine(true, OrgPartRelation.RelationshipTypes.Both, true);

		void TestCreateCache_StockOnHand_SingleDocketLine(bool finalised, string relationType, bool multipleLocations)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_MaxQuantity = 100m;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1_1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, location);
			if (multipleLocations)
			{
				var location2 = data.Whs1.FindLocation("A-2");
				var receiveLine1_2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 7, location2);
			}

			var manyOrg = relationType != null;
			if (manyOrg)
			{
				var org2 = Helper.CreateClient("JJL");
				Helper.CreateProductClientRelationShip(org2, data.Part1, relationType);
				var receive2 = Helper.CreateWhsReceive(org2, data.Whs1);
				var receiveLine2_1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5, location);
				if (multipleLocations)
				{
					var location2 = data.Whs1.FindLocation("A-2");
					var receiveLine1_2 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 7, location2);
				}

				ConditionalFinaliseWithAssertion(receive2, finalised);
			}

			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			var expectedStockOnHand = manyOrg ? 15m : 10m;
			AssertEquals(
				$"Should return one WhsPutawayLocationCache with the stock on hand for that location equal to {expectedStockOnHand}.",
				expectedStockOnHand, cache[0][WhsPutawayLocationCacheSchema.WPC_StockOnHand.Name]);
			AssertEquals($"Should return one WhsPutawayLocationCache with the appropriate available quantity.",
				100m - expectedStockOnHand, cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
		}

		public void TestCreateCache_StockOnHand_MultipleDocketLine_NotFinalised() =>
			TestCreateCache_StockOnHand_MultipleDocketLine(false);

		public void TestCreateCache_StockOnHand_MultipleDocketLine_Finalised() =>
			TestCreateCache_StockOnHand_MultipleDocketLine(true);

		void TestCreateCache_StockOnHand_MultipleDocketLine(bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_MaxQuantity = 100m;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2.5, location);
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with the stock on hand for that location equal to 12.5.",
				12.5m, cache[0][WhsPutawayLocationCacheSchema.WPC_StockOnHand.Name]);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with the available quantity for that location equal to 87.5.",
				87.5m, cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
		}

		public void TestCreateCache_StockOnHand_MultipleProducts_NotFinalised() =>
			TestCreateCache_StockOnHand_MultipleProducts(false);

		public void TestCreateCache_StockOnHand_MultipleProducts_Finalised() =>
			TestCreateCache_StockOnHand_MultipleProducts(true);

		void TestCreateCache_StockOnHand_MultipleProducts(bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			location.WLV_MaxQuantity = 100m;
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 15m, location);
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with the stock on hand for that location equal to 25.", 25m,
				cache[0][WhsPutawayLocationCacheSchema.WPC_StockOnHand.Name]);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with the available quantity for that location equal to 75.",
				75m, cache[0][WhsPutawayLocationCacheSchema.WPC_AvailableQuantity.Name]);
		}

		public void TestCreateCache_StockOnHandData_SingleDocketLine_NotFinalised() =>
			TestCreateCache_StockOnHandData_SingleDocketLine(false, null);

		public void TestCreateCache_StockOnHandData_SingleDocketLine_Finalised() =>
			TestCreateCache_StockOnHandData_SingleDocketLine(true, null);

		public void TestCreateCache_StockOnHandData_SingleDocketLine_NotFinalised_AdditionalOwners() =>
			TestCreateCache_StockOnHandData_SingleDocketLine(false, OrgPartRelation.RelationshipTypes.Owner);

		public void TestCreateCache_StockOnHandData_SingleDocketLine_Finalised_AdditionalOwners() =>
			TestCreateCache_StockOnHandData_SingleDocketLine(true, OrgPartRelation.RelationshipTypes.Owner);

		public void TestCreateCache_StockOnHandData_SingleDocketLine_NotFinalised_AdditionalBoth() =>
			TestCreateCache_StockOnHandData_SingleDocketLine(false, OrgPartRelation.RelationshipTypes.Both);

		public void TestCreateCache_StockOnHandData_SingleDocketLine_Finalised_AdditionalBoth() =>
			TestCreateCache_StockOnHandData_SingleDocketLine(true, OrgPartRelation.RelationshipTypes.Both);

		void TestCreateCache_StockOnHandData_SingleDocketLine(bool finalised, string relationType)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, location);
			OrgPartRelation relation2 = null;
			var manyOrg = relationType != null;
			if (manyOrg)
			{
				var org2 = Helper.CreateClient("JJL");
				relation2 = Helper.CreateProductClientRelationShip(org2, data.Part1, relationType);
				var receive2 = Helper.CreateWhsReceive(org2, data.Whs1);
				var receiveLine2_1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5, location);
				ConditionalFinaliseWithAssertion(receive2, finalised);
			}

			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			var relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var deserialisedJson =
				JsonConvert.DeserializeObject<Dictionary<ZGuid, ZDecimal>>(
					(string)cache[0][WhsPutawayLocationCacheSchema.WPC_StockOnHandData.Name]);

			AssertEquals("Should return one WhsPutawayLocationCache with the stock on hand data for only one product.",
				manyOrg.ToInt() + 1, deserialisedJson.Count);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with the stock on hand data for only the product set to the correct total.",
				10m, deserialisedJson[relation.PK]);
			if (manyOrg)
			{
				AssertEquals(
					"Should return one WhsPutawayLocationCache with the stock on hand data for only the product set to the correct total.",
					5m, deserialisedJson[relation2.PK]);
			}
		}

		public void TestCreateCache_StockOnHandData_MultipleProducts_NotFinalised() =>
			TestCreateCache_StockOnHandData_MultipleProducts(false);

		public void TestCreateCache_StockOnHandData_MultipleProducts_Finalised() =>
			TestCreateCache_StockOnHandData_MultipleProducts(true);

		void TestCreateCache_StockOnHandData_MultipleProducts(bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 5, location);
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			var relation1 =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var relation2 =
				data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var deserialisedJson =
				JsonConvert.DeserializeObject<Dictionary<ZGuid, ZDecimal>>(
					(string)cache[0][WhsPutawayLocationCacheSchema.WPC_StockOnHandData.Name]);

			AssertEquals("Should return one WhsPutawayLocationCache with the stock on hand data for two products.", 2,
				deserialisedJson.Count);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with the stock on hand data for only the product set to the correct total.",
				10m, deserialisedJson[relation1.PK]);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with the stock on hand data for only the product set to the correct total.",
				5m, deserialisedJson[relation2.PK]);
		}

		public void TestCreateCache_StockOnHandData_MultipleDocketLine_NotFinalised() =>
			TestCreateCache_StockOnHandData_MultipleDocketLine(false);

		public void TestCreateCache_StockOnHandData_MultipleDocketLine_Finalised() =>
			TestCreateCache_StockOnHandData_MultipleDocketLine(true);

		void TestCreateCache_StockOnHandData_MultipleDocketLine(bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 2.5, location);
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			var deserialisedJson =
				JsonConvert.DeserializeObject<Dictionary<ZGuid, ZDecimal>>(
					(string)cache[0][WhsPutawayLocationCacheSchema.WPC_StockOnHandData.Name]);
			var relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);

			AssertEquals("Should return one WhsPutawayLocationCache with the stock on hand data for only one product.",
				1, deserialisedJson.Count);
			AssertEquals(
				"Should return one WhsPutawayLocationCache with the stock on hand data for product set to the correct total.",
				12.5m, deserialisedJson[relation.PK]);
		}

		#endregion

		#region TestCreateCache_ProductData

		public void TestCreateCache_ProductData_SingleDocketLine_NotFinalised() =>
			TestCreateCache_ProductData_SingleDocketLine(false);

		public void TestCreateCache_ProductData_SingleDocketLine_Finalised() =>
			TestCreateCache_ProductData_SingleDocketLine(true);

		public void TestCreateCache_ProductData_SingleDocketLine_NotFinalised_AdditionalOwner() =>
			TestCreateCache_ProductData_SingleDocketLine(false, OrgPartRelation.RelationshipTypes.Owner);

		public void TestCreateCache_ProductData_SingleDocketLine_Finalised_AdditionalOwner() =>
			TestCreateCache_ProductData_SingleDocketLine(true, OrgPartRelation.RelationshipTypes.Owner);

		public void TestCreateCache_ProductData_SingleDocketLine_NotFinalised_AdditionalBoth() =>
			TestCreateCache_ProductData_SingleDocketLine(false, OrgPartRelation.RelationshipTypes.Both);

		public void TestCreateCache_ProductData_SingleDocketLine_Finalised_AdditionalBoth() =>
			TestCreateCache_ProductData_SingleDocketLine(true, OrgPartRelation.RelationshipTypes.Both);

		void TestCreateCache_ProductData_SingleDocketLine(bool finalised, string additionalRelationType = null)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 9, location);
			OrgPartRelation relation2 = null;
			var manyOrg = additionalRelationType != null;
			if (additionalRelationType != null)
			{
				var org2 = Helper.CreateClient("JJL");
				relation2 = Helper.CreateProductClientRelationShip(org2, data.Part1, additionalRelationType);
				var receive2 = Helper.CreateWhsReceive(org2, data.Whs1);
				var receiveLine2_1 = Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5, location);
				ConditionalFinaliseWithAssertion(receive2, finalised);
			}

			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			var relation1 =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var productData = JArray.Parse((string)cache[0][WhsPutawayLocationCacheSchema.WPC_ProductData.Name])
				.Select(x => new ZGuid((string)x)).ToList();

			AssertEquals("Should return one WhsPutawayLocationCache with the only one product pk in the ProductData.",
				manyOrg.ToInt() + 1, productData.Count);
			Assert(
				"Should return one WhsPutawayLocationCache with only product pk in the ProductData maching the product.",
				productData.Contains(relation1.PK));
			if (manyOrg)
			{
				Assert(
					"Should return one WhsPutawayLocationCache with only product pk in the ProductData maching the product.",
					productData.Contains(relation2.PK));
			}
		}

		public void TestCreateCache_ProductData_MultipleDocketLine_NotFinalised() =>
			TestCreateCache_ProductData_MultipleDocketLine(false);

		public void TestCreateCache_ProductData_MultipleDocketLine_Finalised() =>
			TestCreateCache_ProductData_MultipleDocketLine(true);

		void TestCreateCache_ProductData_MultipleDocketLine(bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			SetLocationCubicConstraints(location, 100);
			SetPartCubicAttributes(data.Part1, 5);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 9, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 8, location);
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should return one WhsPutawayLocationCache with the only one product pk in the ProductData.",
				1, JArray.Parse((string)cache[0][WhsPutawayLocationCacheSchema.WPC_ProductData.Name]).Count);
		}

		public void TestCreateCache_ProductData_MultipleProducts_NotFinalised() =>
			TestCreateCache_ProductData_MultipleProducts(false);

		public void TestCreateCache_ProductData_MultipleProducts_Finalised() =>
			TestCreateCache_ProductData_MultipleProducts(true);

		void TestCreateCache_ProductData_MultipleProducts(bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			SetLocationCubicConstraints(location, 100);
			SetPartCubicAttributes(data.Part1, 10);
			SetPartCubicAttributes(data.Part2, 5);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 5, location);
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 8, location);
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			var productData = JArray.Parse((string)cache[0][WhsPutawayLocationCacheSchema.WPC_ProductData.Name])
				.Select(x => new ZGuid((string)x)).ToList();
			var relation1 =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var relation2 =
				data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);

			AssertEquals("Should return one WhsPutawayLocationCache should contain 2 product pks in ProductData.", 2,
				productData.Count);
			AssertContainsExactElementsInAnyOrder(new[] { relation1.PK, relation2.PK }, productData);
		}

		#endregion

		#region TestCreateCache_JsonAttributeDictionaryColumns

		public void TestCreateCache_JsonAttributeDictionaryColumns_SingleDocketLine_NotFinalised() =>
			TestCreateCache_JsonAttributeDictionaryColumns_SingleDocketLine(false, null);

		public void TestCreateCache_JsonAttributeDictionaryColumns_SingleDocketLine_Finalised() =>
			TestCreateCache_JsonAttributeDictionaryColumns_SingleDocketLine(true, null);

		public void TestCreateCache_JsonAttributeDictionaryColumns_SingleDocketLine_NotFinalised_AdditionalOwner() =>
			TestCreateCache_JsonAttributeDictionaryColumns_SingleDocketLine(false,
				OrgPartRelation.RelationshipTypes.Owner);

		public void TestCreateCache_JsonAttributeDictionaryColumns_SingleDocketLine_Finalised_AdditionalOwner() =>
			TestCreateCache_JsonAttributeDictionaryColumns_SingleDocketLine(true,
				OrgPartRelation.RelationshipTypes.Owner);

		public void TestCreateCache_JsonAttributeDictionaryColumns_SingleDocketLine_NotFinalised_AdditionalBoth() =>
			TestCreateCache_JsonAttributeDictionaryColumns_SingleDocketLine(false,
				OrgPartRelation.RelationshipTypes.Both);

		public void TestCreateCache_JsonAttributeDictionaryColumns_SingleDocketLine_Finalised_AdditionalBoth() =>
			TestCreateCache_JsonAttributeDictionaryColumns_SingleDocketLine(true,
				OrgPartRelation.RelationshipTypes.Both);

		void TestCreateCache_JsonAttributeDictionaryColumns_SingleDocketLine(bool finalised, string relationType)
		{
			var pa1 = "A";
			var pa2 = "B";
			var pa3 = "C";
			var expiryDate = ZDateTime.Now.Date;
			var packingDate = expiryDate.AddDays(-1);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, location, expiryDate, packingDate, pa1, pa2,
				pa3, "");

			OrgPartRelation relation2 = null;
			var manyOrg = relationType != null;
			if (manyOrg)
			{
				var org2 = Helper.CreateClient("JJL");
				relation2 = Helper.CreateProductClientRelationShip(org2, data.Part1, relationType);
				Helper.SetClientAllAttributeType(org2, true);
				Helper.SetProductAllAttributeUse(org2, data.Part1, use: true, setReleaseCaptured: false,
					useSerialNumber: false);
				var receive2 = Helper.CreateWhsReceive(org2, data.Whs1);
				Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 5, location, expiryDate, packingDate, pa1,
					pa2, pa3, "");
				ConditionalFinaliseWithAssertion(receive2, finalised);
			}

			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			var relation1 =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);

			var numberOfAttributes = Enumerable.Repeat(1, manyOrg.ToInt() + 1);
			var relations = new List<ZGuid> { relation1.PK };
			if (manyOrg)
			{
				relations.Add(relation2.PK);
			}

			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute1Data.Name,
				relations, Enumerable.Repeat(pa1, manyOrg.ToInt() + 1), numberOfAttributes);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute2Data.Name,
				relations, Enumerable.Repeat(pa2, manyOrg.ToInt() + 1), numberOfAttributes);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute3Data.Name,
				relations, Enumerable.Repeat(pa3, manyOrg.ToInt() + 1), numberOfAttributes);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_ExpiryDateData.Name,
				relations, Enumerable.Repeat(expiryDate.ToString("yyyy-MM-dd"), manyOrg.ToInt() + 1),
				numberOfAttributes);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PackingDateData.Name,
				relations, Enumerable.Repeat(packingDate.ToString("yyyy-MM-dd"), manyOrg.ToInt() + 1),
				numberOfAttributes);
		}

		public void TestCreateCache_JsonAttributeDictionaryColumns_MultipleProducts_NotFinalised() =>
			TestCreateCache_JsonAttributeDictionaryColumns_MultipleProducts(false);

		public void TestCreateCache_JsonAttributeDictionaryColumns_MultipleProducts_Finalised() =>
			TestCreateCache_JsonAttributeDictionaryColumns_MultipleProducts(true);

		void TestCreateCache_JsonAttributeDictionaryColumns_MultipleProducts(bool finalised)
		{
			var pa1 = "A";
			var pa2 = "B";
			var pa3 = "C";
			var expiryDate = ZDateTime.Now.Date;
			var packingDate = expiryDate.AddDays(-1);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, location);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1, location, expiryDate, packingDate, pa1, pa2,
				pa3, "");
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			var relation1 =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var relation2 =
				data.Part2.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			var relations = new ZGuid[] { relation1.PK, relation2.PK };
			var numberOfAttributes = new[] { 0, 1 };

			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute1Data.Name,
				relations, new[] { "", pa1 }, numberOfAttributes);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute2Data.Name,
				relations, new[] { "", pa2 }, numberOfAttributes);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute3Data.Name,
				relations, new[] { "", pa3 }, numberOfAttributes);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_ExpiryDateData.Name,
				relations, new[] { "", expiryDate.ToString("yyyy-MM-dd") }, numberOfAttributes);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PackingDateData.Name,
				relations, new[] { "", packingDate.ToString("yyyy-MM-dd") }, numberOfAttributes);
		}

		public void TestCreateCache_JsonAttributeDictionaryColumns_NoAttributes_NotFinalised() =>
			TestCreateCache_JsonAttributeDictionaryColumns_NoAttributes(false);

		public void TestCreateCache_JsonAttributeDictionaryColumns_NoAttributes_Finalised() =>
			TestCreateCache_JsonAttributeDictionaryColumns_NoAttributes(true);

		void TestCreateCache_JsonAttributeDictionaryColumns_NoAttributes(bool finalised)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, location);
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			var relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute1Data.Name,
				relation.PK, "", 0);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute2Data.Name,
				relation.PK, "", 0);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute3Data.Name,
				relation.PK, "", 0);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_ExpiryDateData.Name,
				relation.PK, "", 0);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PackingDateData.Name,
				relation.PK, "", 0);
		}

		public void
			TestCreateCache_JsonAttributeDictionaryColumns_RepeatedAttributes_MultipleDocketLines_NotFinalised() =>
			TestCreateCache_JsonAttributeDictionaryColumns_RepeatedAttributes(false);

		public void TestCreateCache_JsonAttributeDictionaryColumns_RepeatedAttributes_MultipleDocketLines_Finalised() =>
			TestCreateCache_JsonAttributeDictionaryColumns_RepeatedAttributes(true);

		void TestCreateCache_JsonAttributeDictionaryColumns_RepeatedAttributes(bool finalised)
		{
			var pa1 = "A";
			var pa2 = "B";
			var pa3 = "C";
			var expiryDate = ZDateTime.Now.Date;
			var packingDate = expiryDate.AddDays(-1);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, location, expiryDate, packingDate, pa1, pa2,
				pa3, "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, location, expiryDate, packingDate, pa1, pa2,
				pa3, "");
			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			var relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute1Data.Name,
				relation.PK, pa1);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute2Data.Name,
				relation.PK, pa2);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute3Data.Name,
				relation.PK, pa3);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_ExpiryDateData.Name,
				relation.PK, expiryDate.ToString("yyyy-MM-dd"));
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PackingDateData.Name,
				relation.PK, packingDate.ToString("yyyy-MM-dd"));
		}

		public void TestCreateCache_JsonAttributeDictionaryColumns_MultipleAttributes_NotFinalised() =>
			TestCreateCache_JsonAttributeDictionaryColumns_MultipleAttributes(false);

		public void TestCreateCache_JsonAttributeDictionaryColumns_MultipleAttributes_Finalised() =>
			TestCreateCache_JsonAttributeDictionaryColumns_MultipleAttributes(true);

		void TestCreateCache_JsonAttributeDictionaryColumns_MultipleAttributes(bool finalised)
		{
			var pa1_1 = "A";
			var pa2_1 = "B";
			var pa3_1 = "C";
			var expiryDate_1 = ZDateTime.Now.Date;
			var packingDate_1 = expiryDate_1.AddDays(-1);
			var pa1_2 = "D";
			var pa2_2 = "E";
			var pa3_2 = "F";
			var expiryDate_2 = packingDate_1.AddDays(-1);
			var packingDate_2 = expiryDate_2.AddDays(-1);

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, location, expiryDate_1, packingDate_1, pa1_1,
				pa2_1, pa3_1, "");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1, location, expiryDate_2, packingDate_2, pa1_2,
				pa2_2, pa3_2, "");

			ConditionalFinaliseWithAssertion(receive, finalised);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			var relation =
				data.Part1.RelatedOrganisations.FindByOrganisationAndRelationship(data.Org1,
					OrgPartRelation.RelationshipTypes.Owner);

			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute1Data.Name,
				relation.PK, "", 2);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute2Data.Name,
				relation.PK, "", 2);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PartAttribute3Data.Name,
				relation.PK, "", 2);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_ExpiryDateData.Name,
				relation.PK, "", 2);
			AssertJsonAttributeDictionaryColumn(cache, WhsPutawayLocationCacheSchema.WPC_PackingDateData.Name,
				relation.PK, "", 2);
		}

		#endregion

		#region TestCreateCache_AuditColumns

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2020, 2, 20, 20, 20, 00)]
		public void TestCreateCache_AuditColumns()
		{
			TestDateAttribute.UseUNLOCO = true;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should have set WPC_SystemCreateTimeUtc to Utc time.", ZDateTime.UtcNow,
				cache[0][WhsPutawayLocationCacheSchema.WPC_SystemCreateTimeUtc.Name]);
			AssertEquals("Should have set WPC_SystemLastEditTimeUtc to Utc time.", ZDateTime.UtcNow,
				cache[0][WhsPutawayLocationCacheSchema.WPC_SystemLastEditTimeUtc.Name]);
			AssertEquals("Should have set WPC_SystemCreateUser to Utc time.", "~BP",
				cache[0][WhsPutawayLocationCacheSchema.WPC_SystemCreateUser.Name]);
			AssertEquals("Should have set WPC_SystemLastEditUser to Utc time.", "~BP",
				cache[0][WhsPutawayLocationCacheSchema.WPC_SystemLastEditUser.Name]);
		}

		#endregion

		#region TestCreateCache_SystemLastEditTimeUtc

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2020, 2, 20, 20, 20, 00)]
		public void TestCreateCache_SystemLastEditTimeUtc()
		{
			TestDateAttribute.UseUNLOCO = true;
			var now = ZDateTime.UtcNow;

			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 10m);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should have set WPC_SystemCreateTimeUtc to Utc time.", now,
				cache[0][WhsPutawayLocationCacheSchema.WPC_SystemCreateTimeUtc.Name]);
			AssertEquals("Should have set WPC_SystemLastEditTimeUtc to Utc time.", now,
				cache[0][WhsPutawayLocationCacheSchema.WPC_SystemLastEditTimeUtc.Name]);
			AssertEquals("Should have set WPC_SystemCreateUser to Utc time.", "~BP",
				cache[0][WhsPutawayLocationCacheSchema.WPC_SystemCreateUser.Name]);
			AssertEquals("Should have set WPC_SystemLastEditUser to Utc time.", "~BP",
				cache[0][WhsPutawayLocationCacheSchema.WPC_SystemLastEditUser.Name]);

			TestDateAttribute.AddDays(1);

			location.Row.WR_Name = "BLA"; // any change to update cache
			Factory.Save();

			var updatedCache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("No change.", now,
				updatedCache[0][WhsPutawayLocationCacheSchema.WPC_SystemCreateTimeUtc.Name]);
			AssertEquals("Should update.", now.AddDays(1),
				updatedCache[0][WhsPutawayLocationCacheSchema.WPC_SystemLastEditTimeUtc.Name]);
			AssertEquals("No change.", "~BP",
				updatedCache[0][WhsPutawayLocationCacheSchema.WPC_SystemCreateUser.Name]);
			AssertEquals("No change.", "~BP",
				updatedCache[0][WhsPutawayLocationCacheSchema.WPC_SystemLastEditUser.Name]);
		}

		#endregion

		#region TestGetCache_SkipLocations

		public void TestGetCache_SkipLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse1 = data.Whs1;
			Factory.Save();

			var cacheManager = new WhsPutawayLocationCacheManager();
			var location1 = warehouse1.FindLocation("A-1");
			var location2 = warehouse1.FindLocation("A-2");
			var locationPks = new[] { location1.PK, location2.PK };
			cacheManager.CreateCache(Factory, locationPks);

			var cache1 = cacheManager.GetCache(Factory, warehouse1.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }, new[] { location1.PK }).ToArray();
			AssertEquals("Should return 1 WhsPutawayLocationCache.", 1, cache1.Length);
			AssertEquals("Should return the location2", location2.PK, cache1[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]);
		}

		public void TestGetCache_SkipLocations_PartialPallets()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var cacheManager = new WhsPutawayLocationCacheManager();
			var location1 = warehouse.FindLocation("A-1");
			var location2 = warehouse.FindLocation("A-2");
			location1.WLV_MaxQuantity = 10m;
			location2.WLV_MaxQuantity = 10m;

			var locationPks = new[] { location1.PK, location2.PK };
			cacheManager.CreateCache(Factory, locationPks);
			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m, location1, "PLT-123");
			Factory.Save();

			var cache1 = cacheManager.GetCache(Factory, warehouse.PK, new[] { data.Org1.PK }, new[] { data.Part1.PK }, new[] { location1.PK }).ToArray();
			AssertEquals("Should return 1 WhsPutawayLocationCache.", 1, cache1.Length);
			AssertEquals("Should return the location2", location2.PK, cache1[0][WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]);
		}

		#endregion

		#region TestCreateCache_LastAllocatedOrChangedID

		public void TestCreateCache_LastAllocatedOrChangedID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Factory.Save();

			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should have set LastAllocatedOrChangedID of new record to the location change ID.", location.WLV_LastAllocatedOrChangedID,
				cache[0][WhsPutawayLocationCacheSchema.WPC_LastAllocatedOrChangedID.Name]);

			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			Factory.Save();

			cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);
			AssertEquals("Should have updated LastAllocatedOrChangedID of existing record to the location change ID.", location.WLV_LastAllocatedOrChangedID,
				cache[0][WhsPutawayLocationCacheSchema.WPC_LastAllocatedOrChangedID.Name]);
		}

		public void TestCreateCache_LastAllocatedOrChangedID_PartialPallets()
		{
			var data = new TestDataSimpleEnvironment(Factory, 1, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A");
			location.WLV_MaxQuantity = 10m;
			Factory.Save();

			AddUnitConversionForPart(data.Part1, Constants.PkgUnit.Unit, Constants.PkgUnit.Pallet, 15m);

			var receive1 = Helper.CreateWhsReceive(data.Org1, warehouse);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 5m, location, "PLT-123");
			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);

			AssertEquals("Precondition: Should create two WhsPutawayLocationCache records.", 2, cache.Count);
			AssertEquals("Should have set LastAllocatedOrChangedID to the location change ID.", location.WLV_LastAllocatedOrChangedID,
				cache[0][WhsPutawayLocationCacheSchema.WPC_LastAllocatedOrChangedID.Name]);
			AssertEquals("Should have set LastAllocatedOrChangedID to the location change ID.", location.WLV_LastAllocatedOrChangedID,
				cache[1][WhsPutawayLocationCacheSchema.WPC_LastAllocatedOrChangedID.Name]);
		}

		public void TestCreateCache_LastAllocatedOrChangedID_PickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			Helper.CreateProductPickFace(data.Part1, data.Org1, location, replenishMax: 10m);
			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);

			AssertEquals("Should return 1 WhsPutawayLocationCache record.", 1, cache.Count);
			AssertEquals("Should have set LastAllocatedOrChangedID to the location change ID.", location.WLV_LastAllocatedOrChangedID,
				cache[0][WhsPutawayLocationCacheSchema.WPC_LastAllocatedOrChangedID.Name]);
		}

		public void TestCreateCache_LastAllocatedOrChangedID_DynamicPickFace()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var warehouse = data.Whs1;
			var location = warehouse.FindLocation("A-1");
			var dynamicArea = Helper.CreateArea(warehouse, "DYNAMIC", AreaTypes.Codes.DynamicPickFace, true, false);
			var dynamicLocationType = Helper.CreateLocationType("DGD", LocationClasses.Codes.DPF);
			location.WLV_WLT_LocationType = dynamicLocationType.PK;
			location.WLV_WA_PickingArea = dynamicArea.PK;
			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			var whsProduct = WhsProduct.GetWhsProduct(data.Part1);
			AddParamsByWhsAndClient(whsProduct, data.Org1, warehouse, dynamicArea);
			Factory.Save();

			var cache = CreateAndGetCache(Factory, data.Whs1.PK, location.PK);

			AssertEquals("Should create one WhsPutawayLocationCache.", 1, cache.Count);
			AssertEquals("Should have set LastAllocatedOrChangedID to the location change ID.", location.WLV_LastAllocatedOrChangedID,
				cache[0][WhsPutawayLocationCacheSchema.WPC_LastAllocatedOrChangedID.Name]);
		}

		#endregion

		#region Implementation

		static IReadOnlyList<DataRow> GetCacheForLocation(BusinessObjectFactory factory, ZGuid warehousePK,
			ZGuid locationPK)
			=> GetCacheForLocations(factory, warehousePK, new[] { locationPK });

		static IReadOnlyList<DataRow> GetCacheForLocations(BusinessObjectFactory factory, ZGuid warehousePK,
			IEnumerable<ZGuid> locationPKs)
		{
			var locationPksHash = new HashSet<ZGuid>(locationPKs);
			return WhsPutawayLocationCacheTestHelper.GetAllCacheRecords(factory, warehousePK).Where(c =>
				locationPksHash.Contains(((Guid)c[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]))).ToArray();
		}

		static IReadOnlyList<DataRow> CreateAndGetCache(BusinessObjectFactory factory, ZGuid warehousePK,
			ZGuid locationPK) => CreateAndGetCache(factory, warehousePK, new[] { locationPK });

		static IReadOnlyList<DataRow> CreateAndGetCache(BusinessObjectFactory factory, ZGuid warehousePK,
			IEnumerable<ZGuid> locationPK)
		{
			new WhsPutawayLocationCacheManager().CreateCache(factory, locationPK);
			return GetCacheForLocations(factory, warehousePK, locationPK);
		}

		void DeleteTableRow(string tableName, string columnName, ZGuid key)
		{
			var sql = $"DELETE FROM {tableName} WHERE {columnName} = '{key}';";
			var command = ((IDbConnected)Factory).Connection.Command(sql);
			command.ExecuteNonQuery();
		}

		void AddUnitConversionForPart(OrgSupplierPart product, string packType, string parentPackType,
			ZDecimal quantityInParent)
		{
			var unitConversionForUNTToPLT = product.PartUnits.AddNew();
			unitConversionForUNTToPLT.OF_PackType = packType;
			unitConversionForUNTToPLT.OF_ParentPackType = parentPackType;
			unitConversionForUNTToPLT.OF_QuantityInParent = quantityInParent;
		}

		WhsProductParamsByWhsAndClient AddParamsByWhsAndClient(WhsProduct product, OrgHeader org,
			WhsWarehouse warehouse, WhsArea area)
		{
			var productParams = Helper.CreateProductParamsByWhsAndClient(product.Parent, org, warehouse);
			productParams.W3_WA_DynamicPickFaceArea = area.PK;
			return productParams;
		}

		void SetPartWeightAndCubicAttributes(OrgSupplierPart part, ZDecimal weight, ZDecimal cubic,
			string weightUQ = "KG", string cubicUQ = "M3")
		{
			SetPartWeightAttributes(part, weight, weightUQ);
			SetPartCubicAttributes(part, cubic, cubicUQ);
		}

		void SetPartWeightAttributes(OrgSupplierPart part, ZDecimal weight, string weightUQ = "KG")
		{
			part.OP_Weight = weight;
			part.OP_WeightUQ = weightUQ;
		}

		void SetPartCubicAttributes(OrgSupplierPart part, ZDecimal cubic, string cubicUQ = "M3")
		{
			part.OP_Cubic = cubic;
			part.OP_CubicUQ = cubicUQ;
		}

		void SetLocationWeightAndCubicConstraints(WhsLocation location, ZDecimal weight, ZDecimal cubic,
			string weightUQ = "KG", string cubicUQ = "M3")
		{
			SetLocationWeightConstraints(location, weight, weightUQ);
			SetLocationCubicConstraints(location, cubic, cubicUQ);
		}

		void SetLocationWeightConstraints(WhsLocation location, ZDecimal weight, string weightUQ = "KG")
		{
			location.WLV_MaxWeight = weight;
			location.WLV_MaxWeightUnit = weightUQ;
		}

		void SetLocationCubicConstraints(WhsLocation location, ZDecimal cubic, string cubicUQ = "M3")
		{
			location.WLV_MaxCubic = cubic;
			location.WLV_MaxCubicUnit = cubicUQ;
		}

		void ConditionalFinaliseWithAssertion(WhsReceive receive, bool finalised)
		{
			if (finalised)
			{
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
			}
		}

		void AssertAllComputedColumns(IReadOnlyList<DataRow> cache, ZDecimal unitsExpected, ZGuid relationPK,
			ZDecimal partWeight, ZDecimal capacityWeight, ZDecimal partCubic, ZDecimal capacityCubic,
			string partAttribute1, string partAttribute2, string partAttribute3, ZDate? packingDate, ZDate? expiryDate,
			int expectedNumberOfAttributes = 1, int expectedNumberOfRelations = 1)
			=> AssertAllComputedColumns(cache[0], unitsExpected, relationPK, partWeight, capacityWeight, partCubic,
				capacityCubic, partAttribute1, partAttribute2, partAttribute3, packingDate, expiryDate,
				expectedNumberOfAttributes, expectedNumberOfRelations);

		void AssertAllComputedColumns(DataRow cacheRow, ZDecimal unitsExpected, ZGuid relationPK, ZDecimal partWeight,
			ZDecimal capacityWeight, ZDecimal partCubic, ZDecimal capacityCubic, string partAttribute1,
			string partAttribute2, string partAttribute3, ZDate? packingDate, ZDate? expiryDate,
			int expectedNumberOfAttributes = 1, int expectedNumberOfRelations = 1)
			=> AssertAllComputedColumns(cacheRow, unitsExpected, relationPK,
				capacityWeight - (unitsExpected * partWeight), capacityCubic - (unitsExpected * partCubic),
				partAttribute1, partAttribute2, partAttribute3, packingDate, expiryDate, expectedNumberOfAttributes,
				expectedNumberOfRelations);

		void AssertAllComputedColumns(DataRow cacheRow, ZDecimal unitsExpected, ZGuid relationPK,
			ZDecimal remainingWeight, ZDecimal remainingCubic, string partAttribute1, string partAttribute2,
			string partAttribute3, ZDate? packingDate, ZDate? expiryDate, int expectedNumberOfAttributes = 1,
			int expectedNumberOfRelations = 1)
		{
			AssertEquals(
				$"WhsPutawayLocationCache column stock on hand for that location must be equal to {unitsExpected}.",
				unitsExpected, cacheRow[WhsPutawayLocationCacheSchema.WPC_StockOnHand.Name]);

			var stockOnHandData =
				JsonConvert.DeserializeObject<Dictionary<ZGuid, ZDecimal>>(
					(string)cacheRow[WhsPutawayLocationCacheSchema.WPC_StockOnHandData.Name]);
			AssertEquals(
				$"WhsPutawayLocationCache StockOnHandData should contain {(unitsExpected > 0).ToInt()} item(s).",
				(unitsExpected > 0).ToInt(), stockOnHandData.Count);

			var productData = JArray.Parse((string)cacheRow[WhsPutawayLocationCacheSchema.WPC_ProductData.Name])
				.Select(x => new ZGuid((string)x)).ToList();
			AssertEquals($"WhsPutawayLocationCache ProductData should contain {(unitsExpected > 0).ToInt()} item(s).",
				(unitsExpected > 0).ToInt(), productData.Count);

			if (relationPK.IsValid)
			{
				Assert("WhsPutawayLocationCache column must contain this relation pk.",
					productData.Contains(relationPK));
				AssertEquals(
					"WhsPutawayLocationCache column stock on hand data must have the correct total for a given relation pk.",
					unitsExpected, stockOnHandData[relationPK]);
			}

			AssertEquals($"WhsPutawayLocationCache row must have available weight equal to {remainingWeight}.",
				remainingWeight, cacheRow[WhsPutawayLocationCacheSchema.WPC_AvailableWeight.Name]);
			AssertEquals($"WhsPutawayLocationCache must have available Volume equal to {remainingCubic}.",
				remainingCubic, cacheRow[WhsPutawayLocationCacheSchema.WPC_AvailableVolume.Name]);

			AssertJsonAttributeDictionaryColumn(cacheRow, WhsPutawayLocationCacheSchema.WPC_PartAttribute1Data.Name,
				relationPK, partAttribute1, expectedNumberOfAttributes: expectedNumberOfAttributes,
				expectedNumberOfRelations: expectedNumberOfRelations);
			AssertJsonAttributeDictionaryColumn(cacheRow, WhsPutawayLocationCacheSchema.WPC_PartAttribute2Data.Name,
				relationPK, partAttribute2, expectedNumberOfAttributes: expectedNumberOfAttributes,
				expectedNumberOfRelations: expectedNumberOfRelations);
			AssertJsonAttributeDictionaryColumn(cacheRow, WhsPutawayLocationCacheSchema.WPC_PartAttribute3Data.Name,
				relationPK, partAttribute3, expectedNumberOfAttributes: expectedNumberOfAttributes,
				expectedNumberOfRelations: expectedNumberOfRelations);
			AssertJsonAttributeDictionaryColumn(cacheRow, WhsPutawayLocationCacheSchema.WPC_ExpiryDateData.Name,
				relationPK, expiryDate?.ToString("yyyy-MM-dd"), expectedNumberOfAttributes: expectedNumberOfAttributes,
				expectedNumberOfRelations: expectedNumberOfRelations);
			AssertJsonAttributeDictionaryColumn(cacheRow, WhsPutawayLocationCacheSchema.WPC_PackingDateData.Name,
				relationPK, packingDate?.ToString("yyyy-MM-dd"), expectedNumberOfAttributes: expectedNumberOfAttributes,
				expectedNumberOfRelations: expectedNumberOfRelations);
		}

		void AssertJsonAttributeDictionaryColumn(IReadOnlyList<DataRow> cache, string column, ZGuid productRelationPk,
			string expectedAttribute, int expectedNumberOfAttributes = 1)
		{
			if (productRelationPk.IsValid && expectedAttribute != null)
			{
				AssertJsonAttributeDictionaryColumn(cache, column, new[] { productRelationPk },
					new[] { expectedAttribute }, new[] { expectedNumberOfAttributes });
			}
			else
			{
				AssertJsonAttributeDictionaryColumn(cache, column, Array.Empty<ZGuid>(), Array.Empty<string>(), Array.Empty<int>());
			}
		}

		void AssertJsonAttributeDictionaryColumn(IReadOnlyList<DataRow> cache, string column,
			IEnumerable<ZGuid> productRelationPk, IEnumerable<string> expectedAttribute,
			IEnumerable<int> expectedNumberOfAttributes)
		{
			AssertEquals("Precondition: IEnumerable parameters must all have the same number of values.",
				productRelationPk.Count(), expectedAttribute.Count());
			AssertEquals("Precondition: IEnumerable parameters must all have the same number of values.",
				productRelationPk.Count(), expectedNumberOfAttributes.Count());

			for (int i = 0; i < productRelationPk.Count(); i++)
			{
				AssertJsonAttributeDictionaryColumn(cache[0], column, productRelationPk.ElementAt(i),
					expectedAttribute.ElementAt(i), expectedNumberOfAttributes.ElementAt(i), productRelationPk.Count());
			}
		}

		void AssertJsonAttributeDictionaryColumn(DataRow cacheRow, string column, ZGuid productRelationPk,
			string expectedAttribute, int expectedNumberOfAttributes = 1, int expectedNumberOfRelations = 1)
		{
			var deserialisedJson =
				JsonConvert.DeserializeObject<Dictionary<ZGuid, WhsPutawayLocationCacheAttributeData>>(
					(string)cacheRow[column]);
			AssertEquals(
				$"JSON value in WhsPutawayLocationCache column {column} must only contain {expectedNumberOfRelations} relations.",
				expectedNumberOfRelations, deserialisedJson.Count);
			if (expectedNumberOfRelations >= 1)
			{
				Assert($"JSON value in WhsPutawayLocationCache column {column} must contain key {productRelationPk}.",
					deserialisedJson.ContainsKey(productRelationPk));
				AssertEquals(
					$"JSON value in WhsPutawayLocationCache column {column} with key {productRelationPk} must have value {expectedAttribute} as Attribute.",
					expectedAttribute ?? "", deserialisedJson[productRelationPk].Attribute);
				AssertEquals(
					$"JSON value in WhsPutawayLocationCache column {column} with key {productRelationPk} must have value {expectedNumberOfAttributes} as NumberOfAttributes.",
					expectedNumberOfAttributes, deserialisedJson[productRelationPk].NumberOfAttributes);
			}
		}

		IEnumerable<T> GetCacheValuesInColumn<T>(IReadOnlyList<DataRow> cache, string columnName) =>
			cache.Select(row => (T)row[columnName]);

		DataRow GetCacheRow(IReadOnlyList<DataRow> cache, WhsLocation location, string locationCacheType,
			string partialPalletID, OrgSupplierPart product, OrgHeader client) =>
			GetCacheRow(cache, location.PK, locationCacheType, partialPalletID, product.PK, client.PK);

		DataRow GetCacheRow(IReadOnlyList<DataRow> cache, ZGuid locationPk, string locationCacheType,
			string partialPalletID, ZGuid productPk, ZGuid clientPk)
			=> cache.SingleOrDefault(row =>
				GetZGuid(row[WhsPutawayLocationCacheSchema.WPC_WL_Location.Name]).Equals(locationPk) &&
				row[WhsPutawayLocationCacheSchema.WPC_LocationCacheType.Name].Equals(locationCacheType) &&
				row[WhsPutawayLocationCacheSchema.WPC_PartialPalletID.Name].Equals(partialPalletID) &&
				GetZGuid(row[WhsPutawayLocationCacheSchema.WPC_OP_Product.Name]).Equals(productPk) &&
				GetZGuid(row[WhsPutawayLocationCacheSchema.WPC_OH_Client.Name]).Equals(clientPk));

		static ZGuid GetZGuid(object value) => value == DBNull.Value ? ZGuid.Empty : new ZGuid((Guid)value);

		#endregion
	}

	class WhsPutawayLocationCacheAttributeData
	{
		public object Attribute { get; set; }
		public int NumberOfAttributes { get; set; }
	}

	public static class BooleanExtensions
	{
		public static int ToInt(this bool b)
		{
			return b ? 1 : 0;
		}
	}
}
