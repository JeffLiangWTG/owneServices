using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsLocationViewTest : WhsTestCaseWithFactory
	{
		#region TestView_WhsLocationView

		public void TestView_WhsLocationView()
		{
			var whs = Helper.CreateWarehouse("1");
			var row1 = Helper.CreateRow(whs, "A", 1, 1);
			var row2 = Helper.CreateRow(whs, "B", 2, 1);
			var row3 = Helper.CreateRow(whs, "C", 1, 2);
			var row4 = Helper.CreateRow(whs, "D", 1, 1, 2);
			var row5 = Helper.CreateRow(whs, "E", 3, 4, 5);
			Factory.Save();

			AssertEquals("Location string is incorrect.", "A", GetLocationString(whs, row1, 1, 1, 1));
			AssertEquals("Location string is incorrect.", "B-2", GetLocationString(whs, row2, 2, 1, 1));
			AssertEquals("Location string is incorrect.", "C-1-2", GetLocationString(whs, row3, 1, 2, 1));
			AssertEquals("D-1-1-2", "D-1-1-2", GetLocationString(whs, row4, 1, 1, 2));
			AssertEquals("E-3-4-5", "E-3-4-5", GetLocationString(whs, row5, 3, 4, 5));

			whs.WW_LocationComponentDelimiter = ".";
			Factory.Save();
			AssertEquals("E.3.4.5", "E.3.4.5", GetLocationString(whs, row5, 3, 4, 5));

			whs.WW_LocationColumnsAlpha = true;
			Factory.Save();
			AssertEquals("E.C.4.5", "E.C.4.5", GetLocationString(whs, row5, 3, 4, 5));

			whs.WW_LocationLevelsAlpha = true;
			Factory.Save();
			AssertEquals("E.C.D.5", "E.C.D.5", GetLocationString(whs, row5, 3, 4, 5));

			whs.WW_LocationTraysAlpha = true;
			Factory.Save();
			AssertEquals("E.C.D.E", "E.C.D.E", GetLocationString(whs, row5, 3, 4, 5));

			whs.WW_LocationColumnsAlpha = false;
			whs.WW_LocationLevelsAlpha = false;
			whs.WW_LocationTraysAlpha = false;
			whs.WW_LocationComponentDelimiter = "-";
			var row6 = Helper.CreateRow(whs, "F", 100, 10, 10);
			Factory.Save();
			AssertEquals("F-5-6-7", "F-5-6-7", GetLocationString(whs, row6, 5, 6, 7));

			whs.WW_LocationsHaveLeadingZeros = true;
			Factory.Save();
			AssertEquals("F-005-06-07", "F-005-06-07", GetLocationString(whs, row6, 5, 6, 7));
			AssertEquals("F-035-08-01", "F-035-08-01", GetLocationString(whs, row6, 35, 8, 1));
			AssertEquals("F-100-10-10", "F-100-10-10", GetLocationString(whs, row6, 100, 10, 10));

			whs.WW_LocationColumnsZeroBased = true;
			whs.WW_LocationLevelsZeroBased = true;
			whs.WW_LocationTraysZeroBased = true;
			Factory.Save();
			AssertEquals("F-01-9-9", "F-01-9-9", GetLocationString(whs, row6, 2, 10, 10));
		}

		#endregion

		#region TestView_WhsLocationView_ExtraLocationOnRow

		public void TestView_WhsLocationView_ExtraLocationOnRowWithSingleColumnLevelTray_ZeroBase()
		{
			View_WhsLocationView_ExtraLocationOnRowWithSingleColumnLevelTray(zeroBased: true);
		}

		public void TestView_WhsLocationView_ExtraLocationOnRowWithSingleColumnLevelTray_NoneZeroBase()
		{
			View_WhsLocationView_ExtraLocationOnRowWithSingleColumnLevelTray(zeroBased: false);
		}

		void View_WhsLocationView_ExtraLocationOnRowWithSingleColumnLevelTray(bool zeroBased)
		{
			var whs = Helper.CreateWarehouse("ABC");
			whs.WW_LocationColumnsZeroBased = zeroBased;
			whs.WW_LocationLevelsZeroBased = zeroBased;
			whs.WW_LocationTraysZeroBased = zeroBased;

			var row = Helper.CreateRow(whs, "A", 1, 1, 1);
			Factory.Save();

			var extraLocation = Factory.New<WhsLocation>();
			extraLocation.WLV_Column = 2;
			extraLocation.WLV_Level = 2;
			extraLocation.WLV_Tray = 2;
			extraLocation.WLV_PutawayPathSequence = 2;
			extraLocation.WLV_WR = row.PK;
			extraLocation.WLV_WW_Whs = whs.PK;
			extraLocation.WLV_WA_PickingArea = row.Locations[0].WLV_WA_PickingArea;
			extraLocation.WLV_WA_PutawayArea = row.Locations[0].WLV_WA_PutawayArea;
			extraLocation.WLV_WLT_LocationType = row.Locations[0].WLV_WLT_LocationType;

			Factory.Save();

			AssertEquals("A", "A", GetLocationString(whs, row, 1, 1, 1));
			if (zeroBased)
			{
				AssertEquals("A-1-1-1", "A-1-1-1", GetLocationString(whs, row, 2, 2, 2));
			}
			else
			{
				AssertEquals("A-2-2-2", "A-2-2-2", GetLocationString(whs, row, 2, 2, 2));
			}
		}

		#endregion

		#region TestView_WhsLocationViewColumnLevelAndTray

		public void TestView_WhsLocationView_SingleColumnZeroBased()
		{
			View_WhsLocationView_SingleColumnLevelTrayWithZeroBased(locationColumnsZeroBased: true,
				locationLevelsZeroBased: false, locationTraysZeroBased: false);
		}

		public void TestView_WhsLocationView_SingleLevelWithZeroBased()
		{
			View_WhsLocationView_SingleColumnLevelTrayWithZeroBased(locationColumnsZeroBased: false,
				locationLevelsZeroBased: true, locationTraysZeroBased: false);
		}

		public void TestView_WhsLocationView_SingleTrayWithZeroBased()
		{
			View_WhsLocationView_SingleColumnLevelTrayWithZeroBased(locationColumnsZeroBased: false,
				locationLevelsZeroBased: false, locationTraysZeroBased: true);
		}

		public void TestView_WhsLocationView_SingleColumnLevelTrayWithZeroBased()
		{
			View_WhsLocationView_SingleColumnLevelTrayWithZeroBased(locationColumnsZeroBased: true,
				locationLevelsZeroBased: true, locationTraysZeroBased: true);
		}

		void View_WhsLocationView_SingleColumnLevelTrayWithZeroBased(bool locationColumnsZeroBased,
			bool locationLevelsZeroBased, bool locationTraysZeroBased)
		{
			var whs = Helper.CreateWarehouse("ABC");
			var row = Helper.CreateRow(whs, "A", 1, 1, 1);

			whs.WW_LocationColumnsZeroBased = locationColumnsZeroBased;
			whs.WW_LocationLevelsZeroBased = locationLevelsZeroBased;
			whs.WW_LocationTraysZeroBased = locationTraysZeroBased;

			Factory.Save();

			AssertEquals("A", "A", GetLocationString(whs, row, 1, 1, 1));
		}

		#endregion

		#region TestView_IsVirtualWarehouse

		public void TestView_IsVirtualWarehouse_False() => TestView_IsVirtualWarehouse(isVirtualWarehouse: false);

		public void TestView_IsVirtualWarehouse_True() => TestView_IsVirtualWarehouse(isVirtualWarehouse: true);

		void TestView_IsVirtualWarehouse(bool isVirtualWarehouse)
		{
			var whs = Helper.CreateWarehouse("ABC");
			var row = Helper.CreateRow(whs, "A", 1, 1, 1);

			whs.WW_IsVirtualWarehouse = isVirtualWarehouse;
			Factory.Save();

			AssertEquals("WLV_IsVirtualWarehouse should be set to WW_IsVirtualWarehouse.", isVirtualWarehouse,
				GetLocationValue<ZBool>(whs, row, 1, 1, 1, "WLV_IsVirtualWarehouse"));
		}

		#endregion

		#region TestView_WarehouseType

		public void TestView_WarehouseType_PRW() => TestView_WarehouseType(warehouseType: "PRW");

		public void TestView_WarehouseType_TRW() => TestView_WarehouseType(warehouseType: "TRW");

		void TestView_WarehouseType(string warehouseType)
		{
			var whs = Helper.CreateWarehouse("ABC");
			var row = Helper.CreateRow(whs, "A", 1, 1, 1);

			whs.WW_WarehouseType = warehouseType;
			Factory.Save();

			AssertEquals("WLV_WarehouseType should be set to WW_WarehouseType.", warehouseType,
				GetLocationValue<ZString>(whs, row, 1, 1, 1, "WLV_WarehouseType"));
		}

		#endregion

		#region TestView_LocationClass

		public void TestView_LocationClass_NOR() => TestView_LocationClass(locationClass: "NOR");

		public void TestView_LocationClass_DDL() => TestView_LocationClass(locationClass: "DDL");

		void TestView_LocationClass(string locationClass)
		{
			var whs = Helper.CreateWarehouse("ABC");
			var row = Helper.CreateRow(whs, "A", 1, 1, 1);
			Factory.Save();

			var customLocationType = Helper.CreateLocationType("ABC", locationClass);
			row.Locations.Single().WLV_WLT_LocationType = customLocationType.PK;
			Factory.Save();

			AssertEquals("WLV_LocationClass should be set to WLT_LocationClass.", locationClass,
				GetLocationValue<ZString>(whs, row, 1, 1, 1, "WLV_LocationClass"));
		}

		#endregion

		#region TestView_LocationTypeCode

		public void TestView_LocationTypeCode_ABC() => TestView_LocationTypeCode(locationTypeCode: "ABC");

		public void TestView_LocationTypeCode_XYZ() => TestView_LocationTypeCode(locationTypeCode: "XYZ");

		void TestView_LocationTypeCode(string locationTypeCode)
		{
			var whs = Helper.CreateWarehouse("ABC");
			var row = Helper.CreateRow(whs, "A", 1, 1, 1);
			Factory.Save();

			var customLocationType = Helper.CreateLocationType(locationTypeCode);
			row.Locations.Single().WLV_WLT_LocationType = customLocationType.PK;
			Factory.Save();

			AssertEquals("WLV_LocationTypeCode should be set to WLT_Code.", locationTypeCode,
				GetLocationValue<ZString>(whs, row, 1, 1, 1, "WLV_LocationTypeCode"));
		}

		#endregion

		#region TestView_PickingAreaType

		public void TestView_PickingAreaType_SettingTo_DDA() =>
			TestView_PickingAreaType_SettingTo(AreaTypes.Codes.DockDoor);

		public void TestView_PickingAreaType_SettingTo_BON() =>
			TestView_PickingAreaType_SettingTo(AreaTypes.Codes.Bonded);

		public void TestView_PickingAreaType_SettingTo_DPF() =>
			TestView_PickingAreaType_SettingTo(AreaTypes.Codes.DynamicPickFace);

		public void TestView_PickingAreaType_SettingTo_EXC() =>
			TestView_PickingAreaType_SettingTo(AreaTypes.Codes.Excise);

		public void TestView_PickingAreaType_SettingTo_FRE() =>
			TestView_PickingAreaType_SettingTo(AreaTypes.Codes.FreeStore);

		void TestView_PickingAreaType_SettingTo(string areaType)
		{
			var whs = Helper.CreateWarehouse("ABC");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1, 1);
			var area = Helper.CreateArea(whs, "A", AreaTypes.Codes.FreeStore, true, false);
			var location = row.Locations.Single();
			location.WLV_WA_PickingArea = area.PK;
			area.WA_AreaType = areaType;
			Factory.Save();

			AssertEquals($"WLV_PickingAreaType should be set to {areaType}.", areaType,
				GetLocationValue<ZString>(whs, row, 1, 1, 1, "WLV_PickingAreaType"));
		}

		#endregion

		#region TestView_PutawayAreaType

		public void TestView_PutawayAreaType_SettingTo_DDA() =>
			TestView_PutawayAreaType_SettingTo(AreaTypes.Codes.DockDoor);

		public void TestView_PutawayAreaType_SettingTo_BON() =>
			TestView_PutawayAreaType_SettingTo(AreaTypes.Codes.Bonded);

		public void TestView_PutawayAreaType_SettingTo_EXC() =>
			TestView_PutawayAreaType_SettingTo(AreaTypes.Codes.Excise);

		public void TestView_PutawayAreaType_SettingTo_FRE() =>
			TestView_PutawayAreaType_SettingTo(AreaTypes.Codes.FreeStore);

		void TestView_PutawayAreaType_SettingTo(string areaType)
		{
			var whs = Helper.CreateWarehouse("ABC");
			var row = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1, 1);
			var area = Helper.CreateArea(whs, "A");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = area.PK;
			area.WA_AreaType = areaType;
			Factory.Save();

			AssertEquals($"WLV_PutawayAreaType should be set to {areaType}.", areaType,
				GetLocationValue<ZString>(whs, row, 1, 1, 1, "WLV_PutawayAreaType"));
		}

		#endregion

		#region TestView_WLV_LastConfigChangedUtc

		public void TestView_WLV_LastConfigChangedUtc()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS");
			var pickArea = Helper.CreateArea(productWarehouse, "PICK", AreaTypes.Codes.FreeStore, isPutawayArea: false);
			var putawayArea = Helper.CreateArea(productWarehouse, "PUTAWAY", AreaTypes.Codes.FreeStore, isPickingArea: false);
			var row = Helper.CreateRowAndGenerateLocations(productWarehouse, "A");
			var locationType = Helper.CreateLocationType("ABC");
			var location = row.Locations.Single();
			location.WLV_WA_PutawayArea = putawayArea.PK;
			location.WLV_WA_PickingArea = pickArea.PK;
			location.WLV_WLT_LocationType = locationType.PK;

			Factory.Save();

			var dateGetters = new Func<ZDateTime>[]
			{
				() => productWarehouse.WW_SystemLastEditTimeUtc,
				() => pickArea.WA_SystemLastEditTimeUtc,
				() => putawayArea.WA_SystemLastEditTimeUtc,
				() => row.WR_SystemLastEditTimeUtc,
				() => locationType.WLT_SystemLastEditTimeUtc,
				() => location.WLV_SystemLastEditTimeUtc,
			};

			AssertEquals("Precondition: All last edit times are equal.", true, dateGetters.Select(d => d().ToSmallDateTimeFloor()).AllSame());
			AssertEquals("Last Config Changed should be correct.", dateGetters[0].Invoke().ToSmallDateTimeFloor(), GetLocationValue<ZDateTime>(productWarehouse, row, 1, 1, 1, nameof(WhsLocation.WLV_LastConfigChangedUtc)));

			var nextTime = productWarehouse.WW_SystemLastEditTimeUtc.AddDays(1).ToSmallDateTimeFloor();
			CargoWise.Database.TestFramework.ObjectModel.WhsWarehouse.UpdateWhere(productWarehouse.PK.ToGuid()).Set(w => w.WW_SystemLastEditTimeUtc, nextTime.ToDateTime()).Post(TestConnection);
			AssertEquals("Last Config Changed should be Warehouse.", nextTime, GetLocationValue<ZDateTime>(productWarehouse, row, 1, 1, 1, nameof(WhsLocation.WLV_LastConfigChangedUtc)));

			nextTime = nextTime.AddDays(1);
			CargoWise.Database.TestFramework.ObjectModel.WhsRow.UpdateWhere(row.PK.ToGuid()).Set(w => w.WR_SystemLastEditTimeUtc, nextTime.ToDateTime()).Post(TestConnection);
			AssertEquals("Last Config Changed should be Row.", nextTime, GetLocationValue<ZDateTime>(productWarehouse, row, 1, 1, 1, nameof(WhsLocation.WLV_LastConfigChangedUtc)));

			nextTime = nextTime.AddDays(1);
			CargoWise.Database.TestFramework.ObjectModel.WhsArea.UpdateWhere(pickArea.PK.ToGuid()).Set(w => w.WA_SystemLastEditTimeUtc, nextTime.ToDateTime()).Post(TestConnection);
			AssertEquals("Last Config Changed should be Pick Area.", nextTime, GetLocationValue<ZDateTime>(productWarehouse, row, 1, 1, 1, nameof(WhsLocation.WLV_LastConfigChangedUtc)));

			nextTime = nextTime.AddDays(1);
			CargoWise.Database.TestFramework.ObjectModel.WhsArea.UpdateWhere(putawayArea.PK.ToGuid()).Set(w => w.WA_SystemLastEditTimeUtc, nextTime.ToDateTime()).Post(TestConnection);
			AssertEquals("Last Config Changed should be Putaway Area.", nextTime, GetLocationValue<ZDateTime>(productWarehouse, row, 1, 1, 1, nameof(WhsLocation.WLV_LastConfigChangedUtc)));

			nextTime = nextTime.AddDays(1);
			CargoWise.Database.TestFramework.ObjectModel.WhsLocationType.UpdateWhere(locationType.PK.ToGuid()).Set(w => w.WLT_SystemLastEditTimeUtc, nextTime.ToDateTime()).Post(TestConnection);
			AssertEquals("Last Config Changed should be Location Type.", nextTime, GetLocationValue<ZDateTime>(productWarehouse, row, 1, 1, 1, nameof(WhsLocation.WLV_LastConfigChangedUtc)));

			nextTime = nextTime.AddDays(1);
			CargoWise.Database.TestFramework.ObjectModel.WhsLocation.UpdateWhere(location.PK.ToGuid()).Set(w => w.WL_SystemLastEditTimeUtc, nextTime.ToDateTime()).Post(TestConnection);
			AssertEquals("Last Config Changed should be Location.", nextTime, GetLocationValue<ZDateTime>(productWarehouse, row, 1, 1, 1, nameof(WhsLocation.WLV_LastConfigChangedUtc)));
		}

		#endregion

		#region TestView_WLV_IsValidLocationForProductWarehousePutaway

		public void TestView_WLV_IsValidLocationForProductWarehousePutaway()
		{
			var productWarehouse = Helper.CreateWarehouse("WHS");

			var ftzWarehouse = Helper.CreateWarehouse("FTZ");
			ftzWarehouse.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;

			var transitWarehouse = Helper.CreateWarehouse("TWH");
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var virtualWarehouse = Helper.CreateWarehouse("VIR");
			virtualWarehouse.WW_IsVirtualWarehouse = true;

			var inactiveWarehouse = Helper.CreateWarehouse("INA");

			var dockDoorType = Helper.CreateLocationType("DDL", LocationClasses.Codes.DDL);
			var packingStationType = Helper.CreateLocationType("PST", "Packing Station", isPalletIDNeutral: false, 0, LocationClasses.Codes.PST);
			var consolidationType = Helper.CreateLocationType("CON", "Consolidation Location", isPalletIDNeutral: false, 0, LocationClasses.Codes.CON);

			var ddlLocation = Helper.CreateRowAndGenerateLocations(productWarehouse, "DOCK1");
			var packingStation = Helper.CreateRowAndGenerateLocations(productWarehouse, "Packing");
			var consolidationLocation = Helper.CreateRowAndGenerateLocations(productWarehouse, "Consolidate");
			ddlLocation.Locations.Single().WLV_WLT_LocationType = dockDoorType.PK;
			packingStation.Locations.Single().WLV_WLT_LocationType = packingStationType.PK;
			consolidationLocation.Locations.Single().WLV_WLT_LocationType = consolidationType.PK;

			var normalLocations = Helper.CreateRowAndGenerateLocations(productWarehouse, "A", 2);
			var ftzLocation = Helper.CreateRowAndGenerateLocations(ftzWarehouse, "F");
			var transitLocation = Helper.CreateRowAndGenerateLocations(transitWarehouse, "T");
			var virtualLocation = Helper.CreateRowAndGenerateLocations(virtualWarehouse, "V");
			var inactiveLocation = Helper.CreateRowAndGenerateLocations(inactiveWarehouse, "I");

			var normalLocation = normalLocations.Locations.Single(l => l.WLV_Column == 1);
			var voidLocation = normalLocations.Locations.Single(l => l.WLV_Column == 2);
			voidLocation.WLV_LocationStatus = LocationStatus.Codes.Void;
			inactiveWarehouse.WW_IsActive = false;
			Factory.Save();

			AssertEquals("Product Warehouse Location is valid.", true, GetLocationValue<ZBool>(productWarehouse, normalLocations, 1, 1, 1, nameof(WhsLocation.WLV_IsValidLocationForProductWarehousePutaway)));
			AssertEquals("FTZ Warehouse Location is valid.", true, GetLocationValue<ZBool>(ftzWarehouse, ftzLocation, 1, 1, 1, nameof(WhsLocation.WLV_IsValidLocationForProductWarehousePutaway)));
			AssertEquals("Location is Void.", false, GetLocationValue<ZBool>(productWarehouse, normalLocations, 2, 1, 1, nameof(WhsLocation.WLV_IsValidLocationForProductWarehousePutaway)));
			AssertEquals("Location is Dock Door.", false, GetLocationValue<ZBool>(productWarehouse, ddlLocation, 1, 1, 1, nameof(WhsLocation.WLV_IsValidLocationForProductWarehousePutaway)));
			AssertEquals("Location is Packing Station.", false, GetLocationValue<ZBool>(productWarehouse, packingStation, 1, 1, 1, nameof(WhsLocation.WLV_IsValidLocationForProductWarehousePutaway)));
			AssertEquals("Location is Consolidation Location.", false, GetLocationValue<ZBool>(productWarehouse, consolidationLocation, 1, 1, 1, nameof(WhsLocation.WLV_IsValidLocationForProductWarehousePutaway)));
			AssertEquals("Location is in Transit Warehouse.", false, GetLocationValue<ZBool>(transitWarehouse, transitLocation, 1, 1, 1, nameof(WhsLocation.WLV_IsValidLocationForProductWarehousePutaway)));
			AssertEquals("Location is in Virtual Warehouse.", false, GetLocationValue<ZBool>(virtualWarehouse, virtualLocation, 1, 1, 1, nameof(WhsLocation.WLV_IsValidLocationForProductWarehousePutaway)));
			AssertEquals("Location is in Inactive Warehouse.", false, GetLocationValue<ZBool>(inactiveWarehouse, inactiveLocation, 1, 1, 1, nameof(WhsLocation.WLV_IsValidLocationForProductWarehousePutaway)));
		}

		#endregion

		#region TestWhsLocationView_DoNotUse

		#region TestWhsLocationView_DoNotUse_Exists

		public void TestWhsLocationView_DoNotUse_Exists()
		{
			var whs = Helper.CreateWarehouse("ABC");
			var row = Helper.CreateRow(whs, "A", 1, 1, 1);

			whs.WW_LocationColumnsZeroBased = true;
			whs.WW_LocationLevelsZeroBased = true;
			whs.WW_LocationTraysZeroBased = true;

			Factory.Save();

			var result = new DynamicBusinessObjectCollection(Factory);
			var sql =
				$"select WLV_LocationString from dbo.WhsLocationView_DoNotUse Where WLV_WW_Whs = '{whs.PK}' And WLV_RowName = '{row.WR_Name}' And WLV_Column = 1 And WLV_Level = 1 And WLV_Tray = 1";

			AssertNoExceptionThrown(() => result.Load(sql));
			var resultTopRow = result[0];

			AssertEquals("WhsLocationView_DoNotUse should exist. Result should not be null", "A",
				(ZString)resultTopRow["WLV_LocationString"]);
		}

		#endregion

		#region TestWhsLocationView_DoNotUse_OnlyHasColumnsThatDoNotChangeOften

		public void TestWhsLocationView_DoNotUse_OnlyHasColumnsThatDoNotChangeOften()
		{
			// Do *NOT* add columns that are updated often (during day to day warehouse activities) to WhsLocationView_DoNotUse, it should be added to WhsLocationView instead
			// otherwise Indexed View will be rebuild with every update, and this will cause performance problems
			var columnsThatAreNotUpdatedOften = new[]
			{
				WhsLocationViewSchema.Constants.PK, WhsLocationViewSchema.Constants.WLV_ApprovedKnownLocation,
				WhsLocationViewSchema.Constants.WLV_CheckDigit,
				WhsLocationViewSchema.Constants.WLV_Column,
				WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence,
				WhsLocationViewSchema.Constants.WLV_FormattedColumn,
				WhsLocationViewSchema.Constants.WLV_FormattedLevel,
				WhsLocationViewSchema.Constants.WLV_FormattedTray, WhsLocationViewSchema.Constants.WLV_IsValid,
				WhsLocationViewSchema.Constants.WLV_Level, WhsLocationViewSchema.Constants.WLV_LocationStatus,
				WhsLocationViewSchema.Constants.WLV_LocationString,
				WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly,
				WhsLocationViewSchema.Constants.WLV_MaxCubic, WhsLocationViewSchema.Constants.WLV_MaxCubicUnit,
				WhsLocationViewSchema.Constants.WLV_MaximumPickCountBeforeAutomatedStocktake,
				WhsLocationViewSchema.Constants.WLV_MaxQuantity,
				WhsLocationViewSchema.Constants.WLV_MaxQuantityUnit, WhsLocationViewSchema.Constants.WLV_MaxWeight,
				WhsLocationViewSchema.Constants.WLV_MaxWeightUnit, WhsLocationViewSchema.Constants.WLV_MaxWidth,
				WhsLocationViewSchema.Constants.WLV_MaxDepth, WhsLocationViewSchema.Constants.WLV_MaxHeight,
				WhsLocationViewSchema.Constants.WLV_MaxDimensionUnit,
				WhsLocationViewSchema.Constants.WLV_PalletFloorSpaces,
				WhsLocationViewSchema.Constants.WLV_PalletStackHeight,
				WhsLocationViewSchema.Constants.WLV_PickMethod,
				WhsLocationViewSchema.Constants.WLV_PickPathSequence,
				WhsLocationViewSchema.Constants.WLV_PutawayPathSequence,
				WhsLocationViewSchema.Constants.WLV_RowName,
				WhsLocationViewSchema.Constants.WLV_RS_NKTransitServiceLevel,
				WhsLocationViewSchema.Constants.WLV_TransitDischargeAndServiceLevelFakeColumnForUX,
				WhsLocationViewSchema.Constants.WLV_TransitDischargeLRC, WhsLocationViewSchema.Constants.WLV_Tray,
				WhsLocationViewSchema.Constants.WLV_WA_PickingArea,
				WhsLocationViewSchema.Constants.WLV_WA_PutawayArea,
				WhsLocationViewSchema.Constants.WLV_PutawayAreaType,
				WhsLocationViewSchema.Constants.WLV_WLT_LocationType, WhsLocationViewSchema.Constants.WLV_WR,
				WhsLocationViewSchema.Constants.WLV_SQ_DefaultPrintQueue,
				WhsLocationViewSchema.Constants.WLV_WW_Whs, WhsLocationViewSchema.Constants.WLV_IsVirtualWarehouse,
				WhsLocationViewSchema.Constants.WLV_LocationClass, WhsLocationViewSchema.Constants.WLV_WarehouseType, WhsLocationViewSchema.Constants.WLV_LocationTypeCode,
				WhsLocationViewSchema.Constants.WLV_IsValidLocationForProductWarehousePutaway,
				WhsLocationViewSchema.Constants.WLV_SystemCreateTimeUtc, WhsLocationViewSchema.Constants.WLV_SystemCreateUser,
				WhsLocationViewSchema.Constants.WLV_LastConfigChangedUtc
			};

			Helper.CreateWarehouse("WH1", "A", 1, 1);
			Factory.Save();

			var sql = "select top 1 * from dbo.WhsLocationView_DoNotUse";
			var result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sql);
			var location = result.Single();
			CombineAssertions(() =>
			{
				AssertEquals("Number of columns", columnsThatAreNotUpdatedOften.Length, location.PropertyNames.Length);
				AssertContainsExactElementsInAnyOrder("Columns are wrong", columnsThatAreNotUpdatedOften,
					location.PropertyNames);
			});
		}

		#endregion

		#endregion

		#region TestWhsLocationView_FixedWidthLocation

		public void TestWhsLocationView_FixedWidthLocation_Column()
		{
			var whs1 = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row1 = Helper.CreateRow(whs1, "A", 100, 2, 2);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs1, row1, 100, 2, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A10000202", "A-100-002-02");
			AssertLocationStrings(
				GetLocation(whs1, row1, 10, 2, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A01000202", "A-010-002-02");
			AssertLocationStrings(
				GetLocation(whs1, row1, 1, 2, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A00100202", "A-001-002-02");

			whs1.WW_LocationComponentDelimiter = ".";
			Factory.Save();

			var whs2 = Helper.CreateFixedWidthLocationWarehouse("2", 2, 3, 2);
			var row2 = Helper.CreateRow(whs2, "A", 10, 2, 2);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs2, row2, 10, 2, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A1000202", "A-10-002-02");
			AssertLocationStrings(
				GetLocation(whs2, row2, 1, 2, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A0100202", "A-01-002-02");

			var whs3 = Helper.CreateFixedWidthLocationWarehouse("3", 1, 3, 2);
			var row3 = Helper.CreateRow(whs3, "A", 9, 2, 2);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs3, row3, 9, 2, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A900202", "A-9-002-02");
		}

		public void TestWhsLocationView_FixedWidthLocation_Level()
		{
			var whs1 = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row1 = Helper.CreateRow(whs1, "A", 2, 100, 2);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs1, row1, 2, 100, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A00210002", "A-002-100-02");
			AssertLocationStrings(
				GetLocation(whs1, row1, 2, 10, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A00201002", "A-002-010-02");
			AssertLocationStrings(
				GetLocation(whs1, row1, 2, 1, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A00200102", "A-002-001-02");

			whs1.WW_LocationComponentDelimiter = ".";
			Factory.Save();

			var whs2 = Helper.CreateFixedWidthLocationWarehouse("2", 3, 2, 2);
			var row2 = Helper.CreateRow(whs2, "A", 2, 10, 2);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs2, row2, 2, 10, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A0021002", "A-002-10-02");
			AssertLocationStrings(
				GetLocation(whs2, row2, 2, 1, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A0020102", "A-002-01-02");

			var whs3 = Helper.CreateFixedWidthLocationWarehouse("3", 3, 1, 2);
			var row3 = Helper.CreateRow(whs3, "A", 2, 9, 2);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs3, row3, 2, 9, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A002902", "A-002-9-02");
		}

		public void TestWhsLocationView_FixedWidthLocation_Tray()
		{
			var whs1 = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row1 = Helper.CreateRow(whs1, "A", 2, 2, 10);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs1, row1, 2, 2, 10,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A00200210", "A-002-002-10");
			AssertLocationStrings(
				GetLocation(whs1, row1, 2, 2, 1,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A00200201", "A-002-002-01");

			whs1.WW_LocationComponentDelimiter = ".";
			Factory.Save();

			var whs2 = Helper.CreateFixedWidthLocationWarehouse("2", 3, 3, 1);
			var row2 = Helper.CreateRow(whs2, "A", 2, 2, 9);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs2, row2, 2, 2, 9,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A0020029", "A-002-002-9");
		}

		public void TestWhsLocationView_FixedWidthLocation_AlphaColumn()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 1, 3, 2);
			whs.WW_LocationColumnsAlpha = true;
			var row = Helper.CreateRow(whs, "A", 26, 2, 2);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs, row, 1, 2, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "AA00202", "A-A-002-02");
			AssertLocationStrings(
				GetLocation(whs, row, 2, 2, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "AB00202", "A-B-002-02");
			AssertLocationStrings(
				GetLocation(whs, row, 3, 2, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "AC00202", "A-C-002-02");
		}

		public void TestWhsLocationView_FixedWidthLocation_AlphaLevel()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 3, 1, 2);
			whs.WW_LocationLevelsAlpha = true;
			var row = Helper.CreateRow(whs, "A", 2, 26, 2);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs, row, 2, 1, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A002A02", "A-002-A-02");
			AssertLocationStrings(
				GetLocation(whs, row, 2, 2, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A002B02", "A-002-B-02");
			AssertLocationStrings(
				GetLocation(whs, row, 2, 3, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A002C02", "A-002-C-02");
		}

		public void TestWhsLocationView_FixedWidthLocation_AlphaTrays()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 1);
			whs.WW_LocationTraysAlpha = true;
			var row = Helper.CreateRow(whs, "A", 2, 2, 26);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs, row, 2, 2, 1,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A002002A", "A-002-002-A");
			AssertLocationStrings(
				GetLocation(whs, row, 2, 2, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A002002B", "A-002-002-B");
			AssertLocationStrings(
				GetLocation(whs, row, 2, 2, 3,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A002002C", "A-002-002-C");
		}

		public void TestWhsLocationView_FixedWidthLocation_AllAlpha()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 1, 1, 1);
			whs.WW_LocationColumnsAlpha = true;
			whs.WW_LocationLevelsAlpha = true;
			whs.WW_LocationTraysAlpha = true;
			var row = Helper.CreateRow(whs, "A", 5, 5, 5);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs, row, 1, 2, 3,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "AABC", "A-A-B-C");
			AssertLocationStrings(
				GetLocation(whs, row, 2, 3, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "ABCB", "A-B-C-B");
			AssertLocationStrings(
				GetLocation(whs, row, 3, 4, 1,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "ACDA", "A-C-D-A");
		}

		public void TestWhsLocationView_FixedWidthLocation_SingleTray()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row = Helper.CreateRow(whs, "A", 9, 9, 1);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs, row, 2, 2, 1,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A002002", "A-002-002");
		}

		public void TestWhsLocationView_FixedWidthLocation_SingleLevelAndTray()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row = Helper.CreateRow(whs, "A", 9, 1, 1);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs, row, 2, 1, 1,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A002", "A-002");
		}

		public void TestWhsLocationView_FixedWidthLocation_SingleColumnLevelAndTray()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row = Helper.CreateRow(whs, "A", 1, 1, 1);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs, row, 1, 1, 1,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A", "A");
		}

		public void TestWhsLocationView_FixedWidthLocation_SingleColumn()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row = Helper.CreateRow(whs, "A", 1, 2, 2);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs, row, 1, 2, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A00100202", "A-001-002-02");
		}

		public void TestWhsLocationView_FixedWidthLocation_SingleLevel()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row = Helper.CreateRow(whs, "A", 2, 1, 2);
			Factory.Save();

			AssertLocationStrings(
				GetLocation(whs, row, 2, 1, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A00200102", "A-002-001-02");
		}

		public void TestWhsLocationView_NotFixedWidthLocation_UserFriendlyLocationString()
		{
			var whs = Helper.CreateWarehouse("1");
			var row = Helper.CreateRow(whs, "A", 2, 2, 2);
			Factory.Save();

			AssertEquals("Precondition", (byte)0, whs.WW_LocationColumnsFixedWidth);
			AssertEquals("Precondition", (byte)0, whs.WW_LocationLevelsFixedWidth);
			AssertEquals("Precondition", (byte)0, whs.WW_LocationTraysFixedWidth);
			AssertLocationStrings(
				GetLocation(whs, row, 2, 2, 2,
					new[]
					{
						WhsLocationViewSchema.Constants.WLV_LocationString,
						WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly
					}), "A-2-2-2", "A-2-2-2");
		}

		void AssertLocationStrings(DynamicBusinessObject whsLocation, string expectedLocationString,
			string expectedLocationStringUserFriendly)
		{
			AssertEquals("Location string is correct.", expectedLocationString,
				whsLocation[WhsLocationViewSchema.Constants.WLV_LocationString]);
			AssertEquals("Location string user friendly is correct.", expectedLocationStringUserFriendly,
				whsLocation[WhsLocationViewSchema.Constants.WLV_LocationString_UserFriendly]);
		}

		#endregion

		#region TestWhsLocationView_LocationComponents

		public void TestWhsLocationView_LocationComponents_Column()
		{
			var whs = Helper.CreateWarehouse("1");
			var row1 = Helper.CreateRow(whs, "A", 100, 1, 1);
			Factory.Save();

			AssertEquals("100",
				GetLocationValue<ZString>(whs, row1, 100, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));
			AssertEquals("10",
				GetLocationValue<ZString>(whs, row1, 10, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));
			AssertEquals("1",
				GetLocationValue<ZString>(whs, row1, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));

			whs.WW_LocationsHaveLeadingZeros = true;
			Factory.Save();

			AssertEquals("100",
				GetLocationValue<ZString>(whs, row1, 100, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));
			AssertEquals("010",
				GetLocationValue<ZString>(whs, row1, 10, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));
			AssertEquals("001",
				GetLocationValue<ZString>(whs, row1, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));

			var row2 = Helper.CreateRow(whs, "B", 99, 2, 2);
			Factory.Save();

			AssertEquals("99",
				GetLocationValue<ZString>(whs, row2, 99, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));
			AssertEquals("09",
				GetLocationValue<ZString>(whs, row2, 9, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));
		}

		public void TestWhsLocationView_LocationComponents_FixedWidth_Column()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row1 = Helper.CreateRow(whs, "A", 100, 1, 1);
			Factory.Save();

			AssertEquals("100",
				GetLocationValue<ZString>(whs, row1, 100, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));
			AssertEquals("010",
				GetLocationValue<ZString>(whs, row1, 10, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));
			AssertEquals("001",
				GetLocationValue<ZString>(whs, row1, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));

			var row2 = Helper.CreateRow(whs, "B", 99, 2, 2);
			Factory.Save();

			AssertEquals("099",
				GetLocationValue<ZString>(whs, row2, 99, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));
			AssertEquals("009",
				GetLocationValue<ZString>(whs, row2, 9, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));
		}

		public void TestWhsLocationView_LocationComponents_Level()
		{
			var whs = Helper.CreateWarehouse("1");
			var row1 = Helper.CreateRow(whs, "A", 1, 100, 1);
			Factory.Save();

			AssertEquals("100",
				GetLocationValue<ZString>(whs, row1, 1, 100, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));
			AssertEquals("10",
				GetLocationValue<ZString>(whs, row1, 1, 10, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));
			AssertEquals("1",
				GetLocationValue<ZString>(whs, row1, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));

			whs.WW_LocationsHaveLeadingZeros = true;
			Factory.Save();

			AssertEquals("100",
				GetLocationValue<ZString>(whs, row1, 1, 100, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));
			AssertEquals("010",
				GetLocationValue<ZString>(whs, row1, 1, 10, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));
			AssertEquals("001",
				GetLocationValue<ZString>(whs, row1, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));

			var row2 = Helper.CreateRow(whs, "B", 1, 99, 1);
			Factory.Save();

			AssertEquals("99",
				GetLocationValue<ZString>(whs, row2, 1, 99, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));
			AssertEquals("09",
				GetLocationValue<ZString>(whs, row2, 1, 9, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));
		}

		public void TestWhsLocationView_LocationComponents_FixedWidth_Level()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row1 = Helper.CreateRow(whs, "A", 1, 100, 1);
			Factory.Save();

			AssertEquals("100",
				GetLocationValue<ZString>(whs, row1, 1, 100, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));
			AssertEquals("010",
				GetLocationValue<ZString>(whs, row1, 1, 10, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));
			AssertEquals("001",
				GetLocationValue<ZString>(whs, row1, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));

			var row2 = Helper.CreateRow(whs, "B", 1, 99, 1);
			Factory.Save();

			AssertEquals("099",
				GetLocationValue<ZString>(whs, row2, 1, 99, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));
			AssertEquals("009",
				GetLocationValue<ZString>(whs, row2, 1, 9, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));
		}

		public void TestWhsLocationView_LocationComponents_Tray()
		{
			var whs = Helper.CreateWarehouse("1");
			var row1 = Helper.CreateRow(whs, "A", 1, 1, 10);
			Factory.Save();

			AssertEquals("10",
				GetLocationValue<ZString>(whs, row1, 1, 1, 10, WhsLocationViewSchema.Constants.WLV_FormattedTray));
			AssertEquals("1",
				GetLocationValue<ZString>(whs, row1, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedTray));

			whs.WW_LocationsHaveLeadingZeros = true;
			Factory.Save();

			AssertEquals("10",
				GetLocationValue<ZString>(whs, row1, 1, 1, 10, WhsLocationViewSchema.Constants.WLV_FormattedTray));
			AssertEquals("01",
				GetLocationValue<ZString>(whs, row1, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedTray));

			var row2 = Helper.CreateRow(whs, "B", 1, 1, 9);
			Factory.Save();

			AssertEquals("9",
				GetLocationValue<ZString>(whs, row2, 1, 1, 9, WhsLocationViewSchema.Constants.WLV_FormattedTray));
		}

		public void TestWhsLocationView_LocationComponents_FixedWidth_Tray()
		{
			var whs = Helper.CreateFixedWidthLocationWarehouse("1", 3, 3, 2);
			var row1 = Helper.CreateRow(whs, "A", 1, 1, 10);
			Factory.Save();

			AssertEquals("10",
				GetLocationValue<ZString>(whs, row1, 1, 1, 10, WhsLocationViewSchema.Constants.WLV_FormattedTray));
			AssertEquals("01",
				GetLocationValue<ZString>(whs, row1, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedTray));

			var row2 = Helper.CreateRow(whs, "B", 1, 1, 9);
			Factory.Save();

			AssertEquals("09",
				GetLocationValue<ZString>(whs, row2, 1, 1, 9, WhsLocationViewSchema.Constants.WLV_FormattedTray));
		}

		public void TestWhsLocationView_LocationComponents_AlphaComponents()
		{
			var whs = Helper.CreateWarehouse("1");
			whs.WW_LocationColumnsAlpha = true;
			whs.WW_LocationLevelsAlpha = true;
			whs.WW_LocationTraysAlpha = true;
			var row = Helper.CreateRow(whs, "A", 5, 5, 5);
			Factory.Save();

			AssertEquals("A",
				GetLocationValue<ZString>(whs, row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_FormattedColumn));
			AssertEquals("B",
				GetLocationValue<ZString>(whs, row, 1, 2, 1, WhsLocationViewSchema.Constants.WLV_FormattedLevel));
			AssertEquals("C",
				GetLocationValue<ZString>(whs, row, 1, 1, 3, WhsLocationViewSchema.Constants.WLV_FormattedTray));
		}

		#endregion

		#region TestView_WhsLocationView_LastAllocatedOrChangedID

		public void TestView_WhsLocationView_LastAllocatedOrChangedID()
		{
			var whs = Helper.CreateWarehouse("1");
			var row1 = Helper.CreateRow(whs, "A", 1, 1);
			Factory.Save();

			var location = GetLocation(whs, row1, 1, 1, 1, new string[] { WhsLocationViewSchema.Constants.WLV_LastAllocatedOrChangedID });
			AssertEquals("Default LastAllocatedOrChangedID should be empty", ZGuid.Empty, location[WhsLocationViewSchema.Constants.WLV_LastAllocatedOrChangedID]);
		}

		#endregion

		#region Implementation

		ZString GetLocationString(WhsWarehouse whs, WhsRow row, short column, short level, short tray)
		{
			return GetLocationValue<ZString>(whs, row, column, level, tray, "WLV_LocationString");
		}

		DynamicBusinessObject GetLocation(WhsWarehouse whs, WhsRow row, short column, short level, short tray,
			IEnumerable<string> columnNames)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var columns = new ZStringBuilder(columnNames);
			var sql =
				$"select {columns.ToStringWithDelimiterBetweenAppends(",")} from dbo.WhsLocationView Where WLV_WW_Whs = '{whs.PK}' And WLV_RowName = '{row.WR_Name}' And WLV_Column = {column} And WLV_Level = {level} And WLV_Tray = {tray}";

			AssertNoExceptionThrown(() => result.Load(sql));
			return result.Single();
		}

		T GetLocationValue<T>(WhsWarehouse whs, WhsRow row, short column, short level, short tray, string columnName)
			where T : IZType
		{
			var resultTopRow = GetLocation(whs, row, column, level, tray, new[] { columnName });
			return (T)resultTopRow[columnName];
		}

		#endregion
	}
}
