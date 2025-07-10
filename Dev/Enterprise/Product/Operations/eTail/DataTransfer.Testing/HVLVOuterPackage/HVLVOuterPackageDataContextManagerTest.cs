using System.Linq;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HVLVOuterPackageDataContextManager))]
	class HVLVOuterPackageDataContextManagerTest : ShipmentDataContextManagerTestCase<HVLVOuterPackageDataContextManager, HVLVOuterPackage>
	{
		public void TestMatchingByDataContextKey()
		{
			var outerPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			outerPackage.HVO_PackageBarcode = "LTTPACKAGE01";

			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.HVLVOuterPackage, "LTTPACKAGE01");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalShipmentMessage(shipment);
			manager.Process(message);

			CombineAssertions(() =>
			{
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
Updated HVLVOuterPackage from UniversalShipment.
Successfully saved HVLVOuterPackage.
".Trim(), serviceTaskLog.ToString());

				var logNoteText = message.GetLogNoteText();
				AssertMultilineASCIIEquals("Message Log", @"
Successfully loaded matching HVLVOuterPackage.
Populating HVLVOuterPackage...
Updated HVLVOuterPackage from UniversalShipment.
Successfully saved HVLVOuterPackage.
".Trim(), logNoteText);

				var outerPackages = new BusinessObjectFactory().Load<HVLVOuterPackage>(new ZQuery());
				AssertEquals(1, outerPackages.Length);
			});
		}

		public void TestMatchingByDataContextKey_MultipleOuterPackagesWithSamePackageBarcode()
		{
			var newestOuterPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			newestOuterPackage.HVO_PackageBarcode = "LTTPACKAGE01";
			newestOuterPackage.HVO_Weight = 1;
			newestOuterPackage.HVO_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			var oldestOuterPackage = Factory.NewWithValidTestData<HVLVOuterPackage>();
			oldestOuterPackage.HVO_PackageBarcode = "LTTPACKAGE01";
			oldestOuterPackage.HVO_Weight = 2;
			oldestOuterPackage.HVO_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-2);

			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.DataContext = DataContextFactory.New();
			shipment.DataContext.AddDataTarget(DataContextType.HVLVOuterPackage, "LTTPACKAGE01");

			var packingLine = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Barcode = "LTTPACKAGE01",
				TareWeight = 10,
			};

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine> { packingLine });

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalShipmentMessage(shipment);
			manager.Process(message);

			CombineAssertions(() =>
			{
				newestOuterPackage = new BusinessObjectFactory().Load<HVLVOuterPackage>(new ZQuery(HVLVOuterPackageSchema.PK, newestOuterPackage.PK)).Single();
				oldestOuterPackage = new BusinessObjectFactory().Load<HVLVOuterPackage>(new ZQuery(HVLVOuterPackageSchema.PK, oldestOuterPackage.PK)).Single();

				AssertEquals("The newest outer package should have its weight updated", (ZDecimal)10, newestOuterPackage.HVO_Weight);
				AssertEquals("The oldest outer package should have the same weight as before", (ZDecimal)2, oldestOuterPackage.HVO_Weight);
			});
		}

		#region Implementation

		public static HVLVOuterPackage GetOuterPackageWithTestData(UniversalObjectFactory factory)
		{
			#region HVLV Outer Package

			var outerPackageBO = factory.New<HVLVOuterPackage>();
			outerPackageBO.HVO_Status = "OPN";
			outerPackageBO.HVO_PackageBarcode = "ECOM1234";
			outerPackageBO.HVO_ContainerNumber = "WTGC1234";
			outerPackageBO.HVO_F3_NKPackageType = "56L";
			outerPackageBO.HVO_Volume = 1.1m;
			outerPackageBO.HVO_Weight = 2.2m;
			outerPackageBO.HVO_Length = 3.3m;
			outerPackageBO.HVO_Height = 4.4m;
			outerPackageBO.HVO_Width = 5.5m;
			outerPackageBO.HVO_WeightUQ = "KG";
			outerPackageBO.HVO_VolumeUQ = "M3";
			outerPackageBO.HVO_UnitOfDimension = "M";
			outerPackageBO.HVO_PackageReference = "LTT001";
			outerPackageBO.HVO_RH_NKCommodityCode = "KFC";
			outerPackageBO.HVO_PL_NKLastMileCarrierServiceLevel = "D2D";

			#endregion

			#region OrgAddresses

			var destinationDepot = factory.NewWithValidTestData<OrgHeader>();
			destinationDepot.MainAddress.Address1 = "123 NVIDIA Street";
			destinationDepot.OH_Code = "NVIDIA";
			outerPackageBO.HVO_OA_DestinationDepot = destinationDepot.MainAddress.PK;

			var lastMileCarrier = factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.MainAddress.Address1 = "789 AMD Street";
			lastMileCarrier.OH_Code = "AMD";
			outerPackageBO.HVO_OH_LastMileCarrier = lastMileCarrier.PK;

			#endregion

			#region HVLV Items

			var bookingHeader = factory.New<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();

			var billToParty = factory.NewWithValidTestData<OrgHeader>();
			billToParty.MainAddress.Address1 = "1 Nintendo Street";
			billToParty.Contacts.AddNew().OC_ContactName = "Nintendo";
			bookingHeader.HVH_OA_BillToParty = billToParty.MainAddress.PK;
			bookingHeader.HVH_OC_BillToPartyContact = billToParty.Contacts[0].PK;

			var item1 = consignment.Items.AddNew();
			item1.HVI_ShipperReference = "NVIDIA001";
			item1.HVI_CurrentBarcode = "1234567890";
			item1.HVI_F3_NKPackType = "BOX";
			item1.HVI_Height = 3m;
			item1.HVI_Length = 4m;
			item1.HVI_Width = 5m;
			item1.HVI_UnitOfDimension = Length.Metres;
			item1.HVI_ManifestedWeight = 5;
			item1.HVI_ActualWeight = 10;
			item1.HVI_ManifestedVolume = 0.5;
			item1.HVI_ActualVolume = 0.5;
			item1.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			item1.HVI_IsDamaged = true;
			item1.HVI_IsPillaged = true;
			item1.HVI_ItemId = "HVI00001";

			var item2 = consignment.Items.AddNew();
			item2.HVI_ShipperReference = "AMD001";
			item2.HVI_F3_NKPackType = "BOX";
			item2.HVI_ManifestedWeight = 10;
			item2.HVI_ActualWeight = 10;
			item2.HVI_ManifestedVolume = 0.5;
			item2.HVI_ActualVolume = 0.5;
			item2.HVI_Status = HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot;
			item2.HVI_IsDamaged = true;
			item2.HVI_IsPillaged = false;
			item2.HVI_ItemId = "HVI00002";

			#endregion

			#region HVLV Item Lines

			var itemLine1 = item1.Lines.AddNew();
			itemLine1.HVS_OriginTariff = "123456";
			itemLine1.HVS_RN_NKOriginCountryCode = "AU";
			itemLine1.HVS_CustomsValue = 12.345;
			itemLine1.HVS_IntrinsicValue = 54.321;
			itemLine1.HVS_DestinationTariff = "654321";
			itemLine1.HVS_GoodsDescription = "RTX 3090 Ti";
			itemLine1.HVS_GrossWeight = 1.23;
			itemLine1.HVS_ItemURL = "www.nvidia.com/en-au/geforce/graphics-cards/30-series/rtx-3090-3090ti/";
			itemLine1.HVS_NetWeight = 3.21;

			var product = factory.New<OrgSupplierPart>();
			product.OP_PartNum = "RTX3090";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = bookingHeader.BillToParty.OA_OH;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			itemLine1.HVS_ProductCode = "RTX3090";
			itemLine1.HVS_Quantity = 1;
			itemLine1.HVS_WeightUnit = "KG";

			#endregion

			outerPackageBO.Items.AddRange(consignment.Items);

			return outerPackageBO;
		}

		public static string GetValidPopulatedUniversalShipmentXML()
		{
			return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>HVLVOuterPackage</Type>
          <Key>ECOM1234</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <CarrierServiceLevel>
      <Code>D2D</Code>
    </CarrierServiceLevel>
    <CommercialInfo>
      <CommercialInvoiceCollection>
        <CommercialInvoice>
          <CommercialInvoiceLineCollection>
            <CommercialInvoiceLine>
              <LineNo>1</LineNo>
              <CountryOfOrigin>
                <Code>AU</Code>
              </CountryOfOrigin>
              <CustomsQuantity>1</CustomsQuantity>
              <CustomsValue>12.345</CustomsValue>
              <Description>RTX 3090 Ti</Description>
              <HarmonisedCode>654321</HarmonisedCode>
              <InvoiceQuantity>1</InvoiceQuantity>
              <InvoiceQuantityUnit>
                <Code>PCE</Code>
              </InvoiceQuantityUnit>
              <LinePrice>12.345</LinePrice>
              <Link>1</Link>
              <LocalDescription></LocalDescription>
              <NetWeight>3.21</NetWeight>
              <NetWeightUnit>
                <Code>KG</Code>
              </NetWeightUnit>
              <PartNo>RTX3090</PartNo>
              <Weight>1.23</Weight>
              <WeightUnit>
                <Code>KG</Code>
              </WeightUnit>
              <CustomsSupportingInformationCollection>
                <CustomsSupportingInformation>
                  <Category>
                    <Code>SUP</Code>
                  </Category>
                  <Country>
                    <Code>AU</Code>
                  </Country>
                  <Tariff>123456</Tariff>
                </CustomsSupportingInformation>
              </CustomsSupportingInformationCollection>
            </CommercialInvoiceLine>
          </CommercialInvoiceLineCollection>
        </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
    <PackingLineCollection>
      <PackingLine>
        <Barcode>ECOM1234</Barcode>
        <Commodity>
          <Code>KFC</Code>
        </Commodity>
        <ContainerNumber>WTGC1234</ContainerNumber>
        <Height>4.4</Height>
        <IsHVLVClearance>true</IsHVLVClearance>
        <Length>3.3</Length>
        <LengthUnit>
          <Code>M</Code>
          <Description>Meters</Description>
        </LengthUnit>
        <PackQty>1</PackQty>
        <PackType>
          <Code>56L</Code>
        </PackType>
        <ReferenceNumber>LTT001</ReferenceNumber>
        <Status>OPN</Status>
        <TareWeight>2.2</TareWeight>
        <Volume>1.1</Volume>
        <VolumeUnit>
          <Code>M3</Code>
          <Description>Cubic Meters</Description>
        </VolumeUnit>
        <Weight>22.2</Weight>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
        <Width>5.5</Width>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>CustomsDepotAddress</AddressType>
            <Address1>123 NVIDIA Street</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>123 NVIDIA Street</AddressShortCode>
            <City></City>
            <CompanyName></CompanyName>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCategory>BUS</OrganizationCategory>
            <OrganizationCode>NVIDIA</OrganizationCode>
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
            <Address1>123 NVIDIA Street</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>123 NVIDIA Street</AddressShortCode>
            <City></City>
            <CompanyName></CompanyName>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCategory>BUS</OrganizationCategory>
            <OrganizationCode>NVIDIA</OrganizationCode>
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
            <AddressType>DeliveryLocalCartage</AddressType>
            <Address1>789 AMD Street</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>789 AMD Street</AddressShortCode>
            <City></City>
            <CompanyName></CompanyName>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCategory>BUS</OrganizationCategory>
            <OrganizationCode>AMD</OrganizationCode>
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
            <AddressType>PickupLocalCartage</AddressType>
            <Address1>789 AMD Street</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>789 AMD Street</AddressShortCode>
            <City></City>
            <CompanyName></CompanyName>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCategory>BUS</OrganizationCategory>
            <OrganizationCode>AMD</OrganizationCode>
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
            <Barcode>1234567890</Barcode>
            <BillNumber>HVC000000000000001</BillNumber>
            <BillType>
              <Code>HWB</Code>
              <Description>House Waybill</Description>
            </BillType>
            <ContainerNumber></ContainerNumber>
            <GoodsDescription></GoodsDescription>
            <Height>3</Height>
            <IsPerishable>false</IsPerishable>
            <IsPersonalEffects>false</IsPersonalEffects>
            <IsTimber>false</IsTimber>
            <Length>4</Length>
            <LengthUnit>
              <Code>M</Code>
              <Description>Meters</Description>
            </LengthUnit>
            <ManifestedVolume>0.5</ManifestedVolume>
            <ManifestedWeight>1.23</ManifestedWeight>
            <OrderReference>NVIDIA001</OrderReference>
            <OutturnDamagedQty>0</OutturnDamagedQty>
            <OutturnPillagedQty>0</OutturnPillagedQty>
            <OutturnQty>0</OutturnQty>
            <PackQty>1</PackQty>
            <PackType>
              <Code>BOX</Code>
              <Description>Box</Description>
            </PackType>
            <ReferenceNumber>HVI00001</ReferenceNumber>
            <RequiresFumigationCertificate>false</RequiresFumigationCertificate>
            <Volume>0.5</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meters</Description>
            </VolumeUnit>
            <Weight>10</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilograms</Description>
            </WeightUnit>
            <Width>5</Width>
            <PackedItemCollection>
              <PackedItem>
                <CIFValue>12.345</CIFValue>
                <CommercialInvoiceLineLink>1</CommercialInvoiceLineLink>
                <Description>RTX 3090 Ti</Description>
                <GoodsValue>54.321</GoodsValue>
                <GrossWeight>1.23</GrossWeight>
                <GrossWeightUnit>
                  <Code>KG</Code>
                </GrossWeightUnit>
                <ItemSpecificationUrl>www.nvidia.com/en-au/geforce/graphics-cards/30-series/rtx-3090-3090ti/</ItemSpecificationUrl>
                <NetWeight>3.21</NetWeight>
                <NetWeightUnit>
                  <Code>KG</Code>
                </NetWeightUnit>
                <PackedQuantity>1</PackedQuantity>
                <Product>
                  <Code>RTX3090</Code>
                </Product>
              </PackedItem>
            </PackedItemCollection>
          </PackingLine>
          <PackingLine>
            <Barcode>HVI00002</Barcode>
            <BillNumber>HVC000000000000001</BillNumber>
            <BillType>
              <Code>HWB</Code>
              <Description>House Waybill</Description>
            </BillType>
            <ContainerNumber></ContainerNumber>
            <GoodsDescription></GoodsDescription>
            <Height>0</Height>
            <IsPerishable>false</IsPerishable>
            <IsPersonalEffects>false</IsPersonalEffects>
            <IsTimber>false</IsTimber>
            <Length>0</Length>
            <LengthUnit>
              <Code>M</Code>
              <Description>Meters</Description>
            </LengthUnit>
            <ManifestedVolume>0.5</ManifestedVolume>
            <ManifestedWeight>10</ManifestedWeight>
            <OrderReference>AMD001</OrderReference>
            <OutturnDamagedQty>1</OutturnDamagedQty>
            <OutturnPillagedQty>0</OutturnPillagedQty>
            <OutturnQty>1</OutturnQty>
            <PackQty>1</PackQty>
            <PackType>
              <Code>BOX</Code>
              <Description>Box</Description>
            </PackType>
            <ReferenceNumber>HVI00002</ReferenceNumber>
            <RequiresFumigationCertificate>false</RequiresFumigationCertificate>
            <Volume>0.5</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meters</Description>
            </VolumeUnit>
            <Weight>10</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilograms</Description>
            </WeightUnit>
            <Width>0</Width>
          </PackingLine>
        </PackingLineCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override string ValidPopulatedUniversalShipmentXML => GetValidPopulatedUniversalShipmentXML();

		#endregion
	}
}
