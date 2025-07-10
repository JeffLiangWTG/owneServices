using System;
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
	sealed class ExportNotificationMessageSenderCINTest : DocDataObjectMessageSenderTest
	{
		protected override IDocDataObjectMessageSender MessageSender => messageSender ?? (messageSender = new ExportNotificationMessageSender());
		IDocDataObjectMessageSender messageSender;

		protected override ZGuid MenuItemPK => ShipmentSystemFormMenuItems.CINExportNotificationPK;

		protected override ZString DocumentName => ShipmentDocumentDataStoreNames.CINExportNotification;

		protected override ZString MSNReference => "|DEP=Terminal|MST=Export Notification (755)";

		protected override bool AllowSendMessageAmendment => true;

		#region SetBusinessObject

		protected override BusinessObject SetBusinessObject()
		{
			var number1 = Factory.New<CusEntryNumber>();
			number1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			number1.CE_EntryType = "MRN";
			number1.CE_EntryNum = "MRN01";

			var number2 = Factory.New<CusEntryNumber>();
			number2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			number2.CE_EntryType = "COC";
			number2.CE_EntryNum = "N01";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "FRPAR";
			shipment.JS_RL_NKDestination = "FRSXB";
			shipment.JS_HouseBill = "HWB";
			shipment.JS_OuterPacks = 1;
			shipment.JS_UniqueConsignRef = "S00006000";
			shipment.JS_ActualWeight = 20m;
			shipment.JS_UnitOfWeight = "KG";
			shipment.Numbers.Add(number1);
			shipment.Numbers.Add(number2);

			var cfs = Factory.New<OrgHeader>();
			cfs.OH_FullName = "CONSPA";
			cfs.OH_RL_NKClosestPort = "FRPAR";
			cfs.MainAddress.Address1 = "Unit 15";
			cfs.MainAddress.Address2 = "5 Lost Lane";
			cfs.MainAddress.City = "Marseille";
			cfs.MainAddress.Postcode = "2000";
			cfs.MainAddress.OA_RN_NKCountryCode = "FR";
			cfs.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";

			var orgCusCode = cfs.CustomsCodes.AddNew();
			orgCusCode.OK_OH = cfs.PK;
			orgCusCode.OK_CodeType = "CTR";
			orgCusCode.SecuredCustomsRegNo = "N01";
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;

			var orgCINCode = cfs.CustomsCodes.AddNew();
			orgCINCode.OK_OH = cfs.PK;
			orgCINCode.OK_CodeType = "CIN";
			orgCINCode.SecuredCustomsRegNo = "112233";
			orgCINCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			consol.JK_MasterBillNum = "11122222222";
			consol.JK_RL_NKLoadPort = "FRPAR";
			consol.JK_RL_NKDischargePort = "FRSXB";
			consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Core.Constants.TransportModes.Air;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.OH_IsAirLine = true;
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			transport.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			var carrierCINCode = carrier.CustomsCodes.AddNew();
			carrierCINCode.OK_OH = carrier.PK;
			carrierCINCode.OK_CodeType = "CIN";
			carrierCINCode.SecuredCustomsRegNo = "445566";
			carrierCINCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;

			var proxyMainAddress = GlbBranch.CurrentBranch.OrgProxy?.MainAddress ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress;
			var sendingParty = proxyMainAddress?.Header;
			if (sendingParty != null)
			{
				var sendingForwarderCINCode = sendingParty.CustomsCodes.AddNew();
				sendingForwarderCINCode.OK_OH = sendingParty.PK;
				sendingForwarderCINCode.OK_CodeType = "CIN";
				sendingForwarderCINCode.SecuredCustomsRegNo = "778899";
				sendingForwarderCINCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			}

			return shipment;
		}

		#endregion

		#region ExpectedMessage

		protected override ZString ExpectedMessage => @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CIN755/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>S00006000</Key>
        <Type>ForwardingShipment</Type>
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
          <Country Name=""France"">FR</Country>
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

    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType Description=""Pallet"">PLT</TotalNoOfPacksPackageType>
    <TotalWeight>20</TotalWeight>
    <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
    <WayBillNumber>HWB</WayBillNumber>
    <WayBillType Description=""House Waybill"">HWB</WayBillType>
    <AddInfoCollection>
      <AddInfo>
        <Key>MAWB</Key>
        <Value>11122222222</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>FRPAR</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Paris</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Movement Reference Number"">MRN</Type>
        <ReferenceNumber>MRN01</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Customs Office Code"">COC</Type>
        <ReferenceNumber>N01</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>10 HUTCHESON STREET</Address1>
        <Address2>ALBION  QLD</Address2>
        <AddressOverride>false</AddressOverride>
        <City></City>
        <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Brisbane"">AUBNE</Port>
        <Postcode>4010</Postcode>
        <State></State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIN Registration Code"">CIN</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>778899</Value>
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
            <Type Description=""CIN Registration Code"">CIN</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>445566</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCFSAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 15</Address1>
        <Address2>5 Lost Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Marseille</City>
        <CompanyName>CONSPA</CompanyName>
        <Contact></Contact>
        <Country Name=""France"">FR</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Paris"">FRPAR</Port>
        <Postcode>2000</Postcode>
        <State></State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIN Registration Code"">CIN</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>112233</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		#region ExpectedAmendmentMessage

		protected override ZString ExpectedAmendmentMessage => @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CIN755/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>S00006000</Key>
        <Type>ForwardingShipment</Type>
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
          <Country Name=""France"">FR</Country>
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

    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType Description=""Pallet"">PLT</TotalNoOfPacksPackageType>
    <TotalWeight>20</TotalWeight>
    <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
    <WayBillNumber>HWB</WayBillNumber>
    <WayBillType Description=""House Waybill"">HWB</WayBillType>
    <AddInfoCollection>
      <AddInfo>
        <Key>MAWB</Key>
        <Value>11122222222</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>FRPAR</Value>
      </AddInfo>
      <AddInfo>
        <Key>OperationalPort_Name</Key>
        <Value>Paris</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Movement Reference Number"">MRN</Type>
        <ReferenceNumber>MRN01</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type Description=""Customs Office Code"">COC</Type>
        <ReferenceNumber>N01</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>CurrentUser</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>10 HUTCHESON STREET</Address1>
        <Address2>ALBION  QLD</Address2>
        <AddressOverride>false</AddressOverride>
        <City></City>
        <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Brisbane"">AUBNE</Port>
        <Postcode>4010</Postcode>
        <State></State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIN Registration Code"">CIN</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>778899</Value>
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
            <Type Description=""CIN Registration Code"">CIN</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>445566</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>DepartureCFSAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit 15</Address1>
        <Address2>5 Lost Lane</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Marseille</City>
        <CompanyName>CONSPA</CompanyName>
        <Contact></Contact>
        <Country Name=""France"">FR</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Paris"">FRPAR</Port>
        <Postcode>2000</Postcode>
        <State></State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CIN Registration Code"">CIN</Type>
            <CountryOfIssue Name=""France"">FR</CountryOfIssue>
            <Value>112233</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

		#endregion

		#region Validation Error

		public void TestSendMessage_ValidationError()
		{
			AssertSendMessage_ValidationError("S00005000", $"The registry item 'Freight > Port Messaging > France > Allow to send Export Notification (755) to Cargo Information Network' is disabled, so could not send Export Notification (755).", allowToSend: false);
			AssertSendMessage_ValidationError("S00005001", $"Shipment S00005001 transport mode SEA is not Air, so could not send Export Notification (755).", shipmentTransportMode: TransportModes.Sea);
			AssertSendMessage_ValidationError("S00005002", $"Shipment S00005002 origin DEHAM is not in France, so could not send Export Notification (755).", origin: "DEHAM");
			AssertSendMessage_ValidationError("S00005003", $"Shipment S00005003 has no registered MRN reference number, so could not send Export Notification (755).", entryType: "BAG");
			AssertSendMessage_ValidationError("S00005004", $"The FR – COC Reference Number registered at the Shipment S00005004 is different with the value of the FR – CTR Registration number of the departure CFS defined at the consol, so could not send Export Notification (755).", cocEntryNum: "N02");
			AssertSendMessage_ValidationError("S00005005", $"No consol with Air transport mode and master bill and 1st Load in France was attached in Shipment S00005005, so could not send Export Notification (755).", consolTransportMode: TransportModes.Sea);
			AssertSendMessage_ValidationError("S00005006", $"No consol with Air transport mode and master bill and 1st Load in France was attached in Shipment S00005006, so could not send Export Notification (755).", consolMasterBillNum: string.Empty);

			void AssertSendMessage_ValidationError
			(
				string shipmentUniqueConsignRef,
				string expectedMessage,
				bool allowToSend = true,
				string shipmentTransportMode = TransportModes.Air,
				string origin = "FR222",
				string entryType = "MRN",
				string cocEntryNum = "N01",
				string consolTransportMode = TransportModes.Air,
				string consolMasterBillNum = "08133334442"
			)
			{
				using (Factory.AddDisposableService())
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
				using (PortMessagingRegistry.Instance.AllowToSendExportNotification.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, allowToSend))
				{
					var number1 = Factory.New<CusEntryNumber>();
					number1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
					number1.CE_EntryType = entryType;
					number1.CE_EntryNum = "MRN01";

					var number2 = Factory.New<CusEntryNumber>();
					number2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
					number2.CE_EntryType = "COC";
					number2.CE_EntryNum = cocEntryNum;

					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_UniqueConsignRef = shipmentUniqueConsignRef;
					shipment.JS_TransportMode = shipmentTransportMode;
					shipment.JS_RL_NKOrigin = origin;
					shipment.JS_RL_NKDestination = "USLAX";
					shipment.Numbers.AddRange(number1, number2);

					var cfs = Factory.New<OrgHeader>();
					cfs.OH_FullName = "CONSPA";
					cfs.OH_RL_NKClosestPort = "FRPAR";
					cfs.MainAddress.Address1 = "Unit 15";
					cfs.MainAddress.Address2 = "5 Lost Lane";
					cfs.MainAddress.City = "Marseille";
					cfs.MainAddress.Postcode = "2000";
					cfs.MainAddress.OA_RN_NKCountryCode = "FR";
					cfs.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";

					var orgCusCode = cfs.CustomsCodes.AddNew();
					orgCusCode.OK_OH = cfs.PK;
					orgCusCode.OK_CodeType = "CTR";
					orgCusCode.SecuredCustomsRegNo = "N01";
					orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;

					var consol = shipment.Consols.AddNew();
					consol.JK_TransportMode = consolTransportMode;
					consol.JK_RL_NKLoadPort = origin;
					consol.JK_RL_NKDischargePort = "USLAX";
					consol.JK_MasterBillNum = consolMasterBillNum;
					consol.JK_OA_PackDepotAddress = cfs.MainAddress.PK;

					var notifications = new NotificationsHandler();
					var result = MessageSender.SendMessage(shipment, MenuItem, notifications);

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

			oldAllowToSend = PortMessagingRegistry.Instance.AllowToSendExportNotification.Value;
			PortMessagingRegistry.Instance.AllowToSendExportNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			oldCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(CountryCodes.France);
		}

		protected override void TearDown()
		{
			PortMessagingRegistry.Instance.AllowToSendExportNotification.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldAllowToSend);
			GlbCompany.CurrentCompany.SetCountry(oldCountryCode);

			base.TearDown();
		}

		ZString oldCountryCode;
		bool oldAllowToSend;

		#endregion
	}
}
