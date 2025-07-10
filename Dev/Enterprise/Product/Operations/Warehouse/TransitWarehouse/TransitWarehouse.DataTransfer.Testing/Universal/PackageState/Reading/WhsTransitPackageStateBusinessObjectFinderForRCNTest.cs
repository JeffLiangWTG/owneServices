using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsTransitPackageStateBusinessObjectFinderForRCNTest : TransitUniversalTestCase
	{
		#region TestFindPackageStatesByRCN

		public void TestFindPackageStatesByRCN()
		{
			var warehouse = UniversalData.Warehouse;
			var rcn1 = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RC002", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Booked);
			var packageState2 = Helper.CreatePackageState(rcn1, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Booked);
			var packageState3 = Helper.CreatePackageState(rcn2, 1, "PKG", "PKG-3", TransitWarehouseStatuses.Codes.Booked);
			Factory.SaveForTesting();

			var packageStates = new WhsTransitPackageStateBusinessObjectFinderForRCN(rcn1, Factory).Find().ToList();

			AssertEquals("count of packageStates is 2", 2, packageStates.Count);
		}

		#endregion

		#region TestFindPackageStatesByPackingLine

		public void TestFindPackageStatesByPackingLine_PackingLineIsNull()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var rcn = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			Factory.SaveForTesting();

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);
			manager.Setup(m => m.AddPackagesByContainerLink(It.IsAny<ZInt>(), It.IsAny<List<PkgPackage>>()));

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForRCN(rcn, Factory, Logger, shipmentDataObject, shipmentDataObject);
				var packageStateDTO = finder.Find(null);

				Assert(packageStateDTO != null);
				AssertPackageStateDTO(packageStateDTO, warehouse.PK, rcnPK: rcn.PK, parentPackagePK: null, dcnPK: Guid.Empty, isNew: true, unitType: PackageStateUnitType.Codes.PackLine, status: TransitWarehouseStatuses.Codes.Booked);
				shipmentDataObject.SetPackingLineCollection(() => null);
				var finderForNull = new WhsTransitPackageStateBusinessObjectFinderForRCN(rcn, Factory, Logger, shipmentDataObject, shipmentDataObject);
				var packageStateDTO2 = finder.Find(null);
				Assert(packageStateDTO2 == null);
			}
		}

		public void TestFindPackageStatesByPackingLine_IsHighRisk()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var rcn = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			var packingLine1 = Helper.CreatePackingLine("packingLine1", null, Constants.PkgUnit.Pallet, 150m, 5m, 5m, 5m);
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1 });

			Factory.SaveForTesting();

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);
			manager.Setup(m => m.AddPackagesByContainerLink(It.IsAny<ZInt>(), It.IsAny<List<PkgPackage>>()));

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForRCN(rcn, Factory, Logger, shipmentDataObject, shipmentDataObject);
				var packageStateDTO = finder.Find(new PackingLine() { IsHighRisk = true });

				Assert(packageStateDTO != null);
				AssertPackageStateDTO(packageStateDTO, warehouse.PK, rcnPK: rcn.PK, parentPackagePK: null, isNew: true, unitType: PackageStateUnitType.Codes.Package, status: TransitWarehouseStatuses.Codes.Booked, isHighRisk: true);
			}
		}

		public void TestFindPackageStatesByPackingLine_IsNew()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var rcn = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			var packingLine1 = Helper.CreatePackingLine(null, null, Constants.PkgUnit.Pallet, 150m, 5m, 5m, 5m);
			var innerPackingLine = Helper.CreatePackingLine("inner", null, Constants.PkgUnit.Box, 150m, 1m, 1m, 1m);
			packingLine1.SetPackingLineCollection(() => new List<PackingLine> { innerPackingLine });
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1 });

			Factory.SaveForTesting();

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);
			manager.Setup(m => m.AddPackagesByContainerLink(It.IsAny<ZInt>(), It.IsAny<List<PkgPackage>>()));

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForRCN(rcn, Factory, Logger, shipmentDataObject, shipmentDataObject);
				var packageStateDTO = finder.Find(new PackingLine() { Link = 1 });

				Assert(packageStateDTO != null);
				AssertPackageStateDTO(packageStateDTO, warehouse.PK, rcnPK: rcn.PK, parentPackagePK: null, dcnPK: Guid.Empty, isNew: true, unitType: PackageStateUnitType.Codes.Package, status: TransitWarehouseStatuses.Codes.Booked);

				var innerDTO = finder.Find(new PackingLine() { ReferenceNumber = "inner" });
				Assert(innerDTO != null);
				AssertPackageStateDTO(innerDTO, warehouse.PK, rcnPK: rcn.PK, parentPackagePK: packageStateDTO.WPS_KP_Package, dcnPK: Guid.Empty, isNew: false, unitType: null, status: TransitWarehouseStatuses.Codes.Booked);
			}
		}

		public void TestFindPackageStatesByPackingLine_DispatchConsignmentPK()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			var warehouse = UniversalData.Warehouse;
			var rcn = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU001", warehouse.PK, location.PK);

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn1);

			var packingLine1 = Helper.CreatePackingLine(null, null, Constants.PkgUnit.Pallet, 150m, 5m, 5m, 5m);
			shipmentDataObject.PackingLineCollection.Clear();
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1 });

			Factory.SaveForTesting();

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);
			manager.Setup(m => m.AddPackagesByContainerLink(It.IsAny<ZInt>(), It.IsAny<List<PkgPackage>>()));

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForRCN(rcn, Factory, Logger, shipmentDataObject, shipmentDataObject);
				var packageStateDTO = finder.Find(new PackingLine() { Link = 1 });

				Assert(packageStateDTO != null);
				AssertPackageStateDTO(packageStateDTO, warehouse.PK, rcnPK: rcn.PK, parentPackagePK: null, dcnPK: dcn1.PK, isNew: true, unitType: PackageStateUnitType.Codes.Package, status: TransitWarehouseStatuses.Codes.Booked);
			}
		}

		public void TestFindPackageStatesByPackingLine()
		{
			var shipmentDataObject = UniversalData.ShipmentDataObject;
			shipmentDataObject.PackingLineCollection.Clear();
			var warehouse = UniversalData.Warehouse;
			var rcn = Helper.CreateReceiveConsignment("RC001", warehouse.PK);

			var packingLine1 = Helper.CreatePackingLine("packingLine1", null, Constants.PkgUnit.Pallet, 150m, 5m, 5m, 5m);
			var packingLine2 = Helper.CreatePackingLine("packingLine2", null, Constants.PkgUnit.Box, 150m, 1m, 1m, 1m);
			var packingLine3 = Helper.CreatePackingLine(null, null, Constants.PkgUnit.Tube, 1, 1, 1, 1);
			var packingLine4 = Helper.CreatePackingLine(null, null, Constants.PkgUnit.Crate, 2, 2, 2, 2);
			packingLine1.SetPackingLineCollection(() => new List<PackingLine> { packingLine2, packingLine3, packingLine4 });
			shipmentDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine1 } );

			Factory.SaveForTesting();

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);
			manager.Setup(m => m.AddPackagesByContainerLink(It.IsAny<ZInt>(), It.IsAny<List<PkgPackage>>()));

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForRCN(rcn, Factory, Logger, shipmentDataObject, shipmentDataObject);

				var packageStateDTO = finder.Find(new PackingLine() { Link = 1 });
				Assert(packageStateDTO != null);
				AssertEquals("packingLine1 is matched", Constants.PkgUnit.Pallet, Factory.Load<PkgPackage>(packageStateDTO.WPS_KP_Package).KP_F3_NKPackType);
				AssertPackageStateDTO(packageStateDTO, warehouse.PK, rcnPK: rcn.PK, parentPackagePK: null, isNew: true, unitType: PackageStateUnitType.Codes.Overpack, status: TransitWarehouseStatuses.Codes.Booked, isHandingUnit: true);

				var packageStateDTO1 = finder.Find(new PackingLine() { ReferenceNumber = "packingLine2" });
				Assert(packageStateDTO1 != null);
				AssertEquals("packingLine2 is matched", Constants.PkgUnit.Box, Factory.Load<PkgPackage>(packageStateDTO1.WPS_KP_Package).KP_F3_NKPackType);
				AssertPackageStateDTO(packageStateDTO1, warehouse.PK, rcnPK: rcn.PK, parentPackagePK: packageStateDTO.WPS_KP_Package, isNew: true, unitType: PackageStateUnitType.Codes.Package, status: TransitWarehouseStatuses.Codes.Booked);

				var packageStateDTO2 = finder.Find(new PackingLine() { PackType = new PackageType() { Code = Constants.PkgUnit.Tube, Description = Constants.PkgUnit.Tube } });
				Assert(packageStateDTO2 != null);
				AssertEquals("packingLine3 is matched", Constants.PkgUnit.Tube, Factory.Load<PkgPackage>(packageStateDTO2.WPS_KP_Package).KP_F3_NKPackType);
				AssertPackageStateDTO(packageStateDTO2, warehouse.PK, rcnPK: rcn.PK, parentPackagePK: packageStateDTO.WPS_KP_Package, isNew: true, unitType: PackageStateUnitType.Codes.Package, status: TransitWarehouseStatuses.Codes.Booked);

				var packageStateDTO3 = finder.Find(new PackingLine() { Weight = 2, Width = 2, Length = 2 });
				Assert(packageStateDTO3 != null);
				AssertEquals("packingLine4 is matched", Constants.PkgUnit.Crate, Factory.Load<PkgPackage>(packageStateDTO3.WPS_KP_Package).KP_F3_NKPackType);
				AssertPackageStateDTO(packageStateDTO3, warehouse.PK, rcnPK: rcn.PK, parentPackagePK: packageStateDTO.WPS_KP_Package, isNew: true, unitType: PackageStateUnitType.Codes.Package, status: TransitWarehouseStatuses.Codes.Booked);
			}
		}

		#endregion

		#region TestLogMessage_MatchPackageStatesWithoutIDAttachedToSeveralDCNs

		public void TestLogMessage_MatchPackageStatesWithoutIDAttachedToSeveralDCNs()
		{
			var packingLineWithoutPackageID = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) { PackQty = 2, PackType = new PackageType { Code = Constants.PkgUnit.Box } };
			Data.ShipmentDataObject.PackingLineCollection.Clear();
			Data.ShipmentDataObject.PackingLineCollection.Add(packingLineWithoutPackageID);
			var rcn = Helper.CreateReceiveConsignment("RCN", Data.Warehouse.PK);
			var row = Helper.CreateRowAndGenerateLocations(Data.Warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var rtu = Helper.CreateReceiveTransportationUnit("RTU001", Data.Warehouse.PK, location.PK);

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", Data.Warehouse.PK);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", Data.Warehouse.PK);
			var packageState1 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn1);
			var packageState2 = Helper.CreatePackageState(rcn, 1, PackageStateUnitType.Codes.Package, "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn2);
			Factory.SaveForTesting();

			var expectedErrorMessage =
@"Found multiple Dispatch Consignments :
DCN
DCN1 (DCN1)
DCN2 (DCN2)
Package IDs are required for imports to Receive Consignments with more than one Dispatch Consignment. Either add Package IDs or remove the Packages from the other Dispatch Consignments. Packages can be removed from via a Dispatch Instruction from their corresponding Data Source e.g. Forwarding Shipment, or manually via Transit Warehouse.";

			var manager = new Mock<TransitDataObjectReaderHandlerManager>();
			var consolHandler = new TransitReceiveConsolHandler();
			manager.Setup(d => d.GetHandler<TransitReceiveConsolHandler>()).Returns(consolHandler);
			manager.Setup(m => m.AddPackagesByContainerLink(It.IsAny<ZInt>(), It.IsAny<List<PkgPackage>>()));

			using (ObjectFactory.Substitute("TransitDataObjectReaderHandlerManager", manager.Object))
			{
				var finder = new WhsTransitPackageStateBusinessObjectFinderForRCN(rcn, Factory, Logger, Data.ShipmentDataObject, Data.ShipmentDataObject);
				AssertExceptionThrown(typeof(DataObjectReadFailureException), expectedErrorMessage, () => finder.Find(new PackingLine()));
			}
		}

		#endregion

		#region TestPopulateLinesCanHandleDuplicatePackageID

		public void TestPopulateLinesCanHandleDuplicatePackageID()
		{
			Data.SetupForForwardingImport();
			var shipment = Data.CreateShipmentWithPackages("A123", "Pack1", "Pack2");
			shipment.PackingLineCollection[0].ReferenceNumber = "DUPLICATE_ID";
			shipment.PackingLineCollection[1].ReferenceNumber = "DUPLICATE_ID";
			var rcn = Helper.CreateReceiveConsignment("RCN", Data.Warehouse.PK);

			var finder = new WhsTransitPackageStateBusinessObjectFinderForRCN(rcn, Factory, Logger, shipment, shipment);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Reference number DUPLICATE_ID is used for more than one package on a shipment RCN. Please provide a unique reference number or leave it empty.", () => finder.Find(new PackingLine()));
		}

		#endregion

		#region TestThrowExceptionWhenParamIsNull

		public void TestThrowExceptionWhenParamIsNull()
		{
			var warehouse = UniversalData.Warehouse;
			var rcn1 = Helper.CreateReceiveConsignment("RC001", warehouse.PK);
			Factory.SaveForTesting();

			AssertExceptionThrown<ArgumentNullException>(() => new WhsTransitPackageStateBusinessObjectFinderForRCN(null, Factory));
			AssertExceptionThrown<ArgumentNullException>(() => new WhsTransitPackageStateBusinessObjectFinderForRCN(rcn1, null));
		}

		#endregion

		void AssertPackageStateDTO(WhsItemPackageStateDTO packageStateDTO, ZGuid wareHousePK, ZGuid? rcnPK, ZGuid? parentPackagePK, ZGuid? dcnPK = null, bool isNew = false, bool isHandingUnit = false, string unitType = "", string status = "", bool isHighRisk = false) 
		{
			AssertEquals("Warehouse", wareHousePK, packageStateDTO.WPS_WW_Warehouse);
			AssertEquals("ReceiveConsignment", rcnPK, packageStateDTO.WPS_WRC_TransitReceiveConsignment);
			AssertEquals("DispatchConsignment", dcnPK, packageStateDTO.WPS_WDC_TransitDispatchConsignment);
			AssertEquals("ParentPackage", parentPackagePK, packageStateDTO.KP_KP_ParentPackage);
			AssertEquals("IsNew", isNew, packageStateDTO.IsNew);
			AssertEquals("IsHandlingUnit", isHandingUnit, packageStateDTO.WPS_IsHandlingUnit);
			AssertEquals("UnitType", unitType, packageStateDTO.WPS_UnitType);
			AssertEquals("Status", status, packageStateDTO.WPS_Status);
			AssertEquals("IsHighRisk", isHighRisk, packageStateDTO.WPS_IsHighRisk);
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		TestDataForUniversal UniversalData => universalData ?? new TestDataForUniversal(Factory, Logger);
		readonly TestDataForUniversal universalData;
	}
}
