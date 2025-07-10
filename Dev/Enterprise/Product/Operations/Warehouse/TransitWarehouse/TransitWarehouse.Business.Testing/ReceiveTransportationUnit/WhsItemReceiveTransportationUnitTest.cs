using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
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
	public class WhsItemReceiveTransportationUnitTest : TestCaseWithFactory
	{
		#region TestAutoLog

		public void TestAutoLog()
		{
			var transportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			AssertEquals(true, transportationUnit.IsAutoAdminBusinessObjectLoggerEnabled);
		}

		#endregion

		#region TestWarehouse

		public void TestWarehouse()
		{
			var warehouse = Factory.New<WhsWarehouse>();
			var transportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			transportationUnit.WRH_WW_Warehouse = warehouse.PK;

			AssertEquals(warehouse, transportationUnit.Warehouse);
		}

		#endregion

		#region TestLocation

		public void TestLocation()
		{
			var location = Factory.New<WhsLocation>();
			var tranportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			tranportationUnit.WRH_WL_StagingLocation = location.PK;

			AssertEquals(location, tranportationUnit.Location);
		}

		#endregion

		#region TestGateInTime

		public void TestGateInTime()
		{
			var gateInTime = DateTimeOffset.Now;
			var tranportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			tranportationUnit.WRH_GateInTime = gateInTime;

			AssertEquals(gateInTime, tranportationUnit.GateInTime);
		}

		#endregion

		#region TestReadOnlyFields

		static readonly string[] ReadOnlyProperties =
		{
			"WRH_GateOutTime",
			"WRH_GateInTime",
			"WRH_UnloadCompleteTime",
			"WRH_WW_Warehouse",
			"WRH_SystemCreateTimeUtc",
			"WRH_SystemCreateUser",
			"WRH_SystemLastEditTimeUtc",
			"WRH_SystemLastEditUser",
			"WRH_ReferenceNumber",
			"WRH_SignedBySignature",
		};

		public void TestReadOnly()
		{
			var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
			foreach (var propertyName in ReadOnlyProperties)
			{
				var readOnly = ((ZPropertyInfo)typeof(WhsItemReceiveTransportationUnit).GetProperty(propertyName + "Info").GetValue(rtu)).ReadOnly;
				var readOnlyAction = ActionFieldAttribute.Get(typeof(WhsItemReceiveTransportationUnit).GetProperty(propertyName))?.ReadOnly;
				AssertEquals(propertyName, true, readOnly);
				AssertEquals(propertyName, true, readOnlyAction);
			}
		}

		#endregion

		#region TestIsGateOut

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestIsGatedOut()
		{
			var warehouse = Helper.CreateTRWWarehouse("ABC");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			AssertEquals("Precondition: RTU gate out time is empty.", true, rtu.WRH_GateOutTime.IsEmpty);

			rtu.WRH_UnloadCompleteTime = ZDateTimeOffset.Now.AddDays(-1);
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			rtu.IsGatedOut = true;

			AssertEquals("RTU gate out time should be set.", false, rtu.WRH_GateOutTime.IsEmpty);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateOut.Code);
			var rtuLog = rtu.Logs.Find(logQuery).SingleOrDefault();
			AssertNotNull("Should create gate out log for rtu", rtuLog);
			AssertEquals("RTU1|FAC=CFS|REF=V1|TYP=VehicleReference|WHS=ABC", rtuLog.SL_Reference);
			AssertEquals(ZDateTimeOffset.Now.ToDateTime(), rtuLog.SL_EventTime);
			AssertEquals(new ZDateTimeOffset(2021, 1, 1, 10, 0, 0, new TimeSpan(10, 0, 0)), rtu.WRH_GateOutTime);
		}

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestIsGatedOut_UnloadCompleteTimeAndGateOutTimeSame()
		{
			var warehouse = Helper.CreateTRWWarehouse("CBA");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			AssertEquals("Precondition: RTU gate out time is empty.", true, rtu.WRH_GateOutTime.IsEmpty);

			rtu.WRH_UnloadCompleteTime = ZDateTimeOffset.Now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			rtu.IsGatedOut = true;

			AssertEquals("RTU gate out time should be set.", false, rtu.WRH_GateOutTime.IsEmpty);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateOut.Code);
			var rtuLog = rtu.Logs.Find(logQuery).SingleOrDefault();
			AssertNotNull("Should create gate out log for rtu", rtuLog);
			AssertEquals("RTU1|FAC=CFS|REF=V1|TYP=VehicleReference|WHS=CBA", rtuLog.SL_Reference);
			AssertEquals(ZDateTimeOffset.Now.ToDateTime(), rtuLog.SL_EventTime);
			AssertEquals(new ZDateTimeOffset(2021, 1, 1, 10, 0, 0, new TimeSpan(10, 0, 0)), rtu.WRH_GateOutTime);
		}

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestIsGatedOut_ContainerizedPackageStateStatusIsDeptartedWhenIsGatedOutSetToTrue()
		{
			var warehouse = Helper.CreateTRWWarehouse("CBA");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = helper.CreateReceiveTransportationUnitWithContainerType("Container 1", warehouse.PK, warehouse.DefaultLocation.PK, "Container 1");

			AssertEquals("Precondition: RTU gate out time is empty.", true, rtu.WRH_GateOutTime.IsEmpty);

			rtu.WRH_UnloadCompleteTime = ZDateTimeOffset.Now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			rtu.IsGatedOut = true;

			AssertEquals("RTU gate out time should be set.", false, rtu.WRH_GateOutTime.IsEmpty);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateOut.Code);
			var rtuLog = rtu.Logs.Find(logQuery).SingleOrDefault();
			AssertNotNull("Should create gate out log for rtu", rtuLog);
			AssertEquals("Container 1|FAC=CFS|REF=Container 1|TYP=ContainerID|WHS=CBA", rtuLog.SL_Reference);
			AssertEquals(ZDateTimeOffset.Now.ToDateTime(), rtuLog.SL_EventTime);
			AssertEquals(new ZDateTimeOffset(2021, 1, 1, 10, 0, 0, new TimeSpan(10, 0, 0)), rtu.WRH_GateOutTime);
			AssertEquals(TransitWarehouseStatuses.Codes.Departed, rtu.ContainerizedPackageState.WPS_Status);
		}

		[TestDate(2021, 1, 1)]
		public void TestIsGatedOut_DoNotSetGateOutTimeIfUnloadCompleteTimeIsEmpty()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			AssertEquals("Precondition: RTU unload complete time is empty.", true, rtu.WRH_UnloadCompleteTime.IsEmpty);
			AssertEquals("Precondition: RTU gate out time is empty.", true, rtu.WRH_GateOutTime.IsEmpty);

			rtu.IsGatedOut = true;

			AssertEquals("RTU gate out time should not be set.", true, rtu.WRH_GateOutTime.IsEmpty);
		}

		[TestDate(2021, 1, 1)]
		public void TestIsGatedOut_CannotSetGateOutTimeIfUnloadCompleteTimeAndUnloadCompleteNotYetProcessedTimeAreAllEmpty()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			AssertEquals("Precondition: RTU unload complete time is empty.", true, rtu.WRH_UnloadCompleteTime.IsEmpty);
			AssertEquals("Precondition: RTU unload complete not yet processed time is empty.", true, rtu.WRH_UnloadCompleteNotYetProcessedTime.IsEmpty);
			AssertEquals("Precondition: RTU gate out time is empty.", true, rtu.WRH_GateOutTime.IsEmpty);

			rtu.IsGatedOut = true;

			AssertEquals("RTU gate out time should not be set.", true, rtu.WRH_GateOutTime.IsEmpty);
		}

		[TestDate(2021, 1, 1)]
		public void TestIsGatedOut_UnloadCompleteTimeIsEmpty_UnloadCompleteNotYetProcessedTimeEarilerThanGateOutTime()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			rtu.WRH_UnloadCompleteNotYetProcessedTime = ZDateTimeOffset.Now.AddHours(-10);

			AssertEquals("Precondition: RTU unload complete time is empty.", true, rtu.WRH_UnloadCompleteTime.IsEmpty);
			AssertEquals("Precondition: RTU unload complete not yet processed time is not empty.", false, rtu.WRH_UnloadCompleteNotYetProcessedTime.IsEmpty);
			AssertEquals("Precondition: RTU gate out time is empty.", true, rtu.WRH_GateOutTime.IsEmpty);

			rtu.IsGatedOut = true;

			AssertEquals("RTU gate out time should be set.", false, rtu.WRH_GateOutTime.IsEmpty);
		}

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestIsGatedOut_UnloadCompleteTimeIsEmpty_UnloadCompleteNotYetProcessedTimeIsLaterThanGateOutTime()
		{
			var warehouse = Helper.CreateTRWWarehouse("TRW");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			rtu.WRH_UnloadCompleteNotYetProcessedTime = ZDateTimeOffset.Now.AddHours(10);

			AssertEquals("Precondition: RTU unload complete time is empty.", true, rtu.WRH_UnloadCompleteTime.IsEmpty);
			AssertEquals("Precondition: RTU unload complete not yet processed time is not empty.", false, rtu.WRH_UnloadCompleteNotYetProcessedTime.IsEmpty);
			AssertEquals("Precondition: RTU gate out time is empty.", true, rtu.WRH_GateOutTime.IsEmpty);

			rtu.IsGatedOut = true;
			AssertEquals("RTU gate out time should be set.", false, rtu.WRH_GateOutTime.IsEmpty);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateOut.Code);
			var rtuLog = rtu.Logs.Find(logQuery).SingleOrDefault();
			AssertNotNull("Should create gate out log for rtu", rtuLog);
			AssertEquals("RTU1|FAC=CFS|REF=V1|TYP=VehicleReference|WHS=TRW", rtuLog.SL_Reference);
			AssertEquals(ZDateTimeOffset.Now.ToDateTime(), rtuLog.SL_EventTime);
			AssertEquals(new ZDateTimeOffset(2021, 1, 1, 20, 0, 0, new TimeSpan(10, 0, 0)), rtu.WRH_GateOutTime);
		}

		[TestDate(2021, 1, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestIsGatedOut_SetGateOutTimeIfUnloadCompleteTimeIsInTheFuture()
		{
			var warehouse = Helper.CreateTRWWarehouse("TRW");
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "BOX", "P1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);

			AssertEquals("Precondition: RTU unload complete time is empty.", true, rtu.WRH_UnloadCompleteTime.IsEmpty);
			AssertEquals("Precondition: RTU gate out time is empty.", true, rtu.WRH_GateOutTime.IsEmpty);

			rtu.WRH_UnloadCompleteTime = ZDateTimeOffset.Now.AddHours(10);
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;
			rtu.IsGatedOut = true;

			AssertEquals("RTU gate out time should be set.", false, rtu.WRH_GateOutTime.IsEmpty);

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.GateOut.Code);
			var rtuLog = rtu.Logs.Find(logQuery).SingleOrDefault();
			AssertNotNull("Should create gate out log for rtu", rtuLog);
			AssertEquals("RTU1|FAC=CFS|REF=V1|TYP=VehicleReference|WHS=TRW", rtuLog.SL_Reference);
			AssertEquals(ZDateTimeOffset.Now.ToDateTime(), rtuLog.SL_EventTime);
			AssertEquals(new ZDateTimeOffset(2021, 1, 1, 20, 0, 0, new TimeSpan(10, 0, 0)), rtu.WRH_GateOutTime);
		}

		#endregion

		#region TestTransportReference

		public void TestTransportReference()
		{
			var unitWithoutTransportReference = Factory.New<WhsItemReceiveTransportationUnit>();
			var unitWithVehicleNumber = Factory.New<WhsItemReceiveTransportationUnit>();
			var unitWithTransportNumber = Factory.New<WhsItemReceiveTransportationUnit>();
			var unitWithBothVehicleAndTransportNumber = Factory.New<WhsItemReceiveTransportationUnit>();
			var unitWithBothVehicleDifferentTypeOfAdditionalReference = Factory.New<WhsItemReceiveTransportationUnit>();
			unitWithVehicleNumber.WRH_VehicleReference = "V1";
			unitWithBothVehicleAndTransportNumber.WRH_VehicleReference = "V2";
			unitWithBothVehicleDifferentTypeOfAdditionalReference.WRH_VehicleReference = "V3";
			CreateAdditionalReference(unitWithTransportNumber, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, "T1");
			CreateAdditionalReference(unitWithBothVehicleAndTransportNumber, WarehouseAdditionalReferenceTypes.Codes.RunSheetNumber, "B2");
			CreateAdditionalReference(unitWithBothVehicleDifferentTypeOfAdditionalReference, TransportAdditionalReferenceTypes.Codes.BookingPartyReference, "C2");

			AssertEquals("", unitWithoutTransportReference.TransportReference);
			AssertEquals("V1", unitWithVehicleNumber.TransportReference);
			AssertEquals("T1", unitWithTransportNumber.TransportReference);
			AssertEquals("B2", unitWithBothVehicleAndTransportNumber.TransportReference);
			AssertEquals("V3", unitWithBothVehicleDifferentTypeOfAdditionalReference.TransportReference);
		}

		void CreateAdditionalReference<T>(T bizO, string referenceType, string referenceNumber) where T : BusinessObject
		{
			var additionalReference = Factory.NewWithValidTestData<CusEntryNumber>();
			additionalReference.CE_ParentID = bizO.PK;
			additionalReference.CE_ParentTable = WhsItemReceiveTransportationUnitSchema.Constants.TableName;
			additionalReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			additionalReference.CE_EntryNum = referenceNumber;
			additionalReference.CE_EntryType = referenceType;
		}

		#endregion

		#region TestMasterBillNumber

		public void TestMasterBillNumber()
		{
			var unitWithoutMasterBillNumber = Factory.New<WhsItemReceiveTransportationUnit>();
			var unitWithMasterBillNumber = Factory.New<WhsItemReceiveTransportationUnit>();
			var unitWithMasterBillNumberOnASN = Factory.New<WhsItemReceiveTransportationUnit>();
			var asn = Factory.New<WhsItemReceiveASN>();
			CreateAdditionalReference(asn, TransportAdditionalReferenceTypes.Codes.MasterBill, "ASNB");
			Helper.CreateReceiveASNRTUPivot(unitWithMasterBillNumberOnASN.PK, asn.PK);
			CreateAdditionalReference(unitWithMasterBillNumber, TransportAdditionalReferenceTypes.Codes.MasterBill, "B1");
			CreateAdditionalReference(unitWithMasterBillNumberOnASN, TransportAdditionalReferenceTypes.Codes.MasterBill, "B2");

			AssertEquals("", unitWithoutMasterBillNumber.MasterBillNumber);
			AssertEquals("B1", unitWithMasterBillNumber.MasterBillNumber);
			AssertEquals("B2", unitWithMasterBillNumberOnASN.MasterBillNumber);
		}

		public void TestMasterBillNumber_FallbackToASN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);
			CreateAdditionalReference(asn, TransportAdditionalReferenceTypes.Codes.MasterBill, "ASNB1");
			Factory.Save();
			AssertEquals("ASNB1", rtu.MasterBillNumber);

			CreateAdditionalReference(rtu, TransportAdditionalReferenceTypes.Codes.MasterBill, "RTUASN1");
			Factory.Save();
			var newBizOFactory = new BusinessObjectFactory();
			AssertEquals("RTUASN1", newBizOFactory.Load<WhsItemReceiveTransportationUnit>(rtu.PK).MasterBillNumber);
		}

		public void TestMasterBillNumber_FallbackToASN_MultipleASNs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var asn1 = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn1.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn2.PK);
			CreateAdditionalReference(asn1, TransportAdditionalReferenceTypes.Codes.MasterBill, "ASNB1");
			CreateAdditionalReference(asn2, TransportAdditionalReferenceTypes.Codes.MasterBill, "ASNB2");
			Factory.Save();
			AssertEquals("ASNB2", rtu.MasterBillNumber);
		}

		#endregion

		#region TestCarrierBookingReference

		public void TestCarrierBookingReference_FromASN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);
			CreateAdditionalReference(asn, WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference, "ASNBooking");
			Factory.Save();
			AssertEquals("ASNBooking", rtu.CarrierBookingReference);
		}

		public void TestCarrierBookingReference_FromMultipleASNs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var asn1 = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn1.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn2.PK);
			CreateAdditionalReference(asn1, WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference, "ASNBooking1");
			CreateAdditionalReference(asn2, WarehouseAdditionalReferenceTypes.Codes.CarrierBookingReference, "ASNBooking2");
			Factory.Save();
			AssertEquals("ASNBooking1", rtu.CarrierBookingReference);
		}

		#endregion

		#region TestWRH_SignedByOptional

		public void TestWRH_SignedByOptional()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rtu = Helper.CreateReceiveTransportationUnit("r2", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			rtu.RunPreSaveValidation();
			AssertNoErrors(rtu.WRH_SignedByInfo);
		}

		#endregion

		#region TestTransferMode

		public void TestTransferMode()
		{
			var warehouse = Helper.CreateWarehouse("Bobs Warehouse", "WHS", "A");
			Factory.Save();

			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			AssertEquals("Transport mode is empty", ZString.Empty, receiveTransportationUnit.TransportMode);

			var receiveTransportationUnit2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			receiveConsignment.WRC_TransportMode = "Air";
			Helper.CreatePackageState(receiveConsignment, 1, "BOX", "P1", "ARV", receiveUnit: receiveTransportationUnit2);
			Factory.Save();

			AssertEquals("Transport mode has value", "Air", receiveTransportationUnit2.TransportMode);
		}

		#endregion

		#region TestContainers

		#region TestContainerNumber

		public void TestContainerNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var unitWithoutContainer = Helper.CreateReceiveTransportationUnit("Container 1", warehouse.PK, warehouse.DefaultLocation.PK);
			var unitWithContainer = Helper.CreateReceiveTransportationUnitWithContainerType("Container 1", warehouse.PK, warehouse.DefaultLocation.PK, "Container 1");

			AssertEquals(ZString.Empty, unitWithoutContainer.ContainerNumber);
			AssertEquals("Container 1", unitWithContainer.ContainerNumber);
		}

		#endregion

		#region TestHasContainerEquipmentDetails

		public void TestHasContainerEquipmentDetails()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var unitWithoutContainer = Helper.CreateReceiveTransportationUnit("Container 1", warehouse.PK, warehouse.DefaultLocation.PK);
			var unitWithContainer = Helper.CreateReceiveTransportationUnitWithContainerType("Container 1", warehouse.PK, warehouse.DefaultLocation.PK, "Container 1");

			AssertEquals(false, unitWithoutContainer.HasContainerEquipmentDetails);
			AssertEquals(true, unitWithContainer.HasContainerEquipmentDetails);
		}

		#endregion

		#region TestContainerTypeCode

		public void TestContainerTypeCode()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var unitWithoutContainer = Helper.CreateReceiveTransportationUnit("Container 1", warehouse.PK, warehouse.DefaultLocation.PK);
			var unitWithContainer = Helper.CreateReceiveTransportationUnitWithContainerType("Container 1", warehouse.PK, warehouse.DefaultLocation.PK, "Container 1");

			AssertEquals(ZString.Empty, unitWithoutContainer.ContainerTypeCode);
			AssertEquals("20GP", unitWithContainer.ContainerTypeCode);
		}

		#endregion

		#region TestContainerISOType

		public void TestContainerISOType()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var unitWithoutContainer = Helper.CreateReceiveTransportationUnit("Container 1", warehouse.PK, warehouse.DefaultLocation.PK);
			var unitWithContainer = Helper.CreateReceiveTransportationUnitWithContainerType("Container 1", warehouse.PK, warehouse.DefaultLocation.PK, "Container 1");

			AssertEquals(ZString.Empty, unitWithoutContainer.ContainerISOType);
			AssertEquals("22G0", unitWithContainer.ContainerISOType);
		}

		#endregion

		#region TestIsContainerUnitType

		public void TestIsContainerUnitType()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var uldRTU = Helper.CreateReceiveTransportationUnitWithContainerType("ULD 1", warehouse.PK, warehouse.DefaultLocation.PK, "ULD 1", containerType: "AAA");
			var cntRTU = Helper.CreateReceiveTransportationUnitWithContainerType("CNT 1", warehouse.PK, warehouse.DefaultLocation.PK, "CNT 1", containerType: "20GP");
			var vehRTUWithContainer = Helper.CreateReceiveTransportationUnitWithContainerType("VEH 1", warehouse.PK, warehouse.DefaultLocation.PK, "VEH 1", containerType: "DROP");
			var vehRTUWithoutContainer = Helper.CreateReceiveTransportationUnit("VEH 2", warehouse.PK, warehouse.DefaultLocation.PK);

			AssertEquals(true, uldRTU.IsContainerUnitType);
			AssertEquals(true, cntRTU.IsContainerUnitType);
			AssertEquals(true, vehRTUWithContainer.IsContainerUnitType);
			AssertEquals(false, vehRTUWithoutContainer.IsContainerUnitType);
		}

		#endregion

		#endregion

		#region TestContainer

		public void TestContainer()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var rtu = helper.CreateReceiveTransportationUnitWithContainerType("Container 1", warehouse.PK, warehouse.DefaultLocation.PK, "Container 1");
			AssertNotNull(rtu.Container);
			AssertEquals("20GP", rtu.Container.ContainerType.RC_Code);
		}

		#endregion

		#region TestVehicleNumber

		public void TestVehicleNumber()
		{
			var unitWithoutContainer = Factory.New<WhsItemReceiveTransportationUnit>();
			unitWithoutContainer.WRH_VehicleReference = "Vehicle 1";
			unitWithoutContainer.WRH_UnitType = TransportUnitTypes.Vehicle;
			var unitWithContainer = Factory.New<WhsItemReceiveTransportationUnit>();
			unitWithContainer.WRH_VehicleReference = "Vehicle 1";
			unitWithContainer.WRH_UnitType = TransportUnitTypes.ULD;

			AssertEquals(ZString.Empty, unitWithContainer.VehicleNumber);
			AssertEquals("Vehicle 1", unitWithoutContainer.VehicleNumber);
		}

		#endregion

		#region TestJobDocAddresses

		public void TestJobDocAddresses()
		{
			var header = Factory.New<WhsItemReceiveTransportationUnit>();
			AssertEquals(DocAddressType.TransportCompanyDocumentaryAddress, header.TransportCompany.DocAddressType);
			AssertEquals(DocAddressType.ClientRequestedBillingParty, header.ClientRequestedBillToPartyDocAddress.DocAddressType);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			var transportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			AssertEquals("Receive Transportation Unit", transportationUnit.HumanReadableName);

			transportationUnit.WRH_ReferenceNumber = "192";
			AssertEquals("Receive Transportation Unit 192", transportationUnit.HumanReadableName);
		}

		#endregion

		#region TestIJobNumberMembers

		public void TestIJobNumberMembers()
		{
			var transportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			transportationUnit.WRH_ReferenceNumber = "1";
			AssertEquals("1", transportationUnit.JobNumber);
		}

		#endregion

		#region	IProcessHandlingInfoProvider Members

		public void TestIProcessHandlingInfoProvider()
		{
			var infoProvider = (IProcessHandlingInfoProvider)Factory.New<WhsItemReceiveTransportationUnit>();
			AssertEquals(typeof(WhsItemReceiveTransportationUnitProcessHandlingInfoProvider), infoProvider.ProcessHandlingInfo.GetType());
		}

		#endregion

		#region TestIDocManagerSupport

		public void TestIDocManagerSupport()
		{
			var transportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			AssertEquals(Constants.DocManagerCodes.TransitReceiveTransportationUnit, ((IDocManagerSupport)transportationUnit).DocManagerInfo.DocManagerCode);
		}

		#endregion

		#region TestIWorkflowProvider Members

		public void TestIWorkflowProviderMembers()
		{
			var transportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			var newWorkflowItem = transportationUnit.WorkflowItems.AddNew();
			var workflowProvider = (IWorkflowProvider)transportationUnit;

			AssertEquals(WorkflowDescriptors.TransitReceiveTransportationUnit, workflowProvider.WorkflowType);
			AssertEquals(newWorkflowItem, workflowProvider.WorkflowItems.Single());
			AssertEquals(typeof(ColumnValueRanker), workflowProvider.GetTemplateSelectionCriteria().GetType());
			AssertNull(workflowProvider.GetWorkflowInformationProvider());
		}

		public void TestGetTemplateSelectionCriteria()
		{
			var intendedWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			var receiveTransportationUnitWithWarehouse = Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>();
			receiveTransportationUnitWithWarehouse.WRH_WW_Warehouse = intendedWarehouse.PK;
			Factory.Save();

			var criteriaForReceiveTransportationUnitWithWarehouse = (ColumnValueRanker)((IWorkflowProvider)receiveTransportationUnitWithWarehouse).GetTemplateSelectionCriteria();
			AssertContainsExactElementsInAnyOrder(new[] { intendedWarehouse.PK, ZGuid.Empty }, criteriaForReceiveTransportationUnitWithWarehouse.GetValues(ProcessTaskTemplateSchema.P0_WW));
		}

		#endregion

		#region TestIPackingParentSupportsImportingBookedDimensions

		public void TestIPackingParentSupportsImportingBookedDimensions()
		{
			var dtu = Factory.New<WhsItemReceiveTransportationUnit>();
			AssertNotNull("Receive DTU support importing Booked Dimensions.", dtu);
		}

		#endregion

		#region TestIPackingParentMembers

		public void TestIPackingParentMembers()
		{
			var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
			rtu.WRH_ReferenceNumber = "RTU1";
			IPackingParent packingParent = rtu;
			AssertEquals(null, packingParent.ControllerID);
			AssertEquals(DocumentOptions.ShowBasicLabelOnly, packingParent.DocumentOptions);
			AssertEquals(false, packingParent.IsAutoPrintAllowed);
			AssertEquals(false, packingParent.IsParentJobFinalised);
			AssertEquals(false, packingParent.IsScanEventsVisible);
			AssertEquals("", packingParent.JobDescription);
			AssertEquals("RTU1", packingParent.JobNo);
			AssertEquals(null, packingParent.CarrierBookingAgent);
			AssertEquals("", packingParent.TransportReference);
			AssertEquals(false, packingParent.IsLoosePackageIDsSupported);
			AssertEquals(ParentJobType.None, packingParent.ParentJobType);
			AssertEquals(PackageSequenceType.Outer, packingParent.PackageSequenceType);
			AssertEquals(false, packingParent.IsPackingJobReadOnly);
			AssertEquals("NotificationTypeForInvalidContainerNumber should be correct", NotificationTypes.None, packingParent.NotificationTypeForInvalidContainerNumber);
		}

		#endregion

		#region TestIPackingParent_ShouldPackTrackedPackagesViaDivot

		public void TestIPackingParent_ShouldPackTrackedPackagesViaDivot()
		{
			var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
			IPackingParent packingParent = rtu;
			AssertEquals(false, packingParent.ShouldPackTrackedPackagesViaDivot);
		}

		#endregion

		#region TestIPackingParent_OnPackageDelete

		public void TestIPackingParent_OnPackageDelete()
		{
			var transportationUnit = Factory.New<WhsItemReceiveTransportationUnit>();
			var packageState = transportationUnit.PackageStates.AddNew();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(transportationUnit);

			var package = packageJob.Packages.AddNew();
			package.KP_PackageID = "1";
			packageState.WPS_KP_Package = package.PK;
			AssertEquals("Precondition", false, packageState.IsDeleted);

			package.Delete();
			AssertEquals(true, packageState.IsDeleted);
		}

		#endregion

		#region TestIPackingParentWithOutturn Members

		public void TestIPackingParentWithOutturn_OutturnProvider()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var location = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)receiveTransportationUnit;
			var package = Helper.CreatePackage(receiveTransportationUnit.PackageJob, "P1", 1, "PLT");

			var outturnProvider = packingParentWithOutturn.GetOutturnProvider(package);
			AssertNull(outturnProvider);
		}

		public void TestIPackingParentWithOutturn_GetParentContainer()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var location = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)receiveTransportationUnit;
			var package = Helper.CreatePackage(receiveTransportationUnit.PackageJob, "P1", 1, "PLT");

			var containerPair = packingParentWithOutturn.GetParentContainer(package);
			AssertNotNull(containerPair);
			AssertNull(containerPair.Container);
			AssertNull(containerPair.ContainerNumber);
		}

		[TestDate(2020, 08, 28, 04, 39, 50)]
		public void TestIPackingParentWithOutturn_ContainerView()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var location = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, location.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)receiveTransportationUnit;
			var currentTime = ZDateTimeOffset.Now;
			receiveTransportationUnit.WRH_UnloadCompleteTime = currentTime;
			receiveTransportationUnit.WRH_UnloadCompleteNotYetProcessedTime = receiveTransportationUnit.WRH_UnloadCompleteTime;
			var container = receiveTransportationUnit.Container;

			var containerView = packingParentWithOutturn.GetContainerView(container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertEquals(currentTime.ToZDateTime(), containerView.UnpackCompleteDate);
		}

		public void TestIPackingParentWithOutturn_ContainerView_NoDispatchTransportationUnit()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var receiveConsignment = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var location = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, location.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)receiveTransportationUnit;
			var package = receiveConsignment.PackageJob.Containers.AddNew();

			var containerView = packingParentWithOutturn.GetContainerView(package.Container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		public void TestIPackingParentWithOutturn_ContainerView_InvalidDate()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var location = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, location.PK);
			receiveTransportationUnit.WRH_UnloadCompleteTime = ZDateTimeOffset.Invalid;
			receiveTransportationUnit.WRH_UnloadCompleteNotYetProcessedTime = receiveTransportationUnit.WRH_UnloadCompleteTime;
			var packingParentWithOutturn = (IPackingParentWithOutturn)receiveTransportationUnit;
			var container = receiveTransportationUnit.Container;

			var containerView = packingParentWithOutturn.GetContainerView(container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		public void TestIPackingParentWithOutturn_ContainerView_NoDates()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var location = Helper.CreateRowAndGenerateLocations(warehouse, "DOCK");
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, location.PK);
			var packingParentWithOutturn = (IPackingParentWithOutturn)receiveTransportationUnit;
			var container = receiveTransportationUnit.Container;

			var containerView = packingParentWithOutturn.GetContainerView(container);

			AssertNotNull(containerView);
			AssertNull(containerView.PackCompleteDate);
			AssertNull(containerView.UnpackCompleteDate);
		}

		#endregion

		#region IJobCostingPlugIn Member

		public void TestIJobCostingPlugIn()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			AssertEquals("JK_UniqueConsignRef", "RTU1", rtu.JK_UniqueConsignRef);
			AssertEquals("ConsolExchangeRate", 0m, rtu.ConsolExchangeRate);
			AssertEquals("ContainerMode", ZString.Empty, rtu.ContainerMode);
			AssertEquals("ConsolType", ZString.Empty, rtu.ConsolType);
			AssertEquals("Direction", ZString.Empty, rtu.Direction);
			AssertEquals("Module", ApportionmentMethodModules.TransitWarehouse, rtu.Module);
			AssertEquals("ExchangeRateForCurrency", 0m, rtu.ExchangeRateForCurrency(null, ZGuid.Empty));
			AssertEquals("GetPrepaidCollect", ZString.Empty, rtu.GetPrepaidCollect(null));

			AssertNull("LoadPort", rtu.LoadPort);
			AssertNull("DischargePort", rtu.DischargePort);
			AssertNull("ProfitLossContainer", rtu.ProfitLossContainer);
			AssertNull("ConsolCurrency", rtu.ConsolCurrency);
			AssertNull("ReceivingAgent", rtu.ReceivingAgent);
			AssertNull("ReceivingAgentAPInvoicingParty", rtu.ReceivingAgentAPInvoicingParty);
			AssertNull("ReceivingAgentARInvoicingParty", rtu.ReceivingAgentARInvoicingParty);
			AssertNull("SendingAgent", rtu.SendingAgent);
			AssertNull("SendingAgentAPInvoicingParty", rtu.SendingAgentAPInvoicingParty);
			AssertNull("SendingAgentARInvoicingParty", rtu.SendingAgentARInvoicingParty);
			AssertNull("PrepaidCollectList", rtu.PrepaidCollectList);

			AssertNotNull("costSupporter", rtu.CostSupporter);
			Assert("IsMasterCollect", !rtu.IsMasterCollect);
		}

		#endregion

		#region TestReceiveConsignments

		public void TestReceiveConsignments()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, stageLocation.PK);

			Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rcn3, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Arrived, rtu2);

			AssertContainsExactElementsInAnyOrder(new[] { rcn1, rcn2 }, rtu1.ReceiveConsignments);
			AssertContainsExactElementsInAnyOrder(new[] { rcn3 }, rtu2.ReceiveConsignments);
		}

		#endregion

		#region ITransportationUnitForCost Member

		public void TestConsignments()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, stageLocation.PK);
			var rtu3 = Helper.CreateReceiveTransportationUnit("RTU3", warehouse.PK, stageLocation.PK);

			Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rcn3, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Arrived, rtu2);
			Helper.CreatePackageState(rtu3, 1, "PLT", "P5", TransitWarehouseStatuses.Codes.Arrived);

			AssertContainsExactElementsInAnyOrder(new IJobInvoicingPlugIn[] { rcn1, rcn2 }, rtu1.Consignments);
			AssertContainsExactElementsInAnyOrder(new IJobInvoicingPlugIn[] { rcn3 }, rtu2.Consignments);
			AssertEquals(0, rtu3.Consignments.Count());
		}

		public void TestCreditorPK()
		{
			var transportCompany = Helper.CreateClient("TRC");
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			Helper.CreatePackageState(rcn, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu);

			AssertEquals(transportCompany.PK, rtu.CreditorPK);
		}

		public void TestCreditorPK_TransportCompanyIsOverridden()
		{
			var transportCompany = Helper.CreateClient("TRC");
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			Helper.CreatePackageState(rcn, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu);

			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);
			rtu.TransportCompany.E2_AddressOverride = true;
			rtu.TransportCompany.E2_CompanyName = "Transport Co";

			AssertEquals(ZGuid.Empty, rtu.CreditorPK);
		}

		public void TestDefaultChargeGroups()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			Helper.CreatePackageState(rcn, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu);

			AssertContainsExactElementsInAnyOrder(new ZString[] { ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit }, rtu.DefaultChargeGroups);
		}

		public void TestMasterBillNum()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			Helper.CreatePackageState(rcn, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu);
			AssertEquals(ZString.Empty, rtu.MasterBillNum);

			var additionalReference = rtu.AdditionalReferenceNumbers.AddNew();
			additionalReference.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			additionalReference.CE_EntryNum = "MAB Test";

			AssertEquals("MAB Test", rtu.MasterBillNum);
		}

		public void TestETAAndETD()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rtu2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Arrived);

			var testDate = ZDateTime.Now;
			rcn1.WRC_ExpectedArrivalTime = testDate;
			rcn1.WRC_ExpectedDispatchTime = testDate.AddHours(1);
			rcn2.WRC_ExpectedArrivalTime = testDate.AddHours(2);
			rcn2.WRC_ExpectedDispatchTime = testDate.AddHours(3);

			AssertEquals(testDate, rtu1.ETA);
			AssertEquals(testDate.AddHours(3), rtu1.ETD);
			AssertEquals(ZDateTime.Empty, rtu2.ETA);
			AssertEquals(ZDateTime.Empty, rtu2.ETD);
		}

		#endregion

		#region ITransitTransportationUnitForRating Member

		public void TestITransitTransportationUnitForRating()
		{
			var now = ZDateTimeOffset.Now;
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var rtu = helper.CreateReceiveTransportationUnitWithContainerType("Container 1", warehouse.PK, warehouse.DefaultLocation.PK, "Container 1");
			rtu.WRH_GateInTime = now.AddHours(-1);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;

			var transportCompany = Helper.CreateClient("TRC");
			Helper.CreateJobDocAddressFromAddress(rtu, DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, transportCompany.MainAddress);

			var transitRating = (ITransitTransportationUnitForRating)rtu;
			AssertEquals(rtu.TransportCompany, transitRating.TransportCompanyDocAddress);
			AssertEquals(ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit, transitRating.ChargeCodeGroup);
			AssertEquals(rtu.WRH_GateInTime.ToLocalZDateTime(), transitRating.ExpectedArrivalDate);
			AssertEquals(rtu.WRH_UnloadCompleteTime.ToLocalZDateTime(), transitRating.ExpectedDepartureDate);
			AssertNotNull(transitRating.ContainerForRating);
			AssertEquals("20GP", transitRating.ContainerForRating.Value.Key.ContainerType.RC_Code);
		}

		public void TestITransitJobForRating_IsOnPallets_WithContainerizedRTUAsPackage()
		{
			var now = ZDateTimeOffset.Today;
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var rtu = helper.CreateReceiveTransportationUnitWithContainerType("Container 1", warehouse.PK, warehouse.DefaultLocation.PK, "Container 1");
			rtu.WRH_GateInTime = now.AddHours(-1);
			rtu.WRH_UnloadCompleteTime = now;
			rtu.WRH_UnloadCompleteNotYetProcessedTime = rtu.WRH_UnloadCompleteTime;

			var rtu2 = helper.CreateReceiveTransportationUnitWithContainerType("AAA1", warehouse.PK, warehouse.DefaultLocation.PK, containerID: "AAA1", containerType: "AAA");
			rtu2.WRH_GateInTime = now.AddHours(-1);
			rtu2.WRH_UnloadCompleteTime = now;
			rtu2.WRH_UnloadCompleteNotYetProcessedTime = rtu2.WRH_UnloadCompleteTime;

			var vehRTU = helper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, warehouse.DefaultLocation.PK, containerID: "DROP1", containerType: "DROP");
			vehRTU.WRH_GateInTime = now.AddHours(-1);
			vehRTU.WRH_UnloadCompleteTime = now;
			vehRTU.WRH_UnloadCompleteNotYetProcessedTime = vehRTU.WRH_UnloadCompleteTime;

			var arrivedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var arrivedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			var rtuPackageState = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtu.PackageExtension.KPN_KP_Package)).FirstOrDefault();
			rtuPackageState.WPS_UnloadedTime = now;
			rtuPackageState.WPS_UnloadedNotYetProcessedTime = rtuPackageState.WPS_UnloadedTime;
			rtuPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Arrived;
			rtuPackageState.WPS_WRH_TransitReceiveHeader = vehRTU.PK;
			rtuPackageState.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;

			var rtuPackageState2 = Factory.Load<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, rtu2.PackageExtension.KPN_KP_Package)).FirstOrDefault();
			rtuPackageState2.WPS_UnloadedTime = now;
			rtuPackageState2.WPS_UnloadedNotYetProcessedTime = rtuPackageState2.WPS_UnloadedTime;
			rtuPackageState2.WPS_Status = TransitWarehouseStatuses.Codes.Arrived;
			rtuPackageState2.WPS_WRH_TransitReceiveHeader = vehRTU.PK;
			rtuPackageState2.WPS_WL_LastLocation = warehouse.DefaultLocation.PK;
			Factory.Save();

			var transitRating = (ITransitTransportationUnitForRating)vehRTU;
			AssertNotNull(transitRating.ContainerForRating);
			AssertEquals(ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit, transitRating.ChargeCodeGroup);
			AssertEquals("DROP", transitRating.ContainerForRating.Value.Key.ContainerType.RC_Code);
			AssertEquals("IsOnPallets", true, transitRating.ContainerForRating.Value.Value);
		}

		#region TestITransportJobForRating_FreightMode

		public void TestITransportJobForRating_FreightMode_OneReceiveConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			var transitRating = (ITransitJobForRating)rtu;
			AssertEquals(FreightMode.AIR, transitRating.FreightMode);
		}

		public void TestITransportJobForRating_FreightMode_OneReceiveConsignmentHasNoTransportMode()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			AssertEquals("", rcn.WRC_TransportMode);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			var transitRating = (ITransitJobForRating)rtu;
			AssertNull(transitRating.FreightMode);
		}

		public void TestITransportJobForRating_FreightMode_MultipleReceiveConsignmentsWithSameTransportMode()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			rcn2.WRC_TransportMode = TransportModes.AirSea;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn2, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			var transitRating = (ITransitJobForRating)rtu;
			AssertEquals(FreightMode.AIR, transitRating.FreightMode);
		}

		public void TestITransportJobForRating_FreightMode_MultipleReceiveConsignmentsWithDifferentTransportModes()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);
			rcn2.WRC_TransportMode = TransportModes.Sea;

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn2, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			var transitRating = (ITransitJobForRating)rtu;
			AssertEquals(FreightMode.UKN, transitRating.FreightMode);
		}

		public void TestITransportJobForRating_FreightMode_OneOfReceiveConsignmentHasTransportModes()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;
			var rcn2 = Helper.CreateReceiveConsignment("RC2", warehouse.PK);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var packageState2 = Helper.CreatePackageState(rcn2, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			var transitRating = (ITransitJobForRating)rtu;
			AssertEquals(FreightMode.AIR, transitRating.FreightMode);
		}

		public void TestITransportJobForRating_FreightMode_ReceiveTransportationUnitIsContainer()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			Factory.Save();

			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			rcn.WRC_TransportMode = TransportModes.Air;
			var rtu = Helper.CreateReceiveTransportationUnitWithContainerType("RTU1", warehouse.PK, location.PK);

			AssertEquals("Precondition: TransportMode of Container is SEA", TransportModes.Sea, rtu.Container.ContainerType.RC_ShippingMode);

			var loadedPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var loadedPackageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Factory.Save();

			var transitRating = (ITransitJobForRating)rtu;
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
			arrivedPackageState.Package.KP_RH_NKCommodityCode = "HAZ";
			arrivedPackageState.Package.KP_Weight = 10m;
			arrivedPackageState.Package.KP_WeightUQ = Constants.Weight.Kilograms;
			arrivedPackageState.Package.KP_Volume = 5m;
			arrivedPackageState.Package.KP_VolumeUQ = Constants.Volume.CubicMetres;
			arrivedPackageState.Package.KP_RH_NKCommodityCode = "HAZ";
			var undgDataItem = Helper.CreateUNDGDataItem(arrivedPackageState.Package.PK, arrivedPackageState.Package.TablePrefix, undgSubstance, 2, 3);
			arrivedPackageState.Package.UNDGs.Add(undgDataItem);

			var loadedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKG2", TransitWarehouseStatuses.Codes.FreightLoaded, receiveUnit: rtu, dispatchConsignment: dcn, dispatchLoadList: dll, dispatchUnit: dtu);
			loadedPackageState.WPS_UnloadedTime = unloadedTime;
			loadedPackageState.WPS_UnloadedNotYetProcessedTime = loadedPackageState.WPS_UnloadedTime;
			loadedPackageState.WPS_LoadedTime = now;
			var bookedPackageState = Helper.CreatePackageState(rcn, 10, "PKG", "", TransitWarehouseStatuses.Codes.Booked, dispatchConsignment: dcn);
			Factory.Save();

			var transitPackagesForRating = ((ITransitJobForRating)rtu).TransitPackagesForRating;
			AssertEquals(2, transitPackagesForRating.Count());

			var transitPackageForRating1 = transitPackagesForRating.FirstOrDefault(p => p.PK == arrivedPackageState.PK);
			AssertTransitPackageForRating(transitPackageForRating1, arrivedPackageState, true);

			var transitPackageForRating2 = transitPackagesForRating.FirstOrDefault(p => p.PK == loadedPackageState.PK);
			AssertTransitPackageForRating(transitPackageForRating2, loadedPackageState, false);
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
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var arrivedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKGA", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			arrivedPackageState.WPS_UnloadedTime = completeTime;
			arrivedPackageState.WPS_UnloadedNotYetProcessedTime = arrivedPackageState.WPS_UnloadedTime;

			var handlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU-1", handlingUnit, null);
			handlingUnitPackage.WPS_WL_LastLocation = location.PK;
			handlingUnitPackage.WPS_WW_Warehouse = warehouse.PK;
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Factory.Save();

			var transitPackagesForRating = ((ITransitJobForRating)rtu).TransitPackagesForRating;
			AssertEquals(3, transitPackagesForRating.Count());

			var transitPackageForRating1 = transitPackagesForRating.FirstOrDefault(p => p.PK == arrivedPackageState.PK);
			AssertTransitPackageForRating(transitPackageForRating1, arrivedPackageState, false);

			var transitPackageForRating2 = transitPackagesForRating.FirstOrDefault(p => p.PK == childPackage1.PK);
			AssertTransitPackageForRating(transitPackageForRating2, childPackage1, false);

			var transitPackageForRating3 = transitPackagesForRating.FirstOrDefault(p => p.PK == childPackage2.PK);
			AssertTransitPackageForRating(transitPackageForRating3, childPackage2, false);
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
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var arrivedPackageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKGA", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			arrivedPackageState.WPS_UnloadedTime = completeTime;
			arrivedPackageState.WPS_UnloadedNotYetProcessedTime = arrivedPackageState.WPS_UnloadedTime;

			var overpackHandlingUnit = Helper.CreatePackageHandlingUnit();
			var handlingUnitPackage = Helper.CreateHandlingUnitPackage("HU-1", overpackHandlingUnit, rtu, TransitWarehouseStatuses.Codes.Arrived, unitType: PackageStateUnitType.Codes.Overpack, rcn: rcn);
			var childPackage1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			var childPackage2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Arrived, receiveUnit: rtu);
			Helper.DisableTopLevelHUFKForTest(TestConnection);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage1, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Helper.PackPackageIntoHandlingUnit(handlingUnitPackage, childPackage2, ZDateTimeOffset.Now, "AAA", topHandlingUnit: handlingUnitPackage);
			Factory.Save();

			var transitPackagesForRating = ((ITransitJobForRating)rtu).TransitPackagesForRating;
			AssertEquals(2, transitPackagesForRating.Count());

			var transitPackageForRating1 = transitPackagesForRating.FirstOrDefault(p => p.PK == arrivedPackageState.PK);
			AssertTransitPackageForRating(transitPackageForRating1, arrivedPackageState, false);

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

		#region TestGetAttachedJobNumber Member

		public void TestGetAttachedJobNumber()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, warehouse.DefaultInboundDockDoorLocation.PK);
			var package = Helper.CreatePackageState(rcn, 1, "PKG", "P1", "ARV", receiveUnit: rtu);
			Helper.CreateAdditionalReference(package, "S0000001", "BPR");
			Factory.Save();
			AssertEquals("", "S0000001", ((IPackingParentWithAttachedParent)rtu).GetAttachedJobNumber(package.Package));
		}

		#endregion

		#region IWhsItemReceiveTransportationUnit Members

		public void TestIWhsItemReceiveTransportationUnit_Warehouse()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var rtuAsInterface = (IWhsItemReceiveTransportationUnit)receiveTransportationUnit;
			AssertEquals(rtuAsInterface.Warehouse, warehouse);
		}

		public void TestIWhsItemReceiveTransportationUnit_UnitType()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);

			var rtuAsInterface = (IWhsItemReceiveTransportationUnit)receiveTransportationUnit;
			AssertEquals(rtuAsInterface.UnitType, receiveTransportationUnit.WRH_UnitType);
		}

		public void TestIWhsItemReceiveTransportationUnit_LatestReceiveConsignment()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			rcn1.WRC_SystemCreateTimeUtc = DateTime.Now;
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			rcn2.WRC_SystemCreateTimeUtc = DateTime.Now.AddDays(1);
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var rtu2 = Helper.CreateReceiveTransportationUnit("RTU2", warehouse.PK, stageLocation.PK);
			var rtu3 = Helper.CreateReceiveTransportationUnit("RTU3", warehouse.PK, stageLocation.PK);

			Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Arrived, rtu1);
			Helper.CreatePackageState(rcn3, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Arrived, rtu2);

			var rtu1AsInterface = (IWhsItemReceiveTransportationUnit)rtu1;
			var rtu2AsInterface = (IWhsItemReceiveTransportationUnit)rtu2;
			var rtu3AsInterface = (IWhsItemReceiveTransportationUnit)rtu3;

			AssertEquals(rcn2, rtu1AsInterface.LatestReceiveConsignment);
			AssertEquals(rcn3, rtu2AsInterface.LatestReceiveConsignment);
			AssertNull(rtu3AsInterface.LatestReceiveConsignment);
		}

		#endregion

		#region TestCreateJobHeader

		public void TestCreateJobHeader()
		{
			var rtu = Factory.New<WhsItemDispatchTransportationUnit>();
			var header = new JobHeader.Loader(rtu).TryLoadOrCreate();
			AssertEquals(header, rtu.JobHeader);
		}

		#endregion

		#region TestNoteTypes

		public void TestNoteTypes()
		{
			var rtu = Factory.New<WhsItemReceiveTransportationUnit>();
			AssertContainsExactElementsInAnyOrder(new PredefinedNoteType[] {
				PredefinedNoteTypes.Instance.AutoRatingAuditLog,
				PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes,
				PredefinedNoteTypes.Instance.UnmatchedOrgDetails,
				PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes,
				PredefinedNoteTypes.Instance.UnloadLoadNotes }, rtu.NoteTypes);
		}

		#endregion

		#region TestContainedPackageState

		public void TestContainedPackageState()
		{
			var warehouse = Helper.CreateTRWWarehouse("WH1");
			var rtu = helper.CreateReceiveTransportationUnitWithContainerType("Container 1", warehouse.PK, warehouse.DefaultLocation.PK, "Container 1");
			AssertNotNull(rtu.ContainerizedPackageState);
			AssertEquals(true, rtu.ContainerizedPackageState.WPS_IsHandlingUnit);
		}

		#endregion

		#region ReceiveASNsPackageStates

		public void TestCreateASNWithMultipleRCNAllAttachedToASN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);

			var package1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);
			var package2 = Helper.CreatePackageState(rcn1, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);
			var package3 = Helper.CreatePackageState(rcn2, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);
			var package4 = Helper.CreatePackageState(rcn2, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new IWhsItemPackageState[] { package1, package2, package3, package4 }, rtu.ReceiveASNsPackageStates);
		}

		public void TestCreateASNWithMultipleRCNSomeAttachedToASN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);

			var package1OnASN = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);
			var package2OnASN = Helper.CreatePackageState(rcn1, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Booked);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Booked);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new IWhsItemPackageState[] { package1OnASN, package2OnASN }, rtu.ReceiveASNsPackageStates);
		}

		public void TestCreateASNWithMultipleRCNNonAttachedToASN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var asn = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);

			Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked);
			Helper.CreatePackageState(rcn1, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Booked);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Booked);
			Helper.CreatePackageState(rcn2, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Booked);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(Array.Empty<IWhsItemPackageState>(), rtu.ReceiveASNsPackageStates);
		}

		#endregion ReceiveASNsPackageStates

		#region BookedPackagesInPendingASNs

		public void TestBookedPackagesInPendingASNs()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var asn1 = Helper.CreateReceiveASN("ASN1", warehouse.PK);

			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn1.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);

			Factory.Save();

			var packageStates = rtu1.BookedPackagesInPendingASNs;
			AssertEquals("Expected there to be one package state in asn1", 1, packageStates.Count());
			AssertCollectionContains("Expected P1 in dll1", wps1, packageStates);
		}

		public void TestBookedPackagesInPendingASNs_WhenASNHasCompleteTime_ThenExcludeASN()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var asn1 = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			var asn3 = Helper.CreateReceiveASN("ASN3", warehouse.PK);
			asn3.WRP_CompleteTime = ZDateTimeOffset.Now;

			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn1.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn2.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn3.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var rcn4 = Helper.CreateReceiveConsignment("RCN4", warehouse.PK);
			var rcn5 = Helper.CreateReceiveConsignment("RCN5", warehouse.PK);

			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);
			var wps2 = Helper.CreatePackageState(rcn2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);
			var wps3 = Helper.CreatePackageState(rcn3, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn2);
			var wps4 = Helper.CreatePackageState(rcn4, 1, "PKG", "P4", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn3);
			var wps5 = Helper.CreatePackageState(rcn5, 1, "PLT", "P5", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn3);

			Factory.Save();

			var packageStates = rtu1.BookedPackagesInPendingASNs;
			AssertEquals("Expected there to be three package states in asn1 and asn2", 3, packageStates.Count());
			AssertCollectionContains("Expected P1 in asn1", wps1, packageStates);
			AssertCollectionContains("Expected P2 in asn1", wps2, packageStates);
			AssertCollectionContains("Expected P3 in asn2", wps3, packageStates);
		}

		public void TestBookedPackagesInPendingASNs_WhenThereArePackagesOfDifferentStatus_ThenOnlyIncludePackagesWithBookedStatus()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var asn1 = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);

			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn1.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn2.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var rcn4 = Helper.CreateReceiveConsignment("RCN4", warehouse.PK);

			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Committed, receiveASN: asn1, receiveUnit: rtu1);
			var wps2 = Helper.CreatePackageState(rcn2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Arrived, receiveASN: asn1, receiveUnit: rtu1);
			var wps3 = Helper.CreatePackageState(rcn3, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn2);
			var wps4 = Helper.CreatePackageState(rcn4, 1, "PLT", "P4", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn2);

			Factory.Save();

			var packageStates = rtu1.BookedPackagesInPendingASNs;
			AssertEquals("Expected there to be two package states in asn1", 2, packageStates.Count());
			AssertCollectionContains("Expected P3 in asn1", wps3, packageStates);
			AssertCollectionContains("Expected P4 in asn1", wps4, packageStates);
		}

		public void TestBookedPackagesInPendingASNs_GivenNoPendingASNs_ThenReturnEmptyList()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var stageLocation = warehouse.DefaultInboundDockDoorLocation;
			var rtu1 = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, stageLocation.PK);
			var asn1 = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			var asn3 = Helper.CreateReceiveASN("ASN3", warehouse.PK);
			asn1.WRP_CompleteTime = ZDateTimeOffset.Now;
			asn2.WRP_CompleteTime = ZDateTimeOffset.Now;
			asn3.WRP_CompleteTime = ZDateTimeOffset.Now;

			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn1.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn2.PK);
			Helper.CreateReceiveASNRTUPivot(rtu1.PK, asn3.PK);

			var rcn1 = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rcn2 = Helper.CreateReceiveConsignment("RCN2", warehouse.PK);
			var rcn3 = Helper.CreateReceiveConsignment("RCN3", warehouse.PK);
			var rcn4 = Helper.CreateReceiveConsignment("RCN4", warehouse.PK);
			var rcn5 = Helper.CreateReceiveConsignment("RCN5", warehouse.PK);

			var wps1 = Helper.CreatePackageState(rcn1, 1, "PLT", "P1", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);
			var wps2 = Helper.CreatePackageState(rcn2, 1, "PLT", "P2", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn1);
			var wps3 = Helper.CreatePackageState(rcn3, 1, "PLT", "P3", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn2);
			var wps4 = Helper.CreatePackageState(rcn4, 1, "PKG", "P4", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn3);
			var wps5 = Helper.CreatePackageState(rcn5, 1, "PLT", "P5", TransitWarehouseStatuses.Codes.Booked, receiveASN: asn3);

			Factory.Save();

			var packageStates = rtu1.BookedPackagesInPendingASNs;
			AssertEquals("Expected no packages as there are no pending ASNs", 0, packageStates.Count());
		}

		#endregion

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		#region TestIDocumentSupportable

		public void TestIDocumentSupportable()
		{
			var unit = Factory.New<WhsItemReceiveTransportationUnit>();
			AssertNotNull(((IDocumentSupportable)unit).DocumentSupporter);
		}

		#endregion
	}

	#region WhsItemReceiveTransportationUnitWorkflowProviderTest

	[TestedType(typeof(WhsItemReceiveTransportationUnit))]
	public class WhsItemReceiveTransportationUnitWorkflowProviderTest : WorkflowProviderTest<WhsItemReceiveTransportationUnit, WhsItemReceiveTransportationUnitProcessTaskCollection>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.TransitReceiveTransportationUnit; }
		}
	}

	#endregion
}
