using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.Warehouse.Transit.Business.Testing;
using Moq;
using FinderType = Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateBusinessObjectFinderForASN.FinderType;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitPackageStateBusinessObjectFinderForASNTest : TransitUniversalTestCase
	{
		#region TestFindPackageStates

		public void TestFindPackageStates()
		{
			var warehouse = UniversalData.Warehouse;
			var rcn1 = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			var asn1 = Helper.CreateReceiveASN("ASN001", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN002", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);
			var packageState2 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);
			var packageState3 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-3", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn2);
			Factory.SaveForTesting();

			var packageStates = new WhsTransitPackageStateBusinessObjectFinderForASN(FinderType.ByASN, Factory, asn: asn1).Find().ToList();

			AssertEquals("count of packageStates is 2", 2, packageStates.Count);
		}

		public void TestFindPackageStatesByRCN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn1 = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			var asn1 = Helper.CreateReceiveASN("ASN001", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN002", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);
			var packageState2 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);
			var packageState3 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-3", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn2);
			Factory.SaveForTesting();

			var finder = new WhsTransitPackageStateBusinessObjectFinderForASN(FinderType.ByRCN, Factory, rcn: rcn1);
			var packageStates = finder.Find().ToList();

			AssertEquals("count of packageStates is 3", 3, packageStates.Count);
		}

		public void TestFindPackageStatesByPackages()
		{
			var warehouse = UniversalData.Warehouse;
			var rcn1 = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked);
			var packageState2 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked);
			var packageState3 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-3", TransitWarehouseStatuses.Codes.Booked);
			Factory.SaveForTesting();

			var finder = new WhsTransitPackageStateBusinessObjectFinderForASN(FinderType.Packages, Factory, packagePKs: [packageState1.WPS_KP_Package, packageState2.WPS_KP_Package, packageState3.WPS_KP_Package]);
			var packageStates = finder.Find().ToList();

			AssertEquals("count of packageStates is 3", 3, packageStates.Count);
		}

		#region TestFindPackageStatesByGate

		public void TestFindPackageStatesByGate()
		{
			var packageStates = FindPackageStatesByGateCore(false, true);
			AssertEquals("count of packageStates is 3", 3, packageStates.Length);
		}

		public void TestFindPackageStatesByGate_ThrowDataObjectReadFailureException()
		{
			AssertExceptionThrown<DataObjectReadFailureException>("Gate Booking has no container and failed to find a matching ASN without container.", () => FindPackageStatesByGateCore(false, false));
		}

		public void TestFindPackageStatesByGate_ByLatestASN()
		{
			var packageStates = FindPackageStatesByGateCore(true, true);
			AssertEquals("count of packageStates is 1", 1, packageStates.Length);
		}

		public WhsItemPackageState[] FindPackageStatesByGateCore(bool hasContainerNumber, bool hasContainerByASN)
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			if (hasContainerNumber)
			{
				var container = new Container { Link = 1, ContainerNumber = "CNT-1" };
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { container });
			}
			else
			{
				shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>());

				var subShipment = Data.CreateSubShipmentForGateBookingHeaderObject("GMB001", shipmentDataObject.WayBillNumber.Value, true);
				subShipment.SetContainerCollection(() => new DataObjectList<Container>());
				shipmentDataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>() { subShipment });
			}

			var warehouse = UniversalData.Warehouse;
			var rcn1 = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			var asn1 = Helper.CreateReceiveASN("ASN001", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN002", warehouse.PK);
			var asn3 = Helper.CreateReceiveASN("ASN003", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);
			var packageState2 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);
			var packageState3 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-3", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, location.PK);

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU002", warehouse.PK, location.PK);

			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn1.PK);
			Helper.CreateReceiveASNRTUPivot(rtu2.PK, asn2.PK);

			var containerDTOs1 = new List<ContainerDTO>
			{
				new ContainerDTO(rtu1, packageState1),
				new ContainerDTO(rtu1, packageState2)
			};
			var containerDTOs2 = new List<ContainerDTO>
			{
				new ContainerDTO(rtu2, packageState3)
			};

			var containerByASNDTOs = new List<ContainerByASNDTO>
			{
				new ContainerByASNDTO(asn1.PK, containerDTOs1),
				new ContainerByASNDTO(asn2.PK, containerDTOs2)
			};

			if (!hasContainerByASN)
			{
				rtu1.WRH_UnitType = TransportUnitTypes.ULD;
				rtu2.WRH_UnitType = TransportUnitTypes.ULD;
			}

			Factory.SaveForTesting();

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var managerInstance = manager.Object;
			Logger.TopLevelDataObject = shipmentDataObject;
			var consolHandler = managerInstance.BuildHandler(TransitDataObjectReaderHandlerManager.HandlerType.Consol, shipmentDataObject);
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);
			manager.Setup(m => m.AddAffectedASNPK(It.IsAny<ZGuid>()));

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForASN(shipmentDataObject, Factory, [packageState1, packageState2, packageState3], containerByASNDTOs, asn3, "MAB1", latestASNPK: asn2.PK);
				return finder.Find().ToArray();
			}
		}

		#endregion

		#region TestFindPackageStatesForLinkToVehicle

		public void TestFindPackageStatesForLinkToVehicle()
		{
			var packageStates = FindPackageStatesForLinkToVehicleCore(true);
			AssertEquals("count of packageStates is 2", 2, packageStates.Length);
		}

		public void TestFindPackageStatesForLinkToVehicle_ThrowDataObjectReadFailureException()
		{
			AssertExceptionThrown<DataObjectReadFailureException>("Gate Booking has a container and failed to find a matching ASN with the same container.", () => FindPackageStatesForLinkToVehicleCore(false));
		}

		public WhsItemPackageState[] FindPackageStatesForLinkToVehicleCore(bool isContainer)
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var container = new Container { Link = 1, ContainerNumber = "V1" };
			shipmentDataObject.SetContainerCollection(() => new DataObjectList<Container>() { container });

			var warehouse = UniversalData.Warehouse;
			var rcn1 = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			var asn1 = Helper.CreateReceiveASN("ASN001", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN002", warehouse.PK);
			var asn3 = Helper.CreateReceiveASN("ASN003", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);
			var packageState2 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);
			var packageState3 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-3", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn2);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, location.PK);

			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU002", warehouse.PK, location.PK);

			var containerDTOs = new List<ContainerDTO>
			{
				new ContainerDTO(rtu1, packageState1),
				new ContainerDTO(rtu2, packageState2)
			};

			var containerByASNDTOs = new List<ContainerByASNDTO>
			{
				new ContainerByASNDTO(asn1.PK, containerDTOs)
			};

			if (isContainer)
			{
				rtu1.WRH_UnitType = TransportUnitTypes.ULD;
				rtu2.WRH_UnitType = TransportUnitTypes.ULD;
			}

			Factory.SaveForTesting();

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var managerInstance = manager.Object;
			Logger.TopLevelDataObject = shipmentDataObject;
			var consolHandler = managerInstance.BuildHandler(TransitDataObjectReaderHandlerManager.HandlerType.Consol, shipmentDataObject);
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForASN(shipmentDataObject, Factory, containerByASNDTOs);
				return finder.Find().ToArray();
			}
		}

		#endregion

		#endregion

		#region TestThrowExceptionWhenParamIsNull

		public void TestThrowExceptionWhenParamIsNull()
		{
			var warehouse = UniversalData.Warehouse;
			var asn1 = Helper.CreateReceiveASN("ASN001", warehouse.PK);
			Factory.SaveForTesting();

			AssertExceptionThrown<ArgumentNullException>(() => new WhsTransitPackageStateBusinessObjectFinderForASN(FinderType.ByASN, null));
			AssertNoExceptionThrown(() => new WhsTransitPackageStateBusinessObjectFinderForASN(FinderType.ByASN, Factory, asn: asn1));
		}

		#endregion

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		TestDataForUniversal UniversalData => universalData ?? new TestDataForUniversal(Factory, Logger);
		readonly TestDataForUniversal universalData;
	}
}
