using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	sealed class DtbBookingUniversalExportHookTest : DtbBookingTestCaseWithFactory
	{
		public void TestOnUniversalXmlExport_SendingToAuthorisedBookingAgentThatIsNotContainerTransportOptimization_LogCreated()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();

			var mode = new NonPersistentEDICommunicationMode
			{
				EK_CommunicationsTransport = EDIInterchangeTransportTypeList.Codes.eHub,
				EK_Destination = "BLUME_EAD",
			};

			IUniversalExportHook exportHook = new DtbBookingUniversalExportHook();
			exportHook.OnUniversalXmlExport(booking, mode);

			var log = (StmALog)booking.Logs.GetAllLogs().First();
			AssertEquals("|TYP=Booking Request|DEP=BLUME_EAD", log.SL_Reference);
		}

		public void TestOnUniversalXmlExport_SendingToContainerTransportOptimization_LogCreatedAndStatusChanged()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_Status = TransportStatuses.Codes.Available;

			var mode = new NonPersistentEDICommunicationMode
			{
				EK_CommunicationsTransport = EDIInterchangeTransportTypeList.Codes.eHub,
				EK_Destination = "CONTAINER_TRANSPORT_OPTIMIZATION",
			};

			IUniversalExportHook exportHook = new DtbBookingUniversalExportHook();
			exportHook.OnUniversalXmlExport(booking, mode);

			var log = (StmALog)booking.Logs.GetAllLogs().First();
			AssertEquals("|DEP=CONTAINER_TRANSPORT_OPTIMIZATION|FAC=CTO|TYP=Carrier Booking Agent", log.SL_Reference);
			AssertEquals("KM_Status should not have changed", TransportStatuses.Codes.Available, booking.KM_Status);
		}

		public void TestOnUniversalXmlExport_InvalidBookingAgent_NoLog()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();

			var mode = new NonPersistentEDICommunicationMode
			{
				EK_CommunicationsTransport = EDIInterchangeTransportTypeList.Codes.eHub,
				EK_Destination = "NON_AUTHORISED_TEST_AGENT",
			};
			IUniversalExportHook exportHook = new DtbBookingUniversalExportHook();
			exportHook.OnUniversalXmlExport(booking, mode);

			AssertEquals("No logs generated for unauthorised agent", 0, booking.Logs.GetAllLogs().Count);
		}

		public void TestOnUniversalXmlExport_Non_eHubMode_NoLog()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();

			var mode = new NonPersistentEDICommunicationMode
			{
				EK_CommunicationsTransport = EDIInterchangeTransportTypeList.Codes.eAdaptor,
				EK_Destination = "CONTAINER_TRANSPORT_OPTIMIZATION",
			};
			IUniversalExportHook exportHook = new DtbBookingUniversalExportHook();
			exportHook.OnUniversalXmlExport(booking, mode);

			AssertEquals("No logs generated for non-ehub mode", 0, booking.Logs.GetAllLogs().Count);
		}

		public void TestOnUniversalXmlExport_Non_DtbBookingBusinessObject_NoLog()
		{
			var dtbAgentBooking = Factory.New<DtbAgentBooking>();

			var mode = new NonPersistentEDICommunicationMode
			{
				EK_CommunicationsTransport = EDIInterchangeTransportTypeList.Codes.eHub,
				EK_Destination = "CONTAINER_TRANSPORT_OPTIMIZATION",
			};
			IUniversalExportHook exportHook = new DtbBookingUniversalExportHook();
			exportHook.OnUniversalXmlExport(dtbAgentBooking, mode);

			AssertEquals("No logs generated for non DtbBooking type business object", 0, dtbAgentBooking.Logs.GetAllLogs().Count);
		}

		public void TestOnUniversalXmlExportFailure_BookingFailedValidationForSendingToContainerTransportOptimization_LogCreatedAndStatusChanged()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.NotificationBufferForSendingXUSToCTO.AddMessageError("Message Error");
			booking.KM_Status = TransportStatuses.Codes.Available;

			var mode = new NonPersistentEDICommunicationMode
			{
				EK_CommunicationsTransport = EDIInterchangeTransportTypeList.Codes.eHub,
				EK_Destination = "CONTAINER_TRANSPORT_OPTIMIZATION",
			};

			IUniversalExportHook exportHook = new DtbBookingUniversalExportHook();
			exportHook.OnUniversalXmlExportValidationFailure(booking, mode);

			var log = (StmALog)booking.Logs.GetAllLogs().First();
			AssertEquals("|FAC=CTO|MST=Sending XUS to CONTAINER_TRANSPORT_OPTIMIZATION:|TYP=Carrier Booking Agent", log.SL_Reference);
			AssertEquals("KM_Status should have changed to 'Action Required'.", TransportStatuses.Codes.ActionRequired, booking.KM_Status);
		}

		public void TestOnUniversalXmlExportFailure_BookingDidNotFailValidationForSendingToContainerTransportOptimization_NoLog()
		{
			var consolidation = Factory.New<DtbBookingConsolidation>();
			var booking = consolidation.Bookings.AddNew();
			booking.KM_Status = TransportStatuses.Codes.Available;

			AssertEquals("Precondition: NotificationBufferForSendingXUSToCTO should have no Errors.", false, booking.NotificationBufferForSendingXUSToCTO.Events.HasErrors());
			AssertEquals("Precondition: NotificationBufferForSendingXUSToCTO should have no Message Errors.", false, booking.NotificationBufferForSendingXUSToCTO.Events.HasMessageErrors());

			var mode = new NonPersistentEDICommunicationMode
			{
				EK_CommunicationsTransport = EDIInterchangeTransportTypeList.Codes.eHub,
				EK_Destination = "CONTAINER_TRANSPORT_OPTIMIZATION",
			};

			IUniversalExportHook exportHook = new DtbBookingUniversalExportHook();
			exportHook.OnUniversalXmlExportValidationFailure(booking, mode);

			AssertEquals("No logs should be generated for booking.", 0, booking.Logs.GetAllLogs().Count);
			AssertEquals("KM_Status should not have changed.", TransportStatuses.Codes.Available, booking.KM_Status);
		}
	}
}
