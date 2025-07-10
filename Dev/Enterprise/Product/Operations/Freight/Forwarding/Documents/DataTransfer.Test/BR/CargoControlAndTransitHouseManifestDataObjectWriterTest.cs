using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.BR;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.BR.Testing
{
	sealed class CargoControlAndTransitHouseManifestDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var cctHouseManifest =
				new CargoControlAndTransitHouseManifest("C0000001", "ForwardingConsol");

			cctHouseManifest.SendingParty = CreateAddressWithCNPJ(nameof(cctHouseManifest.SendingParty), "13.339.532/0001-09");
			cctHouseManifest.ReceivingAgent = CreateAddressWithCNPJ(nameof(cctHouseManifest.ReceivingAgent), "23.339.532/0001-09");

			cctHouseManifest.AirportOfDeparture = new Unloco(Context.Factory, Context.Unlocos, Context.Countries)
			{
				Code = "AUSYD"
			};

			cctHouseManifest.PortOfOrigin = new Unloco(Context.Factory, Context.Unlocos, Context.Countries)
			{
				Code = "AUSYD"
			};

			cctHouseManifest.AirportOfDestination = new Unloco(Context.Factory, Context.Unlocos, Context.Countries)
			{
				Code = "BRSAO"
			};

			cctHouseManifest.PortOfFirstArrival = new DummyUnloco
			{
				Code = "BRSAO",
				Name = "Sao Paulo",
				IATACode = "SAO"
			};

			cctHouseManifest.ConsolNumber = "C0000001";
			cctHouseManifest.Mawb = "S0000001";
			cctHouseManifest.Packs = 3;

			cctHouseManifest.Weight = new DummyMeasurement
			{
				Value = 1250,
				Unit = new DummyCodeDescription
				{
					Code = "K",
					Description = "Kilogram"
				}
			};

			var shipments = new List<CargoControlAndTransitDetail>();
			var shipment = new CargoControlAndTransitDetail("S0000001", "ForwardingShipment", "CCTHouseManifest");
			shipment.EventDateTime = new ZDateTime(2019, 11, 20, 12, 33, 42);
			shipment.Origin = new Unloco(Context.Factory, Context.Unlocos, Context.Countries)
			{
				Code = "AUSYD"
			};
			shipment.Destination = new Unloco(Context.Factory, Context.Unlocos, Context.Countries)
			{
				Code = "BRSAO"
			};
			shipment.GoodsDescription = "Goods description test";
			shipment.Hawb = "S0000001";
			shipment.Weight = new Measurement()
			{
				Value = 1250,
				Unit = new DummyCodeDescription()
				{
					Code = "K",
					Description = "Kilogram"
				}
			};
			shipment.ShipmentNumber = "S0000001";
			shipment.Packs = 3;

			shipments.Add(shipment);
			cctHouseManifest.Shipments = shipments;

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CargoControlAndTransitHouseManifestDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(cctHouseManifest);

			AssertUXml(dataObject, expectedXml);
		}

		Address CreateAddressWithCNPJ(string organizationType, string taxNumber)
		{
			var address = CreateAddress(organizationType);
			address.TaxNumber = taxNumber;
			address.TaxNumberType = new DummyCodeDescription
			{
				Code = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ,
				Description = "CNPJ Cadastro Nacional da Pessoa Jurídica"
			};

			return address;
		}

		#region Expected Xml

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>ForwardingConsol</Key>
        <Type>C0000001</Type>
      </DataSource>

    </DataContext>

    <PortOfDestination Name=""Sao Paulo"">SAO</PortOfDestination>
    <PortOfFirstArrival Name=""Sao Paulo"">BRSAO</PortOfFirstArrival>
    <PortOfOrigin Name=""Sydney"">SYD</PortOfOrigin>
    <TotalNoOfPacks>3</TotalNoOfPacks>
    <TotalWeight>1250</TotalWeight>
    <TotalWeightUnit Description=""Kilogram"">K</TotalWeightUnit>
    <WayBillNumber>S0000001</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>BRSAO</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C0000001</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
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
        <GovRegNum>13.339.532/0001-09</GovRegNum>
        <GovRegNumType Description=""CNPJ Cadastro Nacional da Pessoa Ju"">CJN</GovRegNumType>
        <Phone>SendingParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SendingPar</Postcode>
        <State>SendingParty state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ReceivingForwarderAddress</AddressType>
        <AdditionalAddressInformation>ReceivingAgent additional info</AdditionalAddressInformation>
        <Address1>ReceivingAgent address line 1</Address1>
        <Address2>ReceivingAgent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ReceivingAgent city</City>
        <CompanyName>ReceivingAgent</CompanyName>
        <Contact>ReceivingAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ReceivingAgent email</Email>
        <Fax>ReceivingAgent fax</Fax>
        <GovRegNum>23.339.532/0001-09</GovRegNum>
        <GovRegNumType Description=""CNPJ Cadastro Nacional da Pessoa Ju"">CJN</GovRegNumType>
        <Phone>ReceivingAgent phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ReceivingA</Postcode>
        <State>ReceivingAgent state</State>

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
        <DataContext>
          <DataSource>
            <Key>ForwardingShipment</Key>
            <Type>S0000001</Type>
          </DataSource>

        </DataContext>

        <GoodsDescription>Goods description test</GoodsDescription>
        <OuterPacks>3</OuterPacks>
        <PortOfDestination Name=""Sao Paulo"">SAO</PortOfDestination>
        <PortOfOrigin Name=""Sydney"">SYD</PortOfOrigin>
        <TotalWeight>1250</TotalWeight>
        <TotalWeightUnit Description=""Kilogram"">K</TotalWeightUnit>
        <WayBillNumber>S0000001</WayBillNumber>

        <DateCollection>
          <Date>
            <Type>Arrival</Type>
            <Value>2019-11-20T12:33:42</Value>
          </Date>
        </DateCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";
		#endregion
	}
}
