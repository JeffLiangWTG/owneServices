using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.US;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.US.Testing
{
	sealed class AcasDataObjectWriterTest : DataObjectWriterTest
	{
		[TestDate(2018, 6, 6)]
		public void TestPopulateDataObject_Original()
		{
			var acas = new AirCargoAdvanceScreening("ForwardingShipment", "zzz", "ACAS Shipment Report");
			PopulateAcas(acas, false);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new AcasDataObjectWriter(manager);

			var dataObject = writer.GetDataObject(acas);

			AssertUXml(dataObject, originalXml);
		}

		[TestDate(2018, 6, 6)]
		public void TestPopulateDataObject_Acknowledgement()
		{
			var acas = new AirCargoAdvanceScreening("ForwardingShipment", "zzz", "ACAS Shipment Report");
			PopulateAcas(acas, true);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new AcasDataObjectWriter(manager);

			var dataObject = writer.GetDataObject(acas);

			AssertUXml(dataObject, acknowledgementXml);
		}

		void PopulateAcas(AirCargoAdvanceScreening acas, bool sendAsAcknowledgement)
		{
			acas.HAWB = "HAWB123";
			acas.MAWB = "MAWB123";

			acas.ConsolNumber = "CONSOL";
			acas.SendersAcasCode = "123";
			acas.NotifyPartysAcasCode = "345";

			acas.GoodsDescription = "Dorcus antaeus";
			acas.FlightNumber = "A1234A";
			acas.ETA = ZDateTime.Today;

			acas.NumberOfPacks = 12;
			acas.Weight = new Measurement
			{
				Value = 100,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};

			acas.Departure = new DummyUnloco
			{
				Code = "AUSYD",
				Name = "Sydney",
				IATACode = "SYD"
			};

			acas.PortOfFirstArrivalIata = "ABC";

			acas.Arrival = new DummyUnloco
			{
				Code = "USLAX",
				Name = "Los Angeles",
				IATACode = "LAX"
			};

			acas.PortOfOriginIata = "DFG";

			acas.State = sendAsAcknowledgement ? AcasState.AcknowledgementRequired : AcasState.None;

			acas.ConsolType = new DummyCodeDescription()
			{
				Code = Core.Constants.AgentType.Agent,
				Description = Core.Constants.AgentTypeDescriptions.Agent
			};
			PopulateOrganizations(acas);
		}

		void PopulateOrganizations(AirCargoAdvanceScreening acas)
		{
			acas.Carrier = CreateAddress(nameof(acas.Carrier));
			acas.CTO = CreateAddress(nameof(acas.CTO));
			acas.Consignee = CreateAddress(nameof(acas.Consignee));
			acas.Shipper = CreateAddress(nameof(acas.Shipper));
			acas.NotifyParty = CreateAddress(nameof(acas.NotifyParty));
			acas.BookingParty = CreateAddress(nameof(acas.BookingParty));

			acas.NotifyPartyType = new CodeDescription(new PartyTypes())
			{
				Code = "AGT",
				Description = "Agent"
			};
		}

		#region Expected Xml

		const string originalXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>zzz</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

    </DataContext>
    <GoodsDescription>Dorcus antaeus</GoodsDescription>
    <OuterPacks>12</OuterPacks>
    <PortOfFirstArrival Name=""Los Angeles"">USLAX</PortOfFirstArrival>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <TotalNoOfPacks>12</TotalNoOfPacks>
    <TotalWeight>100</TotalWeight>
    <TotalWeightUnit>KG</TotalWeightUnit>
    <VoyageFlightNo>A1234A</VoyageFlightNo>
    <WayBillNumber>HAWB123</WayBillNumber>
    <AddInfoCollection>
      <AddInfo>
        <Key>MAWB</Key>
        <Value>MAWB123</Value>
      </AddInfo>
      <AddInfo>
        <Key>NotifyPartyType_Code</Key>
        <Value>AGT</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortOfOriginIata</Key>
        <Value>DFG</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortOfFirstArrivalIata</Key>
        <Value>ABC</Value>
      </AddInfo>
      <AddInfo>
        <Key>ConsolType_Code</Key>
        <Value>AGT</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>USLAX</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>CONSOL</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <Value>2018-06-06T00:00:00</Value>
      </Date>
    </DateCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
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
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Shipper additional info</AdditionalAddressInformation>
        <Address1>Shipper address line 1</Address1>
        <Address2>Shipper address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Shipper city</City>
        <CompanyName>Shipper</CompanyName>
        <Contact>Shipper contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Shipper email</Email>
        <Fax>Shipper fax</Fax>
        <GovRegNum>Shipper tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Shipper phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Shipper po</Postcode>
        <State>Shipper state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
        <Address1>Consignee address line 1</Address1>
        <Address2>Consignee address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Consignee city</City>
        <CompanyName>Consignee</CompanyName>
        <Contact>Consignee contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Consignee email</Email>
        <Fax>Consignee fax</Fax>
        <GovRegNum>Consignee tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Consignee phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Consignee </Postcode>
        <State>Consignee state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
        <Address1>NotifyParty address line 1</Address1>
        <Address2>NotifyParty address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NotifyParty city</City>
        <CompanyName>NotifyParty</CompanyName>
        <Contact>NotifyParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty email</Email>
        <Fax>NotifyParty fax</Fax>
        <GovRegNum>NotifyParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NotifyPart</Postcode>
        <State>NotifyParty state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>BookingParty additional info</AdditionalAddressInformation>
        <Address1>BookingParty address line 1</Address1>
        <Address2>BookingParty address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>BookingParty city</City>
        <CompanyName>BookingParty</CompanyName>
        <Contact>BookingParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>BookingParty email</Email>
        <Fax>BookingParty fax</Fax>
        <GovRegNum>BookingParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>BookingParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>BookingPar</Postcode>
        <State>BookingParty state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		const string acknowledgementXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>zzz</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

    </DataContext>

    <PortOfFirstArrival Name=""Los Angeles"">USLAX</PortOfFirstArrival>
    <WayBillNumber>HAWB123</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>MAWB</Key>
        <Value>MAWB123</Value>
      </AddInfo>
      <AddInfo>
        <Key>ConsolType_Code</Key>
        <Value>AGT</Value>
      </AddInfo>
      <AddInfo>
        <Key>AcknowledgementStatus</Key>
        <Value>Z</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>USLAX</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>CONSOL</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>BookingParty additional info</AdditionalAddressInformation>
        <Address1>BookingParty address line 1</Address1>
        <Address2>BookingParty address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>BookingParty city</City>
        <CompanyName>BookingParty</CompanyName>
        <Contact>BookingParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>BookingParty email</Email>
        <Fax>BookingParty fax</Fax>
        <GovRegNum>BookingParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>BookingParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>BookingPar</Postcode>
        <State>BookingParty state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";
		#endregion
	}
}
