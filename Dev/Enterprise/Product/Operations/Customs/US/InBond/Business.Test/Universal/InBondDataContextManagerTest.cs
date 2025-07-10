using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Business.Universal.Testing
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
				VesselName = "BOB VESSEL"
			};
			inBondData.DataContext.AddDataSource(DataContextType.InBond, "XXXXX");
			inBondData.DataContext.AddDataTarget(DataContextType.InBond, null);
			var shipmentData = GetInBondShipmentData(inBondData);
			var consolData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				TransportMode = new CodeDescriptionPair()
				{ Code = Core.Constants.TransportModes.Sea },
				VesselName = "WENDY VESSEL",
				PortOfLoading = new UNLOCO()
				{ Code = "AUSYD" },
				PortOfDischarge = new UNLOCO()
				{ Code = "USLAX" },
				WayBillNumber = "OB201502101431",
				WayBillType = new WayBillType()
				{ Code = WayBillTypeList.Codes.Master },
			};
			consolData.SetSubShipmentCollection(() => new DataObjectList<Shipment>(new[] { shipmentData }));
			consolData.DataContext.AddDataTarget(DataContextType.ForwardingConsol, null);
			consolData.DataContext.AddDataTarget(DataContextType.InBond, null);
			ProcessMessageAndAssertImportResult(GetQueuedUniversalShipmentMessage(consolData), shipmentData);
		}

		Shipment GetInBondShipmentData(Shipment subInBondData)
		{
			var shipmentData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(),
				TransportMode = new CodeDescriptionPair()
				{ Code = Core.Constants.TransportModes.Sea },
				PortOfOrigin = new UNLOCO()
				{ Code = "AUMEL" },
				PortOfDestination = new UNLOCO()
				{ Code = "USCHI" },
				WayBillNumber = "HB201502101431",
				WayBillType = new WayBillType()
				{ Code = WayBillTypeList.Codes.House },
			};
			shipmentData.SetSubShipmentCollection(() => new DataObjectList<Shipment>(new[] { subInBondData }));
			shipmentData.DataContext.AddDataTarget(DataContextType.ForwardingShipment, null);
			shipmentData.DataContext.AddDataTarget(DataContextType.InBond, null);
			return shipmentData;
		}

		void ProcessMessageAndAssertImportResult(IEDIMessage message, Shipment shipmentData)
		{
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added In-Bond  from UniversalShipment.
Added Shipment (House Bill='HB201502101431') from UniversalShipment.
Added Consol (Master Bill='OB201502101431') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='OB201502101431') with 1 x CusInBondHeader, 1 x ForwardingShipment.
".Trim(), serviceTaskLog.ToString());
			var newFactory = new BusinessObjectFactory();
			var consol = newFactory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, "C00001000");
			AssertEquals("consol.Shipments", 1, consol.Shipments.Count);
			AssertEquals("consol.JK_MasterBillNum", "OB201502101431", consol.JK_MasterBillNum);
			var shipment = consol.Shipments[0];
			AssertEquals("shipment.JS_UniqueConsignRef", "S00001000", shipment.JS_UniqueConsignRef);
			AssertEquals("shipment.JS_HouseBill", "HB201502101431", shipment.JS_HouseBill);
			var inBondHeader = shipment.InBondHeader;
			AssertEquals("inBondHeader.BH_JobReference", "S00001000", inBondHeader.BH_JobReference);
			AssertEquals("inBondHeader.BH_ImportConveyanceName", "BOB VESSEL", inBondHeader.BH_ImportConveyanceName);
			consol.Delete();
			shipment.JS_IsForwardRegistered = false;
			shipment.Delete();
			inBondHeader.Delete();
			newFactory.Save();
			message = GetQueuedUniversalShipmentMessage(shipmentData);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added In-Bond  from UniversalShipment.
