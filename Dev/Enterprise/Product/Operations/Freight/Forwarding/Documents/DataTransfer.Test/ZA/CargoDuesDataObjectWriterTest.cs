using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataTransfer;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.ZA;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.Testing.DataTransfer.ZA
{
	sealed class CargoDuesDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject_Containerised()
		{
			var cargoDues = PrepareTestData();
			cargoDues.IsContainerised = true;

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new CargoDuesDataObjectWriter(manager);

			var dataObject = writer.GetDataObject(cargoDues);

			AssertUXml(dataObject, expectedContainerisedXml);
		}

		public void TestPopulateDataObject_NotContainerised()
		{
			var cargoDues = PrepareTestData();
			cargoDues.IsContainerised = false;

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new CargoDuesDataObjectWriter(manager);

			var dataObject = writer.GetDataObject(cargoDues);

			AssertUXml(dataObject, expectedNotContainerisedXml);
		}

		#region PrepareTestData

		CargoDues PrepareTestData()
		{
			var cargoDues = new CargoDues
			(
				"JobDeclaration",
				"B00001000",
				"CargoDuesBrokerage"
			);
			var emptyCodeDescriptionPairList = new CodeDescriptionPairList();
			cargoDues.WayBillNumber = "BKG0001";
			cargoDues.ContainerMode = new CodeDescription(emptyCodeDescriptionPairList)
			{
				Code = "FCL",
				Description = "Full Container Load"
			};

			cargoDues.CarrierCode = "CK01";
			cargoDues.ContainerOperator = "John";
			cargoDues.Terminal = "T02";
			cargoDues.TNPAOrderNumber = "TNPA01";
			cargoDues.TNPAArrivalNumber = "TNPA02";
			cargoDues.TNPAQuotationNumber = "TNPA03";
			cargoDues.TNPAAccountNumber = "TNPA04";
			cargoDues.RadioCallSign = "R01";
			cargoDues.TotalNumberOfPacks = 10;
			cargoDues.SubTotal = 9.25m;
			cargoDues.VAT = 10.36m;
			cargoDues.TotalR = 10.28m;
			cargoDues.CancellingOrder = true;
			cargoDues.IsTranship = true;
			cargoDues.Eta = new ZDateTime(2020, 1, 21);
			cargoDues.Etd = new ZDateTime(2020, 1, 19);

			PopulatePorts(cargoDues);
			PopulateOrganizations(cargoDues);
			PopulateGoodsInfo(cargoDues);
			PopulateTransports(cargoDues);

			return cargoDues;
		}

		void PopulatePorts(CargoDues cargoDues)
		{
			cargoDues.PortOfLoading = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUBNE",
				Name = "Brisbane"
			};
			cargoDues.PortOfDischarge = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "SGSIN",
				Name = "Singapore"
			};

			cargoDues.ServicePort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};

			cargoDues.PlaceOfReceipt = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUBNE",
				Name = "Brisbane"
			};
			cargoDues.PlaceOfDelivery = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUPER",
				Name = "Perth"
			};
		}

		void PopulateOrganizations(CargoDues cargoDues)
		{
			cargoDues.Shipper = CreateAddress("Shipper");
			cargoDues.Consignee = CreateAddress("Consignee");
			cargoDues.Agent = CreateAddress("Agent");
			cargoDues.ShippingLine = CreateAddress("ShippingLine");
			cargoDues.ArrivalCTO = CreateAddress("ArrivalCTO");
			cargoDues.DepartureCTO = CreateAddress("DepartureCTO");
			cargoDues.CustomsContainerTerminalOperator = CreateAddress("CustomsCTO");
			cargoDues.CurrentUser = CreateAddress("CurrentUser");
		}

		void PopulateGoodsInfo(CargoDues cargoDues)
		{
			var goodsInfos = new List<GoodsInfo>();
			var shipments = new List<ShipmentPackingInfo>();

			goodsInfos.Add(new GoodsInfo(ZGuid.NewZGuid())
			{
				MarksAndNos = "M1",
				NumberOfPacks = 10,
				PackType = "20GP",
				GoodsDescription = "G1",
				GrossMass = new Measurement()
				{
					Value = 10.23m,
					Unit = new CodeDescription(new CodeDescriptionPairList())
					{
						Code = "KG"
					}
				}
			});

			shipments.Add(new ShipmentPackingInfo(ZGuid.NewZGuid())
			{
				TotalWeight = new Measurement()
				{
					Value = 20.5m,
					Unit = new CodeDescription(new CodeDescriptionPairList())
					{
						Code = "KG"
					}
				},
				OuterPacks = 5,
				PackType = new CodeDescription(new CodeDescriptionPairList())
				{
					Code = "PKG"
				},
				Consignee = CreateAddress("SHPConsignee"),
				Consignor = CreateAddress("SHPConsignee"),
				GoodsInfoCollection = goodsInfos
			});

			cargoDues.ShipmentPackingInfos = shipments;

			var container1 = CreateContainer(context, "AAA");
			container1.IsNonOperativeReefer = true;
			var container2 = CreateContainer(context, "BBB");
			container2.IsNonOperativeReefer = false;

			cargoDues.Containers = new[]
			{
				container1,
				container2
			};
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

		void PopulateTransports(CargoDues cargoDues)
		{
			var transport1 = Factory.New<Freight.Business.Transport>();
			transport1.ParentType = typeof(ForwardingConsol);
			transport1.JW_LegOrder = 1;
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transport1.JW_RL_NKLoadPort = "FRPRA";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_Vessel = "Dragon";
			transport1.JW_VoyageFlight = "111";
			transport1.JW_ETD = new ZDateTime(2018, 12, 1);

			var transport2 = Factory.New<Freight.Business.Transport>();
			transport2.ParentType = typeof(ForwardingConsol);
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_Vessel = "Steven";
			transport2.JW_VoyageFlight = "222";

			var arrivalAt = Factory.New<OrgHeader>();
			arrivalAt.OH_FullName = "MAERSK";
			arrivalAt.OH_RL_NKClosestPort = "DKAAL";
			arrivalAt.MainAddress.Address1 = "Unit 13";
			arrivalAt.MainAddress.Address2 = "4 Lost Lane";
			arrivalAt.MainAddress.City = "Aalborg";
			arrivalAt.MainAddress.Postcode = "2000";
			arrivalAt.MainAddress.OA_RN_NKCountryCode = "DK";
			arrivalAt.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.TNP, "10001", Core.Constants.CountryCodes.SouthAfrica);

			var departureFrom = Factory.New<OrgHeader>();
			departureFrom.OH_FullName = "FREIGH";
			departureFrom.OH_RL_NKClosestPort = "AUSYD";
			departureFrom.MainAddress.Address1 = "Unit 18";
			departureFrom.MainAddress.Address2 = "Yin Long";
			departureFrom.MainAddress.City = "Sydney";
			departureFrom.MainAddress.Postcode = "3000";
			departureFrom.MainAddress.OA_RN_NKCountryCode = "CN";
			departureFrom.CustomsCodes.AddNew(OrgCusCode.SouthAfricaCodeTypes.TNP, "10001", Core.Constants.CountryCodes.SouthAfrica);

			transport2.JW_OA_ArrivalLocation = arrivalAt.MainAddress.PK;
			transport2.JW_OA_DepartureLocation = departureFrom.MainAddress.PK;

			var transports = Transports.Create(context, new[] { transport1, transport2 });
			cargoDues.Transports = transports;
		}

		#endregion

		#region ExpectedXml

		const string expectedContainerisedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>B00001000</Key>
        <Type>JobDeclaration</Type>
      </DataSource>
    </DataContext>

    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <PlaceOfDelivery Name=""Perth"">AUPER</PlaceOfDelivery>
    <PlaceOfReceipt Name=""Brisbane"">AUBNE</PlaceOfReceipt>
    <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
    <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
    <TotalNoOfPacks>10</TotalNoOfPacks>
    <WayBillNumber>BKG0001</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>CarrierCode</Key>
        <Value>CK01</Value>
      </AddInfo>
      <AddInfo>
        <Key>ServicePort_Code</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>ServicePort_Name</Key>
        <Value>Sydney</Value>
      </AddInfo>
      <AddInfo>
        <Key>ContainerOperator</Key>
        <Value>John</Value>
      </AddInfo>
      <AddInfo>
        <Key>Terminal</Key>
        <Value>T02</Value>
      </AddInfo>
      <AddInfo>
        <Key>TNPAArrivalNumber</Key>
        <Value>TNPA02</Value>
      </AddInfo>
      <AddInfo>
        <Key>TNPAOrderNumber</Key>
        <Value>TNPA01</Value>
      </AddInfo>
      <AddInfo>
        <Key>TNPAQuotationNumber</Key>
        <Value>TNPA03</Value>
      </AddInfo>
      <AddInfo>
        <Key>TNPAAccountNumber</Key>
        <Value>TNPA04</Value>
      </AddInfo>
      <AddInfo>
        <Key>RadioCallSign</Key>
        <Value>R01</Value>
      </AddInfo>
      <AddInfo>
        <Key>SubTotal</Key>
        <Value>9.25</Value>
      </AddInfo>
      <AddInfo>
        <Key>VAT</Key>
        <Value>10.36</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalR</Key>
        <Value>10.28</Value>
      </AddInfo>
      <AddInfo>
        <Key>CancellingOrder</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsTranship</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Sydney</Value>
      </AddInfo>
    </AddInfoCollection>

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
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-01-21T00:00:00</Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-01-19T00:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Agent additional info</AdditionalAddressInformation>
        <Address1>Agent address line 1</Address1>
        <Address2>Agent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Agent city</City>
        <CompanyName>Agent</CompanyName>
        <Contact>Agent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Agent email</Email>
        <Fax>Agent fax</Fax>
        <GovRegNum>Agent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Agent phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Agent post</Postcode>
        <State>Agent state</State>

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
        <AdditionalAddressInformation>Agent additional info</AdditionalAddressInformation>
        <Address1>Agent address line 1</Address1>
        <Address2>Agent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Agent city</City>
        <CompanyName>Agent</CompanyName>
        <Contact>Agent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Agent email</Email>
        <Fax>Agent fax</Fax>
        <GovRegNum>Agent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Agent phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Agent post</Postcode>
        <State>Agent state</State>

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
        <AdditionalAddressInformation>ShippingLine additional info</AdditionalAddressInformation>
        <Address1>ShippingLine address line 1</Address1>
        <Address2>ShippingLine address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ShippingLine city</City>
        <CompanyName>ShippingLine</CompanyName>
        <Contact>ShippingLine contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ShippingLine email</Email>
        <Fax>ShippingLine fax</Fax>
        <GovRegNum>ShippingLine tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ShippingLine phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ShippingLi</Postcode>
        <State>ShippingLine state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
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
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCTOAddress</AddressType>
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
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CustomsContainerTerminalOperatorAddress</AddressType>
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

    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
        <PortOfLoading Name=""Prauthoy"">FRPRA</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival></ActualArrival>
        <ActualDeparture></ActualDeparture>
        <EstimatedArrival></EstimatedArrival>
        <EstimatedDeparture>2018-12-01T00:00:00</EstimatedDeparture>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO></VesselLloydsIMO>
        <VesselName>Dragon</VesselName>
        <VoyageFlightNo>111</VoyageFlightNo>
      </TransportLeg>
      <TransportLeg>
        <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
        <PortOfLoading Name=""Singapore"">SGSIN</PortOfLoading>
        <LegOrder>2</LegOrder>
        <ActualArrival></ActualArrival>
        <ActualDeparture></ActualDeparture>
        <ArrivalCTO>
          <AddressType>ArrivalCTOAddress</AddressType>
          <AdditionalAddressInformation></AdditionalAddressInformation>
          <Address1>Unit 13</Address1>
          <Address2>4 Lost Lane</Address2>
          <AddressOverride>false</AddressOverride>
          <City>Aalborg</City>
          <CompanyName>MAERSK</CompanyName>
          <Contact></Contact>
          <Country Name=""Denmark"">DK</Country>
          <Email></Email>
          <Fax></Fax>
          <GovRegNum></GovRegNum>
          <Phone></Phone>
          <Port Name=""Aalborg"">DKAAL</Port>
          <Postcode>2000</Postcode>
          <State></State>

          <RegistrationNumberCollection>
            <RegistrationNumber>
              <Type Description=""TNPA Registration Number"">TNP</Type>
              <CountryOfIssue Name=""South Africa"">ZA</CountryOfIssue>
              <Value>10001</Value>
            </RegistrationNumber>
          </RegistrationNumberCollection>
        </ArrivalCTO>
        <DepartureCTO>
          <AddressType>DepartureCTOAddress</AddressType>
          <AdditionalAddressInformation></AdditionalAddressInformation>
          <Address1>Unit 18</Address1>
          <Address2>Yin Long</Address2>
          <AddressOverride>false</AddressOverride>
          <City>Sydney</City>
          <CompanyName>FREIGH</CompanyName>
          <Contact></Contact>
          <Country Name=""China"">CN</Country>
          <Email></Email>
          <Fax></Fax>
          <GovRegNum></GovRegNum>
          <Phone></Phone>
          <Port Name=""Sydney"">AUSYD</Port>
          <Postcode>3000</Postcode>
          <State>NSW</State>

          <RegistrationNumberCollection>
            <RegistrationNumber>
              <Type Description=""TNPA Registration Number"">TNP</Type>
              <CountryOfIssue Name=""South Africa"">ZA</CountryOfIssue>
              <Value>10001</Value>
            </RegistrationNumber>
          </RegistrationNumberCollection>
        </DepartureCTO>
        <EstimatedArrival></EstimatedArrival>
        <EstimatedDeparture></EstimatedDeparture>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LegType>Other</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO></VesselLloydsIMO>
        <VesselName>Steven</VesselName>
        <VoyageFlightNo>222</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>";

		const string expectedNotContainerisedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>B00001000</Key>
        <Type>JobDeclaration</Type>
      </DataSource>
    </DataContext>

    <ContainerMode Description=""Full Container Load"">FCL</ContainerMode>
    <PlaceOfDelivery Name=""Perth"">AUPER</PlaceOfDelivery>
    <PlaceOfReceipt Name=""Brisbane"">AUBNE</PlaceOfReceipt>
    <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
    <PortOfLoading Name=""Brisbane"">AUBNE</PortOfLoading>
    <TotalNoOfPacks>10</TotalNoOfPacks>
    <WayBillNumber>BKG0001</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>CarrierCode</Key>
        <Value>CK01</Value>
      </AddInfo>
      <AddInfo>
        <Key>ServicePort_Code</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>ServicePort_Name</Key>
        <Value>Sydney</Value>
      </AddInfo>
      <AddInfo>
        <Key>ContainerOperator</Key>
        <Value>John</Value>
      </AddInfo>
      <AddInfo>
        <Key>Terminal</Key>
        <Value>T02</Value>
      </AddInfo>
      <AddInfo>
        <Key>TNPAArrivalNumber</Key>
        <Value>TNPA02</Value>
      </AddInfo>
      <AddInfo>
        <Key>TNPAOrderNumber</Key>
        <Value>TNPA01</Value>
      </AddInfo>
      <AddInfo>
        <Key>TNPAQuotationNumber</Key>
        <Value>TNPA03</Value>
      </AddInfo>
      <AddInfo>
        <Key>TNPAAccountNumber</Key>
        <Value>TNPA04</Value>
      </AddInfo>
      <AddInfo>
        <Key>RadioCallSign</Key>
        <Value>R01</Value>
      </AddInfo>
      <AddInfo>
        <Key>SubTotal</Key>
        <Value>9.25</Value>
      </AddInfo>
      <AddInfo>
        <Key>VAT</Key>
        <Value>10.36</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalR</Key>
        <Value>10.28</Value>
      </AddInfo>
      <AddInfo>
        <Key>CancellingOrder</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsTranship</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Sydney</Value>
      </AddInfo>
    </AddInfoCollection>

    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-01-21T00:00:00</Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-01-19T00:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation>Agent additional info</AdditionalAddressInformation>
        <Address1>Agent address line 1</Address1>
        <Address2>Agent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Agent city</City>
        <CompanyName>Agent</CompanyName>
        <Contact>Agent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Agent email</Email>
        <Fax>Agent fax</Fax>
        <GovRegNum>Agent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Agent phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Agent post</Postcode>
        <State>Agent state</State>

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
        <AdditionalAddressInformation>Agent additional info</AdditionalAddressInformation>
        <Address1>Agent address line 1</Address1>
        <Address2>Agent address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Agent city</City>
        <CompanyName>Agent</CompanyName>
        <Contact>Agent contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>Agent email</Email>
        <Fax>Agent fax</Fax>
        <GovRegNum>Agent tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>Agent phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>Agent post</Postcode>
        <State>Agent state</State>

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
        <AdditionalAddressInformation>ShippingLine additional info</AdditionalAddressInformation>
        <Address1>ShippingLine address line 1</Address1>
        <Address2>ShippingLine address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>ShippingLine city</City>
        <CompanyName>ShippingLine</CompanyName>
        <Contact>ShippingLine contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>ShippingLine email</Email>
        <Fax>ShippingLine fax</Fax>
        <GovRegNum>ShippingLine tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>ShippingLine phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>ShippingLi</Postcode>
        <State>ShippingLine state</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
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
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCTOAddress</AddressType>
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
            <Type Description=""AAA desc"">AAA</Type>
            <CountryOfIssue Name=""New Zealand"">NZ</CountryOfIssue>
            <Value>12345</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CustomsContainerTerminalOperatorAddress</AddressType>
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
        <OuterPacks>5</OuterPacks>
        <OuterPacksPackageType>PKG</OuterPacksPackageType>
        <TotalWeight>20.5</TotalWeight>
        <TotalWeightUnit>KG</TotalWeightUnit>
        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsigneeDocumentaryAddress</AddressType>
            <AdditionalAddressInformation>SHPConsignee additional info</AdditionalAddressInformation>
            <Address1>SHPConsignee address line 1</Address1>
            <Address2>SHPConsignee address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SHPConsignee city</City>
            <CompanyName>SHPConsignee</CompanyName>
            <Contact>SHPConsignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>SHPConsignee email</Email>
            <Fax>SHPConsignee fax</Fax>
            <GovRegNum>SHPConsignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>SHPConsignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SHPConsign</Postcode>
            <State>SHPConsignee state</State>
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
            <AdditionalAddressInformation>SHPConsignee additional info</AdditionalAddressInformation>
            <Address1>SHPConsignee address line 1</Address1>
            <Address2>SHPConsignee address line 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SHPConsignee city</City>
            <CompanyName>SHPConsignee</CompanyName>
            <Contact>SHPConsignee contact</Contact>
            <Country Name=""Australia"">AU</Country>
            <Email>SHPConsignee email</Email>
            <Fax>SHPConsignee fax</Fax>
            <GovRegNum>SHPConsignee tax number</GovRegNum>
            <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
            <Phone>SHPConsignee phone</Phone>
            <Port Name=""Sydney"">AUSYD</Port>
            <Postcode>SHPConsign</Postcode>
            <State>SHPConsignee state</State>
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
            <GoodsDescription>G1</GoodsDescription>
            <MarksAndNos>M1</MarksAndNos>
            <PackQty>10</PackQty>
            <PackType>20G</PackType>
            <Weight>10.23</Weight>
            <WeightUnit>KG</WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>

    <TransportLegCollection Content=""Complete"">
      <TransportLeg>
        <PortOfDischarge Name=""Singapore"">SGSIN</PortOfDischarge>
        <PortOfLoading Name=""Prauthoy"">FRPRA</PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualArrival></ActualArrival>
        <ActualDeparture></ActualDeparture>
        <EstimatedArrival></EstimatedArrival>
        <EstimatedDeparture>2018-12-01T00:00:00</EstimatedDeparture>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO></VesselLloydsIMO>
        <VesselName>Dragon</VesselName>
        <VoyageFlightNo>111</VoyageFlightNo>
      </TransportLeg>
      <TransportLeg>
        <PortOfDischarge Name=""Auckland"">NZAKL</PortOfDischarge>
        <PortOfLoading Name=""Singapore"">SGSIN</PortOfLoading>
        <LegOrder>2</LegOrder>
        <ActualArrival></ActualArrival>
        <ActualDeparture></ActualDeparture>
        <ArrivalCTO>
          <AddressType>ArrivalCTOAddress</AddressType>
          <AdditionalAddressInformation></AdditionalAddressInformation>
          <Address1>Unit 13</Address1>
          <Address2>4 Lost Lane</Address2>
          <AddressOverride>false</AddressOverride>
          <City>Aalborg</City>
          <CompanyName>MAERSK</CompanyName>
          <Contact></Contact>
          <Country Name=""Denmark"">DK</Country>
          <Email></Email>
          <Fax></Fax>
          <GovRegNum></GovRegNum>
          <Phone></Phone>
          <Port Name=""Aalborg"">DKAAL</Port>
          <Postcode>2000</Postcode>
          <State></State>

          <RegistrationNumberCollection>
            <RegistrationNumber>
              <Type Description=""TNPA Registration Number"">TNP</Type>
              <CountryOfIssue Name=""South Africa"">ZA</CountryOfIssue>
              <Value>10001</Value>
            </RegistrationNumber>
          </RegistrationNumberCollection>
        </ArrivalCTO>
        <DepartureCTO>
          <AddressType>DepartureCTOAddress</AddressType>
          <AdditionalAddressInformation></AdditionalAddressInformation>
          <Address1>Unit 18</Address1>
          <Address2>Yin Long</Address2>
          <AddressOverride>false</AddressOverride>
          <City>Sydney</City>
          <CompanyName>FREIGH</CompanyName>
          <Contact></Contact>
          <Country Name=""China"">CN</Country>
          <Email></Email>
          <Fax></Fax>
          <GovRegNum></GovRegNum>
          <Phone></Phone>
          <Port Name=""Sydney"">AUSYD</Port>
          <Postcode>3000</Postcode>
          <State>NSW</State>

          <RegistrationNumberCollection>
            <RegistrationNumber>
              <Type Description=""TNPA Registration Number"">TNP</Type>
              <CountryOfIssue Name=""South Africa"">ZA</CountryOfIssue>
              <Value>10001</Value>
            </RegistrationNumber>
          </RegistrationNumberCollection>
        </DepartureCTO>
        <EstimatedArrival></EstimatedArrival>
        <EstimatedDeparture></EstimatedDeparture>
        <LCLCutOff></LCLCutOff>
        <LCLReceivalCommences></LCLReceivalCommences>
        <LegType>Other</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO></VesselLloydsIMO>
        <VesselName>Steven</VesselName>
        <VoyageFlightNo>222</VoyageFlightNo>
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

		#endregion

		CommonContext context;
	}
}
