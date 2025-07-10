using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.CN;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.CN.Testing
{
	class ContainerLoadPlanDataObjectWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var context = new CommonContext(Factory);
			var containerLoadPlan = new ContainerLoadPlan("C0001", "ForwardingConsol", "ContainerLoadPlan");
			containerLoadPlan.ShipmentType = new DummyCodeDescription()
			{
				Code = Core.Constants.AgentType.Agent
			};

			containerLoadPlan.ContainerMode = new DummyCodeDescription()
			{
				Code = Core.Constants.ContainerModes.FCL
			};

			containerLoadPlan.SendingAgent = CreateAddress(nameof(containerLoadPlan.SendingAgent));
			containerLoadPlan.Carrier = CreateAddress(nameof(containerLoadPlan.Carrier));
			containerLoadPlan.DepartureCFSAddress = CreateAddress(nameof(containerLoadPlan.DepartureCFSAddress));
			containerLoadPlan.CurrentUser = CreateAddress("CurrentUser");
			containerLoadPlan.PortOfLoading = new DummyUnloco()
			{
				Code = "CNNGB",
				Name = "Ningbo",
				IATACode = "NGB"
			};
			containerLoadPlan.PortOfDischarge = new DummyUnloco()
			{
				Code = "AUMEL",
				Name = "Melbourne",
				IATACode = "MEL"
			};
			containerLoadPlan.PortOfTranship = new DummyUnloco()
			{
				Code = "AUMEL",
				Name = "Melbourne",
				IATACode = "MEL"
			};
			containerLoadPlan.TransitBerthCode = "AUME1";
			containerLoadPlan.PlaceOfDelivery = new DummyUnloco()
			{
				Code = "AUSYD",
				Name = "Sydney",
				IATACode = "SYD"
			};
			containerLoadPlan.OperationalPort = new DummyUnloco()
			{
				Code = "CNCAN",
				Name = "Guangzhou",
				IATACode = "CAN"
			};
			containerLoadPlan.Vessel = new Vessel()
			{
				Name = "DummyName",
				LloydsIMO = "IMO"
			};

			containerLoadPlan.VoyageFlightNumber = "123789456";

			var container = new ContainerLoadPlanContainer(containerLoadPlan, context, "CONT0001");
			container.Number = "CONT0001";
			container.Mode = new DummyCodeDescription()
			{
				Code = Core.Constants.ContainerModes.FCL,
				Description = Core.Constants.ContainerModeDescriptions.FCL
			};
			container.ContainerType = new DummyContainerType()
			{
				Code = Core.Constants.ContainerModes.Groupage,
				Description = Core.Constants.ContainerModeDescriptions.Groupage
			};

			container.PackDate = new ZDate(2018, 6, 28);
			container.Seal = "First seal";
			container.SecondSeal = "second seal";
			container.ThirdSeal = "third seal";
			container.GrossWeight = new Measurement()
			{
				Value = 66,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Core.Constants.Weight.Kilograms
				}
			};
			container.TareWeight = new Measurement()
			{
				Value = 44,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = Core.Constants.Weight.Kilograms
				}
			};

			container.SetTemperature = new Measurement()
			{
				Value = 30,
				Unit = new CodeDescription(context.TemperatureUnits)
				{
					Code = Core.Constants.Temperature.Centigrade
				}
			};

			container.IsNonOperativeReefer = false;

			var groupings = new List<ContainerLoadPlanSOGrouping>();
			var group = new ContainerLoadPlanSOGrouping("SO00001");
			group.SONumber = "SO00001";
			group.Quantity = 11;
			group.PackageType = new CodeDescription(new CodeDescriptionPairList())
			{
				Code = "PLT",
				Description = "Pallet"
			};
			group.Weight = new Measurement()
			{
				Value = 33,
				Unit = new DummyCodeDescription()
				{
					Code = Core.Constants.Weight.Kilograms,
					Description = Core.Constants.Weight.GetDescription(Core.Constants.Weight.Kilograms, Core.Constants.PluralState.Plural)
				}
			};
			group.Volume = new Measurement()
			{
				Value = 22,
				Unit = new DummyCodeDescription()
				{
					Code = Core.Constants.Volume.CubicMetres,
					Description = Core.Constants.Volume.GetDescription(Core.Constants.Volume.CubicMetres, Core.Constants.PluralState.Plural)
				}
			};
			group.GoodsDescription = "Goods descriptions.";
			group.MarksAndNumbers = "marks and numbers";

			var dangerousGood = new DangerousGood();
			dangerousGood.Quantity = 11;
			dangerousGood.Code = "0005C";
			dangerousGood.Unno = "0005";
			dangerousGood.Variant = "C";
			dangerousGood.ProperShippingName = "shipping name";
			dangerousGood.TechnicalName = "Technical Name";
			dangerousGood.IMOClass = "Class";
			dangerousGood.PackingGroup = "packing group";
			dangerousGood.SubLabel1 = "sublabel1";
			dangerousGood.SubLabel2 = "sublabel2";
			dangerousGood.Weight = new Measurement()
			{
				Value = 12,
				Unit = new DummyCodeDescription()
				{
					Code = Core.Constants.Weight.Kilograms,
					Description = Core.Constants.Weight.GetDescription(Core.Constants.Weight.Kilograms, Core.Constants.PluralState.Plural)
				}
			};

			dangerousGood.Volume = new Measurement()
			{
				Value = 14,
				Unit = new DummyCodeDescription()
				{
					Code = Core.Constants.Volume.CubicMetres,
					Description = Core.Constants.Volume.GetDescription(Core.Constants.Volume.CubicMetres, Core.Constants.PluralState.Plural)
				}
			};

			dangerousGood.PackageType = new DummyCodeDescription()
			{
				Code = "BAG",
				Description = "Bag"
			};

			dangerousGood.MarinePollutant = new DummyCodeDescription()
			{
				Code = "Y",
				Description = "Marine Pollutant"
			};

			dangerousGood.Contact = new Contact() { FullName = "Dangerous Contact", Phone = "14324234" };

			group.DangerousGoods = new List<DangerousGood>() { dangerousGood };
			groupings.Add(group);

			container.Groups = groupings;

			containerLoadPlan.Containers = new ContainerLoadPlanContainer[] { container };

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new ContainerLoadPlanDataObjectWriter(manager);

			var dataObject = writer.GetDataObject(containerLoadPlan);

			AssertUXml(dataObject, expectedXml1);
		}

		public void TestPopulateContainerLink()
		{
			var context = new CommonContext(Factory);
			var containerLoadPlan = new ContainerLoadPlan("C0001", "ForwardingConsol", "ContainerLoadPlan");
			containerLoadPlan.Containers = new List<ContainerLoadPlanContainer>();

			var container1 = new ContainerLoadPlanContainer(containerLoadPlan, context, "CONT00001");
			container1.Number = "CONT00001";
			container1.IsNonOperativeReefer = false;

			var group1 = new ContainerLoadPlanSOGrouping("00001");
			group1.SONumber = "00001";

			var group2 = new ContainerLoadPlanSOGrouping("00002");
			group2.SONumber = "00002";

			container1.Groups = new List<ContainerLoadPlanSOGrouping>() { group1, group2 };

			var container2 = new ContainerLoadPlanContainer(containerLoadPlan, context, "CONT00002");
			container2.Number = "CONT00002";
			container2.IsNonOperativeReefer = false;

			var group3 = new ContainerLoadPlanSOGrouping("00003");
			group3.SONumber = "00003";

			var group4 = new ContainerLoadPlanSOGrouping("00004");
			group4.SONumber = "00004";

			container2.Groups = new List<ContainerLoadPlanSOGrouping>() { group3, group4 };

			containerLoadPlan.Containers = new ContainerLoadPlanContainer[] { container1, container2 };

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new ContainerLoadPlanDataObjectWriter(manager);

			var dataObject = writer.GetDataObject(containerLoadPlan);
			AssertUXml(dataObject, expectedXml2);
		}

		#region Expected Xml

		const string expectedXml1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C0001</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

    </DataContext>

    <ContainerMode>FCL</ContainerMode>
    <LloydsIMO>IMO</LloydsIMO>
    <PlaceOfDelivery Name=""Sydney"">AUSYD</PlaceOfDelivery>
    <PortOfDischarge Name=""Melbourne"">AUMEL</PortOfDischarge>
    <PortOfLoading Name=""Ningbo"">CNNGB</PortOfLoading>
    <ShipmentType>AGT</ShipmentType>
    <VesselName>DummyName</VesselName>
    <VoyageFlightNo>123789456</VoyageFlightNo>

    <AddInfoCollection>
      <AddInfo>
        <Key>PortOfTranship_Code</Key>
        <Value>AUME1</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortOfTranship_Name</Key>
        <Value>Melbourne</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>CNCAN</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Guangzhou</Value>
      </AddInfo>
    </AddInfoCollection>

    <ContainerCollection>
      <Container>
        <ContainerNumber>CONT0001</ContainerNumber>
        <ContainerType>
          <Code>GRP</Code>
          <Description>Groupage / Freight All Kinds</Description>
          <ISOCode></ISOCode>
        </ContainerType>
        <FCL_LCL_AIR Description=""Full Container Load"">FCL</FCL_LCL_AIR>
        <GoodsDescription></GoodsDescription>
        <GrossWeight>66</GrossWeight>
        <Link>1</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <PackDate>2018-06-28T00:00:00</PackDate>
        <Seal>First seal</Seal>
        <SecondSeal>second seal</SecondSeal>
        <SetPointTemp>30</SetPointTemp>
        <SetPointTempUnit>C</SetPointTempUnit>
        <TareWeight>44</TareWeight>
        <ThirdSeal>third seal</ThirdSeal>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
      </Container>
    </ContainerCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
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
        <AddressType>DepartureCFSAddress</AddressType>
        <AdditionalAddressInformation>DepartureCFSAddress additional info</AdditionalAddressInformation>
        <Address1>DepartureCFSAddress address line 1</Address1>
        <Address2>DepartureCFSAddress address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>DepartureCFSAddress city</City>
        <CompanyName>DepartureCFSAddress</CompanyName>
        <Contact>DepartureCFSAddress contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>DepartureCFSAddress email</Email>
        <Fax>DepartureCFSAddress </Fax>
        <GovRegNum>DepartureCFSAddress tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>DepartureCFSAddress </Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>DepartureC</Postcode>
        <State>DepartureCFSAddress state</State>

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
            <Key>SO00001</Key>
            <Type>Booking</Type>
          </DataSource>
        </DataContext>


        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <DetailedDescription>Goods descriptions.</DetailedDescription>
            <MarksAndNos>marks and numbers</MarksAndNos>
            <PackQty>11</PackQty>
            <PackType Description=""Pallet"">PLT</PackType>
            <Volume>22</Volume>
            <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
            <Weight>33</Weight>
            <WeightUnit Description=""Kilograms"">KG</WeightUnit>

            <UNDGCollection>
              <UNDG>
                <Contact>
                  <FullName>Dangerous Contact</FullName>
                  <Email></Email>
                  <Phone>14324234</Phone>
                </Contact>
                <IMOClass>Clas</IMOClass>
                <MarinePollutant Description=""Marine Pollutant"">Y</MarinePollutant>
                <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
                <PackingGroup>pac</PackingGroup>
                <PackQty>11</PackQty>
                <PackType Description=""Bag"">BAG</PackType>
                <ProperShippingName>shipping name</ProperShippingName>
                <Standard></Standard>
                <SubLabel1>sublabel1</SubLabel1>
                <SubLabel2>sublabel2</SubLabel2>
                <TechicalName>Technical Name</TechicalName>
                <UNDGCode>0005</UNDGCode>
                <Volume>14</Volume>
                <VolumeUQ Description=""Cubic Meters"">M3</VolumeUQ>
                <Weight>12</Weight>
                <WeightUQ Description=""Kilograms"">KG</WeightUQ>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";
		const string expectedXml2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <Key>C0001</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

    </DataContext>

    <VoyageFlightNo></VoyageFlightNo>

    <AddInfoCollection>
      <AddInfo>
        <Key>PortOfTranship_Code</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>PortOfTranship_Name</Key>
        <Value></Value>
      </AddInfo>
    </AddInfoCollection>

    <ContainerCollection>
      <Container>
        <ContainerNumber>CONT00001</ContainerNumber>
        <GoodsDescription></GoodsDescription>
        <Link>1</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <PackDate></PackDate>
        <Seal></Seal>
        <SecondSeal></SecondSeal>
        <ThirdSeal></ThirdSeal>
      </Container>
      <Container>
        <ContainerNumber>CONT00002</ContainerNumber>
        <GoodsDescription></GoodsDescription>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <PackDate></PackDate>
        <Seal></Seal>
        <SecondSeal></SecondSeal>
        <ThirdSeal></ThirdSeal>
      </Container>
    </ContainerCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>00001</Key>
            <Type>Booking</Type>
          </DataSource>
        </DataContext>


        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <DetailedDescription></DetailedDescription>
            <MarksAndNos></MarksAndNos>
            <PackQty>0</PackQty>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>00002</Key>
            <Type>Booking</Type>
          </DataSource>
        </DataContext>


        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <DetailedDescription></DetailedDescription>
            <MarksAndNos></MarksAndNos>
            <PackQty>0</PackQty>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>00003</Key>
            <Type>Booking</Type>
          </DataSource>
        </DataContext>


        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <DetailedDescription></DetailedDescription>
            <MarksAndNos></MarksAndNos>
            <PackQty>0</PackQty>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>00004</Key>
            <Type>Booking</Type>
          </DataSource>
        </DataContext>


        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <DetailedDescription></DetailedDescription>
            <MarksAndNos></MarksAndNos>
            <PackQty>0</PackQty>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		#endregion
	}
}
