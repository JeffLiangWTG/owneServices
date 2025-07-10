using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Warehouse.Transit.DataTransfer.Universal.WhsTransitPackageStateDataObjectReaderConstants;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	class WhsItemPackageStateDataObjectReaderTest : TransitUniversalTestCase
	{
		public void TestPopulateBusinessObject_CreatePackageState()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var package = Helper.CreatePackage(receiveConsignment.PackageJob, "P1", 1, "PLT");

			var packageStateDTO = new WhsItemPackageStateDTO();
			packageStateDTO.WPS_KP_Package = package.PK;
			packageStateDTO.WPS_WRC_TransitReceiveConsignment = receiveConsignment.PK;
			packageStateDTO.WPS_WDC_TransitDispatchConsignment = dispatchConsignment.PK;
			packageStateDTO.WPS_WW_Warehouse = warehouse.PK;
			packageStateDTO.WPS_IsHandlingUnit = false;
			packageStateDTO.WPS_IsHighRisk = false;
			packageStateDTO.WPS_UnitType = PackageStateUnitType.Codes.Package;

			var reader = new WhsItemPackageStateDataObjectReader(Data.ShipmentDataObject, Logger, Factory, null, packageStateDTO, WhsTransitPackageStatePopulateStrategy.New, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, receiveConsignment);
			reader.ReadIntoBusinessObject();

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery());

			AssertEquals(1, packageStates.Length);
			AssertEquals(package.PK, packageStates[0].WPS_KP_Package);
			AssertEquals(receiveConsignment.PK, packageStates[0].WPS_WRC_TransitReceiveConsignment);
			AssertEquals(warehouse.PK, packageStates[0].WPS_WW_Warehouse);
			AssertEquals(false, packageStates[0].WPS_IsHandlingUnit);
			AssertEquals(false, packageStates[0].WPS_IsHighRisk);
			AssertEquals(PackageStateUnitType.Codes.Package, packageStates[0].WPS_UnitType);
			AssertEquals(dispatchConsignment.PK, packageStates[0].WPS_WDC_TransitDispatchConsignment);
		}

		public void TestPopulateBusinessObject_DetachPackagePackageFromASN()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var package = Helper.CreatePackage(receiveConsignment.PackageJob, "P1", 1, "PLT");
			var packageState = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Booked, receiveConsignment);
			packageState.WPS_WRP_ReceiveExpectedPacking = asn.PK;

			var reader = new WhsItemPackageStateDataObjectReader(Data.ShipmentDataObject, Logger, Factory, packageState, new WhsItemPackageStateDTO(), WhsTransitPackageStatePopulateStrategy.Detach, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, asn);
			reader.ReadIntoBusinessObject();

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery());

			AssertEquals(1, packageStates.Length);
			AssertEquals(Guid.Empty, packageStates[0].WPS_WRP_ReceiveExpectedPacking);
		}

		public void TestPopulateBusinessObject_AttachPackageStateToASN()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var package = Helper.CreatePackage(receiveConsignment.PackageJob, "P1", 1, "PLT");
			var packageState = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Booked, receiveConsignment);

			var reader = new WhsItemPackageStateDataObjectReader(Data.ShipmentDataObject, Logger, Factory, packageState, new WhsItemPackageStateDTO(), WhsTransitPackageStatePopulateStrategy.Attach, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, asn);
			reader.ReadIntoBusinessObject();

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery());

			AssertEquals(1, packageStates.Length);
			AssertEquals(asn.PK, packageStates[0].WPS_WRP_ReceiveExpectedPacking);
		}

		public void TestUpdateIsHighRiskAndExtenalReference()
		{
			var warehouse = UniversalData.Warehouse;
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var package = Helper.CreatePackage(receiveConsignment.PackageJob, "P1", 1, "PLT");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var packageState = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Arrived, receiveConsignment, receiveUnit: rtu, dispatchConsignment: dcn);
			Factory.SaveForTesting();

			var packageStateDTO = new WhsItemPackageStateDTO();
			packageStateDTO.WPS_IsHighRisk = true;
			packageStateDTO.KP_ExternalReference = "ExternalRef";

			var reader = new WhsItemPackageStateDataObjectReader(Data.ShipmentDataObject, Logger, Factory, packageState, packageStateDTO, WhsTransitPackageStatePopulateStrategy.Update, WhsTransitPackageStateUpdateStrategy.PartialUpdate, dcn);
			reader.ReadIntoBusinessObject();

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery());

			AssertEquals(1, packageStates.Length);
			AssertEquals(true, packageStates[0].WPS_IsHighRisk);
			AssertEquals("ExternalRef", packageStates[0].Package.KP_ExternalReference);
		}

		public void TestPopulateBusinessObject_AttachPackageStateToDCN()
		{
			var warehouse = UniversalData.Warehouse;
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var package = Helper.CreatePackage(receiveConsignment.PackageJob, "P1", 1, "PLT");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var packageState = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Arrived, receiveConsignment, receiveUnit: rtu);

			var ovp = Helper.CreateOverpackPackage("OVP1", receiveConsignment, rtu, status: TransitWarehouseStatuses.Codes.Arrived, rcn: receiveConsignment);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu);
			var innerPackline = Helper.CreatePackageState(receiveConsignment, 10, TransitConstants.TransitPackageUnitType.Packline, "", TransitWarehouseStatuses.Codes.Arrived, rtu);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, innerPackline, ZDateTimeOffset.Now, "ABC", handlingUnitPackage);
			Factory.SaveForTesting();

			var reader1 = new WhsItemPackageStateDataObjectReader(Data.ShipmentDataObject, Logger, Factory, packageState, new WhsItemPackageStateDTO(), WhsTransitPackageStatePopulateStrategy.Attach, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, dcn);
			reader1.ReadIntoBusinessObject();

			var reader2 = new WhsItemPackageStateDataObjectReader(Data.ShipmentDataObject, Logger, Factory, ovp, new WhsItemPackageStateDTO(), WhsTransitPackageStatePopulateStrategy.Attach, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, dcn);
			reader2.ReadIntoBusinessObject();

			var reader3 = new WhsItemPackageStateDataObjectReader(Data.ShipmentDataObject, Logger, Factory, innerPackline, new WhsItemPackageStateDTO(), WhsTransitPackageStatePopulateStrategy.Attach, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, dcn);
			reader3.ReadIntoBusinessObject();

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_UnitType, SQLComparisonOperator.NotEqual, PackageStateUnitType.Codes.HandlingUnit));

			AssertEquals(3, packageStates.Length);
			AssertEquals(dcn.PK, packageStates[0].WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcn.PK, packageStates[1].WPS_WDC_TransitDispatchConsignment);
			AssertEquals(dcn.PK, packageStates[2].WPS_WDC_TransitDispatchConsignment);
		}

		public void TestPopulateBusinessObject_DetachPackageStateFromDCN()
		{
			var warehouse = UniversalData.Warehouse;
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultLocation.PK);
			var package = Helper.CreatePackage(receiveConsignment.PackageJob, "P1", 1, "PLT");
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var packageState = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Arrived, receiveConsignment, receiveUnit: rtu, dispatchConsignment: dcn);
			Factory.SaveForTesting();

			var reader = new WhsItemPackageStateDataObjectReader(Data.ShipmentDataObject, Logger, Factory, packageState, new WhsItemPackageStateDTO(), WhsTransitPackageStatePopulateStrategy.Detach, WhsTransitPackageStateUpdateStrategy.CompleteUpdate, dcn);
			reader.ReadIntoBusinessObject();

			var packageStates = Factory.Load<WhsItemPackageState>(new ZQuery());

			AssertEquals(1, packageStates.Length);
			AssertEquals(ZGuid.Empty, packageStates[0].WPS_WDC_TransitDispatchConsignment);
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		TestDataForUniversal UniversalData => universalData ?? new TestDataForUniversal(Factory, Logger);
		readonly TestDataForUniversal universalData;
	}
}
