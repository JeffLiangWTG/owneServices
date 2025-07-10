using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests
{
	class eAdaptorNextControllerTest : BaseEAdaptorControllerTest
	{
		protected eAdaptorSenderHelper sender;
		protected IEDICommunicationPartyConfig config;
		protected IDisposable disposableMessagingContext;
		Mock<IMessagingContext> mockMessagingContext;

		protected override void SetUp()
		{
			var controller = new eAdaptorNextController();
			sender = new eAdaptorSenderHelper(controller, controller.Post);

			config = AuthenticationTestHelper.SetUpBasicAuthenticationUser(string.Empty, string.Empty);
			config = AuthenticationTestHelper.SetConfigBranchDepComp(config);
			mockMessagingContext = new Mock<IMessagingContext>();
			mockMessagingContext.SetupGet(c => c.CurrentInboundConfig).Returns(config);
			disposableMessagingContext = ObjectFactory.Substitute(mockMessagingContext.Object);

			base.SetUp();
		}

		protected override void TearDown()
		{
			disposableMessagingContext.Dispose();
			base.TearDown();
		}

		const string SampleUniversalEventXML = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>S0001000</Key>
				</DataTarget>
			</DataTargetCollection>
            <EventDepartment>
             <Code>TLC</Code>
            </EventDepartment>
            <CodesMappedToTarget>true</CodesMappedToTarget>
		</DataContext>
		<EventType>Z77</EventType>
		<EventTime>2014-08-18T10:36:33.557</EventTime>
        <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
        <IsEstimate>False</IsEstimate>
	</Event>
</UniversalEvent>";

		public void TestNext_UniversalEventSuccess_200()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001000";

			Factory.Save();

			var xml = SampleUniversalEventXML;

			var result = sender.SendRequest(xml);

			CombineAssertions(() =>
			{
				AssertEquals(System.Net.HttpStatusCode.OK, result.statusCode);

				AssertResponseContains(result.result, "ProcessingStatusCode", "PRS");
				AssertResponseContains(result.result, "ProcessingLog", "Linked Event to Shipment S0001000.");

				AssertMessageNumber(result.result, "00000000000000000001");

				AssertEDIMessageStatus(EDIMessageStatusList.Codes.ProcessedOK);
				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Sent, ReceiveTransmitList.Codes.Transmit);
			});
		}

		public void TestNext_UniversalEventWarning_200()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001000";
			Factory.Save();

			var xml =
				$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>S0001000</Key>
				</DataTarget>
			</DataTargetCollection>
            <EventDepartment>
             <Code>TLC</Code>
            </EventDepartment>
            <CodesMappedToTarget>true</CodesMappedToTarget>
		</DataContext>
		<a></a>
		<EventType>Z77</EventType>
		<EventTime>2014-08-18T10:36:33.557</EventTime>
        <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
        <IsEstimate>False</IsEstimate>
	</Event>
</UniversalEvent>";

			sender.Send = () => (sender.Controller as eAdaptorNextController).PostWithMessageType("UniversalEvent");
			var result = sender.SendRequest(xml);

			CombineAssertions(() =>
			{
				AssertEquals(System.Net.HttpStatusCode.OK, result.statusCode);

				AssertResponseContains(result.result, "ProcessingStatusCode", "WAR");
				AssertResponseContains(result.result, "ProcessingLog", "Linked Event to Shipment S0001000.");

				AssertMessageNumber(result.result, "00000000000000000001");

				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Warning);
				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Sent, ReceiveTransmitList.Codes.Transmit);
			});
		}

		public void TestNext_UniversalEventInvalidTarget_400()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001000";
			Factory.Save();

			var xml =
				$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>S9</Key>
				</DataTarget>
			</DataTargetCollection>
            <EventDepartment>
             <Code>TLC</Code>
            </EventDepartment>
            <CodesMappedToTarget>true</CodesMappedToTarget>
		</DataContext>
		<a></a>
		<EventType>Z77</EventType>
		<EventTime>2014-08-18T10:36:33.557</EventTime>
        <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
        <IsEstimate>False</IsEstimate>
	</Event>
