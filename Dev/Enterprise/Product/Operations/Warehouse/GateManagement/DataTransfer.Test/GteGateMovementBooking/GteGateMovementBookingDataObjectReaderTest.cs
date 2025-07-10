using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using NUnit.Framework;
using static Enterprise.Core.Constants.GateManagementConstants;
using AdditionalReference = Enterprise.UniversalDataBuss.DataObjects.Universal.AdditionalReference;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteGateMovementBookingDataObjectReader))]
	public class GteGateMovementBookingDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		[UseSnapshotProtection]
		public void TestReadFromDataObject()
		{
			#region Setup UXML

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.GateBooking, "");
			shipment.BookingConfirmationReference = "GteBooking001";
			shipment.SlotDateTime = new ZDateTime(2022, 1, 1, 10, 10, 10);

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress
				{
					AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress),
					OrganizationCode = "ABC"
				},
				new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
				{
					AddressType = nameof(DocAddressType.LocalCartageYard),
					Address1 = "1 main st",
					City = "Sydney",
					Postcode = "2020",
					State = "NSW",
					Country = new Country() { Code = "AU" }
				}
			});

			var registrationNumberType = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.ContainerChainCommunityCode };
			var registrationNumber = new RegistrationNumber() { Type = registrationNumberType, Value = "CC123" };
			shipment.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Value == nameof(DocAddressType.LocalCartageYard)).SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { registrationNumber });

			var subShipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BookingConfirmationReference = "GateMovementBooking1"
			};

			subShipment1.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Delivery };
			subShipment1.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackType = new PackageType()
					{
						Code = Core.Constants.PkgUnit.Package,
						Description = Core.Constants.PkgUnit.GetDescription(Core.Constants.PkgUnit.Package)
					},
					PackQty = 1000
				}
			});

			var subShipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				BookingConfirmationReference = "GateMovementBooking2"
			};

			subShipment2.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Delivery };
			subShipment2.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					PackType = new PackageType()
					{
						Code = Core.Constants.PkgUnit.Box,
						Description = Core.Constants.PkgUnit.GetDescription(Core.Constants.PkgUnit.Box)
					},
					PackQty = 2000
				}
			});

			subShipment1.FacilityJobType = new CodeDescriptionPair()
			{
				Code = Core.Constants.FacilityJobType.Codes.Container
			};

			var refContainer1 = Factory.NewWithValidTestData<RefContainer>();

			var container1 = new Container();
			container1.ContainerNumber = "TESTContainer";
			container1.ContainerType = new ContainerType();
			container1.ContainerType.Code = refContainer1.RC_Code;

			subShipment1.SetContainerCollection(() => new DataObjectList<Container> { container1 });
			subShipment1.SetDateCollection(() => new List<Date>() { Date.New(DateType.Start, ZBool.False, new ZDateTime(2022, 1, 1, 10, 10, 10)) });

			var refContainer2 = Factory.NewWithValidTestData<RefContainer>();
			refContainer2.RC_Code = "TST2";

			var container2 = new Container();
			container2.ContainerNumber = "TESTContainer";
			container2.ContainerType = new ContainerType();
			container2.ContainerType.Code = refContainer2.RC_Code;

			subShipment2.SetContainerCollection(() => new DataObjectList<Container> { container2 });
			subShipment2.SetDateCollection(() => new List<Date>() { Date.New(DateType.Start, ZBool.False, new ZDateTime(2022, 1, 1, 10, 10, 10)) });

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment1, subShipment2 });

			var vehicleMovement = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var vehicleRun = new VehicleRun();
			var vehicle = new Vehicle();
			var vehicleRegistration = new Registration();

			vehicleRegistration.Number = "TESTING123";
			vehicle.Registration = vehicleRegistration;
			vehicleRun.Vehicle = vehicle;
			vehicleMovement.VehicleRun = vehicleRun;

			shipment.SetPreCarriageShipmentCollection(() => new List<UniversalShipment> { vehicleMovement });

			var crew = new Crew();
			crew.FullName = "Thomas Jefferson";
			crew.LicenseNumber = "12345678";
			crew.CrewType = CrewType.Driver;

			shipment.VehicleRun = new VehicleRun();
			shipment.VehicleRun.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.VehicleRun.SetCrewCollection(() => new List<Crew> { crew });

			#endregion Setup UXML

			#region Read the UXML

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			booking.GBK_ReferenceNumber = "GteBooking001";

			Factory.SaveForTesting();

			#endregion Read the UXML

			CombineAssertions(() =>
			{
				var newFactory = new BusinessObjectFactory();
				booking = newFactory.Load<GteBooking>(booking.PK);

				var gateMovementBooking1 = booking.GateMovementBookings.FirstOrDefault(b => b.GBM_BookingReferenceNumber == "GateMovementBooking1");
				var gateMovementBooking2 = booking.GateMovementBookings.FirstOrDefault(b => b.GBM_BookingReferenceNumber == "GateMovementBooking2");

				AssertEquals(2, booking.GateMovementBookings.Count);
				AssertNotNull("Expected a GateMovementBooking with GBM_BookingReferenceNumber GateMovementBooking1", gateMovementBooking1);
				AssertNotNull("Expected a GateMovementBooking with GBM_BookingReferenceNumber GateMovementBooking2", gateMovementBooking2);

				AssertEquals(new ZDateTimeOffset(2022, 1, 1, 10, 10, 10), gateMovementBooking1.GBM_SlotStartTime);
				AssertEquals(new ZDateTimeOffset(2022, 1, 1, 10, 10, 10), gateMovementBooking2.GBM_SlotStartTime);
				AssertEquals(Core.Constants.PkgUnit.Package, gateMovementBooking1.GBM_F3_NKPackageType);
				AssertEquals(Core.Constants.PkgUnit.Box, gateMovementBooking2.GBM_F3_NKPackageType);
				AssertEquals("Expect PackingLineDataObject.PackQty -> GBM_Quantity", 1000, gateMovementBooking1.GBM_Quantity);
				AssertEquals("Expect PackingLineDataObject.PackQty -> GBM_Quantity", 2000, gateMovementBooking2.GBM_Quantity);

				AssertEquals(container1.ContainerNumber, gateMovementBooking1.GBM_UnitNumber);
				AssertEquals(refContainer1.PK, gateMovementBooking1.GBM_RC_UnitType);
				AssertEquals("Container Number details should be read", "TESTContainer", gateMovementBooking2.GBM_UnitNumber);
				AssertEquals("Container Type details should be read", "TST2", gateMovementBooking2.UnitType.RC_Code);
			});
		}

		public void TestGivenMatchingExistingGateMovementBooking_WhenReadFromDataObject_ThenDoesNotCreateNewGateMovementBooking()
		{
			#region Setup UXML

			var shipment = SetupShipment();
			var subShipment = shipment.SubShipmentCollection.First();
			subShipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference()
				{
					Type = new EntryType()
					{
						Code = AdditionalReferenceTypes.Codes.BookingPartyReference,
						Description = AdditionalReferenceTypes.Descriptions.BookingPartyReference
					},
					ReferenceNumber = "VBS00000001"
				}
			});

			#endregion

			#region Setup Factory Entities

			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			transportCompany.OH_Code = "XYZ";

			var gateBooking = Factory.NewWithValidTestData<GteBooking>();
			gateBooking.GBK_ReferenceNumber = "";
			gateBooking.GBK_BookingType = BookingTypes.Regular;
			gateBooking.GBK_OH_TransportCompany = transportCompany.PK;

			var gateMovementBooking = Factory.New<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = gateBooking.PK;
			gateMovementBooking.GBM_Source = "VBS";
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS00000001";
			gateMovementBooking.GBM_F3_NKPackageType = Core.Constants.PkgUnit.Box;

			Factory.SaveForTesting();

			#endregion

			AssertEquals("Precondition: Expected one existing GateMovementBooking", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var gateMovementBookings = Factory.Load<GteGateMovementBooking>(new ZQuery());
			AssertEquals("Expected there to only be one GateMovementBooking", 1, gateMovementBookings.Length);
			AssertEquals("Expected GateMovementBooking to have correct source reference number", "VBS00000001", gateMovementBookings[0].GBM_SourceReferenceNumber);
		}

		public void TestGivenNoMatchingExistingGateMovementBooking_WhenReadFromDataObject_ThenCreatesNewGateMovementBooking()
		{
			#region Setup UXML

			var shipment = SetupShipment();
			var subShipment = shipment.SubShipmentCollection.First();
			subShipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference()
				{
					Type = new EntryType()
					{
						Code = AdditionalReferenceTypes.Codes.BookingPartyReference,
						Description = AdditionalReferenceTypes.Descriptions.BookingPartyReference
					},
					ReferenceNumber = "VBS00000001"
				}
			});

			#endregion

			#region Setup Factory Entities

			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			transportCompany.OH_Code = "XYZ";

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "GBK100";
			booking.GBK_BookingType = BookingTypes.Regular;
			booking.GBK_OH_TransportCompany = transportCompany.PK;

			var gateMovementBooking = Factory.New<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_Source = "VBS";
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS00000002";
			gateMovementBooking.GBM_F3_NKPackageType = Core.Constants.PkgUnit.Box;

			Factory.SaveForTesting();

			#endregion

			AssertEquals("Precondition: Expected one existing GateMovementBooking", 1, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);

			new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var gateMovementBookings = Factory.Load<GteGateMovementBooking>(new ZQuery());
			AssertEquals("Expected a new GteGateMovementBooking to be created", 2, gateMovementBookings.Length);

			var newGateMovementBooking = gateMovementBookings.FirstOrDefault(x => x.GBM_SourceReferenceNumber == "VBS00000001");
			AssertNotNull("Expected GateMovementBooking to exist with new SourceReferenceNumber", newGateMovementBooking);
			AssertEquals("Expected new GateMovementBooking to have Source set as VBS", "VBS", newGateMovementBooking.GBM_Source);
		}

		public void TestGivenNonMatchingGBM_WhenUpdatingGBK_ThenOverrwriteAllGBMsInGBK()
		{
			#region Setup UXML

			var shipment = SetupShipment(bookingReferenceNumber: "GBK100");
			var subShipment = shipment.SubShipmentCollection.First();
			subShipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference()
				{
					Type = new EntryType()
					{
						Code = AdditionalReferenceTypes.Codes.BookingPartyReference,
						Description = AdditionalReferenceTypes.Descriptions.BookingPartyReference
					},
					ReferenceNumber = "VBS00000001"
				}
			});

			#endregion

			#region Setup Factory Entities

			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			transportCompany.OH_Code = "XYZ";

			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "GBK100";
			booking.GBK_BookingType = BookingTypes.Regular;
			booking.GBK_OH_TransportCompany = transportCompany.PK;

			var gateMovementBooking1 = Factory.New<GteGateMovementBooking>();
			gateMovementBooking1.GBM_GBK_Booking = booking.PK;
			gateMovementBooking1.GBM_Source = "VBS";
			gateMovementBooking1.GBM_SourceReferenceNumber = "VBS00000002";
			gateMovementBooking1.GBM_F3_NKPackageType = Core.Constants.PkgUnit.Box;

			var gateMovementBooking2 = Factory.New<GteGateMovementBooking>();
			gateMovementBooking2.GBM_GBK_Booking = booking.PK;
			gateMovementBooking2.GBM_Source = "VBS";
			gateMovementBooking2.GBM_SourceReferenceNumber = "VBS00000003";
			gateMovementBooking2.GBM_F3_NKPackageType = Core.Constants.PkgUnit.Box;

			Factory.SaveForTesting();

			#endregion

			AssertEquals("Precondition: Expected one existing GateMovementBooking", 2, Factory.Load<GteGateMovementBooking>(new ZQuery()).Length);

			new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newBooking = Factory.Load<GteBooking>(new ZQuery()).FirstOrDefault();

			AssertEquals("Expected there to be GateMovementBooking in GBK100", 1, newBooking.GateMovementBookings.Count);
			AssertEquals("Expected VBS00000002 and VBS00000003 to be deleted since it is not matched with the UXML, and VBS00000001 created from the UXML", "VBS00000001", newBooking.GateMovementBookings.FirstOrDefault().GBM_SourceReferenceNumber);
		}

		public void TestGivenBookingPartyReferenceWithNoReferenceNumber_WhenReadFromDataObject_ThenCreatesNewGateMovementBooking()
		{
			#region Setup UXML

			var shipment = SetupShipment();
			var subShipment = shipment.SubShipmentCollection.First();
			subShipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference()
				{
					Type = new EntryType()
					{
						Code = AdditionalReferenceTypes.Codes.BookingPartyReference,
						Description = AdditionalReferenceTypes.Descriptions.BookingPartyReference
					},
				}
			});

			#endregion

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var gateMovementBookings = Factory.Load<GteGateMovementBooking>(new ZQuery());
			AssertEquals("Expected a new GteGateMovementBooking to be created", 1, gateMovementBookings.Length);

			var newGateMovementBooking = gateMovementBookings.FirstOrDefault();
			AssertEquals("Expected new GateMovementBooking to have no source reference number set", "", newGateMovementBooking.GBM_SourceReferenceNumber);
			AssertEquals("Expected new GateMovementBooking to have no source set", "", newGateMovementBooking.GBM_Source);
		}

		public void TestGivenBookingPartyReferenceWithBlankReferenceNumber_WhenReadFromDataObject_ThenCreatesNewGateMovementBooking()
		{
			#region Setup UXML

			var shipment = SetupShipment();
			var subShipment = shipment.SubShipmentCollection.First();
			subShipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference()
				{
					Type = new EntryType()
					{
						Code = AdditionalReferenceTypes.Codes.BookingPartyReference,
						Description = AdditionalReferenceTypes.Descriptions.BookingPartyReference
					},
					ReferenceNumber = ""
				}
			});

			#endregion

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var gateMovementBookings = Factory.Load<GteGateMovementBooking>(new ZQuery());
			AssertEquals("Expected a new GteGateMovementBooking to be created", 1, gateMovementBookings.Length);

			var newGateMovementBooking = gateMovementBookings.FirstOrDefault();
			AssertEquals("Expected new GateMovementBooking to have no source reference number set", "", newGateMovementBooking.GBM_SourceReferenceNumber);
			AssertEquals("Expected new GateMovementBooking to have no source set", "", newGateMovementBooking.GBM_Source);
		}

		public void TestReadTransportBookingDirection()
		{
			var shipment = SetupShipment();
			var subShipment = shipment.SubShipmentCollection.First();

			subShipment.TransportBookingDirection = new TransportBookingDirection() { Code = "INV" };
			var assertionMsg = $"Reject XML import if TransportBookingDirection is not one of '{TransportBookingDirections.Codes.Delivery}' or '{TransportBookingDirections.Codes.Pickup}'.";
			var exceptionMsg = $"Failed to read 'Transport Booking Direction'. Value must be either '{TransportBookingDirections.Codes.Delivery}' or '{TransportBookingDirections.Codes.Pickup}'.";
			AssertExceptionThrown<DataObjectReadFailureException>(assertionMsg, exceptionMsg, () => new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject());

			subShipment.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Delivery };
			var deliverybooking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var dlvGateMovementBooking = deliverybooking.GateMovementBookings.FirstOrDefault();

			AssertNotNull("Expected the generated booking to contain one gate movement booking.", dlvGateMovementBooking);
			Assert($"Expected IsPickup to be set to false if TransportBookingDirection is set to '{TransportBookingDirections.Codes.Delivery}'.", !dlvGateMovementBooking.GBM_IsPickup);

			subShipment.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Pickup };
			var pickupBooking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var picGateMovementBooking = pickupBooking.GateMovementBookings.FirstOrDefault();

			AssertNotNull("Expected the generated booking to contain one gate movement booking.", picGateMovementBooking);
			Assert($"Expected IsPickup to be set to true if TransportBookingDirection is set to '{TransportBookingDirections.Codes.Pickup}'.", picGateMovementBooking.GBM_IsPickup);
		}

		public void TestGivenInvalidRefContainerCode_WhenContainerTypeIsUnmatched_ThenSaveWithoutContainerType()
		{
			var shipment = SetupShipment();
			var subShipment = shipment.SubShipmentCollection.First();

			var container = new Container();
			container.ContainerNumber = "TEST1234567";
			container.ContainerType = new ContainerType() { Code = "UNMATCHED" };
			subShipment.SetContainerCollection(() => new DataObjectList<Container> { container });

			var reader = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory);
			var booking = reader.ReadIntoBusinessObject();
			var gateMovementBooking = booking.GateMovementBookings.FirstOrDefault();

			AssertEquals("Container Number is set correctly", "TEST1234567", gateMovementBooking.GBM_UnitNumber);
			AssertEquals("Container Type is ignored", ZGuid.Empty, gateMovementBooking.GBM_RC_UnitType);
		}

		public void TestGivenInvalidRefContainerCode_WhenContainerTypeIsMissing_ThenSaveWithoutContainerType()
		{
			var shipment = SetupShipment();
			var subShipment = shipment.SubShipmentCollection.First();

			var container = new Container();
			container.ContainerNumber = "TEST1234567";
			container.ContainerType = null;
			subShipment.SetContainerCollection(() => new DataObjectList<Container> { container });

			var reader = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory);
			var booking = reader.ReadIntoBusinessObject();
			var gateMovementBooking = booking.GateMovementBookings.FirstOrDefault();

			AssertEquals("Container Number is set correctly", "TEST1234567", gateMovementBooking.GBM_UnitNumber);
			AssertEquals("Container Type is ignored", ZGuid.Empty, gateMovementBooking.GBM_RC_UnitType);
		}

		public void TestPackageTypeAndCargoType()
		{
			#region Setup UXML

			var shipment = SetupShipment();
			var subShipment = shipment.SubShipmentCollection.First();

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Commodity = new Commodity
					{
						Code = "MNSC",
						Description = "Manuscripts"
					},
					PackType = new PackageType
					{
						Code = Core.Constants.PkgUnit.Box,
						Description = Core.Constants.PkgUnit.GetDescription(Core.Constants.PkgUnit.Box)
					}
				}
			});

			#endregion Setup UXML

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var gateBooking = booking.GateMovementBookings.FirstOrDefault();

			AssertEquals("Expected gateBooking to be 'BOX' package type", "BOX", gateBooking.GBM_F3_NKPackageType);
			AssertEquals("Expected gateBooking to be 'MNSC' cargo type", "MNSC", gateBooking.GBM_RH_NKCargoType);
		}

		public void TestPackageTypeAndCargoType_WhenPackageTypeAndCargoTypeCodeIsBlank()
		{
			#region Setup UXML

			var shipment = SetupShipment();
			var subShipment = shipment.SubShipmentCollection.First();

			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Commodity = new Commodity
					{
						Code = "",
					},
					PackType = new PackageType
					{
						Code = "",
					}
				}
			});

			#endregion Setup UXML

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var gateBooking = booking.GateMovementBookings.FirstOrDefault();

			AssertEquals("Expected gateBooking's package type to be blank", "", gateBooking.GBM_F3_NKPackageType);
			AssertEquals("Expected gateBooking's cargo type to be blank", "", gateBooking.GBM_RH_NKCargoType);
		}

		public void TestUXMLMustHaveAtLeastOneGBM_ElseThrowException()
		{
			var shipment1 = SetupShipment();
			var shipment2 = SetupShipment();
			shipment1.SetSubShipmentCollection(() => null);
			shipment2.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			CombineAssertions(() =>
			{
				AssertExceptionThrown<DataObjectReadFailureException>("Booking must contain at least one Gate Movement Booking.", () => new GteBookingDataObjectReader(shipment1, new DummyLogger(), Factory).ReadIntoBusinessObject());
				AssertExceptionThrown<DataObjectReadFailureException>("Booking must contain at least one Gate Movement Booking.", () => new GteBookingDataObjectReader(shipment2, new DummyLogger(), Factory).ReadIntoBusinessObject());
			});
		}

		public void TestGivenMultipleTRFReferences_ThenThrowException()
		{
			#region Setup UXML

			var shipment = SetupShipment();
			var subShipment = shipment.SubShipmentCollection.First();

			subShipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference() { Type = new EntryType() { Code = AdditionalReferenceTypes.Codes.TransportReference, Description = AdditionalReferenceTypes.Descriptions.TransportReference }, ReferenceNumber = "1" },
				new AdditionalReference() { Type = new EntryType() { Code = AdditionalReferenceTypes.Codes.TransportReference, Description = AdditionalReferenceTypes.Descriptions.TransportReference }, ReferenceNumber = "2" }
			});

			#endregion Setup UXML

			var assertionMsg1 = $"Expected to throw an error as more than one TRF";
			var exceptionMsg1 = $"Failed to read Additional References. There can not be more than one reference with a 'TRF' code. However there is currently {subShipment.AdditionalReferenceCollection.Count}.";
			AssertExceptionThrown<DataObjectReadFailureException>(assertionMsg1, exceptionMsg1, () => new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject());
		}

		public void TestReadIntoObjectForTransportReference()
		{
			#region Setup UXML

			var shipment = SetupShipment();
			var subShipment = shipment.SubShipmentCollection.First();

			subShipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference() { Type = new EntryType() { Code = AdditionalReferenceTypes.Codes.TransportReference, Description = AdditionalReferenceTypes.Descriptions.TransportReference }, ReferenceNumber = "TRF_2" }
			});

			#endregion Setup UXML

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();

			AssertEquals("Expected the reference number to be 'TRF_2'", "TRF_2", booking.GateMovementBookings.Cast<GteGateMovementBooking>().FirstOrDefault().GBM_TransportReference);
		}

		public void TestGivenEmptyAdditionalReferenceCollection_ThenTransportReferenceIsEmpty()
		{
			#region Setup UXML

			var shipment = SetupShipment();
			var subShipment = shipment.SubShipmentCollection.First();

			subShipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());

			#endregion Setup UXML

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();

			AssertEquals("Expected empty reference number", ZString.Empty, booking.GateMovementBookings.Cast<GteGateMovementBooking>().FirstOrDefault().GBM_TransportReference);
		}

		public void TestReadIntoObjectForTransportReferenceWithMultipleAdditionalReference()
		{
			#region Setup UXML

			var shipment = SetupShipment();
			var subShipment = shipment.SubShipmentCollection.First();

			subShipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
			{
				new AdditionalReference() { Type = new EntryType() { Code = AdditionalReferenceTypes.Codes.HouseBill, Description = AdditionalReferenceTypes.Descriptions.HouseBill }, ReferenceNumber = "1" },
				new AdditionalReference() { Type = new EntryType() { Code = AdditionalReferenceTypes.Codes.MasterBill, Description = AdditionalReferenceTypes.Descriptions.MasterBill }, ReferenceNumber = "2" },
				new AdditionalReference() { Type = new EntryType() { Code = AdditionalReferenceTypes.Codes.TransportReference, Description = AdditionalReferenceTypes.Descriptions.TransportReference }, ReferenceNumber = "3" }
			});

			#endregion Setup UXML

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();

			AssertEquals("Expected reference number of '3'", "3", booking.GateMovementBookings.Cast<GteGateMovementBooking>().FirstOrDefault().GBM_TransportReference);
		}

		public void TestUXMLWhenNoDates_DoesNotThrowException()
		{
			var shipment1 = SetupShipment();

			shipment1.SubShipmentCollection.FirstOrDefault()?.SetDateCollection(() => null);
			AssertNoExceptionThrown("Expect Shipment to be read successfully when no DateCollection set", () => new GteBookingDataObjectReader(shipment1, new DummyLogger(), Factory).ReadIntoBusinessObject());

			shipment1.SubShipmentCollection.FirstOrDefault()?.SetDateCollection(() => new List<Date>());
			AssertNoExceptionThrown("Expect Shipment to be read successfully when empty DateCollection set", () => new GteBookingDataObjectReader(shipment1, new DummyLogger(), Factory).ReadIntoBusinessObject());
		}

		public void TestUXMLWhenDatesSet_ReadsCorrectlyIntoObject()
		{
			#region Setup UXML

			var shipment = SetupShipment();
			var startDate = Date.New(DateType.Start, ZBool.False, ZDateTime.Now);
			var endDate = Date.New(DateType.End, ZBool.False, ZDateTime.Now.AddHours(1));
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var subShipmentNoSlotBooked = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipmentNoSlotBooked.SetDateCollection(() => new List<Date>());
			subShipmentNoSlotBooked.BookingConfirmationReference = "NoSlot";
			subShipmentNoSlotBooked.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Pickup };

			var subShipmentStartSlotBooked = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipmentStartSlotBooked.SetDateCollection(() => new List<Date>() { startDate });
			subShipmentStartSlotBooked.BookingConfirmationReference = "SlotStartSet";
			subShipmentStartSlotBooked.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Pickup };

			var subShipmentEndSlotBooked = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipmentEndSlotBooked.SetDateCollection(() => new List<Date>() { endDate });
			subShipmentEndSlotBooked.BookingConfirmationReference = "SlotEndSet";
			subShipmentEndSlotBooked.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Pickup };

			var subShipmentFullSlotBooked = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipmentFullSlotBooked.SetDateCollection(() => new List<Date>() { startDate, endDate });
			subShipmentFullSlotBooked.BookingConfirmationReference = "SlotFullySet";
			subShipmentFullSlotBooked.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Pickup };

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipmentNoSlotBooked, subShipmentStartSlotBooked, subShipmentEndSlotBooked, subShipmentFullSlotBooked });

			#endregion

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var bookingWithoutSlot = booking.GateMovementBookings.FirstOrDefault(booking => (booking).GBM_BookingReferenceNumber == "NoSlot");
			var bookingWithSlotStart = booking.GateMovementBookings.FirstOrDefault(booking => (booking).GBM_BookingReferenceNumber == "SlotStartSet");
			var bookingWithSlotEnd = booking.GateMovementBookings.FirstOrDefault(booking => (booking).GBM_BookingReferenceNumber == "SlotEndSet");
			var bookingWithFullSlot = booking.GateMovementBookings.FirstOrDefault(booking => (booking).GBM_BookingReferenceNumber == "SlotFullySet");

			CombineAssertions(() =>
			{
				AssertNotNull("Expected there to be a booking with reference NoSlot", bookingWithoutSlot);
				Assert("Expected NoSlot booking to have no SlotStartTime", !bookingWithoutSlot.GBM_SlotStartTime.IsValid);
				Assert("Expected NoSlot booking to have no SlotEndTime", !bookingWithoutSlot.GBM_SlotEndTime.IsValid);

				AssertNotNull("Expected there to be a booking with reference SlotStartSet", bookingWithSlotStart);
				AssertEquals("Expected SlotStartSet booking to have correct SlotStartTime", bookingWithSlotStart.GBM_SlotStartTime.ToZDateTime(), startDate.Value);
				Assert("Expected SlotStartSet booking to have no SlotEndTime", !bookingWithSlotStart.GBM_SlotEndTime.IsValid);

				AssertNotNull("Expected there to be a booking with reference SlotEndSet", bookingWithSlotEnd);
				Assert("Expected SlotEndSet booking to have no SlotStartTime", !bookingWithSlotEnd.GBM_SlotStartTime.IsValid);
				AssertEquals("Expected SlotEndSet booking to have correct SlotEndTime", bookingWithSlotEnd.GBM_SlotEndTime.ToZDateTime(), endDate.Value);

				AssertNotNull("Expected there to be a booking with reference SlotFullySet", bookingWithFullSlot);
				AssertEquals("Expected SlotFullySet booking to have correct SlotStartTime", bookingWithFullSlot.GBM_SlotStartTime.ToZDateTime(), startDate.Value);
				AssertEquals("Expected SlotFullySet booking to have correct SlotEndTime", bookingWithFullSlot.GBM_SlotEndTime.ToZDateTime(), endDate.Value);
			});
		}

		public void TestExtraDateObjects_DoNotCauseErrors()
		{
			#region Setup UXML

			var shipment = SetupShipment();
			var startDate = Date.New(DateType.Start, ZBool.False, ZDateTime.Now);
			var endDate = Date.New(DateType.End, ZBool.False, ZDateTime.Now.AddHours(1));
			var extraDate1 = Date.New(DateType.Accepted, ZBool.False, new ZDateTime(2000, 01, 01));
			var extraDate2 = Date.New(DateType.Accepted, ZBool.False, new ZDateTime(2000, 02, 01));

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var subShipmentFullSlotBooked = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipmentFullSlotBooked.SetDateCollection(() => new List<Date>() { startDate, endDate, extraDate1, extraDate2 });
			subShipmentFullSlotBooked.BookingConfirmationReference = "DatesSet";
			subShipmentFullSlotBooked.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Pickup };

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipmentFullSlotBooked });

			#endregion

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var bookingWithDates = booking.GateMovementBookings.FirstOrDefault(booking => (booking).GBM_BookingReferenceNumber == "DatesSet");

			CombineAssertions(() =>
			{
				var gateMovementBooking = booking.GateMovementBookings[0];
				AssertEquals("Expect SlotStartTime to be correctly set", gateMovementBooking.GBM_SlotStartTime.ToZDateTime(), startDate.Value);
				AssertEquals("Expect SlotEndTime to be correctly set", gateMovementBooking.GBM_SlotEndTime.ToZDateTime(), endDate.Value);
			});
		}

		public void TestMultipleStartAndEndValues_MatchesLargestWindow()
		{
			#region Setup UXML

			var shipment = SetupShipment();

			var startDate = Date.New(DateType.Start, ZBool.False, ZDateTime.Now);
			var endDate = Date.New(DateType.End, ZBool.False, ZDateTime.Now.AddHours(1));
			var extraStartDate = Date.New(DateType.Accepted, ZBool.False, ZDateTime.Now.AddMinutes(5));
			var extraEndDate = Date.New(DateType.Accepted, ZBool.False, ZDateTime.Now.AddHours(1).AddMinutes(-5));

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>());

			var subShipmentFullSlotBooked = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipmentFullSlotBooked.SetDateCollection(() => new List<Date>() { startDate, endDate, extraStartDate, extraEndDate });
			subShipmentFullSlotBooked.BookingConfirmationReference = "DatesSet";
			subShipmentFullSlotBooked.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Pickup };

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipmentFullSlotBooked });

			#endregion

			var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var bookingWithDates = booking.GateMovementBookings.FirstOrDefault(booking => (booking).GBM_BookingReferenceNumber == "DatesSet");

			CombineAssertions(() =>
			{
				var gateMovementBooking = booking.GateMovementBookings[0];
				AssertEquals("Expect SlotStartTime to be earliest startDate", gateMovementBooking.GBM_SlotStartTime.ToZDateTime(), startDate.Value);
				AssertEquals("Expect SlotEndTime to be latest endDate", gateMovementBooking.GBM_SlotEndTime.ToZDateTime(), endDate.Value);
			});
		}

		#region Test Helpers

		UniversalShipment SetupShipment(string bookingReferenceNumber = "")
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			if (!bookingReferenceNumber.IsNullOrEmpty())
			{
				shipment.DataContext.AddDataTarget(DataContextType.GateBooking, bookingReferenceNumber);
			}
			else
			{
				shipment.DataContext.AddDataTarget(DataContextType.GateBooking, ZString.Empty);
			}

			var transportOrgAddress = new OrganizationAddress
			{
				AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress),
				OrganizationCode = "ABC"
			};

			var facilityOrgAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.LocalCartageYard),
				Address1 = "1 main st",
				City = "Sydney",
				Postcode = "2020",
				State = "NSW",
				Country = new Country() { Code = "AU" }
			};

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				transportOrgAddress,
				facilityOrgAddress
			});

			var registrationNumberType = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.ContainerChainCommunityCode };
			var registrationNumber = new RegistrationNumber() { Type = registrationNumberType, Value = "CC123" };
			shipment.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Value == nameof(DocAddressType.LocalCartageYard)).SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { registrationNumber });

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.BookingConfirmationReference = "GateMovementBooking1";
			subShipment.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Delivery };

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment });

			var vehicleMovement = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var vehicleRun = new VehicleRun();
			var vehicle = new Vehicle();
			var vehicleRegistration = new Registration();

			vehicleRegistration.Number = "TESTING123";
			vehicle.Registration = vehicleRegistration;
			vehicleRun.Vehicle = vehicle;
			vehicleMovement.VehicleRun = vehicleRun;

			shipment.SetPreCarriageShipmentCollection(() => new List<UniversalShipment> { vehicleMovement });

			var crew = new Crew();
			crew.FullName = "Thomas Jefferson";
			crew.LicenseNumber = "12345678";
			crew.CrewType = CrewType.Driver;

			shipment.VehicleRun = new VehicleRun();
			shipment.VehicleRun.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.VehicleRun.SetCrewCollection(() => new List<Crew> { crew });

			return shipment;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			transportCompany.OH_Code = "ABC";

			var facilityCompany = Factory.NewWithValidTestData<OrgHeader>();
			facilityCompany.OH_Code = "FAC";
			var orgCusCode = facilityCompany.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "CC123", string.Empty);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = facilityCompany.MainAddress.PK;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			warehouse.WW_IsActive = true;

			Factory.SaveForTesting();
		}

		#endregion Test Helpers
	}
}
