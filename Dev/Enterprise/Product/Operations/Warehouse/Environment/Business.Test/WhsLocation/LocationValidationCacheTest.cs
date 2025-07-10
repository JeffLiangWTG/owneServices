using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class LocationValidationCacheTest : WhsTestCaseWithFactoryEnv
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationValidationCache(null, Factory.New<WhsRow>(), Enumerable.Empty<WhsLocation>()));
			AssertExceptionThrown<ArgumentNullException>(() => new LocationValidationCache(Factory, Factory.New<WhsRow>(), null));
			AssertExceptionThrown<ArgumentNullException>(() => new LocationValidationCache(Factory, null, Enumerable.Empty<WhsLocation>()));
		}

		public void TestHasDockDoorPick_DoesNotAcceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationValidationCache(Factory, Factory.New<WhsRow>(), Enumerable.Empty<WhsLocation>()).HasDockDoorPick(null));
		}

		public void TestHasPackingStationPick_DoesNotAcceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationValidationCache(Factory, Factory.New<WhsRow>(), Enumerable.Empty<WhsLocation>()).HasPackingStationPick(null));
		}

		public void TestHasPickFace_DoesNotAcceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationValidationCache(Factory, Factory.New<WhsRow>(), Enumerable.Empty<WhsLocation>()).HasPickFace(null));
		}

		public void TestHasUnfinalisedPick_DoesNotAcceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationValidationCache(Factory, Factory.New<WhsRow>(),Enumerable.Empty<WhsLocation>()).HasUnfinalisedPick(null));
		}

		public void TestHasNonPalletStock_DoesNotAcceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationValidationCache(Factory, Factory.New<WhsRow>(), Enumerable.Empty<WhsLocation>()).HasNonPalletStock(null));
		}

		public void TestGetPalletCount_DoesNotAcceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationValidationCache(Factory, Factory.New<WhsRow>(), Enumerable.Empty<WhsLocation>()).GetPalletCount(null));
		}

		public void TestGetProductCount_DoesNotAcceptNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new LocationValidationCache(Factory, Factory.New<WhsRow>(), Enumerable.Empty<WhsLocation>()).GetProductCount(null));
		}

		public void TestHasPickFace_LocationsInMemory()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "TEST", 5, 5);
			var fixType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.FIX));

			foreach (var location in row.Locations)
			{
				location.WLV_WLT_LocationType = fixType.PK;

				var pickFace = Factory.New<WhsPickFace>();
				pickFace.WF_OH_Client = data.Org1.PK;
				pickFace.WF_OP = data.Part1.PK;
				pickFace.WF_WL = location.PK;
			}

			var cache = new LocationValidationCache(Factory, row, row.Locations.ToArray());

			CombineAssertions(() =>
			{
				foreach (var location in row.Locations)
				{
					AssertEquals($"Location {location.WLV_LocationString} should not have a PickFace.", false, cache.HasPickFace(location));
				}
			});
		}

		public void TestHasPickFace_LocationsInDB()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "TEST", 5, 5);
			var fixType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.FIX));

			foreach (var location in row.Locations)
			{
				location.WLV_WLT_LocationType = fixType.PK;
			}

			Factory.Save();

			var count = 0;

			foreach (var location in row.Locations)
			{
				if (count++ % 2 == 0)
				{
					var pickFace = Factory.New<WhsPickFace>();
					pickFace.WF_OH_Client = data.Org1.PK;
					pickFace.WF_OP = data.Part1.PK;
					pickFace.WF_WL = location.PK;
				}
			}

			Factory.Save();

			var cache = new LocationValidationCache(Factory, row, row.Locations.ToArray());

			CombineAssertions(() =>
			{
				count = 0;

				foreach (var location in row.Locations)
				{
					AssertEquals($"Location {location.WLV_LocationString} should return correctly if it has a PickFace or not.", count++ % 2 == 0, cache.HasPickFace(location));
				}
			});
		}

		public void TestHasPick_LocationsInMemory_DDL()
		{
			TestHasPick_LocationsInMemoryCore((pick, locPK) => pick.WP_WL_DockDoor = locPK, LocationClasses.Codes.DDL);
		}

		public void TestHasPick_LocationsInMemory_PST()
		{
			TestHasPick_LocationsInMemoryCore((pick, locPK) => pick.WP_WL_PackingStation = locPK, LocationClasses.Codes.PST);
		}

		void TestHasPick_LocationsInMemoryCore(Action<IWhsPick, ZGuid> setLocationValue, string locationClass)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "TEST", 5, 5);
			var locationType = Helper.CreateLocationType(locationClass, locationClass);

			foreach (var location in row.Locations)
			{
				location.WLV_WLT_LocationType = locationType.PK;

				var pick = Factory.New<IWhsPick>();
				pick.WP_WW_Whs = data.Whs1.PK;
				setLocationValue(pick, location.PK);
			}

			var cache = new LocationValidationCache(Factory, row, row.Locations.ToArray());

			CombineAssertions(() =>
			{
				foreach (var location in row.Locations)
				{
					AssertEquals($"Location {location.WLV_LocationString} should not have a DockDoorPick.", false, cache.HasDockDoorPick(location));
					AssertEquals($"Location {location.WLV_LocationString} should not have a PackingStationPick.", false, cache.HasPackingStationPick(location));
					AssertEquals($"Location {location.WLV_LocationString} should not have a Unfinalised Pick.", false, cache.HasUnfinalisedPick(location));
				}
			});
		}

		public void TestHasPick_LocationsInDB_DDL()
		{
			TestHasPick_LocationsInDBCore((pick, locPK) => pick[WhsPickSchema.WP_WL_DockDoor] = locPK, LocationClasses.Codes.DDL);
		}

		public void TestHasPick_LocationsInDB_PST()
		{
			TestHasPick_LocationsInDBCore((pick, locPK) => pick[WhsPickSchema.WP_WL_PackingStation] = locPK, LocationClasses.Codes.PST);
		}

		void TestHasPick_LocationsInDBCore(Action<BusinessObject, ZGuid> setLocationValue, string locationClass)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var client = Helper.CreateClient("WHS1TST");
			var part = Helper.CreateProduct(client, "P1");

			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "TEST", 5, 5);
			var locationType = Helper.CreateLocationType(locationClass, locationClass);

			foreach (var location in row.Locations)
			{
				location.WLV_WLT_LocationType = locationType.PK;
			}
			Factory.Save();

			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

			var count = 0;
			foreach (var location in row.Locations)
			{
				if (count++ % 2 == 0)
				{
					var orderPK = iHelper.CreateWhsOrder(client.PK, data.Whs1.PK, $"O{count + 1}", Notify);
					iHelper.CreateWhsOrderLine(orderPK, part.PK, 10m);

					var pick = iHelper.CreatePickNew(finaliseOrders: false, finalisePick: false, orderPK);
					setLocationValue(pick, location.PK);
				}
			}
			Factory.Save();

			var cache = new LocationValidationCache(Factory, row, row.Locations.ToArray());

			CombineAssertions(() =>
			{
				count = 0;

				foreach (var location in row.Locations)
				{
					AssertEquals($"Location {location.WLV_LocationString} should return correctly if it has a Pick or not.", locationClass == LocationClasses.Codes.DDL && count % 2 == 0, cache.HasDockDoorPick(location));
					AssertEquals($"Location {location.WLV_LocationString} should return correctly if it has a Pick or not.", locationClass == LocationClasses.Codes.PST && count % 2 == 0, cache.HasPackingStationPick(location));
					AssertEquals($"Location {location.WLV_LocationString} should return correctly if it has an Unfinalised Pick or not.", count % 2 == 0, cache.HasUnfinalisedPick(location));

					count++;
				}
			});
		}

		public void TestHasUnfinalisedPick_PickIsCancelled()
		{
			AssertHasUnfinalisedPick("CAN");
		}

		public void TestHasUnfinalisedPick_PickIsFinalised()
		{
			AssertHasUnfinalisedPick("FIN");
		}

		void AssertHasUnfinalisedPick(string pickStatus)
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "TEST", 5, 5);
			var ddlType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.DDL));

			foreach (var location in row.Locations)
			{
				location.WLV_WLT_LocationType = ddlType.PK;
			}

			Factory.Save();

			var count = 0;

			foreach (var location in row.Locations)
			{
				if (count++ % 2 == 0)
				{
					var pick = Factory.New<IWhsPick>();
					pick.WP_WW_Whs = data.Whs1.PK;

					if (count % 4 == 0)
					{
						var dockDoorAssignment = Factory.New<IWhsDockDoorAssignment>();
						dockDoorAssignment.WDA_WL_AssignedDockDoor = location.PK;
						pick.WP_WDA_DockDoorAssignment = dockDoorAssignment.PK;
					}
					else
					{
						pick.WP_WL_DockDoor = location.PK;
					}
					pick.WP_PickStatus = pickStatus;

					if (pickStatus == "FIN")
					{
						pick.WP_FinalizedDateUtc = ZDateTime.Now;
					}
				}
			}

			Factory.Save();

			var cache = new LocationValidationCache(Factory, row, row.Locations.ToArray());

			CombineAssertions(() =>
			{
				count = 0;

				foreach (var location in row.Locations)
				{
					AssertEquals($"Location {location.WLV_LocationString} should not have Unfinalised Pick.", false, cache.HasUnfinalisedPick(location));
					AssertEquals($"Location {location.WLV_LocationString} should have Pick.", count++ % 2 == 0, cache.HasDockDoorPick(location));
				}
			});
		}

		public void TestHasNonPalletStockAndPalletCount_LocationsInMemory()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "TEST", 5, 5);
			var ddlType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.DDL));

			var count = 0;
			foreach (var location in row.Locations)
			{
				location.WLV_WLT_LocationType = ddlType.PK;

				var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, $"R{++count}", Notify);
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, location.WLV_LocationString, heldCode: "", palletID: "PLT01");
				transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, location.WLV_LocationString, heldCode: "", palletID: "PLT02");
			}

			var cache = new LocationValidationCache(Factory, row, row.Locations.ToArray());

			CombineAssertions(() =>
			{
				foreach (var location in row.Locations)
				{
					AssertEquals($"Location {location.WLV_LocationString} should not have non Pallet stock.", false, cache.HasNonPalletStock(location));
					AssertEquals($"Location {location.WLV_LocationString} should not have Pallets.", 0, cache.GetPalletCount(location));
				}
			});
		}

		public void TestHasNonPalletStockAndPalletCount_AllPallets()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "TEST", 5, 5);
			var ddlType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.DDL));

			Factory.Save();

			var count = 0;

			foreach (var location in row.Locations)
			{
				location.WLV_WLT_LocationType = ddlType.PK;

				if (count++ % 2 == 0)
				{
					var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
					var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, $"R{count}", Notify);
					transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, location.WLV_LocationString, heldCode: "", palletID: "PLT01");
					transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, location.WLV_LocationString, heldCode: "", palletID: "PLT02");
				}
			}

			Factory.Save();

			var cache = new LocationValidationCache(Factory, row, row.Locations.ToArray());

			CombineAssertions(() =>
			{
				count = 0;

				foreach (var location in row.Locations)
				{
					AssertEquals($"Location {location.WLV_LocationString} should not have non Pallet stock.", false, cache.HasNonPalletStock(location));
					AssertEquals($"Location {location.WLV_LocationString} should not have Pallets.", count++ % 2 == 0 ? 2 : 0, cache.GetPalletCount(location));
				}
			});
		}

		public void TestHasNonPalletStockAndPalletCount_EmptyLocation()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "TEST", 5, 5);
			var ddlType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.DDL));

			Factory.Save();

			foreach (var location in row.Locations)
			{
				location.WLV_WLT_LocationType = ddlType.PK;
			}

			Factory.Save();

			var cache = new LocationValidationCache(Factory, row, row.Locations.ToArray());

			CombineAssertions(() =>
			{
				foreach (var location in row.Locations)
				{
					AssertEquals($"Location {location.WLV_LocationString} should not have non Pallet stock.", false, cache.HasNonPalletStock(location));
					AssertEquals($"Location {location.WLV_LocationString} should not have Pallets.", 0, cache.GetPalletCount(location));
				}
			});
		}

		public void TestHasNonPalletStockAndPalletCount_NotAllPallets()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "TEST", 5, 5);
			var ddlType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.DDL));

			Factory.Save();

			var count = 0;

			foreach (var location in row.Locations)
			{
				location.WLV_WLT_LocationType = ddlType.PK;

				if (count++ % 2 == 0)
				{
					var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
					var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, $"R{count}", Notify);
					transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, location.WLV_LocationString, heldCode: "", palletID: "PLT01");
					transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, location.WLV_LocationString, heldCode: "", palletID: "");
				}
			}

			Factory.Save();

			var cache = new LocationValidationCache(Factory, row, row.Locations.ToArray());

			CombineAssertions(() =>
			{
				count = 0;

				foreach (var location in row.Locations)
				{
					var hasNonPalletStock = count % 2 == 0;
					AssertEquals($"Location {location.WLV_LocationString}.", hasNonPalletStock, cache.HasNonPalletStock(location));
					AssertEquals($"Location {location.WLV_LocationString}.", hasNonPalletStock ? 2 : 0, cache.GetPalletCount(location));
					count++;
				}
			});
		}

		public void TestGetProductCount()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");
			Factory.Save();

			var transactionTestHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var goodsReceivePK = transactionTestHelper.CreateWhsReceive(data.Org1.PK, data.Whs1.PK, "R1", Notify);
			transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-2", heldCode: "", palletID: "PLT01");
			transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part1.PK, 10m, "A-3", heldCode: "", palletID: "PLT02");
			transactionTestHelper.CreateWhsReceiveInventoryLine(goodsReceivePK, data.Part2.PK, 10m, "A-3", heldCode: "", palletID: "PLT02");
			transactionTestHelper.FinaliseDocket(goodsReceivePK);
			Factory.Save();

			var cache = new LocationValidationCache(Factory, Factory.New<WhsRow>(), new[] { location1, location2, location3 });
			AssertEquals(0, cache.GetProductCount(location1));
			AssertEquals(1, cache.GetProductCount(location2));
			AssertEquals(2, cache.GetProductCount(location3));
		}

		public void TestHasNonUniqueCheckDigit_DuplicateCheckDigit()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "TEST", 3, 3);
			var locationType = Helper.CreateLocationType(LocationClasses.Codes.NOR, LocationClasses.Codes.NOR);

			Factory.Save();

			foreach (var location in row.Locations)
			{
				location.WLV_WLT_LocationType = locationType.PK;
			}

			row.Locations[0].WLV_CheckDigit = 2;
			row.Locations[1].WLV_CheckDigit = 3;
			row.Locations[2].WLV_CheckDigit = 3;
			row.Locations[3].WLV_CheckDigit = WhsLocation.EmptyCheckDigit;
			row.Locations[4].WLV_CheckDigit = WhsLocation.EmptyCheckDigit;

			Factory.Save();

			var cache = new LocationValidationCache(Factory, row, row.Locations.ToArray());

			AssertEquals(false, cache.HasNonUniqueCheckDigit(row.Locations[0]));
			AssertEquals(true, cache.HasNonUniqueCheckDigit(row.Locations[1]));
			AssertEquals(true, cache.HasNonUniqueCheckDigit(row.Locations[2]));
		}

		public void TestHasNonUniqueCheckDigit_EmptyCheckDigits()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "TEST", 3, 3);
			var locationType = Helper.CreateLocationType(LocationClasses.Codes.NOR, LocationClasses.Codes.NOR);

			Factory.Save();

			foreach (var location in row.Locations)
			{
				location.WLV_WLT_LocationType = locationType.PK;
			}

			row.Locations[0].WLV_CheckDigit = 2;
			row.Locations[3].WLV_CheckDigit = WhsLocation.EmptyCheckDigit;
			row.Locations[4].WLV_CheckDigit = WhsLocation.EmptyCheckDigit;

			Factory.Save();

			var cache = new LocationValidationCache(Factory, row, row.Locations.ToArray());

			AssertEquals(false, cache.HasNonUniqueCheckDigit(row.Locations[0]));
			AssertEquals(false, cache.HasNonUniqueCheckDigit(row.Locations[1]));
			AssertEquals(false, cache.HasNonUniqueCheckDigit(row.Locations[2]));
		}

		public void TestHasNonUniqueCheckDigit_DoesNotAcceptNull()
		{
			var cache = new LocationValidationCache(Factory, Factory.New<WhsRow>(), Enumerable.Empty<WhsLocation>());
			AssertExceptionThrown<ArgumentNullException>(() => cache.HasNonUniqueCheckDigit(null));
		}

		public void TestHasNonUniqueCheckDigit_DeferCheckDigitCacheCreation()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var row = Helper.CreateRowAndGenerateLocations(data.Whs1, "TEST", 3, 3);
			var locationType = Helper.CreateLocationType(LocationClasses.Codes.NOR, LocationClasses.Codes.NOR);

			Factory.Save();

			foreach (var location in row.Locations)
			{
				location.WLV_WLT_LocationType = locationType.PK;
			}

			row.Locations[0].WLV_CheckDigit = 2;
			row.Locations[1].WLV_CheckDigit = 2;

			Factory.Save();

			var cache = new LocationValidationCache(Factory, row, row.Locations.ToArray());

			using (cache.DeferCheckDigitCacheCreation())
			{
				using (cache.DeferCheckDigitCacheCreation())
				{
					AssertEquals(false, cache.HasNonUniqueCheckDigit(row.Locations[0]));
					AssertEquals(false, cache.HasNonUniqueCheckDigit(row.Locations[1]));
				}
				AssertEquals(false, cache.HasNonUniqueCheckDigit(row.Locations[0]));
				AssertEquals(false, cache.HasNonUniqueCheckDigit(row.Locations[1]));
			}
			AssertEquals(true, cache.HasNonUniqueCheckDigit(row.Locations[0]));
			AssertEquals(true, cache.HasNonUniqueCheckDigit(row.Locations[1]));
		}
	}
}
