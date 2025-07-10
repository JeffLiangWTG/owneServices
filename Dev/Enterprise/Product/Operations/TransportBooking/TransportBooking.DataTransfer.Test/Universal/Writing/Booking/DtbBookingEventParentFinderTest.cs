using CargoWise.Types;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	class DtbBookingEventParentFinderTest : DtbBookingTestCaseWithFactory
	{
		public void TestUniversalEventDoesNotHaveContextCollection()
		{
			const string shipmentLevelEventXmlText = @"
<UniversalEvent>
  <Event>
    <EventType>OCR</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
  </Event>
</UniversalEvent>";
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(shipmentLevelEventXmlText);
			AssertNoExceptionThrown(() => subscriber.GetLogParentsForEvent(xmlEvent));
		}

		public void TestGetLogParentsForEventUsingContext()
		{
			var booking = Factory.NewWithValidTestData<DtbBooking>();
			booking.KM_JobID = "TB1";
			Factory.Save();
			var subscriber = GetNewEventParentFinder();
			var logParents = subscriber.GetLogParentsForEvent(GetXMLEvent(""));
			AssertNull("No information provided, so no match found", logParents);
			logParents = subscriber.GetLogParentsForEvent(GetXMLEvent("TB2"));
			AssertNull("Incorrect Job ID provided, so no match found", logParents);
			logParents = subscriber.GetLogParentsForEvent(GetXMLEvent("TB1"));
			AssertNull("DtbBookingEventParentFinder not implemented, so no match found", logParents);
		}

		Event GetXMLEvent(ZString jobID)
		{
			var xmlEvent = new Event();
			xmlEvent.ContextCollection =
			[
				new Context() { Type = nameof(Event.ContextTypes.TransportBookingJobID), Value = jobID },
			];
			return xmlEvent;
		}

		DtbBookingEventParentFinder GetNewEventParentFinder()
		{
			return new DtbBookingEventParentFinder(Factory, new DtbBookingDataContextManager(), new DummyLogger());
		}
	}
}
