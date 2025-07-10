using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteVehicleEntryDataObjectReader))]
	public class GteVehicleEntryDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region Matching Test

		public void TestGivenParentMovementHasNoEntries_WhenGetExistingBusinessObjectForIncoming_ThenNoMatchFound()
		{
			#region Setup Entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			var matchedVehicleEntry = reader.TryGetExistingBusinessObject();
			AssertNull("Should not find a match when no existing entries", matchedVehicleEntry);
		}

		public void TestGivenParentMovementHasNoEntries_WhenGetExistingBusinessObjectForOutgoing_ThenNoMatchFound()
		{
			#region Setup Entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: false);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			var matchedVehicleEntry = reader.TryGetExistingBusinessObject();
			AssertNull("Should not find a match when no existing entries", matchedVehicleEntry);
		}

		public void TestGivenParentMovementHasReversedGateIn_WhenGetExistingBusinessObjectForIncoming_ThenNoMatchFound()
		{
			#region Setup Entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var cancelledGateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			cancelledGateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			cancelledGateIn.GVE_CancelledReason = "Test";
			cancelledGateIn.GVE_CancelledTime = ZDateTimeOffset.Now;
			cancelledGateIn.GVE_GS_NKCancelledBy = "~BP";
			cancelledGateIn.GVE_IsIncoming = true;
			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			var matchedVehicleEntry = reader.TryGetExistingBusinessObject();
			AssertNull("Should not find a match when cancelled gate in", matchedVehicleEntry);
		}

		public void TestGivenParentMovementHasReversedGateOut_WhenGetExistingBusinessObjectForOutgoing_ThenNoMatchFound()
		{
			#region Setup Entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var cancelledGateOut = Factory.NewWithValidTestData<GteVehicleEntry>();
			cancelledGateOut.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			cancelledGateOut.GVE_CancelledReason = "Test";
			cancelledGateOut.GVE_CancelledTime = ZDateTimeOffset.Now;
			cancelledGateOut.GVE_GS_NKCancelledBy = "~BP";
			cancelledGateOut.GVE_IsIncoming = false;
			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: false);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			var matchedVehicleEntry = reader.TryGetExistingBusinessObject();
			AssertNull("Should not find a match when cancelled gate out", matchedVehicleEntry);
		}

		public void TestGivenParentMovementHasGateIn_WhenGetExistingBusinessObjectForIncoming_ThenCorrectMatchFound()
		{
			#region Setup Entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateIn.GVE_IsIncoming = true;
			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			var matchedVehicleEntry = reader.TryGetExistingBusinessObject();
			AssertEquals("Should find correct GateIn VehicleEntry", gateIn.PK, matchedVehicleEntry.PK);
		}

		public void TestGivenParentMovementHasGateOut_WhenGetExistingBusinessObjectForIncoming_ThenNoMatchFound()
		{
			#region Setup Entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateOut = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateOut.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateOut.GVE_IsIncoming = false;
			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			var matchedVehicleEntry = reader.TryGetExistingBusinessObject();
			AssertNull("Should not find a match when no GateIn", matchedVehicleEntry);
		}

		public void TestGivenParentMovementHasGateIn_WhenGetExistingBusinessObjectForOutgoing_ThenNoMatchFound()
		{
			#region Setup Entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateIn.GVE_IsIncoming = true;
			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: false);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			var matchedVehicleEntry = reader.TryGetExistingBusinessObject();
			AssertNull("Should not find a match when no gate out", matchedVehicleEntry);
		}

		public void TestGivenParentMovementHasGateInAndGateOut_WhenGetExistingBusinessObjectForIncoming_ThenCorrectMatchFound()
		{
			#region Setup Entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateIn.GVE_IsIncoming = true;

			var gateOut = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateOut.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateOut.GVE_IsIncoming = false;
			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			var matchedVehicleEntry = reader.TryGetExistingBusinessObject();
			AssertEquals("Should find correct GateIn VehicleEntry", gateIn.PK, matchedVehicleEntry.PK);
		}

		public void TestGivenParentMovementHasGateInAndGateOut_WhenGetExistingBusinessObjectForOutgoing_ThenCorrectMatchFound()
		{
			#region Setup Entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateIn.GVE_IsIncoming = true;

			var gateOut = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateOut.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateOut.GVE_IsIncoming = false;
			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: false);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			var matchedVehicleEntry = reader.TryGetExistingBusinessObject();
			AssertEquals("Should find correct GateOut VehicleEntry", gateOut.PK, matchedVehicleEntry.PK);
		}

		public void TestGivenParentMovementHasCancelledGateInAndGateIn_WhenGetExistingBusinessObjectForIncoming_ThenCorrectMatchFound()
		{
			#region Setup Entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateIn.GVE_IsIncoming = true;

			var cancelledGateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			cancelledGateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			cancelledGateIn.GVE_CancelledReason = "Test";
			cancelledGateIn.GVE_CancelledTime = ZDateTimeOffset.Now;
			cancelledGateIn.GVE_GS_NKCancelledBy = "~BP";
			cancelledGateIn.GVE_IsIncoming = true;

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			var matchedVehicleEntry = reader.TryGetExistingBusinessObject();
			AssertEquals("Should find correct GateIn VehicleEntry", gateIn.PK, matchedVehicleEntry.PK);
		}

		public void TestGivenParentMovementHasCancelledGateOutAndGateOut_WhenGetExistingBusinessObjectForOutgoing_ThenCorrectMatchFound()
		{
			#region Setup Entities
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateOut = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateOut.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateOut.GVE_IsIncoming = false;

			var cancelledGateOut = Factory.NewWithValidTestData<GteVehicleEntry>();
			cancelledGateOut.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			cancelledGateOut.GVE_CancelledReason = "Test";
			cancelledGateOut.GVE_CancelledTime = ZDateTimeOffset.Now;
			cancelledGateOut.GVE_GS_NKCancelledBy = "~BP";
			cancelledGateOut.GVE_IsIncoming = false;

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: false);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			var matchedVehicleEntry = reader.TryGetExistingBusinessObject();
			AssertEquals("Should find correct GateIn VehicleEntry", gateOut.PK, matchedVehicleEntry.PK);
		}

		#endregion

		#region New vehicle entry tests without exception

		public void TestGivenNoMatchingVehicleEntry_WhenReadFullUXMLForIncoming_ThenAllFieldsCorrectlySet()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);
			var vehicleEntry = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry should have correct Lane", LaneForTest.PK, vehicleEntry.GVE_GLN_Lane);
				AssertEquals("GteVehicleEntry should have correct Weight", (ZDecimal)20, vehicleEntry.GVE_Weight);
				AssertEquals("GteVehicleEntry should have correct WeightUQ", "LB", vehicleEntry.GVE_WeightUQ);
				AssertEquals("GteVehicleEntry should have correct Direction", true, vehicleEntry.GVE_IsIncoming);
				AssertEquals("GteVehicleEntry should have correct DriverLicenseNumber", "1234 5678", vehicleEntry.GVE_DriverLicenseNumber);
				AssertEquals("GteVehicleEntry should have correct DriverName", "Jimothy", vehicleEntry.GVE_DriverName);
			});
		}

		public void TestGivenNoMatchingVehicleEntry_WhenReadFullUXMLForOutgoing_ThenAllFieldsCorrectlySet()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: false);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);
			var vehicleEntry = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry should have correct Lane", LaneForTest.PK, vehicleEntry.GVE_GLN_Lane);
				AssertEquals("GteVehicleEntry should have correct Weight", (ZDecimal)20, vehicleEntry.GVE_Weight);
				AssertEquals("GteVehicleEntry should have correct WeightUQ", "LB", vehicleEntry.GVE_WeightUQ);
				AssertEquals("GteVehicleEntry should have correct Direction", false, vehicleEntry.GVE_IsIncoming);
				AssertEquals("GteVehicleEntry should have correct DriverLicenseNumber", "1234 5678", vehicleEntry.GVE_DriverLicenseNumber);
				AssertEquals("GteVehicleEntry should have correct DriverName", "Jimothy", vehicleEntry.GVE_DriverName);
			});
		}

		public void TestGivenNoMatchingVehicleEntry_WhenReadUXMLWithNoWeight_ThenWeightIsZero()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			shipment.TotalWeight = null;
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);
			var vehicleEntry = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry should have correct Lane", LaneForTest.PK, vehicleEntry.GVE_GLN_Lane);
				AssertEquals("GteVehicleEntry should have correct Weight", (ZDecimal)0, vehicleEntry.GVE_Weight);
				AssertEquals("GteVehicleEntry should have correct WeightUQ", "LB", vehicleEntry.GVE_WeightUQ);
				AssertEquals("GteVehicleEntry should have correct Direction", true, vehicleEntry.GVE_IsIncoming);
				AssertEquals("GteVehicleEntry should have correct DriverLicenseNumber", "1234 5678", vehicleEntry.GVE_DriverLicenseNumber);
				AssertEquals("GteVehicleEntry should have correct DriverName", "Jimothy", vehicleEntry.GVE_DriverName);
			});
		}

		public void TestGivenNoMatchingVehicleEntry_WhenReadUXMLWithNegativeWeight_ThenWeightIsZero()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			shipment.TotalWeight = -5;
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);
			var vehicleEntry = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry should have correct Lane", LaneForTest.PK, vehicleEntry.GVE_GLN_Lane);
				AssertEquals("GteVehicleEntry should have correct Weight", (ZDecimal)0, vehicleEntry.GVE_Weight);
				AssertEquals("GteVehicleEntry should have correct WeightUQ", "LB", vehicleEntry.GVE_WeightUQ);
				AssertEquals("GteVehicleEntry should have correct Direction", true, vehicleEntry.GVE_IsIncoming);
				AssertEquals("GteVehicleEntry should have correct DriverLicenseNumber", "1234 5678", vehicleEntry.GVE_DriverLicenseNumber);
				AssertEquals("GteVehicleEntry should have correct DriverName", "Jimothy", vehicleEntry.GVE_DriverName);
			});
		}

		public void TestGivenNoMatchingVehicleEntry_WhenReadUXMLWithWeightAndNoUnit_ThenDefaultUnitIsNA()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			shipment.TotalWeightUnit = null;

			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);
			var vehicleEntry = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry should have correct Lane", LaneForTest.PK, vehicleEntry.GVE_GLN_Lane);
				AssertEquals("GteVehicleEntry should have correct Weight", (ZDecimal)20, vehicleEntry.GVE_Weight);
				AssertEquals("GteVehicleEntry should have correct WeightUQ", "NA", vehicleEntry.GVE_WeightUQ);
				AssertEquals("GteVehicleEntry should have correct Direction", true, vehicleEntry.GVE_IsIncoming);
				AssertEquals("GteVehicleEntry should have correct DriverLicenseNumber", "1234 5678", vehicleEntry.GVE_DriverLicenseNumber);
				AssertEquals("GteVehicleEntry should have correct DriverName", "Jimothy", vehicleEntry.GVE_DriverName);
			});
		}

		public void TestGivenNoMatchingVehicleEntry_WhenReadUXMLWithNoWeightAndNoUnit_ThenWeightAndUnitSetCorrectly()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			shipment.TotalWeightUnit = null;
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);
			var vehicleEntry = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry should have correct Lane", LaneForTest.PK, vehicleEntry.GVE_GLN_Lane);
				AssertEquals("GteVehicleEntry should have correct Weight", (ZDecimal)20, vehicleEntry.GVE_Weight);
				AssertEquals("GteVehicleEntry should have correct WeightUQ", "NA", vehicleEntry.GVE_WeightUQ);
				AssertEquals("GteVehicleEntry should have correct Direction", true, vehicleEntry.GVE_IsIncoming);
				AssertEquals("GteVehicleEntry should have correct DriverLicenseNumber", "1234 5678", vehicleEntry.GVE_DriverLicenseNumber);
				AssertEquals("GteVehicleEntry should have correct DriverName", "Jimothy", vehicleEntry.GVE_DriverName);
			});
		}

		#endregion

		#region Vehicle entry import tests for matched entries

		public void TestGivenMatchingVehicleEntry_WhenReadUXMLWithNoExtraData_ThenNoValuesChanged()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.GVE_IsIncoming = true;
			vehicleEntry.GVE_GLN_Lane = LaneForTest.PK;
			vehicleEntry.GVE_Weight = 20;
			vehicleEntry.GVE_WeightUQ = "KG";
			vehicleEntry.GVE_DriverLicenseNumber = "1234 5678";
			vehicleEntry.GVE_DriverName = "Jimothy";

			Factory.SaveForTesting();

			#endregion

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.AddAddInfo("IsIncoming", (ZString)"true");

			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);
			var newEntry = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry objects should match", vehicleEntry.PK, newEntry.PK);
				AssertEquals("GteVehicleEntry should have correct Lane", LaneForTest.PK, vehicleEntry.GVE_GLN_Lane);
				AssertEquals("GteVehicleEntry should have correct Weight", (ZDecimal)20, vehicleEntry.GVE_Weight);
				AssertEquals("GteVehicleEntry should have correct WeightUQ", "KG", vehicleEntry.GVE_WeightUQ);
				AssertEquals("GteVehicleEntry should have correct Direction", true, vehicleEntry.GVE_IsIncoming);
				AssertEquals("GteVehicleEntry should have correct DriverLicenseNumber", "1234 5678", vehicleEntry.GVE_DriverLicenseNumber);
				AssertEquals("GteVehicleEntry should have correct DriverName", "Jimothy", vehicleEntry.GVE_DriverName);
			});
		}

		public void TestGivenMatchingVehicleEntry_WhenReadUXMLWithEmptyValues_ThenNoValuesChanged()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.GVE_IsIncoming = true;
			vehicleEntry.GVE_GLN_Lane = LaneForTest.PK;
			vehicleEntry.GVE_Weight = 20;
			vehicleEntry.GVE_WeightUQ = "KG";
			vehicleEntry.GVE_DriverLicenseNumber = "1234 5678";
			vehicleEntry.GVE_DriverName = "Jimothy";

			Factory.SaveForTesting();

			#endregion

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TotalWeight = null;
			shipment.TotalWeightUnit = null;
			shipment.WarehouseLocation = null;

			shipment.SetAddInfoCollection(() => new List<AddInfo>()
			{
				new AddInfo() { Key = "IsIncoming", Value = "true" }
			});

			shipment.VehicleRun = new VehicleRun();
			shipment.VehicleRun.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.VehicleRun.SetCrewCollection(() => new List<Crew>()
			{
				new Crew() { FullName = "", LicenseNumber = "" }
			});

			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);
			var newEntry = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry objects should match", vehicleEntry.PK, newEntry.PK);
				AssertEquals("GteVehicleEntry should have correct Lane", LaneForTest.PK, vehicleEntry.GVE_GLN_Lane);
				AssertEquals("GteVehicleEntry should have correct Weight", (ZDecimal)20, vehicleEntry.GVE_Weight);
				AssertEquals("GteVehicleEntry should have correct WeightUQ", "KG", vehicleEntry.GVE_WeightUQ);
				AssertEquals("GteVehicleEntry should have correct Direction", true, vehicleEntry.GVE_IsIncoming);
				AssertEquals("GteVehicleEntry should have correct DriverLicenseNumber", "1234 5678", vehicleEntry.GVE_DriverLicenseNumber);
				AssertEquals("GteVehicleEntry should have correct DriverName", "Jimothy", vehicleEntry.GVE_DriverName);
			});
		}

		public void TestGivenMatchingVehicleEntry_WhenReadUXMLWithInvalidValues_ThenNoValuesChanged()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.GVE_IsIncoming = true;
			vehicleEntry.GVE_GLN_Lane = LaneForTest.PK;
			vehicleEntry.GVE_Weight = 20;
			vehicleEntry.GVE_WeightUQ = "KG";
			vehicleEntry.GVE_DriverLicenseNumber = "1234 5678";
			vehicleEntry.GVE_DriverName = "Jimothy";

			Factory.SaveForTesting();

			#endregion

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TotalWeight = -5;
			shipment.WarehouseLocation = "UnknownLocation";
			shipment.AddAddInfo("IsIncoming", (ZString)"true");

			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);
			var newEntry = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry objects should match", vehicleEntry.PK, newEntry.PK);
				AssertEquals("GteVehicleEntry should have correct Lane", LaneForTest.PK, vehicleEntry.GVE_GLN_Lane);
				AssertEquals("GteVehicleEntry should have correct Weight", (ZDecimal)20, vehicleEntry.GVE_Weight);
				AssertEquals("GteVehicleEntry should have correct WeightUQ", "KG", vehicleEntry.GVE_WeightUQ);
				AssertEquals("GteVehicleEntry should have correct Direction", true, vehicleEntry.GVE_IsIncoming);
				AssertEquals("GteVehicleEntry should have correct DriverLicenseNumber", "1234 5678", vehicleEntry.GVE_DriverLicenseNumber);
				AssertEquals("GteVehicleEntry should have correct DriverName", "Jimothy", vehicleEntry.GVE_DriverName);
			});
		}

		public void TestGivenMatchingVehicleEntry_WhenReadUXMLWithValidChangedData_ThenValuesUpdated()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.GVE_IsIncoming = true;
			vehicleEntry.GVE_GLN_Lane = Factory.NewWithValidTestData<GteLane>().PK;
			vehicleEntry.GVE_Weight = 0;
			vehicleEntry.GVE_WeightUQ = "";
			vehicleEntry.GVE_DriverLicenseNumber = "1234 56789";
			vehicleEntry.GVE_DriverName = "Rebachel";

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);

			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);
			var newEntry = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry objects should match", vehicleEntry.PK, newEntry.PK);
				AssertEquals("GteVehicleEntry should have correct Lane", LaneForTest.PK, vehicleEntry.GVE_GLN_Lane);
				AssertEquals("GteVehicleEntry should have correct Weight", (ZDecimal)20, vehicleEntry.GVE_Weight);
				AssertEquals("GteVehicleEntry should have correct WeightUQ", "LB", vehicleEntry.GVE_WeightUQ);
				AssertEquals("GteVehicleEntry should have correct Direction", true, vehicleEntry.GVE_IsIncoming);
				AssertEquals("GteVehicleEntry should have correct DriverLicenseNumber", "1234 5678", vehicleEntry.GVE_DriverLicenseNumber);
				AssertEquals("GteVehicleEntry should have correct DriverName", "Jimothy", vehicleEntry.GVE_DriverName);
			});
		}
		public void TestGivenMatchingVehicleDriverBooking_WhenReadFullUXMLWithoutVehicleRun_ThenFieldsSetFromBooking()
		{
			#region Setup entities

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;

			var booking = gateMovement.GateMovementBooking.Booking;
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleDriverBooking = Factory.NewWithValidTestData<GteVehicleDriverBooking>();
			vehicleDriverBooking.GBD_GBK_Booking = booking.PK;
			vehicleDriverBooking.GBD_DriverName = "Jimothy";
			vehicleDriverBooking.GBD_DriverLicenseNumber = "1234 5678";

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);
			var vehicleEntry = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry should have correct Lane", LaneForTest.PK, vehicleEntry.GVE_GLN_Lane);
				AssertEquals("GteVehicleEntry should have correct Weight", (ZDecimal)20, vehicleEntry.GVE_Weight);
				AssertEquals("GteVehicleEntry should have correct WeightUQ", "LB", vehicleEntry.GVE_WeightUQ);
				AssertEquals("GteVehicleEntry should have correct Direction", true, vehicleEntry.GVE_IsIncoming);
				AssertEquals("GteVehicleEntry should have correct DriverLicenseNumber", "1234 5678", vehicleEntry.GVE_DriverLicenseNumber);
				AssertEquals("GteVehicleEntry should have correct DriverName", "Jimothy", vehicleEntry.GVE_DriverName);
			});
		}

		#endregion

		#region Unsuccessful import tests

		public void TestGivenNoMatchingVehicleEntry_WhenReadUXMLWithoutDirection_ThenThrowsError()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			shipment.SetAddInfoCollection(() => null);

			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			AssertExceptionThrown<DataObjectReadFailureException>(() => reader.ReadIntoBusinessObject());
		}

		public void TestGivenNoMatchingVehicleEntry_WhenReadUXMLWithInvalidDirection_ThenThrowsError()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			shipment.AddInfoCollection[0].Value = "NIL";

			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			AssertExceptionThrown<DataObjectReadFailureException>(() => reader.ReadIntoBusinessObject());
		}

		public void TestGivenNoMatchingVehicleEntry_WhenReadUXMLWithoutLane_ThenThrowsError()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			shipment.WarehouseLocation =  null;

			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			AssertExceptionThrown<DataObjectReadFailureException>(() => reader.ReadIntoBusinessObject());
		}

		public void TestGivenNoMatchingVehicleEntry_WhenReadUXMLWithInvalidLane_ThenThrowsError()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			shipment.WarehouseLocation = "INVALID LANE";

			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);
			AssertExceptionThrown<DataObjectReadFailureException>(() => reader.ReadIntoBusinessObject());
		}

		public void TestGivenNoMatchingVehicleEntry_WhenReadUXMLWithoutDriver_ThenThrowsError()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			shipment.VehicleRun.CrewCollection[0].FullName = null;

			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);

			AssertExceptionThrown<DataObjectReadFailureException>(() => reader.ReadIntoBusinessObject());
		}

		public void TestGivenNoMatchingVehicleEntry_WhenReadUXMLWithoutDriverLicense_ThenThrowsError()
		{
			#region Setup entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			#endregion

			var shipment = GetVehicleEntryShipmentForTest(isGateIn: true);
			shipment.VehicleRun.CrewCollection[0].LicenseNumber = null;

			var reader = new GteVehicleEntryDataObjectReader(vehicleMovement, booking, shipment, new DummyLogger(), Factory);
			AssertExceptionThrown<DataObjectReadFailureException>(() => reader.ReadIntoBusinessObject());
		}

		#endregion

		#region Helpers

		Shipment GetVehicleEntryShipmentForTest(bool isGateIn)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TotalWeight = 20;
			shipment.TotalWeightUnit = new UnitOfWeight() { Code = "LB" };
			shipment.WarehouseLocation = LaneForTest.Gate.GTE_Code + "|" + LaneForTest.GLN_Code;

			shipment.SetAddInfoCollection(() => new List<AddInfo>()
			{
				new AddInfo() { Key = "IsIncoming", Value = isGateIn ? "true" : "false" }
			});

			shipment.VehicleRun = new VehicleRun();
			shipment.VehicleRun.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.VehicleRun.SetCrewCollection(() => new List<Crew>()
			{
				new Crew() { FullName = "Jimothy", LicenseNumber = "1234 5678" }
			});

			return shipment;
		}

		GteLane LaneForTest
		{
			get
			{
				if (laneForTest == null)
				{
					laneForTest = Factory.NewWithValidTestData<GteLane>();
					laneForTest.GLN_Code = "LNE";
					laneForTest.Gate.GTE_Code = "GTE";
					laneForTest.Gate.GTE_GB_Branch = BranchForTest.PK;
					laneForTest.Gate.GTE_WW_Facility = WarehouseForTest.PK;
					Factory.SaveForTesting();
				}
				return laneForTest;
			}
			set => laneForTest = value;
		}
		GteLane laneForTest;

		GlbBranch BranchForTest
		{
			get
			{
				if (branchForTest == null)
				{
					branchForTest = Factory.NewWithValidTestData<GlbBranch>();
					Factory.SaveForTesting();
				}
				return branchForTest;
			}
			set => branchForTest = value;
		}
		GlbBranch branchForTest;

		IWhsWarehouse WarehouseForTest
		{
			get
			{
				if (warehouseForTest == null)
				{
					warehouseForTest = Factory.BOFactory.New<IWhsWarehouse>();
					((BusinessObject)warehouseForTest).FillWithValidTestData();
					warehouseForTest.WW_GB_RelatedCompanyBranch = BranchForTest.PK;
					Factory.SaveForTesting();
				}
				return warehouseForTest;
			}
			set => warehouseForTest = value;
		}
		IWhsWarehouse warehouseForTest;
		#endregion
	}
}
