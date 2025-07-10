using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.NL.Testing
{
	sealed class PortbaseImportNotificationDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var portbase = PrepareTestData();

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new PortbaseImportNotificationDataObjectWriter(manager);

			var dataObject = writer.GetDataObject(portbase);

			AssertUXml(dataObject, expectedXml);
		}

		PortbaseImportNotification PrepareTestData()
		{
			var portbase = new PortbaseImportNotification("ForwardingConsol", "C00001001", DataContext.NLPortbaseImportNotification);

			portbase.ConsolNumber = "C00001001";
			portbase.TransportMode = new CodeDescription(new CodeDescriptionPairList())
			{
				Code = "SEA"
			};

			portbase.SendersCustomsNo = "Sender001";
			portbase.TerminalFenexRegNo = "Reg001";
			portbase.TerminalDescription = "Description";
			portbase.CTO = CreateAddress("CTO");
			portbase.CurrentUser = CreateAddress("CurrentUser");
			portbase.IsFerryTeminal = true;
			portbase.ReceivingPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "NLRTM",
				Name = "Rotterdam"
			};
			portbase.CarrierBookingRef = "123456";

			var document1 = CreateDocument("001", "AAA");
			var document2 = CreateDocument("002", "BBB");

			var container1 = CreateContainer("CON1", "ref001");
			var container2 = CreateContainer("CON2", "ref002");

			var shipment1 = CreateShipment("S0000001", 10);
			var shipment2 = CreateShipment("S0000002", 20);

			container1.Shipments = new[]
			{
				shipment1
			};

			container2.Shipments = new[]
			{
				shipment2
			};

			document1.Containers = new[]
			{
				container1
			};

			document2.Containers = new[]
			{
				container2
			};

			portbase.Documents = new[]
			{
				document1,
				document2
			};

			return portbase;
		}

		PortbaseDocument CreateDocument(string id, string referenceNumber)
		{
			var document = new PortbaseDocument(id);

			document.ReferenceNumber = referenceNumber;
			document.EntryType = new CodeDescription(new CodeDescriptionPairList())
			{
				Code = "ATA"
			};

			return document;
		}

		PortbaseContainer CreateContainer(string number, string carrierBookingRef)
		{
			var container = new PortbaseContainer();

			container.Number = number;
			container.CarrierBookingRef = carrierBookingRef;
			container.GrossWeight = new Measurement()
			{
				Value = 88,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.IsNonOperativeReefer = false;

			return container;
		}

		PortbaseShipment CreateShipment(string number, int quantity)
		{
			var shipment = new PortbaseShipment();

			shipment.Number = number;
			shipment.Quantity = quantity;
			shipment.PackageType = new DummyCodeDescription
			{
				Code = "PKG",
				Description = "Package"
			};
			shipment.Weight = new Measurement()
			{
				Value = 66,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};

			return shipment;
		}

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001001</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

    </DataContext>
    <BookingConfirmationReference>123456</BookingConfirmationReference>
    <TransportMode>SEA</TransportMode>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>NLRTM</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Rotterdam</Value>
      </AddInfo>
      <AddInfo>
        <Key>Is_FerryTerminal</Key>
        <Value>Y</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00001001</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ArrivalCTOAddress</AddressType>
        <AdditionalAddressInformation>CTO additional info</AdditionalAddressInformation>
        <Address1>CTO address line 1</Address1>
        <Address2>CTO address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CTO city</City>
        <CompanyName>CTO</CompanyName>
        <Contact>CTO contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CTO email</Email>
        <Fax>CTO fax</Fax>
        <GovRegNum>CTO tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CTO phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CTO postco</Postcode>
        <State>CTO state</State>
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
        <AdditionalAddressInformation>CurrentUser additional info</AdditionalAddressInformation>
        <Address1>CurrentUser address line 1</Address1>
        <Address2>CurrentUser address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CurrentUser city</City>
        <CompanyName>CurrentUser</CompanyName>
        <Contact>CurrentUser contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CurrentUser email</Email>
        <Fax>CurrentUser fax</Fax>
        <GovRegNum>CurrentUser tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CurrentUser phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CurrentUse</Postcode>
        <State>CurrentUser state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <AddInfoCollection>
          <AddInfo>
            <Key>ReferenceNumber</Key>
            <Value>AAA</Value>
          </AddInfo>
          <AddInfo>
            <Key>EntryType_Code</Key>
            <Value>ATA</Value>
          </AddInfo>
        </AddInfoCollection>

        <ContainerCollection>
          <Container>
            <ContainerNumber>CON1</ContainerNumber>
            <GrossWeight>88</GrossWeight>
            <Link>1</Link>
            <NonOperatingReefer>false</NonOperatingReefer>
            <WeightUnit>KG</WeightUnit>

            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <PackQty>10</PackQty>
                <PackType Description=""Package"">PKG</PackType>
                <ReferenceNumber>S0000001</ReferenceNumber>
                <Weight>66</Weight>
              </PackingLine>
            </PackingLineCollection>
          </Container>
        </ContainerCollection>
      </SubShipment>
      <SubShipment>
        <AddInfoCollection>
          <AddInfo>
            <Key>ReferenceNumber</Key>
            <Value>BBB</Value>
          </AddInfo>
          <AddInfo>
            <Key>EntryType_Code</Key>
            <Value>ATA</Value>
          </AddInfo>
        </AddInfoCollection>

        <ContainerCollection>
          <Container>
            <ContainerNumber>CON2</ContainerNumber>
            <GrossWeight>88</GrossWeight>
            <Link>1</Link>
            <NonOperatingReefer>false</NonOperatingReefer>
            <WeightUnit>KG</WeightUnit>

            <PackingLineCollection>
              <PackingLine>
                <ContainerLink>1</ContainerLink>
                <PackQty>20</PackQty>
                <PackType Description=""Package"">PKG</PackType>
                <ReferenceNumber>S0000002</ReferenceNumber>
                <Weight>66</Weight>
              </PackingLine>
            </PackingLineCollection>
          </Container>
        </ContainerCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";

		protected override void SetUp()
		{
			base.SetUp();
			context = new CommonContext(Factory);
		}

		CommonContext context;
	}
}
