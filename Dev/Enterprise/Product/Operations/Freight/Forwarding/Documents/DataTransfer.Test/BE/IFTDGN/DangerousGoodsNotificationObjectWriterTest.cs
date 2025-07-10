using System;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataTransfer;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.BE;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;
using BelgianPortsConstants = Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE.BelgianPortsConstants;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.Testing.DataTransfer.BE.testing
{
	sealed class DangerousGoodsNotificationObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateImportDataObject()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var hasFlashPoint = true;
				var dangerousGoodsNotification = PrepareTestData(hasFlashPoint);

				PopulateImportData(dangerousGoodsNotification);

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new DangerousGoodsNotificationObjectWriter(manager);
				var dataObject = writer.GetDataObject(dangerousGoodsNotification);

				var flashPoint1 = "<FlashPoint>85.0</FlashPoint>".PadLeft(41, ' ');
				var flashPoint2 = "<FlashPoint>67.0</FlashPoint>".PadLeft(41, ' ');

				var expectedImportXml = GetImportExpectedXml(flashPoint1, flashPoint2);
				AssertUXml(dataObject, expectedImportXml);

				hasFlashPoint = false;
				dangerousGoodsNotification = PrepareTestData(hasFlashPoint);

				PopulateImportData(dangerousGoodsNotification);

				writer = new DangerousGoodsNotificationObjectWriter(manager);
				dataObject = writer.GetDataObject(dangerousGoodsNotification);

				expectedImportXml = GetImportExpectedXml(null, null);
				AssertUXml(dataObject, expectedImportXml);
			}
		}

		public void TestPopulateExportDataObject()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var hasFlashPoint = true;
				var dangerousGoodsNotification = PrepareTestData(hasFlashPoint);

				PopulateExportData(dangerousGoodsNotification);

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new DangerousGoodsNotificationObjectWriter(manager);

				var dataObject = writer.GetDataObject(dangerousGoodsNotification);

				var flashPoint1 = "<FlashPoint>85.0</FlashPoint>".PadLeft(41, ' ');
				var flashPoint2 = "<FlashPoint>67.0</FlashPoint>".PadLeft(41, ' ');

				var expectedExportXml = GetExportExpectedXml(flashPoint1, flashPoint2);
				AssertUXml(dataObject, expectedExportXml);

				hasFlashPoint = false;
				dangerousGoodsNotification = PrepareTestData(hasFlashPoint);

				PopulateExportData(dangerousGoodsNotification);

				writer = new DangerousGoodsNotificationObjectWriter(manager);

				dataObject = writer.GetDataObject(dangerousGoodsNotification);

				expectedExportXml = GetExportExpectedXml(null, null);
				AssertUXml(dataObject, expectedExportXml);
			}
		}

		public void TestPopulateResendDataObject()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var hasFlashPoint = true;
				var dangerousGoodsNotification = PrepareTestData(hasFlashPoint);

				PopulateExportData(dangerousGoodsNotification);
				dangerousGoodsNotification.DgnSecurityNumber = "DGN001001";

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new DangerousGoodsNotificationObjectWriter(manager);

				var dataObject = writer.GetDataObject(dangerousGoodsNotification);

				var flashPoint1 = "<FlashPoint>85.0</FlashPoint>".PadLeft(41, ' ');
				var flashPoint2 = "<FlashPoint>67.0</FlashPoint>".PadLeft(41, ' ');

				var expectedResendXml = GetResendExpectedXml(flashPoint1, flashPoint2);
				AssertUXml(dataObject, expectedResendXml);

				hasFlashPoint = false;
				dangerousGoodsNotification = PrepareTestData(hasFlashPoint);

				PopulateExportData(dangerousGoodsNotification);
				dangerousGoodsNotification.DgnSecurityNumber = "DGN001001";

				writer = new DangerousGoodsNotificationObjectWriter(manager);

				dataObject = writer.GetDataObject(dangerousGoodsNotification);

				expectedResendXml = GetResendExpectedXml(null, null);
				AssertUXml(dataObject, expectedResendXml);
			}
		}

		DangerousGoodsNotification PrepareTestData(bool hasFlashPoint)
		{
			var dangerousGoodsNotification = new DangerousGoodsNotification("ForwardingConsol", "C00001015");

			dangerousGoodsNotification.ConsolNumber = "C00001015";
			dangerousGoodsNotification.CarrierBookingReference = "BKC001007";
			dangerousGoodsNotification.BillOfLading = "1122334499";
			dangerousGoodsNotification.ContainerMode = new DummyCodeDescription() { Code = Core.Constants.ContainerModes.FCL, Description = "Full Container Load" };
			dangerousGoodsNotification.ShipmentType = new DummyCodeDescription() { Code = Core.Constants.AgentType.Agent, Description = "Agent" };
			dangerousGoodsNotification.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "BEANR",
				Name = "Antwerp"
			};
			dangerousGoodsNotification.VesselStayReference = "V200487";
			dangerousGoodsNotification.VesselStayStartDate = new ZDateTime(2020, 07, 20, 01, 15, 30);
			dangerousGoodsNotification.VesselStayEndDate = new ZDateTime(2020, 08, 02, 15, 06, 21);
			dangerousGoodsNotification.Carrier = CreateAddress("Carrier");
			dangerousGoodsNotification.CarrierPortId = CreateRegistrationNumber("PSN", "CarrierPortId");
			dangerousGoodsNotification.SendingParty = CreateAddress("SendingParty");
			dangerousGoodsNotification.SendingPartyPortId = CreateRegistrationNumber("PSN", "SendingPartyPortId");
			dangerousGoodsNotification.SendingPartyEori = CreateRegistrationNumber("EOR", "SendingPartyEORI");

			var dangerousGood1 = CreateDangerousGood("0503B", "AIR BAG MODULES", "Airbag Mercedes C", "1.4G", "II", "F-B", "S-X", "702B", 5, "BAG", 142.01m, "KG", "Kilograms", 0, "", "", 85.0m, "C", hasFlashPoint);
			var dangerousGood2 = CreateDangerousGood("0453B", "ROCKETS, LINE-THROWING", "Ejection seat", "1.4G", "III", "F-B", "S-X", "914A", 1, "PLT", 213.01m, "KG", "Kilograms", 0, "", "", 67.0m, "C", hasFlashPoint);
			var dangerousGood3 = CreateDangerousGood("0503B", "AIR BAG MODULES", "Airbag Mercedes B", "1.4G", "II", "F-B", "S-X", "702B", 5, "BAG", 120.00m, "KG", "Kilograms", 0, "", "", 85.0m, "C", hasFlashPoint);
			dangerousGood3.PackedInLimitedQuantity = true;
			var dangerousGood4 = CreateDangerousGood("2910", "RADIOACTIVE MATERIAL, EXCEPTED PACKAGE - LIMITED QUANTITY OF MATERIAL", "Misterious revolutionary fuel", "7", "II", "F-B", "S-X", "702B", 5, "BOX", 12.00m, "KG", "Kilograms", 0, "", "", 85.0m, "C", hasFlashPoint);
			dangerousGood4.PackedInExceptedQuantity = true;
			dangerousGood4.MarinePollutant = new DummyCodeDescription() { Code = "Y" };
			dangerousGood4.NetExplosiveWeight = new Measurement
			{
				Value = 17,
				Unit = new DummyCodeDescription() { Code = "KG", Description = "Kilograms" }
			};
			dangerousGood4.Radioactivity = new Measurement
			{
				Value = 7,
				Unit = new DummyCodeDescription() { Code = "CUR", Description = "Curie" }
			};
			dangerousGood4.RadioactiveTransportIndex = 7.0;
			dangerousGood4.RadioactiveCriticalitySafetyIndex = 5.0;

			var packline1 = CreatePackingLine("ROOF Covering", "MSCU1245787", 1, "PLT", "Pallet", 18, "KG", "Kilograms", 0.00m, "M3", "Cubic Metres");

			var packline2 = CreatePackingLine("Goods description packline 2", "MSCU1245787", 1, "PLT", "Pallet", 453.000m, "KG", "Kilograms", 0.00m, "M3", "Cubic Metres");
			packline2.DangerousGoods = new[]
			{
				dangerousGood1,
				dangerousGood2
			};

			var packline3 = CreatePackingLine("AIR BAG", "MSCU1247856", 3, "PLT", "Pallet", 2140.0m, "KG", "Kilograms", 0.00m, "M3", "Cubic Metres");
			packline3.DangerousGoods = new[]
			{
				dangerousGood3
			};

			var packline4 = CreatePackingLine("DASHBOARD MERCEDES", "MSCU8757656", 5, "PLT", "Pallet", 256.15m, "KG", "Kilograms", 150.00m, "M3", "Cubic Metres");
			packline4.DangerousGoods = new[]
			{
				dangerousGood4
			};

			var container1 = CreateContainer(context, "MSCU1245787", 2, true);
			container1.PackingLines = new[]
			{
				packline1,
				packline2
			};

			var container2 = CreateContainer(context, "MSCU1247856", 5, true);
			container2.PackingLines = new[]
			{
				packline3
			};

			var container3 = CreateContainer(context, "MSCU8757656", 3, true);
			container3.PackingLines = new[]
			{
				packline4
			};

			dangerousGoodsNotification.Containers = new[]
			{
				container1,
				container2,
				container3
			};

			return dangerousGoodsNotification;
		}

		void PopulateImportData(DangerousGoodsNotification dangerousGoodsNotification)
		{
			dangerousGoodsNotification.ReceivingForwarder = CreateAddress("ReceivingForwarder");
			dangerousGoodsNotification.ReceivingForwarderPortId = CreateRegistrationNumber("PSN", "ReceivingForwarderPortId");
			dangerousGoodsNotification.ArrivalCTO = CreateAddress("ArrivalCTO");
			dangerousGoodsNotification.ArrivalCTOTerminalId = CreateRegistrationNumber("PSN", "ArrivalCTOTerminalId");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Discharge;
			dangerousGoodsNotification.HandlingDate = new ZDateTime(2020, 10, 7, 13, 15, 00);

			PopulateTransports(dangerousGoodsNotification);
		}

		void PopulateExportData(DangerousGoodsNotification dangerousGoodsNotification)
		{
			dangerousGoodsNotification.SendingForwarder = CreateAddress("SendingForwarder");
			dangerousGoodsNotification.SendingForwarderPortId = CreateRegistrationNumber("PSN", "SendingForwarderPortId");
			dangerousGoodsNotification.DepartureCTO = CreateAddress("DepartureCTO");
			dangerousGoodsNotification.DepartureCTOTerminalId = CreateRegistrationNumber("PSN", "DepartureCTOTerminalId");
			dangerousGoodsNotification.HandlingInstruction = BelgianPortsConstants.HandlingInstructions.Loading;
			dangerousGoodsNotification.HandlingDate = new ZDateTime(2020, 10, 7, 13, 15, 00);

			PopulateTransports(dangerousGoodsNotification, false);
		}

		void PopulateTransports(DangerousGoodsNotification dangerousGoodsNotification, bool importFlow = true)
		{
			var dummyVesselTypeList = new CodeDescriptionPairList();
			dummyVesselTypeList.AddPair("CV", "Cargo Vessel");

			var seaVessel = Factory.New<RefVessel>();
			seaVessel.RV_Name = "MSC UBERTY";
			seaVessel.RV_LloydsNumber = "9337444";
			seaVessel.RV_RadioCallSign = "A8OR4";
			seaVessel.RV_RN_NKCountryOfReg = "CN";
			seaVessel.RV_VesselType = "CV";

			var portCNYTN = "CNYTN";
			var portBEANR = "BEANR";

			var date_port1 = new ZDateTime(2020, 10, 5, 13, 15, 00);
			var date_port2 = new ZDateTime(2020, 10, 6, 13, 15, 00);
			var date_port3 = new ZDateTime(2020, 10, 19, 13, 15, 00);

			var mainTransport = importFlow
				? CreateTransport(1, Core.Constants.TransportModes.Sea, Core.Constants.TransportPlanningType.MainVessel, "007W", date_port1, date_port2, seaVessel, portCNYTN, portBEANR, dangerousGoodsNotification.Carrier)
				: CreateTransport(2, Core.Constants.TransportModes.Sea, Core.Constants.TransportPlanningType.MainVessel, "124", date_port2.AddDays(2), date_port3, seaVessel, portBEANR, portCNYTN, dangerousGoodsNotification.Carrier);
			var transports = Transports.Create(context, new[]
			{
				mainTransport
			});
			dangerousGoodsNotification.Transports = transports;
			dangerousGoodsNotification.PreOrOnTransportMode = new DummyCodeDescription
			{
				Code = Core.Constants.TransportModes.Sea,
				Description = Core.Constants.TransportModeDescriptions.Sea
			};
			dangerousGoodsNotification.PreOrOnVesselName = "CYGNUS";
			dangerousGoodsNotification.PreOrOnVesselENINumber = "02328823";
			if (importFlow)
			{
				dangerousGoodsNotification.PickupDate = date_port2.AddDays(2);
			}
			else
			{
				dangerousGoodsNotification.DeliveryDate = date_port3;
			}
		}

		Freight.Business.Transport CreateTransport(int legOrder, string mode, string type, string voyageFlightNumber, ZDateTime departureDateTime, ZDateTime arrivalDateTime, RefVessel vessel,
			string portOfLoading, string portOfDischarge, Address carrierAddress)
		{
			var transport = Factory.New<Freight.Business.Transport>();
			transport.ParentType = typeof(ForwardingConsol);
			transport.JW_LegOrder = (byte)legOrder;
			transport.JW_TransportMode = mode;
			transport.JW_TransportType = type;
			transport.JW_RL_NKLoadPort = portOfLoading;
			transport.JW_RL_NKDiscPort = portOfDischarge;
			transport.JW_VoyageFlight = voyageFlightNumber;
			transport.JW_ETD = departureDateTime;
			transport.JW_ETA = arrivalDateTime;
			transport.JW_ATD = departureDateTime.AddHours(3).AddMinutes(5);
			transport.JW_ATA = arrivalDateTime.AddHours(1).AddMinutes(15);
			transport.JW_Vessel = vessel.RV_FK;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "CARRIER";
			transport.JW_OA_CarrierAddress_ZAddress.SetOrgWithoutSettingDefaultAddress(org.PK);

			return transport;
		}

		protected override RegistrationNumber CreateRegistrationNumber(string type, string value)
		{
			return new RegistrationNumber()
			{
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.Belgium
				},
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.Belgium))
				{
					Code = type
				},
				Value = value
			};
		}

		DGNContainer CreateContainer(IContext context, string containerNumber, int numberOfPacks, bool containsDangerousGoods)
		{
			var container = new DGNContainer(DefaultDataObjectWriterStrategy.TestInstance);

			container.Number = containerNumber;
			container.PackCount = numberOfPacks;
			container.HasDangerousGoods = containsDangerousGoods;
			container.IsNonOperativeReefer = false;

			return container;
		}

		DGNPackingLine CreatePackingLine(string goodsDescription, string containerNumber, int quantity, string packageCode, string packageCodeDescription,
			decimal weight, string weighUnitCode, string weighUnitCodeDescription,
			decimal volume, string volumeUnitCode, string volumeUnitCodeDescription)
		{
			var packingLine = new DGNPackingLine(ZGuid.NewZGuid());

			packingLine.GoodsDescription = goodsDescription;
			packingLine.ContainerNumber = containerNumber;
			packingLine.Quantity = quantity;
			packingLine.PackageType = new DummyCodeDescription
			{
				Code = packageCode,
				Description = packageCodeDescription
			};

			packingLine.Weight = new Measurement()
			{
				Value = weight,
				Unit = new DummyCodeDescription
				{
					Code = weighUnitCode,
					Description = weighUnitCodeDescription
				}
			};
			packingLine.Volume = new Measurement
			{
				Value = volume,
				Unit = new DummyCodeDescription
				{
					Code = volumeUnitCode,
					Description = volumeUnitCodeDescription
				}
			};

			return packingLine;
		}

		DGNDangerousGood CreateDangerousGood(string undg, string properShippingName, string technicalName, string imoClass, string packingGroup,
			string emergencyScheduleFireCode, string emergencyScheduleSpillageCode, string medicalFirstAidGuide, int quantity, string packType,
			decimal weight, string weighUnitCode, string weighUnitCodeDescription,
			decimal volume, string volumeUnitCode, string volumeUnitCodeDescription,
			decimal flashPoint, string flashpointUnitCode, bool hasFlashPoint)
		{
			var dangerousGood = new DGNDangerousGood(ZGuid.NewZGuid())
			{
				Code = undg,
				Unno = undg,
				RegulationStandard = "IMO",
				Variant = "C",
				ProperShippingName = properShippingName,
				TechnicalName = technicalName,
				IMOClass = imoClass,
				PackingGroup = packingGroup,
				State = "G",
				EmergencyScheduleFire = new DummyCodeDescription() { Code = emergencyScheduleFireCode },
				EmergencyScheduleSpillage = new DummyCodeDescription() { Code = emergencyScheduleSpillageCode },
				MedicalFirstAidGuide = medicalFirstAidGuide,
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
					Value = flashPoint,
					Unit = new DummyCodeDescription() { Code = flashpointUnitCode }
				};
			}

			return dangerousGood;
		}

		#region Expected Import Xml

		string GetImportExpectedXml(string flashpoint1, string flashpoint2) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001015</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>

    <BookingConfirmationReference>BKC001007</BookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <WayBillNumber>1122334499</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Antwerp</Value>
      </AddInfo>
      <AddInfo>
        <Key>VesselStayReference</Key>
        <Value>V200487</Value>
      </AddInfo>
      <AddInfo>
        <Key>HandlingInstruction</Key>
        <Value>LDI</Value>
      </AddInfo>
      <AddInfo>
        <Key>Main_Vessel_RadioCallSign</Key>
        <Value>A8OR4</Value>
      </AddInfo>
      <AddInfo>
        <Key>Main_Vessel_CountryCode</Key>
        <Value>CN</Value>
      </AddInfo>
      <AddInfo>
        <Key>Main_VesselType_Code</Key>
        <Value>CV</Value>
      </AddInfo>
      <AddInfo>
        <Key>Other_TransportMode</Key>
        <Value>SEA</Value>
      </AddInfo>
      <AddInfo>
        <Key>Other_VesselName</Key>
        <Value>CYGNUS</Value>
      </AddInfo>
      <AddInfo>
        <Key>Other_VesselENINumber</Key>
        <Value>02328823</Value>
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
        <ContainerNumber>MSCU1245787</ContainerNumber>
        <Link>1</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <PalletCount>2</PalletCount>
      </Container>
      <Container>
        <ContainerNumber>MSCU1247856</ContainerNumber>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <PalletCount>5</PalletCount>
      </Container>
      <Container>
        <ContainerNumber>MSCU8757656</ContainerNumber>
        <Link>3</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <PalletCount>3</PalletCount>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>VesselStayStartDate</Type>
        <Value>2020-07-20T01:15:30</Value>
      </Date>
      <Date>
        <Type>VesselStayEndDate</Type>
        <Value>2020-08-02T15:06:21</Value>
      </Date>
      <Date>
        <Type>HandlingDate</Type>
        <Value>2020-10-07T13:15:00</Value>
      </Date>
      <Date>
        <Type>Pickup</Type>
        <Value>2020-10-08T13:15:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ArrivalCTOAddress</AddressType>
        <AdditionalAddressInformation>ArrivalCTO additional info</AdditionalAddressInformation>
        <Address1>ArrivalCTO address line 1</Address1>
        <Address2>ArrivalCTO address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ArrivalCTO city</City>
        <CompanyName>ArrivalCTO</CompanyName>
        <Contact>ArrivalCTO contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ArrivalCTO email</Email>
        <Fax>ArrivalCTO fax</Fax>
        <GovRegNum>ArrivalCTO tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ArrivalCTO phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ArrivalCTO</Postcode>
        <State>ArrivalCTO state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>ArrivalCTOTerminalId</Value>
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
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>ReceivingForwarderPortId</Value>
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
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>CarrierPortId</Value>
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
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingPartyPortId</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""EORI code (to be sent verbatim)"">EOR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingPartyEORI</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection>
      <PackingLine>
        <ContainerLink>1</ContainerLink>
        <ContainerNumber>MSCU1245787</ContainerNumber>
        <GoodsDescription>ROOF Covering</GoodsDescription>
        <PackQty>1</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>0</Volume>
        <VolumeUnit Description=""Cubic Metres"">M3</VolumeUnit>
        <Weight>18</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
      </PackingLine>
      <PackingLine>
        <ContainerLink>1</ContainerLink>
        <ContainerNumber>MSCU1245787</ContainerNumber>
        <GoodsDescription>Goods description packline 2</GoodsDescription>
        <PackQty>1</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>0</Volume>
        <VolumeUnit Description=""Cubic Metres"">M3</VolumeUnit>
        <Weight>453.000</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>

        <UNDGCollection>
          <UNDG>
            <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
            <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint1}
            <IMOClass>1.4G</IMOClass>
            <MedicalFirstAidGuide>702B</MedicalFirstAidGuide>
            <PackedInExceptedQuantity>false</PackedInExceptedQuantity>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup>II</PackingGroup>
            <PackQty>5</PackQty>
            <PackType>BAG</PackType>
            <ProperShippingName>AIR BAG MODULES</ProperShippingName>
            <RadioactiveCriticalitySafetyIndex>0</RadioactiveCriticalitySafetyIndex>
            <RadioactiveTransportIndex>0</RadioactiveTransportIndex>
            <Standard>IMO</Standard>
            <State>Gas</State>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName>Airbag Mercedes C</TechicalName>
            <UNDGCode>0503B</UNDGCode>
            <Volume>0</Volume>
            <VolumeUQ></VolumeUQ>
            <Weight>142.01</Weight>
            <WeightUQ Description=""Kilograms"">KG</WeightUQ>
          </UNDG>
          <UNDG>
            <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
            <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint2}
            <IMOClass>1.4G</IMOClass>
            <MedicalFirstAidGuide>914A</MedicalFirstAidGuide>
            <PackedInExceptedQuantity>false</PackedInExceptedQuantity>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup>III</PackingGroup>
            <PackQty>1</PackQty>
            <PackType>PLT</PackType>
            <ProperShippingName>ROCKETS, LINE-THROWING</ProperShippingName>
            <RadioactiveCriticalitySafetyIndex>0</RadioactiveCriticalitySafetyIndex>
            <RadioactiveTransportIndex>0</RadioactiveTransportIndex>
            <Standard>IMO</Standard>
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
      <PackingLine>
        <ContainerLink>2</ContainerLink>
        <ContainerNumber>MSCU1247856</ContainerNumber>
        <GoodsDescription>AIR BAG</GoodsDescription>
        <PackQty>3</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>0</Volume>
        <VolumeUnit Description=""Cubic Metres"">M3</VolumeUnit>
        <Weight>2140.0</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>

        <UNDGCollection>
          <UNDG>
            <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
            <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint1}
            <IMOClass>1.4G</IMOClass>
            <MedicalFirstAidGuide>702B</MedicalFirstAidGuide>
            <PackedInExceptedQuantity>false</PackedInExceptedQuantity>
            <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
            <PackingGroup>II</PackingGroup>
            <PackQty>5</PackQty>
            <PackType>BAG</PackType>
            <ProperShippingName>AIR BAG MODULES</ProperShippingName>
            <RadioactiveCriticalitySafetyIndex>0</RadioactiveCriticalitySafetyIndex>
            <RadioactiveTransportIndex>0</RadioactiveTransportIndex>
            <Standard>IMO</Standard>
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
      <PackingLine>
        <ContainerLink>3</ContainerLink>
        <ContainerNumber>MSCU8757656</ContainerNumber>
        <GoodsDescription>DASHBOARD MERCEDES</GoodsDescription>
        <PackQty>5</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>150.00</Volume>
        <VolumeUnit Description=""Cubic Metres"">M3</VolumeUnit>
        <Weight>256.15</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>

        <UNDGCollection>
          <UNDG>
            <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
            <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint1}
            <IMOClass>7</IMOClass>
            <MarinePollutant>Y</MarinePollutant>
            <MedicalFirstAidGuide>702B</MedicalFirstAidGuide>
            <NetExplosiveWeight>17</NetExplosiveWeight>
            <NetExplosiveWeightUQ Description=""Kilograms"">KG</NetExplosiveWeightUQ>
            <PackedInExceptedQuantity>true</PackedInExceptedQuantity>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup>II</PackingGroup>
            <PackQty>5</PackQty>
            <PackType>BOX</PackType>
            <ProperShippingName>RADIOACTIVE MATERIAL, EXCEPTED PACKAGE - LIMITED QUANTITY OF MATERIAL</ProperShippingName>
            <RadioactiveCriticalitySafetyIndex>5</RadioactiveCriticalitySafetyIndex>
            <RadioactiveTransportIndex>7</RadioactiveTransportIndex>
            <Radioactivity>7</Radioactivity>
            <RadioactivityUQ Description=""Curie"">CUR</RadioactivityUQ>
            <Standard>IMO</Standard>
            <State>Gas</State>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName>Misterious revolutionary fuel</TechicalName>
            <UNDGCode>2910</UNDGCode>
            <Volume>0</Volume>
            <VolumeUQ></VolumeUQ>
            <Weight>12.00</Weight>
            <WeightUQ Description=""Kilograms"">KG</WeightUQ>
          </UNDG>
        </UNDGCollection>
      </PackingLine>
    </PackingLineCollection>

    <TransportLegCollection>
      <TransportLeg>
        <PortOfDischarge Name=""Antwerpen"">BEANR</PortOfDischarge>
        <PortOfLoading Name=""Yantian Pt"">CNYTN</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2020-10-06T14:30:00</ActualArrival>
        <ActualDeparture>2020-10-05T16:20:00</ActualDeparture>
        <EstimatedArrival>2020-10-06T13:15:00</EstimatedArrival>
        <EstimatedDeparture>2020-10-05T13:15:00</EstimatedDeparture>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>9337444</VesselLloydsIMO>
        <VesselName>MSC UBERTY</VesselName>
        <VoyageFlightNo>007W</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";

		#endregion Expected Import Xml

		#region Expected Export Xml

		string GetExportExpectedXml(string flashpoint1, string flashpoint2) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001015</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>

    <BookingConfirmationReference>BKC001007</BookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <WayBillNumber>1122334499</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Antwerp</Value>
      </AddInfo>
      <AddInfo>
        <Key>VesselStayReference</Key>
        <Value>V200487</Value>
      </AddInfo>
      <AddInfo>
        <Key>HandlingInstruction</Key>
        <Value>LLO</Value>
      </AddInfo>
      <AddInfo>
        <Key>Main_Vessel_RadioCallSign</Key>
        <Value>A8OR4</Value>
      </AddInfo>
      <AddInfo>
        <Key>Main_Vessel_CountryCode</Key>
        <Value>CN</Value>
      </AddInfo>
      <AddInfo>
        <Key>Main_VesselType_Code</Key>
        <Value>CV</Value>
      </AddInfo>
      <AddInfo>
        <Key>Other_TransportMode</Key>
        <Value>SEA</Value>
      </AddInfo>
      <AddInfo>
        <Key>Other_VesselName</Key>
        <Value>CYGNUS</Value>
      </AddInfo>
      <AddInfo>
        <Key>Other_VesselENINumber</Key>
        <Value>02328823</Value>
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
        <ContainerNumber>MSCU1245787</ContainerNumber>
        <Link>1</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <PalletCount>2</PalletCount>
      </Container>
      <Container>
        <ContainerNumber>MSCU1247856</ContainerNumber>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <PalletCount>5</PalletCount>
      </Container>
      <Container>
        <ContainerNumber>MSCU8757656</ContainerNumber>
        <Link>3</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <PalletCount>3</PalletCount>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>VesselStayStartDate</Type>
        <Value>2020-07-20T01:15:30</Value>
      </Date>
      <Date>
        <Type>VesselStayEndDate</Type>
        <Value>2020-08-02T15:06:21</Value>
      </Date>
      <Date>
        <Type>HandlingDate</Type>
        <Value>2020-10-07T13:15:00</Value>
      </Date>
      <Date>
        <Type>Delivery</Type>
        <Value>2020-10-19T13:15:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>DepartureCTOAddress</AddressType>
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
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>DepartureCTOTerminalId</Value>
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
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingForwarderPortId</Value>
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
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>CarrierPortId</Value>
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
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingPartyPortId</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""EORI code (to be sent verbatim)"">EOR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingPartyEORI</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection>
      <PackingLine>
        <ContainerLink>1</ContainerLink>
        <ContainerNumber>MSCU1245787</ContainerNumber>
        <GoodsDescription>ROOF Covering</GoodsDescription>
        <PackQty>1</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>0</Volume>
        <VolumeUnit Description=""Cubic Metres"">M3</VolumeUnit>
        <Weight>18</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
      </PackingLine>
      <PackingLine>
        <ContainerLink>1</ContainerLink>
        <ContainerNumber>MSCU1245787</ContainerNumber>
        <GoodsDescription>Goods description packline 2</GoodsDescription>
        <PackQty>1</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>0</Volume>
        <VolumeUnit Description=""Cubic Metres"">M3</VolumeUnit>
        <Weight>453.000</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>

        <UNDGCollection>
          <UNDG>
            <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
            <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint1}
            <IMOClass>1.4G</IMOClass>
            <MedicalFirstAidGuide>702B</MedicalFirstAidGuide>
            <PackedInExceptedQuantity>false</PackedInExceptedQuantity>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup>II</PackingGroup>
            <PackQty>5</PackQty>
            <PackType>BAG</PackType>
            <ProperShippingName>AIR BAG MODULES</ProperShippingName>
            <RadioactiveCriticalitySafetyIndex>0</RadioactiveCriticalitySafetyIndex>
            <RadioactiveTransportIndex>0</RadioactiveTransportIndex>
            <Standard>IMO</Standard>
            <State>Gas</State>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName>Airbag Mercedes C</TechicalName>
            <UNDGCode>0503B</UNDGCode>
            <Volume>0</Volume>
            <VolumeUQ></VolumeUQ>
            <Weight>142.01</Weight>
            <WeightUQ Description=""Kilograms"">KG</WeightUQ>
          </UNDG>
          <UNDG>
            <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
            <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint2}
            <IMOClass>1.4G</IMOClass>
            <MedicalFirstAidGuide>914A</MedicalFirstAidGuide>
            <PackedInExceptedQuantity>false</PackedInExceptedQuantity>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup>III</PackingGroup>
            <PackQty>1</PackQty>
            <PackType>PLT</PackType>
            <ProperShippingName>ROCKETS, LINE-THROWING</ProperShippingName>
            <RadioactiveCriticalitySafetyIndex>0</RadioactiveCriticalitySafetyIndex>
            <RadioactiveTransportIndex>0</RadioactiveTransportIndex>
            <Standard>IMO</Standard>
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
      <PackingLine>
        <ContainerLink>2</ContainerLink>
        <ContainerNumber>MSCU1247856</ContainerNumber>
        <GoodsDescription>AIR BAG</GoodsDescription>
        <PackQty>3</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>0</Volume>
        <VolumeUnit Description=""Cubic Metres"">M3</VolumeUnit>
        <Weight>2140.0</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>

        <UNDGCollection>
          <UNDG>
            <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
            <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint1}
            <IMOClass>1.4G</IMOClass>
            <MedicalFirstAidGuide>702B</MedicalFirstAidGuide>
            <PackedInExceptedQuantity>false</PackedInExceptedQuantity>
            <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
            <PackingGroup>II</PackingGroup>
            <PackQty>5</PackQty>
            <PackType>BAG</PackType>
            <ProperShippingName>AIR BAG MODULES</ProperShippingName>
            <RadioactiveCriticalitySafetyIndex>0</RadioactiveCriticalitySafetyIndex>
            <RadioactiveTransportIndex>0</RadioactiveTransportIndex>
            <Standard>IMO</Standard>
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
      <PackingLine>
        <ContainerLink>3</ContainerLink>
        <ContainerNumber>MSCU8757656</ContainerNumber>
        <GoodsDescription>DASHBOARD MERCEDES</GoodsDescription>
        <PackQty>5</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>150.00</Volume>
        <VolumeUnit Description=""Cubic Metres"">M3</VolumeUnit>
        <Weight>256.15</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>

        <UNDGCollection>
          <UNDG>
            <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
            <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint1}
            <IMOClass>7</IMOClass>
            <MarinePollutant>Y</MarinePollutant>
            <MedicalFirstAidGuide>702B</MedicalFirstAidGuide>
            <NetExplosiveWeight>17</NetExplosiveWeight>
            <NetExplosiveWeightUQ Description=""Kilograms"">KG</NetExplosiveWeightUQ>
            <PackedInExceptedQuantity>true</PackedInExceptedQuantity>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup>II</PackingGroup>
            <PackQty>5</PackQty>
            <PackType>BOX</PackType>
            <ProperShippingName>RADIOACTIVE MATERIAL, EXCEPTED PACKAGE - LIMITED QUANTITY OF MATERIAL</ProperShippingName>
            <RadioactiveCriticalitySafetyIndex>5</RadioactiveCriticalitySafetyIndex>
            <RadioactiveTransportIndex>7</RadioactiveTransportIndex>
            <Radioactivity>7</Radioactivity>
            <RadioactivityUQ Description=""Curie"">CUR</RadioactivityUQ>
            <Standard>IMO</Standard>
            <State>Gas</State>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName>Misterious revolutionary fuel</TechicalName>
            <UNDGCode>2910</UNDGCode>
            <Volume>0</Volume>
            <VolumeUQ></VolumeUQ>
            <Weight>12.00</Weight>
            <WeightUQ Description=""Kilograms"">KG</WeightUQ>
          </UNDG>
        </UNDGCollection>
      </PackingLine>
    </PackingLineCollection>

    <TransportLegCollection>
      <TransportLeg>
        <PortOfDischarge Name=""Yantian Pt"">CNYTN</PortOfDischarge>
        <PortOfLoading Name=""Antwerpen"">BEANR</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2020-10-19T14:30:00</ActualArrival>
        <ActualDeparture>2020-10-08T16:20:00</ActualDeparture>
        <EstimatedArrival>2020-10-19T13:15:00</EstimatedArrival>
        <EstimatedDeparture>2020-10-08T13:15:00</EstimatedDeparture>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>9337444</VesselLloydsIMO>
        <VesselName>MSC UBERTY</VesselName>
        <VoyageFlightNo>124</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";

		#endregion Expected Export Xml

		#region Expected Resend Xml

		string GetResendExpectedXml(string flashpoint1, string flashpoint2) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001015</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
    </DataContext>

    <BookingConfirmationReference>BKC001007</BookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <WayBillNumber>1122334499</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>DgnSecurityNumber</Key>
        <Value>DGN001001</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>BEANR</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Antwerp</Value>
      </AddInfo>
      <AddInfo>
        <Key>VesselStayReference</Key>
        <Value>V200487</Value>
      </AddInfo>
      <AddInfo>
        <Key>HandlingInstruction</Key>
        <Value>LLO</Value>
      </AddInfo>
      <AddInfo>
        <Key>Main_Vessel_RadioCallSign</Key>
        <Value>A8OR4</Value>
      </AddInfo>
      <AddInfo>
        <Key>Main_Vessel_CountryCode</Key>
        <Value>CN</Value>
      </AddInfo>
      <AddInfo>
        <Key>Main_VesselType_Code</Key>
        <Value>CV</Value>
      </AddInfo>
      <AddInfo>
        <Key>Other_TransportMode</Key>
        <Value>SEA</Value>
      </AddInfo>
      <AddInfo>
        <Key>Other_VesselName</Key>
        <Value>CYGNUS</Value>
      </AddInfo>
      <AddInfo>
        <Key>Other_VesselENINumber</Key>
        <Value>02328823</Value>
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
        <ContainerNumber>MSCU1245787</ContainerNumber>
        <Link>1</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <PalletCount>2</PalletCount>
      </Container>
      <Container>
        <ContainerNumber>MSCU1247856</ContainerNumber>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <PalletCount>5</PalletCount>
      </Container>
      <Container>
        <ContainerNumber>MSCU8757656</ContainerNumber>
        <Link>3</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <PalletCount>3</PalletCount>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>VesselStayStartDate</Type>
        <Value>2020-07-20T01:15:30</Value>
      </Date>
      <Date>
        <Type>VesselStayEndDate</Type>
        <Value>2020-08-02T15:06:21</Value>
      </Date>
      <Date>
        <Type>HandlingDate</Type>
        <Value>2020-10-07T13:15:00</Value>
      </Date>
      <Date>
        <Type>Delivery</Type>
        <Value>2020-10-19T13:15:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>DepartureCTOAddress</AddressType>
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
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>DepartureCTOTerminalId</Value>
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
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingForwarderPortId</Value>
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
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>CarrierPortId</Value>
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
            <Type Description=""Port System Number"">PSN</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingPartyPortId</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""EORI code (to be sent verbatim)"">EOR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>SendingPartyEORI</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection>
      <PackingLine>
        <ContainerLink>1</ContainerLink>
        <ContainerNumber>MSCU1245787</ContainerNumber>
        <GoodsDescription>ROOF Covering</GoodsDescription>
        <PackQty>1</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>0</Volume>
        <VolumeUnit Description=""Cubic Metres"">M3</VolumeUnit>
        <Weight>18</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
      </PackingLine>
      <PackingLine>
        <ContainerLink>1</ContainerLink>
        <ContainerNumber>MSCU1245787</ContainerNumber>
        <GoodsDescription>Goods description packline 2</GoodsDescription>
        <PackQty>1</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>0</Volume>
        <VolumeUnit Description=""Cubic Metres"">M3</VolumeUnit>
        <Weight>453.000</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>

        <UNDGCollection>
          <UNDG>
            <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
            <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint1}
            <IMOClass>1.4G</IMOClass>
            <MedicalFirstAidGuide>702B</MedicalFirstAidGuide>
            <PackedInExceptedQuantity>false</PackedInExceptedQuantity>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup>II</PackingGroup>
            <PackQty>5</PackQty>
            <PackType>BAG</PackType>
            <ProperShippingName>AIR BAG MODULES</ProperShippingName>
            <RadioactiveCriticalitySafetyIndex>0</RadioactiveCriticalitySafetyIndex>
            <RadioactiveTransportIndex>0</RadioactiveTransportIndex>
            <Standard>IMO</Standard>
            <State>Gas</State>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName>Airbag Mercedes C</TechicalName>
            <UNDGCode>0503B</UNDGCode>
            <Volume>0</Volume>
            <VolumeUQ></VolumeUQ>
            <Weight>142.01</Weight>
            <WeightUQ Description=""Kilograms"">KG</WeightUQ>
          </UNDG>
          <UNDG>
            <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
            <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint2}
            <IMOClass>1.4G</IMOClass>
            <MedicalFirstAidGuide>914A</MedicalFirstAidGuide>
            <PackedInExceptedQuantity>false</PackedInExceptedQuantity>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup>III</PackingGroup>
            <PackQty>1</PackQty>
            <PackType>PLT</PackType>
            <ProperShippingName>ROCKETS, LINE-THROWING</ProperShippingName>
            <RadioactiveCriticalitySafetyIndex>0</RadioactiveCriticalitySafetyIndex>
            <RadioactiveTransportIndex>0</RadioactiveTransportIndex>
            <Standard>IMO</Standard>
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
      <PackingLine>
        <ContainerLink>2</ContainerLink>
        <ContainerNumber>MSCU1247856</ContainerNumber>
        <GoodsDescription>AIR BAG</GoodsDescription>
        <PackQty>3</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>0</Volume>
        <VolumeUnit Description=""Cubic Metres"">M3</VolumeUnit>
        <Weight>2140.0</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>

        <UNDGCollection>
          <UNDG>
            <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
            <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint1}
            <IMOClass>1.4G</IMOClass>
            <MedicalFirstAidGuide>702B</MedicalFirstAidGuide>
            <PackedInExceptedQuantity>false</PackedInExceptedQuantity>
            <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
            <PackingGroup>II</PackingGroup>
            <PackQty>5</PackQty>
            <PackType>BAG</PackType>
            <ProperShippingName>AIR BAG MODULES</ProperShippingName>
            <RadioactiveCriticalitySafetyIndex>0</RadioactiveCriticalitySafetyIndex>
            <RadioactiveTransportIndex>0</RadioactiveTransportIndex>
            <Standard>IMO</Standard>
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
      <PackingLine>
        <ContainerLink>3</ContainerLink>
        <ContainerNumber>MSCU8757656</ContainerNumber>
        <GoodsDescription>DASHBOARD MERCEDES</GoodsDescription>
        <PackQty>5</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <Volume>150.00</Volume>
        <VolumeUnit Description=""Cubic Metres"">M3</VolumeUnit>
        <Weight>256.15</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>

        <UNDGCollection>
          <UNDG>
            <EmergencyScheduleFire>F-B</EmergencyScheduleFire>
            <EmergencyScheduleSpillage>S-X</EmergencyScheduleSpillage>
{flashpoint1}
            <IMOClass>7</IMOClass>
            <MarinePollutant>Y</MarinePollutant>
            <MedicalFirstAidGuide>702B</MedicalFirstAidGuide>
            <NetExplosiveWeight>17</NetExplosiveWeight>
            <NetExplosiveWeightUQ Description=""Kilograms"">KG</NetExplosiveWeightUQ>
            <PackedInExceptedQuantity>true</PackedInExceptedQuantity>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup>II</PackingGroup>
            <PackQty>5</PackQty>
            <PackType>BOX</PackType>
            <ProperShippingName>RADIOACTIVE MATERIAL, EXCEPTED PACKAGE - LIMITED QUANTITY OF MATERIAL</ProperShippingName>
            <RadioactiveCriticalitySafetyIndex>5</RadioactiveCriticalitySafetyIndex>
            <RadioactiveTransportIndex>7</RadioactiveTransportIndex>
            <Radioactivity>7</Radioactivity>
            <RadioactivityUQ Description=""Curie"">CUR</RadioactivityUQ>
            <Standard>IMO</Standard>
            <State>Gas</State>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName>Misterious revolutionary fuel</TechicalName>
            <UNDGCode>2910</UNDGCode>
            <Volume>0</Volume>
            <VolumeUQ></VolumeUQ>
            <Weight>12.00</Weight>
            <WeightUQ Description=""Kilograms"">KG</WeightUQ>
          </UNDG>
        </UNDGCollection>
      </PackingLine>
    </PackingLineCollection>

    <TransportLegCollection>
      <TransportLeg>
        <PortOfDischarge Name=""Yantian Pt"">CNYTN</PortOfDischarge>
        <PortOfLoading Name=""Antwerpen"">BEANR</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2020-10-19T14:30:00</ActualArrival>
        <ActualDeparture>2020-10-08T16:20:00</ActualDeparture>
        <EstimatedArrival>2020-10-19T13:15:00</EstimatedArrival>
        <EstimatedDeparture>2020-10-08T13:15:00</EstimatedDeparture>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>9337444</VesselLloydsIMO>
        <VesselName>MSC UBERTY</VesselName>
        <VoyageFlightNo>124</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";

		#endregion
		protected override void SetUp()
		{
			base.SetUp();
			context = new CommonContext(Factory);
		}

		CommonContext context;
	}
}
