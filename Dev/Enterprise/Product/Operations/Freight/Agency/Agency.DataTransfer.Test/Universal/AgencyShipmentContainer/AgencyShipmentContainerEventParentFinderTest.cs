using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class AgencyShipmentContainerEventParentFinderTest : TestCaseWithFactory
	{
		public void TestProcessEventsWithEmptyContainerContextValues()
		{
			const string eventXmlMessage = @"<UniversalEvent>
  <Event>
    <EventType>FOB</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MBL001</Value>
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
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "TAIKO", "001", "12345", "MBL001", "S001");
			Factory.Save();
			Factory.ResetDatabaseLoadCount();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have not been found", null, logParent);
			AssertNotEquals("should have tried to process the event (did look for agency shipment in db)", 0, Factory.DatabaseLoadCount);
		}

		public void TestNoMatch()
		{
			var differentHouseBill = "OOOOOO";
			var differentLloydsNumber = "77777";
			var differentVesselName = "HuntedGhost";
			var differentContainerNumber = "TEST0000021";
			var container1 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "TAIKO", "001", "12345", differentHouseBill, "S003");
			var container2 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", differentVesselName, "001", differentLloydsNumber, "MBL001", "S002");
			var container3 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, differentContainerNumber, "AUSYD", "USSFO", "TAIKO", "001", "12345", "MBL001", "S001");
			Factory.Save();
			string eventXmlMessage = GetEventXmlMessage();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", null, logParent);
		}

		public void TestNoMatchIfShipmentIsNotTheBestMatch()
		{
			var differentLloydsNumber = "77777";
			var container1 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "TAIKO", "001", differentLloydsNumber, "MBL001", "S001");
			Factory.Save();
			string eventXmlMessage = GetEventXmlMessage();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", container1, logParent);
			// container 2 belongs to another shipment which is the best match but container itself is not a match
			var differentVesselName = "HuntedGhost";
			var differentContainerNumber = "TEST0000021";
			var container2 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, differentContainerNumber, "AUSYD", "USSFO", differentVesselName, "001", "12345", "MBL001", "S002");
			Factory.Save();
			logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", null, logParent);
		}

		public void TestMatch()
		{
			var container = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "TAIKO", "001", "12345", "MBL001", "S001");
			Factory.Save();
			string eventXmlMessage = GetEventXmlMessage();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", container, logParent);
		}

		public void TestBestMatch()
		{
			var differentLloydsNumber = "77777";
			var container1 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "TAIKO", "001", differentLloydsNumber, "MBL001", "S001");
			Factory.Save();
			string eventXmlMessage = GetEventXmlMessage();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", container1, logParent);
			var differentVesselName = "HuntedGhost";
			var container2 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "", "AUSYD", "USSFO", differentVesselName, "001", "12345", "MBL001", "S002");
			Factory.Save();
			logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", container2, logParent);
			var container3 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, container2.Booking, "TEST4100013", "20GP", "JTC2323");
			Factory.Save();
			logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", container3, logParent);
			var container4 = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, container2.Booking, "TEST4100013", "20FR", "JTC2323");
			Factory.Save();
			logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", container3, logParent);
		}

		#region Top Level Packs
		public void TestNoMatch_TopLevelPack()
		{
			var differentHouseBill = "OOOOOO";
			var differentLloydsNumber = "77777";
			var differentVesselName = "HuntedGhost";
			var differentGoodsItemID = "TEST0000021";
			var container1 = AgencyShipmentContainerUniversalTestHelper.CreateTopLevelPackForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "TAIKO", "001", "12345", differentHouseBill, "S003", Constants.ContainerModes.RollOnRollOff);
			var container2 = AgencyShipmentContainerUniversalTestHelper.CreateTopLevelPackForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", differentVesselName, "001", differentLloydsNumber, "MBL001", "S002", Constants.ContainerModes.RollOnRollOff);
			var container3 = AgencyShipmentContainerUniversalTestHelper.CreateTopLevelPackForAgencyShipment(Factory, differentGoodsItemID, "AUSYD", "USSFO", "TAIKO", "001", "12345", "MBL001", "S001", Constants.ContainerModes.RollOnRollOff);
			Factory.Save();
			string eventXmlMessage = GetRoroEventXmlMessage();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", null, logParent);
		}

		public void TestNoMatch_FCLShipment_TopLevelPackEvent()
		{
			var container = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "TAIKO", "001", "12345", "MBL001", "S001");
			Factory.Save();
			string eventXmlMessage = GetRoroEventXmlMessage();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", null, logParent);
		}

		public void TestNoMatch_FCLShipment_MixedEvent()
		{
			var container = AgencyShipmentContainerUniversalTestHelper.CreateContainerForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "TAIKO", "001", "12345", "MBL001", "S001");
			Factory.Save();
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
        <Value>MBL001</Value>
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
        <Type>GoodsItemID</Type>
        <Value>TEST4100013</Value>
      </Context>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>TEST4100014</Value>
      </Context>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>TEST4100015</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			var logger = new TestErrorLogger();
			var subscriber = new AgencyShipmentContainerEventParentFinder(Factory, new AgencyShipmentContainerDataContextManager(), logger);
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			AssertEquals("Log parent should have been found", null, logParents);
			AssertEquals("Error - Both Goods Item ID and Container ID are used", logger.Logs);
		}

		public void TestNoMatch_RoroShipment_ContainerEvent()
		{
			var container = AgencyShipmentContainerUniversalTestHelper.CreateTopLevelPackForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "TAIKO", "001", "12345", "MBL001", "S001", Constants.ContainerModes.RollOnRollOff);
			Factory.Save();
			string eventXmlMessage = GetEventXmlMessage();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", null, logParent);
		}

		public void TestNoMatchIfShipmentIsNotTheBestMatch_TopLevelPack()
		{
			var differentLloydsNumber = "77777";
			var container1 = AgencyShipmentContainerUniversalTestHelper.CreateTopLevelPackForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "TAIKO", "001", differentLloydsNumber, "MBL001", "S001", Constants.ContainerModes.RollOnRollOff);
			Factory.Save();
			string eventXmlMessage = GetRoroEventXmlMessage();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", container1, logParent);
			// container 2 belongs to another shipment which is the best match but container itself is not a match
			var differentVesselName = "HuntedGhost";
			var differentGoodsItemID = "TEST0000021";
			var container2 = AgencyShipmentContainerUniversalTestHelper.CreateTopLevelPackForAgencyShipment(Factory, differentGoodsItemID, "AUSYD", "USSFO", differentVesselName, "001", "12345", "MBL001", "S002", Constants.ContainerModes.RollOnRollOff);
			Factory.Save();
			logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", null, logParent);
		}

		public void TestMatch_TopLevelPack()
		{
			var container = AgencyShipmentContainerUniversalTestHelper.CreateTopLevelPackForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "TAIKO", "001", "12345", "MBL001", "S001", Constants.ContainerModes.RollOnRollOff);
			Factory.Save();
			string eventXmlMessage = GetRoroEventXmlMessage();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", container, logParent);
		}

		public void TestBestMatch_TopLevelPack()
		{
			var differentLloydsNumber = "77777";
			var container1 = AgencyShipmentContainerUniversalTestHelper.CreateTopLevelPackForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", "TAIKO", "001", differentLloydsNumber, "MBL001", "S001", Constants.ContainerModes.RollOnRollOff);
			Factory.Save();
			string eventXmlMessage = GetRoroEventXmlMessage();
			var logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", container1, logParent);
			var differentVesselName = "HuntedGhost";
			var container2 = AgencyShipmentContainerUniversalTestHelper.CreateTopLevelPackForAgencyShipment(Factory, "TEST4100013", "AUSYD", "USSFO", differentVesselName, "001", "12345", "MBL001", "S002", Constants.ContainerModes.RollOnRollOff);
			Factory.Save();
			logParent = ProcessEventXml(eventXmlMessage);
			AssertEquals("Log parent should have been found", container2, logParent);
		}

		#endregion
		#region Implementation
		BusinessObject ProcessEventXml(string eventXmlMessage)
		{
			var subscriber = new AgencyShipmentContainerEventParentFinder(Factory, new AgencyShipmentContainerDataContextManager(), new DummyLogger());
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			return logParents != null && logParents.Length > 0 ? logParents[0] : null;
		}

		string GetEventXmlMessage()
		{
			return @"
<UniversalEvent>
  <Event>
    <EventType>FOB</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MBL001</Value>
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
      <Context>
        <Type>ContainerReleaseNumber</Type>
        <Value>JTC2323</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
		}

		string GetRoroEventXmlMessage()
		{
			return @"
<UniversalEvent>
  <Event>
    <EventType>FOB</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>MBL001</Value>
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
        <Type>GoodsItemID</Type>
        <Value>TEST4100013</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
		}
		#endregion
	}
}