</UniversalEvent>";

			var result = sender.SendRequest(xml);

			CombineAssertions(() =>
			{
				AssertEquals(System.Net.HttpStatusCode.BadRequest, result.statusCode);

				AssertContains("<Status>PRS</Status>", result.result);
				AssertResponseContains(result.result, "ProcessingStatusCode", "DCD");
				AssertContains("Warning - No Module found a Business Entity to link this Universal Event to.", result.result);

				AssertMessageNumber(result.result, "00000000000000000001");

				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Discarded);
				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Sent, ReceiveTransmitList.Codes.Transmit);
			});
		}

		public void TestNext_InvalidDeptartment_400()
		{
			DataRegistry.Instance.WebDepartment = ZGuid.NewZGuid().ToGuid();

			mockMessagingContext.SetupGet(c => c.CurrentInboundConfig).Returns((EDICommunicationPartyConfig)null);
			var xml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
 <Event>
 <DataContext>
  <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001005</Key>
        </DataTarget>
      </DataTargetCollection>
     </DataContext>
 <EventTime>2019-12-06T12:12:00</EventTime>
 <EventType>Z03</EventType>
 </Event>
</UniversalEvent>";

			var result = sender.SendRequest(xml);

			CombineAssertions(() =>
			{
				AssertEquals(System.Net.HttpStatusCode.BadRequest, result.statusCode);

				AssertContains("<Status>ERR</Status>", result.result);
				AssertContains("Invalid Department, you need to provide a valid department in the System Registry under Web &gt; Web Department", result.result);

				AssertEDIMessageCount(0);
			});
		}

		public void TestNext_SQLTimeout_RKN_503()
		{
			var xml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key/>
        </DataTarget>
      </DataTargetCollection>
      <DataProvider>ULVTSTWTF</DataProvider>
    </DataContext>
  </Shipment>
</UniversalShipment>";

			var saveCount = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				saveCount++;
				if (saveCount >= 3)
				{
					var winException = new Win32Exception(258, "The wait operation timed out");
					throw winException;
				}
			});

			var result = sender.SendRequest(xml);

			CombineAssertions(() =>
			{
				AssertEquals(System.Net.HttpStatusCode.ServiceUnavailable, result.statusCode);

				AssertContains("<Status>ERR</Status>", result.result);
				AssertContains("DB is currently not responsive: if self hosted, contact your administrators; if cloud, raise an incident", result.result);

				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Recognised);
			});
		}

		public void TestNext_SQLTimeout_ERR_503()
		{
			var xml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key/>
        </DataTarget>
      </DataTargetCollection>
      <DataProvider>ULVTSTWTF</DataProvider>
    </DataContext>
  </Shipment>
</UniversalShipment>";

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				var winException = new Win32Exception(258, "The wait operation timed out");
				throw winException;
			});

			var result = sender.SendRequest(xml);

			CombineAssertions(() =>
			{
				AssertEquals(System.Net.HttpStatusCode.ServiceUnavailable, result.statusCode);

				AssertContains("<Status>ERR</Status>", result.result);
				AssertContains("DB is currently not responsive: if self hosted, contact your administrators; if cloud, raise an incident", result.result);

				AssertEDIMessageCount(0);
			});
		}

		public void TestNext_InvalidXML_400()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001000";
			Factory.Save();

			var xml =
				$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>S0001000</Key>
				</DataTarget>
			</DataTargetCollection>
            
            <EventDepartment>
             <Code>TLC</Code>
            </EventDepartment>
            <CodesMappedToTarget>true</CodesMappedToTarget>
		</DataContext>
        <asd>
		<EventType>Z77</EventType>
		<EventTime>2014-08-18T10:36:33.557</EventTime>
        <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
        <IsEstimate>False</IsEstimate>
	</Event>
</UniversalEvent>";

			var result = sender.SendRequest(xml);

			CombineAssertions(() =>
			{
				AssertEquals(System.Net.HttpStatusCode.BadRequest, result.statusCode);

				AssertContains("<Status>PRS</Status>", result.result);
				AssertContains("Reached end of file without finding closing tag for element", result.result);

				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Rejected);
				AssertEDIMessageCount(1);
			});
		}

		public void TestNext_UniversalEventError_400()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001000";
			Factory.Save();

			var xml =
				$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>S0001000</Key>
				</DataTarget>
			</DataTargetCollection>
            
            <EventDepartment>
             <Code>TLC</Code>
            </EventDepartment>
            <CodesMappedToTarget>true</CodesMappedToTarget>
		</DataContext>
		<EventType>ZZ</EventType>
		<EventTime>2014-08-18T10:36:33.557</EventTime>
        <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
        <IsEstimate>False</IsEstimate>
	</Event>
