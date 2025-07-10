using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	class OneOffQuoteEventParentFinderTest : TestCaseWithFactory
	{
		public void TestGetLogParents_UsingEventDataContext()
		{
			var factory = new BusinessObjectFactory();
			var quotedBooking = QuotedBooking.New(QuoteBookingType.SpotQuote, factory);

			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(string.Format(eventXmlText_NotificanForOOQ, quotedBooking.Quote.TH_QuoteNumber));
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNotNull("logParentOneOffQuote", logParent);
			AssertEquals(1, logParent.Length);
			var logParentBooking = logParent[0] as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals(quotedBooking.Quote.PK, logParentBooking.Quote.PK);
		}

		OneOffQuoteEventParentFinder GetNewEventParentFinder()
		{
			return new OneOffQuoteEventParentFinder(Factory, new OneOffQuoteDataContextManager(), new DummyLogger());
		}

		const string eventXmlText_NotificanForOOQ = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>OneOffQuote</Type>
              <Key>{0}</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2023-04-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>IT</MessageType>
           <MessageSubType>notificaMancataConsegna</MessageSubType>
        </EventParameters>
        <ContextCollection>
           <Context>
             <Type>CompanyCode</Type>
             <Value>DEM</Value>
           </Context>
           <Context>
             <Type>eHubAllocatedNumber</Type>
             <Value>EHub-1234567</Value>
           </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";
	}
}
