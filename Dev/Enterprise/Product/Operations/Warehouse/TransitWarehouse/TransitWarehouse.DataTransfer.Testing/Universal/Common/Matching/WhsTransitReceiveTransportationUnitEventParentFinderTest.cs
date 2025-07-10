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
	public class WhsTransitReceiveTransportationUnitEventParentFinderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region TestGateInEvent

		public void TestGateInEvent_CannotFindMatchingRTULinkedToGateMovementBooking_ThrowsDataObjectReadFailureException()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|LOC=L1|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Matching RTU by GateMovementBooking - GBM000000010 not found.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateInEvent_DirectionContextIsNotProvided_ThrowsDataObjectReadFailureException()
		{
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, null, "|FAC=WRH|GIN=GVE00000542|DIR=DLV|LOC=L1|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Direction context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|LOC=L1|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Direction context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent2));
		}

		public void TestGateInEvent_MovementBookingNumberContextIsNotProvided_ThrowsDataObjectReadFailureException()
		{
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", null, eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|LOC=L1|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "MovementBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|LOC=L1|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "MovementBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent2));
		}

		public void TestGateInEvent_GateBookingNumberContextIsNotProvided_ThrowsDataObjectReadFailureException()
		{
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM(null, "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|LOC=L1|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "GateBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("", "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|LOC=L1|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "GateBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent2));
		}

		public void TestGateInEvent_DockContextIsNotProvided_ThrowsDataObjectReadFailureException()
		{
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Dock Location is not provided.", () => finder.GetLogParentsForEvent(xmlEvent));

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|LOC=|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Dock Location is not provided.", () => finder.GetLogParentsForEvent(xmlEvent2));
		}

		public void TestGateInEvent_RTULinkedToGateMovementBookingAlreadyGatedIn_ThrowsDataObjectReadFailureException()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("1", warehouse.PK, location.PK, gateIn: eventTime);
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", $"|FAC=WRH|GIN=GVE00000542|DIR=DLV|LOC={location.WLV_LocationString}|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "RTU - '1' is already gated into the warehouse.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateInEvent_RTULinkedToGateMovementBooking_DockLocationNotFound_ThrowsDataObjectReadFailureException()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("1", warehouse.PK, location.PK);
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|LOC=L2|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Matching Dock Location not found.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateInEvent_RTULinkedToGateMovementBooking_RTUAlreadyHasStagingLocationBeforeGateInEvent_SetRTUGateInTime()
		{
			TestGateInEvent_RTULinkedToGateMovementBooking_SetRTUGateInTimeCore(true);
		}

		public void TestGateInEvent_RTULinkedToGateMovementBooking_RTUHasNoStagingLocationBeforeGateInEvent_SetRTUGateInTime()
		{
			TestGateInEvent_RTULinkedToGateMovementBooking_SetRTUGateInTimeCore(false);
		}

		void TestGateInEvent_RTULinkedToGateMovementBooking_SetRTUGateInTimeCore(bool rtuHasStagingLocation)
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);
			var location2 = CreateLocation("L2", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			if (!rtuHasStagingLocation)
			{
				rtu.WRH_WL_StagingLocation = ZGuid.Empty;
			}
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", $"|FAC=WRH|GIN=GVE00000542|DIR=DLV|LOC={location2.WLV_LocationString}|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemReceiveTransportationUnit>("Should be a Receive Transportation Unit", results[0]);
				var rtu = ((WhsItemReceiveTransportationUnit)results[0]);
				AssertEquals("Staging Location should be set or updated", location2.PK, rtu.WRH_WL_StagingLocation);
				AssertEquals("Gate In time should be set to the event time", xmlEvent.EventTime, rtu.GateInTime);
				AssertContains($"Information - Setting RTU - '{rtu.WRH_ReferenceNumber}' Gate In time to '{xmlEvent.EventTime}'.", logger.Logs);

				if (rtuHasStagingLocation)
				{
					AssertContains($"Information - Updating RTU - '{rtu.WRH_ReferenceNumber}' staging location to 'L2' which is the staging Location in Gate In event.", logger.Logs);
				}
				var rtuEventLog = rtu.Logs.Find(l => l.SL_SE_NKEvent == Events.GateInCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=V1", rtuEventLog.SL_Reference);
			});
		}

		public void TestGateInEvent_ContainerRTULinkedToGateMovementBooking_ContainerRTUAlreadyHasStagingLocationBeforeGateInEvent_SetContainerAndVehicleRTUGateInTime()
		{
			TestGateInEvent_ContainerRTULinkedToGateMovementBooking_SetContainerAndVehicleRTUGateInTimeCore(true);
		}

		public void TestGateInEvent_ContainerRTULinkedToGateMovementBooking_ContainerRTUHasNoStagingLocationBeforeGateInEvent_SetContainerAndVehicleRTUGateInTime()
		{
			TestGateInEvent_ContainerRTULinkedToGateMovementBooking_SetContainerAndVehicleRTUGateInTimeCore(false);
		}

		void TestGateInEvent_ContainerRTULinkedToGateMovementBooking_SetContainerAndVehicleRTUGateInTimeCore(bool rtuHasStagingLocation)
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);
			var location2 = CreateLocation("L2", warehouse);

			var vehiclceRTU = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			if (!rtuHasStagingLocation)
			{
				vehiclceRTU.WRH_WL_StagingLocation = ZGuid.Empty;
			}
			Helper.CreateStmUniversalJobLink(vehiclceRTU.PK.ToGuid(), vehiclceRTU.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));

			var containerRTU1 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, location.PK, "CNT1");
			containerRTU1.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Booked;
			if (!rtuHasStagingLocation)
			{
				containerRTU1.WRH_WL_StagingLocation = ZGuid.Empty;
			}
			Helper.CreateStmUniversalJobLink(containerRTU1.PK.ToGuid(), containerRTU1.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", $"|FAC=WRH|GIN=GVE00000542|DIR=DLV|LOC={location2.WLV_LocationString}|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			vehiclceRTU.Reload();

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemReceiveTransportationUnit>("Should be a Receive Transportation Unit", results[0]);
				var containerRTU = ((WhsItemReceiveTransportationUnit)results[0]);
				AssertEquals("Container RTU reference number should match", "RTU2", containerRTU.WRH_ReferenceNumber);
				AssertEquals("Container RTU Staging Location should be set or updated", location2.PK, containerRTU.WRH_WL_StagingLocation);
				AssertEquals("Container RTU Gate In time should be set to the event time", xmlEvent.EventTime, containerRTU.GateInTime);
				AssertEquals("Container RTU package status should be set to 'Gated In'", TransitWarehouseStatuses.Codes.GatedIn, containerRTU.ContainerizedPackageState.WPS_Status);
				AssertContains($"Information - Setting RTU - '{containerRTU.WRH_ReferenceNumber}' Gate In time to '{xmlEvent.EventTime}'", logger.Logs);
				AssertEquals("Vehicle RTU Staging Location should be set or updated", location2.PK, vehiclceRTU.WRH_WL_StagingLocation);
				AssertEquals("Vehicle RTU Gate In time should be set to the event time", xmlEvent.EventTime, vehiclceRTU.GateInTime);
				AssertContains($"Information - Setting RTU - '{vehiclceRTU.WRH_ReferenceNumber}' Gate In time to '{xmlEvent.EventTime}'", logger.Logs);

				var vehiclceRTUEventLog = vehiclceRTU.Logs.Find(l => l.SL_SE_NKEvent == Events.GateInCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=V1", vehiclceRTUEventLog.SL_Reference);

				var containerRTUEventLog = containerRTU.Logs.Find(l => l.SL_SE_NKEvent == Events.GateInCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=ContainerID|REF=RTU2", containerRTUEventLog.SL_Reference);

				if (rtuHasStagingLocation)
				{
					AssertContains($"Information - Updating RTU - '{containerRTU.WRH_ReferenceNumber}' staging location to 'L2' which is the staging Location in Gate In event.", logger.Logs);
					AssertContains($"Information - Updating RTU - '{vehiclceRTU.WRH_ReferenceNumber}' staging location to 'L2' which is the staging Location in Gate In event.", logger.Logs);
				}
			});

			var containerRTU2 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU3", warehouse.PK, location.PK, "CNT2");
			containerRTU2.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Booked;
			if (!rtuHasStagingLocation)
			{
				containerRTU2.WRH_WL_StagingLocation = ZGuid.Empty;
			}
			Helper.CreateStmUniversalJobLink(containerRTU2.PK.ToGuid(), containerRTU2.TablePrefix, "GBM000000020", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGINEventXUEForGVMAndGGM("GB000000001", "GBM000000020", eventTime.AddHours(2), "DLV", $"|FAC=WRH|GIN=GVE00000542|DIR=DLV|LOC={location2.WLV_LocationString}|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));

			results = finder.GetLogParentsForEvent(xmlEvent2);
			Factory.SaveForTesting();

			vehiclceRTU.Reload();

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemReceiveTransportationUnit>("Should be a Receive Transportation Unit", results[0]);
				var containerRTU = ((WhsItemReceiveTransportationUnit)results[0]);
				AssertEquals("Container RTU reference number should match", "RTU3", containerRTU.WRH_ReferenceNumber);
				AssertEquals("Container RTU Staging Location should be same", location2.PK, containerRTU.WRH_WL_StagingLocation);
				AssertEquals("Container RTU Gate In time should be set to the event time", xmlEvent2.EventTime, containerRTU.GateInTime);
				AssertEquals("Container RTU package status should be set to 'Gated In'", TransitWarehouseStatuses.Codes.GatedIn, containerRTU.ContainerizedPackageState.WPS_Status);
				AssertContains($"Information - Setting RTU - '{containerRTU.WRH_ReferenceNumber}' Gate In time to '{xmlEvent2.EventTime}'", logger.Logs);
				AssertEquals("Vehicle RTU Staging Location should be set or updated", location2.PK, vehiclceRTU.WRH_WL_StagingLocation);
				AssertEquals("Vehicle RTU Gate In time should remain equal to the first container Gate In time and not be updated", xmlEvent.EventTime, vehiclceRTU.GateInTime);

				var containerRTU2EventLog = containerRTU.Logs.Find(l => l.SL_SE_NKEvent == Events.GateInCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=ContainerID|REF=RTU3", containerRTU2EventLog.SL_Reference);
			});
		}

		#endregion

		#region TestGateOutEvent

		public void TestGateOutEvent_CannotFindMatchingRTULinkedToGateMovementBooking_ThrowsDataObjectReadFailureException()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Matching RTU by GateMovementBooking - GBM000000010 not found.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateOutEvent_DirectionContextIsNotProvided_ThrowsDataObjectReadFailureException()
		{
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, null, "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Direction context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Direction context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent2));
		}

		public void TestGateOutEvent_MovementBookingNumberContextIsNotProvided_ThrowsDataObjectReadFailureException()
		{
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", null, eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "MovementBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "MovementBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent2));
		}

		public void TestGateOutEvent_GateBookingNumberContextIsNotProvided_ThrowsDataObjectReadFailureException()
		{
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM(null, "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "GateBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("", "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			AssertExceptionThrown(typeof(DataObjectReadFailureException), "GateBookingNumber context must be provided.", () => finder.GetLogParentsForEvent(xmlEvent2));
		}

		public void TestGateOutEvent_RTULinkedToGateMovementBookingNotYetGatedIn_ThrowsDataObjectReadFailureException()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("1", warehouse.PK, location.PK);
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "RTU is not yet gated into the warehouse.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateOutEvent_RTULinkedToGateMovementBookingNotYetUnloaded_ThrowsDataObjectReadFailureException()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("1", warehouse.PK, location.PK, gateIn: eventTime);
			rtu.WRH_GateInTime = eventTime;
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "RTU is not yet unloaded.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateOutEvent_RTULinkedToGateMovementBookingAlreadyGatedOut_ThrowsDataObjectReadFailureException()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("1", warehouse.PK, location.PK, gateIn: eventTime);
			rtu.WRH_GateInTime = eventTime.AddHours(-3);
			rtu.WRH_UnloadCompleteNotYetProcessedTime = eventTime.AddHours(-2);
			rtu.WRH_UnloadCompleteTime = eventTime.AddHours(-1);
			rtu.WRH_GateOutTime = eventTime;
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "RTU is already gated out of the warehouse.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateOutEvent_ContainerRTULinkedToGateMovementBooking_VehicleRTUNotUnLoadCompleted_ThrowsDataObjectReadFailureException()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var vehiclceRTU = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			vehiclceRTU.WRH_GateInTime = eventTime.AddHours(-3);
			vehiclceRTU.WRH_UnloadCompleteNotYetProcessedTime = eventTime.AddHours(-2);
			Helper.CreateStmUniversalJobLink(vehiclceRTU.PK.ToGuid(), vehiclceRTU.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));

			var containerRTU1 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, location.PK, "CNT1", vehicleRef: "CNT1");
			containerRTU1.WRH_GateInTime = eventTime.AddHours(-3);
			containerRTU1.WRH_UnloadCompleteNotYetProcessedTime = eventTime.AddHours(-2);
			containerRTU1.WRH_UnloadCompleteTime = eventTime.AddHours(-1);
			Helper.CreateStmUniversalJobLink(containerRTU1.PK.ToGuid(), containerRTU1.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Vehicle RTU has not completed unloading yet.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGateOutEvent_RTULinkedToGateMovementBooking_SetRTUGateOutTime()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = eventTime.AddHours(-3);
			rtu.WRH_UnloadCompleteNotYetProcessedTime = eventTime.AddHours(-2);
			rtu.WRH_UnloadCompleteTime = eventTime.AddHours(-1);
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemReceiveTransportationUnit>("Should be a Receive Transportation Unit", results[0]);
				var rtu = ((WhsItemReceiveTransportationUnit)results[0]);
				AssertEquals("Gate Out time should be set to the event time", xmlEvent.EventTime, rtu.WRH_GateOutTime);
				AssertContains($"Information - Setting RTU - '{rtu.WRH_ReferenceNumber}' Gate Out time to '{xmlEvent.EventTime}'.", logger.Logs);

				var rtuEventLog = rtu.Logs.Find(l => l.SL_SE_NKEvent == Events.GateOutCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=V1", rtuEventLog.SL_Reference);
			});
		}

		public void TestGateOutEvent_ContainerRTULinkedToGateMovementBooking_SetContainerAndVehicleRTUGateOutTime()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var vehiclceRTU = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			vehiclceRTU.WRH_GateInTime = eventTime.AddHours(-3);
			vehiclceRTU.WRH_UnloadCompleteNotYetProcessedTime = eventTime.AddHours(-2);
			vehiclceRTU.WRH_UnloadCompleteTime = eventTime.AddHours(-1);
			Helper.CreateStmUniversalJobLink(vehiclceRTU.PK.ToGuid(), vehiclceRTU.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));

			var containerRTU1 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU2", warehouse.PK, location.PK, "CNT1", vehicleRef: "CNT1");
			containerRTU1.WRH_GateInTime = eventTime.AddHours(-3);
			containerRTU1.WRH_UnloadCompleteNotYetProcessedTime = eventTime.AddHours(-2);
			containerRTU1.WRH_UnloadCompleteTime = eventTime.AddHours(-1);
			Helper.CreateStmUniversalJobLink(containerRTU1.PK.ToGuid(), containerRTU1.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			var containerRTU2 = Helper.CreateReceiveTransportationUnitWithContainerType("RTU3", warehouse.PK, location.PK, "CNT2", vehicleRef: "CNT2");
			containerRTU2.WRH_GateInTime = eventTime;
			containerRTU2.WRH_UnloadCompleteNotYetProcessedTime = eventTime.AddHours(1);
			containerRTU2.WRH_UnloadCompleteTime = eventTime.AddHours(2);
			Helper.CreateStmUniversalJobLink(containerRTU2.PK.ToGuid(), containerRTU2.TablePrefix, "GBM000000020", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000010", eventTime, "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			vehiclceRTU.Reload();

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemReceiveTransportationUnit>("Should be a Receive Transportation Unit", results[0]);
				var containerRTU = ((WhsItemReceiveTransportationUnit)results[0]);
				AssertEquals("Container RTU reference number should match", "RTU2", containerRTU.WRH_ReferenceNumber);
				AssertEquals("Container RTU Gate Out time should be set to the event time", xmlEvent.EventTime, containerRTU.WRH_GateOutTime);
				AssertContains($"Information - Setting RTU - '{containerRTU.WRH_ReferenceNumber}' Gate Out time to '{xmlEvent.EventTime}'", logger.Logs);
				AssertEquals("Vehicle RTU Gate Out time should be set to the event time", xmlEvent.EventTime, vehiclceRTU.WRH_GateOutTime);
				AssertContains($"Information - Setting RTU - '{vehiclceRTU.WRH_ReferenceNumber}' Gate Out time to '{xmlEvent.EventTime}'", logger.Logs);

				var containerEventLog = containerRTU.Logs.Find(l => l.SL_SE_NKEvent == Events.GateOutCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=ContainerID|REF=CNT1", containerEventLog.SL_Reference);
				var vehicleEventLog = vehiclceRTU.Logs.Find(l => l.SL_SE_NKEvent == Events.GateOutCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=V1", vehicleEventLog.SL_Reference);

				var containerRTUPkgState = containerRTU.ContainerizedPackageState;
				AssertEquals("Container RTU Containerized PackageState status should be set as departed", TransitWarehouseStatuses.Codes.Departed, containerRTUPkgState.WPS_Status);
			});

			var xmlEvent2 = EventDeserializer.Parse(UniversalHelper.BuildGOUEventXUEForGVMAndGGM("GB000000001", "GBM000000020", eventTime.AddHours(2), "DLV", "|FAC=WRH|GIN=GVE00000542|DIR=DLV|EQN=CLCU1743637|ISO=45G1|DRL=NOT AVAILABLE|DRV=Ben Chen|OFF=010:00"));

			results = finder.GetLogParentsForEvent(xmlEvent2);
			Factory.SaveForTesting();

			vehiclceRTU.Reload();

			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemReceiveTransportationUnit>("Should be a Receive Transportation Unit", results[0]);
				var containerRTU = ((WhsItemReceiveTransportationUnit)results[0]);
				AssertEquals("Container RTU reference number should match", "RTU3", containerRTU.WRH_ReferenceNumber);
				AssertEquals("Container RTU Gate Out time should be set to the event time", xmlEvent2.EventTime, containerRTU.WRH_GateOutTime);
				AssertContains($"Information - Setting RTU - '{containerRTU.WRH_ReferenceNumber}' Gate Out time to '{xmlEvent2.EventTime}'.", logger.Logs);
				AssertEquals("Vehicle RTU Gate Out time should remain equal to the first container Gate Out time and not be updated", xmlEvent.EventTime, vehiclceRTU.WRH_GateOutTime);

				var containerEventLog = containerRTU.Logs.Find(l => l.SL_SE_NKEvent == Events.GateOutCode).Single();
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=ContainerID|REF=CNT2", containerEventLog.SL_Reference);

				var containerRTUPkgState = containerRTU.ContainerizedPackageState;
				AssertEquals("Container RTU PackageState status should be set as departed", TransitWarehouseStatuses.Codes.Departed, containerRTUPkgState.WPS_Status);
			});
		}

		#endregion

		#region TestBKLEvent

		public void TestCancelBookingEvent_CannotFindMatchingRTULinkedToGateMovementBooking()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGateBKLEvent("GBM000000010", nameof(AddressType.DLV)));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Matching RTU by GateMovementBooking - GBM000000010 not found.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCancelBookingEvent_RTULinkedToGateMovementBookingAlreadyGatedIn()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU0001", warehouse.PK, location.PK, gateIn: new ZDateTimeOffset(2020, 12, 22, 10, 0, 0));
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGateBKLEvent("GBM000000010", nameof(AddressType.DLV)));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "RTU - 'RTU0001' is already gated into the warehouse.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCancelBookingEvent_CannotFindMatchingRTU_MovementNumberIsEmpty()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGateBKLEvent(string.Empty, nameof(AddressType.DLV)));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Movement booking number must be provided.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCancelBookingEvent_RTULinkedToGateMovementBooking_CancelBookingAndDetachASN()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU0001", warehouse.PK, location.PK);
			var asn1 = Helper.CreateReceiveASN("ASN001", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN002", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn1.PK);
			Helper.CreateReceiveASNRTUPivot(rtu.PK, asn2.PK);
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var pivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, rtu.PK));
			AssertEquals("Precondition: There should be 2 ASN linked to the RTU", 2, pivots.Length);

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildGateBKLEvent("GBM000000010", nameof(AddressType.DLV)));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			var reloadedRTU = Factory.Load<WhsItemReceiveTransportationUnit>(rtu.PK);
			var realoadedPivots = Factory.Load<WhsItemReceiveASNRTUPivot>(new ZQuery(WhsItemReceiveASNRTUPivotSchema.WAR_WRH_TransitReceiveTransportationUnit, rtu.PK));
			var reloadedASN1 = Factory.Load<WhsItemReceiveASN>(asn1.PK);
			var reloadedASN2 = Factory.Load<WhsItemReceiveASN>(asn2.PK);

			CombineAssertions(() =>
			{
				AssertNotNull("Output should not be null", results);
				AssertEquals("Should have found 1 business objects", 1, results.Length);
				AssertType<WhsItemReceiveTransportationUnit>("Should be a Receive Transportation Unit", results[0]);
				AssertEquals("Should be the matched RTU", rtu.PK, ((WhsItemReceiveTransportationUnit)results[0]).PK);
				AssertNotNull("RTU should not be deleted", reloadedRTU);
				Assert("RTU booking should have been cancelled", reloadedRTU.WRH_IsBookingCancelled);
				AssertEquals("RTU should have no linked ASN", 0, realoadedPivots.Length);
				AssertEquals("ASN should not be deleted", "ASN001", reloadedASN1.WRP_ReferenceNumber);
				AssertEquals("ASN should not be deleted", "ASN002", reloadedASN2.WRP_ReferenceNumber);
				AssertEquals(@"Information - Canceling booked RTU - 'RTU0001'.
Information - Detaching RTU - 'RTU0001' planned ASNs.", logger.Logs);
			});
		}

		#endregion

		#region TestCNCEvent

		public void TestCNCEvent_CannotFindMatchingRTU()
		{
			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.DLV), true));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Matching RTU by GateMovementBooking - GBM000000010 not found.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCNCEvent_CancelGateIn_RTUNotGatedIn()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU0001", warehouse.PK, location.PK);
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.DLV), true));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "RTU is not yet gated into the warehouse.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCNCEvent_CancelGateIn_RTUHasStartedUnload()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU0001", warehouse.PK, location.PK, gateIn: eventTime);
			rtu.WRH_GateInTime = eventTime.AddHours(-3);
			rtu.WRH_UnloadStartTime = eventTime.AddHours(-2);
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.DLV), true));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "RTU has started to unload.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCNCEvent_CancelGateIn_Vehicle()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU0001", warehouse.PK, location.PK, gateIn: eventTime);
			rtu.WRH_GateInTime = eventTime.AddHours(-3);

			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.DLV), true, "GB000000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			var reloadedRTU = Factory.Load<WhsItemReceiveTransportationUnit>(rtu.PK);
			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business object", 1, results.Length);
				AssertType<WhsItemReceiveTransportationUnit>("Should be a Receive Transportation Unit", results[0]);
				AssertEquals("Should be the matched RTU", reloadedRTU.PK, ((WhsItemReceiveTransportationUnit)results[0]).PK);
				AssertContains($"Cancel RTU - '{rtu.WRH_ReferenceNumber}' Gate In Succeeded", logger.Logs);

				var rtuEventLog = reloadedRTU.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).Single();
				AssertEquals("RTU gate in should be cancelled", ZDateTimeOffset.Empty, reloadedRTU.WRH_GateInTime);
				AssertEquals("RTU stage location should be empty", ZGuid.Empty, reloadedRTU.WRH_WL_StagingLocation);
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=V1|EVT=GIN", rtuEventLog.SL_Reference);
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

			var containerRTU = Helper.CreateReceiveTransportationUnitWithContainerType("RTUCNT1", warehouse.PK, location.PK, "CNT1");
			containerRTU.WRH_GateInTime = eventTime;
			containerRTU.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.GatedIn;
			Helper.CreateStmUniversalJobLink(containerRTU.PK.ToGuid(), containerRTU.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			var vehicleRTU = Helper.CreateReceiveTransportationUnit("RTU0001", warehouse.PK, location.PK);
			vehicleRTU.WRH_GateInTime = eventTime.AddHours(-3);
			Helper.CreateStmUniversalJobLink(vehicleRTU.PK.ToGuid(), vehicleRTU.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));

			var anotherContainerRTU = Helper.CreateReceiveTransportationUnitWithContainerType("RTUCNT2", warehouse.PK, ZGuid.Empty, "CNT2");
			if (vehicleHasOtherContainerGatedIn)
			{
				anotherContainerRTU.WRH_GateInTime = eventTime;
				anotherContainerRTU.WRH_WL_StagingLocation = location.PK;
				anotherContainerRTU.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.GatedIn;
			}

			var asn = Helper.CreateReceiveASN("ASN001", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(vehicleRTU.PK, asn.PK);
			asn.PackageStates.Add(containerRTU.ContainerizedPackageState);
			asn.PackageStates.Add(anotherContainerRTU.ContainerizedPackageState);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.DLV), true, "GB000000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			var reloadedContainerRTU = Factory.Load<WhsItemReceiveTransportationUnit>(containerRTU.PK);
			var reloadedAnotherContainerRTU = Factory.Load<WhsItemReceiveTransportationUnit>(anotherContainerRTU.PK);
			var reloadedVehicleRTU = Factory.Load<WhsItemReceiveTransportationUnit>(vehicleRTU.PK);
			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business object", 1, results.Length);
				AssertType<WhsItemReceiveTransportationUnit>("Should be a Receive Transportation Unit", results[0]);
				AssertEquals("Should be the matched RTU", reloadedContainerRTU.PK, ((WhsItemReceiveTransportationUnit)results[0]).PK);
				AssertContains($"Cancel RTU - '{containerRTU.WRH_ReferenceNumber}' Gate In Succeeded.", logger.Logs);

				var containerRTUEventLog = reloadedContainerRTU.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).Single();
				var vehicleRTUEventLog = reloadedVehicleRTU.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault();
				AssertEquals("Container RTU gate in should be cancelled", ZDateTimeOffset.Empty, reloadedContainerRTU.WRH_GateInTime);
				AssertEquals("Container RTU stage location should be empty", ZGuid.Empty, reloadedContainerRTU.WRH_WL_StagingLocation);
				AssertEquals("Container RTU package status should be set as Booked", TransitWarehouseStatuses.Codes.Booked, reloadedContainerRTU.ContainerizedPackageState.WPS_Status);
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=ContainerID|REF=RTUCNT1|EVT=GIN", containerRTUEventLog.SL_Reference);

				if (vehicleHasOtherContainerGatedIn)
				{
					AssertEquals("Vehicle RTU gate in should not be cancelled", eventTime.AddHours(-3), reloadedVehicleRTU.WRH_GateInTime);
					AssertEquals("Vehicle RTU stage location should be empty", location.PK, reloadedVehicleRTU.WRH_WL_StagingLocation);
					AssertNull(vehicleRTUEventLog);
				}
				else
				{
					AssertEquals("Vehicle RTU gate in should be cancelled", ZDateTimeOffset.Empty, reloadedVehicleRTU.WRH_GateInTime);
					AssertEquals("Vehicle RTU stage location should not be empty", ZGuid.Empty, reloadedVehicleRTU.WRH_WL_StagingLocation);
					AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=V1|EVT=GIN", vehicleRTUEventLog.SL_Reference);
				}
			});
		}

		public void TestCNCEvent_CancelGateOut_RTUNotGatedOut()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU0001", warehouse.PK, location.PK);
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.DLV), false));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "RTU is not yet gated out of the warehouse.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCNCEvent_CancelGateOut_RTUGatedOutMoreThan30DaysAgo()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU0001", warehouse.PK, location.PK);
			rtu.WRH_GateInTime = eventTime.AddDays(-30).AddHours(-4);
			rtu.WRH_UnloadCompleteNotYetProcessedTime = eventTime.AddDays(-30).AddHours(-3);
			rtu.WRH_UnloadCompleteTime = eventTime.AddDays(-30).AddHours(-2);
			rtu.WRH_GateOutTime = eventTime.AddDays(-30).AddHours(-1);

			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.DLV), false));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "RTU has gated out more than 30 days ago.", () => finder.GetLogParentsForEvent(xmlEvent));
		}

		public void TestCNCEvent_CancelGateOut_Vehicle()
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);

			var rtu = Helper.CreateReceiveTransportationUnit("RTU0001", warehouse.PK, location.PK, gateIn: eventTime);
			rtu.WRH_GateInTime = eventTime.AddHours(-3);
			rtu.WRH_UnloadCompleteNotYetProcessedTime = eventTime.AddHours(-2);
			rtu.WRH_UnloadCompleteTime = eventTime.AddHours(-1);
			rtu.WRH_GateOutTime = eventTime;

			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));
			Helper.CreateStmUniversalJobLink(rtu.PK.ToGuid(), rtu.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.DLV), false, "GB000000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			var reloadedRTU = Factory.Load<WhsItemReceiveTransportationUnit>(rtu.PK);
			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business object", 1, results.Length);
				AssertType<WhsItemReceiveTransportationUnit>("Should be a Receive Transportation Unit", results[0]);
				AssertEquals("Should be the matched RTU", reloadedRTU.PK, ((WhsItemReceiveTransportationUnit)results[0]).PK);
				AssertContains($"Cancel RTU - '{rtu.WRH_ReferenceNumber}' Gate Out Succeeded.", logger.Logs);

				var rtuEventLog = reloadedRTU.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).Single();
				AssertEquals("RTU gate out should be cancelled", ZDateTimeOffset.Empty, reloadedRTU.WRH_GateOutTime);
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=V1|EVT=GOU", rtuEventLog.SL_Reference);
			});
		}

		public void TestCNCEvent_CancelGateOut_Container_ContainerHasNoPackages_CancelVehicleGateOut() => TestCNCEvent_CancelGateOut_Container(false, false);

		public void TestCNCEvent_CancelGateOut_Container_ContainerHasPackages_CancelVehicleGateOut() => TestCNCEvent_CancelGateOut_Container(false, true);

		public void TestCNCEvent_CancelGateOut_Container_CannotCancelVehicleGateOut() => TestCNCEvent_CancelGateOut_Container(true);

		void TestCNCEvent_CancelGateOut_Container(bool vehicleHasOtherContainerGatedOut, bool containerHasPackages = false)
		{
			var warehouse = Data.Warehouse;
			warehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			var location = CreateLocation("L1", warehouse);
			var rcn = Helper.CreateReceiveConsignment("RCN1", warehouse.PK);

			var containerRTU = Helper.CreateReceiveTransportationUnitWithContainerType("RTUCNT1", warehouse.PK, location.PK, "CNT1");
			containerRTU.WRH_GateInTime = eventTime.AddHours(-3);
			containerRTU.WRH_UnloadCompleteNotYetProcessedTime = eventTime.AddHours(-2);
			containerRTU.WRH_UnloadCompleteTime = eventTime.AddHours(-1);
			containerRTU.WRH_GateOutTime = eventTime;
			containerRTU.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Departed;
			containerRTU.ContainerizedPackageState.WPS_WL_LastLocation = location.PK;

			if (containerHasPackages)
			{
				var containerPackageState1 = Helper.CreatePackageState(rcn, 1, "PKG", "PKG-1", TransitWarehouseStatuses.Codes.Arrived, containerRTU);
			}

			Helper.CreateStmUniversalJobLink(containerRTU.PK.ToGuid(), containerRTU.TablePrefix, "GBM000000010", nameof(DataContextType.GateMovementBooking));

			var vehicleRTU = Helper.CreateReceiveTransportationUnit("RTU0001", warehouse.PK, location.PK);
			vehicleRTU.WRH_GateInTime = eventTime.AddHours(-3);
			vehicleRTU.WRH_UnloadCompleteNotYetProcessedTime = eventTime.AddHours(-2);
			vehicleRTU.WRH_UnloadCompleteTime = eventTime.AddHours(-1);
			vehicleRTU.WRH_GateOutTime = eventTime;
			Helper.CreateStmUniversalJobLink(vehicleRTU.PK.ToGuid(), vehicleRTU.TablePrefix, "GB000000001", nameof(DataContextType.GateBooking));

			var anotherContainerRTU = Helper.CreateReceiveTransportationUnitWithContainerType("RTUCNT2", warehouse.PK, location.PK, "CNT2");
			anotherContainerRTU.ContainerizedPackageState.WPS_WL_LastLocation = location.PK;
			if (vehicleHasOtherContainerGatedOut)
			{
				anotherContainerRTU.WRH_GateInTime = eventTime.AddHours(-3);
				anotherContainerRTU.WRH_UnloadCompleteNotYetProcessedTime = eventTime.AddHours(-2);
				anotherContainerRTU.WRH_UnloadCompleteTime = eventTime.AddHours(-1);
				anotherContainerRTU.WRH_GateOutTime = eventTime;
				anotherContainerRTU.ContainerizedPackageState.WPS_Status = TransitWarehouseStatuses.Codes.Departed;
			}

			var asn = Helper.CreateReceiveASN("ASN001", warehouse.PK);
			Helper.CreateReceiveASNRTUPivot(vehicleRTU.PK, asn.PK);
			asn.PackageStates.Add(containerRTU.ContainerizedPackageState);
			asn.PackageStates.Add(anotherContainerRTU.ContainerizedPackageState);

			vehicleRTU.PackageStates.Add(containerRTU.ContainerizedPackageState);
			vehicleRTU.PackageStates.Add(anotherContainerRTU.ContainerizedPackageState);

			Factory.SaveForTesting();

			var xmlEvent = EventDeserializer.Parse(UniversalHelper.BuildCNCEventForGVM("GBM000000010", nameof(AddressType.DLV), false, "GB000000001"));
			var logger = new TestErrorLogger();
			var finder = GetFinder(logger);

			var results = finder.GetLogParentsForEvent(xmlEvent);
			Factory.SaveForTesting();

			var reloadedContainerRTU = Factory.Load<WhsItemReceiveTransportationUnit>(containerRTU.PK);
			var reloadedAnotherContainerRTU = Factory.Load<WhsItemReceiveTransportationUnit>(anotherContainerRTU.PK);
			var reloadedVehicleRTU = Factory.Load<WhsItemReceiveTransportationUnit>(vehicleRTU.PK);
			CombineAssertions(() =>
			{
				AssertNotEquals("Output should not be null", null, results);
				AssertEquals("Should have found 1 business object", 1, results.Length);
				AssertType<WhsItemReceiveTransportationUnit>("Should be a Receive Transportation Unit", results[0]);
				AssertEquals("Should be the matched RTU", reloadedContainerRTU.PK, ((WhsItemReceiveTransportationUnit)results[0]).PK);
				AssertContains($"Cancel RTU - '{containerRTU.WRH_ReferenceNumber}' Gate Out Succeeded.", logger.Logs);

				var containerRTUEventLog = reloadedContainerRTU.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).Single();
				var vehicleRTUEventLog = reloadedVehicleRTU.Logs.Find(l => l.SL_SE_NKEvent == Events.CancelledCode).SingleOrDefault();
				AssertEquals("Container RTU gate out should be cancelled", ZDateTimeOffset.Empty, reloadedContainerRTU.WRH_GateOutTime);
				AssertEquals($"Container RTU package status should be set as {(containerHasPackages ? "Freight Loaded" : "Arrived Packed")}", containerHasPackages ? TransitWarehouseStatuses.Codes.Unpacked : TransitWarehouseStatuses.Codes.ArrivedPacked, reloadedContainerRTU.ContainerizedPackageState.WPS_Status);
				AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=ContainerID|REF=RTUCNT1|EVT=GOU", containerRTUEventLog.SL_Reference);

				if (vehicleHasOtherContainerGatedOut)
				{
					AssertEquals("Vehicle RTU gate out should not be cancelled", eventTime, reloadedVehicleRTU.WRH_GateOutTime);
					AssertNull(vehicleRTUEventLog);
				}
				else
				{
					AssertEquals("Vehicle RTU gate out should be cancelled", ZDateTimeOffset.Empty, reloadedVehicleRTU.WRH_GateOutTime);
					AssertEquals("|FAC=CFS|LOC=Johannesburg|WHS=TWH|TYP=VehicleReference|REF=V1|EVT=GOU", vehicleRTUEventLog.SL_Reference);
				}
			});
		}

		#endregion

		#region GetFinder 

		protected EventParentFinder GetFinder(IXmlImportLogger logger = null)
		{
			return new WhsTransitReceiveTransportationUnitEventParentFinder(new WhsItemReceiveTransportationUnitDataContextManager(), Factory.BOFactory, logger ?? new TestErrorLogger());
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
