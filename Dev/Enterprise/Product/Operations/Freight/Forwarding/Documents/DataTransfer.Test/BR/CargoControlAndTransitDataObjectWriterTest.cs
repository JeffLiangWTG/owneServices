using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Documents.DataTransfer;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.BR;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.BR.Testing
{
	sealed class CargoControlAndTransitDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var cct = new CargoControlAndTransit("S0000001", "ForwardingShipment");

			cct.AirlinePrefix = "081";
			cct.SerialNo = "000000011";
			cct.AWBNumber = "S0000001";

			cct.Shipper = CreateAddressWithCNPJ(nameof(cct.Shipper), "13.339.532/0001-09");
			cct.Consignee = CreateAddressWithCNPJ(nameof(cct.Consignee), "23.339.532/0001-09");
			cct.ImportAgent = CreateAddressWithCNPJ(nameof(cct.ImportAgent), "33.339.532/0001-09");
			cct.ExportAgent = CreateAddress(nameof(cct.ExportAgent));
			cct.Issuer = CreateAddressWithCNPJ(nameof(cct.Issuer), "43.339.532/0001-09");
			cct.SpecialHandling = CreateSpecialHandlingCodes(cct);
			cct.SpecialServiceRequest = "Special Service Request text.Special Service Request text.Special Service Request text.Special Service Request text.Special Service Request text.Special Service Request text.Special Service Request text.";
			cct.OtherServiceInformation = "Other Service Information text.Other Service Information text.Other Service Information text.Other Service Information text.Other Service Information text.Other Service Information text.Other Service Information text.Other Service Information text.";

			cct.PortOfFirstArrival = new DummyUnloco()
			{
				Code = "USLAX",
				Name = "Los Angeles",
			};

			cct.AirportOfDeparture = new DummyCodeDescription
			{
				Code = "SYD",
				Description = "Sydney"
			};

			cct.To1st = new DummyCodeDescription
			{
				Code = "LAX",
				Description = "Los Angeles"
			};

			cct.AirportOfDestination = new DummyCodeDescription
			{
				Code = "SAO",
				Description = "Sao Paulo"
			};

			cct.ReferenceNumber = "C0000001";
			cct.OptionalShippingInformation = "TERMS: FOB";

			cct.Currency = new DummyCodeDescription
			{
				Code = "USD",
				Description = "United States Dollar"
			};

			cct.Charges = new DummyCodeDescription
			{
				Code = "P",
				Description = "Prepaid"
			};

			cct.WeightPrepaidCollect = new DummyCodeDescription
			{
				Code = "P",
				Description = "Prepaid"
			};

			cct.OtherPrepaidCollect = new DummyCodeDescription
			{
				Code = "C",
				Description = "Collect"
			};

			cct.CarriageValue = new DummyMoney
			{
				Amount = 1250,
				Currency = new DummyCodeDescription
				{
					Code = "USD",
					Description = "United States Dollar"
				}
			};

			cct.CustomsValue = new DummyMoney
			{
				Amount = 1000,
				Currency = new DummyCodeDescription
				{
					Code = "USD",
					Description = "United States Dollar"
				}
			};

			cct.InsuranceValue = new DummyMoney
			{
				Amount = 2000,
				Currency = new DummyCodeDescription
				{
					Code = "USD",
					Description = "United States Dollar"
				}
			};

			cct.TotalWeightPPD = 1250;
			cct.TotalWeightCOL = 1251;

			cct.ShippersSignature = "Shipper";
			cct.IssueDate = new ZDateTime(2019, 10, 31);
			cct.IssuePlace = "Sydney";
			cct.AgentsSignature = "Agent";

			cct.RateLines = CreateRateLines();

			cct.TotalPrepaid = cct.TotalWeightPPD + cct.ValuationPPD + cct.TaxesPPD + cct.OtherChargesDueAgentPPD + cct.OtherChargesDueCarrierPPD;
			cct.TotalCollect = cct.TotalWeightCOL + cct.ValuationCOL + cct.TaxesCOL + cct.OtherChargesDueAgentCOL + cct.OtherChargesDueCarrierCOL;
			cct.RUCReferenceNumber = "6BR987654321DOVAHK001";
			cct.CustomsWarehouse = "WHCODE001";
			cct.WoodenParts = ZBool.True;

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new CargoControlAndTransitDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(cct);

			AssertUXml(dataObject, expectedXml);
		}

		CargoControlAndTransitRateLine[] CreateRateLines()
		{
			var rateLinesList = new[]
			{
				new CargoControlAndTransitRateLine
				{
					NoOfPieces = 12,
					GrossWeight = new Measurement
					{
						Value = 1250,
						Unit = new DummyCodeDescription
						{
							Code = "K",
							Description = "Kilogram"
						}
					},
					ChargeableWeight = new Measurement
					{
						Value = 1250,
						Unit = new DummyCodeDescription
						{
							Code = "K",
							Description = "Kilogram"
						}
					},
					CommodityItemNumber = "123",
					RateClass = "Q",
					RateChargeOrDiscount = 0.7,
					Total = 875,
					NatureAndQtyOfGoods = "Goods Description text with dimensions"
				},
				new CargoControlAndTransitRateLine
				{
					NoOfPieces = 8,
					GrossWeight = new Measurement
					{
						Value = 405,
						Unit = new DummyCodeDescription
						{
							Code = "K",
							Description = "Kilogram"
						}
					},
					ChargeableWeight = new Measurement
					{
						Value = 320,
						Unit = new DummyCodeDescription
						{
							Code = "K",
							Description = "Kilogram"
						}
					},
					CommodityItemNumber = "333",
					RateClass = "Q",
					RateChargeOrDiscount = 0.5,
					Total = 75,
					NatureAndQtyOfGoods = "information included VOL 33 M3"
				},
				new CargoControlAndTransitRateLine
				{
					NoOfPieces = 3,
					GrossWeight = new Measurement
					{
						Value = 123,
						Unit = new DummyCodeDescription
						{
							Code = "K",
							Description = "Kilogram"
						}
					},
					ChargeableWeight = new Measurement
					{
						Value = 234,
						Unit = new DummyCodeDescription
						{
							Code = "K",
							Description = "Kilogram"
						}
					},
					CommodityItemNumber = "333",
					RateClass = "Q",
					RateChargeOrDiscount = 0.3,
					Total = 24,
					IsHSCodeLine = true,
					NatureAndQtyOfGoods = "HS Codes: 111111111111111,"
				},
				new CargoControlAndTransitRateLine
				{
					NoOfPieces = 1,
					GrossWeight = new Measurement
					{
						Value = 131,
						Unit = new DummyCodeDescription
						{
							Code = "K",
							Description = "Kilogram"
						}
					},
					ChargeableWeight = new Measurement
					{
						Value = 2,
						Unit = new DummyCodeDescription
						{
							Code = "K",
							Description = "Kilogram"
						}
					},
					CommodityItemNumber = "444",
					RateClass = "Q",
					RateChargeOrDiscount = 1,
					Total = 75,
					IsHSCodeLine = true,
					NatureAndQtyOfGoods = "222222222222222"
				},
				new CargoControlAndTransitRateLine(),
				new CargoControlAndTransitRateLine(),
				new CargoControlAndTransitRateLine(),
				new CargoControlAndTransitRateLine(),
				new CargoControlAndTransitRateLine(),
				new CargoControlAndTransitRateLine(),
				new CargoControlAndTransitRateLine(),
				new CargoControlAndTransitRateLine(),
				new CargoControlAndTransitRateLine()
			};

			return rateLinesList;
		}

		CargoControlAndTransitSpecialHandling[] CreateSpecialHandlingCodes(CargoControlAndTransit cct)
		{
			var specialHandlingCodeList = new AWBSpecialHandlingCodeDescriptionPairList();

			var specialHandlingItems = new[]
			{
				new CargoControlAndTransitSpecialHandling(0)
				{
					CodeAndDescription = new CodeDescription(specialHandlingCodeList)
					{
						Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.AircraftOnGround,
						Description = AWBSpecialHandlingCodeDescriptionPairList.Descriptions.AircraftOnGround
					}
				},
				new CargoControlAndTransitSpecialHandling(1)
				{
					CodeAndDescription = new CodeDescription(specialHandlingCodeList)
					{
						Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CarbonDioxideSolidDryIce,
						Description = AWBSpecialHandlingCodeDescriptionPairList.Descriptions.CarbonDioxideSolidDryIce
					}
				},
				new CargoControlAndTransitSpecialHandling(2)
				{
					CodeAndDescription = new CodeDescription(specialHandlingCodeList)
					{
						Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.MiscellaneousDangerousGoods,
						Description = AWBSpecialHandlingCodeDescriptionPairList.Descriptions.MiscellaneousDangerousGoods
					}
				},
				new CargoControlAndTransitSpecialHandling(3)
				{
					CodeAndDescription = new CodeDescription(specialHandlingCodeList)
					{
						Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
						Description = AWBSpecialHandlingCodeDescriptionPairList.Descriptions.CargoSecureForPassengerAndAllCargoAircraft
					}
				},
				new CargoControlAndTransitSpecialHandling(4)
				{
					CodeAndDescription = new CodeDescription(specialHandlingCodeList)
					{
						Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.LaboratoryAnimals,
						Description = AWBSpecialHandlingCodeDescriptionPairList.Descriptions.LaboratoryAnimals
					}
				},
			};

			return specialHandlingItems;
		}

		Address CreateAddressWithCNPJ(string organizationType, string taxNumber)
		{
			var address = CreateAddress(organizationType);
			address.TaxNumber = taxNumber;
			address.TaxNumberType = new DummyCodeDescription
			{
				Code = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ,
				Description = "CNPJ Cadastro Nacional da Pessoa Jurídica"
			};

			var dummyList = new CodeDescriptionPairList();
			dummyList.AddPair("AAA", "AAA desc");
			dummyList.AddPair("BBB", "BBB desc");

			address.RegistrationNumbers = new[]
			{
				new DummyRegistrationNumber
				{
					Type = new CodeDescription(dummyList)
					{
						Code = "AAA"
					},
					CountryOfIssue = new Country(Context.Factory, Context.Countries)
					{
						Code = "NZ"
					},
					Value = "12345"
				},
				new DummyRegistrationNumber
				{
					Type = new CodeDescription(dummyList)
					{
						Code = "BBB"
					},
					CountryOfIssue = new Country(Context.Factory, Context.Countries)
					{
						Code = "NZ"
					},
					Value = ""
				}
			};

			return address;
		}

		#region Expected Xml

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>ForwardingShipment</Key>
        <Type>S0000001</Type>
      </DataSource>

    </DataContext>

    <GoodsDescription>Goods Description text with dimensions information included VOL 33 M3 HS Codes: 111111111111111, 222222222222222</GoodsDescription>
    <GoodsValue>1250</GoodsValue>
    <GoodsValueCurrency Description=""United States Dollar"">USD</GoodsValueCurrency>
    <InsuranceValue>2000</InsuranceValue>
    <InsuranceValueCurrency Description=""United States Dollar"">USD</InsuranceValueCurrency>
    <PortOfDestination Name=""Sao Paulo"">SAO</PortOfDestination>
    <PortOfFirstArrival Name=""Los Angeles"">USLAX</PortOfFirstArrival>
    <PortOfOrigin Name=""Sydney"">SYD</PortOfOrigin>
    <TotalNoOfPacks>24</TotalNoOfPacks>
    <TotalWeight>1909</TotalWeight>
    <TotalWeightUnit Description=""Kilogram"">KG</TotalWeightUnit>
    <WayBillNumber>S0000001</WayBillNumber>
    <WayBillType Description=""House Waybill"">HWB</WayBillType>

    <CarrierDocumentsOverride>
      <AWBHeader>
        <CargoSecurityDeclaration>
          <SecurityStatus Description=""Cargo Secure for Passenger and All-Cargo Aircraft"">SPX</SecurityStatus>
        </CargoSecurityDeclaration>
        <OtherServiceInformation>Other Service Information text.Other Service Information text.Other Se</OtherServiceInformation>
        <SpecialServiceRequest>Special Service Request text.Special Service Request text.Special Serv</SpecialServiceRequest>
        <SpecialHandlingCollection>
          <SpecialHandling Description=""Aircraft on Ground"">AOG</SpecialHandling>
          <SpecialHandling Description=""Carbon dioxide, solid (dry ice)"">ICE</SpecialHandling>
          <SpecialHandling Description=""Miscellaneous Dangerous Goods"">RMD</SpecialHandling>
          <SpecialHandling Description=""Cargo Secure for Passenger and All-Cargo Aircraft"">SPX</SpecialHandling>
          <SpecialHandling Description=""Laboratory Animals"">SPF</SpecialHandling>
        </SpecialHandlingCollection>
      </AWBHeader>
    </CarrierDocumentsOverride>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>USLAX</Value>
      </AddInfo>
      <AddInfo>
        <Key>To1stCode</Key>
        <Value>LAX</Value>
      </AddInfo>
      <AddInfo>
        <Key>To1stDescription</Key>
        <Value>Los Angeles</Value>
      </AddInfo>
      <AddInfo>
        <Key>MAWB</Key>
        <Value>081-000000011</Value>
      </AddInfo>
      <AddInfo>
        <Key>ReferenceNumber</Key>
        <Value>C0000001</Value>
      </AddInfo>
      <AddInfo>
        <Key>OptionalShippingInformation</Key>
        <Value>TERMS: FOB</Value>
      </AddInfo>
      <AddInfo>
        <Key>ChargesCode</Key>
        <Value>PPD</Value>
      </AddInfo>
      <AddInfo>
        <Key>ChargesDescription</Key>
        <Value>Prepaid</Value>
      </AddInfo>
      <AddInfo>
        <Key>WeightPrepaidCollectCode</Key>
        <Value>PPD</Value>
      </AddInfo>
      <AddInfo>
        <Key>WeightPrepaidCollectDescription</Key>
        <Value>Prepaid</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherPrepaidCollectCode</Key>
        <Value>CLT</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherPrepaidCollectDescription</Key>
        <Value>Collect</Value>
      </AddInfo>
      <AddInfo>
        <Key>CurrencyCode</Key>
        <Value>USD</Value>
      </AddInfo>
      <AddInfo>
        <Key>CurrencyDescription</Key>
        <Value>United States Dollar</Value>
      </AddInfo>
      <AddInfo>
        <Key>CustomsValueAmount</Key>
        <Value>1000.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>CustomsValueCurrencyCode</Key>
        <Value>USD</Value>
      </AddInfo>
      <AddInfo>
        <Key>CustomsValueCurrencyDescription</Key>
        <Value>United States Dollar</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalWeightCOL</Key>
        <Value>1251.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>ValuationCOL</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>TaxesCOL</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherChargesDueAgentCOL</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherChargesDueCarrierCOL</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalCOL</Key>
        <Value>1251.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalWeightPPD</Key>
        <Value>1250.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>ValuationPPD</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>TaxesPPD</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherChargesDueAgentPPD</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherChargesDueCarrierPPD</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalPPD</Key>
        <Value>1250.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>ShippersSignature</Key>
        <Value>Shipper</Value>
      </AddInfo>
      <AddInfo>
        <Key>IssueDate</Key>
        <Value>2019-10-31T00:00:00</Value>
      </AddInfo>
      <AddInfo>
        <Key>IssuePlace</Key>
        <Value>Sydney</Value>
      </AddInfo>
      <AddInfo>
        <Key>AgentsSignature</Key>
        <Value>Agent</Value>
      </AddInfo>
      <AddInfo>
        <Key>CustomsWarehouse</Key>
        <Value>WHCODE001</Value>
      </AddInfo>
      <AddInfo>
        <Key>WoodenParts</Key>
        <Value>Y</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Referência Única da Carga"">RUC</Type>
        <ReferenceNumber>6BR987654321DOVAHK001</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <OrganizationAddressCollection>
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
        <GovRegNum>13.339.532/0001-09</GovRegNum>
        <GovRegNumType Description=""CNPJ Cadastro Nacional da Pessoa Ju"">CJN</GovRegNumType>
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
        <GovRegNum>23.339.532/0001-09</GovRegNum>
        <GovRegNumType Description=""CNPJ Cadastro Nacional da Pessoa Ju"">CJN</GovRegNumType>
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
        <AddressType>ReceivingForwarderAddress</AddressType>
        <AdditionalAddressInformation>ImportAgent additional info</AdditionalAddressInformation>
        <Address1>ImportAgent address line 1</Address1>
        <Address2>ImportAgent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ImportAgent city</City>
        <CompanyName>ImportAgent</CompanyName>
        <Contact>ImportAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ImportAgent email</Email>
        <Fax>ImportAgent fax</Fax>
        <GovRegNum>33.339.532/0001-09</GovRegNum>
        <GovRegNumType Description=""CNPJ Cadastro Nacional da Pessoa Ju"">CJN</GovRegNumType>
        <Phone>ImportAgent phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ImportAgen</Postcode>
        <State>ImportAgent state</State>

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
        <AdditionalAddressInformation>Issuer additional info</AdditionalAddressInformation>
        <Address1>Issuer address line 1</Address1>
        <Address2>Issuer address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Issuer city</City>
        <CompanyName>Issuer</CompanyName>
        <Contact>Issuer contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Issuer email</Email>
        <Fax>Issuer fax</Fax>
        <GovRegNum>43.339.532/0001-09</GovRegNum>
        <GovRegNumType Description=""CNPJ Cadastro Nacional da Pessoa Ju"">CJN</GovRegNumType>
        <Phone>Issuer phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Issuer pos</Postcode>
        <State>Issuer state</State>

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
        <AdditionalAddressInformation>ExportAgent additional info</AdditionalAddressInformation>
        <Address1>ExportAgent address line 1</Address1>
        <Address2>ExportAgent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ExportAgent city</City>
        <CompanyName>ExportAgent</CompanyName>
        <Contact>ExportAgent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ExportAgent email</Email>
        <Fax>ExportAgent fax</Fax>
        <GovRegNum>ExportAgent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ExportAgent phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ExportAgen</Postcode>
        <State>ExportAgent state</State>
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
        <GoodsDescription>Goods Description text with dimensions</GoodsDescription>
        <Link>1</Link>
        <PackQty>12</PackQty>
        <Weight>1250</Weight>
        <WeightUnit Description=""Kilogram"">KG</WeightUnit>

        <AddInfoCollection>
          <AddInfo>
            <Key>RateClass</Key>
            <Value>Q</Value>
          </AddInfo>
          <AddInfo>
            <Key>CommodityItemNumber</Key>
            <Value>123</Value>
          </AddInfo>
          <AddInfo>
            <Key>RateChargeOrDiscount</Key>
            <Value>0.7</Value>
          </AddInfo>
          <AddInfo>
            <Key>Total</Key>
            <Value>875</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightValue</Key>
            <Value>1250.000</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitCode</Key>
            <Value>KG</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitDescription</Key>
            <Value>Kilograms</Value>
          </AddInfo>
        </AddInfoCollection>
        <ClassificationCollection>
          <Classification>
            <Code>111111111111111</Code>
          </Classification>
          <Classification>
            <Code>222222222222222</Code>
          </Classification>
        </ClassificationCollection>
      </PackingLine>
      <PackingLine>
        <GoodsDescription>information included VOL 33 M3</GoodsDescription>
        <Link>2</Link>
        <PackQty>8</PackQty>
        <Weight>405</Weight>
        <WeightUnit Description=""Kilogram"">KG</WeightUnit>

        <AddInfoCollection>
          <AddInfo>
            <Key>RateClass</Key>
            <Value>Q</Value>
          </AddInfo>
          <AddInfo>
            <Key>CommodityItemNumber</Key>
            <Value>333</Value>
          </AddInfo>
          <AddInfo>
            <Key>RateChargeOrDiscount</Key>
            <Value>0.5</Value>
          </AddInfo>
          <AddInfo>
            <Key>Total</Key>
            <Value>160.0</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightValue</Key>
            <Value>320.000</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitCode</Key>
            <Value>KG</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitDescription</Key>
            <Value>Kilograms</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
      <PackingLine>
        <GoodsDescription>HS Codes: 111111111111111,</GoodsDescription>
        <Link>3</Link>
        <PackQty>3</PackQty>
        <Weight>123</Weight>
        <WeightUnit Description=""Kilogram"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>RateClass</Key>
            <Value>Q</Value>
          </AddInfo>
          <AddInfo>
            <Key>CommodityItemNumber</Key>
            <Value>333</Value>
          </AddInfo>
          <AddInfo>
            <Key>RateChargeOrDiscount</Key>
            <Value>0.3</Value>
          </AddInfo>
          <AddInfo>
            <Key>Total</Key>
            <Value>70.2</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightValue</Key>
            <Value>234.000</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitCode</Key>
            <Value>KG</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitDescription</Key>
            <Value>Kilograms</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
      <PackingLine>
        <GoodsDescription>222222222222222</GoodsDescription>
        <Link>4</Link>
        <PackQty>1</PackQty>
        <Weight>131</Weight>
        <WeightUnit Description=""Kilogram"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>RateClass</Key>
            <Value>Q</Value>
          </AddInfo>
          <AddInfo>
            <Key>CommodityItemNumber</Key>
            <Value>444</Value>
          </AddInfo>
          <AddInfo>
            <Key>RateChargeOrDiscount</Key>
            <Value>1</Value>
          </AddInfo>
          <AddInfo>
            <Key>Total</Key>
            <Value>2</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightValue</Key>
            <Value>2.000</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitCode</Key>
            <Value>KG</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitDescription</Key>
            <Value>Kilograms</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";
		#endregion
	}
}
