using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CN.Testing
{
	sealed class EManifestDataObjectWriterTest : DataObjectWriterTest
	{
		#region TestPopulateDataObject

		public void TestPopulateDataObjectWithTaxInfos()
		{
			var eManifest = SetupEManifest();
			PopulateTaxInfos(eManifest);
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new EManifestDataObjectWriter(manager, MessageType.Original);
			var dataObject = writer.GetDataObject(eManifest);

			AssertUXml(dataObject, expectedXmlWithTaxInfo);
		}

		public void TestPopulateAttachedDocuments()
		{
			var eManifest = SetupEManifest();

			var document = new DummyDocument();

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new EManifestDataObjectWriter(manager, MessageType.Original, document);

			eManifest.IsRequiredSendAttachment = false;
			using (var dataObject = writer.GetDataObject(eManifest))
			{
				AssertNull(dataObject.AttachedDocumentCollection);
			}

			eManifest.IsRequiredSendAttachment = true;
			using (var dataObject = writer.GetDataObject(eManifest))
			{
				var attachments = dataObject.AttachedDocumentCollection;
				AssertEquals(1, attachments.Count);

				DataObjectWriterHelperTest.AssertPDFAttachedDocumentsFileAttributes(attachments[0], new DataObjectWriterHelper.FileAttributes()
				{
					Name = "eManifest (CN)",
					Description = "eManifest (CN)",
					Code = "EMN",
					IsPublished = false
				});
			}
		}

		EManifest SetupEManifest()
		{
			var eManifest = new EManifest
				(
					"ForwardingShipment",
					"S00001000",
					"eManifest"
				);

			eManifest.CarrierBookingReference = "BKG0001";
			eManifest.NumberOfOriginals = 1;
			eManifest.NumberOfCopies = 2;
			eManifest.IsDoorPickup = true;
			eManifest.IsDoorDelivery = false;
			eManifest.UseMasterBillAsMasterSO = false;
			eManifest.UseBkgRefAsMasterSO = true;
			eManifest.ShipmentType = new DummyCodeDescription
			{
				Code = "AGT",
				Description = "Agent"
			};
			eManifest.ContainerMode = new DummyCodeDescription
			{
				Code = "FCL",
				Description = "Full Container Load"
			};
			eManifest.ReleaseType = new DummyCodeDescription
			{
				Code = "SWB",
				Description = "Sea Waybill"
			};
			eManifest.PortOfLoad = new DummyUnloco
			{
				Code = "AUAVV",
				Name = "Avalon",
				IATACode = "AVV"
			};
			eManifest.PortOfDischarge = new DummyUnloco
			{
				Code = "CNCAN",
				Name = "Guangzhou",
				IATACode = "CAN"
			};
			eManifest.PlaceOfIssue = new DummyUnloco
			{
				Code = "AUMEL",
				Name = "Melbourne",
				IATACode = "MEL"
			};
			eManifest.PlaceOfReceipt = new DummyUnloco
			{
				Code = "AUBNE",
				Name = "Brisbane",
				IATACode = "BNE"
			};
			eManifest.PlaceOfDelivery = new DummyUnloco
			{
				Code = "AUPER",
				Name = "Perth",
				IATACode = "PER"
			};
			eManifest.PortOfDestination = new DummyUnloco
			{
				Code = "AUSYD",
				Name = "Sydney",
				IATACode = "SYD"
			};

			eManifest.RequestedDateOfIssue = new ZDateTime(2018, 5, 20);
			eManifest.SpecialInstructions = "Instrucciones Especiales";
			eManifest.IsChargesFreighted = true;

			PopulateTransports(eManifest);
			PopulateOrganizations(eManifest);
			PopulateAdditionalReferenceNumbers(eManifest);
			PopulateBookingsAndContainers(eManifest);
			return eManifest;
		}

		void PopulateBookingsAndContainers(EManifest eManifest)
		{
			var factory = new BusinessObjectFactory();
			var context = new CommonContext(factory);

			const string bookingNumber1 = "SL001";
			const string bookingNumber2 = "SL002";

			var container1 = CreateContainer(context, "AAA");
			container1.IsNonOperativeReefer = true;
			var container2 = CreateContainer(context, "BBB");
			container2.IsNonOperativeReefer = false;

			var packline1 = CreatePackingLine("AAA packline 1", bookingNumber1);
			var packline2 = CreatePackingLine("AAA packline 2", bookingNumber2);
			var packline3 = CreatePackingLine("BBB packline 1", bookingNumber1);

			container1.PackingLines = new[]
			{
				packline1,
				packline2
			};

			container2.PackingLines = new[]
			{
				packline3
			};

			eManifest.Containers = new[]
			{
				container1,
				container2
			};

			var booking1 = new Booking(bookingNumber1)
			{
				BookingNumber = bookingNumber1,
				Send = true,
				Containers = new[]
				{
					new BookingContainerBuilder(context, container1, bookingNumber1, container1.PK).Build(),
					new BookingContainerBuilder(context, container2, bookingNumber1, container2.PK).Build()
				}
			};

			var booking2 = new Booking(bookingNumber2)
			{
				BookingNumber = bookingNumber2,
				Send = true,
				Containers = new[]
				{
					new BookingContainerBuilder(context, container1, bookingNumber2, container1.PK).Build()
				}
			};

			eManifest.Bookings = new[]
			{
				booking1,
				booking2
			};
		}

		Container CreateContainer(IContext context, string containerNumber)
		{
			var container = new Container(DefaultDataObjectWriterStrategy.TestInstance);

			container.Number = containerNumber;
			container.Type = new ContainerType(context.ContainerTypes)
			{
				Code = "20FR",
				Type = new CodeDescription(context.ContainerTypes as IFindBoxListProvider)
				{
					Code = "RFG",
					Description = "Refrigerated"
				}
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

			return container;
		}

		PackingLine CreatePackingLine(string goodsDescription, string exportRefNumber)
		{
			var packingLine = new PackingLine(ZGuid.NewZGuid(), Factory);

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
			packingLine.GoodsDescription = goodsDescription;
			packingLine.MarksAndNumbers = "marks & nums";
			packingLine.HarmonizedCode = new HarmonizedCode() { Code = "HC12345" };
			packingLine.ReferenceNumber = "reference number";
			packingLine.ImportReferenceNumber = "import reference number";
			packingLine.ExportReferenceNumber = exportRefNumber;

			var hc = new HarmonizedCode();
			hc.Country = new Country(Factory, new RefCountryCollection(Factory)) { Code = "CN" };
			hc.Code = "1234.56";

			packingLine.HarmonizedCodes = new List<HarmonizedCode>() { hc };

			return packingLine;
		}

		void PopulateTransports(EManifest eManifest)
		{
			var transports = new DummyTransports();

			var main = new DummyTransport
			{
				LegOrder = 1,

				Mode = new DummyCodeDescription
				{
					Code = "SEA",
					Description = "Sea"
				},
				Type = new DummyCodeDescription
				{
					Code = "MAI",
					Description = "Main"
				},
				VoyageFlightNumber = "AAAA",
				Vessel = new DummyVessel
				{
					Name = "Fudge Fixtures",
					LloydsIMO = "IMO111"
				},
				ETD = new ZDateTime(2018, 6, 10),
				ETA = new ZDateTime(2018, 7, 10),
				ATD = new ZDateTime(2018, 6, 10),
				ATA = new ZDateTime(2018, 7, 10),
				PortOfLoading = new DummyUnloco
				{
					Code = "AUSYD",
					Name = "Sydney",
					Country = new DummyCountry
					{
						Code = "AU",
						Name = "Australia"
					},
					IATACode = "SYD"
				},
				PortOfDischarge = new DummyUnloco
				{
					Code = "NZAKL",
					Name = "Auckland",
					Country = new DummyCountry
					{
						Code = "NZ",
						Name = "Kiwi land"
					},
					IATACode = "AKL"
				}
			};

			transports.Elements.Add(main);

			eManifest.Transports = transports;
		}

		void PopulateOrganizations(EManifest eManifest)
		{
			eManifest.SendingAgent = CreateAddress(nameof(eManifest.SendingAgent));
			eManifest.Carrier = CreateAddress(nameof(eManifest.Carrier));
			eManifest.ReceivingAgent = CreateAddress(nameof(eManifest.ReceivingAgent));
			eManifest.CarrierHandlingAgent = CreateAddress(nameof(eManifest.CarrierHandlingAgent));
			eManifest.CarrierBookingAgent = CreateAddress(nameof(eManifest.CarrierBookingAgent));
			eManifest.NotifyParty = CreateAddress(nameof(eManifest.NotifyParty));
			eManifest.NotifyParty2 = CreateAddress(nameof(eManifest.NotifyParty2));
			eManifest.Forwarder = CreateAddress(nameof(eManifest.Forwarder));
			eManifest.PickupFrom = CreateAddress(nameof(eManifest.PickupFrom));
			eManifest.DeliverTo = CreateAddress(nameof(eManifest.DeliverTo));
		}

		void PopulateAdditionalReferenceNumbers(EManifest eManifest)
		{
			eManifest.BillOfLadingNumber = "bill of lading number";
			eManifest.ShipperReference = "shipper reference number";
			eManifest.FreightForwarderReference = "freight forwarder reference number";
			eManifest.CarrierContractNumber = "carrier contract number";
			eManifest.QuotationNumber = "quotation number";
		}

		void PopulateTaxInfos(EManifest eManifest)
		{
			eManifest.SendingAgentTaxInfo = new TaxInfo
			{
				Code = "GCR",
				Description = "Corporate Identification Number",
				Country = new Country(Factory, Context.Countries) { Code = "BE" },
				ShortLabel = "CNO",
				LongLabel = "BELGIAN COMPANY NUMBER",
				Number = "11111"
			};
			eManifest.ReceivingAgentTaxInfo = new TaxInfo
			{
				Code = "GCR",
				Description = "Central Index Key",
				Country = new Country(Factory, Context.Countries) { Code = "BZ" },
				ShortLabel = "CIK",
				Number = "33333"
			};
			eManifest.NotifyPartyTaxInfo = new TaxInfo
			{
				Code = "GCR",
				Description = "Business Identification Number",
				Country = new Country(Factory, Context.Countries) { Code = "FI" },
				LongLabel = "BUSINESS ID",
				Number = "44444"
			};
			eManifest.NotifyParty2TaxInfo = new TaxInfo
			{
				Code = "GCR",
				Description = "Business Identification Number 2",
				Country = new Country(Factory, Context.Countries) { Code = "US" },
				LongLabel = "BUSINESS ID 2",
				Number = "55555"
			};
		}

		#endregion

		#region Expected Xml

		const string expectedXmlWithTaxInfo = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>S00001000</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

    </DataContext>

    <BookingConfirmationReference>BKG0001</BookingConfirmationReference>
    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <DeliveryMode Description=""Door To Peer"">DTP</DeliveryMode>
    <NoCopyBills>2</NoCopyBills>
    <NoOriginalBills>1</NoOriginalBills>
    <PlaceOfDelivery Name=""Perth"">AUPER</PlaceOfDelivery>
    <PlaceOfIssue Name=""Melbourne"">AUMEL</PlaceOfIssue>
    <PlaceOfReceipt Name=""Brisbane"">AUBNE</PlaceOfReceipt>
    <PortOfDestination Name=""Sydney"">AUSYD</PortOfDestination>
    <PortOfDischarge Name=""Guangzhou"">CNCAN</PortOfDischarge>
    <PortOfLoading Name=""Avalon"">AUAVV</PortOfLoading>
    <ReleaseType Description=""Sea Waybill"">SWB</ReleaseType>
    <ShipmentType Description=""Agent"">AGT</ShipmentType>
    <WayBillNumber>bill of lading number</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>1.0.0</Value>
      </AddInfo>
      <AddInfo>
        <Key>MasterSONumber</Key>
        <Value>CBR</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Shipper Reference"">SHP</Type>
        <ReferenceNumber>shipper reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>freight forwarder reference number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Carrier Contract Number"">CON</Type>
        <ReferenceNumber>carrier contract number</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Quotation Number"">QUO</Type>
        <ReferenceNumber>quotation number</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <BillOfLadingClauseCollection>
      <BillOfLadingClause>
        <Type Description=""Freight Collect"">FCL</Type>
      </BillOfLadingClause>
    </BillOfLadingClauseCollection>

    <ContainerCollection>
      <Container>
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>AAA</ContainerNumber>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Refrigerated"">RFG</Category>
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
        <Link>1</Link>
        <NonOperatingReefer>true</NonOperatingReefer>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
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
        <AirVentFlow>12</AirVentFlow>
        <AirVentFlowRateUnit Description=""Cubic feet per minute"">2L</AirVentFlowRateUnit>
        <ArrivalDeliveryRequiredBy></ArrivalDeliveryRequiredBy>
        <ContainerCount>1</ContainerCount>
        <ContainerNumber>BBB</ContainerNumber>
        <ContainerType>
          <Code>20FR</Code>
          <Category Description=""Refrigerated"">RFG</Category>
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
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <Seal>SEAL1</Seal>
        <SealPartyType Description=""Carrier"">CAR</SealPartyType>
        <SecondSeal>SEAL2</SecondSeal>
        <SecondSealPartyType Description=""Customs"">CUS</SecondSealPartyType>
        <TareWeight>20</TareWeight>
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
        <Type>BillRequiredBy</Type>
        <Value>2018-05-20T00:00:00</Value>
      </Date>
    </DateCollection>

    <NoteCollection>
      <Note>
        <Description>Special Instructions</Description>
        <NoteText>Instrucciones Especiales</NoteText>
      </Note>
      <Note>
        <Description>ChargesFreighted</Description>
        <NoteText>Y</NoteText>
      </Note>
    </NoteCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>SendingAgent additional info</AdditionalAddressInformation>
        <Address1>SendingAgent address line 1</Address1>
        <Address2>SendingAgent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SendingAgent city</City>
        <CompanyName>SendingAgent</CompanyName>
        <Contact>SendingAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>SendingAgent email</Email>
        <Fax>SendingAgent fax</Fax>
        <GovRegNum>SendingAgent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>SendingAgent phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>SendingAge</Postcode>
        <State>SendingAgent state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CNO|BELGIAN COMPANY NUMBER"">GCR</Type>
            <CountryOfIssue Name=""Belgium"">BE</CountryOfIssue>
            <Value>11111</Value>
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
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>ReceivingAgent additional info</AdditionalAddressInformation>
        <Address1>ReceivingAgent address line 1</Address1>
        <Address2>ReceivingAgent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ReceivingAgent city</City>
        <CompanyName>ReceivingAgent</CompanyName>
        <Contact>ReceivingAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ReceivingAgent email</Email>
        <Fax>ReceivingAgent fax</Fax>
        <GovRegNum>ReceivingAgent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ReceivingAgent phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ReceivingA</Postcode>
        <State>ReceivingAgent state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIK|"">GCR</Type>
            <CountryOfIssue Name=""Belize"">BZ</CountryOfIssue>
            <Value>33333</Value>
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
        <GovRegNum>NotifyParty tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NotifyPart</Postcode>
        <State>NotifyParty state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""|BUSINESS ID"">GCR</Type>
            <CountryOfIssue Name=""Finland"">FI</CountryOfIssue>
            <Value>44444</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>NotifyParty2</AddressType>
        <AdditionalAddressInformation>NotifyParty2 additional info</AdditionalAddressInformation>
        <Address1>NotifyParty2 address line 1</Address1>
        <Address2>NotifyParty2 address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>NotifyParty2 city</City>
        <CompanyName>NotifyParty2</CompanyName>
        <Contact>NotifyParty2 contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>NotifyParty2 email</Email>
        <Fax>NotifyParty2 fax</Fax>
        <GovRegNum>NotifyParty2 tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>NotifyParty2 phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>NotifyPart</Postcode>
        <State>NotifyParty2 state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""|BUSINESS ID 2"">GCR</Type>
            <CountryOfIssue Name=""United States"">US</CountryOfIssue>
            <Value>55555</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
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
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorPickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>PickupFrom additional info</AdditionalAddressInformation>
        <Address1>PickupFrom address line 1</Address1>
        <Address2>PickupFrom address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>PickupFrom city</City>
        <CompanyName>PickupFrom</CompanyName>
        <Contact>PickupFrom contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>PickupFrom email</Email>
        <Fax>PickupFrom fax</Fax>
        <GovRegNum>PickupFrom tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>PickupFrom phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>PickupFrom</Postcode>
        <State>PickupFrom state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneePickupDeliveryAddress</AddressType>
        <AdditionalAddressInformation>DeliverTo additional info</AdditionalAddressInformation>
        <Address1>DeliverTo address line 1</Address1>
        <Address2>DeliverTo address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>DeliverTo city</City>
        <CompanyName>DeliverTo</CompanyName>
        <Contact>DeliverTo contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>DeliverTo email</Email>
        <Fax>DeliverTo fax</Fax>
        <GovRegNum>DeliverTo tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>DeliverTo phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>DeliverTo </Postcode>
        <State>DeliverTo state</State>

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
            <Key>SL001</Key>
            <Type>Booking</Type>
          </DataSource>

          <DocumentaryOverride>
            <Purpose Description=""Original"">ORG</Purpose>
          </DocumentaryOverride>
        </DataContext>

        <BookingConfirmationReference>SL001</BookingConfirmationReference>

        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>AAA packline 1</DetailedDescription>
            <ExportReferenceNumber>SL001</ExportReferenceNumber>
            <GoodsDescription>AAA packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>
          </PackingLine>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>BBB packline 1</DetailedDescription>
            <ExportReferenceNumber>SL001</ExportReferenceNumber>
            <GoodsDescription>BBB packline 1</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>SL002</Key>
            <Type>Booking</Type>
          </DataSource>

          <DocumentaryOverride>
            <Purpose Description=""Original"">ORG</Purpose>
          </DocumentaryOverride>
        </DataContext>

        <BookingConfirmationReference>SL002</BookingConfirmationReference>

        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber></ContainerNumber>
            <DetailedDescription>AAA packline 2</DetailedDescription>
            <ExportReferenceNumber>SL002</ExportReferenceNumber>
            <GoodsDescription>AAA packline 2</GoodsDescription>
            <HarmonisedCode>HC12345</HarmonisedCode>
            <ImportReferenceNumber>import reference number</ImportReferenceNumber>
            <MarksAndNos>marks &amp; nums</MarksAndNos>
            <OutturnComment></OutturnComment>
            <PackingLineID></PackingLineID>
            <PackQty>3</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <ReferenceNumber>reference number</ReferenceNumber>
            <RequiresTemperatureControl>false</RequiresTemperatureControl>
            <Volume>55</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>88</Weight>
            <WeightUnit>KG</WeightUnit>

            <ClassificationCollection>
              <Classification>
                <Code>1234.56</Code>
                <Country Name=""China"">CN</Country>
                <Type Description=""Harmonized Code"">HSC</Type>
              </Classification>
            </ClassificationCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>

    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
        <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival>2018-07-10T00:00:00</ActualArrival>
        <ActualDeparture>2018-06-10T00:00:00</ActualDeparture>
        <EstimatedArrival>2018-07-10T00:00:00</EstimatedArrival>
        <EstimatedDeparture>2018-06-10T00:00:00</EstimatedDeparture>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>IMO111</VesselLloydsIMO>
        <VesselName>Fudge Fixtures</VesselName>
        <VoyageFlightNo>AAAA</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>";

		#endregion
	}
}
