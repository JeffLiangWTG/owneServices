using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteGateMovementDataContextManager))]
	public class GteGateMovementDataContextManagerTest : DataContextManagerTestCase<GteGateMovementDataContextManager, GteGateMovement>
	{
		public void TestGivenExistingGGM_WhenIncomingXUE_ThenMatchByMovementBookingNumber()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();

			var gateMovementBooking = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking.GBM_MovementBookingNumber = "BRN001";
			booking.GateMovementBookings.Add(gateMovementBooking);

			var gateMovement = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement.GGM_CancelledReason = ZString.Empty;
			gateMovement.GGM_GBM_MovementBooking = gateMovementBooking.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_CancelledReason = ZString.Empty;
			vehicleMovement.GateMovements.Add(gateMovement);

			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateIn.GVE_IsIncoming = true;
			gateIn.GVE_CancelledReason = ZString.Empty;

			Factory.SaveForTesting();

			var universalEvent = new UniversalEvent();
			universalEvent.DataContext = DataContextFactory.New();
			universalEvent.DataContext.AddDataTarget(DataContextType.GateMovement, "BRNXXX");
			universalEvent.EventType = AutoEvents.GateIn.Code;
			universalEvent.EventTime = new ZDateTimeOffset(2024, 2, 2);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);

			AssertEquals("Expected DCD message status if there is no matching GGM", EDIMessageStatusList.Codes.Discarded, message.EM_Status);

			universalEvent.DataContext.DataTargetCollection.Single().Key = "BRN001";
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			message = GetQueuedUniversalEventMessage(universalEvent);
			manager.Process(message);

			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertEquals("Service Task Log", "Linked Event to Gate Movement.", serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertEquals("Message Log", "Linked Event to Gate Movement.", logNoteText);

				var newGateMovement = new BusinessObjectFactory().Load<GteGateMovement>(gateMovement.PK);
				var importedEvent = newGateMovement.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.GateIn.Code).Single();
				AssertEquals(new ZDateTime(2024, 2, 2), importedEvent.SL_EventTime);
			});
		}

		public void TestEventContextValues()
		{
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_ReferenceNumber = "GB0001010";

			var gateMovementBooking1 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking1.GBM_MovementBookingNumber = "BRN001";
			gateMovementBooking1.GBM_SourceReferenceNumber = "SRN002";
			gateMovementBooking1.GBM_IsPickup = false;
			booking.GateMovementBookings.Add(gateMovementBooking1);

			var gateMovementBooking2 = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gateMovementBooking2.GBM_MovementBookingNumber = "BRN002";
			gateMovementBooking2.GBM_SourceReferenceNumber = "SRN002";
			gateMovementBooking2.GBM_IsPickup = true;
			booking.GateMovementBookings.Add(gateMovementBooking2);

			var gateMovement1 = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement1.GGM_CancelledReason = ZString.Empty;
			gateMovement1.GGM_IsPickup = false;
			gateMovement1.GGM_GBM_MovementBooking = gateMovementBooking1.PK;

			var gateMovement2 = Factory.NewWithValidTestData<GteGateMovement>();
			gateMovement2.GGM_CancelledReason = ZString.Empty;
			gateMovement2.GGM_IsPickup = true;
			gateMovement2.GGM_GBM_MovementBooking = gateMovementBooking2.PK;

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.GVM_CancelledReason = ZString.Empty;
			vehicleMovement.GVM_VehicleRegistration = "VEHICLE1";
			vehicleMovement.GateMovements.Add(gateMovement1);
			vehicleMovement.GateMovements.Add(gateMovement2);

			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			gateIn.GVE_IsIncoming = true;
			gateIn.GVE_CancelledReason = ZString.Empty;

			var expectedContextValues1 = new[]
			{
				"VBSNotificationID - SRN002",
				"GateBookingNumber - GB0001010",
				"MovementBookingNumber - BRN001",
				"Direction - DLV",
				"VehicleRegistration - VEHICLE1"
			};
			var actualContextValues1 = ((IEventDataContextManager)gateMovement1.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("Expected event context values for GGM1", expectedContextValues1, actualContextValues1);

			var expectedContextValues2 = new[]
			{
				"VBSNotificationID - SRN002",
				"GateBookingNumber - GB0001010",
				"MovementBookingNumber - BRN002",
				"Direction - PIC",
				"VehicleRegistration - VEHICLE1"
			};
			var actualContextValues2 = ((IEventDataContextManager)gateMovement2.GetUniversalDataContextManager()).EventContextValues.Select(x => $"{x.Key.Type} - {x.Value}");

			AssertContainsExactElementsInAnyOrder("Expected event context values for GGM2", expectedContextValues2, actualContextValues2);
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("GteGateMovement does not implement IJobNumber", true);
		}
	}
}
