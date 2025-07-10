using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.QuotedBookings.DataTransfer.Universal.Test
{
	class ForwardingBookingEventParentFinderTest : TestCaseWithFactory
	{
		public void TestMatchOnAdditionalReferences()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
        <Context>
          <Type Description=""AMS Number"">AMS</Type>
          <Value>134FREGT</Value>
        </Context>
        <Context>
          <Type Description="""">COC</Type>
          <Value>267AIRGT</Value>
        </Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			var additionalReference1 = matchingBooking.Booking.Numbers.AddNew();
			additionalReference1.CE_EntryNum = "134FREGT";
			additionalReference1.CE_EntryType = "AMS";
			additionalReference1.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			var additionalReference2 = matchingBooking.Booking.Numbers.AddNew();
			additionalReference2.CE_EntryNum = "267AIRGT";
			additionalReference2.CE_EntryType = "COC";
			additionalReference2.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			xmlEvent.DataContext = DataContextFactory.New();
			xmlEvent.DataContext.CodesMappedToTarget = true;
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestMatchOnHouseBillForAir_QuickBooking()
		{
			AssertMatchOnHouseBillForAir(QuoteBookingType.QuickBooking);
		}

		public void TestMatchOnHouseBillForAir_BookingWithQuote()
		{
			AssertMatchOnHouseBillForAir(QuoteBookingType.BookingWithQuote);
		}

		void AssertMatchOnHouseBillForAir(QuoteBookingType quoteBookingType)
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>HAWBNumber</Type>
				  <Value>ONTHEHOUSE</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var matchingBooking = QuotedBooking.New(quoteBookingType, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_TransportMode = Constants.TransportModes.Air;
			matchingBooking.Booking.JS_HouseBill = "ONTHEHOUSE";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestMatchOnHouseBillForSea_QuickBooking()
		{
			AssertMatchOnHouseBillForSea(QuoteBookingType.QuickBooking);
		}

		public void TestMatchOnHouseBillForSea_BookingWithQuote()
		{
			AssertMatchOnHouseBillForSea(QuoteBookingType.BookingWithQuote);
		}

		void AssertMatchOnHouseBillForSea(QuoteBookingType quoteBookingType)
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>HBOLNumber</Type>
				  <Value>ONTHEHOUSE</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var matchingBooking = QuotedBooking.New(quoteBookingType, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			matchingBooking.Booking.JS_HouseBill = "ONTHEHOUSE";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestGetsCorrectLatestBookingWhenUNLOCOMatchReturnsMoreThanOneBestMatch()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ShippersReference</Type>
				  <Value>REFERME123</Value>
				</Context>
				<Context>
				  <Type>HBOLOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var dummyBooking1 = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking1.Booking.JS_BookingReference = "REFERME123";
			dummyBooking1.Booking.JS_RL_NKOrigin = "AUSYD";
			dummyBooking1.Booking.JS_SystemCreateTimeUtc = new ZDateTime(2012, 4, 1, 10, 0, 0);
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_BookingReference = "REFERME123";
			matchingBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			matchingBooking.Booking.JS_SystemCreateTimeUtc = new ZDateTime(2012, 4, 1, 11, 0, 0);
			var dummyBooking2 = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking2.Booking.JS_BookingReference = "REFERME123";
			dummyBooking2.Booking.JS_RL_NKOrigin = "AUMEL";
			dummyBooking2.Booking.JS_SystemCreateTimeUtc = new ZDateTime(2012, 4, 1, 12, 0, 0);
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestGetsLatestBookingWhenMoreThanOneBestMatch()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ShippersReference</Type>
				  <Value>REFERME123</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var dummyBooking1 = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking1.Booking.JS_BookingReference = "REFERME123";
			dummyBooking1.Booking.JS_SystemCreateTimeUtc = new ZDateTime(2012, 4, 1, 10, 0, 0);
			var dummyBooking2 = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking2.Booking.JS_BookingReference = "REFERME123";
			dummyBooking2.Booking.JS_SystemCreateTimeUtc = new ZDateTime(2012, 4, 1, 11, 0, 0);
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_BookingReference = "REFERME123";
			matchingBooking.Booking.JS_SystemCreateTimeUtc = new ZDateTime(2012, 4, 1, 12, 0, 0);
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestWhittlingDownOfMatchesFallsBackToUNLOCOsWhenLeftWithNoMatches()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ShippersReference</Type>
				  <Value>REFERME123</Value>
				</Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>Order</Value>
        </Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>Items</Value>
        </Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>#1</Value>
        </Context>
				<Context>
				  <Type>InterimReceipt</Type>
				  <Value>INTERIM</Value>
				</Context>
				<Context>
				  <Type>CFSReference</Type>
				  <Value>CANTFRIGGENSAVE</Value>
				</Context>
				<Context>
				  <Type>HBOLOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_InterimReceipt = "INTERIM";
			matchingBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			matchingBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			var dummyBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking.Booking.JS_InterimReceipt = "INTERIM";
			dummyBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			dummyBooking.Booking.JS_RL_NKOrigin = "AUMEL";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestWhittlingDownOfMatchesGoesBackUpALevelWhenLeftWithNoMatches()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ShippersReference</Type>
				  <Value>REFERME123</Value>
				</Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>Order</Value>
        </Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>Items</Value>
        </Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>#1</Value>
        </Context>
				<Context>
				  <Type>InterimReceipt</Type>
				  <Value>INTERIM</Value>
				</Context>
				<Context>
				  <Type>CFSReference</Type>
				  <Value>CANTFRIGGENSAVE</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_InterimReceipt = "INTERIM";
			var dummyBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking.Booking.JS_CFSReference = "CANTFRIGGENSAVE";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestOrderNumbersNotPrioritisedInMatching()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ShippersReference</Type>
				  <Value>REFERME123</Value>
				</Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>Order</Value>
        </Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>Items</Value>
        </Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>#1</Value>
        </Context>
				<Context>
				  <Type>InterimReceipt</Type>
				  <Value>INTERIM</Value>
				</Context>
				<Context>
				  <Type>CFSReference</Type>
				  <Value>CANTFRIGGENSAVE</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_InterimReceipt = "INTERIM";
			matchingBooking.Booking.JS_CFSReference = "CANTFRIGGENSAVE";
			var dummyBooking1 = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking1.Booking.DocsAndCartage.JP_OrderItemsAsString = "Order Items #1";
			dummyBooking1.Booking.JS_CFSReference = "CANTFRIGGENSAVE";
			var dummyBooking2 = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking2.Booking.DocsAndCartage.JP_OrderItemsAsString = "Order Items #1";
			dummyBooking2.Booking.JS_InterimReceipt = "INTERIM";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestFindsBestMatchWhenNumberOfMatchesIsEqual()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ShippersReference</Type>
				  <Value>REFERME123</Value>
				</Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>Order</Value>
        </Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>Items</Value>
        </Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>#1</Value>
        </Context>
				<Context>
				  <Type>InterimReceipt</Type>
				  <Value>INTERIM</Value>
				</Context>
				<Context>
				  <Type>CFSReference</Type>
				  <Value>CANTFRIGGENSAVE</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_BookingReference = "REFERME123";
			matchingBooking.Booking.JS_InterimReceipt = "INTERIM";
			var dummyBooking1 = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking1.Booking.JS_BookingReference = "REFERME123";
			dummyBooking1.Booking.JS_CFSReference = "CANTFRIGGENSAVE";
			var dummyBooking2 = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking2.Booking.JS_InterimReceipt = "INTERIM";
			dummyBooking2.Booking.JS_CFSReference = "CANTFRIGGENSAVE";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestFindsBestMatch()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ShippersReference</Type>
				  <Value>REFERME123</Value>
				</Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>Order</Value>
        </Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>Items</Value>
        </Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>#1</Value>
        </Context>
				<Context>
				  <Type>InterimReceipt</Type>
				  <Value>INTERIM</Value>
				</Context>
				<Context>
				  <Type>CFSReference</Type>
				  <Value>CANTFRIGGENSAVE</Value>
				</Context>
				<Context>
				  <Type>HBOLOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
				<Context>
				  <Type>HBOLDestinationUNLOCO</Type>
				  <Value>NZAKL</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_BookingReference = "REFERME123";
			matchingBooking.Booking.DocsAndCartage.JP_OrderItemsAsString = "Order Items #1";
			matchingBooking.Booking.JS_InterimReceipt = "INTERIM";
			matchingBooking.Booking.JS_CFSReference = "CANTFRIGGENSAVE";
			factory.Save();
			var dummyBooking1 = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking1.Booking.JS_BookingReference = "REFERME123";
			dummyBooking1.Booking.JS_InterimReceipt = "INTERIM";
			dummyBooking1.Booking.JS_CFSReference = "CANTFRIGGENSAVE";
			var dummyBooking2 = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking2.Booking.JS_BookingReference = "REFERME123";
			dummyBooking2.Booking.JS_CFSReference = "CANTFRIGGENSAVE";
			var dummyBooking3 = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking3.Booking.JS_InterimReceipt = "INTERIM";
			dummyBooking3.Booking.JS_CFSReference = "CANTFRIGGENSAVE";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestMatchOnHouseBillWhenEventTypeIsSBR()
		{
			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				const string bookingLevelEventXmlText = @"
			<UniversalEvent>
	   <Event>
			  <EventType>SBR</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>02012345675</Value>
				</Context>
				<Context>
				  <Type>CarrierC1CCode</Type>
				  <Value>C1CO</Value>
				</Context>
			  </ContextCollection>
	   </Event>
			</UniversalEvent>";

				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_CargoWiseOneCode = "C1CO";
				shippingLine.RSL_IsNVO = true;

				var factory = new BusinessObjectFactory();
				var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
				matchingBooking.Booking.JS_HouseBill = "02012345675";
				factory.Save();

				var subscriber = GetNewEventParentFinder();
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
				var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
				var logParentBooking = logParent as QuotedBooking;
				AssertNotNull("logParentBooking", logParentBooking);
				AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
			}
		}

		public void TestMatchOnUniqueConsignRefWhenEventTypeIsSBR()
		{
			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				const string bookingLevelEventXmlText = @"
			<UniversalEvent>
	   <Event>
			  <EventType>SBR</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CarriersBookingReference</Type>
				  <Value>Z103439AKL</Value>
				</Context>
				<Context>
				  <Type>CarrierC1CCode</Type>
				  <Value>C1CO</Value>
				</Context>
			  </ContextCollection>
	   </Event>
			</UniversalEvent>";

				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_CargoWiseOneCode = "C1CO";
				shippingLine.RSL_IsNVO = true;

				var factory = new BusinessObjectFactory();
				var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
				matchingBooking.Booking.JS_UniqueConsignRef = "Z103439AKL";
				factory.Save();

				var subscriber = GetNewEventParentFinder();
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
				var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
				var logParentBooking = logParent as QuotedBooking;
				AssertNotNull("logParentBooking", logParentBooking);
				AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
			}
		}

		public void TestMatchOnBookingReference()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>ShippersReference</Type>
				  <Value>REFERME123</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_BookingReference = "REFERME123";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestNoMatchOnOrderReference()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
        <Context>
          <Type>OrderNumber</Type>
          <Value>Order</Value>
        </Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>Items</Value>
        </Context>
        <Context>
          <Type>OrderNumber</Type>
          <Value>#1</Value>
        </Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.DocsAndCartage.JP_OrderItemsAsString = "Order Items #1";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("logParentBooking", logParent);
		}

		public void TestMatchOnInterimReceipt()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>InterimReceipt</Type>
				  <Value>INTERIM</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_InterimReceipt = "INTERIM";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestMatchOnCFSReference()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>CFSReference</Type>
				  <Value>CANTFRIGGENSAVE</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_CFSReference = "CANTFRIGGENSAVE";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestIfOnlyPortOfOriginOrDestinationWillNotMatch()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>HBOLOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
				<Context>
				  <Type>HBOLDestinationUNLOCO</Type>
				  <Value>NZAKL</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var booking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			booking.Booking.JS_UniqueConsignRef = "S10101010";
			booking.Booking.JS_RL_NKOrigin = "AUSYD";
			booking.Booking.JS_RL_NKDestination = "NZAKL";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("logParent", logParent);
		}

		public void TestIfMoreThanOneMatchFallBackToPortOfOrigin()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
        <Context>
				  <Type>ShippersReference</Type>
				  <Value>REFERME123</Value>
				</Context>
				<Context>
				  <Type>HBOLOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var dummyBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking.Booking.JS_BookingReference = "REFERME123";
			dummyBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			dummyBooking.Booking.JS_RL_NKOrigin = "AUMEL";
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_BookingReference = "REFERME123";
			matchingBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			matchingBooking.Booking.JS_TransportMode = Constants.TransportModes.Sea;
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestIfMoreThanOneMatchWhenFallingBackToPortOfOriginAndDestinationWhenEventTypeIsSBR()
		{
			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>SBR</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
				<Context>
				  <Type>MBOLNumber</Type>
				  <Value>Z103439AKL</Value>
				</Context>
				<Context>
				  <Type>CarrierC1CCode</Type>
				  <Value>C1CO</Value>
				</Context>
				<Context>
				  <Type>MBOLOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
				<Context>
				  <Type>MBOLDestinationUNLOCO</Type>
				  <Value>NZAKL</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";

				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_CargoWiseOneCode = "C1CO";
				shippingLine.RSL_IsNVO = true;

				var factory = new BusinessObjectFactory();

				var dummyBooking1 = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
				dummyBooking1.Booking.JS_HouseBill = "Z103439AKL";
				dummyBooking1.Booking.JS_RL_NKOrigin = "AUSYD";
				dummyBooking1.Booking.JS_RL_NKDestination = "SGSIN";

				var dummyBooking2 = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
				dummyBooking2.Booking.JS_HouseBill = "Z103439AKL";
				dummyBooking2.Booking.JS_RL_NKOrigin = "USLAX";
				dummyBooking2.Booking.JS_RL_NKDestination = "NZAKL";

				var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
				matchingBooking.Booking.JS_HouseBill = "Z103439AKL";
				matchingBooking.Booking.JS_RL_NKOrigin = "AUSYD";
				matchingBooking.Booking.JS_RL_NKDestination = "NZAKL";

				factory.Save();

				var subscriber = GetNewEventParentFinder();
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
				var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
				var logParentBooking = logParent as QuotedBooking;
				AssertNotNull("logParentBooking", logParentBooking);
				AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
			}
		}

		public void TestIfMoreThanOneMatchFallBackToPortOfDestination()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
        <Context>
				  <Type>ShippersReference</Type>
				  <Value>REFERME123</Value>
				</Context>
				<Context>
				  <Type>HBOLDestinationUNLOCO</Type>
				  <Value>NZAKL</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var dummyBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking.Booking.JS_BookingReference = "REFERME123";
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_BookingReference = "REFERME123";
			matchingBooking.Booking.JS_RL_NKDestination = "NZAKL";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestIfMoreThanOneMatchWhenFallingBackToPortOfOriginAndDestinationFindsParentThatMatchesBoth()
		{
			const string bookingLevelEventXmlText = @"
			<UniversalEvent>
       <Event>
			  <EventType>CCD</EventType>
			  <EventTime>10-JUL-2010 18:00</EventTime>
			  <EventReference>Dummy Description</EventReference>
			  <DataProvider>Dummy</DataProvider>
			  <ContextCollection>
        <Context>
				  <Type>ShippersReference</Type>
				  <Value>REFERME123</Value>
				</Context>
				<Context>
				  <Type>HBOLOriginUNLOCO</Type>
				  <Value>AUSYD</Value>
				</Context>
				<Context>
				  <Type>HBOLDestinationUNLOCO</Type>
				  <Value>NZAKL</Value>
				</Context>
			  </ContextCollection>
       </Event>
			</UniversalEvent>";
			var factory = new BusinessObjectFactory();
			var dummyBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			dummyBooking.Booking.JS_BookingReference = "REFERME123";
			dummyBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "S10101010";
			matchingBooking.Booking.JS_BookingReference = "REFERME123";
			matchingBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			matchingBooking.Booking.JS_RL_NKDestination = "NZAKL";
			factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(bookingLevelEventXmlText);
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent)[0];
			var logParentBooking = logParent as QuotedBooking;
			AssertNotNull("logParentBooking", logParentBooking);
			AssertEquals("Should get best matching Booking.", GetHumanReadableID(matchingBooking), GetHumanReadableID(logParentBooking));
		}

		public void TestLogIsAddedOnSuccessfulSBR()
		{
			const string eventXmlText = @"
			<UniversalEvent>
				<Event>
				  <EventType>SBR</EventType>
				  <EventTime>10-JUL-2010 18:00</EventTime>
				  <EventReference>Dummy Description</EventReference>
				  <DataProvider>Dummy</DataProvider>
				  <EventParameters>
					<Type>Shipment Visibility</Type>
				  </EventParameters>
				  <ContextCollection>
					<Context>
					  <Type>ShippersReference</Type>
					  <Value>REFERME123</Value>
					</Context>
					<Context>
					  <Type>HBOLOriginUNLOCO</Type>
					  <Value>AUSYD</Value>
					</Context>
					<Context>
					  <Type>HBOLDestinationUNLOCO</Type>
					  <Value>NZAKL</Value>
					</Context>
					<Context>
					  <Type>CarriersBookingReference</Type>
					  <Value>TA3PTQ483800</Value>
					</Context>
					<Context>
					  <Type>CarrierC1CCode</Type>
					  <Value>C1CO</Value>
					</Context>
				  </ContextCollection>
				</Event>
			</UniversalEvent>";

			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			testCompany.CompanyName = "testCompany";
			testCompany.GC_Code = "HMC";

			var testBranch = Factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_BranchName = "testBranch";
			testBranch.GB_Code = "HMB";
			testBranch.GB_GC = testCompany.PK;

			var testDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			testDepartment.GE_Code = "HMD";
			testDepartment.GE_Desc = "Test Department";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "testStaff";
			staff.GS_Code = "XXX";
			staff.GS_GB_HomeBranch = testBranch.PK;

			var dummyBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			dummyBooking.Booking.JS_BookingReference = "REFERME123";
			dummyBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			var matchingBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			matchingBooking.Booking.JS_UniqueConsignRef = "TA3PTQ483800";
			matchingBooking.Booking.JS_BookingReference = "REFERME123";
			matchingBooking.Booking.JS_RL_NKOrigin = "AUSYD";
			matchingBooking.Booking.JS_RL_NKDestination = "NZAKL";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "C1CO";
			shippingLine.RSL_IsNVO = true;
			Factory.Save();

			var logger = new XmlSessionTracker(new SimpleLogger());
			var subscriber = GetNewEventParentFinder(logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText) as UniversalEvent;
			var logParent = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertNull("logParentBooking", logParent);
			AssertContains("Cannot link Forwarding Booking because: [* Tracking is not supported for this Carrier. *]", logger.ToString());

			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				var logParentBooking = (subscriber.GetLogParentsForEvent(xmlEvent)[0]) as QuotedBooking;
				AssertNotNull("logParentBooking", logParentBooking);
				AssertContains("Successfully saved: [*Provider subscription for Carrier Booking Reference 'TA3PTQ483800' was Confirmed. Subscription created*]", logger.ToString());
			}
		}

		public void TestMatchFailureLogWhenEventTypeIsSBR()
		{
			const string eventXmlText = @"
			<UniversalEvent>
				<Event>
				  <EventType>SBR</EventType>
				  <EventTime>10-JUL-2010 18:00</EventTime>
				  <EventReference>Dummy Description</EventReference>
				  <DataProvider>Dummy</DataProvider>
				  <EventParameters>
					<Type>Shipment Visibility</Type>
				  </EventParameters>
				  <ContextCollection>
					<Context>
					  <Type>MBOLNumber</Type>
					  <Value>Z103439AKL</Value>
					</Context>
					<Context>
					  <Type>ShippersReference</Type>
					  <Value>REFERME123</Value>
					</Context>
					<Context>
					  <Type>CarrierC1CCode</Type>
					  <Value>C1CO</Value>
					</Context>
					<Context>
					  <Type>HBOLOriginUNLOCO</Type>
					  <Value>AUSYD</Value>
					</Context>
					<Context>
					  <Type>HBOLDestinationUNLOCO</Type>
					  <Value>NZAKL</Value>
					</Context>
				  </ContextCollection>
				</Event>
			</UniversalEvent>";

			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_CargoWiseOneCode = "C1CO";
				shippingLine.RSL_IsNVO = true;

				var logger = new XmlSessionTracker(new SimpleLogger());
				var subscriber = GetNewEventParentFinder(logger);
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(eventXmlText) as UniversalEvent;
				var logParent = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertNull("logParentBooking", logParent);
				AssertContains("Cannot link Forwarding Booking because: [* Master Bill Number is Invalid. *]", logger.ToString());

				shippingLine.RSL_IsNVO = false;

				logger = new XmlSessionTracker(new SimpleLogger());
				subscriber = GetNewEventParentFinder(logger);
				logParent = subscriber.GetLogParentsForEvent(xmlEvent);
				AssertNull(logParent);
				AssertNotContains("Cannot link Forwarding Shipment because: [* Master Bill Number and Carrier Booking Number are Invalid. *]", logger.ToString());
			}
		}

		ForwardingBookingEventParentFinder GetNewEventParentFinder(IXmlImportLogger logger = null)
		{
			return new ForwardingBookingEventParentFinder(Factory, new QuotedBookingDataContextManager(), logger ?? new DummyLogger(), new UniversalForwardingHelper());
		}

		static string GetHumanReadableID(BusinessObject businessObject)
		{
			return businessObject.HumanReadableName + " - PK: " + businessObject.PK;
		}
	}
}
