using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing.SQL
{
	sealed class Report_PeakSpeedTest : TestCaseWithFactory
	{
		#region TestPeakSpeedFunction_BasicTest

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_BasicTest()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck1 = SetupDataVehicleDeviceAndLocationData(sql, "V1", "truck1-XYZ", "device1", timeFromUtc, 115, 120, 140, 90);
			var truck2 = SetupDataVehicleDeviceAndLocationData(sql, "V2", "truck2-ABC", "device2", timeFromUtc, 100, 102, 90, 97);
			var truck3 = SetupDataVehicleDeviceAndLocationData(sql, "V3", "truck3-123", "device3", timeFromUtc, 89, 75, 110, 110);
			var driver1 = Helper.CreateDriver("Bob", "Bob");
			var driver2 = Helper.CreateDriver("Ted", "Ted");
			var driver3 = Helper.CreateDriver("Sam", "Sam");

			var consignmentRunSheet1 = CreateConsignmentRunSheet("1", truck1, driver1.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet1, truck1);

			var consignmentRunSheet2 = CreateConsignmentRunSheet("2", truck2, driver1.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet2, truck2);

			var consignmentRunSheet3 = CreateConsignmentRunSheet("3", truck2, driver2.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet3, truck2);

			var consignmentRunSheet4 = CreateConsignmentRunSheet("4", truck1, driver3.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet4, truck1);

			var consignmentRunSheet5 = CreateConsignmentRunSheet("5", truck3, driver3.GS_Code, timeFromUtc.AddYears(-2), timeFromUtc.AddYears(-1)); // Out of range
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet5, truck3);

			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 4 results.", 4, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver1.GS_FullName, "truck1-XYZ", 120, timeFromUtc.AddHours(3));
			AssertResultsOfPeakSpeedFunction(results[1], driver1.GS_FullName, "truck2-ABC", 97, timeFromUtc.AddHours(12));
			AssertResultsOfPeakSpeedFunction(results[2], driver3.GS_FullName, "truck1-XYZ", 140, timeFromUtc.AddHours(9));
			AssertResultsOfPeakSpeedFunction(results[3], driver2.GS_FullName, "truck2-ABC", 102, timeFromUtc.AddHours(3));
		}

		#endregion

		#region TestPeakSpeedFunction_DriverAndTruckVariations

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_DriverAndTruckVariations_OneDriver_OneTruck_TwoRunSheets()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver = Helper.CreateDriver("Bob", "Bob");

			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck.PK, timeFromUtc, timeFromUtc.AddHours(6), 90, 120, 100);
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device2", truck.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12), 90, 80, 70);

			var consignmentRunSheet1 = CreateConsignmentRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet1, truck);

			var consignmentRunSheet2 = CreateConsignmentRunSheet("2", truck, driver.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet2, truck);

			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 120, timeFromUtc.AddHours(3));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_DriverAndTruckVariations_OneDriver_TwoTrucks_TwoRunSheets()
		{
			var auBranch = GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Australia, Factory).First();
			auBranch.GB_RL_NKHomePort = "AUSYD";
			using (DisposableEnvironment.ForBranch(auBranch.PK.ToGuid()))
			{
				var sql = new ZStringBuilder();
				TestDateAttribute.UseUNLOCO = true;
				var timeFromUtc = ZDateTime.UtcNow;

				var truck1 = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
				var truck2 = Helper.CreateVehicleWithEquipmentType("V2", "truck2-ABC");
				var driver = Helper.CreateDriver("Bob", "Bob");

				SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck1.PK, timeFromUtc, timeFromUtc.AddHours(6), 80, 100, 120);
				SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device2", truck2.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12), 111, 110, 90);

				var consignmentRunSheet1 = CreateConsignmentRunSheet("1", truck1, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
				CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet1, truck1);

				var consignmentRunSheet2 = CreateConsignmentRunSheet("2", truck2, driver.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
				CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet2, truck2);

				Factory.Save();
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

				var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
				AssertEquals($"Expecting to find 2 results.", 2, results.Count);
				AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 120, timeFromUtc.AddHours(6).AddMinutes(-10));
				AssertResultsOfPeakSpeedFunction(results[1], driver.GS_FullName, "truck2-ABC", 111, timeFromUtc.AddHours(6).AddMinutes(10));
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_DriverAndTruckVariations_TwoDrivers_OneTruck_TwoRunSheets()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck1 = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver1 = Helper.CreateDriver("Bob", "Bob");
			var driver2 = Helper.CreateDriver("Sam", "Sam");

			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck1.PK, timeFromUtc, timeFromUtc.AddHours(12), 70, 120, 121);

			var consignmentRunSheet1 = CreateConsignmentRunSheet("1", truck1, driver1.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet1, truck1);

			var consignmentRunSheet2 = CreateConsignmentRunSheet("2", truck1, driver2.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet2, truck1);

			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 2 results.", 2, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver1.GS_FullName, "truck1-XYZ", 120, timeFromUtc.AddHours(6));
			AssertResultsOfPeakSpeedFunction(results[1], driver2.GS_FullName, "truck1-XYZ", 121, timeFromUtc.AddHours(12).AddMinutes(-10));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_DriverAndTruckVariations_TwoDrivers_TwoTrucks_TwoRunSheets()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck1 = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var truck2 = Helper.CreateVehicleWithEquipmentType("V2", "truck2-ABC");
			var driver1 = Helper.CreateDriver("Bob", "Bob");
			var driver2 = Helper.CreateDriver("Sam", "Sam");

			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck1.PK, timeFromUtc, timeFromUtc.AddHours(12), 60, 70, 90);
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device2", truck2.PK, timeFromUtc, timeFromUtc.AddHours(12), 70, 80, 100);

			var consignmentRunSheet1 = CreateConsignmentRunSheet("1", truck1, driver1.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet1, truck1);

			var consignmentRunSheet2 = CreateConsignmentRunSheet("2", truck2, driver2.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet2, truck2);

			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 2 results.", 2, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver1.GS_FullName, "truck1-XYZ", 90, timeFromUtc.AddHours(12).AddMinutes(-10));
			AssertResultsOfPeakSpeedFunction(results[1], driver2.GS_FullName, "truck2-ABC", 100, timeFromUtc.AddHours(12).AddMinutes(-10));
		}

		#endregion

		#region TestPeakSpeedFunction_EquipmentItemListVariations

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_EquipmentItemListVariations_TruckFromEquipmentItemsListUsed()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck1 = SetupDataVehicleDeviceAndLocationData(sql, "V1", "truck1-XYZ", "device1", timeFromUtc, 115, 120, 140, 90);
			var truck2 = SetupDataVehicleDeviceAndLocationData(sql, "V2", "truck2-ABC", "device2", timeFromUtc, 100, 102, 90, 97);
			var driver = Helper.CreateDriver("Bob", "Bob");

			var consignmentRunSheet = CreateConsignmentRunSheet("1", truck2, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet, truck1);

			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 140, timeFromUtc.AddHours(9));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_EquipmentItemListVariations_TruckNotInEquipmentItemsList()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck1 = SetupDataVehicleDeviceAndLocationData(sql, "V1", "truck1-XYZ", "device1", timeFromUtc, 115, 120, 140, 90);
			var truck2 = SetupDataVehicleDeviceAndLocationData(sql, "V2", "truck2-ABC", "device2", timeFromUtc, 100, 102, 90, 97);
			var driver1 = Helper.CreateDriver("Bob", "Bob");
			var driver2 = Helper.CreateDriver("Sam", "Sam");

			var consignmentRunSheet = CreateConsignmentRunSheet("1", truck1, driver1.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet, truck1);
			CreateConsignmentRunSheet("2", truck2, driver2.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));

			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver1.GS_FullName, "truck1-XYZ", 140, timeFromUtc.AddHours(9));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_EquipmentItemListVariations_TwoTrucksInEquipmentItemsList()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck2 = SetupDataVehicleDeviceAndLocationData(sql, "V2", "truck2-ABC", "device2", timeFromUtc, 100, 102, 90, 97);
			var truck1 = SetupDataVehicleDeviceAndLocationData(sql, "V3", "truck1-XYZ", "device1", timeFromUtc, 115, 120, 140, 90);
			var truck3 = SetupDataVehicleDeviceAndLocationData(sql, "V1", "truck3-DEF", "device3", timeFromUtc, 115, 120, 140, 90);
			var driver = Helper.CreateDriver("Bob", "Bob");

			var consignmentRunSheet = CreateConsignmentRunSheet("1", null, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet, truck2);
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet, truck1);
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet, truck3);

			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 140, timeFromUtc.AddHours(9));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_EquipmentItemListVariations_NoTrucksInEquipmentItemsList()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck = SetupDataVehicleDeviceAndLocationData(sql, "V1", "truck1-XYZ", "device1", timeFromUtc, 115, 120, 140, 90);
			var driver = Helper.CreateDriver("Bob", "Bob");

			var consignmentRunSheet = CreateConsignmentRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			var equipment1 = CreateNonVehicleEquipment("E1", "equipment1");
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet, equipment1);
			var equipment2 = CreateNonVehicleEquipment("E2", "equipment2");
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet, equipment2);
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find no results.", false, results.Any());
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_EquipmentItemListVariations_EmptyEquipmentItemsList()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck = SetupDataVehicleDeviceAndLocationData(sql, "V1", "truck1-XYZ", "device1", timeFromUtc, 115, 120, 140, 90);
			var driver = Helper.CreateDriver("Bob", "Bob");

			CreateConsignmentRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));

			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find no results.", false, results.Any());
		}

		RefEquipment CreateNonVehicleEquipment(ZString equipmentCode, ZString registrationCode)
		{
			var equipment = Factory.New<RefEquipment>();
			var equipmentType = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "STEEL").PK;
			equipment.RQ_RC_RoadContainerType = equipmentType;
			equipment.RQ_ShortCode = equipmentCode;
			equipment.RQ_Registration = registrationCode;
			equipment.RQ_IsVehicle = false;

			return equipment;
		}

		#endregion

		#region TestPeakSpeedFunction_TruckAndDeviceVariations

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_TruckAndDeviceVariations_TwoDrivers_OneTruck_TwoDevices_TwoRunSheets()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck1 = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver1 = Helper.CreateDriver("Bob", "Bob");
			var driver2 = Helper.CreateDriver("Sam", "Sam");

			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck1.PK, timeFromUtc, timeFromUtc.AddHours(6), 110, 130, 100);
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device2", truck1.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12), 131, 125, 36);

			var consignmentRunSheet1 = CreateConsignmentRunSheet("1", truck1, driver1.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet1, truck1);

			var consignmentRunSheet2 = CreateConsignmentRunSheet("2", truck1, driver2.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet2, truck1);

			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 2 results.", 2, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver1.GS_FullName, "truck1-XYZ", 130, timeFromUtc.AddHours(3));
			AssertResultsOfPeakSpeedFunction(results[1], driver2.GS_FullName, "truck1-XYZ", 131, timeFromUtc.AddHours(6).AddMinutes(10));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_DriverAndTruckVariations_OneDriver_OneTruck_TwoDevices_OneRunSheet()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver = Helper.CreateDriver("Bob", "Bob");

			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck.PK, timeFromUtc, timeFromUtc.AddHours(6), 60, 90, 86);
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device2", truck.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12), 92, 85, 30);

			var consignmentRunSheet = CreateConsignmentRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet, truck);
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 92, timeFromUtc.AddHours(6).AddMinutes(10));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_TruckAndDeviceVariations_OneDriver_OneTruck_OneDevice_TwoRunSheets()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver = Helper.CreateDriver("Bob", "Bob");

			var deviceID = CreateGlbDevice(sql, "device1");
			CreateGlbDeviceAssignmentDivot(sql, deviceID, truck.PK, timeFromUtc, timeFromUtc.AddHours(6));
			CreateGlbDeviceAssignmentDivot(sql, deviceID, truck.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));

			CreateGlbDeviceLocation(sql, deviceID, 90m, timeFromUtc);
			CreateGlbDeviceLocation(sql, deviceID, 100m, timeFromUtc.AddHours(3));
			CreateGlbDeviceLocation(sql, deviceID, 80m, timeFromUtc.AddHours(9));
			CreateGlbDeviceLocation(sql, deviceID, 110m, timeFromUtc.AddHours(12));

			var consignmentRunSheet1 = CreateConsignmentRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet1, truck);

			var consignmentRunSheet2 = CreateConsignmentRunSheet("2", truck, driver.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet2, truck);

			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], "Bob", "truck1-XYZ", 110, timeFromUtc.AddHours(12));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_TruckAndDeviceVariations_TwoDrivers_TwoTrucks_OneDevice_TwoRunSheets()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck1 = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var truck2 = Helper.CreateVehicleWithEquipmentType("V2", "truck2-ABC");
			var driver1 = Helper.CreateDriver("Bob", "Bob");
			var driver2 = Helper.CreateDriver("Sam", "Sam");

			var deviceID = CreateGlbDevice(sql, "device1");
			CreateGlbDeviceAssignmentDivot(sql, deviceID, truck1.PK, timeFromUtc, timeFromUtc.AddHours(6));
			CreateGlbDeviceAssignmentDivot(sql, deviceID, truck2.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));

			CreateGlbDeviceLocation(sql, deviceID, 90m, timeFromUtc);
			CreateGlbDeviceLocation(sql, deviceID, 100m, timeFromUtc.AddHours(3));
			CreateGlbDeviceLocation(sql, deviceID, 80m, timeFromUtc.AddHours(9));
			CreateGlbDeviceLocation(sql, deviceID, 110m, timeFromUtc.AddHours(12));

			var consignmentRunSheet1 = CreateConsignmentRunSheet("1", truck1, driver1.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet1, truck1);

			var consignmentRunSheet2 = CreateConsignmentRunSheet("2", truck2, driver2.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet2, truck2);
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 2 results.", 2, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver1.GS_FullName, "truck1-XYZ", 100, timeFromUtc.AddHours(3));
			AssertResultsOfPeakSpeedFunction(results[1], driver2.GS_FullName, "truck2-ABC", 110, timeFromUtc.AddHours(12));
		}

		#endregion

		#region TestPeakSpeedFunction_TimeVariations

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_TimeVariations_SearchRangeCoversRunSheetRange()
		{
			var timeFromUtc = ZDateTime.UtcNow;
			var timeToUtc = timeFromUtc.AddHours(12);

			TestPeakSpeedFunction_TimeVariations_Setup(timeFromUtc, timeToUtc);
			// Search range covers the whole run sheet range
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeToUtc);
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], "Bob", "truck1-XYZ", 110, timeFromUtc.AddHours(12));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_TimeVariations_SearchRangeInsideRunSheetRange()
		{
			var timeFromUtc = ZDateTime.UtcNow;
			var timeToUtc = timeFromUtc.AddHours(12);

			TestPeakSpeedFunction_TimeVariations_Setup(timeFromUtc, timeToUtc);
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc.AddHours(5), timeFromUtc.AddHours(6));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], "Bob", "truck1-XYZ", 90, timeFromUtc.AddHours(6));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_TimeVariations_SearchRangeStartsBeforeRunSheetEnds()
		{
			var timeFromUtc = ZDateTime.UtcNow;
			var timeToUtc = timeFromUtc.AddHours(12);

			TestPeakSpeedFunction_TimeVariations_Setup(timeFromUtc, timeToUtc);
			var results = GetPeakSpeedBetweenTimeRange(timeToUtc.AddHours(-1), timeToUtc.AddHours(1));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], "Bob", "truck1-XYZ", 110, timeFromUtc.AddHours(12));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_TimeVariations_SearchRangeStartsBeforeRunSheetStarts()
		{
			var timeFromUtc = ZDateTime.UtcNow;

			TestPeakSpeedFunction_TimeVariations_Setup(timeFromUtc, timeFromUtc.AddHours(12));
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc.AddHours(-1), timeFromUtc.AddHours(1));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], "Bob", "truck1-XYZ", 70, timeFromUtc.AddHours(1));
		}

		void TestPeakSpeedFunction_TimeVariations_Setup(ZDateTime timeFromUtc, ZDateTime timeToUtc)
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;

			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver = Helper.CreateDriver("Bob", "Bob");
			var consignmentRunSheet = CreateConsignmentRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeToUtc);
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet, truck);
			var deviceID = SetupDeviceWithAssignmentDivot(sql, "device1", truck.PK, timeFromUtc, timeToUtc);

			CreateGlbDeviceLocation(sql, deviceID, 140m, timeFromUtc.AddHours(-10));

			CreateGlbDeviceLocation(sql, deviceID, 60m, timeFromUtc);
			CreateGlbDeviceLocation(sql, deviceID, 70m, timeFromUtc.AddHours(1));

			CreateGlbDeviceLocation(sql, deviceID, 80m, timeFromUtc.AddHours(5));
			CreateGlbDeviceLocation(sql, deviceID, 90m, timeFromUtc.AddHours(6));

			CreateGlbDeviceLocation(sql, deviceID, 100m, timeFromUtc.AddHours(11));
			CreateGlbDeviceLocation(sql, deviceID, 110m, timeFromUtc.AddHours(12));

			CreateGlbDeviceLocation(sql, deviceID, 140m, timeFromUtc.AddHours(22));

			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
		}

		#endregion

		#region TestPeakSpeedFunction_DivotVariations

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_TimeVariations_SearchRangeEndsInTheMiddleOfTwoDivots()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver = Helper.CreateDriver("Bob", "Bob");

			var devicePK = CreateGlbDevice(sql, "device1");
			CreateGlbDeviceAssignmentDivot(sql, devicePK, truck.PK, timeFromUtc, timeFromUtc.AddHours(6));
			CreateGlbDeviceAssignmentDivot(sql, devicePK, truck.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			var consignmentRunSheet = CreateConsignmentRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet, truck);
			CreateGlbDeviceLocation(sql, devicePK, 130m, timeFromUtc.AddHours(3));
			CreateGlbDeviceLocation(sql, devicePK, 145m, timeFromUtc.AddHours(9));
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc.AddHours(-6), timeFromUtc.AddHours(6));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 130, timeFromUtc.AddHours(3));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_TimeVariations_SearchRangeStartsInTheMiddleOfTwoDivots()
		{
			var sql = new ZStringBuilder();

			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;

			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver = Helper.CreateDriver("Bob", "Bob");

			var devicePK = CreateGlbDevice(sql, "device1");
			CreateGlbDeviceAssignmentDivot(sql, devicePK, truck.PK, timeFromUtc, timeFromUtc.AddHours(6));
			CreateGlbDeviceAssignmentDivot(sql, devicePK, truck.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			var consignmentRunSheet = CreateConsignmentRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet, truck);
			CreateGlbDeviceLocation(sql, devicePK, 145m, timeFromUtc.AddHours(3));
			CreateGlbDeviceLocation(sql, devicePK, 130m, timeFromUtc.AddHours(9));
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc.AddHours(6), timeFromUtc.AddHours(18));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 130, timeFromUtc.AddHours(9));
		}

		#endregion

		#region TestRunSheetExtendsOverDayLightSavings

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestRunSheetExtendsOverDayLightSavings()
		{
			var sql = new ZStringBuilder();

			TestDateAttribute.UseUNLOCO = true;
			var timeFromLocal = new ZDateTime(2010, 4, 4, 1, 0, 0, 0);
			var timeToLocal = new ZDateTime(2010, 4, 4, 3, 0, 0, 0);

			var timeFromLocalWithOffset = new ZDateTimeOffset(new ZDateTime(timeFromLocal, DateTimeKind.Unspecified), DateTimeKind.Local);
			var timeToLocalWithOffset = new ZDateTimeOffset(new ZDateTime(timeToLocal, DateTimeKind.Unspecified), DateTimeKind.Local);
			var timeFromUtc = timeFromLocal - timeFromLocalWithOffset.Offset;
			var timeToUtc = timeToLocal - timeToLocalWithOffset.Offset;

			var driver = Helper.CreateDriver("Bob", "Bob");
			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");

			var consignment = Helper.CreateConsignment("1");
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi);
			var deliveryAction = Helper.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);

			var runSheet = Helper.CreateRunSheet(null, null, truck.PK, timeFromLocalWithOffset, timeToLocalWithOffset);
			runSheet.KG_GS_NKTruckDriver = driver.GS_Code;
			Helper.CreateRunSheetInstruction(runSheet, deliveryAction);
			CreateConsignmentRunSheetEquipmentItem(sql, runSheet.PK, truck);

			var deviceID = SetupDeviceWithAssignmentDivot(sql, "device1", truck.PK, new ZDateTime(2010, 4, 3, 14, 0, 0, 0), new ZDateTime(2010, 4, 4, 17, 0, 0, 0));
			CreateGlbDeviceLocation(sql, deviceID, 50m, new ZDateTime(2010, 4, 3, 14, 0, 0, 0));
			CreateGlbDeviceLocation(sql, deviceID, 60m, new ZDateTime(2010, 4, 3, 15, 0, 0, 0));
			CreateGlbDeviceLocation(sql, deviceID, 140m, new ZDateTime(2010, 4, 3, 17, 0, 0, 0));

			// Add time zone periods
			Db.Connection.ExecuteNonQuery($@"INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES ('{ZGuid.NewZGuid()}', 
				'AUSYD', '2009-10-04 03:00:00', '2010-04-04 02:00:00', '11'), ('{ZGuid.NewZGuid()}','AUSYD', '2010-04-04 02:00:00', '2010-10-03 03:00:00', 600)");

			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeToUtc);
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 140, new ZDateTime(2010, 4, 3, 17, 0, 0, 0));
		}

		#endregion

		#region TestNoDriverOnRunSheet
		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestNoDriverOnRunSheet()
		{
			var sql = new ZStringBuilder();

			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			var timeToUtc = timeFromUtc.AddHours(12);

			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck.PK, timeFromUtc, timeToUtc, 80, 90, 75);
			var consignmentRunSheet = CreateConsignmentRunSheet("1", truck, ZString.Empty, timeFromUtc, timeToUtc);
			CreateConsignmentRunSheetEquipmentItem(sql, consignmentRunSheet, truck);
			Factory.Save();

			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeToUtc);
			AssertEquals($"Expecting to find 0 results.", 0, results.Count);
		}

		#endregion

		#region HelperMethods

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		RefEquipment SetupDataVehicleDeviceAndLocationData(ZStringBuilder sql, ZString vehicleCode, ZString registrationCode, ZString deviceName, ZDateTime timeFromUtc, int speedNow, int speedIn3Hours, int speedIn9Hours, int speedIn12Hours)
		{
			var truck = Helper.CreateVehicleWithEquipmentType(vehicleCode, registrationCode);
			var devicePK = CreateGlbDevice(sql, deviceName);
			CreateGlbDeviceAssignmentDivot(sql, devicePK, truck.PK, timeFromUtc, timeFromUtc.AddHours(12));
			CreateGlbDeviceLocation(sql, devicePK, speedNow, timeFromUtc);
			CreateGlbDeviceLocation(sql, devicePK, speedIn3Hours, timeFromUtc.AddHours(3));
			CreateGlbDeviceLocation(sql, devicePK, speedIn9Hours, timeFromUtc.AddHours(9));
			CreateGlbDeviceLocation(sql, devicePK, speedIn12Hours, timeFromUtc.AddHours(12));

			return truck;
		}

		void AssertResultsOfPeakSpeedFunction(DynamicBusinessObject row, ZString expectedDriverName, ZString expectedRegistrationNumber, decimal expectedPeakSpeed, ZDateTime expectedTime)
		{
			var registrationNumber = (ZString)row["RegistrationNumber"];
			var driverName = (ZString)row["DriverName"];
			var peakSpeed = row["PeakSpeed"];
			var peakSpeedTime = row["PeakSpeedTime"];

			AssertEquals($"Expecting to find a registration number of {expectedRegistrationNumber}.", expectedRegistrationNumber, registrationNumber);
			AssertEquals($"Expecting to find a driver name of {expectedDriverName}.", expectedDriverName, driverName);
			AssertEquals($"Expecting to find a peak speed of {expectedPeakSpeed}.", expectedPeakSpeed, peakSpeed);
			AssertEquals($"Expecting to find peak speed at {peakSpeedTime}.", expectedTime, peakSpeedTime);
		}

		DynamicBusinessObjectCollection GetPeakSpeedBetweenTimeRange(ZDateTime timeFromUtc, ZDateTime timeToUtc)
		{
			var timeFromLocalDateTimeOffset = new ZDateTimeOffset(timeFromUtc, DateTimeKind.Local).Offset;
			var timeToLocalDateTimeOffset = new ZDateTimeOffset(timeToUtc, DateTimeKind.Local).Offset;
			var timeFromLocal = timeFromUtc + timeFromLocalDateTimeOffset;
			var timeToLocal = timeToUtc + timeToLocalDateTimeOffset;

			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = $@"SELECT	* FROM Report_PeakSpeedReport('{timeFromLocal.SqlFormat}', '{timeToLocal.SqlFormat}') ORDER BY DriverName, RegistrationNumber";

			result.Load(sql);
			return result;
		}

		ZGuid SetupDeviceWithAssignmentDivot(ZStringBuilder sql, ZString deviceName, ZGuid truckID, ZDateTime timeFromUtc, ZDateTime timeToUtc)
		{
			var deviceID = CreateGlbDevice(sql, deviceName);
			CreateGlbDeviceAssignmentDivot(sql, deviceID, truckID, timeFromUtc, timeToUtc);
			return deviceID;
		}

		void SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(ZStringBuilder sql, ZString deviceName, ZGuid truckID, ZDateTime timeFromUtc, ZDateTime timeToUtc, int speedIn10Mins, int speed2, int speedBefore10Mins)
		{
			var deviceID = CreateGlbDevice(sql, deviceName);
			CreateGlbDeviceAssignmentDivot(sql, deviceID, truckID, timeFromUtc, timeToUtc);

			CreateGlbDeviceLocation(sql, deviceID, speedIn10Mins, timeFromUtc.AddMinutes(10));
			CreateGlbDeviceLocation(sql, deviceID, speed2, timeFromUtc.AddHours((timeToUtc.Hour - timeFromUtc.Hour) / 2));
			CreateGlbDeviceLocation(sql, deviceID, speedBefore10Mins, timeToUtc.AddMinutes(-10));
		}

		ZGuid CreateConsignmentRunSheet(ZString jobID, RefEquipment truck, ZString driverCode, ZDateTime startTimeUtc, ZDateTime endTimeUtc)
		{
			var startTimeLocal = ConvertUtcDateTimeToZDateTimeOffset(startTimeUtc);
			var endTimeLocal = ConvertUtcDateTimeToZDateTimeOffset(endTimeUtc);

			var consignment = Helper.CreateConsignment(jobID);
			Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp);
			var deliveryAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Multi);
			var deliveryAction = Helper.CreateConsignmentAction(deliveryAddress, ActionTypes.Codes.Delivery);

			var runSheet = Helper.CreateRunSheet(null, null, truck?.PK, startTimeLocal, endTimeLocal);
			runSheet.KG_GS_NKTruckDriver = driverCode;
			Helper.CreateRunSheetInstruction(runSheet, deliveryAction);

			return runSheet.PK;
		}

		ZGuid CreateGlbDevice(ZStringBuilder sql, ZString deviceName)
		{
			var pk = ZGuid.NewZGuid();
			sql.Append($"INSERT INTO dbo.GlbDevice (V3_PK, V3_HumanReadableIdentifier, V3_Model, V3_SystemCreateUser, V3_MobileServicesIdentifier, V3_SystemCreateTimeUtc, V3_SystemLastEditTimeUtc, V3_SystemLastEditUser) values ('{pk}', '{deviceName}', 'model1', 'USR', CONVERT(varbinary, '{deviceName}'), GETDATE(), GETDATE(), 'USR')");
			return pk;
		}

		void CreateGlbDeviceAssignmentDivot(ZStringBuilder sql, ZGuid deviceID, ZGuid parentID, ZDateTime startTimeUtc, ZDateTime endTimeUtc)
		{
			sql.Append($"INSERT INTO dbo.GlbDeviceAssignmentDivot (V7_PK, V7_V3_Device, V7_ParentID, V7_ParentTableCode, V7_StartTimeUtc, V7_EndTimeUtc, V7_SystemCreateTimeUtc, V7_SystemCreateUser, V7_SystemLastEditTimeUtc, V7_SystemLastEditUser) values ('{ZGuid.NewZGuid()}', '{deviceID}', '{parentID}', '{RefEquipmentSchema.Constants.Prefix}', '{startTimeUtc}', '{endTimeUtc}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
		}

		void CreateGlbDeviceLocation(ZStringBuilder sql, ZGuid deviceID, decimal speedInKmh, ZDateTime time)
		{
			sql.Append($"INSERT INTO dbo.GlbDeviceLocation (V2_PK, V2_V3_Device, V2_Speedkmh, V2_MeasurementTimeUtc, V2_SystemCreateTimeUtc, V2_SystemCreateUser, V2_SystemLastEditTimeUtc, V2_SystemLastEditUser) values ('{ZGuid.NewZGuid()}', '{deviceID}', '{speedInKmh}', '{time}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
		}

		ZGuid CreateConsignmentRunSheetEquipmentItem(ZStringBuilder sql, ZGuid consignmentRunSheetPK, RefEquipment equipment)
		{
			var dtbEquipmentItem = new CargoWise.Database.TestFramework.ObjectModel.DtbEquipmentItem()
			{
				LTE_ParentID = consignmentRunSheetPK.ToGuid(),
				LTE_RQ_Equipment = equipment.PK.ToGuid(),
				LTE_RC_EquipmentType = equipment.RQ_RC_RoadContainerType.ToGuid(),
				LTE_EquipmentTypeQuantity = 1,
				LTE_ParentTableCode = "KG"
			};
			sql.Append(dtbEquipmentItem.GetInsertStatement());
			return dtbEquipmentItem.PK;
		}

		ZDateTimeOffset ConvertUtcDateTimeToZDateTimeOffset(ZDateTime dateTimeUtc)
		{
			return new ZDateTimeOffset(dateTimeUtc.Add(new ZDateTimeOffset(dateTimeUtc, DateTimeKind.Local).Offset), DateTimeKind.Local);
		}

		TransportConsignmentTestHelper Helper => helper ?? (helper = new TransportConsignmentTestHelper(Factory));
		TransportConsignmentTestHelper helper;

		protected override void SetUp()
		{
			base.SetUp();
			var objectCreator = new TestObjectCreator(Factory);
			var company = objectCreator.CreateNewCompany("C1", "AU");
			var branch = objectCreator.CreateBranch("B1", "Branch1", company);
			branch.GB_RL_NKHomePort = "AUSYD";
			var departmentGuid = GlbDepartment.CurrentDepartment.PK.ToGuid();

			userContext = Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), departmentGuid);
		}

		protected override void TearDown()
		{
			base.TearDown();
			userContext?.Dispose();
		}
		IDisposable userContext;

		#endregion
	}
}
