using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants.GateManagementConstants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteGateMovementBookingDataContextManager))]
	public class GteGateMovementBookingDataContextManagerTest : DataContextManagerTestCase<GteGateMovementBookingDataContextManager, GteGateMovementBooking>
	{
		public void TestGivenExistingGBM_WhenIncomingXUE_ThenMatchByMovementBookingNumber()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_MovementBookingNumber = "BRN001";
			gateMovementBooking.GBM_CancelledReason = ZString.Empty;
			booking.GateMovementBookings.Add(gateMovementBooking);

			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.GateMovementBooking, "BRN999");
			universalEvent.EventType = AutoEvents.Booked.Code;
			universalEvent.EventTime = new ZDateTimeOffset(2024, 2, 2);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);

			AssertEquals("Expected DCD message status if there is no matching GBM", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			universalEvent.DataContext.DataTargetCollection.Single().Key = "BRN001";
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertEquals("Service Task Log", "Linked Event to Gate Movement Booking.", serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertEquals("Message Log", "Linked Event to Gate Movement Booking.", logNoteText);

				var newGateMovementBooking = new BusinessObjectFactory().Load<GteGateMovementBooking>(gateMovementBooking.PK);
				var importedEvent = newGateMovementBooking.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.Booked.Code).Single();
				AssertEquals(new ZDateTime(2024, 2, 2), importedEvent.SL_EventTime);
			});
		}

		public void TestEventContextValues()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking1 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking1.GBM_MovementBookingNumber = "BRN001";
			gateMovementBooking1.GBM_SourceReferenceNumber = "SRN002";
			gateMovementBooking1.GBM_CancelledReason = ZString.Empty;
			gateMovementBooking1.GBM_IsPickup = false;
			booking.GateMovementBookings.Add(gateMovementBooking1);

			var gateMovementBooking2 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking2.GBM_MovementBookingNumber = "BRN002";
			gateMovementBooking2.GBM_SourceReferenceNumber = "SRN002";
			gateMovementBooking2.GBM_CancelledReason = ZString.Empty;
			gateMovementBooking2.GBM_IsPickup = true;
			booking.GateMovementBookings.Add(gateMovementBooking2);

			var expectedContextValues1 = new[]
			{
				"VBSNotificationID - SRN002",
				"Direction - DLV"
			};
			var actualContextValues1 = ((IEventDataContextManager)gateMovementBooking1.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("Expected event context values for GBM1", expectedContextValues1, actualContextValues1);

			var expectedContextValues2 = new[]
			{
				"VBSNotificationID - SRN002",
				"Direction - PIC"
			};
			var actualContextValues2 = ((IEventDataContextManager)gateMovementBooking2.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("Expected event context values for GBM2", expectedContextValues2, actualContextValues2);
		}

		public void TestWhenBookingCancelledEvent_ThenCancelGBM()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "12345";

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_Source = Constants.DataSources.VehicleBookingSystem;
			gateMovementBooking.GBM_SourceReferenceNumber = "100000";

			Factory.SaveForTesting();

			var eventTime = ZDateTimeOffset.Now;

			var eventDataObject = new UniversalEvent();
			eventDataObject.EventType = EventCodes.BookingCanceled;
			eventDataObject.EventTime = eventTime.ToDateTime();
			eventDataObject.EventReference = "|SRC=VBS|RES=Cancelled By Test Facility|";

			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataTarget(DataContextType.GateMovementBooking, "ERC0000001");

			eventDataObject.ContextCollection = new List<Context>
			{
				new ()
				{
					Type = new ContextType()
					{
						Type = nameof(UniversalEvent.ContextTypes.ShippersReference)
					},
					Value = gateMovementBooking.GBM_SourceReferenceNumber
				}
			};

			eventDataObject.AdditionalFieldsToUpdateCollection = new List<AdditionalFieldToUpdate>
			{
				new () { Type = "GteGateMovementBooking.GBM_CancelledReason", Value = "Cancelled By Test Facility" },
				new () { Type = "GteGateMovementBooking.GBM_CancelledSource", Value = Constants.DataSources.VehicleBookingSystem },
				new () { Type = "GteGateMovementBooking.GBM_CancelledTime", Value = eventDataObject.EventTime.ToString() }
			};

			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			Factory.SaveForTesting();

			gateMovementBooking = Factory.Load<GteGateMovementBooking>(gateMovementBooking.PK);

			AssertEquals("Cancelled By Test Facility", gateMovementBooking.GBM_CancelledReason);
			AssertEquals(Constants.DataSources.VehicleBookingSystem, gateMovementBooking.GBM_CancelledSource);
			AssertEquals(eventTime.ToString(), gateMovementBooking.GBM_CancelledTime.ToString());
		}

		public void TestGivenRTU_WhenBKCEventFromTWHFacility_ThenPopulateFacilityJobIDAndCreateUJLFromGBMToRTU()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var rtu = Factory.BOFactory.New<IWhsItemReceiveTransportationUnit>();
			((BusinessObject)rtu).FillWithValidTestData(); 
			rtu.ReferenceNumber = "TR00000521";

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_MovementBookingNumber = "GBM0000001";

			var eventDataObject = new UniversalEvent();
			eventDataObject.EventType = EventCodes.BookingConfirmed;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataSource(DataContextType.TransitReceiveHeader, "TR00000521");
			eventDataObject.DataContext.AddDataTarget(DataContextType.GateMovementBooking, "GBM0000001");

			var manager = (IEventDataContextManager)gateMovementBooking.GetUniversalDataContextManager();
			manager.OnUniversalEventAdded(new TestErrorLogger(), eventDataObject);
			Factory.SaveForTesting();

			gateMovementBooking = Factory.Load<GteGateMovementBooking>(gateMovementBooking.PK);

			var ucl = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, gateMovementBooking.PK)).Single();

			AssertEquals("Expected GBM_FacilityTableCode to be the 'WRH'", WhsItemReceiveTransportationUnitSchema.Constants.Prefix, gateMovementBooking.GBM_FacilityTableCode);
			AssertEquals("Expected GBM_FacilityJobID to be the RTU's PK", rtu.PK, gateMovementBooking.GBM_FacilityJobId);

			AssertEquals("Expected UCL_SourceType to be TransitReceiveHeader", nameof(DataContextType.TransitReceiveHeader), ucl.UCL_SourceType);
			AssertEquals("Expected UCL_SourceKey to be RTU's WRH_ReferenceNumber", "TR00000521", ucl.UCL_SourceKey);
			AssertEquals("Expected UCL_ParentTableCode to be 'GBM'", GteGateMovementBookingSchema.Constants.Prefix, ucl.UCL_ParentTableCode);
		}

		public void TestGivenDTU_WhenBKCEventFromTWHFacility_ThenPopulateFacilityJobIDAndCreateUJLFromGBMToDTU()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var dtu = Factory.BOFactory.New<IWhsItemDispatchTransportationUnit>();
			((BusinessObject)dtu).FillWithValidTestData();
			dtu.ReferenceNumber = "TR00000333";

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_MovementBookingNumber = "GBM0000002";

			var eventDataObject = new UniversalEvent();
			eventDataObject.EventType = EventCodes.BookingConfirmed;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataSource(DataContextType.TransitDispatchHeader, "TR00000333");
			eventDataObject.DataContext.AddDataTarget(DataContextType.GateMovementBooking, "GBM0000002");

			var manager = (IEventDataContextManager)gateMovementBooking.GetUniversalDataContextManager();
			manager.OnUniversalEventAdded(new TestErrorLogger(), eventDataObject);
			Factory.SaveForTesting();

			gateMovementBooking = Factory.Load<GteGateMovementBooking>(gateMovementBooking.PK);

			var ucl = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, gateMovementBooking.PK)).Single();

			AssertEquals("Expected GBM_FacilityTableCode to be the 'WDH'", WhsItemDispatchTransportationUnitSchema.Constants.Prefix, gateMovementBooking.GBM_FacilityTableCode);
			AssertEquals("Expected GBM_FacilityJobID to be the DTU's PK", dtu.PK, gateMovementBooking.GBM_FacilityJobId);

			AssertEquals("Expected UCL_SourceType to be TransitDispatchHeader", nameof(DataContextType.TransitDispatchHeader), ucl.UCL_SourceType);
			AssertEquals("Expected UCL_SourceKey to be DTU's WDH_ReferenceNumber", "TR00000333", ucl.UCL_SourceKey);
			AssertEquals("Expected UCL_ParentTableCode to be 'GBM'", GteGateMovementBookingSchema.Constants.Prefix, ucl.UCL_ParentTableCode);
		}

		public void TestGivenBKCEvent_WhenSourceIsCYDPickup_ThenPopulateFacilityJobIdCreateUJL()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var pickup = Factory.BOFactory.New<ICYDPickup>();
			((BusinessObject)pickup).FillWithValidTestData();
			pickup.YPL_PickupID = "YPL0000001";

			Factory.SaveForTesting();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_MovementBookingNumber = "GBM000001";

			var eventDataObject = new UniversalEvent();
			eventDataObject.EventType = EventCodes.BookingConfirmed;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataSource(DataContextType.CYDPickup, pickup.YPL_PickupID);
			eventDataObject.DataContext.AddDataTarget(DataContextType.GateMovementBooking, gateMovementBooking.GBM_MovementBookingNumber);

			var manager = (IEventDataContextManager)gateMovementBooking.GetUniversalDataContextManager();
			manager.OnUniversalEventAdded(new TestErrorLogger(), eventDataObject);
			Factory.SaveForTesting();

			gateMovementBooking = Factory.Load<GteGateMovementBooking>(gateMovementBooking.PK);

			var ujl = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, gateMovementBooking.PK)).Single();

			AssertNotNull("An universal job link should have been created on the GBM", ujl);

			AssertEquals("Expected GBM_FacilityTableCode to be 'YPL'", CYDPickupSchema.Constants.Prefix, gateMovementBooking.GBM_FacilityTableCode);
			AssertEquals("Expected GBM_FacilityJobID to be the pickup's PK", pickup.PK, gateMovementBooking.GBM_FacilityJobId);

			AssertEquals("Expected UCL_SourceType to be CYDPickup", nameof(DataContextType.CYDPickup), ujl.UCL_SourceType);
			AssertEquals("Expected UCL_SourceKey to be pickup's YPL_PickupID", pickup.YPL_PickupID, ujl.UCL_SourceKey);
			AssertEquals("Expected UCL_ParentTableCode to be 'GBM'", GteGateMovementBookingSchema.Constants.Prefix, ujl.UCL_ParentTableCode);
		}

		public void TestGivenBKCEvent_WhenSourceIsCYDDelivery_ThenPopulateFacilityJobIdAndCreateUJL()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			var delivery = Factory.BOFactory.New<ICYDDelivery>();
			((BusinessObject)delivery).FillWithValidTestData();
			delivery.YDL_DeliveryID  = "YDL0000001";

			Factory.SaveForTesting();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_MovementBookingNumber = "GBM000001";

			var eventDataObject = new UniversalEvent();
			eventDataObject.EventType = EventCodes.BookingConfirmed;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataSource(DataContextType.CYDDelivery, delivery.YDL_DeliveryID);
			eventDataObject.DataContext.AddDataTarget(DataContextType.GateMovementBooking, gateMovementBooking.GBM_MovementBookingNumber);

			var manager = (IEventDataContextManager)gateMovementBooking.GetUniversalDataContextManager();
			manager.OnUniversalEventAdded(new TestErrorLogger(), eventDataObject);
			Factory.SaveForTesting();

			gateMovementBooking = Factory.Load<GteGateMovementBooking>(gateMovementBooking.PK);

			var ujl = Factory.Load<StmUniversalJobLink>(new ZQuery(StmUniversalJobLinkSchema.UCL_ParentID, gateMovementBooking.PK)).FirstOrDefault();

			AssertNotNull("An universal job link should have been created on the GBM", ujl);

			AssertEquals("Expected GBM_FacilityTableCode to be 'YDL'", CYDDeliverySchema.Constants.Prefix, gateMovementBooking.GBM_FacilityTableCode);
			AssertEquals("Expected GBM_FacilityJobID to be the delivery's PK", delivery.PK, gateMovementBooking.GBM_FacilityJobId);

			AssertEquals("Expected UCL_SourceType to be CYDDelivery", nameof(DataContextType.CYDDelivery), ujl.UCL_SourceType);
			AssertEquals("Expected UCL_SourceKey to be delivery's YDL_DeliveryID", delivery.YDL_DeliveryID, ujl.UCL_SourceKey);
			AssertEquals("Expected UCL_ParentTableCode to be 'GBM'", GteGateMovementBookingSchema.Constants.Prefix, ujl.UCL_ParentTableCode);
		}

		public void TestGivenBKLEvent_AndGteGateMovementBookingIsGatedIn_ThenThrowException()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "12345";

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_Source = Constants.DataSources.VehicleBookingSystem;
			gateMovementBooking.GBM_SourceReferenceNumber = "VBS1000";

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GBM_MovementBooking = gateMovementBooking.PK;
			gateMovement.GGM_GVM_VehicleMovement = vehicleMovement.PK;

			var gate = Factory.NewWithValidTestData<GteGate>();
			var lane = Factory.NewWithValidTestData<GteLane>();
			lane.GLN_GTE_Gate = gate.PK;

			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.GVE_GLN_Lane = lane.PK;
			vehicleEntry.GVE_IsIncoming = true;

			Factory.SaveForTesting();

			const string eventXmlText = @"
			<UniversalEvent>
				<Event>
					<EventType>BKL</EventType>
					<EventTime>09-FEB-2017 18:00</EventTime>
					<EventReference>|SRC=VBS|RES=CancelledForTest|</EventReference>
					<DataProvider>ContainerChain_EAD</DataProvider>
					<DataContext>
						<DataTargetCollection>
							<DataTarget>
								<Type>GateMovementBooking</Type>
							</DataTarget>
						</DataTargetCollection>
					</DataContext>
					<ContextCollection>
						<Context>
							<Type>ShippersReference</Type>
							<Value>VBS1000</Value>
						</Context>
					</ContextCollection>

					<AdditionalFieldsToUpdateCollection>
					  <AdditionalFieldsToUpdate>
						<Type>GteGateMovementBooking.GBM_CancelledReason</Type>
						<Value>Cancelled By Transporter</Value>
					  </AdditionalFieldsToUpdate>
					  <AdditionalFieldsToUpdate>
						<Type>GteGateMovementBooking.GBM_CancelledSource</Type>
						<Value>VBS</Value>
					  </AdditionalFieldsToUpdate>
					  <AdditionalFieldsToUpdate>
						<Type>GteGateMovementBooking.GBM_CancelledTime</Type>
						<Value>22-APR-24 13:52:45</Value>
					  </AdditionalFieldsToUpdate>
					</AdditionalFieldsToUpdateCollection>
				</Event>
			</UniversalEvent>";

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText) as UniversalEvent;

			AssertExceptionThrown<Exception>("Expected exception to be thrown when cancelling a GBM that has already been gated-in", "Cannot cancel Gate Movement Booking 'VBS1000' because it is already gated-in.", () =>
			{
				var message = GetQueuedUniversalEventMessage(xmlEvent);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = (IEventDataContextManager)gateMovementBooking.GetUniversalDataContextManager();
				manager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);
				Factory.SaveForTesting();
			});

			var cancelledReason = "CancelledForTest";
			var cancelledBy = "TST";
			var cancelledTime = DateTime.Now;

			vehicleEntry.GVE_CancelledReason = cancelledReason;
			vehicleEntry.GVE_GS_NKCancelledBy = cancelledBy;
			vehicleEntry.GVE_CancelledTime = cancelledTime;

			vehicleMovement.GVM_CancelledReason = cancelledReason;
			vehicleMovement.GVM_GS_NKCancelledBy = cancelledBy;
			vehicleMovement.GVM_CancelledTime = cancelledTime;

			gateMovement.GGM_CancelledReason = cancelledReason;
			gateMovement.GGM_GS_NKCancelledBy = cancelledBy;
			gateMovement.GGM_CancelledTime = cancelledTime;

			Factory.SaveForTesting();

			AssertNoExceptionThrown("Gate in for GateMovementBooking has been reversed / cancelled", () =>
			{
				var message = GetQueuedUniversalEventMessage(xmlEvent);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = (IEventDataContextManager)gateMovementBooking.GetUniversalDataContextManager();
				manager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);
				Factory.SaveForTesting();
			});
		}

		public void TestGivenBKLEvent_WhenGBMIsCancelled_ThenThrowException()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "12345";

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_GBK_Booking = booking.PK;
			gateMovementBooking.GBM_Source = Constants.DataSources.VehicleBookingSystem;
			gateMovementBooking.GBM_SourceReferenceNumber = "100000";
			gateMovementBooking.GBM_CancelledReason = "Cancelled for testing";
			gateMovementBooking.GBM_CancelledSource = "VBS";
			gateMovementBooking.GBM_GS_NKCancelledBy = "ABC";
			gateMovementBooking.GBM_CancelledTime = DateTime.Now;

			Factory.SaveForTesting();

			const string eventXmlText = @"
			<UniversalEvent>
				<Event>
					<EventType>BKL</EventType>
					<EventTime>09-FEB-2017 18:00</EventTime>
					<EventReference>|SRC=VBS|RES=CancelledForTest|</EventReference>
					<DataProvider>ContainerChain_EAD</DataProvider>
					<DataContext>
						<DataTargetCollection>
							<DataTarget>
								<Type>GateMovementBooking</Type>
							</DataTarget>
						</DataTargetCollection>
					</DataContext>
					<ContextCollection>
						<Context>
							<Type>ShippersReference</Type>
							<Value>100000</Value>
						</Context>
					</ContextCollection>

					<AdditionalFieldsToUpdateCollection>
					  <AdditionalFieldsToUpdate>
						<Type>GteGateMovementBooking.GBM_CancelledReason</Type>
						<Value>Cancelled By Transporter</Value>
					  </AdditionalFieldsToUpdate>
					  <AdditionalFieldsToUpdate>
						<Type>GteGateMovementBooking.GBM_CancelledSource</Type>
						<Value>VBS</Value>
					  </AdditionalFieldsToUpdate>
					  <AdditionalFieldsToUpdate>
						<Type>GteGateMovementBooking.GBM_CancelledTime</Type>
						<Value>22-APR-24 13:52:45</Value>
					  </AdditionalFieldsToUpdate>
					</AdditionalFieldsToUpdateCollection>
				</Event>
			</UniversalEvent>";

			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText) as UniversalEvent;

			AssertExceptionThrown<Exception>("Expected exception to be thrown when cancelling a GBM that has already been gated-in", "Cannot cancel Gate Movement Booking '100000' because it is already canceled.", () =>
			{
				var message = GetQueuedUniversalEventMessage(xmlEvent);
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = (IEventDataContextManager)gateMovementBooking.GetUniversalDataContextManager();
				manager.OnUniversalEventAdded(new TestErrorLogger(), xmlEvent);
				Factory.SaveForTesting();
			});
		}

		public void TestGivenCTUEvent_WhenGBMIsCancelled_ThenDoNothing()
		{
			var oldUnitType = Factory.NewWithValidTestData<RefContainer>();
			var newUnitType = Factory.NewWithValidTestData<RefContainer>();

			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_MovementBookingNumber = "BRN001";
			gateMovementBooking.GBM_CancelledReason = "Cancelled for testing";
			gateMovementBooking.GBM_CancelledSource = "VBS";
			gateMovementBooking.GBM_GS_NKCancelledBy = "ABC";
			gateMovementBooking.GBM_CancelledTime = DateTime.Now;
			gateMovementBooking.GBM_RC_UnitType = oldUnitType.PK;
			booking.GateMovementBookings.Add(gateMovementBooking);

			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent();
			universalEvent.EventType = EventCodes.ContainerTypeUpdated;
			universalEvent.EventTime = ZDateTimeOffset.Now;
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.GateMovementBooking, "BRN001");
			universalEvent.EventReference = $"|TYP=ContainerType|NEW={newUnitType.RC_Code}|OLD={oldUnitType.RC_Code}|";

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);

			AssertEquals("Cancelled GBM Unit Type should not be updated", oldUnitType.PK, gateMovementBooking.GBM_RC_UnitType);
		}

		public void TestGivenCTUEvent_WhenGBMIsNotCancelled_ThenUpdateGBMUnitType()
		{
			var oldUnitType = Factory.NewWithValidTestData<RefContainer>();
			var newUnitType = Factory.NewWithValidTestData<RefContainer>();

			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_MovementBookingNumber = "BRN001";
			gateMovementBooking.GBM_RC_UnitType = oldUnitType.PK;
			booking.GateMovementBookings.Add(gateMovementBooking);

			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent();
			universalEvent.EventType = EventCodes.ContainerTypeUpdated;
			universalEvent.EventTime = ZDateTimeOffset.Now;
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.GateMovementBooking, "BRN001");
			universalEvent.EventReference = $"|TYP=ContainerType|NEW={newUnitType.RC_Code}|OLD={oldUnitType.RC_Code}|";

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);

			AssertEquals("GBM Unit Type should be updated", newUnitType.PK, gateMovementBooking.GBM_RC_UnitType);
		}

		public void TestGivenCTUEvent_WhenGBMHasGGMAndIsCancelled_ThenDoNothing()
		{
			var oldUnitType = Factory.NewWithValidTestData<RefContainer>();
			var newUnitType = Factory.NewWithValidTestData<RefContainer>();

			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_MovementBookingNumber = "BRN001";
			gateMovementBooking.GBM_RC_UnitType = oldUnitType.PK;
			booking.GateMovementBookings.Add(gateMovementBooking);

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GBM_MovementBooking = gateMovementBooking.PK;
			gateMovement.GGM_CancelledReason = "Cancelled for testing";
			gateMovement.GGM_GS_NKCancelledBy = "ABC";
			gateMovement.GGM_CancelledTime = DateTime.Now;
			gateMovement.GGM_RC_UnitType = oldUnitType.PK;

			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent();
			universalEvent.EventType = EventCodes.ContainerTypeUpdated;
			universalEvent.EventTime = ZDateTimeOffset.Now;
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.GateMovementBooking, "BRN001");
			universalEvent.EventReference = $"|TYP=ContainerType|NEW={newUnitType.RC_Code}|OLD={oldUnitType.RC_Code}|";

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);

			AssertEquals("GBM Unit Type should be updated", newUnitType.PK, gateMovementBooking.GBM_RC_UnitType);
			AssertEquals("Cancelled GGM Unit Type should not be updated", oldUnitType.PK, gateMovement.GGM_RC_UnitType);
		}

		public void TestGivenCTUEvent_WhenGBMHasGGMAndIsNotCancelled_ThenUpdateGGMUnitType()
		{
			var oldUnitType = Factory.NewWithValidTestData<RefContainer>();
			var newUnitType = Factory.NewWithValidTestData<RefContainer>();

			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_MovementBookingNumber = "BRN001";
			gateMovementBooking.GBM_RC_UnitType = oldUnitType.PK;
			booking.GateMovementBookings.Add(gateMovementBooking);

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_GBM_MovementBooking = gateMovementBooking.PK;
			gateMovement.GGM_RC_UnitType = oldUnitType.PK;

			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent();
			universalEvent.EventType = EventCodes.ContainerTypeUpdated;
			universalEvent.EventTime = ZDateTimeOffset.Now;
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.GateMovementBooking, "BRN001");
			universalEvent.EventReference = $"|TYP=ContainerType|NEW={newUnitType.RC_Code}|OLD={oldUnitType.RC_Code}|";

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);

			AssertEquals("GBM Unit Type should be updated", newUnitType.PK, gateMovementBooking.GBM_RC_UnitType);
			AssertEquals("GGM Unit Type should be updated", newUnitType.PK, gateMovement.GGM_RC_UnitType);
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("GteGateMovementBooking does not implement IJobNumber", true);
		}
	}
}
