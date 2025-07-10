using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Freight.ContainerYard.Business.Testing
{
	sealed class GateTransportEventParentFinderTest : TestCaseWithFactory
	{
		public void TestImportUniversalContainerEventWithGateAsTarget_WhenGateTransportHasCFSDetail()
		{
			var gateTransport = CreateGateTransportWithCFSDetails("ULD012021B", string.Empty, ZDateTime.BrettsBirthday, ZDateTime.Empty);
			var secondGateTransport = CreateGateTransportWithCFSDetails("ULD012021B", string.Empty, ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday);
			var thirdGateTransport = CreateGateTransportWithCFSDetails("ULD012021C", string.Empty, ZDateTime.BrettsBirthday, ZDateTime.Empty);

			var log = ProcessEventXML(UXmlContainerEventSampleMessage);
			AssertLogParentIsCorrect(gateTransport, log);
		}

		public void TestImportUniversalCargoTruckEventWithGateAsTarget()
		{
			var gateTransport = CreateGateTransportWithVehicle("Tm2203", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			var secondGateTransport = CreateGateTransportWithVehicle("Tm2203", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday);
			var thirdGateTransport = CreateGateTransportWithVehicle("Tm2204", ZDateTime.BrettsBirthday, ZDateTime.Empty);

			Factory.Save();

			var log = ProcessEventXML(UXmlCargoTruckEventSampleMessage);
			AssertLogParentIsCorrect(gateTransport, log);
		}

		public static string UXmlContainerEventSampleMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns = ""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>TransitReceiveHeader</Type>
          <Key>TR00000061</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>FLO</Code>
        <Description>Full Loaded</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDescription>test</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>WHS</Code>
          <Description>Warehouse</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>2021-02-12T15:28:20.45</EventTime>
    <EventType>FLO</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>TransportReference</Type>
        <Value>ULD012021B</Value>
      </Context>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>327-0102MABA</Value>
      </Context>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>ULD012021B</Value>
      </Context>
      <Context>
        <Type>DepotCode</Type>
        <Value>BRISBANE DEPOT</Value>
      </Context>
      <Context>
        <Type>TimeOfArrival</Type>
        <Value>2021-02-01T10:32:00.000+11:00</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

		public static string UXmlCargoTruckEventSampleMessage = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>TransitReceiveHeader</Type>
          <Key>TR00001719</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>DAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>AU Demo Company</Name>
      </Company>
      <DataProvider>HYETIPDAU</DataProvider>
      <EnterpriseID>HYE</EnterpriseID>
      <EventBranch>
        <Code>SYD</Code>
        <Name>SYDNEY</Name>
      </EventBranch>
      <EventDepartment>
        <Code>TT3</Code>
        <Name></Name>
      </EventDepartment>
      <EventType>
        <Code>FUL</Code>
        <Description> Full Unloaded</Description>
      </EventType>
      <EventUser>
        <Code>TM</Code>
        <Name>Tiona Munoz</Name>
      </EventUser>
      <ServerID>TIP</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2021-03-22T16:01:27.26</TriggerDate>
      <TriggerDescription>RTU Gate In</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organisation Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>2021-03-22T16:01:27.26</EventTime>
    <EventType>FUL</EventType>
    <CreatedTime>2021-03-22T05:01:27.26</CreatedTime>
    <EventReference>|FAC= CFS | LOC = ALEXANDRIA | TYP = VehicleReference | REF = Tm2203 </EventReference>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>TransportReference</Type>
        <Value>Tm2203</Value>
      </Context>
      <Context>
        <Type>DepotCode</Type>
        <Value>Syd TWH 72 O'Riordan Stre</Value>
      </Context>
      <Context>
        <Type>TimeOfArrival</Type>
        <Value>2021-03-22T16:00:00.000+11:00</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";

		BusinessObject ProcessEventXML(string orderLevelEventXmlText)
		{
			Factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(orderLevelEventXmlText);
			return subscriber.GetLogParentsForEvent(xmlEvent)[0];
		}

		GateTransportEventParentFinder GetNewEventParentFinder()
		{
			return new GateTransportEventParentFinder(Factory, new GateTransportContextManager(), new DummyLogger());
		}

		void AssertLogParentIsCorrect(GateTransport gateTransport, BusinessObject logParent)
		{
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(GateTransport), logParent.GetType());

			var logParentGateTransport = logParent as GateTransport;

			AssertNotNull("logParentOrder", logParentGateTransport);
			AssertEquals("Should get best matching Order.", gateTransport.PK, logParentGateTransport.PK);
		}

		GateTransport CreateGateTransportWithCFSDetails(string containerNumber, string truckNumbr, ZDateTime timeIn, ZDateTime timeOut)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var yardUnit = Factory.NewWithValidTestData<YardUnitPersistentForTesting>();
			yardUnit.GTY_UnitNumber = containerNumber;

			var gateBooking = Factory.NewWithValidTestData<GateBooking>();

			var whsWarehouse = Factory.NewWithValidTestData<WhsWarehouse>();

			var bookingDetail = Factory.New<GateBookingDetail>();
			bookingDetail.GTD_GTY_YardUnit = yardUnit.PK;
			bookingDetail.GTD_WW_Facility = whsWarehouse.PK;
			bookingDetail.GTD_GTB_GateBooking = gateBooking.PK;

			bookingDetail.GTD_OA_BookingPartyAddress = org.MainAddress.PK;

			Factory.Save();

			var gateTransportCFSDetail = Factory.NewWithValidTestData<GateTransportCFSDetail>();
			gateTransportCFSDetail.GTF_GTD_GateBookingDetail = bookingDetail.PK;

			var gateTransport = Factory.NewWithValidTestData<GateTransport>();
			gateTransport.GTT_VehicleRegistration = truckNumbr;
			gateTransport.GTT_TimeIn = timeIn;
			gateTransport.GTT_TimeOut = timeOut;
			gateTransport.GateTransportCFSDetails.Add(gateTransportCFSDetail);
			return gateTransport;
		}

		GateTransport CreateGateTransportWithVehicle(string vehicleRegistrationNumber, ZDateTime timeIn, ZDateTime timeOut)
		{
			var gateTransport = Factory.NewWithValidTestData<GateTransport>();
			gateTransport.GTT_VehicleRegistration = vehicleRegistrationNumber;
			gateTransport.GTT_TimeIn = timeIn;
			gateTransport.GTT_TimeOut = timeOut;
			return gateTransport;
		}
	}
}
