using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR.Testing
{
	sealed class ContainerAdviceToBookingWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var hasFlashPoint = true;
				var containerAdviceToBooking = PrepareTestData(hasFlashPoint);

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new ContainerAdviceToBookingWriter(manager);

				var dataObject = writer.GetDataObject(containerAdviceToBooking);

				var flashPoint = "<FlashPoint>10</FlashPoint>".PadLeft(39, ' ');
				var expectedXml = GetExpectedXml(flashPoint);

				AssertUXml(dataObject, expectedXml);

				hasFlashPoint = false;
				containerAdviceToBooking = PrepareTestData(hasFlashPoint);

				writer = new ContainerAdviceToBookingWriter(manager);

				dataObject = writer.GetDataObject(containerAdviceToBooking);

				flashPoint = null;
				expectedXml = GetExpectedXml(flashPoint);

				AssertUXml(dataObject, expectedXml);
			}
		}

		ContainerAdviceToBooking PrepareTestData(bool hasFlashPoint)
		{
			var containerAdviceToBooking = new ContainerAdviceToBooking("ForwardingConsol", "C00001001");

			containerAdviceToBooking.RequiresTemperatureControl = true;
			containerAdviceToBooking.TemperatureMaximum = new Measurement
			{
				Value = 12,
				Unit = new DummyCodeDescription()
				{
					Code = "C"
				}
			};

			containerAdviceToBooking.TemperatureMinimum = new Measurement
			{
				Value = 20,
				Unit = new DummyCodeDescription()
				{
					Code = "C"
				}
			};

			containerAdviceToBooking.Carrier = CreateAddress("Carrier");
			containerAdviceToBooking.CarrierBookingAgent = CreateAddress("CarrierBookingAgent");
			containerAdviceToBooking.CTO = CreateAddress("CTO");
			containerAdviceToBooking.Transporter = CreateAddress("Transporter");
			containerAdviceToBooking.SendingParty = CreateAddress("SendingParty");
			containerAdviceToBooking.Forwarder = CreateAddress("Forwarder");
			containerAdviceToBooking.SendingForwarder = CreateAddress("SendingForwarder");
			containerAdviceToBooking.ReceivingForwarder = CreateAddress("ReceivingForwarder");

			containerAdviceToBooking.CarrierBookingReference = "C0005698";
			containerAdviceToBooking.BookingConfirmationCBK = "B00001";
			containerAdviceToBooking.OTC = "OTCAP01";
			containerAdviceToBooking.ATPReference = "ATPAP01";
			containerAdviceToBooking.OTCReference = "OTCREF01";
			containerAdviceToBooking.PortLocation = "AUCKD";
			containerAdviceToBooking.PortArea = "AKD";
			containerAdviceToBooking.ConsolNumber = "C00001001";

			containerAdviceToBooking.CarrierSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S001");
			containerAdviceToBooking.CarrierCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C001");

			containerAdviceToBooking.CarrierBookingAgentSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S002");
			containerAdviceToBooking.CarrierBookingAgentCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C002");

			containerAdviceToBooking.CTOSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S003");
			containerAdviceToBooking.CTOCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C003");

			containerAdviceToBooking.TransporterSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S004");
			containerAdviceToBooking.TransporterCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C004");

			containerAdviceToBooking.SendingPartyCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C005");
			containerAdviceToBooking.SendingPartySOA = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOA, "SOA5");
			containerAdviceToBooking.SendingPartySON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "SON5");
			containerAdviceToBooking.SendingPartySOW = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOW, "SOW5");

			containerAdviceToBooking.SendingForwarderSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S006");
			containerAdviceToBooking.SendingForwarderCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C006");

			containerAdviceToBooking.ReceivingForwarderSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S007");
			containerAdviceToBooking.ReceivingForwarderCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C007");

			containerAdviceToBooking.ForwarderCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C008");
			containerAdviceToBooking.ForwarderSOA = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOA, "SOA8");
			containerAdviceToBooking.ForwarderSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "SON8");
			containerAdviceToBooking.ForwarderSOW = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOW, "SOW8");

			containerAdviceToBooking.ContainerMode = new DummyCodeDescription()
			{
				Code = Core.Constants.ContainerModes.FCL
			};

			containerAdviceToBooking.ShipmentType = new DummyCodeDescription()
			{
				Code = Core.Constants.AgentType.Agent
			};

			containerAdviceToBooking.PortOfOrigin = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRPAR",
				Name = "Paris"
			};

			containerAdviceToBooking.PortOfTranshipment = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUBNE",
				Name = "Brisbane"
			};

			containerAdviceToBooking.PortOfDestination = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};

			containerAdviceToBooking.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRPAR",
				Name = "Paris"
			};

			containerAdviceToBooking.VoyageNumber = "123456";

			var container1 = CreateContainer(context, "AAA");
			var container2 = CreateContainer(context, "BBB");

			var packline1 = CreatePackingLine("AAA packline 1", "AAA", hasFlashPoint);
			var packline2 = CreatePackingLine("AAA packline 2", "AAA", hasFlashPoint);
			var packline3 = CreatePackingLine("BBB packline 1", "BBB", hasFlashPoint);

			container1.PackingLines = new[]
			{
				packline1,
				packline2
			};

			container2.PackingLines = new[]
			{
				packline3
			};

			containerAdviceToBooking.Containers = new[]
			{
				container1,
				container2
			};

			return containerAdviceToBooking;
		}

		RegistrationNumber CrateRegistrationNumber(string type, string value)
		{
			return new RegistrationNumber()
			{
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.France
				},
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.France))
				{
					Code = type
				},
				Value = value
			};
		}

		BookingContainer CreateContainer(IContext context, string containerNumber)
		{
			var container = new BookingContainer(DefaultDataObjectWriterStrategy.TestInstance);

			container.Number = containerNumber;
			container.Type = new ContainerType(context.ContainerTypes)
			{
				Code = "20FR"
			};

			container.ContainerCount = 1;
			container.PackCount = 3;
			container.IsEmpty = false;
			container.Seal = "SEAL1";
			container.DepartureSlotDateTime = new ZDateTime(2019, 11, 23);
			container.Mode = new CodeDescription(new CodeDescriptionPairList()) { Code = "FCL" };
			container.DeliveryMode = "CY/CY";
			container.HandlingNotes = "goods handling instructions";
			container.Fumigated = true;
			container.OversizeContainer = true;
			container.HazardousCargo = true;
			container.MarinePollutant = true;
			container.AMQReference = "AMQRef";
			container.ECTReference = "E00001";
			container.IsNonOperativeReefer = false;

			container.SealPartyType = new DummyCodeDescription
			{
				Code = "CAR",
				Description = "Carrier"
			};

			container.TransportMode = new CodeDescription(new CodeDescriptionPairList())
			{
				Code = "RTE"
			};

			container.GoodsWeight = new Measurement()
			{
				Value = 200,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.TareWeight = new Measurement()
			{
				Value = 20,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};

			container.OverhangBack = new Measurement()
			{
				Value = 39,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.OverhangFront = new Measurement()
			{
				Value = 40,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.OverhangHeight = new Measurement()
			{
				Value = 41,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.OverhangLeft = new Measurement()
			{
				Value = 42,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.OverhangRight = new Measurement()
			{
				Value = 43,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			container.FumigationService = new AdditionalService
			{
				Contractor = CreateAddress("Contractor"),
				ServiceNote = "service note"
			};

			return container;
		}

		BookingPackingLine CreatePackingLine(string goodsDescription, string containerNumber, bool hasFlashPoint)
		{
			var packingLine = new BookingPackingLine(ZGuid.NewZGuid());

			packingLine.PackingLineID = "packID";
			packingLine.ContainerNumber = containerNumber;
			packingLine.Quantity = 3;
			packingLine.PackageType = new DummyCodeDescription
			{
				Code = "PLT",
				Description = "Pallet"
			};

			packingLine.Weight = new Measurement()
			{
				Value = 88,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};

			packingLine.GoodsDescription = goodsDescription;
			packingLine.MarksAndNumbers = "marks & nums";

			var dangerousGoods = new List<DangerousGood>();

			var dangerousGood = new DangerousGood()
			{
				Code = "0001C",
				Unno = "0001",
				Quantity = 11,
				Variant = "C",
				ProperShippingName = "Danger",
				TechnicalName = "Technicals",
				IMOClass = "A",
				PackedInLimitedQuantity = true,
				State = "S",
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

			dangerousGoods.Add(dangerousGood);
			packingLine.DangerousGoods = dangerousGoods;

			packingLine.RequiresTemperatureControl = true;
			packingLine.TemperatureMaximum = new Measurement
			{
				Value = 12,
				Unit = new DummyCodeDescription()
				{
					Code = "C"
				}
			};

			packingLine.TemperatureMinimum = new Measurement
			{
				Value = 20,
				Unit = new DummyCodeDescription()
				{
					Code = "C"
				}
			};

			return packingLine;
		}

		string GetExpectedXml(string flashpoint) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001001</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

    </DataContext>
    <BookingConfirmationReference>C0005698</BookingConfirmationReference>
    <ContainerMode>FCL</ContainerMode>
    <PortFirstForeign Name=""Brisbane"">AUBNE</PortFirstForeign>
    <PortOfDestination Name=""Sydney"">AUSYD</PortOfDestination>
    <PortOfOrigin Name=""Paris"">FRPAR</PortOfOrigin>
    <RequiredTemperatureMaximum>12</RequiredTemperatureMaximum>
    <RequiredTemperatureMinimum>20</RequiredTemperatureMinimum>
    <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
    <RequiresTemperatureControl>true</RequiresTemperatureControl>
    <ShipmentType>AGT</ShipmentType>
    <VoyageFlightNo>123456</VoyageFlightNo>
    <WayBillNumber></WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>PortLocation</Key>
        <Value>AUCKD</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortArea</Key>
        <Value>AKD</Value>
      </AddInfo>
      <AddInfo>
        <Key>BookingConfirmationCBK</Key>
        <Value>B00001</Value>
      </AddInfo>
      <AddInfo>
        <Key>OTC</Key>
        <Value>OTCAP01</Value>
      </AddInfo>
      <AddInfo>
        <Key>ATPReference</Key>
        <Value>ATPAP01</Value>
      </AddInfo>
      <AddInfo>
        <Key>OTCReference</Key>
        <Value>OTCREF01</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>FRPAR</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Paris</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>1.0.0</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00001001</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <ContainerCollection>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAA</ContainerNumber>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DeliveryMode>CY/CY</DeliveryMode>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DepartureSlotDateTime>2019-11-23T00:00:00</DepartureSlotDateTime>
        <EmptyRequired></EmptyRequired>
        <FCL_LCL_AIR>FCL</FCL_LCL_AIR>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal></SecondSeal>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Fumigated</Key>
            <Value>Y</Value>
          </AddInfo>
          <AddInfo>
            <Key>OversizeContainer</Key>
            <Value>Y</Value>
          </AddInfo>
          <AddInfo>
            <Key>HazardousCargo</Key>
            <Value>Y</Value>
          </AddInfo>
          <AddInfo>
            <Key>MarinePollutant</Key>
            <Value>Y</Value>
          </AddInfo>
          <AddInfo>
            <Key>HandlingNotes</Key>
            <Value>goods handling instructions</Value>
          </AddInfo>
          <AddInfo>
            <Key>TransportMode</Key>
            <Value>RTE</Value>
          </AddInfo>
          <AddInfo>
            <Key>AMQReference</Key>
            <Value>AMQRef</Value>
          </AddInfo>
          <AddInfo>
            <Key>ECTReference</Key>
            <Value>E00001</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>
        <AdditionalServiceCollection>
          <AdditionalService>
            <Booked></Booked>
            <Completed></Completed>
            <Contractor>
              <AddressType>Contractor</AddressType>
              <AdditionalAddressInformation>Contractor additional info</AdditionalAddressInformation>
              <Address1>Contractor address line 1</Address1>
              <Address2>Contractor address line 2</Address2>
              <AddressOverride>false</AddressOverride>
              <City>Contractor city</City>
              <CompanyName>Contractor</CompanyName>
              <Contact>Contractor contact</Contact>
              <Country Name=""Australia"">AU</Country>
              <Email>Contractor email</Email>
              <Fax>Contractor fax</Fax>
              <GovRegNum>Contractor tax number</GovRegNum>
              <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
              <Phone>Contractor phone</Phone>
              <Port Name=""Sydney"">AUSYD</Port>
              <Postcode>Contractor</Postcode>
              <State>Contractor state</State>
              <RegistrationNumberCollection>
                <RegistrationNumber>
                  <Type Description=""AAA desc"">AAA</Type>
                  <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                  <Value>12345</Value>
                </RegistrationNumber>
              </RegistrationNumberCollection>
            </Contractor>
            <Duration></Duration>
            <References></References>
            <ServiceCount>0</ServiceCount>
            <ServiceNote>service note</ServiceNote>
          </AdditionalService>
        </AdditionalServiceCollection>
      </Container>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>BBB</ContainerNumber>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DeliveryMode>CY/CY</DeliveryMode>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DepartureSlotDateTime>2019-11-23T00:00:00</DepartureSlotDateTime>
        <EmptyRequired></EmptyRequired>
        <FCL_LCL_AIR>FCL</FCL_LCL_AIR>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal></SecondSeal>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Fumigated</Key>
            <Value>Y</Value>
          </AddInfo>
          <AddInfo>
            <Key>OversizeContainer</Key>
            <Value>Y</Value>
          </AddInfo>
          <AddInfo>
            <Key>HazardousCargo</Key>
            <Value>Y</Value>
          </AddInfo>
          <AddInfo>
            <Key>MarinePollutant</Key>
            <Value>Y</Value>
          </AddInfo>
          <AddInfo>
            <Key>HandlingNotes</Key>
            <Value>goods handling instructions</Value>
          </AddInfo>
          <AddInfo>
            <Key>TransportMode</Key>
            <Value>RTE</Value>
          </AddInfo>
          <AddInfo>
            <Key>AMQReference</Key>
            <Value>AMQRef</Value>
          </AddInfo>
          <AddInfo>
            <Key>ECTReference</Key>
            <Value>E00001</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>
        <AdditionalServiceCollection>
          <AdditionalService>
            <Booked></Booked>
            <Completed></Completed>
            <Contractor>
              <AddressType>Contractor</AddressType>
              <AdditionalAddressInformation>Contractor additional info</AdditionalAddressInformation>
              <Address1>Contractor address line 1</Address1>
              <Address2>Contractor address line 2</Address2>
              <AddressOverride>false</AddressOverride>
              <City>Contractor city</City>
              <CompanyName>Contractor</CompanyName>
              <Contact>Contractor contact</Contact>
              <Country Name=""Australia"">AU</Country>
              <Email>Contractor email</Email>
              <Fax>Contractor fax</Fax>
              <GovRegNum>Contractor tax number</GovRegNum>
              <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
              <Phone>Contractor phone</Phone>
              <Port Name=""Sydney"">AUSYD</Port>
              <Postcode>Contractor</Postcode>
              <State>Contractor state</State>
              <RegistrationNumberCollection>
                <RegistrationNumber>
                  <Type Description=""AAA desc"">AAA</Type>
                  <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                  <Value>12345</Value>
                </RegistrationNumber>
              </RegistrationNumberCollection>
            </Contractor>
            <Duration></Duration>
            <References></References>
            <ServiceCount>0</ServiceCount>
            <ServiceNote>service note</ServiceNote>
          </AdditionalService>
        </AdditionalServiceCollection>
      </Container>
    </ContainerCollection>
    <OrganizationAddressCollection>
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
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C006</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>S006</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ReceivingForwarderAddress</AddressType>
        <AdditionalAddressInformation>ReceivingForwarder additional info</AdditionalAddressInformation>
        <Address1>ReceivingForwarder address line 1</Address1>
        <Address2>ReceivingForwarder address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ReceivingForwarder city</City>
        <CompanyName>ReceivingForwarder</CompanyName>
        <Contact>ReceivingForwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ReceivingForwarder email</Email>
        <Fax>ReceivingForwarder f</Fax>
        <GovRegNum>ReceivingForwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ReceivingForwarder p</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ReceivingF</Postcode>
        <State>ReceivingForwarder state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C007</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>S007</Value>
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
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C001</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>S001</Value>
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
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C002</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>S002</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCTOAddress</AddressType>
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
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C003</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>S003</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCFSLocalTransportAddress</AddressType>
        <AdditionalAddressInformation>Transporter additional info</AdditionalAddressInformation>
        <Address1>Transporter address line 1</Address1>
        <Address2>Transporter address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Transporter city</City>
        <CompanyName>Transporter</CompanyName>
        <Contact>Transporter contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Transporter email</Email>
        <Fax>Transporter fax</Fax>
        <GovRegNum>Transporter tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Transporter phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Transporte</Postcode>
        <State>Transporter state</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C004</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>S004</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
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

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C005</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Agent C"">SOA</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SOA5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SON5</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Warehou"">SOW</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SOW5</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation>Forwarder additional info</AdditionalAddressInformation>
        <Address1>Forwarder address line 1</Address1>
        <Address2>Forwarder address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Forwarder city</City>
        <CompanyName>Forwarder</CompanyName>
        <Contact>Forwarder contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Forwarder email</Email>
        <Fax>Forwarder fax</Fax>
        <GovRegNum>Forwarder tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Forwarder phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Forwarder </Postcode>
        <State>Forwarder state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C008</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Agent C"">SOA</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SOA8</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SON8</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Warehou"">SOW</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SOW8</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection>
      <PackingLine>
        <ContainerLink>1</ContainerLink>
        <ContainerNumber>AAA</ContainerNumber>
        <DetailedDescription>AAA packline 1</DetailedDescription>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription>AAA packline 1</GoodsDescription>
        <ImportReferenceNumber></ImportReferenceNumber>
        <MarksAndNos>marks &amp; nums</MarksAndNos>
        <PackingLineID>packID</PackingLineID>
        <PackQty>3</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber></ReferenceNumber>
        <RequiredTemperatureMaximum>12</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>20</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>true</RequiresTemperatureControl>
        <Weight>88</Weight>
        <WeightUnit>KG</WeightUnit>
        <UNDGCollection>
          <UNDG>
{flashpoint}
            <IMOClass>A</IMOClass>
            <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
            <PackingGroup></PackingGroup>
            <PackQty>11</PackQty>
            <ProperShippingName>Danger</ProperShippingName>
            <State>Solid</State>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName>Technicals</TechicalName>
            <UNDGCode>0001</UNDGCode>
          </UNDG>
        </UNDGCollection>
      </PackingLine>
      <PackingLine>
        <ContainerLink>1</ContainerLink>
        <ContainerNumber>AAA</ContainerNumber>
        <DetailedDescription>AAA packline 2</DetailedDescription>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription>AAA packline 2</GoodsDescription>
        <ImportReferenceNumber></ImportReferenceNumber>
        <MarksAndNos>marks &amp; nums</MarksAndNos>
        <PackingLineID>packID</PackingLineID>
        <PackQty>3</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber></ReferenceNumber>
        <RequiredTemperatureMaximum>12</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>20</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>true</RequiresTemperatureControl>
        <Weight>88</Weight>
        <WeightUnit>KG</WeightUnit>
        <UNDGCollection>
          <UNDG>
{flashpoint}
            <IMOClass>A</IMOClass>
            <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
            <PackingGroup></PackingGroup>
            <PackQty>11</PackQty>
            <ProperShippingName>Danger</ProperShippingName>
            <State>Solid</State>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName>Technicals</TechicalName>
            <UNDGCode>0001</UNDGCode>
          </UNDG>
        </UNDGCollection>
      </PackingLine>
      <PackingLine>
        <ContainerLink>2</ContainerLink>
        <ContainerNumber>BBB</ContainerNumber>
        <DetailedDescription>BBB packline 1</DetailedDescription>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription>BBB packline 1</GoodsDescription>
        <ImportReferenceNumber></ImportReferenceNumber>
        <MarksAndNos>marks &amp; nums</MarksAndNos>
        <PackingLineID>packID</PackingLineID>
        <PackQty>3</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber></ReferenceNumber>
        <RequiredTemperatureMaximum>12</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>20</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>true</RequiresTemperatureControl>
        <Weight>88</Weight>
        <WeightUnit>KG</WeightUnit>
        <UNDGCollection>
          <UNDG>
{flashpoint}
            <IMOClass>A</IMOClass>
            <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
            <PackingGroup></PackingGroup>
            <PackQty>11</PackQty>
            <ProperShippingName>Danger</ProperShippingName>
            <State>Solid</State>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName>Technicals</TechicalName>
            <UNDGCode>0001</UNDGCode>
          </UNDG>
        </UNDGCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";

		protected override void SetUp()
		{
			base.SetUp();
			context = new CommonContext(Factory);
		}

		CommonContext context;
	}
}
