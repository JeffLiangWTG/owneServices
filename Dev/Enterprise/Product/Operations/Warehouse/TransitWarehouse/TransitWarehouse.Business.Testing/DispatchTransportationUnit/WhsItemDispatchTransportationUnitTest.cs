using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class WhsItemDispatchTransportationUnitTest : TestCaseWithFactory
	{
		#region TestReadOnlyFields

		static readonly string[] ReadOnlyProperties =
		{
			"WDH_WW_Warehouse",
			"WDH_SystemCreateTimeUtc",
			"WDH_SystemCreateUser",
			"WDH_SystemLastEditTimeUtc",
			"WDH_SystemLastEditUser",
			"WDH_FinalisedTime",
			"WDH_GateOutTime",
			"WDH_GateInTime",
			"WDH_LoadCompleteTime",
			"WDH_ReferenceNumber",
			"WDH_SignedBySignature",
		};

		public void TestReadOnly()
		{
			var dtu = Factory.New<WhsItemDispatchTransportationUnit>();
			foreach (var propertyName in ReadOnlyProperties)
			{
				var readOnly = ((ZPropertyInfo)typeof(WhsItemDispatchTransportationUnit).GetProperty(propertyName + "Info").GetValue(dtu)).ReadOnly;
				var readOnlyAction = ActionFieldAttribute.Get(typeof(WhsItemDispatchTransportationUnit).GetProperty(propertyName))?.ReadOnly;
				AssertEquals(propertyName, true, readOnly);
				AssertEquals(propertyName, true, readOnlyAction);
			}
		}

		#endregion

		#region TestAutoLog

		public void TestAutoLog()
		{
			var header = Factory.New<WhsItemDispatchTransportationUnit>();
			AssertEquals(true, header.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestJobDocAddresses

		public void TestJobDocAddresses()
		{
			var header = Factory.New<WhsItemDispatchTransportationUnit>();
			AssertEquals(DocAddressType.TransportCompanyDocumentaryAddress, header.TransportCompany.DocAddressType);
			AssertEquals(DocAddressType.ClientRequestedBillingParty, header.ClientRequestedBillToPartyDocAddress.DocAddressType);
		}

		#endregion

		#region TestTransportReference

		public void TestTransportReference()
		{
			var unitWithoutTransportReference = Factory.New<WhsItemDispatchTransportationUnit>();
			var unitWithVehicleNumber = Factory.New<WhsItemDispatchTransportationUnit>();
			var unitWithTransportNumber = Factory.New<WhsItemDispatchTransportationUnit>();
			var unitWithBothVehicleAndTransportNumber = Factory.New<WhsItemDispatchTransportationUnit>();
			var unitWithBothVehicleDifferentTypeOfAdditionalReference = Factory.New<WhsItemDispatchTransportationUnit>();
			unitWithVehicleNumber.WDH_VehicleReference = "V1";
			unitWithBothVehicleAndTransportNumber.WDH_VehicleReference = "V2";
			unitWithBothVehicleDifferentTypeOfAdditionalReference.WDH_VehicleReference = "V3";
			CreateAdditionalReference(unitWithTransportNumber, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, "T1");
			CreateAdditionalReference(unitWithBothVehicleAndTransportNumber, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, "B2");
			CreateAdditionalReference(unitWithBothVehicleDifferentTypeOfAdditionalReference, TransportAdditionalReferenceTypes.Codes.BookingPartyReference, "C2");

			AssertEquals("", unitWithoutTransportReference.TransportReference);
			AssertEquals("V1", unitWithVehicleNumber.TransportReference);
			AssertEquals("T1", unitWithTransportNumber.TransportReference);
			AssertEquals("B2", unitWithBothVehicleAndTransportNumber.TransportReference);
			AssertEquals("V3", unitWithBothVehicleDifferentTypeOfAdditionalReference.TransportReference);
		}

		void CreateAdditionalReference<T>(T unit, string referenceType, string referenceNumber) where T : BusinessObject
		{
			var additionalReference = Factory.NewWithValidTestData<CusEntryNumber>();
			additionalReference.CE_ParentID = unit.PK;
			additionalReference.CE_ParentTable = WhsItemDispatchTransportationUnitSchema.Constants.TableName;
			additionalReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			additionalReference.CE_EntryNum = referenceNumber;
			additionalReference.CE_EntryType = referenceType;
		}

		#endregion

		#region TestMasterBillNumber

		public void TestMasterBillNumber()
		{
			var unitWithoutMasterBillNumber = Factory.New<WhsItemDispatchTransportationUnit>();
			var unitWithMasterBillNumber = Factory.New<WhsItemDispatchTransportationUnit>();
			var unitWithMasterBillNumberOnDLL = Factory.New<WhsItemDispatchTransportationUnit>();
			var loadList = Factory.New<WhsItemDispatchLoadList>();
			CreateAdditionalReference(loadList, TransportAdditionalReferenceTypes.Codes.MasterBill, "DLLB");
			CreateAdditionalReference(unitWithMasterBillNumber, TransportAdditionalReferenceTypes.Codes.MasterBill, "B1");
			CreateAdditionalReference(unitWithMasterBillNumberOnDLL, TransportAdditionalReferenceTypes.Codes.MasterBill, "B2");
			Helper.CreateDispatchDLLDTUPivot(loadList.PK, unitWithMasterBillNumberOnDLL.PK);
			AssertEquals("", unitWithoutMasterBillNumber.MasterBillNumber);
			AssertEquals("B1", unitWithMasterBillNumber.MasterBillNumber);
			AssertEquals("B2", unitWithMasterBillNumberOnDLL.MasterBillNumber);
		}

		public void TestMasterBillNumber_FallbackToDLL()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dtu = Helper.CreateDispatchTransportationUnit("D2", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			CreateAdditionalReference(dll, TransportAdditionalReferenceTypes.Codes.MasterBill, "DLLB1");
			Factory.Save();
			AssertEquals("DLLB1", dtu.MasterBillNumber);

			CreateAdditionalReference(dtu, TransportAdditionalReferenceTypes.Codes.MasterBill, "DTUMAB1");
			Factory.Save();
			var newBizOFactory = new BusinessObjectFactory();
			AssertEquals("DTUMAB1", newBizOFactory.Load<WhsItemDispatchTransportationUnit>(dtu.PK).MasterBillNumber);
		}

		public void TestMasterBillNumber_FallbackToDLL_MultipleDLLs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dtu = Helper.CreateDispatchTransportationUnit("D2", warehouse.PK);
			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll1.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu.PK);
			CreateAdditionalReference(dll1, TransportAdditionalReferenceTypes.Codes.MasterBill, "DLLB1");
			CreateAdditionalReference(dll2, TransportAdditionalReferenceTypes.Codes.MasterBill, "DLLB2");
			Factory.Save();
			AssertEquals("DLLB2", dtu.MasterBillNumber);
		}

		#endregion

		#region TestCarrierBookingReference

		public void TestCarrierBookingReference_FromDLL()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dtu = Helper.CreateDispatchTransportationUnit("D2", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			CreateAdditionalReference(dll, WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference, "DLLBooking");
			Factory.Save();
			AssertEquals("DLLBooking", dtu.CarrierBookingReference);
		}

		public void TestCarrierBookingReference_FromMultipleDLLs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dtu = Helper.CreateDispatchTransportationUnit("D2", warehouse.PK);
			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll1.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu.PK);
			CreateAdditionalReference(dll1, WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference, "DLLBooking1");
			CreateAdditionalReference(dll2, WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference, "DLLBooking2");
			Factory.Save();
			AssertEquals("DLLBooking1", dtu.CarrierBookingReference);
		}

		#endregion

		#region TestTransferMode

		public void TestTransferMode()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			AssertEquals("Transport mode is empty", ZString.Empty, dispatchTransportationUnit.TransportMode);

			var dispatchTransportationUnit2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dispatchConsignment = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dispatchConsignment.WDC_TransportMode = "Air";
			Helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", "FLO", receiveTransportationUnit, dispatchConsignment, dispatchTransportationUnit2, loadList, location: warehouse.DefaultOutboundDockDoorLocation);
			Factory.Save();

			AssertEquals("Transport mode has value", "Air", dispatchTransportationUnit2.TransportMode);
		}

		#endregion

		#region TestContainers

		#region TestContainerNumber

		public void TestContainerNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var unitWithoutContainer = Helper.CreateDispatchTransportationUnit("Container 1", warehouse.PK, "Container 1");
			var unitWithContainer = Helper.CreateDispatchTransportationUnitWithContainerType("Container 1", warehouse.PK, "Container 1");

			AssertEquals(ZString.Empty, unitWithoutContainer.ContainerNumber);
			AssertEquals("Container 1", unitWithContainer.ContainerNumber);
		}

		#endregion

		#region TestHasContainerEquipmentDetails

		public void TestHasContainerEquipmentDetails()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var unitWithoutContainer = Helper.CreateDispatchTransportationUnit("Container 1", warehouse.PK, "Container 1");
			var unitWithContainer = Helper.CreateDispatchTransportationUnitWithContainerType("Container 1", warehouse.PK, "Container 1");

			AssertEquals(false, unitWithoutContainer.HasContainerEquipmentDetails);
			AssertEquals(true, unitWithContainer.HasContainerEquipmentDetails);
		}

		#endregion

		#region TestContainerTypeCode

		public void TestContainerTypeCode()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var unitWithoutContainer = Helper.CreateDispatchTransportationUnit("Container 1", warehouse.PK, "Container 1");
			var unitWithContainer = Helper.CreateDispatchTransportationUnitWithContainerType("Container 1", warehouse.PK, "Container 1");

			AssertEquals(ZString.Empty, unitWithoutContainer.ContainerTypeCode);
			AssertEquals("20GP", unitWithContainer.ContainerTypeCode);
		}

		#endregion

		#region TestContainerISOType

		public void TestContainerISOType()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var unitWithoutContainer = Helper.CreateDispatchTransportationUnit("Container 1", warehouse.PK, "Container 1");
			var unitWithContainer = Helper.CreateDispatchTransportationUnitWithContainerType("Container 1", warehouse.PK, "Container 1");

			AssertEquals(ZString.Empty, unitWithoutContainer.ContainerISOType);
			AssertEquals("22G0", unitWithContainer.ContainerISOType);
		}

		#endregion

		#region TestIsContainerUnitType

		public void TestIsContainerUnitType()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var uldDTU = Helper.CreateDispatchTransportationUnitWithContainerType("ULD 1", warehouse.PK, "ULD 1", containerType: "AAA");
			var cntDTU = Helper.CreateDispatchTransportationUnitWithContainerType("CNT 1", warehouse.PK, "CNT 1", containerType: "20GP");
			var vehDTUWithContainer = Helper.CreateDispatchTransportationUnitWithContainerType("VEH 1", warehouse.PK, "VEH 1", containerType: "DROP");
			var vehDTUWithoutContainer = Helper.CreateDispatchTransportationUnit("VEH 2", warehouse.PK, "VEH 2");

			AssertEquals(true, uldDTU.IsContainerUnitType);
			AssertEquals(true, cntDTU.IsContainerUnitType);
			AssertEquals(true, vehDTUWithContainer.IsContainerUnitType);
			AssertEquals(false, vehDTUWithoutContainer.IsContainerUnitType);
		}

		#endregion

		#endregion

		#region TestContainer

		public void TestContainer()
		{
			var warehouse = Helper.CreateTRWWarehouse("WHS");
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("Container 1", warehouse.PK);
			AssertNotNull(dtu.Container);
			AssertEquals("20GP", dtu.Container.ContainerType.RC_Code);
		}

		#endregion

		#region TestVehicleNumber

		public void TestVehicleNumber()
		{
			var unitWithoutContainer = Factory.New<WhsItemDispatchTransportationUnit>();
			unitWithoutContainer.WDH_VehicleReference = "Vehicle 1";
			unitWithoutContainer.WDH_UnitType = TransportUnitTypes.Vehicle;
			var unitWithContainer = Factory.New<WhsItemDispatchTransportationUnit>();
			unitWithContainer.WDH_VehicleReference = "Vehicle 1";
			unitWithContainer.WDH_UnitType = TransportUnitTypes.ULD;

			AssertEquals(ZString.Empty, unitWithContainer.VehicleNumber);
			AssertEquals("Vehicle 1", unitWithoutContainer.VehicleNumber);
		}

		#endregion

		#region TestGateInTime

		public void TestGateInTime()
		{
			var gateInTime = DateTimeOffset.Now;
			var tranportationUnit = Factory.New<WhsItemDispatchTransportationUnit>();
			tranportationUnit.WDH_GateInTime = gateInTime;

			AssertEquals(gateInTime, tranportationUnit.GateInTime);
		}

		#endregion

		#region TestIsGateOut

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestIsGatedOut()
		{
			var warehouse = Helper.CreateTRWWarehouse("TRW");
			warehouse.WarehouseAddress.OA_City = "Sydney";
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			dtu.WDH_ReferenceNumber = "DTU123";
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			Factory.Save();

			AssertEquals("Precondition: DTU gate out time is empty.", true, dtu.WDH_GateOutTime.IsEmpty);
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.FreightLoaded);

			dtu.WDH_LoadCompleteTime = ZDateTimeOffset.Now.AddDays(-1);
			dtu.IsGatedOut = true;

			AssertEquals("DTU gate out time should be set.", false, dtu.WDH_GateOutTime.IsEmpty);
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.Departed);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateOut.Code);
			var dtuLog = dtu.Logs.Find(logQuery).SingleOrDefault();
			AssertNotNull("Should create gate out log for dtu", dtuLog);
			AssertEquals("DTU123|FAC=CFS|LOC=Sydney|REF=V1|TYP=VehicleReference|WHS=TRW", dtuLog.SL_Reference);
			AssertEquals(ZDateTimeOffset.Now.ToDateTime(), dtuLog.SL_EventTime);
			AssertEquals(new ZDateTimeOffset(2021, 1, 1, 10, 0, 0, new TimeSpan(10, 0, 0)), dtu.WDH_GateOutTime);
		}

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestIsGateOut_LoadCompleteTimeAndGateOutTimeSame()
		{
			var warehouse = Helper.CreateTRWWarehouse("TRW");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			Factory.Save();

			AssertEquals("Precondition: DTU gate out time is empty.", true, dtu.WDH_GateOutTime.IsEmpty);
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.FreightLoaded);

			dtu.WDH_LoadCompleteTime = ZDateTimeOffset.Now;
			dtu.IsGatedOut = true;

			AssertEquals("DTU gate out time should be set.", false, dtu.WDH_GateOutTime.IsEmpty);
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.Departed);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateOut.Code);
			var dtuLog = dtu.Logs.Find(logQuery).SingleOrDefault();
			AssertNotNull("Should create gate out log for dtu", dtuLog);
			AssertEquals("DTU1|FAC=CFS|REF=V1|TYP=VehicleReference|WHS=TRW", dtuLog.SL_Reference);
			AssertEquals(ZDateTimeOffset.Now.ToDateTime(), dtuLog.SL_EventTime);
			AssertEquals(new ZDateTimeOffset(2021, 1, 1, 10, 0, 0, new TimeSpan(10, 0, 0)), dtu.WDH_GateOutTime);
		}

		[TestDate(2021, 1, 1)]
		public void TestIsGateOut_DoNotSetGateOutTimeIfLoadCompleteTimeIsEmpty()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			AssertEquals("Precondition: DTU load complete time is empty.", true, dtu.WDH_LoadCompleteTime.IsEmpty);
			AssertEquals("Precondition: DTU gate out time is empty.", true, dtu.WDH_GateOutTime.IsEmpty);
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.FreightLoaded);

			dtu.IsGatedOut = true;

			AssertEquals("DTU gate out time should not be set.", true, dtu.WDH_GateOutTime.IsEmpty);
			AssertNotEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.Departed);
		}

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestIsGateOut_SetGateOutTimeIfLoadCompleteTimeIsInTheFuture()
		{
			var warehouse = Helper.CreateTRWWarehouse("ABC");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			Factory.Save();

			AssertEquals("Precondition: DTU load complete time is empty.", true, dtu.WDH_LoadCompleteTime.IsEmpty);
			AssertEquals("Precondition: DTU gate out time is empty.", true, dtu.WDH_GateOutTime.IsEmpty);
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.FreightLoaded);

			dtu.WDH_LoadCompleteTime = ZDateTimeOffset.Now.AddHours(10);
			dtu.IsGatedOut = true;

			AssertEquals("DTU gate out time should be set.", false, dtu.WDH_GateOutTime.IsEmpty);
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.Departed);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateOut.Code);
			var dtuLog = dtu.Logs.Find(logQuery).SingleOrDefault();
			AssertNotNull("Should create gate out log for dtu", dtuLog);
			AssertEquals("DTU1|FAC=CFS|REF=V1|TYP=VehicleReference|WHS=ABC", dtuLog.SL_Reference);
			AssertEquals(ZDateTimeOffset.Now.ToDateTime(), dtuLog.SL_EventTime.ToDateTime());
			AssertEquals(new ZDateTimeOffset(2021, 1, 1, 20, 0, 0, new TimeSpan(10, 0, 0)), dtu.WDH_GateOutTime);
		}

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWhenUpdateVehicleToGateOutAlsoUpdateLoadedContainerlizedDTUAndTheirInners()
		{
			var warehouse = Helper.CreateTRWWarehouse("TRW");
			warehouse.WarehouseAddress.OA_City = "Sydney";
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var packageStatePKG = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			var (uld, package) = Helper.CreateDispatchTransportationUnitWithPackage("ULD", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);

			var packageStateULD = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.FreightLoaded, receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);
			packageStateULD.WPS_UnitType = "ULD";

			var packageStatePKG11 = Helper.CreatePackageState(rcn, 1, "PKG", "P11", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: uld, dispatchLoadList: dll2);
			packageStatePKG11.WPS_UnitType = "PKG";

			Helper.PackPackageIntoHandlingUnit(packageStateULD, packageStatePKG11, ZDateTimeOffset.Now, "AAA", packageStateULD);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, dcn: dcn, dll: dll2, dtu: uld);
			var handlingUnitPackage2 = Helper.CreateHandlingUnitPackage("HU2", handlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, dcn: dcn, dll: dll, dtu: dtu);
			var subHandlingUnitPackage = Helper.CreateHandlingUnitPackage("HU11", handlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, dcn: dcn, dll: dll2, dtu: uld);

			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "P111", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: uld);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "P222", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: uld);
			var childPackage3 = Helper.CreatePackageState(rcn, 1, "PKG", "P333", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: uld);
			var childPackage4 = Helper.CreatePackageState(rcn, 1, "PKG", "P44", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);

			Helper.DisableTopLevelHUFKForTest(TestConnection);

			Helper.PackPackageIntoHandlingUnit(packageStateULD, handlingUnitPackage, ZDateTimeOffset.Now, "AAA", packageStateULD);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: packageStateULD);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, subHandlingUnitPackage, ZDateTimeOffset.Now, "AAA", topHandlingUnit: packageStateULD);
			Helper.PackPackageIntoHandlingUnit(subHandlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: packageStateULD);
			Helper.PackPackageIntoHandlingUnit(subHandlingUnitPackage, childPackage3, ZDateTimeOffset.Now, "AAA", topHandlingUnit: packageStateULD);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage2, childPackage4, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage2);

			Factory.Save();

			AssertEquals("Precondition: DTU gate out time is empty.", true, dtu.WDH_GateOutTime.IsEmpty);

			AssertPackageStatusChanged(TransitWarehouseStatuses.Codes.FreightLoaded, packageStatePKG, packageStateULD, packageStatePKG11, handlingUnitPackage, handlingUnitPackage2, subHandlingUnitPackage, childPackage1, childPackage2, childPackage3, childPackage4);

			dtu.WDH_LoadCompleteTime = ZDateTimeOffset.Now.AddDays(-1);
			dtu.IsGatedOut = true;

			AssertEquals("DTU gate out time should be set.", false, dtu.WDH_GateOutTime.IsEmpty);
			AssertNotNull(uld.WDH_GateOutTime);

			AssertPackageStatusChanged(TransitWarehouseStatuses.Codes.Departed, packageStatePKG, packageStateULD, packageStatePKG11, handlingUnitPackage, handlingUnitPackage2, subHandlingUnitPackage, childPackage1, childPackage2, childPackage3, childPackage4);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateOut.Code);

			AssertLogs(logQuery, TransitWarehouseStatuses.Codes.Departed, new[] { dtu, uld });
		}

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWhenUpdateContainerlizedDTUToGateOutAlsoUpdateItselfAndItsInners()
		{
			var warehouse = Helper.CreateTRWWarehouse("TRW");
			warehouse.WarehouseAddress.OA_City = "Sydney";
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;

			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var uldDTU = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);

			var uldDTUPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, uldDTU.PackageExtension.KPN_KP_Package)).FirstOrDefault();
			uldDTUPackageState.WPS_Status = "FLO";

			var packageStatePKG11 = Helper.CreatePackageState(rcn, 1, "PKG", "P11", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: uldDTU, dispatchLoadList: dll2);
			packageStatePKG11.WPS_UnitType = "PKG";

			Helper.PackPackageIntoHandlingUnit(uldDTUPackageState, packageStatePKG11, ZDateTimeOffset.Now, "AAA", uldDTUPackageState);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, dcn: dcn, dll: dll2, dtu: uldDTU);
			var handlingUnitPackage2 = Helper.CreateHandlingUnitPackage("HU2", handlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, dcn: dcn, dll: dll, dtu: uldDTU);
			var subHandlingUnitPackage = Helper.CreateHandlingUnitPackage("HU11", handlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, dcn: dcn, dll: dll2, dtu: uldDTU);

			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "P111", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: uldDTU);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "P222", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: uldDTU);
			var childPackage3 = Helper.CreatePackageState(rcn, 1, "PKG", "P333", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: uldDTU);

			Helper.DisableTopLevelHUFKForTest(TestConnection);

			Helper.PackPackageIntoHandlingUnit(uldDTUPackageState, handlingUnitPackage, ZDateTimeOffset.Now, "AAA", uldDTUPackageState);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: uldDTUPackageState);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, subHandlingUnitPackage, ZDateTimeOffset.Now, "AAA", topHandlingUnit: uldDTUPackageState);
			Helper.PackPackageIntoHandlingUnit(subHandlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: uldDTUPackageState);
			Helper.PackPackageIntoHandlingUnit(subHandlingUnitPackage, childPackage3, ZDateTimeOffset.Now, "AAA", topHandlingUnit: uldDTUPackageState);

			Factory.Save();

			AssertEquals("Precondition: DTU gate out time is empty.", true, uldDTU.WDH_GateOutTime.IsEmpty);

			AssertPackageStatusChanged(TransitWarehouseStatuses.Codes.FreightLoaded, uldDTUPackageState, packageStatePKG11, handlingUnitPackage, handlingUnitPackage2, subHandlingUnitPackage, childPackage1, childPackage2, childPackage3);

			uldDTU.WDH_LoadCompleteTime = ZDateTimeOffset.Now.AddDays(-1);
			uldDTU.IsGatedOut = true;

			AssertEquals("DTU gate out time should be set.", false, uldDTU.WDH_GateOutTime.IsEmpty);
			AssertNotNull(uldDTU.WDH_GateOutTime);

			AssertPackageStatusChanged(TransitWarehouseStatuses.Codes.Departed, uldDTUPackageState, packageStatePKG11, handlingUnitPackage, handlingUnitPackage2, subHandlingUnitPackage, childPackage1, childPackage2, childPackage3);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateOut.Code);

			AssertLogs(logQuery, TransitWarehouseStatuses.Codes.Departed, new[] { uldDTU }, isContainerizedDTU: true);
		}

		public void TestIsGateOut_DoNotSetGateOutTimeIfCNTRegistryIsSetToYes() => TestIsGateOut_CNTDTUCore(true, true);

		public void TestIsGateOut_SetGateOutTimeIfCNTRegistryIsSetToNo() => TestIsGateOut_CNTDTUCore(false, false);

		void TestIsGateOut_CNTDTUCore(bool registryValue, bool resultValue)
		{
			WarehouseDataRegistry.Instance.CNTNeedLoadingOntoVehicle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("CNT 1", warehouse.PK, "CNT 1", containerType: "20GP");
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			AssertEquals("Precondition: DTU load complete time is empty.", true, dtu.WDH_LoadCompleteTime.IsEmpty);
			AssertEquals("Precondition: DTU gate out time is empty.", true, dtu.WDH_GateOutTime.IsEmpty);
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.FreightLoaded);

			dtu.WDH_LoadCompleteTime = ZDateTimeOffset.Now;
			dtu.IsGatedOut = true;

			AssertEquals(dtu.WDH_UnitType, "CNT");
			AssertEquals(expected: resultValue, dtu.WDH_GateOutTime.IsEmpty);
			AssertNotEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.Departed);
		}

		[TestDate(2021, 1, 1)]
		public void TestIsGateOut_DoNotSetGateOutTimeIfULDRegistryDateIsInPast() => TestIsGateOut_ULDDTUCore(ZDateTimeOffset.Now.AddDays(-2), true);

		[TestDate(2021, 1, 1)]
		public void TestIsGateOut_SetGateOutTimeIfULDRegistryDateIsInFuture() => TestIsGateOut_ULDDTUCore(ZDateTimeOffset.Now.AddDays(2), false);

		void TestIsGateOut_ULDDTUCore(ZDateTimeOffset registryValue, bool resultValue)
		{
			WarehouseDataRegistry.Instance.ULDNeedLoadingOntoVehicle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue.ToDateTime());
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("ULD 1", warehouse.PK, "ULD 1", containerType: "AAA");
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			AssertEquals("Precondition: DTU load complete time is empty.", true, dtu.WDH_LoadCompleteTime.IsEmpty);
			AssertEquals("Precondition: DTU gate out time is empty.", true, dtu.WDH_GateOutTime.IsEmpty);
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.FreightLoaded);

			dtu.WDH_LoadCompleteTime = ZDateTimeOffset.Now;
			dtu.IsGatedOut = true;

			AssertEquals(dtu.WDH_UnitType, "ULD");
			AssertEquals(expected: resultValue, dtu.WDH_GateOutTime.IsEmpty);
			AssertNotEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.Departed);
		}

		public void TestIsGateOut_DoNotSetGateOutTimeIfCNTRegistryIsSetToNoAndDTUStillLoadedOntoVehicle() => TestIsGateOut_CNTULDDTULoadedOntoVehicleCore(false, ZDateTimeOffset.Now, TransportUnitTypes.Container);

		[TestDate(2021, 1, 1)]
		public void TestIsGateOut_DoNotSetGateOutTimeIfULDRegistryDateIsInFutureAndDTUStillLoadedOntoVehicle() => TestIsGateOut_CNTULDDTULoadedOntoVehicleCore(false, ZDateTimeOffset.Now.AddDays(2), TransportUnitTypes.ULD);

		void TestIsGateOut_CNTULDDTULoadedOntoVehicleCore(bool cntRegistryValue, ZDateTimeOffset uldRegistryValue, string unitType)
		{
			if (unitType == TransportUnitTypes.Container)
			{
				WarehouseDataRegistry.Instance.CNTNeedLoadingOntoVehicle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cntRegistryValue);
			}
			else
			{
				WarehouseDataRegistry.Instance.ULDNeedLoadingOntoVehicle.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, uldRegistryValue.ToDateTime());
			}
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			Factory.Save();

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var vehDTU = helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK, containerID: "DROP1", containerType: "DROP");
			vehDTU.WDH_GateInTime = now.AddHours(-1);
			vehDTU.WDH_LoadCompleteTime = now;

			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, containerID: "20GP1", containerType: "20GP");
			dtu.WDH_GateInTime = now.AddHours(-1);
			dtu.WDH_LoadCompleteTime = now;
			dtu.WDH_UnitType = unitType;

			var loadedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: vehDTU);

			var dtuPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtu.ContainerizedPackageState.WPS_KP_Package)).FirstOrDefault();
			dtuPackageState.WPS_LoadedTime = now;
			dtuPackageState.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
			dtuPackageState.WPS_WDH_TransitDispatchHeader = vehDTU.PK;
			dtuPackageState.WPS_WDL_LoadList = dll.PK;
			dtuPackageState.WPS_IsSecure = true;
			dtuPackageState.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Secured;

			dtu.IsGatedOut = true;

			AssertEquals(dtu.WDH_UnitType, unitType);
			AssertEquals(expected: true, dtu.WDH_GateOutTime.IsEmpty);
		}

		static void AssertPackageDTUAndLoadedTime(ZGuid dtuPK, params WhsItemPackageState[] packageStates)
		{
			foreach (var packageState in packageStates)
			{
				AssertNotNull(packageState.WPS_LoadedTime);
				AssertEquals(dtuPK, packageState.WPS_WDH_TransitDispatchHeader);
			}
		}

		static void AssertPackageStatusChanged(string expectedStatus, params WhsItemPackageState[] packageStates)
		{
			foreach (var p in packageStates)
			{
				AssertEquals(expectedStatus, p.WPS_Status);
			}
		}

		static void AssertLogs(ZQuery logQuery, string status, WhsItemDispatchTransportationUnit[] dtus = null, WhsItemPackageState[] packageStates = null, bool isContainerizedDTU = false)
		{
			if (dtus != null)
			{
				if (status == TransitWarehouseStatuses.Codes.Departed)
				{
					foreach (var dtu in dtus)
					{
						var dtuLog = dtu.Logs.Find(logQuery).SingleOrDefault();
						var typValue = dtu.WDH_UnitType == TransportUnitTypes.ULD ? "ULDID" : "ContainerID";
						AssertNotNull("Should create gate out log for dtu", dtuLog);
						var expectString = isContainerizedDTU ? dtu.WDH_ReferenceNumber + "|FAC=CFS|LOC=Sydney|REF=CNT1|TYP=" + typValue + "|WHS=TRW" : dtu.WDH_ReferenceNumber + "|FAC=CFS|LOC=Sydney|REF=V1|TYP=VehicleReference|WHS=TRW";
						AssertEquals(expectString, dtuLog.SL_Reference);
						AssertEquals(ZDateTimeOffset.Now.ToDateTime(), dtuLog.SL_EventTime);
						AssertEquals(new ZDateTimeOffset(2021, 1, 1, 10, 0, 0, new TimeSpan(10, 0, 0)), dtu.WDH_GateOutTime);
					}
				}
				else if (status == TransitWarehouseStatuses.Codes.Finalized)
				{
					foreach (var dtu in dtus)
					{
						var dtuLog = dtu.Logs.Find(logQuery).SingleOrDefault();

						AssertNotNull("Should create time out log for dtu", dtuLog);
						AssertEquals(dtu.WDH_ReferenceNumber + "|RES=Test|TYP=Finalised", dtuLog.SL_Reference);
					}
				}
			}
			if (packageStates != null && status == TransitWarehouseStatuses.Codes.Finalized)
			{
				foreach (var packageState in packageStates)
				{
					var finalizeLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ItemDocumentJobFinalised.Code);
					var packageStateLog = packageState.Package.Logs.Find(finalizeLogQuery).SingleOrDefault();
					var packageId = packageState.Package.KP_PackageID;

					AssertNotNull("Should create time out log for packageState", packageStateLog);
					AssertEquals(packageId + "|FAC=CFS|LOC=Sydney|TYP=Finalised|WHS=TRW", packageStateLog.SL_Reference);
				}
			}
		}

		#endregion

		#region TestWDH_SignedByOptional

		public void TestWDH_SignedByOptional()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dtu = Helper.CreateDispatchTransportationUnit("D2", warehouse.PK);
			dtu.RunPreSaveValidation();
			AssertNoErrors(dtu.WDH_SignedByInfo);
		}

		#endregion

		#region TestFinalise

		[TestDate(2021, 1, 1)]
		public void TestFinalise()
		{
			var now = ZDateTimeOffset.Now;
			var dateTimeOffsetNowWithoutSeconds = new ZDateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0, now.Offset);

			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			Factory.Save();

			dtu.Finalise(ZDateTimeOffset.Empty, "Test");
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.FreightLoaded);

			dtu.Finalise(ZDateTimeOffset.Invalid, "Test");
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.FreightLoaded);

			dtu.Finalise(dateTimeOffsetNowWithoutSeconds, "Test");
			AssertEquals("Cannot finalise package state if DTU is not gate out", packageState.WPS_Status, TransitWarehouseStatuses.Codes.FreightLoaded);

			dtu.WDH_GateOutTime = dateTimeOffsetNowWithoutSeconds.AddHours(1);
			dtu.Finalise(dateTimeOffsetNowWithoutSeconds, "Test");
			AssertEquals("Cannot finalise package state if DTU Gate Out time is in the future", packageState.WPS_Status, TransitWarehouseStatuses.Codes.FreightLoaded);

			dtu.WDH_GateInTime = dateTimeOffsetNowWithoutSeconds.AddHours(-2);
			dtu.WDH_LoadCompleteTime = dateTimeOffsetNowWithoutSeconds.AddHours(-1);
			dtu.WDH_GateOutTime = dateTimeOffsetNowWithoutSeconds.AddHours(-1);

			dtu.Finalise(dateTimeOffsetNowWithoutSeconds, "Test");
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.Finalized);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
			var finalizeLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ItemDocumentJobFinalised.Code);
			var packageLog = packageState.Package.Logs.Find(finalizeLogQuery).SingleOrDefault();
			AssertNotNull("Should create finalise log for package", packageLog);
			AssertEquals("P1|FAC=CFS|TYP=Finalised|WHS=WHS", packageLog.SL_Reference);

			var dtuLog = dtu.Logs.Find(logQuery).SingleOrDefault();
			AssertNotNull("Should create time out log for dtu", dtuLog);
			AssertEquals("DTU1|RES=Test|TYP=Finalised", dtuLog.SL_Reference);
		}

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWhenUpdateVehicleToFinalisedAlsoUpdateLoadedContainerlizedDTUAndTheirInners()
		{
			var now = ZDateTimeOffset.Now;
			var dateTimeOffsetNowWithoutSeconds = new ZDateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0, now.Offset);
			var warehouse = Helper.CreateTRWWarehouse("TRW");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			warehouse.WarehouseAddress.OA_City = "Sydney";

			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var packageStatePKG = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			var (uld, package) = Helper.CreateDispatchTransportationUnitWithPackage("ULD", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);

			var packageStateULD = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.FreightLoaded, receiveConsignment: rcn, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);
			packageStateULD.WPS_UnitType = "ULD";

			var packageStatePKG11 = Helper.CreatePackageState(rcn, 1, "PKG", "P11", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: uld, dispatchLoadList: dll2);
			packageStatePKG11.WPS_UnitType = "PKG";

			Helper.PackPackageIntoHandlingUnit(packageStateULD, packageStatePKG11, ZDateTimeOffset.Now, "AAA", packageStateULD);

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU1", handlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, dcn: dcn, dll: dll2, dtu: uld);
			var handlingUnitPackage2 = Helper.CreateHandlingUnitPackage("HU2", handlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, dcn: dcn, dll: dll, dtu: dtu);
			var subHandlingUnitPackage = Helper.CreateHandlingUnitPackage("HU11", handlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, dcn: dcn, dll: dll2, dtu: uld);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "P111", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: uld);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "P222", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: uld);
			var childPackage3 = Helper.CreatePackageState(rcn, 1, "PKG", "P333", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: uld);
			var childPackageNotLoaded = Helper.CreatePackageState(rcn, 1, "PKG", "P444", TransitWarehouseStatuses.Codes.Putaway, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2);
			var childPackage4 = Helper.CreatePackageState(rcn, 1, "PKG", "P44", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			childPackageNotLoaded.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Secured;

			Helper.DisableTopLevelHUFKForTest(TestConnection);

			Helper.PackPackageIntoHandlingUnit(packageStateULD, handlingUnitPackage, ZDateTimeOffset.Now, "AAA", packageStateULD);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: packageStateULD);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, subHandlingUnitPackage, ZDateTimeOffset.Now, "AAA", topHandlingUnit: packageStateULD);
			Helper.PackPackageIntoHandlingUnit(subHandlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: packageStateULD);
			Helper.PackPackageIntoHandlingUnit(subHandlingUnitPackage, childPackage3, ZDateTimeOffset.Now, "AAA", topHandlingUnit: packageStateULD);
			Helper.PackPackageIntoHandlingUnit(subHandlingUnitPackage, childPackageNotLoaded, ZDateTimeOffset.Now, "AAA", topHandlingUnit: packageStateULD);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage2, childPackage4, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage2);

			Factory.Save();

			AssertEquals("Precondition: DTU gate out time is empty.", true, dtu.WDH_GateOutTime.IsEmpty);

			AssertPackageStatusChanged(TransitWarehouseStatuses.Codes.FreightLoaded, packageStatePKG, packageStateULD, packageStatePKG11, handlingUnitPackage, handlingUnitPackage2, subHandlingUnitPackage, childPackage1, childPackage2, childPackage3, childPackage4);

			dtu.WDH_GateInTime = dateTimeOffsetNowWithoutSeconds.AddHours(-2);
			dtu.WDH_LoadCompleteTime = dateTimeOffsetNowWithoutSeconds.AddHours(-1);
			dtu.WDH_GateOutTime = dateTimeOffsetNowWithoutSeconds.AddHours(-1);
			uld.WDH_GateInTime = dtu.WDH_GateInTime;
			uld.WDH_LoadCompleteTime = dtu.WDH_LoadCompleteTime;
			uld.WDH_GateOutTime = dtu.WDH_GateOutTime;

			dtu.Finalise(dateTimeOffsetNowWithoutSeconds, "Test");

			Factory.Save();

			AssertEquals("DTU gate out time should be set.", false, dtu.WDH_GateOutTime.IsEmpty);
			AssertNotNull(uld.WDH_GateOutTime);

			AssertPackageStatusChanged(TransitWarehouseStatuses.Codes.Finalized, packageStatePKG, packageStateULD, packageStatePKG11, handlingUnitPackage, handlingUnitPackage2, subHandlingUnitPackage, childPackage1, childPackage2, childPackage3, childPackageNotLoaded, childPackage4);
			AssertPackageDTUAndLoadedTime(dtu.PK, packageStatePKG, packageStateULD, childPackage4);
			AssertPackageDTUAndLoadedTime(uld.PK, packageStatePKG11, childPackage1, childPackage2, childPackage3, childPackageNotLoaded);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);

			AssertLogs(logQuery, TransitWarehouseStatuses.Codes.Finalized, [dtu, uld]);
			AssertLogs(logQuery, TransitWarehouseStatuses.Codes.Finalized, packageStates: [packageStatePKG, packageStateULD, packageStatePKG11, handlingUnitPackage, handlingUnitPackage2, subHandlingUnitPackage, childPackage1, childPackage2, childPackage3, childPackageNotLoaded, childPackage4]);
		}

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWhenUpdateContainerDTUToFinalisedShouldNotUpdateLoadTime()
		{
			var now = ZDateTimeOffset.Now;
			var dateTimeOffsetNowWithoutSeconds = new ZDateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0, now.Offset);
			var warehouse = Helper.CreateTRWWarehouse("TRW");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			warehouse.WarehouseAddress.OA_City = "Sydney";

			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var (uld, package) = Helper.CreateDispatchTransportationUnitWithPackage("ULD", warehouse.PK, dtuUnitType: "CNT");

			var packageStatePKG = Helper.CreatePackageState(rcn, 1, "PKG", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: uld, dispatchLoadList: dll, location: stageLocation);
			packageStatePKG.WPS_SecurityStatus = "SCR";

			var packageStateULD = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Packed, location: stageLocation);
			packageStateULD.WPS_UnitType = "CNT";
			packageStateULD.WPS_IsHandlingUnit = true;
			packageStateULD.WPS_WW_Warehouse = warehouse.PK;
			packageStateULD.WPS_SecurityStatus = "NOT";
			uld.WDH_GateInTime = dateTimeOffsetNowWithoutSeconds.AddHours(-2);
			uld.WDH_LoadCompleteTime = dateTimeOffsetNowWithoutSeconds.AddHours(-1);

			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(packageStateULD, packageStatePKG, ZDateTimeOffset.Now, "AAA", packageStateULD);

			Factory.Save();

			AssertEquals("Precondition: DTU gate out time is empty.", true, uld.WDH_GateOutTime.IsEmpty);

			AssertPackageStatusChanged(TransitWarehouseStatuses.Codes.FreightLoaded, packageStatePKG);
			uld.WDH_GateOutTime = dateTimeOffsetNowWithoutSeconds.AddHours(-1);

			uld.Finalise(dateTimeOffsetNowWithoutSeconds, "Test");

			Factory.Save();

			AssertEquals("DTU gate out time should be set.", false, uld.WDH_GateOutTime.IsEmpty);
			AssertPackageStatusChanged(TransitWarehouseStatuses.Codes.Finalized, packageStatePKG, packageStateULD);
			AssertPackageDTUAndLoadedTime(uld.PK, packageStatePKG);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);

			AssertLogs(logQuery, TransitWarehouseStatuses.Codes.Finalized, [uld]);
			AssertLogs(logQuery, TransitWarehouseStatuses.Codes.Finalized, packageStates: [packageStatePKG]);
		}

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestWhenUpdateContainerDTUToFinalisedShouldUpdateGateOutTimeForEmptyULD()
		{
			var now = ZDateTimeOffset.Now;
			var dateTimeOffsetNowWithoutSeconds = new ZDateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0, now.Offset);
			var warehouse = Helper.CreateTRWWarehouse("TRW");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			warehouse.WarehouseAddress.OA_City = "Sydney";

			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var (uld, package) = Helper.CreateDispatchTransportationUnitWithPackage("ULD", warehouse.PK, dtuUnitType: "ULD");

			var packageStateULD = Helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Packed, location: stageLocation, dispatchLoadList: dll);
			packageStateULD.WPS_UnitType = "CNT";
			packageStateULD.WPS_IsHandlingUnit = true;
			packageStateULD.WPS_WW_Warehouse = warehouse.PK;
			packageStateULD.WPS_SecurityStatus = "REQ";
			uld.WDH_GateInTime = dateTimeOffsetNowWithoutSeconds.AddHours(-2);
			uld.WDH_LoadCompleteTime = dateTimeOffsetNowWithoutSeconds.AddHours(-1);

			Factory.Save();

			AssertEquals("Precondition: DTU gate out time is empty.", true, uld.WDH_GateOutTime.IsEmpty);
			uld.WDH_GateOutTime = dateTimeOffsetNowWithoutSeconds.AddHours(-1);

			uld.Finalise(dateTimeOffsetNowWithoutSeconds, "Test");

			Factory.Save();

			AssertEquals("DTU gate out time should be set.", false, uld.WDH_GateOutTime.IsEmpty);
			AssertEquals("ULD's DTU should be empty.", true, packageStateULD.WPS_WDH_TransitDispatchHeader.IsEmpty);
			AssertPackageStatusChanged(TransitWarehouseStatuses.Codes.Finalized, packageStateULD);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);

			AssertLogs(logQuery, TransitWarehouseStatuses.Codes.Finalized, [uld]);
		}

		[TestDate(2021, 1, 1)]
		public void TestFinalise_GateOutTimeAndFinalisedTimeSame()
		{
			var now = ZDateTimeOffset.Now;
			var dateTimeOffsetNowWithoutSeconds = new ZDateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0, now.Offset);

			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			dtu.WDH_GateInTime = dateTimeOffsetNowWithoutSeconds.AddHours(-2);
			dtu.WDH_LoadCompleteTime = dateTimeOffsetNowWithoutSeconds.AddHours(-1);
			dtu.WDH_GateOutTime = dateTimeOffsetNowWithoutSeconds;

			Factory.Save();

			dtu.Finalise(dateTimeOffsetNowWithoutSeconds, "Test");
			AssertEquals(packageState.WPS_Status, TransitWarehouseStatuses.Codes.Finalized);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.StatusUpdated.Code);
			var finalizeLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ItemDocumentJobFinalised.Code);
			var packageLog = packageState.Package.Logs.Find(finalizeLogQuery).SingleOrDefault();
			AssertNotNull("Should create finalise log for package", packageLog);
			AssertEquals("P1|FAC=CFS|TYP=Finalised|WHS=WHS", packageLog.SL_Reference);

			var dtuLog = dtu.Logs.Find(logQuery).SingleOrDefault();
			AssertNotNull("Should create time out log for dtu", dtuLog);
			AssertEquals("DTU1|RES=Test|TYP=Finalised", dtuLog.SL_Reference);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var itemDispatchTransportationUnit = Factory.New<WhsItemDispatchTransportationUnit>();
			AssertEquals("Dispatch Transportation Unit", itemDispatchTransportationUnit.HumanReadableName);

			itemDispatchTransportationUnit.WDH_ReferenceNumber = "189";
			AssertEquals("Dispatch Transportation Unit 189", itemDispatchTransportationUnit.HumanReadableName);
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var unit = Factory.New<WhsItemDispatchTransportationUnit>();
			AssertEquals(Constants.DocManagerCodes.TransitDispatchTransportationUnit, ((IDocManagerSupport)unit).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region TestIWorkflowProvider Members

		public void TestIWorkflowProviderMembers()
		{
			var header = Factory.New<WhsItemDispatchTransportationUnit>();
			var newWorkflowItem = header.WorkflowItems.AddNew();
			var workflowProvider = (IWorkflowProvider)header;

			AssertEquals(WorkflowDescriptors.TransitDispatchTransportationUnit, workflowProvider.WorkflowType);
			AssertContainsExactElementsInAnyOrder(new[] { newWorkflowItem }, workflowProvider.WorkflowItems);
			AssertEquals(typeof(ColumnValueRanker), workflowProvider.GetTemplateSelectionCriteria().GetType());
			AssertNull(workflowProvider.GetWorkflowInformationProvider());
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var intendedWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			var dispatchTransportationUnitWithWarehouse = Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>();
			dispatchTransportationUnitWithWarehouse.WDH_WW_Warehouse = intendedWarehouse.PK;
			Factory.Save();

			var criteriaForDispatchTransportationUnitWithWarehouse = (ColumnValueRanker)((IWorkflowProvider)dispatchTransportationUnitWithWarehouse).GetTemplateSelectionCriteria();
			AssertContainsExactElementsInAnyOrder(new[] { intendedWarehouse.PK, ZGuid.Empty }, criteriaForDispatchTransportationUnitWithWarehouse.GetValues(ProcessTaskTemplateSchema.P0_WW));
		}

		#endregion

		#region	IProcessHandlingInfoProvider Members

		public void TestIProcessHandlingInfoProvider()
		{
			var infoProvider = (IProcessHandlingInfoProvider)Factory.New<WhsItemDispatchTransportationUnit>();
			AssertEquals(typeof(WhsItemDispatchTransportationUnitProcessHandlingInfoProvider), infoProvider.ProcessHandlingInfo.GetType());
		}

		#endregion

		#region TestIPackingParentMembers

		public void TestIPackingParentMembers()
		{
			var dtu = Factory.New<WhsItemDispatchTransportationUnit>();
			dtu.WDH_ReferenceNumber = "DTU1";
			IPackingParent packingParent = dtu;
			AssertEquals(null, packingParent.ControllerID);
			AssertEquals(DocumentOptions.ShowBasicLabelOnly, packingParent.DocumentOptions);
			AssertEquals(false, packingParent.IsAutoPrintAllowed);
			AssertEquals(false, packingParent.IsParentJobFinalised);
			AssertEquals(false, packingParent.IsScanEventsVisible);
			AssertEquals("", packingParent.JobDescription);
			AssertEquals("DTU1", packingParent.JobNo);
			AssertEquals(null, packingParent.CarrierBookingAgent);
			AssertEquals("", packingParent.TransportReference);
			AssertEquals(false, packingParent.IsLoosePackageIDsSupported);
			AssertEquals(ParentJobType.None, packingParent.ParentJobType);
			AssertEquals(PackageSequenceType.Outer, packingParent.PackageSequenceType);
			AssertEquals(false, packingParent.IsPackingJobReadOnly);
			AssertEquals("NotificationTypeForInvalidContainerNumber should be correct", NotificationTypes.None, packingParent.NotificationTypeForInvalidContainerNumber);
		}

		#endregion

		#region TestIPackingParentWithOutturn Members

		public void TestIPackingParentWithOutturn_OutturnProvider()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)dispatchTransportationUnit;
			var package = Helper.CreatePackage(dispatchTransportationUnit.PackageJob, "P1", 1, "PLT");

			var outturnProvider = packingParentWithOutturn.GetOutturnProvider(package);
			AssertNull(outturnProvider);
		}

		public void TestIPackingParentWithOutturn_GetParentContainer()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)dispatchTransportationUnit;
			var package = Helper.CreatePackage(dispatchTransportationUnit.PackageJob, "P1", 1, "PLT");

			var containerPair = packingParentWithOutturn.GetParentContainer(package);
			AssertNotNull(containerPair);
			AssertNull(containerPair.Container);
			AssertNull(containerPair.ContainerNumber);
		}

		[TestDate(2020, 08, 28, 04, 39, 50)]
		public void TestIPackingParentWithOutturn_ContainerView()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)dispatchTransportationUnit;
			var currentTime = ZDateTimeOffset.Now;
			dispatchTransportationUnit.WDH_LoadCompleteTime = currentTime;
			var container = dispatchTransportationUnit.Container;

			var containerView = packingParentWithOutturn.GetContainerView(container);

			AssertNotNull(containerView);
			AssertEquals(currentTime.ToZDateTime(), containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		public void TestIPackingParentWithOutturn_ContainerView_NoDispatchTransportationUnit()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)dispatchTransportationUnit;
			var package = receiveConsignment.PackageJob.Containers.AddNew();

			var containerView = packingParentWithOutturn.GetContainerView(package.Container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		public void TestIPackingParentWithOutturn_ContainerView_InvalidDate()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			dispatchTransportationUnit.WDH_LoadCompleteTime = ZDateTimeOffset.Invalid;
			var packingParentWithOutturn = (IPackingParentWithOutturn)dispatchTransportationUnit;
			var container = dispatchTransportationUnit.Container;

			var containerView = packingParentWithOutturn.GetContainerView(container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		public void TestIPackingParentWithOutturn_ContainerView_NoDates()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)dispatchTransportationUnit;
			var container = dispatchTransportationUnit.Container;

			var containerView = packingParentWithOutturn.GetContainerView(container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		#endregion

		#region TestIPackingParent_ShouldPackTrackedPackagesViaDivot

		public void TestIPackingParent_ShouldPackTrackedPackagesViaDivot()
		{
			var dtu = Factory.New<WhsItemDispatchTransportationUnit>();
			IPackingParent packingParent = dtu;
			AssertEquals(false, packingParent.ShouldPackTrackedPackagesViaDivot);
		}

		#endregion

		#region IJobCostingPlugIn Member

		public void TestIJobCostingPlugIn()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);

			AssertEquals("JK_UniqueConsignRef", "DTU1", dtu.JK_UniqueConsignRef);
			AssertEquals("ConsolExchangeRate", 0m, dtu.ConsolExchangeRate);
			AssertEquals("ContainerMode", ZString.Empty, dtu.ContainerMode);
			AssertEquals("ConsolType", ZString.Empty, dtu.ConsolType);
			AssertEquals("Direction", ZString.Empty, dtu.Direction);
			AssertEquals("Module", ApportionmentMethodModules.TransitWarehouse, dtu.Module);
			AssertEquals("ExchangeRateForCurrency", 0m, dtu.ExchangeRateForCurrency(null, ZGuid.Empty));
			AssertEquals("GetPrepaidCollect", ZString.Empty, dtu.GetPrepaidCollect(null));

			AssertNull("LoadPort", dtu.LoadPort);
			AssertNull("DischargePort", dtu.DischargePort);
			AssertNull("ProfitLossContainer", dtu.ProfitLossContainer);
			AssertNull("ConsolCurrency", dtu.ConsolCurrency);
			AssertNull("ReceivingAgent", dtu.ReceivingAgent);
			AssertNull("ReceivingAgentAPInvoicingParty", dtu.ReceivingAgentAPInvoicingParty);
			AssertNull("ReceivingAgentARInvoicingParty", dtu.ReceivingAgentARInvoicingParty);
			AssertNull("SendingAgent", dtu.SendingAgent);
			AssertNull("SendingAgentAPInvoicingParty", dtu.SendingAgentAPInvoicingParty);
			AssertNull("SendingAgentARInvoicingParty", dtu.SendingAgentARInvoicingParty);
			AssertNull("PrepaidCollectList", dtu.PrepaidCollectList);

			AssertNotNull("costSupporter", dtu.CostSupporter);
			Assert("IsMasterCollect", !dtu.IsMasterCollect);
		}

		#endregion

		#region ITransportationUnitForCost Member

		public void TestDispatchConsignments()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			var dcn3 = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);

			Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn1, dtu1, dll);
			Helper.CreatePackageState(rcn, 1, "BOX", "P2", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn2, dtu1, dll);
			Helper.CreatePackageState(rcn, 1, "BOX", "P3", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn2, dtu1, dll);
			Helper.CreatePackageState(rcn, 1, "BOX", "P4", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn3, dtu2, dll);

			AssertContainsExactElementsInAnyOrder(new IJobInvoicingPlugIn[] { dcn1, dcn2 }, dtu1.Consignments);
			AssertContainsExactElementsInAnyOrder(new IJobInvoicingPlugIn[] { dcn3 }, dtu2.Consignments);
		}

		public void TestCreditorPK()
		{
			var transportCompany = Helper.CreateClient("TRC");
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);

			AssertEquals(transportCompany.PK, dtu.CreditorPK);
		}

		public void TestCreditorPK_TransportCompanyIsOverridden()
		{
			var transportCompany = Helper.CreateClient("TRC");
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);

			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			dtu.TransportCompany.E2_AddressOverride = true;
			dtu.TransportCompany.E2_CompanyName = "Transport Co";

			AssertEquals(ZGuid.Empty, dtu.CreditorPK);
		}

		public void TestDefaultChargeGroups()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);

			AssertContainsExactElementsInAnyOrder(new ZString[] { ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit }, dtu.DefaultChargeGroups);
		}

		public void TestMasterBillNum()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);

			AssertEquals(ZString.Empty, dtu.MasterBillNum);

			var additionalReference = dtu.AdditionalReferenceNumbers.AddNew();
			additionalReference.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReference.CE_EntryNum = "MAB Test";

			AssertEquals("MAB Test", dtu.MasterBillNum);
		}

		public void TestETAAndETD()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dcn, dtu, dll);

			AssertEquals(ZDateTime.Empty, dtu.ETA);
			AssertEquals(ZDateTime.Empty, dtu.ETD);
		}

		#endregion

		#region ITransitTransportationUnitForRating Member

		public void TestITransportJobForRating()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;

			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = now.AddHours(-1);
			dtu.WDH_LoadCompleteTime = now;

			var transportCompany = Helper.CreateClient("TRC");
			Helper.CreateJobDocAddressFromAddress(dtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			var loadedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var loadedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			Factory.Save();

			var transitRating = (ITransitTransportationUnitForRating)dtu;
			AssertEquals(dtu.TransportCompany, transitRating.TransportCompanyDocAddress);
			AssertEquals(ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit, transitRating.ChargeCodeGroup);
			AssertEquals(dtu.WDH_GateInTime.ToLocalZDateTime(), transitRating.ExpectedArrivalDate);
			AssertEquals(dtu.WDH_LoadCompleteTime.ToLocalZDateTime(), transitRating.ExpectedDepartureDate);
			AssertNotNull(transitRating.ContainerForRating);
			AssertEquals("20GP", transitRating.ContainerForRating.Value.Key.ContainerType.RC_Code);
			AssertEquals("IsOnPallets", true, transitRating.ContainerForRating.Value.Value);
		}

		public void TestITransportJobForRating_IsOnPallets_WithContainerizedDTUAsPackage()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var now = ZDateTimeOffset.Today;
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;

			var vehDTU = helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK, containerID: "DROP1", containerType: "DROP");
			vehDTU.WDH_GateInTime = now.AddHours(-1);
			vehDTU.WDH_LoadCompleteTime = now;

			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, containerID: "20GP1", containerType: "20GP");
			dtu.WDH_GateInTime = now.AddHours(-1);
			dtu.WDH_LoadCompleteTime = now;

			var dtu2 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU3", warehouse.PK, containerID: "AAA1", containerType: "AAA");
			dtu2.WDH_GateInTime = now.AddHours(-1);
			dtu2.WDH_LoadCompleteTime = now;

			var loadedPackageState1 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: vehDTU);
			var loadedPackageState2 = Helper.CreatePackageState(rcn, 1, Constants.PkgUnit.Pallet, "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: vehDTU);
			Factory.Save();

			var dtuPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtu.PackageExtension.KPN_KP_Package)).FirstOrDefault();
			dtuPackageState.WPS_LoadedTime = now;
			dtuPackageState.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
			dtuPackageState.WPS_WDH_TransitDispatchHeader = vehDTU.PK;
			dtuPackageState.WPS_WDL_LoadList = dll.PK;
			dtuPackageState.WPS_IsSecure = true;
			dtuPackageState.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Secured;

			var dtuPackageState2 = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, dtu2.PackageExtension.KPN_KP_Package)).FirstOrDefault();
			dtuPackageState2.WPS_LoadedTime = now;
			dtuPackageState2.WPS_Status = TransitWarehouseStatuses.Codes.FreightLoaded;
			dtuPackageState2.WPS_WDH_TransitDispatchHeader = vehDTU.PK;
			dtuPackageState2.WPS_WDL_LoadList = dll.PK;
			dtuPackageState2.WPS_IsSecure = true;
			dtuPackageState2.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Secured;
			Factory.Save();

			var transitRating = (ITransitTransportationUnitForRating)vehDTU;
			AssertEquals(ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit, transitRating.ChargeCodeGroup);
			AssertNotNull(transitRating.ContainerForRating);
			AssertEquals("DROP", transitRating.ContainerForRating.Value.Key.ContainerType.RC_Code);
			AssertEquals("IsOnPallets", true, transitRating.ContainerForRating.Value.Value);
		}

		#region TestITransportJobForRating_FreightMode

		public void TestITransportJobForRating_FreightMode_OneDispatchLoadList()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			var loadedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var loadedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			Factory.Save();

			var transitRating = (ITransitJobForRating)dtu;
			AssertEquals(FreightMode.AIR, transitRating.FreightMode);
		}

		public void TestITransportJobForRating_FreightMode_OneDispatchLoadListHasNoTransportMode()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var loadedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var loadedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			Factory.Save();

			var transitRating = (ITransitJobForRating)dtu;
			AssertNull(transitRating.FreightMode);
		}

		public void TestITransportJobForRating_FreightMode_MultipleDispatchLoadListsWithSameTransportMode()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			dll2.WDL_TransportMode = TransportModes.Air;
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu.PK);

			var loadedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var loadedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: dtu);
			Factory.Save();

			var transitRating = (ITransitJobForRating)dtu;
			AssertEquals(FreightMode.AIR, transitRating.FreightMode);
		}

		public void TestITransportJobForRating_FreightMode_MultipleDispatchLoadListsWithDifferentTransportModes()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			dll2.WDL_TransportMode = TransportModes.Sea;
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu.PK);

			var loadedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var loadedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: dtu);
			Factory.Save();

			var transitRating = (ITransitJobForRating)dtu;
			AssertEquals(FreightMode.UKN, transitRating.FreightMode);
		}

		public void TestITransportJobForRating_FreightMode_OneOfDispatchLoadListHasNoTransportModes()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu.PK);

			var loadedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var loadedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: dtu);
			Factory.Save();

			var transitRating = (ITransitJobForRating)dtu;
			AssertEquals(FreightMode.AIR, transitRating.FreightMode);
		}

		public void TestITransportJobForRating_FreightMode_DispatchTransportationUnitIsContainer()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnitWithContainerType("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			dll.WDL_TransportMode = TransportModes.Air;
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu.PK);

			AssertEquals("Precondition: TransportMode of Container is SEA", TransportModes.Sea, dtu.Container.ContainerType.RC_ShippingMode);

			var loadedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var loadedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll2, dispatchUnit: dtu);
			Factory.Save();

			var transitRating = (ITransitJobForRating)dtu;
			AssertEquals(FreightMode.FCL, transitRating.FreightMode);
		}

		#endregion

		#region TestTransitPackagesForRating

		[TestDate(2022, 6, 10)]
		public void TestTransitPackagesForRating()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var undgSubstance = Helper.CreateUNDGSubstance("BCDE", "1.5D", "BCDEa");
			undgSubstance.DG_ExceptedQuantityCode = "E1";

			var now = ZDateTimeOffset.Now;
			var unloadedTime = now.AddDays(-5);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var arrivedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu, dispatchConsignment: dcn);
			arrivedPackageState.WPS_UnloadedTime = unloadedTime;
			arrivedPackageState.WPS_UnloadedNotYetProcessedTime = arrivedPackageState.WPS_UnloadedTime;

			var loadedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			loadedPackageState.WPS_UnloadedTime = unloadedTime;
			loadedPackageState.WPS_UnloadedNotYetProcessedTime = loadedPackageState.WPS_UnloadedTime;
			loadedPackageState.WPS_LoadedTime = now;
			loadedPackageState.Package.KP_RH_NKCommodityCode = "HAZ";
			loadedPackageState.Package.KP_Weight = 10m;
			loadedPackageState.Package.KP_WeightUQ = Constants.Weight.Kilograms;
			loadedPackageState.Package.KP_Volume = 5m;
			loadedPackageState.Package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			loadedPackageState.Package.KP_RH_NKCommodityCode = "HAZ";
			var undgDataItem = Helper.CreateUNDGDataItem(loadedPackageState.Package.PK, loadedPackageState.Package.TablePrefix, undgSubstance, 2, 3);
			loadedPackageState.Package.UNDGs.Add(undgDataItem);
			var bookedPackageState = Helper.CreatePackageState(rcn, 10, "PKG", "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			Factory.Save();

			var transitPackagesForRating = ((ITransitJobForRating)dtu).TransitPackagesForRating;
			AssertEquals(1, transitPackagesForRating.Count());

			var transitPackageForRating2 = transitPackagesForRating.FirstOrDefault(p => p.PK == loadedPackageState.PK);
			AssertTransitPackageForRating(transitPackageForRating2, loadedPackageState, true);
		}

		[TestDate(2022, 6, 10)]
		public void TestTransitPackagesForRating_HandlingUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var completeTime = ZDateTimeOffset.Now.AddDays(-5);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var loadedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			loadedPackageState.WPS_UnloadedTime = completeTime;
			loadedPackageState.WPS_UnloadedNotYetProcessedTime = loadedPackageState.WPS_UnloadedTime;
			loadedPackageState.WPS_LoadedTime = now;

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU-1", handlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, dcn: dcn, dll: dll, dtu: dtu);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Factory.Save();

			var transitPackagesForRating = ((ITransitJobForRating)dtu).TransitPackagesForRating;
			AssertEquals(2, transitPackagesForRating.Count());

			var transitPackageForRating1 = transitPackagesForRating.FirstOrDefault(p => p.PK == loadedPackageState.PK);
			AssertTransitPackageForRating(transitPackageForRating1, loadedPackageState, false);

			var transitPackageForRating2 = transitPackagesForRating.FirstOrDefault(p => p.PK == handlingUnitPackage.PK);
			AssertTransitPackageForRating(transitPackageForRating2, handlingUnitPackage, false);
		}

		[TestDate(2022, 6, 10)]
		public void TestTransitPackagesForRating_OverpackageHandlingUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var now = ZDateTimeOffset.Now;
			var completeTime = ZDateTimeOffset.Now.AddDays(-5);
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var dcn = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var loadedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			loadedPackageState.WPS_UnloadedTime = completeTime;
			loadedPackageState.WPS_UnloadedNotYetProcessedTime = loadedPackageState.WPS_UnloadedTime;
			loadedPackageState.WPS_LoadedTime = now;

			var overpackHandlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU-1", overpackHandlingUnit, rtu, TransitWarehouseStatuses.Codes.FreightLoaded, unitType: PackageStateUnitType.Codes.Overpack, rcn: rcn, dcn: dcn, dll: dll, dtu: dtu);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Factory.Save();

			var transitPackagesForRating = ((ITransitJobForRating)dtu).TransitPackagesForRating;
			AssertEquals(2, transitPackagesForRating.Count());

			var transitPackageForRating1 = transitPackagesForRating.FirstOrDefault(p => p.PK == loadedPackageState.PK);
			AssertTransitPackageForRating(transitPackageForRating1, loadedPackageState, false);

			var transitPackageForRating2 = transitPackagesForRating.FirstOrDefault(p => p.PK == handlingUnitPackage.PK);
			AssertTransitPackageForRating(transitPackageForRating2, handlingUnitPackage, false);
		}

		void AssertTransitPackageForRating(TransitPackageForRatingInfo transitPackageForRating, WhsItemPackageState packageState, bool hasDG)
		{
			CombineAssertions(() =>
			{
				AssertNotNull(transitPackageForRating);
				AssertEquals("Package State PK", packageState.PK, transitPackageForRating.PK);
				AssertEquals("Package Qty", packageState.Package.KP_PackageQty, transitPackageForRating.PackageQty);
				AssertEquals("Package Type", packageState.Package.KP_F3_NKPackType, transitPackageForRating.PackageType);
				AssertEquals("Commodity Code", packageState.Package.KP_RH_NKCommodityCode, transitPackageForRating.CommodityCode);
				AssertEquals("Weight", packageState.Package.KP_Weight, transitPackageForRating.Weight);
				AssertEquals("Weight UQ", packageState.Package.KP_WeightUQ, transitPackageForRating.WeightUQ);
				AssertEquals("Volume", packageState.Package.KP_Volume, transitPackageForRating.Volume);
				AssertEquals("Volume UQ", packageState.Package.KP_VolumeUQ, transitPackageForRating.VolumeUQ);
				AssertEquals("Unloaded Time", packageState.WPS_UnloadedTime, transitPackageForRating.UnloadedTime);
				AssertEquals("Loaded Time", packageState.WPS_LoadedTime, transitPackageForRating.LoadedTime);
				AssertEquals("Has Dangerous Goods", hasDG, transitPackageForRating.HasDangerousGoods);
			});
		}

		#endregion

		#endregion

		#region TestIWhsItemDispatchTransportationUnit Members

		public void TestIWhsItemDispatchTransportationUnit_Warehouse()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnit("RTU1", warehouse.PK);

			var dtuAsInterface = (IWhsItemDispatchTransportationUnit)dispatchTransportationUnit;
			AssertEquals(dtuAsInterface.Warehouse, warehouse);
		}

		public void TestIWhsItemDispatchTransportationUnit_UnitType()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dispatchTransportationUnit = Helper.CreateDispatchTransportationUnit("RTU1", warehouse.PK);

			var dtuAsInterface = (IWhsItemDispatchTransportationUnit)dispatchTransportationUnit;
			AssertEquals(dtuAsInterface.UnitType, dispatchTransportationUnit.WDH_UnitType);
		}

		public void TestIWhsItemDispatchTransportationUnit_LatestDispatchConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, stageLocation.PK);

			var dcn1 = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			dcn1.WDC_SystemCreateTimeUtc = DateTime.UtcNow;
			var dcn2 = Helper.CreateDispatchConsignment("DCN2", warehouse.PK);
			dcn2.WDC_SystemCreateTimeUtc = DateTime.UtcNow.AddDays(1);
			var dcn3 = Helper.CreateDispatchConsignment("DCN3", warehouse.PK);
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dtu2 = Helper.CreateDispatchTransportationUnit("DTU2", warehouse.PK);
			var dtu3 = Helper.CreateDispatchTransportationUnit("DTU3", warehouse.PK);

			Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu1, dcn1, dtu1);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Arrived, rtu1, dcn2, dtu1);
			Helper.CreatePackageState(rcn3, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Arrived, rtu2, dcn3, dtu2);

			var dtu1AsInterface = (IWhsItemDispatchTransportationUnit)dtu1;
			var dtu2AsInterface = (IWhsItemDispatchTransportationUnit)dtu2;
			var dtu3AsInterface = (IWhsItemDispatchTransportationUnit)dtu3;

			AssertEquals(dcn2, dtu1AsInterface.LatestDispatchConsignment);
			AssertEquals(dcn3, dtu2AsInterface.LatestDispatchConsignment);
			AssertNull(dtu3AsInterface.LatestDispatchConsignment);
		}

		#endregion

		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var dtu = Factory.New<WhsItemDispatchTransportationUnit>();
			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] {
				PredefinedNoteTypes.Instance.AutoRatingAuditLog,
				PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes,
				PredefinedNoteTypes.Instance.UnmatchedOrgDetails,
				PredefinedNoteTypes.Instance.UnloadLoadNotes }, dtu.NoteTypes);
		}

		#endregion

		#region TestCreateJobHeader

		public void TestCreateJobHeader()
		{
			var dtu = Factory.New<WhsItemDispatchTransportationUnit>();
			var header = new JobHeader.Loader(dtu).TryLoadOrCreate();
			AssertEquals(header, dtu.JobHeader);
		}

		#endregion

		#region TestValidPackageStatesInPendingDLLs

		public void TestValidPackageStatesInPendingDLLs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll1.PK, dtu1.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll1);

			Factory.Save();

			var packageStates = dtu1.ValidPackageStatesInPendingDLLs;
			AssertEquals("Expected there to be one package state in the dll1", 1, packageStates.Count());
			AssertCollectionContains("Expected P1 in dll1", wps1, packageStates);
		}

		public void TestValidPackageStatesInPendingDLLs_WhenDLLHasCompleteTime_ThenExcludeDLL()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			var dll3 = Helper.CreateDispatchLoadList("DLL3", warehouse.PK);
			dll3.WDL_CompleteTime = ZDateTimeOffset.Now;

			Helper.CreateDispatchDLLDTUPivot(dll1.PK, dtu1.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu1.PK);
			Helper.CreateDispatchDLLDTUPivot(dll3.PK, dtu1.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll1);
			var wps2 = Helper.CreatePackageState(rcn1, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll1);
			var wps3 = Helper.CreatePackageState(rcn1, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll2);
			var wps4 = Helper.CreatePackageState(rcn1, 1, "PKG", "P4", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll3);
			var wps5 = Helper.CreatePackageState(rcn1, 1, "PLT", "P5", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll3);

			Factory.Save();

			var packageStates = dtu1.ValidPackageStatesInPendingDLLs;
			AssertEquals("Expected there to be three package states in the dll1 and dll2 together", 3, packageStates.Count());
			AssertCollectionContains("Expected P1 in dll1", wps1, packageStates);
			AssertCollectionContains("Expected P2 in dll1", wps2, packageStates);
			AssertCollectionContains("Expected P3 in dll2", wps3, packageStates);
		}

		public void TestValidPackageStatesInPendingDLLs_WhenDLLIsInactive_ThenExcludeDLL()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			dll2.WDL_IsActive = false;

			Helper.CreateDispatchDLLDTUPivot(dll1.PK, dtu1.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu1.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll1);
			var wps2 = Helper.CreatePackageState(rcn1, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll1);
			var wps3 = Helper.CreatePackageState(rcn1, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll2);
			var wps4 = Helper.CreatePackageState(rcn1, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll2);

			Factory.Save();

			var packageStates = dtu1.ValidPackageStatesInPendingDLLs;
			AssertEquals("Expected there to be two package states in the dll1", 2, packageStates.Count());
			AssertCollectionContains("Expected P1 in dll1", wps1, packageStates);
			AssertCollectionContains("Expected P2 in dll1", wps2, packageStates);
		}

		public void TestValidPackageStatesInPendingDLLs_WhenThereArePackagesOfDifferentStatus_ThenExcludePackagesThatAreDepartedOrFinalizedOrAdjustedOut()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dcn1 = Helper.CreateDispatchConsignment("DC1", warehouse.PK);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			Helper.CreateDispatchDLLDTUPivot(dll1.PK, dtu1.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Departed, dispatchLoadList: dll1, receiveUnit: rtu1, dispatchUnit: dtu1, dispatchConsignment: dcn1);
			var wps2 = Helper.CreatePackageState(rcn1, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.AdjustedOut, dispatchLoadList: dll1, receiveUnit: rtu1);
			var wps3 = Helper.CreatePackageState(rcn1, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Finalized, dispatchLoadList: dll1, receiveUnit: rtu1, dispatchUnit: dtu1, dispatchConsignment: dcn1);
			var wps4 = Helper.CreatePackageState(rcn1, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll1);

			Factory.Save();

			var packageStates = dtu1.ValidPackageStatesInPendingDLLs;
			AssertEquals("Expected there to be one package state in the dll1", 1, packageStates.Count());
			AssertCollectionContains("Expected P4 in dll1", wps4, packageStates);
			AssertEquals("Expected P4 status to be booked", wps4.WPS_Status, TransitWarehouseStatuses.Codes.Booked);
		}

		public void TestValidPackageStatesInPendingDLLs_GivenNoPendingDLLs_ThenReturnEmptyList()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var dtu1 = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			var dll1 = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);
			dll1.WDL_CompleteTime = ZDateTimeOffset.Now;
			dll2.WDL_CompleteTime = ZDateTimeOffset.Now;

			Helper.CreateDispatchDLLDTUPivot(dll1.PK, dtu1.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu1.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll1);
			var wps2 = Helper.CreatePackageState(rcn1, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll1);
			var wps3 = Helper.CreatePackageState(rcn1, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Booked, dispatchLoadList: dll2);

			Factory.Save();

			var packageStates = dtu1.ValidPackageStatesInPendingDLLs;
			AssertEquals("Expected no packages as there are no pending DLLs", 0, packageStates.Count());
		}

		#endregion

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}

	#region WhsItemDispatchTransportationUnitWorkflowProviderTest

	[TestedType(typeof(WhsItemDispatchTransportationUnit))]
	public class WhsItemDispatchTransportationUnitWorkflowProviderTest : WorkflowProviderTest<WhsItemDispatchTransportationUnit, WhsItemDispatchTransportationUnitProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.TransitDispatchTransportationUnit; }
		}
	}

	#endregion
}
