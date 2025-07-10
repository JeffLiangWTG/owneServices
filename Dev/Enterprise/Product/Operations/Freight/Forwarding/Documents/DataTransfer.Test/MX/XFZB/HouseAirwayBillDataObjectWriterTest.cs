using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Documents.DataTransfer;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.MX;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.MX;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.MX.Testing
{
	sealed class HouseAirwayBillDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var hawb = PrepareTestData();
			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new HouseAirwayBillDataObjectWriter(manager);
			var dataObject = writer.GetDataObject(hawb);

			AssertUXml(dataObject, expectedXml);
		}

		CommonContext context;

		HouseAirwayBill PrepareTestData()
		{
			var hawb = new HouseAirwayBill("S0000001", "ForwardingShipment");

			hawb.AWBNumber = "S0000001";
			hawb.ShippersSignature = "Shippers Signature";
			hawb.IssueDate = new ZDateTime(2021, 04, 09);
			hawb.AgentsSignature = "Agents Signature";
			hawb.IssuePlace = "Somwhere over the rainbow";

			hawb.AirlinePrefix = "081";
			hawb.SerialNo = "000000011";
			hawb.AirportOfDeparture = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "MXCUU",
				Name = "Chihuaha"
			};
			hawb.AirportOfDestination = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "USLAX",
				Name = "Los Angeleas"
			};

			hawb.CarriageValue = new DummyMoney
			{
				Amount = 2000,
				Currency = new DummyCodeDescription
				{
					Code = "USD",
					Description = "United States Dollar"
				}
			};
			hawb.CustomsValue = new DummyMoney
			{
				Amount = 4450,
				Currency = new DummyCodeDescription
				{
					Code = "USD",
					Description = "United States Dollar"
				}
			};
			hawb.InsuranceValue = new DummyMoney
			{
				Amount = 7854,
				Currency = new DummyCodeDescription
				{
					Code = "USD",
					Description = "United States Dollar"
				}
			};
			hawb.TotalWeightPPD = 1234.12;
			hawb.TotalWeightCOL = 546.44;
			hawb.Currency = new DummyCodeDescription
			{
				Code = "USD",
				Description = "United States Dollar"
			};
			hawb.ValuationPPD = 456.44;
			hawb.ValuationCOL = 127.47;
			hawb.TaxesPPD = 45456.54;
			hawb.TaxesCOL = 415.45;
			hawb.OtherChargesDueAgentPPD = 45.45;
			hawb.OtherChargesDueAgentCOL = 78.48;
			hawb.OtherChargesDueCarrierPPD = 89774.4;
			hawb.OtherChargesDueCarrierCOL = 897.54;
			hawb.TotalPrepaid = hawb.TotalWeightPPD + hawb.ValuationPPD + hawb.TaxesPPD + hawb.OtherChargesDueAgentPPD + hawb.OtherChargesDueCarrierPPD;
			hawb.TotalCollect = hawb.TotalWeightCOL + hawb.ValuationCOL + hawb.TaxesCOL + hawb.OtherChargesDueAgentCOL + hawb.OtherChargesDueCarrierCOL;
			hawb.WeightPrepaidCollect = new DummyCodeDescription
			{
				Code = "PPD"
			};
			hawb.OtherPrepaidCollect = new DummyCodeDescription
			{
				Code = "COL"
			};
			hawb.TotalGrossWeight = new Measurement
			{
				Value = 1250,
				Unit = new DummyCodeDescription
				{
					Code = "K",
					Description = "Kilogram"
				}
			};
			hawb.TotalNoOfPieces = 45;
			hawb.SpecialHandling = CreateSpecialHandlingCodes(hawb);

			hawb.SendingParty = CreateAddress(nameof(hawb.SendingParty));
			hawb.SendingPartyCode = CreateRegistrationNumber(OrgCusCode.CodeTypes.PortSystemNumber, "1234");
			hawb.Shipper = CreateAddress(nameof(hawb.Shipper));
			hawb.Consignee = CreateAddress(nameof(hawb.Consignee));
			hawb.ExportAgent = CreateAddress(nameof(hawb.ExportAgent));
			hawb.ExportAgentCode = CreateRegistrationNumber(OrgCusCode.CodeTypes.PortSystemNumber, "4567");

			hawb.RateLines = CreateRateLines();

			return hawb;
		}

		protected override RegistrationNumber CreateRegistrationNumber(string type, string value)
		{
			return new RegistrationNumber()
			{
				CountryOfIssue = new Country(context.Factory, context.Countries)
				{
					Code = Core.Constants.CountryCodes.Mexico
				},
				Type = new CodeDescription(new OrgCodeLists().CustomsCodes_List(Core.Constants.CountryCodes.Mexico))
				{
					Code = type
				},
				Value = value
			};
		}

		HouseAirwayBillRateLine[] CreateRateLines()
		{
			var rateLinesList = new[]
			{
				new HouseAirwayBillRateLine
				{
					GrossWeight = new Measurement
					{
						Value = 1250,
						Unit = new DummyCodeDescription
						{
							Code = "K",
							Description = "Kilogram"
						}
					},
					NoOfPieces = 12,
					NatureAndQtyOfGoods = "Goods Description text with dimensions",
					RateClass = "Q",
					CommodityItemNumber = "123",
					ChargeableWeight = new Measurement
					{
						Value = 1250,
						Unit = new DummyCodeDescription
						{
							Code = "K",
							Description = "Kilogram"
						}
					},
					RateChargeOrDiscount = 0.7,
					Total = 875
				},
				new HouseAirwayBillRateLine
				{
					GrossWeight = new Measurement
					{
						Value = 405,
						Unit = new DummyCodeDescription
						{
							Code = "K",
							Description = "Kilogram"
						}
					},
					NoOfPieces = 8,
					NatureAndQtyOfGoods = "information included VOL 33 M3",
					RateClass = "Q",
					CommodityItemNumber = "333",
					ChargeableWeight = new Measurement
					{
						Value = 320,
						Unit = new DummyCodeDescription
						{
							Code = "K",
							Description = "Kilogram"
						}
					},
					RateChargeOrDiscount = 0.5,
					Total = 75,
				},
				new HouseAirwayBillRateLine(),
				new HouseAirwayBillRateLine(),
				new HouseAirwayBillRateLine(),
				new HouseAirwayBillRateLine(),
				new HouseAirwayBillRateLine(),
				new HouseAirwayBillRateLine(),
				new HouseAirwayBillRateLine(),
				new HouseAirwayBillRateLine(),
				new HouseAirwayBillRateLine(),
				new HouseAirwayBillRateLine(),
				new HouseAirwayBillRateLine()
			};

			return rateLinesList;
		}

		HouseAirwayBillSpecialHandling[] CreateSpecialHandlingCodes(HouseAirwayBill hawb)
		{
			var specialHandlingCodeList = new AWBSpecialHandlingCodeDescriptionPairList();

			var specialHandlingItems = new[]
			{
				new HouseAirwayBillSpecialHandling(0)
				{
					CodeAndDescription = new CodeDescription(specialHandlingCodeList)
					{
						Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.AircraftOnGround,
						Description = AWBSpecialHandlingCodeDescriptionPairList.Descriptions.AircraftOnGround
					}
				},
				new HouseAirwayBillSpecialHandling(1)
				{
					CodeAndDescription = new CodeDescription(specialHandlingCodeList)
					{
						Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CarbonDioxideSolidDryIce,
						Description = AWBSpecialHandlingCodeDescriptionPairList.Descriptions.CarbonDioxideSolidDryIce
					}
				},
				new HouseAirwayBillSpecialHandling(2)
				{
					CodeAndDescription = new CodeDescription(specialHandlingCodeList)
					{
						Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.MiscellaneousDangerousGoods,
						Description = AWBSpecialHandlingCodeDescriptionPairList.Descriptions.MiscellaneousDangerousGoods
					}
				},
				new HouseAirwayBillSpecialHandling(3)
				{
					CodeAndDescription = new CodeDescription(specialHandlingCodeList)
					{
						Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft,
						Description = AWBSpecialHandlingCodeDescriptionPairList.Descriptions.CargoSecureForPassengerAndAllCargoAircraft
					}
				},
				new HouseAirwayBillSpecialHandling(4)
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

		#region Expected Xml

		const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>ForwardingShipment</Key>
        <Type>S0000001</Type>
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

    <GoodsDescription>Goods Description text with dimensions information included VOL 33 M3</GoodsDescription>
    <GoodsValue>2000</GoodsValue>
    <GoodsValueCurrency Description=""United States Dollar"">USD</GoodsValueCurrency>
    <InsuranceValue>7854</InsuranceValue>
    <InsuranceValueCurrency Description=""United States Dollar"">USD</InsuranceValueCurrency>
    <PortOfDestination Name=""Los Angeleas"">USLAX</PortOfDestination>
    <PortOfOrigin Name=""Chihuaha"">MXCUU</PortOfOrigin>
    <TotalNoOfPacks>20</TotalNoOfPacks>
    <TotalWeight>1655</TotalWeight>
    <TotalWeightUnit Description=""Kilogram"">KG</TotalWeightUnit>
    <WayBillNumber>S0000001</WayBillNumber>
    <WayBillType Description=""House Waybill"">HWB</WayBillType>

    <CarrierDocumentsOverride>
      <AWBHeader>
        <CargoSecurityDeclaration>
          <SecurityStatus Description=""Cargo Secure for Passenger and All-Cargo Aircraft"">SPX</SecurityStatus>
        </CargoSecurityDeclaration>

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
        <Key>CustomsValueAmount</Key>
        <Value>4450.000</Value>
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
        <Key>OperationalPort_Code</Key>
        <Value>MXCUU</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Chihuaha</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalWeightCOL</Key>
        <Value>546.440</Value>
      </AddInfo>
      <AddInfo>
        <Key>ValuationCOL</Key>
        <Value>127.470</Value>
      </AddInfo>
      <AddInfo>
        <Key>TaxesCOL</Key>
        <Value>415.450</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherChargesDueAgentCOL</Key>
        <Value>78.480</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherChargesDueCarrierCOL</Key>
        <Value>897.540</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalCOL</Key>
        <Value>2065.380</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalWeightPPD</Key>
        <Value>1234.120</Value>
      </AddInfo>
      <AddInfo>
        <Key>ValuationPPD</Key>
        <Value>456.440</Value>
      </AddInfo>
      <AddInfo>
        <Key>TaxesPPD</Key>
        <Value>45456.540</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherChargesDueAgentPPD</Key>
        <Value>45.450</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherChargesDueCarrierPPD</Key>
        <Value>89774.400</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalPPD</Key>
        <Value>136966.950</Value>
      </AddInfo>
      <AddInfo>
        <Key>MAWB</Key>
        <Value>081-000000011</Value>
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
        <Key>WeightPrepaidCollectCode</Key>
        <Value>PPD</Value>
      </AddInfo>
      <AddInfo>
        <Key>WeightPrepaidCollectDescription</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherPrepaidCollectCode</Key>
        <Value>COL</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherPrepaidCollectDescription</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>ShippersSignature</Key>
        <Value>Shippers Signature</Value>
      </AddInfo>
      <AddInfo>
        <Key>IssueDate</Key>
        <Value>2021-04-09T00:00:00</Value>
      </AddInfo>
      <AddInfo>
        <Key>IssuePlace</Key>
        <Value>Somwhere over the rainbow</Value>
      </AddInfo>
      <AddInfo>
        <Key>AgentsSignature</Key>
        <Value>Agents Signature</Value>
      </AddInfo>
    </AddInfoCollection>

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
            <Value>weightType</Value>
          </AddInfo>
        </AddInfoCollection>
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
            <Value>75</Value>
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
            <Value>weightType</Value>
          </AddInfo>
        </AddInfoCollection>
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
		#endregion
	}
}
