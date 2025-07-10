using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Shipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.BE.Testing
{
	sealed class CertifiedPickupDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject_AcceptDecline()
		{
			var certifiedPickup = PrepareTestData(CertifiedPickup.FormModeAcceptDecline);
			certifiedPickup.Containers = new[]
			{
				CreateContainerTestData("MSCU1247856", "REL210426_1", CertifiedPickup.FormModeAcceptDecline, CertifiedPickupConstants.Status.Assigned, CertifiedPickupAction.Codes.Accept, "no reason for accept."),
				CreateContainerTestData("MSCU1247857", "REL210426_2", CertifiedPickup.FormModeAcceptDecline, CertifiedPickupConstants.Status.Assigned, CertifiedPickupAction.Codes.Decline, "no reason for decline."),
			};

			PopulateAddresses(certifiedPickup);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CertifiedPickupDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(certifiedPickup);

			SetWorkflowInfo(dataObject);

			AssertUXml(dataObject, expectedXmlAcceptDecline);
		}

		public void TestPopulateDataObject_Transfer()
		{
			var certifiedPickup = PrepareTestData(CertifiedPickup.FormModeTransfer);
			certifiedPickup.Containers = new[]
			{
				CreateContainerTestData("MSCU1247858", "REL210426_3", CertifiedPickup.FormModeTransfer, CertifiedPickupConstants.Status.TransferSentAwaitingResponse, CertifiedPickupAction.Codes.TransferToForwarder, "no reason for revoke."),
				CreateContainerTestData("MSCU1247858", "REL210426_X", CertifiedPickup.FormModeTransfer, CertifiedPickupConstants.Status.TransferSentAwaitingResponse, CertifiedPickupAction.Codes.TransferToTransporter, "no reason for revoke."),
				CreateContainerTestData("MSCU1247859", "REL210426_4", CertifiedPickup.FormModeTransfer, CertifiedPickupConstants.Status.Accepted, CertifiedPickupAction.Codes.TransferToForwarder, "no reason for transfer to forwarding."),
				CreateContainerTestData("MSCU1247860", "REL210426_5", CertifiedPickup.FormModeTransfer, CertifiedPickupConstants.Status.Accepted, CertifiedPickupAction.Codes.TransferToTransporter, "no reason for transfer to transporter."),
			};

			PopulateAddresses(certifiedPickup);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CertifiedPickupDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(certifiedPickup);

			SetWorkflowInfo(dataObject);

			AssertUXml(dataObject, expectedXmlTransfer);
		}

		public void TestPopulateDataObject_Revoke()
		{
			var certifiedPickup = PrepareTestData(CertifiedPickup.FormModeRevoke);
			certifiedPickup.Containers = new[]
			{
				CreateContainerTestData("MSCU1247858", "REL210426_3", CertifiedPickup.FormModeRevoke, CertifiedPickupConstants.Status.TransferSent, CertifiedPickupAction.Codes.TransferToForwarder, "no reason for revoke.", isRevokeMode: true),
				CreateContainerTestData("MSCU1247859", "REL210426_4", CertifiedPickup.FormModeRevoke, CertifiedPickupConstants.Status.TransferSent, CertifiedPickupAction.Codes.TransferToTransporter, "no reason for revoke.", isRevokeMode: true),
			};

			PopulateAddresses(certifiedPickup);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CertifiedPickupDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(certifiedPickup);

			SetWorkflowInfo(dataObject);

			AssertUXml(dataObject, expectedXmlRevoke);
		}

		public void TestPopulateDataObject_MultipleTransferSentAwaitingResponse_NoneToSend()
		{
			var certifiedPickup = PrepareTestData(CertifiedPickup.FormModeTransfer);
			certifiedPickup.Containers = new[]
			{
				CreateContainerTestData("MSCU1247858", "REL210426_3", CertifiedPickup.FormModeTransfer, CertifiedPickupConstants.Status.TransferSentAwaitingResponse, "", "no reason for revoke."),
				CreateContainerTestData("MSCU1247858", "REL210426_3", CertifiedPickup.FormModeTransfer, CertifiedPickupConstants.Status.TransferSentAwaitingResponse, "", "no reason for revoke."),
			};

			PopulateAddresses(certifiedPickup);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CertifiedPickupDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(certifiedPickup);

			AssertNull("Nothing is created.", dataObject);
		}

		#region Expected Xml

		const string expectedXmlAcceptDecline = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001015</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Department"">BRN</EventDepartment>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2021-09-14T00:00:00.000+10:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>

    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <WayBillNumber>1122334499</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>Terminal_Code</Key>
        <Value>00100</Value>
      </AddInfo>
    </AddInfoCollection>

    <ContainerCollection>
      <Container>
        <ContainerImportDORelease>REL210426_1</ContainerImportDORelease>
        <ContainerNumber>MSCU1247856</ContainerNumber>
        <NonOperatingReefer>false</NonOperatingReefer>
        <AddInfoCollection>
          <AddInfo>
            <Key>Action</Key>
            <Value>Accept</Value>
          </AddInfo>
          <AddInfo>
            <Key>Reason</Key>
            <Value>no reason for accept.</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyName</Key>
            <Value>Wisetech Global</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyIdType</Key>
            <Value>Tin</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyIdCode</Key>
            <Value>AU231129369</Value>
          </AddInfo>
        </AddInfoCollection>
      </Container>
      <Container>
        <ContainerImportDORelease>REL210426_2</ContainerImportDORelease>
        <ContainerNumber>MSCU1247857</ContainerNumber>
        <NonOperatingReefer>false</NonOperatingReefer>
        <AddInfoCollection>
          <AddInfo>
            <Key>Action</Key>
            <Value>Decline</Value>
          </AddInfo>
          <AddInfo>
            <Key>Reason</Key>
            <Value>no reason for decline.</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyName</Key>
            <Value>Wisetech Global</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyIdType</Key>
            <Value>Tin</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyIdCode</Key>
            <Value>AU231129369</Value>
          </AddInfo>
        </AddInfoCollection>
      </Container>
    </ContainerCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation>Carrier additional info</AdditionalAddressInformation>
        <Address1>Carrier address line 1</Address1>
        <Address2>Carrier address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Carrier city</City>
        <CompanyName>Carrier</CompanyName>
        <Contact>Carrier contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Carrier email</Email>
        <Fax>Carrier fax</Fax>
        <GovRegNum>Carrier tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Carrier phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Carrier po</Postcode>
        <State>Carrier state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>CarrierIdentificationId</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>SendingParty additional info</AdditionalAddressInformation>
        <Address1>SendingParty address line 1</Address1>
        <Address2>SendingParty address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SendingParty city</City>
        <CompanyName>SendingParty</CompanyName>
        <Contact>SendingParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingParty email</Email>
        <Fax>SendingParty fax</Fax>
        <GovRegNum>SendingParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SendingPar</Postcode>
        <State>SendingParty state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingPartyId</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";

		const string expectedXmlTransfer = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001015</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Department"">BRN</EventDepartment>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2021-09-14T00:00:00.000+10:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>

    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <WayBillNumber>1122334499</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>Terminal_Code</Key>
        <Value>00100</Value>
      </AddInfo>
    </AddInfoCollection>

    <ContainerCollection>
      <Container>
        <ContainerImportDORelease>REL210426_4</ContainerImportDORelease>
        <ContainerNumber>MSCU1247859</ContainerNumber>
        <NonOperatingReefer>false</NonOperatingReefer>
        <AddInfoCollection>
          <AddInfo>
            <Key>Action</Key>
            <Value>Forwarder</Value>
          </AddInfo>
          <AddInfo>
            <Key>Reason</Key>
            <Value>no reason for transfer to forwarding.</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyName</Key>
            <Value>Wisetech Global</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyIdType</Key>
            <Value>Tin</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyIdCode</Key>
            <Value>AU231129369</Value>
          </AddInfo>
        </AddInfoCollection>
      </Container>
      <Container>
        <ContainerImportDORelease>REL210426_5</ContainerImportDORelease>
        <ContainerNumber>MSCU1247860</ContainerNumber>
        <NonOperatingReefer>false</NonOperatingReefer>
        <AddInfoCollection>
          <AddInfo>
            <Key>Action</Key>
            <Value>Transporter</Value>
          </AddInfo>
          <AddInfo>
            <Key>Reason</Key>
            <Value>no reason for transfer to transporter.</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyName</Key>
            <Value>Wisetech Global</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyIdType</Key>
            <Value>Tin</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyIdCode</Key>
            <Value>AU231129369</Value>
          </AddInfo>
        </AddInfoCollection>
      </Container>
    </ContainerCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ArrivalCFSLocalTransportAddress</AddressType>
        <AdditionalAddressInformation>TransportCompany additional info</AdditionalAddressInformation>
        <Address1>TransportCompany address line 1</Address1>
        <Address2>TransportCompany address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>TransportCompany city</City>
        <CompanyName>TransportCompany</CompanyName>
        <Contact>TransportCompany contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>TransportCompany email</Email>
        <Fax>TransportCompany fax</Fax>
        <GovRegNum>TransportCompany tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>TransportCompany pho</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>TransportC</Postcode>
        <State>TransportCompany state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>TransportCompanyId</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ReceivingForwarderAddress</AddressType>
        <AdditionalAddressInformation>Forwarder additional info</AdditionalAddressInformation>
        <Address1>Forwarder address line 1</Address1>
        <Address2>Forwarder address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Forwarder city</City>
        <CompanyName>Forwarder</CompanyName>
        <Contact>Forwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Forwarder email</Email>
        <Fax>Forwarder fax</Fax>
        <GovRegNum>Forwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Forwarder phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Forwarder </Postcode>
        <State>Forwarder state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>ForwarderId</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation>Carrier additional info</AdditionalAddressInformation>
        <Address1>Carrier address line 1</Address1>
        <Address2>Carrier address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Carrier city</City>
        <CompanyName>Carrier</CompanyName>
        <Contact>Carrier contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Carrier email</Email>
        <Fax>Carrier fax</Fax>
        <GovRegNum>Carrier tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Carrier phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Carrier po</Postcode>
        <State>Carrier state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>CarrierIdentificationId</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>SendingParty additional info</AdditionalAddressInformation>
        <Address1>SendingParty address line 1</Address1>
        <Address2>SendingParty address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SendingParty city</City>
        <CompanyName>SendingParty</CompanyName>
        <Contact>SendingParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingParty email</Email>
        <Fax>SendingParty fax</Fax>
        <GovRegNum>SendingParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SendingPar</Postcode>
        <State>SendingParty state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingPartyId</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";

		const string expectedXmlRevoke = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001015</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Department"">BRN</EventDepartment>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2021-09-14T00:00:00.000+10:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>

    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <WayBillNumber>1122334499</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>Terminal_Code</Key>
        <Value>00100</Value>
      </AddInfo>
    </AddInfoCollection>

    <ContainerCollection>
      <Container>
        <ContainerImportDORelease>REL210426_3</ContainerImportDORelease>
        <ContainerNumber>MSCU1247858</ContainerNumber>
        <NonOperatingReefer>false</NonOperatingReefer>
        <AddInfoCollection>
          <AddInfo>
            <Key>Action</Key>
            <Value>Forwarder</Value>
          </AddInfo>
          <AddInfo>
            <Key>Reason</Key>
            <Value>no reason for revoke.</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyName</Key>
            <Value>Wisetech Global</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyIdType</Key>
            <Value>Tin</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyIdCode</Key>
            <Value>AU231129369</Value>
          </AddInfo>
        </AddInfoCollection>
      </Container>
      <Container>
        <ContainerImportDORelease>REL210426_4</ContainerImportDORelease>
        <ContainerNumber>MSCU1247859</ContainerNumber>
        <NonOperatingReefer>false</NonOperatingReefer>
        <AddInfoCollection>
          <AddInfo>
            <Key>Action</Key>
            <Value>Transporter</Value>
          </AddInfo>
          <AddInfo>
            <Key>Reason</Key>
            <Value>no reason for revoke.</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyName</Key>
            <Value>Wisetech Global</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyIdType</Key>
            <Value>Tin</Value>
          </AddInfo>
          <AddInfo>
            <Key>ReleaseFromPartyIdCode</Key>
            <Value>AU231129369</Value>
          </AddInfo>
        </AddInfoCollection>
      </Container>
    </ContainerCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ArrivalCFSLocalTransportAddress</AddressType>
        <AdditionalAddressInformation>TransportCompany additional info</AdditionalAddressInformation>
        <Address1>TransportCompany address line 1</Address1>
        <Address2>TransportCompany address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>TransportCompany city</City>
        <CompanyName>TransportCompany</CompanyName>
        <Contact>TransportCompany contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>TransportCompany email</Email>
        <Fax>TransportCompany fax</Fax>
        <GovRegNum>TransportCompany tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>TransportCompany pho</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>TransportC</Postcode>
        <State>TransportCompany state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>TransportCompanyId</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ReceivingForwarderAddress</AddressType>
        <AdditionalAddressInformation>Forwarder additional info</AdditionalAddressInformation>
        <Address1>Forwarder address line 1</Address1>
        <Address2>Forwarder address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Forwarder city</City>
        <CompanyName>Forwarder</CompanyName>
        <Contact>Forwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Forwarder email</Email>
        <Fax>Forwarder fax</Fax>
        <GovRegNum>Forwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Forwarder phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Forwarder </Postcode>
        <State>Forwarder state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>ForwarderId</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation>Carrier additional info</AdditionalAddressInformation>
        <Address1>Carrier address line 1</Address1>
        <Address2>Carrier address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Carrier city</City>
        <CompanyName>Carrier</CompanyName>
        <Contact>Carrier contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Carrier email</Email>
        <Fax>Carrier fax</Fax>
        <GovRegNum>Carrier tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Carrier phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Carrier po</Postcode>
        <State>Carrier state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>CarrierIdentificationId</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>SendingParty additional info</AdditionalAddressInformation>
        <Address1>SendingParty address line 1</Address1>
        <Address2>SendingParty address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SendingParty city</City>
        <CompanyName>SendingParty</CompanyName>
        <Contact>SendingParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingParty email</Email>
        <Fax>SendingParty fax</Fax>
        <GovRegNum>SendingParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SendingPar</Postcode>
        <State>SendingParty state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingPartyId</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";

		#endregion

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();
			context = new CommonContext(Factory);
		}

		CommonContext context;

		#endregion

		#region Implementation

		CertifiedPickup PrepareTestData(string formMode)
		{
			var certifiedPickup = new CertifiedPickup("ForwardingConsol", "C00001015");
			certifiedPickup.FormMode = formMode;

			certifiedPickup.ContainerMode = new DummyCodeDescription() { Code = Core.Constants.ContainerModes.FCL, Description = "Full Container Load" };
			certifiedPickup.BillOfLading = "1122334499";

			certifiedPickup.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "BEANR",
				Name = "Antwerp"
			};

			certifiedPickup.Terminal = "00100";

			return certifiedPickup;
		}

		void SetWorkflowInfo(Shipment dataObject)
		{
			var workflowInfo = new WorkflowInfo()
			{
				EventUser = new Staff() { Code = GlbStaff.CurrentUser.GS_Code, Name = GlbStaff.CurrentUser.GS_FullName },
				EventBranch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code, Name = GlbBranch.CurrentBranch.GB_BranchName },
				EventDepartment = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code, Name = GlbDepartment.CurrentDepartment.HumanReadableName },
				TriggerDate = new ZDateTimeOffset(2021, 9, 14),
				TriggerCount = 1,
				TriggerType = TriggerType.Manual,
				TriggerDescription = null,
				ActionPurpose = new CodeDescriptionPair()
				{
					Code = DocDataConstants.ActionPurpuses.Codes.AsPerPayload,
					Description = DocDataConstants.ActionPurpuses.Descriptions.AsPerPayload
				}
			};

			dataObject.DataContext.SetWorkflowInfo(workflowInfo);
		}

		CertifiedPickupContainer CreateContainerTestData(ZString containerNumber, ZString releaseIdentification, string formMode, ZString currentStatus, ZString action, ZString reason, bool isRevokeMode = false)
		{
			var container = new CertifiedPickupContainer(DefaultDataObjectWriterStrategy.TestInstance, formMode);

			container.Number = containerNumber;
			container.ReleaseIdentification = releaseIdentification;
			container.CurrentStatus = currentStatus;
			container.Action.ObjectValue = action;
			container.Action.IsRevokeMode = isRevokeMode;

			container.Reason = reason;

			container.ReleaseFromName = "Wisetech Global";
			container.ReleaseFromId = "Tin";
			container.ReleaseFromCode = "AU231129369";

			container.IsNonOperativeReefer = false;

			return container;
		}

		void PopulateAddresses(CertifiedPickup certifiedPickup)
		{
			certifiedPickup.TransportCompany = CreateAddress("TransportCompany");
			certifiedPickup.TransportCompanyId = CreateRegistrationNumber("PSN", "TransportCompanyId");
			certifiedPickup.Forwarder = CreateAddress("Forwarder");
			certifiedPickup.ForwarderId = CreateRegistrationNumber("PSN", "ForwarderId");
			certifiedPickup.SendingParty = CreateAddress("SendingParty");
			certifiedPickup.SendingPartyId = CreateRegistrationNumber("PSN", "SendingPartyId");
			certifiedPickup.Carrier = CreateAddress("Carrier");
			certifiedPickup.CarrierIdentificationId = CreateRegistrationNumber("PSN", "CarrierIdentificationId");
		}

		protected override RegistrationNumber CreateRegistrationNumber(string type, string value)
		{
			return new RegistrationNumber()
			{
				CountryOfIssue = new DocumentVisualizer.DocDataObjects.Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.Belgium
				},
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.Belgium))
				{
					Code = type
				},
				Value = value
			};
		}

		#endregion
	}
}
