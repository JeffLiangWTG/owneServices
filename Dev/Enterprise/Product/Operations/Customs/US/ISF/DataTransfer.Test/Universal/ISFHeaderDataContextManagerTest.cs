using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Customs.US.ISF.DataTransfer.Universal.Reader;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Testing
{
	[TestedType(typeof(ISFHeaderDataContextManager))]
	sealed class ISFHeaderDataContextManagerTest : ShipmentDataContextManagerTestCase<ISFHeaderDataContextManager, CusISFHeader>
	{
		public void TestOnLogParentFoundFromEDIMessage()
		{
			var eventXmlText = @"
<UniversalEvent>
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>ISF0000002</Key>
              <Type>USImporterSecurityFiling</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2014-09-09T09:30:10</EventTime>
        <EventType>IRJ</EventType>
        <EventParameters>
          <Reason>You are not registered with eHub. Contact WTG to register.</Reason>
          <MessageType>eBond Message to Surety Agent</MessageType>
        </EventParameters>
      </Event>
</UniversalEvent>
";
			var header = Factory.NewWithValidTestData<CusISFHeader>();
			header.BF_JobReference = "ISF0000002";
			header.BF_CustomsReference = "XJ5-11114444458";
			Factory.SaveForTesting();
			var xmlEvent = new XmlEventDeserializer().Parse(eventXmlText);
			var finder = new ISFEventParentFinder(Factory.BOFactory, new ISFHeaderDataContextManager(), new XmlSessionTracker(new ServiceTaskLogForTesting()));
			var logParents = finder.GetLogParentsForEvent(xmlEvent);
			AssertEquals(1, logParents.Length);
			var relatedObj = logParents[0] as CusISFHeader;
			AssertNotNull("Not Null", relatedObj);
			AssertEquals("Number", "ISF0000002", relatedObj.BF_JobReference);
			Assert(SuretyToBrokerNoticeMessageProcessorHelper.IsSuretyToBrokerNoticeMessage(xmlEvent as Event));
		}

		public void TestMatchOnKey()
		{
			var bizOFactory = new BusinessObjectFactory();
			var irrelevantRecord = bizOFactory.New<CusISFHeader>();
			irrelevantRecord.BF_JobReference = "ISF0001";
			var goodRecord = bizOFactory.New<CusISFHeader>();
			goodRecord.BF_JobReference = "ISF1255S000010";
			bizOFactory.Save();
			string newXml = "";
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(ValidPopulatedUniversalShipmentXML)))
			using (var reader = new XmlTextReader(stream))
			{
				var doc = XDocument.Load(reader);
				var nsManager = new XmlNamespaceManager(reader.NameTable);
				XNamespace ns = "http://www.cargowise.com/Schemas/Universal/2011/11";
				nsManager.AddNamespace("u", ns.NamespaceName);
				var dataTargetNode = doc.XPathSelectElement("//u:Shipment/u:DataContext/u:DataTargetCollection/u:DataTarget[u:Type ='USImporterSecurityFiling']", nsManager);
				dataTargetNode.Add(new XElement("Key", goodRecord.BF_JobReference));
				newXml = doc.ToString();
			}

			var logger = new DummyLogger();
			var uShipment = CreateUShipmentFromXml(newXml, logger);
			var isfReader = new ISFHeaderDataObjectReader(uShipment, logger, Factory);
			var matchedISF = isfReader.ReadIntoBusinessObject();
			AssertEquals("We should have found ISF1255S000010 using the data target key", goodRecord.PK, matchedISF.PK);
		}

		Shipment CreateUShipmentFromXml(string newXml, DummyLogger logger)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(newXml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}

			return shipment;
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		protected override bool ManagerChecksDataTargetToImport => true;

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
          <Type>USImporterSecurityFiling</Type>
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
      <DataProvider>HYEDATDUS</DataProvider>
      <EnterpriseID>HYE</EnterpriseID>
      <EventBranch>
        <Code>CHI</Code>
        <Name>Cargowise EDI - Chicago</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ATH</Code>
        <Description>Action Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2015-03-13T01:12:00</TriggerDate>
      <TriggerDescription>AAAAAAAAA</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>CNE</Code>
          <Description>Consignee</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>LAX</Code>
      <Name>Cargowise EDI - Los Angeles</Name>
    </Branch>
    <CommercialInfo>
      <CommercialInvoiceCollection>
        <CommercialInvoice>
          <CommercialInvoiceLineCollection>
            <CommercialInvoiceLine>
              <LineNo>0</LineNo>
              <CountryOfOrigin>
                <Code>US</Code>
                <Name>United States</Name>
              </CountryOfOrigin>
              <HarmonisedCode>0101.10.00</HarmonisedCode>
              <PartNo>TEST LINE 1</PartNo>

              <AddInfoCollection>
                <AddInfo>
                  <Key>CustomAttrib1</Key>
                  <Value>AAAAA1</Value>
                </AddInfo>
                <AddInfo>
                  <Key>CustomAttrib2</Key>
                  <Value>AAAA2</Value>
                </AddInfo>
              </AddInfoCollection>

              <OrganizationAddressCollection>
                <OrganizationAddress>
                  <AddressType>Manufacturer</AddressType>
                  <AddressShortCode>1111 MOGLE WAY</AddressShortCode>
                  <OrganizationCode>DEMCARSYD</OrganizationCode>
                  <Address1>1111 MOGLE WAY</Address1>
                  <Address2></Address2>
                  <AddressOverride>false</AddressOverride>
                  <City>SYNDEY</City>
                  <CompanyName>DEMO CARRIER</CompanyName>
                  <Country>
                    <Code>AU</Code>
                    <Name>Australia</Name>
                  </Country>
                  <Email></Email>
                  <Fax></Fax>
                  <Phone></Phone>
                  <Port>
                    <Code>AUSYD</Code>
                    <Name>Sydney</Name>
                  </Port>
                  <Postcode>611002</Postcode>
                  <ScreeningStatus>
                    <Code>UNK</Code>
                    <Description>Unknown</Description>
                  </ScreeningStatus>
                  <State>NSW</State>
                </OrganizationAddress>
              </OrganizationAddressCollection>
            </CommercialInvoiceLine>
            <CommercialInvoiceLine>
              <LineNo>1</LineNo>
              <CountryOfOrigin>
                <Code>ZA</Code>
                <Name>South Africa</Name>
              </CountryOfOrigin>
              <HarmonisedCode>8001.10.00</HarmonisedCode>
              <PartNo>TEST LINE 3</PartNo>

              <AddInfoCollection>
                <AddInfo>
                  <Key>CustomAttrib1</Key>
                  <Value>CCC1</Value>
                </AddInfo>
                <AddInfo>
                  <Key>CustomAttrib2</Key>
                  <Value>CCCCCC2</Value>
                </AddInfo>
              </AddInfoCollection>

              <OrganizationAddressCollection>
                <OrganizationAddress>
                  <AddressType>Manufacturer</AddressType>
                  <AddressShortCode>100 ELIZABETH STREET</AddressShortCode>
                  <OrganizationCode>REATALSYD</OrganizationCode>
                  <Address1>100 ELIZABETH STREET</Address1>
                  <Address2></Address2>
                  <AddressOverride>false</AddressOverride>
                  <City>SYDNEY</City>
                  <CompanyName>READE &amp; TALE BOOKS</CompanyName>
                  <Country>
                    <Code>AU</Code>
                    <Name>Australia</Name>
                  </Country>
                  <Email></Email>
                  <Fax></Fax>
                  <Phone></Phone>
                  <Port>
                    <Code>AUSYD</Code>
                    <Name>Sydney</Name>
                  </Port>
                  <Postcode>2000</Postcode>
                  <ScreeningStatus>
                    <Code>UNK</Code>
                    <Description>Unknown</Description>
                  </ScreeningStatus>
                  <State>NSW</State>
                </OrganizationAddress>
              </OrganizationAddressCollection>
            </CommercialInvoiceLine>
          </CommercialInvoiceLineCollection>
        </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
    <CustomsContainerMode>
      <Code>NCT</Code>
      <Description>Non-Containerized (Trans. Mode: 10,</Description>
    </CustomsContainerMode>
    <GoodsValue>120.0000</GoodsValue>
    <OwnerRef>THIS IS TEST REF</OwnerRef>
    <PortOfDestination>
      <Code>USCHI</Code>
      <Name>Chicago</Name>
    </PortOfDestination>
    <PortOfDischarge>
      <Code>CNSHA</Code>
      <Name>Shanghai</Name>
    </PortOfDischarge>
    <TotalNoOfPacks>123</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>AE</Code>
      <Description>Aerosol</Description>
    </TotalNoOfPacksPackageType>
    <TotalWeight>3225</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea (Non Container, Container) (10, 11)</Description>
    </TransportMode>
    <WayBillNumber>HB125453215</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>EntryType</Key>
        <Value>1</Value>
      </AddInfo>
      <AddInfo>
        <Key>ISFShipmentType</Key>
        <Value>01</Value>
      </AddInfo>
      <AddInfo>
        <Key>UI_NKCarrierSCAC</Key>
        <Value>AAAA</Value>
      </AddInfo>
      <AddInfo>
        <Key>ActionReason</Key>
        <Value>CT</Value>
      </AddInfo>
      <AddInfo>
        <Key>ImporterIDType</Key>
        <Value>EIN</Value>
      </AddInfo>
      <AddInfo>
        <Key>ImporterID</Key>
        <Value>57-3254016AV</Value>
      </AddInfo>
      <AddInfo>
        <Key>ISFBondHolder</Key>
        <Value>57-3254016AV</Value>
      </AddInfo>
      <AddInfo>
        <Key>ISFShipmentSubType</Key>
        <Value>01</Value>
      </AddInfo>
      <AddInfo>
        <Key>SendEquipment</Key>
        <Value>YES</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalBillCollection>
      <AdditionalBill>
        <BillNumber>HB125453215</BillNumber>
        <BillType>
          <Code>BM</Code>
          <Description>House Bill of Lading</Description>
        </BillType>
        <ParentBillNumber></ParentBillNumber>

        <AddInfoCollection>
          <AddInfo>
            <Key>BillStatus</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>MatchedDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>FirstMatched</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>
      </AdditionalBill>
      <AdditionalBill>
        <BillNumber>MBTEST1232</BillNumber>
        <BillType>
          <Code>MB</Code>
          <Description>Master Bill of Lading</Description>
        </BillType>
        <ParentBillNumber></ParentBillNumber>
      </AdditionalBill>
      <AdditionalBill>
        <BillNumber>231213123</BillNumber>
        <BillType>
          <Code>MB</Code>
          <Description>Master Bill of Lading</Description>
        </BillType>
        <ParentBillNumber></ParentBillNumber>
      </AdditionalBill>
    </AdditionalBillCollection>

    <AdditionalReferenceCollection>
    </AdditionalReferenceCollection>

    <ContainerCollection Content=""Complete"">
      <Container>
        <ContainerNumber>CONT1239802</ContainerNumber>

        <AddInfoCollection>
          <AddInfo>
            <Key>USContainerType</Key>
            <Value>20</Value>
          </AddInfo>
          <AddInfo>
            <Key>ISOSizeTypeCode</Key>
            <Value>20GP</Value>
          </AddInfo>
        </AddInfoCollection>
      </Container>
      <Container>
        <ContainerNumber>CONT2455555</ContainerNumber>

        <AddInfoCollection>
          <AddInfo>
            <Key>USContainerType</Key>
            <Value>20</Value>
          </AddInfo>
          <AddInfo>
            <Key>ISOSizeTypeCode</Key>
            <Value>40GP</Value>
          </AddInfo>
        </AddInfoCollection>
      </Container>
    </ContainerCollection>

    <CustomizedFieldCollection>
      <CustomizedField>
        <Key>CustomAttrib1</Key>
        <DataType>String</DataType>
        <Value>TEST ATTRIB 1</Value>
      </CustomizedField>
      <CustomizedField>
        <Key>CustomAttrib2</Key>
        <DataType>String</DataType>
        <Value>TEST ATTRIB 2</Value>
      </CustomizedField>
      <CustomizedField>
        <Key>CustomAttribute1</Key>
        <DataType>String</DataType>
        <Value></Value>
      </CustomizedField>
      <CustomizedField>
        <Key>CustomAttribute2</Key>
        <DataType>String</DataType>
        <Value></Value>
      </CustomizedField>
    </CustomizedFieldCollection>

    <DateCollection>
      <Date>
        <Type>ISFLastAccepted</Type>
        <IsEstimate>true</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <EntryNumberCollection>
      <EntryNumber>
        <Number>XJ5-59634512127</Number>
        <Type>
          <Code>ISF</Code>
          <Description>ISF Transaction Number</Description>
        </Type>
        <CountryOfIssue>
          <Code>US</Code>
          <Name>United States</Name>
        </CountryOfIssue>
      </EntryNumber>
      <EntryNumber>
        <Number>ENT21486235</Number>
        <Type>
          <Code>ENS</Code>
          <Description>US CBP Entry Number</Description>
        </Type>
        <CountryOfIssue>
          <Code>US</Code>
          <Name>United States</Name>
        </CountryOfIssue>
      </EntryNumber>
    </EntryNumberCollection>

    <NoteCollection>
      <Note>
        <Description>Client Visible Job Notes</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>THIS IS TEST NOTE 2 DESC</NoteText>
        <NoteContext>
          <Code>AAA</Code>
          <Description>Module: A - All, Direction: A - All, Freight: A - All</Description>
        </NoteContext>
        <Visibility>
          <Code>PUB</Code>
          <Description>CLIENT-VISIBLE</Description>
        </Visibility>
      </Note>
      <Note>
        <Description>Special Instructions</Description>
        <IsCustomDescription>false</IsCustomDescription>
        <NoteText>THIS IS TEST NOTE 1 DESC</NoteText>
        <NoteContext>
          <Code>AAA</Code>
          <Description>Module: A - All, Direction: A - All, Freight: A - All</Description>
        </NoteContext>
        <Visibility>
          <Code>PUB</Code>
          <Description>CLIENT-VISIBLE</Description>
        </Visibility>
      </Note>
    </NoteCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ImporterDocumentaryAddress</AddressType>
        <AddressShortCode>HAUPTSTRASSSE 15</AddressShortCode>
        <OrganizationCode>AAAIMPBRE</OrganizationCode>
        <Address1>HAUPTSTRASSSE 15</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>BREMEN</City>
        <CompanyName>AAA IMPORTER</CompanyName>
        <Country>
          <Code>DE</Code>
          <Name>Germany</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>57-3254016AV</GovRegNum>
        <GovRegNumType>
          <Code>EIN</Code>
          <Description>Employer Identification Number</Description>
        </GovRegNumType>
        <Phone></Phone>
        <Port>
          <Code>DEBRE</Code>
          <Name>Bremen</Name>
        </Port>
        <Postcode>11111</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>HB</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BuyingParty</AddressType>
        <AddressShortCode>.</AddressShortCode>
        <OrganizationCode>SHAANTSHA</OrganizationCode>
        <Address1>TEST ADDRESS 1</Address1>
        <Address2>TEST ADDRESS 2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>SHANGHAI</City>
        <CompanyName>SHANGHAI ÀNTS TRÀNSPORTER</CompanyName>
        <Country>
          <Code>CN</Code>
          <Name>China</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>CNSHA</Code>
          <Name>Shanghai</Name>
        </Port>
        <Postcode>210000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>31</State>

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
            <Value>CN12225212</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>BST</Code>
              <Description>Business Tax Registration Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>CN</Code>
              <Name>China</Name>
            </CountryOfIssue>
            <Value>31005252000</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>SellingParty</AddressType>
        <AddressShortCode>MACPHERSON ROAD</AddressShortCode>
        <OrganizationCode>SINWHOSIN</OrganizationCode>
        <Address1>MACPHERSON ROAD</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>SINGAPORE</City>
        <CompanyName>SINGAPORE WHOLESALER DISTRIBUTION</CompanyName>
        <Contact>SINDEMO</Contact>
        <Country>
          <Code>SG</Code>
          <Name>Singapore</Name>
        </Country>
        <Email>kim.yip@cargowise.com</Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>SGSIN</Code>
          <Name>Singapore</Name>
        </Port>
        <Postcode>24678</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>UEN</Code>
              <Description>Unique Entity Number</Description>
            </Type>
            <CountryOfIssue>
              <Code>SG</Code>
              <Name>Singapore</Name>
            </CountryOfIssue>
            <Value>194600000W</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ScheduledContainerStuffingLocation</AddressType>
        <AddressShortCode>184 Test Street</AddressShortCode>
        <OrganizationCode>SADEMOCHI</OrganizationCode>
        <Address1>184 Test Street</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>Riyadh</City>
        <CompanyName>IAN TEST FULL NAME 1</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>USCHI</Code>
          <Name>Chicago</Name>
        </Port>
        <Postcode>1234</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>IL</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Consolidator</AddressType>
        <AddressShortCode>TEST STREET2</AddressShortCode>
        <OrganizationCode>AACFOR_CN</OrganizationCode>
        <Address1>TEST STREET2Ä中山东路</Address1>
        <Address2>测试地址2</Address2>
        <AddressOverride>false</AddressOverride>
        <City>南京市</City>
        <CompanyName>ÄBC FORWARDING CÔMPANY</CompanyName>
        <Contact>Stephanie</Contact>
        <Country>
          <Code>CN</Code>
          <Name>China</Name>
        </Country>
        <Email>lara.lyness@cargowise.com</Email>
        <Fax>+1 (6) 17-9318-7766</Fax>
        <GovRegNum>111-23-4567</GovRegNum>
        <GovRegNumType>
          <Code>SSN</Code>
          <Description>Social Security Number</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone>+1 (6) 17-9318-7624</Phone>
        <Port>
          <Code>CNSHA</Code>
          <Name>Shanghai</Name>
        </Port>
        <Postcode>邮编</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>AK</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Manufacturer</AddressType>
        <AddressShortCode>1111 MOGLE WAY</AddressShortCode>
        <OrganizationCode>DEMCARSYD</OrganizationCode>
        <Address1>1111 MOGLE WAY</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>SYNDEY</City>
        <CompanyName>DEMO CARRIER</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>611002</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Manufacturer</AddressType>
        <AddressShortCode>100 ELIZABETH STREET</AddressShortCode>
        <OrganizationCode>REATALSYD</OrganizationCode>
        <Address1>100 ELIZABETH STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>SYDNEY</City>
        <CompanyName>READE &amp; TALE BOOKS</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShipToParty</AddressType>
        <AddressShortCode>65 DETROIT ROAD</AddressShortCode>
        <OrganizationCode>KRAFOOCHI</OrganizationCode>
        <Address1>65 DETROIT ROAD</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>CHICAGO</City>
        <CompanyName>KRAFT FOOD LIMITED</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone>+1 (228) 001-2222</Phone>
        <Port>
          <Code>USCHI</Code>
          <Name>Chicago</Name>
        </Port>
        <Postcode>60656</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>IL</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShipToParty</AddressType>
        <AddressShortCode>120 GEORGE STREET</AddressShortCode>
        <OrganizationCode>HANSHI_WW</OrganizationCode>
        <Address1>120 GEORGE STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>SYDNEY</City>
        <CompanyName>HANJIN SHIPPING LINE (MALAYSIA) SDN BHD</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax>+61 (2) 9292-4466</Fax>
        <Phone>+61 (2) 9292-4565</Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>CCC</Code>
              <Description>Registered Shipping Agent Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>MY</Code>
              <Name>Malaysia</Name>
            </CountryOfIssue>
            <Value>3878-9</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ShipToParty</AddressType>
        <AddressShortCode>180 FORWARDERS AVENUE</AddressShortCode>
        <OrganizationCode>JOHEXPCHI</OrganizationCode>
        <Address1>180 FORWARDERS AVENUE</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>CHICAGO</City>
        <CompanyName>JOHNSON EXPRESS FREIGHT FORWARDING</CompanyName>
        <Contact>Ryan Miller</Contact>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>ryan@johnsonexpress.com</Email>
        <Fax></Fax>
        <Mobile></Mobile>
        <Phone></Phone>
        <Port>
          <Code>USCHI</Code>
          <Name>Chicago</Name>
        </Port>
        <Postcode>6102</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>IL</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <TransportLegCollection>
      <TransportLeg>
        <PortOfDischarge>
          <Code>CNSHA</Code>
          <Name>Shanghai</Name>
        </PortOfDischarge>
        <PortOfLoading>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </PortOfLoading>
        <LegOrder>0</LegOrder>
        <ActualArrival>2015-01-25T02:49:00</ActualArrival>
        <ActualDeparture>2015-01-21T02:49:00</ActualDeparture>
        <CarrierBookingReference></CarrierBookingReference>
        <CarrierServiceLevel>
          <Code></Code>
        </CarrierServiceLevel>
        <EstimatedArrival>2015-01-26T02:49:00</EstimatedArrival>
        <EstimatedDeparture>2015-01-21T02:49:00</EstimatedDeparture>
        <LegNotes></LegNotes>
        <LegType>Main</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>9316139</VesselLloydsIMO>
        <VesselName>AIDA</VesselName>
        <VoyageFlightNo>FLIGHT1</VoyageFlightNo>
      </TransportLeg>
      <TransportLeg>
        <PortOfDischarge>
          <Code>USCHI</Code>
          <Name>Chicago</Name>
        </PortOfDischarge>
        <PortOfLoading>
          <Code>CNSHA</Code>
          <Name>Shanghai</Name>
        </PortOfLoading>
        <LegOrder>0</LegOrder>
        <ActualArrival>2015-02-10T02:49:00</ActualArrival>
        <ActualDeparture>2015-01-26T02:49:00</ActualDeparture>
        <CarrierBookingReference></CarrierBookingReference>
        <CarrierServiceLevel>
          <Code></Code>
        </CarrierServiceLevel>
        <EstimatedArrival>2015-02-12T02:49:00</EstimatedArrival>
        <EstimatedDeparture>2015-01-27T02:49:00</EstimatedDeparture>
        <LegNotes></LegNotes>
        <LegType>Other</LegType>
        <TransportMode>Sea</TransportMode>
        <VesselLloydsIMO>9150195</VesselLloydsIMO>
        <VesselName>ANL ESPRIT</VesselName>
        <VoyageFlightNo>GLIGHT2</VoyageFlightNo>
      </TransportLeg>
    </TransportLegCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}
	}
}
