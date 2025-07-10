using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(JobSupplierBookingDataContextManager))]
	internal class JobSupplierBookingDataContextManagerTest : ShipmentDataContextManagerTestCase<JobSupplierBookingDataContextManager, JobSupplierBooking>
	{
		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>JobSupplierBooking</Type>
          <Key>SBK001</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <GoodsDescription>Good Desc</GoodsDescription>
    <LoadMode>
      <Code>CFS</Code>
      <Description>Container Freight Station</Description>
    </LoadMode>
    <MarksAndNumbers>Marks &amp; Numbers</MarksAndNumbers>
    <PortOfDestination>
      <Code>SGSIN</Code>
      <Name>Singapore</Name>
    </PortOfDestination>
    <PortOfDischarge>
      <Code>CNCAN</Code>
      <Name>Guangzhou Baiyun International Apt</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfLoading>
    <PortOfOrigin>
      <Code>AUMEL</Code>
      <Name>Melbourne</Name>
    </PortOfOrigin>
    <ShipmentIncoTerm>
      <Code>EXW</Code>
      <Description>Ex Works</Description>
    </ShipmentIncoTerm>
    <ShipmentStatus>
      <Code>PLC</Code>
      <Description>Placed</Description>
    </ShipmentStatus>
    <TotalVolume>15</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Metres</Description>
    </TotalVolumeUnit>
    <TotalWeight>12</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <ContainerCollection Content=""Complete"">
      <Container>
        <ContainerCount>3</ContainerCount>
        <ContainerNumber></ContainerNumber>
        <ContainerType>
          <Code>20FR</Code>
          <Category>
            <Code>FLT</Code>
            <Description>Flat Rack</Description>
          </Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <Seal></Seal>
        <SecondSeal></SecondSeal>
        <ThirdSeal></ThirdSeal>
      </Container>
    </ContainerCollection>
    <CustomizedFieldCollection>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>DAT1</Key>
        <Value>2022-02-27T00:00:00</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>Decimal</DataType>
        <Key>DEC1</Key>
        <Value>12.34</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>Integer</DataType>
        <Key>INT1</Key>
        <Value>12</Value>
      </CustomizedField>
      <CustomizedField>
        <DataType>String</DataType>
        <Key>STR1</Key>
        <Value>ME TOO</Value>
      </CustomizedField>
    </CustomizedFieldCollection>
    <DateCollection>
      <Date>
        <Type>BookedOnDate</Type>
        <Value>2022-02-05T00:00:00</Value>
      </Date>
      <Date>
        <Type>CargoAvailableDate</Type>
        <Value>2022-02-03T00:00:00</Value>
      </Date>
    </DateCollection>
    <NoteCollection Content=""Partial"">
      <Note>
        <Description>Detailed Goods Description</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>Detailed Good Desc</NoteText>
        <NoteContext>
          <Code>AAA</Code>
          <Description>Module: A - All, Direction: A - All, Freight: A - All</Description>
        </NoteContext>
        <Visibility>
          <Code>PUB</Code>
          <Description>CLIENT-VISIBLE</Description>
        </Visibility>
      </Note>
    </NoteCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>#1</AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>BKSIN</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SupplierDocumentaryAddress</AddressType>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>#1</AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>SPSIN</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ControllingCustomer</AddressType>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>#1</AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>CCSZX</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>LocalCartageCFS</AddressType>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>#1</AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>LCCFS</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>#1</AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>CNSHA</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ArrivalCFSAddress</AddressType>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>#1</AddressShortCode>
        <City></City>
        <CompanyName></CompanyName>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>CCCcC</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>NOT</Code>
          <Description>Not Screened</Description>
        </ScreeningStatus>
        <State></State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <SubShipmentCollection>
      <SubShipment>

        <TotalNoOfPacksDecimal>7.1</TotalNoOfPacksDecimal>
        <TotalNoOfPacksPackageType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </TotalNoOfPacksPackageType>
        <Order>
          <OrderNumber>ORD0001</OrderNumber>
          <OrderNumberSplit>1</OrderNumberSplit>
          <OrderLineCollection>
            <OrderLine>
              <ExtendedLinePrice>2507.9544</ExtendedLinePrice>
              <LineNumber>1</LineNumber>
              <LineReference>ORL001</LineReference>
              <Product>
                <Code>ME100770267</Code>
                <Description>T Shirts</Description>
              </Product>
              <RequiredExWorks>2022-03-27T00:00:00</RequiredExWorks>
              <RequiredInStore>2022-04-27T00:00:00</RequiredInStore>
              <SubLineNumber>2</SubLineNumber>
              <UnitPriceRecommended>5.5732</UnitPriceRecommended>
            </OrderLine>
          </OrderLineCollection>
        </Order>
        <AddInfoCollection>
          <AddInfo>
            <Key>OrderContextKey</Key>
            <Value>ORD0001~1~Buyer</Value>
          </AddInfo>
        </AddInfoCollection>
        <DateCollection>
          <Date>
            <Type>ShipmentWindowStart</Type>
            <Value>2023-09-01T00:00:00</Value>
          </Date>
          <Date>
            <Type>ShipmentWindowEnd</Type>
            <Value>2023-09-03T00:00:00</Value>
          </Date>
        </DateCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>Manufacturer</AddressType>
            <Address1>#1</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>#1</AddressShortCode>
            <City></City>
            <CompanyName></CompanyName>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCode>MFAKL</OrganizationCode>
            <Phone></Phone>
            <Port>
              <Code></Code>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>NOT</Code>
              <Description>Not Screened</Description>
            </ScreeningStatus>
            <State></State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>BuyerDocumentaryAddress</AddressType>
            <Address1>#1</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>#1</AddressShortCode>
            <City></City>
            <CompanyName></CompanyName>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCode>Buyer</OrganizationCode>
            <Phone></Phone>
            <Port>
              <Code></Code>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>NOT</Code>
              <Description>Not Screened</Description>
            </ScreeningStatus>
            <State></State>
          </OrganizationAddress>
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <Commodity>
              <Code>GEN</Code>
              <Description>General</Description>
            </Commodity>
            <MarksAndNos>Line Marks&amp;Num</MarksAndNos>
            <PackingLineID>JSL001</PackingLineID>
            <PackQty>6</PackQty>
            <PackType>
              <Code>PLT</Code>
              <Description>Pallet</Description>
            </PackType>
            <ReceivedPacks>5</ReceivedPacks>
            <ReceivedPacksType>
              <Code>PLT</Code>
              <Description>Pallet</Description>
            </ReceivedPacksType>
            <ReceivedQuantity>6</ReceivedQuantity>
            <ReceivedQuantityType>
              <Code>PLT</Code>
              <Description>Pallet</Description>
            </ReceivedQuantityType>
            <ReceivedVolume>14</ReceivedVolume>
            <ReceivedVolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Metres</Description>
            </ReceivedVolumeUnit>
            <ReceivedWeight>11</ReceivedWeight>
            <ReceivedWeightUnit>
              <Code>KG</Code>
              <Description>Kilograms</Description>
            </ReceivedWeightUnit>
            <Volume>15</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Metres</Description>
            </VolumeUnit>
            <Weight>12</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilograms</Description>
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();
			JobSupplierBookingDataObjectHelperTest.BuildJobSupplierBookingForTest(Factory);
			Factory.SaveForTesting();
		}
	}
}
