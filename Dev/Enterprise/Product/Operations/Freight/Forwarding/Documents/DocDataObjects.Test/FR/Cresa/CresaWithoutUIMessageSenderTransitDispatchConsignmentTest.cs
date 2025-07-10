using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Integration;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	sealed class CresaWithoutUIMessageSenderTransitDispatchConsignmentTest : DocDataObjectWithoutUIMessageSenderTest
	{
		#region TestStmNote

		[TestDate(2024, 11, 25, 8, 12, 25)]
		public void TestSendMessage_Succeed()
		{
			var bizObj = CreateBusinessObject();
			var notificationsHandler = new NotificationsHandler();

			using (Factory.AddDisposableService())
			{
				var res = MessageSender.SendMessage(bizObj, notificationsHandler);

				var note = (bizObj as IStmNoteParent).FindOrCreateCRESAStmNote();
				AssertNotNull(note);
				AssertMultilineASCIIEquals(SucceedNote, note.ST_NoteText);
			}
		}

		[TestDate(2024, 11, 25, 8, 12, 25)]
		public void TestSendMessage_Failed()
		{
			var bizObj = CresaTestHelper.CreateDispatchConsignment(Factory);
			var notificationsHandler = new NotificationsHandler();

			using (Factory.AddDisposableService())
			{
				var res = MessageSender.SendMessage(bizObj, notificationsHandler);

				var note = bizObj.FindOrCreateCRESAStmNote();
				AssertNotNull(note);
				AssertMultilineASCIIEquals(FailedNote, note.ST_NoteText);

				var testNote = Factory.Load<StmNote>(note.PK);
				AssertNotNull(testNote);
			}
		}

		#endregion

		#region TestCannotSendCRESAMessage

		public void TestSendMessage_InvalidTransitWarehouseJob()
		{
			var bizObj = CreateBusinessObject();
			var referenceNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			referenceNumber.CE_EntryType = "PEN";
			referenceNumber.CE_SystemCreateUser = "CU3";
			referenceNumber.Parent = bizObj;
			Factory.Save();

			var notificationsHandler = new NotificationsHandler();

			using (Factory.AddDisposableService())
			{
				var res = MessageSender.SendMessage(bizObj, notificationsHandler);
				var errorMessage = string.Join(System.Environment.NewLine, notificationsHandler.Notifications.GetMessageErrors().Select(m => m.Message));

				CombineAssertions("Cannot send CRESA Message from invalid DCN", () =>
				{
					AssertEquals(false, res);
					AssertContains("Unable to send CRESA message when an existing PEN or PAN Governing Reference exists on the consignment", errorMessage);
					AssertContains("We cannot send CRESA message from Dispatch Consignment DC1", errorMessage);
				});
			}
		}

		#endregion

		protected override IDocDataObjectWithoutUIMessageSender MessageSender => messageSender ?? (messageSender = new CresaWithoutUIMessageSender());
		IDocDataObjectWithoutUIMessageSender messageSender;

		protected override BusinessObject CreateBusinessObject() => CresaTestHelper.CreateDispatchConsignment(Factory).PopulateRequiredOrgCodes();

		protected override ZString DataStoreName => ShipmentDocumentDataStoreNames.GoodsReceivedCRESA;

		public override bool DocumentHasMenuItem => false;

		protected override ZString MSNReference => "|DEP=Terminal|MST=Goods Received (CRESA)";

		protected override string SenderObjectFactoryRegistrationKey => ShipmentDocumentDataStoreNames.GoodsReceivedCRESA;

		protected override ZString ExpectedMessage => @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CRESA/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>DC1</Key>
        <Type>TransitDispatch</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Goods Received (CRESA)</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Australia"">AU</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2021-01-01T00:00:00.000+00:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>
    <BookingConfirmationReference>DC1</BookingConfirmationReference>
    <GoodsDescription>PKG1 - Description, PKG2 - Description</GoodsDescription>
    <PortFirstForeign Name=""Amsterdam"">NLAMS</PortFirstForeign>
    <PortOfDestination Name=""Amsterdam"">NLAMS</PortOfDestination>
    <PortOfOrigin Name=""Sydney"">AUSYD</PortOfOrigin>
    <ShipmentType Description=""Standard"">STD</ShipmentType>
    <TotalNoOfPacks>2</TotalNoOfPacks>
    <TotalNoOfPacksPackageType Description=""Pallet"">PLT</TotalNoOfPacksPackageType>
    <TotalVolume>2</TotalVolume>
    <TotalVolumeUnit Description=""Cubic Meters"">M3</TotalVolumeUnit>
    <TotalWeight>4</TotalWeight>
    <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>AUSYD</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Sydney</Value>
      </AddInfo>
      <AddInfo>
        <Key>FormVersion</Key>
        <Value>1.0.0</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortLocation</Key>
        <Value>ZZZ</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortArea</Key>
        <Value>001</Value>
      </AddInfo>
      <AddInfo>
        <Key>TransportMode</Key>
        <Value>RTE</Value>
      </AddInfo>
      <AddInfo>
        <Key>PortServiceReference</Key>
        <Value>002</Value>
      </AddInfo>
      <AddInfo>
        <Key>EntryNumber</Key>
        <Value>DC1</Value>
      </AddInfo>
      <AddInfo>
        <Key>CommodityReference</Key>
        <Value>DC1</Value>
      </AddInfo>
      <AddInfo>
        <Key>GoodsSealed</Key>
        <Value>N</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>DC1</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <Value>2021-01-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>Received</Type>
        <Value>2021-01-01T00:00:00</Value>
      </Date>
      <Date>
        <Type>CargoReceiptDate</Type>
        <Value>2021-01-01T00:00:00</Value>
      </Date>
    </DateCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 13</Address1>
        <Address2>4 Lost Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Aalborg</City>
        <CompanyName>CNR Org</CompanyName>
        <Contact></Contact>
        <Country Name=""Denmark"">DK</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Aalborg"">DKAAL</Port>
        <Postcode>2000</Postcode>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>52 Florence</Address1>
        <Address2>St Clair CT</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Paris</City>
        <CompanyName>CNE Org</CompanyName>
        <Contact></Contact>
        <Country Name=""France"">FR</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Persan"">FRPRS</Port>
        <Postcode>12121</Postcode>
        <State></State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>PickupAgent</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>ABC Forwarder</Address1>
        <Address2>Unit 399</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Sydney</City>
        <CompanyName>BKP Org</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>2050</Postcode>
        <State>NSW</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ci5</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>ABC TW Org</Address1>
        <Address2>Unit 888</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Carlingford</City>
        <CompanyName>TW Org</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>3002</Postcode>
        <State>VIC</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""S)One Port Community System Warehou"">SOW</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>sow</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>ABC Forwarder</Address1>
        <Address2>Unit 399</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Sydney</City>
        <CompanyName>BKP Org</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Sydney"">AUSYD</Port>
        <Postcode>2050</Postcode>
        <State>NSW</State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Ci5 Port Community System Code"">CI5</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>ci5</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection>
      <PackingLine>
        <ContainerNumber></ContainerNumber>
        <DetailedDescription>PKG1 - Description</DetailedDescription>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription>PKG1 - Description</GoodsDescription>
        <Height>0</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <Length>0</Length>
        <LengthUnit Description=""Centimeters"">CM</LengthUnit>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>PKG1</PackingLineID>
        <PackQty>1</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber></ReferenceNumber>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>1</Volume>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <Weight>2</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <Width>0</Width>
      </PackingLine>
      <PackingLine>
        <ContainerNumber></ContainerNumber>
        <DetailedDescription>PKG2 - Description</DetailedDescription>
        <ExportReferenceNumber></ExportReferenceNumber>
        <GoodsDescription>PKG2 - Description</GoodsDescription>
        <Height>0</Height>
        <ImportReferenceNumber></ImportReferenceNumber>
        <Length>0</Length>
        <LengthUnit Description=""Centimeters"">CM</LengthUnit>
        <MarksAndNos></MarksAndNos>
        <PackingLineID>PKG2</PackingLineID>
        <PackQty>1</PackQty>
        <PackType Description=""Pallet"">PLT</PackType>
        <ReferenceNumber></ReferenceNumber>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <Volume>1</Volume>
        <VolumeUnit Description=""Cubic Meters"">M3</VolumeUnit>
        <Weight>2</Weight>
        <WeightUnit Description=""Kilograms"">KG</WeightUnit>
        <Width>0</Width>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>";

		ZString SucceedNote => @"User: CargoWise Support
