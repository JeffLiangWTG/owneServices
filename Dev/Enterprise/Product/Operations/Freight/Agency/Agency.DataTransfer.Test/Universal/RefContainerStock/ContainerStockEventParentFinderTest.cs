using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class ContainerStockEventParentFinderTest : TestCaseWithFactory
	{
		public void TestIncomingEvent_OnlyProcessesValidEventTypes()
		{
			#region EventMessage
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventType>{0}</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>NOTF1111117</Value>
      </Context>
      <Context>
        <Type>DepotCode</Type>
        <Value>RUMBURAK</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			#endregion
			SetUpSender();
			Func<string, string> messageWithEventType = eventType => string.Format(eventXmlMessage, eventType);
			ProcessEventXML(messageWithEventType(Events.DocumentDeliveredCode));
			AssertContainerMovementMessagesCreated("message hasn't been processed", 0);
			ProcessEventXML(messageWithEventType(Events.FreightLoadedCode));
			AssertContainerMovementMessagesCreated("message has been processed", 1);
		}

		public void TestIncomingEvent_OnlyProcessesWithMandatoryContextValueTypes()
		{
			const string eventXmlMessageTemplate = @"
<UniversalEvent>
  <Event>
    <EventType>FLO</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
       {0}
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			SetUpSender();
			var eventXML = string.Format(eventXmlMessageTemplate, @"<Context>
  <Type>ContainerNumber</Type>
  <Value>NOTF1111117</Value>
</Context>");
			ProcessEventXML(eventXML);
			AssertContainerMovementMessagesCreated("did not process the message as DepotCode is missing", 0);
			eventXML = string.Format(eventXmlMessageTemplate, @"<Context>
  <Type>DepotCode</Type>
  <Value>RUMBURAK</Value>
</Context>");
			ProcessEventXML(eventXML);
			AssertContainerMovementMessagesCreated("did not process the message as ContainerNumber is missing", 0);
			eventXML = string.Format(eventXmlMessageTemplate, @"<Context>
  <Type>ContainerNumber</Type>
  <Value>NOTF1111117</Value>
 </Context>
<Context>
  <Type>DepotCode</Type>
  <Value>RUMBURAK</Value>
</Context>");
			ProcessEventXML(eventXML);
			AssertContainerMovementMessagesCreated("did process the message as event has all mndatory values", 1);
		}

		public void TestIncomingEvent_DoesNotMatchNonExistentContainerNumber()
		{
			#region EventMessage
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventType>FOB</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>NOTF1111117</Value>
      </Context>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>NOTF1111118</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			#endregion
			var dummyContainer = Factory.NewWithValidTestData<RefContainerStock>();
			dummyContainer.R6_ContainerNum = "TEST2222223";
			dummyContainer.R6_RC = Container42P0.PK;
			var logParent = ProcessEventXML(eventXmlMessage);
			AssertEquals("No Log parent should have been found", null, logParent);
		}

		public void TestIncomingEvent_MatchesExistingContainerNumber()
		{
			#region EventMessage
			const string eventXmlMessage = @"
<UniversalEvent>
  <Event>
    <EventType>FOB</EventType>
    <EventTime>10-JUL-2010 18:00</EventTime>
    <EventReference>Dummy Description</EventReference>
    <DataProvider>Dummy</DataProvider>
    <ContextCollection>
      <Context>
        <Type>DepotCode</Type>
        <Value>SHCB1</Value>
      </Context>
      <Context>
        <Type>ContainerNumber</Type>
        <Value>TEST1111117</Value>
      </Context>
      <Context>
        <Type>VoyageNumber</Type>
        <Value>0033</Value>
      </Context>
      <Context>
        <Type>LloydsNumber</Type>
        <Value>9290127</Value>
      </Context>
      <Context>
        <Type>ContainerISOCode</Type>
        <Value>22P1</Value>
      </Context>
      <Context>
        <Type>ContainerOwnershipType</Type>
        <Value>Carrier</Value>
      </Context>
      <Context>
        <Type>IsEmptyContainer</Type>
        <Value></Value>
      </Context>
      <Context>
        <Type>MBOLNumber</Type>
        <Value>BillOfLading</Value>
      </Context>
      <Context>
        <Type>CarriersBookingReference</Type>
        <Value>BookingReference</Value>
      </Context>
      <Context>
        <Type>EntryNumber</Type>
        <Value></Value>
      </Context>
      <Context>
        <Type>EntryNumberType</Type>
        <Value>CAN</Value>
      </Context>
      <Context>
        <Type>EntryNumberCountryOfIssue</Type>
        <Value>AU</Value>
      </Context>
      <Context>
        <Type>ContainerGrossWeight</Type>
        <Value>6800</Value>
      </Context>
      <Context>
        <Type>ContainerSealNo</Type>
        <Value>123456</Value>
      </Context>
      <Context>
        <Type>ContainerSealNo2</Type>
        <Value>654321</Value>
      </Context>
      <Context>
        <Type>ContainerSealNo3</Type>
        <Value></Value>
      </Context>
      <Context>
        <Type>PositioningDateTime</Type>
        <Value>2008-06-27 10:10:0</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
";
			#endregion
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Random Vessel";
			vessel.RV_LloydsNumber = "9290127";
			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_VoyageFlight = "0033";
			voyage1.JV_RV_NKVessel = vessel.RV_FK;
			var voyageOrigin = voyage1.Origins.AddNew();
			voyageOrigin.JA_RL_NKPortOfLoading = "NLAMS";
			var voyageDestination = voyage1.Destinations.AddNew();
			voyageDestination.JB_RL_NKPortOfDischarge = "AUSYD";
			var sailing = voyage1.Sailings[0];
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_HouseBill = "BillOfLading";
			shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			shipment.JS_CFSReference = "BookingReference";
			shipment.JS_JX = sailing.PK;
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_Code = "RANDEP1";
			sender.OH_FullName = "Random Depot1";
			sender.OH_IsUnpackDepot = true;
			sender.OH_RL_NKClosestPort = "AUSYD";
			var customsCode = sender.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			customsCode.OK_CodeType = OrgCusCode.CodeTypes.OneStopCode;
			customsCode.OK_CustomsRegNo = "SHCB1";
			var matchingBolContainer = shipment.RealContainers.AddNew();
			matchingBolContainer.JC_ContainerNum = "TEST1111117";
			matchingBolContainer.JC_RC = Container22P1.PK;
			matchingBolContainer.JC_SealNum = "123456";
			matchingBolContainer.JC_Additional2SealNum = "654321";
			Factory.Save();
			var matchingContainer = Factory.LoadTop1<RefContainerStock>(new ZQuery(RefContainerStockSchema.R6_ContainerNum, "TEST1111117"));
			var logParent = ProcessEventXML(eventXmlMessage);
			AssertLogParentIsCorrect(matchingContainer, logParent);
		}

		#region Implementation
		RefContainer Container22P1
		{
			get
			{
				return container22P1 ?? (container22P1 = new RefContainer.Loader(Factory).LoadFromISOType("22P1"));
			}
		}

		RefContainer container22P1;
		RefContainer Container42P0
		{
			get
			{
				return container42P0 ?? (container42P0 = new RefContainer.Loader(Factory).LoadFromISOType("42P0"));
			}
		}

		RefContainer container42P0;
		void SetUpSender()
		{
			base.SetUp();
			var sender = Factory.NewWithValidTestData<OrgHeader>();
			sender.OH_FullName = "Rumburak";
			sender.OH_RL_NKClosestPort = "AUSYD";
			sender.OH_IsPackDepot = true;
			var code = sender.CustomsCodes.AddNew();
			code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			code.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			code.OK_CustomsRegNo = "RUMBURAK";
			Factory.Save();
		}

		void AssertContainerMovementMessagesCreated(string message, int expectedNumberOfMessages)
		{
			var query = new ZQuery(EDIMessageSchema.EM_MessageType, EDIMessage.ApplicationCodes.ContainerManagement);
			var messages = Factory.Load<EDIMessage>(query);
			AssertEquals(message, expectedNumberOfMessages, messages.Length);
		}

		void AssertLogParentIsCorrect(RefContainerStock matchingContainer, BusinessObject logParent)
		{
			AssertNotNull("logParent", logParent);
			AssertEquals("Type of returned logParent", typeof(RefContainerStock), logParent.GetType());
			var logParentContainerStock = logParent as RefContainerStock;
			AssertNotNull("logParentContainerStock", logParentContainerStock);
			AssertEquals("Should get best matching ContainerStock", GetHumanReadableID(matchingContainer), GetHumanReadableID(logParentContainerStock));
		}

		BusinessObject ProcessEventXML(string eventXmlMessage)
		{
			Factory.Save();
			var subscriber = GetNewEventParentFinder();
			var eventDeserializer = new XmlEventDeserializer();
			var xmlEvent = eventDeserializer.Parse(eventXmlMessage);
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);
			return logParents != null && logParents.Length > 0 ? logParents[0] : null;
		}

		ContainerStockEventParentFinder GetNewEventParentFinder()
		{
			return new ContainerStockEventParentFinder(Factory, new ContainerStockDataContextManager(), new DummyLogger());
		}

		static string GetHumanReadableID(BusinessObject businessObject)
		{
			return businessObject.HumanReadableName + " - PK: " + businessObject.PK;
		}
		#endregion
	}
}
