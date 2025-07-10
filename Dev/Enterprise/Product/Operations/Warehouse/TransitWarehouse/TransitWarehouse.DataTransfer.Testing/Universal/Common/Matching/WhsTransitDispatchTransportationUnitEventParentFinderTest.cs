using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal.Testing
{
	public class WhsTransitDispatchTransportationUnitEventParentFinderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region TestGateInEvent

		public void TestGateInEvent_CannotFindMatchingDTULinkedToGateMovementBooking_ThrowsDataObjectReadFailureException()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Matching DTU by GateMovementBooking - GBM000000010 not found.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateInEvent_DirectionContextIsNotProvided_ThrowsDataObjectReadFailureException()
		{
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, null, "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Direction context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Direction context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent2));
		}

		public void TestGateInEvent_MovementBookingNumberContextIsNotProvided_ThrowsDataObjectReadFailureException()
		{
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", null, eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "MovementBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "MovementBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent2));
		}

		public void TestGateInEvent_GateBookingNumberContextIsNotProvided_ThrowsDataObjectReadFailureException()
		{
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM(null, "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "GateBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("", "GBM000000010", eventTime, "PIC", " | FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "GateBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent2));
		}

		public void TestGateInEvent_DTULinkedToGateMovementBookingAlreadyGatedIn_ThrowsDataObjectReadFailureException()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("1", warehouse.PK);
			dtu.WDH_GateInTime = eventTime;
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "DTU - '1' is already gated into the warehouse.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateInEvent_DTULinkedToGateMovementBooking_SetDTUGateInTime()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("1", warehouse.PK);
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemDispatchTransportationUnit>("Should be a Dispatch Transportation Unit", results[0]);
				var dtu = ((WhsItemDispatchTransportationUnit)results[0]);
				AssertEquals("Gate In time should be set to the event time", xmlEvent.EventTime, dtu.GateInTime);
				AssertContains($"Information - Setting DTU - '{dtu.WDH_ReferenceNumber}' Gate In time to '{xmlEvent.EventTime}'", logger.Logs);

				var dtuEventLog = dtu.Logs.Find(l => l.SL_SE_NKEvent == Events.GateInCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=1", dtuEventLog.SL_Reference);
			});
		}

		public void TestGateInEvent_ContainerDTULinkedToGateMovementBooking_SetContainerAndVehicleDTUGateInTime()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var vehiclceDTU = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			Helper.CreateStmUniversalJobLink(vehiclceDTU.PK.ToGuid(), vehiclceDTU.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));

			var containerDTU1 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, "CNT1");
			containerDTU1.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Booked;
			Helper.CreateStmUniversalJobLink(containerDTU1.PK.ToGuid(), containerDTU1.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			vehiclceDTU.Reload();

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemDispatchTransportationUnit>("Should be a Dispatch Transportation Unit", results[0]);
				var containerDTU = ((WhsItemDispatchTransportationUnit)results[0]);
				AssertEquals("Container DTU reference number should match", "DTU2", containerDTU.WDH_ReferenceNumber);
				AssertEquals("Container DTU Gate In time should be set to the event time", xmlEvent.EventTime, containerDTU.GateInTime);
				AssertEquals("Container DTU package status should be set to 'Gate In'", TransitWarehouseStatuses.Codes.GatedIn, containerDTU.ContainerizedPackageState.WPS_Status);
				AssertContains($"Information - Setting DTU - '{containerDTU.WDH_ReferenceNumber}' Gate In time to '{xmlEvent.EventTime}'", logger.Logs);
				AssertEquals("Vehicle DTU Gate In time should be set to the event time", xmlEvent.EventTime, vehiclceDTU.GateInTime);
				AssertContains($"Information - Setting DTU - '{vehiclceDTU.WDH_ReferenceNumber}' Gate In time to '{xmlEvent.EventTime}'", logger.Logs);

				var containerDTUEventLog = containerDTU.Logs.Find(l => l.SL_SE_NKEvent == Events.GateInCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=ContainerID|REF=DTU2", containerDTUEventLog.SL_Reference);

				var vehiclceDTUEventLog = vehiclceDTU.Logs.Find(l => l.SL_SE_NKEvent == Events.GateInCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=DTU1", vehiclceDTUEventLog.SL_Reference);
			});

			var containerDTU2 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU3", warehouse.PK, "CNT2");
			containerDTU2.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Booked;
			Helper.CreateStmUniversalJobLink(containerDTU2.PK.ToGuid(), containerDTU2.TablePrefix, "GBM000000020", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000020", eventTime.AddHours(2), "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));

			results = finder.GetLogParentsForEvent(xmlEvent2);
			Factory.SaveForTesting();

			vehiclceDTU.Reload();

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemDispatchTransportationUnit>("Should be a Dispatch Transportation Unit", results[0]);
				var containerDTU = ((WhsItemDispatchTransportationUnit)results[0]);
				AssertEquals("Container DTU reference number should match", "DTU3", containerDTU.WDH_ReferenceNumber);
				AssertEquals("Container DTU Gate In time should be set to the event time", xmlEvent2.EventTime, containerDTU.GateInTime);
				AssertContains($"Information - Setting DTU - '{containerDTU.WDH_ReferenceNumber}' Gate In time to '{xmlEvent2.EventTime}'", logger.Logs);
				AssertEquals("Vehicle DTU Gate In time should remain equal to the first container Gate In time and not be updated", xmlEvent.EventTime, vehiclceDTU.GateInTime);
			});
		}

		#endregion

		#region TestGateOutEvent

		public void TestGateOutEvent_CannotFindMatchingDTULinkedToGateMovementBooking_ThrowsDataObjectReadFailureException()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Matching DTU by GateMovementBooking - GBM000000010 not found.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateOutEvent_DirectionContextIsNotProvided_ThrowsDataObjectReadFailureException()
		{
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, null, "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Direction context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Direction context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent2));
		}

		public void TestGateOutEvent_MovementBookingNumberContextIsNotProvided_ThrowsDataObjectReadFailureException()
		{
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", null, eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "MovementBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "MovementBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent2));
		}
		public void TestGateOutEvent_GateBookingNumberContextIsNotProvided_ThrowsDataObjectReadFailureException()
		{
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM(null, "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "GateBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("", "GBM000000010", eventTime, "PIC", " | FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "GateBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent2));
		}

		public void TestGateOutEvent_DTULinkedToGateMovementBooking_NotYetGatedIn_ThrowsDataObjectReadFailureException()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("1", warehouse.PK);
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "DTU is not yet gated into the warehouse.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateOutEvent_DTULinkedToGateMovementBooking_NotYetLoaded_ThrowsDataObjectReadFailureException()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("1", warehouse.PK);
			dtu.WDH_GateInTime = eventTime;
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "DTU is not yet loaded.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateOutEvent_DTULinkedToGateMovementBooking_AlreadyGatedOut_ThrowsDataObjectReadFailureException()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("1", warehouse.PK);
			dtu.WDH_GateInTime = eventTime.AddHours(-2);
			dtu.WDH_LoadCompleteTime = eventTime.AddHours(-1);
			dtu.WDH_GateOutTime = eventTime;
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "DTU is already gated out of the warehouse.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateOutEvent_ContainerDTULinkedToGateMovementBooking_VehicleDTUNotLoadCompleted_ThrowsDataObjectReadFailureException()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var vehiclceDTU = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			vehiclceDTU.WDH_GateInTime = eventTime.AddHours(-2);
			Helper.CreateStmUniversalJobLink(vehiclceDTU.PK.ToGuid(), vehiclceDTU.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));

			var containerDTU1 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, "CNT1");
			containerDTU1.WDH_GateInTime = eventTime.AddHours(-2);
			containerDTU1.WDH_LoadCompleteTime = eventTime.AddHours(-1);
			Helper.CreateStmUniversalJobLink(containerDTU1.PK.ToGuid(), containerDTU1.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Vehicle DTU has not completed loading yet.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateOutEvent_ContainerDTULinkedToGateMovementBooking_ContainerNotLoadedOntoExpectedVehicle_ThrowsDataObjectReadFailureException()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);

			var vehiclceDTU = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			vehiclceDTU.WDH_GateInTime = eventTime.AddHours(-2);
			vehiclceDTU.WDH_LoadCompleteTime = eventTime.AddHours(-1);
			Helper.CreateStmUniversalJobLink(vehiclceDTU.PK.ToGuid(), vehiclceDTU.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));

			var differentVehiclceDTU = Helper.CreateDispatchTransportationUnit("OtherDTU1", warehouse.PK);
			vehiclceDTU.WDH_GateInTime = eventTime.AddHours(-2);
			vehiclceDTU.WDH_LoadCompleteTime = eventTime.AddHours(-1);

			var containerDTU1 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, "CNT1");
			containerDTU1.WDH_GateInTime = eventTime.AddHours(-2);
			containerDTU1.WDH_LoadCompleteTime = eventTime.AddHours(-1);
			containerDTU1.ContainerizedPackageState.WPS_WDL_LoadList = dll2.PK;
			containerDTU1.ContainerizedPackageState.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Secured;
			containerDTU1.ContainerizedPackageState.WPS_WDH_TransitDispatchHeader = differentVehiclceDTU.PK;
			Helper.CreateStmUniversalJobLink(containerDTU1.PK.ToGuid(), containerDTU1.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Container/ULD is not loaded onto the expected vehicle.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateOutEvent_DTULinkedToGateMovementBooking_SetDTUGateOutTime()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var dtu = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			dtu.WDH_GateInTime = eventTime.AddHours(-2);
			dtu.WDH_LoadCompleteTime = eventTime.AddHours(-1);
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			var hu = Helper.CreateHandlingUnitPackage("HU1", Helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.FreightLoaded, unitType: PackageStateUnitType.Codes.HandlingUnit, dtu: dtu, dll: dll);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dcn, dispatchUnit: dtu, dispatchLoadList: dll);

			Helper.PackPackageIntoHandlingUnit(hu, packageState1, ZDateTimeOffset.Now, "ABC", hu);
			Helper.PackPackageIntoHandlingUnit(hu, packageState2, ZDateTimeOffset.Now, "ABC", hu);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemDispatchTransportationUnit>("Should be a Dispatch Transportation Unit", results[0]);
				var dtu = ((WhsItemDispatchTransportationUnit)results[0]);
				AssertEquals("Gate Out time should be set to the event time", xmlEvent.EventTime, dtu.WDH_GateOutTime);
				AssertContains($"Information - Setting DTU - '{dtu.WDH_ReferenceNumber}' Gate Out time to '{xmlEvent.EventTime}'.", logger.Logs);

				var dtuEventLog = dtu.Logs.Find(l => l.SL_SE_NKEvent == Events.GateOutCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=DTU1", dtuEventLog.SL_Reference);

				AssertEquals("DTU must have 3 child package states", 3, dtu.PackageStates.Count);
				Assert("All DTU Child PackageStates status must set to departed", dtu.PackageStates.All(p => p.WPS_Status == "DEP"));

				var departedEventLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DepartureCode);
				departedEventLogQuery.AddToFilter(StmALogSchema.SL_Reference, "DTU1|FAC=CFS|LOC=Johannesburg|RES=Scanned|RFN=DCN1|TYP=VehicleReference|WHS=TWH");
				Assert("All DTU Child PackageStates must have a departed event", dtu.PackageStates.All(p => p.Package.Logs.Find(departedEventLogQuery).Any()));
			});
		}

		public void TestGateOutEvent_ContainerDTULinkedToGateMovementBooking_SetContainerAndVehicleDTUGateOutTime()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL2", warehouse.PK);

			var vehiclceDTU = Helper.CreateDispatchTransportationUnit("DTU1", warehouse.PK);
			vehiclceDTU.WDH_GateInTime = eventTime.AddHours(-2);
			vehiclceDTU.WDH_LoadCompleteTime = eventTime.AddHours(-1);
			Helper.CreateStmUniversalJobLink(vehiclceDTU.PK.ToGuid(), vehiclceDTU.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));

			var hu1 = Helper.CreateHandlingUnitPackage("HU1", Helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.FreightLoaded, unitType: PackageStateUnitType.Codes.HandlingUnit, dtu: vehiclceDTU, dll: dll);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dcn, dispatchUnit: vehiclceDTU, dispatchLoadList: dll);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dcn, dispatchUnit: vehiclceDTU, dispatchLoadList: dll);

			Helper.PackPackageIntoHandlingUnit(hu1, packageState1, ZDateTimeOffset.Now, "ABC", hu1);
			Helper.PackPackageIntoHandlingUnit(hu1, packageState2, ZDateTimeOffset.Now, "ABC", hu1);

			var containerDTU1 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU2", warehouse.PK, "CNT1");
			containerDTU1.WDH_GateInTime = eventTime.AddHours(-2);
			containerDTU1.WDH_LoadCompleteTime = eventTime.AddHours(-1);
			containerDTU1.ContainerizedPackageState.WPS_WDL_LoadList = dll2.PK;
			containerDTU1.ContainerizedPackageState.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Secured;
			containerDTU1.ContainerizedPackageState.WPS_WDH_TransitDispatchHeader = vehiclceDTU.PK;
			Helper.CreateStmUniversalJobLink(containerDTU1.PK.ToGuid(), containerDTU1.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			var hu2 = Helper.CreateHandlingUnitPackage("HU2", Helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.FreightLoaded, unitType: PackageStateUnitType.Codes.HandlingUnit, dtu: containerDTU1, dll: dll);

			var packageState3 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-3", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dcn, dispatchUnit: containerDTU1, dispatchLoadList: dll);
			var packageState4 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-4", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dcn, dispatchUnit: containerDTU1, dispatchLoadList: dll);

			Helper.PackPackageIntoHandlingUnit(hu2, packageState3, ZDateTimeOffset.Now, "ABC", hu2);
			Helper.PackPackageIntoHandlingUnit(hu2, packageState4, ZDateTimeOffset.Now, "ABC", hu2);

			var containerDTU2 = Helper.CreateDispatchTransportationUnitWithContainerType("DTU3", warehouse.PK, "CNT2");
			containerDTU2.WDH_GateInTime = eventTime.AddHours(-2);
			containerDTU2.WDH_LoadCompleteTime = eventTime.AddHours(-1);
			containerDTU2.ContainerizedPackageState.WPS_WDL_LoadList = dll2.PK;
			containerDTU2.ContainerizedPackageState.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Secured;
			containerDTU2.ContainerizedPackageState.WPS_WDH_TransitDispatchHeader = vehiclceDTU.PK;
			Helper.CreateStmUniversalJobLink(containerDTU2.PK.ToGuid(), containerDTU2.TablePrefix, "GBM000000020", nameof(DataContextType.GateMovementBooking));

			var hu3 = Helper.CreateHandlingUnitPackage("HU3", Helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.FreightLoaded, unitType: PackageStateUnitType.Codes.HandlingUnit, dtu: containerDTU2, dll: dll);

			var packageState5 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-5", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dcn, dispatchUnit: containerDTU2, dispatchLoadList: dll);
			var packageState6 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-6", TransitWarehouseStatuses.Codes.FreightLoaded, rtu, dispatchConsignment: dcn, dispatchUnit: containerDTU2, dispatchLoadList: dll);

			Helper.PackPackageIntoHandlingUnit(hu3, packageState5, ZDateTimeOffset.Now, "ABC", hu3);
			Helper.PackPackageIntoHandlingUnit(hu3, packageState6, ZDateTimeOffset.Now, "ABC", hu3);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			vehiclceDTU.Reload();

			var departedEventLogQueryForContainerizedPackageState = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DepartureCode);
			departedEventLogQueryForContainerizedPackageState.AddToFilter(StmALogSchema.SL_Reference, "DTU1|FAC=CFS|LOC=Johannesburg|RES=Scanned|TYP=ContainerID|WHS=TWH");

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemDispatchTransportationUnit>("Should be a Dispatch Transportation Unit", results[0]);
				var containerDTU = ((WhsItemDispatchTransportationUnit)results[0]);
				AssertEquals("Container DTU reference number should match", "DTU2", containerDTU.WDH_ReferenceNumber);
				AssertEquals("Container DTU Gate Out time should be set to the event time", xmlEvent.EventTime, containerDTU.WDH_GateOutTime);
				AssertContains($"Information - Setting DTU - '{containerDTU.WDH_ReferenceNumber}' Gate Out time to '{xmlEvent.EventTime}'.", logger.Logs);
				AssertEquals("Vehicle DTU Gate Out time should be set to the event time", xmlEvent.EventTime, vehiclceDTU.WDH_GateOutTime);
				AssertContains($"Information - Setting DTU - '{vehiclceDTU.WDH_ReferenceNumber}' Gate Out time to '{xmlEvent.EventTime}'.", logger.Logs);

				var containerEventLog = containerDTU.Logs.Find(l => l.SL_SE_NKEvent == Events.GateOutCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=ContainerID|REF=DTU2", containerEventLog.SL_Reference);
				var vehicleEventLog = vehiclceDTU.Logs.Find(l => l.SL_SE_NKEvent == Events.GateOutCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=DTU1", vehicleEventLog.SL_Reference);

				var containerDTUPkgState = containerDTU.ContainerizedPackageState;
				var departedEventLogQueryForConatinerInnerPackages = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DepartureCode);
				departedEventLogQueryForConatinerInnerPackages.AddToFilter(StmALogSchema.SL_Reference, "DTU2|FAC=CFS|LOC=Johannesburg|RES=Scanned|RFN=DCN1|TYP=VehicleReference|WHS=TWH");

				var departedEventLogQueryForVehicleInnerPackages = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DepartureCode);
				departedEventLogQueryForVehicleInnerPackages.AddToFilter(StmALogSchema.SL_Reference, "DTU1|FAC=CFS|LOC=Johannesburg|RES=Scanned|RFN=DCN1|TYP=VehicleReference|WHS=TWH");

				AssertEquals("Container DTU Containerized PackageState status should be set as departed", TransitWarehouseStatuses.Codes.Departed, containerDTUPkgState.WPS_Status);
				Assert("Container DTU Containerized PackageState must have a departed event", containerDTUPkgState.Package.Logs.Find(departedEventLogQueryForContainerizedPackageState).Any());
				AssertEquals("Container DTU must have 3 child package states", 3, containerDTU.PackageStates.Count);
				Assert("All Container DTU Child PackageStates status must set to departed", containerDTU.PackageStates.All(p => p.WPS_Status == "DEP"));
				Assert("All Container DTU Child PackageStates must have a departed event", containerDTU.PackageStates.All(p => p.Package.Logs.Find(departedEventLogQueryForConatinerInnerPackages).Any()));

				AssertEquals("Vehicle DTU must have 5 child package state", 5, vehiclceDTU.PackageStates.Count);
				Assert("All Vehicle DTU Non-Containerized Child PackageStates status must set to departed", vehiclceDTU.PackageStates.Where(ps => ps.WPS_UnitType != PackageStateUnitType.Codes.SeaContainer).All(p => p.WPS_Status == "DEP"));
				Assert("All Vehicle DTU Non-Containerized Child PackageStates must have a departed event", vehiclceDTU.PackageStates.Where(ps => ps.WPS_UnitType != PackageStateUnitType.Codes.SeaContainer).All(p => p.Package.Logs.Find(departedEventLogQueryForVehicleInnerPackages).Any()));
			});

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000020", eventTime.AddHours(2), "PIC", "|FAC=WDH|GIN=GVE00000542|DIR=PIC|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));

			results = finder.GetLogParentsForEvent(xmlEvent2);
			Factory.SaveForTesting();

			vehiclceDTU.Reload();

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemDispatchTransportationUnit>("Should be a Dispatch Transportation Unit", results[0]);
				var containerDTU = ((WhsItemDispatchTransportationUnit)results[0]);
				AssertEquals("Container DTU reference number should match", "DTU3", containerDTU.WDH_ReferenceNumber);
				AssertEquals("Container DTU Gate Out time should be set to the event time", xmlEvent2.EventTime, containerDTU.WDH_GateOutTime);
				AssertContains($"Information - Setting DTU - '{containerDTU.WDH_ReferenceNumber}' Gate Out time to '{xmlEvent2.EventTime}'.", logger.Logs);
				AssertEquals("Vehicle DTU Gate Out time should remain equal to the first container Gate Out time and not be updated", xmlEvent.EventTime, vehiclceDTU.WDH_GateOutTime);

				var containerEventLog = containerDTU.Logs.Find(l => l.SL_SE_NKEvent == Events.GateOutCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=ContainerID|REF=DTU3", containerEventLog.SL_Reference);

				var containerDTUPkgState = containerDTU.ContainerizedPackageState;
				var departedEventLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DepartureCode);
				departedEventLogQuery.AddToFilter(StmALogSchema.SL_Reference, "DTU3|FAC=CFS|LOC=Johannesburg|RES=Scanned|RFN=DCN1|TYP=VehicleReference|WHS=TWH");

				AssertEquals("Container DTU Containerized PackageState status should be set as departed", TransitWarehouseStatuses.Codes.Departed, containerDTUPkgState.WPS_Status);
				Assert("Container DTU Containerized PackageState must have a departed event", containerDTUPkgState.Package.Logs.Find(departedEventLogQueryForContainerizedPackageState).Any());
				AssertEquals("Container DTU must have 3 child package states", 3, containerDTU.PackageStates.Count);
				Assert("All Container DTU Child PackageStates status must set to departed", containerDTU.PackageStates.All(p => p.WPS_Status == "DEP"));
				Assert("All Container DTU Child PackageStates must have a departed event", containerDTU.PackageStates.All(p => p.Package.Logs.Find(departedEventLogQuery).Any()));

				AssertEquals("Vehicle DTU must have 5 child package state", 5, vehiclceDTU.PackageStates.Count);
				Assert("All Vehicle DTU Child PackageStates status must set to departed", vehiclceDTU.PackageStates.All(p => p.WPS_Status == "DEP"));
			});
		}

		#endregion

		#region TestBKLEvent

		public void TestCancelBookingEvent_CannotFindMatchingDTULinkedToGateMovementBooking()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGateBKLEvent("GBM000000010", nameof(AddressType.PIC)));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Matching DTU by GateMovementBooking - GBM000000010 not found.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCancelBookingEvent_DTULinkedToGateMovementBookingAlreadyGatedIn()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("DTU0001", warehouse.PK);
			dtu.WDH_GateInTime = new ZDateTimeOffset(2020, 12, 22, 10, 0, 0);
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGateBKLEvent("GBM000000010", nameof(AddressType.PIC)));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "DTU - 'DTU0001' is already gated into the warehouse.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCancelBookingEvent_CannotFindMatchingDTU_MovementNumberIsEmpty()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGateBKLEvent(string.Empty, nameof(AddressType.PIC)));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Movement booking number must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCancelBookingEvent_DTULinkedToGateMovementBooking_CancelBookingAndDetachDLL()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("DTU0001", warehouse.PK);
			var dll1 = Helper.CreateDispatchLoadList("DLL0001", warehouse.PK);
			var dll2 = Helper.CreateDispatchLoadList("DLL0002", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(dll1.PK, dtu.PK);
			Helper.CreateDispatchDLLDTUPivot(dll2.PK, dtu.PK);
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var pivots = Factory.Load<WhsItemDispatchLoadListDTUPivot>(new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, dtu.PK));
			AssertEquals("Precondition: There should be 2 DLL linked to the DTU", 2, pivots.Length);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGateBKLEvent("GBM000000010", nameof(AddressType.PIC)));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			var reloadedDTU = Factory.Load<WhsItemDispatchTransportationUnit>(dtu.PK);
			var reloadedPivots = Factory.Load<WhsItemDispatchLoadListDTUPivot>(new ZQuery(WhsItemDispatchLoadListDTUPivotSchema.WLD_WDH_TransitDispatchTransportationUnit, dtu.PK));
			var reloadedDLL1 = Factory.Load<WhsItemDispatchLoadList>(dll1.PK);
			var reloadedDLL2 = Factory.Load<WhsItemDispatchLoadList>(dll2.PK);

			CombineAssertions(() =>
			{
				AssertNotNull("Output should not be null", results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemDispatchTransportationUnit>("Should be a Dispatch Transportation Unit", results[0]);
				AssertEquals("Should be the matched DTU", dtu.PK, ((WhsItemDispatchTransportationUnit)results[0]).PK);
				AssertNotNull("DTU should not be deleted", reloadedDTU);
				Assert("DTU booking should have been cancelled", reloadedDTU.WDH_IsBookingCancelled);
				AssertEquals("DLL should have been detached", 0, reloadedPivots.Length);
				AssertEquals("DLL should not be deleted", "DLL0001", reloadedDLL1.WDL_ReferenceNumber);
				AssertEquals("DLL should not be deleted", "DLL0002", reloadedDLL2.WDL_ReferenceNumber);
				AssertEquals(@"Information - Canceling booked DTU - 'DTU0001'.
Information - Detaching DTU - 'DTU0001' planned DLLs.", logger.Logs);
				var dtuEventLog = dtu.Logs.Find(l => l.SL_SE_NKEvent == Events.BookingCancelledCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=DTU0001", dtuEventLog.SL_Reference);
			});
		}

		#endregion

		#region TestCNCEvent

		public void TestCNCEvent_CannotFindMatchingDTULinkedToGateMovementBooking()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.PIC), true));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Matching DTU by GateMovementBooking - GBM000000010 not found.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCNCEvent_CancelGateIn_DTULinkedToGateMovementBookingNotGatedIn()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("DTU0001", warehouse.PK);
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.PIC), true));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "DTU is not yet gated into the warehouse.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCNCEvent_CancelGateIn_DTULinkedToGateMovementBooking_DTUHasStartedToLoad()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("DTU0001", warehouse.PK);
			dtu.WDH_GateInTime = eventTime.AddHours(-2);
			dtu.WDH_LoadStartTime = eventTime.AddHours(2);
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.PIC), true));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "DTU has started to load.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCNCEvent_CancelGateIn_Vehicle()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("DTU0001", warehouse.PK);
			dtu.WDH_GateInTime = eventTime.AddHours(-2);

			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.PIC), true, "GB000000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			var reloadedDTU = Factory.Load<WhsItemDispatchTransportationUnit>(dtu.PK);
			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemDispatchTransportationUnit>("Should be a Dispatch Transportation Unit", results[0]);
				AssertEquals("Should find the matched DTU", reloadedDTU.PK, ((WhsItemDispatchTransportationUnit)results[0]).PK);
				AssertEquals("DTU gate in should be cancelled", ZDateTimeOffset.Empty, reloadedDTU.WDH_GateInTime);
				AssertContains("Cancel DTU - 'DTU0001' Gate In Succeeded.", logger.Logs);

				var dtuEventLog = reloadedDTU.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=DTU0001|EVT=GIN", dtuEventLog.SL_Reference);
			});
		}

		public void TestCNCEvent_CancelGateIn_Container_CancelVehicleGateIn() => TestCNCEvent_CancelGateIn_Container(false);

		public void TestCNCEvent_CancelGateIn_Container_CannotCancelVehicleGateIn() => TestCNCEvent_CancelGateIn_Container(true);

		void TestCNCEvent_CancelGateIn_Container(bool vehicleHasOtherContainerGatedIn)
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var containerDTU = Helper.CreateDispatchTransportationUnitWithContainerType("DTUCNT1", warehouse.PK, "CNT1");
			containerDTU.WDH_GateInTime = eventTime.AddHours(-2);
			containerDTU.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.GatedIn;
			Helper.CreateStmUniversalJobLink(containerDTU.PK.ToGuid(), containerDTU.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			var vehicleDTU = Helper.CreateDispatchTransportationUnit("DTU0002", warehouse.PK);
			vehicleDTU.WDH_GateInTime = eventTime.AddHours(-3);
			Helper.CreateStmUniversalJobLink(vehicleDTU.PK.ToGuid(), vehicleDTU.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));

			var anotherContainerDTU = Helper.CreateDispatchTransportationUnitWithContainerType("DTUCNT2", warehouse.PK, "CNT2");
			if (vehicleHasOtherContainerGatedIn)
			{
				anotherContainerDTU.WDH_GateInTime = eventTime.AddHours(-2);
				anotherContainerDTU.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.GatedIn;
			}

			var loadList = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);
			Helper.CreateDispatchDLLDTUPivot(loadList.PK, vehicleDTU.PK);
			loadList.PackageStates.Add(containerDTU.ContainerizedPackageState);
			loadList.PackageStates.Add(anotherContainerDTU.ContainerizedPackageState);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.PIC), true, "GB000000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			var reloadedContainerDTU = Factory.Load<WhsItemDispatchTransportationUnit>(containerDTU.PK);
			var reloadedVehicleDTU = Factory.Load<WhsItemDispatchTransportationUnit>(vehicleDTU.PK);

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemDispatchTransportationUnit>("Should be a Dispatch Transportation Unit", results[0]);
				AssertEquals("Should find the matched DTU", reloadedContainerDTU.PK, ((WhsItemDispatchTransportationUnit)results[0]).PK);
				AssertEquals("Container DTU gate in should be cancelled", ZDateTimeOffset.Empty, reloadedContainerDTU.WDH_GateInTime);
				AssertEquals("Container DTU package status should be set as Booked", TransitWarehouseStatuses.Codes.Booked, reloadedContainerDTU.ContainerizedPackageState.WPS_Status);
				AssertContains("Cancel DTU - 'DTUCNT1' Gate In Succeeded.", logger.Logs);

				var containerDTUEventLog = reloadedContainerDTU.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=ContainerID|REF=DTUCNT1|EVT=GIN", containerDTUEventLog.SL_Reference);

				var vehicleDTUEventLog = reloadedVehicleDTU.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).FirstOrDefault();
				if (vehicleHasOtherContainerGatedIn)
				{
					AssertEquals("Vehicle DTU gate in should remain unchanged", eventTime.AddHours(-3), reloadedVehicleDTU.WDH_GateInTime);
					AssertNull(vehicleDTUEventLog);
				}
				else
				{
					AssertEquals("Vehicle DTU gate in should be cancelled", ZDateTimeOffset.Empty, reloadedVehicleDTU.WDH_GateInTime);
					AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=DTU0002|EVT=GIN", vehicleDTUEventLog.SL_Reference);
				}
			});
		}

		public void TestCNCEvent_CancelGateOut_DTULinkedToGateMovementBookingFinalised()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("DTU0001", warehouse.PK);
			dtu.WDH_GateInTime = eventTime.AddHours(-3);
			dtu.WDH_LoadCompleteTime = eventTime.AddHours(-2);
			dtu.WDH_GateOutTime = eventTime.AddHours(-1);
			dtu.WDH_FinalisedTime = eventTime;

			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.PIC), false));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "DTU has been finalised.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCNCEvent_CancelGateOut_DTULinkedToGateMovementBookingNotGatedOut()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("DTU0001", warehouse.PK);
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.PIC), false));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "DTU is not yet gated out of the warehouse.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCNCEvent_CancelGateOut_DTULinkedToGateMovementBookingGatedOutMoreThan30DaysAgo()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("DTU0001", warehouse.PK);
			dtu.WDH_GateInTime = eventTime.AddDays(-30).AddHours(-3);
			dtu.WDH_LoadCompleteTime = eventTime.AddDays(-30).AddHours(-2);
			dtu.WDH_GateOutTime = eventTime.AddDays(-30).AddHours(-1);

			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.PIC), false));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "DTU has gated out more than 30 days ago.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCNCEvent_CancelGateOut_Vehicle()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var dtu = Helper.CreateDispatchTransportationUnit("DTU0001", warehouse.PK);
			dtu.WDH_GateInTime = eventTime.AddHours(-2);
			dtu.WDH_LoadCompleteTime = eventTime.AddHours(-1);
			dtu.WDH_GateOutTime = eventTime;

			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(dtu.PK.ToGuid(), dtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.PIC), false, "GB000000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			var reloadedDTU = Factory.Load<WhsItemDispatchTransportationUnit>(dtu.PK);
			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemDispatchTransportationUnit>("Should be a Dispatch Transportation Unit", results[0]);
				AssertEquals("Should find the matched DTU", reloadedDTU.PK, ((WhsItemDispatchTransportationUnit)results[0]).PK);
				AssertEquals("DTU gate out should be cancelled", ZDateTimeOffset.Empty, reloadedDTU.WDH_GateOutTime);
				AssertContains("Cancel DTU - 'DTU0001' Gate Out Succeeded", logger.Logs);

				var dtuEventLog = reloadedDTU.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=DTU0001|EVT=GOU", dtuEventLog.SL_Reference);

				var freightLoadedEventLogQueryForInnerPackages = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.FreightLoadedCode);
				freightLoadedEventLogQueryForInnerPackages.AddToFilter(StmALogSchema.SL_Reference, "DTU0001|FAC=CFS|LOC=Johannesburg|RES=Scanned|RFN=DCN1|TYP=VehicleReference|WHS=TWH");

				Assert("All DTU Child PackageStates status must set to freight loaded", reloadedDTU.PackageStates.All(p => p.WPS_Status == "FLO"));
				Assert("All DTU Child PackageStates must have a freight loaded event", reloadedDTU.PackageStates.All(p => p.Package.Logs.Find(freightLoadedEventLogQueryForInnerPackages).Any()));
			});
		}

		public void TestCNCEvent_CancelGateOut_Container_CancelVehicleGateOut() => TestCNCEvent_CancelGateOut_Container(false);

		public void TestCNCEvent_CancelGateOut_Container_CannotCancelVehicleGateOut() => TestCNCEvent_CancelGateOut_Container(true);

		void TestCNCEvent_CancelGateOut_Container(bool vehicleHasOtherContainerGatedOut)
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			var dcn = Helper.CreateDispatchConsignment("DCN1", warehouse.PK);
			var dll = Helper.CreateDispatchLoadList("DLL1", warehouse.PK);

			var containerDTU = Helper.CreateDispatchTransportationUnitWithContainerType("DTUCNT1", warehouse.PK, "CNT1");
			containerDTU.WDH_GateInTime = eventTime.AddHours(-2);
			containerDTU.WDH_LoadCompleteTime = eventTime.AddHours(-1);
			containerDTU.WDH_GateOutTime = eventTime;
			containerDTU.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Departed;
			containerDTU.ContainerizedPackageState.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Secured;

			Helper.CreateStmUniversalJobLink(containerDTU.PK.ToGuid(), containerDTU.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			var hu1 = Helper.CreateHandlingUnitPackage("HU1", Helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.Departed, unitType: PackageStateUnitType.Codes.HandlingUnit, dtu: containerDTU, dll: dll);

			var packageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchUnit: containerDTU, dispatchLoadList: dll);
			var packageState2 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-2", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchUnit: containerDTU, dispatchLoadList: dll);

			Helper.PackPackageIntoHandlingUnit(hu1, packageState1, ZDateTimeOffset.Now, "ABC", hu1);
			Helper.PackPackageIntoHandlingUnit(hu1, packageState2, ZDateTimeOffset.Now, "ABC", hu1);

			var vehicleDTU = Helper.CreateDispatchTransportationUnit("DTU0002", warehouse.PK);
			vehicleDTU.WDH_GateInTime = eventTime.AddHours(-2);
			vehicleDTU.WDH_LoadCompleteTime = eventTime.AddHours(-1);
			vehicleDTU.WDH_GateOutTime = eventTime;
			Helper.CreateStmUniversalJobLink(vehicleDTU.PK.ToGuid(), vehicleDTU.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));

			var hu2 = Helper.CreateHandlingUnitPackage("HU2", Helper.CreatePackageHandlingUnit(), rtu, TransitWarehouseStatuses.Codes.Departed, unitType: PackageStateUnitType.Codes.HandlingUnit, dtu: vehicleDTU, dll: dll);

			var packageState3 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-3", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchUnit: vehicleDTU, dispatchLoadList: dll);
			var packageState4 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-4", TransitWarehouseStatuses.Codes.Departed, rtu, dispatchConsignment: dcn, dispatchUnit: vehicleDTU, dispatchLoadList: dll);

			Helper.PackPackageIntoHandlingUnit(hu2, packageState3, ZDateTimeOffset.Now, "ABC", hu2);
			Helper.PackPackageIntoHandlingUnit(hu2, packageState4, ZDateTimeOffset.Now, "ABC", hu2);

			var anotherContainerDTU = Helper.CreateDispatchTransportationUnitWithContainerType("DTUCNT2", warehouse.PK, "CNT2");
			anotherContainerDTU.ContainerizedPackageState.WPS_SecurityStatus = TransitWarehouseSecurityStatuses.Codes.Secured;
			if (vehicleHasOtherContainerGatedOut)
			{
				anotherContainerDTU.WDH_GateInTime = eventTime.AddHours(-2);
				anotherContainerDTU.WDH_LoadCompleteTime = eventTime.AddHours(-1);
				anotherContainerDTU.WDH_GateOutTime = eventTime;
				anotherContainerDTU.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Departed;
			}

			Helper.CreateDispatchDLLDTUPivot(dll.PK, vehicleDTU.PK);
			dll.PackageStates.Add(containerDTU.ContainerizedPackageState);
			dll.PackageStates.Add(anotherContainerDTU.ContainerizedPackageState);

			vehicleDTU.PackageStates.Add(containerDTU.ContainerizedPackageState);
			vehicleDTU.PackageStates.Add(anotherContainerDTU.ContainerizedPackageState);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.PIC), false, "GB000000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			var reloadedContainerDTU = Factory.Load<WhsItemDispatchTransportationUnit>(containerDTU.PK);
			var reloadedVehicleDTU = Factory.Load<WhsItemDispatchTransportationUnit>(vehicleDTU.PK);

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemDispatchTransportationUnit>("Should be a Dispatch Transportation Unit", results[0]);
				AssertEquals("Should find the matched DTU", reloadedContainerDTU.PK, ((WhsItemDispatchTransportationUnit)results[0]).PK);
				AssertEquals("Container DTU Gate Out should be cancelled", ZDateTimeOffset.Empty, reloadedContainerDTU.WDH_GateOutTime);
				AssertEquals("Container DTU package status should be set as Freight Loaded", TransitWarehouseStatuses.Codes.FreightLoaded, reloadedContainerDTU.ContainerizedPackageState.WPS_Status);
				AssertContains("Cancel DTU - 'DTUCNT1' Gate Out Succeeded.", logger.Logs);

				var containerDTUEventLog = reloadedContainerDTU.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=ContainerID|REF=DTUCNT1|EVT=GOU", containerDTUEventLog.SL_Reference);

				var freightLoadedEventLogQueryForConatinerInnerPackages = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.FreightLoadedCode);
				freightLoadedEventLogQueryForConatinerInnerPackages.AddToFilter(StmALogSchema.SL_Reference, "DTUCNT1|FAC=CFS|LOC=Johannesburg|RES=Scanned|RFN=DCN1|TYP=VehicleReference|WHS=TWH");

				Assert("All Container DTU Child PackageStates status must set to freight loaded", reloadedContainerDTU.PackageStates.All(p => p.WPS_Status == "FLO"));
				Assert("All Container DTU Child PackageStates must have a freight loaded event", reloadedContainerDTU.PackageStates.All(p => p.Package.Logs.Find(freightLoadedEventLogQueryForConatinerInnerPackages).Any()));

				var vehicleDTUEventLog = reloadedVehicleDTU.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).FirstOrDefault();
				if (vehicleHasOtherContainerGatedOut)
				{
					AssertEquals("Vehicle DTU Gate Out should remain unchanged", eventTime, reloadedVehicleDTU.WDH_GateOutTime);
					AssertNull(vehicleDTUEventLog);
				}
				else
				{
					AssertEquals("Vehicle DTU Gate Out should be cancelled", ZDateTimeOffset.Empty, reloadedVehicleDTU.WDH_GateOutTime);
					AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=DTU0002|EVT=GOU", vehicleDTUEventLog.SL_Reference);

					var freightLoadedEventLogQueryForVehicleInnerPackages = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.FreightLoadedCode);
					freightLoadedEventLogQueryForVehicleInnerPackages.AddToFilter(StmALogSchema.SL_Reference, "DTU0002|FAC=CFS|LOC=Johannesburg|RES=Scanned|RFN=DCN1|TYP=VehicleReference|WHS=TWH");

					var nonContainerPackageStatesOnVehicle = reloadedVehicleDTU.PackageStates.Where(p => !p.IsContainerizedPackageState);
					Assert("All Vehicle DTU Child PackageStates status must set to freight loaded", nonContainerPackageStatesOnVehicle.All(p => p.WPS_Status == "FLO"));
					Assert("All Vehicle DTU Child PackageStates must have a freight loaded event", nonContainerPackageStatesOnVehicle.All(p => p.Package.Logs.Find(freightLoadedEventLogQueryForVehicleInnerPackages).Any()));
				}
			});
		}

		#endregion

		#region GetFinder 

		protected EventParentFinder GetFinder(IXmlImportLogger logger = null)
		{
			return new WhsTransitDispatchTransportationUnitEventParentFinder(new WhsItemDispatchTransportationUnitDataContextManager(), Factory.BOFactory, logger ?? new TestErrorLogger());
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			ZDateTimeOffset dateTimeOffset = ZDateTimeOffset.Now;
			eventTime = new ZDateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, dateTimeOffset.Minute, 0, 0, dateTimeOffset.Offset);
		}

		protected XmlEventDeserializer EventDeserializer => eventDeserializer ?? (eventDeserializer = new XmlEventDeserializer());
		XmlEventDeserializer eventDeserializer;

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory.BOFactory));
		WhsTransitTestHelper helper;

		protected TestDataForUniversal Data => data ?? (data = new TestDataForUniversal(Factory, new TestErrorLogger()));
		TestDataForUniversal data;

		protected TestHelperForUniversal UniversalHelper => new TestHelperForUniversal(Factory.BOFactory);

		protected WhsLocation CreateLocation(string code, WhsWarehouse warehouse = null)
		{
			return Helper.CreateRowAndGenerateLocations(warehouse ?? Data.Warehouse, code, 1, 1).Locations[0];
		}

		protected ZDateTimeOffset eventTime;

		#endregion
	}
}
