using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.BR;
using NUnit.Framework;
using ConsolDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.DocDataObjects.ConsolDocumentDataStoreNames;

namespace Enterprise.Freight.Forwarding.Documents.DataObjects.BR.Testing
{
	sealed class CargoControlAndTransitHouseManifestMessageSenderTest : DocDataObjectMessageSenderTest
	{
		protected override IDocDataObjectMessageSender MessageSender => messageSender ?? (messageSender = new CargoControlAndTransitHouseManifestMessageSender());
		IDocDataObjectMessageSender messageSender;

		protected override BusinessObject SetBusinessObject() => CreateConsolWithoutErrors("215-98757411", "C00000001", "AUSYD", "BRSAO");

		protected override ZGuid MenuItemPK => ConsolSystemFormMenuItems.CCTHouseManifestPK;

		protected override ZString DocumentName => ConsolDocumentDataStoreNames.AdvancedManifestBR;

		protected override ZString MSNReference => "|DEP=Customs|LOC=BR|MST=Advanced Manifest";

		protected override ZString ExpectedMessage => @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CCTHouseCheckList/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>C00000001</Key>
        <Type>ForwardingConsol</Type>
      </DataSource>
      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Advanced Manifest</DocumentName>
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
        <EventUser Name=""BR Testing User"">BR1</EventUser>
        <TriggerCount>1</TriggerCount>
        <TriggerDate>2021-01-01T00:00:00.000+00:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>
    <PortOfDestination Name=""Sao Paulo"">SAO</PortOfDestination>
    <PortOfFirstArrival Name=""Sao Paulo"">BRSAO</PortOfFirstArrival>
    <PortOfOrigin Name=""Santiago"">SCL</PortOfOrigin>
    <TotalNoOfPacks>2</TotalNoOfPacks>
    <TotalWeight>20</TotalWeight>
    <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
    <WayBillNumber>215-98757411</WayBillNumber>
    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>BRSAO</Value>
      </AddInfo>
    </AddInfoCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type Description=""Freight Forwarder Reference"">FFW</Type>
        <ReferenceNumber>C00000001</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>SendingForwarderAddress</AddressType>
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
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ReceivingForwarderAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>ImportAgentAddress</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>Sydney</City>
        <CompanyName>ImportAgent</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>REG0001</GovRegNum>
        <GovRegNumType Description=""CNPJ Cadastro Nacional da Pessoa Ju"">CJN</GovRegNumType>
        <Phone></Phone>
        <Port></Port>
        <Postcode>2021</Postcode>
        <State></State>
        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type Description=""CNPJ Cadastro Nacional da Pessoa Ju"">CJN</Type>
            <CountryOfIssue Name=""Brazil"">BR</CountryOfIssue>
            <Value>REG0001</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>S00001001</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>
        <GoodsDescription>notes</GoodsDescription>
        <OuterPacks>2</OuterPacks>
        <PortOfDestination Name=""Sao Paulo"">SAO</PortOfDestination>
        <PortOfOrigin Name=""Sydney"">SYD</PortOfOrigin>
        <TotalWeight>20</TotalWeight>
        <TotalWeightUnit Description=""Kilograms"">KG</TotalWeightUnit>
        <WayBillNumber>215-98757411</WayBillNumber>
        <DateCollection>
          <Date>
            <Type>Arrival</Type>
            <Value>2021-01-01T01:00:00</Value>
          </Date>
        </DateCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

		[TestDate(2021, 1, 1)]
		public override void TestSendMessage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BR1";
			staff.GS_FullName = "BR Testing User";
			var password = Factory.New<IGlbExternalPassword_CCT>();
			password.GP_GS = staff.PK;
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				base.TestSendMessage();
			}
		}

		[TestDate(2024, 1, 1)]
		public void TestMessageCannotBeSent_ShipmentHasInvalidIssueDate()
		{
			using (Factory.AddDisposableService())
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "BR1";
				staff.GS_FullName = "BR Testing User";
				var password = Factory.New<IGlbExternalPassword_CCT>();
				password.GP_GS = staff.PK;
				password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

				using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					var bizObj = (ForwardingConsol)SetBusinessObject();
					bizObj.Shipments[0].AWBHeader.EH_AWBIssueDate = new ZDateTime(2024, 1, 2);

					var notifications = new NotificationsHandler();

					var res = MessageSender.SendMessage(bizObj, MenuItem, notifications);

					Assert("message has not been sent", !res);
					AssertEquals("there is 1 error message", 1, notifications.Notifications.Count);
					AssertEquals(@"HAWB has been detected with a 'future' Issue Date for Shipment(s) S00001001, which will lead to an erroneous association at CCT. As per CCT Requirements, the CCT House Manifest must be sent when each linked HAWB has an 'actual' Issue Date.
If the above Shipment(s) has been declared with a 'future' Issue Date by mistake, please withdraw the previous CCT Shipment Report and resend after correction.", notifications.Notifications.GetFirstMessage());
				}
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestMessageCannotBeSent_ConsolTypeIsDRT()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "BR1";
			staff.GS_FullName = "BR Testing User";

			var password = Factory.New<IGlbExternalPassword_CCT>();
			password.GP_GS = staff.PK;
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var bizObj = (ForwardingConsol)SetBusinessObject();
				bizObj.JK_AgentType = Core.Constants.AgentType.Direct;

				var notifications = new NotificationsHandler();

				var res = MessageSender.SendMessage(bizObj, MenuItem, notifications);

				Assert("message has not been sent", !res);
				AssertEquals("there is 1 error message", 1, notifications.Notifications.Count);
				AssertEquals(@"Advanced Manifest is not available from Consolidations with type DRT.", notifications.Notifications.GetFirstMessage());
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestMessageCannotBeSent_NoCertificate_NoPassword()
		{
			using (Factory.AddDisposableService())
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "BR1";
				staff.GS_FullName = "BR Testing User";

				using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					AssertNull("pre-condition: current user does not have CCT password setup.", GlbStaff.CurrentUser?.GetBRWrapper().CCTPassword);

					var bizObj = SetBusinessObject();
					var notifications = new NotificationsHandler();

					var res = MessageSender.SendMessage(bizObj, MenuItem, notifications);

					Assert("message has not been sent", !res);
					AssertEquals("there is 1 error message", 1, notifications.Notifications.Count);
					AssertEquals("To send messages to CCT you must have a valid certificate loaded against your staff profile.", notifications.Notifications.GetFirstMessage());
				}
			}
		}

		[TestDate(2021, 1, 1)]
		public void TestMessageCannotBeSent_NoCertificate_InvalidPassword()
		{
			using (Factory.AddDisposableService())
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "BR1";
				staff.GS_FullName = "BR Testing User";
				var password = Factory.New<IGlbExternalPassword_CCT>();
				password.GP_GS = staff.PK;
				password.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;

				using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					AssertNotEquals("pre-condition: current user does not have valid CCT password.", PasswordStatusList.Codes.Valid, GlbStaff.CurrentUser?.GetBRWrapper().CCTPassword.GP_PasswordStatus);

					var bizObj = SetBusinessObject();
					var notifications = new NotificationsHandler();

					var res = MessageSender.SendMessage(bizObj, MenuItem, notifications);

					Assert("message has not been sent", !res);
					AssertEquals("there is 1 error message", 1, notifications.Notifications.Count);
					AssertEquals("To send messages to CCT you must have a valid certificate loaded against your staff profile.", notifications.Notifications.GetFirstMessage());
				}
			}
		}
	}
}
