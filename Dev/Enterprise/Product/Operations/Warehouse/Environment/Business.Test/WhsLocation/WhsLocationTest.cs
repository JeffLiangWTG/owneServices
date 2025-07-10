using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DbUpgrader.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business.US.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsLocation))]
	class WhsLocationTest : WhsEnvBusinessObjectTestCase
	{
		#region TestWLV_WarehouseType

		public void TestWLV_WarehouseType()
		{
			var warehouse = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 1);
			var location = row.Locations[0];

			AssertEquals(WarehouseTypes.Codes.Product, location.WLV_WarehouseType);

			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			AssertEquals(WarehouseTypes.Codes.Transit, location.WLV_WarehouseType);
		}

		public void TestWLV_WarehouseType_Setter()
		{
			var location = Factory.New<WhsLocation>();
			AssertExceptionThrown<NotSupportedException>(() => location.WLV_WarehouseType = "TST");
		}

		public void TestWLV_WarehouseType_NullWarehouse()
		{
			var location = Factory.New<WhsLocation>();
			AssertEquals(string.Empty, location.WLV_WarehouseType);
		}

		#endregion

		#region TestWLV_IsVirtualWarehouse

		public void TestWLV_IsVirtualWarehouse()
		{
			var warehouse = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 1);
			var location = row.Locations[0];

			AssertEquals(false, location.WLV_IsVirtualWarehouse);

			warehouse.WW_IsVirtualWarehouse = true;
			AssertEquals(true, location.WLV_IsVirtualWarehouse);
		}

		public void TestWLV_IsVirtualWarehouse_Setter()
		{
			var location = Factory.New<WhsLocation>();
			AssertExceptionThrown<NotSupportedException>(() => location.WLV_IsVirtualWarehouse = true);
		}

		public void TestWLV_IsVirtualWarehouse_NullWarehouse()
		{
			var location = Factory.New<WhsLocation>();
			AssertEquals(false, location.WLV_IsVirtualWarehouse);
		}

		#endregion

		#region TestWLV_LocationClass

		public void TestWLV_LocationClass()
		{
			var warehouse = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 1);
			var locationType = Helper.CreateLocationType("AAA", LocationClasses.Codes.NOR);
			var location = row.Locations[0];

			location.WLV_WLT_LocationType = locationType.PK;
			AssertEquals(LocationClasses.Codes.NOR, location.WLV_LocationClass);

			locationType.WLT_LocationClass = LocationClasses.Codes.FIX;
			AssertEquals(LocationClasses.Codes.FIX, location.WLV_LocationClass);
		}

		public void TestWLV_LocationClass_Setter()
		{
			var location = Factory.New<WhsLocation>();
			AssertExceptionThrown<NotSupportedException>(() => location.WLV_LocationClass = "TST");
		}

		public void TestWLV_LocationClass_NullLocationType()
		{
			var location = Factory.New<WhsLocation>();
			AssertEquals(string.Empty, location.WLV_LocationClass);
		}

		#endregion

		#region TestWLV_LocationTypeCode

		public void TestWLV_LocationTypeCode()
		{
			var warehouse = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 1);
			var locationType = Helper.CreateLocationType("AAA", LocationClasses.Codes.NOR);
			var location = row.Locations[0];

			location.WLV_WLT_LocationType = locationType.PK;
			Factory.Save();

			AssertEquals("AAA", location.WLV_LocationTypeCode);
		}

		public void TestWLV_LocationTypeCode_NullLocationType()
		{
			var location = Factory.New<WhsLocation>();
			AssertEquals(string.Empty, location.WLV_LocationTypeCode);
		}

		public void TestWLV_LocationTypeCode_Setter()
		{
			var location = Factory.New<WhsLocation>();
			AssertExceptionThrown<NotSupportedException>(() => location.WLV_LocationTypeCode = "ABC");
		}

		#endregion

		#region TestWLV_RS_NKTransitServiceLevel

		public void TestWLV_RS_NKTransitServiceLevel()
		{
			var location = Factory.New<WhsLocation>();
			AssertNoErrors(location.WLV_TransitDischargeLRCInfo);

			location.WLV_RS_NKTransitServiceLevel = "STD";
			AssertEquals("STD", location.WLV_RS_NKTransitServiceLevel);
			AssertHasErrors("Ensure changing ServiceLevel validates transit discharge.", location.WLV_TransitDischargeLRCInfo);
		}

		#endregion

		#region TestWLV_TransitDischargeLRC

		public void TestWLV_TransitDischargeLRC()
		{
			var location = Factory.New<WhsLocation>();
			AssertNoErrors(location.WLV_RS_NKTransitServiceLevelInfo);

			location.WLV_TransitDischargeLRC = "AUS";
			AssertEquals("AUS", location.WLV_TransitDischargeLRC);
			AssertHasErrors("Ensure changing DischargeLRC validates Service Level.", location.WLV_RS_NKTransitServiceLevelInfo);
		}

		#endregion

		#region TestWLV_WLT_LocationType_ValidationForPickFaceLocationMaxCapacity

		public void TestWLV_WLT_LocationType_ValidationForPickFaceLocationMaxCapacity()
		{
			var expectedMessage = "The max capacity of Fixed or Dynamic Pick Face Location must be 0.";
			var warehouse = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 1);
			var locationType_Normal = Helper.CreateLocationType("AAA", LocationClasses.Codes.NOR);
			var locationType = Helper.CreateLocationType("BBB", "BBB Test", false, 1, LocationClasses.Codes.FIX);
			var location = row.Locations[0];

			AssertNoErrors("Precondition: Should not have error", location.WLV_WLT_LocationTypeInfo);
			AssertNoErrors("Precondition: Should not have error", location.WLV_MaxWeightInfo);
			AssertNoErrors("Precondition: Should not have error", location.WLV_MaxCubicInfo);
			AssertNoErrors("Precondition: Should not have error", location.WLV_MaxQuantityInfo);
			Factory.Save();

			location.WLV_MaxWeight = 5m;
			location.WLV_MaxCubic = 5m;
			location.WLV_MaxQuantity = 5m;
			location.WLV_WLT_LocationType = locationType.PK;
			AssertHasError("Should have error", location.WLV_WLT_LocationTypeInfo, expectedMessage);
			AssertHasError("Should have error", location.WLV_MaxWeightInfo, expectedMessage);
			AssertHasError("Should have error", location.WLV_MaxCubicInfo, expectedMessage);
			AssertHasError("Should have error", location.WLV_MaxQuantityInfo, expectedMessage);

			location.WLV_WLT_LocationType = locationType_Normal.PK;
			AssertNoErrors("Should not have error", location.WLV_WLT_LocationTypeInfo);
			AssertNoErrors("Should not have error", location.WLV_MaxWeightInfo);
			AssertNoErrors("Should not have error", location.WLV_MaxCubicInfo);
			AssertNoErrors("Should not have error", location.WLV_MaxQuantityInfo);
		}

		#endregion

		#region TestMaxCapacity_ValidationForLocationType

		public void TestWLV_MaxWeight_ValidationForLocationType()
		{
			TestMaxCapacity_ValidationForLocationTypeCore(l => l.WLV_MaxWeightInfo);
		}

		public void TestWLV_MaxCubic_ValidationForLocationType()
		{
			TestMaxCapacity_ValidationForLocationTypeCore(l => l.WLV_MaxCubicInfo);
		}

		public void TestWLV_MaxQuantity_ValidationForLocationType()
		{
			TestMaxCapacity_ValidationForLocationTypeCore(l => l.WLV_MaxQuantityInfo);
		}

		void TestMaxCapacity_ValidationForLocationTypeCore(Func<WhsLocation, ZPropertyInfo> getPropertyInfo)
		{
			var expectedMessage = "The max capacity of Fixed or Dynamic Pick Face Location must be 0.";
			var warehouse = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 1);
			var locationType = Helper.CreateLocationType("BBB", "BBB Test", false, 1, LocationClasses.Codes.FIX);
			var location = row.Locations[0];
			var propertyInfo = getPropertyInfo(location);

			AssertNoErrors("Precondition: Should not have error", location.WLV_WLT_LocationTypeInfo);
			AssertNoErrors("Precondition: Should not have error", propertyInfo);
			Factory.Save();

			location.WLV_WLT_LocationType = locationType.PK;
			location.SetPropertyValue(propertyInfo.Name, (ZDecimal)5m);
			AssertHasError("Should have error", location.WLV_WLT_LocationTypeInfo, expectedMessage);
			AssertHasError("Should have error", propertyInfo, expectedMessage);

			location.SetPropertyValue(propertyInfo.Name, (ZDecimal)0m);
			AssertNoErrors("Should not have error", location.WLV_WLT_LocationTypeInfo);
			AssertNoErrors("Should not have error", propertyInfo);
		}

		#endregion

		#region TestLocationString

		public void TestLocationStringInMemory()
		{
			TestLocationStringCore(true);
		}

		public void TestLocationStringInDB()
		{
			TestLocationStringCore(false);
		}

		void TestLocationStringCore(bool saveToFactory)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2);

			if (saveToFactory)
			{
				Factory.Save();
			}

			AssertContainsExactElementsInAnyOrder(new[] { "A-1-1", "A-1-2", "A-2-1", "A-2-2" }, row.Locations.Select(l => l.WLV_LocationString));
		}

		public void TestLocationString_FixedWidthLocationWarehouseInMemory()
		{
			TestLocationString_FixedWidthLocationWarehouseCore(true);
		}

		public void TestLocationString_FixedWidthLocationWarehouseInDB()
		{
			TestLocationString_FixedWidthLocationWarehouseCore(false);
		}

		void TestLocationString_FixedWidthLocationWarehouseCore(bool saveToFactory)
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("WHS", 3, 3, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);
			if (saveToFactory)
			{
				Factory.Save();
			}

			AssertContainsExactElementsInAnyOrder(new[] { "A00100101", "A00100102", "A00100201", "A00100202", "A00200101", "A00200102", "A00200201", "A00200202" }, row.Locations.Select(l => l.WLV_LocationString));
		}

		#endregion

		#region TestLocationString_UserFriendly

		public void TestLocationString_UserFriendlyInMemory()
		{
			TestLocationString_UserFriendlyCore(true);
		}

		public void TestLocationString_UserFriendlyInDB()
		{
			TestLocationString_UserFriendlyCore(false);
		}

		void TestLocationString_UserFriendlyCore(bool saveToFactory)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2);

			if (saveToFactory)
			{
				Factory.Save();
			}

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"A-1-1",
					"A-1-2",
					"A-2-1",
					"A-2-2"
				}, row.Locations.Select(l => l.WLV_LocationString_UserFriendly));
		}

		public void TestLocationString_UserFriendly_FixedWidthLocationWarehouseInMemory()
		{
			TestLocationString_UserFriendly_FixedWidthLocationWarehouseCore(true);
		}

		public void TestLocationString_UserFriendly_FixedWidthLocationWarehouseInDB()
		{
			TestLocationString_UserFriendly_FixedWidthLocationWarehouseCore(false);
		}

		void TestLocationString_UserFriendly_FixedWidthLocationWarehouseCore(bool saveToFactory)
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("WHS", 3, 3, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);
			if (saveToFactory)
			{
				Factory.Save();
			}

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"A-001-001-01",
					"A-001-001-02",
					"A-001-002-01",
					"A-001-002-02",
					"A-002-001-01",
					"A-002-001-02",
					"A-002-002-01",
					"A-002-002-02"
				}, row.Locations.Select(l => l.WLV_LocationString_UserFriendly));
		}

		public void TestLocationString_UserFriendly_SetThrowsAnException()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2);
			var location = row.Locations[0];

			AssertExceptionThrown<NotSupportedException>(() => location.WLV_LocationString_UserFriendly = "ABC");
		}

		#endregion

		#region TestFindLocation

		public void TestFindLocation()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);

			WhsRow row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 4, 3, 2);
			Factory.Save();

			AssertNull("Empty", WhsLocation.FindLocation(Factory, "", warehouse.PK));

			AssertNull("SHEEP", WhsLocation.FindLocation(Factory, "SHEEP", warehouse.PK));

			// partial (starts with tests)
			AssertEquals("A", row.Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1).PK, WhsLocation.FindLocation(Factory, "A", warehouse.PK).PK);
			AssertEquals("A-4", row.Locations.Single(l => l.WLV_Column == 4 && l.WLV_Level == 1 && l.WLV_Tray == 1).PK, WhsLocation.FindLocation(Factory, "A-4", warehouse.PK).PK);
			AssertEquals("A-4-3", row.Locations.Single(l => l.WLV_Column == 4 && l.WLV_Level == 3 && l.WLV_Tray == 1).PK, WhsLocation.FindLocation(Factory, "A-4-3", warehouse.PK).PK);
			AssertNull("A-5", WhsLocation.FindLocation(Factory, "A-5", warehouse.PK));
			AssertNull("A-4-4", WhsLocation.FindLocation(Factory, "A-4-4", warehouse.PK));

			// full (exact matches)
			AssertEquals("A-1-2-1", row.Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 2 && l.WLV_Tray == 1).PK, WhsLocation.FindLocation(Factory, "A-1-2-1", warehouse.PK).PK);
			AssertEquals("A-4-3-1", row.Locations.Single(l => l.WLV_Column == 4 && l.WLV_Level == 3 && l.WLV_Tray == 1).PK, WhsLocation.FindLocation(Factory, "A-4-3-1", warehouse.PK).PK);
			AssertEquals("A-4-3-2", row.Locations.Single(l => l.WLV_Column == 4 && l.WLV_Level == 3 && l.WLV_Tray == 2).PK, WhsLocation.FindLocation(Factory, "A-4-3-2", warehouse.PK).PK);
			AssertNull("A-4-3-3", WhsLocation.FindLocation(Factory, "A-4-3-3", warehouse.PK));
		}

		public void TestFindLocation_AlphaColumns()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			warehouse.WW_LocationColumnsAlpha = true;

			WhsRow row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 4, 3, 2);
			Factory.Save();

			AssertNull("A-4-3-1", WhsLocation.FindLocation(Factory, "A-4-3-1", warehouse.PK));
			AssertEquals("A-A-3-1", row.Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 3 && l.WLV_Tray == 1).PK, WhsLocation.FindLocation(Factory, "A-A-3-1", warehouse.PK).PK);
			AssertEquals("A-D-3-1", row.Locations.Single(l => l.WLV_Column == 4 && l.WLV_Level == 3 && l.WLV_Tray == 1).PK, WhsLocation.FindLocation(Factory, "A-D-3-1", warehouse.PK).PK);
		}

		public void TestFindLocation_AlphaLevels()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			warehouse.WW_LocationLevelsAlpha = true;

			WhsRow row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 4, 3, 2);
			Factory.Save();

			AssertNull("A-4-3-1", WhsLocation.FindLocation(Factory, "A-4-3-1", warehouse.PK));
			AssertNull("A-D-C-1", WhsLocation.FindLocation(Factory, "A-D-C-1", warehouse.PK));
			AssertEquals("A-1-B-1", row.Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 2 && l.WLV_Tray == 1).PK, WhsLocation.FindLocation(Factory, "A-1-B-1", warehouse.PK).PK);
			AssertEquals("A-4-C-1", row.Locations.Single(l => l.WLV_Column == 4 && l.WLV_Level == 3 && l.WLV_Tray == 1).PK, WhsLocation.FindLocation(Factory, "A-4-C-1", warehouse.PK).PK);
		}

		public void TestFindLocation_AlphaTrays()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			warehouse.WW_LocationTraysAlpha = true;

			WhsRow row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 4, 3, 2);
			Factory.Save();

			AssertNull("A-4-3-1", WhsLocation.FindLocation(Factory, "A-4-3-1", warehouse.PK));
			AssertNull("A-D-C-1", WhsLocation.FindLocation(Factory, "A-D-C-1", warehouse.PK));
			AssertEquals("A-1-2-B", row.Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 2 && l.WLV_Tray == 2).PK, WhsLocation.FindLocation(Factory, "A-1-2-B", warehouse.PK).PK);
			AssertEquals("A-4-3-B", row.Locations.Single(l => l.WLV_Column == 4 && l.WLV_Level == 3 && l.WLV_Tray == 2).PK, WhsLocation.FindLocation(Factory, "A-4-3-B", warehouse.PK).PK);
		}

		public void TestFindLocation_DifferentDelimiter()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			warehouse.WW_LocationComponentDelimiter = "=";

			WhsRow row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 4, 3, 2);
			Factory.Save();

			AssertNull("A 4 3 2", WhsLocation.FindLocation(Factory, "A 4 3 2", warehouse.PK));
			AssertNull("A-4-3-2", WhsLocation.FindLocation(Factory, "A-4-3-2", warehouse.PK));
			AssertEquals("A=4=3=2", row.Locations.Single(l => l.WLV_Column == 4 && l.WLV_Level == 3 && l.WLV_Tray == 2).PK, WhsLocation.FindLocation(Factory, "A=4=3=2", warehouse.PK).PK);
		}

		public void TestFindLocation_ZeroBased()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			warehouse.WW_LocationColumnsZeroBased = true;
			warehouse.WW_LocationLevelsZeroBased = true;
			warehouse.WW_LocationTraysZeroBased = true;

			WhsRow row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 4, 3, 2);
			Factory.Save();

			AssertEquals("A-0-1-0", row.Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 2 && l.WLV_Tray == 1).PK, WhsLocation.FindLocation(Factory, "A-0-1-0", warehouse.PK).PK);
			AssertEquals("A-3-2-0", row.Locations.Single(l => l.WLV_Column == 4 && l.WLV_Level == 3 && l.WLV_Tray == 1).PK, WhsLocation.FindLocation(Factory, "A-3-2-0", warehouse.PK).PK);
			AssertEquals("A-3-2-1", row.Locations.Single(l => l.WLV_Column == 4 && l.WLV_Level == 3 && l.WLV_Tray == 2).PK, WhsLocation.FindLocation(Factory, "A-3-2-1", warehouse.PK).PK);
			AssertNull("A-3-2-2", WhsLocation.FindLocation(Factory, "A-3-2-2", warehouse.PK));
			AssertNull("A-4-3-2", WhsLocation.FindLocation(Factory, "A-4-3-2", warehouse.PK));
		}

		public void TestFindLocation_Ordering()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			WhsRow row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 1, 1);
			warehouse.WW_IsActive = false;
			var firstLocation = row.Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1);
			var secondLocation = row.Locations.Single(l => l.WLV_Column == 2 && l.WLV_Level == 1 && l.WLV_Tray == 1);
			var secondLocationPK = secondLocation.PK;
			secondLocation.WLV_Column = 1;
			firstLocation.WLV_Column = 2;
			Factory.Save();
			AssertEquals(secondLocationPK, WhsLocation.FindLocation(Factory, "A", warehouse.PK).PK);
		}

		public void TestFindLocation_FixedWidthLocation()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("1", 2, 2, 2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 4, 3, 2);
			Factory.Save();

			AssertEquals("A010101", row.Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1).PK, WhsLocation.FindLocation(Factory, "A010101", warehouse.PK).PK);
			AssertEquals("A040302", row.Locations.Single(l => l.WLV_Column == 4 && l.WLV_Level == 3 && l.WLV_Tray == 2).PK, WhsLocation.FindLocation(Factory, "A040302", warehouse.PK).PK);
		}

		public void TestFindLocation_DBHits()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 10, 10);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var warehouseInNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			warehouseInNewFactory.FindLocation("A-10-10");
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);

			AssertDbHits(expectedDbHits, newFactory);
		}

		public void TestFindLocationPK()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			Helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1);
			Factory.Save();

			AssertEquals(ZGuid.Empty, WhsLocation.FindLocationPK(Factory, "", warehouse.PK));
			AssertEquals(ZGuid.Empty, WhsLocation.FindLocationPK(Factory, "SHEEP", warehouse.PK));
			AssertEquals(warehouse.Rows.Single(r => r.WR_Name == "A").Locations.Single(l => l.WLV_Column == 1 && l.WLV_Level == 1 && l.WLV_Tray == 1).PK, WhsLocation.FindLocationPK(Factory, "A", warehouse.PK));
		}

		#endregion

		#region TestRowPathSequence

		public void TestRowPathSequence()
		{
			var location = Factory.New<WhsLocation>();
			AssertEquals(new ZShort(0), location.RowPathSequence);

			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRow(warehouse, "A");
			row.WR_PickPathSequence = 2;
			location.WLV_WR = row.PK;
			AssertEquals(new ZShort(2), location.RowPathSequence);
		}

		#endregion

		#region TestRowLocationSequence

		public void TestRowLocationSequence()
		{
			var location = Factory.New<WhsLocation>();
			AssertEquals("", location.RowLocationSequence);

			var row = Factory.New<WhsRow>();
			location.WLV_WR = row.PK;
			AssertEquals("0-0", location.RowLocationSequence);

			location.WLV_PickPathSequence = 1;
			AssertEquals("0-1", location.RowLocationSequence);

			row.WR_PickPathSequence = 2;
			AssertEquals("2-1", location.RowLocationSequence);
		}

		#endregion

		#region TestIsEmpty

		public void TestIsEmpty()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A");
			var location = warehouse.DefaultLocation;
			AssertEquals(true, location.IsEmpty);

			var client = Helper.CreateClient();
			var product = Helper.CreateProduct("BOWLHAT", client);
			var stockHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			stockHelper.CreateStock(warehouse.PK, client.PK, product.PK, 10m);

			Factory.Save();
			AssertEquals(false, location.IsEmpty);
		}

		#endregion

		#region TestGetNewValidation

		public void TestGetNewValidation()
		{
			var helper = new WhsTestHelperFunctionsEnvUS(Factory);
			var warehouseUS = helper.CreateWarehouse("TST WHS", "A");
			var locationUS = warehouseUS.DefaultLocation;
			AssertEquals(typeof(US.WhsLocationValidation), locationUS.Validation.GetType());

			var location = Factory.New<WhsLocation>();
			AssertEquals(typeof(WhsLocationViewValidation), location.Validation.GetType());
		}

		#endregion

		#region TestCountryCode

		public void TestCountryCode()
		{
			AssertNull("Precondition: Row should be null", Location.Row);
			AssertEquals(ZString.Empty, Location.CountryCode);

			WhsWarehouse whs = Helper.CreateWarehouse("WHS TST");
			WhsRow row = Helper.CreateRow(whs, "ROW");
			Location.WLV_WR = row.PK;
			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Enterprise.Core.Constants.CountryCodes.UnitedStates).RL_Code;
			AssertEquals(Enterprise.Core.Constants.CountryCodes.UnitedStates, Location.CountryCode);

			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = Helper.GetCountryUNLOCO(Enterprise.Core.Constants.CountryCodes.Australia).RL_Code;
			AssertEquals(Enterprise.Core.Constants.CountryCodes.Australia, Location.CountryCode);
		}

		#endregion

		#region TestGetNewLookups

		public void TestGetNewLookups()
		{
			AssertEquals(typeof(WhsLocationViewLookups), Location.Lookups.GetType());
		}

		#endregion

		#region TestGetNewLookupsUS

		public void TestGetNewLookupsUS()
		{
			WhsTestHelperFunctionsEnvUS helper = new WhsTestHelperFunctionsEnvUS(Factory);
			WhsWarehouse whs = helper.CreateWarehouse("TST WHS");
			WhsRow row = helper.CreateRow(whs, "ROW");
			Location.WLV_WR = row.PK;
			AssertEquals(typeof(US.WhsLocationLookups), Location.Lookups.GetType());
		}

		#endregion

		#region TestBarcode

		public void TestBarcode()
		{
			var warehouse1 = Helper.CreateWarehouse("WHS1");
			Helper.CreateRowAndGenerateLocations(warehouse1, "R1", 2, 2, 2);
			var warehouse2 = Helper.CreateWarehouse("WHS2");
			Helper.CreateRowAndGenerateLocations(warehouse2, "R2", 3, 2, 2);

			AssertLocationsBarcodes(warehouse1);
			AssertLocationsBarcodes(warehouse2);
		}

		void AssertLocationsBarcodes(WhsWarehouse warehouse)
		{
			var row = warehouse.Rows.Single(r => r.Locations.Count > 1);
			foreach (var location in row.Locations)
			{
				AssertEquals("#W" + warehouse.WW_WarehouseCode + row.WR_Name + "-" + location.WLV_Column.ToString() + "-" + location.WLV_Level.ToString() + "-" + location.WLV_Tray.ToString(), location.OldBarcode);
			}
		}

		#endregion

		#region TestLocationClassFlag

		#region TestIsDockDoorLocation

		public void TestIsDockDoorLocation()
		{
			TestLocationClassFlag(LocationClasses.Codes.DDL, (normalLocation, location) =>
			{
				AssertEquals(false, normalLocation.IsDockDoorLocation);
				AssertEquals(true, location.IsDockDoorLocation);
			});
		}

		#endregion

		#region TestIsPackingStationLocation

		public void TestIsPackingStationLocation()
		{
			TestLocationClassFlag(LocationClasses.Codes.PST, (normalLocation, location) =>
			{
				AssertEquals(false, normalLocation.IsPackingStationLocation);
				AssertEquals(true, location.IsPackingStationLocation);
			});
		}

		#endregion

		#region TestIsFixedLocation

		public void TestIsFixedLocation()
		{
			TestLocationClassFlag(LocationClasses.Codes.FIX, (normalLocation, location) =>
			{
				AssertEquals(false, normalLocation.IsFixedLocation);
				AssertEquals(true, location.IsFixedLocation);
			});
		}

		#endregion

		#region TestIsDynamicPickFaceLocation

		public void TestIsDynamicPickFaceLocation()
		{
			TestLocationClassFlag(LocationClasses.Codes.DPF, (normalLocation, location) =>
			{
				AssertEquals(false, normalLocation.IsDynamicPickFaceLocation);
				AssertEquals(true, location.IsDynamicPickFaceLocation);
			});
		}

		#endregion

		#region TestIsPackingConsolidationLocation

		public void TestIsPackingConsolidationLocation()
		{
			TestLocationClassFlag(LocationClasses.Codes.CON, (normalLocation, location) =>
			{
				AssertEquals(false, normalLocation.IsPackingConsolidationLocation);
				AssertEquals(true, location.IsPackingConsolidationLocation);
			});
		}

		#endregion

		#region TestIsEligibleForCheckDigits

		public void TestIsEligibleForCheckDigits_DDL()
		{
			TestLocationClassFlag(LocationClasses.Codes.DDL, (normalLocation, location) =>
			{
				AssertEquals(false, location.IsEligibleForCheckDigits);
				AssertEquals(true, normalLocation.IsEligibleForCheckDigits);
			});
		}

		public void TestIsEligibleForCheckDigits_PST()
		{
			TestLocationClassFlag(LocationClasses.Codes.PST, (normalLocation, location) =>
			{
				AssertEquals(false, location.IsEligibleForCheckDigits);
				AssertEquals(true, normalLocation.IsEligibleForCheckDigits);
			});
		}

		public void TestIsEligibleForCheckDigits_CON()
		{
			TestLocationClassFlag(LocationClasses.Codes.CON, (normalLocation, location) =>
			{
				AssertEquals(false, location.IsEligibleForCheckDigits);
				AssertEquals(true, normalLocation.IsEligibleForCheckDigits);
			});
		}

		#endregion

		public void TestLocationClassFlag(ZString locationClass, Action<WhsLocation, WhsLocation> assertAction)
		{
			var warehouse = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 1);
			var locationType_Normal = Helper.CreateLocationType("AAA", LocationClasses.Codes.NOR);
			var locationType = locationClass == "FIX" ? Helper.CreateLocationType("BBB", "BBB Test", true, 1, locationClass) : Helper.CreateLocationType("BBB", locationClass);

			var normalLocation = row.Locations[0];
			normalLocation.WLV_WLT_LocationType = locationType_Normal.PK;
			var location = row.Locations[1];
			location.WLV_WLT_LocationType = locationType.PK;

			AssertEquals("Precondition", "AAA", normalLocation.LocationType.WLT_Code);
			AssertEquals("Precondition", "BBB", location.LocationType.WLT_Code);
			assertAction(normalLocation, location);
		}

		#endregion

		#region TestIsInBondedArea

		public void TestIsInBondedArea()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 4, 1);
			Factory.Save();

			var bondedArea = Helper.CreateArea(data.Whs1, "Bonded", AreaTypes.Codes.Bonded);

			var locationPickingBonded = data.Whs1.FindLocation("A-1");
			locationPickingBonded.WLV_WA_PickingArea = bondedArea.PK;

			var locationPutawayBonded = data.Whs1.FindLocation("A-2");
			locationPutawayBonded.WLV_WA_PutawayArea = bondedArea.PK;

			var locationBothBonded = data.Whs1.FindLocation("A-3");
			locationBothBonded.WLV_WA_PickingArea = bondedArea.PK;
			locationBothBonded.WLV_WA_PutawayArea = bondedArea.PK;

			var locationNormal = data.Whs1.FindLocation("A-4");
			Factory.Save();

			AssertEquals("Location with Picking Area that is bonded should be true.", true, locationPickingBonded.IsInBondedArea);
			AssertEquals("Location with Putaway Area that is bonded should be true.", true, locationPutawayBonded.IsInBondedArea);
			AssertEquals("Location with Picking and Putaway Area that are non bonded should be false.", false, locationNormal.IsInBondedArea);
			AssertEquals("Location with Picking and Putaway Area that are bonded should be true.", true, locationBothBonded.IsInBondedArea);
		}

		#endregion

		#region TestIsInInwardProcessingArea

		public void TestIsInInwardProcessingArea()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 4, 1);
			data.Whs1.WW_IsVirtualWarehouse = true;
			var inwardProcessingArea = Helper.CreateArea(data.Whs1, "IPR", AreaTypes.Codes.InwardProcessing);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			location1.WLV_WA_PickingArea = inwardProcessingArea.PK;
			location1.WLV_WA_PutawayArea = inwardProcessingArea.PK;

			var location2 = data.Whs1.FindLocation("A-2");
			location2.WLV_WA_PutawayArea = inwardProcessingArea.PK;
			location2.WLV_WA_PickingArea = inwardProcessingArea.PK;

			var location4 = data.Whs1.FindLocation("A-4");
			Factory.Save();

			AssertEquals("Location with Picking and Putaway Area that are IPR should be true.", true, location1.IsInInwardProcessingArea);
			AssertEquals("Location with Picking and Putaway Area that are IPR should be true.", true, location2.IsInInwardProcessingArea);
			AssertEquals("Location with Picking and Putaway Area that are non IPR should be false.", false, location4.IsInInwardProcessingArea);
		}

		#endregion

		#region TestIsApprovedKnownLocationNoCountry

		public void TestIsApprovedKnownLocationNoCountry()
		{
			Location.WLV_WR = ZGuid.Empty;
			Location.WLV_ApprovedKnownLocation = CodeLists.ApprovedKnownStatus.Codes.NO;
			AssertEquals(false, Location.IsApprovedKnownLocation);

			WhsWarehouse whs = Helper.CreateWarehouse("TST WHS");
			WhsRow row = Helper.CreateRow(whs, "ROW");

			row.WR_WW_Whs = ZGuid.Empty;
			Location.WLV_WR = row.PK;
			AssertEquals(false, Location.IsApprovedKnownLocation);

			row.WR_WW_Whs = whs.PK;
			whs.WarehouseAddress.OA_RL_NKRelatedPortCode = ZString.Empty;
			AssertEquals(false, Location.IsApprovedKnownLocation);
		}

		#endregion

		#region TestIsApprovedKnownLocationUS

		public void TestIsApprovedKnownLocationUS()
		{
			WhsTestHelperFunctionsEnvUS helper = new WhsTestHelperFunctionsEnvUS(Factory);
			Location.WLV_WR = ZGuid.Empty;
			Location.WLV_ApprovedKnownLocation = CodeLists.US.TSAStatus.Codes.Unknown;
			AssertEquals(false, Location.IsApprovedKnownLocation);
			Location.WLV_ApprovedKnownLocation = CodeLists.US.TSAStatus.Codes.Known;
			AssertEquals(false, Location.IsApprovedKnownLocation);

			WhsWarehouse whs = helper.CreateWarehouse("TST WHS");
			WhsRow row = helper.CreateRow(whs, "ROW");

			row.WR_WW_Whs = ZGuid.Empty;
			Location.WLV_WR = row.PK;
			Location.WLV_ApprovedKnownLocation = CodeLists.US.TSAStatus.Codes.Unknown;
			AssertEquals(false, Location.IsApprovedKnownLocation);
			Location.WLV_ApprovedKnownLocation = CodeLists.US.TSAStatus.Codes.Known;
			AssertEquals(false, Location.IsApprovedKnownLocation);

			row.WR_WW_Whs = whs.PK;
			helper.SetOrgAddressTSAStatus(whs.WarehouseAddress, CodeLists.US.TSAStatus.Codes.Unknown);
			Location.WLV_ApprovedKnownLocation = CodeLists.US.TSAStatus.Codes.Unknown;
			AssertEquals(false, Location.IsApprovedKnownLocation);
			Location.WLV_ApprovedKnownLocation = CodeLists.US.TSAStatus.Codes.Known;
			AssertEquals(false, Location.IsApprovedKnownLocation);

			helper.SetOrgAddressTSAStatus(whs.WarehouseAddress, CodeLists.US.TSAStatus.Codes.Known);
			Location.WLV_ApprovedKnownLocation = CodeLists.US.TSAStatus.Codes.Unknown;
			AssertEquals(false, Location.IsApprovedKnownLocation);
			Location.WLV_ApprovedKnownLocation = CodeLists.US.TSAStatus.Codes.Known;
			AssertEquals(true, Location.IsApprovedKnownLocation);
		}

		#endregion

		#region TestSetDefaultValues

		public void TestSetDefaultValues()
		{
			AssertEquals("Location column", (short)1, Location.WLV_Column);
			AssertEquals("Location level", (short)1, Location.WLV_Level);
			AssertEquals("Location tray", (short)1, Location.WLV_Tray);
			AssertEquals("Location CheckDigit", WhsLocation.EmptyCheckDigit, Location.WLV_CheckDigit);
			AssertEquals("Location status", CodeLists.LocationStatus.Codes.Normal, Location.WLV_LocationStatus);
			AssertEquals("Location type", ZGuid.Empty, Location.WLV_WLT_LocationType);
			AssertEquals("Location PickMethod", WarehouseDataRegistry.Instance.PickMethod.Value.DefaultCode, Location.WLV_PickMethod);
			AssertEquals("Location ApprovedKnown Status", CodeLists.ApprovedKnownStatus.Codes.NO, Location.WLV_ApprovedKnownLocation);
			AssertEquals("Location Default MaxQuantity Unit", "UNT", Location.WLV_MaxQuantityUnit);
		}

		#endregion

		#region TestMaxQuantityUnit

		public void TestMaxQuantityUnit()
		{
			var location = Factory.New<WhsLocation>();
			AssertEquals("UNT", location.WLV_MaxQuantityUnit);
			AssertEquals(0m, location.WLV_MaxQuantity);

			location.WLV_MaxQuantityUnit = "";
			AssertEquals("", location.WLV_MaxQuantityUnit);

			location.WLV_MaxQuantity = 3m;
			AssertEquals("UNT", location.WLV_MaxQuantityUnit);
		}

		#endregion

		#region TestMaxWeightUnit_Lookup

		public void TestMaxWeightUnit_Lookup()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(WhsLocation), nameof(WhsLocation.WLV_MaxWeightUnit), false, a => a.ListDataSourceMember == "Lookups.WeightUnits");
		}

		#endregion

		#region TestMaxCubicUnit_Lookup

		public void TestMaxCubicUnit_Lookup()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(WhsLocation), nameof(WhsLocation.WLV_MaxCubicUnit), false, a => a.ListDataSourceMember == "Lookups.CubicUnits");
		}

		#endregion

		#region TestMaxDimensionUnit_Lookup

		public void TestMaxDimensionUnit_Lookup()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(WhsLocation), nameof(WhsLocation.WLV_MaxDimensionUnit), false, a => a.ListDataSourceMember == "Lookups.DimensionUnits");
		}

		#endregion

		#region TestWarehouseAndWLV_WW_Whs

		public void TestWarehouseAndWLV_WW_Whs()
		{
			var whs = Factory.New<WhsWarehouse>();
			Location.WLV_WW_Whs = whs.PK;
			AssertEquals(whs, Location.Warehouse);
			AssertEquals(whs.PK, Location.WLV_WW_Whs);
		}

		#endregion

		#region TestAreaAndWLV_WA_PickingArea

		public void TestWLV_WA_PickingAreaAreaAndWLV_WA_PutawayArea()
		{
			var pickingArea = Factory.New<WhsArea>();
			var putawayArea = Factory.New<WhsArea>();
			var location = Factory.New<WhsLocation>();
			location.WLV_WA_PickingArea = pickingArea.PK;
			AssertEquals("Picking area must be set.", pickingArea, location.PickingArea);
			AssertNull("Putaway area hasn't been set therefore remain null.", location.PutawayArea);
			AssertEquals("WLV_WA_PickingArea must be pickingArea.PK.", pickingArea.PK, location.WLV_WA_PickingArea);
			AssertEquals("Putaway area hasn't been set therefore WLV_WA_PutawayArea must return empty guid.", ZGuid.Empty, location.WLV_WA_PutawayArea);

			location.WLV_WA_PutawayArea = putawayArea.PK;
			AssertEquals("Setting putaway area must no change picking area.", pickingArea, location.PickingArea);
			AssertEquals("Putaway area must be set.", putawayArea, location.PutawayArea);
			AssertEquals("pickingArea.PK must not be changed.", pickingArea.PK, location.WLV_WA_PickingArea);
			AssertEquals("Putaway area must be putawayArea.PK.", putawayArea.PK, location.WLV_WA_PutawayArea);
		}

		#endregion

		#region TestPickingAreaName

		public void TestPickingAreaName()
		{
			var pickingArea = Factory.New<WhsArea>();
			pickingArea.WA_Name = "PICK";

			var putawayArea = Factory.New<WhsArea>();
			putawayArea.WA_Name = "PUT";

			var location = Factory.New<WhsLocation>();
			location.WLV_WA_PutawayArea = putawayArea.PK;
			AssertEquals("Should have no picking area name.", ZString.Empty, location.PickingAreaName);

			location.WLV_WA_PickingArea = pickingArea.PK;
			AssertEquals("Should have picking area name.", "PICK", location.PickingAreaName);
		}

		#endregion

		#region TestPickingArea_SetsPickingAreaType

		public void TestPickingArea_SetsPickingAreaType()
		{
			var pickingArea = Factory.New<WhsArea>();
			pickingArea.WA_AreaType = AreaTypes.Codes.DockDoor;

			var location = Factory.New<WhsLocation>();
			location.WLV_WA_PickingArea = pickingArea.PK;
			AssertEquals($"Picking area type should be {AreaTypes.Codes.DockDoor}.", AreaTypes.Codes.DockDoor, location.WLV_PickingAreaType);
		}

		#endregion

		#region TestPickingArea_SettingToSameValue_DoesNotCallFactoryLoad

		public void TestPickingArea_SettingToSameValue_DoesNotCallFactoryLoad()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			Helper.CreateRowAndGenerateLocations(warehouse, "A");
			var pickingArea = warehouse.Areas[0];
			var location = warehouse.Rows[0].Locations[0];
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var locationInNewFactory = newFactory.Load<WhsLocation>(location.PK);
			locationInNewFactory.WLV_WA_PickingArea = pickingArea.PK;
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsAreaSchema.Constants.TableName, 0);

			AssertDbHits(expectedDbHits, newFactory);
		}

		#endregion

		#region TestPickingAreaType_Setter

		public void TestPickingAreaType_Setter()
		{
			var location = Factory.New<WhsLocation>();
			AssertExceptionThrown<NotSupportedException>(() => location.WLV_PutawayAreaType = AreaTypes.Codes.DockDoor);
		}

		#endregion

		#region TestPutawayArea_SetsPutawayAreaType

		public void TestPutawayArea_SetsPutawayAreaType()
		{
			var putawayArea = Factory.New<WhsArea>();
			putawayArea.WA_AreaType = AreaTypes.Codes.FreeStore;

			var location = Factory.New<WhsLocation>();
			location.WLV_WA_PutawayArea = putawayArea.PK;
			AssertEquals($"Putaway area type should be {AreaTypes.Codes.FreeStore}.", AreaTypes.Codes.FreeStore, location.WLV_PutawayAreaType);
		}

		#endregion

		#region TestPutawayArea_SettingToSameValue_DoesNotCallFactoryLoad

		public void TestPutawayArea_SettingToSameValue_DoesNotCallFactoryLoad()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			Helper.CreateRowAndGenerateLocations(warehouse, "A");
			var putawayArea = warehouse.Areas[0];
			var location = warehouse.Rows[0].Locations[0];
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var locationInNewFactory = newFactory.Load<WhsLocation>(location.PK);
			locationInNewFactory.WLV_WA_PutawayArea = putawayArea.PK;
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsAreaSchema.Constants.TableName, 0);

			AssertDbHits(expectedDbHits, newFactory);
		}

		#endregion

		#region TestPutawayAreaType_Setter

		public void TestPutawayAreaType_Setter()
		{
			var location = Factory.New<WhsLocation>();
			AssertExceptionThrown<NotSupportedException>(() => location.WLV_PutawayAreaType = AreaTypes.Codes.FreeStore);
		}

		#endregion

		#region TestPickingAreaType_GetterDoesNotCallFactoryLoad

		public void TestPickingAreaType_GetterDoesNotCallFactoryLoad()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			Helper.CreateRowAndGenerateLocations(warehouse, "A");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var warehouseInNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			var location = warehouseInNewFactory.FindLocation("A");
			var pickingAreaType = location.WLV_PickingAreaType;
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsAreaSchema.Constants.TableName, 0);

			AssertDbHits(expectedDbHits, newFactory);
		}

		#endregion

		#region TestPutawayAreaType_GetterDoesNotCallFactoryLoad

		public void TestPutawayAreaType_GetterDoesNotCallFactoryLoad()
		{
			var warehouse = Helper.CreateWarehouse("1", shouldPreGenerateDDL: false);
			Helper.CreateRowAndGenerateLocations(warehouse, "A");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var warehouseInNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			var location = warehouseInNewFactory.FindLocation("A");
			var putawayAreaType = location.WLV_PutawayAreaType;
			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsLocationViewSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsAreaSchema.Constants.TableName, 0);

			AssertDbHits(expectedDbHits, newFactory);
		}

		#endregion

		#region TestRowAndRowNameAndWLV_WR

		public void TestRowAndRowNameAndWLV_WR()
		{
			WhsRow row = Factory.New<WhsRow>();
			row.WR_Name = "AA";
			Location.WLV_WR = row.PK;
			AssertEquals(row, Location.Row);
			AssertEquals(row.PK, Location.WLV_WR);
			AssertEquals("AA", Location.RowName);
		}

		#endregion

		#region TestWLV_WRWithWW_WLT_DefaultLocationTypeAndWLV_WW_Whs

		public void TestWLV_WRWithWW_WLT_DefaultLocationTypeAndWLV_WW_Whs()
		{
			var whs = Helper.CreateWarehouse("whs", "A", 1, 1);
			var row = whs.Rows[0];
			AssertNotNull("Precondition:", row);
			AssertEquals("Precondition:", true, whs.WW_WLT_DefaultLocationType.IsValid);

			Location.WLV_WR = row.PK;
			AssertEquals(row.PK, Location.WLV_WR);
			AssertEquals(whs.WW_WLT_DefaultLocationType, Location.WLV_WLT_LocationType);
			AssertEquals(whs, Location.Warehouse);
			AssertEquals(whs.PK, Location.WLV_WW_Whs);
		}

		#endregion

		#region TestWLV_IsValidLocationForProductWarehousePutaway

		public void TestWLV_IsValidLocationForProductWarehousePutaway_Setter()
		{
			var location = Factory.New<WhsLocation>();
			AssertExceptionThrown<NotSupportedException>(() => location.WLV_IsValidLocationForProductWarehousePutaway = true);
		}

		#endregion

		#region TestWLV_LastConfigChangedUtc

		public void TestWLV_LastConfigChangedUtc_Setter()
		{
			var location = Factory.New<WhsLocation>();
			AssertExceptionThrown<NotSupportedException>(() => location.WLV_LastConfigChangedUtc = ZDateTime.Now);
		}

		#endregion

		#region TestRowNameInfo

		public void TestRowNameInfo()
		{
			AssertNotNull(Location.RowNameInfo);
		}

		#endregion

		#region TestToLocationString

		public void TestToLocationString()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("WHS");
			WhsRow row = Helper.CreateRowAndGenerateLocations(whs, "AA", 3, 2);
			Location = row.Locations[5];
			AssertEquals("AA", "AA-3-2", Location.ToLocationString());
		}

		#endregion

		#region TestToSpaceDelimitedLocationString

		public void TestToSpaceDelimitedLocationString()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "AA", 3, 2);
			var location = row.Locations[5];
			AssertEquals("AA 3 2", location.ToSpaceDelimitedLocationString());
		}

		public void TestToSpaceDelimitedLocationString_FixedWidthLocationWarehouse()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("WHS", 3, 3, 2);
			var row = Helper.CreateRowAndGenerateLocations(whs, "AA", 3, 2, 2);
			var location = row.Locations[5];
			AssertEquals("AA 002 001 02", location.ToSpaceDelimitedLocationString());
		}

		#endregion

		#region TestCheckDigit

		public void TestEmptyCheckDigit() => AssertEquals((byte)255, WhsLocation.EmptyCheckDigit);

		public void TestFormattedCheckDigit() => TestFormattedCheckDigitCore(12, "12");

		public void TestFormattedCheckDigit_SingleDigit() => TestFormattedCheckDigitCore(2, "02");

		public void TestFormattedCheckDigit_Zero() => TestFormattedCheckDigitCore(0, "00");

		public void TestFormattedCheckDigit_Empty() => TestFormattedCheckDigitCore(WhsLocation.EmptyCheckDigit, "");

		void TestFormattedCheckDigitCore(ZByte checkDigit, string expectedFormattedCheckDigit)
		{
			var location = Factory.New<WhsLocation>();

			location.WLV_CheckDigit = checkDigit;
			AssertEquals(expectedFormattedCheckDigit, location.FormattedCheckDigit);
		}

		public void TestFormattedCheckDigit_Setter() => TestFormattedCheckDigit_Setter_Core("12", 12);

		public void TestFormattedCheckDigit_Setter_SingleDigit() => TestFormattedCheckDigit_Setter_Core("2", 2);

		public void TestFormattedCheckDigit_Setter_SingleDigitFrom2Digits() => TestFormattedCheckDigit_Setter_Core("03", 3);

		public void TestFormattedCheckDigit_Setter_SingleDigit_Zero()
		{
			TestFormattedCheckDigit_Setter_Core("0", 0);
			TestFormattedCheckDigit_Setter_Core("00", 0);
			TestFormattedCheckDigit_Setter_Core("000", 0);
		}

		public void TestFormattedCheckDigit_Setter_Empty() => TestFormattedCheckDigit_Setter_Core("", WhsLocation.EmptyCheckDigit);

		public void TestFormattedCheckDigit_Setter_Invalid()
		{
			var location = Factory.New<WhsLocation>();
			TestFormattedCheckDigit_Setter_Core("1A", WhsLocation.EmptyCheckDigit);
			TestFormattedCheckDigit_Setter_Core("A1", WhsLocation.EmptyCheckDigit);
			TestFormattedCheckDigit_Setter_Core("A A", WhsLocation.EmptyCheckDigit);
			TestFormattedCheckDigit_Setter_Core("1.1", WhsLocation.EmptyCheckDigit);
			TestFormattedCheckDigit_Setter_Core("1 1", WhsLocation.EmptyCheckDigit);
			TestFormattedCheckDigit_Setter_Core(" ", WhsLocation.EmptyCheckDigit);
			TestFormattedCheckDigit_Setter_Core("-1", WhsLocation.EmptyCheckDigit);
			TestFormattedCheckDigit_Setter_Core("1234", WhsLocation.EmptyCheckDigit);
		}

		void TestFormattedCheckDigit_Setter_Core(string formattedCheckDigit, ZByte expectedCheckDigit)
		{
			var location = Factory.New<WhsLocation>();

			location.FormattedCheckDigit = formattedCheckDigit;
			AssertEquals(expectedCheckDigit, location.WLV_CheckDigit);
		}

		public void TestFormattedCheckDigit_Setter_SetNumberThenSetToSpace()
		{
			// Need to test that setting a valid value sets the the underlying value to something that is
			// not 255 (the default blank value), and then checks that setting it to a single space will set it back to 255
			var location = Factory.New<WhsLocation>();
			location.FormattedCheckDigit = "02";
			AssertEquals((ZByte)2, location.WLV_CheckDigit);
			location.FormattedCheckDigit = " ";
			AssertEquals((ZByte)WhsLocation.EmptyCheckDigit, location.WLV_CheckDigit);
		}

		public void TestFormattedCheckDigitInfo_MaxLength()
		{
			var location = Factory.New<WhsLocation>();
			AssertHasCustomAttribute<MaxLengthAttribute>(typeof(WhsLocation), nameof(WhsLocation.FormattedCheckDigit), false, a => a.MaxLength == 2);
		}

		public void TestWLV_WLT_LocationType_ValidationForCheckDigit_DDL()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 1);
			Factory.Save();
			var location = whs.FindLocation("A-1");
			location.WLV_CheckDigit = 1;

			AssertNoErrors("Precondition: Should not have error", location.WLV_CheckDigitInfo);

			var ddlLocationType = Helper.CreateLocationType("DDL", "DDL");
			location.WLV_WLT_LocationType = ddlLocationType.PK;
			Assert("Precondition:", location.IsDockDoorLocation);
			AssertHasErrors("CheckDigit being added to a dock door location should result in an error", location.WLV_CheckDigitInfo);

			var normalLocationType = Helper.CreateLocationType("NOR", "NOR");
			location.WLV_WLT_LocationType = normalLocationType.PK;
			AssertNoErrors("Should not have error", location.WLV_CheckDigitInfo);
		}

		public void TestWLV_WLT_LocationType_ValidationForCheckDigit_PST()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 1);
			Factory.Save();
			var location = whs.FindLocation("A-1");
			location.WLV_CheckDigit = 1;

			AssertNoErrors("Precondition: Should not have error", location.WLV_CheckDigitInfo);

			var pstLocationType = Helper.CreateLocationType("PST", "PST", true, 1, LocationClasses.Codes.PST);
			location.WLV_WLT_LocationType = pstLocationType.PK;
			Assert("Precondition:", location.IsPackingStationLocation);
			AssertHasErrors("CheckDigit being added to a packing station location should result in an error", location.WLV_CheckDigitInfo);

			var normalLocationType = Helper.CreateLocationType("NOR", "NOR");
			location.WLV_WLT_LocationType = normalLocationType.PK;
			AssertNoErrors("Should not have error", location.WLV_CheckDigitInfo);
		}

		public void TestWLV_WLT_LocationType_ValidationForCheckDigit_CON()
		{
			var whs = Helper.CreateWarehouse("W1", "A", 2, 1);
			Factory.Save();
			var location = whs.FindLocation("A-1");
			location.WLV_CheckDigit = 1;

			AssertNoErrors("Precondition: Should not have error", location.WLV_CheckDigitInfo);

			var conLocationType = Helper.CreateLocationType("CON", "CON", true, 1, LocationClasses.Codes.CON);
			location.WLV_WLT_LocationType = conLocationType.PK;
			Assert("Precondition:", location.IsPackingConsolidationLocation);
			AssertHasErrors("CheckDigit being added to a packing consolidation location should result in an error", location.WLV_CheckDigitInfo);

			var normalLocationType = Helper.CreateLocationType("NOR", "NOR");
			location.WLV_WLT_LocationType = normalLocationType.PK;
			AssertNoErrors("Should not have error", location.WLV_CheckDigitInfo);
		}

		#endregion

		#region TestFormattedLocationComponents

		public void TestFormattedLocationComponents()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "R1", 1, 2, 3);
			Factory.Save();

			Location = row.Locations[row.Locations.Count - 1];
			AssertEquals("FormattedColumn property value is invalid", "1", Location.FormattedColumn);
			AssertEquals("FormattedLevel property value is invalid", "2", Location.FormattedLevel);
			AssertEquals("FormattedTray property value is invalid", "3", Location.FormattedTray);
			AssertEquals("Location name is invalid", "R1-1-2-3", Location.ToLocationString());

			whs.WW_LocationColumnsZeroBased = true;
			whs.WW_LocationLevelsZeroBased = true;
			whs.WW_LocationTraysZeroBased = true;
			Factory.Save();

			AssertEquals("FormattedColumn property value is invalid", "0", Location.FormattedColumn);
			AssertEquals("FormattedLevel property value is invalid", "1", Location.FormattedLevel);
			AssertEquals("FormattedTray property value is invalid", "2", Location.FormattedTray);
			AssertEquals("Location name is invalid", "R1-0-1-2", Location.ToLocationString());

			whs.WW_LocationColumnsAlpha = true;
			whs.WW_LocationLevelsAlpha = true;
			whs.WW_LocationTraysAlpha = true;
			Factory.Save();

			AssertEquals("FormattedColumn property value is invalid", "A", Location.FormattedColumn);
			AssertEquals("FormattedLevel property value is invalid", "B", Location.FormattedLevel);
			AssertEquals("FormattedTray property value is invalid", "C", Location.FormattedTray);
			AssertEquals("Location name is invalid", "R1-A-B-C", Location.ToLocationString());
		}

		public void TestFormattedLocationComponents_FixedWidthLocationWarehouse()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("WHS", 3, 3, 2);
			var row = Helper.CreateRowAndGenerateLocations(whs, "R1", 1, 2, 3);
			Factory.Save();

			Location = row.Locations[row.Locations.Count - 1];
			AssertEquals("FormattedColumn property value is invalid", "001", Location.FormattedColumn);
			AssertEquals("FormattedLevel property value is invalid", "002", Location.FormattedLevel);
			AssertEquals("FormattedTray property value is invalid", "03", Location.FormattedTray);
			AssertEquals("Location name is invalid", "R100100203", Location.ToLocationString());
		}

		#endregion

		#region TestLocationType_Trigger

		#region TestLocationType_Trigger_PreventsChangingLocationType

		public void TestLocationType_Trigger_PreventsChangingLocationType()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = iHelper.CreateClient("WHS1TST");
			var part = iHelper.CreateProduct(orgPK, "P1");

			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			Factory.Save();

			var location1 = whs.FindLocation("A-1");
			Factory.Save();

			var receivePK = iHelper.CreateWhsReceive(orgPK, whs.PK, "1", Notify);
			iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1");
			iHelper.FinaliseDocket(receivePK);
			Factory.Save();

			var order = iHelper.CreateWhsOrder(orgPK, whs.PK, orgPK, "O1");
			iHelper.CreateWhsOrderLine(order.PK, part.PK, 10m);

			AssertEquals("Precondition", "RNO", location1.LocationType.WLT_Code);
			location1.WLV_WLT_LocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "PFC")).PK;
			AssertHasError("Precondition.", location1.WLV_WLT_LocationTypeInfo, "Cannot change Location Type as there is Existing or Pending Inventory.");

			// Try actually saving despite error
			AssertThrowsChangeLocationTypeExceptionOnFactorySave();
		}

		#endregion

		#region TestLocationType_Trigger_PreventsConcurrencyProblems_NewInventory

		public void TestLocationType_Trigger_PreventsConcurrencyProblems_NewInventory()
		{
			var iHelper_Factory1 = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = iHelper_Factory1.CreateClient("WHS1TST");
			var part = iHelper_Factory1.CreateProduct(orgPK, "P1");

			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			Factory.Save();

			var location1 = whs.FindLocation("A-1");
			location1.WLV_LastInventoryChangeDate = ZDateTimeOffset.Now.AddDays(1); // without this line concurrency stop before getting change Location Type error
			Factory.Save();

			location1.WLV_WLT_LocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "PFC")).PK;
			AssertNoErrors("Precondition.", location1.WLV_WLT_LocationTypeInfo);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var iHelper_Factory2 = WhsTransactionTestHelperCreator.GetNewHelper(factory2);
			var receivePK = iHelper_Factory2.CreateWhsReceive(orgPK, whs.PK, "1", Notify);
			iHelper_Factory2.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1");
			iHelper_Factory2.FinaliseDocket(receivePK);

			factory2.Save(); // Save New Inventory
			AssertThrowsChangeLocationTypeExceptionOnFactorySave(); // Change location Type
		}

		#endregion

		#region TestLocationType_Trigger_PreventsConcurrencyProblems_ChangeLocationOnExistingInventory

		public void TestLocationType_Trigger_PreventsConcurrencyProblems_ChangeLocationOnExistingInventory()
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = iHelper.CreateClient("WHS1TST");
			var part = iHelper.CreateProduct(orgPK, "P1");

			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			Factory.Save();

			var location1 = whs.FindLocation("A-1");
			var location2 = whs.FindLocation("A-2");
			Factory.Save();

			var receivePK = iHelper.CreateWhsReceive(orgPK, whs.PK, "1", Notify);
			var receiveLinePK = iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-1");
			iHelper.FinaliseDocket(receivePK);
			Factory.Save();

			location2.WLV_WLT_LocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "PFC")).PK;
			AssertNoErrors("Precondition.", location2.WLV_WLT_LocationTypeInfo);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveLine_InFactory2 = factory2.Load<IWhsReceiveLine>(receiveLinePK);
			receiveLine_InFactory2.WE_WL = location2.PK;

			factory2.Save(); // Change Location on Existing Inventory
			AssertThrowsChangeLocationTypeExceptionOnFactorySave(); // Change Location Type
		}

		#endregion

		#region TestLocationType_Trigger_WhenHavingPicksForTheDDL

		public void TestLocationType_Trigger_WP_WL_DockDoor_ActivePick_NOR()
		{
			TestLocationType_Trigger_WP_WL_DockDoor_CoreActivePick((whs, loc) => CreateDDLPick(whs, loc, "ENT"));
		}

		public void TestLocationType_Trigger_WP_WL_DockDoor_ActivePick_PST()
		{
			TestLocationType_Trigger_WP_WL_DockDoor_CoreActivePick((whs, loc) => CreateDDLPick(whs, loc, "ENT"), otherLocationClass: LocationClasses.Codes.PST);
		}

		public void TestLocationType_Trigger_WP_WL_DockDoor_FinalisedPick()
		{
			TestLocationType_Trigger_WP_WL_DockDoor_CoreActivePick(
				(whs, loc) =>
				{
					var pick_Finalised = CreateDDLPick(whs, loc, "FIN");
					pick_Finalised.WP_FinalizedDateUtc = ZDateTime.UtcNow;
				});
		}

		public void TestLocationType_Trigger_WP_WL_DockDoor_CancelledPick()
		{
			TestLocationType_Trigger_WP_WL_DockDoor_CoreActivePick((whs, loc) => CreateDDLPick(whs, loc, "CAN"));
		}

		void TestLocationType_Trigger_WP_WL_DockDoor_CoreActivePick(Action<WhsWarehouse, WhsLocation> createPick, string otherLocationClass = "NOR")
		{
			var ddlLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.DDL);
			var otherLocationType = Helper.CreateLocationType("777", otherLocationClass);

			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			Factory.Save();

			var locationA1 = whs.FindLocation("A-1");
			locationA1.WLV_WLT_LocationType = ddlLocationType.PK;

			createPick(whs, locationA1);
			Factory.Save();

			// Precondition: No error
			locationA1.Validation.ValidateWLV_WLT_LocationType();
			AssertNoErrors("Precondition", locationA1.WLV_WLT_LocationTypeInfo);

			// changing type of DDL of active pick - Error
			locationA1.WLV_WLT_LocationType = otherLocationType.PK;
			AssertHasError("Precondition", locationA1.WLV_WLT_LocationTypeInfo, WhsLocationViewValidation.ErrorChangingTypeFromDockDoorIfUsedByPick);
			AssertThrowsChangeLocationTypeExceptionOnFactorySave("Attempt to change Location Type for a Dock Door Location that is used by Existing Picks."); // Try actually saving despite error
		}

		IWhsPick CreateDDLPick(WhsWarehouse whs, WhsLocation location, ZString status)
		{
			var pick = Factory.New<IWhsPick>();
			pick.WP_PickStatus = status;
			pick.WP_WW_Whs = whs.PK;
			pick.WP_WL_DockDoor = location.PK;

			return pick;
		}

		#endregion

		#region TestLocationType_Trigger_WhenHavingPicksForThePST

		const string ExpectedPSTTriggerException = "Attempt to change Location Type for a Packing Station Location that is used by Existing Picks.";

		public void TestLocationType_Trigger_WP_WL_PackingStation_ActivePick_DDL()
			=> TestLocationType_Trigger_WP_WL_PackingStation_ActivePick(otherLocationClass: LocationClasses.Codes.DDL);

		public void TestLocationType_Trigger_WP_WL_PackingStation_ActivePick_NOR()
			=> TestLocationType_Trigger_WP_WL_PackingStation_ActivePick(otherLocationClass: LocationClasses.Codes.NOR);

		void TestLocationType_Trigger_WP_WL_PackingStation_ActivePick(string otherLocationClass)
		{
			var pstLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.PST);
			var otherLocationType = Helper.CreateLocationType("456", otherLocationClass);

			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			var location = whs.DefaultLocation;
			location.WLV_WLT_LocationType = pstLocationType.PK;

			var client = Helper.CreateClient("WHS1TST");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orderPK = transactionHelper.CreateWhsOrder(client.PK, whs.PK, "O1", Notify);
			transactionHelper.CreateWhsOrderLine(orderPK, part.PK, 100m);

			var unfinalisedPick = transactionHelper.CreatePickNew(finaliseOrders: false, finalisePick: false, orderPK);
			unfinalisedPick[WhsPickSchema.WP_WL_PackingStation] = location.PK;
			Factory.Save();

			// Precondition: No error
			location.Validation.ValidateWLV_WLT_LocationType();
			AssertNoErrors("Precondition", location.WLV_WLT_LocationTypeInfo);

			// changing type of DDL of active pick - Error
			location.WLV_WLT_LocationType = otherLocationType.PK;
			AssertHasError("Precondition", location.WLV_WLT_LocationTypeInfo, WhsLocationViewValidation.ErrorChangingTypeFromPackingStationIfUsedByPick);
			AssertThrowsChangeLocationTypeExceptionOnFactorySave("Attempt to change Location Type for a Packing Station Location that is used by Existing Picks."); // Try actually saving despite error
		}

		public void TestLocationType_Trigger_WP_WL_PackingStation_CancelledPick()
		{
			var pstLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.PST);
			var normalLocationType = Helper.CreateLocationType("777", LocationClasses.Codes.NOR);

			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			var location = whs.DefaultLocation;
			location.WLV_WLT_LocationType = pstLocationType.PK;

			var client = Helper.CreateClient("WHS1TST");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var transactionHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orderPK = transactionHelper.CreateWhsOrder(client.PK, whs.PK, "O1", Notify);
			transactionHelper.CreateWhsOrderLine(orderPK, part.PK, 100m);

			var unfinalisedPick = transactionHelper.CreatePickNew(finaliseOrders: false, finalisePick: false, orderPK);
			unfinalisedPick[WhsPickSchema.WP_WL_PackingStation] = location.PK;
			Factory.Save();

			unfinalisedPick[WhsPickSchema.WP_PickStatus] = "CAN";
			Factory.Save();

			// changing type of PST of active pick - Error
			location.WLV_WLT_LocationType = normalLocationType.PK;
			AssertHasError("Precondition", location.WLV_WLT_LocationTypeInfo, WhsLocationViewValidation.ErrorChangingTypeFromPackingStationIfUsedByPick);
			AssertThrowsChangeLocationTypeExceptionOnFactorySave(ExpectedPSTTriggerException); // Try actually saving despite error
		}

		public void TestLocationType_Trigger_WP_WL_PackingStation_FinalisedPick()
		{
			var pstLocationType = Helper.CreateLocationType("123", LocationClasses.Codes.PST);
			var normalLocationType = Helper.CreateLocationType("777", LocationClasses.Codes.NOR);

			var whs = Helper.CreateWarehouse("WHS", "A", 2, 1);
			var location = whs.DefaultLocation;
			location.WLV_WLT_LocationType = pstLocationType.PK;

			var client = Helper.CreateClient("WHS1TST");
			var part = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orderPK = iHelper.CreateWhsOrder(client.PK, whs.PK, "O1", Notify);
			iHelper.CreateWhsOrderLine(orderPK, part.PK, 10m);

			var finalisedPick = iHelper.CreatePickNew(finaliseOrders: true, finalisePick: true, orderPK);
			finalisedPick[WhsPickSchema.WP_WL_PackingStation] = location.PK;
			Factory.Save();

			// changing type of PST of finalised / cancelled pick  - Error
			location.WLV_WLT_LocationType = normalLocationType.PK;
			AssertHasError("Precondition", location.WLV_WLT_LocationTypeInfo, WhsLocationViewValidation.ErrorChangingTypeFromPackingStationIfUsedByPick);
			AssertThrowsChangeLocationTypeExceptionOnFactorySave(ExpectedPSTTriggerException); // Try actually saving despite error
		}

		#endregion

		#region AssertThrowsChangeLocationTypeExceptionOnFactorySave

		void AssertThrowsChangeLocationTypeExceptionOnFactorySave(string expectedErrorMessage = "Attempt to change Location Type for a Location with Existing or Pending Inventory added by a user in another instance.")
		{
			ZSaveException exCaught = null;

			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				exCaught = ex;
				AssertEquals(expectedErrorMessage, ex.GetInnermostException().Message);
			}

			AssertNotNull("Expected a ZSaveException.", exCaught);
		}

		#endregion

		#endregion

		#region TestLocationStatuses

		public void TestLocationStatuses()
		{
			AssertNotNull(Location.LocationStatuses);
			AssertEquals(true, Location.LocationStatuses.ContainsCode(CodeLists.LocationStatus.Codes.Normal));
		}

		#endregion

		#region TestPickMethods

		public void TestPickMethods()
		{
			AssertNotNull(Location.PickMethods);
			AssertEquals(true, Location.PickMethods.ContainsCode("ANY"));
		}

		#endregion

		#region TestIsMaximumTouchCountUsed

		public void TestIsMaximumTouchCountUsed()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_MaximumPickCountBeforeAutomatedStocktake = 0;
			AssertEquals(false, location.IsMaximumTouchCountUsed);

			location.WLV_MaximumPickCountBeforeAutomatedStocktake = 1;
			AssertEquals(true, location.IsMaximumTouchCountUsed);
		}

		#endregion

		#region TestHasCapacityLimitation

		public void TestHasCapacityLimitation_MaxQuantity()
		{
			var location = Factory.New<WhsLocation>();
			AssertEquals(false, location.HasCapacityLimitation);

			location.WLV_MaxWeight = 0;
			location.WLV_MaxCubic = 0;
			location.WLV_MaxQuantity = 1;
			AssertEquals(true, location.HasCapacityLimitation);
		}

		public void TestHasCapacityLimitation_MaxWeight()
		{
			var location = Factory.New<WhsLocation>();
			AssertEquals(false, location.HasCapacityLimitation);

			location.WLV_MaxWeight = 1;
			location.WLV_MaxCubic = 0;
			location.WLV_MaxQuantity = 0;
			AssertEquals(true, location.HasCapacityLimitation);
		}

		public void TestHasCapacityLimitation_MaxCubic()
		{
			var location = Factory.New<WhsLocation>();
			AssertEquals(false, location.HasCapacityLimitation);

			location.WLV_MaxWeight = 0;
			location.WLV_MaxCubic = 1;
			location.WLV_MaxQuantity = 0;
			AssertEquals(true, location.HasCapacityLimitation);
		}

		#endregion

		#region TestWL_MaxWeightUnit_ConstraintSynchronisedWithLookup

		public void TestWL_MaxWeightUnit_ConstraintSynchronisedWithLookup() => TestConstraintSynchronisedWithLookup(WhsLocationSchema.Constants.WL_MaxWeightUnit, locl => locl.WeightUnits);

		public void TestConstraintSynchronisedWithLookup(string columnName, Func<WhsLocationViewLookups, CodeDescriptionPairList> getLookupList)
		{
			var location = Factory.New<WhsLocation>();
			var codesOnLookup = getLookupList(location.Lookups).GetAllCodes();
			var codesOnConstraint = TestDbObjectHelper.GetInFiltersFromConstraint(columnName);

			AssertContainsExactElementsInAnyOrder(codesOnLookup, codesOnConstraint);
		}

		#endregion

		#region TestWL_MaxCubicUnit_ConstraintSynchronisedWithLookup

		public void TestWL_MaxCubicUnit_ConstraintSynchronisedWithLookup() => TestConstraintSynchronisedWithLookup(WhsLocationSchema.Constants.WL_MaxCubicUnit, locl => locl.CubicUnits);

		#endregion

		#region TestUpdateCurrentTouchCount

		public void TestUpdateCurrentTouchCount()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_FinalisedPickCount = 0;
			AssertEquals(false, location.UpdateCurrentTouchCount());
			AssertEquals(0, location.WLV_FinalisedPickCount);

			location.WLV_MaximumPickCountBeforeAutomatedStocktake = 2;
			location.WLV_FinalisedPickCount = 0;
			AssertEquals(false, location.UpdateCurrentTouchCount());
			AssertEquals(1, location.WLV_FinalisedPickCount);

			AssertEquals(true, location.UpdateCurrentTouchCount());
			AssertEquals(0, location.WLV_FinalisedPickCount);
		}

		#endregion

		#region TestWLV_FinalisedPickCount

		[TestDate(2012, 08, 19)]
		public void TestWLV_FinalisedPickCount()
		{
			var location = Factory.New<WhsLocation>();
			AssertEquals(ZDateTimeOffset.Empty, location.WLV_LastInventoryChangeDate);

			location.WLV_FinalisedPickCount = 1;
			AssertEquals(ZDateTimeOffset.Now, location.WLV_LastInventoryChangeDate);
			AssertEquals(1, location.WLV_FinalisedPickCount);
		}

		#endregion

		#region TestWL_LastInventoryChangeDate

		[TestDate(2019, 3, 21, 3, 15, 30)]
		public void TestWL_LastInventoryChangeDate()
		{
			var now = ZDateTimeOffset.Now;
			var location = Factory.New<WhsLocation>();
			AssertEquals("Precondition", ZDateTimeOffset.Empty, location.WLV_LastInventoryChangeDate);
			location.OnStockOnHandChanged();
			AssertEquals("Set value if is empty", now, location.WLV_LastInventoryChangeDate);

			TestDateAttribute.Date = now.AddDays(-1).ToDateTime();
			location.OnStockOnHandChanged();
			AssertEquals("Not change it if is in the past.", now, location.WLV_LastInventoryChangeDate);

			TestDateAttribute.Date = now.AddDays(1).ToDateTime();
			location.OnStockOnHandChanged();
			AssertEquals("Should update with new value.", now.AddDays(1), location.WLV_LastInventoryChangeDate);
		}

		#endregion

		#region TestRollbackChangeLastInventoryChangeDate

		[TestDate(2019, 3, 21, 3, 15, 30)]
		public void TestRollbackChangeLastInventoryChangeDate()
		{
			var now = ZDateTimeOffset.Now;
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = row.Locations[0];
			AssertEquals("Precondition", ZDateTimeOffset.Empty, location.WLV_LastInventoryChangeDate);
			location.OnStockOnHandChanged();
			AssertEquals("Set value if is empty", now, location.WLV_LastInventoryChangeDate);

			location.RollbackChangeLastInventoryChangeDate();
			AssertEquals("Should clear value.", ZDateTimeOffset.Empty, location.WLV_LastInventoryChangeDate);
			Factory.Save();

			location.OnStockOnHandChanged();
			AssertEquals("Set value if is empty", now, location.WLV_LastInventoryChangeDate);
			location.RollbackChangeLastInventoryChangeDate();
			AssertEquals("Should clear value.", ZDateTimeOffset.Empty, location.WLV_LastInventoryChangeDate);

			TestDateAttribute.Date = now.AddDays(1).ToDateTime();
			location.OnStockOnHandChanged();
			AssertEquals("Should update with last date.", now.AddDays(1), location.WLV_LastInventoryChangeDate);
			Factory.Save();

			TestDateAttribute.Date = now.AddDays(2).ToDateTime();
			location.OnStockOnHandChanged();
			AssertEquals("Should update with last date.", now.AddDays(2), location.WLV_LastInventoryChangeDate);
			location.RollbackChangeLastInventoryChangeDate();
			AssertEquals("Should back to original value.", now.AddDays(1), location.WLV_LastInventoryChangeDate);
		}

		#endregion

		#region TestConcurrencyPolicy_IgnoreProperties

		public void TestConcurrencyPolicy_IgnoreProperties()
		{
			TestConcurrencyPolicy(ConcurrencyPolicy.Ignore, nameof(WhsLocation.WLV_LocationString));
			TestConcurrencyPolicy(ConcurrencyPolicy.Ignore, nameof(WhsLocation.WLV_LocationString_UserFriendly));
			TestConcurrencyPolicy(ConcurrencyPolicy.Ignore, nameof(WhsLocation.WLV_FormattedColumn));
			TestConcurrencyPolicy(ConcurrencyPolicy.Ignore, nameof(WhsLocation.WLV_FormattedLevel));
			TestConcurrencyPolicy(ConcurrencyPolicy.Ignore, nameof(WhsLocation.WLV_FormattedTray));
			TestConcurrencyPolicy(ConcurrencyPolicy.Ignore, nameof(WhsLocation.WLV_LastInventoryChangeDate));
			TestConcurrencyPolicy(ConcurrencyPolicy.Ignore, nameof(WhsLocation.WLV_LastAllocatedOrChangedDateUtc));
			TestConcurrencyPolicy(ConcurrencyPolicy.Ignore, nameof(WhsLocation.WLV_IsVirtualWarehouse));
			TestConcurrencyPolicy(ConcurrencyPolicy.Ignore, nameof(WhsLocation.WLV_LocationClass));
			TestConcurrencyPolicy(ConcurrencyPolicy.Ignore, nameof(WhsLocation.WLV_LocationTypeCode));
			TestConcurrencyPolicy(ConcurrencyPolicy.Ignore, nameof(WhsLocation.WLV_WarehouseType));
			TestConcurrencyPolicy(ConcurrencyPolicy.Ignore, nameof(WhsLocation.WLV_IsValidLocationForProductWarehousePutaway));
			TestConcurrencyPolicy(ConcurrencyPolicy.Ignore, nameof(WhsLocation.WLV_LastConfigChangedUtc));
		}

		public void TestConcurrencyPolicy(ConcurrencyPolicy expectedConcurrencyPolicy, string propertyName)
		{
			var location1 = Factory.New<WhsLocation>();
			AssertEquals(expectedConcurrencyPolicy, location1.ZPropertyInfoHash[propertyName].ConcurrencyPolicy);

			var location2 = Factory.New<WhsLocation>();
			var row = ((IBusinessObjectInternals)location2).Row;
			AssertEquals(expectedConcurrencyPolicy, ConcurrencyInfo.Get(row, row.Table.Columns[propertyName]));
		}

		#endregion

		#region TestWLV_LastInventoryChangeDate_Concurrency

		public void TestWLV_LastInventoryChangeDate_Concurrency()
		{
			var now = ZDateTimeOffset.Now;
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = row.Locations[0];
			Factory.Save();

			location.WLV_LastInventoryChangeDate = now;
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var locationInNewFactory = newFactory.Load<WhsLocation>(location.PK);
			locationInNewFactory.WLV_LastInventoryChangeDate = now.AddHours(-1);
			newFactory.Save();

			AssertNoExceptionThrown("Even both factory change WLV_LastInventoryChangeDate but it should ignore and save without error", Factory.Save);
			AssertEquals("Should save without issue.", now, location.WLV_LastInventoryChangeDate);
		}

		#endregion

		#region TestWLV_LastAllocatedOrChangedDateUtc

		[TestDate(2019, 3, 21, 3, 15, 30)]
		public void TestUpdateWLV_LastAllocatedOrChangedDateUtc_DefaultTime()
		{
			var now = ZDateTime.UtcNow;
			var nowAddOneDay = now.AddDays(1);
			var whs = Helper.CreateWarehouse("WHS");

			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = row.Locations[0];
			Factory.Save();

			AssertEquals("Precondition; WLV_LastAllocatedOrChangedDateUtc is empty.", ZDateTime.Empty, location.WLV_LastAllocatedOrChangedDateUtc);
			location.UpdateWLV_LastAllocatedOrChangedDateUtc(nowAddOneDay);
			AssertEquals("WLV_LastAllocatedOrChangedDateUtc set to datetime now + one day.", nowAddOneDay, location.WLV_LastAllocatedOrChangedDateUtc);
		}

		public void TestUpdateWLV_LastAllocatedOrChangedDateUtc_ThrowsWithNonUTC()
		{
			var location = Helper.CreateRowAndGenerateLocations(Helper.CreateWarehouse("WHS"), "A", 1, 1).Locations[0];
			AssertExceptionThrown<ArgumentException>(() => location.UpdateWLV_LastAllocatedOrChangedDateUtc(ZDateTime.Now));
		}

		[TestDate(2019, 3, 21, 3, 15, 30)]
		public void TestUpdateWLV_LastAllocatedOrChangedDateUtc_Empty()
		{
			var now = ZDateTime.Now;
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = row.Locations[0];
			Factory.Save();

			AssertEquals("Precondition; WLV_LastAllocatedOrChangedDateUtc is empty.", ZDateTime.Empty, location.WLV_LastAllocatedOrChangedDateUtc);
			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			AssertEquals("WLV_LastAllocatedOrChangedDateUtc set to datetime now.", now, location.WLV_LastAllocatedOrChangedDateUtc);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2019, 3, 21, 3, 15, 30)]
		public void TestUpdateWLV_LastAllocatedOrChangedDateUtc_UsesUTCTime()
		{
			TestDateAttribute.UseUNLOCO = true;
			var whs = Helper.CreateWarehouse("WHS");

			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = row.Locations[0];
			Factory.Save();

			AssertEquals("Precondition; WLV_LastAllocatedOrChangedDateUtc is empty.", ZDateTime.Empty, location.WLV_LastAllocatedOrChangedDateUtc);

			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			AssertEquals("WLV_LastAllocatedOrChangedDateUtc should use UtcNow.", ZDateTime.UtcNow, location.WLV_LastAllocatedOrChangedDateUtc);
		}

		[TestDate(2019, 3, 21, 3, 15, 30)]
		public void TestUpdateWLV_LastAllocatedOrChangedDateUtc_EarlierDate()
		{
			var now = ZDateTime.Now;
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = row.Locations[0];
			Factory.Save();

			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			AssertEquals("Precondition; WLV_LastAllocatedOrChangedDateUtc is not empty.", now, location.WLV_LastAllocatedOrChangedDateUtc);

			TestDateAttribute.Date = now.AddDays(-1).ToDateTime();
			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			AssertEquals("WLV_LastAllocatedOrChangedDateUtc should not change to an earlier date.", now, location.WLV_LastAllocatedOrChangedDateUtc);
		}

		[TestDate(2019, 3, 21, 3, 15, 30)]
		public void TestUpdateWLV_LastAllocatedOrChangedDateUtc_LaterDate()
		{
			var now = ZDateTime.Now;
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = row.Locations[0];
			Factory.Save();

			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			Factory.Save();

			AssertEquals("Precondition; WLV_LastAllocatedOrChangedDateUtc is npt empty.", now, location.WLV_LastAllocatedOrChangedDateUtc);

			TestDateAttribute.Date = now.AddDays(1).ToDateTime();
			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			AssertEquals("WLV_LastAllocatedOrChangedDateUtc should be update with new value.", now.AddDays(1), location.WLV_LastAllocatedOrChangedDateUtc);
		}

		#endregion

		#region TestWLV_LastAllocatedOrChangedID

		public void TestWLV_LastAllocatedOrChangedID_DefaultConcurrencyPolicy()
		{
			var now = ZDateTime.Now;
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = row.Locations[0];
			Factory.Save();

			AssertEquals("Default concurrency policy should be Ignore.", ConcurrencyPolicy.Ignore, location.WLV_LastAllocatedOrChangedIDInfo.ConcurrencyPolicy);
		}

		public void TestWLV_LastAllocatedOrChangedID_ValuesAfterChange()
		{
			var now = ZDateTime.Now;
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = row.Locations[0];
			Factory.Save();

			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			var changeID = location.WLV_LastAllocatedOrChangedID;
			AssertNotEquals("LastAllocatedOrChangedID should not be empty after update", ZGuid.Empty, changeID);
			Factory.Save();

			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			AssertNotEquals("LastAllocatedOrChangedID should change after each update", changeID, location.WLV_LastAllocatedOrChangedID);
		}

		public void TestWLV_LastAllocatedOrChangedID_DoNotChangeIDWithoutSave()
		{
			var whs = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1);
			var location = row.Locations[0];
			Factory.Save();

			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			var changeID = location.WLV_LastAllocatedOrChangedID;
			AssertNotEquals("LastAllocatedOrChangedID should not be empty after update", ZGuid.Empty, changeID);

			location.UpdateWLV_LastAllocatedOrChangedDateUtc();
			AssertEquals("LastAllocatedOrChangedID should not change before saving", changeID, location.WLV_LastAllocatedOrChangedID);
		}

		#endregion

		#region TestWLV_LastInventoryChangeDateForBinding

		public void TestWLV_LastInventoryChangeDateForBinding()
		{
			var testDate = new ZDateTime(2019, 09, 17, 10, 00, 00);
			var location = Factory.New<WhsLocation>();
			AssertEquals("Precondition: WLV_LastInventoryChangeDate is empty", ZDateTimeOffset.Empty, location.WLV_LastInventoryChangeDate);
			AssertEquals(ZDateTime.Empty, location.WLV_LastInventoryChangeDateForBinding);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			location.WLV_LastInventoryChangeDate = new ZDateTimeOffset(testDate, TimeSpan.FromMinutes(600));
			AssertEquals("Should display local time(SYD)", testDate, location.WLV_LastInventoryChangeDateForBinding);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNNJG";
			AssertEquals("Should display local time(NJG)", testDate.AddHours(-2), location.WLV_LastInventoryChangeDateForBinding);
		}

		#endregion

		#region TestWLV_CycleCountLastPerformedForBinding

		public void TestWLV_CycleCountLastPerformedForBinding()
		{
			var testDate = new ZDateTime(2019, 09, 17, 10, 00, 00);
			var location = Factory.New<WhsLocation>();
			AssertEquals("Precondition: WLV_CycleCountLastPerformed is empty", ZDateTimeOffset.Empty, location.WLV_CycleCountLastPerformed);
			AssertEquals(ZDateTime.Empty, location.WLV_CycleCountLastPerformedForBinding);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			location.WLV_CycleCountLastPerformed = new ZDateTimeOffset(testDate, TimeSpan.FromMinutes(600));
			AssertEquals("Should display local time(SYD)", testDate, location.WLV_CycleCountLastPerformedForBinding);

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "CNNJG";
			AssertEquals("Should display local time(NJG)", testDate.AddHours(-2), location.WLV_CycleCountLastPerformedForBinding);
		}

		#endregion

		#region TestCanTransferToOrFromAreaType

		public void TestCanTransferToOrFromAreaType_NullOrEmpty()
		{
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.FreeStore, null));
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.FreeStore, string.Empty));
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(null, AreaTypes.Codes.FreeStore));
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(string.Empty, AreaTypes.Codes.FreeStore));
		}

		public void TestCanTransferToOrFromAreaType_FreeStore()
		{
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.FreeStore, AreaTypes.Codes.FreeStore));
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.FreeStore, AreaTypes.Codes.DynamicPickFace));
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.FreeStore, AreaTypes.Codes.Bonded));
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.FreeStore, AreaTypes.Codes.Excise));
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.FreeStore, AreaTypes.Codes.DockDoor));
		}

		public void TestCanTransferToOrFromAreaType_Bonded()
		{
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.Bonded, AreaTypes.Codes.Bonded));
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.Bonded, AreaTypes.Codes.Excise));
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.Bonded, AreaTypes.Codes.DockDoor));
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.Bonded, AreaTypes.Codes.FreeStore));
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.Bonded, AreaTypes.Codes.DynamicPickFace));
		}

		public void TestCanTransferToOrFromAreaType_Excise()
		{
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.Excise, AreaTypes.Codes.Excise));
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.Excise, AreaTypes.Codes.Bonded));
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.Excise, AreaTypes.Codes.DockDoor));
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.Excise, AreaTypes.Codes.FreeStore));
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.Excise, AreaTypes.Codes.DynamicPickFace));
		}

		public void TestCanTransferToOrFromAreaType_DynamicPickFace()
		{
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.DynamicPickFace));
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.DockDoor));
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.FreeStore));
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.Bonded));
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.DynamicPickFace, AreaTypes.Codes.Excise));
		}

		public void TestCanTransferToOrFromAreaType_DockDoor()
		{
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.DockDoor, AreaTypes.Codes.DockDoor));
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.DockDoor, AreaTypes.Codes.DynamicPickFace));
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.DockDoor, AreaTypes.Codes.Bonded));
			AssertEquals(true, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.DockDoor, AreaTypes.Codes.Excise));
			AssertEquals(false, WhsLocation.CanTransferToOrFromAreaType(AreaTypes.Codes.DockDoor, AreaTypes.Codes.FreeStore));
		}

		#endregion

		#region TestCheckIsPropertyIsEnabledForValidation

		#region TestCheckWLV_RowNameIsDisabledForValidation

		public void TestCheckWLV_RowNameIsDisabledForValidation()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_RowName = "";
			AssertEquals("WLV_RowName Validation should be disabled.", false, location.IsValidationEnabled(location.WLV_RowNameInfo));
		}

		#endregion

		#region TestCheckWLV_WW_WhsIsDisabledForValidation

		public void TestCheckWLV_WW_WhsIsDisabledForValidation()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_WW_Whs = ZGuid.Empty;
			AssertEquals("WLV_WW_Whs Validation should be disabled.", false, location.IsValidationEnabled(location.WLV_WW_WhsInfo));
		}

		#endregion

		#endregion

		#region TestAuditFields

		[TestDate(2019, 05, 07)]
		public void TestAuditFields()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			Factory.Save();
			var locationA1 = warehouse.FindLocation("A");
			locationA1.WLV_WLT_LocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.DDL)).PK;
			locationA1.WLV_PutawayPathSequence = 1;
			var now = ZDateTime.UtcNow;

			AssertEquals("WLV_SystemCreateTimeUtc should be empty untill saved.", now, locationA1.WLV_SystemCreateTimeUtc);
			AssertEquals("WLV_SystemCreateUser should be empty untill saved.", "E", locationA1.WLV_SystemCreateUser);
			AssertEquals("WLV_SystemLastEditTimeUtc should be empty untill saved.", now, locationA1.WLV_SystemLastEditTimeUtc);
			AssertEquals("WLV_SystemLastEditUser should be empty untill saved.", "E", locationA1.WLV_SystemLastEditUser);

			Factory.Save();

			AssertEquals("WLV_SystemCreateTimeUtc should be now.", now, locationA1.WLV_SystemCreateTimeUtc);
			AssertEquals("WLV_SystemLastEditTimeUtc should be now.", now, locationA1.WLV_SystemLastEditTimeUtc);

			locationA1.WLV_PutawayPathSequence = 2;
			TestDateAttribute.Date = now.AddDays(2).ToDateTime();
			Factory.Save();

			AssertEquals("WLV_SystemCreateTimeUtc should be now.", now, locationA1.WLV_SystemCreateTimeUtc);
			AssertEquals("WLV_SystemLastEditTimeUtc should be now.", now.AddDays(2), locationA1.WLV_SystemLastEditTimeUtc);
		}

		#endregion

		#region TestUniqueIndexFailureHandlers

		// not sure why, but saving the location rolled back the transaction, this test works when using snapshot protection
		[UseSnapshotProtection(skipTransaction: true)]
		public void TestUniqueIndexFailureHandlers()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.IsFixedWidthLocation = true;
			warehouse.WW_LocationColumnsFixedWidth = 3;
			warehouse.WW_LocationLevelsFixedWidth = 1;
			warehouse.WW_LocationTraysFixedWidth = 1;

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "B", cols: 130);
			AssertType<LocationStringUniqueIndexValidationHandler>(((IBusinessObjectInternals)row.Locations[0]).UniqueIndexFailureHandlers.Single());
			Factory.Save();

			// row is saved first, then locations, the locations will cause the save failure
			Helper.CreateRowAndGenerateLocations(warehouse, "B006");

			var notifier = new TestNotificationHandler();
			var saveHitCount = 0;
			ZExceptionReporting.ProcessWithSaveExceptionHandling(() =>
			{
				// only try to save once, to check unique index handler.
				if (saveHitCount++ == 0)
				{
					Factory.Save();
				}
			}, null, notifier: notifier);
			AssertEquals("Should have correct error message from unique index validation handler.",
				"The changes to the location configurations on the row have resulted in a duplicate Location Barcode in the warehouse. Ensure the Row Name or Number of Columns/Levels/Trays does not result in a duplicate Location Barcode for Fixed Width Locations.", notifier.Messages.Single());
		}

		class TestNotificationHandler : INotificationHandler
		{
			public IEnumerable<string> Messages => messages;
			readonly List<string> messages = new List<string>();

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				messages.Add(message);
			}

			public void ReportInformation(string message, string caption)
			{
				messages.Add(message);
			}
		}

		#endregion

		#region TestIWhsLocationMembers

		public void TestIWhsLocationMembers()
		{
			var location = Factory.New<WhsLocation>();
			location.WLV_Column = 1;
			location.WLV_Level = 2;
			location.WLV_Tray = 3;
			location.WLV_PickPathSequence = 4;
			location.WLV_LocationStatus = "ZZZ";

			var pickingArea = Factory.New<WhsArea>();
			pickingArea.WA_Name = "PICK";
			location.WLV_WA_PickingArea = pickingArea.PK;

			var locationType = Helper.CreateLocationType("AAA", LocationClasses.Codes.NOR);
			location.WLV_WLT_LocationType = locationType.PK;

			var locationInterface = (IWhsLocation)location;
			AssertEquals(nameof(IWhsLocation.PK), location.PK, locationInterface.PK);
			AssertEquals(nameof(IWhsLocation.WLV_LocationClass), "NOR", locationInterface.WLV_LocationClass);
			AssertEquals(nameof(IWhsLocation.WLV_LocationStatus), "ZZZ", locationInterface.WLV_LocationStatus);
			AssertEquals(nameof(IWhsLocation.PickingAreaName), "PICK", locationInterface.PickingAreaName);
			AssertEquals(nameof(IWhsLocation.WLV_Column), (short)1, locationInterface.WLV_Column);
			AssertEquals(nameof(IWhsLocation.WLV_Level), (short)2, locationInterface.WLV_Level);
			AssertEquals(nameof(IWhsLocation.WLV_Tray), (short)3, locationInterface.WLV_Tray);
			AssertEquals(nameof(IWhsLocation.WLV_PickPathSequence), 4, locationInterface.WLV_PickPathSequence);
		}

		public void TestIWhsLocationMembers_WLV_LocationTypeCode()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var locationInterface = (IWhsLocation)location;
			AssertEquals(nameof(IWhsLocation.WLV_LocationTypeCode), "RNO", locationInterface.WLV_LocationTypeCode);

			var locationType = Helper.CreateLocationType("AAA", LocationClasses.Codes.NOR);
			location.WLV_WLT_LocationType = locationType.PK;
			Factory.Save();
			AssertEquals(nameof(IWhsLocation.WLV_LocationTypeCode), "AAA", locationInterface.WLV_LocationTypeCode);
		}

		public void TestIWhsLocationMembers_WLV_RowName()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var location = data.Whs1.DefaultLocation;
			var locationInterface = (IWhsLocation)location;
			Factory.Save();

			AssertEquals(nameof(IWhsLocation.WLV_RowName), "A", locationInterface.WLV_RowName);
		}

		#endregion

		#region TestICodeDescriptionMembers

		public void TestICodeDescriptionMembers()
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("WHS", 3, 3, 2);
			Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);
			Factory.Save();

			var location = warehouse.FindLocation("A00200202");
			AssertNotNull("Precondition", location);
			AssertEquals("Precondition", "A00200202", location.WLV_LocationString);
			AssertEquals("Precondition", "A-002-002-02", location.WLV_LocationString_UserFriendly);

			var locationCodeDescription = (ICodeDescription)location;
			AssertEquals("UserFriendly location string is used as location code.", "A-002-002-02", locationCodeDescription.Code);
			AssertEquals("UserFriendly location string is used as location description.", "A-002-002-02", locationCodeDescription.Description);
		}

		#endregion

		#region TestWarehouseFromIWhsLocation

		public void TestWarehouseFromIWhsLocation()
		{
			var location = Factory.New<WhsLocation>();
			var warehouse = Factory.New<WhsWarehouse>();
			location.WLV_WW_Whs = warehouse.PK;
			var whs = ((IWhsLocation)location).Warehouse;
			AssertNotNull(whs);
			AssertEquals(warehouse, whs);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			WhsRow row = factory.NewWithValidTestData<WhsRow>();
			row.WR_Columns = 1;
			row.WR_Levels = 1;
			((WhsRowInternals)row).GenerateLocations();
			WhsLocation result = row.Locations[0];
			result.FillWithValidTestData();
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			var location = (WhsLocation)base.GetNewBusinessObjectForDefaultLightValidationTest();
			location.Row.WR_WW_Whs = ZGuid.Empty;
			return location;
		}

		WhsLocation Location
		{
			get { return location ?? (location = Factory.New<WhsLocation>()); }
			set { location = value; }
		}

		WhsLocation location;

		#endregion
	}

	class LocationTriggerTest : TestCaseWithFactory
	{
		#region TestTriggerPreventsDeletionDefaultDockDoorLocation

		public void TestTriggerPreventsDeletionDefaultDockDoorLocation()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			Factory.Save();

			Assert("Default ODDL must be set during save", !warehouse.WW_DefaultOutboundDockDoor.IsEmpty);
			AssertEquals("2 rows must be created. DOCKDOOR and A.", 2, warehouse.Rows.Count);

			warehouse.DefaultOutboundDockDoorLocation.Delete();
			// shouldn't be able to delete location pointed by WW_DefaultOutboundDockDoor
			AssertExceptionThrown<ZSaveException>(() => Factory.Save());
		}

		#endregion

		#region TestItIsOkToDeleteDockDoorLocationIfItIsNotReferenced

		public void TestItIsOkToDeleteDockDoorLocationIfItIsNotReferenced()
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			Factory.Save();
			var locationA1 = warehouse.FindLocation("A");
			locationA1.WLV_WLT_LocationType = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_LocationClass, LocationClasses.Codes.DDL)).PK;
			Factory.Save();

			Assert("Default ODDL must be set during save", !warehouse.WW_DefaultOutboundDockDoor.IsEmpty);
			AssertEquals("2 rows must be created. DOCKDOOR and A.", 2, warehouse.Rows.Count);

			var defaultDDL = warehouse.DefaultOutboundDockDoorLocation;
			warehouse.WW_DefaultInboundDockDoor = locationA1.PK;
			warehouse.WW_DefaultOutboundDockDoor = locationA1.PK;
			Factory.Save(); // switching default DDL for a warehouse

			defaultDDL.Delete();
			AssertNoExceptionThrown(() => Factory.Save());
			// now we can delete original DDL location without any problems
		}

		#endregion

		#region TestTriggerDisablePickFaceLocationCapacity

		const string DisablePickFaceLocationCapacity = "Max capacity of Pick Face Location must be 0.";

		#region TestTriggerDisablePickFaceLocationCapacity_MaxCapacity

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Insert_FIXLocation_WL_MaxWeight()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.FIX, true, l => l.WLV_MaxWeightInfo);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Insert_FIXLocation_WL_MaxCubic()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.FIX, true, l => l.WLV_MaxCubicInfo);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Insert_FIXLocation_WL_MaxQuantity()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.FIX, true, l => l.WLV_MaxQuantityInfo);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Insert_DPFLocation_WL_MaxWeight()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.DPF, true, l => l.WLV_MaxWeightInfo);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Insert_DPFLocation_WL_MaxCubic()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.DPF, true, l => l.WLV_MaxCubicInfo);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Insert_DPFLocation_WL_MaxQuantity()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.DPF, true, l => l.WLV_MaxQuantityInfo);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Insert_NonPickFaceLocation()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.NOR, false, l => l.WLV_MaxWeightInfo);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Update_FIXLocation_WL_MaxWeight()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.FIX, true, l => l.WLV_MaxWeightInfo, isUpdate: true);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Update_FIXLocation_WL_MaxCubic()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.FIX, true, l => l.WLV_MaxCubicInfo, isUpdate: true);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Update_FIXLocation_WL_MaxQuantity()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.FIX, true, l => l.WLV_MaxQuantityInfo, isUpdate: true);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Update_DPFLocation_WL_MaxWeight()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.DPF, true, l => l.WLV_MaxWeightInfo, isUpdate: true);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Update_DPFLocation_WL_MaxCubic()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.DPF, true, l => l.WLV_MaxCubicInfo, isUpdate: true);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Update_DPFLocation_WL_MaxQuantity()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.DPF, true, l => l.WLV_MaxQuantityInfo, isUpdate: true);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_Update_NonPickFaceLocation()
		{
			TestTriggerDisablePickFaceLocationCapacityCore(LocationClasses.Codes.NOR, false, l => l.WLV_MaxWeightInfo, isUpdate: true);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacityCore(ZString locationClass, bool expectedHasError, Func<WhsLocation, ZPropertyInfo> propertyFunc, bool isUpdate = false)
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var locationType = locationClass == "FIX" ? Helper.CreateLocationType("AAA", "AAA Test", false, 1, locationClass) : Helper.CreateLocationType("AAA", locationClass);
			Factory.Save();

			var location = warehouse.FindLocation("A");
			location.WLV_WLT_LocationType = locationType.PK;
			location.WLV_MaxWeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			location.WLV_MaxCubicUnit = Enterprise.Core.Constants.Volume.CubicMetres;

			if (isUpdate)
			{
				Factory.Save();
			}

			var propertyInfo = propertyFunc(location);
			location.SetPropertyValue(propertyInfo.Name, (ZDecimal)5m);
			if (expectedHasError)
			{
				NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(SqlException), DisablePickFaceLocationCapacity), "Should have SQLException");
			}
			else
			{
				AssertNoExceptionThrown("Should not have exception", () => Factory.Save());
			}
		}

		#endregion

		#region TestTriggerDisablePickFaceLocationCapacity_WL_WLT_LocationType

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_WL_WLT_LocationType_FIX()
		{
			TestTriggerDisablePickFaceLocationCapacity_WL_WLT_LocationTypeCore(LocationClasses.Codes.FIX);
		}

		[ExpectNoExceptions]
		public void TestTriggerDisablePickFaceLocationCapacity_WL_WLT_LocationType_DPF()
		{
			TestTriggerDisablePickFaceLocationCapacity_WL_WLT_LocationTypeCore(LocationClasses.Codes.DPF);
		}

		[ExpectNoExceptions]
		void TestTriggerDisablePickFaceLocationCapacity_WL_WLT_LocationTypeCore(string locationClass)
		{
			var warehouse = Helper.CreateWarehouse("W1", "A");
			var locationType = Helper.CreateLocationType("AAA", LocationClasses.Codes.NOR);
			var targetLocationType = locationClass == "FIX" ? Helper.CreateLocationType("BBB", "BBB Test", false, 1, locationClass) : Helper.CreateLocationType("BBB", locationClass);
			Factory.Save();

			var location = warehouse.FindLocation("A");
			location.WLV_MaxWeight = 5m;
			location.WLV_MaxWeightUnit = Enterprise.Core.Constants.Weight.Kilograms;
			AssertNoExceptionThrown("Should not have exception", () => Factory.Save());

			location.WLV_WLT_LocationType = targetLocationType.PK;
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), DisablePickFaceLocationCapacity), "Should have SQLException");
		}

		#endregion

		#endregion

		#region Implementation

		WhsTestHelperFunctionsEnv helper;
		protected WhsTestHelperFunctionsEnv Helper => helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory));

		#endregion
	}
}
