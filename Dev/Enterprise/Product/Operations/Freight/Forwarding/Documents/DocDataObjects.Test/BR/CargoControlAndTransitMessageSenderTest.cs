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

namespace Enterprise.Freight.Forwarding.Documents.DataObjects.BR.Testing
{
	sealed class CargoControlAndTransitMessageSenderTest : DocDataObjectMessageSenderTest
	{
		protected override IDocDataObjectMessageSender MessageSender => messageSender ?? (messageSender = new CargoControlAndTransitMessageSender());
		IDocDataObjectMessageSender messageSender;

		protected override BusinessObject SetBusinessObject() => CreateShipmentWithoutErrors("081001", "S54625711", "AUSYD", "BRSAO");

		protected override ZGuid MenuItemPK => ShipmentSystemFormMenuItems.DocumentMenuCCTShipmentReport;

		protected override ZString DocumentName => ShipmentDocumentDataStoreNames.AdvancedCargoReportBR;

		protected override ZString MSNReference => "|DEP=Customs|LOC=BR|MST=Advanced Cargo Report";

		protected override bool AllowSendMessageAmendment => true;

		protected override ZString ExpectedMessage => @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CCTShipmentReport/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>S54625711</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>1</DataVersion>
        <DocumentName>Advanced Cargo Report</DocumentName>
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
    <GoodsDescription>notes VOL 122.000 M3</GoodsDescription>
    <GoodsValue>0</GoodsValue>
    <GoodsValueCurrency></GoodsValueCurrency>
    <InsuranceValue>0</InsuranceValue>
    <InsuranceValueCurrency Description=""Australian Dollar"">AUD</InsuranceValueCurrency>
    <PortOfDestination Name=""Sao Paulo"">SAO</PortOfDestination>
    <PortOfFirstArrival Name=""Sao Paulo"">BRSAO</PortOfFirstArrival>
    <PortOfOrigin Name=""Sydney"">SYD</PortOfOrigin>
    <TotalNoOfPacks>5</TotalNoOfPacks>
    <TotalWeight>21</TotalWeight>
    <TotalWeightUnit Description=""Kilogram"">KG</TotalWeightUnit>
    <WayBillNumber>081001</WayBillNumber>
    <WayBillType Description=""House Waybill"">HWB</WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>BRSAO</Value>
      </AddInfo>
      <AddInfo>
        <Key>To1stCode</Key>
        <Value>SAO</Value>
      </AddInfo>
      <AddInfo>
        <Key>To1stDescription</Key>
        <Value>Sao Paulo</Value>
      </AddInfo>
      <AddInfo>
        <Key>MAWB</Key>
        <Value>081-001</Value>
      </AddInfo>
      <AddInfo>
        <Key>ReferenceNumber</Key>
        <Value>C00001004</Value>
      </AddInfo>
      <AddInfo>
        <Key>OptionalShippingInformation</Key>
        <Value>TERMS: FOB</Value>
      </AddInfo>
      <AddInfo>
        <Key>ChargesCode</Key>
        <Value>CP</Value>
      </AddInfo>
      <AddInfo>
        <Key>ChargesDescription</Key>
        <Value>Destination Collect Cash</Value>
      </AddInfo>
      <AddInfo>
        <Key>WeightPrepaidCollectCode</Key>
        <Value>CLT</Value>
      </AddInfo>
      <AddInfo>
        <Key>WeightPrepaidCollectDescription</Key>
        <Value>Collect</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherPrepaidCollectCode</Key>
        <Value>PPD</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherPrepaidCollectDescription</Key>
        <Value>Prepaid</Value>
      </AddInfo>
      <AddInfo>
        <Key>CurrencyCode</Key>
        <Value>AUD</Value>
      </AddInfo>
      <AddInfo>
        <Key>CurrencyDescription</Key>
        <Value>Australian Dollar</Value>
      </AddInfo>
      <AddInfo>
        <Key>CustomsValueAmount</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>CustomsValueCurrencyCode</Key>
        <Value>AUD</Value>
      </AddInfo>
      <AddInfo>
        <Key>CustomsValueCurrencyDescription</Key>
        <Value>Australian Dollar</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalWeightCOL</Key>
        <Value>46.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>ValuationCOL</Key>
        <Value>2.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>TaxesCOL</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherChargesDueAgentCOL</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherChargesDueCarrierCOL</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalCOL</Key>
        <Value>48.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>ShippersSignature</Key>
        <Value>BR Testing User</Value>
      </AddInfo>
      <AddInfo>
        <Key>IssueDate</Key>
        <Value>2021-01-01T00:00:00</Value>
      </AddInfo>
      <AddInfo>
        <Key>IssuePlace</Key>
        <Value>Brisbane</Value>
      </AddInfo>
      <AddInfo>
        <Key>AgentsSignature</Key>
        <Value>AGENT SIGNATURE</Value>
      </AddInfo>
      <AddInfo>
        <Key>WoodenParts</Key>
        <Value>N</Value>
      </AddInfo>
    </AddInfoCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit52</Address1>
        <Address2>Dorcus yamadai</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Sydney</City>
        <CompanyName>Consignor</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>REG1111</GovRegNum>
        <GovRegNumType Description=""CNPJ Cadastro Nacional da Pessoa Ju"">CJN</GovRegNumType>
        <Phone></Phone>
        <Postcode>2017</Postcode>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>801</Address1>
        <Address2>Prismognathus delislei</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Somewhere</City>
        <CompanyName>Consignee</CompanyName>
        <Contact></Contact>
        <Country Name=""United States"">US</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>REG2222</GovRegNum>
        <GovRegNumType Description=""CNPJ Cadastro Nacional da Pessoa Ju"">CJN</GovRegNumType>
        <Phone></Phone>
        <Postcode>10043</Postcode>
        <State>CA</State>
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
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>UNIT 3, 480 NUDGEE ROAD</Address1>
        <Address2>HENDRA ROAD, QLD 4011</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Brisbane</City>
        <CompanyName>Eagle Datamation International</CompanyName>
        <Contact>BR Testing User</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Brisbane"">AUBNE</Port>
        <Postcode></Postcode>
        <State></State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection>
      <PackingLine>
        <GoodsDescription>notes</GoodsDescription>
        <Link>1</Link>
        <PackQty>2</PackQty>
        <Weight>20</Weight>
        <WeightUnit Description=""Kilogram"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>RateClass</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>CommodityItemNumber</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>RateChargeOrDiscount</Key>
            <Value>0</Value>
          </AddInfo>
          <AddInfo>
            <Key>Total</Key>
            <Value>0</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightValue</Key>
            <Value>20.000</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitCode</Key>
            <Value>KG</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitDescription</Key>
            <Value>Kilograms</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
      <PackingLine>
        <GoodsDescription>VOL 122.000 M3</GoodsDescription>
        <Link>13</Link>
        <PackQty>3</PackQty>
        <Weight>1</Weight>
        <WeightUnit Description=""Kilogram"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>RateClass</Key>
            <Value>Q</Value>
          </AddInfo>
          <AddInfo>
            <Key>CommodityItemNumber</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>RateChargeOrDiscount</Key>
            <Value>0.02</Value>
          </AddInfo>
          <AddInfo>
            <Key>Total</Key>
            <Value>5.12</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightValue</Key>
            <Value>256.000</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitCode</Key>
            <Value>KG</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitDescription</Key>
            <Value>Kilograms</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>";

