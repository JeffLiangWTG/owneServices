using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DataTransfer.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture.Core;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR.Testing
{
	sealed class OutturnReportWriterTest : DataObjectWriterTest
	{
		public void TestPopulateDataObject()
		{
			var outturnReport = PrepareTestData();

			var manager = new DataWritingManager(DefaultDataObjectWriterStrategy.Instance);
			var writer = new OutturnReportWriter(manager);

			var dataObject = writer.GetDataObject(outturnReport);

			AssertUXml(dataObject, expectedXml);
		}

		OutturnReport PrepareTestData()
		{
			var outturnReport = new OutturnReport("ForwardingConsol", "C00001001");

			outturnReport.SendingForwarder = CreateAddress("SendingForwarder");
			outturnReport.ReceivingForwarder = CreateAddress("ReceivingForwarder");
			outturnReport.CFS = CreateAddress("CFS");
			outturnReport.SendingParty = CreateAddress("SendingParty");
			outturnReport.CurrentUser = CreateAddress("CurrentUser");

			outturnReport.ATPReference = "ATPAP01";
			outturnReport.ConsolNumber = "C00001001";
			outturnReport.VesselName = "MAERSK EMDEN";
			outturnReport.VoyageFlightNo = "029N";
			outturnReport.BillOfLading = "BOL";

			outturnReport.SendingForwarderSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S001");
			outturnReport.SendingForwarderCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C001");

			outturnReport.ReceivingForwarderSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S002");
			outturnReport.ReceivingForwarderCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C002");

			outturnReport.CFSSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S003");
			outturnReport.CFSCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C003");
			outturnReport.CFSSOW = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOW, "W003");
			outturnReport.CFSSOA = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOA, "A003");

			outturnReport.SendingPartySON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S004");
			outturnReport.SendingPartyCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C004");
			outturnReport.SendingPartySOW = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOW, "W004");
			outturnReport.SendingPartySOA = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOA, "A004");

			outturnReport.CurrentUserSON = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SON, "S005");
			outturnReport.CurrentUserCI5 = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.CI5, "C005");
			outturnReport.CurrentUserSOW = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOW, "W005");
			outturnReport.CurrentUserSOA = CrateRegistrationNumber(OrgCusCode.FranceCodeTypes.SOA, "A005");

			outturnReport.ContainerMode = new DummyCodeDescription()
			{
				Code = Core.Constants.ContainerModes.FCL
			};

			outturnReport.ShipmentType = new DummyCodeDescription()
			{
				Code = Core.Constants.AgentType.Agent
			};

			outturnReport.PortOfOrigin = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "AUSYD",
				Name = "Sydney"
			};

			outturnReport.PortOfDestination = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRPAR",
				Name = "Paris"
			};

			outturnReport.OperationalPort = new Unloco(Factory, context.Unlocos, context.Countries)
			{
				Code = "FRPAR",
				Name = "Paris"
			};

			var container1 = CreateContainer(context, "AAA");
			var container2 = CreateContainer(context, "BBB");

			var packsummary1 = CreatePackingSummary("ICV AAA");
			var packsummary2 = CreatePackingSummary("ICV BBB");
			var packsummary3 = CreatePackingSummary("ICV CCC");

			container1.PackingSummaries = new[]
			{
				packsummary1,
				packsummary2
			};

			container2.PackingSummaries = new[]
			{
				packsummary3
			};

			outturnReport.Containers = new[]
			{
				container1,
				container2
			};

			var goodsDetail = CreateGoodsDetail();

			outturnReport.GoodsDetails = new[] { goodsDetail };

			return outturnReport;
		}

		GoodsDetail CreateGoodsDetail()
		{
			var goodsDetail = new GoodsDetail(ZGuid.NewZGuid());
			goodsDetail.ShipmentNumber = "S00001";
			goodsDetail.HouseBillNumber = "H00001";
			goodsDetail.MarksAndNumbers = "Goods Marks";
			goodsDetail.CommodityReference = "C00056";
			goodsDetail.ICVReference = "ICV001";

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

			container.LPDReference = "LPD000003434";
			container.ImportDepotCustomsReference = "ICT0000000344";
			container.UnpackLocation = "DELLOC";
			container.UnpackArea = "DELAREA";
			container.UnpackingNotes = "Container Unpacking Notes";
			container.IsNonOperativeReefer = false;

			return container;
		}

		BookingPackingSummary CreatePackingSummary(string icvReference)
		{
			var packingSummary = new BookingPackingSummary(ZGuid.NewZGuid());

			packingSummary.ShipmentNumber = "S00001";
			packingSummary.ICVReference = icvReference;

			packingSummary.TotalPackages = 3;
			packingSummary.TotalPackagesUnit = "PLT";
			packingSummary.TotalWeight = new Measurement()
			{
				Value = 99m,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			packingSummary.TotalVolume = new Measurement()
			{
				Value = 33.3m,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};
			packingSummary.TotalOutturnedPackages = 2;
			packingSummary.TotalOutturnedWeight = new Measurement()
			{
				Value = 66m,
				Unit = new DummyCodeDescription
				{
					Code = "KG"
				}
			};
			packingSummary.TotalOutturnedVolume = new Measurement()
			{
				Value = 22.2m,
				Unit = new DummyCodeDescription
				{
					Code = "M3"
				}
			};
			packingSummary.UnpackedReference = "unpackref";
			packingSummary.SurplusIndicator = false;
			packingSummary.UnpackingIndicator = false;
			packingSummary.ReserveIndicator = false;

			return packingSummary;
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
    <PortOfDestination Name=""Paris"">FRPAR</PortOfDestination>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ShipmentType>AGT</ShipmentType>
    <VesselName>MAERSK EMDEN</VesselName>
    <VoyageFlightNo>029N</VoyageFlightNo>
    <WayBillNumber>BOL</WayBillNumber>

    <AddInfoCollection>
      <AddInfo>
        <Key>ATPReference</Key>
        <Value>ATPAP01</Value>
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
        <ContainerNumber>AAA</ContainerNumber>
        <ImportDepotCustomsReference>ICT0000000344</ImportDepotCustomsReference>
        <Link>1</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <WeightUnit>KG</WeightUnit>

        <AddInfoCollection>
          <AddInfo>
            <Key>LPDReference</Key>
            <Value>LPD000003434</Value>
          </AddInfo>
          <AddInfo>
            <Key>ICTReference</Key>
            <Value>ICT0000000344</Value>
          </AddInfo>
          <AddInfo>
            <Key>UnpackLocation</Key>
            <Value>DELLOC</Value>
          </AddInfo>
          <AddInfo>
            <Key>UnpackArea</Key>
            <Value>DELAREA</Value>
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
        <ContainerNumber>BBB</ContainerNumber>
        <ImportDepotCustomsReference>ICT0000000344</ImportDepotCustomsReference>
        <Link>2</Link>
        <NonOperatingReefer>false</NonOperatingReefer>
        <WeightUnit>KG</WeightUnit>

        <AddInfoCollection>
          <AddInfo>
            <Key>LPDReference</Key>
            <Value>LPD000003434</Value>
          </AddInfo>
          <AddInfo>
            <Key>ICTReference</Key>
            <Value>ICT0000000344</Value>
          </AddInfo>
          <AddInfo>
            <Key>UnpackLocation</Key>
            <Value>DELLOC</Value>
          </AddInfo>
          <AddInfo>
            <Key>UnpackArea</Key>
            <Value>DELAREA</Value>
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
        <AddressType>ArrivalCFSAddress</AddressType>
        <AdditionalAddressInformation>CFS additional info</AdditionalAddressInformation>
        <Address1>CFS address line 1</Address1>
        <Address2>CFS address line 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>CFS city</City>
        <CompanyName>CFS</CompanyName>
        <Contact>CFS contact</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email>CFS email</Email>
        <Fax>CFS fax</Fax>
        <GovRegNum>CFS tax number</GovRegNum>
        <GovRegNumType Description=""Goods and Services Tax"">GST</GovRegNumType>
        <Phone>CFS phone</Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>CFS postco</Postcode>
        <State>CFS state</State>

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
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Warehou"">SOW</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>W003</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Agent C"">SOA</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>A003</Value>
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
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C004</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>S004</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Warehou"">SOW</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>W004</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Agent C"">SOA</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>A004</Value>
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
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>C005</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Code"">SON</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>S005</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Warehou"">SOW</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>W005</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Agent C"">SOA</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>A005</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>

        <WayBillNumber>H00001</WayBillNumber>

        <AddInfoCollection>
          <AddInfo>
            <Key>CommodityReference</Key>
            <Value>C00056</Value>
          </AddInfo>
          <AddInfo>
            <Key>MarksAndNumbers</Key>
            <Value>Goods Marks</Value>
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
            <NoteText>Goods Marks</NoteText>
          </Note>
        </NoteCollection>

        <PackingLineCollection>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>AAA</ContainerNumber>
            <ImportReferenceNumber>ICV AAA</ImportReferenceNumber>
            <MarksAndNos>Goods Marks</MarksAndNos>
            <OutturnedVolume>22.2</OutturnedVolume>
            <OutturnedWeight>66</OutturnedWeight>
            <OutturnQty>2</OutturnQty>
            <PackQty>3</PackQty>
            <PackType>PLT</PackType>
            <Volume>33.3</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>99</Weight>
            <WeightUnit>KG</WeightUnit>

            <AddInfoCollection>
              <AddInfo>
                <Key>UnpackedReference</Key>
                <Value>unpackref</Value>
              </AddInfo>
              <AddInfo>
                <Key>SurplusIndicator</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>UnpackingIndicator</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>ReserveIndicator</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>FormVersion</Key>
                <Value>1.0.0</Value>
              </AddInfo>
            </AddInfoCollection>
          </PackingLine>
          <PackingLine>
            <ContainerLink>1</ContainerLink>
            <ContainerNumber>AAA</ContainerNumber>
            <ImportReferenceNumber>ICV BBB</ImportReferenceNumber>
            <MarksAndNos>Goods Marks</MarksAndNos>
            <OutturnedVolume>22.2</OutturnedVolume>
            <OutturnedWeight>66</OutturnedWeight>
            <OutturnQty>2</OutturnQty>
            <PackQty>3</PackQty>
            <PackType>PLT</PackType>
            <Volume>33.3</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>99</Weight>
            <WeightUnit>KG</WeightUnit>

            <AddInfoCollection>
              <AddInfo>
                <Key>UnpackedReference</Key>
                <Value>unpackref</Value>
              </AddInfo>
              <AddInfo>
                <Key>SurplusIndicator</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>UnpackingIndicator</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>ReserveIndicator</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>FormVersion</Key>
                <Value>1.0.0</Value>
              </AddInfo>
            </AddInfoCollection>
          </PackingLine>
          <PackingLine>
            <ContainerLink>2</ContainerLink>
            <ContainerNumber>BBB</ContainerNumber>
            <ImportReferenceNumber>ICV CCC</ImportReferenceNumber>
            <MarksAndNos>Goods Marks</MarksAndNos>
            <OutturnedVolume>22.2</OutturnedVolume>
            <OutturnedWeight>66</OutturnedWeight>
            <OutturnQty>2</OutturnQty>
            <PackQty>3</PackQty>
            <PackType>PLT</PackType>
            <Volume>33.3</Volume>
            <VolumeUnit>M3</VolumeUnit>
            <Weight>99</Weight>
            <WeightUnit>KG</WeightUnit>

            <AddInfoCollection>
              <AddInfo>
                <Key>UnpackedReference</Key>
                <Value>unpackref</Value>
              </AddInfo>
              <AddInfo>
                <Key>SurplusIndicator</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>UnpackingIndicator</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>ReserveIndicator</Key>
                <Value>N</Value>
              </AddInfo>
              <AddInfo>
                <Key>FormVersion</Key>
                <Value>1.0.0</Value>
              </AddInfo>
            </AddInfoCollection>
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
