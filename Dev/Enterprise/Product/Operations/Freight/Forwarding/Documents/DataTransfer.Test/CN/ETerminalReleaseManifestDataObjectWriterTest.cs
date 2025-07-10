using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.UniversalDataBuss.DataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CN.Testing
{
	sealed class ETerminalReleaseManifestDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			context = new CommonContext(Factory);
			var terminalRelease = new ETerminalReleaseManifest
(
	"ForwardingConsol",
	"C00001000",
	"eTerminalReleaseManifest"
);

			PopulateGeneralInfo(terminalRelease);
			PopulatePorts(terminalRelease);
			PopulateBookingsAndContainers(terminalRelease);
			PopulateOrganizations(terminalRelease);
			PopulatePaymentInstructions(terminalRelease);
			PopulateAdditionalReferenceNumbers(terminalRelease);

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new ETerminalReleaseManifestDataObjectWriter(manager);

			var dataObject = writer.GetDataObject(terminalRelease);

			AssertUXml(dataObject, expectedXml);
		}

		void PopulateGeneralInfo(ETerminalReleaseManifest terminalRelease)
		{
			var releaseTypes = new ChinaReleaseTypes();
			terminalRelease.ReleaseType = new CodeDescription(releaseTypes) { Code = ChinaReleaseTypes.Codes.SeaWaybill };

			var transport = TransportTest.GetNewTransportUsingReflectionAsCtorIsIntentionallyPrivate();
			transport.VoyageFlightNumber = "VF00001";
			transport.Vessel = new Vessel()
			{
				Name = "Release Vessel",
				LloydsIMO = "IMO1234",
			};

			var transports = new Transports()
			{
				Main = transport
			};

			terminalRelease.Transports = transports;

			terminalRelease.NumberOfOriginals = 1;
			terminalRelease.NumberOfCopies = 2;
			terminalRelease.IsDoorPickup = true;
			terminalRelease.IsDoorDelivery = false;
			terminalRelease.UseMasterBillAsMasterSO = false;
			terminalRelease.UseBkgRefAsMasterSO = true;
			terminalRelease.IsFreightPrepaid = true;
			terminalRelease.RequestedDateOfIssue = new ZDateTime(2018, 08, 07);
			terminalRelease.SpecialInstructions = "Special Instructions A";
		}

		void PopulatePorts(ETerminalReleaseManifest terminalRelease)
		{
			terminalRelease.PortOfLoad = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "CNNGB" };
			terminalRelease.PortOfDischarge = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "AUSYD" };
			terminalRelease.PlaceOfIssue = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "CNNGB" };
			terminalRelease.PlaceOfDelivery = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "AUSYD" };
			terminalRelease.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries) { Code = "CNCAN" };
		}

		CommonContext context;

		void PopulateBookingsAndContainers(ETerminalReleaseManifest eManifest)
		{
			var factory = new BusinessObjectFactory();
			var context = new CommonContext(factory);

			const string bookingNumber1 = "SL001";
			const string bookingNumber2 = "SL002";

			var container1 = CreateContainer(context, "AAA");
			container1.IsNonOperativeReefer = false;
			var container2 = CreateContainer(context, "BBB");
			container1.IsNonOperativeReefer = true;

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
				Send = false,
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

			return packingLine;
		}

		void PopulateOrganizations(ETerminalReleaseManifest eManifest)
		{
			eManifest.Carrier = CreateAddress(nameof(eManifest.Carrier));
			eManifest.Forwarder = CreateAddress(nameof(eManifest.Forwarder));
			eManifest.Shipper = CreateAddress(nameof(eManifest.Shipper));
			eManifest.Consignee = CreateAddress(nameof(eManifest.Consignee));
			eManifest.NotifyParty = CreateAddress(nameof(eManifest.NotifyParty));
			eManifest.NotifyParty2 = CreateAddress(nameof(eManifest.NotifyParty2));
			eManifest.CurrentUser = CreateAddress("CurrentUser");
		}

		void PopulateAdditionalReferenceNumbers(ETerminalReleaseManifest eManifest)
		{
			eManifest.BillOfLadingNumber = "BOL00001";
			eManifest.ShipperReference = "shipper reference number";
			eManifest.FreightForwarderReference = "freight forwarder reference number";
			eManifest.CarrierContractNumber = "carrier contract number";
			eManifest.QuotationNumber = "quotation number";
		}

		void PopulatePaymentInstructions(ETerminalReleaseManifest terminalRelease)
		{
			terminalRelease.OtherCharges = new OtherCharges
			{
				Remarks = "Remarks"
			};

			terminalRelease.FreightPayableAt = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUMEL"
			};

			terminalRelease.IsFreightCollect = true;
		}

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

    </DataContext>

    <LloydsIMO>IMO1234</LloydsIMO>
    <VesselName>Release Vessel</VesselName>
    <VoyageFlightNo>VF00001</VoyageFlightNo>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>CNCAN</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Guangzhou Baiyun International Apt</Value>
      </AddInfo>
    </AddInfoCollection>

    <OrganizationAddressCollection>
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

        <PortOfDischarge Name=""Sydney"">AUSYD</PortOfDischarge>
        <PortOfLoading Name=""Ningbo Lishe International Apt"">CNNGB</PortOfLoading>

        <SubShipmentCollection>
          <SubShipment>

            <DeliveryMode Description=""Door To Peer"">DTP</DeliveryMode>
            <NoCopyBills>2</NoCopyBills>
            <NoOriginalBills>1</NoOriginalBills>
            <PlaceOfDelivery Name=""Sydney"">AUSYD</PlaceOfDelivery>
            <PlaceOfIssue Name=""Ningbo Lishe International Apt"">CNNGB</PlaceOfIssue>
            <ReleaseType Description=""Sea Waybill"">SWB</ReleaseType>
            <WayBillNumber>BOL00001</WayBillNumber>

            <AddInfoCollection>
              <AddInfo>
                <Key>FreightPayableAt_Code</Key>
                <Value>AUMEL</Value>
              </AddInfo>
              <AddInfo>
                <Key>FreightPayableAt_Name</Key>
                <Value>Melbourne</Value>
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
                <Type Description=""Freight Prepaid"">FPP</Type>
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
                <Value>2018-08-07T00:00:00</Value>
              </Date>
            </DateCollection>

            <NoteCollection>
              <Note>
                <Description>Special Instructions</Description>
                <NoteText>Special Instructions A</NoteText>
              </Note>
              <Note>
                <Description>Freight Payable At</Description>
                <NoteText>AUMEL</NoteText>
              </Note>
              <Note>
                <Description>Payment Instruction Remark</Description>
                <NoteText>Remarks</NoteText>
              </Note>
            </NoteCollection>

            <OrganizationAddressCollection>
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
                <AddressType>ConsignorDocumentaryAddress</AddressType>
                <AdditionalAddressInformation>Shipper additional info</AdditionalAddressInformation>
                <Address1>Shipper address line 1</Address1>
                <Address2>Shipper address line 2</Address2>
                <AddressOverride>false</AddressOverride>
                <City>Shipper city</City>
                <CompanyName>Shipper</CompanyName>
                <Contact>Shipper contact</Contact>
                <Country Name=""Australia"">AU</Country>
                <Email>Shipper email</Email>
                <Fax>Shipper fax</Fax>
                <GovRegNum>Shipper tax number</GovRegNum>
                <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
                <Phone>Shipper phone</Phone>
                <Port Name=""Sydney"">AUSYD</Port>
                <Postcode>Shipper po</Postcode>
                <State>Shipper state</State>

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
                <AdditionalAddressInformation>Consignee additional info</AdditionalAddressInformation>
                <Address1>Consignee address line 1</Address1>
                <Address2>Consignee address line 2</Address2>
                <AddressOverride>false</AddressOverride>
                <City>Consignee city</City>
                <CompanyName>Consignee</CompanyName>
                <Contact>Consignee contact</Contact>
                <Country Name=""Australia"">AU</Country>
                <Email>Consignee email</Email>
                <Fax>Consignee fax</Fax>
                <GovRegNum>Consignee tax number</GovRegNum>
                <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
                <Phone>Consignee phone</Phone>
                <Port Name=""Sydney"">AUSYD</Port>
                <Postcode>Consignee </Postcode>
                <State>Consignee state</State>

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
                    <Type Description=""AAA desc"">AAA</Type>
                    <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
                    <Value>12345</Value>
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
                  </PackingLine>
                </PackingLineCollection>
              </SubShipment>
            </SubShipmentCollection>
          </SubShipment>
        </SubShipmentCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";
	}
}
