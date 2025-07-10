using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class CommonCartageXmlExportToEmailDirectorTest : BaseFreightTest
	{
		public void TestMessageShownWhenSourceHasChanges()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			InternalCartageManager manager = new InternalCartageManager(cartageType);
			dummyParent.HasChanges = true;
			Assert("Precondition", dummyParent.HasChanges);
			NotificationBuffer buffer = new NotificationBuffer();
			CommonCartageXmlExportToEmailDirector director = new CommonCartageXmlExportToEmailDirector(new CommonCartageBookingValueObjectDataAdapter());
			director.RunExport(manager, buffer, CancellationToken.None);
			Assert(buffer.HasErrors);
			Assert(buffer.Events.ContainsNotificationContaining("Please save your changes before you continue."));
		}

		public void TestExportWhenNoCartageCo()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			InternalCartageManager manager = new InternalCartageManager(cartageType);
			ICartageExporter cartageExporter = manager;
			dummyParent.HasChanges = false;
			Assert("Precondition", !dummyParent.HasChanges);
			cartageType.RemoveCartageOrganisation();
			AssertNull("Precondition", cartageType.LocalTransportProviderAddress);
			NotificationBuffer buffer = new NotificationBuffer();
			CommonCartageXmlExportToEmailDirector director = new CommonCartageXmlExportToEmailDirector(new CommonCartageBookingValueObjectDataAdapter());
			director.RunExport(cartageExporter, buffer, CancellationToken.None);
			Assert(buffer.HasErrors);
			Assert(buffer.Events.ContainsNotificationContaining("Please enter a valid " + cartageExporter.Description + " Local Transport Company"));
		}

		public void TestExportWithNoMode()
		{
			if (GlbCompany.CurrentCompany.OrgProxy == null && GlbBranch.CurrentBranch.OrgProxy == null)
			{
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			}

			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = (DummyCartageType)dummyCartageParent.CartageTypes.First();
			InternalCartageManager manager = new InternalCartageManager(cartageType);
			ICartageExporter cartageExporter = manager;
			dummyParent.HasChanges = false;
			CommonCartage cartage = Factory.NewWithValidTestData<CommonCartage>();
			cartage.JJ_ConsignmentID = TestConsignmentID;
			NotificationBuffer buffer = new NotificationBuffer();
			CommonCartageXmlExportToEmailDirector director = new CommonCartageXmlExportToEmailDirector(new CommonCartageBookingValueObjectDataAdapter());
			director.RunExport(cartageExporter, buffer, CancellationToken.None);
			Assert(buffer.HasErrors);
			Assert(buffer.Events.ContainsNotificationContaining("does not have a valid Communication Mode for TRN - Port Transport"));
		}

		[TestDate(2011, 4, 17, 2, 2, 2)]
		public void TestShipmentExportThruEHub()
		{
			using (Factory.AddDisposableService())
			{
				var cartage = Factory.New<CommonCartage>();
				var shipment = (CommonShipment)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
				shipment.JS_UniqueConsignRef = "S1";
				var transportAddress = Factory.NewWithValidTestData<OrgAddress>();
				transportAddress.Header.OH_IsLocalTransport = true;
				transportAddress.Header.OH_Code = "TRANSCOSYD";
				var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
				var cfsAddress = Factory.NewWithValidTestData<OrgAddress>();
				var mode = transportAddress.Header.EDICommunicationsModes.AddNew();
				mode.EK_Module = JobInvoicingConsumerTypes.LocalCartage.Code;
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				mode.EK_Destination = "9CHARCODE";
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = transportAddress.PK;
				shipment.DocsAndCartage.JP_FCLDeliveryEquipmentNeeded = "PSL";
				shipment.ConsigneeDeliveryAddress.E2_OA_Address = consigneeAddress.PK;
				shipment.JS_OA_ImportReleaseDepot = cfsAddress.PK;
				Factory.Save();
				var cartageParent = (ICartageParent)shipment;
				var cartageType = cartageParent.CartageTypes.ElementAt(1); // delivery
				var manager = new InternalCartageManager(cartageType);
				var cartageExporter = (ICartageExporter)manager;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var buffer = new NotificationBuffer();
				var director = new CommonCartageXmlExportToEmailDirectorForTest(new CommonCartageBookingValueObjectDataAdapter());
				director.BookingStatus = FreightConstants.LocalCartageBookingStatus.Codes.FirmBookingRequest;
				director.BookingComment = "hello";
				director.RunExport(cartageExporter, buffer, CancellationToken.None);
				AssertEquals("Buffer should have no errors", false, buffer.HasErrors);
				Assert("Buffer should contain notification Port Transport Job successfully exported to XML", buffer.Events.ContainsNotificationContaining("Port Transport Job successfully exported to XML."));
				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, shipment.PK));
				AssertEquals("EDIMessages linked to Shipment", 1, messages.Length);
				// Exported XML no longer contains JobNumber as now newly created JobCartage record has blank JJ_ConsignmentID before saving
				var message = messages[0];
				AssertXMLEquals("message.EM_MessageText", @$"
<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <InterchangeInfo>
    <Date>2011-04-17T02:02:02+10:00</Date>
    <XmlType>LightWeight</XmlType>
    <Source>
      <EnterpriseCode>EDI</EnterpriseCode>
      <CompanyCode>EDI</CompanyCode>
      <OriginServer>DAT</OriginServer>
      <LoginName>CWSupport</LoginName>
    </Source>
    <Target>
      <Type>LocalCartageBooking</Type>
    </Target>
    <EDIOrganisation EDICode=""EDICUS"" OwnerCode=""EDICUS"">
      <OrganisationDetails>
        <Name>EDI CUSTOMS BROKERS</Name>
        <Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>
        <Addresses>
          <Address AddressType=""MAIN"">
            <AddressLine1>10 HUTCHESON STREET</AddressLine1>
            <AddressLine2>ALBION  QLD</AddressLine2>
            <AddressCode>PST: 10 HUTCHESON STREET</AddressCode>
            <PostCode>4010</PostCode>
            <Language>EN</Language>
            <Location>AUBNE</Location>
            <Sequence>1</Sequence>
            <AddressCapabilities>
              <AddressCapability AddressType=""MAIN"" />
              <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
            </AddressCapabilities>
          </Address>
        </Addresses>
      </OrganisationDetails>
    </EDIOrganisation>
  </InterchangeInfo>
  <Payload>
    <CartageJobs>
      <CartageJob>
        <Type>CJHR</Type>
        <JobType>IALC</JobType>
        <DropMode>PSL</DropMode>
        <Action>CAR</Action>
        <ActionType>FBR</ActionType>
        <MessageDescription>hello</MessageDescription>
        <MessageResponseAddress>Default@edi.com.au</MessageResponseAddress>
        <MessageSystemType>{Core.Constants.ProductName}</MessageSystemType>
        <ClientJobReference>S1</ClientJobReference>
        <BillTo EDICode=""EDICUS"" OwnerCode=""EDICUS"">
          <OrganisationDetails>
            <Name>EDI CUSTOMS BROKERS</Name>
            <Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>
            <Addresses>
              <Address AddressType=""MAIN"">
                <AddressLine1>10 HUTCHESON STREET</AddressLine1>
                <AddressLine2>ALBION  QLD</AddressLine2>
                <AddressCode>PST: 10 HUTCHESON STREET</AddressCode>
                <PostCode>4010</PostCode>
                <Language>EN</Language>
                <Location>AUBNE</Location>
                <Sequence>1</Sequence>
                <AddressCapabilities>
                  <AddressCapability AddressType=""MAIN"" />
                  <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
                </AddressCapabilities>
              </Address>
            </Addresses>
          </OrganisationDetails>
        </BillTo>
        <CartageOrg EDICode=""TRANSCOSYD"" OwnerCode=""TRANSCOSYD"">
          <OrganisationDetails>
            <Name>Header</Name>
            <Addresses>
              <Address AddressType=""MAIN"">
                <AddressLine1>#2</AddressLine1>
                <AddressCode>#2</AddressCode>
                <Language>EN</Language>
                <Sequence>1</Sequence>
                <AddressCapabilities>
                  <AddressCapability AddressType=""MAIN"" />
                  <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
                </AddressCapabilities>
              </Address>
            </Addresses>
          </OrganisationDetails>
        </CartageOrg>
        <OuterPacks>
          <OuterPacksWeight>0</OuterPacksWeight>
          <OuterPacksVolume>0</OuterPacksVolume>
        </OuterPacks>
        <SailingInfo />
        <CustomAttributes />
      </CartageJob>
    </CartageJobs>
  </Payload>
</XmlInterchange>
".Trim(), message.EM_MessageText);
				var statusLog = shipment.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
				AssertNotNull("STU event", statusLog);
				AssertEquals("STU event", "FBR-Firm Booking Request (PortTransport) sent to TRANSCOSYD - Header.", statusLog.SL_Reference);
				var dataExportLog = shipment.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.DataExport.Code && x.SL_Reference == string.Empty);
				AssertNotNull("DEX event", dataExportLog);
				var notes = shipment.Notes.FindByDescription(FreightConstants.LocalCartageNote);
				AssertEquals("there should be a note", 1, notes.Length);
				AssertEquals("Note contents", @"@17-Apr-11 02:02 - FBR-Firm Booking Request (PortTransport) sent to TRANSCOSYD - Header.
Comments: hello", notes[0].ST_NoteDataAsText);
			}
		}

		class CommonCartageXmlExportToEmailDirectorForTest : CommonCartageXmlExportToEmailDirector
		{
			public CommonCartageXmlExportToEmailDirectorForTest(CommonCartageValueObjectDataAdapter adapter) : base(adapter)
			{
			}

			public string BookingStatus { get; set; }

			public string BookingComment { get; set; }

			protected override bool QueryAdditionalBookingInformation(ICartageExporter cartageExporter, CommonCartage cartage, NotificationBuffer buffer)
			{
				cartage.BookingInformation.BookingStatus = BookingStatus;
				cartage.BookingInformation.BookingComment = BookingComment;
				return true;
			}
		}

		[TestDate(2011, 4, 17, 2, 2, 2)]
		public void TestTransportStatusExportThruEHub()
		{
			using (Factory.AddDisposableService())
			{
				var cartage = Factory.New<CommonCartage>();
				cartage.JJ_ConsignmentID = "T0123456";
				cartage.JJ_OrderReferenceNumber = "S01020304";
				cartage.JJ_E3_NKJobType = Core.Constants.CartageJobType.NEW_AirImport;
				Factory.Save();
				var client = Factory.NewWithValidTestData<OrgHeader>();
				client.OH_Code = "FORWARDSYD";
				client.OH_FullName = "Forwarder Co";
				var job = new JobHeader.Loader(cartage).TryLoadOrCreateWithoutMutexForTestOnly();
				cartage.LocalClientAddressPK = client.MainAddress.PK;
				var mode = client.EDICommunicationsModes.AddNew();
				mode.EK_Module = JobInvoicingConsumerTypes.LocalCartage.Code;
				mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				mode.EK_Destination = "9CHARCODE";
				Factory.Save();
				var manager = new StandaloneCartageManager(cartage);
				var cartageExporter = (ICartageExporter)manager;
				cartage.BookingInformation.BookingStatus = FreightConstants.LocalCartageBookingStatus.Codes.BookingAccepted;
				cartage.BookingInformation.BookingComment = "hello";
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var buffer = new NotificationBuffer();
				var director = new CommonCartageXmlExportToEmailDirector(new CommonCartageStatusValueObjectDataAdapter());
				director.RunExport(cartageExporter, buffer, CancellationToken.None);
				Assert(!buffer.HasErrors);
				Assert(buffer.Events.ContainsNotificationContaining("Port Transport Job successfully exported to XML."));
				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, cartage.PK));
				AssertEquals("EDIMessages linked to Cartage", 1, messages.Length);
				var message = messages[0];
				AssertXMLEquals("message.EM_MessageText", @$"
<XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/"">
  <InterchangeInfo>
    <Date>2011-04-17T02:02:02+10:00</Date>
    <XmlType>LightWeight</XmlType>
    <Source>
      <EnterpriseCode>EDI</EnterpriseCode>
      <CompanyCode>EDI</CompanyCode>
      <OriginServer>DAT</OriginServer>
      <LoginName>CWSupport</LoginName>
    </Source>
    <Target>
      <Type>LocalCartageStatus</Type>
    </Target>
    <EDIOrganisation EDICode=""EDICUS"" OwnerCode=""EDICUS"">
      <OrganisationDetails>
        <Name>EDI CUSTOMS BROKERS</Name>
        <Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>
        <Addresses>
          <Address AddressType=""MAIN"">
            <AddressLine1>10 HUTCHESON STREET</AddressLine1>
            <AddressLine2>ALBION  QLD</AddressLine2>
            <AddressCode>PST: 10 HUTCHESON STREET</AddressCode>
            <PostCode>4010</PostCode>
            <Language>EN</Language>
            <Location>AUBNE</Location>
            <Sequence>1</Sequence>
            <AddressCapabilities>
              <AddressCapability AddressType=""MAIN"" />
              <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
            </AddressCapabilities>
          </Address>
        </Addresses>
      </OrganisationDetails>
    </EDIOrganisation>
  </InterchangeInfo>
  <Payload>
    <CartageJobs>
      <CartageJob>
        <Type>CJHR</Type>
        <JobType>IALC</JobType>
        <JobNumber>T0123456</JobNumber>
        <Action>STA</Action>
        <ActionType>ACC</ActionType>
        <MessageDescription>hello</MessageDescription>
        <MessageResponseAddress>Default@edi.com.au</MessageResponseAddress>
        <MessageSystemType>{Core.Constants.ProductName}</MessageSystemType>
        <ClientJobReference>S01020304</ClientJobReference>
        <BillTo EDICode=""EDICUS"" OwnerCode=""EDICUS"">
          <OrganisationDetails>
            <Name>EDI CUSTOMS BROKERS</Name>
            <Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>
            <Addresses>
              <Address AddressType=""MAIN"">
                <AddressLine1>10 HUTCHESON STREET</AddressLine1>
                <AddressLine2>ALBION  QLD</AddressLine2>
                <AddressCode>PST: 10 HUTCHESON STREET</AddressCode>
                <PostCode>4010</PostCode>
                <Language>EN</Language>
                <Location>AUBNE</Location>
                <Sequence>1</Sequence>
                <AddressCapabilities>
                  <AddressCapability AddressType=""MAIN"" />
                  <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
                </AddressCapabilities>
              </Address>
            </Addresses>
          </OrganisationDetails>
        </BillTo>
        <OuterPacks>
          <OuterPacksWeight>0</OuterPacksWeight>
          <OuterPacksVolume>0</OuterPacksVolume>
        </OuterPacks>
        <SailingInfo />
        <CustomAttributes />
      </CartageJob>
    </CartageJobs>
  </Payload>
</XmlInterchange>
".Trim(), message.EM_MessageText);
				var statusLog = cartage.Logs.MostRecentLogByEventTime(Events.StatusUpdated);
				AssertNotNull("STU event", statusLog);
				AssertEquals("STU event", "ACC-Booking Request Accepted (PortTransport) sent to FORWARDSYD - Forwarder Co.", statusLog.SL_Reference); //ACC-Booking Request Accepted (Port Transport) sent
				var dataExportLog = cartage.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.SL_SE_NKEvent == Events.DataExport.Code && x.SL_Reference == string.Empty);
				AssertNotNull("DEX event", dataExportLog);
				var notes = cartage.Notes.FindByDescription(FreightConstants.LocalCartageNote);
				AssertEquals("there should be a note", 1, notes.Length);
				AssertEquals("Note contents", @"@17-Apr-11 02:02 - ACC-Booking Request Accepted (PortTransport) sent to FORWARDSYD - Forwarder Co.
Comments: hello", notes[0].ST_NoteDataAsText);
			}
		}

		const string TestConsignmentID = "ABC0123";
	}
}
