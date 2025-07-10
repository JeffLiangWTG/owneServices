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
	sealed class FinalManifestWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var hasFlashPoint = true;
				var finalManifest = PrepareTestData(hasFlashPoint);

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new FinalManifestWriter(manager);

				var dataObject = writer.GetDataObject(finalManifest);

				var flashPoint = "<FlashPoint>-5</FlashPoint>".PadLeft(43, ' ');
				var expectedXml = GetExpectedXml(flashPoint);

				AssertUXml(dataObject, expectedXml);

				hasFlashPoint = false;
				finalManifest = PrepareTestData(hasFlashPoint);

				writer = new FinalManifestWriter(manager);

				dataObject = writer.GetDataObject(finalManifest);

				flashPoint = null;
				expectedXml = GetExpectedXml(flashPoint);

				AssertUXml(dataObject, expectedXml);
			}
		}

		FinalManifest PrepareTestData(bool hasFlashPoint)
		{
			var finalManifest = new FinalManifest("ForwardingConsol", "C00001266");
			finalManifest.TemperatureMaximum = new Measurement
			{
				Value = -2,
				Unit = new DummyCodeDescription()
				{
					Code = "C"
				}
			};

			finalManifest.TemperatureMinimum = new Measurement
			{
				Value = -5,
				Unit = new DummyCodeDescription()
				{
					Code = "C",
					Description = "Centigrade"
				}
			};

			finalManifest.VoyageFlightNo = "029N";
			finalManifest.Carrier = CreateAddress("Carrier");
			finalManifest.SendingParty = CreateAddress("SendingParty");
			finalManifest.CurrentUser = CreateAddress("CurrentUser");
			finalManifest.ReceivingForwarder = CreateAddress("ReceivingForwarder");
			finalManifest.Transporter = CreateAddress("Transporter");
			finalManifest.OTCReference = "OTCREF";
			finalManifest.ATPReference = "ATPREF";
			finalManifest.CBKReference = "CBKREF";
			finalManifest.BookingConfirmation = "CBKREFERENCE";
			finalManifest.VoyageServiceCode = "SER00002";
			finalManifest.PortLocation = "PORTLOC";
			finalManifest.PortArea = "PORTAREA";
			finalManifest.DeliveryLocation = "DELLOC";
			finalManifest.DeliveryArea = "DELAREA";
			finalManifest.ConsolNumber = "C00001266";
			finalManifest.Vessel = "MAERSK EMDEN";
			finalManifest.CarrierBookingRef = "CARRIERBOOKINGREF";
			finalManifest.VehicleRegistration = "V0000023";
			//finalManifest.PackingFunctionalReference = "PACKREF";

			finalManifest.PortOfOrigin = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "LKCMB",
				Name = "Colombo"
			};

			finalManifest.PortOfDestination = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "INKRI",
				Name = "Krishnapatnam"
			};

			finalManifest.ReceivingForwarderSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S002");
			finalManifest.ReceivingForwarderCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C002");

			finalManifest.TransporterSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S003");
			finalManifest.TransporterCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C003");

			finalManifest.SendingPartyCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C004");
			finalManifest.SendingPartySOA = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOA, "SOA4");
			finalManifest.SendingPartySON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "SON4");
			finalManifest.SendingPartySOW = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOW, "SOW4");

			finalManifest.CarrierSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S005");
			finalManifest.CarrierCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C005");

			finalManifest.CurrentUserCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C006");
			finalManifest.CurrentUserSOA = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOA, "SOA6");
			finalManifest.CurrentUserSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "SON6");
			finalManifest.CurrentUserSOW = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOW, "SOW6");

			finalManifest.ExpectedArrivalAtPort = new ZDate(2020, 07, 20);

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

			finalManifest.Containers = new[]
			{
				container1
			};

			var goodsDetail = CreateGoodsDetail();
			goodsDetail.PackingLines = new[] {
				packline1
			};

			finalManifest.GoodsDetails = new[] { goodsDetail };

			finalManifest.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRMRS",
				Name = "Marseille"
			};

			finalManifest.ShipmentType = new DummyCodeDescription()
			{
				Code = Core.Constants.AgentType.Agent,
				Description = "Agent"
			};

			finalManifest.ContainerMode = new DummyCodeDescription()
			{
				Code = Core.Constants.ContainerModes.FCL,
				Description = "Full Container Load"
			};

			finalManifest.TransportMode = "RTE1";

			return finalManifest;
		}

		GoodsDetail CreateGoodsDetail()
		{
			var goodsDetail = new GoodsDetail(ZGuid.NewZGuid());
			goodsDetail.ShipmentNumber = "S00001266";
			goodsDetail.ConsignmentNumber = "ERC001";
			goodsDetail.HazardousCargo = false;
			goodsDetail.AppliesToAllPacks = true;
			goodsDetail.DeclaredPackCount = 10;
			goodsDetail.DeclaredPackWeight = new Measurement { Value = 250.5 };
			goodsDetail.DeclarationReferenceNumber = "DEC000098";
			goodsDetail.CommodityReference = "ICV0000001";
			goodsDetail.CustomsStatus = "Clear";
			goodsDetail.ShowAppliesToAllPacks = true;

			return goodsDetail;
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
				Code = "40HC"
			};

			container.ContainerCount = 1;
			container.PackCount = 3;
			container.IsEmpty = false;
			container.Seal = "MLLK0271601";
			container.HandlingNotes = "Unpacking Notes";
			container.PackingReference = "PACKREF";
			container.Fumigated = false;
			container.OversizeContainer = false;
			container.HazardousCargo = false;
			container.MarinePollutant = false;
			container.SecondSealPartyType = new DummyCodeDescription();
			container.ThirdSealPartyType = null;
			container.OverhangBack = new Measurement { Value = 0.000 };
			container.OverhangFront = new Measurement { Value = 0.000 };
			container.OverhangHeight = new Measurement { Value = 0.000 };
			container.OverhangLeft = new Measurement { Value = 0.000 };
			container.OverhangRight = new Measurement { Value = 0.000 };
			container.SetTemperature = new Measurement { Value = 1.000, Unit = new DummyCodeDescription() };

			container.SealPartyType = new DummyCodeDescription
			{
				Code = "CAR",
				Description = "Carrier/Shipping Line"
			};

			var contractorAddress = CreateAddress("Contractor");
			contractorAddress.RegistrationNumbers = new[]
			{
				CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C006"),
				CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S006")
			};
			container.FumigationService = new AdditionalService
			{
				Contractor = contractorAddress,
				ServiceNote = "SERVICE NOTES",
				ServiceCode = new DummyCodeDescription
				{
					Code = "FUM",
					Description = "Fumigation"
				}
			};

			container.GrossWeight = new Measurement()
			{
				Value = 7801.800,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.TareWeight = new Measurement()
			{
				Value = 3950.000,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};

			container.TransportMode = new CodeDescription(new CodeDescriptionPairList())
			{
				Code = "RTE"
			};

			container.VehicleRegistration = "V0000023";
			container.AMQAPPlusID = "AMQ000039";
			container.AMQReference = "C00003844";
			container.LDEReference = "LDE000011";
			container.LDEIsFinal = true;

			container.PortDuesPortCode = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRMRS",
				Name = "Marseille"
			};

			container.PortDuesAmount = 10000;
			container.PortDuesCurrency = new DummyCodeDescription() { Code = "EUR" };
			container.PortDuesPayingParty = "BOLLORE";
			container.IsNonOperativeReefer = false;

			return container;
		}

		BookingPackingLine CreatePackingLine(string goodsDescription, string containerNumber, bool hasFlashPoint)
		{
			var packingLine = new BookingPackingLine(ZGuid.NewZGuid());

			packingLine.PackingLineID = "packID";
			packingLine.GoodsDescription = goodsDescription;
			packingLine.ContainerNumber = containerNumber;
			packingLine.Quantity = 225;
			packingLine.PackageType = new DummyCodeDescription
			{
				Code = "PKG",
				Description = "Package"
			};

			packingLine.Weight = new Measurement()
			{
				Value = 7801.800,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			packingLine.Volume = new Measurement
			{
				Value = 37.350,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};

			var dangerousGoods = new List<DangerousGood>();
			var dangerousGood = new DangerousGood()
			{
				Code = "1266a",
				Unno = "1266a",
				Variant = "C",
				ProperShippingName = "PERFUMERY PRODUCTS",
				IMOClass = "3",
				PackedInLimitedQuantity = true,
				PackingGroup = "II",
				MarinePollutant = new DummyCodeDescription() { Code = "true" },
				State = "G",
			};

			if (hasFlashPoint)
			{
				dangerousGood.FlashPoint = new Measurement
				{
					Value = -5,
					Unit = new DummyCodeDescription() { Code = "C" }
				};
			}

			dangerousGoods.Add(dangerousGood);

			packingLine.DangerousGoods = dangerousGoods;

			return packingLine;
		}

		string GetExpectedXml(string flashpoint) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001266</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

    </DataContext>

    <BookingConfirmationReference>CARRIERBOOKINGREF</BookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <PortOfDestination Name=""Krishnapatnam"">INKRI</PortOfDestination>
    <PortOfOrigin Name=""Colombo"">LKCMB</PortOfOrigin>
    <RequiredTemperatureMaximum>-2</RequiredTemperatureMaximum>
    <RequiredTemperatureMinimum>-5</RequiredTemperatureMinimum>
    <RequiredTemperatureUnit Description=""Centigrade"">C</RequiredTemperatureUnit>
    <RequiresTemperatureControl>true</RequiresTemperatureControl>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <VesselName>MAERSK EMDEN</VesselName>
    <VoyageFlightNo>029N</VoyageFlightNo>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>FRMRS</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Marseille</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortLocation</Key>
        <Value>PORTLOC</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortArea</Key>
        <Value>PORTAREA</Value>
      </AddInfo>
      <AddInfo>
        <Key>ATP</Key>
        <Value>ATPREF</Value>
      </AddInfo>
      <AddInfo>
        <Key>OTC</Key>
        <Value>OTCREF</Value>
      </AddInfo>
      <AddInfo>
        <Key>CBK</Key>
        <Value>CBKREF</Value>
      </AddInfo>
      <AddInfo>
        <Key>VoyageServiceCode</Key>
        <Value>SER00002</Value>
      </AddInfo>
      <AddInfo>
        <Key>BookingConfirmationCBK</Key>
        <Value>CBKREFERENCE</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>1.0.0</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00001266</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <ContainerCollection>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAA</ContainerNumber>
        <ContainerType>
          <Code>40HC</Code>
          <Category Description=""Dry Storage"">DRY</Category>
          <Description>Forty foot high cube</Description>
          <ISOCode>45G0</ISOCode>
        </ContainerType>
        <DeliveryMode></DeliveryMode>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DepartureSlotDateTime></DepartureSlotDateTime>
        <EmptyRequired></EmptyRequired>
        <GrossWeight>7801.8</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <OverhangBack>0</OverhangBack>
        <OverhangFront>0</OverhangFront>
        <OverhangHeight>0</OverhangHeight>
        <OverhangLeft>0</OverhangLeft>
        <OverhangRight>0</OverhangRight>
        <Seal>MLLK0271601</Seal>
        <SealPartyType Description=""Carrier/Shipping Line"">CAR</SealPartyType>
        <SecondSeal></SecondSeal>
        <SecondSealPartyType></SecondSealPartyType>
        <SetPointTemp>1</SetPointTemp>
        <SetPointTempUnit></SetPointTempUnit>
        <TareWeight>3950</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal></ThirdSeal>

        <AddInfoCollection>
          <AddInfo>
            <Key>Fumigated</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>OversizeContainer</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>HazardousCargo</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>MarinePollutant</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>LDEAPPlusID</Key>
            <Value>LDE000011</Value>
          </AddInfo>
          <AddInfo>
            <Key>LDEStatus</Key>
            <Value>VAL</Value>
          </AddInfo>
          <AddInfo>
            <Key>DeliveryLocation</Key>
            <Value>DELLOC</Value>
          </AddInfo>
          <AddInfo>
            <Key>DeliveryArea</Key>
            <Value>DELAREA</Value>
          </AddInfo>
          <AddInfo>
            <Key>HandlingNotes</Key>
            <Value>Unpacking Notes</Value>
          </AddInfo>
          <AddInfo>
            <Key>DateOfArrival</Key>
            <Value>20-Jul-20 00:00:00</Value>
          </AddInfo>
          <AddInfo>
            <Key>TransportMode</Key>
            <Value>RTE1</Value>
          </AddInfo>
          <AddInfo>
            <Key>VehicleRegistration</Key>
            <Value>V0000023</Value>
          </AddInfo>
          <AddInfo>
            <Key>PackingReference</Key>
            <Value>PACKREF</Value>
          </AddInfo>
          <AddInfo>
            <Key>AMQReference</Key>
            <Value>C00003844</Value>
          </AddInfo>
          <AddInfo>
            <Key>AMQAPPlusID</Key>
            <Value>AMQ000039</Value>
          </AddInfo>
          <AddInfo>
            <Key>PortDuesPortCode</Key>
            <Value>FRMRS</Value>
          </AddInfo>
          <AddInfo>
            <Key>PortDuesAmount</Key>
            <Value>10000</Value>
          </AddInfo>
          <AddInfo>
            <Key>PortDuesCurrency</Key>
            <Value>EUR</Value>
          </AddInfo>
          <AddInfo>
            <Key>PortDuesPayingParty</Key>
            <Value>BOLLORE</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>

        <AdditionalServiceCollection>
          <AdditionalService>
            <ServiceCode Description=""Fumigation"">FUM</ServiceCode>
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
            </Contractor>
            <Duration></Duration>
            <References></References>
            <ServiceCount>0</ServiceCount>
            <ServiceNote>SERVICE NOTES</ServiceNote>
          </AdditionalService>
        </AdditionalServiceCollection>
      </Container>
    </ContainerCollection>

    <OrganizationAddressCollection>
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
            <Value>C004</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Agent C"">SOA</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SOA4</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SON4</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Warehou"">SOW</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SOW4</Value>
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
            <Value>C005</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>S005</Value>
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
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C006</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Agent C"">SOA</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SOA6</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SON6</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Warehou"">SOW</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>SOW6</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>

        <GoodsDescription></GoodsDescription>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <WayBillNumber></WayBillNumber>

        <AddInfoCollection>
          <AddInfo>
            <Key>HazardousCargo</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>AppliesToAllPacks</Key>
            <Value>Y</Value>
          </AddInfo>
          <AddInfo>
            <Key>DeclaredPackCount</Key>
            <Value>10</Value>
          </AddInfo>
          <AddInfo>
            <Key>DeclaredPackWeight</Key>
            <Value>250.5</Value>
          </AddInfo>
          <AddInfo>
            <Key>DeclarationAPPlusID</Key>
            <Value>DEC000098</Value>
          </AddInfo>
          <AddInfo>
            <Key>CommodityReference</Key>
            <Value>ICV0000001</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>

        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type Description=""Freight Forwarder Reference"">FFW</Type>
            <ReferenceNumber>S00001266</ReferenceNumber>
          </AdditionalReference>
          <AdditionalReference>
            <Type Description=""Export Receive Consignment Number"">ERC</Type>
            <ReferenceNumber>ERC001</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>AAA</ContainerNumber>
            <DetailedDescription>AAA packline 1</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>AAA packline 1</GoodsDescription>
            <ImportReferenceNumber></ImportReferenceNumber>
            <MarksAndNos></MarksAndNos>
            <PackingLineID>packID</PackingLineID>
            <PackQty>225</PackQty>
            <PackType Description=""Package"">PKG</PackType>
            <ReferenceNumber></ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>37.35</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>7801.8</Weight>
            <WeightUnit>KG</WeightUnit>

            <UNDGCollection>
              <UNDG>
{flashpoint}
                <IMOClass>3</IMOClass>
                <MarinePollutant>t</MarinePollutant>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup>II</PackingGroup>
                <PackQty>0</PackQty>
                <ProperShippingName>PERFUMERY PRODUCTS</ProperShippingName>
                <State>Gas</State>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName></TechicalName>
                <UNDGCode>1266a</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		protected override void SetUp()
		{
			base.SetUp();
			context = new CommonContext(Factory);
		}

		CommonContext context;
	}
}
