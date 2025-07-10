using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing.FR
{
	sealed class DossierDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject_Shipment()
		{
			var dossier = new Dossier("ForwardingConsol", "S00001015");

			dossier.Carrier = CreateAddress(nameof(dossier.Carrier));
			dossier.ThirdParty = CreateAddress(nameof(dossier.ThirdParty));
			dossier.ReceivingForwarder = CreateAddress(nameof(dossier.ReceivingForwarder));
			dossier.SendingForwarder = CreateAddress(nameof(dossier.SendingForwarder));
			dossier.SendingParty = CreateAddress(nameof(dossier.SendingParty));
			dossier.Agent = CreateAddress(nameof(dossier.Agent));
			dossier.CarrierBookingReference = "BookingReferenceData";
			dossier.BillOfLading = "MasterBillNumberData";
			dossier.ShipmentNumber = "0001";
			dossier.ThirdPartyReference = "00011";
			dossier.DossierAPPlusID = "DossierAPPlusID";
			dossier.OTC = "OTC Data";
			dossier.ATP = "ATP Data";
			dossier.ConfirmationReference = "X0001";
			dossier.ECVReference = "AMQReference, AMQ123";
			dossier.AgentReference = "Agent1, Agent2";
			dossier.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRPAR",
				Name = "Paris"
			};
			dossier.CarrierSON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "CarrierSON");
			dossier.CarrierCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "CarrierCI5");

			dossier.ReceivingForwarderSON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "ReceivingForwarderSON");
			dossier.ReceivingForwarderCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "ReceivingForwarderCI5");

			dossier.SendingForwarderSON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "SendingForwarderSON");
			dossier.SendingForwarderCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "SendingForwarderCI5");

			dossier.SendingPartySON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "SendingPartySON");
			dossier.SendingPartyCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "SendingPartyCI5");

			dossier.ThirdPartySON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "ThirdPartySON");
			dossier.ThirdPartyCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "ThirdPartyCI5");

			dossier.AgentSOA = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "AgentSOA");
			dossier.AgentCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "AgentCI5");

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new DossierDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(dossier);

			AssertUXml(dataObject, expectedShipmentXml);
		}

		public void TestPopulateDataObject_Consol()
		{
			var dossier = new Dossier("ForwardingConsol", "C00001015");

			dossier.Carrier = CreateAddress(nameof(dossier.Carrier));
			dossier.ThirdParty = CreateAddress(nameof(dossier.ThirdParty));
			dossier.ReceivingForwarder = CreateAddress(nameof(dossier.ReceivingForwarder));
			dossier.SendingForwarder = CreateAddress(nameof(dossier.SendingForwarder));
			dossier.SendingParty = CreateAddress(nameof(dossier.SendingParty));
			dossier.Agent = CreateAddress(nameof(dossier.Agent));
			dossier.CarrierBookingReference = "BookingReferenceData";
			dossier.BillOfLading = "MasterBillNumberData";
			dossier.ConsolNumber = "0001";
			dossier.ThirdPartyReference = "00011";
			dossier.DossierAPPlusID = "DossierAPPlusID";
			dossier.OTC = "OTC Data";
			dossier.ATP = "ATP Data";
			dossier.ConfirmationReference = "X0001";
			dossier.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRPAR",
				Name = "Paris"
			};
			dossier.CarrierSON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "CarrierSON");
			dossier.CarrierCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "CarrierCI5");

			dossier.ReceivingForwarderSON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "ReceivingForwarderSON");
			dossier.ReceivingForwarderCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "ReceivingForwarderCI5");

			dossier.SendingForwarderSON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "SendingForwarderSON");
			dossier.SendingForwarderCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "SendingForwarderCI5");

			dossier.SendingPartySON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "SendingPartySON");
			dossier.SendingPartyCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "SendingPartyCI5");

			dossier.ThirdPartySON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "ThirdPartySON");
			dossier.ThirdPartyCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "ThirdPartyCI5");

			dossier.AgentSOA = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "AgentSOA");
			dossier.AgentCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "AgentCI5");

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new DossierDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(dossier);

			AssertUXml(dataObject, expectedConsolXml);
		}

		protected override RegistrationNumber CreateRegistrationNumber(string type, string value)
		{
			return new RegistrationNumber()
			{
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.France
				},
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.France))
				{
					Code = type
				},
				Value = value
			};
		}

		const string expectedShipmentXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>S00001015</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

    </DataContext>

    <BookingConfirmationReference>BookingReferenceData</BookingConfirmationReference>
    <WayBillNumber>MasterBillNumberData</WayBillNumber>
    <WayBillType Description=""Master Waybill"">MWB</WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>FRPAR</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Paris</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>1.0.0</Value>
      </AddInfo>
      <AddInfo>
        <Key>OTC</Key>
        <Value>OTC Data</Value>
      </AddInfo>
      <AddInfo>
        <Key>ATP</Key>
        <Value>ATP Data</Value>
      </AddInfo>
      <AddInfo>
        <Key>ThirdParty_Reference</Key>
        <Value>00011</Value>
      </AddInfo>
      <AddInfo>
        <Key>FileComplete</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>UniqueDeclaration</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>MultipleDeclaration</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>LastDeclaration</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>ImplicitAcknowledgement</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>ExplicitAcknowledgement</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>ImplicitBAET</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>ExplicitBAET</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>NumberOfDeclarations</Key>
        <Value>0</Value>
      </AddInfo>
      <AddInfo>
        <Key>DOSReference</Key>
        <Value>X0001</Value>
      </AddInfo>
      <AddInfo>
        <Key>AMQReference</Key>
        <Value>AMQReference|AMQ123</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>0001</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BookingReferenceData</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>Agent1</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>Agent2</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ReceivingForwarderAddress</AddressType>
        <AdditionalAddressInformation>ReceivingForwarder additional info</AdditionalAddressInformation>
        <Address1>ReceivingForwarder address line 1</Address1>
        <Address2>ReceivingForwarder address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ReceivingForwarder city</City>
        <CompanyName>ReceivingForwarder</CompanyName>
        <Contact>ReceivingForwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ReceivingForwarder email</Email>
        <Fax>ReceivingForwarder f</Fax>
        <GovRegNum>ReceivingForwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ReceivingForwarder p</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ReceivingF</Postcode>
        <State>ReceivingForwarder state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ReceivingForwarderCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ReceivingForwarderSON</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
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
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SendingForwarderCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SendingForwarderSON</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ThirdPartyAddress</AddressType>
        <AdditionalAddressInformation>ThirdParty additional info</AdditionalAddressInformation>
        <Address1>ThirdParty address line 1</Address1>
        <Address2>ThirdParty address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ThirdParty city</City>
        <CompanyName>ThirdParty</CompanyName>
        <Contact>ThirdParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ThirdParty email</Email>
        <Fax>ThirdParty fax</Fax>
        <GovRegNum>ThirdParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ThirdParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ThirdParty</Postcode>
        <State>ThirdParty state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ThirdPartyCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ThirdPartySON</Value>
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
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SendingPartyCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SendingPartySON</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation>Agent additional info</AdditionalAddressInformation>
        <Address1>Agent address line 1</Address1>
        <Address2>Agent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Agent city</City>
        <CompanyName>Agent</CompanyName>
        <Contact>Agent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Agent email</Email>
        <Fax>Agent fax</Fax>
        <GovRegNum>Agent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Agent phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Agent post</Postcode>
        <State>Agent state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>AgentCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>AgentSOA</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string expectedConsolXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001015</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

    </DataContext>

    <BookingConfirmationReference>BookingReferenceData</BookingConfirmationReference>
    <WayBillNumber>MasterBillNumberData</WayBillNumber>
    <WayBillType Description=""Master Waybill"">MWB</WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>FRPAR</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Paris</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>1.0.0</Value>
      </AddInfo>
      <AddInfo>
        <Key>OTC</Key>
        <Value>OTC Data</Value>
      </AddInfo>
      <AddInfo>
        <Key>ATP</Key>
        <Value>ATP Data</Value>
      </AddInfo>
      <AddInfo>
        <Key>ThirdParty_Reference</Key>
        <Value>00011</Value>
      </AddInfo>
      <AddInfo>
        <Key>FileComplete</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>UniqueDeclaration</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>MultipleDeclaration</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>LastDeclaration</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>ImplicitAcknowledgement</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>ExplicitAcknowledgement</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>ImplicitBAET</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>ExplicitBAET</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>NumberOfDeclarations</Key>
        <Value>0</Value>
      </AddInfo>
      <AddInfo>
        <Key>DOSReference</Key>
        <Value>X0001</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>0001</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Booking Reference"">BKG</Type>
        <ReferenceNumber>BookingReferenceData</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

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
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>CarrierCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>CarrierSON</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ReceivingForwarderAddress</AddressType>
        <AdditionalAddressInformation>ReceivingForwarder additional info</AdditionalAddressInformation>
        <Address1>ReceivingForwarder address line 1</Address1>
        <Address2>ReceivingForwarder address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ReceivingForwarder city</City>
        <CompanyName>ReceivingForwarder</CompanyName>
        <Contact>ReceivingForwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ReceivingForwarder email</Email>
        <Fax>ReceivingForwarder f</Fax>
        <GovRegNum>ReceivingForwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ReceivingForwarder p</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ReceivingF</Postcode>
        <State>ReceivingForwarder state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ReceivingForwarderCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ReceivingForwarderSON</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
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
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SendingForwarderCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SendingForwarderSON</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ThirdPartyAddress</AddressType>
        <AdditionalAddressInformation>ThirdParty additional info</AdditionalAddressInformation>
        <Address1>ThirdParty address line 1</Address1>
        <Address2>ThirdParty address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ThirdParty city</City>
        <CompanyName>ThirdParty</CompanyName>
        <Contact>ThirdParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ThirdParty email</Email>
        <Fax>ThirdParty fax</Fax>
        <GovRegNum>ThirdParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ThirdParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ThirdParty</Postcode>
        <State>ThirdParty state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ThirdPartyCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ThirdPartySON</Value>
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
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SendingPartyCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SendingPartySON</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation>Agent additional info</AdditionalAddressInformation>
        <Address1>Agent address line 1</Address1>
        <Address2>Agent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Agent city</City>
        <CompanyName>Agent</CompanyName>
        <Contact>Agent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Agent email</Email>
        <Fax>Agent fax</Fax>
        <GovRegNum>Agent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Agent phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Agent post</Postcode>
        <State>Agent state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>AgentCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>AgentSOA</Value>
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