Time: 25-Nov-24 08:12:25 +00:00
Message Status: CRESA Message has been sent and is waiting for response.
CRESA Message Details:
Operational Port    PCS            Transport Mode    Transhipment Port    Port of Arrival    Port Area      Port Service Reference    Port Location    Cargo Receipt Date    ETA at Port of Arrival
AUSYD               -              RTE               NLAMS                NLAMS              001            002                       ZZZ              25-Nov-24 00:00:00    25-Nov-24 08:12:25
Organization Details:
Buyer          Supplier       Sending Party    Forwarder      Agent
CNE Org        CNR Org        TW Org           BKP Org        BKP Org
Additional References:
Booking Reference    Warehouse Entry Number    ECV Reference    CRESA Reference
DC1                  DC1                       -                -
Goods Details:
Packs          Weight         Volume         Goods Description
1              2              1              PKG1 - Description
1              2              1              PKG2 - Description
";

		ZString FailedNote => @"User: CargoWise Support
Time: 25-Nov-24 08:12:25 +00:00
Failed reason: Message Error - Value: Sending Party Provider ID is missing from this organization > Config > Registration Numbers/Codes - type SOW.
Message Error - Value: Forwarder Provider ID is missing from this organization > Config > Registration Numbers/Codes - type SON.
Message Error - Value: Forwarder Provider ID is missing from this organization > Config > Registration Numbers/Codes - type CI5.
Message Error - Value: Agent Provider ID is missing from this organization > Config > Registration Numbers/Codes - type SOA.
Message Error - Value: Agent Provider ID is missing from this organization > Config > Registration Numbers/Codes - type CI5.
Message Error - PortServiceCodeReference: Port Service Reference is missing from DCN > Booking Party > Organization > Config > Registration Numbers/Codes - type PSR.
Message Error - PortArea: Port Area is missing from organization DCN > Booking Party > Organization > Config > Registration Numbers/Codes - type PSN (code before '\')
Message Error - PortLocation: Port Location is missing from organization DCN > Booking Party > Organization > Config > Registration Numbers/Codes - type PSN (code after '\')
CRESA Message Details:
Operational Port    PCS            Transport Mode    Transhipment Port    Port of Arrival    Port Area      Port Service Reference    Port Location    Cargo Receipt Date    ETA at Port of Arrival
AUSYD               -              RTE               NLAMS                NLAMS              -              -                         -                25-Nov-24 00:00:00    25-Nov-24 08:12:25
Organization Details:
Buyer          Supplier       Sending Party    Forwarder      Agent
CNE Org        CNR Org        TW Org           BKP Org        BKP Org
Additional References:
Booking Reference    Warehouse Entry Number    ECV Reference    CRESA Reference
DC1                  DC1                       -                -
Goods Details:
Packs          Weight         Volume         Goods Description
1              2              1              PKG1 - Description
1              2              1              PKG2 - Description
";
	}
}
