using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	[TestedType(typeof(InBondDataContextManager))]
	sealed class InBondDataContextManagerTest : DataTransfer.Universal.Testing.CusInBondHeaderDataContextManagerTest<InBondDataContextManager, CusInBondHeader, CusInBondBill, CusInBondMoveHeader, CusInBondMoveDetail, CusInBondContainer, CusInBondCargoDesc>
	{
		public override void TestImportInBond()
		{
			var inBondData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				TransportMode = new CodeDescriptionPair()
				{ Code = Core.Constants.TransportModes.Sea },
				VesselName = "IAN VESSEL",
				MessageType = new CodeDescriptionPair()
				{ Code = DirectionTypeList.Codes.MVOCC }
			};
			inBondData.DataContext.AddDataSource(DataContextType.USAMS, "XXXXX");
			inBondData.DataContext.AddDataTarget(DataContextType.USAMS, null);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(GetQueuedUniversalShipmentMessage(inBondData));
			AssertMultilineASCIIEquals("Service Task Log", @"
Added AMS  from UniversalShipment.
Successfully saved AMS AMS0000001.", serviceTaskLog.ToString());
			var newFactory = new BusinessObjectFactory();
			var amsHeader = newFactory.LoadTop1<CusInBondHeader>(new ZQuery(CusInBondHeaderSchema.BH_JobReference, "AMS0000001"));
			AssertNotNull(amsHeader);
			AssertEquals("IAN VESSEL", amsHeader.BH_ImportConveyanceName);
		}

		public void TestGetShipmentDataObjectReaderForHVLVShipment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S123";

			var hvlvShipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvShipmentData.MessageType = new CodeDescriptionPair() { Code = "N" };

			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.ShipmentType = new CodeDescriptionPair() { Code = "HVL" };
			subShipment.DataContext = DataContextFactory.New();
			subShipment.DataContext.AddDataSource(DataContextType.ForwardingShipment, "S123");

			hvlvShipmentData.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			hvlvShipmentData.SubShipmentCollection.Add(subShipment);

			var contextManager = new InBondDataContextManager();
			var method = typeof(InBondDataContextManager).GetMethod("GetShipmentDataObjectReaderCore", BindingFlags.NonPublic | BindingFlags.Instance);
			var reader = method.Invoke(contextManager, new object[] { hvlvShipmentData, new DummyLogger(), Factory, null });
			AssertType<HVLVCusInBondHeaderDataObjectReader>("Should return HVLVCusInBondHeaderDataObectReader", reader);
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>USAMS</Type>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>DUS</Code>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Name>Your United States Demo Company</Name>
      </Company>
      <DataProvider>HYECMTDUS</DataProvider>
      <EnterpriseID>HYE</EnterpriseID>
      <EventBranch>
        <Code>PHL</Code>
        <Name>PHL</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ADD</Code>
        <Description>Added a record to the system</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>CMT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2015-11-04T16:08:02.783</TriggerDate>
      <TriggerDescription>1415421541</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ARP</Code>
          <Description>AirCargo Responsible Party</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>LAX</Code>
      <Name>Los Angeles</Name>
    </Branch>
    <CustomsContainerMode>
      <Code>CNT</Code>
      <Description>Containerized (Trans. Mode: 11, 21,</Description>
    </CustomsContainerMode>
    <LloydsIMO>9204791</LloydsIMO>
    <PortOfDischarge>
      <Code>AUARA</Code>
      <Name>Kogarah</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code></Code>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Vessel containerized (Container)</Description>
    </TransportMode>
    <VesselCountryOfRegistration>
      <Code>KR</Code>
      <Name>Korea, Republic of</Name>
    </VesselCountryOfRegistration>
    <VesselName>23</VesselName>
    <VoyageFlightNo>111</VoyageFlightNo>

    <AddInfoCollection>
      <AddInfo>
        <Key>CarrierSCAC</Key>
        <Value>OTT1</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsPaperlessMIBParticipant</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsOutboundCargo</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortOfDischargeScheduleD</Key>
        <Value>8277</Value>
      </AddInfo>
      <AddInfo>
        <Key>FIRMS</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>UniqueVoyageIdentifier</Key>
        <Value>123151321</Value>
      </AddInfo>
    </AddInfoCollection>

    <AddInfoGroupCollection>
    </AddInfoGroupCollection>

    <AdditionalBillCollection>
      <AdditionalBill>
        <BillNumber>501245</BillNumber>
        <BillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </BillType>
        <Link>1</Link>
        <NoOfPacks>50</NoOfPacks>
        <PackType>
          <Code>BAG</Code>
          <Description>Bag</Description>
        </PackType>

        <AddInfoCollection>
          <AddInfo>
            <Key>IssuerCode</Key>
            <Value>2K</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillStatus</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>PortOfLading</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>PortOfLadingScheduleK</Key>
            <Value>15213</Value>
          </AddInfo>
          <AddInfo>
            <Key>PlaceOfReceiptScheduleD</Key>
            <Value>1231421421</Value>
          </AddInfo>
          <AddInfo>
            <Key>TransportModeToPortOfLading</Key>
            <Value>10</Value>
          </AddInfo>
          <AddInfo>
            <Key>LastForeignPort</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>LastForeignPortScheduleK</Key>
            <Value>15213</Value>
          </AddInfo>
          <AddInfo>
            <Key>ForeignPortOfContract</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>ForeignPortOfContractScheduleK</Key>
            <Value>15213</Value>
          </AddInfo>
          <AddInfo>
            <Key> Weight</Key>
            <Value>1.0000</Value>
          </AddInfo>
          <AddInfo>
            <Key>WeightUnit</Key>
            <Value>KG</Value>
          </AddInfo>
          <AddInfo>
            <Key>Volume</Key>
            <Value>100.0000</Value>
          </AddInfo>
          <AddInfo>
            <Key>VolumeUnit</Key>
            <Value>M3</Value>
          </AddInfo>
          <AddInfo>
            <Key>PaymentMethod</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>PortOfUnlading</Key>
            <Value>AUARA</Value>
          </AddInfo>
          <AddInfo>
            <Key>PortOfUnladingScheduleD</Key>
            <Value>8277</Value>
          </AddInfo>
          <AddInfo>
            <Key>EstimatedUnloadDate</Key>
            <Value>12-Mar-18</Value>
          </AddInfo>
          <AddInfo>
            <Key>MasterInBondIndicator</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>FIRMS</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>

        <AddInfoGroupCollection>
        </AddInfoGroupCollection>

        <CustomsReferenceCollection>
          <CustomsReference>
            <Type>
              <Code>ADR</Code>
              <Description>Additional Reference</Description>
            </Type>
            <Reference>2312312123</Reference>
            <SubType>
              <Code>2K</Code>
              <Description>Food and Drug Administration (FDA) Product Type</Description>
            </SubType>
          </CustomsReference>
          <CustomsReference>
            <Type>
              <Code>SNP</Code>
              <Description>Secondary Notify Party</Description>
            </Type>
            <Reference>1</Reference>
            <SubType>
              <Code>OTT1</Code>
              <Description>TEST CARRIER</Description>
            </SubType>
          </CustomsReference>
        </CustomsReferenceCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsigneeAddress</AddressType>
            <Address1>172 GLOUCESTER ROAD</Address1>
            <Address2>WAN CHAI DISTRICT</Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>171-172 GLOUCESTER ROAD</AddressShortCode>
            <City>HONG KONG</City>
            <CompanyName>ACE TEST SUPPLIER HK</CompanyName>
            <Country>
              <Code>HK</Code>
              <Name>Hong Kong</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <GovRegNum>160101-13123</GovRegNum>
            <GovRegNumType>
              <Code>CBN</Code>
              <Description>CBP Assigned Number</Description>
            </GovRegNumType>
            <OrganizationCode>ACETESHKG</OrganizationCode>
            <Phone>+22555567651</Phone>
            <Port>
              <Code>HKHKG</Code>
              <Name>Hong Kong</Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>
                  <Code>GMC</Code>
                  <Description>Glazing Manufacturer Code</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>1</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>MID</Code>
                  <Description>Supplier/Manufacturer ID Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>HKGROIND1711HON</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>TMC</Code>
                  <Description>Tire Manufacturer Code</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>123</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>PFR</Code>
                  <Description>Food Facility Registration Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>19148237698</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ForeignShipperDocumentaryAddress</AddressType>
            <Address1>123 MADISON AVE</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>MAIN ADDRESS</AddressShortCode>
            <City>NEW YORK</City>
            <CompanyName>ACE TEST IMPORTER 1</CompanyName>
            <Country>
              <Code>US</Code>
              <Name>United States</Name>
            </Country>
            <Email>email@test.com</Email>
            <Fax></Fax>
            <GovRegNum>58-123456789</GovRegNum>
            <GovRegNumType>
              <Code>EIN</Code>
              <Description>Employer Identification Number</Description>
            </GovRegNumType>
            <OrganizationCode>ACETESPHL</OrganizationCode>
            <Phone>+12155551212</Phone>
            <Port>
              <Code>USPHL</Code>
              <Name>Philadelphia</Name>
            </Port>
            <Postcode>10016</Postcode>
            <ScreeningStatus>
              <Code>CLR</Code>
              <Description>Clear</Description>
            </ScreeningStatus>
            <State>NY</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>
                  <Code>TTB</Code>
                  <Description>TTB Import Permit Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>AA-A-111</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>DDT</Code>
                  <Description>DDTC Registration Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>999999</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>AMS</Code>
                  <Description>AMS (USDA) Assigned ID Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>1234</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>APH</Code>
                  <Description>APHIS Establishment Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>123</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>FRM</Code>
                  <Description>FIRMS Code</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>W235</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>DEA</Code>
                  <Description>DEA Registration Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>111AAA2BB</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ImportBroker</AddressType>
            <Address1>3990 STREET</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>3990 STREET</AddressShortCode>
            <City>BURTON</City>
            <CompanyName>ACE TEST SUPPLIER CA</CompanyName>
            <Country>
              <Code>CA</Code>
              <Name>Canada</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <GovRegNum>123123123</GovRegNum>
            <GovRegNumType>
              <Code>DUN</Code>
              <Description>Data Universal Numbering System</Description>
            </GovRegNumType>
            <OrganizationCode>ACETES6</OrganizationCode>
            <Phone></Phone>
            <Port>
              <Code>CABON</Code>
              <Name>Burton</Name>
            </Port>
            <Postcode>A1A1A1</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>BC</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>
                  <Code>CBN</Code>
                  <Description>CBP Assigned Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>161101-00903</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>MID</Code>
                  <Description>Supplier/Manufacturer ID Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>XCACETES3990BUR</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>GMC</Code>
                  <Description>Glazing Manufacturer Code</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>888</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <Address1>3 BISHOP DUNN PLACE</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>3 BISHOP DUNN PLACE</AddressShortCode>
            <City>BOTANY</City>
            <CompanyName>NZ 1 IMPORTER/EXPORTER LTD</CompanyName>
            <Country>
              <Code>NZ</Code>
              <Name>New Zealand</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCode>NZ1IMPAKL</OrganizationCode>
            <Phone></Phone>
            <Port>
              <Code>NZAKL</Code>
              <Name>Auckland</Name>
            </Port>
            <Postcode>2213</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>AUK</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>
                  <Code>CCD</Code>
                  <Description>Customs Client Code</Description>
                </Type>
                <CountryOfIssue>
                  <Code>NZ</Code>
                  <Name>New Zealand</Name>
                </CountryOfIssue>
                <Value>00782903F</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <Address1>88 GLOUCETER ROAD</Address1>
            <Address2>UNIT 801</Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>88 GLOUCETER ROAD</AddressShortCode>
            <City>WAN CHAI</City>
            <CompanyName>HK 1 IMPORTER/EXPORTER CORPORATION</CompanyName>
            <Country>
              <Code>HK</Code>
              <Name>Hong Kong</Name>
            </Country>
            <Email></Email>
            <Fax>+852 8760-0211</Fax>
            <OrganizationCode>HK1IMPHKG</OrganizationCode>
            <Phone>+852 8760-0200</Phone>
            <Port>
              <Code>HKHKG</Code>
              <Name>Hong Kong</Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <Type>
                  <Code>MID</Code>
                  <Description>Supplier/Manufacturer ID Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Value>CA123123132</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <Type>
                  <Code>LSC</Code>
                  <Description>Legacy System Code</Description>
                </Type>
                <CountryOfIssue>
                  <Code>CA</Code>
                  <Name>Canada</Name>
                </CountryOfIssue>
                <Value>HK1IMPHKG</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </AdditionalBill>
      <AdditionalBill>
        <BillNumber>22KK</BillNumber>
        <BillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </BillType>
        <Link>2</Link>
        <NoOfPacks>10</NoOfPacks>
        <PackType>
          <Code>BDL</Code>
          <Description>Bundle</Description>
        </PackType>

        <AddInfoCollection>
          <AddInfo>
            <Key>IssuerCode</Key>
            <Value>2K</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillStatus</Key>
            <Value>0</Value>
          </AddInfo>
          <AddInfo>
            <Key>PortOfLading</Key>
            <Value>MQFDF</Value>
          </AddInfo>
          <AddInfo>
            <Key>PortOfLadingScheduleK</Key>
            <Value>28351</Value>
          </AddInfo>
          <AddInfo>
            <Key>PlaceOfReceiptScheduleD</Key>
            <Value>FORT-DE-FRANCE</Value>
          </AddInfo>
          <AddInfo>
            <Key>TransportModeToPortOfLading</Key>
            <Value>10</Value>
          </AddInfo>
          <AddInfo>
            <Key>LastForeignPort</Key>
            <Value>MQFDF</Value>
          </AddInfo>
          <AddInfo>
            <Key>LastForeignPortScheduleK</Key>
            <Value>28351</Value>
          </AddInfo>
          <AddInfo>
            <Key>ForeignPortOfContract</Key>
            <Value>MQFDF</Value>
          </AddInfo>
          <AddInfo>
            <Key>ForeignPortOfContractScheduleK</Key>
            <Value>28351</Value>
          </AddInfo>
          <AddInfo>
            <Key> Weight</Key>
            <Value>100.0000</Value>
          </AddInfo>
          <AddInfo>
            <Key>WeightUnit</Key>
            <Value>DT</Value>
          </AddInfo>
          <AddInfo>
            <Key>Volume</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>VolumeUnit</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>PaymentMethod</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>PortOfUnlading</Key>
            <Value>AUARA</Value>
          </AddInfo>
          <AddInfo>
            <Key>PortOfUnladingScheduleD</Key>
            <Value>2835</Value>
          </AddInfo>
          <AddInfo>
            <Key>EstimatedUnloadDate</Key>
            <Value>12-Mar-18</Value>
          </AddInfo>
          <AddInfo>
            <Key>MasterInBondIndicator</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>FIRMS</Key>
            <Value>A000</Value>
          </AddInfo>
        </AddInfoCollection>

        <AddInfoGroupCollection>
        </AddInfoGroupCollection>

        <CustomsReferenceCollection>
          <CustomsReference>
            <Type>
              <Code>ADR</Code>
              <Description>Additional Reference</Description>
            </Type>
            <Reference>654243123</Reference>
            <SubType>
              <Code>BN</Code>
              <Description>Booking number</Description>
            </SubType>
          </CustomsReference>
          <CustomsReference>
            <Type>
              <Code>SNP</Code>
              <Description>Secondary Notify Party</Description>
            </Type>
            <Reference>1</Reference>
            <SubType>
              <Code>OTT1</Code>
              <Description>TEST CARRIER</Description>
            </SubType>
          </CustomsReference>
          <CustomsReference>
            <Type>
              <Code>SNP</Code>
              <Description>Secondary Notify Party</Description>
            </Type>
            <Reference>2</Reference>
            <SubType>
              <Code>A000</Code>
              <Description>CBP RAIL VACIS - PORT OF BUFFALO (14210)</Description>
            </SubType>
          </CustomsReference>
        </CustomsReferenceCollection>
      </AdditionalBill>
    </AdditionalBillCollection>

    <ContainerCollection>
      <Container>
        <ContainerNumber>CONT12354645</ContainerNumber>
        <ContainerType>
          <Code>20GP</Code>
          <Category>
            <Code>DRY</Code>
            <Description>Dry Storage</Description>
          </Category>
          <Description>Twenty foot general purpose</Description>
          <ISOCode>2000</ISOCode>
        </ContainerType>
        <IsEmptyContainer>false</IsEmptyContainer>
        <Link>1</Link>
        <Seal>DSQ41</Seal>
        <SecondSeal>125AD</SecondSeal>

        <AddInfoCollection>
          <AddInfo>
            <Key>ForeignPort</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>ForeignPortScheduleK</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>TypeOfService</Key>
            <Value>CS</Value>
          </AddInfo>
        </AddInfoCollection>

        <CustomsReferenceCollection>
          <CustomsReference>
            <Type>
              <Code>VIN</Code>
              <Description>Vehicle</Description>
            </Type>
            <Reference>12145555555555</Reference>
          </CustomsReference>
        </CustomsReferenceCollection>

        <UNDGCollection>
          <UNDG>
            <Contact>
              <FullName>John</FullName>
              <Phone>+12675551212</Phone>
            </Contact>
            <FlashPoint>0.0</FlashPoint>
            <IMOClass>1.1D</IMOClass>
            <MarinePollutant>
              <Code></Code>
              <Description></Description>
            </MarinePollutant>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup></PackingGroup>
            <PackQty>0</PackQty>
            <PackType>
              <Code></Code>
            </PackType>
            <ProperShippingName>AMMONIUM PICRATE</ProperShippingName>
            <SubLabel1></SubLabel1>
            <SubLabel2></SubLabel2>
            <TechicalName></TechicalName>
            <UNDGCode>0004a</UNDGCode>
            <Volume>0.000</Volume>
            <VolumeUQ>
              <Code></Code>
            </VolumeUQ>
            <Weight>0.000</Weight>
            <WeightUQ>
              <Code></Code>
            </WeightUQ>
          </UNDG>
        </UNDGCollection>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2018-03-12T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <InBondMoveHeaderCollection>
      <InBondMoveHeader>
        <BioterrorismActIndicator></BioterrorismActIndicator>
        <CustomsStatus>
          <Code>AAV</Code>
          <Description>Awaiting Arrival</Description>
        </CustomsStatus>
        <DestinationPortScheduleD>
          <Code></Code>
        </DestinationPortScheduleD>
        <EntryType>
          <Code></Code>
        </EntryType>
        <ExportVesselName></ExportVesselName>
        <ForeignDestinationPortScheduleK>
          <Code></Code>
        </ForeignDestinationPortScheduleK>
        <InBondCarrierID></InBondCarrierID>
        <InBondCarrierSCAC></InBondCarrierSCAC>
        <MessagingApplicationCode>
          <Code>AMS</Code>
          <Description>AMS Vessel Movement</Description>
        </MessagingApplicationCode>
        <SequenceNumber>021412</SequenceNumber>
        <TransferOfLiabilityCarrierCode></TransferOfLiabilityCarrierCode>
        <TransferOfLiabilityCarrierID></TransferOfLiabilityCarrierID>
        <TransferOfLiabilityCityName></TransferOfLiabilityCityName>
        <TransferOfLiabilityStateCode>
          <Code></Code>
        </TransferOfLiabilityStateCode>

        <DateCollection>
          <Date>
            <Type>EntryDate</Type>
            <IsEstimate>true</IsEstimate>
            <Value></Value>
          </Date>
          <Date>
            <Type>Arrival</Type>
            <IsEstimate>true</IsEstimate>
            <Value></Value>
          </Date>
          <Date>
            <Type>Departure</Type>
            <IsEstimate>true</IsEstimate>
            <Value></Value>
          </Date>
          <Date>
            <Type>TransferOfLiability</Type>
            <IsEstimate>true</IsEstimate>
            <Value></Value>
          </Date>
        </DateCollection>

        <EntryNumberCollection>
          <EntryNumber>
            <Number></Number>
            <Type>
              <Code>INB</Code>
              <Description>In-Bond Transit Number</Description>
            </Type>
          </EntryNumber>
        </EntryNumberCollection>

        <InBondMoveDetailCollection>
          <InBondMoveDetail>
            <AdditionalBillLink>1</AdditionalBillLink>
            <ArrivalStatus>
              <Code></Code>
            </ArrivalStatus>
            <CustomsStatus>
              <Code></Code>
            </CustomsStatus>
            <DepartureStatus>
              <Code></Code>
            </DepartureStatus>
            <ExportationStatus>
              <Code></Code>
            </ExportationStatus>
            <ExportDate></ExportDate>
            <ExportVesselName></ExportVesselName>
            <ForeignDestPortScheduleK></ForeignDestPortScheduleK>
            <InBondQuantity>0</InBondQuantity>
            <MessageStatus>
              <Code>ANG</Code>
              <Description>Adding</Description>
            </MessageStatus>
            <MonetaryValue>0.0000</MonetaryValue>
            <TransferOfLiabilityStatus>
              <Code></Code>
            </TransferOfLiabilityStatus>

            <ContainerLinkCollection>
              <ContainerLink>
                <ContainerNumber>CONT12354645</ContainerNumber>
                <Link>1</Link>
              </ContainerLink>
            </ContainerLinkCollection>

            <EntryNumberCollection>
              <EntryNumber>
                <Number></Number>
                <Type>
                  <Code>PIT</Code>
                  <Description>Previous In-Bond Transit Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
              </EntryNumber>
            </EntryNumberCollection>
          </InBondMoveDetail>
          <InBondMoveDetail>
            <AdditionalBillLink>2</AdditionalBillLink>
            <ArrivalStatus>
              <Code></Code>
            </ArrivalStatus>
            <CustomsStatus>
              <Code></Code>
            </CustomsStatus>
            <DepartureStatus>
              <Code></Code>
            </DepartureStatus>
            <ExportationStatus>
              <Code></Code>
            </ExportationStatus>
            <ExportDate></ExportDate>
            <ExportVesselName></ExportVesselName>
            <ForeignDestPortScheduleK></ForeignDestPortScheduleK>
            <InBondQuantity>0</InBondQuantity>
            <MessageStatus>
              <Code>ANG</Code>
              <Description>Adding</Description>
            </MessageStatus>
            <MonetaryValue>0.0000</MonetaryValue>
            <TransferOfLiabilityStatus>
              <Code></Code>
            </TransferOfLiabilityStatus>

            <ContainerLinkCollection>
            </ContainerLinkCollection>

            <EntryNumberCollection>
              <EntryNumber>
                <Number></Number>
                <Type>
                  <Code>PIT</Code>
                  <Description>Previous In-Bond Transit Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
              </EntryNumber>
            </EntryNumberCollection>
          </InBondMoveDetail>
        </InBondMoveDetailCollection>
      </InBondMoveHeader>
      <InBondMoveHeader>
        <BioterrorismActIndicator></BioterrorismActIndicator>
        <CustomsStatus>
          <Code></Code>
        </CustomsStatus>
        <DestinationPortScheduleD>
          <Code></Code>
        </DestinationPortScheduleD>
        <EntryType>
          <Code></Code>
        </EntryType>
        <ExportVesselName></ExportVesselName>
        <ForeignDestinationPortScheduleK>
          <Code></Code>
        </ForeignDestinationPortScheduleK>
        <InBondCarrierID>12-3456789AB</InBondCarrierID>
        <InBondCarrierSCAC></InBondCarrierSCAC>
        <MessagingApplicationCode>
          <Code>PTT</Code>
          <Description>Permit To Transfer Movement</Description>
        </MessagingApplicationCode>
        <SequenceNumber>021412</SequenceNumber>
        <TransferOfLiabilityCarrierCode></TransferOfLiabilityCarrierCode>
        <TransferOfLiabilityCarrierID></TransferOfLiabilityCarrierID>
        <TransferOfLiabilityCityName></TransferOfLiabilityCityName>
        <TransferOfLiabilityStateCode>
          <Code></Code>
        </TransferOfLiabilityStateCode>

        <DateCollection>
          <Date>
            <Type>EntryDate</Type>
            <IsEstimate>true</IsEstimate>
            <Value></Value>
          </Date>
          <Date>
            <Type>Arrival</Type>
            <IsEstimate>true</IsEstimate>
            <Value></Value>
          </Date>
          <Date>
            <Type>Departure</Type>
            <IsEstimate>true</IsEstimate>
            <Value></Value>
          </Date>
          <Date>
            <Type>TransferOfLiability</Type>
            <IsEstimate>true</IsEstimate>
            <Value></Value>
          </Date>
        </DateCollection>

        <EntryNumberCollection>
          <EntryNumber>
            <Number></Number>
            <Type>
              <Code>INB</Code>
              <Description>In-Bond Transit Number</Description>
            </Type>
          </EntryNumber>
        </EntryNumberCollection>

        <InBondMoveDetailCollection>
          <InBondMoveDetail>
            <AdditionalBillLink>1</AdditionalBillLink>
            <ArrivalStatus>
              <Code></Code>
            </ArrivalStatus>
            <CustomsStatus>
              <Code></Code>
            </CustomsStatus>
            <DepartureStatus>
              <Code></Code>
            </DepartureStatus>
            <ExportationStatus>
              <Code></Code>
            </ExportationStatus>
            <ExportDate></ExportDate>
            <ExportVesselName></ExportVesselName>
            <ForeignDestPortScheduleK></ForeignDestPortScheduleK>
            <InBondQuantity>500</InBondQuantity>
            <MessageStatus>
              <Code>APT</Code>
              <Description>Awaiting Permit To Transfer </Description>
            </MessageStatus>
            <MonetaryValue>0.0000</MonetaryValue>
            <TransferOfLiabilityStatus>
              <Code></Code>
            </TransferOfLiabilityStatus>

            <ContainerLinkCollection>
            </ContainerLinkCollection>

            <EntryNumberCollection>
              <EntryNumber>
                <Number></Number>
                <Type>
                  <Code>PIT</Code>
                  <Description>Previous In-Bond Transit Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
              </EntryNumber>
            </EntryNumberCollection>
          </InBondMoveDetail>
        </InBondMoveDetailCollection>
      </InBondMoveHeader>
      <InBondMoveHeader>
        <BioterrorismActIndicator>N</BioterrorismActIndicator>
        <CustomsStatus>
          <Code></Code>
        </CustomsStatus>
        <DestinationPortScheduleD>
          <Code></Code>
        </DestinationPortScheduleD>
        <EntryType>
          <Code>62</Code>
          <Description>Transport and Export</Description>
        </EntryType>
        <ExportVesselName></ExportVesselName>
        <ForeignDestinationPortScheduleK>
          <Code>01520</Code>
          <Description>HAMILTON, ONT, CA.</Description>
        </ForeignDestinationPortScheduleK>
        <InBondCarrierID></InBondCarrierID>
        <InBondCarrierSCAC></InBondCarrierSCAC>
        <MessagingApplicationCode>
          <Code>SIB</Code>
          <Description>Subsequent In-Bond Movement</Description>
        </MessagingApplicationCode>
        <SequenceNumber>021412</SequenceNumber>
        <TransferOfLiabilityCarrierCode></TransferOfLiabilityCarrierCode>
        <TransferOfLiabilityCarrierID></TransferOfLiabilityCarrierID>
        <TransferOfLiabilityCityName></TransferOfLiabilityCityName>
        <TransferOfLiabilityStateCode>
          <Code></Code>
        </TransferOfLiabilityStateCode>

        <DateCollection>
          <Date>
            <Type>EntryDate</Type>
            <IsEstimate>true</IsEstimate>
            <Value></Value>
          </Date>
          <Date>
            <Type>Arrival</Type>
            <IsEstimate>true</IsEstimate>
            <Value></Value>
          </Date>
          <Date>
            <Type>Departure</Type>
            <IsEstimate>true</IsEstimate>
            <Value></Value>
          </Date>
          <Date>
            <Type>TransferOfLiability</Type>
            <IsEstimate>true</IsEstimate>
            <Value></Value>
          </Date>
        </DateCollection>

        <EntryNumberCollection>
          <EntryNumber>
            <Number>000000094</Number>
            <Type>
              <Code>INB</Code>
              <Description>In-Bond Transit Number</Description>
            </Type>
          </EntryNumber>
        </EntryNumberCollection>

        <InBondMoveDetailCollection>
          <InBondMoveDetail>
            <AdditionalBillLink>2</AdditionalBillLink>
            <ArrivalStatus>
              <Code></Code>
            </ArrivalStatus>
            <CustomsStatus>
              <Code></Code>
            </CustomsStatus>
            <DepartureStatus>
              <Code>ADP</Code>
              <Description>Awaiting Departure</Description>
            </DepartureStatus>
            <ExportationStatus>
              <Code></Code>
            </ExportationStatus>
            <ExportDate></ExportDate>
            <ExportVesselName></ExportVesselName>
            <ForeignDestPortScheduleK>01520</ForeignDestPortScheduleK>
            <InBondQuantity>10</InBondQuantity>
            <MessageStatus>
              <Code>ADP</Code>
              <Description>Awaiting Departure</Description>
            </MessageStatus>
            <MonetaryValue>0.0000</MonetaryValue>
            <TransferOfLiabilityStatus>
              <Code></Code>
            </TransferOfLiabilityStatus>

            <ContainerLinkCollection>
            </ContainerLinkCollection>

            <EntryNumberCollection>
              <EntryNumber>
                <Number>00000056666</Number>
                <Type>
                  <Code>PIT</Code>
                  <Description>Previous In-Bond Transit Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
              </EntryNumber>
            </EntryNumberCollection>
          </InBondMoveDetail>
        </InBondMoveDetailCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>InBondCarrier</AddressType>
            <Address1>P.O. BOX 50</Address1>
            <Address2>FONTANASTRASSE 1</Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>PST: P.O. BOX 50</AddressShortCode>
            <City>VIENNA</City>
            <CompanyName>AUSTRIAN AIRWAYS</CompanyName>
            <Country>
              <Code>AT</Code>
              <Name>Austria</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCode>AUSAIRVIE</OrganizationCode>
            <Phone></Phone>
            <Port>
              <Code>ATVIE</Code>
              <Name>Wien</Name>
            </Port>
            <Postcode>1107</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </InBondMoveHeader>
      <InBondMoveHeader>
        <BioterrorismActIndicator>Y</BioterrorismActIndicator>
        <CustomsStatus>
          <Code></Code>
        </CustomsStatus>
        <DestinationPortScheduleD>
          <Code>1101</Code>
          <Description>PHILADELPHIA, PA</Description>
        </DestinationPortScheduleD>
        <EntryType>
          <Code>61</Code>
          <Description>Immediate Transport</Description>
        </EntryType>
        <ExportVesselName>ASDADASDSADDSADSADA</ExportVesselName>
        <ForeignDestinationPortScheduleK>
          <Code>60200</Code>
          <Description>ALL OTHER AUSTRALIA PORTS</Description>
        </ForeignDestinationPortScheduleK>
        <InBondCarrierID>12-3456789AB</InBondCarrierID>
        <InBondCarrierSCAC></InBondCarrierSCAC>
        <MessagingApplicationCode>
          <Code>INB</Code>
          <Description>Master In-Bond Movement</Description>
        </MessagingApplicationCode>
        <SequenceNumber>021412</SequenceNumber>
        <TransferOfLiabilityCarrierCode></TransferOfLiabilityCarrierCode>
        <TransferOfLiabilityCarrierID>TOL22031801</TransferOfLiabilityCarrierID>
        <TransferOfLiabilityCityName>DDDQ11A</TransferOfLiabilityCityName>
        <TransferOfLiabilityStateCode>
          <Code>AZ</Code>
          <Description>Arizona</Description>
        </TransferOfLiabilityStateCode>

        <DateCollection>
          <Date>
            <Type>EntryDate</Type>
            <IsEstimate>true</IsEstimate>
            <Value></Value>
          </Date>
          <Date>
            <Type>Arrival</Type>
            <IsEstimate>true</IsEstimate>
            <Value>2018-03-12T17:43:00</Value>
          </Date>
          <Date>
            <Type>Departure</Type>
            <IsEstimate>true</IsEstimate>
            <Value>2018-03-07T17:43:00</Value>
          </Date>
          <Date>
            <Type>TransferOfLiability</Type>
            <IsEstimate>true</IsEstimate>
            <Value>2018-03-02T17:43:00</Value>
          </Date>
        </DateCollection>

        <EntryNumberCollection>
          <EntryNumber>
            <Number>000000072</Number>
            <Type>
              <Code>INB</Code>
              <Description>In-Bond Transit Number</Description>
            </Type>
          </EntryNumber>
        </EntryNumberCollection>

        <InBondMoveDetailCollection>
          <InBondMoveDetail>
            <AdditionalBillLink>1</AdditionalBillLink>
            <ArrivalStatus>
              <Code></Code>
            </ArrivalStatus>
            <CustomsStatus>
              <Code></Code>
            </CustomsStatus>
            <DepartureStatus>
              <Code>ADP</Code>
              <Description>Awaiting Departure</Description>
            </DepartureStatus>
            <ExportationStatus>
              <Code></Code>
            </ExportationStatus>
            <ExportDate>2018-03-12T17:43:00</ExportDate>
            <ExportVesselName>A PROLOGUE</ExportVesselName>
            <ForeignDestPortScheduleK>3901</ForeignDestPortScheduleK>
            <InBondQuantity>50</InBondQuantity>
            <MessageStatus>
              <Code>ADP</Code>
              <Description>Awaiting Departure</Description>
            </MessageStatus>
            <MonetaryValue>0.0000</MonetaryValue>
            <TransferOfLiabilityStatus>
              <Code></Code>
            </TransferOfLiabilityStatus>

            <ContainerLinkCollection>
            </ContainerLinkCollection>

            <EntryNumberCollection>
              <EntryNumber>
                <Number>00000021412</Number>
                <Type>
                  <Code>PIT</Code>
                  <Description>Previous In-Bond Transit Number</Description>
                </Type>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
              </EntryNumber>
            </EntryNumberCollection>
          </InBondMoveDetail>
        </InBondMoveDetailCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>InBondCarrier</AddressType>
            <Address1>TEST1</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>OFC: TEST1</AddressShortCode>
            <City>TEST1</City>
            <CompanyName>12345678901234567890123456789012345678901234567890</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCode>123456SYD</OrganizationCode>
            <Phone></Phone>
            <Port>
              <Code>AUSYD</Code>
              <Name>Sydney</Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>NSW</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>TransferOfLiabilityCarrier</AddressType>
            <Address1>901 LONG RD</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <AddressShortCode>901 LONG RD</AddressShortCode>
            <City>MANGERE</City>
            <CompanyName>AUCKLAND TRANSPORT COMPANY</CompanyName>
            <Country>
              <Code>NZ</Code>
              <Name>New Zealand</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <OrganizationCode>AUCTRAAKL</OrganizationCode>
            <Phone></Phone>
            <Port>
              <Code>NZAKL</Code>
              <Name>Auckland</Name>
            </Port>
            <Postcode>2022</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>AUK</State>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </InBondMoveHeader>
    </InBondMoveHeaderCollection>

    <PackingLineCollection>
      <PackingLine>
        <ContainerLink>1</ContainerLink>
        <CountryOfOrigin>
          <Code></Code>
        </CountryOfOrigin>
        <GoodsDescription>E1EWEDSADSA</GoodsDescription>
        <HarmonisedCode>0101100010</HarmonisedCode>
        <LinePrice>12314.0000</LinePrice>
        <MarksAndNos>141241515</MarksAndNos>
        <PackQty>50</PackQty>
        <PackType>
          <Code>BAG</Code>
          <Description>Bag</Description>
        </PackType>
        <ReferenceNumber>dadaq1w341</ReferenceNumber>
        <Weight>100.0000</Weight>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>

        <PackedItemCollection>
        </PackedItemCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>";
			}
		}
	}
}
