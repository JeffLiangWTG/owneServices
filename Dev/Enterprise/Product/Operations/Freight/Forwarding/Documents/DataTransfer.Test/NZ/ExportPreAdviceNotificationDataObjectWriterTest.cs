using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NZ;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.NZ.Testing
{
	sealed class ExportPreAdviceNotificationDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var notification = PrepareTestData();
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new ExportPreAdviceNotificationDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(notification);

			AssertUXml(dataObject, expectedXml);
		}

		protected override void SetUp()
		{
			base.SetUp();
			context = new CommonContext(Factory);
		}

		CommonContext context;

		#region PrepareTestData
		ExportPreAdviceNotification PrepareTestData()
		{
			var notification = new ExportPreAdviceNotification("ForwardingConsol", "C00001001");
			CreateOrganizations(notification);
			CreatePorts(notification);
			notification.BookingConfirmationReference = "AKL300440100";
			notification.CarrierBookingReference = "BK123456";
			notification.FreightForwardersReference = "C00001386";
			notification.Vessel = new Vessel
			{
				Name = "TIANJIN BRIDGE"
			};
			notification.Voyage = "178";

			var modeLookup = new CodeDescriptionPairList();
			modeLookup.AddPair(Core.Constants.TransportModes.Road, "Road");
			notification.PreCarriageMode = new CodeDescription(modeLookup)
			{
				Code = Core.Constants.TransportModes.Road
			};

			notification.Containers = new List<Container>
			{
				CreateContainer("Container1")
			};

			return notification;
		}

		void CreateOrganizations(ExportPreAdviceNotification notification)
		{
			notification.Shipper = CreateAddress(nameof(DocAddressType.ConsignorDocumentaryAddress));
			notification.Carrier = CreateAddress(nameof(DocAddressType.ShippingLineAddress));
			notification.DepartureCTOAddress = CreateAddress(nameof(DocAddressType.DepartureCTOAddress));
		}

		void CreatePorts(ExportPreAdviceNotification notification)
		{
			notification.PortOfLoad = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "NZTRG",
				Name = "Tauranga, New Zealand"
			};

			notification.PortOfDischarge = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "IDJKT",
				Name = "Jakarta, Java, Indonesia"
			};

			notification.Origin = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "NZAKL",
				Name = "Auckland, New Zealand"
			};

			notification.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "NZTRG",
				Name = "Tauranga, New Zealand"
			};
		}
		Container CreateContainer(string number)
		{
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			container.ContainerCount = 1;
			container.Number = number;

			container.Type = new ContainerType(context.ContainerTypes)
			{
				ISOCode = "22P1"
			};

			container.GoodsWeight = new Measurement
			{
				Value = 100,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};

			container.TareWeight = new Measurement
			{
				Value = 200,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};

			container.GrossWeight = new Measurement
			{
				Value = 300,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};

			container.OverhangBack = new Measurement
			{
				Value = 40,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.OverhangFront = new Measurement
			{
				Value = 40,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.OverhangHeight = new Measurement
			{
				Value = 40,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.OverhangLeft = new Measurement
			{
				Value = 40,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.OverhangRight = new Measurement
			{
				Value = 40,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.HSCode = "0807";

			container.Seal = "SEAL1";
			container.SecondSeal = "SEAL2";
			container.ThirdSeal = "SEAL3";

			container.VerifiedByAddress = CreateAddress(nameof(container.VerifiedByAddress));
			container.VerifiedDate = new ZDateTime(2018, 6, 10);
			container.VerifiedMethod = new CodeDescription(new CodeDescriptionPairList())
			{
				Code = "CNT",
				Description = "Method 1 - Container"
			};

			var packLine = new PackingLine(ZGuid.NewZGuid(), Factory);
			packLine.DangerousGoods = new [] { CreateDangerousGood() };
			container.PackingLines = new [] { packLine };
			 
			return container;
		}

		DangerousGood CreateDangerousGood()
		{
			var dangerousGood = new DangerousGood(ZGuid.NewZGuid())
			{
				Contact = new Contact
				{
					FullName = "Test",
					Email = "test@test.com",
					Phone = "+61000"
				},
				Code = "1139b",
				Variant = "C",
				IMOClass = "3",
				PackingGroup = "II",
				State = "Liquid",
				PackageType = new DummyCodeDescription
				{
					Code = "PKG",
					Description = "Package"
				},
				Weight = new Measurement
				{
					Value = 65,
					Unit = new DummyCodeDescription
					{
						Code = "KG",
						Description = "Kilograms"
					}
				},
				Volume = new Measurement
				{
					Value = 1.58,
					Unit = new DummyCodeDescription
					{
						Code = "M3",
						Description = "Cubic Meters"
					}
				},
				ProperShippingName = "COATING SOLUTION",
				Standard = "IMO"
			};
			return dangerousGood;
		}

		#endregion

		#region Expected xmls

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001001</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>

    <BookingConfirmationReference>AKL300440100</BookingConfirmationReference>
    <PortOfDischarge Name=""Jakarta, Java, Indonesia"">IDJKT</PortOfDischarge>
    <PortOfLoading Name=""Tauranga, New Zealand"">NZTRG</PortOfLoading>
    <PortOfOrigin Name=""Auckland, New Zealand"">NZAKL</PortOfOrigin>
    <VesselName>TIANJIN BRIDGE</VesselName>
    <VoyageFlightNo>178</VoyageFlightNo>
    <AddInfoCollection>
      <AddInfo>
        <Key>Other_TransportMode</Key>
        <Value>ROA</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>NZTRG</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Tauranga, New Zealand</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00001386</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <ContainerCollection>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <Commodity>0807</Commodity>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>Container1</ContainerNumber>
        <ContainerType>
          <Code></Code>
          <Description></Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>100</GoodsWeight>
        <GrossWeight>300</GrossWeight>
        <GrossWeightVerificationDateTime>2018-06-10T00:00:00</GrossWeightVerificationDateTime>
        <GrossWeightVerificationType Description=""Method 1 - Container"">CNT</GrossWeightVerificationType>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <NonOperatingReefer>false</NonOperatingReefer>
        <OverhangBack>40</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>40</OverhangHeight>
        <OverhangLeft>40</OverhangLeft>
        <OverhangRight>40</OverhangRight>
        <Seal>SEAL1</Seal>
        <SecondSeal>SEAL2</SecondSeal>
        <TareWeight>200</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <WeightUnit>KG</WeightUnit>
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
        <UNDGCollection>
          <UNDG>
            <Contact>
              <FullName>Test</FullName>
              <Email>test@test.com</Email>
              <Phone>+61000</Phone>
            </Contact>
            <IMOClass>3</IMOClass>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup>II</PackingGroup>
            <PackQty>0</PackQty>
            <PackType Description=""Package"">PKG</PackType>
            <ProperShippingName>COATING SOLUTION</ProperShippingName>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName></TechicalName>
            <UNDGCode>1139b</UNDGCode>
            <Volume>1.58</Volume>
            <VolumeUQ Description=""Cubic Meters"">M3</VolumeUQ>
            <Weight>65</Weight>
            <WeightUQ Description=""Kilograms"">KG</WeightUQ>
          </UNDG>
        </UNDGCollection>
      </Container>
    </ContainerCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
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
            <Type>PSN</Type>
            <CountryOfIssue>NZ</CountryOfIssue>
            <Value></Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
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
            <Type>CCC</Type>
            <CountryOfIssue>US</CountryOfIssue>
            <Value></Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
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
            <Type>PSN</Type>
            <CountryOfIssue>NZ</CountryOfIssue>
            <Value></Value>
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
