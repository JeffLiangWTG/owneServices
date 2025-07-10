using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business.Testing.GPS.Telematics
{
	class GPSDtbConsignmentRunSheetInstructionUpdaterTest : TestCaseWithFactory
	{
		public void TestProcessLocation_WhenNoRunSheetMatched_ShouldSkipProcess()
		{
			var transportCo = TransportConsignmentTestHelperHelper.CreateOrganisation("TC0");
			var truck = Factory.NewWithValidTestData<RefEquipment>();
			var runSheet = TransportConsignmentTestHelperHelper.CreateRunSheet(transportCo, "RS001", truck.PK);
			Factory.Save();

			var deviceLocationWithEntity = Factory.Load<IDeviceLocationWithEntity>(((BusinessObject)deviceLocation).PK);
			gpsDtbConsignmentRunSheetInstructionUpdater.ProcessLocation(deviceLocationWithEntity);

			var logsQuery = new ZQuery() { DefaultJoinCondition = JoinCondition.Or };
			logsQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.GateIn.Code);
			logsQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.GateOut.Code);
			var result = Factory.LoadTop1<StmALog>(logsQuery);

			AssertNull(result);
		}

		public void TestProcessLocation_WithMultipleInstructions_WhenGPSLocationInNextRunSheetInstructionGeofence_NextInstructionsHasTimeIn_ShouldSkipProcess()
		{
			telematicsTestHelper.CreateDeviceAssignment(device1PK, truck1.PK, utcEventTime.AddDays(-1), utcEventTime.AddDays(1), RefEquipmentSchema.Constants.Prefix);
			(var runSheetInstruction1, var runSheetInstruction2) = InitializeTestRunSheetInstructionsData("RS001", "CN001", "ADRCode", truck1, transportCo1, transportCo2);
			runSheetInstruction1.K1_IsAcceptedByDriver = true;
			runSheetInstruction1.K1_TimeIn = ZDateTimeOffset.Now.AddHours(-1);
			runSheetInstruction1.K1_TimeOut = ZDateTimeOffset.Now.AddMinutes(-30);
			runSheetInstruction2.K1_IsAcceptedByDriver = true;
			runSheetInstruction2.K1_TimeIn = ZDateTimeOffset.Now.AddMinutes(-5);

			Factory.Save();

			var deviceLocationWithEntity = Factory.Load<IDeviceLocationWithEntity>(((BusinessObject)deviceLocation).PK);
			gpsDtbConsignmentRunSheetInstructionUpdater.ProcessLocation(deviceLocationWithEntity);

			var logsQuery = new ZQuery() { DefaultJoinCondition = JoinCondition.Or };
			logsQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.GateIn.Code);
			var result = Factory.LoadTop1<StmALog>(logsQuery);

			AssertNull(result);
		}

		public void TestProcessLocation_WithMultipleInstructions_WhenGPSLocationInNextRunSheetInstructionGeofence_NextInstructionHasNoTimeIn_ShouldRaiseGINEvent()
		{
			telematicsTestHelper.CreateDeviceAssignment(device1PK, truck1.PK, utcEventTime.AddDays(-1), utcEventTime.AddDays(1), RefEquipmentSchema.Constants.Prefix);
			(var runSheetInstruction1, var runSheetInstruction2) = InitializeTestRunSheetInstructionsData("RS001", "CN001", "ADRCode", truck1, transportCo1, transportCo2);
			runSheetInstruction1.K1_IsAcceptedByDriver = true;
			runSheetInstruction1.K1_TimeIn = ZDateTimeOffset.Now.AddHours(-1);
			runSheetInstruction1.K1_TimeOut = ZDateTimeOffset.Now.AddMinutes(-30);
			runSheetInstruction2.K1_IsAcceptedByDriver = true;

			Factory.Save();

			var deviceLocationWithEntity = Factory.Load<IDeviceLocationWithEntity>(((BusinessObject)deviceLocation).PK);
			gpsDtbConsignmentRunSheetInstructionUpdater.ProcessLocation(deviceLocationWithEntity);

			var refreshedInstruction2 = Factory.Load<DtbConsignmentRunSheetInstruction>(runSheetInstruction2.PK);
			var gateInLogs = refreshedInstruction2.Logs.Find(log => log.Event.SE_Code == Events.GateIn.Code);

			AssertEquals(1, gateInLogs.Count());
			AssertEquals("TC2 - ADRCode", gateInLogs.First().SL_Reference);
		}

		public void TestProcessLocation_WithMultipleInstructions_WhenGPSLocationOutsideOfNextRunSheetInstructionGeofence_InstructionHasNoTimeIn_ShouldSkipProcess()
		{
			telematicsTestHelper.CreateDeviceAssignment(device1PK, truck1.PK, utcEventTime.AddDays(-1), utcEventTime.AddDays(1), RefEquipmentSchema.Constants.Prefix);
			(var runSheetInstruction1, var runSheetInstruction2) = InitializeTestRunSheetInstructionsData("RS001", "CN001", "ADRCode", truck1, transportCo1, transportCo2, -38.368181m, 144.345903m);
			runSheetInstruction1.K1_IsAcceptedByDriver = true;
			runSheetInstruction1.K1_TimeIn = ZDateTimeOffset.Now.AddHours(-1);
			runSheetInstruction1.K1_TimeOut = ZDateTimeOffset.Now.AddMinutes(-30);
			runSheetInstruction2.K1_IsAcceptedByDriver = true;

			Factory.Save();

			var deviceLocationWithEntity = Factory.Load<IDeviceLocationWithEntity>(((BusinessObject)deviceLocation).PK);
			gpsDtbConsignmentRunSheetInstructionUpdater.ProcessLocation(deviceLocationWithEntity);

			var logsQuery = new ZQuery() { DefaultJoinCondition = JoinCondition.Or };
			logsQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.GateIn.Code);
			var result = Factory.LoadTop1<StmALog>(logsQuery);

			AssertNull(result);
		}

		public void TestProcessLocation_WithMultipleInstructions_WhenGPSLocationOutsideOfNextRunSheetInstructionGeofence_InstructionHasTimeInAndNoTimeOut_RaiseGOUEvent()
		{
			telematicsTestHelper.CreateDeviceAssignment(device1PK, truck1.PK, utcEventTime.AddDays(-1), utcEventTime.AddDays(1), RefEquipmentSchema.Constants.Prefix);
			(var runSheetInstruction1, var runSheetInstruction2) = InitializeTestRunSheetInstructionsData("RS001", "CN001", "ADRCode", truck1, transportCo1, transportCo2, -38.368181m, 144.345903m);
			runSheetInstruction1.K1_IsAcceptedByDriver = true;
			runSheetInstruction1.K1_TimeIn = ZDateTimeOffset.Now.AddHours(-1);
			runSheetInstruction1.K1_TimeOut = ZDateTimeOffset.Now.AddMinutes(-30);
			runSheetInstruction2.K1_IsAcceptedByDriver = true;
			runSheetInstruction2.K1_TimeIn = ZDateTimeOffset.Now.AddMinutes(-5);

			Factory.Save();

			var deviceLocationWithEntity = Factory.Load<IDeviceLocationWithEntity>(((BusinessObject)deviceLocation).PK);
			gpsDtbConsignmentRunSheetInstructionUpdater.ProcessLocation(deviceLocationWithEntity);

			var refreshedInstruction2 = Factory.Load<DtbConsignmentRunSheetInstruction>(runSheetInstruction2.PK);
			var gateOutLogs = refreshedInstruction2.Logs.Find(log => log.Event.SE_Code == Events.GateOut.Code);

			AssertEquals(1, gateOutLogs.Count());
			AssertEquals("TC2 - ADRCode", gateOutLogs.First().SL_Reference);
		}

		public void TestProcessLocation_WithMultipleInstructions_WhenGPSLocationOutsideOfNextRunSheetInstructionGeofence_InstructionHasTimeInAndNoTimeOut_TwoGPSLocationComeIn_ShouldRaiseGOUEventOnce()
		{
			telematicsTestHelper.CreateDeviceAssignment(device1PK, truck1.PK, utcEventTime.AddDays(-1), utcEventTime.AddDays(1), RefEquipmentSchema.Constants.Prefix);
			(var runSheetInstruction1, var runSheetInstruction2) = InitializeTestRunSheetInstructionsData("RS001", "CN001", "ADRCode", truck1, transportCo1, transportCo2, -38.368181m, 144.345903m);
			runSheetInstruction1.K1_IsAcceptedByDriver = true;
			runSheetInstruction1.K1_TimeIn = ZDateTimeOffset.Now.AddHours(-1);
			runSheetInstruction1.K1_TimeOut = ZDateTimeOffset.Now.AddMinutes(-30);
			runSheetInstruction2.K1_IsAcceptedByDriver = true;
			runSheetInstruction2.K1_TimeIn = ZDateTimeOffset.Now.AddMinutes(-5);

			Factory.Save();

			var deviceLocationWithEntity = Factory.Load<IDeviceLocationWithEntity>(((BusinessObject)deviceLocation).PK);
			gpsDtbConsignmentRunSheetInstructionUpdater.ProcessLocation(deviceLocationWithEntity);
			gpsDtbConsignmentRunSheetInstructionUpdater.ProcessLocation(deviceLocationWithEntity);

			var refreshedInstruction2 = Factory.Load<DtbConsignmentRunSheetInstruction>(runSheetInstruction2.PK);
			var gateOutLogs = refreshedInstruction2.Logs.Find(log => log.Event.SE_Code == Events.GateOut.Code);

			AssertEquals(1, gateOutLogs.Count());
			AssertEquals("TC2 - ADRCode", gateOutLogs.First().SL_Reference);
		}

		public void TestProcessLocation_WithMultipleInstructions_WhenMultipleRunSheets_GPSLocationOutsideOfOneRunSheet_InsideAnotherRunSheet_RaiseGINAndGOUEvents()
		{
			telematicsTestHelper.CreateDeviceAssignment(device1PK, truck1.PK, utcEventTime.AddDays(-1), utcEventTime.AddDays(1), RefEquipmentSchema.Constants.Prefix);
			(var runSheetInstruction1, var runSheetInstruction2) = InitializeTestRunSheetInstructionsData("RS001", "CN001", "ADRCode", truck1, transportCo1, transportCo2, -38.368181m, 144.345903m);
			runSheetInstruction1.K1_IsAcceptedByDriver = true;
			runSheetInstruction1.K1_TimeIn = ZDateTimeOffset.Now.AddHours(-1);
			runSheetInstruction1.K1_TimeOut = ZDateTimeOffset.Now.AddMinutes(-30);
			runSheetInstruction2.K1_IsAcceptedByDriver = true;
			runSheetInstruction2.K1_TimeIn = ZDateTimeOffset.Now.AddMinutes(-5);

			(var runSheetInstruction3, var runSheetInstruction4) = InitializeTestRunSheetInstructionsData("RS002", "CN002", "ACCCode", truck1, transportCo2, transportCo1);
			runSheetInstruction3.K1_IsAcceptedByDriver = true;
			runSheetInstruction3.K1_TimeIn = ZDateTimeOffset.Now.AddHours(-1);
			runSheetInstruction3.K1_TimeOut = ZDateTimeOffset.Now.AddMinutes(-30);
			runSheetInstruction4.K1_IsAcceptedByDriver = true;

			Factory.Save();

			var deviceLocationWithEntity = Factory.Load<IDeviceLocationWithEntity>(((BusinessObject)deviceLocation).PK);
			gpsDtbConsignmentRunSheetInstructionUpdater.ProcessLocation(deviceLocationWithEntity);

			var refreshedInstruction2 = Factory.Load<DtbConsignmentRunSheetInstruction>(runSheetInstruction2.PK);
			var instruction2Logs = refreshedInstruction2.Logs.Find(log => true);

			AssertEquals(1, instruction2Logs.Count());
			AssertEquals("TC2 - ADRCode", instruction2Logs.First().SL_Reference);
			AssertEquals(Events.GateOut.Code, instruction2Logs.First().Event.SE_Code);
			AssertEquals(utcEventTime, instruction2Logs.First().SL_EventTime);

			var refreshedInstruction4 = Factory.Load<DtbConsignmentRunSheetInstruction>(runSheetInstruction4.PK);
			var instruction4Logs = refreshedInstruction4.Logs.Find(log => true);

			AssertEquals(1, instruction4Logs.Count());
			AssertEquals("TC1 - ACCCode", instruction4Logs.First().SL_Reference);
			AssertEquals(Events.GateIn.Code, instruction4Logs.First().Event.SE_Code);
			AssertEquals(utcEventTime, instruction4Logs.First().SL_EventTime);
		}

		public void TestProcessLocation_WithMultipleInstructions_WhenDevideAssignedToDriver_GPSLocationOutsideOfOneRunSheet_InsideAnotherRunSheet_RaiseGINAndGOUEvents()
		{
			telematicsTestHelper.CreateDeviceAssignment(device1PK, driver.PK, utcEventTime.AddDays(-1), utcEventTime.AddDays(1), GlbStaffSchema.Constants.Prefix);
			(var runSheetInstruction1, var runSheetInstruction2) = InitializeTestRunSheetInstructionsData("RS001", "CN001", "ADRCode", truck1, transportCo1, transportCo2, -38.368181m, 144.345903m, driver);
			runSheetInstruction1.K1_IsAcceptedByDriver = true;
			runSheetInstruction1.K1_TimeIn = ZDateTimeOffset.Now.AddHours(-1);
			runSheetInstruction1.K1_TimeOut = ZDateTimeOffset.Now.AddMinutes(-30);
			runSheetInstruction2.K1_IsAcceptedByDriver = true;
			runSheetInstruction2.K1_TimeIn = ZDateTimeOffset.Now.AddMinutes(-5);

			(var runSheetInstruction3, var runSheetInstruction4) = InitializeTestRunSheetInstructionsData("RS002", "CN002", "ACCCode", truck2, transportCo2, transportCo1, driver: driver);
			runSheetInstruction3.K1_IsAcceptedByDriver = true;
			runSheetInstruction3.K1_TimeIn = ZDateTimeOffset.Now.AddHours(-1);
			runSheetInstruction3.K1_TimeOut = ZDateTimeOffset.Now.AddMinutes(-30);
			runSheetInstruction4.K1_IsAcceptedByDriver = true;

			Factory.Save();

			var deviceLocationWithEntity = Factory.Load<IDeviceLocationWithEntity>(((BusinessObject)deviceLocation).PK);
			gpsDtbConsignmentRunSheetInstructionUpdater.ProcessLocation(deviceLocationWithEntity);

			var refreshedInstruction2 = Factory.Load<DtbConsignmentRunSheetInstruction>(runSheetInstruction2.PK);
			var instruction2Logs = refreshedInstruction2.Logs.Find(log => true);

			AssertEquals(1, instruction2Logs.Count());
			AssertEquals("TC2 - ADRCode", instruction2Logs.First().SL_Reference);
			AssertEquals(Events.GateOut.Code, instruction2Logs.First().Event.SE_Code);
			AssertEquals(utcEventTime, instruction2Logs.First().SL_EventTime);

			var refreshedInstruction4 = Factory.Load<DtbConsignmentRunSheetInstruction>(runSheetInstruction4.PK);
			var instruction4Logs = refreshedInstruction4.Logs.Find(log => true);

			AssertEquals(1, instruction4Logs.Count());
			AssertEquals("TC1 - ACCCode", instruction4Logs.First().SL_Reference);
			AssertEquals(Events.GateIn.Code, instruction4Logs.First().Event.SE_Code);
			AssertEquals(utcEventTime, instruction4Logs.First().SL_EventTime);
		}

		(DtbConsignmentRunSheetInstruction instruction1, DtbConsignmentRunSheetInstruction instruction2) InitializeTestRunSheetInstructionsData(
			string runSheetNo,
			string consignmentNo,
			string addressCode,
			RefEquipment truck,
			OrgHeader pickupOrgAddress,
			OrgHeader deliveryOrgAddress,
			decimal latitude = -38.168181m,
			decimal longitude = 144.345903m,
			GlbStaff driver = null)
		{
			var pickupAddress = TransportConsignmentTestHelperHelper.CreateOrgAddress(pickupOrgAddress, "ADL");
			var deliveryAddress = TransportConsignmentTestHelperHelper.CreateOrgAddress(deliveryOrgAddress, "ADL", addressCode, latitude: latitude, longitude: longitude);
			var runSheet = TransportConsignmentTestHelperHelper.CreateRunSheet(transportCo1, runSheetNo, truck.PK, actualStartTime: utcEventTime.AddMinutes(-1).ToOffset());
			var consignment = TransportConsignmentTestHelperHelper.CreateConsignment(consignmentNo);
			var pickupConsignmentAdress = TransportConsignmentTestHelperHelper.CreateConsignmentAddress(consignment, "PIC", pickupAddress);
			var deliveryConsignmentAdress = TransportConsignmentTestHelperHelper.CreateConsignmentAddress(consignment, "DLV", deliveryAddress);
			var dtbConsignmentAction1 = TransportConsignmentTestHelperHelper.CreateConsignmentAction(pickupConsignmentAdress, "PIC");
			var dtbConsignmentAction2 = TransportConsignmentTestHelperHelper.CreateConsignmentAction(deliveryConsignmentAdress, "DLV");
			var runSheetInstruction1 = TransportConsignmentTestHelperHelper.CreateRunSheetInstruction(dtbConsignmentAction1, runSheet.PK, 1);
			var runSheetInstruction2 = TransportConsignmentTestHelperHelper.CreateRunSheetInstruction(dtbConsignmentAction2, runSheet.PK, 2);
			if (driver != null)
			{
				runSheet.KG_GS_NKTruckDriver = driver.GS_Code;
			}

			return (runSheetInstruction1, runSheetInstruction2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			truck1 = Factory.NewWithValidTestData<RefEquipment>();
			truck2 = Factory.NewWithValidTestData<RefEquipment>();
			driver = Factory.NewWithValidTestData<GlbStaff>();
			device1PK = TelematicsTestHelper.CreateDevice("m1", "one", new byte[] { 1 });
			utcEventTime = ZDateTime.UtcNow;
			gpsDtbConsignmentRunSheetInstructionUpdater = new GPSDtbConsignmentRunSheetInstructionUpdater(Factory);
			deviceLocation = TelematicsTestHelper.CreateDeviceLocation(device1PK, truck1.PK, (double)LongitudeWTG, (double)LatitudeWTG, utcEventTime, speedKmh: speedkmh, headingDegrees: headingDegrees);
			transportCo1 = TransportConsignmentTestHelperHelper.CreateOrganisation("TC1");
			transportCo2 = TransportConsignmentTestHelperHelper.CreateOrganisation("TC2");
		}

		protected override void TearDown()
		{
			transportConsignmentTestHelper = null;
			telematicsTestHelper = null;
			base.TearDown();
		}

		protected ITelematicsTestHelper TelematicsTestHelper
		{
			get
			{
				return telematicsTestHelper ?? (telematicsTestHelper = ObjectFactory.Get<ITelematicsTestHelper>("ITelematicsTestHelper", Factory));
			}
		}

		TransportConsignmentTestHelper TransportConsignmentTestHelperHelper
		{
			get { return transportConsignmentTestHelper ?? (transportConsignmentTestHelper = new TransportConsignmentTestHelper(Factory)); }
		}

		RefEquipment truck1;
		RefEquipment truck2;
		GlbStaff driver;
		ZGuid device1PK;
		readonly ZDecimal speedkmh = 5;
		readonly ZDecimal headingDegrees = 100;
		ZDateTime utcEventTime;
		GPSDtbConsignmentRunSheetInstructionUpdater gpsDtbConsignmentRunSheetInstructionUpdater;
		IDeviceLocation deviceLocation;
		readonly ZDecimal LongitudeWTG = 144.345903126472m;
		readonly ZDecimal LatitudeWTG = -38.1681812288127m;

		TransportConsignmentTestHelper transportConsignmentTestHelper;
		ITelematicsTestHelper telematicsTestHelper;
		OrgHeader transportCo1;
		OrgHeader transportCo2;
	}
}
