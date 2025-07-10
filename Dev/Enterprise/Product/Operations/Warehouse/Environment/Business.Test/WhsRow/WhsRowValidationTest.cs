using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	public class WhsRowValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWR_PickPathSequence

		public void TestCheckWR_PickPathSequence()
		{
			var row = Factory.New<WhsRow>();
			AssertNoErrors("Precondition", row.WR_PickPathSequenceInfo);

			row.WR_PickPathSequence = 0;
			AssertNoErrors(row.WR_PickPathSequenceInfo);
			AssertHasWarning(row.WR_PickPathSequenceInfo, "A value of Zero indicates this Row will be *Last* in the Pick Sequence.");

			row.WR_PickPathSequence = new ZShort(-1);
			AssertHasError(row.WR_PickPathSequenceInfo, "Row Path Sequence cannot be negative.");
			AssertNoWarnings(row.WR_PickPathSequenceInfo);

			row.WR_PickPathSequence = new ZShort(1);
			AssertNoErrors(row.WR_PickPathSequenceInfo);
			AssertNoWarnings(row.WR_PickPathSequenceInfo);
		}

		#endregion

		#region TestCheckWR_WW_Whs

		public void TestCheckWR_WW_Whs()
		{
			TestMandatoryGuid(Row.WR_WW_WhsInfo, ErrorCheckType.HasErrors);
			Row.WR_WW_Whs = Whs.PK; // Add warehouse to allow save
			Row.WR_Name = "B";

			var melWarehouse = Helper.CreateWarehouse("MEL");
			var sydWarehouse = Helper.CreateWarehouse("SYD");

			var row = melWarehouse.Rows.AddNew();
			row.WR_Name = "A";
			AssertNoErrors(row.WR_WW_WhsInfo);

			row.WR_WW_Whs = sydWarehouse.PK;
			AssertNoErrors(row.WR_WW_WhsInfo);

			Factory.Save();
			row.WR_WW_Whs = melWarehouse.PK;
			AssertHasErrorContaining(row.WR_WW_WhsInfo, "The Warehouse cannot be changed once the Row is saved.");

			row.WR_WW_Whs = sydWarehouse.PK;
			AssertNoErrors(row.WR_WW_WhsInfo);
		}

		public void TestCheckWR_WW_Whs_WhenWarehouseHasNoDefaultsConfigured()
		{
			var whs1 = Helper.CreateWarehouse("WH1");
			var row = Helper.CreateRow(whs1, "rw1");
			AssertNoErrors(row.WR_WW_WhsInfo);
			AssertNoWarnings(row.WR_WW_WhsInfo);

			var defaultArea = whs1.Areas.Single(a => a.WA_Name == "DEFAULT");
			defaultArea.WA_IsDefaultPickArea = false;
			defaultArea.WA_IsDefaultPutawayArea = false;

			row.Validation.ValidateWR_WW_Whs();
			AssertHasError(row.WR_WW_WhsInfo, "Warehouse does not have a Default Pick Area set.");
			AssertHasError(row.WR_WW_WhsInfo, "Warehouse does not have a Default Putaway Area set.");
		}

		#endregion

		#region TestCheckWR_Name

		public void TestCheckWR_Name()
		{
			TestMandatoryString(Row.WR_NameInfo, ErrorCheckType.HasErrors);

			Whs.WW_LocationComponentDelimiter = "~";
			Row.WR_Name = "A 1";
			AssertHasError(Row.WR_NameInfo, ErrMsg(Row));

			Row.WR_Name = "A,1";
			AssertHasError(Row.WR_NameInfo, ErrMsg(Row));

			Row.WR_Name = "A.1";
			AssertHasError(Row.WR_NameInfo, ErrMsg(Row));

			Row.WR_Name = "A-1";
			AssertHasError(Row.WR_NameInfo, ErrMsg(Row));

			Row.WR_Name = "A" + Whs.WW_LocationComponentDelimiter + "1";
			AssertHasError(Row.WR_NameInfo, ErrMsg(Row));

			Row.WR_Name = "A1";
			Assert("Valid name has an error", !Row.WR_NameInfo.HasErrors());
		}

		public void TestCheckWR_Name_WithNullWW_LocationComponentDelimiter()
		{
			Whs.WW_LocationComponentDelimiter = null;
			Row.WR_Name = "A1";
			AssertHasError(Row.WR_NameInfo, ErrMsg(Row));
		}

		string ErrMsg(WhsRow row)
		{
			return string.Format(
				"The {0} cannot contain the following spaces or the following characters:\r\n" +
				". , - {1}", DataBoundResourceStrings.GetColumnDescriptiveName(row.TableName, row.WR_NameInfo.Name), row.Warehouse.WW_LocationComponentDelimiter);
		}

		#endregion

		#region TestCheckWR_NameForDuplicates

		public void TestCheckWR_NameForDuplicates()
		{
			Row.WR_Name = "AA";

			var row2 = Whs.Rows.AddNew();
			row2.WR_Name = "AB";
			AssertEquals("Should be ok", false, row2.WR_NameInfo.HasErrors());

			row2.WR_Name = "AA";
			AssertHasError(row2.WR_NameInfo, "Duplicate Row names are not allowed");
			row2.WR_Name = "AB";

			// multi warehouse test, same row names allowed on different warehouses
			var whs2 = Factory.New<WhsWarehouse>();
			var row21 = whs2.Rows.AddNew();
			row21.WR_Name = "AA";
			AssertEquals("Should be ok", false, row21.WR_NameInfo.HasErrors());

			var row22 = whs2.Rows.AddNew();
			row22.WR_Name = "AA";
			AssertHasError(row22.WR_NameInfo, "Duplicate Row names are not allowed");
			row22.WR_Name = "AB";
			AssertEquals("Should be ok", false, row22.WR_NameInfo.HasErrors());
		}

		#endregion

		#region TestCheckWR_Name_FixedWidthSet_ForPrefixDuplicates

		public void TestCheckWR_Name_FixedWidthSet_ForPrefixDuplicates()
		{
			AssertCheckWR_Name_FixedWidthSet_ForPrefixDuplicates(validationRegistryValue: true);
		}

		public void TestCheckWR_Name_FixedWidthSet_ForPrefixDuplicates_RegistryOff()
		{
			AssertCheckWR_Name_FixedWidthSet_ForPrefixDuplicates(validationRegistryValue: false);
		}

		void AssertCheckWR_Name_FixedWidthSet_ForPrefixDuplicates(bool validationRegistryValue)
		{
			using (WarehouseDataRegistry.Instance.EnableRowNamePrefixValidationForFixedWidthWarehouses.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, validationRegistryValue))
			{
				const string FixedWidthPrefixError = "With 'Fixed Width Location' set, Row names cannot be prefixes of other Row names from the same Warehouse.";
				var whs = Helper.CreateWarehouse("TST");
				whs.IsFixedWidthLocation = true;
				var row1 = whs.Rows.AddNew();
				row1.WR_Name = "B";

				var row2 = whs.Rows.AddNew();
				row2.WR_Name = "AB";
				AssertNoErrors(row2.WR_NameInfo);
				Factory.Save();

				row2.WR_Name = "B123";
				if (validationRegistryValue)
				{
					AssertHasError(row2.WR_NameInfo, FixedWidthPrefixError);
				}
				else
				{
					AssertNoErrors(row2.WR_NameInfo);
				}

				row2.WR_Name = "AB";
				row1.WR_Name = "BA";
				AssertNoErrors(row2.WR_NameInfo);
				Factory.Save();

				row2.WR_Name = "BA6";
				if (validationRegistryValue)
				{
					AssertHasError(row2.WR_NameInfo, FixedWidthPrefixError);
				}
				else
				{
					AssertNoErrors(row2.WR_NameInfo);
				}

				row2.WR_Name = "AB";
				row1.WR_Name = "B2";
				Factory.Save();

				row2.WR_Name = "B123";
				AssertNoErrors(row2.WR_NameInfo);
				Factory.Save();

				row2.WR_Name = "B";
				if (validationRegistryValue)
				{
					AssertHasError(row2.WR_NameInfo, FixedWidthPrefixError);
				}
				else
				{
					AssertNoErrors(row2.WR_NameInfo);
				}

				var whs2 = Helper.CreateWarehouse("T2T");
				Factory.Save();

				AssertEquals("Precondition: IsFixedWidthLocation should be false", false, whs2.IsFixedWidthLocation);

				var row21 = whs2.Rows.AddNew();
				row21.WR_Name = "B2AA";
				Factory.Save();

				var row22 = whs2.Rows.AddNew();
				row22.WR_Name = "B2";

				AssertNoErrors(row21.WR_NameInfo);
				AssertNoErrors(row22.WR_NameInfo);
			}
		}

		#endregion

		#region TestCheckWR_Columns

		public void TestCheckWR_Columns()
		{
			Whs.WW_LocationColumnsAlpha = false;
			TestMinMaxShort(Row.WR_ColumnsInfo, ErrorCheckType.HasErrors, 1, 999);
			Whs.WW_LocationColumnsAlpha = true;
			TestMinMaxShort(Row.WR_ColumnsInfo, ErrorCheckType.HasErrors, 1, 26);
		}

		#endregion

		#region TestCheckWR_Levels

		public void TestCheckWR_Levels()
		{
			Whs.WW_LocationLevelsAlpha = false;
			TestMinMaxShort(Row.WR_LevelsInfo, ErrorCheckType.HasErrors, 1, 999);
			Whs.WW_LocationLevelsAlpha = true;
			TestMinMaxShort(Row.WR_LevelsInfo, ErrorCheckType.HasErrors, 1, 26);
		}

		#endregion

		#region TestCheckWR_Trays

		public void TestCheckWR_Trays()
		{
			Whs.WW_LocationTraysAlpha = false;
			TestMinMaxShort(Row.WR_TraysInfo, ErrorCheckType.HasErrors, 1, 99);

			Whs.WW_LocationTraysAlpha = true;
			TestMinMaxShort(Row.WR_TraysInfo, ErrorCheckType.HasErrors, 1, 26);
		}

		public void TestCheckWR_Trays_ContainerYardWarehouse()
		{
			var productWarehouse = Helper.CreateWarehouse("PRW");
			var rowInPRW = productWarehouse.Rows.AddNew();
			rowInPRW.WR_Trays = 2;
			AssertEquals("Precondition: warehouse type is Product.", WarehouseTypes.Codes.Product, productWarehouse.WW_WarehouseType);
			AssertNoErrors("It's a Product Warehouse, WR_Tray can be greater than 1.", rowInPRW.WR_TraysInfo);

			var cydWarehouse = Helper.CreateWarehouse("CYD");
			cydWarehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			var rowInCYD = cydWarehouse.Rows.AddNew();

			rowInCYD.WR_Trays = 1;
			AssertNoErrors("Container Yard WR_Tray must remain 1.", rowInCYD.WR_TraysInfo);

			rowInCYD.WR_Trays = 0;
			AssertHasError(rowInCYD.WR_TraysInfo, "A Row must have at least 1 Tray.");

			rowInCYD.WR_Trays = 2;
			AssertHasError(rowInCYD.WR_TraysInfo, "Container Yard do not use trays and the value must remain at 1.");

			rowInCYD.WR_Trays = 1;
			AssertNoErrors("Container Yard WR_Tray must remain 1.", rowInCYD.WR_TraysInfo);
		}

		#endregion

		#region Test WR_Columns, WR_Levels, WR_Trays CannotBeReducedAfterSaving

		public void TestCheckWR_Columns_CannotBeReducedAfterSaving()
		{
			TestCheckDimensionCannotBeReducedAfterSaving(WhsRowSchema.WR_Columns, WhsLocationViewSchema.WLV_Column, "Columns");
		}

		public void TestCheckWR_Levels_CannotBeReducedAfterSaving()
		{
			TestCheckDimensionCannotBeReducedAfterSaving(WhsRowSchema.WR_Levels, WhsLocationViewSchema.WLV_Level, "Levels");
		}

		public void TestCheckWR_Trays_CannotBeReducedAfterSaving()
		{
			TestCheckDimensionCannotBeReducedAfterSaving(WhsRowSchema.WR_Trays, WhsLocationViewSchema.WLV_Tray, "Trays");
		}

		/// <summary>
		/// Dimension should be one of Column, Level or Tray.
		/// </summary>
		public void TestCheckDimensionCannotBeReducedAfterSaving(SchemaColumn dimensionColumn, SchemaShortColumn locationColumn, ZString dimensionText)
		{
			var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var orgPK = iHelper.CreateClient("WHS1TST");
			var part = iHelper.CreateProduct(orgPK, "P1");
			var warehouse = Helper.CreateWarehouse("MEL");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 5, 5, 5);

			row[dimensionColumn.Name] = (short)10; // saved value
			Factory.Save();

			var dimensionInfo = row.ZPropertyInfoHash[dimensionColumn.Name];
			row[dimensionColumn.Name] = (short)15;
			AssertNoErrors("Should be no errors - only increased dimension.", dimensionInfo);

			row[dimensionColumn.Name] = (short)8; // saved value
			AssertNoErrors("Should be no errors - decreased, locations not in use.", dimensionInfo);

			var receivePK = iHelper.CreateWhsReceive(orgPK, warehouse.PK, "1", Notify);
			var receiveLinePK = iHelper.CreateWhsReceiveInventoryLine(receivePK, part.PK, 10m, "A-3-3-3");
			iHelper.FinaliseDocket(receivePK);

			row.SortPickPathMethod = CodeLists.SortPathMethods.Codes.ColumnThenLevel;
			row.UpdatePathSequenceOnLocations();
			AssertEquals("Pick Path Sequence should have the correct order", true, TestPickPathSequenceWasRegenerated(row));

			Factory.Save();
			TestRowLocations(row, locationColumn, 8);

			row[dimensionColumn.Name] = (short)6;
			AssertNoErrors("Should be no errors - decreased, locations not in use.", dimensionInfo);

			row[dimensionColumn.Name] = (short)10; // saved value
			AssertNoErrors("Should be no errors - only increased dimension.", dimensionInfo);

			Factory.Save();
			TestRowLocations(row, locationColumn, 10);
			AssertEquals("Pick Path Sequence should not have the correct order after locations is increased.", false, TestPickPathSequenceWasRegenerated(row));

			row[dimensionColumn.Name] = (short)2;
			AssertHasError(dimensionInfo, string.Format("The number of {0} cannot be reduced. Enter a value greater than or equal to 10.\r\n\r\nLocations, however, can be given a void status.", dimensionText));

			row[dimensionColumn.Name] = (short)5; // saved value
			AssertNoErrors(dimensionInfo);

			row.Locations[10].WLV_PickPathSequence = 999; // PickPathSequence error
			AssertEquals("Pick Path Sequence still shouldn't have the correct order.", false, TestPickPathSequenceWasRegenerated(row));

			Factory.Save();
			TestRowLocations(row, locationColumn, 5);
			AssertEquals("Pick Path Sequence still shouldn't have the correct order. Save will not re-sort the path sequence.", false, TestPickPathSequenceWasRegenerated(row)); // PickPathSequence regenerated on save
		}

		void TestRowLocations(WhsRow row, SchemaShortColumn locationColumn, short maxDimension)
		{
			// Deleted items
			AssertEquals("Locations should be deleted.", false, row.Locations.Any(x => (ZShort)x[locationColumn] > maxDimension));

			// Remain items
			var locationsCount = 0;
			for (ZShort col = 1; col <= row.WR_Columns; col++)
			{
				for (ZShort level = 1; level <= row.WR_Levels; level++)
				{
					for (ZShort tray = 1; tray <= row.WR_Trays; tray++)
					{
						Assert("Should contain location", row.Locations.FindByColumnLevelTray(col, level, tray) != null);
						locationsCount++;
					}
				}
			}

			AssertEquals("The location count should match.", true, locationsCount == row.Locations.Count);
		}

		bool TestPickPathSequenceWasRegenerated(WhsRow row)
		{
			foreach (var location in row.Locations.ToArray())
			{
				var proposedPickSequence = ((location.WLV_Column - 1) * row.WR_Levels * row.WR_Trays) + ((location.WLV_Level - 1) * row.WR_Trays) + location.WLV_Tray;

				if (location.WLV_PickPathSequence != proposedPickSequence)
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		#region TestColumnsLevelsAndTraysCannotMultiplyToOverTenThousand

		public void TestColumnsLevelsAndTraysCannotMultiplyToOverTenThousand()
		{
			var errorMessage = "The number of locations is over 10000 with this combination of Columns, Levels and Trays.";

			var row = Factory.New<WhsRow>();
			row.WR_Columns = 10;
			row.WR_Levels = 10;
			row.WR_Trays = 10;
			// 10 X 10 x 10 = 1000
			AssertColumnsLevelsTrays_NoLocationErrors(row);

			row.WR_Columns = 100;
			// 100 x 10 x 10 = 10000
			AssertColumnsLevelsTrays_NoLocationErrors(row);

			row.WR_Levels = 20;
			// 100 x 20 x 10 = 20000
			AssertColumnsLevelsTrays_LocationErrors(row, errorMessage);

			row.WR_Levels = 5;
			// 100 x 5 x 10 = 5000
			AssertColumnsLevelsTrays_NoLocationErrors(row);

			row.WR_Columns = 210;
			// 210 x 5 x 10 = 10500
			AssertColumnsLevelsTrays_LocationErrors(row, errorMessage);

			row.WR_Columns = 100;
			// 100 x 5 x 10 = 5000
			AssertColumnsLevelsTrays_NoLocationErrors(row);

			row.WR_Trays = 30;
			// 100 x 5 x 30 = 15000
			AssertColumnsLevelsTrays_LocationErrors(row, errorMessage);

			row.WR_Trays = 10;
			// 100 x 5 x 10 = 5000
			AssertColumnsLevelsTrays_NoLocationErrors(row);
		}

		#endregion

		#region TestColumnsLevelsAndTraysCannotMultiplyToOverRegistryLocationLimit

		public void TestColumnsLevelsAndTraysCannotMultiplyToOverRegistryLocationLimit()
		{
			WarehouseDataRegistry.Instance.MaxNumberOfLocationsPerRow.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20000);
			var errorMessage = "The number of locations is over 20000 with this combination of Columns, Levels and Trays.";

			var row = Factory.New<WhsRow>();
			row.WR_Columns = 100;
			row.WR_Levels = 10;
			row.WR_Trays = 10;
			// 100 x 10 x 10 = 10000
			AssertColumnsLevelsTrays_NoLocationErrors(row);

			row.WR_Levels = 15;
			// 100 x 15 x 10 = 15000
			AssertColumnsLevelsTrays_NoLocationErrors(row);

			row.WR_Levels = 20;
			// 100 x 20 x 10 = 20000
			AssertColumnsLevelsTrays_NoLocationErrors(row);

			row.WR_Trays = 11;
			// 100 x 20 x 11 = 22000
			AssertColumnsLevelsTrays_LocationErrors(row, errorMessage);

			row.WR_Trays = 10;
			// 100 x 20 x 10 = 20000
			AssertColumnsLevelsTrays_NoLocationErrors(row);

			row.WR_Levels = 21;
			// 100 x 21 x 10 = 21000
			AssertColumnsLevelsTrays_LocationErrors(row, errorMessage);

			row.WR_Levels = 20;
			// 100 x 20 x 10 = 20000
			AssertColumnsLevelsTrays_NoLocationErrors(row);

			row.WR_Columns = 101;
			// 101 x 20 x 10 = 20200
			AssertColumnsLevelsTrays_LocationErrors(row, errorMessage);

			row.WR_Columns = 100;
			// 100 x 20 x 10 = 20000
			AssertColumnsLevelsTrays_NoLocationErrors(row);
		}

		#endregion

		#region TestColumnsLevelsAndTraysCannotMultiplyToOverTenThousand_CanSaveWithExistingInvalidData

		public void TestColumnsLevelsAndTraysCannotMultiplyToOverTenThousand_CanSaveWithExistingInvalidData()
		{
			var validationMessage = "The number of locations is over 10000 with this combination of Columns, Levels and Trays.";

			var whs = Helper.CreateWarehouse("WHS", "A");
			var row = whs.Rows[0];
			using (row.SuspendValidationTesting())
			{
				// 100 x 5 x 30 = 15,000
				row.WR_Columns = 100;
				row.WR_Levels = 5;
				row.WR_Trays = 30;
				Factory.Save();
			}

			// No change
			row.WR_Columns = 100; // Poke
			AssertColumnsLevelsTrays_NoLocationErrors_HasWarnings(row, validationMessage);

			// Increase columns
			row.WR_Columns += 1;
			AssertColumnsLevelsTrays_LocationErrors(row, validationMessage);

			row.WR_Columns -= 1;
			AssertColumnsLevelsTrays_NoLocationErrors_HasWarnings(row, validationMessage);

			// Increase levels
			row.WR_Levels += 1;
			AssertColumnsLevelsTrays_LocationErrors(row, validationMessage);

			row.WR_Levels -= 1;
			AssertColumnsLevelsTrays_NoLocationErrors_HasWarnings(row, validationMessage);

			// Increase trays
			row.WR_Trays += 1;
			AssertColumnsLevelsTrays_LocationErrors(row, validationMessage);

			row.WR_Trays -= 1;
			AssertColumnsLevelsTrays_NoLocationErrors_HasWarnings(row, validationMessage);
		}

		#endregion

		#region TestColumnsLevelsAndTraysCannotMultiplyToOverRegistryLimit_CanSaveWithExistingInvalidData

		public void TestColumnsLevelsAndTraysCannotMultiplyToOverRegistryLimit_CanSaveWithExistingInvalidData()
		{
			WarehouseDataRegistry.Instance.MaxNumberOfLocationsPerRow.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 20000);
			var validationMessage = "The number of locations is over 20000 with this combination of Columns, Levels and Trays.";

			var whs = Helper.CreateWarehouse("WHS", "A");
			var row = whs.Rows[0];
			using (row.SuspendValidationTesting())
			{
				// 100 x 7 x 30 = 21,000
				row.WR_Columns = 100;
				row.WR_Levels = 7;
				row.WR_Trays = 30;
				Factory.Save();
			}

			// No change
			row.WR_Columns = 100; // Poke
			AssertColumnsLevelsTrays_NoLocationErrors_HasWarnings(row, validationMessage);

			// Increase columns
			row.WR_Columns += 1;
			AssertColumnsLevelsTrays_LocationErrors(row, validationMessage);

			row.WR_Columns -= 1;
			AssertColumnsLevelsTrays_NoLocationErrors_HasWarnings(row, validationMessage);

			// Increase levels
			row.WR_Levels += 1;
			AssertColumnsLevelsTrays_LocationErrors(row, validationMessage);

			row.WR_Levels -= 1;
			AssertColumnsLevelsTrays_NoLocationErrors_HasWarnings(row, validationMessage);

			// Increase trays
			row.WR_Trays += 1;
			AssertColumnsLevelsTrays_LocationErrors(row, validationMessage);

			row.WR_Trays -= 1;
			AssertColumnsLevelsTrays_NoLocationErrors_HasWarnings(row, validationMessage);
		}

		#endregion

		#region Assert Columns/Levels/Rows

		void AssertColumnsLevelsTrays_NoLocationErrors_HasWarnings(WhsRow row, ZString warning)
		{
			AssertColumnsLevelsTrays_NoLocationErrors(row);
			AssertHasWarning(row.WR_ColumnsInfo, warning);
			AssertHasWarning(row.WR_LevelsInfo, warning);
			AssertHasWarning(row.WR_TraysInfo, warning);
		}

		void AssertColumnsLevelsTrays_LocationErrors(WhsRow row, ZString error)
		{
			AssertHasError(row.WR_ColumnsInfo, error);
			AssertHasError(row.WR_LevelsInfo, error);
			AssertHasError(row.WR_TraysInfo, error);
		}

		void AssertColumnsLevelsTrays_NoLocationErrors(WhsRow row)
		{
			AssertNoErrors(row.WR_ColumnsInfo);
			AssertNoErrors(row.WR_LevelsInfo);
			AssertNoErrors(row.WR_TraysInfo);
		}

		#endregion

		#region TestCheckForCurrentStock

		//public void TestCheckForCurrentStock()
		//{
		//    var iHelper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);

		//    var orgPK = iHelper.CreateClient("WHS1TST");
		//    var prodPK = iHelper.CreateProduct(orgPK, "P1");

		//    var whs = Helper.CreateWarehouse("W1");
		//    var rowA = Helper.CreateRowAndGenerateLocations(whs, "A", 1, 1, 1);
		//    var rowB = Helper.CreateRowAndGenerateLocations(whs, "B", 3, 3, 3);

		//    var receivePK = iHelper.CreateWhsReceive(orgPK, whs.PK, "1", Notify);
		//    iHelper.CreateWhsReceiveInventoryLine(receivePK, prodPK, 10m, "A");
		//    iHelper.CreateWhsReceiveInventoryLine(receivePK, prodPK, 10m, "B-1-1-1");
		//    iHelper.CreateWhsReceiveInventoryLine(receivePK, prodPK, 10m, "B-2-1-1");
		//    iHelper.CreateWhsReceiveInventoryLine(receivePK, prodPK, 10m, "B-1-2-1");
		//    iHelper.CreateWhsReceiveInventoryLine(receivePK, prodPK, 10m, "B-1-1-2");
		//    iHelper.FinaliseDocket(receivePK);
		//    Factory.Save(); // needed for DBOnly Query in Row.Inventory

		//    rowB.WR_Columns = 1;
		//    AssertHasError(rowB.WR_ColumnsInfo, WhsRowValidation.CurrentStockInColumnErrorMsg);
		//    AssertNoErrors(rowB.WR_LevelsInfo);
		//    AssertNoErrors(rowB.WR_TraysInfo);

		//    rowB.WR_Columns = 2;
		//    rowB.WR_Levels = 1;
		//    AssertNoErrors(rowB.WR_ColumnsInfo);
		//    AssertHasError(rowB.WR_LevelsInfo, WhsRowValidation.CurrentStockInLevelErrorMsg);
		//    AssertNoErrors(rowB.WR_TraysInfo);

		//    rowB.WR_Levels = 2;
		//    rowB.WR_Trays = 1;
		//    AssertNoErrors(rowB.WR_ColumnsInfo);
		//    AssertNoErrors(rowB.WR_LevelsInfo);
		//    AssertHasError(rowB.WR_TraysInfo, WhsRowValidation.CurrentStockInTrayErrorMsg);

		//    // remove all stock qty's, so locations should now be allowed to be removed
		//    Db.Connection.ExecuteNonQuery("update whsinventory set wi_totalunits = 0");    This is easier

		//    rowB.WR_Columns = 1;
		//    rowB.WR_Levels = 1;
		//    rowB.WR_Trays = 1;
		//    AssertNoErrors(rowB.WR_ColumnsInfo);
		//    AssertNoErrors(rowB.WR_LevelsInfo);
		//    AssertNoErrors(rowB.WR_TraysInfo);
		//}

		#endregion

		#region Non Persistent

		public void TestValidateSortPickPathMethod()
		{
			var row = Factory.New<WhsRow>();
			AssertEquals(true, row.SortPickPathMethod.IsEmpty);
			AssertEquals(false, row.SortPickPathMethodInfo.HasErrors());

			row.SortPickPathMethod = "ABC";
			AssertEquals(true, row.SortPickPathMethodInfo.HasErrors());

			row.SortPickPathMethod = CodeLists.SortPathMethods.Codes.ColumnThenLevel;
			AssertEquals(false, row.SortPickPathMethodInfo.HasErrors());
		}

		public void TestValidateSortPutawayPathMethod()
		{
			var row = Factory.New<WhsRow>();
			AssertEquals(true, row.SortPutawayPathMethod.IsEmpty);
			AssertEquals(false, row.SortPutawayPathMethodInfo.HasErrors());

			row.SortPutawayPathMethod = "ABC";
			AssertEquals(true, row.SortPutawayPathMethodInfo.HasErrors());

			row.SortPutawayPathMethod = CodeLists.SortPathMethods.Codes.ColumnThenLevel;
			AssertEquals(false, row.SortPutawayPathMethodInfo.HasErrors());
		}

		public void TestValidateSortCycleCountMethod()
		{
			var row = Factory.New<WhsRow>();
			AssertEquals(true, row.SortCycleCountMethod.IsEmpty);
			AssertEquals(false, row.SortCycleCountMethodInfo.HasErrors());

			row.SortCycleCountMethod = "ABC";
			AssertEquals(true, row.SortCycleCountMethodInfo.HasErrors());

			row.SortCycleCountMethod = CodeLists.SortPathMethods.Codes.ColumnThenLevel;
			AssertEquals(false, row.SortCycleCountMethodInfo.HasErrors());
		}

		#endregion

		#region TestSettingTooBigForLocationsFixedWidth

		public void TestSettingTooBigForLocationsFixedWidth_Columns_FixedWidthIsTwo()
		{
			TestSettingTooBigForLocationsFixedWidth(
				(row, warehouse) =>
				{
					warehouse.WW_LocationColumnsFixedWidth = 2;
					row.WR_Columns = 99;
				},
				row => row.WR_Columns = 100,
				row => row.WR_ColumnsInfo,
				"A Row must not have more than 99 Columns.");
		}

		public void TestSettingTooBigForLocationsFixedWidth_Columns_FixedWidthIsOne()
		{
			TestSettingTooBigForLocationsFixedWidth(
				(row, warehouse) =>
				{
					row.WR_Columns = 9;
				},
				row => row.WR_Columns = 10,
				row => row.WR_ColumnsInfo,
				"A Row must not have more than 9 Columns.");
		}

		public void TestSettingTooBigForLocationsFixedWidth_Columns_AlphaColumns()
		{
			TestSettingTooBigForLocationsFixedWidth(
				(row, warehouse) =>
				{
					warehouse.WW_LocationColumnsAlpha = true;
					row.WR_Columns = 26;
				},
				row => row.WR_Columns = 27,
				row => row.WR_ColumnsInfo,
				"A Row must not have more than 26 Columns.");
		}

		public void TestSettingTooBigForLocationsFixedWidth_Levels_FixedWidthIsTwo()
		{
			TestSettingTooBigForLocationsFixedWidth(
				(row, warehouse) =>
				{
					warehouse.WW_LocationLevelsFixedWidth = 2;
					row.WR_Levels = 99;
				},
				row => row.WR_Levels = 100,
				row => row.WR_LevelsInfo,
				"A Row must not have more than 99 Levels.");
		}

		public void TestSettingTooBigForLocationsFixedWidth_Levels_FixedWidthIsOne()
		{
			TestSettingTooBigForLocationsFixedWidth(
				(row, warehouse) =>
				{
					row.WR_Levels = 9;
				},
				row => row.WR_Levels = 10,
				row => row.WR_LevelsInfo,
				"A Row must not have more than 9 Levels.");
		}

		public void TestSettingTooBigForLocationsFixedWidth_Levels_AlphaLevels()
		{
			TestSettingTooBigForLocationsFixedWidth(
				(row, warehouse) =>
				{
					warehouse.WW_LocationLevelsAlpha = true;
					row.WR_Levels = 26;
				},
				row => row.WR_Levels = 27,
				row => row.WR_LevelsInfo,
				"A Row must not have more than 26 Levels.");
		}

		public void TestSettingTooBigForLocationsFixedWidth_Trays()
		{
			TestSettingTooBigForLocationsFixedWidth(
				(row, warehouse) =>
				{
					row.WR_Trays = 9;
				},
				row => row.WR_Trays = 10,
				row => row.WR_TraysInfo,
				"A Row must not have more than 9 Trays.");
		}

		public void TestSettingTooBigForLocationsFixedWidth_Trays_AlphaTrays()
		{
			TestSettingTooBigForLocationsFixedWidth(
				(row, warehouse) =>
				{
					warehouse.WW_LocationTraysAlpha = true;
					row.WR_Trays = 26;
				},
				row => row.WR_Trays = 27,
				row => row.WR_TraysInfo,
				"A Row must not have more than 26 Trays.");
		}

		public void TestSettingTooBigForLocationsFixedWidth(
			Action<WhsRow, WhsWarehouse> warehouseAndRowConfigSetter,
			Action<WhsRow> rowPropertySetter,
			Func<WhsRow, ZPropertyInfo> getPropertyInfo,
			ZString expectedErrorMessage)
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("WH1", 1, 1, 1);
			var row = Helper.CreateRow(warehouse, "A");
			warehouseAndRowConfigSetter(row, warehouse);
			AssertNoErrors("Precondition", getPropertyInfo(row));

			rowPropertySetter(row);
			AssertHasError(getPropertyInfo(row), expectedErrorMessage);
		}

		#endregion

		#region Implementation

		protected WhsWarehouse Whs
		{
			get { return whs ?? (whs = Helper.CreateWarehouse("MEL")); }
		}

		protected WhsRow Row
		{
			get { return row ?? (row = Whs.Rows.AddNew()); }
		}

		WhsWarehouse whs;
		WhsRow row;

		#endregion
	}
}
