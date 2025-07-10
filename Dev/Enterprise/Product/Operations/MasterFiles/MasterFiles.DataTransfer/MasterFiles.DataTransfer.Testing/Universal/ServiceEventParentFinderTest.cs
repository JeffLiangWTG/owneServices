using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using DummyLogger = Enterprise.UniversalDataBuss.Integration.DummyLogger;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class ServiceEventParentFinderTest : TestCaseWithFactory
	{
		public void TestGetLogParentsForEventUsingContext_ContextServiceIdIsEmpty()
		{
			const string eventXmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns = ""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>Service</Type>
          <Key></Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>DAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Your Australia Corp</Name>
      </Company>
      <DataProvider>WTLKKKDAU</DataProvider>
      <EnterpriseID>WTL</EnterpriseID>
      <EventBranch>
        <Code>A01</Code>
        <Name>AU - Branch 1</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>SVR</Code>
        <Description>Service Requested</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>KKK</ServerID>
      <TriggerCount>26</TriggerCount>
      <TriggerDate>2023-09-19T18:01:00</TriggerDate>
      <TriggerDescription>test line triggers</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organisation Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>2023-09-19T18:01:00</EventTime>
    <EventType>SVR</EventType>
    <CreatedTime>2023-10-13T10:14:06.803</CreatedTime>
    <EventReference>|TYP=FUS</EventReference>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>ServiceId</Type>
        <Value></Value>
      </Context>
      <Context>
        <Type>ExternalServiceId</Type>
        <Value></Value>
      </Context>
      <Context>
        <Type>ServiceType</Type>
        <Value>FUS</Value>
      </Context>
      <Context>
        <Type>ServiceCount</Type>
        <Value>1.000</Value>
      </Context>
      <Context>
        <Type>ServiceContractor</Type>
        <Value>ACCGREAKL</Value>
      </Context>
      <Context>
        <Type>ServiceNotes</Type>
        <Value>ddd</Value>
      </Context>
      <Context>
        <Type>ServiceRate</Type>
        <Value>2.0000</Value>
      </Context>
      <Context>
        <Type>ServiceSubLocation</Type>
        <Value>123</Value>
      </Context>
      <Context>
        <Type>ServiceLocation</Type>
        <Value>ABC</Value>
      </Context>
      <Context>
        <Type>ServiceLocationAddress</Type>
        <Value>100 ST GEORGES TERRACE</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
			var log = ProcessEventXml(eventXmlText);
			AssertNull(log);
		}

		public void TestGetLogParentsForEventUsingContext_MatchBasedOnExternalServiceId()
		{
			var contractor = Factory.NewWithValidTestData<OrgHeader>();
			contractor.OH_Code = "XXXXXX";

			var location = Factory.NewWithValidTestData<OrgHeader>();
			location.OH_Code = "YYYYYY";
			location.MainAddress.OA_Code = "ZZZZZZ";

			var dummyConsignmentWithServices = Factory.NewWithValidTestData<DummyConsignmentWithServices>();
			dummyConsignmentWithServices.ShipmentNumberForTest = "AAAAAA";

			var service = dummyConsignmentWithServices.Services.AddNew();
			service.ES_ServiceId = "WTLKKK00000044";
			service.ES_ServiceCode = Core.Constants.FreightServiceType.Codes.Fumigation;
			service.ES_ServiceCount = 1;
			service.ES_ServiceNote = "Note";
			service.ES_ServiceRate = 150m;
			service.ES_OA_Location = location.MainAddress.PK;
			service.ES_RX_NKServiceRateCurrency = Core.Constants.CurrencyCodes.Australia;
			service.ES_MeasurementBasis = JobServiceInfo.Constants.Codes.Day;
			service.ES_SubLocation = "123";
			service.ES_Duration = new TimeSpan(3, 0, 0, 0);
			service.ES_References = "REF1111111";
			service.ES_Completed = ZDateTime.Now;
			service.ES_OH_Contractor = contractor.PK;
			service.ES_ExternalServiceId = "WTLKKK00000043";

			const string eventXmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns = ""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>Service</Type>
          <Key>WTLKKK00000043</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>DAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Your Australia Corp</Name>
      </Company>
      <DataProvider>WTLKKKDAU</DataProvider>
      <EnterpriseID>WTL</EnterpriseID>
      <EventBranch>
        <Code>A01</Code>
        <Name>AU - Branch 1</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>SVR</Code>
        <Description>Service Requested</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>KKK</ServerID>
      <TriggerCount>26</TriggerCount>
      <TriggerDate>2023-09-19T18:01:00</TriggerDate>
      <TriggerDescription>test line triggers</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organisation Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>2023-09-19T18:01:00</EventTime>
    <EventType>SVR</EventType>
    <CreatedTime>2023-10-13T10:14:06.803</CreatedTime>
    <EventReference>|TYP=FUS</EventReference>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>ServiceId</Type>
        <Value>WTLKKK00000043</Value>
      </Context>
      <Context>
        <Type>ExternalServiceId</Type>
        <Value>WTLKKK00000044</Value>
      </Context>
      <Context>
        <Type>ServiceType</Type>
        <Value>FUS</Value>
      </Context>
      <Context>
        <Type>ServiceCount</Type>
        <Value>1.000</Value>
      </Context>
      <Context>
        <Type>ServiceContractor</Type>
        <Value>ACCGREAKL</Value>
      </Context>
      <Context>
        <Type>ServiceNotes</Type>
        <Value>ddd</Value>
      </Context>
      <Context>
        <Type>ServiceRate</Type>
        <Value>2.0000</Value>
      </Context>
      <Context>
        <Type>ServiceSubLocation</Type>
        <Value>123</Value>
      </Context>
      <Context>
        <Type>ServiceLocation</Type>
        <Value>ABC</Value>
      </Context>
      <Context>
        <Type>ServiceLocationAddress</Type>
        <Value>100 ST GEORGES TERRACE</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
			var log = ProcessEventXml(eventXmlText);
			AssertEquals(service, log);
		}

		public void TestGetLogParentsForEventUsingContext_NoMatchFound()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001586";

			const string eventXmlText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns = ""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>Service</Type>
          <Key>WTLKKK00000043</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>DAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Your Australia Corp</Name>
      </Company>
      <DataProvider>WTLKKKDAU</DataProvider>
      <EnterpriseID>WTL</EnterpriseID>
      <EventBranch>
        <Code>A01</Code>
        <Name>AU - Branch 1</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>SVR</Code>
        <Description>Service Requested</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>KKK</ServerID>
      <TriggerCount>26</TriggerCount>
      <TriggerDate>2023-09-19T18:01:00</TriggerDate>
      <TriggerDescription>test line triggers</TriggerDescription>
      <TriggerType>Trigger</TriggerType>
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organisation Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>2023-09-19T18:01:00</EventTime>
    <EventType>SVR</EventType>
    <CreatedTime>2023-10-13T10:14:06.803</CreatedTime>
    <EventReference>|TYP=FUS</EventReference>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>ServiceId</Type>
        <Value>WTLKKK00000043</Value>
      </Context>
      <Context>
        <Type>ExternalServiceId</Type>
        <Value></Value>
      </Context>
      <Context>
        <Type>ServiceType</Type>
        <Value>FUS</Value>
      </Context>
      <Context>
        <Type>ServiceCount</Type>
        <Value>1.000</Value>
      </Context>
      <Context>
        <Type>ServiceContractor</Type>
        <Value>ACCGREAKL</Value>
      </Context>
      <Context>
        <Type>ServiceNotes</Type>
        <Value>ddd</Value>
      </Context>
      <Context>
        <Type>ServiceRate</Type>
        <Value>2.0000</Value>
      </Context>
      <Context>
        <Type>ServiceSubLocation</Type>
        <Value>123</Value>
      </Context>
      <Context>
        <Type>ServiceLocation</Type>
        <Value>ABC</Value>
      </Context>
      <Context>
        <Type>ServiceLocationAddress</Type>
        <Value>100 ST GEORGES TERRACE</Value>
      </Context>
      <Context>
        <Type>ShipmentNumber</Type>
        <Value>S00001586</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
			var log = ProcessEventXml(eventXmlText);
			AssertNotNull(log);
			AssertEquals(shipment, log.RequestForServiceParent);
		}

		JobService ProcessEventXml(string eventXmlText)
		{
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlText);
			var subscriber = GetNewEventParentFinder();
			return subscriber.GetLogParentsForEvent(xmlEvent)?.FirstOrDefault() as JobService;
		}

		ServiceEventParentFinder GetNewEventParentFinder()
		{
			return new ServiceEventParentFinder(Factory, new ServiceDataContextManager(), new DummyLogger());
		}
	}
}
