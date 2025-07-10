using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing
{
	sealed class HouseBillDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var boleroEBLConfiguration = new BoleroEBLConfiguration()
			{
				EnableEBLIntegration = true,
				GalileoEndPointUrl = "http://test.test",
				GalileoAudience = Guid.NewGuid().ToString(),
				GalileoTestEndPointUrl = "http://test.test",
				GalileoTestAudience = Guid.NewGuid().ToString()
			};

			using (FreightPacksDataRegistry.Instance.ActivateIsCombustibleForDGItems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
			using (FreightDataRegistry.Instance.ConsignorShipperTerminology.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CST123456"))
			{
				var hasFlashPoint = true;
				var houseBill = PrepareTestData(hasFlashPoint);
				var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
				var writer = new HouseBillDataObjectWriter(manager);
				var dataObject = writer.GetDataObject(houseBill);

				AssertNotNull("DataObject has been produced", dataObject);
				AssertEquals("NoOriginalBills", (byte)2, dataObject.NoOriginalBills);
				AssertEquals("NoCopyBills", (byte)3, dataObject.NoCopyBills);

				var flashPoint = "<FlashPoint>10</FlashPoint>".PadLeft(39, ' ');
				var expectedXml = GetExpectedXml(flashPoint);

				AssertUXml(dataObject, expectedXml);

				hasFlashPoint = false;
				houseBill = PrepareTestData(hasFlashPoint);
				writer = new HouseBillDataObjectWriter(manager);
				dataObject = writer.GetDataObject(houseBill);

				AssertNotNull("DataObject has been produced", dataObject);
				AssertEquals("NoOriginalBills", (byte)2, dataObject.NoOriginalBills);
				AssertEquals("NoCopyBills", (byte)3, dataObject.NoCopyBills);

				flashPoint = null;
				expectedXml = GetExpectedXml(flashPoint);

				AssertUXml(dataObject, expectedXml);
				AssertNullOrEmpty(dataObject.CoLoadBookingConfirmationReference);
				AssertNullOrEmpty(dataObject.CoLoadMasterBillNumber);

				houseBill.IsDraft = true;
				writer = new HouseBillDataObjectWriter(manager);
				dataObject = writer.GetDataObject(houseBill);

				AssertEquals("S00001142", dataObject.CoLoadBookingConfirmationReference);
				AssertEquals("HOUSE_BILL_NO12", dataObject.CoLoadMasterBillNumber);
			}
		}

		public void TestPopulateDraftHBLWithAttachment()
		{
			var hasFlashPoint = false;
			var houseBill = PrepareTestData(hasFlashPoint);
			houseBill.IsDraft = true;
			houseBill.IsElectronicBOL = false;

			var document = new DummyDocument();

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new HouseBillDataObjectWriter(manager, document);
			using (var dataObject = writer.GetDataObject(houseBill))
			{
				var attachments = dataObject.AttachedDocumentCollection;
				AssertEquals(1, attachments.Count);

				var attachement = attachments[0];

				DataObjectWriterHelperTest.AssertPDFAttachedDocumentsFileAttributes(attachement, new DataObjectWriterHelper.FileAttributes()
				{
					Name = "Draft Bill (HOUSE_BILL_NO12)",
					Description = "Draft Bill of Lading",
					Code = "DBL",
					IsPublished = false
				});

				CombineAssertions(() =>
				{
					AssertNotNull("ImageData", attachement.ImageData);
					AssertEquals("dataObject.CoLoadBookingConfirmationReference", "S00001142", dataObject.CoLoadBookingConfirmationReference);
					AssertEquals("dataObject.CoLoadMasterBillNumber", "HOUSE_BILL_NO12", dataObject.CoLoadMasterBillNumber);
				});
			}

			houseBill.IsElectronicBOL = true;
			using (var dataObject = writer.GetDataObject(houseBill))
			{
				var attachments = dataObject.AttachedDocumentCollection;
				AssertEquals(0, attachments?.Count ?? 0);
			}
		}

		public void TestPackingLineFilledWhenHouseBillTypeIsFIATA()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HouseBillOfLadingType = "FIA";
			var parameters = new DummyDocDataObjectParameters {	DocumentTitle = "COPY" };

			var loosePackLine = shipment.OuterPackLines.AddNew();
			loosePackLine.JL_PackageCount = 1;
			loosePackLine.JL_Description = "Kids Toys - Lego";
			loosePackLine.JL_PackLineId = "LoosePackline";

			var loosePackLine1 = shipment.OuterPackLines.AddNew();
			loosePackLine1.JL_PackageCount = 2;
			loosePackLine1.JL_Description = "I'm loose baby!";
			loosePackLine1.JL_PackLineId = "LoosePackline1";

			var builder = new HouseBillBuilder(shipment, parameters);
			var houseBill = builder.Build();

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new HouseBillDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(houseBill);
			AssertEquals(2, dataObject.PackingLineCollection.Count);
			
			var contentGoodsDescription = dataObject.PackingLineCollection[0].GoodsDescription;
			AssertEquals("Kids Toys - Lego", contentGoodsDescription);
			AssertEquals(3, dataObject.TotalNoOfPacks);
		}

		CommonContext context;

		protected override void SetUp()
		{
			base.SetUp();
			context = new CommonContext(Factory);
		}

		HouseBill PrepareTestData(bool hasFlashPoint)
		{
			var houseBill = new HouseBill("ForwardingShipment", "S00001142")
			{
				NumberOfOriginals = 2,
				NumberOfCopies = 3
			};

			PopulateContainersAndPackingLines(houseBill, hasFlashPoint);
			PopulateTransports(houseBill);
			PopulateOrganizations(houseBill);

			houseBill.ContainerMode = new DummyCodeDescription { Code = "FCL", Description = "Full Container Load" };
			houseBill.PlaceOfDelivery = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "ZADUR", Name = "Durban" };
			houseBill.PlaceOfIssue = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "ZADUR", Name = "Durban" };
			houseBill.PlaceOfReceipt = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "AUSYD", Name = "Sydney" };
			houseBill.PortOfOrigin = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "AUSYD", Name = "Sydney" };
			houseBill.PortOfLoading = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "AUSYD", Name = "Sydney" };
			houseBill.PortOfDestination = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "CNXIA", Name = "Xian" };
			houseBill.PortOfDischarge = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "CNXIA", Name = "Xian" };
			houseBill.ReleaseType = new DummyCodeDescription { Code = "BOL", Description = "BOL Original" };
			houseBill.TotalPackCount = 5;
			houseBill.TotalPackType = new DummyCodeDescription { Code = "PKG", Description = "Package" };
			houseBill.TotalVolume = new Measurement { Value = 1.2, Unit = new DummyCodeDescription() { Code = "M3", Description = "Cubic Meters" } };
			houseBill.TotalWeight = new Measurement { Value = 5, Unit = new DummyCodeDescription() { Code = "KG", Description = "Kilograms" } };
			houseBill.FreightAmount = new DocDataObjects.Money() { Amount = 20.0m, Currency = new DummyCodeDescription { Code = "AUD", Description = "Australian Dollar" } };
			houseBill.ExcessValueDeclaration = new DocDataObjects.Money() { Amount = 21.0m, Currency = new DummyCodeDescription { Code = "AUD", Description = "Australian Dollar" } };
			houseBill.DeclaredValueOfGoods = new DocDataObjects.Money() { Amount = 20.0m, Currency = new DummyCodeDescription { Code = "AUD", Description = "Australian Dollar" } };
			houseBill.FreightPayableAt = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "ZADUR", Name = "Durban" };
			houseBill.ShipperLoadAndCount = new DummyCodeDescription { Code = "SLC", Description = "Shippers's Load, Stowage and Count" };
			houseBill.GoodsDescription = "Kids Toys - Lego";
			houseBill.HouseBillNumber = "HOUSE_BILL_NO12";
			houseBill.ShipmentNumber = "S00001142";
			houseBill.HouseBillOfLadingType = new DummyCodeDescription { Code = "FIA", Description = "FIATA" };
			houseBill.DateOfIssue = new ZDateTime(2021, 05, 04);
			var dummyShippedOnBoardTypeList = new CodeDescriptionPairList();
			houseBill.ShippedOnBoard = new ShippedOnBoard(dummyShippedOnBoardTypeList)
			{
				Code = "SHP",
				Description = "SHP",
				Date = new ZDateTime(2021, 05, 04)
			};
			houseBill.PaymentTerms = new DummyCodeDescription { Code = "CCX", Description = "Collect" };
			houseBill.MarksAndNumbers = "Marks and numbers XXXXXXXXXXXXXXXXX";
			houseBill.CoLoadMasterBillNumber = "MBL00365";
			houseBill.HIRReference = CreateRegistrationNumber("HIR", "eHub Interchange Reference", "HIR123", "AU");
			houseBill.AmendmentRequestID = "0000001";
			houseBill.INCO = new DummyCodeDescription { Code = Core.Constants.IncoTerms.FreeOnBoard, Description = "Free On Board" };
			houseBill.DepartureDate = new ZDateTime(2025, 04, 16);
			houseBill.ArrivalDate = new ZDateTime(2025, 04, 17);
			houseBill.CTKNumber = "CTK123456";
			houseBill.CustomsEntryNumber = new CustomsEntryNumber
			{
				Value = "CEN123456",
				Type = new DummyCodeDescription
				{
					Code = "CEN",
					Description = "Customs Entry Number"
				}
			};

			houseBill.MoveTypeFrom = "MTF123456";
			houseBill.MoveTypeTo = "MTT123456";
			houseBill.AsAgentOption = new DummyCodeDescription { Code = "AAO123456" };
			houseBill.AsAgentDetail = "AAD123456";
			houseBill.ACIDNO = "ACI123456";

			return houseBill;
		}

		void PopulateContainersAndPackingLines(HouseBill houseBill, bool hasFlashPoint)
		{
			var containers = new List<Container>();

			var container = new Container("1");
			container.ContainerCount = 1;
			container.PackCount = 5;
			container.Number = "QAZX1234563";
			container.Type = new ContainerType(context.ContainerTypes) { Code = "20GP" };
			container.Seal = "SE12541252";

			var packline1 = CreatePackingLine("Kids Toys - Lego", "QAZX1234563", hasFlashPoint);
			container.PackingLines = new[]
			{
				packline1
			};

			containers.Add(container);

			houseBill.Containers = containers;
		}

		RegistrationNumber CreateRegistrationNumber(string type, string description, string value, string country)
		{
			return new RegistrationNumber()
			{
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = country
				},
				Type = new DummyCodeDescription
				{
					Code = type,
					Description = description
				},
				Value = value
			};
		}

		PackingLine CreatePackingLine(string goodsDescription, string containerNumber, bool hasFlashPoint)
		{
			var packingLine = new PackingLine(ZGuid.NewZGuid(), Factory);
			
			packingLine.ContainerNumber = containerNumber;
			packingLine.Quantity = 5;
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

			return packingLine;
		}

		void PopulateTransports(HouseBill houseBill)
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
			houseBill.Transports = transports;
		}

		void PopulateOrganizations(HouseBill houseBill)
		{
			houseBill.Shipper = CreateAddress("ConsignorDocumentaryAddress");
			houseBill.ShipperTaxInfo = CreateTaxInfo();
			houseBill.Consignee = CreateAddress("ConsigneeDocumentaryAddress");
			houseBill.ConsigneeTaxInfo = CreateTaxInfo();
			houseBill.NotifyParty = CreateAddress("NotifyParty");
			houseBill.NotifyPartyTaxInfo = CreateTaxInfo();
			houseBill.GoodsDelivery = CreateAddress("DeliveryAgent");
			houseBill.SendingForwarder = CreateAddress("SendingForwarderAddress");
		}

		string GetExpectedXml(string flashpoint) => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>S00001142</Key>
        <Type>ForwardingShipment</Type>
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

    <ContainerCount>1</ContainerCount>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <GoodsDescription>Kids Toys - Lego</GoodsDescription>
    <HouseBillOfLadingType Description=""FIATA"">FIA</HouseBillOfLadingType>
    <NoCopyBills>3</NoCopyBills>
    <NoOriginalBills>2</NoOriginalBills>
    <PlaceOfDelivery Name=""Durban"">ZADUR</PlaceOfDelivery>
    <PlaceOfIssue Name=""Durban"">ZADUR</PlaceOfIssue>
    <PlaceOfReceipt Name=""Sydney"">AUSYD</PlaceOfReceipt>
    <PortOfDestination Name=""Xian"">CNXIA</PortOfDestination>
    <PortOfDischarge Name=""Xian"">CNXIA</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ReleaseType Description=""BOL Original"">BOL</ReleaseType>
    <ShipmentIncoTerm Description=""Free On Board"">FOB</ShipmentIncoTerm>
    <TotalNoOfPacks>5</TotalNoOfPacks>
    <TotalNoOfPacksPackageType Description=""Package"">PKG</TotalNoOfPacksPackageType>
    <TotalVolume>1.2</TotalVolume>
    <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
    <TotalWeight>5</TotalWeight>
    <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
    <VesselName>Titanic</VesselName>
    <VoyageFlightNo>12A</VoyageFlightNo>
    <WayBillNumber>HOUSE_BILL_NO12</WayBillNumber>
    <WayBillType Description=""House Waybill"">HWB</WayBillType>
    <AddInfoCollection>
      <AddInfo>
        <Key>Freight Amount</Key>
        <Value>20.0</Value>
      </AddInfo>
      <AddInfo>
        <Key>FreightCurrency</Key>
        <Value>AUD</Value>
      </AddInfo>
      <AddInfo>
        <Key>Declared Value</Key>
        <Value>20.0</Value>
      </AddInfo>
      <AddInfo>
        <Key>DeclaredValueCurrency</Key>
        <Value>AUD</Value>
      </AddInfo>
      <AddInfo>
        <Key>Freight Payable at</Key>
        <Value>ZADUR</Value>
      </AddInfo>
      <AddInfo>
        <Key>ExcessValueDeclarationAmount</Key>
        <Value>21.0</Value>
      </AddInfo>
      <AddInfo>
        <Key>ExcessValueDeclarationCurrency</Key>
        <Value>AUD</Value>
      </AddInfo>
      <AddInfo>
        <Key>MoveTypeFrom</Key>
        <Value>MTF123456</Value>
      </AddInfo>
      <AddInfo>
        <Key>MoveTypeTo</Key>
        <Value>MTT123456</Value>
      </AddInfo>
      <AddInfo>
        <Key>AsAgentOption</Key>
        <Value>AAO123456</Value>
      </AddInfo>
      <AddInfo>
        <Key>AsAgentDetail</Key>
        <Value>AAD123456</Value>
      </AddInfo>
      <AddInfo>
        <Key>ConsignorShipperTerminology</Key>
        <Value>CST123456</Value>
      </AddInfo>
      <AddInfo>
        <Key>ACIDNO</Key>
        <Value>ACI123456</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>1.0.0</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""eHub Interchange Reference"">HIR</Type>
        <ReferenceNumber>HIR123</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Cargo Tracking Note"">CTK</Type>
        <ReferenceNumber>CTK123456</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""Shippers's Load, Stowage and Count"">SLC</Type>
      </BillOfLadingClause>
      <BillOfLadingClause>
        <Type Description=""Freight Collect"">FCL</Type>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>
    <ContainerCollection>
      <Container>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>QAZX1234563</ContainerNumber>
        <ContainerType>
          <Code>20GP</Code>
          <Category Description=""Dry Storage"">DRY</Category>
          <Description>Twenty foot general purpose</Description>
          <ISOCode>22G0</ISOCode>
        </ContainerType>
        <DepartureEstimatedPickup></DepartureEstimatedPickup>
        <EmptyRequired></EmptyRequired>
        <ExportDepotCustomsReference></ExportDepotCustomsReference>
        <GrossWeightVerificationDateTime></GrossWeightVerificationDateTime>
        <HumidityPercent>0</HumidityPercent>
        <ImportDepotCustomsReference></ImportDepotCustomsReference>
        <IsControlledAtmosphere>false</IsControlledAtmosphere>
        <IsEmptyContainer>false</IsEmptyContainer>
        <IsShipperOwned>false</IsShipperOwned>
        <LengthUnit Description=""Feet"">FT</LengthUnit>
        <Link>1</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <Seal>SE12541252</Seal>
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
        </OrganizationAddressCollection>
      </Container>
    </ContainerCollection>
    <DateCollection>
      <Date>
        <Type>ShippedOnBoard</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2021-05-04T00:00:00</Value>
      </Date>
      <Date>
        <Type>BillIssued</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2021-05-04T00:00:00</Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2025-04-16T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2025-04-17T00:00:00</Value>
      </Date>
    </DateCollection>
    <EntryNumberCollection>
      <EntryNumber>
        <Number>CEN123456</Number>
        <Type Description=""Customs Entry Number"">CEN</Type>
      </EntryNumber>
    </EntryNumberCollection>
    <NoteCollection>
      <Note>
        <Description>Marks &amp; Numbers</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>Marks and numbers XXXXXXXXXXXXXXXXX</NoteText>
      </Note>
    </NoteCollection>
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
        <GovRegNum>TAX123456</GovRegNum>
        <GovRegNumType Description=""Tax Identification Number"">VAT</GovRegNumType>
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
        <GovRegNum>TAX123456</GovRegNum>
        <GovRegNumType Description=""Tax Identification Number"">VAT</GovRegNumType>
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
        <AddressType>NotifyParty</AddressType>
        <AdditionalAddressInformation>NotifyParty additional info</AdditionalAddressInformation>
        <Address1>NotifyParty address line 1</Address1>
        <Address2>NotifyParty address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NotifyParty city</City>
        <CompanyName>NotifyParty</CompanyName>
        <Contact>NotifyParty contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty email</Email>
        <Fax>NotifyParty fax</Fax>
        <GovRegNum>TAX123456</GovRegNum>
        <GovRegNumType Description=""Tax Identification Number"">VAT</GovRegNumType>
        <Phone>NotifyParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NotifyPart</Postcode>
        <State>NotifyParty state</State>
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
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
        <AdditionalAddressInformation>SendingForwarderAddress additional info</AdditionalAddressInformation>
        <Address1>SendingForwarderAddress address line 1</Address1>
        <Address2>SendingForwarderAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SendingForwarderAddress city</City>
        <CompanyName>SendingForwarderAddress</CompanyName>
        <Contact>SendingForwarderAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingForwarderAddress email</Email>
        <Fax>SendingForwarderAddr</Fax>
        <GovRegNum>SendingForwarderAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingForwarderAddr</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SendingFor</Postcode>
        <State>SendingForwarderAddress s</State>
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
        <ContainerLink>1</ContainerLink>
        <ContainerNumber>QAZX1234563</ContainerNumber>
        <DetailedDescription>Kids Toys - Lego</DetailedDescription>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription>Kids Toys - Lego</GoodsDescription>
        <HarmonisedCode>HC12345</HarmonisedCode>
        <Height>56</Height>
        <ImportReferenceNumber>import reference number</ImportReferenceNumber>
        <Length>58</Length>
        <LengthUnit>M</LengthUnit>
        <MarksAndNos>marks &amp; nums</MarksAndNos>
        <OutturnComment></OutturnComment>
        <PackingLineID></PackingLineID>
        <PackQty>5</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber>reference number</ReferenceNumber>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>55</Volume>
        <VolumeUnit>M3</VolumeUnit>
        <Weight>88</Weight>
        <WeightUnit>KG</WeightUnit>
        <Width>57</Width>
        <UNDGCollection>
          <UNDG>
{flashpoint}
            <IMOClass>A</IMOClass>
            <PackedInLimitedQuantity>true</PackedInLimitedQuantity>
            <PackingGroup></PackingGroup>
            <PackQty>11</PackQty>
            <ProperShippingName>Danger</ProperShippingName>
            <Standard></Standard>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName>Technicals</TechicalName>
            <UNDGCode>0001</UNDGCode>
          </UNDG>
        </UNDGCollection>
      </PackingLine>
    </PackingLineCollection>
    <PaymentHandlingInstructionCollection>
      <PaymentHandlingInstruction>
        <Category Description=""Freight"">FRT</Category>
        <PaymentMethod Description=""Collect"">CCX</PaymentMethod>
      </PaymentHandlingInstruction>
    </PaymentHandlingInstructionCollection>
  </Shipment>
</UniversalShipment>
";
	}
}
