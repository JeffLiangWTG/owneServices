using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class UnderbondMovementRequestDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var request = new UnderbondMovementRequest(nameof(ForwardingConsol), "C00001000");
			request.ConsolNumber = "C00001000";
			request.DTI = true;

			request.CarrierBookingReference = "B0001100";
			request.ContainerMode = new DummyCodeDescription { Code = Constants.ContainerModes.FCL };
			request.ShipmentType = new DummyCodeDescription { Code = Core.Constants.AgentType.Agent };

			request.PortOfOrigin = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "FRPAR", Name = "Paris" };
			request.PortOfDestination = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "AUSYD", Name = "Sydney" };
			request.PortOfTranshipment = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "NZAKL", Name = "Auckland" };

			request.VesselName = "Dragon";
			request.VoyageFlightNo = "029N";
			request.BillOfLading = "SUDUN0SHA051511X";

			PopulateAddInfos(request);

			PopulateAddresses(request);

			PopulateRegistrationNumbers(request);

			PopulateContainers(request);

			PopulateNotes(request);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new UnderbondMovementRequestDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(request);

			AssertUXml(dataObject, expectedXml);
		}

		static void PopulateNotes(UnderbondMovementRequest request)
		{
			request.Notes = "Carrier Booking Notes";
		}

		void PopulateContainers(UnderbondMovementRequest request)
		{
			var container = new UnderbondMovementRequestContainer
			{
				Number = "AAAA0000007",
				ECTICTNumber = "ECT0000976",
				ContainerType = new ContainerType(context.ContainerTypes)
				{
					Code = "20GP"
				},
				IsNonOperativeReefer = false
			};

			request.Containers = new[] { container };
		}

		void PopulateRegistrationNumbers(UnderbondMovementRequest request)
		{
			request.CarrierSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "CARSON");
			request.CarrierCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "CARCI5");
			request.CarrierCCC = CrateRegistrationNumber(OrgCusCode.CodeTypes.CarrierCode, "CARCCC");
			request.SendingForwarderSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "SEDSON");
			request.SendingForwarderCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "SEDCI5");
			request.ReceivingForwarderSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "REVSON");
			request.ReceivingForwarderCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "REVCI5");
			request.TransporterSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "REVSON");
			request.TransporterCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "REVCI5");
			request.SendingPartySON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "GLBSON");
			request.SendingPartyCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "GLBCI5");
		}

		void PopulateAddresses(UnderbondMovementRequest request)
		{
			request.Carrier = CreateAddress("Carrier");
			request.SendingForwarder = CreateAddress("SendingForwarder");
			request.ReceivingForwarder = CreateAddress("ReceivingForwarder");
			request.Transporter = CreateAddress("Transporter");
			request.SendingParty = CreateAddress("SendingParty");
		}

		void PopulateAddInfos(UnderbondMovementRequest request)
		{
			request.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "FRPAR", Name = "Paris" };
			request.PortLocationFrom = new DummyCodeDescription { Code = "PORTLOCFROM" };
			request.PortAreaFrom = new DummyCodeDescription { Code = "PORTAREAFROM" };
			request.PortLocationTo = new DummyCodeDescription { Code = "PORTLOCTO" };
			request.PortAreaTo = new DummyCodeDescription { Code = "PORTAREATO" };
			request.ATP = "ATPREF";
			request.BookingConfirmationCBK = "CBKREFERENCE";
			request.TransportMode = "RTE";
			request.VehicleRegistration = "1IC7YF";
			request.Subcontracted = true;
			request.RequestStatus = new UnderbondMovementRequestStatus { IsProvisional = true };
			request.ReasonID = new UnderbondMovementRequestReasonID { IsDE = true };
			request.ReasonNote = "REASON NOTES";
			request.AuthorizationRequired = new UnderbondMovementRequestAuthorizationRequired { IsPhytosanitary = true };
			request.DateOfResponse = new ZDateTime(2020, 7, 20, 7, 30, 1);
			request.ResponseType = new UnderbondMovementRequestAuthorizationResponseType { IsAgreementWithReservations = true };
			request.DeclarationDate = new ZDateTime(2020, 7, 21, 7, 50, 6);
			request.DeclarationNumber = "DEC000098";
			request.DeclarationVersion = "1";
			request.BOLAPPlusID = "BOLAP000098";
			request.MoveReqAPPlusID = "REQAP000098";
			request.PortDuesAmount = 10000;
			request.PortDuesPort = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "FRMRS", Name = "Marseille" };
			request.PortDuesCurrency = new DummyCodeDescription { Code = "EUR" };
			request.PortDuesPayingPartyAPPlusID = "BOLLORE";
		}

		RegistrationNumber CrateRegistrationNumber(string type, string value)
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

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

    </DataContext>

    <BookingConfirmationReference>B0001100</BookingConfirmationReference>
    <ContainerMode>FCL</ContainerMode>
    <PortFirstForeign Name=""Auckland"">NZAKL</PortFirstForeign>
    <PortOfDestination Name=""Sydney"">AUSYD</PortOfDestination>
    <PortOfOrigin Name=""Paris"">FRPAR</PortOfOrigin>
    <ShipmentType>AGT</ShipmentType>
    <VesselName>Dragon</VesselName>
    <VoyageFlightNo>029N</VoyageFlightNo>
    <WayBillNumber>SUDUN0SHA051511X</WayBillNumber>

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
        <Key>PortLocationFrom</Key>
        <Value>PORTLOCFROM</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortAreaFrom</Key>
        <Value>PORTAREAFROM</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortLocationTo</Key>
        <Value>PORTLOCTO</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortAreaTo</Key>
        <Value>PORTAREATO</Value>
      </AddInfo>
      <AddInfo>
        <Key>ATP</Key>
        <Value>ATPREF</Value>
      </AddInfo>
      <AddInfo>
        <Key>BookingConfirmationCBK</Key>
        <Value>CBKREFERENCE</Value>
      </AddInfo>
      <AddInfo>
        <Key>TransportMode</Key>
        <Value>RTE</Value>
      </AddInfo>
      <AddInfo>
        <Key>VehicleRegistration</Key>
        <Value>1IC7YF</Value>
      </AddInfo>
      <AddInfo>
        <Key>Subcontracted</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>RequestStatus</Key>
        <Value>PRO</Value>
      </AddInfo>
      <AddInfo>
        <Key>ReasonID</Key>
        <Value>DE</Value>
      </AddInfo>
      <AddInfo>
        <Key>ReasonNote</Key>
        <Value>REASON NOTES</Value>
      </AddInfo>
      <AddInfo>
        <Key>AuthorizationRequired</Key>
        <Value>P</Value>
      </AddInfo>
      <AddInfo>
        <Key>DateOfResponse</Key>
        <Value>2020-07-20T07:30:01</Value>
      </AddInfo>
      <AddInfo>
        <Key>ResponseType</Key>
        <Value>PAR</Value>
      </AddInfo>
      <AddInfo>
        <Key>DeclarationDate</Key>
        <Value>2020-07-21T07:50:06</Value>
      </AddInfo>
      <AddInfo>
        <Key>DeclarationNumber</Key>
        <Value>DEC000098</Value>
      </AddInfo>
      <AddInfo>
        <Key>DeclarationVersion</Key>
        <Value>1</Value>
      </AddInfo>
      <AddInfo>
        <Key>BOLAPPlusID</Key>
        <Value>BOLAP000098</Value>
      </AddInfo>
      <AddInfo>
        <Key>MoveReqAPPlusID</Key>
        <Value>REQAP000098</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortDuesPortCode</Key>
        <Value>FRMRS</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortDuesAmount</Key>
        <Value>10000</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortDuesCurrency</Key>
        <Value>EUR</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortDuesPayingParty</Key>
        <Value>BOLLORE</Value>
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
        <ContainerNumber>AAAA0000007</ContainerNumber>
        <ContainerType>
          <Code>20GP</Code>
          <Category Description=""Dry Storage"">DRY</Category>
          <Description>Twenty foot general purpose</Description>
          <ISOCode>22G0</ISOCode>
        </ContainerType>
        <IsEmptyContainer>false</IsEmptyContainer>
        <NonOperatingReefer>false</NonOperatingReefer>
        <AddInfoCollection>
          <AddInfo>
            <Key>ECTICTNumber</Key>
            <Value>ECT0000976</Value>
          </AddInfo>
        </AddInfoCollection>
      </Container>
    </ContainerCollection>

    <NoteCollection>
      <Note>
        <Description>Additional Instruction Notes</Description>
        <NoteText>Carrier Booking Notes</NoteText>
      </Note>
    </NoteCollection>

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
            <Value>CARCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>CARSON</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""Customs Carrier Code"">CCC</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>CARCCC</Value>
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
            <Value>SEDCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SEDSON</Value>
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
            <Value>REVCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>REVSON</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ArrivalCFSLocalTransportAddress</AddressType>
        <AdditionalAddressInformation>Transporter additional info</AdditionalAddressInformation>
        <Address1>Transporter address line 1</Address1>
        <Address2>Transporter address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Transporter city</City>
        <CompanyName>Transporter</CompanyName>
        <Contact>Transporter contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Transporter email</Email>
        <Fax>Transporter fax</Fax>
        <GovRegNum>Transporter tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Transporter phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Transporte</Postcode>
        <State>Transporter state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>REVCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>REVSON</Value>
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
            <Value>GLBCI5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>GLBSON</Value>
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
