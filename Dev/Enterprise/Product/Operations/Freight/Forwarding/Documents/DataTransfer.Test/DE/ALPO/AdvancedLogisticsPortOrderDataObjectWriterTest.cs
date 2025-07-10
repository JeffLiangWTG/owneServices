using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataTransfer;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.DE;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;
using Money = Enterprise.Freight.Forwarding.Documents.DocDataObjects.Money;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.Testing.DataTransfer.DE.Testing
{
	sealed class AdvancedLogisticsPortOrderDataObjectWriterTest : DataObjectWriterTest
	{
		CommonContext context;

		public void TestPopulateDataObject()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var hasFlashPoint = true;
				var advancedLogisticsPortOrder = PrepareTestData(hasFlashPoint);

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new AdvancedLogisticsPortOrderDataObjectWriter(manager);
				var dataObject = writer.GetDataObject(advancedLogisticsPortOrder);

				var flashPoint = "<FlashPoint>10</FlashPoint>".PadLeft(43, ' ');
				var expectedXml = GetExpectedXml(flashPoint);

				AssertUXml(dataObject, expectedXml);

				hasFlashPoint = false;
				advancedLogisticsPortOrder = PrepareTestData(hasFlashPoint);

				writer = new AdvancedLogisticsPortOrderDataObjectWriter(manager);
				dataObject = writer.GetDataObject(advancedLogisticsPortOrder);

				flashPoint = null;
				expectedXml = GetExpectedXml(flashPoint);

				AssertUXml(dataObject, expectedXml);
			}
		}

		public void TestPopulateDataObjectNRE()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var hasFlashPoint = false;
				var advancedLogisticsPortOrder = PrepareTestData(hasFlashPoint);

				advancedLogisticsPortOrder.CarrierBookingReference = null;
				advancedLogisticsPortOrder.ContainerMode = null;
				advancedLogisticsPortOrder.Vessel.LloydsIMO = null;
				advancedLogisticsPortOrder.PortOfDestination = null;
				advancedLogisticsPortOrder.PortOfDischarge = null;
				advancedLogisticsPortOrder.PortOfOrigin = null;
				advancedLogisticsPortOrder.PortOfOrigin = null;
				advancedLogisticsPortOrder.Vessel.Name = null;
				advancedLogisticsPortOrder.VoyageFlightNo = null;
				advancedLogisticsPortOrder.BillOfLading = null;

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new AdvancedLogisticsPortOrderDataObjectWriter(manager);
				var dataObject = writer.GetDataObject(advancedLogisticsPortOrder);

				AssertUXml(dataObject, expectedXmlNRE);
			}
		}

		AdvancedLogisticsPortOrder PrepareTestData(bool hasFlashPoint)
		{
			var advancedLogisticsPortOrder = new AdvancedLogisticsPortOrder("ForwardingConsol", "C00001015");

			advancedLogisticsPortOrder.ConsolNumber = "C00001015";
			advancedLogisticsPortOrder.CarrierBookingReference = "BKC001007";
			advancedLogisticsPortOrder.ContainerMode = new DummyCodeDescription() { Code = Core.Constants.ContainerModes.FCL, Description = "Full Container Load" };
			advancedLogisticsPortOrder.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "DEBRV",
				Name = "Bremerhaven"
			};
			advancedLogisticsPortOrder.PortOfDestination = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "DEBRV",
				Name = "Bremerhaven"
			};
			advancedLogisticsPortOrder.PortOfDischarge = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "DEBRV",
				Name = "Bremerhaven"
			};
			advancedLogisticsPortOrder.PortOfOrigin = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};
			advancedLogisticsPortOrder.Direction = Core.Constants.FreightShipmentDirection.Description.Import;
			advancedLogisticsPortOrder.MarksAndNumbers = "M&N";
			advancedLogisticsPortOrder.TransportModePreCarriageOrOnForwarding = new DummyCodeDescription { Code = "ROA", Description = "Road" };
			advancedLogisticsPortOrder.PreCarriageOrOnForwardingID = "PreCarriageID";

			advancedLogisticsPortOrder.ALPOUserID = "ALPO user";
			advancedLogisticsPortOrder.ALPOReference = "ALPO Reference";
			advancedLogisticsPortOrder.SisNumber = "N0001";

			advancedLogisticsPortOrder.VoyageFlightNo = "V0001";
			advancedLogisticsPortOrder.BillOfLading = "1122334499";
			advancedLogisticsPortOrder.ETD = new ZDateTime(2021, 5, 11, 15, 20, 00);
			advancedLogisticsPortOrder.ETA = new ZDateTime(2021, 6, 1, 13, 15, 00);
			advancedLogisticsPortOrder.Vessel = new DummyVessel
			{
				Name = "MSC UBERTY",
				LloydsIMO = "9337444"
			};

			advancedLogisticsPortOrder.CTO = CreateAddress("DepartureCTO");
			advancedLogisticsPortOrder.CTOWarehouseCode = CreateRegistrationNumber("PSN", "DepartureCTOWarehouseCode");
			advancedLogisticsPortOrder.EoriNumber = CreateRegistrationNumber("EOR", "abcde");
			advancedLogisticsPortOrder.EoriBranchSuffix = CreateRegistrationNumber("EBS", "12345");
			advancedLogisticsPortOrder.Forwarder = CreateAddress("SendingForwarder");
			advancedLogisticsPortOrder.Carrier = CreateAddress("Carrier");
			advancedLogisticsPortOrder.CarrierCode = CreateRegistrationNumber("PSN", "CarrierCode");
			advancedLogisticsPortOrder.SendingParty = CreateAddress("SendingParty");

			var dangerousgood121 = CreateDangerousGood("0503B", "AIR BAG MODULES", "Airbag Mercedes C", "1.4G", "II", "F-B", "S-X", 5, "PLT", 142.01m, "G", "Grams", 0, "F3", "Cubic Feet", hasFlashPoint, 0m);
			var dangerousgood122 = CreateDangerousGood("0453B", "ROCKETS, LINE-THROWING", "Ejection seat", "1.4G", "III", "F-B", "S-X", 1, "PLT", 213.01m, "KG", "Kilograms", 0, "", "", hasFlashPoint, 1m);
			var dangerousgood211 = CreateDangerousGood("0503B", "AIR BAG MODULES", "Airbag Mercedes B", "1.4G", "II", "F-B", "S-X", 5, "BAG", 120.00m, "KG", "Kilograms", 0, "", "", hasFlashPoint, 0.2m);
			dangerousgood211.PackedInLimitedQuantity = true;

			var packline11 = CreatePackingLine("ROOF Covering", "MSCU1245787", 1, 18, 10, "1", "1");
			var packline12 = CreatePackingLine("Goods description packline 2", "MSCU1245787", 1, 453, 0, "1", "2");
			packline12.DangerousGoods = new[]
			{
				dangerousgood121,
				dangerousgood122
			};
			var packline21 = CreatePackingLine("DASHBOARD MERCEDES", "MSCU1247856", 5, 256.15m, 150.00m, null, null);
			packline21.DangerousGoods = new[]
			{
				dangerousgood211
			};

			var container1 = CreateContainer(context, "MSCU1245787", 2, 471, 100, 20, 100, 10);
			container1.IsNonOperativeReefer = true;
			container1.PackingLines = new[]
			{
				packline11,
				packline12
			};
			packline11.ContainerNumber = container1.Number;
			packline12.ContainerNumber = container1.Number;

			var container2 = CreateContainer(context, "MSCU1247856", 5, 256, 100, 20, 256, 150);
			container2.IsNonOperativeReefer = false;
			container2.PackingLines = new[]
			{
				packline21
			};

			packline21.ContainerNumber = container2.Number;

			advancedLogisticsPortOrder.Containers = new[]
			{
				container1,
				container2
			};

			var shipment1 = CreateShipment("S0001");
			var shipment2 = CreateShipment("S0002");

			advancedLogisticsPortOrder.Shipments = new List<Shipment>() { shipment1, shipment2 };

			packline11.ShipmentID = shipment1.ShipmentID;
			packline12.ShipmentID = shipment1.ShipmentID;
			packline21.ShipmentID = shipment2.ShipmentID;

			shipment1.PackingLines = new List<PackingLine> { packline11, packline12 };
			shipment2.PackingLines = new List<PackingLine> { packline21 };

			return advancedLogisticsPortOrder;
		}

		const string expectedXmlNRE = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00001015</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

      <Workflow>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Department"">BRN</EventDepartment>
        <EventUser Name=""CargoWise Support"">E</EventUser>
      </Workflow>
    </DataContext>

    <BookingConfirmationReference></BookingConfirmationReference>
    <LloydsIMO></LloydsIMO>
    <VesselName></VesselName>
    <VoyageFlightNo></VoyageFlightNo>
    <WayBillNumber></WayBillNumber>
    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>DEBRV</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Bremerhaven</Value>
      </AddInfo>
      <AddInfo>
        <Key>AlpoReference</Key>
        <Value>ALPO Reference</Value>
      </AddInfo>
      <AddInfo>
        <Key>AlpoUserId</Key>
        <Value>ALPO user</Value>
      </AddInfo>
      <AddInfo>
        <Key>Direction</Key>
        <Value>Import</Value>
      </AddInfo>
      <AddInfo>
        <Key>MarksAndNumbers</Key>
        <Value>M&amp;N</Value>
      </AddInfo>
      <AddInfo>
        <Key>Other_TransportMode</Key>
        <Value>ROA</Value>
      </AddInfo>
      <AddInfo>
        <Key>Other_TransportID</Key>
        <Value>PreCarriageID</Value>
      </AddInfo>
      <AddInfo>
        <Key>SIS_Number</Key>
        <Value>N0001</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>2.0.0</Value>
      </AddInfo>
    </AddInfoCollection>
    <ContainerCollection>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>MSCU1245787</ContainerNumber>
        <ContainerType>
          <Code>42G1</Code>
          <Description></Description>
          <ISOCode>42G1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>20</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>471</GoodsWeight>
        <GrossWeight>100</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>true</NonOperatingReefer>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>100</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>MSCU1247856</ContainerNumber>
        <ContainerType>
          <Code>42G1</Code>
          <Description></Description>
          <ISOCode>42G1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>20</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>256</GoodsWeight>
        <GrossWeight>256</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>100</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <WeightUnit>KG</WeightUnit>
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
    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <Value>2021-05-11T15:20:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <Value>2021-06-01T13:15:00</Value>
      </Date>
    </DateCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Warehouse</AddressType>
        <AdditionalAddressInformation>DepartureCTO additional info</AdditionalAddressInformation>
        <Address1>DepartureCTO address line 1</Address1>
        <Address2>DepartureCTO address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>DepartureCTO city</City>
        <CompanyName>DepartureCTO</CompanyName>
        <Contact>DepartureCTO contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>DepartureCTO email</Email>
        <Fax>DepartureCTO fax</Fax>
        <GovRegNum>DepartureCTO tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>DepartureCTO phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>DepartureC</Postcode>
        <State>DepartureCTO state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Germany"">DE</CountryOfIssue>
            <Value>DepartureCTOWarehouseCode</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
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
            <Type Description=""EORI code (to be sent verbatim)"">EOR</Type>
            <CountryOfIssue Name=""Germany"">DE</CountryOfIssue>
            <Value>abcde</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""EORI branch suffix"">EBS</Type>
            <CountryOfIssue Name=""Germany"">DE</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
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
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Germany"">DE</CountryOfIssue>
            <Value>CarrierCode</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
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
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S0001</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference></BookingConfirmationReference>
        <GoodsValue>1001</GoodsValue>
        <GoodsValueCurrency Description=""Euro"">EUR</GoodsValueCurrency>
        <PortOfDestination Name=""Sydney"">AUSYD</PortOfDestination>
        <PortOfOrigin Name=""Bremerhaven"">DEBRV</PortOfOrigin>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber></WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy></DeliveryRequiredBy>
          <PickupRequiredBy></PickupRequiredBy>
        </LocalProcessing>
        <EntryNumberCollection>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>ConsignorDocumentaryAddress additional info</AdditionalAddressInformation>
            <Address1>ConsignorDocumentaryAddress address line 1</Address1>
            <Address2>ConsignorDocumentaryAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>ConsignorDocumentaryAddress city</City>
            <CompanyName>ConsignorDocumentaryAddress</CompanyName>
            <Contact>ConsignorDocumentaryAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>ConsignorDocumentaryAddress email</Email>
            <Fax>ConsignorDocumentary</Fax>
            <GovRegNum>ConsignorDocumentaryAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>ConsignorDocumentary</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>ConsignorD</Postcode>
            <State>ConsignorDocumentaryAddre</State>
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
            <AdditionalAddressInformation>ConsigneeDocumentaryAddress additional info</AdditionalAddressInformation>
            <Address1>ConsigneeDocumentaryAddress address line 1</Address1>
            <Address2>ConsigneeDocumentaryAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>ConsigneeDocumentaryAddress city</City>
            <CompanyName>ConsigneeDocumentaryAddress</CompanyName>
            <Contact>ConsigneeDocumentaryAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>ConsigneeDocumentaryAddress email</Email>
            <Fax>ConsigneeDocumentary</Fax>
            <GovRegNum>ConsigneeDocumentaryAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>ConsigneeDocumentary</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>ConsigneeD</Postcode>
            <State>ConsigneeDocumentaryAddre</State>
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
            <Commodity Description=""Commodity XXX"">CMD</Commodity>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>MSCU1245787</ContainerNumber>
            <DetailedDescription>ROOF Covering</DetailedDescription>
            <ExportReferenceNumber>EXP Number</ExportReferenceNumber>
            <GoodsDescription>ROOF Covering</GoodsDescription>
            <HarmonisedCode>HC Code</HarmonisedCode>
            <ImportReferenceNumber></ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment>Outturn comment</OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>1</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>10</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>18</Weight>
            <WeightUnit>KG</WeightUnit>
            <AddInfoCollection>
              <AddInfo>
                <Key>EntryType</Key>
                <Value>AES</Value>
              </AddInfo>
              <AddInfo>
                <Key>CustomsStatusComplete</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>Shortage</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>CargoItem</Key>
                <Value>1</Value>
              </AddInfo>
              <AddInfo>
                <Key>PackageNumber</Key>
                <Value>1</Value>
              </AddInfo>
            </AddInfoCollection>
          </PackingLine>
          <PackingLine>
            <Commodity Description=""Commodity XXX"">CMD</Commodity>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>MSCU1245787</ContainerNumber>
            <DetailedDescription>Goods description packline 2</DetailedDescription>
            <ExportReferenceNumber>EXP Number</ExportReferenceNumber>
            <GoodsDescription>Goods description packline 2</GoodsDescription>
            <HarmonisedCode>HC Code</HarmonisedCode>
            <ImportReferenceNumber></ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment>Outturn comment</OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>1</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>0</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>453</Weight>
            <WeightUnit>KG</WeightUnit>
            <AddInfoCollection>
              <AddInfo>
                <Key>EntryType</Key>
                <Value>AES</Value>
              </AddInfo>
              <AddInfo>
                <Key>CustomsStatusComplete</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>Shortage</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>CargoItem</Key>
                <Value>1</Value>
              </AddInfo>
              <AddInfo>
                <Key>PackageNumber</Key>
                <Value>2</Value>
              </AddInfo>
            </AddInfoCollection>
            <UNDGCollection>
              <UNDG>
                <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
                <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
                <IMOClass>1.4G</IMOClass>
                <NetExplosiveWeight>0</NetExplosiveWeight>
                <NetExplosiveWeightUQ Description=""Kilogram"">KG</NetExplosiveWeightUQ>
                <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
                <PackingGroup>II</PackingGroup>
                <PackQty>5</PackQty>
                <PackType>PLT</PackType>
                <ProperShippingName>AIR BAG MODULES</ProperShippingName>
                <Standard></Standard>
                <State>Gas</State>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Airbag Mercedes C</TechicalName>
                <UNDGCode>0503B</UNDGCode>
                <Volume>0</Volume>
                <VolumeUQ Description=""Cubic Feet"">F3</VolumeUQ>
                <Weight>142.01</Weight>
                <WeightUQ Description=""Grams"">G</WeightUQ>
              </UNDG>
              <UNDG>
                <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
                <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
                <IMOClass>1.4G</IMOClass>
                <NetExplosiveWeight>1</NetExplosiveWeight>
                <NetExplosiveWeightUQ Description=""Kilogram"">KG</NetExplosiveWeightUQ>
                <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
                <PackingGroup>III</PackingGroup>
                <PackQty>1</PackQty>
                <PackType>PLT</PackType>
                <ProperShippingName>ROCKETS, LINE-THROWING</ProperShippingName>
                <Standard></Standard>
                <State>Gas</State>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Ejection seat</TechicalName>
                <UNDGCode>0453B</UNDGCode>
                <Volume>0</Volume>
                <VolumeUQ></VolumeUQ>
                <Weight>213.01</Weight>
                <WeightUQ Description=""Kilograms"">KG</WeightUQ>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S0002</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference></BookingConfirmationReference>
        <GoodsValue>1001</GoodsValue>
        <GoodsValueCurrency Description=""Euro"">EUR</GoodsValueCurrency>
        <PortOfDestination Name=""Sydney"">AUSYD</PortOfDestination>
        <PortOfOrigin Name=""Bremerhaven"">DEBRV</PortOfOrigin>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber></WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy></DeliveryRequiredBy>
          <PickupRequiredBy></PickupRequiredBy>
        </LocalProcessing>
        <EntryNumberCollection>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>ConsignorDocumentaryAddress additional info</AdditionalAddressInformation>
            <Address1>ConsignorDocumentaryAddress address line 1</Address1>
            <Address2>ConsignorDocumentaryAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>ConsignorDocumentaryAddress city</City>
            <CompanyName>ConsignorDocumentaryAddress</CompanyName>
            <Contact>ConsignorDocumentaryAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>ConsignorDocumentaryAddress email</Email>
            <Fax>ConsignorDocumentary</Fax>
            <GovRegNum>ConsignorDocumentaryAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>ConsignorDocumentary</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>ConsignorD</Postcode>
            <State>ConsignorDocumentaryAddre</State>
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
            <AdditionalAddressInformation>ConsigneeDocumentaryAddress additional info</AdditionalAddressInformation>
            <Address1>ConsigneeDocumentaryAddress address line 1</Address1>
            <Address2>ConsigneeDocumentaryAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>ConsigneeDocumentaryAddress city</City>
            <CompanyName>ConsigneeDocumentaryAddress</CompanyName>
            <Contact>ConsigneeDocumentaryAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>ConsigneeDocumentaryAddress email</Email>
            <Fax>ConsigneeDocumentary</Fax>
            <GovRegNum>ConsigneeDocumentaryAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>ConsigneeDocumentary</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>ConsigneeD</Postcode>
            <State>ConsigneeDocumentaryAddre</State>
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
            <Commodity Description=""Commodity XXX"">CMD</Commodity>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>MSCU1247856</ContainerNumber>
            <DetailedDescription>DASHBOARD MERCEDES</DetailedDescription>
            <ExportReferenceNumber>EXP Number</ExportReferenceNumber>
            <GoodsDescription>DASHBOARD MERCEDES</GoodsDescription>
            <HarmonisedCode>HC Code</HarmonisedCode>
            <ImportReferenceNumber></ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment>Outturn comment</OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>5</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>150.00</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>256.15</Weight>
            <WeightUnit>KG</WeightUnit>
            <AddInfoCollection>
              <AddInfo>
                <Key>EntryType</Key>
                <Value>AES</Value>
              </AddInfo>
              <AddInfo>
                <Key>CustomsStatusComplete</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>Shortage</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>CargoItem</Key>
                <Value></Value>
              </AddInfo>
              <AddInfo>
                <Key>PackageNumber</Key>
                <Value></Value>
              </AddInfo>
            </AddInfoCollection>
            <UNDGCollection>
              <UNDG>
                <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
                <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
                <IMOClass>1.4G</IMOClass>
                <NetExplosiveWeight>0.2</NetExplosiveWeight>
                <NetExplosiveWeightUQ Description=""Kilogram"">KG</NetExplosiveWeightUQ>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup>II</PackingGroup>
                <PackQty>5</PackQty>
                <PackType>BAG</PackType>
                <ProperShippingName>AIR BAG MODULES</ProperShippingName>
                <Standard></Standard>
                <State>Gas</State>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Airbag Mercedes B</TechicalName>
                <UNDGCode>0503B</UNDGCode>
                <Volume>0</Volume>
                <VolumeUQ></VolumeUQ>
                <Weight>120.00</Weight>
                <WeightUQ Description=""Kilograms"">KG</WeightUQ>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";

		string GetExpectedXml(string flashpoint) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00001015</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

      <Workflow>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Department"">BRN</EventDepartment>
        <EventUser Name=""CargoWise Support"">E</EventUser>
      </Workflow>
    </DataContext>

    <BookingConfirmationReference>BKC001007</BookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <LloydsIMO>9337444</LloydsIMO>
    <PortOfDestination Name=""Bremerhaven"">DEBRV</PortOfDestination>
    <PortOfDischarge Name=""Bremerhaven"">DEBRV</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <VesselName>MSC UBERTY</VesselName>
    <VoyageFlightNo>V0001</VoyageFlightNo>
    <WayBillNumber>1122334499</WayBillNumber>
    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>DEBRV</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Bremerhaven</Value>
      </AddInfo>
      <AddInfo>
        <Key>AlpoReference</Key>
        <Value>ALPO Reference</Value>
      </AddInfo>
      <AddInfo>
        <Key>AlpoUserId</Key>
        <Value>ALPO user</Value>
      </AddInfo>
      <AddInfo>
        <Key>Direction</Key>
        <Value>Import</Value>
      </AddInfo>
      <AddInfo>
        <Key>MarksAndNumbers</Key>
        <Value>M&amp;N</Value>
      </AddInfo>
      <AddInfo>
        <Key>Other_TransportMode</Key>
        <Value>ROA</Value>
      </AddInfo>
      <AddInfo>
        <Key>Other_TransportID</Key>
        <Value>PreCarriageID</Value>
      </AddInfo>
      <AddInfo>
        <Key>SIS_Number</Key>
        <Value>N0001</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>2.0.0</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>BKC001007</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <ContainerCollection>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>MSCU1245787</ContainerNumber>
        <ContainerType>
          <Code>42G1</Code>
          <Description></Description>
          <ISOCode>42G1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>20</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>471</GoodsWeight>
        <GrossWeight>100</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>true</NonOperatingReefer>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>100</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>MSCU1247856</ContainerNumber>
        <ContainerType>
          <Code>42G1</Code>
          <Description></Description>
          <ISOCode>42G1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>20</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>256</GoodsWeight>
        <GrossWeight>256</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>100</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <WeightUnit>KG</WeightUnit>
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
    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <Value>2021-05-11T15:20:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <Value>2021-06-01T13:15:00</Value>
      </Date>
    </DateCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Warehouse</AddressType>
        <AdditionalAddressInformation>DepartureCTO additional info</AdditionalAddressInformation>
        <Address1>DepartureCTO address line 1</Address1>
        <Address2>DepartureCTO address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>DepartureCTO city</City>
        <CompanyName>DepartureCTO</CompanyName>
        <Contact>DepartureCTO contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>DepartureCTO email</Email>
        <Fax>DepartureCTO fax</Fax>
        <GovRegNum>DepartureCTO tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>DepartureCTO phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>DepartureC</Postcode>
        <State>DepartureCTO state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Germany"">DE</CountryOfIssue>
            <Value>DepartureCTOWarehouseCode</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
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
            <Type Description=""EORI code (to be sent verbatim)"">EOR</Type>
            <CountryOfIssue Name=""Germany"">DE</CountryOfIssue>
            <Value>abcde</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""EORI branch suffix"">EBS</Type>
            <CountryOfIssue Name=""Germany"">DE</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShippingLineAddress</AddressType>
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
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Germany"">DE</CountryOfIssue>
            <Value>CarrierCode</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
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
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S0001</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference></BookingConfirmationReference>
        <GoodsValue>1001</GoodsValue>
        <GoodsValueCurrency Description=""Euro"">EUR</GoodsValueCurrency>
        <PortOfDestination Name=""Sydney"">AUSYD</PortOfDestination>
        <PortOfOrigin Name=""Bremerhaven"">DEBRV</PortOfOrigin>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber></WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy></DeliveryRequiredBy>
          <PickupRequiredBy></PickupRequiredBy>
        </LocalProcessing>
        <EntryNumberCollection>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>ConsignorDocumentaryAddress additional info</AdditionalAddressInformation>
            <Address1>ConsignorDocumentaryAddress address line 1</Address1>
            <Address2>ConsignorDocumentaryAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>ConsignorDocumentaryAddress city</City>
            <CompanyName>ConsignorDocumentaryAddress</CompanyName>
            <Contact>ConsignorDocumentaryAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>ConsignorDocumentaryAddress email</Email>
            <Fax>ConsignorDocumentary</Fax>
            <GovRegNum>ConsignorDocumentaryAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>ConsignorDocumentary</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>ConsignorD</Postcode>
            <State>ConsignorDocumentaryAddre</State>
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
            <AdditionalAddressInformation>ConsigneeDocumentaryAddress additional info</AdditionalAddressInformation>
            <Address1>ConsigneeDocumentaryAddress address line 1</Address1>
            <Address2>ConsigneeDocumentaryAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>ConsigneeDocumentaryAddress city</City>
            <CompanyName>ConsigneeDocumentaryAddress</CompanyName>
            <Contact>ConsigneeDocumentaryAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>ConsigneeDocumentaryAddress email</Email>
            <Fax>ConsigneeDocumentary</Fax>
            <GovRegNum>ConsigneeDocumentaryAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>ConsigneeDocumentary</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>ConsigneeD</Postcode>
            <State>ConsigneeDocumentaryAddre</State>
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
            <Commodity Description=""Commodity XXX"">CMD</Commodity>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>MSCU1245787</ContainerNumber>
            <DetailedDescription>ROOF Covering</DetailedDescription>
            <ExportReferenceNumber>EXP Number</ExportReferenceNumber>
            <GoodsDescription>ROOF Covering</GoodsDescription>
            <HarmonisedCode>HC Code</HarmonisedCode>
            <ImportReferenceNumber></ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment>Outturn comment</OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>1</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>10</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>18</Weight>
            <WeightUnit>KG</WeightUnit>
            <AddInfoCollection>
              <AddInfo>
                <Key>EntryType</Key>
                <Value>AES</Value>
              </AddInfo>
              <AddInfo>
                <Key>CustomsStatusComplete</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>Shortage</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>CargoItem</Key>
                <Value>1</Value>
              </AddInfo>
              <AddInfo>
                <Key>PackageNumber</Key>
                <Value>1</Value>
              </AddInfo>
            </AddInfoCollection>
          </PackingLine>
          <PackingLine>
            <Commodity Description=""Commodity XXX"">CMD</Commodity>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>MSCU1245787</ContainerNumber>
            <DetailedDescription>Goods description packline 2</DetailedDescription>
            <ExportReferenceNumber>EXP Number</ExportReferenceNumber>
            <GoodsDescription>Goods description packline 2</GoodsDescription>
            <HarmonisedCode>HC Code</HarmonisedCode>
            <ImportReferenceNumber></ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment>Outturn comment</OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>1</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>0</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>453</Weight>
            <WeightUnit>KG</WeightUnit>
            <AddInfoCollection>
              <AddInfo>
                <Key>EntryType</Key>
                <Value>AES</Value>
              </AddInfo>
              <AddInfo>
                <Key>CustomsStatusComplete</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>Shortage</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>CargoItem</Key>
                <Value>1</Value>
              </AddInfo>
              <AddInfo>
                <Key>PackageNumber</Key>
                <Value>2</Value>
              </AddInfo>
            </AddInfoCollection>
            <UNDGCollection>
              <UNDG>
                <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
                <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint}
                <IMOClass>1.4G</IMOClass>
                <NetExplosiveWeight>0</NetExplosiveWeight>
                <NetExplosiveWeightUQ Description=""Kilogram"">KG</NetExplosiveWeightUQ>
                <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
                <PackingGroup>II</PackingGroup>
                <PackQty>5</PackQty>
                <PackType>PLT</PackType>
                <ProperShippingName>AIR BAG MODULES</ProperShippingName>
                <Standard></Standard>
                <State>Gas</State>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Airbag Mercedes C</TechicalName>
                <UNDGCode>0503B</UNDGCode>
                <Volume>0</Volume>
                <VolumeUQ Description=""Cubic Feet"">F3</VolumeUQ>
                <Weight>142.01</Weight>
                <WeightUQ Description=""Grams"">G</WeightUQ>
              </UNDG>
              <UNDG>
                <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
                <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint}
                <IMOClass>1.4G</IMOClass>
                <NetExplosiveWeight>1</NetExplosiveWeight>
                <NetExplosiveWeightUQ Description=""Kilogram"">KG</NetExplosiveWeightUQ>
                <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
                <PackingGroup>III</PackingGroup>
                <PackQty>1</PackQty>
                <PackType>PLT</PackType>
                <ProperShippingName>ROCKETS, LINE-THROWING</ProperShippingName>
                <Standard></Standard>
                <State>Gas</State>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Ejection seat</TechicalName>
                <UNDGCode>0453B</UNDGCode>
                <Volume>0</Volume>
                <VolumeUQ></VolumeUQ>
                <Weight>213.01</Weight>
                <WeightUQ Description=""Kilograms"">KG</WeightUQ>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S0002</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <BookingConfirmationReference></BookingConfirmationReference>
        <GoodsValue>1001</GoodsValue>
        <GoodsValueCurrency Description=""Euro"">EUR</GoodsValueCurrency>
        <PortOfDestination Name=""Sydney"">AUSYD</PortOfDestination>
        <PortOfOrigin Name=""Bremerhaven"">DEBRV</PortOfOrigin>
        <ShipmentType>STD</ShipmentType>
        <WayBillNumber></WayBillNumber>
        <LocalProcessing>
          <DeliveryRequiredBy></DeliveryRequiredBy>
          <PickupRequiredBy></PickupRequiredBy>
        </LocalProcessing>
        <EntryNumberCollection>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Internal Transaction Number"">ITN</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""Declaração Única de Exportação"">DUE</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type Description=""UniqueConsignementReference"">UCR</Type>
            <EntryIsSystemGenerated>false</EntryIsSystemGenerated>
          </EntryNumber>
        </EntryNumberCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>ConsignorDocumentaryAddress additional info</AdditionalAddressInformation>
            <Address1>ConsignorDocumentaryAddress address line 1</Address1>
            <Address2>ConsignorDocumentaryAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>ConsignorDocumentaryAddress city</City>
            <CompanyName>ConsignorDocumentaryAddress</CompanyName>
            <Contact>ConsignorDocumentaryAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>ConsignorDocumentaryAddress email</Email>
            <Fax>ConsignorDocumentary</Fax>
            <GovRegNum>ConsignorDocumentaryAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>ConsignorDocumentary</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>ConsignorD</Postcode>
            <State>ConsignorDocumentaryAddre</State>
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
            <AdditionalAddressInformation>ConsigneeDocumentaryAddress additional info</AdditionalAddressInformation>
            <Address1>ConsigneeDocumentaryAddress address line 1</Address1>
            <Address2>ConsigneeDocumentaryAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>ConsigneeDocumentaryAddress city</City>
            <CompanyName>ConsigneeDocumentaryAddress</CompanyName>
            <Contact>ConsigneeDocumentaryAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>ConsigneeDocumentaryAddress email</Email>
            <Fax>ConsigneeDocumentary</Fax>
            <GovRegNum>ConsigneeDocumentaryAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>ConsigneeDocumentary</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>ConsigneeD</Postcode>
            <State>ConsigneeDocumentaryAddre</State>
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
            <Commodity Description=""Commodity XXX"">CMD</Commodity>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>MSCU1247856</ContainerNumber>
            <DetailedDescription>DASHBOARD MERCEDES</DetailedDescription>
            <ExportReferenceNumber>EXP Number</ExportReferenceNumber>
            <GoodsDescription>DASHBOARD MERCEDES</GoodsDescription>
            <HarmonisedCode>HC Code</HarmonisedCode>
            <ImportReferenceNumber></ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment>Outturn comment</OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>5</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>150.00</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>256.15</Weight>
            <WeightUnit>KG</WeightUnit>
            <AddInfoCollection>
              <AddInfo>
                <Key>EntryType</Key>
                <Value>AES</Value>
              </AddInfo>
              <AddInfo>
                <Key>CustomsStatusComplete</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>Shortage</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>CargoItem</Key>
                <Value></Value>
              </AddInfo>
              <AddInfo>
                <Key>PackageNumber</Key>
                <Value></Value>
              </AddInfo>
            </AddInfoCollection>
            <UNDGCollection>
              <UNDG>
                <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
                <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint}
                <IMOClass>1.4G</IMOClass>
                <NetExplosiveWeight>0.2</NetExplosiveWeight>
                <NetExplosiveWeightUQ Description=""Kilogram"">KG</NetExplosiveWeightUQ>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup>II</PackingGroup>
                <PackQty>5</PackQty>
                <PackType>BAG</PackType>
                <ProperShippingName>AIR BAG MODULES</ProperShippingName>
                <Standard></Standard>
                <State>Gas</State>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Airbag Mercedes B</TechicalName>
                <UNDGCode>0503B</UNDGCode>
                <Volume>0</Volume>
                <VolumeUQ></VolumeUQ>
                <Weight>120.00</Weight>
                <WeightUQ Description=""Kilograms"">KG</WeightUQ>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
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

		protected override RegistrationNumber CreateRegistrationNumber(string type, string value)
		{
			return new RegistrationNumber()
			{
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.Germany
				},
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.Germany))
				{
					Code = type
				},
				Value = value
			};
		}

		Container CreateContainer(IContext context, string containerNumber, int numberOfPacks, decimal goodsWeight, decimal tareWeight, decimal dunnage, decimal grossWeight, decimal volume)
		{
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);

			container.Number = containerNumber;
			container.Type = new ContainerType(context.ContainerTypes)
			{
				Code = "42G1",
				ISOCode = "42G1"
			};
			container.ContainerCount = 1;
			container.PackCount = numberOfPacks;
			container.IsEmpty = false;

			container.Seal = "SEAL1";
			container.SealPartyType = new DummyCodeDescription
			{
				Code = "CAR",
				Description = "Carrier"
			};
			container.SecondSeal = "SEAL2";
			container.SecondSealPartyType = new DummyCodeDescription
			{
				Code = "CUS",
				Description = "Customs"
			};
			container.ThirdSeal = "SEAL3";
			container.ThirdSealPartyType = new DummyCodeDescription
			{
				Code = "CTP",
				Description = "Terminal"
			};
			container.GoodsWeight = new Measurement()
			{
				Value = goodsWeight,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.TareWeight = new Measurement()
			{
				Value = tareWeight,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.Dunnage = new Measurement()
			{
				Value = dunnage,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.GrossWeight = new Measurement()
			{
				Value = grossWeight,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.Volume = new Measurement()
			{
				Value = volume,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			return container;
		}

		Shipment CreateShipment(ZString shipmentID)
		{
			var shipment = new Shipment(ZGuid.NewZGuid());
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipment.ShipmentType = new CodeDescription(new CodeDescriptionPairList())
			{
				Code = Core.Constants.ShipmentTypes.StandardHouse
			};
			shipment.ShipmentID = shipmentID;
			shipment.Origin = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "DEBRV",
				Name = "Bremerhaven"
			};
			shipment.Destination = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};

			shipment.GoodsValue = new Money
			{
				Amount = 1001,
				Currency = new CodeDescription(shipmentBO.Lookups.RefCurrency_List) { Code = Core.Constants.CurrencyCodes.EuropeanUnion }
			};
			shipment.Consignee = CreateAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			shipment.Consignor = CreateAddress(nameof(DocAddressType.ConsignorDocumentaryAddress));

			return shipment;
		}

		PackingLine CreatePackingLine(string goodsDescription, string containerNumber, int quantity, decimal weight, decimal volume, string cargoItem, string packageNumber)
		{
			var packingLine = new PackingLine(ZGuid.NewZGuid(), Factory);

			packingLine.Quantity = quantity;
			packingLine.PackageType = new DummyCodeDescription
			{
				Code = "PLT",
				Description = "Pallet"
			};
			packingLine.Weight = new Measurement()
			{
				Value = weight,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			packingLine.Volume = new Measurement()
			{
				Value = volume,
				Unit = new DummyCodeDescription
				{
					Code = "M3",
				}
			};
			packingLine.GoodsDescription = goodsDescription;
			packingLine.ContainerNumber = containerNumber;

			packingLine.MarksAndNumbers = "marks & nums";
			packingLine.OutturnComment = "Outturn comment";
			packingLine.ReferenceNumber = "reference number";
			packingLine.EntryType = new CodeDescription(new EntryTypes()) { Code = EntryTypes.Codes.EntryTypes_AES };
			packingLine.HarmonizedCode = new HarmonizedCode { Code = "HC Code" };
			packingLine.ExportReferenceNumber = "EXP Number";
			packingLine.PackageNumber = packageNumber;
			packingLine.CargoItem = cargoItem;

			packingLine.Commodity = new DummyCodeDescription
			{
				Code = "CMD",
				Description = "Commodity XXX"
			};

			return packingLine;
		}

		DangerousGood CreateDangerousGood(string undg, string properShippingName, string technicalName, string imoClass, string packingGroup,
			string emergencyScheduleFireCode, string emergencyScheduleSpillageCode, int quantity, string packType,
			decimal weight, string weighUnitCode, string weighUnitCodeDescription,
			decimal volume, string volumeUnitCode, string volumeUnitCodeDescription, bool hasFlashPoint, decimal netExplosiveWeight)
		{
			var dangerousGood = new DangerousGood()
			{
				Code = undg,
				Unno = undg,
				Variant = "C",
				ProperShippingName = properShippingName,
				TechnicalName = technicalName,
				IMOClass = imoClass,
				PackingGroup = packingGroup,
				State = "G",
				EmergencyScheduleFire = new DummyCodeDescription() { Code = emergencyScheduleFireCode },
				EmergencyScheduleSpillage = new DummyCodeDescription() { Code = emergencyScheduleSpillageCode },
				Quantity = quantity,
				PackageType = new DummyCodeDescription() { Code = packType },
				Weight = new Measurement
				{
					Value = weight,
					Unit = new DummyCodeDescription()
					{
						Code = weighUnitCode,
						Description = weighUnitCodeDescription
					}
				},
				NetExplosiveWeight = new Measurement
				{
					Value = netExplosiveWeight,
					Unit = new DummyCodeDescription()
					{
						Code = Core.Constants.Weight.Kilograms,
						Description = Core.Constants.Weight.GetDescription(Core.Constants.Weight.Kilograms, Core.Constants.PluralState.NonPlural)
					}
				},
				Volume = new Measurement
				{
					Value = volume,
					Unit = new DummyCodeDescription()
					{
						Code = volumeUnitCode,
						Description = volumeUnitCodeDescription
					}
				},
			};

			if (hasFlashPoint)
			{
				dangerousGood.FlashPoint = new Measurement
				{
					Value = 10,
					Unit = new CodeDescription(context.TemperatureUnits)
					{
						Code = "C"
					}
				};
			}

			return dangerousGood;
		}
	}
}
