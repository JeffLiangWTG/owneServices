using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Environment.DataTransfer.Testing
{
	public class SysMergeWarehouseValueObjectDataAdapterTest : WhsTestCaseWithFactoryEnv
	{
		#region TestExport

		#region TestExport_WhsWarehouse

		public void TestExport_WhsWarehouse()
		{
			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery());
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var locationType = Helper.CreateLocationType("AAA");
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "TEST WAREHOUSE";
			warehouse.WW_AutoPrintOrderCopyForMOPOnPick = true;
			warehouse.WW_AutoPrintOrderSummaryOnPick = true;
			warehouse.WW_AutoPrintPackingSlip = true;
			warehouse.WW_AutoPrintPickingNonPickedItems = true;
			warehouse.WW_AutoPrintPickingShortfallItems = false;
			warehouse.WW_AutoPrintPickingSlip = false;
			warehouse.WW_GB_RelatedCompanyBranch = branch.PK;
			warehouse.WW_IsActive = true;
			warehouse.WW_IsBondedWarehouse = false;
			warehouse.WW_IsVirtualWarehouse = false;
			warehouse.WW_LocationColumnsAlpha = false;
			warehouse.WW_LocationColumnsZeroBased = false;
			warehouse.WW_LocationColumnsFixedWidth = 3;
			warehouse.WW_LocationComponentDelimiter = "-";
			warehouse.WW_LocationLevelsAlpha = true;
			warehouse.WW_LocationLevelsZeroBased = false;
			warehouse.WW_LocationLevelsFixedWidth = 2;
			warehouse.WW_LocationsHaveLeadingZeros = true;
			warehouse.WW_LocationTraysAlpha = false;
			warehouse.WW_LocationTraysZeroBased = true;
			warehouse.WW_LocationTraysFixedWidth = 1;
			warehouse.WW_WLT_DefaultLocationType = locationType.PK;
			warehouse.WW_OA_WarehouseAddress = org.Addresses[0].PK;
			WarehouseDataRegistry.Instance.PackingSlipTitles.SetValue(Guid.Empty, warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, "Packing Slip Test Title");
			warehouse.WW_UseArrivalDateForInwardsFinalisedDate = false;
			warehouse.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1, 1);
			var location = row.Locations[0];
			warehouse.WW_DefaultOutboundDockDoor = location.PK;

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(warehouse, new ValueObjectExportContext(Notify));
			AssertEquals("Warehouse.PK", warehouse.PK.ToString(), xsdWarehouse.PK, xsdWarehouse.PKSpecified);
			AssertEquals("Warehouse.WarehouseCode", "WHS", xsdWarehouse.WarehouseCode, xsdWarehouse.WarehouseCodeSpecified);
			AssertEquals("Warehouse.WarehouseName", "TEST WAREHOUSE", xsdWarehouse.WarehouseName, xsdWarehouse.WarehouseNameSpecified);
			AssertEquals("Warehouse.AutoPrintOrderCopyForMOPOnPick", true, xsdWarehouse.AutoPrintOrderCopyForMOPOnPick, xsdWarehouse.AutoPrintOrderCopyForMOPOnPickSpecified);
			AssertEquals("Warehouse.AutoPrintOrderSummaryOnPick", true, xsdWarehouse.AutoPrintOrderSummaryOnPick, xsdWarehouse.AutoPrintOrderSummaryOnPickSpecified);
			AssertEquals("Warehouse.AutoPrintPackingSlip", true, xsdWarehouse.AutoPrintPackingSlip, xsdWarehouse.AutoPrintPackingSlipSpecified);
			AssertEquals("Warehouse.AutoPrintPickingNonPickedItems", true, xsdWarehouse.AutoPrintPickingNonPickedItems, xsdWarehouse.AutoPrintPickingNonPickedItemsSpecified);
			AssertEquals("Warehouse.AutoPrintPickingShortfallItems", false, xsdWarehouse.AutoPrintPickingShortfallItems, xsdWarehouse.AutoPrintPickingShortfallItemsSpecified);
			AssertEquals("Warehouse.AutoPrintPickingSlip", false, xsdWarehouse.AutoPrintPickingSlip, xsdWarehouse.AutoPrintPickingSlipSpecified);
			AssertEquals("Warehouse.IsActive", true, xsdWarehouse.IsActive, xsdWarehouse.IsActiveSpecified);
			AssertEquals("Warehouse.IsBondedWarehouse", false, xsdWarehouse.IsBondedWarehouse, xsdWarehouse.IsBondedWarehouseSpecified);
			AssertEquals("Warehouse.IsVirtualWarehouse", false, xsdWarehouse.IsVirtualWarehouse, xsdWarehouse.IsVirtualWarehouseSpecified);
			AssertEquals("Warehouse.LocaitonTrayAlpha", false, xsdWarehouse.LocaitonTraysAlpha, xsdWarehouse.LocaitonTraysAlphaSpecified);
			AssertEquals("Warehouse.LocationColumnsAlpha", false, xsdWarehouse.LocationColumnsAlpha, xsdWarehouse.LocationColumnsAlphaSpecified);
			AssertEquals("Warehouse.LocationColumnsZeroBased", false, xsdWarehouse.LocationColumnsZeroBased, xsdWarehouse.LocationColumnsZeroBasedSpecified);
			AssertEquals("Warehouse.LocationComponentDelimiter", "-", xsdWarehouse.LocationComponentDelimiter, xsdWarehouse.LocationComponentDelimiterSpecified);
			AssertEquals("Warehouse.LocationLevelAlpha", true, xsdWarehouse.LocationLevelsAlpha, xsdWarehouse.LocationLevelsAlphaSpecified);
			AssertEquals("Warehouse.LocationLevelsZeroBased", false, xsdWarehouse.LocationLevelsZeroBased, xsdWarehouse.LocationLevelsZeroBasedSpecified);
			AssertEquals("Warehouse.LocationsHaveLeadingZeros", true, xsdWarehouse.LocationsHaveLeadingZeros, xsdWarehouse.LocationsHaveLeadingZerosSpecified);
			AssertEquals("Warehouse.LocationTraysZeroBased", true, xsdWarehouse.LocationTraysZeroBased, xsdWarehouse.LocationTraysZeroBasedSpecified);
			AssertEquals("Warehouse.PackingSlipTitle", "Packing Slip Test Title", xsdWarehouse.PackingSlipTitle, xsdWarehouse.PackingSlipTitleSpecified);
			AssertEquals("Warehouse.RelatedCompanyBranchCode", branch.GB_Code, xsdWarehouse.RelatedCompanyBranchCode, xsdWarehouse.RelatedCompanyBranchCodeSpecified);
			AssertEquals("Warehouse.UseArrivalDateForInwardsFinalisedDate", false, xsdWarehouse.UseArrivalDateForInwardsFinalisedDate, xsdWarehouse.UseArrivalDateForInwardsFinalisedDateSpecified);
			AssertEquals("Warehouse.UseRequiredDateForOutwardsFinalisedDate", true, xsdWarehouse.UseRequiredDateForOutwardsFinalisedDate, xsdWarehouse.UseRequiredDateForOutwardsFinalisedDateSpecified);
			AssertEquals("Warehouse.WarehouseAddress", warehouse.WW_OA_WarehouseAddress.ToString(), xsdWarehouse.WarehouseAddressPK, xsdWarehouse.WarehouseAddressPKSpecified);
			AssertEquals("Warehouse.DefaultLocationType", locationType.WLT_Code, xsdWarehouse.DefaultLocationType, xsdWarehouse.DefaultLocationTypeSpecified);
			AssertEquals("Warehouse.DefaultLocationType", location.PK.ToString(), xsdWarehouse.DefaultOutboundDockDoorPK);
			AssertEquals("Warehouse.LocationColumnsFixedWidth", warehouse.WW_LocationColumnsFixedWidth, xsdWarehouse.LocationColumnsFixedWidth);
			AssertEquals("Warehouse.LocationLevelsFixedWidth", warehouse.WW_LocationLevelsFixedWidth, xsdWarehouse.LocationLevelsFixedWidth);
			AssertEquals("Warehouse.LocationTraysFixedWidth", warehouse.WW_LocationTraysFixedWidth, xsdWarehouse.LocationTraysFixedWidth);
		}

		#endregion

		#region TestExport_WhsRow

		public void TestExport_WhsRow()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			var rowA = Helper.CreateRow(whs, "A", 4, 3, 2);
			var rowB = Helper.CreateRow(whs, "BULK", 1, 1, 1);

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(whs, new ValueObjectExportContext(Notify));
			AssertEquals("WarehouseXML should contain 3 rows.", 3, xsdWarehouse.Rows.Count);
			AssertContainsRow(xsdWarehouse.Rows, rowA);
			AssertContainsRow(xsdWarehouse.Rows, rowB);
			AssertContainsRow(xsdWarehouse.Rows, whs.DefaultOutboundDockDoorLocation.Row);
		}

		void AssertContainsRow(SystemMergeRowCollection xsdRows, WhsRow expectedRow)
		{
			var xsdRow = xsdRows.Cast<SystemMergeRow>().Single(r => r.PK == expectedRow.PK.ToString());
			AssertEquals("Row.Name", expectedRow.WR_Name, xsdRow.Name, xsdRow.NameSpecified);
			AssertEquals("Row.Columns", expectedRow.WR_Columns, xsdRow.Columns, xsdRow.ColumnsSpecified);
			AssertEquals("Row.Levels", expectedRow.WR_Levels, xsdRow.Levels, xsdRow.LevelsSpecified);
			AssertEquals("Row.Trays", expectedRow.WR_Trays, xsdRow.Trays, xsdRow.TraysSpecified);
		}

		#endregion

		#region TestExport_WhsRow_WhsLocations

		public void TestExport_WhsRow_WhsLocations()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			var area = Helper.CreateArea(whs, "STAGING", AreaTypes.Codes.Excise);
			var locations = rowA.Locations;
			var locationType = Helper.CreateLocationType("DAM");
			locations[0].WLV_ApprovedKnownLocation = "TSA";
			Helper.SetLocationMaxWeightAndVolume(locations[1], 12m, Constants.Weight.Tonnes, 4.5m, Constants.Volume.CubicMetres);
			locations[2].WLV_MaxDepth = 2m;
			locations[3].WLV_MaxHeight = 1m;
			locations[4].WLV_MaxWidth = 2m;
			locations[4].WLV_MaxDimensionUnit = "FT";
			locations[5].WLV_LocationStatus = LocationStatus.Codes.Void;
			locations[6].WLV_WLT_LocationType = locationType.PK;
			locations[7].WLV_PalletFloorSpaces = 2;
			locations[7].WLV_PalletStackHeight = 3;
			locations[7].WLV_PickMethod = "TST";
			locations[7].WLV_WA_PickingArea = area.PK;
			locations[7].WLV_WA_PutawayArea = area.PK;
			locations[7].WLV_MaxDimensionUnit = Constants.Length.Metres;

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(whs, new ValueObjectExportContext(Notify));
			AssertEquals("WarehouseXML should contain 2 Rows.", 2, xsdWarehouse.Rows.Count);
			foreach (var row in whs.Rows)
			{
				var xsdRow = xsdWarehouse.Rows.Cast<SystemMergeRow>().Single(r => r.Locations.Count == row.Locations.Count);
				foreach (var location in row.Locations)
				{
					AssertContainsLocation(xsdRow.Locations, location);
				}
			}
		}

		void AssertContainsLocation(SystemMergeLocationCollection locationsXML, WhsLocation expectedLocation)
		{
			var locationXML = locationsXML.Cast<SystemMergeLocation>().Single(l => l.PK == expectedLocation.PK.ToString());
			AssertNotNull($"Row {expectedLocation.ToLocationString()} could not be found.", locationXML);
			AssertEquals("Location.PK", expectedLocation.PK.ToString(), locationXML.PK, locationXML.PKSpecified);
			AssertEquals("Location.ApprovedKnownLocation", expectedLocation.WLV_ApprovedKnownLocation, locationXML.ApprovedKnownLocation, locationXML.ApprovedKnownLocationSpecified);
			AssertEquals("Location.Column", expectedLocation.WLV_Column, locationXML.Column, locationXML.ColumnSpecified);
			AssertEquals("Location.Level", expectedLocation.WLV_Level, locationXML.Level, locationXML.LevelSpecified);
			AssertEquals("Location.Tray", expectedLocation.WLV_Tray, locationXML.Tray, locationXML.TraySpecified);
			AssertEquals("Location.MaxDepth", expectedLocation.WLV_MaxDepth, locationXML.MaxDepth, locationXML.MaxDepthSpecified);
			AssertEquals("Location.MaxHeight", expectedLocation.WLV_MaxHeight, locationXML.MaxHeight, locationXML.MaxHeightSpecified);
			AssertEquals("Location.MaxWidth", expectedLocation.WLV_MaxWidth, locationXML.MaxWidth, locationXML.MaxWidthSpecified);
			AssertEquals("Location.MaxDimensionUQ", expectedLocation.WLV_MaxDimensionUnit, locationXML.MaxDimensionUQ, locationXML.MaxDimensionUQSpecified);
			AssertEquals("Location.LocationStatus", expectedLocation.WLV_LocationStatus, locationXML.LocationStatus, locationXML.LocationStatusSpecified);
			AssertEquals("Location.LocationType", expectedLocation.LocationType.WLT_Code, locationXML.LocationType, locationXML.LocationTypeSpecified);
			AssertEquals("Location.MaxVolume", expectedLocation.WLV_MaxCubic, locationXML.MaxVolume, locationXML.MaxVolumeSpecified);
			AssertEquals("Location.MaxVolumeUQ", expectedLocation.WLV_MaxCubicUnit, locationXML.MaxVolumeUQ, locationXML.MaxVolumeUQSpecified);
			AssertEquals("Location.MaxWeight", expectedLocation.WLV_MaxWeight, locationXML.MaxWeight, locationXML.MaxWeightSpecified);
			AssertEquals("Location.MaxWeightUQ", expectedLocation.WLV_MaxWeightUnit, locationXML.MaxWeightUQ, locationXML.MaxWeightUQSpecified);
			AssertEquals("Location.PickMethod", expectedLocation.WLV_PickMethod, locationXML.PickMethod, locationXML.PickMethodSpecified);
			AssertEquals("Location.PalletFloorSpaces", expectedLocation.WLV_PalletFloorSpaces, locationXML.PalletFloorSpaces, locationXML.PalletFloorSpacesSpecified);
			AssertEquals("Location.PalletStackHeight", expectedLocation.WLV_PalletStackHeight, locationXML.PalletStackHeight, locationXML.PalletStackHeightSpecified);
			AssertEquals("Location.PickingArea", expectedLocation.WLV_WA_PickingArea.ToString(), locationXML.PickingAreaPK, locationXML.PickingAreaPKSpecified);
			AssertEquals("Location.PutawayArea", expectedLocation.WLV_WA_PutawayArea.ToString(), locationXML.PutawayAreaPK, locationXML.PutawayAreaPKSpecified);
		}

		#endregion

		#region TestExport_WhsArea

		public void TestExport_WhsArea()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			whs.Areas.DeleteAll(); // remove Default Area
			var stagingArea = Helper.CreateArea(whs, "STAGING", AreaTypes.Codes.Bonded);
			var bulkArea = Helper.CreateArea(whs, "BULK", AreaTypes.Codes.FreeStore);

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(whs, new ValueObjectExportContext(Notify));
			AssertEquals("WarehouseXML should contain 2 areas.", 2, xsdWarehouse.Areas.Count);
			AssertContainsArea(xsdWarehouse, stagingArea);
			AssertContainsArea(xsdWarehouse, bulkArea);
		}

		void AssertContainsArea(SystemMergeWarehouse xsdWarehouse, WhsArea expectedArea)
		{
			var areaXML = xsdWarehouse.Areas.Cast<SystemMergeArea>().Single(a => a.PK == expectedArea.PK.ToString());
			AssertNotNull(string.Format("Area {0} could not be found.", expectedArea.WA_Name), areaXML);
			AssertEquals("Area.PK", expectedArea.PK.ToString(), areaXML.PK, areaXML.PKSpecified);
			AssertEquals("Area.Name", expectedArea.WA_Name, areaXML.Name, areaXML.NameSpecified);
			AssertEquals("Area.AreaType", expectedArea.WA_AreaType, areaXML.AreaType, areaXML.AreaTypeSpecified);
		}

		#endregion

		#region Asserts

		void AssertEquals(string message, ZString expectedValue, ZString actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", !expectedValue.IsEmpty, actualValueSpecified);
		}

		void AssertEquals(string message, ZBool expectedValue, ZBool actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", expectedValue, actualValueSpecified);
		}

		void AssertEquals(string message, ZShort expectedValue, ZShort actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", true, actualValueSpecified);
		}

		void AssertEquals(string message, ZDecimal expectedValue, ZDecimal actualValue, bool actualValueSpecified)
		{
			AssertEquals(message, expectedValue, actualValue);
			AssertEquals(message + " specified", true, actualValueSpecified);
		}

		#endregion

		#endregion

		#region TestImport

		#region TestImport_WhsWarehouse

		public void TestImport_WhsWarehouse()
		{
			var branch = Factory.LoadTop1<GlbBranch>(new ZQuery());
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var locationType = Helper.CreateLocationType("AAA");
			Factory.Save();
			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "TEST WAREHOUSE";
			warehouse.WW_AutoPrintOrderCopyForMOPOnPick = true;
			warehouse.WW_AutoPrintOrderSummaryOnPick = true;
			warehouse.WW_AutoPrintPackingSlip = true;
			warehouse.WW_AutoPrintPickingNonPickedItems = true;
			warehouse.WW_AutoPrintPickingShortfallItems = false;
			warehouse.WW_AutoPrintPickingSlip = false;
			warehouse.WW_GB_RelatedCompanyBranch = branch.PK;
			warehouse.WW_IsActive = true;
			warehouse.WW_IsBondedWarehouse = false;
			warehouse.WW_IsVirtualWarehouse = false;
			warehouse.WW_LocationColumnsAlpha = false;
			warehouse.WW_LocationColumnsZeroBased = false;
			warehouse.WW_LocationColumnsFixedWidth = 3;
			warehouse.WW_LocationComponentDelimiter = "-";
			warehouse.WW_LocationLevelsAlpha = true;
			warehouse.WW_LocationLevelsZeroBased = false;
			warehouse.WW_LocationLevelsFixedWidth = 2;
			warehouse.WW_LocationsHaveLeadingZeros = true;
			warehouse.WW_LocationTraysAlpha = false;
			warehouse.WW_LocationTraysZeroBased = true;
			warehouse.WW_LocationTraysFixedWidth = 1;
			warehouse.WW_OA_WarehouseAddress = org.Addresses[0].PK;
			warehouse.WW_WLT_DefaultLocationType = locationType.PK;
			WarehouseDataRegistry.Instance.PackingSlipTitles.SetValue(Guid.Empty, warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, "Packing Slip Test Title");
			warehouse.WW_UseArrivalDateForInwardsFinalisedDate = false;
			warehouse.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(warehouse, new ValueObjectExportContext(Notify));

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseValueObjectDataAdapter();
			var importedWarehouseInOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdWarehouse, contextInOtherFactory);
			AssertEquals("warehouseXML.PK", warehouse.PK, importedWarehouseInOtherFactory.PK);
			AssertEquals("warehouseXML.WW_WarehouseCode", "WHS", importedWarehouseInOtherFactory.WW_WarehouseCode);
			AssertEquals("warehouseXML.WW_WarehouseName", "TEST WAREHOUSE", importedWarehouseInOtherFactory.WW_WarehouseName);
			AssertEquals("warehouseXML.WW_AutoPrintOrderCopyForMOPOnPick", true, importedWarehouseInOtherFactory.WW_AutoPrintOrderCopyForMOPOnPick);
			AssertEquals("warehouseXML.WW_AutoPrintOrderSummaryOnPick", true, importedWarehouseInOtherFactory.WW_AutoPrintOrderSummaryOnPick);
			AssertEquals("warehouseXML.WW_AutoPrintPackingSlip", true, importedWarehouseInOtherFactory.WW_AutoPrintPackingSlip);
			AssertEquals("warehouseXML.WW_AutoPrintPickingNonPickedItems", true, importedWarehouseInOtherFactory.WW_AutoPrintPickingNonPickedItems);
			AssertEquals("warehouseXML.WW_AutoPrintPickingShortfallItems", false, importedWarehouseInOtherFactory.WW_AutoPrintPickingShortfallItems);
			AssertEquals("warehouseXML.WW_AutoPrintPickingSlip", false, importedWarehouseInOtherFactory.WW_AutoPrintPickingSlip);
			AssertEquals("warehouseXML.WW_GB_RelatedCompanyBranch", branch.GB_Code, importedWarehouseInOtherFactory.RelatedCompanyBranch.GB_Code);
			AssertEquals("warehouseXML.WW_IsActive", true, importedWarehouseInOtherFactory.WW_IsActive);
			AssertEquals("warehouseXML.WW_IsBondedWarehouse", false, importedWarehouseInOtherFactory.WW_IsBondedWarehouse);
			AssertEquals("warehouseXML.WW_IsVirtualWarehouse", false, importedWarehouseInOtherFactory.WW_IsVirtualWarehouse);
			AssertEquals("warehouseXML.WW_LocationColumnsAlpha", false, importedWarehouseInOtherFactory.WW_LocationColumnsAlpha);
			AssertEquals("warehouseXML.WW_LocationColumnsZeroBased", false, importedWarehouseInOtherFactory.WW_LocationColumnsZeroBased);
			AssertEquals("warehouseXML.WW_LocationComponentDelimiter", "-", importedWarehouseInOtherFactory.WW_LocationComponentDelimiter);
			AssertEquals("warehouseXML.WW_LocationLevelsAlpha", true, importedWarehouseInOtherFactory.WW_LocationLevelsAlpha);
			AssertEquals("warehouseXML.WW_LocationLevelsZeroBased", false, importedWarehouseInOtherFactory.WW_LocationLevelsZeroBased);
			AssertEquals("warehouseXML.WW_LocationsHaveLeadingZeros", true, importedWarehouseInOtherFactory.WW_LocationsHaveLeadingZeros);
			AssertEquals("warehouseXML.WW_LocationTraysAlpha", false, importedWarehouseInOtherFactory.WW_LocationTraysAlpha);
			AssertEquals("warehouseXML.WW_LocationTraysZeroBased", true, importedWarehouseInOtherFactory.WW_LocationTraysZeroBased);
			AssertEquals("warehouseXML.WW_OA_WarehouseAddress", org.Addresses[0].PK, importedWarehouseInOtherFactory.WW_OA_WarehouseAddress);
			AssertEquals("warehouseXML.WW_PackingSlipTitle", "Packing Slip Test Title", importedWarehouseInOtherFactory.PackingSlipTitle);
			AssertEquals("warehouseXML.WW_UseArrivalDateForInwardsFinalisedDate", false, importedWarehouseInOtherFactory.WW_UseArrivalDateForInwardsFinalisedDate);
			AssertEquals("warehouseXML.WW_UseRequiredDateForOutwardsFinalisedDate", true, importedWarehouseInOtherFactory.WW_UseRequiredDateForOutwardsFinalisedDate);
			AssertEquals("warehouseXML.WW_WLT_DefaultLocationType", locationType.PK, importedWarehouseInOtherFactory.WW_WLT_DefaultLocationType);
			AssertEquals("warehouseXML.WW_LocationColumnsFixedWidth", (ZByte)3, importedWarehouseInOtherFactory.WW_LocationColumnsFixedWidth);
			AssertEquals("warehouseXML.WW_LocationLevelsFixedWidth", (ZByte)2, importedWarehouseInOtherFactory.WW_LocationLevelsFixedWidth);
			AssertEquals("warehouseXML.WW_LocationTraysFixedWidth", (ZByte)1, importedWarehouseInOtherFactory.WW_LocationTraysFixedWidth);
		}

		#endregion

		#region TestImport_WhsWarehouse_WrongBranch

		public void TestImport_WhsWarehouse_WrongBranch()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "ZZZ";

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_GB_RelatedCompanyBranch = branch.PK;
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "TEST WAREHOUSE";

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(warehouse, new ValueObjectExportContext(Notify));

			var otherFactory = new BusinessObjectFactory();
			otherFactory.ImportFromAnotherFactory(warehouse.WarehouseAddress);
			var expectedErrorMessage = string.Format("Warehouse: [({0}) - {1} - {2}]: branch skipped. Reason: branch [{3}] doesn't exist.",
				warehouse.PK, warehouse.WW_WarehouseCode, warehouse.WW_WarehouseName, warehouse.RelatedCompanyBranch.GB_Code);

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseValueObjectDataAdapter();
			var importedWarehouseInOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdWarehouse, contextInOtherFactory);
			AssertEquals("If branch cannot be found, it should be left blank on import.", ZGuid.Empty, importedWarehouseInOtherFactory.WW_GB_RelatedCompanyBranch);
			AssertEquals("Branch is not in the factory, so it should not be set for warehouse.", 1, notificationBuffer.Events.Length);
			AssertEquals("User should be notified that branch was not imported.", "Information", notificationBuffer.Events[0].Type.EnumValueName);
			AssertEquals("User should be notified that branch was not imported.", expectedErrorMessage, notificationBuffer.Events[0].Message);
		}

		#endregion

		#region TestImport_WhsWarehouse_BranchWithSameCodeExist

		public void TestImport_WhsWarehouse_BranchWithSameCodeExist()
		{
			var existingBranch = Factory.LoadTop1<GlbBranch>(new ZQuery());
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_Code = existingBranch.GB_Code;

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_GB_RelatedCompanyBranch = newBranch.PK;
			warehouse.WW_WarehouseCode = "WHS";
			warehouse.WW_WarehouseName = "TEST WAREHOUSE";

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(warehouse, new ValueObjectExportContext(Notify));

			var otherFactory = new BusinessObjectFactory();
			otherFactory.ImportFromAnotherFactory(warehouse.WarehouseAddress);

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseValueObjectDataAdapter();
			var importedWarehouseInOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdWarehouse, contextInOtherFactory);
			AssertEquals("If branch exist in dest DB, it should be matched via Code even if it is a different branch PK.", existingBranch.PK, importedWarehouseInOtherFactory.WW_GB_RelatedCompanyBranch);
		}

		#endregion

		#region TestImport_WhsWarehouse_WrongAddress

		public void TestImport_WhsWarehouse_WrongAddress()
		{
			var org = Helper.CreateClient();
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = org.MainAddress.PK;

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(warehouse, new ValueObjectExportContext(Notify));

			var otherFactory = new BusinessObjectFactory();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseValueObjectDataAdapter();
			var expectedErrorMessage = string.Format("Warehouse: [({0}) - {1} - {2}]\r\nCould not find Address with PK = ({3}).\r\nPlease import it first and then retry the import operation.\r\n",
				warehouse.PK, warehouse.WW_WarehouseCode, warehouse.WW_WarehouseName, warehouse.WW_OA_WarehouseAddress);

			AssertExceptionThrown(
				"Should not import warehouse if its address is not in DB.",
				typeof(ArgumentException),
				expectedErrorMessage,
				() => dataAdapter.CreateOrUpdateFromValueObject(xsdWarehouse, contextInOtherFactory));

			var warehouseInOtherFactory = otherFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertNull("If warehouse had critical error, then it should not had been saved to DB.", warehouseInOtherFactory);
		}

		#endregion

		#region TestImport_WhsWarehouse_WrongLocationType

		public void TestImport_WhsWarehouse_WrongLocationType()
		{
			var locationType = Helper.CreateLocationType("222");
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WLT_DefaultLocationType = locationType.PK;

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(warehouse, new ValueObjectExportContext(Notify));

			var otherFactory = new BusinessObjectFactory();
			otherFactory.ImportFromAnotherFactory(warehouse.WarehouseAddress);

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseValueObjectDataAdapter();
			var expectedErrorMessage = string.Format("Warehouse: [({0}) - {1} - {2}]\r\nCould not find Location Type with Code '{3}'.\r\nPlease import or create it first and then retry the import operation.\r\n",
				warehouse.PK, warehouse.WW_WarehouseCode, warehouse.WW_WarehouseName, warehouse.LocationType.WLT_Code);

			AssertExceptionThrown(
				"Should not import warehouse if its default location type is not in DB.",
				typeof(ArgumentException),
				expectedErrorMessage,
				() => dataAdapter.CreateOrUpdateFromValueObject(xsdWarehouse, contextInOtherFactory));

			var warehouseInOtherFactory = otherFactory.Load<WhsWarehouse>(warehouse.PK);
			AssertNull("If warehouse had critical error, then it should not had been saved to DB.", warehouseInOtherFactory);
		}

		#endregion

		#region TestImport_WhsWarehouse_WrongDefaultInboundDockDoor

		public void TestImport_WhsWarehouse_WrongDefaultInboundDockDoor()
		{
			var warehouse = Helper.CreateWarehouse("WH1", shouldPreGenerateDDL: false);
			Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);

			AssertEquals("Precondition", AreaTypes.Codes.FreeStore, warehouse.Areas.Single().WA_AreaType);
			warehouse.WW_DefaultInboundDockDoor = warehouse.Rows[0].Locations[0].PK;
			Import_WhsWarehouse_WrongDefaultDockDoor_assert(warehouse, shouldImport: false, dockDoorType: "inbound");

			Helper.EnableWarehouseForDockDoor(warehouse, true);
			warehouse.WW_DefaultInboundDockDoor = warehouse.Rows.Single(a => a.WR_Name == WhsWarehouse.DefaultDockDoorRowName).Locations[0].PK;
			Import_WhsWarehouse_WrongDefaultDockDoor_assert(warehouse, shouldImport: true, dockDoorType: "inbound");

			warehouse.WW_DefaultInboundDockDoor = ZGuid.Empty;
			Import_WhsWarehouse_WrongDefaultDockDoor_assert(warehouse, shouldImport: true, dockDoorType: "inbound");

			warehouse.WW_DefaultInboundDockDoor = ZGuid.NewZGuid(); // wrong GUID
			Import_WhsWarehouse_WrongDefaultDockDoor_assert(warehouse, shouldImport: false, dockDoorType: "inbound");
		}

		#endregion

		#region TestImport_WhsWarehouse_WrongDefaultOutboundDockDoor

		public void TestImport_WhsWarehouse_WrongDefaultOutboundDockDoor()
		{
			var warehouse = Helper.CreateWarehouse("WH1", shouldPreGenerateDDL: false);
			Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);

			AssertEquals("Precondition", AreaTypes.Codes.FreeStore, warehouse.Areas.Single().WA_AreaType);
			warehouse.WW_DefaultOutboundDockDoor = warehouse.Rows[0].Locations[0].PK;
			Import_WhsWarehouse_WrongDefaultDockDoor_assert(warehouse, shouldImport: false, dockDoorType: "outbound");

			Helper.EnableWarehouseForDockDoor(warehouse, true);
			warehouse.WW_DefaultOutboundDockDoor = warehouse.Rows.Single(a => a.WR_Name == WhsWarehouse.DefaultDockDoorRowName).Locations[0].PK;
			Import_WhsWarehouse_WrongDefaultDockDoor_assert(warehouse, shouldImport: true, dockDoorType: "outbound");

			warehouse.WW_DefaultOutboundDockDoor = ZGuid.Empty;
			Import_WhsWarehouse_WrongDefaultDockDoor_assert(warehouse, shouldImport: true, dockDoorType: "outbound");

			warehouse.WW_DefaultOutboundDockDoor = ZGuid.NewZGuid(); // wrong GUID
			Import_WhsWarehouse_WrongDefaultDockDoor_assert(warehouse, shouldImport: false, dockDoorType: "outbound");
		}

		void Import_WhsWarehouse_WrongDefaultDockDoor_assert(WhsWarehouse warehouse, bool shouldImport, string dockDoorType)
		{
			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(warehouse, new ValueObjectExportContext(Notify));

			var otherFactory = new BusinessObjectFactory();
			otherFactory.ImportFromAnotherFactory(warehouse.WarehouseAddress);

			var otherAdapter = new SysMergeWarehouseValueObjectDataAdapter();
			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);

			if (shouldImport)
			{
				AssertNoExceptionThrown(
					"Should import warehouse when default dock door is correct.",
					() => otherAdapter.CreateOrUpdateFromValueObject(xsdWarehouse, contextInOtherFactory));
			}
			else
			{
				var expectedErrorMessage = $"Warehouse: [({warehouse.PK}) - {warehouse.WW_WarehouseCode} - {warehouse.WW_WarehouseName}]\r\nCould not find default {dockDoorType} dock door Location.\r\nPlease fix it first and then retry the import operation.\r\n";

				AssertExceptionThrown(
					"Should not import warehouse default dock door is wrong.",
					typeof(ArgumentException),
					expectedErrorMessage,
					() => otherAdapter.CreateOrUpdateFromValueObject(xsdWarehouse, contextInOtherFactory));

				var warehouseInOtherFactory = otherFactory.Load<WhsWarehouse>(warehouse.PK);
				AssertNull("If warehouse had critical error, then it should not have been saved to DB.", warehouseInOtherFactory);
			}
		}

		#endregion

		#region TestImport_WhsArea

		public void TestImport_WhsArea()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE", shouldPreGenerateDDL: false);
			whs.Areas.DeleteAll(); // remove Default Area
			var stagingArea = Helper.CreateArea(whs, "STAGING", AreaTypes.Codes.Bonded);
			var bulkArea = Helper.CreateArea(whs, "BULK", AreaTypes.Codes.FreeStore);

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(whs, new ValueObjectExportContext(Notify));

			var otherFactory = new BusinessObjectFactory();
			otherFactory.ImportFromAnotherFactory(whs.WarehouseAddress);

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseValueObjectDataAdapter();
			var importedWarehouseInOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdWarehouse, contextInOtherFactory);
			AssertEquals(2, importedWarehouseInOtherFactory.Areas.Count);
			AssertContainsArea(importedWarehouseInOtherFactory, stagingArea);
			AssertContainsArea(importedWarehouseInOtherFactory, bulkArea);
		}

		void AssertContainsArea(WhsWarehouse warehouse, WhsArea expectedArea)
		{
			var area = warehouse.Areas.Cast<WhsArea>().Single(a => a.PK == expectedArea.PK);
			AssertEquals("Area.WA_AreaType", expectedArea.WA_AreaType, area.WA_AreaType);
			AssertEquals("Area.WA_Name", expectedArea.WA_Name, area.WA_Name);
		}

		#endregion

		#region	TestImport_WhsRow()

		public void TestImport_WhsRow()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			var rowA = Helper.CreateRow(whs, "A", 4, 3, 2);
			var rowB = Helper.CreateRow(whs, "BULK", 1, 1, 1);

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(whs, new ValueObjectExportContext(Notify));

			var otherFactory = new BusinessObjectFactory();
			otherFactory.ImportFromAnotherFactory(whs.WarehouseAddress);

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseValueObjectDataAdapter();
			var importedWarehouseInOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdWarehouse, contextInOtherFactory);
			AssertEquals(3, importedWarehouseInOtherFactory.Rows.Count);
			AssertContainsRow(importedWarehouseInOtherFactory, rowA);
			AssertContainsRow(importedWarehouseInOtherFactory, rowB);
			AssertContainsRow(importedWarehouseInOtherFactory, whs.DefaultOutboundDockDoorLocation.Row);
		}

		void AssertContainsRow(WhsWarehouse warehouse, WhsRow expectedRow)
		{
			var row = warehouse.Rows.Cast<WhsRow>().Single(r => r.PK == expectedRow.PK);
			AssertEquals("Row.Name", expectedRow.WR_Name, row.WR_Name);
			AssertEquals("Row.Columns", expectedRow.WR_Columns, row.WR_Columns);
			AssertEquals("Row.Levels", expectedRow.WR_Levels, row.WR_Levels);
			AssertEquals("Row.Trays", expectedRow.WR_Trays, row.WR_Trays);
		}

		#endregion

		#region TestImport_WhsRow_WhsLocation

		public void TestImport_WhsRow_WhsLocation()
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE");
			var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			var area = Helper.CreateArea(whs, "STAGING", AreaTypes.Codes.Excise);
			var locationType = Helper.CreateLocationType("DAM");
			var locations = rowA.Locations;
			locations[0].WLV_ApprovedKnownLocation = "TSA";
			Helper.SetLocationMaxWeightAndVolume(locations[1], 12m, Constants.Weight.Tonnes, 4.5m, Constants.Volume.CubicMetres);
			locations[2].WLV_MaxDepth = 2m;
			locations[3].WLV_MaxHeight = 1m;
			locations[4].WLV_MaxWidth = 2m;
			locations[4].WLV_MaxDimensionUnit = "FT";
			locations[5].WLV_LocationStatus = LocationStatus.Codes.Void;
			locations[6].WLV_WLT_LocationType = locationType.PK;
			locations[7].WLV_PalletFloorSpaces = 2;
			locations[7].WLV_PalletStackHeight = 3;
			locations[7].WLV_PickMethod = "TST";
			locations[7].WLV_WA_PickingArea = area.PK;
			locations[7].WLV_WA_PutawayArea = area.PK;
			locations[7].WLV_MaxDimensionUnit = Constants.Length.Metres;

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(whs, new ValueObjectExportContext(Notify));

			var otherFactory = new BusinessObjectFactory();
			otherFactory.ImportFromAnotherFactory(whs.WarehouseAddress);
			otherFactory.ImportFromAnotherFactory(locationType);

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseValueObjectDataAdapter();
			var importedWarehouseInOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdWarehouse, contextInOtherFactory);

			AssertEquals("Imported warehouse should contain 2 Rows.", 2, importedWarehouseInOtherFactory.Rows.Count);
			foreach (var row in importedWarehouseInOtherFactory.Rows)
			{
				var originalRow = whs.Rows.Single(r => r.Locations.Count == row.Locations.Count);
				foreach (var originalLocation in originalRow.Locations)
				{
					AssertContainsLocation(row.Locations, originalLocation);
				}
			}
		}

		void AssertContainsLocation(WhsLocationCollection allLocationsCollection, WhsLocation expectedLocation)
		{
			var location = allLocationsCollection.Single(l => l.PK == expectedLocation.PK);
			AssertEquals("Location.WLV_ApprovedKnownLocation", expectedLocation.WLV_ApprovedKnownLocation, location.WLV_ApprovedKnownLocation);
			AssertEquals("Location.WLV_MaxHeight", expectedLocation.WLV_MaxHeight, location.WLV_MaxHeight);
			AssertEquals("Location.WLV_MaxWidth", expectedLocation.WLV_MaxWidth, location.WLV_MaxWidth);
			AssertEquals("Location.WLV_MaxDepth", expectedLocation.WLV_MaxDepth, location.WLV_MaxDepth);
			AssertEquals("Location.WLV_MaxDimensionUnit", expectedLocation.WLV_MaxDimensionUnit, location.WLV_MaxDimensionUnit);
			AssertEquals("Location.WLV_Column", expectedLocation.WLV_Column, location.WLV_Column);
			AssertEquals("Location.WLV_Level", expectedLocation.WLV_Level, location.WLV_Level);
			AssertEquals("Location.WLV_Tray", expectedLocation.WLV_Tray, location.WLV_Tray);
			AssertEquals("Location.WLV_LocationStatus", expectedLocation.WLV_LocationStatus, location.WLV_LocationStatus);
			AssertEquals("Location.WL_WLT_LocationType", expectedLocation.WLV_WLT_LocationType, location.WLV_WLT_LocationType);
			AssertEquals("Location.WLV_MaxCubic", expectedLocation.WLV_MaxCubic, location.WLV_MaxCubic);
			AssertEquals("Location.WLV_MaxCubicUnit", expectedLocation.WLV_MaxCubicUnit, location.WLV_MaxCubicUnit);
			AssertEquals("Location.WLV_MaxWeight", expectedLocation.WLV_MaxWeight, location.WLV_MaxWeight);
			AssertEquals("Location.WLV_MaxWeightUnit", expectedLocation.WLV_MaxWeightUnit, location.WLV_MaxWeightUnit);
			AssertEquals("Location.WLV_PalletFloorSpaces", expectedLocation.WLV_PalletFloorSpaces, location.WLV_PalletFloorSpaces);
			AssertEquals("Location.WLV_PalletStackHeight", expectedLocation.WLV_PalletStackHeight, location.WLV_PalletStackHeight);
			AssertEquals("Location.WLV_PickMethod", expectedLocation.WLV_PickMethod, location.WLV_PickMethod);
			AssertEquals("Location.WLV_WA_PickingArea", expectedLocation.WLV_WA_PickingArea, location.WLV_WA_PickingArea);
		}

		#endregion

		#region TestImport_WhsLocation_NoRow_NoArea

		public void TestImport_WhsLocation_NoRow_NoArea()
		{
			var warehouse = Helper.CreateWarehouse("TEST WAREHOUSE", shouldPreGenerateDDL: false);
			warehouse.Areas.DeleteAll(); // delete default area

			AssertEquals("Precondition", 0, warehouse.Rows.Count);
			AssertEquals("Precondition", 0, warehouse.Areas.Count);

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(warehouse, new ValueObjectExportContext(Notify));
			xsdWarehouse.DefaultOutboundDockDoorPK = ZGuid.NewZGuid().ToString();

			var otherFactory = new BusinessObjectFactory();
			otherFactory.ImportFromAnotherFactory(warehouse.WarehouseAddress);

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(otherFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseValueObjectDataAdapter();

			var expectedErrorMessage = $"Warehouse: [({warehouse.PK}) - {warehouse.WW_WarehouseCode} - {warehouse.WW_WarehouseName}]\r\nCould not find default outbound dock door Location.\r\nPlease fix it first and then retry the import operation.\r\n";

			AssertExceptionThrown(
				"Should not import warehouse default dock door is wrong.",
				typeof(ArgumentException),
				expectedErrorMessage,
				() => dataAdapter.CreateOrUpdateFromValueObject(xsdWarehouse, contextInOtherFactory));
		}

		#endregion

		#region TestImport_WhsWarehouse

		public void TestImport_WhsWarehouse_WithDefaultDockDoor()
		{
			Import_WhsWarehouse_DefaultDockDoorCore(true);
		}

		public void TestImport_WhsWarehouse_NoDefaultDockDoor()
		{
			Import_WhsWarehouse_DefaultDockDoorCore(false);
		}

		void Import_WhsWarehouse_DefaultDockDoorCore(bool generateDefaultDockDoor)
		{
			var whs = Helper.CreateWarehouse("TEST WAREHOUSE", generateDefaultDockDoor);
			Helper.CreateRowAndGenerateLocations(whs, "A", 2, 2, 2);
			whs.WarehouseAddress.FillWithValidTestData();

			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			var xsdWarehouse = adapter.ExportToValueObject(whs, new ValueObjectExportContext(Notify));

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			newFactory.ImportFromAnotherFactory(whs.RelatedCompanyBranch);
			newFactory.ImportFromAnotherFactory(whs.WarehouseAddress.Header);
			newFactory.ImportFromAnotherFactory(whs.WarehouseAddress);
			AssertEquals("Precondition", 0, newFactory.Load<WhsWarehouse>(new ZQuery()).Length);
			AssertEquals("Precondition", 0, newFactory.Load<WhsLocation>(new ZQuery()).Length);

			if (generateDefaultDockDoor)
			{
				AssertNotNull(xsdWarehouse.Areas.Cast<SystemMergeArea>().Single(a => a.Name == WhsWarehouse.DefaultDockDoorAreaName));
				var rowDDD = xsdWarehouse.Rows.Cast<SystemMergeRow>().Single(r => r.Name == WhsWarehouse.DefaultDockDoorRowName);
				var locationsDDD = rowDDD.Locations;
				AssertEquals("WarehouseXML should contain 1 Locations.", 1, locationsDDD.Count);
			}
			else
			{
				AssertNull(xsdWarehouse.Areas.Cast<SystemMergeArea>().SingleOrDefault(a => a.Name == WhsWarehouse.DefaultDockDoorAreaName));
			}

			AssertNotNull(xsdWarehouse.Areas.Cast<SystemMergeArea>().Single(a => a.Name == "DEFAULT"));
			var rowA = xsdWarehouse.Rows.Cast<SystemMergeRow>().Single(r => r.Name == "A");

			var locationsA = rowA.Locations;
			AssertEquals("WarehouseXML should contain 8 ( 2 * 2 * 2 ) Locations.", 8, locationsA.Count);

			var notificationBuffer = new NotificationBuffer();
			var contextInOtherFactory = new ValueObjectImportContext(newFactory, notificationBuffer);
			var dataAdapter = new SysMergeWarehouseValueObjectDataAdapter();
			var importedWarehouseInOtherFactory = dataAdapter.CreateOrUpdateFromValueObject(xsdWarehouse, contextInOtherFactory);
			if (generateDefaultDockDoor)
			{
				AssertEquals(1, importedWarehouseInOtherFactory.Rows.Single(a => a.WR_Name == WhsWarehouse.DefaultDockDoorRowName).Locations.Count);
				AssertEquals("Precondition - before save", 9, newFactory.Load<WhsLocation>(new ZQuery()).Length);
			}
			else
			{
				AssertEquals("Precondition - before save", 8, newFactory.Load<WhsLocation>(new ZQuery()).Length);
			}

			AssertEquals(8, importedWarehouseInOtherFactory.Rows.Single(a => a.WR_Name == "A").Locations.Count);
			newFactory.Save();

			var importFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var whsimported = importFactory.Load<WhsWarehouse>(whs.PK);
			var importedRowPKs = whsimported.Rows.Select(r => r.PK);
			var importedAreaPKs = whsimported.Areas.Select(a => a.PK);
			var importedLocationPKs = whsimported.Rows.SelectMany(a => a.Locations.Select(l => l.PK));
			AssertEquals("Regardless if xml originally had default dock door location, at the end of import it should include default dock door location", 2, importedRowPKs.Count());
			AssertEquals("Regardless if xml originally had default dock door location, at the end of import it should include default dock door location", 2, importedAreaPKs.Count());
			AssertEquals("Regardless if xml originally had default dock door location, at the end of import it should include default dock door location", 9, importedLocationPKs.Count());

			if (!generateDefaultDockDoor)
			{
				importedRowPKs = importedRowPKs.Except(new ZGuid[] { importedWarehouseInOtherFactory.Rows.Single(a => a.WR_Name == "DOCKDOOR").PK });
				importedAreaPKs = importedAreaPKs.Except(new ZGuid[] { importedWarehouseInOtherFactory.Areas.Single(a => a.WA_Name == "[DEFAULT DOCK DOOR]").PK });
				importedLocationPKs = importedLocationPKs.Except(new ZGuid[] { importedWarehouseInOtherFactory.Rows.Single(a => a.WR_Name == "DOCKDOOR").Locations.Single().PK });
			}
			AssertContainsExactElementsInAnyOrder("Should not change the PKs", whs.Rows.Select(r => r.PK), importedRowPKs);
			AssertContainsExactElementsInAnyOrder("Should not change the PKs", whs.Areas.Select(a => a.PK), importedAreaPKs);
			AssertContainsExactElementsInAnyOrder("Should not change the PKs", whs.Rows.SelectMany(a => a.Locations).Select(l => l.PK), importedLocationPKs);
		}

		#endregion

		#endregion

		#region TestOverrides

		public void TestOverrides()
		{
			var adapter = new SysMergeWarehouseValueObjectDataAdapter();
			AssertEquals("SystemMergeWarehouses", adapter.RootCollectionElementName);
			AssertEquals("SystemMergeWarehouse", adapter.RootElementName);
			AssertNull(adapter.CollectionSchema);
			AssertNull(adapter.Schema);
		}

		#endregion
	}
}
