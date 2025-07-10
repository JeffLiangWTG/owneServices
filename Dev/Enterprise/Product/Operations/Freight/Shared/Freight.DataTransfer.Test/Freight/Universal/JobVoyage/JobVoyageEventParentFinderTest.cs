using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class JobVoyageEventParentFinderTest : TestCaseWithFactory
	{
		public void TestNoMatch()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventType>ARV</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>TransportMode</Type>
        <Value>SEA</Value>
      </Context>
      <Context>
        <Type>VesselName</Type>
        <Value>TAIKO</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>002</Value>
      </Context>
      <Context>
        <Type>IsCharter</Type>
        <Value>false</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";

			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("No Log parent should have been found", null, logParent);
		}

		public void TestNoMatchFromEventType()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventType>{0}</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>TransportMode</Type>
        <Value>SEA</Value>
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
        <Type>IsCharter</Type>
        <Value>true</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";

			var logParent = ProcessEventXml(string.Format(eventXmlMessage, "ATH"));
			AssertEquals("No Log parent should have been found, Event Type ATH is invalid for voyages", null, logParent);

			logParent = ProcessEventXml(string.Format(eventXmlMessage, "ARV"));
			AssertEquals("Log parent should have been found, Event Type ARV is valid for voyages", voyageSea1, logParent);

			logParent = ProcessEventXml(string.Format(eventXmlMessage, "DEP"));
			AssertEquals("Log parent should have been found, Event Type DEP is valid for voyages", voyageSea1, logParent);

			logParent = ProcessEventXml(string.Format(eventXmlMessage, "COF"));
			AssertEquals("Log parent should have been found, Event Type COF is valid for voyages", voyageSea1, logParent);

			logParent = ProcessEventXml(string.Format(eventXmlMessage, "CAV"));
			AssertEquals("Log parent should have been found, Event Type CAV is valid for voyages", voyageSea1, logParent);

			logParent = ProcessEventXml(string.Format(eventXmlMessage, "SRC"));
			AssertEquals("Log parent should have been found, Event Type SRC is valid for voyages", voyageSea1, logParent);

			logParent = ProcessEventXml(string.Format(eventXmlMessage, "RTC"));
			AssertEquals("Log parent should have been found, Event Type RTC is valid for voyages", voyageSea1, logParent);

			logParent = ProcessEventXml(string.Format(eventXmlMessage, "WTE"));
			AssertEquals("No Log parent should have been found, Event Type WTE is invalid for voyages", null, logParent);
		}

		public void TestMatch()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventType>ARV</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>TransportMode</Type>
        <Value>SEA</Value>
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
        <Type>IsCharter</Type>
        <Value>true</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";

			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", voyageSea1, logParent);
		}

		public void TestBestMatch()
		{
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventType>ARV</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>TransportMode</Type>
        <Value>AIR</Value>
      </Context>
     <Context>
        <Type>FlightNumber</Type>
        <Value>001</Value>
      </Context>
      <Context>
        <Type>FlightDate</Type>
        <Value>2012-07-19T11:00:00</Value>
      </Context>
      <Context>
        <Type>IsCharter</Type>
        <Value>false</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";

			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should match flight closest to date", voyageAir2, logParent);
		}

		#region Implementation

		BusinessObject ProcessEventXml(string eventXmlMessage)
		{
			Factory.Save();

			var subscriber = new JobVoyageEventParentFinder(Factory, new JobVoyageDataContextManager(), new DummyLogger());
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			return logParents != null && logParents.Length > 0 ? logParents[0] : null;
		}

		JobVoyage voyageSea1;
		JobVoyage voyageSea2;
		JobVoyage voyageAir1;
		JobVoyage voyageAir2;
		JobVoyage voyageRail1;
		JobVoyage voyageRoad1;

		protected override void SetUp()
		{
			base.SetUp();

			voyageSea1 = UniversalTestHelper.CreateSeaVoyage(Factory, "TAIKO", "001");
			voyageSea1.JV_IsChartered = true;

			voyageSea2 = UniversalTestHelper.CreateSeaVoyage(Factory, "ADMIRAL KUZNETSOV", "001");
			voyageSea2.JV_IsChartered = false;

			voyageAir1 = UniversalTestHelper.CreateAirVoyage(Factory, "001");
			voyageAir1.JV_FlightDate = new ZDateTime(2012, 7, 18, 10, 30, 0);
			voyageAir1.JV_IsChartered = false;

			voyageAir2 = UniversalTestHelper.CreateAirVoyage(Factory, "001");
			voyageAir2.JV_FlightDate = new ZDateTime(2012, 7, 20, 10, 30, 0);
			voyageAir2.JV_IsChartered = false;

			voyageRail1 = UniversalTestHelper.CreateRailVoyage(Factory, "TAIKO", "001");
			voyageRail1.JV_IsChartered = false;

			voyageRoad1 = UniversalTestHelper.CreateRoadVoyage(Factory, "001");
			voyageRoad1.JV_IsChartered = false;
		}

		#endregion
	}
}
