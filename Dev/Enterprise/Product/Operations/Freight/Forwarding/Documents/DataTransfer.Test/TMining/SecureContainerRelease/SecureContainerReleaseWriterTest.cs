using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Shipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using TMiningConstants = Enterprise.Freight.Forwarding.Business.TMiningConstants;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class SecureContainerReleaseWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject_Transfer()
		{
			var scr = PrepareTestData(SecureContainerRelease.FormModeTransfer);
			scr.Containers = new[]
			{
				CreateContainerTestData("MSCU1247858", "REL210426_3", SecureContainerRelease.FormModeTransfer, TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse, true),
				CreateContainerTestData("MSCU1247858", "REL210426_X", SecureContainerRelease.FormModeTransfer, TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse, false),
				CreateContainerTestData("MSCU1247859", "REL210426_4", SecureContainerRelease.FormModeTransfer, TMiningConstants.SecureContainerReleaseStatus.Accepted, true),
				CreateContainerTestData("MSCU1247860", "REL210426_5", SecureContainerRelease.FormModeTransfer, TMiningConstants.SecureContainerReleaseStatus.Accepted, false),
			};

			PopulateAddresses(scr);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new SecureContainerReleaseDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(scr);

			SetWorkflowInfo(dataObject);

			AssertUXml(dataObject, expectedXmlTransfer);
		}

		public void TestPopulateDataObject_Revoke()
		{
			var scr = PrepareTestData(SecureContainerRelease.FormModeRevoke);
			scr.Containers = new[]
			{
				CreateContainerTestData("MSCU1247858", "REL210426_3", SecureContainerRelease.FormModeRevoke, TMiningConstants.SecureContainerReleaseStatus.TransferSent, true),
				CreateContainerTestData("MSCU1247859", "REL210426_4", SecureContainerRelease.FormModeRevoke, TMiningConstants.SecureContainerReleaseStatus.TransferSent, false),
			};

			PopulateAddresses(scr);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new SecureContainerReleaseDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(scr);

			SetWorkflowInfo(dataObject);

			AssertUXml(dataObject, expectedXmlRevoke);
		}

		#region Expected Xml

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
        <TriggerDate>2023-02-01T00:00:00.000+10:00</TriggerDate>
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
            <Type Description=""EORI code (to be sent verbatim)"">EOR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>TransportCompanyIdEOR</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""DUNS Data Universal Numbering Syste"">DUN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>TransportCompanyIdDUN</Value>
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
            <Type Description=""EORI code (to be sent verbatim)"">EOR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>ForwarderIdEOR</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""DUNS Data Universal Numbering Syste"">DUN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>ForwarderIdDUN</Value>
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
            <Type Description=""EORI code (to be sent verbatim)"">EOR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingPartyIdEOR</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""DUNS Data Universal Numbering Syste"">DUN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingPartyIdDUN</Value>
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
        <TriggerDate>2023-02-01T00:00:00.000+10:00</TriggerDate>
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
            <Type Description=""EORI code (to be sent verbatim)"">EOR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>TransportCompanyIdEOR</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""DUNS Data Universal Numbering Syste"">DUN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>TransportCompanyIdDUN</Value>
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
            <Type Description=""EORI code (to be sent verbatim)"">EOR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>ForwarderIdEOR</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""DUNS Data Universal Numbering Syste"">DUN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>ForwarderIdDUN</Value>
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
            <Type Description=""EORI code (to be sent verbatim)"">EOR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingPartyIdEOR</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""DUNS Data Universal Numbering Syste"">DUN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingPartyIdDUN</Value>
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

		SecureContainerRelease PrepareTestData(string formMode)
		{
			var scr = new SecureContainerRelease("ForwardingConsol", "C00001015");
			scr.FormMode = formMode;

			scr.ContainerMode = new DummyCodeDescription() { Code = Core.Constants.ContainerModes.FCL, Description = "Full Container Load" };
			scr.BillOfLading = "1122334499";

			scr.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "BEANR",
				Name = "Antwerp"
			};

			return scr;
		}

		void SetWorkflowInfo(Shipment dataObject)
		{
			var workflowInfo = new WorkflowInfo()
			{
				EventUser = new Staff() { Code = GlbStaff.CurrentUser.GS_Code, Name = GlbStaff.CurrentUser.GS_FullName },
				EventBranch = new Branch() { Code = GlbBranch.CurrentBranch.GB_Code, Name = GlbBranch.CurrentBranch.GB_BranchName },
				EventDepartment = new Department { Code = GlbDepartment.CurrentDepartment.GE_Code, Name = GlbDepartment.CurrentDepartment.HumanReadableName },
				TriggerDate = new ZDateTimeOffset(2023, 02, 01),
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

		SecureContainerReleaseContainer CreateContainerTestData(ZString containerNumber, ZString releaseIdentification, string formMode, ZString currentStatus, ZBool isTranferToForwarder)
		{
			var container = new SecureContainerReleaseContainer(DefaultDataObjectWriterStrategy.TestInstance, formMode);

			container.Number = containerNumber;
			container.ReleaseIdentification = releaseIdentification;
			container.CurrentStatus = currentStatus;
			container.IsNonOperativeReefer = false;
			container.IsTranferToForwarder = isTranferToForwarder;

			return container;
		}

		void PopulateAddresses(SecureContainerRelease scr)
		{
			scr.TransportCompany = CreateAddress("TransportCompany");
			scr.TransportCompanyIdEOR = CreateRegistrationNumber("EOR", "TransportCompanyIdEOR");
			scr.TransportCompanyIdDUN = CreateRegistrationNumber("DUN", "TransportCompanyIdDUN");
			scr.Forwarder = CreateAddress("Forwarder");
			scr.ForwarderIdEOR = CreateRegistrationNumber("EOR", "ForwarderIdEOR");
			scr.ForwarderIdDUN = CreateRegistrationNumber("DUN", "ForwarderIdDUN");
			scr.SendingParty = CreateAddress("SendingParty");
			scr.SendingPartyIdEOR = CreateRegistrationNumber("EOR", "SendingPartyIdEOR");
			scr.SendingPartyIdDUN = CreateRegistrationNumber("DUN", "SendingPartyIdDUN");
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
