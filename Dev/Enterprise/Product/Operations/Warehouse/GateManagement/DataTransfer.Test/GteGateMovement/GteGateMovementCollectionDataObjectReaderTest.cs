using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteGateMovementCollectionDataObjectReader))]
	public class GteGateMovementCollectionDataObjectReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			#region Setup

			var gateBooking = Factory.NewWithValidTestData<GteBooking>();
			var vehicleMovement = Factory.New<GteVehicleMovement>();
			vehicleMovement.GVM_GBK_MainBooking = gateBooking.PK;
			vehicleMovement.FillWithValidTestData();

			var movementBooking1 = Factory.New<GteGateMovementBooking>();
			movementBooking1.GBM_GBK_Booking = gateBooking.PK;
			movementBooking1.FillWithValidTestData();
			movementBooking1.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking1.GBM_Source = "VBS";

			var movementBooking2 = Factory.New<GteGateMovementBooking>();
			movementBooking2.GBM_GBK_Booking = gateBooking.PK;
			movementBooking2.FillWithValidTestData();
			movementBooking2.GBM_SourceReferenceNumber = "VBS-002";
			movementBooking2.GBM_Source = "VBS";

			gateBooking.GateMovementBookings.AddRange(new List<GteGateMovementBooking>() { movementBooking1, movementBooking2 });
			Factory.SaveForTesting();

			#endregion

			var shipmentCollection = new Shipment[]
			{
				GetGateMovementShipmentForTest(movementBooking1.GBM_MovementBookingNumber, null),
				GetGateMovementShipmentForTest(movementBooking2.GBM_MovementBookingNumber, null),
			};

			var collectionReader = new GteGateMovementCollectionDataObjectReader(vehicleMovement, gateBooking, new DummyLogger(), Factory, shipmentCollection);
			collectionReader.ReadIntoCollection();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: VehicleMovement should have two GateMovements", 2, vehicleMovement.GateMovements.Count);
				AssertContainsExactElementsInAnyOrder("Postcondition: GateMovements should link to MovementBookings", new string[] { "VBS-001", "VBS-002" }, vehicleMovement.GateMovements.Select(movement => movement.GateMovementBooking.GBM_SourceReferenceNumber));
				AssertEquals("Postcondition: GateMovement should have correct parent booking", gateBooking.PK, vehicleMovement.GateMovements[0].GateMovementBooking.Booking.PK);
			});
		}

		public void TestGivenBookingsExistForAllMovements_WhenImportUXML_ThenNoNewBookingsCreatedAndMovementsAreLinked()
		{
			#region Setup

			var gateBooking = Factory.NewWithValidTestData<GteBooking>();
			var vehicleMovement = Factory.New<GteVehicleMovement>();
			vehicleMovement.GVM_GBK_MainBooking = gateBooking.PK;
			vehicleMovement.FillWithValidTestData();

			var movementBooking1 = Factory.New<GteGateMovementBooking>();
			movementBooking1.GBM_GBK_Booking = gateBooking.PK;
			movementBooking1.FillWithValidTestData();
			movementBooking1.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking1.GBM_Source = "VBS";

			var movementBooking2 = Factory.New<GteGateMovementBooking>();
			movementBooking2.GBM_GBK_Booking = gateBooking.PK;
			movementBooking2.FillWithValidTestData();
			movementBooking2.GBM_SourceReferenceNumber = "VBS-002";
			movementBooking2.GBM_Source = "VBS";

			var movementBooking3 = Factory.New<GteGateMovementBooking>();
			movementBooking3.GBM_GBK_Booking = gateBooking.PK;
			movementBooking3.FillWithValidTestData();
			movementBooking3.GBM_SourceReferenceNumber = "VBS-003";
			movementBooking3.GBM_Source = "VBS";

			gateBooking.GateMovementBookings.AddRange(new List<GteGateMovementBooking>() { movementBooking1, movementBooking2, movementBooking3 });
			Factory.SaveForTesting();

			#endregion

			var shipmentCollection = new Shipment[]
			{
				GetGateMovementShipmentForTest(movementBooking1.GBM_MovementBookingNumber, null),
				GetGateMovementShipmentForTest(movementBooking2.GBM_MovementBookingNumber, null),
				GetGateMovementShipmentForTest(movementBooking3.GBM_MovementBookingNumber, null),
			};

			var collectionReader = new GteGateMovementCollectionDataObjectReader(vehicleMovement, gateBooking, new DummyLogger(), Factory, shipmentCollection);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: There should be one existing GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: There should be three existing GteGateMovementBookings", 3, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: There should be no existing GteGateMovements", 0, Factory.Load<GteGateMovement>(new ZQuery()).Length);
			});

			collectionReader.ReadIntoCollection();
			Factory.SaveForTesting();

			var movementBookingPKs = new ZGuid[] { movementBooking1.PK, movementBooking2.PK, movementBooking3.PK };
			var movementCollection = vehicleMovement.GateMovements.ToList();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: There should be one GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: There should be three GteGateMovementBookings", 3, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: There should be three GteGateMovements", 3, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertContainsExactElementsInAnyOrder("Postcondition: Each existing GteGateMovementBooking should be linked to new GteGateMovements", movementBookingPKs, movementCollection.Select(movement => movement.GGM_GBM_MovementBooking));
			});
		}

		public void TestGivenBookingsAndMovementsExistForAllMovements_WhenImportUXML_ThenNoNewBookingsOrMovementsCreated()
		{
			#region Setup

			var gateBooking = Factory.NewWithValidTestData<GteBooking>();
			var vehicleMovement = Factory.New<GteVehicleMovement>();
			vehicleMovement.GVM_GBK_MainBooking = gateBooking.PK;
			vehicleMovement.FillWithValidTestData();

			var movementBooking1 = Factory.New<GteGateMovementBooking>();
			movementBooking1.GBM_GBK_Booking = gateBooking.PK;
			movementBooking1.FillWithValidTestData();
			movementBooking1.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking1.GBM_Source = "VBS";

			var movementBooking2 = Factory.New<GteGateMovementBooking>();
			movementBooking2.GBM_GBK_Booking = gateBooking.PK;
			movementBooking2.FillWithValidTestData();
			movementBooking2.GBM_SourceReferenceNumber = "VBS-002";
			movementBooking2.GBM_Source = "VBS";

			var movementBooking3 = Factory.New<GteGateMovementBooking>();
			movementBooking3.GBM_GBK_Booking = gateBooking.PK;
			movementBooking3.FillWithValidTestData();
			movementBooking3.GBM_SourceReferenceNumber = "VBS-003";
			movementBooking3.GBM_Source = "VBS";

			var gateMovement1 = Factory.New<GteGateMovement>();
			gateMovement1.GGM_GBM_MovementBooking = movementBooking1.PK;
			gateMovement1.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement1.FillWithValidTestData();

			var gateMovement2 = Factory.New<GteGateMovement>();
			gateMovement2.GGM_GBM_MovementBooking = movementBooking2.PK;
			gateMovement2.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement2.FillWithValidTestData();

			var gateMovement3 = Factory.New<GteGateMovement>();
			gateMovement3.GGM_GBM_MovementBooking = movementBooking3.PK;
			gateMovement3.GGM_GVM_VehicleMovement = vehicleMovement.PK;
			gateMovement3.FillWithValidTestData();

			Factory.SaveForTesting();

			#endregion

			var shipmentCollection = new Shipment[]
			{
				GetGateMovementShipmentForTest(movementBooking1.GBM_MovementBookingNumber, null),
				GetGateMovementShipmentForTest(movementBooking2.GBM_MovementBookingNumber, null),
				GetGateMovementShipmentForTest(movementBooking3.GBM_MovementBookingNumber, null),
			};

			var collectionReader = new GteGateMovementCollectionDataObjectReader(vehicleMovement, gateBooking, new DummyLogger(), Factory, shipmentCollection);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: There should be one existing GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: There should be three existing GteGateMovementBookings", 3, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: There should be three existing GteGateMovements", 3, Factory.Load<GteGateMovement>(new ZQuery()).Length);
			});

			var oldGateMovementKeys = new ZGuid[] { gateMovement1.PK, gateMovement2.PK, gateMovement3.PK };

			collectionReader.ReadIntoCollection();
			Factory.SaveForTesting();

			var newGateMovementKeys = Factory.Load<GteGateMovement>(new ZQuery()).Select(movement => movement.PK);

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: There should be one GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: There should be three GteGateMovementBookings", 3, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: There should be three GteGateMovements", 3, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertContainsExactElementsInAnyOrder("Postcondition: No deletions or insertions to GteGateMovements", oldGateMovementKeys, newGateMovementKeys);
			});
		}

		public void TestGivenBookingsDoNotExistForMovements_WhenImportUXML_ThenNewBookingsCreatedAndMovementsLinked()
		{
			#region Setup

			var gateBooking = Factory.NewWithValidTestData<GteBooking>();
			var vehicleMovement = Factory.New<GteVehicleMovement>();
			vehicleMovement.GVM_GBK_MainBooking = gateBooking.PK;
			vehicleMovement.FillWithValidTestData();
			Factory.SaveForTesting();

			#endregion

			var shipmentCollection = new Shipment[]
			{
				GetGateMovementShipmentForTest("GBM-001", null),
				GetGateMovementShipmentForTest("GBM-002", null),
				GetGateMovementShipmentForTest("GBM-003", null),
			};

			var collectionReader = new GteGateMovementCollectionDataObjectReader(vehicleMovement, gateBooking, new DummyLogger(), Factory, shipmentCollection);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: There should be one existing GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: There should be no existing GteGateMovementBookings", 0, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: There should be no existing GteGateMovements", 0, Factory.Load<GteGateMovement>(new ZQuery()).Length);
			});

			collectionReader.ReadIntoCollection();
			Factory.SaveForTesting();

			var movementCollection = vehicleMovement.GateMovements.ToList();
			var movementBookings = movementCollection.Select(movement => movement.GateMovementBooking);
			var actualReferenceNumbers = movementBookings.Select(booking => booking.GBM_MovementBookingNumber);
			var expectedReferenceNumbers = new string[] { "GBM-001", "GBM-002", "GBM-003" };

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: There should be one GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: There should be three GteGateMovementBookings", 3, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: There should be three GteGateMovements", 3, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertContainsExactElementsInAnyOrder("Postcondition: Each GteGateMovementBooking has correct MovementBookingNumber", expectedReferenceNumbers, actualReferenceNumbers);
			});
		}

		public void TestGivenSomeBookingsExistForMovements_WhenImportUXML_ThenCorrectBookingsCreatedAndMovementsLinked()
		{
			#region Setup

			var gateBooking = Factory.NewWithValidTestData<GteBooking>();
			var vehicleMovement = Factory.New<GteVehicleMovement>();
			vehicleMovement.GVM_GBK_MainBooking = gateBooking.PK;
			vehicleMovement.FillWithValidTestData();

			var movementBooking1 = Factory.New<GteGateMovementBooking>();
			movementBooking1.GBM_GBK_Booking = gateBooking.PK;
			movementBooking1.FillWithValidTestData();
			movementBooking1.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking1.GBM_Source = "VBS";

			Factory.SaveForTesting();

			#endregion

			var shipmentCollection = new Shipment[]
			{
				GetGateMovementShipmentForTest(movementBooking1.GBM_MovementBookingNumber, null),
				GetGateMovementShipmentForTest("GBM-002", null),
				GetGateMovementShipmentForTest("GBM-003", null),
			};

			var collectionReader = new GteGateMovementCollectionDataObjectReader(vehicleMovement, gateBooking, new DummyLogger(), Factory, shipmentCollection);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: There should be one existing GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: There should be one existing GteGateMovementBooking", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: There should be no existing GteGateMovements", 0, Factory.Load<GteGateMovement>(new ZQuery()).Length);
			});

			collectionReader.ReadIntoCollection();
			Factory.SaveForTesting();

			var actualReferenceNumbers = vehicleMovement.GateMovements.Select(movement => movement.GateMovementBooking.GBM_MovementBookingNumber);
			var expectedReferenceNumbers = new string[] { movementBooking1.GBM_MovementBookingNumber, "GBM-002", "GBM-003" };

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: There should be one GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: There should be three GteGateMovementBookings", 3, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: There should be three GteGateMovements", 3, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertContainsExactElementsInAnyOrder("Postcondition: Each GteGateMovementBooking has correct MovementBookingNumber", expectedReferenceNumbers, actualReferenceNumbers);
			});
		}

		public void TestGivenMoreBookingsExistThanMovements_WhenImportUXML_ThenOldBookingsAreNotLinkedOrDeleted()
		{
			#region Setup

			var gateBooking = Factory.NewWithValidTestData<GteBooking>();
			var vehicleMovement = Factory.New<GteVehicleMovement>();
			vehicleMovement.GVM_GBK_MainBooking = gateBooking.PK;
			vehicleMovement.FillWithValidTestData();

			var movementBooking1 = Factory.New<GteGateMovementBooking>();
			movementBooking1.GBM_GBK_Booking = gateBooking.PK;
			movementBooking1.FillWithValidTestData();
			movementBooking1.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking1.GBM_Source = "VBS";

			var movementBooking2 = Factory.New<GteGateMovementBooking>();
			movementBooking2.GBM_GBK_Booking = gateBooking.PK;
			movementBooking2.FillWithValidTestData();
			movementBooking2.GBM_SourceReferenceNumber = "VBS-002";
			movementBooking2.GBM_Source = "VBS";

			var movementBooking3 = Factory.New<GteGateMovementBooking>();
			movementBooking3.GBM_GBK_Booking = gateBooking.PK;
			movementBooking3.FillWithValidTestData();
			movementBooking3.GBM_SourceReferenceNumber = "VBS-003";
			movementBooking3.GBM_Source = "VBS";

			var movementBooking4 = Factory.New<GteGateMovementBooking>();
			movementBooking4.GBM_GBK_Booking = gateBooking.PK;
			movementBooking4.FillWithValidTestData();
			movementBooking4.GBM_SourceReferenceNumber = "VBS-004";
			movementBooking4.GBM_Source = "VBS";

			gateBooking.GateMovementBookings.AddRange(new List<GteGateMovementBooking>() { movementBooking1, movementBooking2, movementBooking3, movementBooking4 });
			Factory.SaveForTesting();

			#endregion

			var shipmentCollection = new Shipment[]
			{
				GetGateMovementShipmentForTest(movementBooking1.GBM_MovementBookingNumber, null),
				GetGateMovementShipmentForTest(movementBooking2.GBM_MovementBookingNumber, null),
				GetGateMovementShipmentForTest(movementBooking3.GBM_MovementBookingNumber, null),
			};

			var collectionReader = new GteGateMovementCollectionDataObjectReader(vehicleMovement, gateBooking, new DummyLogger(), Factory, shipmentCollection);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: There should be one existing GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: There should be four existing GteGateMovementBookings", 4, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: There should be no existing GteGateMovements", 0, Factory.Load<GteGateMovement>(new ZQuery()).Length);
			});

			collectionReader.ReadIntoCollection();
			Factory.SaveForTesting();

			var movementBookingPKs = new ZGuid[] { movementBooking1.PK, movementBooking2.PK, movementBooking3.PK };
			var movementCollection = vehicleMovement.GateMovements.ToList();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: There should be one GteBooking", 1, Factory.Load<GteBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: There should be four GteGateMovementBookings", 4, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: There should be three GteGateMovements", 3, Factory.Load<GteGateMovement>(new ZQuery()).Length);
				AssertContainsExactElementsInAnyOrder("Postcondition: Each matched GteGateMovementBooking should be linked to new GteGateMovements", movementBookingPKs, movementCollection.Select(movement => movement.GGM_GBM_MovementBooking));
				AssertEquals("Postcondition: Unmatched GteGateMovementBooking should not have a GteGateMovement", 0, Factory.Load<GteGateMovement>(new ZQuery(GteGateMovementSchema.GGM_GBM_MovementBooking, movementBooking4.PK)).Length);
			});
		}

		public void TestGivenMovementBookingExistsForGateMovement_WhenImportUXML_ThenMovementBookingNotUpdated()
		{
			#region Setup Entities

			var dock = Factory.NewWithValidTestData<WhsLocation>();
			dock.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;

			var unitType = Factory.NewWithValidTestData<RefContainer>();
			var startTime = new DateTime(2024, 06, 13, 12, 00, 00);
			var endTime = new DateTime(2024, 06, 13, 13, 00, 00);

			var booking = Factory.NewWithValidTestData<GteBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			var movementBooking = Factory.New<GteGateMovementBooking>();
			movementBooking.GBM_GBK_Booking = booking.PK;
			movementBooking.FillWithValidTestData();
			movementBooking.GBM_SlotStartTime = startTime;
			movementBooking.GBM_SlotEndTime = endTime;
			movementBooking.GBM_TransportReference = "TRFN-ORGN";
			movementBooking.GBM_BookingReferenceNumber = "BRFN-ORGN";
			movementBooking.GBM_IsPickup = false;
			movementBooking.GBM_WL_Dock = dock.PK;
			movementBooking.GBM_RH_NKCargoType = "ORGN";
			movementBooking.GBM_F3_NKPackageType = "ORG";
			movementBooking.GBM_RC_UnitType = unitType.PK;
			movementBooking.GBM_UnitNumber = "UNT-ORGN";
			movementBooking.GBM_SourceReferenceNumber = "VBS-001";
			movementBooking.GBM_Source = "VBS";

			Factory.SaveForTesting();

			#endregion

			var shipmentCollection = new Shipment[] { GetGateMovementShipmentForTest(movementBooking.GBM_MovementBookingNumber, "VBS-001") };
			var collectionReader = new GteGateMovementCollectionDataObjectReader(vehicleMovement, booking, new DummyLogger(), Factory, shipmentCollection);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: There should only be one GteGateMovementBooking", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Precondition: MovementBooking.SlotStartTime should be correct", startTime, movementBooking.GBM_SlotStartTime.ToDateTime());
				AssertEquals("Precondition: MovementBooking.SlotEndTime should be correct", endTime, movementBooking.GBM_SlotEndTime.ToDateTime());
				AssertEquals("Precondition: MovementBooking.TransportReference should be correct", "TRFN-ORGN", movementBooking.GBM_TransportReference);
				AssertEquals("Precondition: MovementBooking.BookingReferenceNumber should be correct", "BRFN-ORGN", movementBooking.GBM_BookingReferenceNumber);
				AssertEquals("Precondition: MovementBooking.IsPickup should be correct", false, movementBooking.GBM_IsPickup);
				AssertEquals("Precondition: MovementBooking.Dock should be correct", dock.PK, movementBooking.GBM_WL_Dock);
				AssertEquals("Precondition: MovementBooking.CargoType should be correct", "ORGN", movementBooking.GBM_RH_NKCargoType);
				AssertEquals("Precondition: MovementBooking.PackageType should be correct", "ORG", movementBooking.GBM_F3_NKPackageType);
				AssertEquals("Precondition: MovementBooking.UnitType should be correct", unitType.PK, movementBooking.GBM_RC_UnitType);
				AssertEquals("Precondition: MovementBooking.UnitNumber should be correct", "UNT-ORGN", movementBooking.GBM_UnitNumber);
				AssertEquals("Precondition: MovementBooking.SourceReferenceNumber should be correct", "VBS-001", movementBooking.GBM_SourceReferenceNumber);
				AssertEquals("Precondition: MovementBooking.Source should be correct", "VBS", movementBooking.GBM_Source);
			});

			collectionReader.ReadIntoCollection();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: There should only be one GteGateMovementBooking", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: MovementBooking.SlotStartTime should be unchanged", startTime, movementBooking.GBM_SlotStartTime.ToDateTime());
				AssertEquals("Postcondition: MovementBooking.SlotEndTime should be unchanged", endTime, movementBooking.GBM_SlotEndTime.ToDateTime());
				AssertEquals("Postcondition: MovementBooking.TransportReference should be unchanged", "TRFN-ORGN", movementBooking.GBM_TransportReference);
				AssertEquals("Postcondition: MovementBooking.BookingReferenceNumber should be unchanged", "BRFN-ORGN", movementBooking.GBM_BookingReferenceNumber);
				AssertEquals("Postcondition: MovementBooking.IsPickup should be unchanged", false, movementBooking.GBM_IsPickup);
				AssertEquals("Postcondition: MovementBooking.Dock should be unchanged", dock.PK, movementBooking.GBM_WL_Dock);
				AssertEquals("Postcondition: MovementBooking.CargoType should be unchanged", "ORGN", movementBooking.GBM_RH_NKCargoType);
				AssertEquals("Postcondition: MovementBooking.PackageType should be unchanged", "ORG", movementBooking.GBM_F3_NKPackageType);
				AssertEquals("Postcondition: MovementBooking.UnitType should be unchanged", unitType.PK, movementBooking.GBM_RC_UnitType);
				AssertEquals("Postcondition: MovementBooking.UnitNumber should be unchanged", "UNT-ORGN", movementBooking.GBM_UnitNumber);
				AssertEquals("Postcondition: MovementBooking.SourceReferenceNumber should be unchanged", "VBS-001", movementBooking.GBM_SourceReferenceNumber);
				AssertEquals("Postcondition: MovementBooking.Source should be unchanged", "VBS", movementBooking.GBM_Source);
			});
		}

		public void TestGivenNoMovementBookingExistsForGateMovement_WhenImportUXML_ThenMovementBookingCreatedAndFieldsSetFromMovement()
		{
			#region Setup Entities

			var booking = Factory.NewWithValidTestData<GteBooking>();
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();

			Factory.SaveForTesting();

			#endregion

			var shipmentCollection = new Shipment[] { GetGateMovementShipmentForTest("GBM-001", "VBS-001") };
			var collectionReader = new GteGateMovementCollectionDataObjectReader(vehicleMovement, booking, new DummyLogger(), Factory, shipmentCollection);

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: There should only be no GteGateMovementBooking", 0, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
			});

			collectionReader.ReadIntoCollection();
			Factory.SaveForTesting();

			var gateMovement = Factory.LoadTop1<GteGateMovement>(new ZQuery());
			var movementBooking = Factory.LoadTop1<GteGateMovementBooking>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertEquals("Postcondition: There should only be one GteGateMovementBooking", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: There should only be one GteGateMovement", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);
				AssertEquals("Postcondition: MovementBooking.SlotStartTime should be blank", ZDateTimeOffset.Empty, movementBooking.GBM_SlotStartTime);
				AssertEquals("Postcondition: MovementBooking.SlotEndTime should be blank", ZDateTimeOffset.Empty, movementBooking.GBM_SlotEndTime);
				AssertEquals("Postcondition: MovementBooking.TransportReference should be set from GateMovement", gateMovement.GGM_TransportReference, movementBooking.GBM_TransportReference);
				AssertEquals("Postcondition: MovementBooking.BookingReferenceNumber should be blank", "", movementBooking.GBM_BookingReferenceNumber);
				AssertEquals("Postcondition: MovementBooking.IsPickup should be set from GateMovement", gateMovement.GGM_IsPickup, movementBooking.GBM_IsPickup);
				AssertEquals("Postcondition: MovementBooking.Dock should be set from GateMovement", gateMovement.GGM_WL_Dock, movementBooking.GBM_WL_Dock);
				AssertEquals("Postcondition: MovementBooking.CargoType should be set from GateMovement", gateMovement.GGM_RH_NKCargoType, movementBooking.GBM_RH_NKCargoType);
				AssertEquals("Postcondition: MovementBooking.PackageType should be set from GateMovement", gateMovement.GGM_F3_NKPackageType, movementBooking.GBM_F3_NKPackageType);
				AssertEquals("Postcondition: MovementBooking.UnitType should be set from GateMovement", gateMovement.GGM_RC_UnitType, movementBooking.GBM_RC_UnitType);
				AssertEquals("Postcondition: MovementBooking.UnitNumber should be set from GateMovement", gateMovement.GGM_UnitNumber, movementBooking.GBM_UnitNumber);
				AssertEquals("Postcondition: MovementBooking.SourceReferenceNumber should be set from Shipment", "VBS-001", movementBooking.GBM_SourceReferenceNumber);
				AssertEquals("Postcondition: MovementBooking.Source should be set from Shipment", "VBS", movementBooking.GBM_Source);
				AssertEquals("Postcondition: MovementBooking.MovementBookingNumber should be set from Shipment", "GBM-001", movementBooking.GBM_MovementBookingNumber);
			});
		}

		#region Helpers

		Shipment GetGateMovementShipmentForTest(ZString movementBookingNumber, ZString? sourceReference)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.BookingConfirmationReference = "BRN001";

			if (!string.IsNullOrEmpty(sourceReference))
			{
				shipment.AddAdditionalReference(AdditionalReferenceTypes.Codes.BookingPartyReference, AdditionalReferenceTypes.Descriptions.BookingPartyReference, sourceReference);
			}
			shipment.AddAdditionalReference(
				GateManagementConstants.ReferenceTypes.Codes.MovementBookingNumber,
				GateManagementConstants.ReferenceTypes.Descriptions.MovementBookingNumber,
				movementBookingNumber
			);
			shipment.AddAdditionalReference(AdditionalReferenceTypes.Codes.TransportReference, AdditionalReferenceTypes.Descriptions.TransportReference, "TRF001");

			shipment.TransportBookingDirection = new TransportBookingDirection()
			{
				Code = GateManagementConstants.TransportBookingDirections.Codes.Pickup
			};
			shipment.WarehouseLocation = DockForTest.ToLocationString();
			var address = DockForTest.Warehouse.WarehouseAddress;
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				new OrganizationAddress()
				{
					AddressType = nameof(DocAddressType.LocalCartageYard),
					Address1 = address.Address1,
					Address2 = address.Address2,
					City = address.City,
					Postcode = address.Postcode,
					AddressShortCode = address.OA_Code,
					OrganizationCode = address.Header.OH_Code,
				}
			});

			shipment.SetDateCollection(() => new List<Date>()
			{
				new Date() { Type = DateType.Start, Value = new ZDateTime(2024, 04, 18, 12, 0, 0) },
				new Date() { Type = DateType.End, Value = new ZDateTime(2024, 04, 19, 12, 0, 0) }
			});

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>()
			{
				new PackingLine() { Commodity = new Commodity() { Code = "AABT" }, PackType = new PackageType() { Code = "BAG" } }
			});

			shipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container() { ContainerNumber = "UNT-001", ContainerType = new ContainerType() { Code = UnitTypeForTest.RC_Code } }
			});

			return shipment;
		}

		WhsLocation DockForTest
		{
			get
			{
				if (dockForTest == null)
				{
					dockForTest = Factory.NewWithValidTestData<WhsLocation>();
					dockForTest.WLV_WA_PutawayArea = Factory.NewWithValidTestData<WhsArea>().PK;
					dockForTest.Warehouse.WW_IsActive = true;
					dockForTest.Warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
					var address = dockForTest.Warehouse.WarehouseAddress;
					address.Address1 = "Address 1";
					address.Address2 = "Address 2";
					address.City = "SYD";
					address.Postcode = "0000";
					address.OA_Code = "BLANK SYD";
					address.Header.OH_Code = "ZZ";
					Factory.SaveForTesting();
				}
				return dockForTest;
			}
			set => dockForTest = value;
		}
		WhsLocation dockForTest;

		RefContainer UnitTypeForTest
		{
			get
			{
				if (unitTypeForTest == null)
				{
					unitTypeForTest = Factory.NewWithValidTestData<RefContainer>();
					unitTypeForTest.RC_Code = "20FR";
					Factory.SaveForTesting();
				}
				return unitTypeForTest;
			}
			set => unitTypeForTest = value;
		}
		RefContainer unitTypeForTest;

		#endregion
	}
}
