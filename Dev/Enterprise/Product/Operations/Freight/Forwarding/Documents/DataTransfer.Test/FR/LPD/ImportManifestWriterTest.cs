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
	sealed class ImportManifestWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var hasFlashPoint = true;
				var importManifest = PrepareTestData(hasFlashPoint);

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new ImportManifestWriter(manager);

				var dataObject = writer.GetDataObject(importManifest);

				AssertUXml(dataObject, expectedXml);
			}
		}

		ImportManifest PrepareTestData(bool hasFlashPoint)
		{
			var importManifest = new ImportManifest("ForwardingConsol", "C00001001");

			importManifest.RequiresTemperatureControl = true;
			importManifest.TemperatureMaximum = new Measurement
			{
				Value = 12,
				Unit = new DummyCodeDescription()
				{
					Code = "C"
				}
			};

			importManifest.TemperatureMinimum = new Measurement
			{
				Value = 20,
				Unit = new DummyCodeDescription()
				{
					Code = "C"
				}
			};

			importManifest.SendingForwarder = CreateAddress("SendingForwarder");
			importManifest.ReceivingForwarder = CreateAddress("ReceivingForwarder");
			importManifest.Transporter = CreateAddress("Transporter");
			importManifest.SendingParty = CreateAddress("SendingParty");
			importManifest.Carrier = CreateAddress("Carrier");

			importManifest.ATPReference = "ATPAP01";
			importManifest.PortLocation = "AUCKD";
			importManifest.PortArea = "AKD";
			importManifest.ConsolNumber = "C00001001";
			importManifest.VesselName = "MAERSK EMDEN";
			importManifest.OTCReference = "029N";
			importManifest.VoyageNumber = "F098";
			importManifest.ETA = new ZDateTime(2020, 8, 23);

			importManifest.SendingForwarderSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S001");
			importManifest.SendingForwarderCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C001");

			importManifest.ReceivingForwarderSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S002");
			importManifest.ReceivingForwarderCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C002");

			importManifest.TransporterSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S003");
			importManifest.TransporterCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C003");

			importManifest.SendingPartySON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S004");
			importManifest.SendingPartyCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C004");

			importManifest.CarrierSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S005");
			importManifest.CarrierCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C005");

			importManifest.BillOfLadingNumber = "bol001";

			importManifest.ContainerMode = new DummyCodeDescription()
			{
				Code = Core.Constants.ContainerModes.FCL
			};

			importManifest.ShipmentType = new DummyCodeDescription()
			{
				Code = Core.Constants.AgentType.Agent
			};

			importManifest.PortOfOrigin = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRPAR",
				Name = "Paris"
			};

			importManifest.PortOfTranshipment = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUBNE",
				Name = "Brisbane"
			};

			importManifest.PortOfDestination = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};

			importManifest.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRPAR",
				Name = "Paris"
			};

			var container1 = CreateContainer(context, "AAA");
			var container2 = CreateContainer(context, "BBB");

			var packline1 = CreatePackingLine("AAA packline 1", "AAA", "S00001", 1, !hasFlashPoint);
			var packline2 = CreatePackingLine("AAA packline 2", "AAA", "S00002", 2, !hasFlashPoint);
			var packline3 = CreatePackingLine("BBB packline 1", "BBB", "S00002", 3, hasFlashPoint);
			var packline4 = CreatePackingLine("BBB packline 2", "BBB", "S00002", 4, hasFlashPoint);

			container1.PackingLines = new[]
			{
				packline1,
				packline2
			};

			container2.PackingLines = new[]
			{
				packline3,
				packline4
			};

			importManifest.Containers = new[]
			{
				container1,
				container2
			};

			var goodsDetail1 = CreateGoodsDetail("S00001");
			goodsDetail1.PackingLines = new[]
			{
				packline1
			};

			var goodsDetail2 = CreateGoodsDetail("S00002");
			goodsDetail2.PackingLines = new[]
			{
				packline2,
				packline3,
				packline4
			};

			importManifest.GoodsDetails = new[] { goodsDetail1, goodsDetail2 };

			return importManifest;
		}

		GoodsDetail CreateGoodsDetail(string shipmentNumber)
		{
			var goodsDetail = new GoodsDetail(ZGuid.NewZGuid());
			goodsDetail.ShipmentNumber = shipmentNumber;
			goodsDetail.HouseBillNumber = "H00001";
			goodsDetail.GoodsDescription = "Goods Detail Description";
			goodsDetail.MarksAndNumbers = "Goods Detail Marks And Numbers";
			goodsDetail.GoodsHandlingNotes = "Goods Handling Notes";
			goodsDetail.RequiresTemperatureControl = true;

			goodsDetail.Shipper = CreateAddress("GoodsDetailShipper");
			goodsDetail.Consignee = CreateAddress("GoodsDetailConsignee");
			goodsDetail.NotifyParty = CreateAddress("GoodsDetailNotifyParty");
			goodsDetail.NotifyParty2 = CreateAddress("GoodsDetailNotifyParty2");
			goodsDetail.ThirdParty = CreateAddress("DeliveryAgent");
			goodsDetail.ThirdPartySON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S005");
			goodsDetail.ThirdPartyCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C005");

			goodsDetail.PortOfOrigin = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRPAR",
				Name = "Paris"
			};

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

			container.SealPartyType = new DummyCodeDescription
			{
				Code = "CAR",
				Description = "Carrier"
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

			container.TransportMode = new CodeDescription(new CodeDescriptionPairList())
			{
				Code = "RTE"
			};

			container.VehicleRegistration = "V0000023";
			container.UnpackingReference = "U0001";
			container.LPDStatus = "PRO";
			container.UnpackingNotes = "Container Unpacking Notes";
			container.LPDReference = "LPD000003434";
			container.IsNonOperativeReefer = false;

			return container;
		}

		BookingPackingLine CreatePackingLine(string goodsDescription, string containerNumber, string shipmentId, int quantity, bool hasFlashPoint)
		{
			var packingLine = new BookingPackingLine(ZGuid.NewZGuid());

			packingLine.PackingLineID = "packID";
			packingLine.ShipmentID = shipmentId;
			packingLine.ContainerNumber = containerNumber;
			packingLine.Quantity = quantity;
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

			packingLine.Volume = new Measurement()
			{
				Value = 2,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
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

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001001</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>
    <ContainerMode>FCL</ContainerMode>
    <PortFirstForeign Name=""Brisbane"">AUBNE</PortFirstForeign>
    <PortOfDestination Name=""Sydney"">AUSYD</PortOfDestination>
    <PortOfOrigin Name=""Paris"">FRPAR</PortOfOrigin>
    <ShipmentType>AGT</ShipmentType>
    <VesselName>MAERSK EMDEN</VesselName>
    <VoyageFlightNo>F098</VoyageFlightNo>
    <WayBillNumber>bol001</WayBillNumber>
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
        <Key>ATPReference</Key>
        <Value>ATPAP01</Value>
      </AddInfo>
      <AddInfo>
        <Key>OTCReference</Key>
        <Value>029N</Value>
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
            <Key>VehicleRegistration</Key>
            <Value>V0000023</Value>
          </AddInfo>
          <AddInfo>
            <Key>UnpackingReference</Key>
            <Value>U0001</Value>
          </AddInfo>
          <AddInfo>
            <Key>LPDStatus</Key>
            <Value>PRO</Value>
          </AddInfo>
          <AddInfo>
            <Key>LPDReference</Key>
            <Value>LPD000003434</Value>
          </AddInfo>
          <AddInfo>
            <Key>UnpackingNotes</Key>
            <Value>Container Unpacking Notes</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>
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
            <Key>VehicleRegistration</Key>
            <Value>V0000023</Value>
          </AddInfo>
          <AddInfo>
            <Key>UnpackingReference</Key>
            <Value>U0001</Value>
          </AddInfo>
          <AddInfo>
            <Key>LPDStatus</Key>
            <Value>PRO</Value>
          </AddInfo>
          <AddInfo>
            <Key>LPDReference</Key>
            <Value>LPD000003434</Value>
          </AddInfo>
          <AddInfo>
            <Key>UnpackingNotes</Key>
            <Value>Container Unpacking Notes</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>
      </Container>
    </ContainerCollection>
    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-08-23T00:00:00</Value>
      </Date>
    </DateCollection>
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
        <AddressType>ArrivalCFSLocalTransportAddress</AddressType>
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
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>S004</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <SubShipmentCollection>
      <SubShipment>
        <GoodsDescription>Goods Detail Description</GoodsDescription>
        <PortOfOrigin Name=""Paris"">FRPAR</PortOfOrigin>
        <RequiredTemperatureMaximum>12</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>20</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>true</RequiresTemperatureControl>
        <WayBillNumber>H00001</WayBillNumber>
        <AddInfoCollection>
          <AddInfo>
            <Key>HazardousCargo</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>
        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type Description=""Freight Forwarder Reference"">FFW</Type>
            <ReferenceNumber>S00001</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>
        <NoteCollection>
          <Note>
            <Description>Marks &amp; Numbers</Description>
            <NoteText>Goods Detail Marks And Numbers</NoteText>
          </Note>
          <Note>
            <Description>Goods Handling Notes</Description>
            <NoteText>Goods Handling Notes</NoteText>
          </Note>
        </NoteCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>GoodsDetailShipper additional info</AdditionalAddressInformation>
            <Address1>GoodsDetailShipper address line 1</Address1>
            <Address2>GoodsDetailShipper address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>GoodsDetailShipper city</City>
            <CompanyName>GoodsDetailShipper</CompanyName>
            <Contact>GoodsDetailShipper contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>GoodsDetailShipper email</Email>
            <Fax>GoodsDetailShipper f</Fax>
            <GovRegNum>GoodsDetailShipper tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>GoodsDetailShipper p</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>GoodsDetai</Postcode>
            <State>GoodsDetailShipper state</State>
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
            <AdditionalAddressInformation>GoodsDetailConsignee additional info</AdditionalAddressInformation>
            <Address1>GoodsDetailConsignee address line 1</Address1>
            <Address2>GoodsDetailConsignee address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>GoodsDetailConsignee city</City>
            <CompanyName>GoodsDetailConsignee</CompanyName>
            <Contact>GoodsDetailConsignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>GoodsDetailConsignee email</Email>
            <Fax>GoodsDetailConsignee</Fax>
            <GovRegNum>GoodsDetailConsignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>GoodsDetailConsignee</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>GoodsDetai</Postcode>
            <State>GoodsDetailConsignee stat</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>GoodsDetailNotifyParty additional info</AdditionalAddressInformation>
            <Address1>GoodsDetailNotifyParty address line 1</Address1>
            <Address2>GoodsDetailNotifyParty address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>GoodsDetailNotifyParty city</City>
            <CompanyName>GoodsDetailNotifyParty</CompanyName>
            <Contact>GoodsDetailNotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>GoodsDetailNotifyParty email</Email>
            <Fax>GoodsDetailNotifyPar</Fax>
            <GovRegNum>GoodsDetailNotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>GoodsDetailNotifyPar</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>GoodsDetai</Postcode>
            <State>GoodsDetailNotifyParty st</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>GoodsDetailNotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>GoodsDetailNotifyParty2 address line 1</Address1>
            <Address2>GoodsDetailNotifyParty2 address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>GoodsDetailNotifyParty2 city</City>
            <CompanyName>GoodsDetailNotifyParty2</CompanyName>
            <Contact>GoodsDetailNotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>GoodsDetailNotifyParty2 email</Email>
            <Fax>GoodsDetailNotifyPar</Fax>
            <GovRegNum>GoodsDetailNotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>GoodsDetailNotifyPar</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>GoodsDetai</Postcode>
            <State>GoodsDetailNotifyParty2 s</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DeliveryAgent</AddressType>
            <AdditionalAddressInformation>DeliveryAgent additional info</AdditionalAddressInformation>
            <Address1>DeliveryAgent address line 1</Address1>
            <Address2>DeliveryAgent address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DeliveryAgent city</City>
            <CompanyName>DeliveryAgent</CompanyName>
            <Contact>DeliveryAgent contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryAgent email</Email>
            <Fax>DeliveryAgent fax</Fax>
            <GovRegNum>DeliveryAgent tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryAgent phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DeliveryAg</Postcode>
            <State>DeliveryAgent state</State>
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
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>AAA</ContainerNumber>
            <DetailedDescription>Goods Detail Description</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>Goods Detail Description</GoodsDescription>
            <ImportReferenceNumber></ImportReferenceNumber>
            <MarksAndNos>Goods Detail Marks And Numbers</MarksAndNos>
            <PackQty>1</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber></ReferenceNumber>
            <RequiredTemperatureMaximum>12</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>20</RequiredTemperatureMinimum>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>2</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <UNDGCollection>
              <UNDG>
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
      </SubShipment>
      <SubShipment>
        <GoodsDescription>Goods Detail Description</GoodsDescription>
        <PortOfOrigin Name=""Paris"">FRPAR</PortOfOrigin>
        <RequiredTemperatureMaximum>12</RequiredTemperatureMaximum>
        <RequiredTemperatureMinimum>20</RequiredTemperatureMinimum>
        <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
        <RequiresTemperatureControl>true</RequiresTemperatureControl>
        <WayBillNumber>H00001</WayBillNumber>
        <AddInfoCollection>
          <AddInfo>
            <Key>HazardousCargo</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>FormVersion</Key>
            <Value>1.0.0</Value>
          </AddInfo>
        </AddInfoCollection>
        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type Description=""Freight Forwarder Reference"">FFW</Type>
            <ReferenceNumber>S00002</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>
        <NoteCollection>
          <Note>
            <Description>Marks &amp; Numbers</Description>
            <NoteText>Goods Detail Marks And Numbers</NoteText>
          </Note>
          <Note>
            <Description>Goods Handling Notes</Description>
            <NoteText>Goods Handling Notes</NoteText>
          </Note>
        </NoteCollection>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>GoodsDetailShipper additional info</AdditionalAddressInformation>
            <Address1>GoodsDetailShipper address line 1</Address1>
            <Address2>GoodsDetailShipper address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>GoodsDetailShipper city</City>
            <CompanyName>GoodsDetailShipper</CompanyName>
            <Contact>GoodsDetailShipper contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>GoodsDetailShipper email</Email>
            <Fax>GoodsDetailShipper f</Fax>
            <GovRegNum>GoodsDetailShipper tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>GoodsDetailShipper p</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>GoodsDetai</Postcode>
            <State>GoodsDetailShipper state</State>
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
            <AdditionalAddressInformation>GoodsDetailConsignee additional info</AdditionalAddressInformation>
            <Address1>GoodsDetailConsignee address line 1</Address1>
            <Address2>GoodsDetailConsignee address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>GoodsDetailConsignee city</City>
            <CompanyName>GoodsDetailConsignee</CompanyName>
            <Contact>GoodsDetailConsignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>GoodsDetailConsignee email</Email>
            <Fax>GoodsDetailConsignee</Fax>
            <GovRegNum>GoodsDetailConsignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>GoodsDetailConsignee</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>GoodsDetai</Postcode>
            <State>GoodsDetailConsignee stat</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AdditionalAddressInformation>GoodsDetailNotifyParty additional info</AdditionalAddressInformation>
            <Address1>GoodsDetailNotifyParty address line 1</Address1>
            <Address2>GoodsDetailNotifyParty address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>GoodsDetailNotifyParty city</City>
            <CompanyName>GoodsDetailNotifyParty</CompanyName>
            <Contact>GoodsDetailNotifyParty contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>GoodsDetailNotifyParty email</Email>
            <Fax>GoodsDetailNotifyPar</Fax>
            <GovRegNum>GoodsDetailNotifyParty tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>GoodsDetailNotifyPar</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>GoodsDetai</Postcode>
            <State>GoodsDetailNotifyParty st</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AdditionalAddressInformation>GoodsDetailNotifyParty2 additional info</AdditionalAddressInformation>
            <Address1>GoodsDetailNotifyParty2 address line 1</Address1>
            <Address2>GoodsDetailNotifyParty2 address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>GoodsDetailNotifyParty2 city</City>
            <CompanyName>GoodsDetailNotifyParty2</CompanyName>
            <Contact>GoodsDetailNotifyParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>GoodsDetailNotifyParty2 email</Email>
            <Fax>GoodsDetailNotifyPar</Fax>
            <GovRegNum>GoodsDetailNotifyParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>GoodsDetailNotifyPar</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>GoodsDetai</Postcode>
            <State>GoodsDetailNotifyParty2 s</State>
            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>DeliveryAgent</AddressType>
            <AdditionalAddressInformation>DeliveryAgent additional info</AdditionalAddressInformation>
            <Address1>DeliveryAgent address line 1</Address1>
            <Address2>DeliveryAgent address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>DeliveryAgent city</City>
            <CompanyName>DeliveryAgent</CompanyName>
            <Contact>DeliveryAgent contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>DeliveryAgent email</Email>
            <Fax>DeliveryAgent fax</Fax>
            <GovRegNum>DeliveryAgent tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>DeliveryAgent phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>DeliveryAg</Postcode>
            <State>DeliveryAgent state</State>
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
        </OrganizationAddressCollection>
        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>AAA</ContainerNumber>
            <DetailedDescription>Goods Detail Description</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>Goods Detail Description</GoodsDescription>
            <ImportReferenceNumber></ImportReferenceNumber>
            <MarksAndNos>Goods Detail Marks And Numbers</MarksAndNos>
            <PackQty>2</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber></ReferenceNumber>
            <RequiredTemperatureMaximum>12</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>20</RequiredTemperatureMinimum>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>2</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <UNDGCollection>
              <UNDG>
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
            <DetailedDescription>Goods Detail Description</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>Goods Detail Description</GoodsDescription>
            <ImportReferenceNumber></ImportReferenceNumber>
            <MarksAndNos>Goods Detail Marks And Numbers</MarksAndNos>
            <PackQty>7</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber></ReferenceNumber>
            <RequiredTemperatureMaximum>12</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>20</RequiredTemperatureMinimum>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>4</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>176</Weight>
            <WeightUnit>KG</WeightUnit>
            <UNDGCollection>
              <UNDG>
                <FlashPoint>10</FlashPoint>
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
              <UNDG>
                <FlashPoint>10</FlashPoint>
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
