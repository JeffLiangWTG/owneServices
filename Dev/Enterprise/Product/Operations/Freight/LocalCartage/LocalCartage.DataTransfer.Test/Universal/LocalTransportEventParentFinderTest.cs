using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;

namespace Enterprise.Freight.LocalCartage.DataTransfer.Universal.Testing
{
	class LocalTransportEventParentFinderTest : TestCaseWithFactory
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
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ConsignmentID = "T00001000";
			cartage.JJ_OrderReferenceNumber = "Order1234";
			cartage.JJ_WaybillNumber = "W00001000";
			cartage.JJ_QuoteNumber = "Q00001000";
			Factory.Save();
			var subscriber = GetNewEventParentFinder();
			var logParents = subscriber.GetLogParentsForEvent(GetXMLEvent("1", "1", "1", "1"));
			AssertNull("No Local Transport with these refs, so can't be updated.", logParents);
			logParents = subscriber.GetLogParentsForEvent(GetXMLEvent("T00001000", "1", "1", "1"));
			AssertEquals("A Local Transport with Job ID 'T00001000' can be found, so can be updated.", cartage, logParents[0]);
			logParents = subscriber.GetLogParentsForEvent(GetXMLEvent("1", "Order1234", "1", "1"));
			AssertEquals("A Local Transport with Order Ref 'Order1234' can be found, so can be updated.", cartage, logParents[0]);
			logParents = subscriber.GetLogParentsForEvent(GetXMLEvent("1", "1", "W00001000", "1"));
			AssertNull("A Local Transport with Waybill ID 'W00001000' would be found, but requires a order number or consignment id, so can't be updated.", logParents);
			logParents = subscriber.GetLogParentsForEvent(GetXMLEvent("1", "1", "1", "Q00001000"));
			AssertNull("A Local Transport with Quote ID 'Q00001000' would be found, but requires a order number or consignment id, so can't be updated.", logParents);
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_ConsignmentID = "T00001001";
			Factory.Save();
			logParents = subscriber.GetLogParentsForEvent(GetXMLEvent("T00001001", "Order1234", "1", "1"));
			AssertEquals("2 Local Transport Jobs can be found, but Transport ref takes precedence over order #.", cartage2, logParents[0]);
			logParents = subscriber.GetLogParentsForEvent(GetXMLEvent("T00001001", "Order1234", "1", "Q00001000"));
			AssertEquals("Cartage 1 has more matches, but Transport ref still takes precedence over order #.", cartage2, logParents[0]);
		}

		Event GetXMLEvent(ZString connoteNumber, ZString orderNumber, ZString waybillNumber, ZString quoteNumber)
		{
			var xmlEvent = new Event();
			xmlEvent.ContextCollection = new List<Context>();
			xmlEvent.ContextCollection.Add(new Context()
			{ Type = nameof(Event.ContextTypes.ConsignmentNoteNumber), Value = connoteNumber });
			xmlEvent.ContextCollection.Add(new Context()
			{ Type = nameof(Event.ContextTypes.OrderNumber), Value = orderNumber });
			xmlEvent.ContextCollection.Add(new Context()
			{ Type = nameof(Event.ContextTypes.WaybillNumber), Value = waybillNumber });
			xmlEvent.ContextCollection.Add(new Context()
			{ Type = nameof(Event.ContextTypes.QuoteNumber), Value = quoteNumber });
			return xmlEvent;
		}

		LocalTransportEventParentFinder GetNewEventParentFinder()
		{
			return new LocalTransportEventParentFinder(Factory, new LocalTransportDataContextManager(), new DummyLogger());
		}
	}
}
