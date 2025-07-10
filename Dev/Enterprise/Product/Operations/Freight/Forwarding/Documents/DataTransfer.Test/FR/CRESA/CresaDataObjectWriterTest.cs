using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing.FR
{
	sealed class CresaDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var cresa = PrepareTestData();

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CresaDataObjectWriter(manager);

			var dataObject = writer.GetDataObject(cresa);

			AssertUXml(dataObject, expectedXml);
		}

		Cresa PrepareTestData()
		{
			var cresa = new Cresa("ForwardingShipment", "S00001266");

			cresa.SendingParty = CreateAddress(nameof(cresa.SendingParty));
			cresa.SendingPartyCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "SendingPartyCI5Code");
			cresa.SendingPartySON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "SendingPartySONCode");
			cresa.SendingPartySOA = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOA, "SendingPartySOACode");
			cresa.SendingPartySOW = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOW, "SendingPartySOWCode");

			cresa.Transporter = CreateAddress(nameof(cresa.Transporter));
			cresa.TransporterCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "TransporterCI5Code");
			cresa.TransporterSON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "TransporterSONCode");

			cresa.Agent = CreateAddress(nameof(cresa.Agent));
			cresa.AgentCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "AgentCI5Code");
			cresa.AgentSOA = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOA, "AgentSOACode");

			cresa.Buyer = CreateAddress(nameof(cresa.Buyer));
			cresa.BuyerCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "BuyerCI5Code");
			cresa.BuyerSON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "BuyerSONCode");

			cresa.Supplier = CreateAddress(nameof(cresa.Supplier));
			cresa.SupplierCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "SupplierCI5Code");
			cresa.SupplierSON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "SupplierSONCode");

			cresa.SendingForwarder = CreateAddress(nameof(cresa.SendingForwarder));
			cresa.SendingForwarderCI5 = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "SendingForwarderCI5Code");
			cresa.SendingForwarderSON = CreateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "SendingForwarderSONCode");

			cresa.TransporterID = "TRANSP";
			cresa.TransportMode = "RTE";
			cresa.PortOfTranshipment = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "INKRI",
				Name = "Krishnapatnam"
			};

			cresa.PortOfArrival = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "INKRI",
				Name = "Krishnapatnam"
			};

			cresa.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRMAR",
				Name = "Marseille"
			};

			cresa.ETA = new ZDateTime(2020, 7, 28, 14, 15, 0);

			cresa.ShipmentType = new DummyCodeDescription()
			{
				Code = Core.Constants.ShipmentTypes.StandardHouse,
				Description = Core.Constants.ShipmentTypeDescriptions.StandardHouse
			};

			cresa.ContainerMode = new DummyCodeDescription()
			{
				Code = Core.Constants.ContainerModes.LCL,
				Description = "Less Container Load"
			};

			cresa.PortServiceCodeReference = "SERVICEREF";
			cresa.PortArea = "PORTAREA";
			cresa.PortLocation = "PORTLOC";

			cresa.CarrierBookingReference = "CARRIERBOOKINGREF";
			cresa.AMQReference = "AMQ000098";
			cresa.ECVReference = "ECV000011";
			cresa.EntryNumber = "DEC000098";

			cresa.ShipmentNumber = "S00001266";
			cresa.CommodityReference = "COM000098";
			cresa.GoodsInDateTime = new ZDateTime(2020, 7, 28, 14, 15, 0);
			cresa.GoodsSealed = false;
			cresa.TotalPackCount = 765;
			cresa.PackType = new DummyCodeDescription
			{
				Code = "PKG",
				Description = "Package"
			};
			cresa.TotalWeight = new Measurement()
			{
				Value = 52240.77,
				Unit = new DummyCodeDescription
				{
					Code = "KG",
					Description = "Kilograms"
				}
			};
			cresa.TotalVolume = new Measurement()
			{
				Value = 132,
				Unit = new DummyCodeDescription
				{
					Code = "M3",
					Description = "Cubic Meters"
				}
			};
			cresa.GoodsDescription = "Goods Description";
			cresa.GoodsReceiptNotes = "Goods Receipt Notes";

			cresa.CargoReceiptDate = new ZDateTime(2020, 12, 12, 6, 6, 0);

			var packingline = CreatePackingLine();

			cresa.PackingLines = new[]
			{
				packingline
			};

			return cresa;
		}

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>S00001266</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

    </DataContext>

    <BookingConfirmationReference>CARRIERBOOKINGREF</BookingConfirmationReference>
    <ContainerMode Description=""Less Container Load"">LCL</ContainerMode>
    <GoodsDescription>Goods Description</GoodsDescription>
    <PortFirstForeign Name=""Krishnapatnam"">INKRI</PortFirstForeign>
    <PortOfDestination Name=""Krishnapatnam"">INKRI</PortOfDestination>
    <PortOfOrigin Name=""Marseille"">FRMAR</PortOfOrigin>
    <ShipmentType Description=""Standard House"">STD</ShipmentType>
    <TotalNoOfPacks>765</TotalNoOfPacks>
    <TotalNoOfPacksPackageType Description=""Package"">PKG</TotalNoOfPacksPackageType>
    <TotalVolume>132</TotalVolume>
    <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
    <TotalWeight>52240.77</TotalWeight>
    <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>FRMAR</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Marseille</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>1.0.0</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortLocation</Key>
        <Value>PORTLOC</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortArea</Key>
        <Value>PORTAREA</Value>
      </AddInfo>
      <AddInfo>
        <Key>TransportMode</Key>
        <Value>RTE</Value>
      </AddInfo>
      <AddInfo>
        <Key>TransporterID</Key>
        <Value>TRANSP</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortServiceReference</Key>
        <Value>SERVICEREF</Value>
      </AddInfo>
      <AddInfo>
        <Key>EntryNumber</Key>
        <Value>DEC000098</Value>
      </AddInfo>
      <AddInfo>
        <Key>AMQReference</Key>
        <Value>ECV000011</Value>
      </AddInfo>
      <AddInfo>
        <Key>CommodityReference</Key>
        <Value>COM000098</Value>
      </AddInfo>
      <AddInfo>
        <Key>GoodsSealed</Key>
        <Value>N</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>S00001266</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <Value>2020-07-28T14:15:00</Value>
      </Date>
      <Date>
        <Type>Received</Type>
        <Value>2020-07-28T14:15:00</Value>
      </Date>
      <Date>
        <Type>CargoReceiptDate</Type>
        <Value>2020-12-12T06:06:00</Value>
      </Date>
    </DateCollection>

    <NoteCollection>
      <Note>
        <Description>Goods Receipt Notes</Description>
        <NoteText>Goods Receipt Notes</NoteText>
      </Note>
    </NoteCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Supplier additional info</AdditionalAddressInformation>
        <Address1>Supplier address line 1</Address1>
        <Address2>Supplier address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Supplier city</City>
        <CompanyName>Supplier</CompanyName>
        <Contact>Supplier contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Supplier email</Email>
        <Fax>Supplier fax</Fax>
        <GovRegNum>Supplier tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Supplier phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Supplier p</Postcode>
        <State>Supplier state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SupplierCI5Code</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SupplierSONCode</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Buyer additional info</AdditionalAddressInformation>
        <Address1>Buyer address line 1</Address1>
        <Address2>Buyer address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Buyer city</City>
        <CompanyName>Buyer</CompanyName>
        <Contact>Buyer contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Buyer email</Email>
        <Fax>Buyer fax</Fax>
        <GovRegNum>Buyer tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Buyer phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Buyer post</Postcode>
        <State>Buyer state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>BuyerCI5Code</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>BuyerSONCode</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>PickupLocalCartage</AddressType>
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
            <Value>TransporterCI5Code</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>TransporterSONCode</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>PickupAgent</AddressType>
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
            <Value>AgentCI5Code</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Agent C"">SOA</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>AgentSOACode</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
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
            <Value>SendingPartyCI5Code</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SendingPartySONCode</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Agent C"">SOA</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SendingPartySOACode</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Warehou"">SOW</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SendingPartySOWCode</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
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
            <Value>SendingForwarderCI5Code</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SendingForwarderSONCode</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection>
      <PackingLine>
        <ContainerNumber></ContainerNumber>
        <DetailedDescription>Goods Description</DetailedDescription>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription>Goods Description</GoodsDescription>
        <Height>56</Height>
        <ImportReferenceNumber>import reference number</ImportReferenceNumber>
        <Length>58</Length>
        <LengthUnit Description=""Centimeters"">CM</LengthUnit>
        <MarksAndNos>marks &amp; nums</MarksAndNos>
        <PackingLineID>1</PackingLineID>
        <PackQty>225</PackQty>
        <PackType Description=""Package"">PKG</PackType>
        <ReferenceNumber>reference number</ReferenceNumber>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>33</Volume>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <Weight>7896.67</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <Width>57</Width>
      </PackingLine>
    </PackingLineCollection>
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
