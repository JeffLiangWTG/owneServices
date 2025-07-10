using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.GateManagement.Integration.Interfaces;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants.GateManagementConstants;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	class GateManagementFacilityJobFactoryTest : TestCaseWithFactoryAndMessagingHelpers
	{
		WhsWarehouse warehouse;
		FacilityJobForTest containerYardFacilityJob1;
		FacilityJobForTest containerYardFacilityJob2;
		FacilityJobForTest transitWarehouseReceiveFacilityJob;
		FacilityJobForTest transitWarehouseDispatchFacilityJob;

		Mock<IGateManagementFacilityDataContextManager> containerYardDataContextManagerMock;
		Mock<IGateManagementFacilityDataContextManager> transitWarehouseDataContextManagerMock;
		KeyObjectHandleDictionaryObject mockGateManagementFacilityManagerList;

		List<IDisposable> mockIntegrationRegistryValues;

		protected override void SetUp()
		{
			base.SetUp();

			containerYardFacilityJob1 = new FacilityJobForTest(FacilityJobTablePrefix.CYDPickup);
			containerYardFacilityJob2 = new FacilityJobForTest(FacilityJobTablePrefix.CYDPickup);
			transitWarehouseReceiveFacilityJob = new FacilityJobForTest(FacilityJobTablePrefix.WhsItemReceiveTransportationUnit);
			transitWarehouseDispatchFacilityJob = new FacilityJobForTest(FacilityJobTablePrefix.WhsItemDispatchTransportationUnit);

			containerYardDataContextManagerMock = new Mock<IGateManagementFacilityDataContextManager>();
			containerYardDataContextManagerMock.Setup(m => m.GetLinkedEntity(It.IsAny<UniversalObjectFactory>(), It.IsAny<IGteGateMovementBooking>()))
				.Returns((UniversalObjectFactory _, IGteGateMovementBooking booking) =>
				{
					switch (booking.GBM_BookingReferenceNumber)
					{
						case "GBM_001":
							return containerYardFacilityJob1;
						case "GBM_002":
							return containerYardFacilityJob2;
						default:
							return null;
					}
				}
			);

			transitWarehouseDataContextManagerMock = new Mock<IGateManagementFacilityDataContextManager>();
			transitWarehouseDataContextManagerMock.Setup(m => m.GetLinkedEntity(It.IsAny<UniversalObjectFactory>(), It.IsAny<IGteGateMovementBooking>()))
				.Returns((UniversalObjectFactory _, IGteGateMovementBooking booking) =>
				{
					switch (booking.GBM_BookingReferenceNumber)
					{
						case "GBM_001":
							return transitWarehouseReceiveFacilityJob;
						case "GBM_002":
							return transitWarehouseDispatchFacilityJob;
						default:
							return null;
					}
				}
			);

			mockGateManagementFacilityManagerList = new KeyObjectHandleDictionaryObject
			{
				{ nameof(RecipientRoleType.CYD), new TestObjectHandle(containerYardDataContextManagerMock.Object) },
				{ nameof(ServiceCodeType.TWR), new TestObjectHandle(transitWarehouseDataContextManagerMock.Object) },
				{ nameof(ServiceCodeType.TWD), new TestObjectHandle(transitWarehouseDataContextManagerMock.Object) },
			};

			mockIntegrationRegistryValues = new List<IDisposable>
			{
				WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndContainerYard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false),
				WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndTransitWarehouse.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false)
			};

			warehouse = CreateNewWarehouse();
			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			transportCompany.OH_Code = "ABC";

			SetupFacility();

			Factory.SaveForTesting();
		}

		protected override void TearDown()
		{
			base.TearDown();
			mockIntegrationRegistryValues.ForEach(x => x.Dispose());
		}

		public void TestGivenBookingUXML_WhenRecipientRoleHasFacilityCode_ThenLinkToCYDFacilityJobs()
		{
			var shipment = GetNewShipmentForTest("", "ABC", warehouse.WarehouseAddress.Header.OH_Code, isContainerYard: true);

			var subShipment1 = CreateSubShipmentForTest(
				bookingReference: "GBM_001",
				facilityTypeCode: "CYD",
				unitNumber: "CONT1",
				containerCode: "20GP",
				packageCode: Core.Constants.PkgUnit.Package,
				sourceReferenceNumber: "GBM_001");

			var subShipment2 = CreateSubShipmentForTest(
				bookingReference: "GBM_002",
				facilityTypeCode: "CYD",
				unitNumber: "CONT2",
				containerCode: "20GP",
				packageCode: Core.Constants.PkgUnit.Package,
				sourceReferenceNumber: "GBM_002");

			var subShipment3 = CreateSubShipmentForTest(
				bookingReference: "GBM_003",
				facilityTypeCode: "CYD",
				unitNumber: "CONT3",
				containerCode: "20GP",
				packageCode: Core.Constants.PkgUnit.Package,
				sourceReferenceNumber: "GBM_003");

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment1, subShipment2, subShipment3 });

			shipment.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CYD } };

			var logger = new TestErrorLogger();
			logger.TopLevelDataObject = shipment;

			using (ObjectFactory.Substitute("GateManagementFacilityManagerList", mockGateManagementFacilityManagerList))
			{
				var originalDataTargets = shipment.DataContext.DataTargetCollection.ToArray();
				var booking = new GteBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertContains("Unable to match booking 'GBM_003' to valid facility job", logger.Logs);

				var bizoFactory = new BusinessObjectFactory();

				CombineAssertions("quantity of entities created should be correct", () =>
				{
					AssertEquals("Bookings.Count", 1, bizoFactory.Load<GteBooking>(new ZQuery()).Length);
					AssertEquals("GateMovementBookings.Count", 3, booking.GateMovementBookings.Count);
				});

				var collection = bizoFactory.Load<IWhsItemReceiveConsignment>(new ZQuery());

				var gateMovementBooking1 = bizoFactory.LoadTop1<GteGateMovementBooking>(new ZQuery(GteGateMovementBookingSchema.GBM_BookingReferenceNumber, "GBM_001"));
				var gateMovementBooking2 = bizoFactory.LoadTop1<GteGateMovementBooking>(new ZQuery(GteGateMovementBookingSchema.GBM_BookingReferenceNumber, "GBM_002"));
				var gateMovementBooking3 = bizoFactory.LoadTop1<GteGateMovementBooking>(new ZQuery(GteGateMovementBookingSchema.GBM_BookingReferenceNumber, "GBM_003"));

				CombineAssertions("GateMovementBookings should be linked to correct Facility Job", () =>
				{
					AssertNotNull(nameof(gateMovementBooking1), gateMovementBooking1);
					AssertNotNull(nameof(gateMovementBooking2), gateMovementBooking2);
					AssertEquals("gateMovementBooking1.FacilityJobId should match", containerYardFacilityJob1.PK, gateMovementBooking1.GBM_FacilityJobId);
					AssertEquals("gateMovementBooking1.FacilityTableCode should match", containerYardFacilityJob1.TablePrefix, gateMovementBooking1.GBM_FacilityTableCode);
					AssertEquals("gateMovementBooking2.FacilityJobId should match", containerYardFacilityJob2.PK, gateMovementBooking2.GBM_FacilityJobId);
					AssertEquals("gateMovementBooking2.FacilityTableCode should match", containerYardFacilityJob2.TablePrefix, gateMovementBooking2.GBM_FacilityTableCode);
				});

				CombineAssertions("Unmatched GateMovementBookings should contain empty fields", () =>
				{
					AssertNotNull(nameof(gateMovementBooking3), gateMovementBooking3);
					AssertEquals(ZGuid.Empty, gateMovementBooking3.GBM_FacilityJobId);
					AssertNullOrEmptyOrWhitespace(gateMovementBooking3.GBM_FacilityTableCode);
				});

				AssertArrayEqualsByElements("Added data targets do not persist in data object", originalDataTargets, shipment.DataContext.DataTargetCollection.ToArray());
				containerYardDataContextManagerMock.Verify(x => x.UseIncomingShipmentData(It.IsAny<ITopLevelDataObject>(), It.IsAny<IXmlImportLogger>(), It.IsAny<IUniversalObjectFactory>()), Times.Once, "Hook into FacilityDataContextManager should be called");
			}
		}

		public void TestGivenBookingUXML_WhenRecipientRoleIsATW_ThenLinkWithTRWFacilityJobs()
		{
			var shipment = GetNewShipmentForTest("", "ABC", warehouse.WarehouseAddress.Header.OH_Code, isTransitWarehouse: true);

			var subShipment1 = CreateSubShipmentForTest(
				bookingReference: "GBM_001",
				facilityTypeCode: "TWH",
				unitNumber: "LL1",
				containerCode: "20GP",
				packageCode: Core.Constants.PkgUnit.Package,
				sourceReferenceNumber: "GBM_001");

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment1 });
			shipment.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } };

			var logger = new TestErrorLogger();
			logger.TopLevelDataObject = shipment;

			using (ObjectFactory.Substitute("GateManagementFacilityManagerList", mockGateManagementFacilityManagerList))
			{
				var originalDataTargets = shipment.DataContext.DataTargetCollection.ToArray();
				var booking = new GteBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var bizoFactory = new BusinessObjectFactory();

				CombineAssertions("quantity of entities created should be correct", () =>
				{
					AssertEquals("Bookings.Count", 1, bizoFactory.Load<GteBooking>(new ZQuery()).Length);
					AssertEquals("GateMovementBookings.Count", 1, booking.GateMovementBookings.Count);
				});

				var gateMovementBooking = bizoFactory.LoadTop1<GteGateMovementBooking>(new ZQuery(GteGateMovementBookingSchema.GBM_BookingReferenceNumber, "GBM_001"));

				CombineAssertions("GateMovementBookings should be linked to correct Facility Job", () =>
				{
					AssertNotNull(nameof(gateMovementBooking), gateMovementBooking);
					AssertEquals("gateMovementBooking1.FacilityJobId should match", transitWarehouseReceiveFacilityJob.PK, gateMovementBooking.GBM_FacilityJobId);
					AssertEquals("gateMovementBooking1.FacilityTableCode should match", transitWarehouseReceiveFacilityJob.TablePrefix, gateMovementBooking.GBM_FacilityTableCode);
				});

				transitWarehouseDataContextManagerMock.Verify(x => x.UseIncomingShipmentData(It.IsAny<ITopLevelDataObject>(), It.IsAny<IXmlImportLogger>(), It.IsAny<IUniversalObjectFactory>()), Times.Once, "Hook into FacilityDataContextManager should be called");
			}
		}

		public void TestGivenBookingUXML_WhenRecipientRoleIsDTW_ThenCreateFacilityJobs()
		{
			var shipment = GetNewShipmentForTest("", "ABC", warehouse.WarehouseAddress.Header.OH_Code, isTransitWarehouse: true);

			var subShipment1 = CreateSubShipmentForTest(
				bookingReference: "GBM_002",
				facilityTypeCode: "TWH",
				unitNumber: "LL3",
				containerCode: "20GP",
				packageCode: Core.Constants.PkgUnit.Package,
				sourceReferenceNumber: "GBM_002");

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment1 });
			shipment.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } };

			var logger = new TestErrorLogger();
			logger.TopLevelDataObject = shipment;

			using (ObjectFactory.Substitute("GateManagementFacilityManagerList", mockGateManagementFacilityManagerList))
			{
				var originalDataTargets = shipment.DataContext.DataTargetCollection.ToArray();
				var booking = new GteBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var bizoFactory = new BusinessObjectFactory();

				CombineAssertions("quantity of entities created should be correct", () =>
				{
					AssertEquals("Bookings.Count", 1, bizoFactory.Load<GteBooking>(new ZQuery()).Length);
					AssertEquals("GateMovementBookings.Count", 1, booking.GateMovementBookings.Count);
				});

				var gateMovementBooking = bizoFactory.LoadTop1<GteGateMovementBooking>(new ZQuery(GteGateMovementBookingSchema.GBM_BookingReferenceNumber, "GBM_002"));

				CombineAssertions("GateMovementBookings should be linked to correct Facility Job", () =>
				{
					AssertNotNull(nameof(gateMovementBooking), gateMovementBooking);
					AssertEquals("gateMovementBooking1.FacilityJobId should match", transitWarehouseDispatchFacilityJob.PK, gateMovementBooking.GBM_FacilityJobId);
					AssertEquals("gateMovementBooking1.FacilityTableCode should match", transitWarehouseDispatchFacilityJob.TablePrefix, gateMovementBooking.GBM_FacilityTableCode);
				});

				transitWarehouseDataContextManagerMock.Verify(x => x.UseIncomingShipmentData(It.IsAny<ITopLevelDataObject>(), It.IsAny<IXmlImportLogger>(), It.IsAny<IUniversalObjectFactory>()), Times.Once, "Hook into FacilityDataContextManager should be called");
			}
		}

		public void TestWhenNoFacilityCodeOrMatchingFacilityContextManagerInUXML_ShowInfoLogs()
		{
			var shipment = GetNewShipmentForTest("", "ABC", warehouse.WarehouseAddress.Header.OH_Code, isContainerYard: true);

			var subShipment1 = CreateSubShipmentForTest(
				bookingReference: "GBM_001",
				facilityTypeCode: "CYD",
				unitNumber: "CONT1234561",
				containerCode: "20GP",
				packageCode: Core.Constants.PkgUnit.Package,
				sourceReferenceNumber: "GBM_001");

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment1 });
			shipment.DataContext.RecipientRoleCollection = new List<RecipientRole>() { };

			var logger = new TestErrorLogger();
			logger.TopLevelDataObject = shipment;

			var bookingObjectReader = new GteBookingDataObjectReader(shipment, logger, Factory);
			bookingObjectReader.ReadIntoBusinessObject();

			Factory.SaveForTesting();

			AssertContains("Information - Imported UXML does not contain a Facility Type", logger.Logs);

			using (ObjectFactory.Substitute("GateManagementFacilityManagerList", new KeyObjectHandleDictionaryObject()))
			{
				shipment.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CYD } };
				bookingObjectReader.ReadIntoBusinessObject();
				Factory.SaveForTesting();

				AssertContains("Information - No Facility Job Manager defined for Facility type 'CYD'", logger.Logs);
			}
		}

		public void TestGivenUniversalIntegrationEnabledForContainerYard_WhenReadBookingUXML_ThenContainerYardJobLinkerNotInvoked()
		{
			mockIntegrationRegistryValues.ForEach(x => x.Dispose());
			mockIntegrationRegistryValues = new List<IDisposable>()
			{
				WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndContainerYard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true),
				WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndTransitWarehouse.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false)
			};
			var shipment = GetNewShipmentForTest("", "ABC", warehouse.WarehouseAddress.Header.OH_Code, isContainerYard: true);

			var subShipment1 = CreateSubShipmentForTest(
				bookingReference: "GBM_001",
				facilityTypeCode: "CYD",
				unitNumber: "LL1",
				containerCode: "20GP",
				packageCode: Core.Constants.PkgUnit.Package,
				sourceReferenceNumber: "GBM_001");

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment1 });
			shipment.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CYD } };

			var logger = new TestErrorLogger();
			logger.TopLevelDataObject = shipment;

			using (ObjectFactory.Substitute("GateManagementFacilityManagerList", mockGateManagementFacilityManagerList))
			{
				var originalDataTargets = shipment.DataContext.DataTargetCollection.ToArray();
				var booking = new GteBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var bizoFactory = new BusinessObjectFactory();

				CombineAssertions("quantity of entities created should be correct", () =>
				{
					AssertEquals("Bookings.Count", 1, bizoFactory.Load<GteBooking>(new ZQuery()).Length);
					AssertEquals("GateMovementBookings.Count", 1, booking.GateMovementBookings.Count);
				});

				var gateMovementBooking = bizoFactory.LoadTop1<GteGateMovementBooking>(new ZQuery(GteGateMovementBookingSchema.GBM_BookingReferenceNumber, "GBM_001"));

				CombineAssertions("GateMovementBookings should not be linked to a Facility Job", () =>
				{
					AssertNotNull(nameof(gateMovementBooking), gateMovementBooking);
					AssertEquals("gateMovementBooking1.FacilityJobId should empty", Guid.Empty, gateMovementBooking.GBM_FacilityJobId);
					AssertEquals("gateMovementBooking1.FacilityTableCode should be blank", string.Empty, gateMovementBooking.GBM_FacilityTableCode);
				});

				CombineAssertions("Logs should reflect that linking was not performed", () =>
				{
					AssertNotContains("Information - Try link matching facility jobs", logger.Logs);
					AssertContains("Information - Linking of jobs skipped due to registry settings (Warehouse > Gate Management > Enable universal integration between gate and container yard)", logger.Logs);
				});
			}
		}

		public void TestGivenUniversalIntegrationEnabledForTransitWarehouse_WhenReadBookingUXML_ThenTransitWarehouseJobLinkerNotInvoked()
		{
			mockIntegrationRegistryValues.ForEach(x => x.Dispose());
			mockIntegrationRegistryValues = new List<IDisposable>()
			{
				WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndContainerYard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false),
				WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndTransitWarehouse.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)
			};
			var shipment = GetNewShipmentForTest("", "ABC", warehouse.WarehouseAddress.Header.OH_Code, isTransitWarehouse: true);

			var subShipment1 = CreateSubShipmentForTest(
				bookingReference: "GBM_001",
				facilityTypeCode: "TWH",
				unitNumber: "LL1",
				containerCode: "20GP",
				packageCode: Core.Constants.PkgUnit.Package,
				sourceReferenceNumber: "GBM_001");

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment1 });
			shipment.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } };

			var logger = new TestErrorLogger();
			logger.TopLevelDataObject = shipment;

			using (ObjectFactory.Substitute("GateManagementFacilityManagerList", mockGateManagementFacilityManagerList))
			{
				var originalDataTargets = shipment.DataContext.DataTargetCollection.ToArray();
				var booking = new GteBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var bizoFactory = new BusinessObjectFactory();

				CombineAssertions("quantity of entities created should be correct", () =>
				{
					AssertEquals("Bookings.Count", 1, bizoFactory.Load<GteBooking>(new ZQuery()).Length);
					AssertEquals("GateMovementBookings.Count", 1, booking.GateMovementBookings.Count);
				});

				var gateMovementBooking = bizoFactory.LoadTop1<GteGateMovementBooking>(new ZQuery(GteGateMovementBookingSchema.GBM_BookingReferenceNumber, "GBM_001"));

				CombineAssertions("GateMovementBookings should not be linked to a Facility Job", () =>
				{
					AssertNotNull(nameof(gateMovementBooking), gateMovementBooking);
					AssertEquals("gateMovementBooking1.FacilityJobId should empty", Guid.Empty, gateMovementBooking.GBM_FacilityJobId);
					AssertEquals("gateMovementBooking1.FacilityTableCode should be blank", string.Empty, gateMovementBooking.GBM_FacilityTableCode);
				});

				CombineAssertions("Logs should reflect that linking was not performed", () =>
				{
					AssertNotContains("Information - Try link matching facility jobs", logger.Logs);
					AssertContains("Information - Linking of jobs skipped due to registry settings (Warehouse > Gate Management > Enable universal integration between gate and transit warehouse)", logger.Logs);
				});
			}
		}

		public void TestGivenUniversalIntegrationEnabledForBothTransitWarehouseAndContainerYard_WhenReadBookingUXML_ThenNoJobLinkerNotInvoked()
		{
			mockIntegrationRegistryValues.ForEach(x => x.Dispose());
			mockIntegrationRegistryValues = new List<IDisposable>()
			{
				WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndContainerYard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true),
				WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndTransitWarehouse.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)
			};
			var shipment = GetNewShipmentForTest("", "ABC", warehouse.WarehouseAddress.Header.OH_Code, isTransitWarehouse: true);

			var subShipment1 = CreateSubShipmentForTest(
				bookingReference: "GBM_001",
				facilityTypeCode: "TWH",
				unitNumber: "LL1",
				containerCode: "20GP",
				packageCode: Core.Constants.PkgUnit.Package,
				sourceReferenceNumber: "GBM_001");

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment1 });
			shipment.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } };

			var logger = new TestErrorLogger();
			logger.TopLevelDataObject = shipment;

			using (ObjectFactory.Substitute("GateManagementFacilityManagerList", mockGateManagementFacilityManagerList))
			{
				var originalDataTargets = shipment.DataContext.DataTargetCollection.ToArray();
				var booking = new GteBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var bizoFactory = new BusinessObjectFactory();

				CombineAssertions("quantity of entities created should be correct", () =>
				{
					AssertEquals("Bookings.Count", 1, bizoFactory.Load<GteBooking>(new ZQuery()).Length);
					AssertEquals("GateMovementBookings.Count", 1, booking.GateMovementBookings.Count);
				});

				var gateMovementBooking = bizoFactory.LoadTop1<GteGateMovementBooking>(new ZQuery(GteGateMovementBookingSchema.GBM_BookingReferenceNumber, "GBM_001"));

				CombineAssertions("Logs should reflect that linking was not performed", () =>
				{
					AssertNotContains("Information - Try link matching facility jobs", logger.Logs);
					AssertContains("Information - Linking of jobs skipped due to registry settings (Warehouse > Gate Management > Enable universal integration between gate and transit warehouse)", logger.Logs);
				});
			}
		}

		public void TestGivenContainerYardUniversalIntegrationDisabled_WhenReadBookingUXML_ThenContainerYardJobLinkerInvoked()
		{
			mockIntegrationRegistryValues.ForEach(x => x.Dispose());
			mockIntegrationRegistryValues = new List<IDisposable>()
			{
				WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndContainerYard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false),
				WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndTransitWarehouse.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true)
			};
			var shipment = GetNewShipmentForTest("", "ABC", warehouse.WarehouseAddress.Header.OH_Code , isContainerYard: true);

			var subShipment1 = CreateSubShipmentForTest(
				bookingReference: "GBM_001",
				facilityTypeCode: "CYD",
				unitNumber: "LL1",
				containerCode: "20GP",
				packageCode: Core.Constants.PkgUnit.Package,
				sourceReferenceNumber: "GBM_001");

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment1 });
			shipment.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.CYD } };

			var logger = new TestErrorLogger();
			logger.TopLevelDataObject = shipment;

			using (ObjectFactory.Substitute("GateManagementFacilityManagerList", mockGateManagementFacilityManagerList))
			{
				var originalDataTargets = shipment.DataContext.DataTargetCollection.ToArray();
				var booking = new GteBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var bizoFactory = new BusinessObjectFactory();

				CombineAssertions("quantity of entities created should be correct", () =>
				{
					AssertEquals("Bookings.Count", 1, bizoFactory.Load<GteBooking>(new ZQuery()).Length);
					AssertEquals("GateMovementBookings.Count", 1, booking.GateMovementBookings.Count);
				});

				var gateMovementBooking = bizoFactory.LoadTop1<GteGateMovementBooking>(new ZQuery(GteGateMovementBookingSchema.GBM_BookingReferenceNumber, "GBM_001"));

				CombineAssertions("GateMovementBookings should be linked to a Facility Job", () =>
				{
					AssertNotNull(nameof(gateMovementBooking), gateMovementBooking);
					AssertEquals("FacilityJobId should be set", containerYardFacilityJob1.PK, gateMovementBooking.GBM_FacilityJobId);
					AssertEquals("FacilityTableCode should be set", containerYardFacilityJob1.TablePrefix, gateMovementBooking.GBM_FacilityTableCode.ToString());
				});

				containerYardDataContextManagerMock.Verify(
					x => x.UseIncomingShipmentData(It.IsAny<ITopLevelDataObject>(), It.IsAny<IXmlImportLogger>(), It.IsAny<IUniversalObjectFactory>()),
					Times.Once,
					"Expected the linker to be invoked"
				);

				AssertContains("Try link matching facility jobs", logger.Logs);
				AssertNotContains("Linking of jobs skipped due to registry settings", logger.Logs);
			}
		}

		public void TestGivenTransitWarehouseUniversalIntegrationDisabled_WhenReadBookingUXML_ThenTransitWarehouseJobLinkerInvoked()
		{
			mockIntegrationRegistryValues.ForEach(x => x.Dispose());
			mockIntegrationRegistryValues = new List<IDisposable>()
			{
				WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndContainerYard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true),
				WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndTransitWarehouse.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false)
			};
			var shipment = GetNewShipmentForTest("", "ABC", warehouse.WarehouseAddress.Header.OH_Code, isTransitWarehouse: true);

			var subShipment1 = CreateSubShipmentForTest(
				bookingReference: "GBM_001",
				facilityTypeCode: "TWH",
				unitNumber: "LL1",
				containerCode: "20GP",
				packageCode: Core.Constants.PkgUnit.Package,
				sourceReferenceNumber: "GBM_001");

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment1 });
			shipment.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.ATW, ServiceCode = ServiceCodeType.TWR } };

			var logger = new TestErrorLogger();
			logger.TopLevelDataObject = shipment;

			using (ObjectFactory.Substitute("GateManagementFacilityManagerList", mockGateManagementFacilityManagerList))
			{
				var originalDataTargets = shipment.DataContext.DataTargetCollection.ToArray();
				var booking = new GteBookingDataObjectReader(shipment, logger, Factory).ReadIntoBusinessObject();
				Factory.SaveForTesting();

				var bizoFactory = new BusinessObjectFactory();

				CombineAssertions("quantity of entities created should be correct", () =>
				{
					AssertEquals("Bookings.Count", 1, bizoFactory.Load<GteBooking>(new ZQuery()).Length);
					AssertEquals("GateMovementBookings.Count", 1, booking.GateMovementBookings.Count);
				});

				var gateMovementBooking = bizoFactory.LoadTop1<GteGateMovementBooking>(new ZQuery(GteGateMovementBookingSchema.GBM_BookingReferenceNumber, "GBM_001"));

				CombineAssertions("GateMovementBookings should be linked to correct Facility Job", () =>
				{
					AssertNotNull(nameof(gateMovementBooking), gateMovementBooking);
					AssertEquals("gateMovementBooking1.FacilityJobId should match", transitWarehouseReceiveFacilityJob.PK, gateMovementBooking.GBM_FacilityJobId);
					AssertEquals("gateMovementBooking1.FacilityTableCode should match", transitWarehouseReceiveFacilityJob.TablePrefix, gateMovementBooking.GBM_FacilityTableCode);
				});

				transitWarehouseDataContextManagerMock.Verify(x => x.UseIncomingShipmentData(It.IsAny<ITopLevelDataObject>(), It.IsAny<IXmlImportLogger>(), It.IsAny<IUniversalObjectFactory>()), Times.Once, "Hook into FacilityDataContextManager should be called");
			}
		}

		public void TestGivenBookingPartySourceReferenceNumberInAdditionalReference_ThenAddItAsADataSource()
		{
			var shipment = GetNewShipmentForTest("", "ABC", warehouse.WarehouseAddress.Header.OH_Code, isContainerYard: true);

			shipment.DataContext.RecipientRoleCollection = new List<RecipientRole>() { new RecipientRole { Code = RecipientRoleType.DTW, ServiceCode = ServiceCodeType.TWD } };
			shipment.SubShipmentCollection.FirstOrDefault()?.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()
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

			using (ObjectFactory.Substitute("GateManagementFacilityManagerList", mockGateManagementFacilityManagerList))
			{
				var booking = new GteBookingDataObjectReader(shipment, new DummyLogger(), Factory).ReadIntoBusinessObject();

				Factory.SaveForTesting();

				var gateMovementBooking = shipment.SubShipmentCollection[0];
				var gateMovementBookingDataSource = gateMovementBooking.DataContext.DataSourceCollection.FirstOrDefault(s => s.Type.Equals(nameof(DataContextType.GateMovementBooking)));
				AssertNotNull("Expected there to be a GateMovementBooking data source in the subshipment", gateMovementBookingDataSource);
				AssertEquals("Expected VBS00000001 to be the GateMovementBooking data source of the subshipment", gateMovementBookingDataSource.Key, "VBS00000001");

				var gateBookingDataSource = gateMovementBooking.DataContext.DataSourceCollection.FirstOrDefault(s => s.Type.Equals(nameof(DataContextType.GateBooking)));
				AssertNotNull("Expected there to be a GateBooking data source in the subshipment", gateBookingDataSource);
				AssertEquals("Expected VBS00000001 to be the GateBooking data source of the subshipment", gateBookingDataSource.Key, "VBS00000001");
			}
		}

		#region Implementation

		UniversalShipment GetNewShipmentForTest(string bookingReferenceNumber, string orgCode, string warehouseOrgCode, bool isContainerYard = false, bool isTransitWarehouse = false)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.GateBooking, bookingReferenceNumber);
			shipment.AddOrgAddress(new OrganizationAddress { AddressType = nameof(DocAddressType.TransportCompanyDocumentaryAddress), OrganizationCode = orgCode });

			if (isTransitWarehouse)
			{
				shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.ATW } } });
				shipment.AddOrgAddress(new OrganizationAddress { AddressType = nameof(DocAddressType.ArrivalCFSAddress), OrganizationCode = warehouseOrgCode });
				shipment.AddOrgAddress(new OrganizationAddress { AddressType = nameof(DocAddressType.DepartureCFSAddress), OrganizationCode = warehouseOrgCode });
			}

			else if (isContainerYard)
			{
				shipment.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail { Type = RecipientRoleType.CYD } } });
				shipment.AddOrgAddress(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) { AddressType = nameof(DocAddressType.LocalCartageYard), OrganizationCode = "XYZ" });

				var registrationNumberType = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.ContainerChainCommunityCode };
				var registrationNumber = new RegistrationNumber() { Type = registrationNumberType, Value = "CC123" };
				shipment.OrganizationAddressCollection.FirstOrDefault(a => a.AddressType.Value == nameof(DocAddressType.LocalCartageYard)).SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { registrationNumber });
			}

			shipment.SetPreCarriageShipmentCollection(() => new List<UniversalShipment>
			{
				new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					VehicleRun = new VehicleRun()
					{
						Vehicle = new Vehicle()
						{
							Registration = new Registration()
							{
								Number = "TEST123"
							}
						}
					}
				}
			});

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.BookingConfirmationReference = "GateMovementBooking1";
			subShipment.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Delivery };

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment });

			var crew = new Crew();
			crew.FullName = "Thomas Jefferson";
			crew.LicenseNumber = "12345678";
			crew.CrewType = CrewType.Driver;

			shipment.VehicleRun = new VehicleRun();
			shipment.VehicleRun.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.VehicleRun.SetCrewCollection(() => new List<Crew>
			{
				new Crew()
				{
					FullName = "Thomas Jefferson",
					LicenseNumber = "12345678",
					CrewType = CrewType.Driver
				}
			});

			return shipment;
		}
		UniversalShipment CreateSubShipmentForTest(string bookingReference, string facilityTypeCode, string unitNumber, string containerCode, string packageCode, string sourceReferenceNumber)
		{
			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.BookingConfirmationReference = bookingReference;
			subShipment.TransportBookingDirection = new TransportBookingDirection() { Code = TransportBookingDirections.Codes.Delivery };
			subShipment.FacilityJobType = new CodeDescriptionPair() { Code = facilityTypeCode };
			subShipment.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>
			{
				new AdditionalReference
				{
					Type = new EntryType { Code = AdditionalReferenceTypes.Codes.BookingPartyReference },
					ReferenceNumber = sourceReferenceNumber
				}
			});

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			packline.PackType = new PackageType() { Code = packageCode };
			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packline });

			var container = new Container();
			container.ContainerNumber = unitNumber;
			container.ContainerType = new ContainerType() { Code = containerCode };

			subShipment.SetContainerCollection(() => new DataObjectList<Container> { container });

			subShipment.DataContext = DataContextFactory.New();
			return subShipment;
		}

		WhsWarehouse CreateNewWarehouse()
		{
			var warehouseOrg = Factory.NewWithValidTestData<OrgHeader>();
			warehouseOrg.OH_Code = "WAREHOUSE";

			var warehouseAddress = warehouseOrg.MainAddress;
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory.BOFactory);

			var newBranch = Factory.BOFactory.New<GlbBranch>();
			newBranch.GB_GC = GlbBranch.CurrentBranch.GB_GC;

			var warehouse = (WhsWarehouse)helper.CreateWarehouse("Transit Warehouse 2", "TW2", "A");
			warehouse.WW_GB_RelatedCompanyBranch = newBranch.PK;
			warehouse.WW_OA_WarehouseAddress = warehouseAddress.PK;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			Factory.SaveForTesting();

			return warehouse;
		}

		void SetupFacility()
		{
			var facilityCompany = Factory.NewWithValidTestData<OrgHeader>();
			facilityCompany.OH_Code = "XYZ";
			facilityCompany.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ContainerChainCommunityCode, "CC123", string.Empty);

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = facilityCompany.MainAddress.PK;
			warehouse.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			warehouse.WW_IsActive = true;
		}

		sealed class FacilityJobForTest : NonPersistentBusinessObject, IObsoleteValidation
		{
			public FacilityJobForTest(string tablePrefix)
				: base()
			{
				this.tablePrefix = tablePrefix;
			}

			readonly string tablePrefix;
			public override string TablePrefix => tablePrefix;
		}

		#endregion Implementation
	}
}
