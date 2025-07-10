using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ConsolDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.DocDataObjects.ConsolDocumentDataStoreNames;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing.FR
{
	sealed class DemandDeTracingMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendMessage_NoConsol() => AssertNoExceptionThrown(() => DemandeDeTracingMessageSender.SendMessage(null, DemandeDeTracingDirection.Export, out _));

		[TestDate(2020, 7, 25)]
		public void TestSendMessage()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var consol = CreateConsol(isImport: false);
			var containers = CreateContainers();

			Factory.Save();

			var selectedContainers = new[]
			{
				containers.FirstOrDefault()
			};

			var containerSelector = new DummyContainerSelector(selectedContainers);

			var registryCodes = new CommunitySystemCodesOfForwarderAndAgentCollection();
			var code = registryCodes.AddNew();
			code.AgentCode = "AGENT";
			code.ForwarderCode = "FORWARDER";
			code.Port = "FRPAR";
			code.PCS = FrenchPortSystemCodeList.Codes.MGI;

			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			using (ObjectFactory.Substitute<IContainerSelector>(containerSelector))
			using (Factory.AddDisposableService())
			{
				var res = DemandeDeTracingMessageSender.SendMessage(new ForwardingConsolTRCDetailsProvider(consol), DemandeDeTracingDirection.Export, out var notifications);

				Assert("message has been sent", res);
				AssertEquals("no errors messages", 0, notifications.Count);
			}

			var documentDataStorageQuery = new ZQuery();
			documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_Name, ConsolDocumentDataStoreNames.DemandeDeTracingExport);
			documentDataStorageQuery.AddToFilter(JobDocumentDataSchema.JDD_ParentID, consol.PK);

			var documentDataStorage = Factory.LoadTop1<IVisualizerDocumentData>(documentDataStorageQuery);

			AssertNotNull("document data has been created", documentDataStorage);

			var logs = (documentDataStorage as IStmALogParent)
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(l => l.SL_SE_NKEvent == Events.MessageSentCode
					|| l.SL_SE_NKEvent == Events.DataExportCode)
				.ToArray();

			var msn = logs.Single(l => l.SL_SE_NKEvent == Events.MessageSentCode);
			AssertEquals("MSN reference", "|DEP=Terminal|MST=Tracing Request (TRC) - Export", msn.SL_Reference);

			var dex = logs.Single(l => l.SL_SE_NKEvent == Events.DataExportCode);
			var message = dex.RelatedEDIMessage;
			AssertNotNull("EDI message has been created", message);

			AssertMultilineASCIIEquals("UXml",
$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/TRC/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Tracing Request (TRC) - Export</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2020-07-25T00:00:00.000+00:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>

    <BookingConfirmationReference>BookingReference</BookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <PortOfDestination Name=""Sydney"">AUSYD</PortOfDestination>
    <PortOfOrigin Name=""Paris"">FRPAR</PortOfOrigin>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <WayBillNumber>BOL_Reference</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>FRPAR</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Paris</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00001000</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <ContainerCollection>
      <Container>
        <ContainerNumber>123</ContainerNumber>
        <NonOperatingReefer>false</NonOperatingReefer>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <Value>2022-04-24T00:00:00</Value>
      </Date>
    </DateCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{message.Message.Interchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{message.Message.Interchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{message.Message.EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 200</Address1>
        <Address2>55 Why Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Sydney</City>
        <CompanyName>YUMMY</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>2000</Postcode>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 13</Address1>
        <Address2>4 Lost Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Aalborg</City>
        <CompanyName>MAERSK</CompanyName>
        <Contact></Contact>
        <Country Name=""Denmark"">DK</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Aalborg"">DKAAL</Port>
        <Postcode>2000</Postcode>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>10 HUTCHESON STREET</Address1>
        <Address2>ALBION  QLD</Address2>
        <AddressOverride>false</AddressOverride>
        <City></City>
        <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
        <Contact>CargoWise Support</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Brisbane"">AUBNE</Port>
        <Postcode>4010</Postcode>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>FORWARDER</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>",
				message.Message.EM_MessageText);
		}

		public void TestSendMessage_NonUserInteractive()
		{
			var consol = CreateConsol(isImport: false);
			var containers = CreateContainers();
			consol.Containers.AddRange(containers);

			Factory.Save();

			var registryCodes = new CommunitySystemCodesOfForwarderAndAgentCollection();
			var code = registryCodes.AddNew();
			code.AgentCode = "AGENT";
			code.ForwarderCode = "FORWARDER";
			code.Port = "FRPAR";
			code.PCS = FrenchPortSystemCodeList.Codes.MGI;

			using (PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			using (Globals.SetIsUserInteractiveForTest(false))
			using (Factory.AddDisposableService())
			{
				var res = DemandeDeTracingMessageSender.SendMessage(new ForwardingConsolTRCDetailsProvider(consol), DemandeDeTracingDirection.Export, out var notifications);

				Assert("message has been sent", res);
				AssertEquals("no errors messages", 0, notifications.Count);
				AssertNoExceptionThrown("Expecting successful save", Factory.Save);
			}
		}

		#region implement
		ForwardingConsol CreateConsol(bool isAddressAvailableToCreate = true, bool isImport = true)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;

			if (isImport)
			{
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "FRPAR";

				var transportLeg1 = consol.Transports[0];
				transportLeg1.JW_LegOrder = 1;
				transportLeg1.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transportLeg1.JW_RL_NKLoadPort = "AUSYD";
				transportLeg1.JW_RL_NKDiscPort = "BEANR";

				var transportLeg2 = consol.Transports.AddNew();
				transportLeg2.JW_LegOrder = 2;
				transportLeg2.JW_TransportMode = Core.Constants.TransportModes.Road;
				transportLeg2.JW_RL_NKLoadPort = "BEANR";
				transportLeg2.JW_RL_NKDiscPort = "BEZEE";

				var transportLeg3 = consol.Transports.AddNew();
				transportLeg3.JW_LegOrder = 3;
				transportLeg3.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transportLeg3.JW_RL_NKLoadPort = "BEZEE";
				transportLeg3.JW_RL_NKDiscPort = "NLRTM";

				var transportLe4 = consol.Transports.AddNew();
				transportLe4.JW_LegOrder = 4;
				transportLe4.JW_TransportMode = Core.Constants.TransportModes.Road;
				transportLe4.JW_RL_NKLoadPort = "NLRTM";
				transportLe4.JW_RL_NKDiscPort = "NLAMS";

				var transportLeg5 = consol.Transports.AddNew();
				transportLeg5.JW_LegOrder = 5;
				transportLeg5.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transportLeg5.JW_RL_NKLoadPort = "NLAMS";
				transportLeg5.JW_RL_NKDiscPort = "FRPAR";
				transportLeg5.JW_ETA = new DateTime(2022, 04, 14);
			}
			else
			{
				consol.JK_RL_NKLoadPort = "FRPAR";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var transportLeg1 = consol.Transports[0];
				transportLeg1.JW_LegOrder = 1;
				transportLeg1.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transportLeg1.JW_RL_NKLoadPort = "FRPAR";
				transportLeg1.JW_RL_NKDiscPort = "NLAMS";
				transportLeg1.JW_ETD = new DateTime(2022, 04, 24);

				var transportLeg2 = consol.Transports.AddNew();
				transportLeg2.JW_LegOrder = 2;
				transportLeg2.JW_TransportMode = Core.Constants.TransportModes.Road;
				transportLeg2.JW_RL_NKLoadPort = "NLAMS";
				transportLeg2.JW_RL_NKDiscPort = "NLRTM";

				var transportLeg3 = consol.Transports.AddNew();
				transportLeg3.JW_LegOrder = 3;
				transportLeg3.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transportLeg3.JW_RL_NKLoadPort = "NLRTM";
				transportLeg3.JW_RL_NKDiscPort = "BEZEE";

				var transportLe4 = consol.Transports.AddNew();
				transportLe4.JW_LegOrder = 4;
				transportLe4.JW_TransportMode = Core.Constants.TransportModes.Road;
				transportLe4.JW_RL_NKLoadPort = "BEZEE";
				transportLe4.JW_RL_NKDiscPort = "BEANR";

				var transportLeg5 = consol.Transports.AddNew();
				transportLeg5.JW_LegOrder = 5;
				transportLeg5.JW_TransportMode = Core.Constants.TransportModes.Sea;
				transportLeg5.JW_RL_NKLoadPort = "BEANR";
				transportLeg5.JW_RL_NKDiscPort = "AUSYD";
			}

			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			if (isAddressAvailableToCreate)
			{
				CreateAddresses(consol);
			}

			return consol;
		}

		IReadOnlyCollection<ForwardingContainer> CreateContainers()
		{
			var containers = new List<ForwardingContainer>();

			var container1 = Factory.New<ForwardingContainer>();
			container1.JC_ContainerNum = "123";
			containers.Add(container1);

			var container2 = Factory.New<ForwardingContainer>();
			container2.JC_ContainerNum = "456";
			containers.Add(container2);

			var container3 = Factory.New<ForwardingContainer>();
			container3.JC_ContainerNum = "789";
			containers.Add(container3);

			return containers;
		}

		void CreateAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var bookingAgent = Factory.New<OrgHeader>();
			bookingAgent.OH_FullName = "I'm Handling Stuff";
			bookingAgent.OH_RL_NKClosestPort = "AUSYD";
			bookingAgent.MainAddress.Address1 = "Unit 2";
			bookingAgent.MainAddress.Address2 = "60 What Lane";
			bookingAgent.MainAddress.City = "Sydney";
			bookingAgent.MainAddress.Postcode = "2023";
			bookingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "YUMMY";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 200";
			receivingForwarder.MainAddress.Address2 = "55 Why Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "2000";
			receivingForwarder.MainAddress.State = "NSW";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "YUMMY";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.State = "NSW";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
		}
		#endregion
	}
}
