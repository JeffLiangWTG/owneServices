using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Macros;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HVLVOriginLoadListDataContextManager))]
	public class HVLVOriginLoadListDataContextManagerTest : ShipmentDataContextManagerTestCase<HVLVOriginLoadListDataContextManager, HVLVOriginLoadList>
	{
		public void TestMatchingByDataContextKey()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_UniqueReference = "HVL000000000000001";

			Factory.SaveForTesting();

			var loadListDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			loadListDataObject.DataContext = DataContextFactory.New();
			loadListDataObject.DataContext.AddDataTarget(DataContextType.HVLVOriginLoadList, "HVL000000000000001");
			loadListDataObject.TransportMode = new CodeDescriptionPair();
			loadListDataObject.TransportMode.Code = TransportModes.Air;
			loadListDataObject.ServiceLevel = new ServiceLevel { Code = "D2D" };
			loadListDataObject.OperationalStatus = new CodeDescriptionPair
			{
				Code = "OPN",
				Description = "Open"
			};

			loadListDataObject.SetAddInfoCollection(() => new List<AddInfo>());
			loadListDataObject.SetAdditionalBillCollection(() => new List<AdditionalBill>());
			loadListDataObject.SetContainerCollection(() => new DataObjectList<Container>());
			loadListDataObject.SetDateCollection(() => new List<Date>());
			loadListDataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());

			var org = new Organization(Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "DESORG")).FirstOrDefault());
			var destinationDepot = new OrganizationAddress();
			destinationDepot.OrganizationCode = org.Code;
			destinationDepot.AddressShortCode = "2DDS";
			destinationDepot.AddressType = "ArrivalCFSAddress";
			loadListDataObject.OrganizationAddressCollection.Add(destinationDepot);

			var packline = new PackingLine(DefaultDataObjectWriterStrategy.TestInstance);
			loadListDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>());
			loadListDataObject.PackingLineCollection.Add(packline);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var message = GetQueuedUniversalShipmentMessage(loadListDataObject);
			manager.Process(message);

			CombineAssertions(() =>
			{
				AssertEquals("Expected message.EM_Status be ProcessedOK", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

				AssertMultilineASCIIEquals("Expected the following service task log", @"
Updated HVLV Origin Load List HVL000000000000001 from UniversalShipment.
Successfully saved HVLV Origin Load List HVL000000000000001.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Expected message log note text", @"
Successfully loaded matching HVLVOriginLoadList.
Populating HVLVOriginLoadList...
Matching 'ArrivalCFSAddress':- Matched to 'DESORG' by code, address '2DDS' by short code.
Updated HVLV Origin Load List HVL000000000000001 from UniversalShipment.
Successfully saved HVLV Origin Load List HVL000000000000001.
".Trim(), message.GetLogNoteText());

				var loadLists = new BusinessObjectFactory().Load<HVLVOriginLoadList>(new ZQuery());
				AssertEquals("There should be an existing HVLVOriginLoadList", 1, loadLists.Length);
			});
		}

		#region Implementation

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => System.Array.Empty<RecipientRoleType>();

		public static string SampleLoadListXML
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>HVLVOriginLoadList</Type>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>DAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Your Australia Corp</Name>
      </Company>
      <DataProvider>WTLBS1DAU</DataProvider>
      <EnterpriseID>WTL</EnterpriseID>
      <EventBranch>
        <Code>A01</Code>
        <Name>AU - Branch 1</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>AAS</Code>
        <Description>Accreditation Attempt Commenced</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>BS1</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDescription>Test</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ACF</Code>
          <Description>Arrival CFS</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <PortOfDestination>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfOrigin>
    <OperationalStatus>
      <Code>OPN</Code>
      <Description>Open</Description>
    </OperationalStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FC1</Code>
    </ShipmentIncoTerm>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>AIDA</VesselName>
    <VoyageFlightNo>VOYAGE001</VoyageFlightNo>

    <AddInfoCollection>
      <AddInfo>
        <Key>ReferenceNumber</Key>
        <Value>LOADLIST1</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalBillCollection>
      <AdditionalBill>
        <BillNumber>111222333</BillNumber>
        <BillType>
          <Code>MB</Code>
          <Description>Master Bill</Description>
        </BillType>
      </AdditionalBill>
      <AdditionalBill>
        <BillNumber>456456456</BillNumber>
        <BillType>
          <Code>HB</Code>
          <Description>House Bill</Description>
        </BillType>
      </AdditionalBill>
    </AdditionalBillCollection>

    <ContainerCollection>
      <Container>
        <ContainerNumber>C3</ContainerNumber>
        <ContainerType>
          <Code>TNK</Code>
          <Category>
            <Code>TNK</Code>
            <Description>Tank</Description>
			<ISOCode>TNK</ISOCode>
          </Category>
          <Description>TANK FOR GAS</Description>
          <ISOCode>TNK</ISOCode>
        </ContainerType>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-03-04T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-03-05T00:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <Address1>Carrier Avenue</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>Carrier Avenue</AddressShortCode>
        <City>Los Angeles</City>
        <CompanyName>ACCU GREEN MANUFACTURING (AU) CORPORATION</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>main.auper@accugreen.com</Email>
        <Fax>+61294471223</Fax>
        <OrganizationCode>CARRIERORG</OrganizationCode>
        <Phone>+61294471212</Phone>
        <Port>
          <Code>USLAX</Code>
          <Name>Los Angeles</Name>
        </Port>
        <Postcode>4000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State Description=""California"">CA</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LSC</Code>
              <Description>Legacy System Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>ACCGRE_AU</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCFSAddress</AddressType>
        <Address1>1 Origin Depot to Departure Street</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1 Origin Depot to Departure Street</AddressShortCode>
        <City>Brisbane</City>
        <CompanyName>AU CFS DEPOT</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>main.ausyd@cfsdepot.com</Email>
        <Fax>+61296852222</Fax>
        <OrganizationCode>ORIGINORG</OrganizationCode>
        <Phone>+611300746123</Phone>
        <Port>
          <Code>AUBNE</Code>
          <Name>Brisbane</Name>
        </Port>
        <Postcode>4000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State Description=""Queensland"">QL</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LSC</Code>
              <Description>Legacy System Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>AUCFSDSYD</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>CCP</Code>
              <Description>Customs Controlled Premises Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>7818M</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ArrivalCFSAddress</AddressType>
        <Address1>2 Destination Depot Street</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>2DDS</AddressShortCode>
        <City>Sydney</City>
        <CompanyName>AU CFS DEPOT</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>main.aumel@cfsdepot.com</Email>
        <Fax>+61392099876</Fax>
        <OrganizationCode>DESORG</OrganizationCode>
        <Phone>+61392096543</Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State Description=""Victoria"">VIC</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LSC</Code>
              <Description>Legacy System Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>AUCFSDMEL</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>CCP</Code>
              <Description>Customs Controlled Premises Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>9975N</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection Content=""Complete"">
        <PackingLine>
            <ReferenceNumber>IanIsHot</ReferenceNumber>
         </PackingLine>
        <PackingLine>
            <ReferenceNumber>WindItUp</ReferenceNumber>
        </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		public static string SampleLoadListXMLWithWrongDestinationDepot
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>HVLVOriginLoadList</Type>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>DAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Your Australia Corp</Name>
      </Company>
      <DataProvider>WTLBS1DAU</DataProvider>
      <EnterpriseID>WTL</EnterpriseID>
      <EventBranch>
        <Code>A01</Code>
        <Name>AU - Branch 1</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>AAS</Code>
        <Description>Accreditation Attempt Commenced</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>BS1</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDescription>Test</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ACF</Code>
          <Description>Arrival CFS</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <PortOfDestination>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfOrigin>
    <OperationalStatus>
      <Code>OPN</Code>
      <Description>Open</Description>
    </OperationalStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FC1</Code>
    </ShipmentIncoTerm>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>AIDA</VesselName>
    <VoyageFlightNo>VOYAGE001</VoyageFlightNo>

    <AddInfoCollection>
      <AddInfo>
        <Key>ReferenceNumber</Key>
        <Value>LOADLIST1</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalBillCollection>
      <AdditionalBill>
        <BillNumber>111222333</BillNumber>
        <BillType>
          <Code>MB</Code>
          <Description>Master Bill</Description>
        </BillType>
      </AdditionalBill>
      <AdditionalBill>
        <BillNumber>456456456</BillNumber>
        <BillType>
          <Code>HB</Code>
          <Description>House Bill</Description>
        </BillType>
      </AdditionalBill>
    </AdditionalBillCollection>

    <ContainerCollection>
      <Container>
        <ContainerNumber>C3</ContainerNumber>
        <ContainerType>
          <Code>TNK</Code>
          <Category>
            <Code>TNK</Code>
            <Description>Tank</Description>
			<ISOCode>TNK</ISOCode>
          </Category>
          <Description>TANK FOR GAS</Description>
          <ISOCode>TNK</ISOCode>
        </ContainerType>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-03-04T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-03-05T00:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <Address1>Carrier Avenue</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>Carrier Avenue</AddressShortCode>
        <City>Los Angeles</City>
        <CompanyName>ACCU GREEN MANUFACTURING (AU) CORPORATION</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>main.auper@accugreen.com</Email>
        <Fax>+61294471223</Fax>
        <OrganizationCode>CARRIERORG</OrganizationCode>
        <Phone>+61294471212</Phone>
        <Port>
          <Code>USLAX</Code>
          <Name>Los Angeles</Name>
        </Port>
        <Postcode>4000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State Description=""California"">CA</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LSC</Code>
              <Description>Legacy System Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>ACCGRE_AU</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCFSAddress</AddressType>
        <Address1>1 Origin Depot to Departure Street</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1 Origin Depot to Departure Street</AddressShortCode>
        <City>Brisbane</City>
        <CompanyName>AU CFS DEPOT</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>main.ausyd@cfsdepot.com</Email>
        <Fax>+61296852222</Fax>
        <OrganizationCode>ORIGINORG</OrganizationCode>
        <Phone>+611300746123</Phone>
        <Port>
          <Code>AUBNE</Code>
          <Name>Brisbane</Name>
        </Port>
        <Postcode>4000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State Description=""Queensland"">QL</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LSC</Code>
              <Description>Legacy System Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>AUCFSDSYD</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>CCP</Code>
              <Description>Customs Controlled Premises Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>7818M</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ArrivalCFSAddress</AddressType>
        <Address1>Some Wrong Address</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>WrongCode</AddressShortCode>
        <City>Sydney</City>
        <CompanyName>WrongCompanyName</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>main.aumel@cfsdepot.com</Email>
        <Fax>+61392099876</Fax>
        <OrganizationCode>WrongCode</OrganizationCode>
        <Phone>+61392096543</Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State Description=""Victoria"">VIC</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection Content=""Complete"">
        <PackingLine>
            <ReferenceNumber>IanIsHot</ReferenceNumber>
         </PackingLine>
        <PackingLine>
            <ReferenceNumber>WindItUp</ReferenceNumber>
        </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		public static string SampleLoadListXMLWithoutDestinationDepot
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>HVLVOriginLoadList</Type>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>DAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Your Australia Corp</Name>
      </Company>
      <DataProvider>WTLBS1DAU</DataProvider>
      <EnterpriseID>WTL</EnterpriseID>
      <EventBranch>
        <Code>A01</Code>
        <Name>AU - Branch 1</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>AAS</Code>
        <Description>Accreditation Attempt Commenced</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>BS1</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDescription>Test</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ACF</Code>
          <Description>Arrival CFS</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <PortOfDestination>
      <Code>USLAX</Code>
      <Name>Los Angeles</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfOrigin>
    <OperationalStatus>
      <Code>OPN</Code>
      <Description>Open</Description>
    </OperationalStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FC1</Code>
    </ShipmentIncoTerm>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>AIDA</VesselName>
    <VoyageFlightNo>VOYAGE001</VoyageFlightNo>

    <AddInfoCollection>
      <AddInfo>
        <Key>ReferenceNumber</Key>
        <Value>LOADLIST1</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalBillCollection>
      <AdditionalBill>
        <BillNumber>111222333</BillNumber>
        <BillType>
          <Code>MB</Code>
          <Description>Master Bill</Description>
        </BillType>
      </AdditionalBill>
      <AdditionalBill>
        <BillNumber>456456456</BillNumber>
        <BillType>
          <Code>HB</Code>
          <Description>House Bill</Description>
        </BillType>
      </AdditionalBill>
    </AdditionalBillCollection>

    <ContainerCollection>
      <Container>
        <ContainerNumber>C3</ContainerNumber>
        <ContainerType>
          <Code>TNK</Code>
          <Category>
            <Code>TNK</Code>
            <Description>Tank</Description>
			<ISOCode>TNK</ISOCode>
          </Category>
          <Description>TANK FOR GAS</Description>
          <ISOCode>TNK</ISOCode>
        </ContainerType>
      </Container>
    </ContainerCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-03-04T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
        <Value>2020-03-05T00:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <Address1>Carrier Avenue</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>Carrier Avenue</AddressShortCode>
        <City>Los Angeles</City>
        <CompanyName>ACCU GREEN MANUFACTURING (AU) CORPORATION</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email>main.auper@accugreen.com</Email>
        <Fax>+61294471223</Fax>
        <OrganizationCode>CARRIERORG</OrganizationCode>
        <Phone>+61294471212</Phone>
        <Port>
          <Code>USLAX</Code>
          <Name>Los Angeles</Name>
        </Port>
        <Postcode>4000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State Description=""California"">CA</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LSC</Code>
              <Description>Legacy System Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Value>ACCGRE_AU</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCFSAddress</AddressType>
        <Address1>1 Origin Depot to Departure Street</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>1 Origin Depot to Departure Street</AddressShortCode>
        <City>Brisbane</City>
        <CompanyName>AU CFS DEPOT</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>main.ausyd@cfsdepot.com</Email>
        <Fax>+61296852222</Fax>
        <OrganizationCode>ORIGINORG</OrganizationCode>
        <Phone>+611300746123</Phone>
        <Port>
          <Code>AUBNE</Code>
          <Name>Brisbane</Name>
        </Port>
        <Postcode>4000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State Description=""Queensland"">QL</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>LSC</Code>
              <Description>Legacy System Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>AUCFSDSYD</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>CCP</Code>
              <Description>Customs Controlled Premises Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>7818M</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection Content=""Complete"">
        <PackingLine>
            <ReferenceNumber>IanIsHot</ReferenceNumber>
         </PackingLine>
        <PackingLine>
            <ReferenceNumber>WindItUp</ReferenceNumber>
        </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		public static string SampleLoadListXML_WithValidDataTarget
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>HVLVOriginLoadList</Type>
          <Key>LOADLIST1</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <OperationalStatus>
      <Code>LDG</Code>
      <Description>Open</Description>
    </OperationalStatus>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ArrivalCFSAddress</AddressType>
        <Address1>2 Destination Depot Street</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>2DDS</AddressShortCode>
        <City>Sydney</City>
        <CompanyName>AU CFS DEPOT</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>main.aumel@cfsdepot.com</Email>
        <Fax>+61392099876</Fax>
        <OrganizationCode>DESORG</OrganizationCode>
        <Phone>+61392096543</Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State Description=""Victoria"">VIC</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		public static string SampleExistingLoadListXML_WithCompletePackingLine
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>HVLVOriginLoadList</Type>
          <Key>LOADLIST1</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <PackingLineCollection Content=""Complete"">
        <PackingLine>
            <ReferenceNumber>IanIsHot</ReferenceNumber>
         </PackingLine>
        <PackingLine>
            <ReferenceNumber>WindItUp</ReferenceNumber>
        </PackingLine>
    </PackingLineCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ArrivalCFSAddress</AddressType>
        <Address1>2 Destination Depot Street</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>2DDS</AddressShortCode>
        <City>Sydney</City>
        <CompanyName>AU CFS DEPOT</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>main.aumel@cfsdepot.com</Email>
        <Fax>+61392099876</Fax>
        <OrganizationCode>DESORG</OrganizationCode>
        <Phone>+61392096543</Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State Description=""Victoria"">VIC</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		public static string SampleExistingLoadListXML_WithPartialPackingLine
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>HVLVOriginLoadList</Type>
          <Key>LOADLIST1</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <PackingLineCollection Content=""Partial"">
        <PackingLine>
            <ReferenceNumber>IanIsHot</ReferenceNumber>
         </PackingLine>
        <PackingLine>
            <ReferenceNumber>WindItUp</ReferenceNumber>
        </PackingLine>
    </PackingLineCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ArrivalCFSAddress</AddressType>
        <Address1>2 Destination Depot Street</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>2DDS</AddressShortCode>
        <City>Sydney</City>
        <CompanyName>AU CFS DEPOT</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>main.aumel@cfsdepot.com</Email>
        <Fax>+61392099876</Fax>
        <OrganizationCode>DESORG</OrganizationCode>
        <Phone>+61392096543</Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State Description=""Victoria"">VIC</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		public static string SampleLoadListXML_IsMasterHouse
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>HVLVOriginLoadList</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsMasterHouse>true</IsMasterHouse>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ArrivalCFSAddress</AddressType>
        <Address1>2 Destination Depot Street</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>2DDS</AddressShortCode>
        <City>Sydney</City>
        <CompanyName>AU CFS DEPOT</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email>main.aumel@cfsdepot.com</Email>
        <Fax>+61392099876</Fax>
        <OrganizationCode>DESORG</OrganizationCode>
        <Phone>+61392096543</Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>2000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State Description=""Victoria"">VIC</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		public static string SampleLoadListXML_WithOuterPackage
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>HVLVOriginLoadList</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <PackingLineCollection Content=""Complete"">
        <PackingLine>
            <ReferenceNumber>SimpleItem</ReferenceNumber>
        </PackingLine>
        <PackingLine>
            <ReferenceNumber>OuterPackageRef</ReferenceNumber>
            <Barcode>Ian</Barcode>
            <ContainerNumber>1223456</ContainerNumber>
            <PackType>
                <Code>BAG</Code>
            </PackType>
            <Commodity>
                <Code>KFC</Code>
            </Commodity>
            <Height>4.4</Height>
            <IsHVLVClearance>true</IsHVLVClearance>
            <Length>3.3</Length>
            <LengthUnit>
                <Code>M</Code>
                <Description>Meters</Description>
            </LengthUnit>
            <Status>OPN</Status>
            <TareWeight>2.2</TareWeight>
            <Volume>1.1</Volume>
            <VolumeUnit>
                <Code>M3</Code>
                <Description>Cubic Meters</Description>
            </VolumeUnit>
            <Weight>2.2</Weight>
            <WeightUnit>
                <Code>KG</Code>
                <Description>Kilograms</Description>
            </WeightUnit>
            <Width>5.5</Width>

            <OrganizationAddressCollection>
                <OrganizationAddress>
                    <AddressType>CustomsDepotAddress</AddressType>
                    <Address1>123 NVIDIA Street</Address1>
                    <Address2></Address2>
                    <AddressOverride>false</AddressOverride>
                    <AddressShortCode>123 NVIDIA Street</AddressShortCode>
                    <City></City>
                    <CompanyName></CompanyName>
                    <Email></Email>
                    <Fax></Fax>
                    <OrganizationCode>NVIDIA</OrganizationCode>
                    <Phone></Phone>
                    <Port>
                        <Code></Code>
                    </Port>
                    <Postcode></Postcode>
                    <ScreeningStatus>
                        <Code>NOT</Code>
                        <Description>Not Screened</Description>
                    </ScreeningStatus>
                    <State></State>
                </OrganizationAddress>
                <OrganizationAddress>
                    <AddressType>DeliveryLocalCartage</AddressType>
                    <Address1>789 AMD Street</Address1>
                    <Address2></Address2>
                    <AddressOverride>false</AddressOverride>
                    <AddressShortCode>789 AMD Street</AddressShortCode>
                    <City></City>
                    <CompanyName></CompanyName>
                    <Email></Email>
                    <Fax></Fax>
                    <OrganizationCode>AMD</OrganizationCode>
                    <Phone></Phone>
                    <Port>
                        <Code></Code>
                    </Port>
                    <Postcode></Postcode>
                    <ScreeningStatus>
                        <Code>NOT</Code>
                        <Description>Not Screened</Description>
                    </ScreeningStatus>
                    <State></State>
				</OrganizationAddress>
            </OrganizationAddressCollection>
            <PackingLineCollection Content=""Complete"">
                 <PackingLine>
                     <ReferenceNumber>OuterPackageItem1</ReferenceNumber>
                 </PackingLine>
                 <PackingLine>
                     <ReferenceNumber>OuterPackageItem2</ReferenceNumber>
                 </PackingLine>
            </PackingLineCollection>
        </PackingLine>
    </PackingLineCollection>
	<OrganizationAddressCollection>
		<OrganizationAddress>
			<AddressType>ArrivalCFSAddress</AddressType>
			<Address1>2 Destination Depot Street</Address1>
			<Address2></Address2>
			<AddressOverride>false</AddressOverride>
			<AddressShortCode>2DDS</AddressShortCode>
			<City>Sydney</City>
			<CompanyName>AU CFS DEPOT</CompanyName>
			<Country>
				<Code>AU</Code>
				<Name>Australia</Name>
			</Country>
			<Email>main.aumel@cfsdepot.com</Email>
			<Fax>+61392099876</Fax>
			<OrganizationCode>DESORG</OrganizationCode>
			<Phone>+61392096543</Phone>
			<Port>
				<Code>AUSYD</Code>
				<Name>Sydney</Name>
			</Port>
			<Postcode>2000</Postcode>
			<ScreeningStatus>
				<Code>UNK</Code>
				<Description>Unknown</Description>
			</ScreeningStatus>
			<State Description=""Victoria"">VIC</State>
			</OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		protected override string ValidPopulatedUniversalShipmentXML => SampleLoadListXML;

		public static UniversalShipment GetDataObjectFromSampleLoadListXUS(string sampleLoadListXml)
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(sampleLoadListXml)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, new DummyLogger());
			}

			return shipment;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "DESORG";

			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepot.OA_OH = orgHeader.PK;
			destinationDepot.OA_RL_NKRelatedPortCode = "AUSYD";
			destinationDepot.OA_Address1 = "2 Destination Depot Street";
			destinationDepot.OA_City = "Sydney";
			destinationDepot.OA_PostCode = "2000";
			destinationDepot.OA_Code = "2DDS";
			orgHeader.Addresses.Add(destinationDepot);

			Factory.SaveForTesting();
		}

		public static HVLVOriginLoadList SampleLoadList()
		{
			var factory = new BusinessObjectFactory();

			var loadList = factory.New<HVLVOriginLoadList>();
			loadList.HVL_VesselName = "AIDA";
			loadList.HVL_UniqueReference = "LOADLIST1";
			loadList.HVL_VoyageFlight = "VOYAGE001";
			loadList.HVL_TransportMode = TransportModes.Sea;
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			loadList.HVL_RS_NKServiceLevel = "STD";
			loadList.HVL_INCO = IncoTerms.FreeCarrierSeller;
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_RL_NKDestination = "USLAX";

			loadList.HVL_ContainerNumber = "C3";
			var refContainer = factory.New<RefContainer>();
			refContainer.RC_Code = ContainerTypes.Tank;
			loadList.HVL_RC_ContainerType = refContainer.PK;

			loadList.HVL_E_Arv = new ZDateTime(2020, 3, 5);
			loadList.HVL_E_Dep = new ZDateTime(2020, 3, 4);

			#region Carrier Org and Address

			var carrier = factory.New<OrgHeader>();
			carrier.OH_Code = "CARRIERORG";
			loadList.HVL_OH_Carrier = carrier.PK;

			carrier.MainAddress.OA_OH = carrier.PK;
			carrier.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			carrier.MainAddress.OA_Address1 = "Carrier Avenue";
			carrier.MainAddress.OA_City = "Los Angeles";

			#endregion

			#region Origin Depot Address

			var originDepotAddress = factory.NewWithValidTestData<OrgAddress>();
			loadList.HVL_OA_OriginDepot = originDepotAddress.PK;

			originDepotAddress.OA_Address1 = "1 Origin Depot to Departure Street";
			originDepotAddress.OA_City = "Brisbane";
			originDepotAddress.OA_PostCode = "4000";
			originDepotAddress.OA_RL_NKRelatedPortCode = "AUBNE";

			var originDepotHeader = factory.New<OrgHeader>();
			originDepotHeader.OH_Code = "ORIGINORG";
			originDepotAddress.OA_OH = originDepotHeader.PK;

			#endregion

			#region Origin CTO Address

			var org1 = factory.New<OrgHeader>();
			var address1 = org1.Addresses.AddNew();

			var org2 = factory.New<OrgHeader>();
			var address2 = org2.Addresses.AddNew();
			address2.OA_Address1 = "1 Origin CTO to Departure Street";
			address2.OA_City = "Brisbane";
			address2.OA_PostCode = "4000";
			address2.OA_RL_NKRelatedPortCode = "AUBNE";

			var org3 = factory.New<OrgHeader>();
			var address3 = org3.Addresses.AddNew();

			var seaCTO1 = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			seaCTO1.O5_PortOrCountry = "USLAX";
			seaCTO1.O5_OA_AgentOfficeAddress = address1.PK;

			var seaCTO2 = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			seaCTO2.O5_PortOrCountry = "AUBNE";
			seaCTO2.O5_OA_AgentOfficeAddress = address2.PK;

			var seaCTO3 = carrier.CarrierAppointedAgentPorts_Stevedore.AddNew();
			seaCTO3.O5_PortOrCountry = "AUSYD";
			seaCTO3.O5_OA_AgentOfficeAddress = address3.PK;

			#endregion

			#region Destination Depot Address

			var destinationDepot = factory.New<OrgAddress>();
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;

			destinationDepot.OA_RL_NKRelatedPortCode = "AUSYD";
			destinationDepot.OA_Address1 = "2 Destination Depot Street";
			destinationDepot.OA_Code = "2DDS";
			destinationDepot.OA_City = "Sydney";
			destinationDepot.OA_PostCode = "2000";

			var destinationDepotHeader = factory.New<OrgHeader>();
			destinationDepotHeader.OH_Code = "DESORG";
			destinationDepot.OA_OH = destinationDepotHeader.PK;

			#endregion

			loadList.HVL_MasterBillNumber = "111222333";
			loadList.HVL_HouseBillNumber = "456456456";

			var consignment = factory.NewWithValidTestData<HVLVConsignment>();

			var item1 = factory.New<HVLVItem>();
			item1.HVI_HVL_LoadList = loadList.PK;
			item1.HVI_ItemId = "HVI001";
			item1.HVI_ShipperReference = "HVI123456789";
			item1.HVI_CurrentBarcode = "Barcode";
			item1.HVI_F3_NKPackType = "BOX";
			item1.HVI_Height = 3m;
			item1.HVI_Length = 4m;
			item1.HVI_Width = 5m;
			item1.HVI_UnitOfDimension = Length.Metres;
			item1.HVI_ManifestedWeight = 5;
			item1.HVI_ActualWeight = 10;
			item1.HVI_ManifestedVolume = 2;
			item1.HVI_ActualVolume = 2;
			item1.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			item1.HVI_IsDamaged = true;
			item1.HVI_IsPillaged = true;
			item1.HVI_HVC_Consignment = consignment.PK;

			var item2 = factory.New<HVLVItem>();
			item2.HVI_HVL_LoadList = loadList.PK;
			item2.HVI_ItemId = "HVI002";
			item2.HVI_ShipperReference = "Lamelo Ball for MVP";
			item2.HVI_HVC_Consignment = consignment.PK;

			return loadList;
		}

		#endregion
	}
}