Added Shipment (House Bill='HB201502101431') from UniversalShipment.
Successfully saved Shipment S00001001 (House Bill='HB201502101431') with 1 x CusInBondHeader.
".Trim(), serviceTaskLog.ToString());
			newFactory = new BusinessObjectFactory();
			shipment = newFactory.LoadFromNaturalKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, "S00001001");
			AssertEquals("shipment.JS_HouseBill", "HB201502101431", shipment.JS_HouseBill);
			inBondHeader = shipment.InBondHeader;
			AssertEquals("inBondHeader.BH_JobReference", "S00001001", inBondHeader.BH_JobReference);
			AssertEquals("inBondHeader.BH_ImportConveyanceName", "BOB VESSEL", inBondHeader.BH_ImportConveyanceName);
			shipment.JS_IsForwardRegistered = false;
			shipment.Delete();
			inBondHeader.Delete();
			newFactory.Save();
			shipmentData.DataContext = DataContextFactory.New();
			shipmentData.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			shipmentData.DataContext.AddDataTarget(DataContextType.InBond, null);
			message = GetQueuedUniversalShipmentMessage(shipmentData);
			serviceTaskLog = new ServiceTaskLogForTesting();
			manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			AssertMultilineASCIIEquals("Service Task Log", @"
Added In-Bond  from UniversalShipment.
Added Declaration (House Bill='HB201502101431') from UniversalShipment.
Successfully saved Declaration B00001000 with 1 x CusInBondHeader.".Trim(), serviceTaskLog.ToString());
			newFactory = new BusinessObjectFactory();
			var declaration = newFactory.LoadTop1<US.Business.JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_DeclarationReference, "B00001000"));
			AssertEquals("declaration.JE_HouseBill", "HB201502101431", declaration.JE_HouseBill);
			inBondHeader = declaration.InBondHeader;
			AssertEquals("inBondHeader.BH_JobReference", "B00001000", inBondHeader.BH_JobReference);
			AssertEquals("inBondHeader.BH_ImportConveyanceName", "BOB VESSEL", inBondHeader.BH_ImportConveyanceName);
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>InBond</Type>
        </DataTarget>
      </DataTargetCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>DUS</Code>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Name>U.S. Demo Company</Name>
      </Company>
      <DataProvider>HYECMTDUS</DataProvider>
      <EnterpriseID>HYE</EnterpriseID>
      <EventBranch>
        <Code>CHI</Code>
        <Name>CHI</Name>
      </EventBranch>
      <EventDepartment>
        <Code>TDP</Code>
        <Name>Cartage Domestic Pickup</Name>
      </EventDepartment>
      <EventType>
        <Code>ATH</Code>
        <Description>Action Authorized</Description>
      </EventType>
      <EventUser>
        <Code>DN</Code>
        <Name>Dong The Ding</Name>
      </EventUser>
      <ServerID>CMT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDescription>SDF</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>CHI</Code>
      <Name>CHI</Name>
    </Branch>
    <ContainerMode>
      <Code>CNT</Code>
      <Description>Containerized (Trans. Mode: 11, 21,</Description>
    </ContainerMode>
    <LloydsIMO></LloydsIMO>
    <PortOfDischarge>
      <Code></Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code></Code>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea (Non Container, Container) (10, 11)</Description>
    </TransportMode>
    <VesselCountryOfRegistration>
      <Code>AU</Code>
      <Name>Australia</Name>
    </VesselCountryOfRegistration>
    <VesselName>AALSMEERGRACHT</VesselName>
    <VoyageFlightNo>1655</VoyageFlightNo>

    <AdditionalBillCollection>
      <AdditionalBill>
        <BillNumber>MB1</BillNumber>
        <BillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </BillType>
        <Link>1</Link>
        <NoOfPacks>20</NoOfPacks>

        <AddInfoCollection>
          <AddInfo>
            <Key>ManifestUnit</Key>
            <Value>KEG</Value>
          </AddInfo>
          <AddInfo>
            <Key>Weight</Key>
            <Value>934.0000</Value>
          </AddInfo>
          <AddInfo>
            <Key>WeightUnit</Key>
            <Value>KG</Value>
          </AddInfo>
          <AddInfo>
            <Key>Volume</Key>
            <Value>11.0000</Value>
          </AddInfo>
          <AddInfo>
            <Key>VolumeUnit</Key>
            <Value>MM</Value>
          </AddInfo>
          <AddInfo>
            <Key>PortOfLadingScheduleK</Key>
            <Value>56000</Value>
          </AddInfo>
          <AddInfo>
            <Key>PlaceOfReceiptScheduleD</Key>
            <Value>3000</Value>
          </AddInfo>
          <AddInfo>
            <Key>IssuerCode</Key>
            <Value>APLJ</Value>
          </AddInfo>
        </AddInfoCollection>

        <CustomsReferenceCollection>
          <CustomsReference>
            <Type>
              <Code>ADR</Code>
              <Description>Additional Reference</Description>
            </Type>
            <Reference>82SKD</Reference>
            <SubType>
              <Code>CX</Code>
              <Description>Consignment Classification Number</Description>
            </SubType>
          </CustomsReference>
          <CustomsReference>
            <Type>
              <Code>ADR</Code>
              <Description>Additional Reference</Description>
            </Type>
            <Reference>956SJ</Reference>
            <SubType>
              <Code>WY</Code>
              <Description>Rail Waybill Number</Description>
            </SubType>
          </CustomsReference>
        </CustomsReferenceCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsigneeAddress</AddressType>
            <Address1>PERRY LOGISTICS CENTER</Address1>
            <Address2>NINOY AQUINO AV, SAN DIONISIO</Address2>
            <AddressOverride>true</AddressOverride>
            <City>PARANAQUE</City>
            <CompanyName>E2 LOGISTICS PHILS INC</CompanyName>
            <Contact></Contact>
            <Country>
              <Code>PH</Code>
              <Name>Philippines</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <GovRegNum></GovRegNum>
            <GovRegNumType>
              <Code>EIN</Code>
              <Description>Employer Identification Number</Description>
            </GovRegNumType>
            <Mobile></Mobile>
            <Phone></Phone>
            <Postcode>323</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ForeignShipperDocumentaryAddress</AddressType>
            <Address1>ORIENT PLADS 1, MEZZ</Address1>
            <Address2></Address2>
            <AddressOverride>true</AddressOverride>
            <City>COPENHAGEN</City>
            <CompanyName>F3 AUDIOVECTOR APS</CompanyName>
            <Contact></Contact>
            <Country>
              <Code>DK</Code>
              <Name>Denmark</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <GovRegNum></GovRegNum>
            <GovRegNumType>
              <Code>EIN</Code>
              <Description>Employer Identification Number</Description>
            </GovRegNumType>
            <Mobile></Mobile>
            <Phone></Phone>
            <Postcode>DK-2100</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>DENMARK</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <Address1>ALGADE 23</Address1>
            <Address2></Address2>
            <AddressOverride>true</AddressOverride>
            <City>HAARBY</City>
            <CompanyName>HAARBY BOLIGMONTERING</CompanyName>
            <Contact></Contact>
            <Country>
              <Code>DE</Code>
              <Name>Germany</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <GovRegNum></GovRegNum>
            <GovRegNumType>
              <Code>EIN</Code>
              <Description>Employer Identification Number</Description>
            </GovRegNumType>
            <Mobile></Mobile>
            <Phone></Phone>
            <Postcode>DK 5683</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>HH</State>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </AdditionalBill>
      <AdditionalBill>
        <BillNumber>MB2</BillNumber>
        <BillType>
          <Code>MWB</Code>
          <Description>Master Waybill</Description>
        </BillType>
        <Link>2</Link>
        <NoOfPacks>10</NoOfPacks>

        <AddInfoCollection>
          <AddInfo>
            <Key>ManifestUnit</Key>
            <Value>PAL</Value>
          </AddInfo>
          <AddInfo>
            <Key>Weight</Key>
            <Value>2034.0000</Value>
          </AddInfo>
          <AddInfo>
            <Key>WeightUnit</Key>
            <Value>LT</Value>
          </AddInfo>
          <AddInfo>
            <Key>Volume</Key>
            <Value>323.0000</Value>
          </AddInfo>
          <AddInfo>
            <Key>VolumeUnit</Key>
            <Value>CF</Value>
          </AddInfo>
          <AddInfo>
            <Key>PortOfLadingScheduleK</Key>
            <Value>56100</Value>
          </AddInfo>
          <AddInfo>
            <Key>PlaceOfReceiptScheduleD</Key>
            <Value>4101</Value>
          </AddInfo>
          <AddInfo>
            <Key>IssuerCode</Key>
            <Value>APLJ</Value>
          </AddInfo>
        </AddInfoCollection>

        <CustomsReferenceCollection>
          <CustomsReference>
            <Type>
              <Code>ADR</Code>
              <Description>Additional Reference</Description>
            </Type>
            <Reference>932K</Reference>
            <SubType>
              <Code>8S</Code>
              <Description>Broker Identification - This is used by Rail And Truck AMS</Description>
            </SubType>
          </CustomsReference>
          <CustomsReference>
            <Type>
              <Code>ADR</Code>
              <Description>Additional Reference</Description>
            </Type>
            <Reference>92K08</Reference>
            <SubType>
              <Code>CG</Code>
              <Description>Consignee's Order Number</Description>
            </SubType>
          </CustomsReference>
        </CustomsReferenceCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsigneeAddress</AddressType>
            <AddressShortCode>OFC: USIMP ADDRESS 1</AddressShortCode>
            <OrganizationCode>USIMP</OrganizationCode>
            <Address1>USIMP ADDRESS 1</Address1>
            <Address2>USIMP ADDRESS 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>USIMPCITY</City>
            <CompanyName>USIMP COMPANY NAME</CompanyName>
            <Country>
              <Code>US</Code>
              <Name>United States</Name>
            </Country>
            <Email>main@usimp.com</Email>
            <Fax>+1 (801) 232-2288</Fax>
            <GovRegNum>91-013199000</GovRegNum>
            <GovRegNumType>
              <Code>EIN</Code>
              <Description>Employer Identification Number</Description>
            </GovRegNumType>
            <Phone>+1 (801) 232-2299</Phone>
            <Port>
              <Code>USLAX</Code>
              <Name>Los Angeles</Name>
            </Port>
            <Postcode>96100</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>CA</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ForeignShipperDocumentaryAddress</AddressType>
            <AddressShortCode>AUEXP ADDRESS 1</AddressShortCode>
            <OrganizationCode>AUEXP</OrganizationCode>
            <Address1>AUEXP ADDRESS 1</Address1>
            <Address2>AUEXP ADDRESS 2</Address2>
            <AddressOverride>false</AddressOverride>
            <City>AUEXPCITY</City>
            <CompanyName>AUEXP COMPANY NAME</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email>main@auexp.com</Email>
            <Fax>+61 (2) 9845-6577</Fax>
            <Phone>+61 (2) 9845-6576</Phone>
            <Port>
              <Code>AUSYD</Code>
              <Name>Sydney</Name>
            </Port>
            <Postcode>4345</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>NSW</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>MID</Code>
                  <Description>Supplier/Manufacturer ID Number</Description>
                </Type>
                <Value>AUAUECOM2AUE</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>FEI</Code>
                  <Description>FDA Establishment Identifier</Description>
                </Type>
                <Value>157</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AddressShortCode>ALL: C/O 164 VICTORIA</AddressShortCode>
            <OrganizationCode>DAANETMEL</OrganizationCode>
            <Address1>C/O 164 VICTORIA</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>NORTH GEELONG</City>
            <CompanyName>DAANET PTY LTD</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <GovRegNum>32141234</GovRegNum>
            <GovRegNumType>
              <Code>EIN</Code>
              <Description>Employer Identification Number</Description>
            </GovRegNumType>
            <Phone></Phone>
            <Port>
              <Code>AUMEL</Code>
              <Name>Melbourne</Name>
            </Port>
            <Postcode>3001</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>VIC</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>AU</Code>
                  <Name>Australia</Name>
                </CountryOfIssue>
                <Type>
                  <Code>CCD</Code>
                  <Description>Customs Client Code</Description>
                </Type>
                <Value>4382981G</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </AdditionalBill>
    </AdditionalBillCollection>

    <ContainerCollection>
      <Container>
        <ContainerNumber>CONT1</ContainerNumber>
        <ContainerType>
          <Code>4008</Code>
          <Category>
            <Code>DRY</Code>
            <Description>Dry Storage</Description>
          </Category>
          <Description>40 FT LONG X 8 FT WIDE X 8 FT HIGH</Description>
          <ISOCode>4210</ISOCode>
        </ContainerType>
        <Link>1</Link>
        <Seal>SL31</Seal>
        <SecondSeal>SL34</SecondSeal>
      </Container>
      <Container>
        <ContainerNumber>NC</ContainerNumber>
        <Link>2</Link>
        <Seal></Seal>
        <SecondSeal></SecondSeal>

        <UNDGCollection>
          <UNDG>
            <UNDGCode>2000</UNDGCode>
            <FlashPoint>1.0</FlashPoint>
            <IMOClass>4.1</IMOClass>
            <MarinePollutant>
              <Code></Code>
              <Description></Description>
            </MarinePollutant>
            <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
            <PackingGroup>III</PackingGroup>
            <ProperShippingName>CELLULOID</ProperShippingName>
            <TechicalName></TechicalName>
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
      <Container>
        <ContainerNumber>CONT1</ContainerNumber>
        <ContainerType>
          <Code>4008</Code>
          <Category>
            <Code>DRY</Code>
            <Description>Dry Storage</Description>
          </Category>
          <Description>40 FT LONG X 8 FT WIDE X 8 FT HIGH</Description>
          <ISOCode>4210</ISOCode>
        </ContainerType>
        <Link>3</Link>
        <Seal>SL31</Seal>
        <SecondSeal>SL34</SecondSeal>
      </Container>
      <Container>
        <ContainerNumber>CONT1</ContainerNumber>
        <Link>4</Link>
        <Seal></Seal>
        <SecondSeal></SecondSeal>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2014-02-11T00:00:00</Value>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2014-02-05T00:00:00</Value>
      </Date>
    </DateCollection>

    <InBondMoveHeaderCollection>
      <InBondMoveHeader>
        <AdditionalText></AdditionalText>
        <BioterrorismActIndicator>Y</BioterrorismActIndicator>
        <CustomsAgent>
          <Code>DN</Code>
          <Name>Dong The Ding</Name>
        </CustomsAgent>
        <CustomsStatus>
          <Code></Code>
          <Description>Not Sent</Description>
        </CustomsStatus>
        <DestinationPortScheduleD>
          <Code>4900</Code>
          <Description>FIELD OPERATIONS</Description>
        </DestinationPortScheduleD>
        <EntryType>
          <Code>62</Code>
          <Description>Transport and Export</Description>
        </EntryType>
        <ExportTransportMode>
          <Code>20</Code>
          <Description>Rail, Non-container</Description>
        </ExportTransportMode>
        <ExportVesselName>GALAXY ACE</ExportVesselName>
        <ForeignDestinationPortScheduleK>
          <Code>61401</Code>
          <Description>AUCKLAND, NEW ZEALAND</Description>
        </ForeignDestinationPortScheduleK>
        <ForeignDestinationPortUNLOCO>
          <Code>NZAKL</Code>
          <Name>Auckland</Name>
        </ForeignDestinationPortUNLOCO>
        <InBondCarrierID>34353</InBondCarrierID>
        <InBondCarrierSCAC>APLU</InBondCarrierSCAC>
        <LastForeignPortScheduleK>
          <Code></Code>
        </LastForeignPortScheduleK>
        <MonetaryValue>30233.0000</MonetaryValue>
        <PortOfPresentationScheduleD>
          <Code>3901</Code>
          <Description>CHICAGO, IL</Description>
        </PortOfPresentationScheduleD>
        <Seals></Seals>
        <TransferOfLiabilityCarrierCode>TO32</TransferOfLiabilityCarrierCode>
        <TransferOfLiabilityCarrierID>11-987654300</TransferOfLiabilityCarrierID>
        <TransferOfLiabilityCityName>TOL CITY</TransferOfLiabilityCityName>
        <TransferOfLiabilityStateCode>
          <Code>AL</Code>
          <Description>Alabama</Description>
        </TransferOfLiabilityStateCode>

        <DateCollection>
          <Date>
            <Type>EntryDate</Type>
            <IsEstimate>true</IsEstimate>
            <Value>2014-02-05T00:00:00</Value>
          </Date>
          <Date>
            <Type>Arrival</Type>
            <IsEstimate>true</IsEstimate>
            <Value>2014-02-12T23:57:00</Value>
          </Date>
          <Date>
            <Type>Departure</Type>
            <IsEstimate>true</IsEstimate>
            <Value>2014-02-15T23:57:00</Value>
          </Date>
          <Date>
            <Type>TransferOfLiability</Type>
            <IsEstimate>true</IsEstimate>
            <Value>2014-02-15T23:57:00</Value>
          </Date>
        </DateCollection>

        <EntryNumberCollection>
          <EntryNumber>
            <Number></Number>
            <Type>
              <Code>GON</Code>
              <Description>G.O. Number</Description>
            </Type>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type>
              <Code>INB</Code>
              <Description>In-Bond Transit Number</Description>
            </Type>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type>
              <Code>PDN</Code>
              <Description>Pedimento Number</Description>
            </Type>
          </EntryNumber>
        </EntryNumberCollection>

        <InBondMoveDetailCollection>
          <InBondMoveDetail>
            <AdditionalBillLink>1</AdditionalBillLink>
            <CustomsStatus>
              <Code></Code>
              <Description>Not Sent</Description>
            </CustomsStatus>
            <InBondQuantity>20</InBondQuantity>
            <PreviousInBondTransitDate></PreviousInBondTransitDate>
            <PreviousInBondTransitNumber></PreviousInBondTransitNumber>
            <PreviousInBondTransitPortScheduleD>
              <Code></Code>
            </PreviousInBondTransitPortScheduleD>
            <PreviousInBondTransitType>
              <Code></Code>
            </PreviousInBondTransitType>
            <SequenceNumber></SequenceNumber>

            <CustomsReferenceCollection>
              <CustomsReference>
                <Type>
                  <Code>SNP</Code>
                  <Description>Secondary Notify Party</Description>
                </Type>
                <Reference>3901SV9</Reference>
                <SubType>
                  <Code>1</Code>
                  <Description>First</Description>
                </SubType>
              </CustomsReference>
            </CustomsReferenceCollection>
          </InBondMoveDetail>
          <InBondMoveDetail>
            <AdditionalBillLink>2</AdditionalBillLink>
            <CustomsStatus>
              <Code></Code>
              <Description>Not Sent</Description>
            </CustomsStatus>
            <InBondQuantity>10</InBondQuantity>
            <PreviousInBondTransitDate></PreviousInBondTransitDate>
            <PreviousInBondTransitNumber></PreviousInBondTransitNumber>
            <PreviousInBondTransitPortScheduleD>
              <Code></Code>
            </PreviousInBondTransitPortScheduleD>
            <PreviousInBondTransitType>
              <Code></Code>
            </PreviousInBondTransitType>
            <SequenceNumber></SequenceNumber>

            <CustomsReferenceCollection>
              <CustomsReference>
                <Type>
                  <Code>SNP</Code>
                  <Description>Secondary Notify Party</Description>
                </Type>
                <Reference>3901SV9</Reference>
                <SubType>
                  <Code>1</Code>
                  <Description>First</Description>
                </SubType>
              </CustomsReference>
            </CustomsReferenceCollection>
          </InBondMoveDetail>
        </InBondMoveDetailCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>InBondCarrier</AddressType>
            <AddressShortCode>PST: 44 VO VAN TAN STREET</AddressShortCode>
            <OrganizationCode>AALOGISGN</OrganizationCode>
            <Address1>44 VO VAN TAN STREET, WARD 6, DISTRICT 3</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>HOCHIMINH CITY</City>
            <CompanyName>AA &amp; LOGISTICSX</CompanyName>
            <Contact>Nguyen Chi</Contact>
            <Country>
              <Code>VN</Code>
              <Name>Viet Nam</Name>
            </Country>
            <Email>""nguyen chi"" &lt;chi.account@annam.com.vn&gt;</Email>
            <Fax>+61 (2) 589-7852</Fax>
            <GovRegNum>12-3987234</GovRegNum>
            <GovRegNumType>
              <Code>EIN</Code>
              <Description>Employer Identification Number</Description>
            </GovRegNumType>
            <Mobile></Mobile>
            <Phone>+84 (3) 3333</Phone>
            <Port>
              <Code>VNSGN</Code>
              <Name>Ho Chi Minh City</Name>
            </Port>
            <Postcode>61205</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>CCC</Code>
                  <Description>Standard Carrier Alpha Code</Description>
                </Type>
                <Value>APLU</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>TransferOfLiabilityCarrier</AddressType>
            <AddressShortCode>PST: 27 ALICK ROAD</AddressShortCode>
            <OrganizationCode>GAFLOGMEL</OrganizationCode>
            <Address1>27 ALICK ROAD</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>BROOKLYN</City>
            <CompanyName>GAFFNEY LOGISTICS PTY LTD</CompanyName>
            <Contact>ANNA</Contact>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email>anna.anthoulas@gaffneylogistics.com.au</Email>
            <Fax>+61 (0) 3-8325-5845</Fax>
            <GovRegNum>11-987654300</GovRegNum>
            <GovRegNumType>
              <Code>EIN</Code>
              <Description>Employer Identification Number</Description>
            </GovRegNumType>
            <Mobile></Mobile>
            <Phone>+61 (0) 3-8325-5800</Phone>
            <Port>
              <Code>AUMEL</Code>
              <Name>Melbourne</Name>
            </Port>
            <Postcode>3012</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>VIC</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>AU</Code>
                  <Name>Australia</Name>
                </CountryOfIssue>
                <Type>
                  <Code>ABN</Code>
                  <Description>Australian Business Number ( Regist</Description>
                </Type>
                <Value>17055667993001</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>AU</Code>
                  <Name>Australia</Name>
                </CountryOfIssue>
                <Type>
                  <Code>CMP</Code>
                  <Description>Customs Manifest Provider Code</Description>
                </Type>
                <Value>C667993001</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>AU</Code>
                  <Name>Australia</Name>
                </CountryOfIssue>
                <Type>
                  <Code>CCP</Code>
                  <Description>Customs Controlled Premises Code</Description>
                </Type>
                <Value>EF79D</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </InBondMoveHeader>
      <InBondMoveHeader>
        <AdditionalText></AdditionalText>
        <BioterrorismActIndicator>N</BioterrorismActIndicator>
        <CustomsAgent>
          <Code>DN</Code>
          <Name>Dong The Ding</Name>
        </CustomsAgent>
        <CustomsStatus>
          <Code></Code>
          <Description>Not Sent</Description>
        </CustomsStatus>
        <DestinationPortScheduleD>
          <Code></Code>
        </DestinationPortScheduleD>
        <EntryType>
          <Code>61</Code>
          <Description>Immediate Transport</Description>
        </EntryType>
        <ExportTransportMode>
          <Code></Code>
        </ExportTransportMode>
        <ExportVesselName></ExportVesselName>
        <ForeignDestinationPortScheduleK>
          <Code></Code>
        </ForeignDestinationPortScheduleK>
        <ForeignDestinationPortUNLOCO>
          <Code>SGSIN</Code>
          <Name>Singapore</Name>
        </ForeignDestinationPortUNLOCO>
        <InBondCarrierID>27227</InBondCarrierID>
        <InBondCarrierSCAC>FAAC</InBondCarrierSCAC>
        <LastForeignPortScheduleK>
          <Code></Code>
        </LastForeignPortScheduleK>
        <MonetaryValue>0.0000</MonetaryValue>
        <PortOfPresentationScheduleD>
          <Code>3901</Code>
          <Description>CHICAGO, IL</Description>
        </PortOfPresentationScheduleD>
        <Seals></Seals>
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
            <Value>2014-02-06T00:00:00</Value>
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
              <Code>GON</Code>
              <Description>G.O. Number</Description>
            </Type>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type>
              <Code>INB</Code>
              <Description>In-Bond Transit Number</Description>
            </Type>
          </EntryNumber>
          <EntryNumber>
            <Number></Number>
            <Type>
              <Code>PDN</Code>
              <Description>Pedimento Number</Description>
            </Type>
          </EntryNumber>
        </EntryNumberCollection>

        <InBondMoveDetailCollection>
          <InBondMoveDetail>
            <AdditionalBillLink>2</AdditionalBillLink>
            <CustomsStatus>
              <Code></Code>
              <Description>Not Sent</Description>
            </CustomsStatus>
            <InBondQuantity>32</InBondQuantity>
            <PreviousInBondTransitDate>2014-02-10T00:05:00</PreviousInBondTransitDate>
            <PreviousInBondTransitNumber>3IT23</PreviousInBondTransitNumber>
            <PreviousInBondTransitPortScheduleD>
              <Code>4101</Code>
              <Description>CLEVELAND, OH</Description>
            </PreviousInBondTransitPortScheduleD>
            <PreviousInBondTransitType>
              <Code>61</Code>
              <Description>Immediate Transport</Description>
            </PreviousInBondTransitType>
            <SequenceNumber></SequenceNumber>

            <CustomsReferenceCollection>
              <CustomsReference>
                <Type>
                  <Code>SNP</Code>
                  <Description>Secondary Notify Party</Description>
                </Type>
                <Reference>3901SV9</Reference>
                <SubType>
                  <Code>1</Code>
                  <Description>First</Description>
                </SubType>
              </CustomsReference>
              <CustomsReference>
                <Type>
                  <Code>SNP</Code>
                  <Description>Secondary Notify Party</Description>
                </Type>
                <Reference>SNP2</Reference>
                <SubType>
                  <Code>2</Code>
                  <Description>Second</Description>
                </SubType>
              </CustomsReference>
              <CustomsReference>
                <Type>
                  <Code>SNP</Code>
                  <Description>Secondary Notify Party</Description>
                </Type>
                <Reference>SNP3</Reference>
                <SubType>
                  <Code>3</Code>
                  <Description>Third</Description>
                </SubType>
              </CustomsReference>
              <CustomsReference>
                <Type>
                  <Code>SNP</Code>
                  <Description>Secondary Notify Party</Description>
                </Type>
                <Reference>SNP4</Reference>
                <SubType>
                  <Code>4</Code>
                  <Description>Fourth</Description>
                </SubType>
              </CustomsReference>
            </CustomsReferenceCollection>
          </InBondMoveDetail>
        </InBondMoveDetailCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>InBondCarrier</AddressType>
            <AddressShortCode>11F BAIKNAM BLDG 188-3 EU</AddressShortCode>
            <OrganizationCode>FAILINSEL</OrganizationCode>
            <Address1>11F BAIKNAM BLDG 188-3 EULJIRO 1 GA JOONG KU</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>SEOUL</City>
            <CompanyName>FAIRCON LINE CO. LTD</CompanyName>
            <Country>
              <Code>KR</Code>
              <Name>Korea, Republic of</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>KRSEL</Code>
              <Name>Seoul</Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>11</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>CCC</Code>
                  <Description>Standard Carrier Alpha Code</Description>
                </Type>
                <Value>FAAC</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>
      </InBondMoveHeader>
    </InBondMoveHeaderCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ImporterDocumentaryAddress</AddressType>
        <AddressShortCode>OFC: USIMP ADDRESS 1</AddressShortCode>
        <OrganizationCode>USIMP</OrganizationCode>
        <Address1>USIMP ADDRESS 1</Address1>
        <Address2>USIMP ADDRESS 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>USIMPCITY</City>
        <CompanyName>USIMP COMPANY NAME</CompanyName>
        <Contact>Bob Smith</Contact>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>bob@usimp.com</Email>
        <Fax>+1 (801) 232-2277</Fax>
        <GovRegNum>91-013199000</GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone>+1 (801) 324-2366</Phone>
        <Port>
          <Code>USLAX</Code>
          <Name>Los Angeles</Name>
        </Port>
        <Postcode>96100</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>CA</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <PackingLineCollection>
      <PackingLine>
        <ContainerLink>1</ContainerLink>
        <GoodsDescription></GoodsDescription>
        <HarmonisedCode>1001100010</HarmonisedCode>
        <LinePrice>0.0000</LinePrice>
        <MarksAndNos>N/M</MarksAndNos>
        <PackQty>0</PackQty>
        <PackType>
          <Code>KEG</Code>
        </PackType>
        <Weight>0.0000</Weight>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
      </PackingLine>
      <PackingLine>
        <ContainerLink>2</ContainerLink>
        <GoodsDescription>GOODS</GoodsDescription>
        <HarmonisedCode>3001100010</HarmonisedCode>
        <LinePrice>30.0000</LinePrice>
        <MarksAndNos>MAKRSN/M</MarksAndNos>
        <PackQty>2</PackQty>
        <PackType>
          <Code>KEG</Code>
        </PackType>
        <Weight>200.0000</Weight>
        <WeightUnit>
          <Code>KG</Code>
          <Description>Kilograms</Description>
        </WeightUnit>
      </PackingLine>
      <PackingLine>
        <ContainerLink>2</ContainerLink>
        <GoodsDescription>493L</GoodsDescription>
        <HarmonisedCode>5001000000</HarmonisedCode>
        <LinePrice>2.0000</LinePrice>
        <MarksAndNos>MAKRSN/M</MarksAndNos>
        <PackQty>24</PackQty>
        <PackType>
          <Code>DJ</Code>
          <Description>Demijohn, Non Protected</Description>
        </PackType>
        <Weight>32.0000</Weight>
        <WeightUnit>
          <Code>LB</Code>
          <Description>Pounds</Description>
        </WeightUnit>
      </PackingLine>
      <PackingLine>
        <ContainerLink>3</ContainerLink>
        <GoodsDescription></GoodsDescription>
        <HarmonisedCode>6001102000</HarmonisedCode>
        <LinePrice>1.0000</LinePrice>
        <MarksAndNos>N/M</MarksAndNos>
        <PackQty>0</PackQty>
        <PackType>
          <Code>PAL</Code>
          <Description>Pallet</Description>
        </PackType>
        <Weight>0.0000</Weight>
        <WeightUnit>
          <Code>LT</Code>
          <Description>Pounds Troy</Description>
        </WeightUnit>
      </PackingLine>
      <PackingLine>
        <ContainerLink>4</ContainerLink>
        <GoodsDescription></GoodsDescription>
        <HarmonisedCode>4001100000</HarmonisedCode>
        <LinePrice>0.0000</LinePrice>
        <MarksAndNos>N/M</MarksAndNos>
        <PackQty>0</PackQty>
        <PackType>
          <Code>PAL</Code>
          <Description>Pallet</Description>
        </PackType>
        <Weight>0.0000</Weight>
        <WeightUnit>
          <Code>LT</Code>
          <Description>Pounds Troy</Description>
        </WeightUnit>
      </PackingLine>
    </PackingLineCollection>

    <TransportLegCollection>
      <TransportLeg>
        <PortOfLoading>
          <Code>AU</Code>
          <Name>Australia</Name>
        </PortOfLoading>
        <LegOrder>1</LegOrder>
        <ActualDeparture>2014-02-06T00:00:00</ActualDeparture>
      </TransportLeg>
      <TransportLeg>
        <PortOfDischarge>
          <Code></Code>
        </PortOfDischarge>
        <PortOfLoading>
          <Code></Code>
        </PortOfLoading>
        <LegOrder>2</LegOrder>
        <ActualDeparture>2014-02-05T00:00:00</ActualDeparture>
        <EstimatedArrival>2014-02-11T00:00:00</EstimatedArrival>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO></VesselLloydsIMO>
        <VesselName>AALSMEERGRACHT</VesselName>
        <VoyageFlightNo>1655</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}
	}
}
