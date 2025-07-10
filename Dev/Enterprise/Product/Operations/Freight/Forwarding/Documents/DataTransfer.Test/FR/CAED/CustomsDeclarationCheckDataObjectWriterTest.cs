using System.Collections.Generic;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class CustomsDeclarationCheckDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var context = new CommonContext(Factory);
			var customsDeclarationCheck = new CustomsDeclarationCheck("ForwardingShipment", "S00001001");

			customsDeclarationCheck.CurrentUser = CreateAddress(nameof(customsDeclarationCheck.CurrentUser));
			customsDeclarationCheck.SendingForwarderAddress = CreateAddress(nameof(customsDeclarationCheck.SendingForwarderAddress));
			customsDeclarationCheck.ReceivingForwarderAddress = CreateAddress(nameof(customsDeclarationCheck.ReceivingForwarderAddress));
			customsDeclarationCheck.ExportBrokerAddress = CreateAddress(nameof(customsDeclarationCheck.ExportBrokerAddress));
			customsDeclarationCheck.ImportBrokerAddress = CreateAddress(nameof(customsDeclarationCheck.ImportBrokerAddress));
			customsDeclarationCheck.DepartureCTOAddress = CreateAddress(nameof(customsDeclarationCheck.DepartureCTOAddress));
			customsDeclarationCheck.ArrivalCTOAddress = CreateAddress(nameof(customsDeclarationCheck.ArrivalCTOAddress));

			customsDeclarationCheck.SendingPartySONCode = new RegistrationNumber()
			{
				Value = "SON123",
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.France))
				{
					Code = OrgCusCode.FranceCodeTypes.SON
				},
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.France
				}
			};
			customsDeclarationCheck.SendingPartyCI5Code = new RegistrationNumber()
			{
				Value = "CI5123",
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.France))
				{
					Code = OrgCusCode.FranceCodeTypes.CI5
				},
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.France
				}
			};
			customsDeclarationCheck.CTOSONCode = new RegistrationNumber()
			{
				Value = "SON123",
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.France))
				{
					Code = OrgCusCode.FranceCodeTypes.SON
				},
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.France
				}
			};
			customsDeclarationCheck.CTOCI5Code = new RegistrationNumber()
			{
				Value = "CI5123",
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.France))
				{
					Code = OrgCusCode.FranceCodeTypes.CI5
				},
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.France
				}
			};
			customsDeclarationCheck.ShipmentNumber = "ShipmentNumber";
			customsDeclarationCheck.TotalNumberOfPacks = 123;
			customsDeclarationCheck.CommonAccessRef = "CommonAccessRef";
			customsDeclarationCheck.AppliesToAllPacks = true;
			customsDeclarationCheck.PackageType = new DummyCodeDescription
			{
				Code = "PLT",
				Description = "Pallet"
			};
			customsDeclarationCheck.CustomsOfficeCode = new CodeDescription(new CustomsOfficeCodes())
			{
				Code = "FR005130"
			};
			customsDeclarationCheck.DeclarationType = "DeclarationType";
			customsDeclarationCheck.DeclarationFileNumber = "DeclarationFileNumber";
			customsDeclarationCheck.DeclarantsSIRETNumber = new RegistrationNumber()
			{
				Value = "DeclarantsSIRETNumber",
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.France))
				{
					Code = OrgCusCode.FranceCodeTypes.Siret
				},
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.France
				}
			};

			var containers = new List<Container>();
			var container = new Container();
			container.Number = "TBNN1234562";
			container.GrossWeight = new Measurement()
			{
				Value = 0.0,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = "KG",
				}
			};
			container.ImportDepotCustomsReference = "ImportDepotCustomsReference";
			container.ExportDepotCustomsReference = "ExportDepotCustomsReference";

			containers.Add(container);
			customsDeclarationCheck.Containers = containers;

			customsDeclarationCheck.Port = Unloco.Create(Context, RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Paris", "FR"));
			customsDeclarationCheck.PortDuesAmount = 123.456;

			customsDeclarationCheck.PortDuesCurrency = new DummyCodeDescription
			{
				Code = "EUR",
				Description = "Euro"
			};

			customsDeclarationCheck.ContainerMode = new DummyCodeDescription
			{
				Code = "FCL",
				Description = "Full Container Load"
			};
			customsDeclarationCheck.ShipmentType = new DummyCodeDescription
			{
				Code = "STD",
				Description = "Standard House"
			};
			customsDeclarationCheck.PortOfDestination = Unloco.Create(Context, RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Sydney", "AU"));
			customsDeclarationCheck.PortOfOrigin = Unloco.Create(Context, RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Paris", "FR"));

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CustomsDeclarationCheckDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(customsDeclarationCheck);

			AssertUXml(dataObject, expectedXml);
		}

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>S00001001</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

    </DataContext>

    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <PortOfDestination Name=""Sydney"">AUSYD</PortOfDestination>
    <PortOfOrigin Name=""Paris"">FRPAR</PortOfOrigin>
    <ShipmentType Description=""Standard House"">STD</ShipmentType>
    <TotalNoOfPacks>123</TotalNoOfPacks>
    <TotalNoOfPacksPackageType Description=""Pallet"">PLT</TotalNoOfPacksPackageType>

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
        <Key>DeclarationType</Key>
        <Value>DeclarationType</Value>
      </AddInfo>
      <AddInfo>
        <Key>CustomsOfficeCode</Key>
        <Value>FR005130</Value>
      </AddInfo>
      <AddInfo>
        <Key>DeclarationFileNumber</Key>
        <Value>DeclarationFileNumber</Value>
      </AddInfo>
      <AddInfo>
        <Key>CommonAccessRef</Key>
        <Value>CommonAccessRef</Value>
      </AddInfo>
      <AddInfo>
        <Key>AppliesToAllPacks</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortDuesAmount</Key>
        <Value>123.456</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortDuesCurrency</Key>
        <Value>EUR</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>ShipmentNumber</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <ContainerCollection>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>0</ContainerCount>
        <ContainerNumber>TBNN1234562</ContainerNumber>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference>ExportDepotCustomsRe</ExportDepotCustomsReference>
        <GrossWeight>0</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference>ImportDepotCustomsRe</ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <NonOperatingReefer>false</NonOperatingReefer>
        <Seal></Seal>
        <SecondSeal></SecondSeal>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>

    <OrganizationAddressCollection>
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
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>CI5123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SON123</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AdditionalAddressInformation>SendingForwarderAddress additional info</AdditionalAddressInformation>
        <Address1>SendingForwarderAddress address line 1</Address1>
        <Address2>SendingForwarderAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SendingForwarderAddress city</City>
        <CompanyName>SendingForwarderAddress</CompanyName>
        <Contact>SendingForwarderAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingForwarderAddress email</Email>
        <Fax>SendingForwarderAddr</Fax>
        <GovRegNum>SendingForwarderAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingForwarderAddr</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SendingFor</Postcode>
        <State>SendingForwarderAddress s</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ReceivingForwarderAddress</AddressType>
        <AdditionalAddressInformation>ReceivingForwarderAddress additional info</AdditionalAddressInformation>
        <Address1>ReceivingForwarderAddress address line 1</Address1>
        <Address2>ReceivingForwarderAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ReceivingForwarderAddress city</City>
        <CompanyName>ReceivingForwarderAddress</CompanyName>
        <Contact>ReceivingForwarderAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ReceivingForwarderAddress email</Email>
        <Fax>ReceivingForwarderAd</Fax>
        <GovRegNum>ReceivingForwarderAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ReceivingForwarderAd</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ReceivingF</Postcode>
        <State>ReceivingForwarderAddress</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ExportBroker</AddressType>
        <AdditionalAddressInformation>ExportBrokerAddress additional info</AdditionalAddressInformation>
        <Address1>ExportBrokerAddress address line 1</Address1>
        <Address2>ExportBrokerAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ExportBrokerAddress city</City>
        <CompanyName>ExportBrokerAddress</CompanyName>
        <Contact>ExportBrokerAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ExportBrokerAddress email</Email>
        <Fax>ExportBrokerAddress </Fax>
        <GovRegNum>ExportBrokerAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ExportBrokerAddress </Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ExportBrok</Postcode>
        <State>ExportBrokerAddress state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""SIRET Establishment Identifier"">SRT</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>DeclarantsSIRETNumber</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ImportBroker</AddressType>
        <AdditionalAddressInformation>ExportBrokerAddress additional info</AdditionalAddressInformation>
        <Address1>ExportBrokerAddress address line 1</Address1>
        <Address2>ExportBrokerAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ExportBrokerAddress city</City>
        <CompanyName>ExportBrokerAddress</CompanyName>
        <Contact>ExportBrokerAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ExportBrokerAddress email</Email>
        <Fax>ExportBrokerAddress </Fax>
        <GovRegNum>ExportBrokerAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ExportBrokerAddress </Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ExportBrok</Postcode>
        <State>ExportBrokerAddress state</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCTOAddress</AddressType>
        <AdditionalAddressInformation>DepartureCTOAddress additional info</AdditionalAddressInformation>
        <Address1>DepartureCTOAddress address line 1</Address1>
        <Address2>DepartureCTOAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>DepartureCTOAddress city</City>
        <CompanyName>DepartureCTOAddress</CompanyName>
        <Contact>DepartureCTOAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>DepartureCTOAddress email</Email>
        <Fax>DepartureCTOAddress </Fax>
        <GovRegNum>DepartureCTOAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>DepartureCTOAddress </Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>DepartureC</Postcode>
        <State>DepartureCTOAddress state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>CI5123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SON123</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ArrivalCTOAddress</AddressType>
        <AdditionalAddressInformation>ArrivalCTOAddress additional info</AdditionalAddressInformation>
        <Address1>ArrivalCTOAddress address line 1</Address1>
        <Address2>ArrivalCTOAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ArrivalCTOAddress city</City>
        <CompanyName>ArrivalCTOAddress</CompanyName>
        <Contact>ArrivalCTOAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ArrivalCTOAddress email</Email>
        <Fax>ArrivalCTOAddress fa</Fax>
        <GovRegNum>ArrivalCTOAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ArrivalCTOAddress ph</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ArrivalCTO</Postcode>
        <State>ArrivalCTOAddress state</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";
	}
}
