using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	sealed class CresaWithoutUIMessageSenderForwardingTest : DocDataObjectWithoutUIMessageSenderTest
	{
		protected override IDocDataObjectWithoutUIMessageSender MessageSender => messageSender ?? (messageSender = new CresaWithoutUIMessageSender());
		IDocDataObjectWithoutUIMessageSender messageSender;

		protected override BusinessObject CreateBusinessObject() => CresaTestHelper.CreateShipment(Factory).PopulateRequiredOrgCodes();

		protected override ZString DataStoreName => ShipmentDocumentDataStoreNames.GoodsReceivedCRESA;

		public override bool DocumentHasMenuItem => true;

		protected override ZString MSNReference => "|DEP=Terminal|MST=Goods Received (CRESA)";

		protected override string SenderObjectFactoryRegistrationKey => ShipmentDocumentDataStoreNames.GoodsReceivedCRESA;

		protected override ZString ExpectedMessage => @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CRESA/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>SH0001000</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Goods Received (CRESA)</DocumentName>
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
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2021-01-01T00:00:00.000+00:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>

    <BookingConfirmationReference>SH0001000</BookingConfirmationReference>
    <ContainerMode>LCL</ContainerMode>
    <GoodsDescription>goods description</GoodsDescription>
    <PortFirstForeign Name=""Marseille"">FRMRS</PortFirstForeign>
    <PortOfDestination Name=""Melbourne"">AUMEL</PortOfDestination>
    <PortOfOrigin Name=""Marmande"">FRMAR</PortOfOrigin>
    <ShipmentType Description=""Standard House"">STD</ShipmentType>
    <TotalNoOfPacks>15</TotalNoOfPacks>
    <TotalNoOfPacksPackageType Description=""Pallet"">PLT</TotalNoOfPacksPackageType>
    <TotalVolume>4875</TotalVolume>
    <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
    <TotalWeight>1500</TotalWeight>
    <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>FRMAR</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Marmande</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>1.0.0</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortLocation</Key>
        <Value>ZZZ</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortArea</Key>
        <Value>001</Value>
      </AddInfo>
      <AddInfo>
        <Key>TransportMode</Key>
        <Value>RTE</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortServiceReference</Key>
        <Value>002</Value>
      </AddInfo>
      <AddInfo>
        <Key>EntryNumber</Key>
        <Value>SH0001000</Value>
      </AddInfo>
      <AddInfo>
        <Key>CommodityReference</Key>
        <Value>SH0001000</Value>
      </AddInfo>
      <AddInfo>
        <Key>GoodsSealed</Key>
        <Value>N</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>SH0001000</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <Value>2021-01-06T00:00:00</Value>
      </Date>
      <Date>
        <Type>Received</Type>
        <Value>2021-01-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>CargoReceiptDate</Type>
        <Value>2021-01-01T00:00:00</Value>
      </Date>
    </DateCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <NoteCollection>
      <Note>
        <Description>Goods Receipt Notes</Description>
        <NoteText>Delivery Order Receipt Note</NoteText>
      </Note>
    </NoteCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 200</Address1>
        <Address2>55 haha Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>wahaha Ave</City>
        <CompanyName>I'm consignor</CompanyName>
        <Contact></Contact>
        <Country Name=""China"">CN</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Bayin"">CNBSX</Port>
        <Postcode>10000</Postcode>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 223</Address1>
        <Address2>553 What Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Melbourne</City>
        <CompanyName>I'm consignee</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Melbourne"">AUMEL</Port>
        <Postcode>3023</Postcode>
        <State>VIC</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>PickupLocalCartage</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 283</Address1>
        <Address2>283 Drive</Address2>
        <AddressOverride>false</AddressOverride>
        <City>unknown city</City>
        <CompanyName>pickup agent org</CompanyName>
        <Contact></Contact>
        <Country Name=""France"">FR</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Nice"">FRNCE</Port>
        <Postcode>2836</Postcode>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ci5</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>PickupAgent</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 283</Address1>
        <Address2>283 Drive</Address2>
        <AddressOverride>false</AddressOverride>
        <City>unknown city</City>
        <CompanyName>pickup agent org</CompanyName>
        <Contact></Contact>
        <Country Name=""France"">FR</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Nice"">FRNCE</Port>
        <Postcode>2836</Postcode>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ci5</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 15</Address1>
        <Address2>5 Lost Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Marseille</City>
        <CompanyName>CONSPA</CompanyName>
        <Contact></Contact>
        <Country Name=""France"">FR</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Marmande"">FRMAR</Port>
        <Postcode>2000</Postcode>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ci5</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>10 HUTCHESON STREET</Address1>
        <Address2>ALBION  QLD</Address2>
        <AddressOverride>false</AddressOverride>
        <City></City>
        <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
        <Contact>CargoWise Support</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Brisbane"">AUBNE</Port>
        <Postcode>4010</Postcode>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ci5</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection>
      <PackingLine>
        <ContainerNumber></ContainerNumber>
        <DetailedDescription>Amrut Indian Peated Single Malt Detailed Description</DetailedDescription>
        <ExportReferenceNumber>AMRUT57%</ExportReferenceNumber>
        <GoodsDescription>Amrut Indian Peated Single Malt Detailed Description</GoodsDescription>
        <HarmonisedCode>WHISKY</HarmonisedCode>
        <Height>300</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <Length>1000</Length>
        <LengthUnit Description=""Centimeters"">CM</LengthUnit>
        <MarksAndNos>MarksAndNumbers1</MarksAndNos>
        <PackingLineID>FRCRESA0000001</PackingLineID>
        <PackQty>4</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber>AMR-57</ReferenceNumber>
        <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>1200</Volume>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <Weight>400</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <Width>1000</Width>

        <ClassificationCollection>
        </ClassificationCollection>

        <UNDGCollection>
        </UNDGCollection>
      </PackingLine>
      <PackingLine>
        <ContainerNumber></ContainerNumber>
        <DetailedDescription>Amrut Indian Chill Filter Single Malt Detailed Description</DetailedDescription>
        <ExportReferenceNumber>AMRUT43%</ExportReferenceNumber>
        <GoodsDescription>Amrut Indian Chill Filter Single Malt Detailed Description</GoodsDescription>
        <HarmonisedCode>WHISKY</HarmonisedCode>
        <Height>300</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <Length>1500</Length>
        <LengthUnit Description=""Centimeters"">CM</LengthUnit>
        <MarksAndNos>MarksAndNumbers2</MarksAndNos>
        <PackingLineID>FRCRESA0000002</PackingLineID>
        <PackQty>6</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber>AMR-43</ReferenceNumber>
        <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>2700</Volume>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <Weight>600</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <Width>1000</Width>

        <ClassificationCollection>
        </ClassificationCollection>

        <UNDGCollection>
        </UNDGCollection>
      </PackingLine>
      <PackingLine>
        <ContainerNumber></ContainerNumber>
        <DetailedDescription>ALCOHOLIC BEVERAGES 50%</DetailedDescription>
        <ExportReferenceNumber>AMRUT50%</ExportReferenceNumber>
        <GoodsDescription>ALCOHOLIC BEVERAGES 50%</GoodsDescription>
        <HarmonisedCode>WHISKY</HarmonisedCode>
        <Height>300</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <Length>1500</Length>
        <LengthUnit Description=""Centimeters"">CM</LengthUnit>
        <MarksAndNos>MarksAndNumbers3</MarksAndNos>
        <PackingLineID>FRCRESA0000003</PackingLineID>
        <PackQty>3</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber>AMR-50</ReferenceNumber>
        <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>675</Volume>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <Weight>300</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <Width>500</Width>

        <ClassificationCollection>
        </ClassificationCollection>

        <UNDGCollection>
        </UNDGCollection>
      </PackingLine>
      <PackingLine>
        <ContainerNumber></ContainerNumber>
        <DetailedDescription>goods description</DetailedDescription>
        <ExportReferenceNumber>AMRUT64%</ExportReferenceNumber>
        <GoodsDescription>goods description</GoodsDescription>
        <HarmonisedCode>WHISKY</HarmonisedCode>
        <Height>300</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <Length>1000</Length>
        <LengthUnit Description=""Centimeters"">CM</LengthUnit>
        <MarksAndNos>marks &amp; numbers</MarksAndNos>
        <PackingLineID>FRCRESA0000004</PackingLineID>
        <PackQty>2</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber>AMR-64</ReferenceNumber>
        <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>300</Volume>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <Weight>200</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <Width>500</Width>

        <ClassificationCollection>
        </ClassificationCollection>

        <UNDGCollection>
        </UNDGCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>";
	}
}
