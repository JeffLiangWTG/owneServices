using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class VerifiedGrossMassDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			var context = new CommonContext(factory);

			var verifiedGrossMass = new VerifiedGrossMass
			(
				"ForwardingConsol",
				"C00001000"
			);

			verifiedGrossMass.ContainerMode = new CodeDescription(new CodeDescriptionPairList())
			{
				Code = Core.Constants.ContainerModes.FCL,
				Description = "Full Container Load"
			};

			verifiedGrossMass.ShipmentType = new DummyCodeDescription
			{
				Code = Core.Constants.AgentType.Agent,
				Description = Core.Constants.AgentTypeDescriptions.Agent
			};

			verifiedGrossMass.OperationalPort = new DummyUnloco
			{
				Code = "CNNBO",
				Name = "Ningbo",
				IATACode = "NBO"
			};

			PopulateOrganizations(verifiedGrossMass);

			var containers = new List<VGMMessagingContainer>()
			{
				CreateContainer(context, "123", true, addSLD: true, isNonOperativeReefer: true),
				CreateContainer(context, "234", true),
				CreateContainer(context, "345", false, isNonOperativeReefer: true),
				CreateContainer(context, "456", false),
			};

			verifiedGrossMass.Containers = containers;
			verifiedGrossMass.CarrierBookingReference = "BK123456";
			verifiedGrossMass.BillOfLadingNumber = "BL123";
			verifiedGrossMass.FreightForwardersReference = "FF123";

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new VerifiedGrossMassDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(verifiedGrossMass);

			AssertUXml(dataObject, expectedXml);
		}

		public void TestPopulateDataObject_SendAllContainers()
		{
			var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			var context = new CommonContext(factory);

			var verifiedGrossMass = new VerifiedGrossMass(
				"ForwardingConsol",
				"C00001000");

			PopulateOrganizations(verifiedGrossMass);

			var containers = new List<VGMMessagingContainer>
			{
				CreateContainer(context, "123", false, isNonOperativeReefer: true),
				CreateContainer(context, "234", false),
				CreateContainer(context, "345", false, isNonOperativeReefer: true),
				CreateContainer(context, "456", false),
			};

			verifiedGrossMass.Containers = containers;
			verifiedGrossMass.CarrierBookingReference = "BK123456";
			verifiedGrossMass.BillOfLadingNumber = "BL123";
			verifiedGrossMass.FreightForwardersReference = "FF123";

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new VerifiedGrossMassDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(verifiedGrossMass);

			AssertUXml(dataObject, expectedUxmlAllContainers);
		}

		public void TestPopulateAttachedDocuments()
		{
			var verifiedGrossMass = new VerifiedGrossMass(
				"ForwardingConsol",
				"C00001000");
			verifiedGrossMass.Containers = new List<VGMMessagingContainer>();

			var document = new DummyDocument();

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new VerifiedGrossMassDataObjectWriter(manager, document);

			verifiedGrossMass.IsRequiredSendAttachment = false;
			using (var dataObject = writer.GetDataObject(verifiedGrossMass))
			{
				AssertNull(dataObject.AttachedDocumentCollection);
			}

			verifiedGrossMass.IsRequiredSendAttachment = true;
			using (var dataObject = writer.GetDataObject(verifiedGrossMass))
			{
				var attachments = dataObject.AttachedDocumentCollection;
				AssertEquals(1, attachments.Count);

				DataObjectWriterHelperTest.AssertPDFAttachedDocumentsFileAttributes(attachments[0], new DataObjectWriterHelper.FileAttributes()
				{
					Name = "Verified Gross Container Weight",
					Description = "Verified Gross Container Weight",
					Code = "VGM",
					IsPublished = false
				});
			}
		}

		#region Implementation

		VGMMessagingContainer CreateContainer(IContext context, string containerNumber, bool shouldSend, bool addSLD = true, bool isNonOperativeReefer = false)
		{
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);

			container.Number = containerNumber;
			container.Type = new ContainerType(context.ContainerTypes)
			{
				ISOCode = "22P1"
			};

			container.ContainerCount = 1;
			container.Seal = "SEAL1";
			container.GrossWeight = new Measurement
			{
				Value = 272,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};

			container.VerifiedStatus = new DummyCodeDescription
			{
				Code = Core.Constants.ContainerGrossWeightVerificationStatuses.Codes.NotSent,
				Description = Core.Constants.ContainerGrossWeightVerificationStatuses.Descriptions.NotSent
			};

			container.VerifiedByAddress = CreateAddress(nameof(container.VerifiedByAddress));
			container.VerifiedDate = new ZDateTime(2018, 6, 10);

			container.IsNonOperativeReefer = isNonOperativeReefer;

			var vgmContainer = new VGMMessagingContainer(container, null)
			{
				ShiLianDan = addSLD
					? "SLD123"
					: "",
				Statement = "this is a statement"
			};

			return vgmContainer;
		}

		void PopulateOrganizations(VerifiedGrossMass verifiedGrossMass)
		{
			verifiedGrossMass.Shipper = CreateAddress(nameof(DocAddressType.ConsignorDocumentaryAddress));
			verifiedGrossMass.Carrier = CreateAddress(nameof(DocAddressType.ShippingLineAddress));
			verifiedGrossMass.Consignee = CreateAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress));
			verifiedGrossMass.FreightForwarder = CreateAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress));
			verifiedGrossMass.CarrierHandlingAgent = CreateAddress(nameof(DocAddressType.CarrierHandlingAgent));
			verifiedGrossMass.CarrierBookingAgent = CreateAddress(nameof(DocAddressType.CarrierBookingAgent));
			verifiedGrossMass.CurrentUser = CreateAddress(nameof(verifiedGrossMass.CurrentUser));
		}

		#region Expected xmls

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

    </DataContext>

    <BookingConfirmationReference>BK123456</BookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <WayBillNumber>BL123</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>CNNBO</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Ningbo</Value>
      </AddInfo>
    </AddInfoCollection>

    <ContainerCollection>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>123</ContainerNumber>
        <ContainerType>
          <Code></Code>
          <Description></Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime>2018-06-10T00:00:00</GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <NonOperatingReefer>true</NonOperatingReefer>
        <Seal>SEAL1</Seal>
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
          <OrganizationAddress>
            <AddressType>GrossWeightVerifiedBy</AddressType>
            <AdditionalAddressInformation>VerifiedByAddress additional info</AdditionalAddressInformation>
            <Address1>VerifiedByAddress address line 1</Address1>
            <Address2>VerifiedByAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>VerifiedByAddress city</City>
            <CompanyName>VerifiedByAddress</CompanyName>
            <Contact>VerifiedByAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>VerifiedByAddress email</Email>
            <Fax>VerifiedByAddress fa</Fax>
            <GovRegNum>VerifiedByAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>VerifiedByAddress ph</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>VerifiedBy</Postcode>
            <State>VerifiedByAddress state</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>234</ContainerNumber>
        <ContainerType>
          <Code></Code>
          <Description></Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime>2018-06-10T00:00:00</GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <NonOperatingReefer>false</NonOperatingReefer>
        <Seal>SEAL1</Seal>
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
          <OrganizationAddress>
            <AddressType>GrossWeightVerifiedBy</AddressType>
            <AdditionalAddressInformation>VerifiedByAddress additional info</AdditionalAddressInformation>
            <Address1>VerifiedByAddress address line 1</Address1>
            <Address2>VerifiedByAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>VerifiedByAddress city</City>
            <CompanyName>VerifiedByAddress</CompanyName>
            <Contact>VerifiedByAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>VerifiedByAddress email</Email>
            <Fax>VerifiedByAddress fa</Fax>
            <GovRegNum>VerifiedByAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>VerifiedByAddress ph</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>VerifiedBy</Postcode>
            <State>VerifiedByAddress state</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>345</ContainerNumber>
        <ContainerType>
          <Code></Code>
          <Description></Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime>2018-06-10T00:00:00</GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <NonOperatingReefer>true</NonOperatingReefer>
        <Seal>SEAL1</Seal>
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
          <OrganizationAddress>
            <AddressType>GrossWeightVerifiedBy</AddressType>
            <AdditionalAddressInformation>VerifiedByAddress additional info</AdditionalAddressInformation>
            <Address1>VerifiedByAddress address line 1</Address1>
            <Address2>VerifiedByAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>VerifiedByAddress city</City>
            <CompanyName>VerifiedByAddress</CompanyName>
            <Contact>VerifiedByAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>VerifiedByAddress email</Email>
            <Fax>VerifiedByAddress fa</Fax>
            <GovRegNum>VerifiedByAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>VerifiedByAddress ph</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>VerifiedBy</Postcode>
            <State>VerifiedByAddress state</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>456</ContainerNumber>
        <ContainerType>
          <Code></Code>
          <Description></Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime>2018-06-10T00:00:00</GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <NonOperatingReefer>false</NonOperatingReefer>
        <Seal>SEAL1</Seal>
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
          <OrganizationAddress>
            <AddressType>GrossWeightVerifiedBy</AddressType>
            <AdditionalAddressInformation>VerifiedByAddress additional info</AdditionalAddressInformation>
            <Address1>VerifiedByAddress address line 1</Address1>
            <Address2>VerifiedByAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>VerifiedByAddress city</City>
            <CompanyName>VerifiedByAddress</CompanyName>
            <Contact>VerifiedByAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>VerifiedByAddress email</Email>
            <Fax>VerifiedByAddress fa</Fax>
            <GovRegNum>VerifiedByAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>VerifiedByAddress ph</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>VerifiedBy</Postcode>
            <State>VerifiedByAddress state</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>

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
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation>ShippingLineAddress additional info</AdditionalAddressInformation>
        <Address1>ShippingLineAddress address line 1</Address1>
        <Address2>ShippingLineAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ShippingLineAddress city</City>
        <CompanyName>ShippingLineAddress</CompanyName>
        <Contact>ShippingLineAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ShippingLineAddress email</Email>
        <Fax>ShippingLineAddress </Fax>
        <GovRegNum>ShippingLineAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ShippingLineAddress </Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ShippingLi</Postcode>
        <State>ShippingLineAddress state</State>

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
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>BookingPartyDocumentaryAddress additional info</AdditionalAddressInformation>
        <Address1>BookingPartyDocumentaryAddress address line 1</Address1>
        <Address2>BookingPartyDocumentaryAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>BookingPartyDocumentaryAddress city</City>
        <CompanyName>BookingPartyDocumentaryAddress</CompanyName>
        <Contact>BookingPartyDocumentaryAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>BookingPartyDocumentaryAddress email</Email>
        <Fax>BookingPartyDocument</Fax>
        <GovRegNum>BookingPartyDocumentaryAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>BookingPartyDocument</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>BookingPar</Postcode>
        <State>BookingPartyDocumentaryAd</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CarrierHandlingAgent</AddressType>
        <AdditionalAddressInformation>CarrierHandlingAgent additional info</AdditionalAddressInformation>
        <Address1>CarrierHandlingAgent address line 1</Address1>
        <Address2>CarrierHandlingAgent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CarrierHandlingAgent city</City>
        <CompanyName>CarrierHandlingAgent</CompanyName>
        <Contact>CarrierHandlingAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CarrierHandlingAgent email</Email>
        <Fax>CarrierHandlingAgent</Fax>
        <GovRegNum>CarrierHandlingAgent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CarrierHandlingAgent</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CarrierHan</Postcode>
        <State>CarrierHandlingAgent stat</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CarrierBookingAgent</AddressType>
        <AdditionalAddressInformation>CarrierBookingAgent additional info</AdditionalAddressInformation>
        <Address1>CarrierBookingAgent address line 1</Address1>
        <Address2>CarrierBookingAgent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CarrierBookingAgent city</City>
        <CompanyName>CarrierBookingAgent</CompanyName>
        <Contact>CarrierBookingAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CarrierBookingAgent email</Email>
        <Fax>CarrierBookingAgent </Fax>
        <GovRegNum>CarrierBookingAgent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CarrierBookingAgent </Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CarrierBoo</Postcode>
        <State>CarrierBookingAgent state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
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

		const string expectedUxmlAllContainers = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

    </DataContext>

    <BookingConfirmationReference>BK123456</BookingConfirmationReference>
    <WayBillNumber>BL123</WayBillNumber>

    <ContainerCollection>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>123</ContainerNumber>
        <ContainerType>
          <Code></Code>
          <Description></Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime>2018-06-10T00:00:00</GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <NonOperatingReefer>true</NonOperatingReefer>
        <Seal>SEAL1</Seal>
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
          <OrganizationAddress>
            <AddressType>GrossWeightVerifiedBy</AddressType>
            <AdditionalAddressInformation>VerifiedByAddress additional info</AdditionalAddressInformation>
            <Address1>VerifiedByAddress address line 1</Address1>
            <Address2>VerifiedByAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>VerifiedByAddress city</City>
            <CompanyName>VerifiedByAddress</CompanyName>
            <Contact>VerifiedByAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>VerifiedByAddress email</Email>
            <Fax>VerifiedByAddress fa</Fax>
            <GovRegNum>VerifiedByAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>VerifiedByAddress ph</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>VerifiedBy</Postcode>
            <State>VerifiedByAddress state</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>234</ContainerNumber>
        <ContainerType>
          <Code></Code>
          <Description></Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime>2018-06-10T00:00:00</GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <NonOperatingReefer>false</NonOperatingReefer>
        <Seal>SEAL1</Seal>
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
          <OrganizationAddress>
            <AddressType>GrossWeightVerifiedBy</AddressType>
            <AdditionalAddressInformation>VerifiedByAddress additional info</AdditionalAddressInformation>
            <Address1>VerifiedByAddress address line 1</Address1>
            <Address2>VerifiedByAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>VerifiedByAddress city</City>
            <CompanyName>VerifiedByAddress</CompanyName>
            <Contact>VerifiedByAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>VerifiedByAddress email</Email>
            <Fax>VerifiedByAddress fa</Fax>
            <GovRegNum>VerifiedByAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>VerifiedByAddress ph</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>VerifiedBy</Postcode>
            <State>VerifiedByAddress state</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>345</ContainerNumber>
        <ContainerType>
          <Code></Code>
          <Description></Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime>2018-06-10T00:00:00</GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <NonOperatingReefer>true</NonOperatingReefer>
        <Seal>SEAL1</Seal>
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
          <OrganizationAddress>
            <AddressType>GrossWeightVerifiedBy</AddressType>
            <AdditionalAddressInformation>VerifiedByAddress additional info</AdditionalAddressInformation>
            <Address1>VerifiedByAddress address line 1</Address1>
            <Address2>VerifiedByAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>VerifiedByAddress city</City>
            <CompanyName>VerifiedByAddress</CompanyName>
            <Contact>VerifiedByAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>VerifiedByAddress email</Email>
            <Fax>VerifiedByAddress fa</Fax>
            <GovRegNum>VerifiedByAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>VerifiedByAddress ph</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>VerifiedBy</Postcode>
            <State>VerifiedByAddress state</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>456</ContainerNumber>
        <ContainerType>
          <Code></Code>
          <Description></Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime>2018-06-10T00:00:00</GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <NonOperatingReefer>false</NonOperatingReefer>
        <Seal>SEAL1</Seal>
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
          <OrganizationAddress>
            <AddressType>GrossWeightVerifiedBy</AddressType>
            <AdditionalAddressInformation>VerifiedByAddress additional info</AdditionalAddressInformation>
            <Address1>VerifiedByAddress address line 1</Address1>
            <Address2>VerifiedByAddress address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>VerifiedByAddress city</City>
            <CompanyName>VerifiedByAddress</CompanyName>
            <Contact>VerifiedByAddress contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>VerifiedByAddress email</Email>
            <Fax>VerifiedByAddress fa</Fax>
            <GovRegNum>VerifiedByAddress tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>VerifiedByAddress ph</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>VerifiedBy</Postcode>
            <State>VerifiedByAddress state</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>

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
        <AddressType>ShippingLineAddress</AddressType>
        <AdditionalAddressInformation>ShippingLineAddress additional info</AdditionalAddressInformation>
        <Address1>ShippingLineAddress address line 1</Address1>
        <Address2>ShippingLineAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ShippingLineAddress city</City>
        <CompanyName>ShippingLineAddress</CompanyName>
        <Contact>ShippingLineAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ShippingLineAddress email</Email>
        <Fax>ShippingLineAddress </Fax>
        <GovRegNum>ShippingLineAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ShippingLineAddress </Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ShippingLi</Postcode>
        <State>ShippingLineAddress state</State>

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
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>BookingPartyDocumentaryAddress additional info</AdditionalAddressInformation>
        <Address1>BookingPartyDocumentaryAddress address line 1</Address1>
        <Address2>BookingPartyDocumentaryAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>BookingPartyDocumentaryAddress city</City>
        <CompanyName>BookingPartyDocumentaryAddress</CompanyName>
        <Contact>BookingPartyDocumentaryAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>BookingPartyDocumentaryAddress email</Email>
        <Fax>BookingPartyDocument</Fax>
        <GovRegNum>BookingPartyDocumentaryAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>BookingPartyDocument</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>BookingPar</Postcode>
        <State>BookingPartyDocumentaryAd</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CarrierHandlingAgent</AddressType>
        <AdditionalAddressInformation>CarrierHandlingAgent additional info</AdditionalAddressInformation>
        <Address1>CarrierHandlingAgent address line 1</Address1>
        <Address2>CarrierHandlingAgent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CarrierHandlingAgent city</City>
        <CompanyName>CarrierHandlingAgent</CompanyName>
        <Contact>CarrierHandlingAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CarrierHandlingAgent email</Email>
        <Fax>CarrierHandlingAgent</Fax>
        <GovRegNum>CarrierHandlingAgent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CarrierHandlingAgent</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CarrierHan</Postcode>
        <State>CarrierHandlingAgent stat</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CarrierBookingAgent</AddressType>
        <AdditionalAddressInformation>CarrierBookingAgent additional info</AdditionalAddressInformation>
        <Address1>CarrierBookingAgent address line 1</Address1>
        <Address2>CarrierBookingAgent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CarrierBookingAgent city</City>
        <CompanyName>CarrierBookingAgent</CompanyName>
        <Contact>CarrierBookingAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CarrierBookingAgent email</Email>
        <Fax>CarrierBookingAgent </Fax>
        <GovRegNum>CarrierBookingAgent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CarrierBookingAgent </Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CarrierBoo</Postcode>
        <State>CarrierBookingAgent state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
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
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		#endregion
	}
}
