using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsRow))]
	class WhsRowTest : WhsEnvBusinessObjectTestCase
	{
		#region Business Object Overrides

		#region TestWhsRowSaveWithoutWarehouse

		public void TestWhsRowSaveWithoutWarehouse()
		{
			AssertEquals("SetDefaultValues must set WR_Columns to 1", Whs.PK, Row.WR_WW_Whs);
			Row.WR_WW_Whs = ZGuid.Empty;

			AssertHasErrors(Row.WR_WW_WhsInfo);
			AssertEquals("Incorrect Error Message", "Please enter a Warehouse.", Row.WR_WW_WhsInfo.GetErrors().GetFirstMessage());
		}
		#endregion

		#region TestSetDefaultValues

		public void TestSetDefaultValues()
		{
			AssertEquals("SetDefaultValues must set WR_Columns to 1", (short)1, Row.WR_Columns);
			AssertEquals("SetDefaultValues must set WR_Levels to 1", (short)1, Row.WR_Levels);
			AssertEquals("SetDefaultValues must set WR_Trays to 1", (short)1, Row.WR_Trays);
		}

		#endregion

		#region Delete

		#region TestDelete

		public void TestDelete()
		{
			Row = Helper.CreateRowAndGenerateLocations(Whs, "B", 2, 2);
			AssertEquals(4, Row.Locations.Count);

			Row.Delete();
			AssertEquals(true, Row.IsDeleted);
			AssertEquals(0, Row.Locations.Count);
		}

		#endregion

		#region TestDelete_WhenLocationsAreNotLoaded

		public void TestDelete_WhenLocationsAreNotLoaded()
		{
			var row = Helper.CreateRowAndGenerateLocations(Whs, "B", 1, 2);
			AssertEquals("Precondition", 2, row.Locations.Count);
			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var rowLoadedInNewFactory = newFactory.Load<WhsRow>(row.PK); // it doesn't have loacations loaded since it wasn't accessed
			rowLoadedInNewFactory.Delete();
			AssertNoExceptionThrown("No exception should be thrown while saving.", () => newFactory.Save());
			AssertEquals(true, rowLoadedInNewFactory.IsDeleted);
			AssertEquals(0, rowLoadedInNewFactory.Locations.Count);
		}

		#endregion

		#endregion

		#region TestOnLoaded

		public void TestOnLoaded()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestOnSaving

		public void TestOnSaving()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = warehouse.Rows.AddNew();
			row.WR_Name = "A";
			row.WR_Trays = 2;
			AssertEquals("Precondition", 0, row.Locations.Count);

			Factory.Save();
			AssertEquals(2, row.Locations.Count);

			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 1);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 1);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 1);

			foreach (var location in row.Locations)
			{
				location.WLV_PickPathSequence = 0;
				location.WLV_PutawayPathSequence = 0;
				location.WLV_CycleCountPathSequence = 0;
			}

			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 0);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 0);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 0);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 0);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 0);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 0);

			row.WR_PickPathSequence = 10;
			Factory.Save();

			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 1);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 1);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 1);

			row.WR_Levels = 2;
			Factory.Save();
			AssertEquals("Locations is increased to 4.", 4, row.Locations.Count);

			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 1);
			AssertLocation(row, 1, 2, 1, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 2);
			AssertLocation(row, 1, 2, 2, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 2);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 1);
			AssertLocation(row, 1, 2, 1, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 2);
			AssertLocation(row, 1, 2, 2, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 2);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 1);
			AssertLocation(row, 1, 2, 1, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 2);
			AssertLocation(row, 1, 2, 2, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 2);
		}

		public void TestOnSaving_DoesNotReSortPathSequenceIfNotZero()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = warehouse.Rows.AddNew();
			row.WR_Name = "A";
			row.WR_Trays = 2;
			AssertEquals("Precondition", 0, row.Locations.Count);

			Factory.Save();
			AssertEquals(2, row.Locations.Count);

			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 1);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 1);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 1);

			foreach (var location in row.Locations)
			{
				location.WLV_PickPathSequence = 3;
				location.WLV_PutawayPathSequence = 3;
				location.WLV_CycleCountPathSequence = 3;
			}

			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 3);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 3);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 3);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 3);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 3);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 3);

			row.WR_PickPathSequence = 10;
			Factory.Save();

			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 3);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 3);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 3);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 3);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 3);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 3);
		}

		public void TestOnSaving_HighestSequenceIsShortMaxValue()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = warehouse.Rows.AddNew();
			row.WR_Name = "A";
			row.WR_Trays = 2;
			AssertEquals("Precondition", 0, row.Locations.Count);

			Factory.Save();
			AssertEquals(2, row.Locations.Count);

			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 1);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 1);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 1);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 1);

			foreach (var location in row.Locations)
			{
				location.WLV_PickPathSequence = 0;
				location.WLV_PutawayPathSequence = 0;
				location.WLV_CycleCountPathSequence = 0;
			}

			var location1 = row.Locations[0];
			location1.WLV_PickPathSequence = short.MaxValue;
			location1.WLV_PutawayPathSequence = short.MaxValue;
			location1.WLV_CycleCountPathSequence = short.MaxValue;

			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PickPathSequence, short.MaxValue);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 0);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, short.MaxValue);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 0);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, short.MaxValue);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 0);

			row.WR_PickPathSequence = 10;
			Factory.Save();
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PickPathSequence, short.MaxValue);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PickPathSequence, 0);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, short.MaxValue);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_PutawayPathSequence, 1);
			AssertLocation(row, 1, 1, 1, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, short.MaxValue);
			AssertLocation(row, 1, 1, 2, WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence, 0);
		}

		#endregion

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestFetchStrategy

		public void TestFetchStrategy()
		{
			var row = Factory.New<WhsRow>();
			AssertEquals(typeof(WhsRowFetchStrategy), row.FetchStrategy.GetType());
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var row = Factory.New<WhsRow>();
			AssertEquals("Row", row.HumanReadableName);

			row.WR_Name = "r1";
			AssertEquals("Row r1", row.HumanReadableName);
		}

		#endregion

		public void TestRunPreSaveValidation_DoesNotHitDBMultipleTimesForPickFaceOrPick()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 5, 5);
			var locationType = Helper.CreateLocationType("AXA");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var row = newFactory.Load<WhsRow>(data.Whs1.Rows.Single(r => r.WR_Name == "A").PK);
			newFactory.SuspendValidation();
			row.Locations.ForEach(l => l.WLV_LocationStatus = LocationStatus.Codes.Damaged);
			row.Locations.ForEach(l => l.WLV_WLT_LocationType = locationType.PK);
			newFactory.ResumeValidation();

			using (TestConnection.TrackExecutedCommands())
			using (RowFactory.SetCachedTables())
			{
				row.RunPreSaveValidation();
				AssertEquals("Should have only hit the DB once, even though we are updating multiple Locations.", 1, TestConnection.ExecutedCommands.Count(c => c.Contains("WL_PK IN (SELECT Value FROM @Locations)")));
				AssertEquals(0, newFactory.TableSelects.Count(t => t.TableName == WhsPickSchema.Constants.TableName));
				AssertEquals(0, newFactory.TableSelects.Count(t => t.TableName == WhsPickFaceSchema.Constants.TableName));
				AssertEquals(true, row.Locations.All(l => l.WLV_LocationStatus == LocationStatus.Codes.Damaged));
				AssertEquals(true, row.Locations.All(l => l.WLV_WLT_LocationType == locationType.PK));
			}
		}

		public void TestIsFirstPackingStationLocationInThisWarehouse()
		{
			var whs1 = Helper.CreateWarehouse("WHS1");
			var row = Helper.CreateRowAndGenerateLocations(whs1, "L1W1", 5, 5);
			var locationTypePST = Helper.CreateLocationType("PST", LocationClasses.Codes.PST);
			var locationTypeNOR = Helper.CreateLocationType("NOR", LocationClasses.Codes.NOR);

			var location1 = row.Locations[0];
			var location2 = row.Locations[1];
			var location3 = row.Locations[2];
			var location4 = row.Locations[3];
			Factory.Save();

			location1.WLV_WLT_LocationType = locationTypeNOR.PK;
			location1.HasChanges = true;
			AssertEquals(location1.Row.IsFirstPackingStationLocationInThisWarehouse(), false);
			Factory.Save();

			location2.WLV_WLT_LocationType = locationTypePST.PK;
			location2.HasChanges = true;
			AssertEquals(location2.Row.IsFirstPackingStationLocationInThisWarehouse(), true);
			Factory.Save();

			location3.WLV_WLT_LocationType = locationTypePST.PK;
			location3.HasChanges = true;
			AssertEquals(location3.Row.IsFirstPackingStationLocationInThisWarehouse(), false);
			Factory.Save();

			location4.WLV_WLT_LocationType = locationTypePST.PK;
			location4.HasChanges = false;
			AssertEquals(location4.Row.IsFirstPackingStationLocationInThisWarehouse(), false);
			Factory.Save();
		}

		#endregion

		#region Related Entities

		#region TestWR_WW_Whs_DefaultsNextUnusedRowPathSequence

		public void TestWR_WW_Whs_DefaultsNextUnusedRowPathSequence()
		{
			var branch1 = Helper.CreateGlbBranch("BR1");
			var branch2 = Helper.CreateGlbBranch("BR2");
			var client = Helper.CreateClient("CLIENT");
			var whs1 = Helper.CreateWarehouse("WH1", client.MainAddress, branch1, shouldPreGenerateDDL: false);
			var row1 = whs1.Rows.AddNew();
			AssertEquals("Should default to 1 as this is the first row in first warehouse", new ZShort(1), row1.WR_PickPathSequence);

			var whs2 = Helper.CreateWarehouse("WH2", client.MainAddress, branch2, shouldPreGenerateDDL: false);
			var row2 = Factory.New<WhsRow>();
			AssertEquals("Precondition:", ZShort.Zero, row2.WR_PickPathSequence);

			row2.WR_WW_Whs = whs2.PK;
			AssertEquals("Should default to 1 as this is the first row in second warehouse", new ZShort(1), row2.WR_PickPathSequence);

			var row3 = Factory.New<WhsRow>();
			AssertEquals("Precondition:", ZShort.Zero, row3.WR_PickPathSequence);

			row3.WR_WW_Whs = whs2.PK;
			AssertEquals("Should default to 2 as this is the second row in second warehouse", new ZShort(2), row3.WR_PickPathSequence);

			row3.WR_PickPathSequence = 3;
			row3.WR_WW_Whs = whs1.PK;
			AssertEquals("Should change row sequence if warehouse changes", new ZShort(2), row3.WR_PickPathSequence);

			row3.WR_PickPathSequence = 0;
			row3.WR_WW_Whs = whs1.PK;
			AssertEquals("Should not change row sequence if the warehouse did not change", ZShort.Zero, row3.WR_PickPathSequence);
		}

		#endregion

		#region TestLocations

		public void TestLocations()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestInventory

		public void TestInventory()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#endregion

		#region Properties

		#region TestSortPickPathMethodForBinding

		public void TestSortPickPathMethodForBinding()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRow(warehouse, "A");
			AssertEquals("Defaults to blank.", ZString.Empty, row.SortPickPathMethod);

			row.SortPickPathMethod = CodeLists.SortPathMethods.Codes.ColumnThenLevel;
			AssertEquals("Sort Pick Path method has been updated to column then level.", SortPathMethods.Codes.ColumnThenLevel, row.SortPickPathMethod);

			row.SortPickPathMethod = CodeLists.SortPathMethods.Codes.LevelThenColumn;
			AssertEquals("Sort Pick Path method has been updated to level then column.", SortPathMethods.Codes.LevelThenColumn, row.SortPickPathMethod);

			row.SortPickPathMethod = CodeLists.SortPathMethods.Codes.UserDefined;
			AssertEquals("Sort Pick Path method has been updated to user defined.", SortPathMethods.Codes.UserDefined, row.SortPickPathMethod);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var rowInNewFactory = newFactory.Load<WhsRow>(row.PK);
			AssertEquals("Defaults to blank, not persisted.", ZString.Empty, rowInNewFactory.SortPickPathMethod);
		}

		#endregion

		#region TestCycleCountMethodForBinding

		public void TestCycleCountMethodForBinding()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRow(warehouse, "A");
			AssertEquals("Defaults to blank.", ZString.Empty, row.SortCycleCountMethod);

			row.SortCycleCountMethod = CodeLists.SortPathMethods.Codes.ColumnThenLevel;
			AssertEquals("Sort Pick Path method has been updated to column then level.", SortPathMethods.Codes.ColumnThenLevel, row.SortCycleCountMethod);

			row.SortCycleCountMethod = CodeLists.SortPathMethods.Codes.LevelThenColumn;
			AssertEquals("Sort Pick Path method has been updated to level then column.", SortPathMethods.Codes.LevelThenColumn, row.SortCycleCountMethod);

			row.SortCycleCountMethod = CodeLists.SortPathMethods.Codes.UserDefined;
			AssertEquals("Sort Pick Path method has been updated to user defined.", SortPathMethods.Codes.UserDefined, row.SortCycleCountMethod);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var rowInNewFactory = newFactory.Load<WhsRow>(row.PK);
			AssertEquals("Defaults to blank, not persisted.", ZString.Empty, rowInNewFactory.SortCycleCountMethod);
		}

		#endregion

		#region TestSortMethodsForBindingInfo

		public void TestSortMethodsForBindingInfo()
		{
			var row = Factory.New<WhsRow>();
			AssertEquals(3, row.SortPickPathMethodInfo.MaxLength);
			AssertEquals(3, row.SortPutawayPathMethodInfo.MaxLength);
			AssertEquals(3, row.SortCycleCountMethodInfo.MaxLength);
		}

		#endregion

		#region TestSelectedLocations

		public void TestSelectedLocations()
		{
			AssertEquals(0, Row.SelectedLocations.Count);

			TestIRowForm parent = new TestIRowForm();
			List<WhsLocation> locations = new List<WhsLocation>();
			locations.Add(Row.Locations.AddNew());
			locations.Add(Row.Locations.AddNew());
			Row.Locations.AddNew();

			Row.ParentForm = parent;
			parent.SelectedLocations = locations;
			AssertEquals(locations, Row.SelectedLocations);
			AssertEquals(2, Row.SelectedLocations.Count);
		}

		#endregion

		#region TestParentForm

		public void TestParentForm()
		{
			AssertNull(Row.ParentForm);

			TestIRowForm parent = new TestIRowForm();
			Row.ParentForm = parent;
			AssertEquals(parent, Row.ParentForm);
		}

		#endregion

		#region TestApprovedKnownStatus

		public void TestApprovedKnownStatus()
		{
			Row.ApprovedKnownStatus = ZString.Empty;
			AssertEquals(ZString.Empty, Row.ApprovedKnownStatus);

			Row.ApprovedKnownStatus = "TST";
			AssertEquals("TST", Row.ApprovedKnownStatus);

			Row.ApprovedKnownStatus = "TS1";
			AssertEquals("TS1", Row.ApprovedKnownStatus);
		}

		#endregion

		#region TestApprovedKnownStatusInfo

		public void TestApprovedKnownStatusInfo()
		{
			AssertEquals("ApprovedKnownStatus", Row.ApprovedKnownStatusInfo.Name);
			AssertEquals(true, Row.ApprovedKnownStatusInfo.ReadOnly);
		}

		#endregion

		#region TestApprovedKnownStatusInfoUS

		public void TestApprovedKnownStatusInfoUS()
		{
			US.Testing.WhsTestHelperFunctionsEnvUS helper = new US.Testing.WhsTestHelperFunctionsEnvUS(Factory);
			WhsWarehouse whs = helper.CreateWarehouse("WHS");
			Row.WR_WW_Whs = whs.PK;

			AssertEquals("ApprovedKnownStatus", Row.ApprovedKnownStatusInfo.Name);

			helper.SetWarehouseTSAStatus(whs, CodeLists.US.TSAStatus.Codes.Unknown);
			AssertEquals(true, Row.ApprovedKnownStatusInfo.ReadOnly);

			helper.SetWarehouseTSAStatus(whs, CodeLists.US.TSAStatus.Codes.Known);
			AssertEquals(false, Row.ApprovedKnownStatusInfo.ReadOnly);
		}

		#endregion

		#region TestCountryCode

		public void TestCountryCode()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("WHS");
			Row.WR_WW_Whs = whs.PK;

			whs.WarehouseAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			AssertEquals(Enterprise.Core.Constants.CountryCodes.Australia, Row.CountryCode);

			whs.WarehouseAddress.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(Enterprise.Core.Constants.CountryCodes.UnitedStates, Row.CountryCode);

			Row.WR_WW_Whs = ZGuid.Empty;
			AssertEquals(ZString.Empty, Row.CountryCode);
		}

		#endregion

		#region TestMaxWeight

		public void TestMaxWeight()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestMaxWeightUnit

		public void TestMaxWeightUnit()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestMaxCubic

		public void TestMaxCubic()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestMaxCubicUnit

		public void TestMaxCubicUnit()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestMaxHeight

		public void TestMaxHeight()
		{
			var row = Factory.New<WhsRow>();
			AssertEquals(0m, row.MaxHeight);

			row.MaxHeight = 3.14m;
			AssertEquals(3.14m, row.MaxHeight);
		}

		#endregion

		#region TestMaxDepth

		public void TestMaxDepth()
		{
			var row = Factory.New<WhsRow>();
			AssertEquals(0m, row.MaxDepth);

			row.MaxDepth = 3.14m;
			AssertEquals(3.14m, row.MaxDepth);
		}

		#endregion

		#region TestMaxWidth

		public void TestMaxWidth()
		{
			var row = Factory.New<WhsRow>();
			AssertEquals(0m, row.MaxWidth);

			row.MaxWidth = 3.14m;
			AssertEquals(3.14m, row.MaxWidth);
		}

		#endregion

		#region TestMaxDimensionUnit

		public void TestMaxDimensionUnit()
		{
			var row = Factory.New<WhsRow>();
			AssertEquals(string.Empty, row.MaxDimensionUnit);

			row.MaxDimensionUnit = "CM";
			AssertEquals("CM", row.MaxDimensionUnit);

			AssertExceptionThrown<MaxLengthExceededException>(() => row.MaxDimensionUnit = "ABC");
			ErrorReporter.Clear();
		}

		#endregion

		#region TestMaxWeightUnit_Lookup

		public void TestMaxWeightUnit_Lookup()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(WhsRow), nameof(WhsRow.MaxWeightUnit), false, a => a.ListDataSourceMember == "Lookups.WeightUnitTypes");
		}

		#endregion

		#region TestMaxCubicUnit_Lookup

		public void TestMaxCubicUnit_Lookup()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(WhsRow), nameof(WhsRow.MaxCubicUnit), false, a => a.ListDataSourceMember == "Lookups.CubicUnitTypes");
		}

		#endregion

		#region TestMaxDimensionUnit_Lookup

		public void TestMaxDimensionUnit_Lookup()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(WhsRow), nameof(WhsRow.MaxDimensionUnit), false, a => a.ListDataSourceMember == "Lookups.DimensionUnitTypes");
		}

		#endregion

		#region TestPalletFloorSpaces

		public void TestPalletFloorSpaces()
		{
			var row = Factory.New<WhsRow>();
			AssertEquals(new ZByte(0), row.PalletFloorSpaces);

			row.PalletFloorSpaces = 3;
			AssertEquals(new ZByte(3), row.PalletFloorSpaces);
		}

		#endregion

		#region TestPalletStackHeight

		public void TestPalletStackHeight()
		{
			var row = Factory.New<WhsRow>();
			AssertEquals(new ZByte(0), row.PalletStackHeight);

			row.PalletStackHeight = 2;
			AssertEquals(new ZByte(2), row.PalletStackHeight);
		}

		#endregion

		#region TestLocationType

		public void TestLocationType()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestLocationStatus

		public void TestLocationStatus()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestPickMethod

		public void TestPickMethod()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestArea

		public void TestArea()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestLocationTypes

		public void TestLocationTypes()
		{
			AssertNotNull(Row.LocationTypes);
		}

		#endregion

		#region TestLocationStatuses

		public void TestLocationStatuses()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestPickMethods

		public void TestPickMethods()
		{
			AssertNotNull(Row.PickMethods);
		}

		#endregion

		#region TestMaxWeightInfo

		public void TestMaxWeightInfo()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestMaxCubicInfo

		public void TestMaxCubicInfo()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestMaxWeightUnitInfo

		public void TestMaxWeightUnitInfo()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestMaxCubicUnitInfo

		public void TestMaxCubicUnitInfo()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestLocationStatusInfo

		public void TestLocationStatusInfo()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestLocationTypeInfo

		public void TestLocationTypeInfo()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestPickMethodInfo

		public void TestPickMethodInfo()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestAreaInfo

		public void TestAreaInfo()
		{
			Assert("incomplete test", true);
		}

		#endregion

		#region TestRangeMessage

		public void TestRangeMessage()
		{
			AssertEquals("Row A only has 1 Column so the Column must be 1.", Row.RangeMessage("Column", 1, false));
			AssertEquals("Please enter a Location Column between 1 and 5 (Row A contains 5 Columns)", Row.RangeMessage("Column", 5, false));

			AssertEquals("Row A only has 1 Column so the Column must be 0.", Row.RangeMessage("Column", 1, true));
			AssertEquals("Please enter a Location Column between 0 and 4 (Row A contains 5 Columns)", Row.RangeMessage("Column", 5, true));
		}

		#endregion

		#region TestNonPersistentPropertyDoesNotSetHasChangesToTrue

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_MaxQuantity()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.MaxQuantity = 10m);
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_MaxWeight()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.MaxWeight = 10m);
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_MaxCubic()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.MaxCubic = 10m);
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_MaxDepth()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.MaxDepth = 10m);
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_MaxWidth()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.MaxWidth = 10m);
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_MaxHeight()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.MaxHeight = 10m);
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_MaxWeightUnit()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.MaxWeightUnit = "KG");
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_MaxCubicUnit()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.MaxCubicUnit = "M3");
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_MaxDimensionUnit()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.MaxDimensionUnit = "M");
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_PalletFloorSpaces()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.PalletFloorSpaces = 2);
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_PalletStackHeight()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.PalletStackHeight = 2);
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_LocationType()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.LocationType = ZGuid.BrettsGuid);
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_LocationStatus()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.LocationStatus = "LS");
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_PickMethod()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.PickMethod = "PM");
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_PickingArea_MassUpdate()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.PickingArea_MassUpdate = ZGuid.BrettsGuid);
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_PutawayArea_MassUpdate()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.PutawayArea_MassUpdate = ZGuid.BrettsGuid);
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_ApprovedKnownStatus()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.ApprovedKnownStatus = "NO");
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_MaximumTouchCount()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.MaximumTouchCount = 1);
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_SortCycleCountMethodForBinding()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.SortCycleCountMethod = SortPathMethods.Codes.ColumnThenLevel);
		}

		public void TestNonPersistentPropertyDoesNotSetHasChangesToTrue_SortPickPathMethodForBinding()
		{
			TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore((row) => row.SortPickPathMethod = SortPathMethods.Codes.ColumnThenLevel);
		}

		void TestNonPersistentPropertyDoesNotSetHasChangesToTrueCore(Action<WhsRow> nonPersistentPropertySetter)
		{
			var row = Factory.NewWithValidTestData<WhsRow>();
			Factory.Save();

			AssertEquals("Precondition", false, row.HasChanges);
			nonPersistentPropertySetter(row);
			AssertEquals(false, row.HasChanges);
		}

		#endregion

		#endregion

		#region WhsRowInternals Members

		[ExpectNoExceptions]
		public void TestGenerateLocationsHandlesNullWarehouse()
		{
			((WhsRowInternals)Row).GenerateLocations();
		}

		public void TestGenerateLocations()
		{
			BaseGenerate(4, 3, 2);
			BaseAssert();
		}

		public void TestGenerateLocationsIncreasing()
		{
			BaseGenerate(4, 3, 2);
			BaseGenerate(6, 5, 4);
			BaseAssert();
		}

		public void TestGenerateLocationsReducing()
		{
			BaseGenerate(4, 3, 2);
			BaseGenerate(3, 2, 1);
			BaseAssert();

			for (short c = 1; c <= 4; c++)
			{
				for (short l = 1; l <= 3; l++)
				{
					for (short t = 1; t <= 2; t++)
					{
						if (c == 4 || l == 3 || t == 2)
						{
							AssertEquals(0, (Factory.Load(typeof(WhsLocation), GetLocationFilter(Row, c, l, t))).Length);
						}
					}
				}
			}
		}

		public void TestGenerateLocationsIncreasingAndReducing()
		{
			BaseGenerate(4, 3, 2);
			BaseGenerate(2, 3, 4);
			BaseAssert();

			for (short c = 3; c <= 4; c++)
			{
				for (short l = 1; l <= 3; l++)
				{
					for (short t = 1; t <= 2; t++)
					{
						AssertEquals(0, (Factory.Load(typeof(WhsLocation), GetLocationFilter(Row, c, l, t))).Length);
					}
				}
			}
		}

		public void TestGenerateLocations_DefaultsAreConfigured()
		{
			BaseGenerate(2, 1, 1);
			Assert(Row.Locations.All(l => l.PickingArea.WA_Name == "DEFAULT"));
			Assert(Row.Locations.All(l => l.PutawayArea.WA_Name == "DEFAULT"));
		}

		public void TestGenerateLocations_DefaultsArePartiallyConfigured()
		{
			var whs = Helper.CreateWarehouse("whs");
			Factory.Save();
			var area1 = Helper.CreateArea(whs, "area1", AreaTypes.Codes.DockDoor, true, true);
			area1.WA_IsDefaultPutawayArea = true;
			area1.WA_IsDefaultPickArea = true;
			var area2 = Helper.CreateArea(whs, "area2", AreaTypes.Codes.FreeStore, true, true);
			var row = Helper.CreateRow(whs, "rw1");
			Factory.Save();

			GenerateLocations(Factory, row, 2, 2, 2);

			Assert(row.Locations.All(l => l.PickingArea.WA_Name == "area1"));
			Assert(row.Locations.All(l => l.PutawayArea.WA_Name == "area1"));
		}

		public void TestStoreCurrentLocationSettingAsOriginal_Columns_Add()
		{
			// Columns = 3 , Levels = 3 , Trays = 3
			StoreCurrentLocationSettingAsOriginalCore((row) => { row.WR_Columns++; }, 36, 36);
		}

		public void TestStoreCurrentLocationSettingAsOriginal_Columns_Remove()
		{
			StoreCurrentLocationSettingAsOriginalCore((row) => { row.WR_Columns--; }, 18, 9);
		}

		public void TestStoreCurrentLocationSettingAsOriginal_Levels_Add()
		{
			StoreCurrentLocationSettingAsOriginalCore((row) => { row.WR_Levels++; }, 36, 36);
		}

		public void TestStoreCurrentLocationSettingAsOriginal_Levels_Remove()
		{
			StoreCurrentLocationSettingAsOriginalCore((row) => { row.WR_Levels--; }, 18, 9);
		}

		public void TestStoreCurrentLocationSettingAsOriginal_Trays_Add()
		{
			StoreCurrentLocationSettingAsOriginalCore((row) => { row.WR_Trays++; }, 36, 36);
		}

		public void TestStoreCurrentLocationSettingAsOriginal_Trays_Remove()
		{
			StoreCurrentLocationSettingAsOriginalCore((row) => { row.WR_Trays--; }, 18, 9);
		}

		void StoreCurrentLocationSettingAsOriginalCore(Action<WhsRow> changeGenerateLocationDataAction, int countWithoutStoreSetting, int countWithStoreSetting)
		{
			var whs = Helper.CreateWarehouse("Warehouse test");
			var row = Helper.CreateRow(Whs, "ABC");
			row.WR_Columns = 3;
			row.WR_Levels = 3;
			row.WR_Trays = 3;

			var locationCount = row.WR_Columns * row.WR_Levels * row.WR_Trays;
			((WhsRowInternals)row).GenerateLocations();
			AssertEquals(locationCount, row.Locations.Count(l => !l.IsDeleted));

			changeGenerateLocationDataAction(row);
			var newLocationCount = row.WR_Columns * row.WR_Levels * row.WR_Trays;
			((WhsRowInternals)row).GenerateLocations();
			AssertEquals(countWithoutStoreSetting, row.Locations.Count(l => !l.IsDeleted));

			changeGenerateLocationDataAction(row);
			row.StorePreeditLocationSetting();
			newLocationCount = row.WR_Columns * row.WR_Levels * row.WR_Trays;
			((WhsRowInternals)row).GenerateLocations();
			AssertEquals(countWithStoreSetting, row.Locations.Count(l => !l.IsDeleted));
		}

		#endregion

		#region Methods

		#region TestUpdateLocationValues

		public void TestUpdateLocationValues_MultipleLocations()
		{
			var location1 = Factory.New<WhsLocation>();
			var location2 = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();
			location1.WLV_WR = row1.PK;
			location2.WLV_WR = row1.PK;

			row1.MaxQuantity = 10m;
			row1.LocationStatus = LocationStatus.Codes.Damaged;
			location1.WLV_MaxQuantity = 0m;
			location2.WLV_MaxQuantity = 0m;
			location1.WLV_LocationStatus = LocationStatus.Codes.Normal;
			location2.WLV_LocationStatus = LocationStatus.Codes.Normal;
			row1.UpdateLocationValues(new[] { location1, location2 });
			AssertEquals(10m, location1.WLV_MaxQuantity);
			AssertEquals(10m, location2.WLV_MaxQuantity);
			AssertEquals(LocationStatus.Codes.Damaged, location1.WLV_LocationStatus);
			AssertEquals(LocationStatus.Codes.Damaged, location2.WLV_LocationStatus);
		}

		public void TestUpdateLocationValues_MultipleLocations_DoesNotHitDBMultipleTimesForPickFaceOrPick()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory, 5, 5);
			var locationType = Helper.CreateLocationType("AXA");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var row = newFactory.Load<WhsRow>(data.Whs1.Rows.Single(r => r.WR_Name == "A").PK);
			row.LocationStatus = LocationStatus.Codes.Damaged;
			row.LocationType = locationType.PK;

			using (TestConnection.TrackExecutedCommands())
			using (RowFactory.SetCachedTables())
			{
				row.UpdateLocationValues(row.Locations.ToArray());
				AssertEquals("Should have only hit the DB once, even though we are updating multiple Locations.", 1, TestConnection.ExecutedCommands.Count(c => c.Contains("WL_PK IN (SELECT Value FROM @Locations)")));
				AssertEquals(0, newFactory.TableSelects.Count(t => t.TableName == WhsPickSchema.Constants.TableName));
				AssertEquals(0, newFactory.TableSelects.Count(t => t.TableName == WhsPickFaceSchema.Constants.TableName));
				AssertEquals(true, row.Locations.All(l => l.WLV_LocationStatus == LocationStatus.Codes.Damaged));
				AssertEquals(true, row.Locations.All(l => l.WLV_WLT_LocationType == locationType.PK));
			}
		}

		public void TestUpdateLocationValues_MaxQuantity()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.MaxQuantity = 10m;
			location.WLV_MaxQuantity = 0m;
			row1.UpdateLocationValues([location]);
			AssertEquals(10m, location.WLV_MaxQuantity);

			row1.MaxQuantity = 0m;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row MaxQuantity is 0.", 10m, location.WLV_MaxQuantity);
		}

		public void TestUpdateLocationValues_MaxWeight()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.MaxWeight = 10m;
			location.WLV_MaxWeight = 0m;
			row1.UpdateLocationValues([location]);
			AssertEquals(10m, location.WLV_MaxWeight);

			row1.MaxWeight = 0m;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row MaxWeight is 0.", 10m, location.WLV_MaxWeight);
		}

		public void TestUpdateLocationValues_MaxCubic()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.MaxCubic = 15m;
			location.WLV_MaxCubic = 0m;
			row1.UpdateLocationValues([location]);
			AssertEquals(15m, location.WLV_MaxCubic);

			row1.MaxCubic = 0m;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row MaxCubic is 0.", 15m, location.WLV_MaxCubic);
		}

		public void TestUpdateLocationValues_MaxWeightUnit()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.MaxWeightUnit = "KG";
			location.WLV_MaxWeightUnit = ZString.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("KG", location.WLV_MaxWeightUnit);

			row1.MaxWeightUnit = ZString.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row MaxWeightUnit is empty.", "KG", location.WLV_MaxWeightUnit);
		}

		public void TestUpdateLocationValues_MaxCubicUnit()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.MaxCubicUnit = "M3";
			location.WLV_MaxCubicUnit = ZString.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("M3", location.WLV_MaxCubicUnit);

			row1.MaxCubicUnit = ZString.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row MaxCubicUnit is empty.", "M3", location.WLV_MaxCubicUnit);
		}

		public void TestUpdateLocationValues_MaxHeight()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.MaxHeight = 10m;
			location.WLV_MaxHeight = 0m;
			row1.UpdateLocationValues([location]);
			AssertEquals(10m, location.WLV_MaxHeight);

			row1.MaxHeight = 0m;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row MaxHeight is 0.", 10m, location.WLV_MaxHeight);
		}

		public void TestUpdateLocationValues_MaxDepth()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.MaxDepth = 10m;
			location.WLV_MaxDepth = 0m;
			row1.UpdateLocationValues([location]);
			AssertEquals(10m, location.WLV_MaxDepth);

			row1.MaxDepth = 0m;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row MaxDepth is 0.", 10m, location.WLV_MaxDepth);
		}

		public void TestUpdateLocationValues_MaxWidth()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.MaxWidth = 10m;
			location.WLV_MaxWidth = 0m;
			row1.UpdateLocationValues([location]);
			AssertEquals(10m, location.WLV_MaxWidth);

			row1.MaxWidth = 0m;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row MaxWidth is 0.", 10m, location.WLV_MaxWidth);
		}

		public void TestUpdateLocationValues_MaxDimensionUnit()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.MaxDimensionUnit = "M";
			location.WLV_MaxDimensionUnit = ZString.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("M", location.WLV_MaxDimensionUnit);

			row1.MaxDimensionUnit = ZString.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row MaxDimensionUnit is empty.", "M", location.WLV_MaxDimensionUnit);
		}

		public void TestUpdateLocationValues_PalletFloorSpaces()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.PalletFloorSpaces = 1;
			location.WLV_PalletFloorSpaces = 0;
			row1.UpdateLocationValues([location]);
			AssertEquals(new ZByte(1), location.WLV_PalletFloorSpaces);

			row1.PalletFloorSpaces = 0;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row PalletFloorSpaces is empty.", new ZByte(1), location.WLV_PalletFloorSpaces);
		}

		public void TestUpdateLocationValues_PalletStackHeight()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.PalletStackHeight = 1;
			location.WLV_PalletStackHeight = 0;
			row1.UpdateLocationValues([location]);
			AssertEquals(new ZByte(1), location.WLV_PalletStackHeight);

			row1.PalletStackHeight = 0;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row PalletStackHeight is empty.", new ZByte(1), location.WLV_PalletStackHeight);
		}

		public void TestUpdateLocationValues_LocationType()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			var locationType1 = Helper.CreateLocationType("LOC");
			row1.LocationType = locationType1.PK;
			location.WLV_WLT_LocationType = ZGuid.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("LOC", location.LocationType.WLT_Code);

			row1.LocationType = ZGuid.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row LocationType is empty.", "LOC", location.LocationType.WLT_Code);
		}

		public void TestUpdateLocationValues_LocationStatus()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.LocationStatus = "LS";
			location.WLV_LocationStatus = ZString.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("LS", location.WLV_LocationStatus);

			row1.LocationStatus = ZString.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row LocationStatus is empty.", "LS", location.WLV_LocationStatus);
		}

		public void TestUpdateLocationValues_PickMethod()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.PickMethod = "LP";
			location.WLV_PickMethod = ZString.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("LP", location.WLV_PickMethod);

			row1.PickMethod = ZString.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row PickMethod is empty.", "LP", location.WLV_PickMethod);
		}

		public void TestUpdateLocationValues_PickingArea_MassUpdate()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.PickingArea_MassUpdate = ZGuid.Empty;
			var pickingAreaPK = ZGuid.NewZGuid();
			location.WLV_WA_PickingArea = pickingAreaPK;
			row1.UpdateLocationValues([location]);
			AssertEquals("Picking area must not be cleared.", pickingAreaPK, location.WLV_WA_PickingArea);
			AssertNotEquals(nameof(location.WLV_WA_PutawayArea) + " must not set putaway area PK", pickingAreaPK, location.WLV_WA_PutawayArea);

			var newPickingAreaPK = ZGuid.NewZGuid();
			row1.PickingArea_MassUpdate = newPickingAreaPK;
			row1.UpdateLocationValues([location]);
			AssertEquals("Picking area must be changed to the value set in PickingArea_MassUpdate.", newPickingAreaPK, location.WLV_WA_PickingArea);
			AssertNotEquals(nameof(location.WLV_WA_PutawayArea) + " must not set putaway area PK", newPickingAreaPK, location.WLV_WA_PutawayArea);
		}

		public void TestUpdateLocationValues_PutawayArea_MassUpdate()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.PutawayArea_MassUpdate = ZGuid.Empty;
			var putawayAreaPK = ZGuid.NewZGuid();
			location.WLV_WA_PutawayArea = putawayAreaPK;
			row1.UpdateLocationValues([location]);
			AssertEquals("Putaway area PK must not be cleared.", putawayAreaPK, location.WLV_WA_PutawayArea);
			AssertNotEquals(nameof(location.WLV_WA_PutawayArea) + " must not set picking area PK.", putawayAreaPK, location.WLV_WA_PickingArea);

			var newPutawayAreaPK = ZGuid.NewZGuid();
			row1.PutawayArea_MassUpdate = newPutawayAreaPK;
			row1.UpdateLocationValues([location]);
			AssertEquals("Putaway area must be changed to the value set in PutawayArea_MassUpdate.", newPutawayAreaPK, location.WLV_WA_PutawayArea);
			AssertNotEquals(nameof(location.WLV_WA_PutawayArea) + " must not set picking area PK.", newPutawayAreaPK, location.WLV_WA_PickingArea);
		}

		public void TestUpdateLocationValues_ApprovedKnownStatus()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.ApprovedKnownStatus = "NO";
			location.WLV_ApprovedKnownLocation = ZString.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("NO", location.WLV_ApprovedKnownLocation);

			row1.ApprovedKnownStatus = ZString.Empty;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row ApprovedKnownStatus is empty.", "NO", location.WLV_ApprovedKnownLocation);
		}

		public void TestUpdateLocationValues_TouchFrequency()
		{
			var location = Factory.New<WhsLocation>();
			var row1 = Factory.New<WhsRow>();

			row1.MaximumTouchCount = 1;
			location.WLV_MaximumPickCountBeforeAutomatedStocktake = 0;
			row1.UpdateLocationValues([location]);
			AssertEquals(1, location.WLV_MaximumPickCountBeforeAutomatedStocktake);

			row1.MaximumTouchCount = 0;
			row1.UpdateLocationValues([location]);
			AssertEquals("Value not overridden as row MaximumTouchCount is 0.", 1, location.WLV_MaximumPickCountBeforeAutomatedStocktake);
		}

		public void TestUpdateLocationValues_GenerateCheckDigits_SmallRows_SameLocationsSameWarehouseAreDeterministicWithAlteredSort()
		{
			TestUpdateLocationValues_GenerateCheckDigits_SmallRows_SameLocationsSameWarehouseAreDeterministicCore(true);
		}

		public void TestUpdateLocationValues_GenerateCheckDigits_SmallRows_SameLocationsSameWarehouseAreDeterministicWithoutAlteredSort()
		{
			TestUpdateLocationValues_GenerateCheckDigits_SmallRows_SameLocationsSameWarehouseAreDeterministicCore(false);
		}

		void TestUpdateLocationValues_GenerateCheckDigits_SmallRows_SameLocationsSameWarehouseAreDeterministicCore(bool withAlteredSort)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row1 = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);
			var row2 = Helper.CreateRowAndGenerateLocations(warehouse, "B", 2, 2, 2);
			var row3 = Helper.CreateRowAndGenerateLocations(warehouse, "C", 2, 2, 2);

			var allOriginalLocations = new[] { row1, row2, row3 }.SelectMany(r => r.Locations).ToList();

			foreach (var row in new[] { row1, row2, row3 })
			{
				row.GenerateCheckDigit = true;
				row.UpdateLocationValues(row.Locations.ToArray());
			}

			var row1Clone = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);
			var row2Clone = Helper.CreateRowAndGenerateLocations(warehouse, "B", 2, 2, 2);
			var row3Clone = Helper.CreateRowAndGenerateLocations(warehouse, "C", 2, 2, 2);

			var allClonedLocations = new[] { row1Clone, row2Clone, row3Clone }.SelectMany(r => r.Locations).ToList();

			foreach (var row in new[] { row1Clone, row2Clone, row3Clone })
			{
				if (withAlteredSort)
				{
					row.Locations.ApplySort(WhsLocationViewSchema.Constants.WLV_Tray, ListSortDirection.Ascending);
				}
				row.GenerateCheckDigit = true;
				row.UpdateLocationValues(row.Locations.ToArray());
			}

			AssertEquals(allOriginalLocations.Count, allClonedLocations.Count);
			for (var i = 0; i < allOriginalLocations.Count; i++)
			{
				AssertEquals($"Check digits generated for location {allOriginalLocations[i].WLV_LocationString} do not match: {allOriginalLocations[i].WLV_CheckDigit} != {allClonedLocations[i].WLV_CheckDigit}", allOriginalLocations[i].WLV_CheckDigit, allClonedLocations[i].WLV_CheckDigit);
			}
		}

		public void TestUpdateLocationValues_GenerateCheckDigits_SmallRows_GeneratedCheckDigitsAreUnique()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);

			row.GenerateCheckDigit = true;

			const int numTimesToIncrementColumnAndLevel = 10;
			for (var i = 0; i < numTimesToIncrementColumnAndLevel; i++)
			{
				row.UpdateLocationValues(row.Locations.ToArray());

				foreach (var location in row.Locations)
				{
					// For next round increment column and level to provide some more coverage for check digit generation
					location.WLV_Column += 2;
					location.WLV_Level += 2;
				}

				var checkDigits = row.Locations.ToArray().Select(l => l.WLV_CheckDigit).ToList();
				AssertEquals(checkDigits.Count, checkDigits.Distinct().Count());
			}
		}

		public void TestUpdateLocationValues_GenerateCheckDigits_DuplicatesHaveWarningsAfterUpdateLocationValues()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 5, 5, 5);

			row.GenerateCheckDigit = true;
			row.UpdateLocationValues(row.Locations.ToArray());

			var checkDigitGroups = row.Locations
				.GroupBy(location => location.WLV_CheckDigit)
				.ToDictionary(group => group.Key, group => group.ToList());

			foreach (var checkDigitGroup in checkDigitGroups.Values)
			{
				if (checkDigitGroup.Count > 1)
				{
					foreach (var location in checkDigitGroup)
					{
						AssertHasWarnings("WLV_CheckDigit added is not unique, warning should have been raised but was not.", location.WLV_CheckDigitInfo);
					}
				}
				else
				{
					foreach (var location in checkDigitGroup)
					{
						AssertNoWarnings("WLV_CheckDigit added is unique, warning should not have been raised but was.", location.WLV_CheckDigitInfo);
					}
				}
			}
		}

		public void TestUpdateLocationValues_GenerateCheckDigits_AllColumnLevelTray_GeneratedCheckDigitsAreValid()
		{
			const int numColumns = 11;
			const int numLevels = 11;
			const int numTrays = 11;

			var globalFrequencies = new int[100];
			var warehouse = Helper.CreateWarehouse("WHS");

			foreach (var rowName in new[] { "A", "K", "P", "U", "Z", "1", "4", "7", "9" })
			{
				var frequencies = new int[100];
				var row = Helper.CreateRowAndGenerateLocations(warehouse, rowName, numColumns, numLevels, numTrays);

				using (row.GetValidationSuspender())
				using (((IBusinessObjectCollection)row.Locations).SuspendListChanged())
				{
					row.GenerateCheckDigit = true;
					row.UpdateLocationValues(row.Locations.ToArray());
				}

				foreach (var location in row.Locations)
				{
					AssertGreaterThanOrEqualTo("Check digit generated was invalid, below 0.", location.WLV_CheckDigit, 0);
					AssertLessThan("Check digit generated was invalid, above 100.", location.WLV_CheckDigit, 100);

					frequencies[location.WLV_CheckDigit]++;
					globalFrequencies[location.WLV_CheckDigit]++;
				}

				AssertEquals("All check digits must be used in a row of this size.", 100, frequencies.Count(f => f > 0));
				AssertUniformDistribution(frequencies, 20m);

				AssertDifferences(row.Locations); // Column, Level, Tray
				AssertDifferences(row.Locations.OrderBy(l => l.WLV_Level).ThenBy(l => l.WLV_Column).ThenBy(l => l.WLV_Tray));
				AssertDifferences(row.Locations.OrderBy(l => l.WLV_Column).ThenBy(l => l.WLV_Tray).ThenBy(l => l.WLV_Level));
			}

			AssertUniformDistribution(globalFrequencies, 5m);

			static void AssertUniformDistribution(int[] frequencies, decimal absoluteTolerancePercentage)
			{
				var totalFrequencies = frequencies.Sum();
				var expectedFrequency = totalFrequencies / frequencies.Length;

				var frequencyDifferentToExpected = 0;
				foreach (var frequency in frequencies)
				{
					var percentageDifference = Math.Abs((frequency - expectedFrequency) / (decimal)expectedFrequency) * 100m;
					AssertLessThanOrEqualTo($"Frequency should be within tolerance of {absoluteTolerancePercentage}%.", percentageDifference, absoluteTolerancePercentage);

					if (frequency != expectedFrequency)
					{
						frequencyDifferentToExpected++;
					}
				}

				AssertLessThanOrEqualTo("Count of frequencies different to the expected value exceeded 10%.", frequencyDifferentToExpected, (int)(totalFrequencies * 0.10m));
			}

			static void AssertDifferences(IEnumerable<WhsLocation> locations)
			{
				var checkDigits = locations.Select(location => location.WLV_CheckDigit).ToArray();
				var differences = checkDigits.Skip(1).Select((current, index) => current - checkDigits[index]).Select(diff => diff < 0 ? 100 + diff : diff).ToArray();

				AssertGreaterThanOrEqualTo("Check Digits should not follow a simple linear pattern.", differences.Distinct().Count(), 25);
				AssertEquals("No subsequent locations should have the same check digit.", 0, differences.Count(d => d == 0));
			}
		}

		public void TestUpdateLocationValues_GenerateCheckDigits_DoesNotGenerateCheckDigitsIfDisabled()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2, 2);

			row.GenerateCheckDigit = false;
			row.UpdateLocationValues(row.Locations.ToArray());

			foreach (var location in row.Locations)
			{
				AssertEquals((byte)255, location.WLV_CheckDigit);
			}
		}

		public void TestUpdateLocationValues_GenerateCheckDigits_DoesNotGenerateCheckDigitForDDL()
		{
			TestUpdateLocationValues_GenerateCheckDigits_DoesNotGenerateCheckDigitForLocationType(LocationClasses.Codes.DDL);
		}

		public void TestUpdateLocationValues_GenerateCheckDigits_DoesNotGenerateCheckDigitForPST()
		{
			TestUpdateLocationValues_GenerateCheckDigits_DoesNotGenerateCheckDigitForLocationType(LocationClasses.Codes.PST);
		}

		public void TestUpdateLocationValues_GenerateCheckDigits_DoesNotGenerateCheckDigitForCON()
		{
			TestUpdateLocationValues_GenerateCheckDigits_DoesNotGenerateCheckDigitForLocationType(LocationClasses.Codes.CON);
		}

		void TestUpdateLocationValues_GenerateCheckDigits_DoesNotGenerateCheckDigitForLocationType(ZString locationClass)
		{
			var location = Factory.New<WhsLocation>();
			var blankRow = Factory.New<WhsRow>();
			blankRow.GenerateCheckDigit = true;
			var locationType = Helper.CreateLocationType("BBB", locationClass);
			location.WLV_WLT_LocationType = locationType.PK;
			blankRow.UpdateLocationValues([location]);
			AssertEquals($"Check digit should not be generated for {locationClass} locations.", (byte)255, location.WLV_CheckDigit);
		}

		#endregion

		#region TestUpdatePathSequenceOnLocations_ColumnThenLevel

		public void TestUpdatePathSequenceOnLocations_ColumnThenLevel_WLV_PickPathSequence()
		{
			TestUpdatePathSequenceOnLocations_ColumnThenLevelCore(WhsLocationViewSchema.Constants.WLV_PickPathSequence);
		}

		public void TestUpdatePathSequenceOnLocations_ColumnThenLevel_WLV_CycleCountPathSequence()
		{
			TestUpdatePathSequenceOnLocations_ColumnThenLevelCore(WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence);
		}

		public void TestUpdatePathSequenceOnLocations_ColumnThenLevelCore(ZString pathPropertyName)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 3, 2);

			foreach (var location in row.Locations)
			{
				location.FindPropertyInfo(pathPropertyName).Value = ZInt.Zero;
			}

			if (pathPropertyName == WhsLocationViewSchema.Constants.WLV_PickPathSequence)
			{
				row.SortPickPathMethod = SortPathMethods.Codes.ColumnThenLevel;
			}
			else if (pathPropertyName == WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence)
			{
				row.SortCycleCountMethod = SortPathMethods.Codes.ColumnThenLevel;
			}

			row.UpdatePathSequenceOnLocations();
			AssertLocation(row, 1, 1, 1, pathPropertyName, 1);
			AssertLocation(row, 1, 1, 2, pathPropertyName, 2);
			AssertLocation(row, 1, 2, 1, pathPropertyName, 3);
			AssertLocation(row, 1, 2, 2, pathPropertyName, 4);
			AssertLocation(row, 1, 3, 1, pathPropertyName, 5);
			AssertLocation(row, 1, 3, 2, pathPropertyName, 6);
			AssertLocation(row, 2, 1, 1, pathPropertyName, 7);
			AssertLocation(row, 2, 1, 2, pathPropertyName, 8);
			AssertLocation(row, 2, 2, 1, pathPropertyName, 9);
			AssertLocation(row, 2, 2, 2, pathPropertyName, 10);
			AssertLocation(row, 2, 3, 1, pathPropertyName, 11);
			AssertLocation(row, 2, 3, 2, pathPropertyName, 12);
		}

		#endregion

		#region TestUpdatePathSequenceOnLocations_LevelThenColumn

		public void TestUpdatePathSequenceOnLocations_LevelThenColumn_WLV_PickPathSequence()
		{
			TestUpdatePathSequenceOnLocations_LevelThenColumnCore(WhsLocationViewSchema.Constants.WLV_PickPathSequence);
		}

		public void TestUpdatePathSequenceOnLocations_LevelThenColumn_WLV_CycleCountPathSequence()
		{
			TestUpdatePathSequenceOnLocations_LevelThenColumnCore(WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence);
		}

		public void TestUpdatePathSequenceOnLocations_LevelThenColumnCore(ZString pathPropertyName)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 3, 2);

			foreach (var location in row.Locations)
			{
				location.FindPropertyInfo(pathPropertyName).Value = ZInt.Zero;
			}

			if (pathPropertyName == WhsLocationViewSchema.Constants.WLV_PickPathSequence)
			{
				row.SortPickPathMethod = SortPathMethods.Codes.LevelThenColumn;
			}
			else if (pathPropertyName == WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence)
			{
				row.SortCycleCountMethod = SortPathMethods.Codes.LevelThenColumn;
			}

			row.UpdatePathSequenceOnLocations();
			AssertLocation(row, 1, 1, 1, pathPropertyName, 1);
			AssertLocation(row, 1, 1, 2, pathPropertyName, 2);
			AssertLocation(row, 1, 2, 1, pathPropertyName, 5);
			AssertLocation(row, 1, 2, 2, pathPropertyName, 6);
			AssertLocation(row, 1, 3, 1, pathPropertyName, 9);
			AssertLocation(row, 1, 3, 2, pathPropertyName, 10);
			AssertLocation(row, 2, 1, 1, pathPropertyName, 3);
			AssertLocation(row, 2, 1, 2, pathPropertyName, 4);
			AssertLocation(row, 2, 2, 1, pathPropertyName, 7);
			AssertLocation(row, 2, 2, 2, pathPropertyName, 8);
			AssertLocation(row, 2, 3, 1, pathPropertyName, 11);
			AssertLocation(row, 2, 3, 2, pathPropertyName, 12);
		}

		#endregion

		#region TestNewLocationsAreCreatedWithPickingAndPutawayArea

		public void TestNewLocationsAreCreatedWithPickingAndPutawayArea_AllFreeStoreAreas()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.Areas.RemoveAllFromRelationship();
			var row1 = Helper.CreateRow(warehouse, "R");
			var pickingArea = Helper.CreateArea(warehouse, "A", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: false);
			var putawayArea = Helper.CreateArea(warehouse, "B", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: false, isPutawayArea: true);
			Helper.CreateArea(warehouse, "C", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: true);

			((WhsRowInternals)row1).GenerateLocations();

			var location = row1.Locations.Single();
			AssertEquals(pickingArea, location.PickingArea);
			AssertEquals(putawayArea, location.PutawayArea);
		}

		public void TestNewLocationsAreCreatedWithPickingAndPutawayArea_PickingOnlyAreaIsDockDoor()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.Areas.RemoveAllFromRelationship();
			var row = Helper.CreateRow(warehouse, "R");
			var pickingArea = Helper.CreateArea(warehouse, "A", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: false);
			var putawayArea = Helper.CreateArea(warehouse, "B", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: false, isPutawayArea: true);
			var bothAreaType = Helper.CreateArea(warehouse, "C", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: true);

			pickingArea.WA_AreaType = AreaTypes.Codes.DockDoor;
			((WhsRowInternals)row).GenerateLocations();

			var location = row.Locations.Single();
			AssertEquals(bothAreaType, location.PickingArea);
			AssertEquals(putawayArea, location.PutawayArea);
		}

		public void TestNewLocationsAreCreatedWithPickingAndPutawayArea_PutawayOnlyAreaIsDockDoor()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.Areas.RemoveAllFromRelationship();
			var row = Helper.CreateRow(warehouse, "R");
			var pickingArea = Helper.CreateArea(warehouse, "A", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: false);
			var putawayArea = Helper.CreateArea(warehouse, "B", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: false, isPutawayArea: true);
			var bothAreaType = Helper.CreateArea(warehouse, "C", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: true);

			putawayArea.WA_AreaType = AreaTypes.Codes.DockDoor;
			((WhsRowInternals)row).GenerateLocations();
			var location = row.Locations.Single();
			AssertEquals(pickingArea, location.PickingArea);
			AssertEquals(bothAreaType, location.PutawayArea);
		}

		public void TestNewLocationsAreCreatedWithPickingAndPutawayArea_AllDockDoorAreas()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.Areas.RemoveAllFromRelationship();
			var row = Helper.CreateRow(warehouse, "R");
			var pickingArea = Helper.CreateArea(warehouse, "A", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: false);
			var putawayArea = Helper.CreateArea(warehouse, "B", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: false, isPutawayArea: true);
			var bothAreaType = Helper.CreateArea(warehouse, "C", CodeLists.AreaTypes.Codes.FreeStore, isPickingArea: true, isPutawayArea: true);

			pickingArea.WA_AreaType = AreaTypes.Codes.DockDoor;
			putawayArea.WA_AreaType = AreaTypes.Codes.DockDoor;
			bothAreaType.WA_AreaType = AreaTypes.Codes.DockDoor;
			((WhsRowInternals)row).GenerateLocations();
			var location = row.Locations.Single();
			AssertEquals(pickingArea, location.PickingArea);
			AssertEquals(putawayArea, location.PutawayArea);
		}

		#endregion

		#region TestUpdatePickSequenceOnLocations_UserDefined

		public void TestUpdatePathSequenceOnLocations_UserDefined_WLV_PickPathSequence()
		{
			TestUpdatePathSequenceOnLocations_UserDefinedCore(WhsLocationViewSchema.Constants.WLV_PickPathSequence);
		}

		public void TestUpdatePathSequenceOnLocations_UserDefined_WLV_CycleCountPathSequence()
		{
			TestUpdatePathSequenceOnLocations_UserDefinedCore(WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence);
		}

		public void TestUpdatePathSequenceOnLocations_UserDefinedCore(ZString pathPropertyName)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 3, 2);

			foreach (var location in row.Locations)
			{
				location.FindPropertyInfo(pathPropertyName).Value = ZInt.Zero;
			}

			if (pathPropertyName == WhsLocationViewSchema.Constants.WLV_PickPathSequence)
			{
				row.SortPickPathMethod = SortPathMethods.Codes.UserDefined;
			}
			else if (pathPropertyName == WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence)
			{
				row.SortCycleCountMethod = SortPathMethods.Codes.UserDefined;
			}

			row.UpdatePathSequenceOnLocations();
			foreach (var location in row.Locations)
			{
				AssertEquals(ZInt.Zero, location.FindPropertyInfo(pathPropertyName).Value);
			}
		}

		#endregion

		#region TestUpdatePathSequenceOnLocations_SetsSequenceToZeroIfProposedPathSequenceIsGreaterThanMaxShortValue

		public void TestUpdatePathSequenceOnLocations_SetsSequenceToZeroIfProposedPathSequenceIsGreaterThanMaxShortValue_WLV_PickPathSequence()
		{
			TestUpdatePathSequenceOnLocations_SetsSequenceToZeroIfProposedPathSequenceIsGreaterThanMaxShortValueCore(WhsLocationViewSchema.Constants.WLV_PickPathSequence);
		}

		public void TestUpdatePathSequenceOnLocations_SetsSequenceToZeroIfProposedPathSequenceIsGreaterThanMaxShortValue_WLV_CycleCountPathSequence()
		{
			TestUpdatePathSequenceOnLocations_SetsSequenceToZeroIfProposedPathSequenceIsGreaterThanMaxShortValueCore(WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence);
		}

		public void TestUpdatePathSequenceOnLocations_SetsSequenceToZeroIfProposedPathSequenceIsGreaterThanMaxShortValueCore(ZString pathPropertyName)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRow(warehouse, "A", 40, 40, 40);
			var location1 = row.Locations.AddNew();
			location1.WLV_Column = 21;
			location1.WLV_Level = 20;
			location1.WLV_Tray = 7;

			var location2 = row.Locations.AddNew();
			location2.WLV_Column = 21;
			location2.WLV_Level = 20;
			location2.WLV_Tray = 8;

			var location3 = row.Locations.AddNew();
			location3.WLV_Column = 21;
			location3.WLV_Level = 20;
			location3.WLV_Tray = 9;

			if (pathPropertyName == WhsLocationViewSchema.Constants.WLV_PickPathSequence)
			{
				row.SortPickPathMethod = SortPathMethods.Codes.ColumnThenLevel;
			}
			else if (pathPropertyName == WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence)
			{
				row.SortCycleCountMethod = SortPathMethods.Codes.ColumnThenLevel;
			}

			row.UpdatePathSequenceOnLocations();
			AssertLocation(row, 21, 20, 7, pathPropertyName, short.MaxValue);
			AssertLocation(row, 21, 20, 8, pathPropertyName, 0);
			AssertLocation(row, 21, 20, 9, pathPropertyName, 0);
		}

		#endregion

		#region TestUpdatePathSequenceOnLocations_InvalidSortPathMethod

		public void TestUpdatePathSequenceOnLocations_InvalidSortPathMethod_PickPathMethod()
		{
			TestUpdatePathSequenceOnLocations_InvalidSortPathMethodCore(WhsLocationViewSchema.Constants.WLV_PickPathSequence);
		}

		public void TestUpdatePathSequenceOnLocations_InvalidSortPathMethod_CycleCountMethod()
		{
			TestUpdatePathSequenceOnLocations_InvalidSortPathMethodCore(WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence);
		}

		public void TestUpdatePathSequenceOnLocations_InvalidSortPathMethodCore(ZString pathPropertyName)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 3, 2);

			var invalidPathSortMethod = "ABC";
			if (pathPropertyName == WhsLocationViewSchema.Constants.WLV_PickPathSequence)
			{
				row.SortPickPathMethod = invalidPathSortMethod;
				AssertEquals("Precondition: SortPickPathMethod property info has errors.", true, row.SortPickPathMethodInfo.HasErrors());
			}
			else if (pathPropertyName == WhsLocationViewSchema.Constants.WLV_PutawayPathSequence)
			{
				row.SortPutawayPathMethod = invalidPathSortMethod;
				AssertEquals("Precondition: SortPutawayPathMethod property info has errors.", true, row.SortPutawayPathMethodInfo.HasErrors());
			}
			else if (pathPropertyName == WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence)
			{
				row.SortCycleCountMethod = invalidPathSortMethod;
				AssertEquals("Precondition: SortCycleCountMethod property info has errors.", true, row.SortCycleCountMethodInfo.HasErrors());
			}

			row.UpdatePathSequenceOnLocations();
			AssertEquals("Error notification added to notification buffer.", true, ((NotificationBuffer)row.NotificationSubscriber).HasErrors);
			// path sequence is not updated due to error
			AssertLocation(row, 1, 1, 1, pathPropertyName, 0);
			AssertLocation(row, 1, 1, 2, pathPropertyName, 0);
			AssertLocation(row, 1, 2, 1, pathPropertyName, 0);
			AssertLocation(row, 1, 2, 2, pathPropertyName, 0);
			AssertLocation(row, 1, 3, 1, pathPropertyName, 0);
			AssertLocation(row, 1, 3, 2, pathPropertyName, 0);
			AssertLocation(row, 2, 1, 1, pathPropertyName, 0);
			AssertLocation(row, 2, 1, 2, pathPropertyName, 0);
			AssertLocation(row, 2, 2, 1, pathPropertyName, 0);
			AssertLocation(row, 2, 2, 2, pathPropertyName, 0);
			AssertLocation(row, 2, 3, 1, pathPropertyName, 0);
			AssertLocation(row, 2, 3, 2, pathPropertyName, 0);
		}

		#endregion

		#endregion

		#region Lookups

		public void TestGetNewLookups()
		{
			AssertEquals(typeof(WhsRowLookups), Row.Lookups.GetType());
		}

		public void TestGetNewLookupsUS()
		{
			US.Testing.WhsTestHelperFunctionsEnvUS helper = new US.Testing.WhsTestHelperFunctionsEnvUS(Factory);
			WhsWarehouse whs = helper.CreateWarehouse("TST WHS");
			Row.WR_WW_Whs = whs.PK;
			AssertEquals(typeof(US.WhsRowLookups), Row.Lookups.GetType());
		}

		#endregion

		#region TestUniqueIndexFailureHandlers

		public void TestUniqueIndexFailureHandlers()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			warehouse.IsFixedWidthLocation = true;
			warehouse.WW_LocationColumnsFixedWidth = 3;
			warehouse.WW_LocationLevelsFixedWidth = 1;
			warehouse.WW_LocationTraysFixedWidth = 1;

			var row = Helper.CreateRowAndGenerateLocations(warehouse, "B", cols: 130);
			AssertType<LocationStringUniqueIndexValidationHandler>(((IBusinessObjectInternals)row).UniqueIndexFailureHandlers.Single());
			var otherRow = Helper.CreateRowAndGenerateLocations(warehouse, "XXX");
			Factory.Save();

			otherRow.WR_Name = "B006";

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

		#region IDocManagerSupport Members

		public void TestDocManagerInfo()
		{
			DocManagerInfo info = Row.DocManagerInfo;
			AssertEquals(Row, info.BusinessEntity);
			AssertEquals("WRO", info.DocManagerCode);
		}

		#endregion

		#region Implementation

		void AssertLocation(WhsRow row, short column, short level, short tray, ZString pathPropetyName, ZInt expectedPathSequence)
		{
			var location = row.Locations.Single(o => o.WLV_Column == column && o.WLV_Level == level && o.WLV_Tray == tray);
			AssertEquals($"Expected value of {pathPropetyName}", expectedPathSequence, location.FindPropertyInfo(pathPropetyName).Value);
		}

		void BaseGenerate(short columns, short levels, short trays) =>
			GenerateLocations(Factory, Row, columns, levels, trays);

		static void GenerateLocations(BusinessObjectFactory factory, WhsRow row, short columns, short levels, short trays)
		{
			row.WR_Columns = columns;
			row.WR_Levels = levels;
			row.WR_Trays = trays;

			int locationCount = columns * levels * trays;

			((WhsRowInternals)row).GenerateLocations();
			factory.Save();

			AssertEquals(locationCount, factory.GetDatabaseCount(typeof(WhsLocation), new ZQuery(WhsLocationViewSchema.WLV_WR, row.PK)));
			AssertEquals(locationCount, row.Locations.Count);
		}

		void BaseAssert()
		{
			for (short c = 1; c <= Row.WR_Columns; c++)
			{
				for (short l = 1; l <= Row.WR_Levels; l++)
				{
					for (short t = 1; t <= Row.WR_Trays; t++)
					{
						AssertEquals(1, (Factory.Load(typeof(WhsLocation), GetLocationFilter(Row, c, l, t))).Length);
					}
				}
			}
		}

		ZQuery GetLocationFilter(WhsRow row, short column, short level, short tray)
		{
			ZQuery filter = new ZQuery(WhsLocationViewSchema.WLV_WR, row.PK);
			filter.AddToFilter(WhsLocationViewSchema.WLV_Column, column);
			filter.AddToFilter(WhsLocationViewSchema.WLV_Level, level);
			filter.AddToFilter(WhsLocationViewSchema.WLV_Tray, tray);
			return filter;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Helper.CreateRow(Helper.CreateWarehouse("TST DEL"), "RD");
		}

		protected override void SetUp()
		{
			base.SetUp();
			Whs = Helper.CreateWarehouse("W");
			Row = Helper.CreateRow(Whs, "A");
		}

		WhsRow Row;
		WhsWarehouse Whs;

		#endregion
	}

	#region TestIRowForm

	internal class TestIRowForm : IRowForm
	{
		#region IRowForm Members

		public List<WhsLocation> SelectedLocations
		{
			get { return selectedLocations; }
			set { selectedLocations = value; }
		}

		#endregion

		#region Implementation

		List<WhsLocation> selectedLocations;

		#endregion
	}

	#endregion

	#region WhsRowDocumentSupporterTest

	internal class WhsRowDocumentSupporterTest : WhsDocumentSupporterTestCase
	{
		#region TestGetContactOrganisation

		public override void TestGetContactOrganisation()
		{
			WhsRow row = (WhsRow)BusinessObject;
			AssertNull(row.DocumentSupporter.GetContactOrganisation(ZString.Empty, ContactType.Warehouse3PL, DocumentDirection.ANY));
		}

		#endregion

		#region TestGetDocBusinessObjects

		public override void TestGetDocBusinessObjects()
		{
			AssertEquals("Precondition - Row should have 8 locations.", 8, Row.Locations.Count);

			// Printing from Rows Grid.
			Row.ParentForm = null;
			AssertEquals("Precondition - no locations should be selected.", 0, Row.SelectedLocations.Count);

			IBODocDataProvider[] docBizoList = DocSupporter.GetDocumentWrappers(DataContext, null);
			AssertEquals("Only one Document Wrapper should be generated, only one document should be printed.", 1, docBizoList.Length);
			AssertEquals(typeof(WhsLocationLabelList), docBizoList[0].ParentBusinessObject.GetType());
			AssertEquals("Location Labels should be printed for all locations of the Row.", 8, ((WhsLocationLabelList)docBizoList[0].ParentBusinessObject).Locations.Count);

			// Printing from Locations Form, no locations selected.
			var parentForm = new TestIRowForm();
			Row.ParentForm = parentForm;
			AssertNull("Precondition - Row.SelectedLocations should be null", Row.SelectedLocations);

			docBizoList = DocSupporter.GetDocumentWrappers(DataContext, null);
			AssertNull("No Document Wrappers should be generated, no Documents should be printed.", docBizoList);

			parentForm.SelectedLocations = new List<WhsLocation>();
			AssertEquals("Precondition - no locations should be selected.", 0, Row.SelectedLocations.Count);

			docBizoList = DocSupporter.GetDocumentWrappers(DataContext, null);
			AssertNull("No Document Wrappers should be generated, no Documents should be printed.", docBizoList);

			// Printing from Locations Form, 2 locations selected.
			var selectedLocations = new List<WhsLocation>();
			selectedLocations.Add(Row.Locations[0]);
			selectedLocations.Add(Row.Locations[1]);
			parentForm.SelectedLocations = selectedLocations;
			AssertEquals("Precondition - 2 locations should be selected.", 2, Row.SelectedLocations.Count);

			docBizoList = DocSupporter.GetDocumentWrappers(DataContext, null);
			AssertEquals("Only one Document Wrapper should be generated, only one document should be printed.", 1, docBizoList.Length);
			AssertEquals(typeof(WhsLocationLabelList), docBizoList[0].ParentBusinessObject.GetType());
			AssertEquals("Location Labels should be printed for 2 selected locations only.", 2, ((WhsLocationLabelList)docBizoList[0].ParentBusinessObject).Locations.Count);
			AssertEquals(selectedLocations, ((WhsLocationLabelList)docBizoList[0].ParentBusinessObject).Locations);
		}

		#endregion

		#region TestBusinessContext

		public override void TestBusinessContext()
		{
			AssertEquals(BusinessContext.WhsRow, DocSupporter.BusinessContext);
		}

		#endregion

		#region TestGetBODocDataProvidersNotFoundMessageReturnOneReason

		protected override IEnumerable<Tuple<Constants.DataContext, string>> SupportedDataContextAndNotFoundMessageReasonPairs
		{
			get
			{
				var dataContextAndMessagePairs = new List<Tuple<Constants.DataContext, string>>();

				dataContextAndMessagePairs.Add(new Tuple<Constants.DataContext, string>(Constants.DataContext.WhsLocationLabels, "Cannot find Warehouse Location."));

				return dataContextAndMessagePairs;
			}
		}

		#endregion

		#region Implementation

		#region Properties

		protected override Constants.DataContext DataContext
		{
			get { return Enterprise.Core.Constants.DataContext.WhsLocationLabels; }
		}

		protected override ISecurityCheckpoint ExpectedCustomisationSecurityCheckpoint => Env.Security.WhsConfigWarehouseCustomiseDocuments;

		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return Row;
		}

		protected override void SetUp()
		{
			base.SetUp();
			WhsWarehouse warehouse = Helper.CreateWarehouse("WHS");
			Row = Helper.CreateRowAndGenerateLocations(warehouse, "ROW", 2, 2, 2);
		}

		WhsRow Row;

		#endregion
	}

	#endregion

	#region WhsRowDocumentSupporterDocumentSupporterTest

	[TestedType(typeof(WhsRowDocumentSupporter))]
	class WhsRowDocumentSupporterDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "ROW", 2, 2, 2);
			return row;
		}

		protected WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}
		WhsTestHelperFunctionsEnv helper;
	}

	#endregion

	class WhsRowTriggersTest : TestCaseWithFactory
	{
		#region Test trigger which prevents too big Row part values for Alpha

		public void TestSettingTooBigColumnsForAlpha()
		{
			TestSettingTooBigLocationPartForAlpha(warehouse => warehouse.WW_LocationColumnsAlpha = true, row => row.WR_Columns = 27, "Unable to set number of Columns to be more than 26 in warehouse with A-Z display Columns.");
		}

		public void TestSettingTooBigLevelsForAlpha()
		{
			TestSettingTooBigLocationPartForAlpha(warehouse => warehouse.WW_LocationLevelsAlpha = true, row => row.WR_Levels = 27, "Unable to set number of Levels to be more than 26 in warehouse with A-Z display Levels.");
		}

		public void TestSettingTooBigTraysForAlpha()
		{
			TestSettingTooBigLocationPartForAlpha(warehouse => warehouse.WW_LocationTraysAlpha = true, row => row.WR_Trays = 27, "Unable to set number of Trays to be more than 26 in warehouse with A-Z display Trays.");
		}

		public void TestSettingTooBigLocationPartForAlpha(Action<WhsWarehouse> warehousePropertySetter, Action<WhsRow> rowPropertySetter, ZString exceptionMessage)
		{
			var warehouse = Helper.CreateWarehouse("WH1", "A");
			warehousePropertySetter(warehouse);
			var row = warehouse.Rows.Single(r => r.WR_Name == "A");
			Factory.Save();

			rowPropertySetter(row);
			try
			{
				Factory.Save();
				Fail("Trigger should prevent save.");
			}
			catch (ZSaveException ex)
			{
				AssertEquals(exceptionMessage, ex.FriendlyMessage);
			}
		}

		#endregion

		#region Test trigger prevents too big Row part values based on location fixed width

		public void TestSettingTooBigColumnsForColumnsFixedWidth_FixedWidthIsTwo()
		{
			TestSettingTooBigForLocationsFixedWidth(
				(row, warehouse) => { row.WR_Columns = 99; warehouse.WW_LocationColumnsFixedWidth = 2; },
				row => row.WR_Columns = 100,
				"Unable to set the number of Columns to be more than what is allowed by the Columns Fixed Width.");
		}

		public void TestSettingTooBigColumnsForColumnsFixedWidth_FixedWidthIsOne()
		{
			TestSettingTooBigForLocationsFixedWidth(
				(row, warehouse) => { row.WR_Columns = 9; },
				row => row.WR_Columns = 10,
				"Unable to set the number of Columns to be more than what is allowed by the Columns Fixed Width.");
		}

		public void TestSettingTooBigLevelsForLevelsFixedWidth_FixedWidthIsTwo()
		{
			TestSettingTooBigForLocationsFixedWidth(
				(row, warehouse) => { row.WR_Levels = 99; warehouse.WW_LocationLevelsFixedWidth = 2; },
				row => row.WR_Levels = 100,
				"Unable to set the number of Levels to be more than what is allowed by the Levels Fixed Width.");
		}

		public void TestSettingTooBigLevelsForLevelsFixedWidth_FixedWidthIsOne()
		{
			TestSettingTooBigForLocationsFixedWidth(
				(row, warehouse) => { row.WR_Levels = 9; },
				row => row.WR_Levels = 10,
				"Unable to set the number of Levels to be more than what is allowed by the Levels Fixed Width.");
		}

		public void TestSettingTooBigTraysForTraysFixedWidth()
		{
			TestSettingTooBigForLocationsFixedWidth(
				(row, warehouse) => { row.WR_Trays = 9; },
				row => row.WR_Trays = 10,
				"Unable to set the number of Trays to be more than what is allowed by the Trays Fixed Width.");
		}

		public void TestSettingTooBigForLocationsFixedWidth(Action<WhsRow, WhsWarehouse> warehouseAndRowConfigSetter, Action<WhsRow> rowPropertySetter, ZString expectedMessage)
		{
			var warehouse = Helper.CreateFixedWidthLocationWarehouse("WH1", 1, 1, 1);
			var row = Helper.CreateRow(warehouse, "A");
			warehouseAndRowConfigSetter(row, warehouse);
			Factory.Save();

			rowPropertySetter(row);
			try
			{
				Factory.Save();
				Fail("Trigger should prevent save.");
			}
			catch (ZSaveException ex)
			{
				AssertEquals(expectedMessage, ex.FriendlyMessage);
			}
		}

		public void TestTriggerAlphaColumnsFixedWidth()
		{
			TestTriggerDoesNotPreventsSettingLocationFixedWidth_Alpha(
				row => row.WR_Columns = 9,
				(row, warehouse) => { warehouse.WW_LocationColumnsAlpha = true; row.WR_Columns = 10; });
		}

		public void TestTriggerAlphaLevelsFixedWidth()
		{
			TestTriggerDoesNotPreventsSettingLocationFixedWidth_Alpha(
				row => row.WR_Levels = 9,
				(row, warehouse) => { warehouse.WW_LocationLevelsAlpha = true; row.WR_Levels = 10; });
		}

		public void TestTriggerAlphaTraysFixedWidth()
		{
			TestTriggerDoesNotPreventsSettingLocationFixedWidth_Alpha(
				row => { row.WR_Trays = 9; },
				(row, warehouse) => { warehouse.WW_LocationTraysAlpha = true; row.WR_Trays = 10; });
		}

		void TestTriggerDoesNotPreventsSettingLocationFixedWidth_Alpha(Action<WhsRow> rowConfigSetter, Action<WhsRow, WhsWarehouse> warehouseAndRowPropertySetter)
		{
			var warehouse = Helper.CreateWarehouse("WH1", "A");
			warehouse.WW_LocationColumnsFixedWidth = 1;
			warehouse.WW_LocationLevelsFixedWidth = 1;
			warehouse.WW_LocationTraysFixedWidth = 1;
			warehouse.WW_LocationsHaveLeadingZeros = true;
			var row = warehouse.Rows.Single(r => r.WR_Name == "A");
			rowConfigSetter(row);
			Factory.Save();

			warehouseAndRowPropertySetter(row, warehouse);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctionsEnv helper;
		protected WhsTestHelperFunctionsEnv Helper => helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory));

		#endregion
	}

	#region WhsRowAffectLocationViewTestCase

	[TestedType(typeof(WhsRow))]
	class WhsRowAffectLocationViewTestCase : AffectLocationStringTestCase
	{
		#region TestReloadLocations

		public void TestReloadLocations()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			var row = warehouse.Rows.AddNew();
			row.WR_Name = "A";
			row.WR_Columns = 2;
			AssertEquals("Precondition", 0, row.Locations.Count);

			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { "A-1", "A-2" }, row.Locations.Select(l => l.WLV_LocationString));

			row.WR_Levels = 2;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { "A-1-1", "A-1-2", "A-2-1", "A-2-2" }, row.Locations.Select(l => l.WLV_LocationString));
		}

		#region TestReloadLocationsOnceWhenWarehouseAndRowBothChanged

		public void TestReloadLocationsOnceWhenWarehouseAndRowBothChanged()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			var row = warehouse.Rows.AddNew();
			row.WR_Name = "A";
			row.WR_Columns = 10;
			AssertEquals("Precondition", 0, row.Locations.Count);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var warehouseInNewFactory = newFactory.Load<WhsWarehouse>(warehouse.PK);
			var rowInNewFactory = newFactory.Load<WhsRow>(row.PK);
			warehouseInNewFactory.WW_LocationColumnsAlpha = true;
			rowInNewFactory.WR_Name = "B";
			newFactory.Save();

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(WhsLocationViewSchema.Constants.TableName, 2); // 1 DB hit for Row Generating Locations, 1 DB Hit for Warehouse Reloading Locations
			expectedDbHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			expectedDbHits.Add(WhsRowSchema.Constants.TableName, 2);
			expectedDbHits.Add(WhsAreaSchema.Constants.TableName, 1);
			AssertDbHits(expectedDbHits, newFactory);
		}

		#endregion

		#endregion

		#region Implementation

		protected override IEnumerable<WhsLocation> GetLocations(IAffectLocationView p)
		{
			WhsRow parent = (WhsRow)p;

			return parent.Locations;
		}

		protected override IAffectLocationView GetNewParent()
		{
			var helper = new WhsTestHelperFunctionsEnv(Factory);
			var warehouse = helper.CreateWarehouse("WHS");
			return helper.CreateRowAndGenerateLocations(warehouse, "A", 2, 2);
		}

		protected override void ModifyColumnsThatAffectionLocationView(IAffectLocationView p)
		{
			WhsRow parent = (WhsRow)p;

			parent.WR_Name = "B";
		}

		protected override IEnumerable<SchemaColumn> ExpectedColumnsThatAffectLocationView
		{
			get
			{
				return new SchemaColumn[]
				{
					WhsRowSchema.WR_Name,
					WhsRowSchema.WR_Columns,
					WhsRowSchema.WR_Levels,
					WhsRowSchema.WR_Trays,
				};
			}
		}

		#endregion
	}

	#endregion
}
