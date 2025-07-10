using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	class Report_PeakSpeedTest : TestCaseWithFactory
	{
		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_BasicTest()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			FixRLOOffset(timeFromUtc);
			var truck1 = SetupDataVehicleDeviceAndLocationData(sql, "V1", "truck1-XYZ", "device1", timeFromUtc, 115, 120, 140, 90);
			var truck2 = SetupDataVehicleDeviceAndLocationData(sql, "V2", "truck2-ABC", "device2", timeFromUtc, 100, 102, 90, 97);
			var truck3 = SetupDataVehicleDeviceAndLocationData(sql, "V3", "truck3-123", "device3", timeFromUtc, 89, 75, 110, 110);
			var driver1 = Helper.CreateDriver("Bob", "Bob");
			var driver2 = Helper.CreateDriver("Ted", "Ted");
			var driver3 = Helper.CreateDriver("Sam", "Sam");
			CreateJobCartageRunSheet("1", truck2, driver2.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
			CreateJobCartageRunSheet("2", truck1, driver3.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			CreateJobCartageRunSheet("3", truck3, driver3.GS_Code, timeFromUtc.AddYears(-2), timeFromUtc.AddYears(-1)); // Out of range
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 4 results.", 2, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver3.GS_FullName, "truck1-XYZ", 140);
			AssertResultsOfPeakSpeedFunction(results[1], driver2.GS_FullName, "truck2-ABC", 102);
		}

		RefEquipment SetupDataVehicleDeviceAndLocationData(ZStringBuilder sql, ZString vehicleCode, ZString registrationCode, ZString deviceName, ZDateTime timeFromUtc, int speed1, int speed2, int speed3, int speed4)
		{
			var truck = Helper.CreateVehicleWithEquipmentType(vehicleCode, registrationCode);
			var devicePK = CreateGlbDevice(sql, deviceName);
			CreateGlbDeviceAssignmentDivot(sql, devicePK, truck.PK, timeFromUtc, timeFromUtc.AddHours(12));
			CreateGlbDeviceLocation(sql, devicePK, speed1, timeFromUtc);
			CreateGlbDeviceLocation(sql, devicePK, speed2, timeFromUtc.AddHours(3));
			CreateGlbDeviceLocation(sql, devicePK, speed3, timeFromUtc.AddHours(9));
			CreateGlbDeviceLocation(sql, devicePK, speed4, timeFromUtc.AddHours(12));
			return truck;
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_DriverAndTruckVariations_OneDriver_OneTruck_TwoRunSheets()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			FixRLOOffset(timeFromUtc);
			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver = Helper.CreateDriver("Bob", "Bob");
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck.PK, timeFromUtc, timeFromUtc.AddHours(6), 90, 120, 100);
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device2", truck.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12), 90, 80, 70);
			CreateJobCartageRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
			CreateJobCartageRunSheet("2", truck, driver.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 120);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_DriverAndTruckVariations_OneDriver_TwoTrucks_TwoRunSheets()
		{
			var sql = new ZStringBuilder();
			var auBranch = GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Australia, Factory).First();
			auBranch.GB_RL_NKHomePort = "AUSYD";
			using (var environment = DisposableEnvironment.ForBranch(auBranch.PK.ToGuid()))
			{
				TestDateAttribute.UseUNLOCO = true;
				var timeFromUtc = ZDateTime.UtcNow;
				FixRLOOffset(timeFromUtc);
				var truck1 = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
				var truck2 = Helper.CreateVehicleWithEquipmentType("V2", "truck2-ABC");
				var driver = Helper.CreateDriver("Bob", "Bob");
				SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck1.PK, timeFromUtc, timeFromUtc.AddHours(6), 80, 100, 120);
				SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device2", truck2.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12), 111, 110, 90);
				CreateJobCartageRunSheet("1", truck1, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
				CreateJobCartageRunSheet("2", truck2, driver.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
				Factory.Save();
				Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
				var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
				AssertEquals($"Expecting to find 2 results.", 2, results.Count);
				AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 120);
				AssertResultsOfPeakSpeedFunction(results[1], driver.GS_FullName, "truck2-ABC", 111);
			}
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_DriverAndTruckVariations_TwoDrivers_OneTruck_TwoRunSheets()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			FixRLOOffset(timeFromUtc);
			var truck1 = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver1 = Helper.CreateDriver("Bob", "Bob");
			var driver2 = Helper.CreateDriver("Sam", "Sam");
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck1.PK, timeFromUtc, timeFromUtc.AddHours(12), 70, 120, 121);
			CreateJobCartageRunSheet("1", truck1, driver1.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
			CreateJobCartageRunSheet("2", truck1, driver2.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 2 results.", 2, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver1.GS_FullName, "truck1-XYZ", 120);
			AssertResultsOfPeakSpeedFunction(results[1], driver2.GS_FullName, "truck1-XYZ", 121);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_DriverAndTruckVariations_TwoDrivers_TwoTrucks_TwoRunSheets()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			FixRLOOffset(timeFromUtc);
			var truck1 = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var truck2 = Helper.CreateVehicleWithEquipmentType("V2", "truck2-ABC");
			var driver1 = Helper.CreateDriver("Bob", "Bob");
			var driver2 = Helper.CreateDriver("Sam", "Sam");
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck1.PK, timeFromUtc, timeFromUtc.AddHours(12), 60, 70, 90);
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device2", truck2.PK, timeFromUtc, timeFromUtc.AddHours(12), 70, 80, 100);
			CreateJobCartageRunSheet("1", truck1, driver1.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			CreateJobCartageRunSheet("2", truck2, driver2.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 2 results.", 2, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver1.GS_FullName, "truck1-XYZ", 90);
			AssertResultsOfPeakSpeedFunction(results[1], driver2.GS_FullName, "truck2-ABC", 100);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_TruckAndDeviceVariations_TwoDrivers_OneTruck_TwoDevices_TwoRunSheets()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			FixRLOOffset(timeFromUtc);
			var truck1 = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver1 = Helper.CreateDriver("Bob", "Bob");
			var driver2 = Helper.CreateDriver("Sam", "Sam");
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck1.PK, timeFromUtc, timeFromUtc.AddHours(6), 110, 130, 100);
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device2", truck1.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12), 131, 125, 36);
			CreateJobCartageRunSheet("1", truck1, driver1.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
			CreateJobCartageRunSheet("2", truck1, driver2.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 2 results.", 2, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver1.GS_FullName, "truck1-XYZ", 130);
			AssertResultsOfPeakSpeedFunction(results[1], driver2.GS_FullName, "truck1-XYZ", 131);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_DriverAndTruckVariations_OneDriver_OneTruck_TwoDevices_OneRunSheet()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			FixRLOOffset(timeFromUtc);
			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver = Helper.CreateDriver("Bob", "Bob");
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck.PK, timeFromUtc, timeFromUtc.AddHours(6), 60, 90, 86);
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device2", truck.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12), 92, 85, 30);
			CreateJobCartageRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 92);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_DriverAndTruckVariations_NoAssignmentEndDate()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			FixRLOOffset(timeFromUtc);
			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver = Helper.CreateDriver("Bob", "Bob");
			var deviceID = CreateGlbDevice(sql, "device1");
			CreateGlbDeviceAssignmentDivot(sql, deviceID, truck.PK, timeFromUtc.AddDays(-5));
			CreateGlbDeviceLocation(sql, deviceID, 60, timeFromUtc.AddMinutes(10));
			CreateGlbDeviceLocation(sql, deviceID, 90, timeFromUtc.AddHours(6));
			CreateGlbDeviceLocation(sql, deviceID, 86, timeFromUtc.AddHours(11));
			CreateJobCartageRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 90);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_TruckAndDeviceVariations_OneDriver_OneTruck_OneDevice_TwoRunSheets()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			FixRLOOffset(timeFromUtc);
			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver = Helper.CreateDriver("Bob", "Bob");
			var deviceID = CreateGlbDevice(sql, "device1");
			CreateGlbDeviceAssignmentDivot(sql, deviceID, truck.PK, timeFromUtc, timeFromUtc.AddHours(6));
			CreateGlbDeviceAssignmentDivot(sql, deviceID, truck.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			CreateGlbDeviceLocation(sql, deviceID, 90m, timeFromUtc);
			CreateGlbDeviceLocation(sql, deviceID, 100m, timeFromUtc.AddHours(3));
			CreateGlbDeviceLocation(sql, deviceID, 80m, timeFromUtc.AddHours(9));
			CreateGlbDeviceLocation(sql, deviceID, 110m, timeFromUtc.AddHours(12));
			CreateJobCartageRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
			CreateJobCartageRunSheet("2", truck, driver.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], "Bob", "truck1-XYZ", 110);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_TruckAndDeviceVariations_TwoDrivers_TwoTrucks_OneDevice_TwoRunSheets()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			FixRLOOffset(timeFromUtc);
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
			CreateJobCartageRunSheet("1", truck1, driver1.GS_Code, timeFromUtc, timeFromUtc.AddHours(6));
			CreateJobCartageRunSheet("2", truck2, driver2.GS_Code, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeFromUtc.AddHours(12));
			AssertEquals($"Expecting to find 2 results.", 2, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver1.GS_FullName, "truck1-XYZ", 100);
			AssertResultsOfPeakSpeedFunction(results[1], driver2.GS_FullName, "truck2-ABC", 110);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_TimeVariations_SearchRangeCoversRunSheetRange()
		{
			var timeFromUtc = ZDateTime.UtcNow;
			var timeToUtc = timeFromUtc.AddHours(12);
			TestPeakSpeedFunction_JobCartageRunSheet_TimeVariations_Setup(timeFromUtc, timeToUtc);
			// Search range covers the whole run sheet range
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeToUtc);
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], "Bob", "truck1-XYZ", 110);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_TimeVariations_SearchRangeInsideRunSheetRange()
		{
			var timeFromUtc = ZDateTime.UtcNow;
			var timeToUtc = timeFromUtc.AddHours(12);
			TestPeakSpeedFunction_JobCartageRunSheet_TimeVariations_Setup(timeFromUtc, timeToUtc);
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc.AddHours(5), timeFromUtc.AddHours(6));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], "Bob", "truck1-XYZ", 90);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_TimeVariations_SearchRangeStartsBeforeRunSheetEnds()
		{
			var timeFromUtc = ZDateTime.UtcNow;
			var timeToUtc = timeFromUtc.AddHours(12);
			TestPeakSpeedFunction_JobCartageRunSheet_TimeVariations_Setup(timeFromUtc, timeToUtc);
			var results = GetPeakSpeedBetweenTimeRange(timeToUtc.AddHours(-1), timeToUtc.AddHours(1));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], "Bob", "truck1-XYZ", 110);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_TimeVariations_SearchRangeStartsBeforeRunSheetStarts()
		{
			var timeFromUtc = ZDateTime.UtcNow;
			var timeToUtc = timeFromUtc.AddHours(12);
			TestPeakSpeedFunction_JobCartageRunSheet_TimeVariations_Setup(timeFromUtc, timeFromUtc.AddHours(12));
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc.AddHours(-1), timeFromUtc.AddHours(1));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], "Bob", "truck1-XYZ", 70);
		}

		void TestPeakSpeedFunction_JobCartageRunSheet_TimeVariations_Setup(ZDateTime timeFromUtc, ZDateTime timeToUtc)
		{
			FixRLOOffset(timeFromUtc);
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver = Helper.CreateDriver("Bob", "Bob");
			CreateJobCartageRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeToUtc);
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

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_TimeVariations_SearchRangeEndsInTheMiddleOfTwoDivots()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			FixRLOOffset(timeFromUtc);
			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver = Helper.CreateDriver("Bob", "Bob");
			var devicePK = CreateGlbDevice(sql, "device1");
			CreateGlbDeviceAssignmentDivot(sql, devicePK, truck.PK, timeFromUtc, timeFromUtc.AddHours(6));
			CreateGlbDeviceAssignmentDivot(sql, devicePK, truck.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			CreateJobCartageRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			CreateGlbDeviceLocation(sql, devicePK, 130m, timeFromUtc.AddHours(3));
			CreateGlbDeviceLocation(sql, devicePK, 145m, timeFromUtc.AddHours(9));
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc.AddHours(-6), timeFromUtc.AddHours(6));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 130);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_TimeVariations_SearchRangeStartsInTheMiddleOfTwoDivots()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			FixRLOOffset(timeFromUtc);
			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var driver = Helper.CreateDriver("Bob", "Bob");
			var devicePK = CreateGlbDevice(sql, "device1");
			CreateGlbDeviceAssignmentDivot(sql, devicePK, truck.PK, timeFromUtc, timeFromUtc.AddHours(6));
			CreateGlbDeviceAssignmentDivot(sql, devicePK, truck.PK, timeFromUtc.AddHours(6), timeFromUtc.AddHours(12));
			CreateJobCartageRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeFromUtc.AddHours(12));
			CreateGlbDeviceLocation(sql, devicePK, 145m, timeFromUtc.AddHours(3));
			CreateGlbDeviceLocation(sql, devicePK, 130m, timeFromUtc.AddHours(9));
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc.AddHours(6), timeFromUtc.AddHours(18));
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 130);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestPeakSpeedFunction_JobCartageRunSheet_TestRunSheetExtendsOverDayLightSavings()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromLocal = new ZDateTime(2010, 4, 4, 1, 0, 0, 0);
			var timeToLocal = new ZDateTime(2010, 4, 4, 3, 0, 0, 0);
			var timeFromUtcDateTimeOffset = new ZDateTimeOffset(timeFromLocal, DateTimeKind.Local).Offset;
			var timeToUtcDateTimeOffset = new ZDateTimeOffset(timeToLocal, DateTimeKind.Local).Offset;
			var timeFromUtc = timeFromLocal - timeFromUtcDateTimeOffset;
			var timeToUtc = timeToLocal - timeToUtcDateTimeOffset;
			var driver = Helper.CreateDriver("Bob", "Bob");
			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.EY_RQ_Truck = truck.PK;
			runSheet.EY_GS_NKTruckDriver = driver.GS_Code;
			runSheet.EY_StartTime = timeFromLocal;
			runSheet.EY_EndTime = timeToLocal;
			runSheet.EY_OH_TransportCo = org1.PK;
			var deviceID = SetupDeviceWithAssignmentDivot(sql, "device1", truck.PK, new ZDateTime(2010, 4, 3, 14, 0, 0, 0), new ZDateTime(2010, 4, 4, 17, 0, 0, 0));
			CreateGlbDeviceLocation(sql, deviceID, 50m, new ZDateTime(2010, 4, 3, 14, 0, 0, 0));
			CreateGlbDeviceLocation(sql, deviceID, 60m, new ZDateTime(2010, 4, 3, 15, 0, 0, 0));
			CreateGlbDeviceLocation(sql, deviceID, 140m, new ZDateTime(2010, 4, 3, 17, 0, 0, 0));
			// Add time zone periods
			Db.Connection.ExecuteNonQuery($@"INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) VALUES ('{ZGuid.NewZGuid()}', 
				'AUSYD', '2009-10-04 03:00:00', '2010-04-04 02:00:00', 660), ('{ZGuid.NewZGuid()}','AUSYD', '2010-04-04 02:00:00', '2010-10-03 03:00:00', 600)");
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeToUtc);
			AssertEquals($"Expecting to find 1 result.", 1, results.Count);
			AssertResultsOfPeakSpeedFunction(results[0], driver.GS_FullName, "truck1-XYZ", 140);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_NoStmaLogForRunSheet()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			var timeToUtc = timeFromUtc.AddHours(12);
			FixRLOOffset(timeFromUtc);
			var driver = Helper.CreateDriver("Bob", "Bob");
			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck.PK, timeFromUtc, timeToUtc, 90, 100, 105);
			var runSheetPK = CreateJobCartageRunSheet("1", truck, driver.GS_Code, timeFromUtc, timeToUtc);
			Factory.Save();
			Db.Connection.ExecuteNonQuery($@"UPDATE dbo.StmaLog SET SL_SE_NKEvent = 'LGO' WHERE StmaLog.SL_Parent = '{runSheetPK}'");
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeToUtc);
			AssertEquals($"Expecting to find 0 results.", 0, results.Count);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		[TestDate(2017, 1, 1, 10, 0, 0, 0)]
		public void TestPeakSpeedFunction_JobCartageRunSheet_NoDriverOnRunSheet()
		{
			var sql = new ZStringBuilder();
			TestDateAttribute.UseUNLOCO = true;
			var timeFromUtc = ZDateTime.UtcNow;
			var timeToUtc = timeFromUtc.AddHours(12);
			FixRLOOffset(timeFromUtc);
			var truck = Helper.CreateVehicleWithEquipmentType("V1", "truck1-XYZ");
			SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(sql, "device1", truck.PK, timeFromUtc, timeToUtc, 80, 90, 75);
			CreateJobCartageRunSheet("1", truck, ZString.Empty, timeFromUtc, timeToUtc);
			Factory.Save();
			Db.Connection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			var results = GetPeakSpeedBetweenTimeRange(timeFromUtc, timeToUtc);
			AssertEquals($"Expecting to find 0 results.", 0, results.Count);
		}

		ZGuid CreateJobCartageRunSheet(ZString jobID, RefEquipment truck, ZString driverCode, ZDateTime startTimeUtc, ZDateTime endTimeUtc)
		{
			var dateTimeOffset = new ZDateTimeOffset(ZDateTime.Now, DateTimeKind.Local).Offset;
			var startTimeLocal = startTimeUtc + dateTimeOffset;
			var endTimeLocal = endTimeUtc + dateTimeOffset;
			var transportOrg = Factory.NewWithValidTestData<OrgHeader>();
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.EY_RQ_Truck = truck.PK;
			runSheet.EY_GS_NKTruckDriver = driverCode;
			runSheet.EY_StartTime = startTimeLocal;
			runSheet.EY_EndTime = endTimeLocal;
			runSheet.EY_OH_TransportCo = transportOrg.PK;
			return runSheet.PK;
		}

		void AssertResultsOfPeakSpeedFunction(DynamicBusinessObject row, ZString expectedDriverName, ZString expectedRegistrationNumber, decimal expectedPeakSpeed)
		{
			var registrationNumber = (ZString)row["RegistrationNumber"];
			var driverName = (ZString)row["DriverName"];
			var peakSpeed = row["PeakSpeed"];
			AssertEquals($"Expecting to find a registration number of {expectedRegistrationNumber}.", expectedRegistrationNumber, registrationNumber);
			AssertEquals($"Expecting to find a driver name of {expectedDriverName}.", expectedDriverName, driverName);
			AssertEquals($"Expecting to find a peak speed of {expectedPeakSpeed}.", expectedPeakSpeed, peakSpeed);
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

		void SetupDeviceWithAssignmentDivotAndThreeDeviceLocations(ZStringBuilder sql, ZString deviceName, ZGuid truckID, ZDateTime timeFromUtc, ZDateTime timeToUtc, int speed1, int speed2, int speed3)
		{
			var deviceID = CreateGlbDevice(sql, deviceName);
			CreateGlbDeviceAssignmentDivot(sql, deviceID, truckID, timeFromUtc, timeToUtc);
			CreateGlbDeviceLocation(sql, deviceID, speed1, timeFromUtc.AddMinutes(10));
			CreateGlbDeviceLocation(sql, deviceID, speed2, timeFromUtc.AddHours((timeToUtc.Hour - timeFromUtc.Hour) / 2));
			CreateGlbDeviceLocation(sql, deviceID, speed3, timeToUtc.AddMinutes(-10));
		}

		ZGuid CreateGlbDevice(ZStringBuilder sql, ZString deviceName)
		{
			var pk = ZGuid.NewZGuid();
			sql.Append($"INSERT INTO dbo.GlbDevice (V3_PK, V3_HumanReadableIdentifier, V3_Model, V3_SystemCreateUser, V3_MobileServicesIdentifier, V3_SystemCreateTimeUtc) values ('{pk}', '{deviceName}', 'model1', 'USR', CONVERT(varbinary, '{deviceName}'), GETDATE())");
			return pk;
		}

		void CreateGlbDeviceAssignmentDivot(ZStringBuilder sql, ZGuid deviceID, ZGuid parentID, ZDateTime startTimeUtc, ZDateTime? endTimeUtc = null)
		{
			sql.Append($"INSERT INTO dbo.GlbDeviceAssignmentDivot (V7_PK, V7_V3_Device, V7_ParentID, V7_ParentTableCode, V7_StartTimeUtc{(endTimeUtc.HasValue ? ", V7_EndTimeUtc" : "")}) values ('{ZGuid.NewZGuid()}', '{deviceID}', '{parentID}', '{RefEquipmentSchema.Constants.Prefix}', '{startTimeUtc}'{(endTimeUtc.HasValue ? $", '{endTimeUtc}'" : "")})");
		}

		void CreateGlbDeviceLocation(ZStringBuilder sql, ZGuid deviceID, decimal speedInKmh, ZDateTime time)
		{
			sql.Append($"INSERT INTO dbo.GlbDeviceLocation (V2_PK, V2_V3_Device, V2_Speedkmh, V2_MeasurementTimeUtc) values ('{ZGuid.NewZGuid()}', '{deviceID}', '{speedInKmh}', '{time}')");
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
		protected override void SetUp()
		{
			base.SetUp();
			var company = CreateNewCompany("C1", "AU");
			var branch = CreateBranch("B1", "Branch1", company);
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
		public GlbCompany CreateNewCompany(ZString companyCode, ZString countryCode)
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = companyCode;
			company.GC_RN_NKCountryCode = countryCode.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : countryCode;
			Factory.Save();
			return Factory.Load<GlbCompany>(company.PK);
		}

		public GlbBranch CreateBranch(string code, string name, GlbCompany company)
		{
			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = code;
			branch.GB_BranchName = name;
			branch.GB_GC = company.PK;
			var unlocoQuery = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, company.GC_RN_NKCountryCode)
			{ OrderBy = RefUNLOCOSchema.Constants.RL_Code };
			branch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(unlocoQuery).Code;
			branch.Factory.Save();
			return Factory.Load<GlbBranch>(branch.PK);
		}

		// needed as sometimes offset values in test system are incorrect due to transform needing to run from hours to minutes (not 100% sure but would explain why unit tests are failing)
		void FixRLOOffset(ZDateTime timeFromUtc)
		{
			short offset = 660;
			var unlocoCode = "AUSYD";
			var timeFromUtcAsTSQLString = FormattableString.Invariant($@"'{timeFromUtc.Year}-{timeFromUtc.Month.ToString("D2")}-{timeFromUtc.Day.ToString("D2")}T{timeFromUtc.Hour.ToString("D2")}:{timeFromUtc.Minute.ToString("D2")}:{timeFromUtc.Second.ToString("D2")}'");
			var whereClause = FormattableString.Invariant($@" WHERE RLO_RL_NKCode = 'AUSYD' AND RLO_StartTimeUtc <= {timeFromUtcAsTSQLString} AND RLO_EndTimeUtc >= {timeFromUtcAsTSQLString}");
			var sql = FormattableString.Invariant($@"IF EXISTS(SELECT * FROM dbo.RefDatabase_RefUNLOCOUtcOffset {whereClause})
BEGIN
	UPDATE dbo.RefDatabase_RefUNLOCOUtcOffset SET RLO_OffsetMinutesFromUtc = {offset} {whereClause}
END ELSE BEGIN
	INSERT INTO dbo.RefDatabase_RefUNLOCOUtcOffset (RLO_PK, RLO_RL_NKCode, RLO_StartTimeUtc, RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc)
	VALUES (newid(), '{unlocoCode}', DATEADD(day, -30, {timeFromUtcAsTSQLString}), DATEADD(day, 30, {timeFromUtcAsTSQLString}), {offset})
END");
			Db.Connection.ExecuteNonQuery(sql);
		}
	}
}
