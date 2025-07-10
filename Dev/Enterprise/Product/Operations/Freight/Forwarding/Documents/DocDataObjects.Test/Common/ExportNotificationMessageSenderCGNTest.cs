using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Common;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing.Common
{
	sealed class ExportNotificationMessageSenderCGNTest : DocDataObjectMessageSenderTest
	{
		protected override IDocDataObjectMessageSender MessageSender => messageSender ?? (messageSender = new ExportNotificationMessageSender());
		IDocDataObjectMessageSender messageSender;

		protected override ZGuid MenuItemPK => ConsolSystemFormMenuItems.DocumentMenuExportNotificationCargonautNLPK;

		protected override ZString DocumentName => ConsolDocumentDataStoreNames.CGNExportNotification;

		protected override ZString MSNReference => "|DEP=Terminal|MST=Export Notification (755)";

		protected override bool AllowSendMessageAmendment => true;

		#region SetBusinessObject

		protected override BusinessObject SetBusinessObject()
		{
			return CreateConsole("C00001000");
		}

		ForwardingConsol CreateConsole(string consolRef)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = consolRef;
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "NLAMS";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_MasterBillNum = "123-12345678";

			CreateAddresses(consol);
			CreateShipments(consol);
			return consol;
		}

		void CreateShipments(ForwardingConsol consol)
		{
			var number1 = Factory.New<CusEntryNumber>();
			number1.CE_RN_NKCountryCode = CountryCodes.Netherlands;
			number1.CE_EntryType = "MRN";
			number1.CE_EntryNum = "MRN01";

			var shipment = consol.Shipments.AddNew();
			shipment.Numbers.Add(number1);
			shipment.JS_UniqueConsignRef = consol.JK_UniqueConsignRef;
			shipment.JS_PackingMode = ContainerModes.ULD;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "NLAMS";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_HouseBill = "H001";
			shipment.JS_ActualWeight = 20;
			shipment.JS_OuterPacks = 1;
			shipment.JS_F3_NKPackType = "PKG";
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_ConsolReference = "WhiskyTreasure";
			shipment.JS_GoodsDescription = "goods description";
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_UnitOfVolume = "D3";
		}

		void CreateAddresses(ForwardingConsol consol)
		{
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.MainAddress.OA_RN_NKCountryCode = "NL";
			branchProxy.OH_FullName = "TestCompany";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			var cgnCode1 = GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.AddNew();
			cgnCode1.OK_CodeType = OrgCusCode.NetherlandsCodeTypes.CargonautRegistationCode;
			cgnCode1.OK_RN_NKCodeCountry = CountryCodes.Netherlands;
			cgnCode1.OK_CustomsRegNo = "C001";

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.OH_IsAirLine = true;
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "NL";

			var firstTransportLeg = consol.Transports.Cast<Freight.Business.Transport>().First();
			firstTransportLeg.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			var cgnCode2 = carrier.CustomsCodes.AddNew();
			cgnCode2.OK_CodeType = OrgCusCode.NetherlandsCodeTypes.CargonautRegistationCode;
			cgnCode2.OK_RN_NKCodeCountry = CountryCodes.Netherlands;
			cgnCode2.OK_CustomsRegNo = "C002";

			Factory.Save();
		}

		#endregion

		#region ExpectedMessage

		protected override ZString ExpectedMessage => @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CGN755/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Export Notification (755)</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>ORG</Purpose>
        <SubmissionVersion>1</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Netherlands"">NL</Country>
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


    <AddInfoCollection>
      <AddInfo>
        <Key>MAWB</Key>
        <Value>123-12345678</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>NLAMS</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Amsterdam</Value>
      </AddInfo>
    </AddInfoCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City></City>
        <CompanyName>TestCompany</CompanyName>
        <Contact></Contact>
        <Country Name=""Netherlands"">NL</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port></Port>
        <Postcode></Postcode>
        <State></State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Cargonaut Registration Code"">CGN</Type>
            <CountryOfIssue Name=""Netherlands"">NL</CountryOfIssue>
            <Value>C001</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 13</Address1>
        <Address2>4 Lost Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Aalborg</City>
        <CompanyName>MAERSK</CompanyName>
        <Contact></Contact>
        <Country Name=""Netherlands"">NL</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Aalborg"">DKAAL</Port>
        <Postcode>2000</Postcode>
        <State></State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Cargonaut Registration Code"">CGN</Type>
            <CountryOfIssue Name=""Netherlands"">NL</CountryOfIssue>
            <Value>C002</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>C00001000</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <TotalNoOfPacks>1</TotalNoOfPacks>
        <TotalNoOfPacksPackageType Description=""Package"">PKG</TotalNoOfPacksPackageType>
        <TotalWeight>20</TotalWeight>
        <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
        <WayBillNumber>H001</WayBillNumber>
        <WayBillType>HBL</WayBillType>
        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type Description=""Movement Reference Number"">MRN</Type>
            <ReferenceNumber>MRN01</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		#region ExpectedAmendmentMessage

		protected override ZString ExpectedAmendmentMessage => @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CGN755/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00001000</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>Export Notification (755)</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>AMD</Purpose>
        <SubmissionVersion>2</SubmissionVersion>
      </DocumentaryOverride>
      <Workflow>
        <ActionPurpose Description=""As Per Payload"">APP</ActionPurpose>
        <Company>
          <Code>EDI</Code>
          <Country Name=""Netherlands"">NL</Country>
          <Name>Eagle Datamation International</Name>
        </Company>
        <EventBranch Name=""BN - AUBNE"">BNE</EventBranch>
        <EventDepartment Name=""Branch"">BRN</EventDepartment>
        <EventType></EventType>
        <EventUser Name=""CargoWise Support"">E</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2021-01-02T00:00:00.000+00:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>


    <AddInfoCollection>
      <AddInfo>
        <Key>MAWB</Key>
        <Value>123-12345678</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>NLAMS</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Amsterdam</Value>
      </AddInfo>
    </AddInfoCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>#1</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City></City>
        <CompanyName>TestCompany</CompanyName>
        <Contact></Contact>
        <Country Name=""Netherlands"">NL</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port></Port>
        <Postcode></Postcode>
        <State></State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Cargonaut Registration Code"">CGN</Type>
            <CountryOfIssue Name=""Netherlands"">NL</CountryOfIssue>
            <Value>C001</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 13</Address1>
        <Address2>4 Lost Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Aalborg</City>
        <CompanyName>MAERSK</CompanyName>
        <Contact></Contact>
        <Country Name=""Netherlands"">NL</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Aalborg"">DKAAL</Port>
        <Postcode>2000</Postcode>
        <State></State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""Cargonaut Registration Code"">CGN</Type>
            <CountryOfIssue Name=""Netherlands"">NL</CountryOfIssue>
            <Value>C002</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>C00001000</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>

        <TotalNoOfPacks>1</TotalNoOfPacks>
        <TotalNoOfPacksPackageType Description=""Package"">PKG</TotalNoOfPacksPackageType>
        <TotalWeight>20</TotalWeight>
        <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
        <WayBillNumber>H001</WayBillNumber>
        <WayBillType>HBL</WayBillType>
        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type Description=""Movement Reference Number"">MRN</Type>
            <ReferenceNumber>MRN01</ReferenceNumber>
          </AdditionalReference>
        </AdditionalReferenceCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		#region Validation Error

		public void TestSendMessage_ValidationError()
		{
			AssertSendMessage_ValidationError("C00001000", "The registry item 'Freight > Port Messaging > Netherlands > Allow to send Export Notification (755) to Cargonaut' is disabled, so could not send Export Notification (755).", allowToSend: false);
			AssertSendMessage_ValidationError("C00001001", "Consol C00001001 transport mode SEA is not Air, so could not send Export Notification (755).", consolTransportMode: TransportModes.Sea);
			AssertSendMessage_ValidationError("C00001002", "Consol C00001002 first load DEHAM is not in Netherlands, so could not send Export Notification (755).", loadPort: "DEHAM");
			void AssertSendMessage_ValidationError
			(
				string consolUniqueConsignRef,
				string expectedMessage,
				bool allowToSend = true,
				string consolTransportMode = TransportModes.Air,
				string loadPort = "NLAMS"
			)
			{
				using (Factory.AddDisposableService())
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Netherlands))
				using (PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, allowToSend))
				{
					var consol = CreateConsole(consolUniqueConsignRef);
					consol.JK_TransportMode = consolTransportMode;
					consol.JK_RL_NKLoadPort = loadPort;
					consol.JK_MasterBillNum = "123";

					var notifications = new NotificationsHandler();
					var result = MessageSender.SendMessage(consol, MenuItem, notifications);

					Assert("message has not been sent", !result);
					AssertEquals("there is 1 error message", 1, notifications.Notifications.Count);
					AssertEquals(expectedMessage, notifications.Notifications.GetFirstMessage());
				}
			}
		}

		#endregion

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();

			oldAllowToSend = PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.Value;
			PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			oldCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Netherlands);
		}

		protected override void TearDown()
		{
			PortMessagingRegistry.Instance.AllowToSendExportNotificationToCargonaut.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldAllowToSend);
			GlbCompany.CurrentCompany.SetCountry(oldCountryCode);

			base.TearDown();
		}

		ZString oldCountryCode;
		bool oldAllowToSend;

		#endregion
	}
}
