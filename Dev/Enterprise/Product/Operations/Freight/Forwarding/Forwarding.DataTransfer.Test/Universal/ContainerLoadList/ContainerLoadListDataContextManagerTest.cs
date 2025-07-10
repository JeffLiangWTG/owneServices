using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ContainerLoadListDataContextManager))]
	class ContainerLoadListDataContextManagerTest : ShipmentDataContextManagerTestCase<ContainerLoadListDataContextManager, CYContainerLoadList>
	{
		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ContainerLoadList</Type>
          <Key>CLL0001</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <LoadMode>
      <Code>CY</Code>
      <Description>Full Container</Description>
    </LoadMode>
    <PortOfDischarge>
      <Code></Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code></Code>
    </PortOfLoading>
    <ShipmentStatus>
      <Code>INC</Code>
      <Description>INC</Description>
    </ShipmentStatus>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
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
        <OrganizationCode>BKT001</OrganizationCode>
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
    <ParentShipmentCollection>
      <ParentShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingConsol</Type>
              <Key>CON001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>


        <ContainerCollection>
          <Container>
            <ContainerCount>1</ContainerCount>
            <ContainerNumber>MWLF9771112</ContainerNumber>
            <ContainerType>
              <Code>20GP</Code>
              <Category>
                <Code>DRY</Code>
                <Description>Dry Storage</Description>
              </Category>
              <Description>Twenty foot general purpose</Description>
              <ISOCode>22G0</ISOCode>
            </ContainerType>
            <DeliveryMode>CY/CY</DeliveryMode>
            <FCL_LCL_AIR>
              <Code>LCL</Code>
              <Description>Less Container Load</Description>
            </FCL_LCL_AIR>
            <Link>1</Link>
          </Container>
        </ContainerCollection>
      </ParentShipment>
    </ParentShipmentCollection>
    <RelatedShipmentCollection>
      <RelatedShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>JobSupplierBooking</Type>
              <Key>SBK001</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>
      </RelatedShipment>
    </RelatedShipmentCollection>
    <SubShipmentCollection>
      <SubShipment>

        <TotalNoOfPacksDecimal>67</TotalNoOfPacksDecimal>
        <TotalNoOfPacksPackageType>
          <Code>PLT</Code>
          <Description>Pallet</Description>
        </TotalNoOfPacksPackageType>
        <AddInfoCollection>
          <AddInfo>
            <Key>JobSupplierBooking</Key>
            <Value>SBK001</Value>
          </AddInfo>
          <AddInfo>
            <Key>ShipmentID</Key>
            <Value>S0001001</Value>
          </AddInfo>
        </AddInfoCollection>
        <PackingLineCollection>
          <PackingLine>
            <Commodity>
              <Code>GEN</Code>
              <Description>General</Description>
            </Commodity>
            <ContainerLink>1</ContainerLink>
            <ContainerPackingOrder>78</ContainerPackingOrder>
            <HarmonisedCode>HC0001</HarmonisedCode>
            <PackingLineID>JSL001</PackingLineID>
            <PackQty>45</PackQty>
            <PackType>
              <Code>PCE</Code>
              <Description>Piece</Description>
            </PackType>
            <ReferenceNumber>RNXX87</ReferenceNumber>
            <Volume>12.3</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meters</Description>
            </VolumeUnit>
            <Weight>34.5</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilograms</Description>
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		protected override CYContainerLoadList GetNewBusinessObjectForTesting()
		{
			return (CYContainerLoadList)ContainerLoadListDataObjectHelper.BuildDataForTest(Factory, SupplierBookingLoadModeList.Codes.CY);
		}

		protected override void SetupDataForDataContextManagerTestCase()
		{
			base.SetupDataForDataContextManagerTestCase();
			ContainerLoadListDataObjectHelper.BuildDataForTest(Factory, SupplierBookingLoadModeList.Codes.CY);

			Factory.SaveForTesting();
		}

		public void TestGetShipmentDataObjectReaderShouldReturnNullOnCFS()
		{
			var cfsContainerLoadList = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			cfsContainerLoadList.LoadMode = new LoadMode { Code = ContainerLoadListHeaderLoadMode.ContainerFreightStation };
			Factory.SaveForTesting();
			var manager = new ContainerLoadListDataContextManager();
			var writer = ((IShipmentDataContextManagerInternal)manager).GetShipmentDataObjectReader(cfsContainerLoadList, new DummyLogger(), Factory);
			AssertNull(writer);
		}
	}
}
