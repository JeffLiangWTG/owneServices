using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing.FR
{
	sealed class DemandeDeTracingDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var demandeDeTracing = new DemandeDeTracing("ForwardingConsol", "C00001015");

			demandeDeTracing.ConsolNumber = "C00001015";
			demandeDeTracing.Carrier = CreateAddress(nameof(demandeDeTracing.Carrier));
			demandeDeTracing.SendingForwarder = CreateAddress(nameof(demandeDeTracing.SendingForwarder));
			demandeDeTracing.SendingParty = CreateAddress(nameof(demandeDeTracing.SendingParty));

			demandeDeTracing.SendingPartyCI5 = new RegistrationNumber
			{
				Type = new DummyCodeDescription
				{
					Code = "CI5"
				},
				Value = "CI5 number"
			};

			demandeDeTracing.SendingPartySON = new RegistrationNumber
			{
				Type = new DummyCodeDescription
				{
					Code = "SON"
				},
				Value = "SON number"
			};

			demandeDeTracing.PortOfOrigin = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRPAR",
				Name = "Paris"
			};
			demandeDeTracing.PortOfDestination = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};
			demandeDeTracing.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRPAR",
				Name = "Paris"
			};

			demandeDeTracing.BookingConfirmationReference = "BookingConfirmationReferenceData";

			demandeDeTracing.ContainerMode = new DummyCodeDescription
			{
				Code = Core.Constants.ContainerModes.FCL,
				Description = "Full Container Load"
			};

			demandeDeTracing.ShipmentType = new DummyCodeDescription
			{
				Code = Core.Constants.AgentType.Agent,
				Description = Core.Constants.AgentTypeDescriptions.Agent
			};

			var containers = new List<DemandeDeTracingContainer>()
			{
				CreateContainer("123"),
				CreateContainer("234"),
				CreateContainer("345"),
				CreateContainer("456"),
			};

			demandeDeTracing.Containers = containers;

			demandeDeTracing.WaybillNumber = "WaybillNumberData";

			demandeDeTracing.ETA = new ZDateTime(2022, 04, 14, 00, 00, 00);
			demandeDeTracing.ETD = new ZDateTime(2022, 04, 24, 00, 00, 00);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new DemandeDeTracingDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(demandeDeTracing);

			var workflowInfo = new WorkflowInfo()
			{
				EventUser = new Staff() { Code = GlbStaff.CurrentUser.GS_Code, Name = GlbStaff.CurrentUser.GS_FullName },
				EventBranch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code, Name = GlbBranch.CurrentBranch.GB_BranchName },
				EventDepartment = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code, Name = GlbDepartment.CurrentDepartment.HumanReadableName },
				TriggerDate = new ZDateTimeOffset(2020, 8, 11),
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

			AssertUXml(dataObject, expectedXml);
		}

		DemandeDeTracingContainer CreateContainer(string containerNumber)
		{
			var demandeDeTracingContainer = new DemandeDeTracingContainer(ZGuid.Empty)
			{
				ContainerNumber = containerNumber,
				IsNonOperativeReefer = false
			};

			return demandeDeTracingContainer;
		}

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00001015</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
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
        <EventDepartment Name=""Department"">BRN</EventDepartment>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2020-08-11T00:00:00.000+10:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>

    <BookingConfirmationReference>BookingConfirmationReferenceData</BookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <PortOfDestination Name=""Sydney"">AUSYD</PortOfDestination>
    <PortOfOrigin Name=""Paris"">FRPAR</PortOfOrigin>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <WayBillNumber>WaybillNumberData</WayBillNumber>

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
        <ReferenceNumber>C00001015</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <ContainerCollection>
      <Container>
        <ContainerNumber>123</ContainerNumber>
        <NonOperatingReefer>false</NonOperatingReefer>
      </Container>
      <Container>
        <ContainerNumber>234</ContainerNumber>
        <NonOperatingReefer>false</NonOperatingReefer>
      </Container>
      <Container>
        <ContainerNumber>345</ContainerNumber>
        <NonOperatingReefer>false</NonOperatingReefer>
      </Container>
      <Container>
        <ContainerNumber>456</ContainerNumber>
        <NonOperatingReefer>false</NonOperatingReefer>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <Value>2022-04-14T00:00:00</Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <Value>2022-04-24T00:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AdditionalAddressInformation>SendingForwarder additional info</AdditionalAddressInformation>
        <Address1>SendingForwarder address line 1</Address1>
        <Address2>SendingForwarder address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SendingForwarder city</City>
        <CompanyName>SendingForwarder</CompanyName>
        <Contact>SendingForwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingForwarder email</Email>
        <Fax>SendingForwarder fax</Fax>
        <GovRegNum>SendingForwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingForwarder pho</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SendingFor</Postcode>
        <State>SendingForwarder state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
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
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
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
            <Type>CI5</Type>
            <Value>CI5 number</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>SON</Type>
            <Value>SON number</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		protected override void SetUp()
		{
			base.SetUp();
			context = new CommonContext(Factory);
		}

		CommonContext context;
	}
}
