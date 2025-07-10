using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class ConsolidationAdviceDataObjectWriterTest : DataObjectWriterTest
	{
		readonly CodeDescriptionPairList emptyCodeDescriptionPairList = new CodeDescriptionPairList();

		public void TestPopulateDataObject()
		{
			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var hasFlashPoint = true;
				var consolidationAdvice = PrepareData(hasFlashPoint);

				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new ConsolidationAdviceDataObjectWriter(manager);
				var dataObject = writer.GetDataObject(consolidationAdvice);

				var flashPoint = "<FlashPoint>10</FlashPoint>".PadLeft(43, ' ');
				var expectedXml = GetExpectedXml(flashPoint);

				AssertUXml(dataObject, expectedXml);

				hasFlashPoint = false;
				consolidationAdvice = PrepareData(hasFlashPoint);

				writer = new ConsolidationAdviceDataObjectWriter(manager);
				dataObject = writer.GetDataObject(consolidationAdvice);

				flashPoint = null;
				expectedXml = GetExpectedXml(flashPoint);

				AssertUXml(dataObject, expectedXml);
			}
		}

		public void TestPopulateAttachedDocuments()
		{
			var hasFlashPoint = false;
			var consolidationAdvice = PrepareData(hasFlashPoint);

			var document = new DummyDocument();

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new ConsolidationAdviceDataObjectWriter(manager, document);
			using (var dataObject = writer.GetDataObject(consolidationAdvice))
			{
				var attachments = dataObject.AttachedDocumentCollection;
				AssertEquals(1, attachments.Count);

				DataObjectWriterHelperTest.AssertPDFAttachedDocumentsFileAttributes(attachments[0], new DataObjectWriterHelper.FileAttributes()
				{
					Name = "Consolidation Advice",
					Description = "Consolidation Advice",
					Code = "COA",
					IsPublished = false
				});
			}
		}

		#region Prepare Data

		ConsolidationAdvice PrepareData(bool hasFlashPoint)
		{
			var consolidationAdvice = new ConsolidationAdvice(nameof(ForwardingShipment), "S00001499");

			PopulateGeneralInfo(consolidationAdvice);
			PopulatePorts(consolidationAdvice);
			PopulateOrganizatons(consolidationAdvice);
			PopulateSubShipments(consolidationAdvice);
			PopulateTransports(consolidationAdvice);
			PopulateContainersAndPackingLines(consolidationAdvice, hasFlashPoint);

			return consolidationAdvice;
		}

		void PopulateGeneralInfo(ConsolidationAdvice consolidationAdvice)
		{
			consolidationAdvice.BookingReference = "123";
			consolidationAdvice.MasterBillNumber = "bill of lading number";
			consolidationAdvice.ContainerMode = new CodeDescription(emptyCodeDescriptionPairList)
			{
				Code = "LCL",
				Description = "Less Container Load"
			};
			consolidationAdvice.TransportMode = new CodeDescription(emptyCodeDescriptionPairList)
			{
				Code = "SEA",
				Description = "Sea"
			};
			consolidationAdvice.ShipmentType = new CodeDescription(emptyCodeDescriptionPairList)
			{
				Code = "CLD",
				Description = "Co-Load"
			};
			consolidationAdvice.VesselName = "Vessel";
			consolidationAdvice.LloydsIMO = "V001";
			consolidationAdvice.VoyageNumber = "029N";
			consolidationAdvice.ContractNumber = "C001";
			consolidationAdvice.BookingConfirmationNotes = "Note";
			consolidationAdvice.EstimatedTimeArrival = new ZDateTime(2021, 01, 01);
			consolidationAdvice.EstimatedTimeDeparture = new ZDateTime(2021, 01, 02);
		}

		void PopulateOrganizatons(ConsolidationAdvice consolidationAdvice)
		{
			consolidationAdvice.CurrentUser = CreateAddress("CurrentUser");
			consolidationAdvice.BookingParty = CreateAddress("BookingParty");
			consolidationAdvice.ReceivingForwarder = CreateAddress("ReceivingForwarder");
			consolidationAdvice.SendingForwarder = CreateAddress("SendingForwarder");
		}

		void PopulatePorts(ConsolidationAdvice consolidationAdvice)
		{
			consolidationAdvice.PortOfLoading = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUBNE",
				Name = "Brisbane"
			};

			consolidationAdvice.PortOfDischarge = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "SGSIN",
				Name = "Singapore"
			};

			consolidationAdvice.Origin = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};

			consolidationAdvice.Destination = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "CNSHA",
				Name = "Shanghai"
			};
		}

		void PopulateSubShipments(ConsolidationAdvice consolidationAdvice)
		{
			var subShipment1 = CreateSubShipment("S00001001", "Ref001", "SHP001", "BookingParty1");
			var subShipment2 = CreateSubShipment("S00001002", "Ref002", "SHP002", "BookingParty2");

			var subShipments = new[] { subShipment1, subShipment2 };
			consolidationAdvice.SubShipments = subShipments;
		}

		SubShipment CreateSubShipment(ZString shipmentNumber, ZString shippersRef, ZString messageRef, ZString bookingParty)
		{
			var subShipment = new SubShipment(new ZGuid());
			subShipment.ShipmentNumber = shipmentNumber;
			subShipment.ShippersRef = shippersRef;
			subShipment.MessageRef = messageRef;
			subShipment.BookingParty = CreateAddress(bookingParty);
			return subShipment;
		}

		void PopulateTransports(ConsolidationAdvice consolidationAdvice)
		{
			var transport = Factory.New<Freight.Business.Transport>();
			transport.ParentType = typeof(ForwardingConsol);
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "CNXIA";
			transport.JW_VoyageFlight = "12A";
			transport.JW_ETD = new ZDateTime(2018, 6, 10);
			transport.JW_ETA = new ZDateTime(2018, 12, 1);
			transport.JW_ATD = new ZDateTime(2018, 6, 10);
			transport.JW_ATA = new ZDateTime(2018, 7, 10);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "Titanic";
			vessel.RV_LloydsNumber = "12345";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "12A";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CNXIA";
			voyage.GenerateSailings();

			transport.JW_Vessel = vessel.RV_FK;
			transport.JW_VoyageFlight = "12A";
			transport.JW_JX = voyage.Sailings[0].PK;
			transport.Sailing.JX_DepotReceivalCommences = new ZDateTime(2019, 8, 2);
			transport.Sailing.JX_DepotCutOff = new ZDateTime(2019, 8, 1);

			var transports = Transports.Create(context, new[] { transport });
			consolidationAdvice.Transports = transports;
		}

		void PopulateContainersAndPackingLines(ConsolidationAdvice consolidationAdvice, bool hasFlashPoint)
		{
			var factory = new CargoWise.EntityFramework.BusinessObjectFactory();
			var context = new CommonContext(factory);

			var container1 = CreateContainer(context, "AAA");
			container1.IsNonOperativeReefer = true;
			var container2 = CreateContainer(context, "BBB");
			container2.IsNonOperativeReefer = false;

			var packline1 = CreatePackingLine("AAA packline 1", "AAA", 1, 10, hasFlashPoint);
			var packline2 = CreatePackingLine("AAA BBB packline 2", "AAABBB", 0, 0, hasFlashPoint);
			var packline3 = CreatePackingLine("BBB packline 1", "BBB", -1, 1, hasFlashPoint);

			container1.PackingLines = new[] { packline1, packline2 };
			container2.PackingLines = new[] { packline2, packline3 };

			consolidationAdvice.Containers = new[] { container1, container2 };
		}

		Container CreateContainer(IContext context, string containerNumber)
		{
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);

			container.Number = containerNumber;
			container.Type = new ContainerType(context.ContainerTypes)
			{
				Code = "20FR"
			};
			container.AirVentFlow = new Measurement
			{
				Value = 12,
				Unit = new CodeDescription(context.AirVentFlow)
				{
					Code = "2L"
				}
			};

			container.ContainerCount = 1;
			container.PackCount = 3;
			container.IsEmpty = false;
			container.IsPartOf = false;
			container.IsShipperOwned = true;
			container.Seal = "SEAL1";
			container.ArrivalDeliveryRequiredBy = new ZDateTime(2019, 11, 23);

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
			container.Dunnage = new Measurement()
			{
				Value = 30,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.GrossWeight = new Measurement()
			{
				Value = 272,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			container.Volume = new Measurement()
			{
				Value = 320,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
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

			container.VolumeCapacity = new Measurement()
			{
				Value = 44,
				Unit = new DummyCodeDescription
				{
					Code = "M"
				}
			};

			return container;
		}

		PackingLine CreatePackingLine(string goodsDescription, string containerNumber, Decimal temperatureMinimum, Decimal temperatureMaximum, bool hasFlashPoint)
		{
			var packingLine = new PackingLine(ZGuid.NewZGuid(), Factory);

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
			packingLine.Volume = new Measurement()
			{
				Value = 55,
				Unit = new DummyCodeDescription
				{
					Code = "M3",
				}
			};
			packingLine.Height = new Measurement()
			{
				Value = 56,
				Unit = new DummyCodeDescription
				{
					Code = "M",
				}
			};
			packingLine.Width = new Measurement()
			{
				Value = 57,
				Unit = new DummyCodeDescription
				{
					Code = "M",
				}
			};
			packingLine.Length = new Measurement()
			{
				Value = 58,
				Unit = new DummyCodeDescription
				{
					Code = "M",
				}
			};

			packingLine.GoodsDescription = goodsDescription;
			packingLine.MarksAndNumbers = "marks & nums";
			packingLine.HarmonizedCode = new HarmonizedCode() { Code = "HC12345" };
			packingLine.ReferenceNumber = "reference number";
			packingLine.ImportReferenceNumber = "import reference number";

			var dangerousGoods = new List<DangerousGood>();

			var dangerousGood = new DangerousGood()
			{
				Code = "0001C",
				Unno = "0001",
				Quantity = 11,
				Variant = "C",
				ProperShippingName = "Danger",
				Standard = "IMO",
				TechnicalName = "Technicals",
				IMOClass = "A",
				PackedInLimitedQuantity = true,
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

			var hc = new HarmonizedCode();
			hc.Country = new Country(Factory, new RefCountryCollection(Factory)) { Code = "CN" };
			hc.Code = "1234.56";

			packingLine.HarmonizedCodes = new List<HarmonizedCode>() { hc };

			packingLine.RequiresTemperatureControl = true;
			packingLine.TemperatureMaximum = new Measurement
			{
				Value = temperatureMaximum,
				Unit = new DummyCodeDescription()
				{
					Code = "C"
				}
			};

			packingLine.TemperatureMinimum = new Measurement
			{
				Value = temperatureMinimum,
				Unit = new DummyCodeDescription()
				{
					Code = "C"
				}
			};

			return packingLine;
		}

		#endregion

		#region Expected XML

		string GetExpectedXml(string flashpoint) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>S00001499</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

      <Workflow>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Department"">BRN</EventDepartment>
        <EventUser Name=""CargoWise Support"">E</EventUser>
      </Workflow>
    </DataContext>

    <CoLoadBookingConfirmationReference>123</CoLoadBookingConfirmationReference>
    <CoLoadMasterBillNumber>bill of lading number</CoLoadMasterBillNumber>
    <ContainerMode Description=""Less Container Load"">LCL</ContainerMode>
    <LloydsIMO>V001</LloydsIMO>
    <PortOfDestination Name=""Shanghai"">CNSHA</PortOfDestination>
    <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
    <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ShipmentType Description=""Co-Load"">CLD</ShipmentType>
    <TransportMode Description=""Sea"">SEA</TransportMode>
    <VesselName>Vessel</VesselName>
    <VoyageFlightNo>029N</VoyageFlightNo>

    <AdditionalReferenceCollection Content=""Partial"">
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>C001</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <ContainerCollection>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAA</ContainerNumber>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <NonOperatingReefer>true</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>AAA</ContainerNumber>
            <DetailedDescription>AAA packline 1</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>AAA packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>10</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>1</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>

            <UNDGCollection>
              <UNDG>
{flashpoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IMO</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
          <PackingLine>
            <ContainerNumber>AAABBB</ContainerNumber>
            <DetailedDescription>AAA BBB packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>AAA BBB packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>

            <UNDGCollection>
              <UNDG>
{flashpoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IMO</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </Container>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy>2019-11-23T00:00:00</ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>BBB</ContainerNumber>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Flat Rack"">FLT</Category>
          <Description>Twenty foot flatrack</Description>
          <ISOCode>22P1</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <DunnageWeight>30</DunnageWeight>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GoodsWeight>200</GoodsWeight>
        <GrossWeight>272</GrossWeight>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>true</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <NonOperatingReefer>false</NonOperatingReefer>
        <OverhangBack>39</OverhangBack>
        <OverhangFront>40</OverhangFront>
        <OverhangHeight>41</OverhangHeight>
        <OverhangLeft>42</OverhangLeft>
        <OverhangRight>43</OverhangRight>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
        <TempRecorderSerialNo></TempRecorderSerialNo>
        <ThirdSeal>SEAL3</ThirdSeal>
        <ThirdSealPartyType Description=""Terminal"">CTP</ThirdSealPartyType>
        <VolumeUnit>M</VolumeUnit>
        <WeightUnit>KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>Genset</Key>
            <Value>false</Value>
          </AddInfo>
        </AddInfoCollection>
        <OrganizationAddressCollection>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerNumber>AAABBB</ContainerNumber>
            <DetailedDescription>AAA BBB packline 2</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>AAA BBB packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>0</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>0</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>

            <UNDGCollection>
              <UNDG>
{flashpoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IMO</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
          <PackingLine>
            <ContainerNumber>BBB</ContainerNumber>
            <DetailedDescription>BBB packline 1</DetailedDescription>
            <ExportReferenceNumber></ExportReferenceNumber>
            <GoodsDescription>BBB packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <Height>56</Height>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <Length>58</Length>
            <LengthUnit>M</LengthUnit>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiredTemperatureMaximum>1</RequiredTemperatureMaximum>
            <RequiredTemperatureMinimum>-1</RequiredTemperatureMinimum>
            <RequiredTemperatureUnit>C</RequiredTemperatureUnit>
            <RequiresTemperatureControl>true</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>
            <Width>57</Width>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>

            <UNDGCollection>
              <UNDG>
{flashpoint}
                <IMOClass>A</IMOClass>
                <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <PackQty>11</PackQty>
                <ProperShippingName>Danger</ProperShippingName>
                <Standard>IMO</Standard>
                <SubLabel1></SubLabel1>
                <SubLabel2></SubLabel2>
                <TechicalName>Technicals</TechicalName>
                <UNDGCode>0001</UNDGCode>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2021-01-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2021-01-02T00:00:00</Value>
      </Date>
    </DateCollection>

    <NoteCollection Content=""Partial"">
      <Note>
        <Description>Booking Confirmation Notes</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>Note</NoteText>
      </Note>
    </NoteCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>BookingParty additional info</AdditionalAddressInformation>
        <Address1>BookingParty address line 1</Address1>
        <Address2>BookingParty address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>BookingParty city</City>
        <CompanyName>BookingParty</CompanyName>
        <Contact>BookingParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>BookingParty email</Email>
        <Fax>BookingParty fax</Fax>
        <GovRegNum>BookingParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>BookingParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>BookingPar</Postcode>
        <State>BookingParty state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
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
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
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

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00001001</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <AgentsReference>Ref001</AgentsReference>
        <CoLoadBookingConfirmationReference>S00001001</CoLoadBookingConfirmationReference>

        <AdditionalReferenceCollection Content=""Partial"">
          <AdditionalReference>
            <Type Description=""eHub Interchange Reference"">HIR</Type>
            <ReferenceNumber>SHP001</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>BookingPartyDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>BookingParty1 additional info</AdditionalAddressInformation>
            <Address1>BookingParty1 address line 1</Address1>
            <Address2>BookingParty1 address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>BookingParty1 city</City>
            <CompanyName>BookingParty1</CompanyName>
            <Contact>BookingParty1 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>BookingParty1 email</Email>
            <Fax>BookingParty1 fax</Fax>
            <GovRegNum>BookingParty1 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>BookingParty1 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>BookingPar</Postcode>
            <State>BookingParty1 state</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00001002</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <AgentsReference>Ref002</AgentsReference>
        <CoLoadBookingConfirmationReference>S00001002</CoLoadBookingConfirmationReference>

        <AdditionalReferenceCollection Content=""Partial"">
          <AdditionalReference>
            <Type Description=""eHub Interchange Reference"">HIR</Type>
            <ReferenceNumber>SHP002</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>BookingPartyDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>BookingParty2 additional info</AdditionalAddressInformation>
            <Address1>BookingParty2 address line 1</Address1>
            <Address2>BookingParty2 address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>BookingParty2 city</City>
            <CompanyName>BookingParty2</CompanyName>
            <Contact>BookingParty2 contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>BookingParty2 email</Email>
            <Fax>BookingParty2 fax</Fax>
            <GovRegNum>BookingParty2 tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>BookingParty2 phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>BookingPar</Postcode>
            <State>BookingParty2 state</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type Description=""AAA desc"">AAA</Type>
                <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                <Value>12345</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </SubShipment>
    </SubShipmentCollection>

    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Xian"">CNXIA</PortOfDischarge>
        <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2018-07-10T00:00:00</ActualArrival>
        <ActualDeparture>2018-06-10T00:00:00</ActualDeparture>
        <EstimatedArrival>2018-12-01T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2018-06-10T00:00:00</EstimatedDeparture>
        <LCLCutOff>2019-08-01T00:00:00</LCLCutOff>
        <LCLReceivalCommences>2019-08-02T00:00:00</LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>12345</VesselLloydsIMO>
        <VesselName>Titanic</VesselName>
        <VoyageFlightNo>12A</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			context = new CommonContext(Factory);
		}

		CommonContext context;

		#endregion
	}
}
