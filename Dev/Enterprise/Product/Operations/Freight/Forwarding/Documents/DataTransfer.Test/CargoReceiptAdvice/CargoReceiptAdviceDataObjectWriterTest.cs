using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.UniversalDataBuss.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class CargoReceiptAdviceDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var context = new CommonContext(Factory.GetCachedReadOnlyFactory());
			var cargoReceiptAdvice = new CargoReceiptAdvice("ForwardingShipment", "S00001001");

			cargoReceiptAdvice.HIRReference = "H001";
			cargoReceiptAdvice.MarksAndNumbers = "MarksAndNumbers";
			cargoReceiptAdvice.BookingParty = CreateAddress(nameof(cargoReceiptAdvice.BookingParty));
			cargoReceiptAdvice.DepartureCFSAddress = CreateAddress(nameof(cargoReceiptAdvice.DepartureCFSAddress));

			cargoReceiptAdvice.InterimReceipt = "interimReceipt123";
			cargoReceiptAdvice.InterimReceiptDate = new ZDateTime(2022, 3, 8, 10, 58, 51);

			cargoReceiptAdvice.PackingLines = new[]
			{
				new PackingLine("zzz", Factory)
				{
					Quantity = 7,
					Length = new Measurement
					{
						Value = 1,
						Unit = new DummyCodeDescription
						{
							Code = Core.Constants.Length.Metres
						}
					},
					Width = new Measurement
					{
						Value = 2,
						Unit = new DummyCodeDescription
						{
							Code = Core.Constants.Length.Metres
						}
					},
					Height = new Measurement
					{
						Value = 3,
						Unit = new DummyCodeDescription
						{
							Code = Core.Constants.Length.Metres
						}
					},
					Weight = new Measurement
					{
						Value = 5,
						Unit = new DummyCodeDescription
						{
							Code = Core.Constants.Weight.Kilograms
						}
					}
				}
			};

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CargoReceiptAdviceDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(cargoReceiptAdvice);

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

      <Workflow>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Department"">BRN</EventDepartment>
        <EventUser Name=""CargoWise Support"">E</EventUser>
      </Workflow>
    </DataContext>

    <InterimReceiptNumber>interimReceipt123</InterimReceiptNumber>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""eHub Interchange Reference"">HIR</Type>
        <ReferenceNumber>H001</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <DateCollection>
      <Date>
        <Type>Received</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2022-03-08T10:58:51</Value>
      </Date>
    </DateCollection>

    <NoteCollection>
      <Note>
        <Description>Marks &amp; Numbers</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>MarksAndNumbers</NoteText>
      </Note>
    </NoteCollection>

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
      <OrganizationAddress>
        <AddressType>DepartureCFSAddress</AddressType>
        <AdditionalAddressInformation>DepartureCFSAddress additional info</AdditionalAddressInformation>
        <Address1>DepartureCFSAddress address line 1</Address1>
        <Address2>DepartureCFSAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>DepartureCFSAddress city</City>
        <CompanyName>DepartureCFSAddress</CompanyName>
        <Contact>DepartureCFSAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>DepartureCFSAddress email</Email>
        <Fax>DepartureCFSAddress </Fax>
        <GovRegNum>DepartureCFSAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>DepartureCFSAddress </Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>DepartureC</Postcode>
        <State>DepartureCFSAddress state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection>
      <PackingLine>
        <ContainerNumber></ContainerNumber>
        <DetailedDescription></DetailedDescription>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription></GoodsDescription>
        <Height>3</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <Length>1</Length>
        <LengthUnit>M</LengthUnit>
        <MarksAndNos></MarksAndNos>
        <OutturnComment></OutturnComment>
        <PackingLineID></PackingLineID>
        <PackQty>7</PackQty>
        <ReferenceNumber></ReferenceNumber>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Weight>5</Weight>
        <WeightUnit>KG</WeightUnit>
        <Width>2</Width>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>";
	}
}
