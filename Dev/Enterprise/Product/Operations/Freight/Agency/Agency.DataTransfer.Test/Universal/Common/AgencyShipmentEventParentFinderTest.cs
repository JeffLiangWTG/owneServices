using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal abstract class AgencyShipmentEventParentFinderTest<T> : TestCaseWithFactory where T : AgencyShipment
	{
		public void TestDoNotTryToProcessEventsWithEmptyContainerContextValues()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventType>FOB</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>S0001</Value>
      </Context>
      <Context>
        <Type>LloydsNumber</Type>
        <Value>12345</Value>
      </Context>
      <Context>
        <Type>VesselName</Type>
        <Value>TAIKO</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>001</Value>
      </Context>
      <Context>
        <Type>LegOriginUNLOCO</Type>
        <Value>AUSYD</Value>
      </Context>
      <Context>
        <Type>LegDestinationUNLOCO</Type>
        <Value>NZAKL</Value>
      </Context>
      <Context>
        <Type>ContainerNumber</Type>
        <Value></Value>
      </Context>
      <Context>
        <Type>ContainerISOCode</Type>
        <Value></Value>
      </Context>
      <Context>
        <Type>ContainerReleaseNumber</Type>
        <Value></Value>
      </Context>
      <Context>
        <Type>GoodsItemID</Type>
        <Value></Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var agencyShipment = Factory.NewWithValidTestData<T>();
			agencyShipment.JS_HouseBill = "S0001";
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "NZAKL", "TAIKO", "001");
			agencyShipment.JS_JX = sailing.PK;
			Factory.Save();
			Factory.ResetDatabaseLoadCount();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have not been found", null, logParent);
			AssertEquals("should have not tried to process the event (didn't look for agency shipment in db)", 0, Factory.DatabaseLoadCount);
		}

		public void TestNoMatch()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventType>FOB</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>S0001</Value>
      </Context>
      <Context>
        <Type>LloydsNumber</Type>
        <Value>12345</Value>
      </Context>
      <Context>
        <Type>VesselName</Type>
        <Value>TAIKO</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>001</Value>
      </Context>
      <Context>
        <Type>LegOriginUNLOCO</Type>
        <Value>AUSYD</Value>
      </Context>
      <Context>
        <Type>LegDestinationUNLOCO</Type>
        <Value>NZAKL</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var agencyShipment = Factory.NewWithValidTestData<T>();
			agencyShipment.JS_HouseBill = "XXX";
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_HouseBill = "S0001";
			Factory.Save();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("No Log parent should have been found", null, logParent);
		}

		public void TestMatch()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventType>FOB</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>S0001</Value>
      </Context>
      <Context>
        <Type>CarriersBookingReference</Type>
        <Value>BookingRef001</Value>
      </Context>
      <Context>
        <Type>LloydsNumber</Type>
        <Value>12345</Value>
      </Context>
      <Context>
        <Type>VesselName</Type>
        <Value>TAIKO</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>001</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var agencyShipment = Factory.NewWithValidTestData<T>();
			agencyShipment.JS_HouseBill = "S0001";
			agencyShipment.JS_CFSReference = "BookingRef001";
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "NZAKL", "TAIKO", "001");
			agencyShipment.JS_JX = sailing.PK;
			Factory.Save();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", agencyShipment, logParent);
		}

		public void TestBestMatch()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventType>FOB</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>S0001</Value>
      </Context>
      <Context>
        <Type>LloydsNumber</Type>
        <Value>12345</Value>
      </Context>
      <Context>
        <Type>CarriersBookingReference</Type>
        <Value>BookingRef001</Value>
      </Context>
      <Context>
        <Type>VesselName</Type>
        <Value>TAIKO</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>002</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var agencyShipment1 = Factory.NewWithValidTestData<T>();
			agencyShipment1.JS_HouseBill = "S0001";
			var sailing1 = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "NZAKL", "TAIKO", "001");
			agencyShipment1.JS_JX = sailing1.PK;
			agencyShipment1.JS_CFSReference = "BookingRef001";
			agencyShipment1.JS_SystemCreateTimeUtc = new ZDateTime(2016, 9, 1, 13, 30, 0);
			var agencyShipment2 = Factory.NewWithValidTestData<T>();
			agencyShipment2.JS_HouseBill = "S0001";
			agencyShipment2.JS_CFSReference = "BookingRef001";
			agencyShipment2.JS_SystemCreateTimeUtc = new ZDateTime(2016, 9, 1, 14, 30, 0);
			var sailing2 = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "NZAKL", "TAIKO", "002");
			agencyShipment2.JS_JX = sailing2.PK;
			Factory.Save();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should match correct BOL, Booking Ref", agencyShipment2, logParent);
		}

		public void TestBestMatchWhenEventTypeIsSBR()
		{
			var eventXmlMessage = @"
			<UniversalEvent>
			  <Event>
				<EventType>SBR</EventType>
				<EventParameters>
					<Type>Shipment Visibility</Type>
				</EventParameters>
				<EventTime>10-JUL-2010 18:00</EventTime>
				<EventReference>Dummy Description</EventReference>
				<DataProvider>Dummy</DataProvider>
				<ContextCollection>
				  <Context>
					<Type>MBOLNumber</Type>
					<Value>S0001</Value>
				  </Context>
				  <Context>
					<Type>CarriersBookingReference</Type>
					<Value>BookingRef001</Value>
				  </Context>
				  <Context>
					<Type>CarrierC1CCode</Type>
					<Value>C1CO</Value>
				  </Context>
				</ContextCollection>
			  </Event>
			</UniversalEvent>
			";

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CargoWiseOneCode = "C1CO";
			shippingLine.RSL_IsShippingLine = true;

			var agencyShipment1 = Factory.NewWithValidTestData<T>();
			agencyShipment1.JS_HouseBill = "S0001";
			agencyShipment1.JS_UniqueConsignRef = "BookingRef001";
			agencyShipment1.JS_SystemCreateTimeUtc = new ZDateTime(2016, 9, 1, 13, 30, 0);

			var agencyShipment2 = Factory.NewWithValidTestData<T>();
			agencyShipment2.JS_HouseBill = "S0001";
			agencyShipment2.JS_UniqueConsignRef = "BookingRef002";
			agencyShipment2.JS_SystemCreateTimeUtc = new ZDateTime(2016, 9, 1, 14, 30, 0);

			Factory.Save();

			var logger = new XmlSessionTracker(new SimpleLogger());
			var logParent = ProcessEventXml(eventXmlMessage, logger);
			AssertNull(logParent);
			AssertContains($"Cannot link {GetShipmentType()} because: [* Tracking is not supported for this Carrier. *]", logger.ToString());

			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				logParent = ProcessEventXml(eventXmlMessage, logger);
				AssertEquals("Log parent should match correct BOL, Booking Ref", agencyShipment1, logParent);
				AssertContains("Successfully saved: [*Provider subscription for Carrier Booking Reference 'BookingRef001' was Confirmed. Subscription created*]", logger.ToString());
			}
		}

		public void TestIfMoreThanOneMatchWhenFallingBackToPortOfOriginAndDestinationWhenEventTypeIsSBR()
		{
			const string eventXmlMessage = @"
			<UniversalEvent>
				<Event>
					<EventType>SBR</EventType>
					<EventParameters>
						<Type>Shipment Visibility</Type>
					</EventParameters>
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
			shippingLine.RSL_IsShippingLine = true;

			var factory = new BusinessObjectFactory();

			var agencyShipment1 = Factory.NewWithValidTestData<T>();
			agencyShipment1.JS_HouseBill = "Z103439AKL";
			agencyShipment1.JS_RL_NKOrigin = "AUSYD";
			agencyShipment1.JS_RL_NKDestination = "SGSIN";

			var agencyShipment2 = Factory.NewWithValidTestData<T>();
			agencyShipment2.JS_HouseBill = "Z103439AKL";
			agencyShipment2.JS_RL_NKOrigin = "USLAX";
			agencyShipment2.JS_RL_NKDestination = "NZAKL";

			var agencyShipment3 = Factory.NewWithValidTestData<T>();
			agencyShipment3.JS_HouseBill = "Z103439AKL";
			agencyShipment3.JS_RL_NKOrigin = "AUSYD";
			agencyShipment3.JS_RL_NKDestination = "NZAKL";

			factory.Save();

			var logger = new XmlSessionTracker(new SimpleLogger());
			var logParent = ProcessEventXml(eventXmlMessage, logger);
			AssertNull(logParent);
			AssertContains($"Cannot link {GetShipmentType()} because: [* Tracking is not supported for this Carrier. *]", logger.ToString());

			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				logParent = ProcessEventXml(eventXmlMessage, logger);
				AssertEquals("Log parent should match correct MBOLOriginUNLOCO and MBOLDestinationUNLOCO", agencyShipment3, logParent);
				AssertContains("Successfully saved: [*Provider subscription for Master Bill 'Z103439AKL' was Confirmed. Subscription created*]", logger.ToString());
			}
		}

		public void TestNoMatchIfItIsContainerContext()
		{
			string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventType>FOB</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>S0001</Value>
      </Context>
      <Context>
        <Type>CarriersBookingReference</Type>
        <Value>BookingRef001</Value>
      </Context>
      <Context>
        <Type>LloydsNumber</Type>
        <Value>12345</Value>
      </Context>
      <Context>
        <Type>VesselName</Type>
        <Value>TAIKO</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>001</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var agencyShipment = Factory.NewWithValidTestData<T>();
			agencyShipment.JS_HouseBill = "S0001";
			agencyShipment.JS_CFSReference = "BookingRef001";
			var sailing = UniversalTestHelper.CreateSailingWithVoyage(Factory, "AUSYD", "NZAKL", "TAIKO", "001");
			agencyShipment.JS_JX = sailing.PK;
			Factory.Save();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", agencyShipment, logParent);
			eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventType>FOB</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>S0001</Value>
      </Context>
      <Context>
        <Type>CarriersBookingReference</Type>
        <Value>BookingRef001</Value>
      </Context>
      <Context>
        <Type>LloydsNumber</Type>
        <Value>12345</Value>
      </Context>
      <Context>
        <Type>VesselName</Type>
        <Value>TAIKO</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>001</Value>
      </Context>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>TEST4100013</Value>
      </Context>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>TEST4100014</Value>
      </Context>
      <Context>
        <Type>ContainerISOCode</Type>
        <Value>22G0</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			Factory.Save();
			logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", null, logParent);
		}

		public void TestMatchFailureLogWhenEventTypeIsSBR()
		{
			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty,
					   Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			{
				var eventXmlMessage = @"
				<UniversalEvent>
				  <Event>
					<EventType>SBR</EventType>
					<EventTime>10-JUL-2010 18:00</EventTime>
					<EventReference>Dummy Description</EventReference>
					<EventParameters>
						<Type>Shipment Visibility</Type>
					</EventParameters>
					<DataProvider>Dummy</DataProvider>
					<ContextCollection>
					  <Context>
						<Type>CarrierC1CCode</Type>
						<Value>C1CO</Value>
					  </Context>
					  <Context>
						<Type>MBOLNumber</Type>
						<Value>S0001</Value>
					  </Context>
					  <Context>
						<Type>CarriersBookingReference</Type>
						<Value>BookingRef001</Value>
					  </Context>
					  <Context>
						<Type>CarrierC1CCode</Type>
						<Value>C1CO</Value>
					  </Context>
					</ContextCollection>
				  </Event>
				</UniversalEvent>
				";

				var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
				shippingLine.RSL_CargoWiseOneCode = "C1CO";
				shippingLine.RSL_IsShippingLine = true;

				var logger = new XmlSessionTracker(new SimpleLogger());
				var logParent = ProcessEventXml(eventXmlMessage, logger);
				AssertNull(logParent);
				AssertContains($"Cannot link {GetShipmentType()} because: [* Master Bill Number and Carrier Booking Number are Invalid. *]", logger.ToString());

				shippingLine.RSL_IsShippingLine = false;

				logger = new XmlSessionTracker(new SimpleLogger());
				logParent = ProcessEventXml(eventXmlMessage, logger);
				AssertNull(logParent);
				AssertNotContains($"Cannot link {GetShipmentType()} because: [* Master Bill Number and Carrier Booking Number are Invalid. *]", logger.ToString());
			}
		}

		#region Implementation
		BusinessObject ProcessEventXml(string eventXmlMessage, IXmlImportLogger logger = null)
		{
			var subscriber = new AgencyShipmentEventParentFinder<T>(Factory, GetNewEventDataContextManager(), logger ?? new DummyLogger());
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			return logParents != null && logParents.Length > 0 ? logParents[0] : null;
		}

		protected abstract IEventDataContextManager GetNewEventDataContextManager();

		protected abstract ZString GetShipmentType();

		#endregion
	}
}