		protected override ZString ExpectedAmendmentMessage => @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11/CCTShipmentReport/1"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataSource>
        <DataProvider Type=""EnterpriseID"">EDIDATEDI</DataProvider>
        <Key>S54625711</Key>
        <Type>ForwardingShipment</Type>
      </DataSource>

      <DocumentaryOverride>
        <DataVersion>2</DataVersion>
        <DocumentName>Advanced Cargo Report</DocumentName>
        <IsSystemDefined>true</IsSystemDefined>
        <Purpose>AMD</Purpose>
        <SubmissionVersion>2</SubmissionVersion>
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
        <TriggerDate>2021-01-02T00:00:00.000+00:00</TriggerDate>
        <TriggerDescription></TriggerDescription>
        <TriggerType>Manual</TriggerType>
      </Workflow>
    </DataContext>
    <GoodsDescription>notes VOL 122.000 M3</GoodsDescription>
    <GoodsValue>0</GoodsValue>
    <GoodsValueCurrency></GoodsValueCurrency>
    <InsuranceValue>0</InsuranceValue>
    <InsuranceValueCurrency Description=""Australian Dollar"">AUD</InsuranceValueCurrency>
    <PortOfDestination Name=""Sao Paulo"">SAO</PortOfDestination>
    <PortOfFirstArrival Name=""Sao Paulo"">BRSAO</PortOfFirstArrival>
    <PortOfOrigin Name=""Sydney"">SYD</PortOfOrigin>
    <TotalNoOfPacks>5</TotalNoOfPacks>
    <TotalWeight>21</TotalWeight>
    <TotalWeightUnit Description=""Kilogram"">KG</TotalWeightUnit>
    <WayBillNumber>081001</WayBillNumber>
    <WayBillType Description=""House Waybill"">HWB</WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>OperationalPort_Code</Key>
        <Value>BRSAO</Value>
      </AddInfo>
      <AddInfo>
        <Key>To1stCode</Key>
        <Value>SAO</Value>
      </AddInfo>
      <AddInfo>
        <Key>To1stDescription</Key>
        <Value>Sao Paulo</Value>
      </AddInfo>
      <AddInfo>
        <Key>MAWB</Key>
        <Value>081-001</Value>
      </AddInfo>
      <AddInfo>
        <Key>ReferenceNumber</Key>
        <Value>C00001004</Value>
      </AddInfo>
      <AddInfo>
        <Key>OptionalShippingInformation</Key>
        <Value>TERMS: FOB</Value>
      </AddInfo>
      <AddInfo>
        <Key>ChargesCode</Key>
        <Value>CP</Value>
      </AddInfo>
      <AddInfo>
        <Key>ChargesDescription</Key>
        <Value>Destination Collect Cash</Value>
      </AddInfo>
      <AddInfo>
        <Key>WeightPrepaidCollectCode</Key>
        <Value>CLT</Value>
      </AddInfo>
      <AddInfo>
        <Key>WeightPrepaidCollectDescription</Key>
        <Value>Collect</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherPrepaidCollectCode</Key>
        <Value>PPD</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherPrepaidCollectDescription</Key>
        <Value>Prepaid</Value>
      </AddInfo>
      <AddInfo>
        <Key>CurrencyCode</Key>
        <Value>AUD</Value>
      </AddInfo>
      <AddInfo>
        <Key>CurrencyDescription</Key>
        <Value>Australian Dollar</Value>
      </AddInfo>
      <AddInfo>
        <Key>CustomsValueAmount</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>CustomsValueCurrencyCode</Key>
        <Value>AUD</Value>
      </AddInfo>
      <AddInfo>
        <Key>CustomsValueCurrencyDescription</Key>
        <Value>Australian Dollar</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalWeightCOL</Key>
        <Value>46.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>ValuationCOL</Key>
        <Value>2.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>TaxesCOL</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherChargesDueAgentCOL</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>OtherChargesDueCarrierCOL</Key>
        <Value>0.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>TotalCOL</Key>
        <Value>48.000</Value>
      </AddInfo>
      <AddInfo>
        <Key>ShippersSignature</Key>
        <Value>BR Testing User</Value>
      </AddInfo>
      <AddInfo>
        <Key>IssueDate</Key>
        <Value>2021-01-02T00:00:00</Value>
      </AddInfo>
      <AddInfo>
        <Key>IssuePlace</Key>
        <Value>Brisbane</Value>
      </AddInfo>
      <AddInfo>
        <Key>AgentsSignature</Key>
        <Value>AGENT SIGNATURE</Value>
      </AddInfo>
      <AddInfo>
        <Key>WoodenParts</Key>
        <Value>N</Value>
      </AddInfo>
    </AddInfoCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{0}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{1}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{2}</MessageNumber>
    </MessageNumberCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Unit52</Address1>
        <Address2>Dorcus yamadai</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Sydney</City>
        <CompanyName>Consignor</CompanyName>
        <Contact></Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>REG1111</GovRegNum>
        <GovRegNumType Description=""CNPJ Cadastro Nacional da Pessoa Ju"">CJN</GovRegNumType>
        <Phone></Phone>
        <Postcode>2017</Postcode>
        <State>NSW</State>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>801</Address1>
        <Address2>Prismognathus delislei</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Somewhere</City>
        <CompanyName>Consignee</CompanyName>
        <Contact></Contact>
        <Country Name=""United States"">US</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>REG2222</GovRegNum>
        <GovRegNumType Description=""CNPJ Cadastro Nacional da Pessoa Ju"">CJN</GovRegNumType>
        <Phone></Phone>
        <Postcode>10043</Postcode>
        <State>CA</State>
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
      <OrganizationAddress>
        <AddressType>BookingPartyDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>UNIT 3, 480 NUDGEE ROAD</Address1>
        <Address2>HENDRA ROAD, QLD 4011</Address2>
        <AddressOverride>false</AddressOverride>
        <City>Brisbane</City>
        <CompanyName>Eagle Datamation International</CompanyName>
        <Contact>BR Testing User</Contact>
        <Country Name=""Australia"">AU</Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <Phone></Phone>
        <Port Name=""Brisbane"">AUBNE</Port>
        <Postcode></Postcode>
        <State></State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <PackingLineCollection>
      <PackingLine>
        <GoodsDescription>notes</GoodsDescription>
        <Link>1</Link>
        <PackQty>2</PackQty>
        <Weight>20</Weight>
        <WeightUnit Description=""Kilogram"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>RateClass</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>CommodityItemNumber</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>RateChargeOrDiscount</Key>
            <Value>0</Value>
          </AddInfo>
          <AddInfo>
            <Key>Total</Key>
            <Value>0</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightValue</Key>
            <Value>20.000</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitCode</Key>
            <Value>KG</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitDescription</Key>
            <Value>Kilograms</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
      <PackingLine>
        <GoodsDescription>VOL 122.000 M3</GoodsDescription>
        <Link>13</Link>
        <PackQty>3</PackQty>
        <Weight>1</Weight>
        <WeightUnit Description=""Kilogram"">KG</WeightUnit>
        <AddInfoCollection>
          <AddInfo>
            <Key>RateClass</Key>
            <Value>Q</Value>
          </AddInfo>
          <AddInfo>
            <Key>CommodityItemNumber</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>RateChargeOrDiscount</Key>
            <Value>0.02</Value>
          </AddInfo>
          <AddInfo>
            <Key>Total</Key>
            <Value>5.12</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightValue</Key>
            <Value>256.000</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitCode</Key>
            <Value>KG</Value>
          </AddInfo>
          <AddInfo>
            <Key>ChargeableWeightUnitDescription</Key>
            <Value>Kilograms</Value>
          </AddInfo>
        </AddInfoCollection>
      </PackingLine>
    </PackingLineCollection>
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