</UniversalEvent>";

			var result = sender.SendRequest(xml);

			CombineAssertions(() =>
			{
				AssertEquals(System.Net.HttpStatusCode.BadRequest, result.statusCode);

				AssertContains("<Status>ERR</Status>", result.result);

				AssertResponseContains(result.result, "ProcessingStatusCode", "REJ");
				AssertResponseContains(result.result, "FailureReason", @$"Error - [ZZ] is not a valid {Core.Constants.ProductName} Event Code. Cannot import XML Event unless it has a valid code.");
				AssertProcessingLogs(result.result, @$"Error - [ZZ] is not a valid {Core.Constants.ProductName} Event Code. Cannot import XML Event unless it has a valid code.
Message Rejected.
Error - Delivery failed due to validation exception.");

				AssertMessageNumber(result.result, "00000000000000000001");

				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Rejected);
				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Sent, ReceiveTransmitList.Codes.Transmit);
			});
		}

		public void TestNext_UniversalTransactionValidationError_400()
		{
			var xml = @"<UniversalTransaction>
	<TransactionInfo>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccountingInvoice</Type>
					<Key>AR INV TI00034457</Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>EDI</Code>
				<Country>
					<Code>AU</Code>
				</Country>
			</Company>
			<RecipientRoleCollection>
				<RecipientRole>
					<Code>ORP</Code>
					<Description>Organisation Proxy</Description>
				</RecipientRole>
			</RecipientRoleCollection>
		</DataContext>
		<APAccountGroup>
			<Code>TPY</Code>
		</APAccountGroup>
		<ARAccountGroup>
			<Code>TPY</Code>
		</ARAccountGroup>
		<Branch>
			<Code>SIM</Code>
		</Branch>
		<BranchAddress>
			<AddressType>OFC</AddressType>
			<AddressOverride>false</AddressOverride>
			<OrganizationCode>SEAINT_AU</OrganizationCode>
		</BranchAddress>
		<Category>FIN</Category>
		<Department>
			<Code>BRN</Code>
		</Department>
		<Description>Murray's test description so its obvious</Description>
		<IsCancelled>false</IsCancelled>
		<Job>
			<Type>Job</Type>
		</Job>
		<JobInvoiceNumber/>
		<Ledger>AR</Ledger>
		<LocalCurrency>
			<Code>AUD</Code>
		</LocalCurrency>
		<LocalExVATAmount>126084.600</LocalExVATAmount>
		<LocalTotal>138693.060</LocalTotal>
		<LocalVATAmount>12608.460</LocalVATAmount>
		<Number>00034457</Number>
		<OrganizationAddress>
			<AddressType>OFC</AddressType>
			<AddressOverride>false</AddressOverride>
			<OrganizationCode>DEPORT_AU</OrganizationCode>
		</OrganizationAddress>
		<OSCurrency>
			<Code>AUD</Code>
		</OSCurrency>
		<OSExGSTVATAmount>126084.600</OSExGSTVATAmount>
		<OSGSTVATAmount>12608.460</OSGSTVATAmount>
		<OSTotal>138693.060</OSTotal>
		<PostDate>2021-08-31</PostDate>
		<TransactionDate>2021-08-31</TransactionDate>
		<DueDate>2021-09-30</DueDate>
		<TransactionType>INV</TransactionType>
		<PostingJournalCollection>
			<PostingJournal>
				<Branch>
					<Code>SYD</Code>
				</Branch>
				<Department>
					<Code>BRN</Code>
				</Department>
				<Description>Rail Rebate 20FT</Description>
				<GLAccount>
					<AccountCode>1080.10.10</AccountCode>
				</GLAccount>
				<GLPostDate>2021-08-31</GLPostDate>
				<Job>
					<Type>Job</Type>
				</Job>
				<LocalAmount>26552.70</LocalAmount>
				<LocalCurrency>
					<Code>AUD</Code>
				</LocalCurrency>
				<LocalGSTVATAmount>2655.27</LocalGSTVATAmount>
				<LocalTotalAmount>29207.97</LocalTotalAmount>
				<Organization>
					<Type>Organization</Type>
					<Key>DEPORT_AU</Key>
				</Organization>
				<OSCurrency>
					<Code>AUD</Code>
				</OSCurrency>
				<OSAmount>26552.70</OSAmount>
				<OSGSTVATAmount>2655.27</OSGSTVATAmount>
				<OSTotalAmount>2655.27</OSTotalAmount>
				<RevenueRecognitionType>IMM</RevenueRecognitionType>
				<TransactionCategory>FIN</TransactionCategory>
				<TransactionType>REV</TransactionType>
				<VATTaxID>
					<TaxCode>GST</TaxCode>
					<Description>Standard Rated (Non-Capital)</Description>
					<TaxRate>10</TaxRate>
					<TaxType>
						<Code>RAT</Code>
					</TaxType>
				</VATTaxID>
			</PostingJournal>
			<PostingJournal>
				<Branch>
					<Code>SYD</Code>
				</Branch>
				<Department>
					<Code>BRN</Code>
				</Department>
				<Description>Rail Rebate 40FT</Description>
				<GLAccount>
					<AccountCode>1080.10.10</AccountCode>
				</GLAccount>
				<GLPostDate>2021-08-31</GLPostDate>
				<Job>
					<Type>Job</Type>
				</Job>
				<LocalAmount>99531.90</LocalAmount>
				<LocalCurrency>
					<Code>AUD</Code>
				</LocalCurrency>
				<LocalGSTVATAmount>9953.19</LocalGSTVATAmount>
				<LocalTotalAmount>109485.09</LocalTotalAmount>
				<Organization>
					<Type>Organization</Type>
					<Key>DEPORT_AU</Key>
				</Organization>
				<OSCurrency>
					<Code>AUD</Code>
				</OSCurrency>
				<OSAmount>99531.90</OSAmount>
				<OSGSTVATAmount>9953.19</OSGSTVATAmount>
				<OSTotalAmount>9953.19</OSTotalAmount>
				<RevenueRecognitionType>IMM</RevenueRecognitionType>
				<TransactionCategory>FIN</TransactionCategory>
				<TransactionType>REV</TransactionType>
				<VATTaxID>
					<TaxCode>GST</TaxCode>
					<Description>Standard Rated (Non-Capital)</Description>
					<TaxRate>10</TaxRate>
					<TaxType>
						<Code>RAT</Code>
					</TaxType>
				</VATTaxID>
			</PostingJournal>
		</PostingJournalCollection>
	</TransactionInfo>
</UniversalTransaction>";

			CreateTestPeriod();
			CreateAUDBankAccount();
			Factory.Save();

			var result = sender.SendRequest(xml);

			CombineAssertions(() =>
			{
				AssertEquals(System.Net.HttpStatusCode.BadRequest, result.statusCode);

				AssertContains("<Status>ERR</Status>", result.result);
				AssertResponseContains(result.result, "FailureReason", @"Import failed because transaction has validation errors:
Error - Generic Charge: Enter a valid selection.
Error - Tax Date: No rate found for selected date.
Error - Generic Charge: Enter a valid selection.
Error - Tax Date: No rate found for selected date.
Error - Terms: Please enter a Terms.
Error - Address Override: Please enter an Account.
Error - Account: Please enter an Account.
Error - Invoice Post Date: This date does not fall into a valid accounting period’s date range.
Please go to Manage &gt; General Ledger &gt; Period Management &gt; Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.
");

				AssertMessageNumber(result.result, "00000000000000000001");

				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Rejected);
				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Sent, ReceiveTransmitList.Codes.Transmit);
			});
		}

		public void TestNext_WarehouseImportNoMatchingProduct_400()
		{
			var factory = new BusinessObjectFactory();

			var warehouse = (IWhsWarehouse)factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			warehouse.WW_WarehouseCode = "BRA";

			var company1 = factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "XYZ";

			var branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "BRA";
			branch1.GB_GC = company1.PK;

			var org = factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "WHIPOWHLZ";
			org.OH_IsWarehouseClient = true;

			factory.Save();

			var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" version=""1.1"">
   <Shipment>
      <DataContext>
         <DataTargetCollection>
            <DataTarget>
               <Type>WarehouseOrder</Type>
            </DataTarget>
         </DataTargetCollection>
         <Company>
            <Code>XYZ</Code>
         </Company>
         <EnterpriseID>EDI</EnterpriseID>
         <ServerID>DAT</ServerID>
      </DataContext>
      <Order>
         <OrderNumber>1300901</OrderNumber>
         <ClientReference>14073187</ClientReference>
         <Warehouse>
            <Code>BRA</Code>
         </Warehouse>
         <OrderLineCollection>
            <OrderLine>
               <Product>
                  <Code>12X26X12P3756PR</Code>
               </Product>
               <OrderedQty>2</OrderedQty>
               <LineComment />
               <LineNumber>1</LineNumber>
            </OrderLine>
         </OrderLineCollection>
         <TotalUnits>2</TotalUnits>
      </Order>
      <OrganizationAddressCollection>
         <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <OrganizationCode>WHIPOWHLZ</OrganizationCode>
         </OrganizationAddress>
      </OrganizationAddressCollection>
      <JobCosting>
         <Branch>
            <Code>BRA</Code>
         </Branch>
      </JobCosting>
   </Shipment>
</UniversalShipment>";

			var result = sender.SendRequest(xml);

			CombineAssertions(() =>
			{
				AssertEquals(System.Net.HttpStatusCode.BadRequest, result.statusCode);

				AssertResponseContains(result.result, "ProcessingStatusCode", "DCD");
				AssertContains("No changes were made due to the above errors. Please fix the errors and try again.", result.result);

				AssertMessageNumber(result.result, "00000000000000000001");

				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Discarded);
				AssertEDIMessageStatus(EDIMessageStatusList.Codes.Sent, ReceiveTransmitList.Codes.Transmit);
			});
		}

		public void TestCustomHeaders()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001000";

			Factory.Save();

			var xml = SampleUniversalEventXML;
			var partyConfigSecurityProxy = AuthenticationTestHelper.SetSeurityProxyUser(config);

			var result = sender.SendRequest(xml);

			var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			var responseMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));

			CombineAssertions(() =>
			{
				AssertResponseHeaderContains(result.headers, "eAdaptor-ReceiveEDIMessageNumber", requestMessage.EM_MessageNum);
				AssertResponseHeaderContains(result.headers, "eAdaptor-TransmitEDIMessageNumber", responseMessage.EM_MessageNum);
				AssertResponseHeaderContains(result.headers, "eAdaptor-MessageType", "XUE");
				AssertResponseHeaderContains(result.headers, "eAdaptor-EDIClientName", config.Party.ECP_Name);
			});
		}

		public void TestMessagesShouldBeCreatedBySecurityProxyUser()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001000";
			Factory.Save();

			var xml = SampleUniversalEventXML;
			var partyConfigSecurityProxy = AuthenticationTestHelper.SetSeurityProxyUser(config);

			var result = sender.SendRequest(xml);

			var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(partyConfigSecurityProxy.GS_Code, ((EDIMessage)requestMessage).EM_SystemCreateUser);
			AssertEquals(partyConfigSecurityProxy.GS_Code, ((EDIMessage)requestMessage).EM_SystemLastEditUser);

			var responseMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));
			AssertEquals(partyConfigSecurityProxy.GS_Code, ((EDIMessage)responseMessage).EM_SystemCreateUser);
			AssertEquals(partyConfigSecurityProxy.GS_Code, ((EDIMessage)responseMessage).EM_SystemLastEditUser);
		}

		public void TestNativeMessagesShouldBeCreatedBySecurityProxyUser()
		{
			var inboundXml =
$@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">AUSYD</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  <MessageNumberCollection>
      <MessageNumber Type=""External"">{ExternalReferenceNumber}</MessageNumber>
  </MessageNumberCollection>
  </Body>
</Native>";

			var partyConfigSecurityProxy = AuthenticationTestHelper.SetSeurityProxyUser(config);

			var result = sender.SendRequest(inboundXml);

			var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(partyConfigSecurityProxy.GS_Code, ((EDIMessage)requestMessage).EM_SystemCreateUser);
			AssertEquals(partyConfigSecurityProxy.GS_Code, ((EDIMessage)requestMessage).EM_SystemLastEditUser);

			var responseMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));
			AssertEquals(partyConfigSecurityProxy.GS_Code, ((EDIMessage)responseMessage).EM_SystemCreateUser);
			AssertEquals(partyConfigSecurityProxy.GS_Code, ((EDIMessage)responseMessage).EM_SystemLastEditUser);
		}

		public void TestCustomHeadersWithInvalidRequest()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001000";

			Factory.Save();

			var xml =
				$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<Event2>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>S0001000</Key>
				</DataTarget>
			</DataTargetCollection>
            <EventDepartment>
             <Code>TLC</Code>
            </EventDepartment>
            <CodesMappedToTarget>true</CodesMappedToTarget>
		</DataContext>
		<EventType>Z77</EventType>
		<EventTime>2014-08-18T10:36:33.557</EventTime>
        <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
        <IsEstimate>False</IsEstimate>
	</Event2>
