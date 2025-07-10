using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	#region class WhsLocationCollectionForRowTest

	[TestedType(typeof(WhsLocationCollection))]
	public class WhsLocationCollectionForRowTest : WhsActiveBusinessObjectCollectionTestCase<WhsLocationCollection>
	{
		#region TestSortByPickPathSequenceSortsZerosLast

		public void TestSortByPickPathSequenceSortsZerosLast()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 2);
			Factory.Save();

			var location1 = warehouse.FindLocation("A-1-1");
			location1.WLV_PickPathSequence = 3;
			var location2 = warehouse.FindLocation("A-1-2");
			location2.WLV_PickPathSequence = 1;
			var location3 = warehouse.FindLocation("A-2-1");
			location3.WLV_PickPathSequence = 0;
			var location4 = warehouse.FindLocation("A-2-2");
			location4.WLV_PickPathSequence = 2;

			var row = warehouse.Rows.Single(r => r.WR_Name == "A");
			row.Locations.ApplySort(WhsLocationViewSchema.Constants.WLV_PickPathSequence, ListSortDirection.Ascending);
			int index1 = 0;
			AssertEquals(row.Locations.Single(o => o.WLV_PickPathSequence == 1), row.Locations[index1++]);
			AssertEquals(row.Locations.Single(o => o.WLV_PickPathSequence == 2), row.Locations[index1++]);
			AssertEquals(row.Locations.Single(o => o.WLV_PickPathSequence == 3), row.Locations[index1++]);
			AssertEquals(row.Locations.Single(o => o.WLV_PickPathSequence == 0), row.Locations[index1++]);

			int index2 = 0;
			row.Locations.ApplySort(WhsLocationViewSchema.Constants.WLV_PickPathSequence, ListSortDirection.Descending);
			AssertEquals(row.Locations.Single(o => o.WLV_PickPathSequence == 0), row.Locations[index2++]);
			AssertEquals(row.Locations.Single(o => o.WLV_PickPathSequence == 3), row.Locations[index2++]);
			AssertEquals(row.Locations.Single(o => o.WLV_PickPathSequence == 2), row.Locations[index2++]);
			AssertEquals(row.Locations.Single(o => o.WLV_PickPathSequence == 1), row.Locations[index2++]);
		}

		#endregion

		#region TestFindByColumnLevelTray

		public void TestFindByColumnLevelTray()
		{
			AssertEquals(Row.Locations[0], Row.Locations.FindByColumnLevelTray(1, 1, 1));
			AssertEquals(Row.Locations[4], Row.Locations.FindByColumnLevelTray(1, 3, 1));
			AssertEquals(Row.Locations[21], Row.Locations.FindByColumnLevelTray(4, 2, 2));
		}

		#endregion

		#region TestFindByColumnLevelTray_InvalidCollectionTypes

		public void TestFindByColumnLevelTray_InvalidCollectionTypes()
		{
			var warehouse = Helper.CreateWarehouse("1");
			TestFindByColumnLevelTray_InvalidCollectionTypesCore(new WhsLocationCollection(Factory));
			TestFindByColumnLevelTray_InvalidCollectionTypesCore(new WhsLocationCollection(Factory, false));
			TestFindByColumnLevelTray_InvalidCollectionTypesCore(new WhsLocationCollection(Factory, true));
			TestFindByColumnLevelTray_InvalidCollectionTypesCore(new WhsLocationCollection(warehouse));
			TestFindByColumnLevelTray_InvalidCollectionTypesCore(new WhsLocationCollection(warehouse, false));
			TestFindByColumnLevelTray_InvalidCollectionTypesCore(new WhsLocationCollection(warehouse, true));
		}

		void TestFindByColumnLevelTray_InvalidCollectionTypesCore(WhsLocationCollection locationCollection)
		{
#if NETFRAMEWORK
			AssertExceptionThrown<ArgumentException>("Should throw exception,", "Value cannot be null.\r\nParameter name: Cannot call FindByColumnLevelTray() on a WhsLocationCollection whose parent is a WhsRow.", () => locationCollection.FindByColumnLevelTray(1, 1, 1));
#elif NET
			AssertExceptionThrown<ArgumentException>("Should throw exception,", "Value cannot be null. (Parameter 'Cannot call FindByColumnLevelTray() on a WhsLocationCollection whose parent is a WhsRow.')", () => locationCollection.FindByColumnLevelTray(1, 1, 1));
#endif
		}

		#endregion

		#region TestFindByColumnLevelTray_ColumnOutOfRange

		public void TestFindByColumnLevelTray_ColumnOutOfRange()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1, 1);

			AssertNull("Should return null if the column is under range.", row.Locations.FindByColumnLevelTray(0, 1, 1));
			AssertNotNull("Should return a location if the column is in range.", row.Locations.FindByColumnLevelTray(1, 1, 1));
			AssertNull("Should return null if the column is over range.", row.Locations.FindByColumnLevelTray(2, 1, 1));
		}

		#endregion

		#region TestFindByColumnLevelTray_LevelOutOfRange

		public void TestFindByColumnLevelTray_LevelOutOfRange()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1, 1);

			AssertNull("Should return null if the level is under range.", row.Locations.FindByColumnLevelTray(1, 0, 1));
			AssertNotNull("Should return a location if the level is in range.", row.Locations.FindByColumnLevelTray(1, 1, 1));
			AssertNull("Should return null if the level is over range.", row.Locations.FindByColumnLevelTray(1, 2, 1));
		}

		#endregion

		#region TestFindByColumnLevelTray_TrayOutOfRange

		public void TestFindByColumnLevelTray_TrayOutOfRange()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1, 1);

			AssertNull("Should return null if the tray is under range.", row.Locations.FindByColumnLevelTray(1, 1, 0));
			AssertNotNull("Should return a location if the tray is in range.", row.Locations.FindByColumnLevelTray(1, 1, 1));
			AssertNull("Should return null if the tray is over range.", row.Locations.FindByColumnLevelTray(1, 1, 2));
		}

		#endregion

		#region TestSortByColLevelTray

		public void TestSortByColLevelTray()
		{
			foreach (WhsLocation locn in Row.Locations)
			{
				locn.WLV_MaxCubicUnit = Enterprise.Core.Constants.Volume.CubicMetres;
				locn.WLV_MaxCubic = 9;
			}

			Row.Locations[0].WLV_MaxCubic = 8;
			Row.Locations[1].WLV_MaxCubic = 7;
			Row.Locations[2].WLV_MaxCubic = 6;
			Row.Locations[3].WLV_MaxCubic = 5;
			Row.Locations[4].WLV_MaxCubic = 4;
			Row.Locations[5].WLV_MaxCubic = 3;
			Row.Locations[6].WLV_MaxCubic = 2;
			Row.Locations[7].WLV_MaxCubic = 1;

			Factory.Save();

			Row.Locations.ApplySort(WhsLocationViewSchema.WLV_MaxCubic.Name, ListSortDirection.Ascending);

			AssertEquals("A-2-1-2", Row.Locations[0].ToLocationString());
			AssertEquals("A-2-1-1", Row.Locations[1].ToLocationString());
			AssertEquals("A-1-3-2", Row.Locations[2].ToLocationString());
			AssertEquals("A-1-3-1", Row.Locations[3].ToLocationString());
			AssertEquals("A-1-2-2", Row.Locations[4].ToLocationString());
			AssertEquals("A-1-2-1", Row.Locations[5].ToLocationString());
			AssertEquals("A-1-1-2", Row.Locations[6].ToLocationString());
			AssertEquals("A-1-1-1", Row.Locations[7].ToLocationString());

			Row.Locations.ApplySort(new WhsLocationCollection.SortByColLevelTray());

			AssertEquals("A-1-1-1", Row.Locations[0].ToLocationString());
			AssertEquals("A-1-1-2", Row.Locations[1].ToLocationString());
			AssertEquals("A-1-2-1", Row.Locations[2].ToLocationString());
			AssertEquals("A-1-2-2", Row.Locations[3].ToLocationString());
			AssertEquals("A-1-3-1", Row.Locations[4].ToLocationString());
			AssertEquals("A-1-3-2", Row.Locations[5].ToLocationString());
			AssertEquals("A-2-1-1", Row.Locations[6].ToLocationString());
			AssertEquals("A-2-1-2", Row.Locations[7].ToLocationString());
		}

		#endregion

		#region TestSetValues

		public void TestSetValues()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 4, 3, 2);
			var location1 = row.Locations.FindByColumnLevelTray(1, 1, 1);
			var location2 = row.Locations.FindByColumnLevelTray(1, 2, 1);
			var location3 = row.Locations.FindByColumnLevelTray(1, 3, 1);
			var locationType = Helper.CreateLocationType("LT");

			SetupLocationValues(location1, 10m, 10m, "W1", "C1", "TY1", "ST1", "PM1", ZGuid.NewZGuid(), ZGuid.NewZGuid(), "AK1", 0);
			SetupLocationValues(location2, 20m, 20m, "W2", "C2", "TY2", "ST2", "PM2", ZGuid.NewZGuid(), ZGuid.NewZGuid(), "AK2", 1);
			SetupLocationValues(location3, 30m, 30m, "W3", "C3", "TY3", "ST3", "PM3", ZGuid.NewZGuid(), ZGuid.NewZGuid(), "AK3", 2);

			row.MaxWeight = 100m;
			row.MaxCubic = 200m;
			row.MaxWeightUnit = "WU";
			row.MaxCubicUnit = "CU";
			row.LocationType = locationType.PK;
			row.LocationStatus = "LS";
			row.PickMethod = "PM";
			row.PickingArea_MassUpdate = ZGuid.NewZGuid();
			row.PutawayArea_MassUpdate = ZGuid.NewZGuid();
			row.ApprovedKnownStatus = "YES";
			row.MaximumTouchCount = 4;

			row.Locations.SetValues(new[] { location1, location2, location3 });
			AssertLocationRowValues(row, location1);
			AssertLocationRowValues(row, location2);
			AssertLocationRowValues(row, location3);
		}

		public void TestSetValues_DoesNotHitDBMultipleTimesForPickFaceOrPick()
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
				row.Locations.SetValues(row.Locations.ToArray());
				AssertEquals("Should have only hit the DB once, even though we are updating multiple Locations.", 1, TestConnection.ExecutedCommands.Count(c => c.Contains("WL_PK IN (SELECT Value FROM @Locations)")));
				AssertEquals(0, newFactory.TableSelects.Count(t => t.TableName == WhsPickSchema.Constants.TableName));
				AssertEquals(0, newFactory.TableSelects.Count(t => t.TableName == WhsPickFaceSchema.Constants.TableName));
				AssertEquals(true, row.Locations.All(l => l.WLV_LocationStatus == LocationStatus.Codes.Damaged));
				AssertEquals(true, row.Locations.All(l => l.WLV_WLT_LocationType == locationType.PK));
			}
		}

		public void TestSetValues_DoesNotUpdateLocationPathSequence_PickPathSequence()
		{
			TestSetValues_DoesNotUpdateLocationPathSequenceCore(WhsLocationViewSchema.WLV_PickPathSequence.Name);
		}

		public void TestSetValues_DoesNotUpdateLocationPathSequence_PutawayPathSequence()
		{
			TestSetValues_DoesNotUpdateLocationPathSequenceCore(WhsLocationViewSchema.WLV_PutawayPathSequence.Name);
		}

		public void TestSetValues_DoesNotUpdateLocationPathSequence_CycleCountPathSequence()
		{
			TestSetValues_DoesNotUpdateLocationPathSequenceCore(WhsLocationViewSchema.WLV_CycleCountPathSequence.Name);
		}

		public void TestSetValues_DoesNotUpdateLocationPathSequenceCore(string pathSequenceProperty)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 4, 3, 2);
			var location1 = row.Locations.FindByColumnLevelTray(1, 1, 1);
			var location2 = row.Locations.FindByColumnLevelTray(1, 2, 1);
			var location3 = row.Locations.FindByColumnLevelTray(1, 3, 1);
			var locationType = Helper.CreateLocationType("LT");

			SetupLocationValues(location1, 10m, 10m, "W1", "C1", "TY1", "ST1", "PM1", ZGuid.NewZGuid(), ZGuid.NewZGuid(), "AK1", 0);
			SetupLocationValues(location2, 20m, 20m, "W2", "C2", "TY2", "ST2", "PM2", ZGuid.NewZGuid(), ZGuid.NewZGuid(), "AK2", 1);
			SetupLocationValues(location3, 30m, 30m, "W3", "C3", "TY3", "ST3", "PM3", ZGuid.NewZGuid(), ZGuid.NewZGuid(), "AK3", 2);

			if (pathSequenceProperty == WhsLocationViewSchema.WLV_PickPathSequence.Name)
			{
				row.SortPickPathMethod = SortPathMethods.Codes.ColumnThenLevel;
				AssertEquals("Precondition: sort path method is not empty.", false, row.SortPickPathMethod.IsEmpty);
			}
			else if (pathSequenceProperty == WhsLocationViewSchema.WLV_PutawayPathSequence.Name)
			{
				row.SortPutawayPathMethod = SortPathMethods.Codes.ColumnThenLevel;
				AssertEquals("Precondition: sort path method is not empty.", false, row.SortPutawayPathMethod.IsEmpty);
			}
			else
			{
				row.SortCycleCountMethod = SortPathMethods.Codes.ColumnThenLevel;
				AssertEquals("Precondition: sort path method is not empty.", false, row.SortCycleCountMethod.IsEmpty);
			}

			AssertEquals("Precondition: path sequence is not assigned.", ZInt.Zero, location1.FindPropertyInfo(pathSequenceProperty).Value);
			AssertEquals("Precondition: path sequence is not assigned.", ZInt.Zero, location2.FindPropertyInfo(pathSequenceProperty).Value);
			AssertEquals("Precondition: path sequence is not assigned.", ZInt.Zero, location3.FindPropertyInfo(pathSequenceProperty).Value);

			row.Locations.SetValues(new[] { location1, location2, location3 });
			AssertEquals("Path sequence is still 0.", ZInt.Zero, location1.FindPropertyInfo(pathSequenceProperty).Value);
			AssertEquals("Path sequence is still 0.", ZInt.Zero, location2.FindPropertyInfo(pathSequenceProperty).Value);
			AssertEquals("Path sequence is still 0.", ZInt.Zero, location3.FindPropertyInfo(pathSequenceProperty).Value);
		}

		void SetupLocationValues(WhsLocation location, ZDecimal maxWeight, ZDecimal maxCubic, ZString maxWeightUnit,
								ZString maxCubicUnit, ZString locationType, ZString locationStatus,
								ZString pickMethod, ZGuid pickingArea, ZGuid putawayArea, ZString approvedKnownStatus, ZInt maximumTouchCount)
		{
			var tstlocationType = Helper.CreateLocationType(locationType, "Test", false, 0, "DDL");
			location.WLV_MaxWeight = maxWeight;
			location.WLV_MaxCubic = maxCubic;
			location.WLV_MaxWeightUnit = maxWeightUnit;
			location.WLV_MaxCubicUnit = maxCubicUnit;
			location.WLV_WLT_LocationType = tstlocationType.PK;
			location.WLV_LocationStatus = locationStatus;
			location.WLV_PickMethod = pickMethod;
			location.WLV_WA_PickingArea = pickingArea;
			location.WLV_WA_PutawayArea = putawayArea;
			location.WLV_ApprovedKnownLocation = approvedKnownStatus;
			location.WLV_FinalisedPickCount = maximumTouchCount;
			location.WLV_PickPathSequence = 0;
			location.WLV_PutawayPathSequence = 0;
			location.WLV_CycleCountPathSequence = 0;
		}

		void AssertLocationRowValues(WhsRow row, WhsLocation location)
		{
			AssertEquals(row.MaxWeight, location.WLV_MaxWeight);
			AssertEquals(row.MaxCubic, location.WLV_MaxCubic);
			AssertEquals(row.MaxWeightUnit, location.WLV_MaxWeightUnit);
			AssertEquals(row.MaxCubicUnit, location.WLV_MaxCubicUnit);
			AssertEquals(row.LocationType, location.LocationType.PK);
			AssertEquals(row.LocationStatus, location.WLV_LocationStatus);
			AssertEquals(row.PickMethod, location.WLV_PickMethod);
			AssertEquals(row.PickingArea_MassUpdate, location.WLV_WA_PickingArea);
			AssertEquals(row.PutawayArea_MassUpdate, location.WLV_WA_PutawayArea);
			AssertEquals(row.ApprovedKnownStatus, location.WLV_ApprovedKnownLocation);
			AssertEquals(row.MaximumTouchCount, location.WLV_MaximumPickCountBeforeAutomatedStocktake);
		}

		#endregion

		#region TestICompositeCollection

		public void TestICompositeCollection()
		{
			ICompositeCollection compositeCollection = Row.Locations;
			AssertEquals(typeof(WhsLocation), compositeCollection.TypeOfElementFromCode(""));
			AssertEquals(typeof(WhsLocation), compositeCollection.TypeOfElementFromPK(ZGuid.Empty));
			AssertEquals(36, compositeCollection.MaxLength);
		}

		#endregion

		#region TestLocationsSortedProperly

		public void TestColumnsSortedProperly()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 10, 2, 1);
			Factory.Save();

			row.Locations.ApplySort(nameof(WhsLocation.FormattedColumn), ListSortDirection.Ascending);
			AssertEquals("A-1-1", row.Locations[0].ToLocationString());
			AssertEquals("A-1-2", row.Locations[1].ToLocationString());
			AssertEquals("A-2-1", row.Locations[2].ToLocationString());
			AssertEquals("A-2-2", row.Locations[3].ToLocationString());
			AssertEquals("A-3-1", row.Locations[4].ToLocationString());
			AssertEquals("A-3-2", row.Locations[5].ToLocationString());
			AssertEquals("A-4-1", row.Locations[6].ToLocationString());
			AssertEquals("A-4-2", row.Locations[7].ToLocationString());
			AssertEquals("A-5-1", row.Locations[8].ToLocationString());
			AssertEquals("A-5-2", row.Locations[9].ToLocationString());
			AssertEquals("A-6-1", row.Locations[10].ToLocationString());
			AssertEquals("A-6-2", row.Locations[11].ToLocationString());
			AssertEquals("A-7-1", row.Locations[12].ToLocationString());
			AssertEquals("A-7-2", row.Locations[13].ToLocationString());
			AssertEquals("A-8-1", row.Locations[14].ToLocationString());
			AssertEquals("A-8-2", row.Locations[15].ToLocationString());
			AssertEquals("A-9-1", row.Locations[16].ToLocationString());
			AssertEquals("A-9-2", row.Locations[17].ToLocationString());
			AssertEquals("A-10-1", row.Locations[18].ToLocationString());
			AssertEquals("A-10-2", row.Locations[19].ToLocationString());

			row.Locations.ApplySort(nameof(WhsLocation.FormattedColumn), ListSortDirection.Descending);
			AssertEquals("A-10-1", row.Locations[0].ToLocationString());
			AssertEquals("A-10-2", row.Locations[1].ToLocationString());
			AssertEquals("A-9-1", row.Locations[2].ToLocationString());
			AssertEquals("A-9-2", row.Locations[3].ToLocationString());
			AssertEquals("A-8-1", row.Locations[4].ToLocationString());
			AssertEquals("A-8-2", row.Locations[5].ToLocationString());
			AssertEquals("A-7-1", row.Locations[6].ToLocationString());
			AssertEquals("A-7-2", row.Locations[7].ToLocationString());
			AssertEquals("A-6-1", row.Locations[8].ToLocationString());
			AssertEquals("A-6-2", row.Locations[9].ToLocationString());
			AssertEquals("A-5-1", row.Locations[10].ToLocationString());
			AssertEquals("A-5-2", row.Locations[11].ToLocationString());
			AssertEquals("A-4-1", row.Locations[12].ToLocationString());
			AssertEquals("A-4-2", row.Locations[13].ToLocationString());
			AssertEquals("A-3-1", row.Locations[14].ToLocationString());
			AssertEquals("A-3-2", row.Locations[15].ToLocationString());
			AssertEquals("A-2-1", row.Locations[16].ToLocationString());
			AssertEquals("A-2-2", row.Locations[17].ToLocationString());
			AssertEquals("A-1-1", row.Locations[18].ToLocationString());
			AssertEquals("A-1-2", row.Locations[19].ToLocationString());
		}

		public void TestLevelsSortedProperly()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 10, 1);
			Factory.Save();

			row.Locations.ApplySort(nameof(WhsLocation.FormattedLevel), ListSortDirection.Ascending);
			AssertEquals("A-1-1", row.Locations[0].ToLocationString());
			AssertEquals("A-1-2", row.Locations[1].ToLocationString());
			AssertEquals("A-1-3", row.Locations[2].ToLocationString());
			AssertEquals("A-1-4", row.Locations[3].ToLocationString());
			AssertEquals("A-1-5", row.Locations[4].ToLocationString());
			AssertEquals("A-1-6", row.Locations[5].ToLocationString());
			AssertEquals("A-1-7", row.Locations[6].ToLocationString());
			AssertEquals("A-1-8", row.Locations[7].ToLocationString());
			AssertEquals("A-1-9", row.Locations[8].ToLocationString());
			AssertEquals("A-1-10", row.Locations[9].ToLocationString());

			row.Locations.ApplySort(nameof(WhsLocation.FormattedLevel), ListSortDirection.Descending);
			AssertEquals("A-1-10", row.Locations[0].ToLocationString());
			AssertEquals("A-1-9", row.Locations[1].ToLocationString());
			AssertEquals("A-1-8", row.Locations[2].ToLocationString());
			AssertEquals("A-1-7", row.Locations[3].ToLocationString());
			AssertEquals("A-1-6", row.Locations[4].ToLocationString());
			AssertEquals("A-1-5", row.Locations[5].ToLocationString());
			AssertEquals("A-1-4", row.Locations[6].ToLocationString());
			AssertEquals("A-1-3", row.Locations[7].ToLocationString());
			AssertEquals("A-1-2", row.Locations[8].ToLocationString());
			AssertEquals("A-1-1", row.Locations[9].ToLocationString());
		}

		public void TestTraysSortedProperly()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1, 10);
			Factory.Save();

			row.Locations.ApplySort(nameof(WhsLocation.FormattedTray), ListSortDirection.Ascending);
			AssertEquals("A-1-1-1", row.Locations[0].ToLocationString());
			AssertEquals("A-1-1-2", row.Locations[1].ToLocationString());
			AssertEquals("A-1-1-3", row.Locations[2].ToLocationString());
			AssertEquals("A-1-1-4", row.Locations[3].ToLocationString());
			AssertEquals("A-1-1-5", row.Locations[4].ToLocationString());
			AssertEquals("A-1-1-6", row.Locations[5].ToLocationString());
			AssertEquals("A-1-1-7", row.Locations[6].ToLocationString());
			AssertEquals("A-1-1-8", row.Locations[7].ToLocationString());
			AssertEquals("A-1-1-9", row.Locations[8].ToLocationString());
			AssertEquals("A-1-1-10", row.Locations[9].ToLocationString());

			row.Locations.ApplySort(nameof(WhsLocation.FormattedTray), ListSortDirection.Descending);
			AssertEquals("A-1-1-10", row.Locations[0].ToLocationString());
			AssertEquals("A-1-1-9", row.Locations[1].ToLocationString());
			AssertEquals("A-1-1-8", row.Locations[2].ToLocationString());
			AssertEquals("A-1-1-7", row.Locations[3].ToLocationString());
			AssertEquals("A-1-1-6", row.Locations[4].ToLocationString());
			AssertEquals("A-1-1-5", row.Locations[5].ToLocationString());
			AssertEquals("A-1-1-4", row.Locations[6].ToLocationString());
			AssertEquals("A-1-1-3", row.Locations[7].ToLocationString());
			AssertEquals("A-1-1-2", row.Locations[8].ToLocationString());
			AssertEquals("A-1-1-1", row.Locations[9].ToLocationString());
		}

		public void TestRowLocSeqSortedProperly()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 10, 1, 1);
			Factory.Save();

			row.WR_PickPathSequence = 1;
			row.Locations.FindByColumnLevelTray(1, 1, 1).WLV_PickPathSequence = 10;
			row.Locations.FindByColumnLevelTray(2, 1, 1).WLV_PickPathSequence = 9;
			row.Locations.FindByColumnLevelTray(3, 1, 1).WLV_PickPathSequence = 8;
			row.Locations.FindByColumnLevelTray(4, 1, 1).WLV_PickPathSequence = 7;
			row.Locations.FindByColumnLevelTray(5, 1, 1).WLV_PickPathSequence = 6;
			row.Locations.FindByColumnLevelTray(6, 1, 1).WLV_PickPathSequence = 5;
			row.Locations.FindByColumnLevelTray(7, 1, 1).WLV_PickPathSequence = 4;
			row.Locations.FindByColumnLevelTray(8, 1, 1).WLV_PickPathSequence = 3;
			row.Locations.FindByColumnLevelTray(9, 1, 1).WLV_PickPathSequence = 2;
			row.Locations.FindByColumnLevelTray(10, 1, 1).WLV_PickPathSequence = 1;

			row.Locations.ApplySort(nameof(WhsLocation.RowLocationSequence), ListSortDirection.Ascending);
			AssertEquals("1-1", row.Locations[0].RowLocationSequence);
			AssertEquals("1-2", row.Locations[1].RowLocationSequence);
			AssertEquals("1-3", row.Locations[2].RowLocationSequence);
			AssertEquals("1-4", row.Locations[3].RowLocationSequence);
			AssertEquals("1-5", row.Locations[4].RowLocationSequence);
			AssertEquals("1-6", row.Locations[5].RowLocationSequence);
			AssertEquals("1-7", row.Locations[6].RowLocationSequence);
			AssertEquals("1-8", row.Locations[7].RowLocationSequence);
			AssertEquals("1-9", row.Locations[8].RowLocationSequence);
			AssertEquals("1-10", row.Locations[9].RowLocationSequence);

			row.Locations.ApplySort(nameof(WhsLocation.RowLocationSequence), ListSortDirection.Descending);
			AssertEquals("1-10", row.Locations[0].RowLocationSequence);
			AssertEquals("1-9", row.Locations[1].RowLocationSequence);
			AssertEquals("1-8", row.Locations[2].RowLocationSequence);
			AssertEquals("1-7", row.Locations[3].RowLocationSequence);
			AssertEquals("1-6", row.Locations[4].RowLocationSequence);
			AssertEquals("1-5", row.Locations[5].RowLocationSequence);
			AssertEquals("1-4", row.Locations[6].RowLocationSequence);
			AssertEquals("1-3", row.Locations[7].RowLocationSequence);
			AssertEquals("1-2", row.Locations[8].RowLocationSequence);
			AssertEquals("1-1", row.Locations[9].RowLocationSequence);
		}

		public void TestLocationsSortedProperly()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "A", 1, 1, 10);
			Factory.Save();

			row.Locations.ApplySort(nameof(WhsLocation.WLV_LocationString), ListSortDirection.Ascending);
			AssertEquals("A-1-1-1", row.Locations[0].ToLocationString());
			AssertEquals("A-1-1-2", row.Locations[1].ToLocationString());
			AssertEquals("A-1-1-3", row.Locations[2].ToLocationString());
			AssertEquals("A-1-1-4", row.Locations[3].ToLocationString());
			AssertEquals("A-1-1-5", row.Locations[4].ToLocationString());
			AssertEquals("A-1-1-6", row.Locations[5].ToLocationString());
			AssertEquals("A-1-1-7", row.Locations[6].ToLocationString());
			AssertEquals("A-1-1-8", row.Locations[7].ToLocationString());
			AssertEquals("A-1-1-9", row.Locations[8].ToLocationString());
			AssertEquals("A-1-1-10", row.Locations[9].ToLocationString());

			row.Locations.ApplySort(nameof(WhsLocation.WLV_LocationString), ListSortDirection.Descending);
			AssertEquals("A-1-1-10", row.Locations[0].ToLocationString());
			AssertEquals("A-1-1-9", row.Locations[1].ToLocationString());
			AssertEquals("A-1-1-8", row.Locations[2].ToLocationString());
			AssertEquals("A-1-1-7", row.Locations[3].ToLocationString());
			AssertEquals("A-1-1-6", row.Locations[4].ToLocationString());
			AssertEquals("A-1-1-5", row.Locations[5].ToLocationString());
			AssertEquals("A-1-1-4", row.Locations[6].ToLocationString());
			AssertEquals("A-1-1-3", row.Locations[7].ToLocationString());
			AssertEquals("A-1-1-2", row.Locations[8].ToLocationString());
			AssertEquals("A-1-1-1", row.Locations[9].ToLocationString());
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Warehouse = Helper.CreateWarehouse("2");
			Row = Helper.CreateRowAndGenerateLocations(Warehouse, "A", 4, 3, 2);
		}

		protected override WhsLocationCollection GetCollectionToTest()
		{
			return new WhsLocationCollection(Factory.New<WhsRow>());
		}

		WhsWarehouse Warehouse;
		WhsRow Row;

		#endregion
	}

	#endregion

	#region class WhsLocationCollectionForWarehouseTest

	[TestedType(typeof(WhsLocationCollection))]
	public class WhsLocationCollectionForWarehouseTest : WhsActiveBusinessObjectCollectionTestCase<WhsLocationCollection>
	{
		#region TestWarehouse

		public void TestWarehouse()
		{
			AssertEquals(Warehouse, Collection.RelationshipForWarehouse.Warehouse);
			AssertEquals(Warehouse, Collection.RelationshipForWarehouse.Master);
		}

		#endregion

		#region TestFindBoxListProvider

		public void TestFindBoxListProvider()
		{
			var whs = Helper.CreateWarehouse("BigWhs");
			var row = Helper.CreateRow(whs, "Row", 1, 1);
			var collection = new WhsLocationCollectionForTest(whs);
			Factory.Save(); // this will generate the first location for the Warehouse
			AssertEquals(row.Locations[0].PK, collection.FindBoxListProvider.PrimaryKeyFromCode("Row"));
			AssertEquals(ZGuid.Invalid, collection.FindBoxListProvider.PrimaryKeyFromCode("Row-1"));
			AssertEquals(ZGuid.Invalid, collection.FindBoxListProvider.PrimaryKeyFromCode("Row-1-1"));
			AssertEquals(ZGuid.Invalid, collection.FindBoxListProvider.PrimaryKeyFromCode("CRAP"));

			var collectionWithNoWhs = new WhsLocationCollectionForTest(Factory);
			AssertEquals(ZGuid.Invalid, collectionWithNoWhs.FindBoxListProvider.PrimaryKeyFromCode("CRAP"));
		}

		#endregion

		#region TestRelationship

		public void TestRelationship()
		{
			var row = Warehouse.Rows.AddNew();
			row.WR_Name = "A";
			WhsWarehouse randomWarehouse = Helper.CreateWarehouse("RandomWhs", "RandomRow");
			Factory.Save(); // this will generate the first location for each Warehouse

			AssertCollectionContains(Warehouse.Rows[0].Locations[0], Collection);
			AssertCollectionNotContains(randomWarehouse.Rows[0].Locations[0], Collection);
		}

		#endregion

		#region TestApplyVoidStatusFilter

		public void TestApplyVoidStatusFilter()
		{
			var warehouse1 = Helper.CreateWarehouse("W1", "A", 1, 2);
			var warehouse2 = Helper.CreateWarehouse("W2", "B", 1, 3);
			Factory.Save();

			var normalLocationInWarehouse1 = warehouse1.FindLocation("A-1-1");
			var voidLocationInWarehouse1 = warehouse1.FindLocation("A-1-2");
			var damagedLocationInWarehouse2 = warehouse2.FindLocation("B-1-2");
			var voidLocationInWarehouse2 = warehouse2.FindLocation("B-1-3");
			normalLocationInWarehouse1.WLV_LocationStatus = LocationStatus.Codes.Normal;
			voidLocationInWarehouse1.WLV_LocationStatus = LocationStatus.Codes.Void;
			damagedLocationInWarehouse2.WLV_LocationStatus = LocationStatus.Codes.Damaged;
			voidLocationInWarehouse2.WLV_LocationStatus = LocationStatus.Codes.Void;
			Factory.Save();

			AssertCollectionNotContains(new[] { voidLocationInWarehouse1, voidLocationInWarehouse2 }, new WhsLocationCollection(Factory));
			AssertCollectionNotContains(new[] { voidLocationInWarehouse1 }, new WhsLocationCollection(warehouse1));
			AssertCollectionNotContains(new[] { voidLocationInWarehouse2 }, new WhsLocationCollection(warehouse2));
		}

		#endregion

		#region TestFilterBusinessObjectDefaults

		public void TestFilterBusinessObjectDefaults()
		{
			FilterBusinessObjectDefault warehouseDefault = Collection.FilterBusinessObjectDefaults[WhsLocationCollection.FilterSchema.Warehouse + ":Property"];
			AssertEquals("Property", warehouseDefault.PropertyName);
			AssertEquals(Warehouse.PK, warehouseDefault.Value);
			AssertEquals(false, warehouseDefault.IsRemovable);
		}

		public void TestFilterBusinessObjectDefaults_DockDoorLocations()
		{
			var collectionWithWarehouse = new WhsLocationCollection(Warehouse, true);
			var dockDoorLocationsFilter = collectionWithWarehouse.FilterBusinessObjectDefaults[WhsLocationCollection.FilterSchema.DockDoorLocation + ":Property0"];
			AssertEquals(true, dockDoorLocationsFilter.Value);
			AssertEquals(false, dockDoorLocationsFilter.IsRemovable);

			var collectionWithoutWarehouse = new WhsLocationCollection(Factory, true);
			dockDoorLocationsFilter = collectionWithoutWarehouse.FilterBusinessObjectDefaults[WhsLocationCollection.FilterSchema.DockDoorLocation + ":Property0"];
			AssertEquals(true, dockDoorLocationsFilter.Value);
			AssertEquals(false, dockDoorLocationsFilter.IsRemovable);
		}

		#endregion

		#region Implementation

		protected new WhsLocationCollection Collection
		{
			get { return base.Collection; }
		}

		protected override WhsLocationCollection GetCollectionToTest()
		{
			return new WhsLocationCollection(Warehouse);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			// relationship uses a dbonlyquery so we'll need to save it to the DB.

			WhsLocation location;

			if (Warehouse.Rows.Count == 0)
			{
				var row = Warehouse.Rows.AddNew();
				row.WR_Name = "A";
				Warehouse.WW_IsVirtualWarehouse = true; // to prevent creating dock door locations
				Factory.Save(); // this will auto-generate the first location
				location = Warehouse.Rows[0].Locations[0];
			}
			else
			{
				Warehouse.Rows[0].WR_Columns++;
				((WhsRowInternals)Warehouse.Rows[0]).GenerateLocations();
				location = Warehouse.Rows[0].Locations.Last();
				Factory.Save();
			}

			return location;
		}

		protected WhsWarehouse Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					var branch = Helper.CreateGlbBranch("BR1");
					var client = Helper.CreateClient("CLIENT");
					warehouse = Helper.CreateWarehouse("WH1", client.MainAddress, branch, shouldPreGenerateDDL: false);
					warehouse.WW_IsVirtualWarehouse = true; // to prevent creation of DDL
				}

				return warehouse;
			}
		}

		WhsWarehouse warehouse;

		#endregion
	}

	#endregion

	#region class WhsLocationAreaCollectionTest

	[TestedType(typeof(WhsLocationAreaCollection))]
	public class WhsLocationAreaCollectionTest : WhsActiveBusinessObjectCollectionTestCase<WhsLocationAreaCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)GetCollectionToTest()).AllowNew);
		}

		#region Implementation

		protected override WhsLocationAreaCollection GetCollectionToTest()
		{
			var area = Factory.New<WhsArea>();
			return new WhsLocationAreaCollection(area, Factory, WhsLocationViewSchema.WLV_WA_PickingArea);
		}

		#endregion
	}

	#endregion

	#region class WhsLocationCollectionForTest

	class WhsLocationCollectionForTest : WhsLocationCollection
	{
		public WhsLocationCollectionForTest(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsLocationCollectionForTest(WhsWarehouse warehouse)
			: base(warehouse)
		{
		}

		public new IFindBoxListProvider FindBoxListProvider
		{
			get { return base.FindBoxListProvider; }
		}
	}

	#endregion

	#region class GetEmptyWhsLocationCollectionTest

	class GetEmptyWhsLocationCollectionTest : WhsTestCaseWithFactoryEnv
	{
		#region TestRelationship

		public void TestRelationship()
		{
			var warehouse = Helper.CreateWarehouse("WHS", "A", 2, 2);
			Factory.Save();

			var location1 = warehouse.FindLocation("A-1-1");
			var location2 = warehouse.FindLocation("A-1-2");
			var location3 = warehouse.FindLocation("A-2-1");
			var location4 = warehouse.FindLocation("A-2-2");

			AssertNotNull("Location should have been created and not be null", location1);
			AssertNotNull("Location should have been created and not be null", location2);
			AssertNotNull("Location should have been created and not be null", location3);
			AssertNotNull("Location should have been created and not be null", location4);

			AssertEquals("The WhsLocationCollection should be empty", 0, WhsLocationCollection.GetEmptyLocationCollection(Factory).Count);
		}

		#endregion

		#region TestAdd

		public void TestAdd()
		{
			var bizO1 = Factory.New<WhsLocation>();
			var bizO2 = Factory.New<WhsLocation>();

			var collection = WhsLocationCollection.GetEmptyLocationCollection(Factory);
			AssertExceptionThrown(typeof(InvalidOperationException), () => collection.Add(bizO1));
			AssertExceptionThrown(typeof(InvalidOperationException), () => collection.Add(bizO2));

			AssertEquals("Collection count", 0, collection.Count);
		}

		#endregion

		#region TestDelete

		public void TestDelete()
		{
			var bizO1 = Factory.New<WhsLocation>();
			var bizO2 = Factory.New<WhsLocation>();

			var collection = WhsLocationCollection.GetEmptyLocationCollection(Factory);
			AssertEquals("Precondition : Collection count", 0, collection.Count);

			collection.Delete(bizO1);

			AssertEquals("Collection count", 0, collection.Count);
			Assert("Element 1 was removed, and deleted", bizO1.IsDeleted);
		}

		#endregion
	}

	#endregion
}