</UniversalEvent>";

			var result = sender.SendRequest(xml);

			var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));

			CombineAssertions(() =>
			{
				AssertResponseHeaderContains(result.headers, "eAdaptor-ReceiveEDIMessageNumber", requestMessage.EM_MessageNum);
				AssertResponseHeaderContains(result.headers, "eAdaptor-MessageType", "XUE");
				AssertResponseHeaderContains(result.headers, "eAdaptor-EDIClientName", config.Party.ECP_Name);
			});
		}

		public void TestNext_eAdaptorNextRegistry()
		{
			var config = new DummyConfig()
			{
				Name = "eAdaptorTest",
				ResponseWriter = new eAdaptorNextHttpResponseWriter(),
				WebConfig = new WebAppEnvironment.Config(true, true),
				IsActive = false,
				ThrowIfNotActive = true,
				ThrowOnParsingError = false
			};

			var xml =
	$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>S0001000</Key>
				</DataTarget>
			</DataTargetCollection>
            
            <EventDepartment>
             <Code>TLC</Code>
            </EventDepartment>
            <CodesMappedToTarget>true</CodesMappedToTarget>
		</DataContext>
		<EventTime>2014-08-18T10:36:33.557</EventTime>
		<EventType>Z00</EventType>
        <EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
        <IsEstimate>False</IsEstimate>
	</Event>
</UniversalEvent>";

			var result = sender.SendRequest(xml, config);

			CombineAssertions(() =>
			{
				AssertEquals(System.Net.HttpStatusCode.BadRequest, result.statusCode);

				AssertContains("<Status>ERR</Status>", result.result);
				AssertContains(eAdaptorLogs.AdaptorDisabled(config.Name), result.result);
				AssertEDIMessageCount(0);
			});
		}

		public void TestSetupEnvironmentUsingCurrentInboundConfig()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";
			Factory.Save();

			var responseXml = sender.SendRequest(@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
<Event>
<DataContext>
<DataTargetCollection>
    <DataTarget>
        <Type>ForwardingShipment</Type>
        <Key>S00001001</Key>
    </DataTarget>
    </DataTargetCollection>
    </DataContext>
<EventTime>2019-12-06T12:12:00</EventTime>
<EventType>Z99</EventType>
</Event>
</UniversalEvent>
");

			var log = Factory.Load<IStmALog>(new ZQuery()).Where(log => log.SL_Parent == shipment.PK && log.SL_SE_NKEvent == "Z99").First();
			var messageRCV = Factory.Load<IEDIMessage>(new ZQuery()).Where(message => message.EM_ReceiveTransmit == "RCV").First();
			var messageTRX = Factory.Load<IEDIMessage>(new ZQuery()).Where(message => message.EM_ReceiveTransmit == "TRX").First();

			AssertEquals(Factory.Load<IGlbBranch>(config.ECC_GB_Branch).GB_Code, log.SL_GB_NKBranch);
			AssertEquals(Factory.Load<IGlbDepartment>(config.ECC_GE_Department).GE_Code, log.SL_GE_NKDepartment);
			AssertEquals(config.PK, messageRCV.EM_ECC_CommunicationPartyConfig);
			AssertEquals(config.PK, messageTRX.EM_ECC_CommunicationPartyConfig);
		}

		public void TestSetupEnvironmentWithInactiveDepartmentUsed()
		{
			var department = Factory.Load<GlbDepartment>(config.ECC_GE_Department);
			var initialValue = department.GE_IsActive;
			department.GE_IsActive = false;
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";
			Factory.Save();

			var responseXml = sender.SendRequest(@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	 <Event>
	 <DataContext>
	  <DataTargetCollection>
			<DataTarget>
			  <Type>ForwardingShipment</Type>
			  <Key>S00001001</Key>
			</DataTarget>
		  </DataTargetCollection>
		 </DataContext>
	 <EventTime>2019-12-06T12:12:00</EventTime>
	 <EventType>Z99</EventType>
	 </Event>
	</UniversalEvent>
	").result;
			AssertContains($"Department {Factory.Load<IGlbDepartment>(config.ECC_GE_Department).GE_Code} is not active, you need to provide an active department in EDI Client Details &gt; Department", responseXml);

			department.GE_IsActive = initialValue;
		}

		public void TestSetupEnvironmentWithInvalidDepartmentPKUsed()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";
			Factory.Save();

			var nullDepartmentConfig = Factory.Load<EDICommunicationPartyConfig>(config.PK);
			nullDepartmentConfig.ECC_GE_Department = Guid.NewGuid();

			mockMessagingContext.SetupGet(c => c.CurrentInboundConfig).Returns(nullDepartmentConfig);

			var responseXml = sender.SendRequest(@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
<Event>
<DataContext>
<DataTargetCollection>
	<DataTarget>
		<Type>ForwardingShipment</Type>
		<Key>S00001001</Key>
	</DataTarget>
	</DataTargetCollection>
	</DataContext>
<EventTime>2019-12-06T12:12:00</EventTime>
<EventType>Z99</EventType>
</Event>
</UniversalEvent>
").result;

			AssertContains("Invalid Department, you need to provide a valid department in EDI Client Details &gt; Department", responseXml);
		}

		public void TestSetupEnvironmentWithInactiveBranchUsed()
		{
			var branch = Factory.Load<GlbBranch>(config.ECC_GB_Branch);
			var initialValue = branch.GB_IsActive;
			branch.GB_IsActive = false;
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";
			Factory.Save();

			var responseXml = sender.SendRequest(@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
 <Event>
 <DataContext>
  <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
     </DataContext>
 <EventTime>2019-12-06T12:12:00</EventTime>
 <EventType>Z99</EventType>
 </Event>
</UniversalEvent>
").result;

			AssertContains($"Branch {Factory.Load<IGlbBranch>(config.ECC_GB_Branch).GB_Code} is not active, you need to provide an active branch in EDI Client Details &gt; Branch", responseXml);
			branch.GB_IsActive = initialValue;
		}

		public void TestSetupEnvironmentWithInvalidBranchPKUsed()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";
			Factory.Save();

			var nullBranchConfig = Factory.Load<EDICommunicationPartyConfig>(config.PK);
			nullBranchConfig.ECC_GB_Branch = Guid.NewGuid();

			mockMessagingContext.SetupGet(c => c.CurrentInboundConfig).Returns(nullBranchConfig);

			var responseXml = sender.SendRequest(@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
<Event>
<DataContext>
<DataTargetCollection>
    <DataTarget>
        <Type>ForwardingShipment</Type>
        <Key>S00001001</Key>
    </DataTarget>
    </DataTargetCollection>
    </DataContext>
<EventTime>2019-12-06T12:12:00</EventTime>
<EventType>Z99</EventType>
</Event>
</UniversalEvent>
").result;

			AssertContains("Invalid Branch, you need to provide a valid branch in EDI Client Details &gt; Branch", responseXml);
		}

		public void TestSetupEnvironmentWithEmptyBranchPKUsed()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";
			var emptyBranchConfig = Factory.Load<EDICommunicationPartyConfig>(config.PK);
			emptyBranchConfig.ECC_GB_Branch = ZGuid.Empty;
			Factory.Save();

			var responseXml = sender.SendRequest(@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
<Event>
<DataContext>
<DataTargetCollection>
    <DataTarget>
        <Type>ForwardingShipment</Type>
        <Key>S00001001</Key>
    </DataTarget>
    </DataTargetCollection>
    </DataContext>
<EventTime>2019-12-06T12:12:00</EventTime>
<EventType>Z99</EventType>
</Event>
</UniversalEvent>
").result;

			AssertContains("Invalid Branch, you need to provide a valid branch in EDI Client Details &gt; Branch", responseXml);
		}

		public void TestSetupEnvironmentWithEmptyDepartmentPKUsed()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001001";
			var emptyDepartmentConfig = Factory.Load<EDICommunicationPartyConfig>(config.PK);
			emptyDepartmentConfig.ECC_GE_Department = ZGuid.Empty;
			Factory.Save();

			var responseXml = sender.SendRequest(@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
<Event>
<DataContext>
<DataTargetCollection>
	<DataTarget>
		<Type>ForwardingShipment</Type>
		<Key>S00001001</Key>
	</DataTarget>
	</DataTargetCollection>
	</DataContext>
<EventTime>2019-12-06T12:12:00</EventTime>
<EventType>Z99</EventType>
</Event>
</UniversalEvent>
").result;

			AssertContains("Invalid Department, you need to provide a valid department in EDI Client Details &gt; Department", responseXml);
		}

		public const string ExternalReferenceNumber = "8a64619e-9a9b-40b8-aa52-330511287dc6";

		public void TestExternalReferenceNumberInUniversalXml()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001000";

			Factory.Save();

			var xml =
				$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>S0001000</Key>
				</DataTarget>
			</DataTargetCollection>
			<EventDepartment>
			<Code>TLC</Code>
			</EventDepartment>
			<CodesMappedToTarget>true</CodesMappedToTarget>
		</DataContext>
		<EventType>Z77</EventType>
		<EventTime>2014-08-18T10:36:33.557</EventTime>
		<EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
		<IsEstimate>False</IsEstimate>
		<MessageNumberCollection>
			<MessageNumber Type=""External"">{ExternalReferenceNumber}</MessageNumber>
		</MessageNumberCollection>
	</Event>
</UniversalEvent>";

			var result = sender.SendRequest(xml);

			var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			var responseMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));

			CombineAssertions(() =>
			{
				AssertEquals(System.Net.HttpStatusCode.OK, result.statusCode);
				AssertExternalReferenceNumber(result.result, ExternalReferenceNumber);
				AssertEquals("Check the External Reference Number in Request EDIMessage", ExternalReferenceNumber, requestMessage.EM_ExternalReferenceNumber);
				AssertEquals("Check the External Reference Number in Response EDIMessage", ExternalReferenceNumber, responseMessage.EM_ExternalReferenceNumber);
			});
		}

		public void TestExternalReferenceNumberInNativeXml()
		{
			var factory = new BusinessObjectFactory();
			var inboundXml =
$@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">AUSYD</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  <MessageNumberCollection>
      <MessageNumber Type=""External"">{ExternalReferenceNumber}</MessageNumber>
  </MessageNumberCollection>
  </Body>
</Native>";

			var result = sender.SendRequest(inboundXml);

			var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			var responseMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.OK, result.statusCode);
				AssertExternalReferenceNumber(result.result, ExternalReferenceNumber);
				AssertEquals("Check the External Reference Number in Request EDIMessage", ExternalReferenceNumber, requestMessage.EM_ExternalReferenceNumber);
				AssertEquals("Check the External Reference Number in Response EDIMessage", ExternalReferenceNumber, responseMessage.EM_ExternalReferenceNumber);
			});
		}

		public void TestEDIMessageTransportTypeForUniversalXml()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001000";

			Factory.Save();

			var xml =
				$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>S0001000</Key>
				</DataTarget>
			</DataTargetCollection>
			<EventDepartment>
			<Code>TLC</Code>
			</EventDepartment>
			<CodesMappedToTarget>true</CodesMappedToTarget>
		</DataContext>
		<EventType>Z77</EventType>
		<EventTime>2014-08-18T10:36:33.557</EventTime>
		<EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
		<IsEstimate>False</IsEstimate>
		<MessageNumberCollection>
			<MessageNumber Type=""External"">{ExternalReferenceNumber}</MessageNumber>
		</MessageNumberCollection>
	</Event>
</UniversalEvent>";

			var result = sender.SendRequest(xml);

			var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			var responseMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.OK, result.statusCode);
				AssertEquals("Check the transport type in Request EDIMessage", "EAD", requestMessage.EM_TransportType);
				AssertEquals("Check the transport type in Response EDIMessage", "EAD", responseMessage.EM_TransportType);
			});
		}

		public void TTestEDIMessageTransportTypeForNativeXml()
		{
			var factory = new BusinessObjectFactory();
			var inboundXml =
$@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">AUSYD</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  <MessageNumberCollection>
      <MessageNumber Type=""External"">{ExternalReferenceNumber}</MessageNumber>
  </MessageNumberCollection>
  </Body>
</Native>";

			var result = sender.SendRequest(inboundXml);

			var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			var responseMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));

			CombineAssertions(() =>
			{
				AssertEquals(HttpStatusCode.OK, result.statusCode);
				AssertEquals("Check the transport type in Request EDIMessage", "EAD", requestMessage.EM_TransportType);
				AssertEquals("Check the transport type in Response EDIMessage", "EAD", responseMessage.EM_TransportType);
			});
		}
	}
}
