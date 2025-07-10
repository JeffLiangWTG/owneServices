using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Management;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.DataTransfer;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.XmlIO.XmlReading;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static NUnit.Framework.XmlAssertions;
using SimpleLogger = Enterprise.UniversalDataBuss.Management.SimpleLogger;

namespace Enterprise.Services.ServiceHost.Tests
{
	sealed class eAdaptorControllerTest : BaseEAdaptorControllerTest
	{
		const string AddStorageDocWithUniversalEventStart = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			<Event>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>ForwardingShipment</Type>
							<Key>S00001953</Key>
						</DataTarget>
					</DataTargetCollection>
					<CodesMappedToTarget>true</CodesMappedToTarget>
				</DataContext>

				<EventTime>2014-08-18T10:36:33.557</EventTime>
				<EventType>DDI</EventType>
				<IsEstimate>false</IsEstimate>

				<ContextCollection>
					<Context>
						<Type>OrderNumber</Type>
						<Value>3380343</Value>
					</Context>
				</ContextCollection>
				<AttachedDocumentCollection>
					<AttachedDocument>
						<FileName>docTest.txt</FileName>
						<ImageData>";

		const string AddStorageDocWithUniversalEventEnd = @"5465737420446F63756D656E742055706C6F61642066696C652E</ImageData>
						<Type>
							<Code>HCC</Code>
						</Type>
						<IsPublished>false</IsPublished>
						<VisibleBranchCode>{0}</VisibleBranchCode>
			<VisibleCompanyCode>{1}</VisibleCompanyCode>
			<VisibleDepartmentCode>{2}</VisibleDepartmentCode>
			</AttachedDocument>
				</AttachedDocumentCollection>
			</Event>
		</UniversalEvent>";

		public void TestWarehouseImportNoMatchingProduct()
		{
			var warehouse = (IWhsWarehouse)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			warehouse.WW_WarehouseCode = "BRA";

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "XYZ";

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "BRA";
			branch1.GB_GC = company1.PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "WHIPOWHLZ";
			org.OH_IsWarehouseClient = true;

			Factory.Save();

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
      <LocalProcessing>
         <DeliveryRequiredBy>2021-12-10T13:00:09</DeliveryRequiredBy>
      </LocalProcessing>
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
			var res1 = sender.SendRequest(xml).result;

			AssertContains("Unable to match Product: 12X26X12P3756PR for Client WHIPOWHLZ.", res1);
			AssertRequestMessageStatus(EDIMessageStatusList.Codes.Discarded);
		}

		public void TestShouldNotDuplicateProcessLogWhenInactiveDepartmentUsed()
		{
			var departmentCode = "BRN";
			var shipment = (Forwarding.IForwardingShipment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Forwarding.IForwardingShipment)));
			Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, departmentCode).GE_IsActive = false;
			Factory.Save();
			var duplicateWarningMessage =
				$"Warning - Department {departmentCode} is not active. Falling back to evaluate default Department BRN";
			var inboundXml =
				$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
					<Event>
					<DataContext>
						<DataTargetCollection>
							<DataTarget>
								<Type>ForwardingShipment</Type>
								<Key>{shipment.JS_UniqueConsignRef}</Key>
							</DataTarget>
						</DataTargetCollection>
						<EventDepartment>
							<Code>{departmentCode}</Code>
						</EventDepartment>
						<CodesMappedToTarget>true</CodesMappedToTarget>
					</DataContext>
					<EventTime>2022-07-11T10:10:00</EventTime>
					<EventType>Z77</EventType>
					<EventReference>APP|98b8357d-0631-43fe-8578-4c574971ee5b</EventReference>
					<IsEstimate>False</IsEstimate>
					</Event>
					</UniversalEvent>";

			var occurences = sender.SendRequest(inboundXml).result.CountMatches(duplicateWarningMessage);
			AssertEquals(1, occurences);
		}

		public void TestWarehouseImportNoMatchingOrg()
		{
			AssertNoExistingMessages();

			var warehouse = (IWhsWarehouse)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			warehouse.WW_WarehouseCode = "BRA";

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "XYZ";

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "BRA";
			branch1.GB_GC = company1.PK;

			Factory.Save();

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
      <LocalProcessing>
         <DeliveryRequiredBy>2021-12-10T13:00:09</DeliveryRequiredBy>
      </LocalProcessing>
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
			var res1 = sender.SendRequest(xml).result;

			AssertContains("Warning - Matching 'ConsignorDocumentaryAddress':- No match found for", res1);
			AssertRequestMessageStatus(EDIMessageStatusList.Codes.Discarded);
		}

		void AssertNoExistingMessages()
		{
			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals("Precondition: No existing messages", 0, messages.Length);
		}

		void AssertRequestMessageStatus(string expectedStatus)
		{
			var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals("Expecting 1 request message", 1, messages.Length);
			AssertEquals(expectedStatus, messages[0].EM_Status);
		}

		public void TestAReallyBigMessage()
		{
			using (var bigStream = new VirtualMemoryStream())
			using (var wr = new StreamWriter(bigStream, Encoding.UTF8, 100, true))
			{
				wr.Write(AddStorageDocWithUniversalEventStart);
				var iterationsNeeded = 10_000_000;
				long stringSize = 1_000_000_000; // Determined using math.
				for (int i = 0; i < iterationsNeeded; i++)
				{
					wr.Write("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");
				}
				wr.Write(AddStorageDocWithUniversalEventEnd);
				wr.Flush();
				bigStream.Position = 0;

				var maxValue = default(long);

				XmlReader.OnNextTag_TestHook.Value = () =>
				{
					var next = GC.GetTotalMemory(true);
					if (maxValue < next)
					{
						maxValue = next;
					}
				};

				using (var request = new HttpRequestMessage())
				using (var controller = new eAdaptorController())
				{
					request.Content = new StreamContent(new WrappyStream(bigStream));
					controller.Request = request;
					var start = GC.GetTotalMemory(true);
					using (var response = controller.Post())
					{
						var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
						AssertEquals(1, messages.Length);

						AssertXMLEquals("", $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>
  <Data><UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <Company>
        <Code>DEM</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Demo Company</Name>
      </Company>
      <DataProvider>EDIDATDEM</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <EventType>DIF</EventType>

    <ContextCollection>
      <Context>
        <Type>OrderNumber</Type>
        <Value>3380343</Value>
      </Context>
      <Context>
        <Type>FailureReason</Type>
        <Value>Warning - No Module found a Business Entity to link this Universal Event to.</Value>
      </Context>
      <Context>
        <Type>ProcessingStatusCode</Type>
        <Value>DCD</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
</Data>
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Warning - No Module found a Business Entity to link this Universal Event to.
Message Discarded.</ProcessingLog>
</UniversalResponse>
", response.Content.ReadAsStringAsync().Result);
					}

					AssertLessThan("Lower is better, higher is worse. Start was: " + start, maxValue, start + stringSize * 2);
				}
			}
		}

		public void TestJobCostingBranchDoesNotMatchCompany_2012_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2012_11))
			{
				AssertJobCostingBranchDoesNotMatchCompany();
			}
		}

		public void TestJobCostingBranchDoesNotMatchCompany_2011_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				AssertJobCostingBranchDoesNotMatchCompany();
			}
		}

		void AssertJobCostingBranchDoesNotMatchCompany()
		{
			var warehouse = (IWhsWarehouse)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IWhsWarehouse)));
			warehouse.WW_WarehouseCode = "BRA";

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			company1.GC_Code = "XYZ";
			company2.GC_Code = "DEF";

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			branch2.GB_GC = company2.PK;
			branch2.GB_Code = "BRA";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "WHIPOWHLZ";
			org.OH_IsWarehouseClient = true;

			Factory.Save();

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
      <LocalProcessing>
         <DeliveryRequiredBy>2021-12-10T13:00:09</DeliveryRequiredBy>
      </LocalProcessing>
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

			using (SystemDataRegistry.Instance.CreateMissingWarehouseProduct.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var res1 = sender.SendRequest(xml).result;
				AssertContains("The branch 'BRA' in the &lt;JobCosting&gt; does not belong to the system company 'XYZ'", res1);
			}
		}

		public void TestNoTemplateApplicationFromUniversalEvent()
		{
			AssertNoExistingMessages();

			var inboundXml = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>{0}</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>ANGRY</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(string.Format(CultureInfo.InvariantCulture, inboundXml, "CHIPPY"));
				controller.Request = request;
				using (var response = controller.Post())
				{
					_ = response.Content.ReadAsStringAsync().Result;
				}
			}

			var query = new ZQuery(JobShipmentSchema.JS_HouseBill, "ANGRY");
			var reloadedShipment = Factory.LoadTop1<Forwarding.IForwardingShipment>(query);
			AssertEquals("ANGRY", reloadedShipment.JS_HouseBill);
			AssertEquals("CHIPPY", reloadedShipment.JS_GoodsDescription);

			var templateFactory = new BusinessObjectFactory();
			var template = templateFactory.New<ProcessTaskTemplate>();
			template.P0_Name = "Crunky Template";
			template.P0_ProcessType = "SHP";
			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.TemplateConditions.TemplateCondition2 = "UDF";
			milestone.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";
			milestone.TriggerConditions.TriggerEventCode = "Z03";
			var milestone2 = template.WorkflowItems.Milestones.AddNew();
			milestone2.TemplateConditions.TemplateCondition2 = "UDF";
			milestone2.TemplateConditions.TemplateCondition2Value = "\"2\"==\"1\"";
			milestone2.TriggerConditions.TriggerEventCode = "Z03";
			templateFactory.Save();

			reloadedShipment.JS_AdditionalTerms = "Trigger template application";
			Factory.Save();
			AssertEquals(1, ((IWorkflowProvider)reloadedShipment).WorkflowItems.Milestones.Count);

			milestone2.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";
			templateFactory.Save();

			var universalEvent = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
 <Event>
 <DataContext>
  <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>{reloadedShipment.JS_UniqueConsignRef}</Key>
        </DataTarget>
      </DataTargetCollection>
     </DataContext>
 <EventTime>2019-12-06T12:12:00</EventTime>
 <EventType>Z03</EventType>
 </Event>
</UniversalEvent>";

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(universalEvent);
				controller.Request = request;
				using (var response = controller.Post())
				{
					_ = response.Content.ReadAsStringAsync().Result;
				}
			}

			reloadedShipment = new BusinessObjectFactory().LoadTop1<Forwarding.IForwardingShipment>(query);
			AssertEquals(true, ((EnterpriseBusinessObject)reloadedShipment).Logs.GetAllLogs().Cast<StmALog>().Any(l => l.SL_SE_NKEvent == "Z03"));
			AssertEquals("Adding a universal event that just fires process tasks should not trigger template application as this is expensive. If this test is failing after making changes to a process task during save, wrap the changes in the SuspendSettingHasChangesForTemplateApplication disposable.", 1, ((IWorkflowProvider)reloadedShipment).WorkflowItems.Milestones.Count);
		}

		public void TestActiveBusinessObjectCollectionsDeactivated()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";
			Factory.Save();

			var universalEvent = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
 <Event>
 <DataContext>
  <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>{shipment.JS_UniqueConsignRef}</Key>
        </DataTarget>
      </DataTargetCollection>
     </DataContext>
 <EventTime>2019-12-06T12:12:00</EventTime>
 <EventType>Z03</EventType>
 </Event>
</UniversalEvent>";

			ActiveBusinessObjectCollection<DummyDependantBusinessObject> requestFactoryCollection = null;
			ActiveBusinessObjectCollection<DummyDependantBusinessObject> processingFactoryCollection = null;

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				var bizo = factory.New<DummyWithDependentsBusinessObject>();
				var collection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(bizo);
				collection.AddNew();
				((IBusiness)collection).IncrementReadOnlyIncludingChildren();

				if (factory.NameForDebugging == "Request Message")
				{
					requestFactoryCollection = collection;
				}
				else if (factory.NameForDebugging == MasterFilesIntegrationConstants.PublishUniversalXmlInternallyFactoryName)
				{
					processingFactoryCollection = collection;
				}
			});

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(universalEvent);
				controller.Request = request;
				using (var response = controller.Post())
				{
					_ = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertNotNull(requestFactoryCollection);
			AssertNotNull(processingFactoryCollection);
			AssertEquals(true, requestFactoryCollection.IsDeactivated);
			AssertEquals(true, processingFactoryCollection.IsDeactivated);
		}

		public void TestExceptionWhenDeactivatingActiveCollections()
		{
			var requestXml = @"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SS00011826</Key>
        </DataTarget>
      </DataTargetCollection>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>";

			var isFirstRequest = true;
			ActiveBusinessObjectCollectionTest.DummyActiveBusinessObjectCollection collection;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(_ =>
			{
				if (isFirstRequest)
				{
					isFirstRequest = false;
					var factory = new BusinessObjectFactory();
					collection = new ActiveBusinessObjectCollectionTest.DummyActiveBusinessObjectCollection(factory);
					collection.AddNew();
					collection.SetHookOnIndexDisposed(() => throw new InvalidOperationException());
				}
			});

			var response = sender.SendRequest(requestXml).result;

			AssertNotNull(response);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();

			response = sender.SendRequest(requestXml).result;

			AssertNotNull(response);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestTemplateApplicationFromUniversalShipment()
		{
			AssertNoExistingMessages();

			var inboundXml = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>{0}</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>ANGRY</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(string.Format(CultureInfo.InvariantCulture, inboundXml, "CHIPPY"));
				controller.Request = request;
				using (var response = controller.Post())
				{
					_ = response.Content.ReadAsStringAsync().Result;
				}
			}

			var query = new ZQuery(JobShipmentSchema.JS_HouseBill, "ANGRY");
			var reloadedShipment = Factory.LoadTop1<Forwarding.IForwardingShipment>(query);
			AssertEquals("ANGRY", reloadedShipment.JS_HouseBill);
			AssertEquals("CHIPPY", reloadedShipment.JS_GoodsDescription);

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Crunky Template";
			template.P0_ProcessType = "SHP";
			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.TemplateConditions.TemplateCondition2 = "UDF";
			milestone.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";
			milestone.TriggerConditions.TriggerEventCode = "Z03";
			Factory.Save();

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(string.Format(CultureInfo.InvariantCulture, inboundXml, "SMOOMOO"));
				controller.Request = request;
				using (var response = controller.Post())
				{
					_ = response.Content.ReadAsStringAsync().Result;
				}
			}

			reloadedShipment = new BusinessObjectFactory().LoadTop1<Forwarding.IForwardingShipment>(query);
			AssertEquals(1, ((IWorkflowProvider)reloadedShipment).WorkflowItems.Milestones.Count);

			var wtaEvent = ((IStmALogParent)reloadedShipment).Logs.Find(l => l.SL_SE_NKEvent == "WTA" && l.SL_Reference.Contains(template.P0_Name)).First();
			AssertNotNull(wtaEvent);

			var webBranch = Factory.Load<GlbBranch>(DataRegistry.Instance.WebBranch);
			var webDepartment = Factory.Load<GlbDepartment>(DataRegistry.Instance.WebDepartment);
			AssertEquals(User.InterchangeUserCode, wtaEvent.User.GS_Code);
			AssertEquals(webBranch.GB_Code, wtaEvent.SL_GB_NKBranch);
			AssertEquals(webBranch.Company.GC_Code, wtaEvent.CompanyCode);
			AssertEquals(webDepartment.GE_Code, wtaEvent.SL_GE_NKDepartment);
		}

		public void TestJobHeaderHasNoDefaultBranch()
		{
			JobBranchDefaultOrderRule rule = new JobBranchDefaultOrderRule();

			rule.DefaultToBlank = 1;
			rule.DefaultToBranchOfOrganisation = 0;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
			rule.DefaultToLoginUserDefault = 0;
			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			var inboundXml = @"<UniversalShipment xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>DEM</Code>
        <Description>AU Demo Company</Description>
      </Company>
      <EventBranch>
        <Code>020</Code>
        <Description>Domestic Dummy 1</Description>
      </EventBranch>
      <EventDepartment>
        <Code>FEA</Code>
        <Description>Forwarding Export Air</Description>
      </EventDepartment>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <GoodsDescription>test</GoodsDescription>
    <LocalProcessing>
      <InsuranceRequired>false</InsuranceRequired>
    </LocalProcessing>
    <OuterPacks>0</OuterPacks>
    <TotalWeight>0,0</TotalWeight>
    <TotalWeightUnit>
      <Code>KG </Code>
    </TotalWeightUnit>
    <TotalVolume>0,0</TotalVolume>
    <ActualChargeable>0,0</ActualChargeable>
    <TransportMode>
      <Code>AIR</Code>
    </TransportMode>
    <ContainerMode>
      <Code>LSE</Code>
    </ContainerMode>
    <ShipmentIncoTerm>
      <Code>CFR</Code>
    </ShipmentIncoTerm>
    <ServiceLevel>
      <Code>STD</Code>
    </ServiceLevel>
    <JobCosting>
      <Branch>
        <Code>S01</Code>
      </Branch>
      <Department>
        <Code>FEA</Code>
      </Department>
    </JobCosting>
    <WayBillNumber>12345ABCDEXYZ95534</WayBillNumber>
    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>true</IsEstimate>
      </Date>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>true</IsEstimate>
      </Date>
    </DateCollection>
    <PackingLineCollection>
      <PackingLine>
        <PackQty>2</PackQty>
        <PackType>
          <Code>CTN</Code>
        </PackType>
        <Weight>11,0</Weight>
        <WeightUnit>
          <Code>KG</Code>
        </WeightUnit>
        <Volume>1,0</Volume>
        <VolumeUnit>
          <Code>M3</Code>
        </VolumeUnit>
        <Commodity>
          <Code>GEN</Code>
        </Commodity>
        <GoodsDescription>test</GoodsDescription>
        <RequiresTemperatureControl>false</RequiresTemperatureControl>
        <CustomizedFieldCollection>
          <CustomizedField>
            <DataType>Boolean</DataType>
            <Key>Stackable</Key>
            <Value>false</Value>
          </CustomizedField>
        </CustomizedFieldCollection>
      </PackingLine>
    </PackingLineCollection>
    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type>
          <Code>TOM</Code>
          <Description>Tom Id Reference</Description>
        </Type>
        <ReferenceNumber>T155913</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type>
          <Code>CNR</Code>
          <Description>Consignor Reference</Description>
        </Type>
        <ReferenceNumber>23</ReferenceNumber>
      </AdditionalReference>
      <AdditionalReference>
        <Type>
          <Code>CNE</Code>
          <Description>Consignee Reference</Description>
        </Type>
        <ReferenceNumber>23</ReferenceNumber>
      </AdditionalReference>
    </AdditionalReferenceCollection>
    <NoteCollection>
      <Note>
        <Description>URL TOM Order</Description>
        <IsCustomDescription>true</IsCustomDescription>
        <NoteContext>
          <Code>AAA</Code>
          <Description>Module: A - All, Direction: A - All, Freight: A - All</Description>
        </NoteContext>
        <NoteText>http://localhost:8080/v2/order/15591?token=6990478D-A619-4D3A-945B-9BBB99524652</NoteText>
      </Note>
      <Note>
        <Description>Order Source</Description>
        <IsCustomDescription>true</IsCustomDescription>
        <NoteContext>
          <Code>AAA</Code>
          <Description>Module: A - All, Direction: A - All, Freight: A - All</Description>
        </NoteContext>
        <NoteText>Manual</NoteText>
      </Note>
    </NoteCollection>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <Address1>1 Park St</Address1>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>Wookie</AddressShortCode>
        <City>Sydney</City>
        <Country>
          <Code>AU</Code>
          <Label>Australia</Label>
        </Country>
        <Postcode>2000</Postcode>
        <State>NSW</State>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <OrganizationCode>WOOMISCHI1</OrganizationCode>
      </OrganizationAddress>
      <OrganizationAddress>
        <Address1>1 Park St</Address1>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>Wookie</AddressShortCode>
        <City>Sydney</City>
        <Country>
          <Code>AU</Code>
          <Label>Australia</Label>
        </Country>
        <Postcode>2000</Postcode>
        <State>NSW</State>
        <AddressType>ControllingCustomer</AddressType>
        <OrganizationCode>WOOMISCHI1</OrganizationCode>
      </OrganizationAddress>
      <OrganizationAddress>
        <Address1>1 Park St</Address1>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>Wookie</AddressShortCode>
        <City>Sydney</City>
        <Country>
          <Code>AU</Code>
          <Label>Australia</Label>
        </Country>
        <Postcode>2000</Postcode>
        <State>NSW</State>
        <AddressType>NotifyParty</AddressType>
        <OrganizationCode>WOOMISCHI1</OrganizationCode>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressOverride>false</AddressOverride>
        <AddressType>ControllingAgent</AddressType>
        <OrganizationCode>WOOMISCHI1</OrganizationCode>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <CustomizedFieldCollection>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>ETD requested by Customer</Key>
      </CustomizedField>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>ETA requested by Customer</Key>
      </CustomizedField>
      <CustomizedField>
        <DataType>DateTime</DataType>
        <Key>Cargo Availability</Key>
        <Value>2021-04-10T05:21:00</Value>
      </CustomizedField>
    </CustomizedFieldCollection>

  </Shipment>
</UniversalShipment>";

			var responseString = "";
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseString = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains("Error - Job Costing Branch could not be defaulted.", responseString);
		}

		public void TestDoNotReportInvalidXmlViaErrorReporter()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var inboundXml = string.Format(CultureInfo.InvariantCulture, @"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key><>SS00011826</></Key>
        </DataTarget>
      </DataTargetCollection>
      <EnterpriseID>{0}</EnterpriseID>
      <ServerID>{1}</ServerID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>", registrationKey.EnterpriseCode, registrationKey.ServerCode);

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;

				AssertNoExceptionThrown(() => controller.Post().Dispose());
			}
		}

		public void TestDocumnetRequestDoesNotDuplicateDexEvents()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentNumber = shipment.JS_UniqueConsignRef = "S00009001";

			var docManagerInfo = ((IDocManagerSupport)shipment).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "File1", "INV");
			var eDoc2 = docManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "File2", "INV");

			docManagerInfo.MasterFactory.Save();
			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var requestXml =
$@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
<DocumentRequest>
<DataContext>
    <DataTargetCollection>
    <DataTarget>
        <Type>ForwardingShipment</Type>
        <Key>{shipmentNumber}</Key>
    </DataTarget>
    </DataTargetCollection>
    <Company>
    <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
    </Company>
    <EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
    <ServerID>{registrationKey.ServerCode}</ServerID>
</DataContext>
</DocumentRequest>
</UniversalDocumentRequest>";

			var response = sender.SendRequest(requestXml).result;
			AssertContains("<FileName>File1</FileName>", response);
			AssertContains("<FileName>File2</FileName>", response);

			var logs = (new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(shipment.PK) as IStmALogParent).Logs;
			var dexLogs = logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent,Events.DataExportCode));
			AssertEquals(1, dexLogs.Length);
			AssertEquals("DEX event should be linked to the response message", ReceiveTransmitList.Codes.Transmit, dexLogs[0].RelatedEDIMessage?.Message.EM_ReceiveTransmit);
		}

		public void TestInvalidElementProducesWarning()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00001001";
			shipment.JS_UniqueConsignRef = "S00001001";

			Factory.Save();

			var eventWithInvalidElement = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
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
 <EventType>Z03</EventType>
<Foo/>
 </Event>
</UniversalEvent>";

			using (var bigStream = new VirtualMemoryStream())
			using (var wr = new StreamWriter(bigStream, Encoding.UTF8, 100, true))
			{
				wr.Write(eventWithInvalidElement);
				wr.Flush();
				bigStream.Position = 0;

				using (var request = new HttpRequestMessage())
				using (var controller = new eAdaptorController())
				{
					request.Content = new StreamContent(bigStream);
					controller.Request = request;
					using (var response = controller.Post())
					{
						var note = Factory.Load<StmNote>(new ZQuery()).Where(x => x.ST_Table == "EDIMessage").First();
						Assert(
							"Expect 1 warnings about Foo element",
							((string)note.ST_NoteText).Split(new char[] { ' ', '\r', '\n' }).Where(x => x == "Warning").Count() == 1 &&
							((string)note.ST_NoteText).Contains("Foo"));

						var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
						AssertEquals(1, messages.Length);

						AssertXMLEquals("", $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>
  <Data><UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001001</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>DEM</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Demo Company</Name>
      </Company>
      <DataProvider>EDIDATDEM</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <EventType>DIM</EventType>

    <ContextCollection>
      <Context>
        <Type>HBOLNumber</Type>
        <Value>S00001001</Value>
      </Context>
      <Context>
        <Type>ProcessingLog</Type>
        <Value>Linked Event to Shipment S00001001 (House Bill='S00001001').</Value>
      </Context>
      <Context>
        <Type>ProcessingStatusCode</Type>
        <Value>WAR</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
</Data>
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Warning - Line 13: &lt;Event&gt; - Unrecognised element &lt;Foo&gt; found. Element skipped.
Linked Event to Shipment S00001001 (House Bill='S00001001').</ProcessingLog>
</UniversalResponse>
", response.Content.ReadAsStringAsync().Result);
					}
				}
			}
		}

		public void TestMessageNumberCollectionPopulatedInEDIMessage()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00001001";
			shipment.JS_UniqueConsignRef = "S00001001";

			Factory.Save();

			var messageCollectionEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
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
 <EventType>Z03</EventType>
<Foo/>
 </Event>
</UniversalEvent>";

			using (var bigStream = new VirtualMemoryStream())
			using (var wr = new StreamWriter(bigStream, Encoding.UTF8, 100, true))
			{
				wr.Write(messageCollectionEvent);
				wr.Flush();
				bigStream.Position = 0;

				using (var request = new HttpRequestMessage())
				using (var controller = new eAdaptorController())
				{
					request.Content = new StreamContent(bigStream);
					controller.Request = request;
					using (var response = controller.Post())
					{
						var requestMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
						AssertEquals(1, requestMessages.Length);
						var requestMessage = requestMessages[0];

						var responseMessages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));
						AssertEquals(1, responseMessages.Length);
						var responseMessage = responseMessages[0];

						Assert("Message number collection populated in response EDI message", responseMessage.EM_MessageText.Contains($@"<MessageNumber Type=""MessageNumber"">{requestMessage.EM_MessageNum}</MessageNumber>"));
					}
				}
			}
		}

		public void TestHandlesHTTPException()
		{
			eAdaptorHandlerFactory.SetGetHandlerHook((ref IHttpXmlMessageHandler h) =>
			{
				throw new HttpException("The client is disconnected because the underlying request has been completed. There is no longer an HttpContext available.");
			});

			var response = sender.SendRequest("<UniversalEvent></UniversalEvent>");
			AssertMultilineASCIIEquals("",
			"{\"Message\":\"Post request lost input\"}",
			response.result);
		}

		public void TestHandlesBadWebEnvException()
		{
			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;
			ukBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";
			Factory.Save();
			DataRegistry.Instance.WebBranch = ukBranch.PK.ToGuid();
			DataRegistry.Instance.WebDepartment = ZGuid.NewZGuid().ToGuid();
			var eventWithInvalidElement = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
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

			using (var bigStream = new VirtualMemoryStream())
			using (var wr = new StreamWriter(bigStream, Encoding.UTF8, 100, true))
			{
				wr.Write(eventWithInvalidElement);
				wr.Flush();
				bigStream.Position = 0;

				using (var request = new HttpRequestMessage())
				using (var controller = new eAdaptorController())
				{
					request.Content = new StreamContent(bigStream);
					controller.Request = request;
					using (var response = controller.Post())
					{
						AssertContains("Invalid Department, you need to provide a valid department in the System Registry under Web > Web Department", response.Content.ReadAsStringAsync().Result);
					}
				}
			}
		}

		public void TestHandlesBadWebEnvException_NoCompany()
		{
			//This case can only happen if someone has hacked around in the DB. If the branch is loaded the company must exist due to FK constraint
			var company = (BusinessObject)Factory.New<IGlbCompany>();
			company[GlbCompanySchema.GC_Code.Name] = "PUK";
			company[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var branch = (BusinessObject)Factory.New<IGlbBranch>();
			branch[GlbBranchSchema.GB_Code.Name] = "PUK";
			branch[GlbBranchSchema.GB_GC.Name] = company.PK;
			branch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";
			Factory.Save();
			DataRegistry.Instance.WebBranch = branch.PK.ToGuid();

			Db.Connection.Command("ALTER TABLE dbo.GlbBranch DROP CONSTRAINT GlbBranch_GB_GC_FK2_GlbCompany_RRR_120N").ExecuteNonQuery();
			company.Delete();
			Factory.Save();

			var universalEventXml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
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

			var result = sender.SendRequest(universalEventXml).result;

			AssertContains("Unable to load company for branch", result);
		}

		public void TestHandlesSpecificHTTPExceptionDisconnect()
		{
			var ex = new HttpException(500, "It all went wrong", unchecked((int)0x80070040));
			AssertExceptionErrorMessage(ex, "Abnormal client termination.", sender);
		}

		public void TestHandlesClientAbnormalDisconnectException()
		{
			var ex = new HttpException("It all went wrong", new COMException("Bye", unchecked((int)0x800703E3)));
			AssertExceptionErrorMessage(ex, "Abnormal client termination.", sender);
		}

		public void TestHandlesCOMException()
		{
			var ex = new HttpException("It all went wrong", new COMException());
			AssertExceptionErrorMessage(ex, "Unexpected COM exception.", sender);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestHandlesClientDisconnectedException()
		{
			var ex = new HttpException("The client disconnected", new COMException("Bye", unchecked((int)0x80004005)));
			var statusCode = AssertExceptionErrorMessage(ex, "Abnormal client termination.", sender);
			AssertEquals(HttpStatusCode.BadRequest, statusCode);
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestPost_UsesDbSafelyFromAnotherThread()
		{
			ThreadStart threadstart1 = new ThreadStart(delegate
			{
				TestUniversalRequest();
			});

			Thread thread1 = new Thread(threadstart1);
			thread1.Start();
			thread1.Join();
		}

		public void TestUniversalRequest()
		{
			const string xml = @"<TestXML>AAA</TestXML>";

			var handler = new DummyHttpXmlMessageHandler()
			{
				processingResult = new HttpXmlProcessingResult(),
			};

			handler.requestMessage = handler.CreateRequestMessage();
			handler.requestMessage.Save();
			handler.responseMessage = handler.CreateResponseMessage();
			handler.responseMessage.Status = "STS";
			handler.responseMessage.Save();

			using (var dataStream = new CargoWise.IO.Shim.SubStreamableStream())
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				new StreamWriter(dataStream) { AutoFlush = true }.Write("Data");
				handler.processingResult.Status = "STS";
				handler.processingResult.ResponseMessageText = dataStream;

				eAdaptorHandlerFactory.SetGetHandlerHook((ref IHttpXmlMessageHandler h) => { h = handler; });

				request.Content = new StringContent(xml);
				controller.Request = request;

				using (var response = controller.Post())
				{
					var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
					AssertEquals(2, messages.Length);
					var message = messages.Where(m => m.PK != ((IEDIMessage)handler.requestMessage).PK).First();
					AssertXMLEquals($@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>STS</Status>
  <Data>Data</Data>
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{message.EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog />
</UniversalResponse>", response.Content.ReadAsStringAsync().Result);
				}
			}
		}

		[TestDate(2021, 03, 15)]
		public void TestUniversalTransactionBatchImport()
		{
			CreateTestPeriod();
			CreateAUDBankAccount();
			Factory.Save();

			AssertNoExistingMessages();

			var inboundXml = @"<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<DataContext>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>DAT</ServerID>
			<Company>
				<Code>EDI</Code>
				<Name>Eagle Datamation International</Name>
			</Company>
		</DataContext>
		<TransactionCollection>
			<Transaction>
				<DataContext>
					<DataTargetCollection>
						<DataTarget>
							<Type>AccountingReceipt</Type>
						</DataTarget>
					</DataTargetCollection>
				</DataContext>
				<Ledger>AR</Ledger>
				<TransactionType>REC</TransactionType>
				<TransactionDate>2021-03-15T14:12:00</TransactionDate>
				<PostDate>2021-03-15T14:12:00</PostDate>
				<OrganizationAddress>
					<AddressType>ConsigneeDocumentaryAddress</AddressType>
					<AddressShortCode>Pick Up Address</AddressShortCode>
					<OrganizationCode>BAROPT</OrganizationCode>
					<Address1>12 COOLIBAH DRIVE</Address1>
					<Address2></Address2>
					<AddressOverride>false</AddressOverride>
					<City>PALM BEACH</City>
					<CompanyName>BARZ OPTICS</CompanyName>
					<Country>
						<Code>AU</Code>
						<Name>Australia</Name>
					</Country>
					<Port>
						<Code>AUBNE</Code>
						<Name>Brisbane</Name>
					</Port>
					<Postcode>4221</Postcode>
					<State>QLD</State>
				</OrganizationAddress>
				<Description>AR REC</Description>
				<PaymentOrReceiptType>CSH</PaymentOrReceiptType>
				<BankAccount>ZHSBCAUD</BankAccount>
				<CheckBookCode></CheckBookCode>
				<CheckNumberOrPaymentRef>CASH</CheckNumberOrPaymentRef>
				<OSCurrency>
					<Code>AUD</Code>
					<Description>Australian Dollar</Description>
				</OSCurrency>
				<OSExGSTVATAmount>100.0000</OSExGSTVATAmount>
				<OSTotal>100.0000</OSTotal>
				<LocalCurrency>
					<Code>AUD</Code>
					<Description>Australian Dollar</Description>
				</LocalCurrency>
				<LocalExVATAmount>100.0000</LocalExVATAmount>
				<LocalTotal>100.0000</LocalTotal>
				<Branch>
					<Code>BNE</Code>
					<Name>BN - AUBNE</Name>
				</Branch>
				<Department>
					<Code>BRN</Code>
					<Name>Branch</Name>
				</Department>
				<CheckDrawer></CheckDrawer>
				<DrawerBank></DrawerBank>
				<DrawerBranch></DrawerBranch>
				<PostingJournalCollection>
					<PostingJournal></PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
					AssertEquals(1, messages.Length);
					var expectedResult = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>
  <Data><UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AccountingMatching</Type>
          <Key></Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <EventType>DIM</EventType>
    <ContextCollection>
      <Context>
        <Type>ProcessingLog</Type>
        <Value></Value>
      </Context>
      <Context>
        <Type>ProcessingStatusCode</Type>
        <Value>PRS</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
</Data>
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Matching 'ConsigneeDocumentaryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Begin processing Transaction AR REC BAROPT ZHSBCAUD CASH: 
  Completed Processing Transaction.</ProcessingLog>
</UniversalResponse>";
					AssertXMLEquals(expectedResult, response.Content.ReadAsStringAsync().Result);
				}
			}
		}

		[TestDate(2021, 03, 15)]
		public void TestUniversalTransactionImport_WithValidationError_AR()
		{
			var inboundXml = @"<UniversalTransaction>
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
			<OrganizationCode>
DEPORT_AU</OrganizationCode>
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
					<Key>
DEPORT_AU</Key>
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

			AssertNoExistingMessages();

			var financialTransactionsBefore = Factory.Load<AccTransactionHeader>(new ZQuery());
			AssertEquals("Precondition: no transactions", 0, financialTransactionsBefore.Length);

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;

				using (var response = controller.Post())
				{
					CombineAssertions(() =>
					{
						var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
						AssertEquals(1, messages.Length);
						AssertXMLEquals($@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>ERR</Status>
  <Data><UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <Company>
        <Code>DEM</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Demo Company</Name>
      </Company>
      <DataProvider>EDIDATDEM</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <EventType>DIF</EventType>

    <ContextCollection>
      <Context>
        <Type>FailureReason</Type>
        <Value>Import failed because transaction has validation errors:
Error - Generic Charge: Enter a valid selection.
Error - Tax Date: No rate found for selected date.
Error - Generic Charge: Enter a valid selection.
Error - Tax Date: No rate found for selected date.
Error - Terms: Please enter a Terms.
Error - Address Override: Please enter an Account.
Error - Account: Please enter an Account.
Error - Invoice Post Date: This date does not fall into a valid accounting period’s date range.
Please go to Manage &gt; General Ledger &gt; Period Management &gt; Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.
</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
</Data>
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Warning - Line 58: &lt;TransactionInfo&gt;.&lt;OrganizationAddress&gt;.&lt;OrganizationCode&gt;The field does not accept carriage returns but a carriage return was detected in the XML. All carriage returns have been converted to a white space.
Warning - Line 95: &lt;TransactionInfo&gt;.&lt;PostingJournalCollection&gt;.&lt;PostingJournal&gt;.&lt;Organization&gt;.&lt;Key&gt;The field does not accept carriage returns but a carriage return was detected in the XML. All carriage returns have been converted to a white space.
Warning - Matching 'OFC':- No match found for '[Org. Code: DEPORT_AU]'.
Warning - Transaction Header Branch Address is missing or incomplete. You must at-least populate BranchAddress/AddressType and BranchAddress/Country/Code when importing XML Universal Transactions.
Error - Import failed because transaction has validation errors:
Error - Generic Charge: Enter a valid selection.
Error - Tax Date: No rate found for selected date.
Error - Generic Charge: Enter a valid selection.
Error - Tax Date: No rate found for selected date.
Error - Terms: Please enter a Terms.
Error - Address Override: Please enter an Account.
Error - Account: Please enter an Account.
Error - Invoice Post Date: This date does not fall into a valid accounting period’s date range.
Please go to Manage &gt; General Ledger &gt; Period Management &gt; Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.
</ProcessingLog>
</UniversalResponse>", response.Content.ReadAsStringAsync().Result);

						var newTransactions = Factory.Load<AccTransactionHeader>(new ZQuery());
						AssertEquals("imports with validation errors should not create database records. This is very bad for Accounting.", 0, newTransactions.Length);
					});
				}
			}
		}

		public void TestValidation()
		{
			var inboundXml = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>{0}</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>ANGRY</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

			var validationCount = 0;
			PersistentFactoryCacheManager.Instance.OnFactoryAdded = f => f.OnBeginValidation += b => ++validationCount;

			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(string.Format(CultureInfo.InvariantCulture, inboundXml, "CHIPPY"));
				controller.Request = request;
				using (var response = controller.Post())
				{
					_ = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertEquals(0, validationCount);
		}

		public void TestGetHandler()
		{
			var sessionTracker = ObjectFactory.Get<IXmlSessionTracker>("IXmlSessionTracker", new SimpleLogger());
			var config = new DefaultProcessingConfig();
			var request = new HttpRequestMessage() { Content = new StringContent("") };

			using (var incomingStream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				request.Content.CopyToAsync(incomingStream).GetAwaiter().GetResult();
				var handler = eAdaptorHandlerFactory.GetHandler(sessionTracker, config, "", incomingStream);
				AssertNull(handler);
			}

			void AssertGetHandler(string handlerName)
			{
				using (var incomingStream = new CargoWise.IO.Shim.SubStreamableStream())
				{
					request.Content.CopyToAsync(incomingStream).GetAwaiter().GetResult();

					var handler = eAdaptorHandlerFactory.GetHandler(sessionTracker, config, handlerName, incomingStream);
					AssertNotNull(handler);
					AssertEquals(handlerName, handler.GetType().Name);
				}
			}

			string[] handlerNameTestCasesList = {
				"UniversalShipmentRequestHandler",
				"UniversalTransactionBatchRequestHandler",
				"UniversalEventImportHandler",
				"UniversalShipmentImportHandler",
				"UniversalTransactionBatchImportHandler"
				};

			handlerNameTestCasesList.ForEach(h => AssertGetHandler(h));

			var xml = @"<Native><Body><UNLOCO></UNLOCO></Body></Native>";
			request = new HttpRequestMessage { Content = new StringContent(xml) };
			AssertGetHandler("NativeXmlRequestHandler");
		}

		public void TestInvoiceWithConsolLines_RequestHasConsolCosting()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consol = testObjectCreator.CreateConsol();
			var shipment = testObjectCreator.CreateShipment("S001", consol);
			var job = testObjectCreator.CreateJob(shipment);
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: testObjectCreator.Creditor1);
			var line = testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AUD, 1.0m, 10m, 10m, 0m);
			invoice.AH_PostDate = new ZDateTime(2009, 06, 15);
			var charge = job.Charges.AddNew();
			charge.JR_AC = testObjectCreator.CC1.PK;
			charge.JR_OSSellAmt = 0M;
			line.AL_AC = testObjectCreator.CC1.PK;
			line.AL_JH = job.PK;
			line.AL_AT = testObjectCreator.CC1.GSTRate.PK;
			charge.JR_AL_APLine = line.PK;
			charge.SetAmountsFromLinkedLinesForTests();
			Factory.Save();

			var consolCost = testObjectCreator.CreateConsolCost(invoice, consol, testObjectCreator.CC1, 10);
			invoice.ImportSingleCost(consolCost, line);
			Factory.Save();

			var enterpriseID = "EDI";
			var serverID = "DAT";
			var dataContextCompanyCode = "EDI";
			var dataTargetType = "ForwardingShipment";

			var requestContent =
				$@"<UniversalShipmentRequest version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
<ShipmentRequest>
	<DataContext>
		<EnterpriseID>{enterpriseID}</EnterpriseID>
		<ServerID>{serverID}</ServerID>
		<Company><Code>{dataContextCompanyCode}</Code></Company>
		<DataTargetCollection>
			<DataTarget><Type>{dataTargetType}</Type><Key>{shipment.JS_UniqueConsignRef}</Key></DataTarget>
		</DataTargetCollection>
	</DataContext>
</ShipmentRequest>
</UniversalShipmentRequest>";
			using (var adaptor = new eAdaptorController { Request = new HttpRequestMessage { Content = new StringContent(requestContent) } })
			using (var response = adaptor.Post())
			{
				AssertContains("<ConsolCosts>", response.Content.ReadAsStringAsync().Result);
				response.Content.Dispose();
			}
		}

		public void TestCriteria_MissingEntity()
		{
			var requestContent = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <Organization>
      <CriteriaGroup Type=""Key"">
        <Criteria FieldName=""Fierd"">
          AUSDISMEL
        </Criteria>
      </CriteriaGroup>
    </Organization>
  </Body>
</Native>";
			using (var adaptor = new eAdaptorController { Request = new HttpRequestMessage { Content = new StringContent(requestContent) } })
			using (var response = adaptor.Post())
			{
				var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
				AssertEquals(1, messages.Length);

				AssertMultilineASCIIEquals($@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>ERR</Status>
  <Data />
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Error - Criteria was missing attribute Entity
Information - 0 matches found.</ProcessingLog>
</UniversalResponse>", response.Content.ReadAsStringAsync().Result);
				response.Content.Dispose();
			}
		}

		public void TestCriteria_MissingFieldName()
		{
			var requestContent = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <Organization>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""OrgHeader"">
          AUSDISMEL
        </Criteria>
      </CriteriaGroup>
    </Organization>
  </Body>
</Native>";
			using (var adaptor = new eAdaptorController { Request = new HttpRequestMessage { Content = new StringContent(requestContent) } })
			using (var response = adaptor.Post())
			{
				var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
				AssertEquals(1, messages.Length);

				AssertMultilineASCIIEquals($@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>ERR</Status>
  <Data />
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Error - Criteria was missing attribute FieldName
Information - 0 matches found.</ProcessingLog>
</UniversalResponse>", response.Content.ReadAsStringAsync().Result);
				response.Content.Dispose();
			}
		}

		public void TestForwardingShipment_RequestHasJobCosting()
		{
			var forwardingShipment = Factory.New<Forwarding.IForwardingShipment>();
			forwardingShipment.JS_TransportMode = "SEA"; // Sea Freight
			forwardingShipment.JS_PackingMode = "LCL";
			forwardingShipment.JS_ShipmentType = "STD"; // Standard House

			var consignor = Factory.New<IOrgHeader>();
			consignor.OH_FullName = "I'LL SEND IT";
			consignor.OH_RL_NKClosestPort = "NZAKL";

			var consignee = Factory.New<IOrgHeader>();
			consignee.OH_FullName = "GIMME GIMME";
			consignee.OH_RL_NKClosestPort = "AUSYD";
			consignee.OH_IsDebtor = true;

			forwardingShipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			forwardingShipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;

			var shipmentBO = forwardingShipment as BusinessObject;
			var creator = ObjectFactory.New<IAccountingTestDataCreator>();
			creator.CreateJobHeader(shipmentBO, consignee.PK);
			creator.AddChargeLineToCreatedJobHeader("FRT", "FAT RICH TRUNKS", 123.45m, "AUD");

			Factory.Save();

			var enterpriseID = "EDI";
			var serverID = "DAT";
			var dataContextCompanyCode = "EDI";
			var dataTargetType = "ForwardingShipment";

			var requestContent =
				$@"<UniversalShipmentRequest version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
<ShipmentRequest>
	<DataContext>
		<EnterpriseID>{enterpriseID}</EnterpriseID>
		<ServerID>{serverID}</ServerID>
		<Company><Code>{dataContextCompanyCode}</Code></Company>
		<DataTargetCollection>
			<DataTarget><Type>{dataTargetType}</Type><Key>{forwardingShipment.JS_UniqueConsignRef}</Key></DataTarget>
		</DataTargetCollection>
	</DataContext>
</ShipmentRequest>
</UniversalShipmentRequest>";
			using (var adaptor = new eAdaptorController { Request = new HttpRequestMessage { Content = new StringContent(requestContent) } })
			using (var response = adaptor.Post())
			{
				AssertContains("<JobCosting>", response.Content.ReadAsStringAsync().Result);
				response.Content.Dispose();
			}
		}

		public void TestProcessEndToEnd_FailOn_NoContext_OnResponse()
		{
			var enterpriseID = "EDI";
			var serverID = "DAT";
			var dataContextCompanyCode = "EDI";
			var dataTargetType = "ForwardingShipment";
			var jobCostingBranchCode = "BHX";
			var departmentCode = "FEA";
			var chargeLineChargeCode = "BAF";
			var costOSAmount = "120";
			var currencyCode = "AUD";
			var creditorType = "Organization";
			var creditorKey = "AALSHI";
			var chargeLineDescription = "FREIGHT";
			var chargeLineDisplaySequence = "1";
			var importMetadataInstruction = "Insert";

			HttpResponseMessage httpResponse = null;

			var requestContent =
$@"<UniversalShipment version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Shipment>
		<DataContext>
			<EnterpriseID>{enterpriseID}</EnterpriseID>
			<ServerID>{serverID}</ServerID>
			<Company><Code>{dataContextCompanyCode}</Code></Company>
			<DataTargetCollection>
				<DataTarget><Type>{dataTargetType}</Type></DataTarget>
			</DataTargetCollection>
		</DataContext>
		<JobCosting>
			<Branch><Code>{jobCostingBranchCode}</Code></Branch>
			<Department><Code>{departmentCode}</Code></Department>
			<ChargeLineCollection>
				<ChargeLine>
					<ChargeCode><Code>{chargeLineChargeCode}</Code></ChargeCode>
					<CostOSAmount>{costOSAmount}</CostOSAmount>
					<CostOSCurrency><Code>{currencyCode}</Code></CostOSCurrency>
					<Creditor><Type>{creditorType}</Type><Key>{creditorKey}</Key></Creditor>
					<Description>{chargeLineDescription}</Description>
					<DisplaySequence>{chargeLineDisplaySequence}</DisplaySequence>
					<ImportMetaData><Instruction>{importMetadataInstruction}</Instruction></ImportMetaData>
				</ChargeLine>
			</ChargeLineCollection>
		</JobCosting>
	</Shipment>
</UniversalShipment>";
			using (var adaptor = new eAdaptorController())
			{
				adaptor.Request = new HttpRequestMessage() { Content = new StringContent(requestContent) };

				httpResponse = adaptor.Post();
			}

			var xmlResponse = System.Xml.Linq.XDocument.Parse(httpResponse.Content.ReadAsStringAsync().Result);
			httpResponse.Dispose();
			httpResponse = null;

			var responseElementCount = xmlResponse.Document.Elements().Count(e => e.Name.LocalName == "UniversalResponse");
			var statusElementValue = xmlResponse.Descendants().Where(e => e.Name.LocalName == "Status")?.FirstOrDefault()?.Value;
			var dataContextElement = xmlResponse.Descendants().Where(e => e.Name.LocalName == "DataContext")?.FirstOrDefault();
			var dataSourceType = dataContextElement?.Descendants().Where(e => e.Name.LocalName == "DataSourceCollection")?.FirstOrDefault()?.Descendants().FirstOrDefault(dc => dc.Name.LocalName == "Type" && !string.IsNullOrEmpty(dc.Value))?.Value;
			var dataSourceKey = dataContextElement?.Descendants().Where(e => e.Name.LocalName == "DataSourceCollection")?.FirstOrDefault()?.Descendants().FirstOrDefault(dc => dc.Name.LocalName == "Key" && !string.IsNullOrEmpty(dc.Value))?.Value;
			var failureElement = xmlResponse.Document.Elements().FirstOrDefault(e => e.Name.LocalName == "UniversalResponse")?.Descendants().FirstOrDefault(ur => ur.Name.LocalName == "Data")?.Descendants().FirstOrDefault(ue => ue.Name.LocalName == "Event")?.Descendants().FirstOrDefault(ev => ev.Name.LocalName == "ContextCollection")?.Descendants().FirstOrDefault(f => f.Value.Contains("Error") && f.Name.LocalName == "Value");
			var failureReason = failureElement != null ? ": " + failureElement.Value : string.Empty;

			AssertEquals("eAdaptor: Response not received.", 1, responseElementCount);
			AssertEquals("eAdaptor: Status value not equal to PRS.", "PRS", statusElementValue);
			AssertNotNull("eAdaptor: Data Context missing in response.", dataContextElement);
			AssertNotEquals("eAdaptor: Data Source type is empty.", true, string.IsNullOrEmpty(dataSourceType));
			AssertNull("eAdaptor: There was a failure in the request" + failureReason, failureElement);
			AssertNotEquals("eAdaptor: Data Source key is not present.", true, string.IsNullOrEmpty(dataSourceKey));
		}

		public void TestProcessEndToEnd_FailOn_NoMatchingOrg_ResponseContainsWarning()
		{
			var inboundXml = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>CustomsCommercialInvoice</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <CommercialInfo>
      <CommercialInvoiceCollection>
      <CommercialInvoice>
        <Buyer>
          <AddressType>Buyer</AddressType>
          <OrganizationCode>MECBRAMEL</OrganizationCode>
        </Buyer>
        <Supplier>
          <AddressType>Supplier</AddressType>
          <OrganizationCode>INTCOSSHA1</OrganizationCode>
        </Supplier>
        <CommercialInvoiceLineCollection>
          <CommercialInvoiceLine>
            <LineNo>1</LineNo>
            <PartNo>12345678</PartNo>
          </CommercialInvoiceLine>
        </CommercialInvoiceLineCollection>
      </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
  </Shipment>
</UniversalShipment>
";
			string responseBody = null;

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			var messagesQuery = new ZQuery();
			messagesQuery.OrderBy = EDIMessageSchema.EM_ApplicationCode.Name + ", " + EDIMessageSchema.EM_ReceiveTransmit.Name;
			var message = Factory.LoadTop1<IEDIMessage>(messagesQuery);
			AssertNotNull("message reloaded", message);
			AssertEquals("Right Message Type - App|Dir|Typ|Status", "UDQ RCV XUS DCD", message.EM_ApplicationCode + " " + message.EM_ReceiveTransmit + " " + message.EM_MessageSubType + " " + message.EM_Status);

			AssertXMLEquals("responseBody", $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>
  <Data><UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <Company>
        <Code>DEM</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Demo Company</Name>
      </Company>
      <DataProvider>EDIDATDEM</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <EventType>DIF</EventType>

    <ContextCollection>
      <Context>
        <Type>FailureReason</Type>
        <Value>No Module used this Universal Shipment data.</Value>
      </Context>
      <Context>
        <Type>ProcessingStatusCode</Type>
        <Value>DCD</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
</Data>
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{message.EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Warning - Matching 'Buyer':- No match found for '[Org. Code: MECBRAMEL]'.
Warning - Matching 'Supplier':- No match found for '[Org. Code: INTCOSSHA1]'.
Error - Cannot populate BaseJobComInvoiceHeader because:
Supplier is required for Standalone Commercial Invoice; no valid Supplier was found.
Buyer is required for Standalone Commercial Invoice; no valid Buyer was found.
InvoiceNumber must not be empty.
No Module used this Universal Shipment data.
Message Discarded.</ProcessingLog>
</UniversalResponse>
", responseBody);
		}

		public void TestProcessEndToEnd()
		{
			AssertNoExistingMessages();

			var inboundXml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

			string responseBody = null;

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertStartsWith("Response Message",
				@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>", responseBody);

			AssertContains(
				@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S00001000</Key>
        </DataSource>
      </DataSourceCollection>", responseBody);

			AssertContains("<EventType>DIM</EventType>", responseBody);
			AssertNotContains("<AttachedDocumentCollection>", responseBody);

			var query = new ZQuery(JobShipmentSchema.JS_HouseBill, "SHOWMETHEWUGGETS");
			var reloadedShipment = Factory.LoadTop1<Forwarding.IForwardingShipment>(query);
			AssertEquals("SHOWMETHEWUGGETS", reloadedShipment.JS_HouseBill);
			AssertEquals("WINNER WINNER CHICKEN DINNER", reloadedShipment.JS_GoodsDescription);

			var messagesQuery = new ZQuery();
			messagesQuery.OrderBy = EDIMessageSchema.EM_ApplicationCode.Name + ", " + EDIMessageSchema.EM_ReceiveTransmit.Name;
			var messages = Factory.Load<IEDIMessage>(messagesQuery);
			AssertNotNull("messages reloaded", messages);
			AssertMultilineASCIIEquals("App|Dir|Typ|Status", @"
UDQ RCV XUS PRS
UDQ TRX XUR SNT
".Trim(), string.Join("\r\n", messages.Select(message => message.EM_ApplicationCode + " " + message.EM_ReceiveTransmit + " " + message.EM_MessageSubType + " " + message.EM_Status)));
		}

		public void TestProcessEndToEnd_USImporterSecurityFiling()
		{
			AssertNoExistingMessages();
			var inboundXml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>USImporterSecurityFiling</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <Branch>
      <Code>BNE</Code>
    </Branch>
    <CommercialInfo>
      <CommercialInvoiceCollection>
        <CommercialInvoice>
          <CommercialInvoiceLineCollection>
            <CommercialInvoiceLine>
              <LineNo>0</LineNo>
              <CountryOfOrigin>
                <Code>IN</Code>
              </CountryOfOrigin>
              <HarmonisedCode>330749</HarmonisedCode>
              <OrganizationAddressCollection>
                <OrganizationAddress>
                  <AddressType>Manufacturer</AddressType>
                  <Address1>PLOT NO. 199 &amp; 200 BAIKAMPADY</Address1>
                  <AddressOverride>true</AddressOverride>
                  <AddressShortCode/>
                  <City>MANGALORE</City>
                  <CompanyName>PRIMACY INDUSTRIES LTD.</CompanyName>
                  <Country>
                    <Code>IN</Code>
                  </Country>
                </OrganizationAddress>
              </OrganizationAddressCollection>
            </CommercialInvoiceLine>
          </CommercialInvoiceLineCollection>
        </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
    <CustomsContainerMode>
      <Code>CNT</Code>
    </CustomsContainerMode>
    <TransportMode>
      <Code>SEA</Code>
    </TransportMode>
    <WayBillNumber>CHSL328807714BLR</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
    </WayBillType>
  </Shipment>
</UniversalShipment>";
			string responseBody = null;
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}
			AssertNotNull(responseBody);
			AssertContains("Successfully saved ISF0000001 (HouseBill='CHSL328807714BLR')", responseBody);
		}

		public void TestInvalidJobChargeData()
		{
			var inboundXml =
				$@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Shipment>
     <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>EDI</Code>
        <Name>Eagle Datamation International</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <ContainerMode>
      <Code>FCL</Code>
      <Description>Full Container Load</Description>
    </ContainerMode>
    <GoodsValueCurrency>
      <Code>AUD</Code>
      <Description>Australia, Dollars</Description>
    </GoodsValueCurrency>
    <HBLAWBChargesDisplay>
      <Code>SHW</Code>
      <Description>Show Collect Charges</Description>
    </HBLAWBChargesDisplay>
    <InsuranceValueCurrency>
      <Code>AUD</Code>
      <Description>Australia, Dollars</Description>
    </InsuranceValueCurrency>
    <JobCosting>
      <AccrualNotRecognized>0.0000</AccrualNotRecognized>
      <AccrualRecognized>-124.0000</AccrualRecognized>
      <AgentRevenue>0</AgentRevenue>
      <Branch>
        <Code>SYD</Code>
        <Name>EDIHQ</Name>
      </Branch>
      <Currency>
        <Code>AUD</Code>
        <Description>Australia, Dollars</Description>
      </Currency>
      <LocalClientRevenue>0</LocalClientRevenue>
      <OperationsStaff>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </OperationsStaff>
      <OtherDebtorRevenue>0</OtherDebtorRevenue>
      <TotalAccrual>-124.0000</TotalAccrual>
      <TotalCost>0.0000</TotalCost>
      <TotalJobProfit>0.0000</TotalJobProfit>
      <TotalRevenue>0.0000</TotalRevenue>
      <TotalWIP>124.0000</TotalWIP>
      <WIPNotRecognized>0.0000</WIPNotRecognized>
      <WIPRecognized>124.0000</WIPRecognized>

      <ChargeLineCollection>
        <ChargeLine>
          <Branch>
            <Code>SYD</Code>
            <Name>EDIHQ</Name>
          </Branch>
          <ChargeCode>
            <Code>CFSCUST</Code>
            <Description>CFS Customs Hold</Description>
          </ChargeCode>
          <ChargeCodeGroup>
            <Code>CSH</Code>
            <Description>CFS Shipment</Description>
          </ChargeCodeGroup>
          <CostOSCurrency>
            <Code>AUD</Code>
            <Description>Australia, Dollars</Description>
          </CostOSCurrency>
          <Creditor>
            <Type>Organization</Type>
            <Key>WWS</Key>
          </Creditor>
          <Debtor>
            <Type>Organization</Type>
            <Key>ACAINT</Key>
          </Debtor>
          <Department>
            <Code>FIS</Code>
            <Name>Forwarding Import Sea</Name>
          </Department>
          <Description>CFS Customs Hold</Description>
					<ImportMetaData>
                <Instruction>UpdateAndInsertIfNotFound</Instruction>
                <MatchingCriteriaCollection>
                  <MatchingCriteria>
                    <FieldName>ChargeCode</FieldName>
                    <Value>CFSCUST</Value>
                  </MatchingCriteria>
                  <MatchingCriteria>
                    <FieldName>SellOSCurrency</FieldName>
                    <Value>AUD</Value>
                  </MatchingCriteria>
                </MatchingCriteriaCollection>
          </ImportMetaData>
        </ChargeLine>
      </ChargeLineCollection>
    </JobCosting>
    <NoCopyBills>3</NoCopyBills>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <PortOfDestination>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfDestination>
    <PortOfOrigin>
      <Code>USCHI</Code>
      <Name>Chicago</Name>
    </PortOfOrigin>
    <ReleaseType>
      <Code>SWB</Code>
      <Description>Sea Waybill</Description>
    </ReleaseType>
    <ScreeningStatus>
      <Code>UNK</Code>
      <Description>Unknown</Description>
    </ScreeningStatus>
    <ServiceLevel>
      <Code>STD</Code>
      <Description>Standard</Description>
    </ServiceLevel>
    <ShipmentIncoTerm>
      <Code>FOB</Code>
      <Description>Free On Board</Description>
    </ShipmentIncoTerm>
    <ShipmentType>
      <Code>STD</Code>
      <Description>Standard House</Description>
    </ShipmentType>
    <ShippedOnBoard>
      <Code>SHP</Code>
      <Description>Shipped</Description>
    </ShippedOnBoard>
    <ShipperCODAmount>0.0000</ShipperCODAmount>
    <ShipperCODPayMethod>
      <Code></Code>
    </ShipperCODPayMethod>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>CTN</Code>
      <Description>Carton</Description>
    </TotalNoOfPacksPackageType>
    <TotalVolume>0.000</TotalVolume>
    <TotalVolumeUnit>
      <Code>M3</Code>
      <Description>Cubic Meters</Description>
    </TotalVolumeUnit>
    <TotalWeight>0.000</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TranshipToOtherCFS>false</TranshipToOtherCFS>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WarehouseLocation></WarehouseLocation>
    <WayBillNumber>AD1232138</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";
			string responseBody = null;

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			using (Factory.AddDisposableService())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains("Error - Whilst importing Charge Line: Job Number", responseBody);
			AssertContains("Error - Creditor: Enter a valid Creditor", responseBody);
			AssertNotContains("Log levels should not be duplicated", "Error - Error", responseBody);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestOrganisation_JobRequiredDocumentConcurrencyChecker()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = "ORG000";
			var doc1 = organisation.RequiredDocuments.AddNew();
			doc1.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			doc1.EQ_DocType = Core.Constants.RefDocTypes.AgentsInvoice;
			Factory.Save();

			var inboundXml = $@"<UniversalShipment version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <Shipment>
        <DataContext>
            <Company>
                <Code>EDI</Code>
            </Company>
            <EnterpriseID>EDI</EnterpriseID>
            <ServerID>DAT</ServerID>
            <DataProvider>EDIDATDEM</DataProvider>
            <CodesMappedToTarget>true</CodesMappedToTarget>
            <DataTargetCollection>
                <DataTarget>
                    <Type>CustomsCommercialInvoice</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <CommercialInfo>
            <CommercialInvoiceCollection>
                <CommercialInvoice>
                    <InvoiceNumber>8200008010l-test13</InvoiceNumber>
                    <InvoiceAmount>44710.6</InvoiceAmount>
                    <OrganizationAddressCollection>
                        <OrganizationAddress>
                            <AddressType>Importer</AddressType>
                            <OrganizationCode>{organisation.OH_Code}</OrganizationCode>
                        </OrganizationAddress>
                    </OrganizationAddressCollection>
                    <Supplier>
                        <OrganizationCode>{organisation.OH_Code}</OrganizationCode>
                        <AddressType>Supplier</AddressType>
                    </Supplier>
                    <InvoiceDate>2021-04-14</InvoiceDate>
                    <IncoTerm>
                        <Code>CIP</Code>
                    </IncoTerm>
                    <InvoiceCurrency>
                        <Code>GBP</Code>
                    </InvoiceCurrency>
                    <CommercialInvoiceLineCollection>
                        <CommercialInvoiceLine>
                            <LineNo>1</LineNo>
                            <Description>6000001457 39079110 20.16.40 Price VAT Gross price POLIMAL 143 AWTP - 2 IBC NEW</Description>
                            <CountryOfOrigin>
                                <Code>PL</Code>
                            </CountryOfOrigin>
                            <LinePrice>21120</LinePrice>
                            <InvoiceQuantityUnit>
                                <Code>KG</Code>
                            </InvoiceQuantityUnit>
                            <InvoiceQuantity>11000</InvoiceQuantity>
                        </CommercialInvoiceLine>
                        <CommercialInvoiceLine>
                            <LineNo>2</LineNo>
                            <Description>6000000274 39079110 Price VAT Gross price POLIMAL 143 AWTP - 2 a220kg</Description>
                            <CountryOfOrigin>
                                <Code>PL</Code>
                            </CountryOfOrigin>
                            <LinePrice>1707.2</LinePrice>
                            <InvoiceQuantityUnit>
                                <Code>KG</Code>
                            </InvoiceQuantityUnit>
                            <InvoiceQuantity>880</InvoiceQuantity>
                        </CommercialInvoiceLine>
                        <CommercialInvoiceLine>
                            <LineNo>3</LineNo>
                            <Description>6000000132 39079110 20.16.40 Price VAT Gross price POLIMAL 1094 AWTP - 1 a220kg</Description>
                            <CountryOfOrigin>
                                <Code>PL</Code>
                            </CountryOfOrigin>
                            <LinePrice>1812.8</LinePrice>
                            <InvoiceQuantityUnit>
                                <Code>KG</Code>
                            </InvoiceQuantityUnit>
                            <InvoiceQuantity>880</InvoiceQuantity>
                        </CommercialInvoiceLine>
                        <CommercialInvoiceLine>
                            <LineNo>4</LineNo>
                            <Description>6000000251 39079110 20.16.40 Price VAT Gross price POLIMAL 143 AWTP - 2 S IBC NEW</Description>
                            <CountryOfOrigin>
                                <Code>PL</Code>
                            </CountryOfOrigin>
                            <LinePrice>11220</LinePrice>
                            <InvoiceQuantityUnit>
                                <Code>KG</Code>
                            </InvoiceQuantityUnit>
                            <InvoiceQuantity>5500</InvoiceQuantity>
                        </CommercialInvoiceLine>
                        <CommercialInvoiceLine>
                            <LineNo>5</LineNo>
                            <Description>6000000517 39079110 POLIMAL 122-2 AWTP a220kg</Description>
                            <CountryOfOrigin>
                                <Code>PL</Code>
                            </CountryOfOrigin>
                            <LinePrice>1359.6</LinePrice>
                            <InvoiceQuantityUnit>
                                <Code>KG</Code>
                            </InvoiceQuantityUnit>
                            <InvoiceQuantity>660</InvoiceQuantity>
                        </CommercialInvoiceLine>
                        <CommercialInvoiceLine>
                            <LineNo>6</LineNo>
                            <Description>16000001455 39079110 POLIMAL 1094 AWTP- 1 IBC NEW</Description>
                            <CountryOfOrigin>
                                <Code>PL</Code>
                            </CountryOfOrigin>
                            <LinePrice>7491</LinePrice>
                            <InvoiceQuantityUnit>
                                <Code>KG</Code>
                            </InvoiceQuantityUnit>
                            <InvoiceQuantity>3300</InvoiceQuantity>
                        </CommercialInvoiceLine>
                    </CommercialInvoiceLineCollection>
                </CommercialInvoice>
            </CommercialInvoiceCollection>
        </CommercialInfo>
    </Shipment>
</UniversalShipment>";
			string responseBody = null;

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			using (Factory.AddDisposableService())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				AssertNoExceptionThrown(() =>
				{
					using (var response = controller.Post())
					{
						responseBody = response.Content.ReadAsStringAsync().Result;
					}
				});
			}

			AssertContains(@"<Context>
        <Type>ProcessingStatusCode</Type>
        <Value>PRS</Value>
      </Context>", responseBody);
			AssertContains("Successfully saved Invoice", responseBody);
		}

		public void TestTriggeringEventSourceInfoItemsIsAccessibleInMacro()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;
			shipment.JS_UniqueConsignRef = "S00001000";

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<JS_GoodsDescription>";
			action.PQ_FieldValue = "<TriggeringEvent.SourceInfoItems.Find(\"{Key}\" == \"Data Source Company\").Data>";

			Factory.Save();

			var requestXml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001000</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>Z01</EventType>
  </Event>
</UniversalEvent>";

			sender.SendRequest(requestXml);

			Factory.Save();

			MasterFilesTestHelper.RunLogWalker();

			((BusinessObject)shipment).Reload();

			AssertEquals("EDI", shipment.JS_GoodsDescription);
		}

		public void TestMacroErrorsAreNotLoggedToConsol()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;
			shipment.JS_UniqueConsignRef = "S00001000";

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
			action.PQ_FieldName = "<JS_GoodsDescription>";
			action.PQ_FieldValue = "<WorkflowItems.Find(\"{P9_Description}\"==\\n\"Import\").P9_Status>";// \\n makes this macro invalid

			Factory.Save();

			var requestXml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001000</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>Z01</EventType>
  </Event>
</UniversalEvent>";

			var consolError = Console.Error;
			try
			{
				using (var writer = new StringWriter())
				{
					Console.SetError(writer);
					sender.SendRequest(requestXml);
					AssertEquals("Expecting no logging to console", "", writer.ToString());
				}
			}
			finally
			{
				Console.SetError(consolError);
			}
		}

		public void TestHowManyEDINumbersDoWeAllocate()
		{
			var usCompany = (BusinessObject)Factory.New<IGlbCompany>();
			usCompany[GlbCompanySchema.GC_Code.Name] = "PUS";
			usCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "US";

			var usBranch = (BusinessObject)Factory.New<IGlbBranch>();
			usBranch[GlbBranchSchema.GB_Code.Name] = "PUS";
			usBranch[GlbBranchSchema.GB_GC.Name] = usCompany.PK;

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";

			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var inboundXml = string.Format(CultureInfo.InvariantCulture,
	@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SS00011826</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>PUS</Code>
      </Company>
      <EnterpriseID>{0}</EnterpriseID>
      <ServerID>{1}</ServerID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>", registrationKey.EnterpriseCode, registrationKey.ServerCode);

			var numbersAllocated = Env.NumberFountains.HttpXmlEDIMessageNumber.GetNext(Db.Connection);
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				controller.Post().Dispose();
			}

			numbersAllocated++; // Incrementing because I just allocated another number
			AssertEquals(numbersAllocated + Factory.Load<IEDIMessage>(new ZQuery()).Length, Env.NumberFountains.HttpXmlEDIMessageNumber.GetNext(Db.Connection));
		}

		public void TesteAdaptorHttpXMLShipmentRequestDoesNotApplyWorkflowTemplate()
		{
			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;
			ukBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";

			var usCompany = (BusinessObject)Factory.New<IGlbCompany>();
			usCompany[GlbCompanySchema.GC_Code.Name] = "PUS";
			usCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "US";

			var usBranch = (BusinessObject)Factory.New<IGlbBranch>();
			usBranch[GlbBranchSchema.GB_Code.Name] = "PUS";
			usBranch[GlbBranchSchema.GB_GC.Name] = usCompany.PK;

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";

			Factory.Save();

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Test Template";
			template.P0_ProcessType = "SHP";
			template.P0_GC = ukCompany.PK;
			template.P0_GB = ukBranch.PK;

			var trigger = template.WorkflowItems.Milestones.AddNew();
			trigger.P9_Description = "Test Trigger";
			trigger.P9_SE_NKMilestoneEvent = "Z00";

			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var inboundXml = string.Format(CultureInfo.InvariantCulture,
	@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SS00011826</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>PUS</Code>
      </Company>
      <EnterpriseID>{0}</EnterpriseID>
      <ServerID>{1}</ServerID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>", registrationKey.EnterpriseCode, registrationKey.ServerCode);

			DataRegistry.Instance.WebBranch = ukBranch.PK.ToGuid();
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				controller.Post().Dispose();
			}

			var factoryCheck = Factory.CreateNewFactory();
			factoryCheck.RefreshEnabled = false;

			var shipmentCheck = factoryCheck.Load<Forwarding.IForwardingShipment>(shipment.PK);
			AssertEquals(0, ((IWorkflowProvider)shipmentCheck).WorkflowItems.Milestones.Count);
		}

		public void TestSaveFailsBecauseDatabasesArentReliable()
		{
			var inboundXml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

			var saveCount = 0;
			var row = (Factory.New<DummyBusinessObject>() as INeedRow).Row;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				saveCount++;
				if (saveCount == 2)
				{
					// Throw a transient exception on every second save.
					throw new ZSaveException(new ZDataException(new TransactionException("Transactions can never be trusted!!!!", OdysseyDataErrorType.TransactionRolledBack), row, Db.Connection), Factory);
				}
			});

			var responseBody = sender.SendRequest(inboundXml).result;

			AssertContains("Successfully saved Shipment", responseBody);
		}

		public void TestHandleUserContextLostException()
		{
			var inboundXml =
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

			var row = (Factory.New<DummyBusinessObject>() as INeedRow).Row;
			var userContextFailureMessage = "";

			try
			{
				BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
				{
					Env.Instance.UserContextChanging -= OnUserContextChanging;
					Env.Instance.UserContextChanging += OnUserContextChanging;
				});

				AssertNullOrEmpty(userContextFailureMessage);

				var exception = AssertExceptionThrown<UserContextLostException>(() => eAdaptorTestHelper.PostRequest(inboundXml));
				AssertContains("Test SQL Exception", exception.InnerException.Message);

				Env.Instance.UserContextManagerForTesting.ClearCurrentThread();
			}
			finally
			{
				Env.Instance.UserContextChanging -= OnUserContextChanging;
			}

			void OnUserContextChanging(object sender, IUserContextChangingEventArgs e)
			{
				if (e.IsRevert)
				{
					// should revert back from CWAutoDataImport to CWWeb
					if (e.OldUserContext.User.LoginName != "CWAutoDataImport" && e.NewUserContext.User.LoginName != "CWWeb")
					{
						userContextFailureMessage = string.Format(CultureInfo.InvariantCulture, "Unexpected context switch: old value is '{0}', new value is '{1}'.", e.OldUserContext.User.LoginName, e.NewUserContext.User.LoginName);
					}
					var error = SqlExceptionBuilder.CreateSqlError(942, 1, 1, "", "Test SQL Exception should have been caught", "", 1);
					var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
					var sqlException = SqlExceptionBuilder.CreateSqlException(errors);
					throw sqlException;
				}
			}
		}

		public void TestDatabaseLockRequestTimeoutExceptionHandling()
		{
			var inboundXml =
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
			var expectedErorrMessage = "";
			var row = (Factory.New<DummyBusinessObject>() as INeedRow).Row;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				saveCount++;
				if (saveCount >= 3)
				{
					var sqlException = SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(1222, byte.MaxValue, byte.MinValue, Core.Constants.ProductName, "Lock request time out period exceeded.", "", 1)
					));

					var exception = new ZSaveException(new ZDataException(sqlException, row, Db.Connection), Factory);
					expectedErorrMessage = new DbErrorMatch(sqlException).GetUserFriendlyMessage(Db.Connection);
					throw exception;
				}
			});
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					var responseBody = response.Content.ReadAsStringAsync().Result;
					AssertEquals("503 Service Unavailable", HttpStatusCode.ServiceUnavailable, response.StatusCode);
					AssertContains(expectedErorrMessage, responseBody);
				}
			}
		}

		public void TestDatabaseExecutionTimeoutExpiredExceptionHandling()
		{
			var inboundXml =
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

			var expectedErorrMessage = "";
			var row = (Factory.New<DummyBusinessObject>() as INeedRow).Row;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				var winException = new Win32Exception(258, "The wait operation timed out");
				var error = SqlExceptionBuilder.CreateSqlError(-2, 1, 1, "", "Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 1);
				var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				var sqlException = SqlExceptionBuilder.CreateSqlException(errorCollection, winException);
				expectedErorrMessage = new DbErrorMatch(sqlException).GetUserFriendlyMessage(TestConnection);

				throw sqlException;
			});

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					var responseBody = response.Content.ReadAsStringAsync().Result;
					AssertEquals("503 Service Unavailable", HttpStatusCode.ServiceUnavailable, response.StatusCode);
					AssertContains(expectedErorrMessage, responseBody);
				}
			}
		}

		public void TestDatabaseLockRequestTimeoutSqlExceptionHandlingShouldRetry()
		{
			var inboundXml =
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
			var retryCount = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (retryCount < 2 && f.NameForDebugging == "Publish Universal Xml Internally")
				{
					retryCount++;
					var sqlException = SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(1222, byte.MaxValue, byte.MinValue, Core.Constants.ProductName, "Lock request time out period exceeded.", "", 1)
					));

					throw sqlException;
				}
			});
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					var responseBody = response.Content.ReadAsStringAsync().Result;
					AssertContains("Successfully saved Shipment", responseBody);
				}
			}
		}

		public void TestDatabaseDeadlockExceptionHandling()
		{
			var inboundXml =
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
			string responseBody = null;
			var saveCount = 0;
			var row = (Factory.New<DummyBusinessObject>() as INeedRow).Row;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				saveCount++;
				if (saveCount >= 3)
				{
					var sqlException = SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(1205, byte.MaxValue, byte.MinValue, Core.Constants.ProductName, "Transaction (Process ID 1404) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.", "", 1)
					));

					var exception = new ZSaveException(new ZDataException(sqlException, row, Db.Connection), Factory);
					throw exception;
				}
			});
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
					AssertEquals("500 Internal Server Error", HttpStatusCode.InternalServerError, response.StatusCode);
					AssertContains("A save exception has occurred due to deadlock with another operation. Please try again.", responseBody);
				}
			}
		}

		public void TestNoConnectionToDbExceptionHandled()
		{
			var inboundXml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				//throw every time we try to save
				throw new InvalidOperationException("The transaction no longer has an active connection.");
			});
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					var responseBody = response.Content.ReadAsStringAsync().Result;
					AssertContains("This service is temporarily unavailable.", responseBody);
					AssertEquals("503 Error", HttpStatusCode.ServiceUnavailable, response.StatusCode);
				}
			}
		}

		public void TestIncomingXmlHasInvalidChars()
		{
			var inboundXml = @"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001111</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
  </Shipment>
</UniversalShipment>";

			inboundXml = inboundXml.Replace("S00001111", "S00001111\0");
			var responseBody = sender.SendRequest(inboundXml).result;

			AssertContains("Error - Line 7: Hexadecimal value 0x00 is an invalid XML character.", responseBody);
		}

		public void TestImportingEDocs_RecoverableZSaveException_MessageIsFailedWhenRetryFails()
		{
			var bizo = Factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			Factory.Save();

			var messageResponse = ImportEdoc_AndThrowZSaveException(Factory, saveNumber: 2, recover: false);

			AssertContains("UniversalXmlWorkflowProcessor.SendUniversalXmlInternally (saving message)", @"A write was rolled back by the database.", messageResponse);

			var relaodBizo = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(bizo.PK);
			var savedEDocs = ((IDocManagerSupport)relaodBizo).DocManagerInfo.Files.Count;
			AssertEquals("Edocs failed to save", 0, savedEDocs);
		}

		internal static string ImportEdoc_AndThrowZSaveException(BusinessObjectFactory factory, int saveNumber, bool recover = true, bool returnSaveCount = false)
		{
			string inboundXml = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>DDI</EventType>
    <AttachedDocumentCollection>
      <AttachedDocument>
        <FileName>CIV-Commercial_Invoice-DOUCMENT_NUMBER_{saveNumber}.pdf</FileName>
        <Type>
          <Code>PDF</Code>
          <Description>Uploaded from Hubtran.</Description>
        </Type>
        <ImageData>U2lubmUgRmlhbm5hIEbDoWlsLA0KYXTDoSBmYW9pIGdoZWFsbCBhZyDDiWlyaW5uLA0KQnXDrW9uIGTDoXIgc2x1YQ0KdGhhciB0b2lubiBkbyByw6FpbmlnIGNodWdhaW5uLA0KRmFvaSBtaMOzaWQgYmhlaXRoIHNhb3INClNlYW50w61yIMOhciBzaW5zZWFyIGZlYXN0YSwNCk7DrSBmaMOhZ2ZhciBmYW9pbiB0w61vcsOhbiBuw6EgZmFvaW4gdHLDoWlsbC4NCkFub2NodCBhIHRow6lhbSBzYSBiaGVhcm5hIGJoYW9pbCwNCkxlIGdlYW4gYXIgR2hhZWlsLCBjaHVuIGLDoWlzIG7DsyBzYW9pbCwNCkxlIGd1bm5hLXNjcsOpYWNoIGZhb2kgbMOhbWhhY2ggbmEgYnBpbMOpYXIsDQpTZW8gbGliaCBjYW5haWcgYW1ocsOhbiBuYSBiaEZpYW5uLg==</ImageData>
        <IsPublished>true</IsPublished>
      </AttachedDocument>
    </AttachedDocumentCollection>
  </Event>
</UniversalEvent>";
			string responseBody = null;
			var saveCount = 0;
			var row = (factory.New<DummyBusinessObject>() as INeedRow).Row;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				saveCount++;
				if (recover ? saveCount == saveNumber : (saveCount >= saveNumber || saveCount <= saveNumber + 3)) // 3 retries in UniversalXmlImportHandler
				{
					var recoverableException = new ZDataException(new TransactionException("", OdysseyDataErrorType.TransactionRolledBack), row, Db.Connection);
					recoverableException.SetFriendlyMessageForTest("I can recover with on retry");
					throw new ZSaveException(recoverableException, factory);
				}
			});
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}
			return returnSaveCount ? saveCount.ToString() : responseBody;
		}

		public void TesteAdaptorHttpXMLLargeConsolRequestClosesTags()
		{
			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;
			ukBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";

			var usCompany = (BusinessObject)Factory.New<IGlbCompany>();
			usCompany[GlbCompanySchema.GC_Code.Name] = "PUS";
			usCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "US";

			var usBranch = (BusinessObject)Factory.New<IGlbBranch>();
			usBranch[GlbBranchSchema.GB_Code.Name] = "PUS";
			usBranch[GlbBranchSchema.GB_GC.Name] = usCompany.PK;

			var consol = Factory.New<Forwarding.IForwardingConsol>();
			((BusinessObject)consol).FillWithValidTestData();
			consol.JK_UniqueConsignRef = "C12345678";

			for (int i = 0; i < 500; ++i)
			{
				var shipment = Factory.New<Forwarding.IForwardingShipment>();
				shipment.JS_HouseBill = $"SS0001182{i}";
				shipment.JS_UniqueConsignRef = $"SS00011826{i}";
				consol.AddShipment(shipment);
			}

			Factory.Save();

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Test Template";
			template.P0_ProcessType = "SHP";
			template.P0_GC = ukCompany.PK;
			template.P0_GB = ukBranch.PK;

			var trigger = template.WorkflowItems.Milestones.AddNew();
			trigger.P9_Description = "Test Trigger";
			trigger.P9_SE_NKMilestoneEvent = "Z00";

			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var inboundXml = string.Format(CultureInfo.InvariantCulture,
	@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingConsol</Type>
          <Key>C12345678</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>PUS</Code>
      </Company>
      <EnterpriseID>{0}</EnterpriseID>
      <ServerID>{1}</ServerID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>", registrationKey.EnterpriseCode, registrationKey.ServerCode);

			DataRegistry.Instance.WebBranch = ukBranch.PK.ToGuid();
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					var responseBody = response.Content.ReadAsStringAsync().Result;
					AssertContains("</Data>", responseBody);
				}
			}

			var factoryCheck = Factory.CreateNewFactory();
			factoryCheck.RefreshEnabled = false;
		}

		public void TestUniversalXmlRequestBaseHandlerShouldNotErrorReportOnContextSwitching()
		{
			var webCompany = (BusinessObject)Factory.New<IGlbCompany>();
			webCompany[GlbCompanySchema.GC_Code.Name] = "WWW";

			var webBranch = (BusinessObject)Factory.New<IGlbBranch>();
			webBranch[GlbBranchSchema.GB_Code.Name] = "WWW";
			webBranch[GlbBranchSchema.GB_GC.Name] = webCompany.PK;
			webBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";

			var company = (BusinessObject)Factory.New<IGlbCompany>();
			company[GlbCompanySchema.GC_Code.Name] = "BBB";

			var branch = (BusinessObject)Factory.New<IGlbBranch>();
			branch[GlbBranchSchema.GB_Code.Name] = "BBB";
			branch[GlbBranchSchema.GB_GC.Name] = company.PK;

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS0001000";
			shipment.JS_UniqueConsignRef = "SS0001000";

			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var inboundXml = string.Format(CultureInfo.InvariantCulture,
				@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SS0001000</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>BBB</Code>
      </Company>
      <EnterpriseID>{0}</EnterpriseID>
      <ServerID>{1}</ServerID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>", registrationKey.EnterpriseCode, registrationKey.ServerCode);

			DataRegistry.Instance.WebBranch = webBranch.PK.ToGuid();
			using (Env.SetTemporaryUserContext("CWWeb", webBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var responseXml = sender.SendRequest(inboundXml).result;
			}

			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TesteAdaptorHttpXMLProvidesResponseWithDeclarationFromSpecifiedCompanyContext()
		{
			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;

			var usCompany = (BusinessObject)Factory.New<IGlbCompany>();
			usCompany[GlbCompanySchema.GC_Code.Name] = "PUS";
			usCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "US";

			var usBranch = (BusinessObject)Factory.New<IGlbBranch>();
			usBranch[GlbBranchSchema.GB_Code.Name] = "PUS";
			usBranch[GlbBranchSchema.GB_GC.Name] = usCompany.PK;
			ukBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";

			Factory.Save();

			Integration.Customs.IBaseJobDeclaration ukDeclaration = null;
			Integration.Customs.IBaseJobDeclaration usDeclaration = null;
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			{
				ukDeclaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
				ukDeclaration[JobDeclarationSchema.JE_GoodsDescription.Name] = "UK Declaration Goods Description";
				ukDeclaration[JobDeclarationSchema.JE_OverrideFreightDefaults.Name] = true;
				ukDeclaration.JE_JS = shipment.PK;

				Factory.Save();
			}

			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, usBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			{
				usDeclaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
				usDeclaration[JobDeclarationSchema.JE_GoodsDescription.Name] = "US Declaration Goods Description";
				usDeclaration[JobDeclarationSchema.JE_OverrideFreightDefaults.Name] = true;
				usDeclaration.JE_JS = shipment.PK;
				Factory.Save();
			}

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var inboundXml = string.Format(CultureInfo.InvariantCulture,
				@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SS00011826</Key>
        </DataTarget>
      </DataTargetCollection>

      <Company>
        <Code>PUS</Code>
      </Company>
      <EnterpriseID>{0}</EnterpriseID>
      <ServerID>{1}</ServerID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>", registrationKey.EnterpriseCode, registrationKey.ServerCode);
			string responseBody = null;

			DataRegistry.Instance.WebBranch = ukBranch.PK.ToGuid();
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains("<GoodsDescription>US Declaration Goods Description</GoodsDescription>", responseBody);
		}

		public void TesteAdaptorHttpXMLProvidesResponseIfCompanyContextWasNotSpecified()
		{
			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;
			ukBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";

			Factory.Save();

			Integration.Customs.IBaseJobDeclaration ukDeclaration = null;

			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			{
				ukDeclaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
				ukDeclaration[JobDeclarationSchema.JE_GoodsDescription.Name] = "UK Declaration Goods Description";
				ukDeclaration[JobDeclarationSchema.JE_OverrideFreightDefaults.Name] = true;
				ukDeclaration.JE_JS = shipment.PK;
				Factory.Save();
			}

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var inboundXml = string.Format(CultureInfo.InvariantCulture, @"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SS00011826</Key>
        </DataTarget>
      </DataTargetCollection>
      <EnterpriseID>{0}</EnterpriseID>
      <ServerID>{1}</ServerID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>", registrationKey.EnterpriseCode, registrationKey.ServerCode);

			string responseBody = null;

			DataRegistry.Instance.WebBranch = ukBranch.PK.ToGuid();
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains("<GoodsDescription>UK Declaration Goods Description</GoodsDescription>", responseBody);
		}

		public void TesteAdaptorHttpXMLProvidesResponseIfContextWasNotSpecified()
		{
			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;
			ukBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";

			Factory.Save();

			Integration.Customs.IBaseJobDeclaration ukDeclaration = null;

			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			{
				ukDeclaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
				ukDeclaration[JobDeclarationSchema.JE_GoodsDescription.Name] = "UK Declaration Goods Description";
				ukDeclaration[JobDeclarationSchema.JE_OverrideFreightDefaults.Name] = true;
				ukDeclaration.JE_JS = shipment.PK;
				Factory.Save();
			}

			var inboundXml =
				$@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SS00011826</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>";

			string responseBody = null;

			DataRegistry.Instance.WebBranch = ukBranch.PK.ToGuid();
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains("<GoodsDescription>UK Declaration Goods Description</GoodsDescription>", responseBody);
		}

		public void TesteAdaptorHttpXMLProvidesResponseIfDataContextWasOmitted()
		{
			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;
			ukBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";

			Factory.Save();

			Integration.Customs.IBaseJobDeclaration ukDeclaration = null;

			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			{
				ukDeclaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
				ukDeclaration[JobDeclarationSchema.JE_GoodsDescription.Name] = "UK Declaration Goods Description";
				ukDeclaration[JobDeclarationSchema.JE_OverrideFreightDefaults.Name] = true;
				ukDeclaration.JE_JS = shipment.PK;
				Factory.Save();
			}

			var inboundXml =
				$@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
  </ShipmentRequest>
</UniversalShipmentRequest>";

			string responseBody = null;

			DataRegistry.Instance.WebBranch = ukBranch.PK.ToGuid();
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains("There is no business object matching the criteria.", responseBody);
		}

		public void TesteAdaptorHttpXMLReturnsErrorWithExplanationIfEnterpriseIdWasSpecifiedIncorrectly()
		{
			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;
			ukBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";

			Factory.Save();

			Integration.Customs.IBaseJobDeclaration ukDeclaration = null;

			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			{
				ukDeclaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
				ukDeclaration[JobDeclarationSchema.JE_GoodsDescription.Name] = "UK Declaration Goods Description";
				ukDeclaration[JobDeclarationSchema.JE_OverrideFreightDefaults.Name] = true;
				ukDeclaration.JE_JS = shipment.PK;
				Factory.Save();
			}

			var inboundXml =
				$@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SS00011826</Key>
        </DataTarget>
      </DataTargetCollection>
      <EnterpriseID>BOO</EnterpriseID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>";

			string responseBody = null;

			DataRegistry.Instance.WebBranch = ukBranch.PK.ToGuid();
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains("DataContext EnterpriseId was specified, however it does not match the system EnterpriseId.", responseBody);
		}

		public void TesteAdaptorHttpXMLReturnsErrorWithExplanationIfServerIdWasSpecifiedIncorrectly()
		{
			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;
			ukBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";

			Factory.Save();

			Integration.Customs.IBaseJobDeclaration ukDeclaration = null;

			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			{
				ukDeclaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
				ukDeclaration[JobDeclarationSchema.JE_GoodsDescription.Name] = "UK Declaration Goods Description";
				ukDeclaration[JobDeclarationSchema.JE_OverrideFreightDefaults.Name] = true;
				ukDeclaration.JE_JS = shipment.PK;
				Factory.Save();
			}

			var inboundXml =
				$@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SS00011826</Key>
        </DataTarget>
      </DataTargetCollection>
      <ServerID>BOO</ServerID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>";

			string responseBody = null;

			DataRegistry.Instance.WebBranch = ukBranch.PK.ToGuid();
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains("DataContext ServerId was specified, however it does not match the system ServerId.", responseBody);
		}

		public void TesteAdaptorHttpXMLReturnsErrorWithExplanationIfServerIdWasOmittedButEnterpriseIdWithCompanyWasSpecified()
		{
			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;
			ukBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";

			Factory.Save();

			Integration.Customs.IBaseJobDeclaration ukDeclaration = null;

			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			{
				ukDeclaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
				ukDeclaration[JobDeclarationSchema.JE_GoodsDescription.Name] = "UK Declaration Goods Description";
				ukDeclaration[JobDeclarationSchema.JE_OverrideFreightDefaults.Name] = true;
				ukDeclaration.JE_JS = shipment.PK;
				Factory.Save();
			}

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var inboundXml = string.Format(CultureInfo.InvariantCulture, @"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SS00011826</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>PUS</Code>
      </Company>
      <EnterpriseID>{0}</EnterpriseID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>", registrationKey.EnterpriseCode);

			string responseBody = null;

			DataRegistry.Instance.WebBranch = ukBranch.PK.ToGuid();
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains("Company was specified, however EnterpriseId or ServerId were not specified. You should specify EnterpriseId, ServerId and Company if you want to map to a particular Company.", responseBody);
		}

		public void TesteAdaptorHttpXMLReturnsErrorWithExplanationIfEnterpriseIdWasOmittedButServerIdWithCompanyWasSpecified()
		{
			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;
			ukBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";

			Factory.Save();

			Integration.Customs.IBaseJobDeclaration ukDeclaration = null;

			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			{
				ukDeclaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
				ukDeclaration[JobDeclarationSchema.JE_GoodsDescription.Name] = "UK Declaration Goods Description";
				ukDeclaration[JobDeclarationSchema.JE_OverrideFreightDefaults.Name] = true;
				ukDeclaration.JE_JS = shipment.PK;
				Factory.Save();
			}

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;

			var inboundXml = string.Format(CultureInfo.InvariantCulture, @"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SS00011826</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>PUS</Code>
      </Company>
      <ServerID>{0}</ServerID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>", registrationKey.ServerCode);

			string responseBody = null;

			DataRegistry.Instance.WebBranch = ukBranch.PK.ToGuid();
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains("Company was specified, however EnterpriseId or ServerId were not specified. You should specify EnterpriseId, ServerId and Company if you want to map to a particular Company.", responseBody);
		}

		public void TesteAdaptorHttpXMLReturnsErrorWithExplanationIfEnterpriseIdAndServerIdWasOmittedButCompanyWasSpecified()
		{
			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;
			ukBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";

			Factory.Save();

			Integration.Customs.IBaseJobDeclaration ukDeclaration = null;

			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			{
				ukDeclaration = Factory.New<Integration.Customs.IBaseJobDeclaration>();
				ukDeclaration[JobDeclarationSchema.JE_GoodsDescription.Name] = "UK Declaration Goods Description";
				ukDeclaration[JobDeclarationSchema.JE_OverrideFreightDefaults.Name] = true;
				ukDeclaration.JE_JS = shipment.PK;
				Factory.Save();
			}

			var inboundXml =
				$@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SS00011826</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>PUS</Code>
      </Company>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>";

			string responseBody = null;

			DataRegistry.Instance.WebBranch = ukBranch.PK.ToGuid();
			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, ukBranch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains("Company was specified, however EnterpriseId or ServerId were not specified. You should specify EnterpriseId, ServerId and Company if you want to map to a particular Company.", responseBody);
		}

		public void TestPostReturnsErrorResponseWhenCalledWithRequestExceedingMaximumLength()
		{
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				var exceptionType = typeof(HttpException);
				var exception = exceptionType.Assembly.CreateInstance(exceptionType.FullName, false, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { "Maximum request length exceeded.", null, WebEventCodes.RuntimeErrorPostTooLarge }, null, null) as HttpException;
				eAdaptorHandlerFactory.SetGetHandlerHook((ref IHttpXmlMessageHandler h) => throw exception);
				request.Content = new StringContent("<UniversalShipmentRequest></UniversalShipmentRequest>");
				controller.Request = request;

				using (var response = controller.Post())
				{
					var runtimeSection = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
					AssertEquals(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
					AssertEquals($"{{\"Message\":\"Maximum request length exceeded. Current limit is {runtimeSection.MaxRequestLength}KB. Please raise an incident if you believe the limit should be raised.\"}}", response.Content.ReadAsStringAsync().Result);
				}
			}
		}

		public void TestPostSaveConcurrencyError()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			var sqlText = $"RAISERROR('{{{dummyBizO.PK},True}} ConcurrencyError', 16, 1)";
			var testCommand = Db.Connection.Command(sqlText);

			using (var testSaveCommand = new ZSaveCommand(testCommand, new List<ILargeColumnSaver>()))
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				ZSaveCommandException saveCommandException = null;
				try
				{
					testSaveCommand.Execute();
				}
				catch (ZSaveCommandException actualSaveCommandException)
				{
					saveCommandException = actualSaveCommandException;
				}

				var saveConcurrencyException = new ZSaveConcurrencyException(
					new ZDataConcurrencyException(saveCommandException.InnerException, ((INeedRow)dummyBizO).Row, Db.Connection), Factory);

				eAdaptorHandlerFactory.SetGetHandlerHook((ref IHttpXmlMessageHandler h) => throw saveConcurrencyException);
				request.Content = new StringContent("<UniversalShipmentRequest></UniversalShipmentRequest>");
				controller.Request = request;

				using (var response = controller.Post())
				{
					AssertEquals(HttpStatusCode.Conflict, response.StatusCode);
					AssertEquals($"{{\"Message\":\"\\r\\n--- Save Aborted Due to Concurrency Check ---\\r\\n\\r\\nROW INFORMATION\\r\\nTable      = DummyBizo\\r\\nPK         = {dummyBizO.PK}\\r\\nRowState   = Added\\r\\n\\r\\nCOLUMN INFORMATION\\r\\nRow has no original version. Row is new.\"}}", response.Content.ReadAsStringAsync().Result);
				}
			}
		}

		public void TestPostRethrowsNonMaxRequestLengthHttpExceptions()
		{
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				var stream = new Mock<Stream>();
				stream.Setup(s => s.BeginRead(It.IsAny<byte[]>(), It.IsAny<int>(), Moq.It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>())).Throws(new HttpException());
				stream.Setup(s => s.CanRead).Returns(true);
				request.Content = new StreamContent(stream.Object);
				controller.Request = request;
				try
				{
					AssertExceptionThrown<HttpException>(() => controller.Post().Dispose());
				}
				finally
				{
					ExceptionReporter.Instance.TotalReportCount = 0;
				}
			}
		}

		public void TestNativeXmlRetrieveErrorCriteria()
		{
			var requestXml =
@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Codde"">FOO</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";

			var responseXml = sender.SendRequest(requestXml).result;

			var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(1, messages.Length);

			AssertXMLEquals("Response Message",
				$@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>ERR</Status>
  <Data />
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Error - Key based retrieval only supports Primary or Candidate keys as field name. ""Codde"" is not a primary or candidate key for table ""RefUNLOCO""
Information - 0 matches found.</ProcessingLog>
</UniversalResponse>", responseXml);
		}

		public void TestUniversalShipmentOrderCannotImportMessageIsUniversalResponse()
		{
			var requestXml =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>OrderManagerOrder</Type>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>GWC</Code>
        <Country>
          <Code>PL</Code>
          <Name>Poland</Name>
        </Country>
        <Name>Meiko Trans Polska Sp. z o.o.</Name>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
      <DataProvider>EDIDATDEM</DataProvider>
    </DataContext>
    <BookingConfirmationReference>3414142</BookingConfirmationReference>
    <CommercialInfo>
      <CommercialInvoiceCollection>
        <CommercialInvoice>
          <InvoiceNumber>2246979</InvoiceNumber>
          <InvoiceDate>2021-10-14</InvoiceDate>
        </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
    <ContainerMode>
	<Code>OTH</Code>
    </ContainerMode>
    <GoodsDescription>AL1431_N116-191-152*A9</GoodsDescription>
    <OuterPacks>3</OuterPacks>
    <OuterPacksPackageType>
      <Code>PLT</Code>
      <Description>Pallet</Description>
    </OuterPacksPackageType>
    <SecondBuyerContact></SecondBuyerContact>
    <ShipmentIncoTerm>
      <Code>DAP</Code>
      <Description>Delivered At Place</Description>
    </ShipmentIncoTerm>
    <TransportMode>
      <Code>ROA</Code>
      <Description>Road Freight</Description>
    </TransportMode>
    <DateCollection>
      <Date>
        <Type>OrderDate</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2021-10-14</Value>
      </Date>
    </DateCollection>
  </Shipment>
</UniversalShipment>
";

			var responseXml = sender.SendRequest(requestXml).result;

			AssertContains("Event Type", "<EventType>DIF</EventType>", responseXml);

			AssertContains("Response Message", @"<ProcessingLog>No matching Order found, creating new Order.
Populating Order...
Error - Cannot Import Order
No Buyer Address was provided.
No changes were made due to the above errors. Please fix the errors and try again.
No Module used this Universal Shipment data.
Message Discarded.</ProcessingLog>", responseXml);
		}

		public void TestNativeXmlWithoutBody()
		{
			var requestXml =
@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Codde"">FOO</Criteria>
      </CriteriaGroup>
    </UNLOCO>
</Native>";

			string responseXml = sender.SendRequest(requestXml).result;

			AssertContains("Response Message",
				@$"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>ERR</Status>
  <Data />
  <ProcessingLog>Unrecognized Native Dataset. Examples of supported Native Datasets are: UNLOCO, Organization, etc. A complete list of Native Datasets can be found in the eAdaptor Developer's guide or alternatively in the XML Schemas generated by {Core.Constants.ProductName}.", responseXml);
		}

		public void TestNativeXmlUnrecognizedNativeDataset()
		{
			var requestXml =
@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UnrecognizedNativeDataset>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Codde"">FOO</Criteria>
      </CriteriaGroup>
    </UnrecognizedNativeDataset>
  </Body>
</Native>";

			var responseXml = sender.SendRequest(requestXml).result;

			AssertEquals("Response Message",
				@$"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>ERR</Status>
  <Data />
  <ProcessingLog>Unrecognized Native Dataset. Examples of supported Native Datasets are: UNLOCO, Organization, etc. A complete list of Native Datasets can be found in the eAdaptor Developer's guide or alternatively in the XML Schemas generated by {Core.Constants.ProductName}.</ProcessingLog>
</UniversalResponse>", responseXml);
		}

		public void TestNativeXmlUnrecognizedRootElement()
		{
			var requestXml =
@"<UnrecognizedRootElement>
  <Body>
  </Body>
</UnrecognizedRootElement>";

			var responseXml = sender.SendRequest(requestXml).result;

			AssertEquals("Response Message",
				@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>ERR</Status>
  <Data />
  <ProcessingLog>Unrecognized root element or malformed XML. Examples of supported root elements are: UniversalShipmentRequest, Native, etc.</ProcessingLog>
</UniversalResponse>", responseXml);
		}

		public void TestAllUniversalXmlTypes_ShouldWork()
		{
			foreach (var type in Enum.GetNames(typeof(UniversalDataType)).Where(x => !baselinedUniversalDataTypes.Contains(x)))
			{
				var shortTypeName = type.Replace("Universal", string.Empty);
				var additionalXmlContent = "";
				if (shortTypeName == "Event")
				{
					additionalXmlContent = $@"
  <EventTime>2019-05-21T05:28:54.147</EventTime>
  <EventType>DDI</EventType>
";
				}
				var xml = $@"
<{type}>
  <{shortTypeName}>
  {additionalXmlContent}
  </{shortTypeName}>
</{type}>";

				var responseXml = sender.SendRequest(xml).result;

				AssertContains($"If you've added a new type of Universal Xml, please be sure to make it work in the eAdaptorController. It failed for the type {type}. Surprise!", "<Status>PRS</Status>", responseXml);
			}
		}

		readonly string[] baselinedUniversalDataTypes = { nameof(UniversalDataType.UniversalSchedule) };

		public void TestNativeXml_OrgCode()
		{
			var requestXml = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
<Header>
	<OwnerCode>DHGLBNJ1</OwnerCode>
	<EnableCodeMapping>true</EnableCodeMapping>
</Header>
<Body>
	<Organization version=""2.0"">
		<OrgHeader Action=""INSERT"">
			<IsActive>true</IsActive>
			<FullName>TERMINAL DE L'ATLANTIQUE</FullName>
			<ClosestPort TableName=""RefUNLOCO""><Code>FRLEH</Code></ClosestPort>
		</OrgHeader>
	</Organization>
</Body>
</Native>";

			var orgCodeAlgorithm = new OrgCodeAlgorithm();
			orgCodeAlgorithm.Elements[OrgCodeElementDescription.FirstName].Order = 1;
			orgCodeAlgorithm.Elements[OrgCodeElementDescription.FirstName].Length = 3;
			orgCodeAlgorithm.Elements[OrgCodeElementDescription.SecondName].Order = 2;
			orgCodeAlgorithm.Elements[OrgCodeElementDescription.SecondName].Length = 3;
			orgCodeAlgorithm.Elements[OrgCodeElementDescription.IataCode].Order = 3;
			orgCodeAlgorithm.Elements[OrgCodeElementDescription.IataCode].Length = 3;
			orgCodeAlgorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Order = 4;
			orgCodeAlgorithm.Elements[OrgCodeElementDescription.CodeSpecificUniqueNumber].Length = 1;
			orgCodeAlgorithm.AlgorithmType = OrgCodeAlgorithmType.Default;
			OrganisationsDataRegistry.Instance.OrgCodeAlgorithmDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, orgCodeAlgorithm);

			for (var i = 0; i < 11; i++)
			{
				var responseXml = sender.SendRequest(requestXml).result;
				AssertContains("Information - OrgHeader - 1 inserts, 0 updates, 0 deletes", responseXml);
			}

			var response = sender.SendRequest(requestXml).result;
			AssertNotContains("Error - GetNextTailNumberCheckingForDuplicates went into infinite loop with nextTail", response);
			AssertContains("Information - OrgHeader - 1 inserts, 0 updates, 0 deletes", response);
		}

		public void TestNativeXml_DataContext()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			Factory.Save();

			var requestXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Header xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
    <OwnerCode>EDIDATEDI</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
    <nv:DataContext xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:nv=""http://www.cargowise.com/Schemas/Native/2011/11"">
      <Company>
        <Code>{company.GC_Code}</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
    </nv:DataContext>
  </Header>
  <Body>
    <Organization version=""1.0"">
      <OrgHeader Action=""MERGE"">
        <FullName>JR</FullName>
		<ClosestPort TableName=""RefUNLOCO"">
			<Code>BGSOF</Code>
		</ClosestPort>
	  </OrgHeader>
    </Organization>
  </Body>
</Native>";

			var responseXml = sender.SendRequest(requestXml).result;
			AssertContains("Information - OrgHeader - 1 inserts, 0 updates, 0 deletes", responseXml);
			AssertContains($"Targeting Branch '{branch.GB_Code}', Company '{company.GC_Code}' from Data Context", responseXml);

			var org = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, "JR"))[0];
			var addLog = org.Logs.Find(l => l.SL_SE_NKEvent == Events.AddedARecordToTheSystemCode).FirstOrDefault();
			var dimLog = org.Logs.Find(l => l.SL_SE_NKEvent == Events.DataImportCode).FirstOrDefault();
			AssertNull("Expecting no ADD log", addLog);
			AssertEquals("Expecting events created in specified context", branch.GB_Code, dimLog?.SL_GB_NKBranch);
		}

		public void TestNativeXmlRetrieveEndToEnd()
		{
			AssertNoExistingMessages();

			var requestXml =
@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">AUSYD</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";

			string responseXml = sender.SendRequest(requestXml).result;

			AssertStartsWith("Response Message starts with",
				@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>
  <Data><Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Body>
    <UNLOCO>
      <RefUNLOCO>", responseXml);

			AssertContains("<PortName>Sydney</PortName>", responseXml);
			AssertContains("<IATA>SYD</IATA>", responseXml);
			AssertContains("<Code>AUSYD</Code>", responseXml);

			AssertContains(@"<CountryCode TableName=""RefCountry"">
          <Code>AU</Code>", responseXml);

			AssertContains("Response Message has 1 match found",
		@"<ProcessingLog>Information - 1 matches found.</ProcessingLog>
</UniversalResponse>", responseXml);

			var messagesQuery = new ZQuery();
			messagesQuery.OrderBy = EDIMessageSchema.EM_ApplicationCode.Name + ", " + EDIMessageSchema.EM_ReceiveTransmit.Name;
			var messages = Factory.Load<IEDIMessage>(messagesQuery);
			AssertNotNull("messages reloaded", messages);
			AssertMultilineASCIIEquals("App|Dir|Typ|Status", @"
NDQ RCV XNL RCV
NDQ TRX XNL SNT
".Trim(), string.Join("\r\n", messages.Select(message => message.EM_ApplicationCode + " " + message.EM_ReceiveTransmit + " " + message.EM_MessageSubType + " " + message.EM_Status)));
		}

		[ExpectNoExceptions]
		public void TestNativeXmlRetrievesBizoWithInvalidXmlCharacters()
		{
			AssertNoExistingMessages();

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "TestOrg1";
			header.OH_FullName = "Ben\u001E & Jerry's";
			Factory.Save();

			var requestXml =
@"<?xml version=""1.0"" encoding=""UTF - 8"" standalone=""yes""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native"" version=""2.0"">
	<Body>
		<Organization>
			<CriteriaGroup Type=""Partial"">
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">TestOrg1</Criteria>
			</CriteriaGroup>
		</Organization>
	</Body>
</Native>";

			string responseXml = sender.SendRequest(requestXml).result;

			AssertContains("<Status>PRS</Status>", responseXml);
			AssertContains("<FullName>Ben &amp; Jerry's</FullName>", responseXml);
			AssertContains("Response Message has 1 match found",
		@"<ProcessingLog>Information - 1 matches found.</ProcessingLog>
</UniversalResponse>", responseXml);
		}

		[ExpectNoExceptions]
		public void TestNativeXmlRetrievesBizoWithSurrogatePairs()
		{
			AssertNoExistingMessages();

			var header = Factory.New<OrgHeader>();
			header.OH_Code = "TestOrg1";
			header.OH_FullName = "Smiley\u0020Face:\uD83E\uDD11";
			Factory.Save();

			var requestXml =
@"<?xml version=""1.0"" encoding=""UTF - 8"" standalone=""yes""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native"" version=""2.0"">
	<Body>
		<Organization>
			<CriteriaGroup Type=""Partial"">
				<Criteria Entity=""OrgHeader"" FieldName=""Code"">TestOrg1</Criteria>
			</CriteriaGroup>
		</Organization>
	</Body>
</Native>";

			string responseXml = sender.SendRequest(requestXml).result;

			AssertContains("<Status>PRS</Status>", responseXml);
			AssertContains("<FullName>Smiley Face:🤑</FullName>", responseXml);
			AssertContains("Response Message has 1 match found",
		@"<ProcessingLog>Information - 1 matches found.</ProcessingLog>
</UniversalResponse>", responseXml);
		}

		public void TestNativeXmlRetrieveEndToEndTwoRecords()
		{
			var requestXml =
@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">AUSYD</Criteria>
      </CriteriaGroup>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">ADALV</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";

			string responseXml = sender.SendRequest(requestXml).result;

			AssertStartsWith("Response Message starts with",
				@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>
  <Data><Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Body>
    <UNLOCO>
      <RefUNLOCO>", responseXml);

			AssertContains("<PortName>Sydney</PortName>", responseXml);
			AssertContains("<IATA>SYD</IATA>", responseXml);
			AssertContains("<Code>AUSYD</Code>", responseXml);

			AssertContains("<PortName>Andorra la Vella</PortName>", responseXml);

			AssertContains(@"<CountryCode TableName=""RefCountry"">
          <Code>AU</Code>", responseXml);

			AssertContains(@"<CountryCode TableName=""RefCountry"">
          <Code>AD</Code>", responseXml);

			AssertContains("Response Message has 2 match found",
		@"<ProcessingLog>Information - 2 matches found.</ProcessingLog>
</UniversalResponse>", responseXml);
		}

		public void TestNativeXmlRetrieveZeroMatches()
		{
			var requestXml =
@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">FOO</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";

			var responseXml = sender.SendRequest(requestXml).result;

			var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(1, messages.Length);

			AssertXMLEquals("Response Message",
				$@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>
  <Data />
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Information - 0 matches found.</ProcessingLog>
</UniversalResponse>", responseXml);
		}

		public void TestNativeXmlUpdate()
		{
			var requestXml =
@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <RefUNLOCO Action=""UPDATE"">
        <Code>AUSYD</Code>
        <PortName>FOO</PortName>
      </RefUNLOCO>
    </UNLOCO>
  </Body>
</Native>";

			var responseXml = sender.SendRequest(requestXml).result;

			var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(1, messages.Length);

			AssertXMLEquals("Response Message starts with",
				$@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>
  <Data><UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <Company>
        <Code>DEM</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Demo Company</Name>
      </Company>
      <DataProvider>EDIDATDEM</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <EventType>DIM</EventType>

    <ContextCollection>
      <Context>
        <Type>ProcessingStatusCode</Type>
        <Value>PRS</Value>
      </Context>
      <Context>
        <Type>EntityPrimaryKey</Type>
        <Value>ad87872e-96b8-4c9a-bc0b-6a4088247a64</Value>
      </Context>
      <Context>
        <Type>NativeEntityName</Type>
        <Value>UNLOCO</Value>
      </Context>
      <Context>
        <Type>EntityLocalCode</Type>
        <Value>AUSYD</Value>
      </Context>
      <Context>
        <Type>EntityExternalCode</Type>
        <Value>AUSYD</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
</Data>
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Information - RefUNLOCO - 0 inserts, 1 updates, 0 deletes</ProcessingLog>
</UniversalResponse>", responseXml);

			var factory = new BusinessObjectFactory();
			var query = new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD");
			var collection = factory.Load<RefUNLOCO>(query);
			Assert("One record with AUSYD code", collection.Length == 1);
			AssertEquals("PortName updated", "FOO", collection[0].RL_PortName);
		}

		public void TestNativeXmlDelete()
		{
			var threeLetterCode = "AWU";
			var factory = new BusinessObjectFactory();
			var query = new ZQuery(RefAirlineSchema.RM_ThreeLetterCode, threeLetterCode);
			var collection = factory.Load<RefAirline>(query);
			Assert("Precondition: One record", collection.Length == 1);

			var requestXml =
$@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
	<Airline>
      <RefAirline Action=""DELETE"">
        <ThreeLetterCode>{threeLetterCode}</ThreeLetterCode>
      </RefAirline>
	</Airline>
  </Body>
</Native>";

			string responseXml = sender.SendRequest(requestXml).result;

			var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(1, messages.Length);

			AssertXMLEquals("Response Message starts with",
				$@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>
  <Data><UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <Company>
        <Code>DEM</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Demo Company</Name>
      </Company>
      <DataProvider>EDIDATDEM</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <EventType>DIM</EventType>

    <ContextCollection>
      <Context>
        <Type>ProcessingStatusCode</Type>
        <Value>PRS</Value>
      </Context>
      <Context>
        <Type>EntityPrimaryKey</Type>
        <Value>d7027de7-1da2-41dd-b32b-b35733a54f4a</Value>
      </Context>
      <Context>
        <Type>NativeEntityName</Type>
        <Value>Airline</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
</Data>
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Information - RefAirline - 0 inserts, 0 updates, 1 deletes</ProcessingLog>
</UniversalResponse>", responseXml);

			factory = new BusinessObjectFactory();
			query = new ZQuery(RefAirlineSchema.RM_ThreeLetterCode, threeLetterCode);
			collection = factory.Load<RefAirline>(query);
			Assert("Zero records", collection.Length == 0);
		}

		public void TestNativeXmlUpdateEndToEnd()
		{
			AssertNoExistingMessages();
			var headerCode = "FAFFAS";
			var addressCode = "PST: OSAKA HIGASHI";
			var contactName = "Some one";
			var phone = "+61430000001";
			var requestXml =
$@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <Organization>
      <OrgHeader>
        <Code>{headerCode}</Code>
        <Name>Some Organisation</Name>
        <OrgAddressCollection>
          <OrgAddress Action=""UPDATE"">
            <Code>{addressCode}</Code>
            <Address1>Some changed value</Address1>
            <Address2>Some existing value</Address2>
          </OrgAddress>
          <OrgAddress Action=""INSERT"">
            <Address1>New Address 1</Address1>
            <City>Copenhagen</City>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgContactCollection>
          <OrgContact Action=""INSERT"">
            <ContactName>{contactName}</ContactName>
            <Phone>{phone}</Phone>
          </OrgContact>
        </OrgContactCollection>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

			string responseXml = sender.SendRequest(requestXml).result;

			var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(1, messages.Length);

			AssertXMLEquals("Response Message starts with",
				$@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>
  <Data><UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <Company>
        <Code>DEM</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Demo Company</Name>
      </Company>
      <DataProvider>EDIDATDEM</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <EventType>DIM</EventType>

    <ContextCollection>
      <Context>
        <Type>ProcessingStatusCode</Type>
        <Value>PRS</Value>
      </Context>
      <Context>
        <Type>EntityPrimaryKey</Type>
        <Value>aa1895c4-612f-42a5-b75b-0024986de21b</Value>
      </Context>
      <Context>
        <Type>NativeEntityName</Type>
        <Value>Organization</Value>
      </Context>
      <Context>
        <Type>EntityLocalCode</Type>
        <Value>FAFFAS</Value>
      </Context>
      <Context>
        <Type>EntityExternalCode</Type>
        <Value>FAFFAS</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
</Data>
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Information - OrgContact - 1 inserts, 0 updates, 0 deletes
Information - OrgAddress - 1 inserts, 1 updates, 0 deletes</ProcessingLog>
</UniversalResponse>", responseXml);

			var factory = new BusinessObjectFactory();
			var query = new ZQuery(OrgAddressSchema.OA_Code, addressCode);
			var collection = factory.Load<OrgAddress>(query);
			Assert("One OrgAddress record", collection.Length == 1);
			AssertEquals("Address1 updated", "Some changed value", collection[0].OA_Address1);
			AssertEquals("Address2 updated", "Some existing value", collection[0].OA_Address2);

			query = new ZQuery(OrgAddressSchema.OA_Address1, "New Address 1");
			collection = factory.Load<OrgAddress>(query);
			Assert("One OrgAddress record", collection.Length == 1);
			AssertEquals("Address inserted", "Copenhagen", collection[0].OA_City);

			query = new ZQuery(OrgContactSchema.OC_ContactName, contactName);
			var contactQuery = new ZDBOnlyQuery(typeof(OrgContact));
			var headerSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgContactSchema.OC_OH);
			headerSubQuery.AddToFilter(OrgHeaderSchema.OH_Code, headerCode);
			contactQuery.AddSubQuery(headerSubQuery, JoinCondition.And);
			query.AddToFilter(contactQuery);

			var contacts = factory.Load<OrgContact>(query);
			AssertEquals("One contact found", 1, contacts.Length);
			AssertEquals("Correct OrgHeader", headerCode, contacts[0].Header.OH_Code);
			AssertEquals("Contact phone updated", phone, contacts[0].OC_Phone);

			var messagesQuery = new ZQuery();
			messagesQuery.OrderBy = EDIMessageSchema.EM_ApplicationCode.Name + ", " + EDIMessageSchema.EM_ReceiveTransmit.Name;
			messages = Factory.Load<IEDIMessage>(messagesQuery);
			AssertNotNull("messages reloaded", messages);
			AssertMultilineASCIIEquals("App|Dir|Typ|Status", @"
NDQ RCV XRO RCV
NDQ TRX XRO SNT
".Trim(), string.Join("\r\n", messages.Select(message => message.EM_ApplicationCode + " " + message.EM_ReceiveTransmit + " " + message.EM_MessageSubType + " " + message.EM_Status)));
		}

		public void TestNativeXmlUnknownAction()
		{
			var requestXml =
@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <RefUNLOCO>
        <Code>AUSYD</Code>
      </RefUNLOCO>
    </UNLOCO>
  </Body>
</Native>";

			string responseXml = sender.SendRequest(requestXml).result;

			var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(1, messages.Length);

			AssertXMLEquals("Response Message starts with",
				$@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>
  <Data><UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>

      <Company>
        <Code>DEM</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Demo Company</Name>
      </Company>
      <DataProvider>EDIDATDEM</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>

    <EventType>DIM</EventType>

    <ContextCollection>
      <Context>
        <Type>ProcessingStatusCode</Type>
        <Value>PRS</Value>
      </Context>
      <Context>
        <Type>EntityPrimaryKey</Type>
        <Value>ad87872e-96b8-4c9a-bc0b-6a4088247a64</Value>
      </Context>
      <Context>
        <Type>NativeEntityName</Type>
        <Value>UNLOCO</Value>
      </Context>
      <Context>
        <Type>EntityLocalCode</Type>
        <Value>AUSYD</Value>
      </Context>
      <Context>
        <Type>EntityExternalCode</Type>
        <Value>AUSYD</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
</Data>
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Information - No insert/update action performed.</ProcessingLog>
</UniversalResponse>", responseXml);
		}

		public void TestNativeXmlUpdateUnknownCode()
		{
			var requestXml =
@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <RefUNLOCO Action=""UPDATE"">
        <Code>FOO</Code>
        <PortName>BAR</PortName>
      </RefUNLOCO>
    </UNLOCO>
  </Body>
</Native>";

			string responseXml = sender.SendRequest(requestXml).result;

			var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(1, messages.Length);

			AssertXMLEquals("Response Message starts with",
				$@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>ERR</Status>
  <Data />
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Error - There is no UNLOCO with the following values: [Code:FOO][PortName:BAR].</ProcessingLog>
</UniversalResponse>", responseXml);
		}

		public void TestNativeXmlUpdateNumericBoolean()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			Factory.Save();
			var responseXml = sender.SendRequest($@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>DEMORGSYD</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Product version=""2.0"">
      <OrgSupplierPart Action=""MERGE"">
        <PK>{product.PK}</PK>
        <PartNum>{product.OP_PartNum}</PartNum>
        <IsActive>1</IsActive>
        <CustomFlag5>true</CustomFlag5>
        <AutoPrintAssemblyInstructions>false</AutoPrintAssemblyInstructions>
      </OrgSupplierPart>
    </Product>
  </Body>
</Native>").result;

			AssertNotContains("String was not recognized as a valid Boolean", responseXml);
		}

		public void TestNativeXmlCriteriaByKeySearch()
		{
			var requestXml =
@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
	<Body>
		<Product>
			<CriteriaGroup Type=""Key"">
				<Criteria Entity=""OrgSupplierPart"" FieldName=""PK"">1000</Criteria>
			</CriteriaGroup>
		</Product>
	</Body>
</Native>";

			string responseXml = sender.SendRequest(requestXml).result;

			var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(1, messages.Length);

			AssertXMLEquals("Response Message starts with",
				$@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>ERR</Status>
  <Data />
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Error - The value 1000 is not a valid Unique Identifier.
Information - 0 matches found.</ProcessingLog>
</UniversalResponse>", responseXml);
		}

		public void TestNativeXmlDIMEventCreatedCorrectly()
		{
			var requestXml = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"">
  <Body>
    <Organization>
      <OrgHeader Action=""MERGE"">
        <Code>RACBURDBO</Code>
        <FullName>RACK AND BURLINGTON</FullName>
        <Language>EN</Language>
        <IsActive>true</IsActive>
        <IsConsignee>true</IsConsignee>
        <IsConsignor>true</IsConsignor>
        <IsTransportClient>true</IsTransportClient>
        <IsWarehouseClient>true</IsWarehouseClient>
        <IsForwarder>true</IsForwarder>
        <IsShippingProvider>true</IsShippingProvider>
        <Category>BUS</Category>
        <ScreeningStatus>NOT</ScreeningStatus>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <Code>12 Bundy Street</Code>
            <Address1>12 Bundy Street</Address1>
            <State>NSW</State>
            <PostCode>2827</PostCode>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <IsActive>true</IsActive>
            <AuthorityToLeave>DEF</AuthorityToLeave>
            <City>Dubbo</City>
            <Language>EN</Language>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>AUDBO</Code>
            </RelatedPortCode>
            <CountryCode TableName=""RefCountry"">
              <Code>AU</Code>
            </CountryCode>
          </OrgAddress>
        </OrgAddressCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUDBO</Code>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

			var originalLogs = Factory.Load<IStmALog>(new ZQuery());

			string responseXml = sender.SendRequest(requestXml).result;

			AssertEquals("Using web user context", User.WebUserCode, Environment.Env.CurrentUser.Initials);

			AssertContains("Response Message starts with", @"<ProcessingLog>Information - OrgHeader - 1 inserts, 0 updates, 0 deletes
Information - OrgAddress - 1 inserts, 0 updates, 0 deletes
Information - OrgAddressCapability - 1 inserts, 0 updates, 0 deletes</ProcessingLog>", responseXml);

			var logs = Factory.Load<IStmALog>(new ZQuery()).Except(originalLogs).ToArray();

			AssertEquals("logs.Length == 1", 1, logs.Length);

			var dimEvent = logs.Where(log => log.SL_SE_NKEvent == Events.DataImportCode).First();

			CombineAssertions("dimEvent", () =>
			{
				AssertEquals("dimEvent.SL_GB_NKBranch", Environment.Env.CurrentBranch.Code, dimEvent.SL_GB_NKBranch);
				AssertEquals("dimEvent.SL_GE_NKDepartment", Environment.Env.CurrentDepartment.Code, dimEvent.SL_GE_NKDepartment);
				AssertEquals("dimEvent.SL_GS_NKUser", User.InterchangeUserCode, dimEvent.SL_GS_NKUser);
			});
		}

		public void TestProcessingLogsAddedToNativeXMLRequestLogs()
		{
			AssertNoExistingMessages();
			var requestXml = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">FOO</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";

			sender.SendRequest(requestXml);
			var messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));

			AssertEquals("Should be one request message", 1, messages.Length);
			var msg = messages[0];
			AssertEquals("Message body should match request", requestXml, msg.EM_MessageText);

			var notes = ((EnterpriseBusinessObject)msg).Notes;
			AssertEquals("Should add a single note", 1, notes.GetAllNotes().Count);

			var logNotes = notes.FindByDescription("Data Import Log Text", false);
			AssertEquals("Note should be a data import log", 1, logNotes.Length);
			AssertEquals("Query should return no matches and log this in notes", "Information - 0 matches found.", logNotes[0].ST_NoteText);
		}

		public void TestUniversalActivityDIMEventCreatedCorrectly()
		{
			var requestXml = @"<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>WorkItem</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <Summary>Well hello there sir</Summary>
    <CreatedBy>
      <Code>ARI</Code>
      <Name>Ariadna Romanenko</Name>
    </CreatedBy>
    <Description></Description>
    <Location>
      <Code></Code>
      <Description></Description>
    </Location>
    <SelectionCriterion1>
      <Code>BS4</Code>
      <Description>Barsha</Description>
    </SelectionCriterion1>
    <SelectionCriterion2>
      <Code>TS1</Code>
      <Description>Product</Description>
    </SelectionCriterion2>
    <SelectionCriterion3>
      <Code></Code>
      <Description></Description>
    </SelectionCriterion3>
    <SelectionCriterion4>
      <Code></Code>
      <Description></Description>
    </SelectionCriterion4>
    <SelectionCriterion5>
      <Code></Code>
      <Description></Description>
    </SelectionCriterion5>
    <Status>
      <Code>WRK</Code>
      <Description>Working</Description>
    </Status>
    <TaskSetCollection>
    </TaskSetCollection>
  </Activity>
</UniversalActivity>";

			var originalLogs = Factory.Load<IStmALog>(new ZQuery());

			string responseXml = sender.SendRequest(requestXml).result;

			var logs = Factory.Load<IStmALog>(new ZQuery()).Except(originalLogs).ToArray();

			var dimEvent = logs.Where(log => log.SL_SE_NKEvent == Events.DataImportCode).ToArray();

			AssertEquals(1, dimEvent.Length);
		}

		public void TestQuickBookingConvertedToShipmentConcurrencyError()
		{
			AssertEquals("Precondition", 0, Factory.Load<IEDIMessage>(new ZQuery()).Length);

			var booking = Factory.New<Forwarding.IForwardingShipment>();
			booking.JS_BookingReference = "BLATTICUS";
			booking.JS_IsBooking = true;
			booking.JS_IsForwardRegistered = false;
			booking.JS_IsCancelled = false;
			Factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				//Convert the booking to a shipment right before save happens in HTTP+XML causing a concurrency expcetion
				if (factory.NameForDebugging == MasterFilesIntegrationConstants.PublishUniversalXmlInternallyFactoryName)
				{
					var newFactory = new BusinessObjectFactory();
					var reloadBooking = newFactory.Load<Forwarding.IForwardingShipment>(booking.PK);
					new BuildConsolHelper().TurnBookingIntoShipment(reloadBooking as CommonShipment, null);
					AssertEquals(true, reloadBooking.JS_IsForwardRegistered);
					newFactory.Save();
				}
			});

			var responseXml = sender.SendRequest(@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <EventTime>2019-05-21T05:28:54.147</EventTime>
    <EventType>ATH</EventType>
    <IsEstimate>false</IsEstimate>
    <ContextCollection>
      <Context>
        <Type>ShippersReference</Type>
        <Value>BLATTICUS</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
").result;

			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertContains("FAL", messages[0].EM_Status);
			AssertContains("Another user has converted the booking into a shipment.", responseXml);
		}
		public void TestUniversalUpdateNumericBoolean()
		{
			var responseXml = sender.SendRequest(@"<?xml version=""1.0"" encoding=""utf-8""?>
	<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
	  <Shipment>
		<DataContext>
		  <DataTargetCollection>
			<DataTarget>
			  <Type>ForwardingShipment</Type>
			</DataTarget>
		  </DataTargetCollection>
		</DataContext>
		<ShipmentType>
		  <Code>AGT</Code>
		  <Description>Agent</Description>
		</ShipmentType>
		<TransportMode>
		  <Code>SEA</Code>
		  <Description>Sea Freight</Description>
		</TransportMode>
		<HasProhibitedPackaging>0</HasProhibitedPackaging>
	  </Shipment>
	</UniversalShipment>
  </Body>").result;

			AssertNotContains("Value must be a valid Boolean (true or false)", responseXml);
		}

		public void TestImportShipment_TooManyParentsMatched()
		{
			TestImportShipment_TooManyParentsMatched_Core(false);
		}

		public void TestImportShipment_TooManyParentsMatched_DoubleLinking()
		{
			TestImportShipment_TooManyParentsMatched_Core(true);
		}

		void TestImportShipment_TooManyParentsMatched_Core(bool isCFSRegistered)
		{
			var forwardingShipmentType = ObjectFactory.GetType<Forwarding.IForwardingShipment>();

			CombinationKeyMatcherParentLimit.OverrideParentLimitForTesting(5);
			for (var i = 0; i < 6; i++)
			{
				var shipment = Factory.NewWithValidTestData(forwardingShipmentType) as Forwarding.IForwardingShipment;
				shipment.JS_BookingReference = "BLATTICUS";
				shipment.JS_IsBooking = false;
				shipment.JS_IsCFSRegistered = false;
			}

			var booking = Factory.NewWithValidTestData(forwardingShipmentType) as Forwarding.IForwardingShipment;
			booking.JS_BookingReference = "BLATTICUS";
			booking.JS_IsBooking = true;
			booking.JS_IsCFSRegistered = isCFSRegistered;
			booking.JS_IsForwardRegistered = false;
			booking.JS_IsCancelled = false;
			Factory.Save();

			var originalLogs = Factory.Load<IStmALog>(new ZQuery());
			var originalNotes = Factory.Load<StmNote>(new ZQuery());
			var responseXml = sender.SendRequest(@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <EventTime>2019-05-21T05:28:54.147</EventTime>
    <EventType>ATH</EventType>
    <IsEstimate>false</IsEstimate>

    <ContextCollection>
      <Context>
        <Type>ShippersReference</Type>
        <Value>BLATTICUS</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
").result;

			CombineAssertions(() =>
			{
				var logs = Factory.Load<IStmALog>(new ZQuery()).Except(originalLogs).ToArray();
				AssertEquals("Message is rejected therefore now changes should be made to the target bizos", 1, logs.Length);
				AssertEquals("Count(SL_SE_NKEvent == ADD) == 0", 0, logs.Count(log => log.SL_SE_NKEvent == "ADD"));
				AssertEquals("Count(SL_SE_NKEvent == STU) == 1", 1, logs.Count(log => log.SL_SE_NKEvent == "STU"));

				var messages = Factory.Load<EDIMessage>(new ZQuery()).OrderBy(message => message.EM_MessageNum).ToArray();
				AssertEquals(2, messages.Length);
				AssertEquals("Data Note Added", 1, messages[0].DataImportLogNoteCount);
				AssertEquals(EDIMessageStatusList.Codes.Error, messages[0].EM_Status);
				AssertEquals(0, messages[1].DataImportLogNoteCount);

				AssertContains("Error - Too many parents to match against. Please ensure that any Additional References used are unique identifiers.", responseXml);

				var notes = Factory.Load<StmNote>(new ZQuery()).Except(originalNotes).ToArray();
				AssertEquals(1, notes.Length);
			});
		}

		[UseSnapshotProtection]
		public void TestOutOfConnectionsErrorHandled()
		{
			DbEnv.SetDbEnvironment(new TestEnvironment());

			var threads = new List<Thread>();

			var requestXml =
@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">AUSYD</Criteria>
      </CriteriaGroup>
    </UNLOCO>
  </Body>
</Native>";

			var pauseEvent = new ManualResetEvent(false);
			var signalCount = 0;

			for (int i = 0; i < 2; ++i)
			{
				threads.Add(new Thread(() =>
				{
					try
					{
						eAdaptorHandlerFactory.SetGetHandlerHook((ref IHttpXmlMessageHandler h) =>
						{
							Interlocked.Increment(ref signalCount);
							pauseEvent.WaitOne();
							// Bail
							throw new Exception();
						});

						var response = sender.SendRequest("<UniversalEvent></UniversalEvent>");
					}
					catch (Exception)
					{
					}
				}));
			}

			foreach (var thread in threads)
			{
				thread.Start();
			}

			while (signalCount < 2 || threads.All(x => !x.IsAlive))
			{
				Thread.Yield();
			}

			Exception exception = null;
			string responseXml = null;
			threads.Add(new Thread(() =>
			{
				try
				{
					using (var request = new HttpRequestMessage())
					using (var controller = new eAdaptorController())
					{
						request.Content = new StringContent(requestXml);
						controller.Request = request;
						using (var response = controller.Post())
						{
							responseXml = response.Content.ReadAsStringAsync().Result;
						}
					}
				}
				catch (Exception ex)
				{
					exception = ex;
				}
			}));

			threads[2].Start();
			threads[2].Join();

			pauseEvent.Set();

			foreach (var thread in threads)
			{
				thread.Join();
			}

			AssertEquals(null, exception);
			AssertEquals("{\"Message\":\"Maximum number of concurrent connections has been exceeded.\"}", responseXml);
		}

		public void TestTransactionExceptionsHandledGracefully()
		{
			AssertEquals("Precondition", 0, Factory.Load<IEDIMessage>(new ZQuery()).Length);

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00001001";
			shipment.JS_UniqueConsignRef = "S00001001";

			Factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				if (factory.NameForDebugging == MasterFilesIntegrationConstants.PublishUniversalXmlInternallyFactoryName)
				{
					throw new TransactionException("Transaction has been rolled back in the server (application transaction count pending reset).");
				}
			});

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

			var messages = Factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertContains("FAL", messages[0].EM_Status);
			AssertContains("Transaction has been rolled back in the server (application transaction count pending reset).", responseXml);

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TesteAdaptorControllerFactorySaves()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			company.FillWithValidTestData();
			company.Branches.AddNew().FillWithValidTestData();
			company.GC_OH_OrgProxy = org.PK;
			var match = company.OrgProxy.CreatePatternMatchOverrideForTest();
			match.OO_ForeignCode = "MatchDisSuka";
			match.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			match.OO_LocalCode = org.OH_Code;
			match.OO_LocalGuid = org.PK;
			Factory.Save();

			var inboundXml = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key/>
        </DataTarget>
      </DataTargetCollection>
      <DataProvider>ULVTSTWTF</DataProvider>
      <EnterpriseID>ULV</EnterpriseID>
      <ServerID>TST</ServerID>
      <Company><Code>WTF</Code></Company>
    </DataContext>
  </Shipment>
</UniversalShipment>";
			string responseBody = null;

			int factoryCount = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest((thefactory) => factoryCount++);

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			//As Factory Save's are removed this should change.
			AssertLessThanOrEqualTo("More than X Factory Saves made.", factoryCount, 4);
		}

		#region Universal Activity

		public void TestProcessUniversalActivity_ShouldIncludeNewJobKeyInResponse()
		{
			AssertNoExistingMessages();

			var inboundXml =
				$@"<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>WorkItem</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <Summary>Complete Work</Summary>
  </Activity>
</UniversalActivity>
";

			string responseBody = null;

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains(
				@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>WorkItem</Type>
          <Key>WI00000001</Key>
        </DataSource>
      </DataSourceCollection>", responseBody);
		}

		[TestDate(2019, 06, 05)]
		public void TestUniversalTransaction()
		{
			DisposableLeakListener.Instance.StackTraceEnabled = true;
			new AccountingPeriodTestHelper().PostPeriodsForEntireYear(2019);

			AssertEquals(0, Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.TransactionsPendingAllocation)).Length);

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var xml = FormattableString.Invariant($@"<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AccountingInvoice</Type>
          <Key>AP INV ART123</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
				<Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
				<Name>{GlbCompany.CurrentCompany.GC_Name}</Name>
			</Company>
			<EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
			<ServerID>{registrationKey.ServerCode}</ServerID>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>BNE</Code>
      <Name>BN - AUBNE</Name>
    </Branch>
    <BranchAddress>
      <AddressType>OFC</AddressType>
      <Address1>10 HUTCHESON STREET</Address1>
      <Address2>ALBION  QLD</Address2>
      <AddressOverride>false</AddressOverride>
      <AddressShortCode>PST: 10 HUTCHESON STREET</AddressShortCode>
      <City>CITY</City>
      <CompanyName>EDI CUSTOMS BROKERS</CompanyName>
      <Country>
        <Code>AU</Code>
        <Name>Australia</Name>
      </Country>
      <Email></Email>
      <Fax></Fax>
      <OrganizationCode>EDICUS</OrganizationCode>
      <Phone></Phone>
      <Port>
        <Code>AUBNE</Code>
        <Name>Brisbane</Name>
      </Port>
      <Postcode>4010</Postcode>
      <ScreeningStatus>
        <Code>UNK</Code>
        <Description>Unknown</Description>
      </ScreeningStatus>
      <State>QLD</State>
    </BranchAddress>
    <Category>STD</Category>
    <CheckDrawer></CheckDrawer>
    <CheckNumberOrPaymentRef></CheckNumberOrPaymentRef>
    <CreateTime>2019-06-05T08:15:00</CreateTime>
    <CreateUser>E</CreateUser>
    <Department>
      <Code>BRN</Code>
      <Name>Branch</Name>
    </Department>
    <Description>AP INVOICE</Description>
    <DrawerBank></DrawerBank>
    <DrawerBranch></DrawerBranch>
    <DueDate>2019-06-05T18:15:00</DueDate>
    <ExchangeRate>1.000000</ExchangeRate>
    <InvoiceTerm>COD</InvoiceTerm>
    <InvoiceTermDays>0</InvoiceTermDays>
    <IsCancelled>false</IsCancelled>
    <IsCreatedByMatchingProcess>false</IsCreatedByMatchingProcess>
    <IsPrinted>false</IsPrinted>
    <Job>
      <Type>Job</Type>
    </Job>
    <JobInvoiceNumber>00001000</JobInvoiceNumber>
    <Ledger>AP</Ledger>
    <LocalCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </LocalCurrency>
    <LocalExVATAmount>-971.0000</LocalExVATAmount>
    <LocalTotal>-971.0000</LocalTotal>
    <LocalVATAmount>0.0000</LocalVATAmount>
    <Number>ART123</Number>
    <NumberOfSupportingDocuments>1</NumberOfSupportingDocuments>
    <OrganizationAddress>
      <AddressType>OFC</AddressType>
      <Address1>PO BOX 10446</Address1>
      <Address2>ADELAIDE ST, BRISBANE  QLD</Address2>
      <AddressOverride>false</AddressOverride>
      <AddressShortCode>PST: PO BOX 10446</AddressShortCode>
      <City></City>
      <CompanyName>A.A.L. SHIPPING AGENCIES P/L</CompanyName>
      <Country>
        <Code>AU</Code>
        <Name>Australia</Name>
      </Country>
      <Email></Email>
      <Fax></Fax>
      <OrganizationCode>AALSHI</OrganizationCode>
      <Phone></Phone>
      <Port>
        <Code>AUBNE</Code>
        <Name>Brisbane</Name>
      </Port>
      <Postcode>4000</Postcode>
      <ScreeningStatus>
        <Code>UNK</Code>
        <Description>Unknown</Description>
      </ScreeningStatus>
      <State></State>
    </OrganizationAddress>
    <OSCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </OSCurrency>
    <OSExGSTVATAmount>-971.0000</OSExGSTVATAmount>
    <OSGSTVATAmount>0.00</OSGSTVATAmount>
    <OSTotal>-971.0000</OSTotal>
    <OutstandingAmount>-971.0000</OutstandingAmount>
    <PlaceOfIssue>Brisbane</PlaceOfIssue>
    <PostDate>2019-06-05T18:15:00</PostDate>
    <ReceiptOrDirectDebitNumber></ReceiptOrDirectDebitNumber>
    <RequisitionDate>2019-06-05T18:15:00</RequisitionDate>
    <RequisitionStatus>NRM</RequisitionStatus>
    <TransactionDate>2019-06-05T18:15:00</TransactionDate>
    <TransactionType>INV</TransactionType>

    <PostingJournalCollection>
      <PostingJournal>
        <Branch>
          <Code>BNE</Code>
          <Name>BN - AUBNE</Name>
        </Branch>
        <Department>
          <Code>BRN</Code>
          <Name>Branch</Name>
        </Department>
        <Description>WAREHOUSE HANDLING COSTS ACTUAL</Description>
        <GLAccount>
          <AccountCode>1210.20.10</AccountCode>
          <Description>WAREHOUSE HANDLING COSTS ACTUAL</Description>
        </GLAccount>
        <GLPostDate>2019-06-05T18:15:00</GLPostDate>
        <IsFinalCharge>false</IsFinalCharge>
        <Job>
          <Type>Job</Type>
        </Job>
        <LocalAmount>-971.0000</LocalAmount>
        <LocalCurrency>
          <Code>AUD</Code>
          <Description>Australian Dollar</Description>
        </LocalCurrency>
        <LocalGSTVATAmount>0.0000</LocalGSTVATAmount>
        <LocalTotalAmount>-971.0000</LocalTotalAmount>
        <Organization>
          <Type>Organization</Type>
          <Key>AALSHI</Key>
        </Organization>
        <OSAmount>-971.00</OSAmount>
        <OSCurrency>
          <Code>AUD</Code>
          <Description>Australian Dollar</Description>
        </OSCurrency>
        <OSGSTVATAmount>0.00</OSGSTVATAmount>
        <OSTotalAmount>-971.0000</OSTotalAmount>
        <RevenueRecognitionType>IMM</RevenueRecognitionType>
        <Sequence>1</Sequence>
        <TransactionCategory>STD</TransactionCategory>
        <TransactionType>CST</TransactionType>

        <PostingJournalDetailCollection>
          <PostingJournalDetail>
            <CreditGLAccount>
              <AccountCode>8210.00.00</AccountCode>
              <Description>TRADE CREDITORS CONTROL</Description>
            </CreditGLAccount>
            <DebitGLAccount>
              <AccountCode>4900.00.00</AccountCode>
              <Description>RETAINED EARNINGS FROM PREVIOUS YR</Description>
            </DebitGLAccount>
            <PostingAmount>971.0000</PostingAmount>
            <PostingCurrency>
              <Code>AUD</Code>
              <Description>Australian Dollar</Description>
            </PostingCurrency>
            <PostingDate>2019-06-05T18:15:00</PostingDate>
            <PostingPeriod>202001</PostingPeriod>
          </PostingJournalDetail>
          <PostingJournalDetail>
            <CreditGLAccount>
              <AccountCode>4900.00.00</AccountCode>
              <Description>RETAINED EARNINGS FROM PREVIOUS YR</Description>
            </CreditGLAccount>
            <DebitGLAccount>
              <AccountCode>1210.20.10</AccountCode>
              <Description>WAREHOUSE HANDLING COSTS ACTUAL</Description>
            </DebitGLAccount>
            <PostingAmount>971.0000</PostingAmount>
            <PostingCurrency>
              <Code>AUD</Code>
              <Description>Australian Dollar</Description>
            </PostingCurrency>
            <PostingDate>2019-06-05T18:15:00</PostingDate>
            <PostingPeriod>202001</PostingPeriod>
          </PostingJournalDetail>
        </PostingJournalDetailCollection>
      </PostingJournal>
    </PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>");

			string responseBody;

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(xml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains("<Status>PRS</Status>", responseBody);

			AssertContains(@"<DataTargetCollection>
        <DataTarget>
          <Type>AccountingInvoice</Type>
          <Key>AP INV ART123</Key>
        </DataTarget>
      </DataTargetCollection>", responseBody);

			AssertContains("Successfully saved Unallocated Transaction", responseBody);
			AssertEquals(1, Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.TransactionsPendingAllocation)).Length);
		}

		public void TestProcessUniversalActivityRequest()
		{
			var workItem = (BusinessObject)Factory.New<IWorkItem>();
			workItem.FillWithValidTestData();
			workItem[WorkItemSchema.WKI_WorkItemNumber] = "WI12345678";
			workItem[WorkItemSchema.WKI_Summary] = "And that's the way the news goes.";

			Factory.Save();

			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var xml = FormattableString.Invariant($@"<UniversalActivityRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
				  <ActivityRequest>
				    <DataContext>
				      <DataTargetCollection>
				        <DataTarget>
				          <Type>WorkItem</Type>
				          <Key>WI12345678</Key>
				        </DataTarget>
				      </DataTargetCollection>

				      <Company>
				        <Code>{GlbCompany.CurrentCompany.GC_Code}</Code>
				        <Name>{GlbCompany.CurrentCompany.GC_Name}</Name>
				      </Company>
				      <EnterpriseID>{registrationKey.EnterpriseCode}</EnterpriseID>
				      <ServerID>{registrationKey.ServerCode}</ServerID>
				    </DataContext>
				  </ActivityRequest>
				</UniversalActivityRequest>");

			string responseBody;

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(xml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains(@"<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Activity>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>WorkItem</Type>
          <Key>WI12345678</Key>
        </DataSource>
      </DataSourceCollection>", responseBody);
		}

		[TestUtcOffset(10, 0, 0)]
		public void TestUniversalUTCTimeFilter()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00001001";
			shipment.JS_UniqueConsignRef = "S00001001";
			string currentTime = ZDateTime.UtcNow.AddMinutes(-1).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff");
			string futureTime = ZDateTime.UtcNow.AddMinutes(1).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff");
			string currentSysTime = ZDateTime.UtcNow.AddHours(-11).AddMinutes(-1).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff");

			var docManagerInfo = ((IDocManagerSupport)shipment).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "File1", "INV");

			docManagerInfo.MasterFactory.Save();
			Factory.Save();

			var responseXml = sender.SendRequest($@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <DocumentRequest>
        <DataContext>
            <DataTargetCollection>
                <DataTarget>
                    <Type>ForwardingShipment</Type>
                    <Key>S00001001</Key>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <FilterCollection>
            <Filter>
                <Type>SaveDateUTCFrom</Type>
                <Value>{currentTime}Z</Value>
            </Filter>
        </FilterCollection>
    </DocumentRequest>
</UniversalDocumentRequest>
").result;
			AssertNotContains("Time Zone Offset Detected and Ignored.</ProcessingLog>", responseXml);
			AssertNotContains("<FileName>File1</FileName>", responseXml);

			responseXml = sender.SendRequest($@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <DocumentRequest>
        <DataContext>
            <DataTargetCollection>
                <DataTarget>
                    <Type>ForwardingShipment</Type>
                    <Key>S00001001</Key>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <FilterCollection>
            <Filter>
                <Type>SaveDateUTCFrom</Type>
                <Value>{currentSysTime}Z</Value>
            </Filter>
        </FilterCollection>
    </DocumentRequest>
</UniversalDocumentRequest>
").result;
			AssertNotContains("Time Zone Offset Detected and Ignored.</ProcessingLog>", responseXml);
			AssertContains("<FileName>File1</FileName>", responseXml);

			responseXml = sender.SendRequest($@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <DocumentRequest>
        <DataContext>
            <DataTargetCollection>
                <DataTarget>
                    <Type>ForwardingShipment</Type>
                    <Key>S00001001</Key>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <FilterCollection>
            <Filter>
                <Type>SaveDateUTCFrom</Type>
                <Value>{currentTime}</Value>
            </Filter>
        </FilterCollection>
    </DocumentRequest>
</UniversalDocumentRequest>
").result;
			AssertNotContains("Time Zone Offset Detected and Ignored.</ProcessingLog>", responseXml);
			AssertContains("<FileName>File1</FileName>", responseXml);

			responseXml = sender.SendRequest($@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <DocumentRequest>
        <DataContext>
            <DataTargetCollection>
                <DataTarget>
                    <Type>ForwardingShipment</Type>
                    <Key>S00001001</Key>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <FilterCollection>
            <Filter>
                <Type>SaveDateUTCFrom</Type>
                <Value>{futureTime}</Value>
            </Filter>
        </FilterCollection>
    </DocumentRequest>
</UniversalDocumentRequest>
").result;
			AssertNotContains("Time Zone Offset Detected and Ignored.</ProcessingLog>", responseXml);
			AssertNotContains("<FileName>File1</FileName>", responseXml);

			responseXml = sender.SendRequest($@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <DocumentRequest>
        <DataContext>
            <DataTargetCollection>
                <DataTarget>
                    <Type>ForwardingShipment</Type>
                    <Key>S00001001</Key>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <FilterCollection>
            <Filter>
                <Type>SaveDateUTCFrom</Type>
                <Value>{currentTime}</Value>
            </Filter>
            <Filter>
                <Type>SaveDateUTCTo</Type>
                <Value>{futureTime}</Value>
            </Filter>
        </FilterCollection>
    </DocumentRequest>
</UniversalDocumentRequest>
").result;
			AssertNotContains("Time Zone Offset Detected and Ignored.</ProcessingLog>", responseXml);
			AssertContains("<FileName>File1</FileName>", responseXml);
		}

		public void TestUniversalUTCTimeWarning()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00001001";
			shipment.JS_UniqueConsignRef = "S00001001";
			string currentTime = ZDateTime.UtcNow.AddMinutes(-1).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fff");

			var docManagerInfo = ((IDocManagerSupport)shipment).DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "File1", "INV");

			docManagerInfo.MasterFactory.Save();
			Factory.Save();

			var responseXml = sender.SendRequest($@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <DocumentRequest>
        <DataContext>
            <DataTargetCollection>
                <DataTarget>
                    <Type>ForwardingShipment</Type>
                    <Key>S00001001</Key>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <FilterCollection>
            <Filter>
                <Type>SaveDateUTCFrom</Type>
                <Value>{currentTime}+{DateTimeOffset.Now.Offset.ToString("hh':'mm")}</Value>
            </Filter>
        </FilterCollection>
    </DocumentRequest>
</UniversalDocumentRequest>
").result;
			AssertContains("<ProcessingLog>Warning - SaveDateUTCFrom:Time Zone Offset Detected and Ignored.</ProcessingLog>", responseXml);
			AssertContains("<FileName>File1</FileName>", responseXml);

			responseXml = sender.SendRequest($@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
    <DocumentRequest>
        <DataContext>
            <DataTargetCollection>
                <DataTarget>
                    <Type>ForwardingShipment</Type>
                    <Key>S00001001</Key>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <FilterCollection>
            <Filter>
                <Type>SaveDateUTCFrom</Type>
                <Value>{currentTime}+8:00</Value>
            </Filter>
        </FilterCollection>
    </DocumentRequest>
</UniversalDocumentRequest>
").result;
			AssertContains("<ProcessingLog>Warning - SaveDateUTCFrom:Time Zone Offset Detected and Ignored.</ProcessingLog>", responseXml);
			AssertContains("<FileName>File1</FileName>", responseXml);
		}
		#endregion

		public void TestZDataExceptionHandledCorrectly()
		{
			var inboundXml =
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
			string responseBody = null;
			var saveCount = 0;
			var row = (Factory.New<DummyBusinessObject>() as INeedRow).Row;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				saveCount++;
				if (saveCount > 2)
				{
					var sqlException = SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(2601, byte.MaxValue, byte.MinValue, Core.Constants.ProductName, "Data failed to save because unique index conflict 'NR_UX__P9L_ParentId_P9L_ParentTableCode_P9L_P9T_TemplateTrigger'.", "", 1)
					));

					var exception = new ZSaveException(new ZDataException(sqlException, row, Db.Connection), Factory);
					throw exception;
				}
			});
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}
			AssertContains("A Save exception has occurred due to Unique Index Violation 'NR_UX__P9L_ParentId_P9L_ParentTableCode_P9L_P9T_TemplateTrigger', this may be a transient error and could possibly succeed on retry.", responseBody);
		}

		public void TestWin32Exception_10054()
		{
			//Win32Exception(10054, "An existing connection was forcibly closed by the remote host")

			var bizo = Factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			Factory.Save();

			var inboundXml = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>DDI</EventType>
  </Event>
</UniversalEvent>";

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				try
				{
					factory.New<DummyThatThrowsInConstructor>();
				}
				catch (ApplicationException)
				{
					throw;
				}
				catch { }
				Fail("Precondition: Expecting application exception");
			});

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					AssertEquals(HttpStatusCode.ServiceUnavailable, response.StatusCode);
					AssertContains("DB is currently not responsive: if self hosted, contact your administrators; if cloud, raise an incident", response.Content.ReadAsStringAsync().Result);
				}
			}
		}

		public void TestWin32Exception_258()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "SS00011826";
			shipment.JS_UniqueConsignRef = "SS00011826";
			Factory.Save();

			var inboundXml = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
 <Event>
 <DataContext>
  <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>{shipment.JS_UniqueConsignRef}</Key>
        </DataTarget>
      </DataTargetCollection>
     </DataContext>
 <EventTime>2019-12-06T12:12:00</EventTime>
 <EventType>Z03</EventType>
 </Event>
</UniversalEvent>";

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				throw new Win32Exception(258, "The wait operation timed out");
			});

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					AssertEquals(HttpStatusCode.ServiceUnavailable, response.StatusCode);
					AssertContains("DB is currently not responsive: if self hosted, contact your administrators; if cloud, raise an incident", response.Content.ReadAsStringAsync().Result);
					AssertEquals(0, ErrorReporter.TotalErrorCount);
				}
			}
		}

		public void TestEDIMessageRemovedBeforeNewFactory()
		{
			TestCaseHelper.ClearTable("EDIMessage");

			var inboundXml =
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

			var ediMessageNumber = "";

			var saveCount = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				saveCount++;

				if (saveCount == 3)
				{
					ediMessageNumber = factory.Load<EDIMessage>(new ZQuery())[0].EM_MessageNum;
					TestCaseHelper.ClearTable("EDIMessage");

					var winException = new Win32Exception(258, "The wait operation timed out");
					var error = SqlExceptionBuilder.CreateSqlError(-2, 1, 1, "", "Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 1);
					var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
					var exception = SqlExceptionBuilder.CreateSqlException(errorCollection, winException);
					throw exception;
				}
			});

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					AssertEquals(HttpStatusCode.InternalServerError, response.StatusCode);
					AssertContains(string.Format("Failed to update message [{0}] status to 'FAL' because it was deleted.", ediMessageNumber), response.Content.ReadAsStringAsync().Result);
				}
			}
		}

		public void TestNext_InvalidBranch_400()
		{
			var controller = new eAdaptorNextController();
			sender = new eAdaptorSenderHelper(controller, controller.Post);

			var ukCompany = (BusinessObject)Factory.New<IGlbCompany>();
			ukCompany[GlbCompanySchema.GC_Code.Name] = "PUK";
			ukCompany[GlbCompanySchema.GC_RN_NKCountryCode.Name] = "GB";

			var ukBranch = (BusinessObject)Factory.New<IGlbBranch>();
			ukBranch[GlbBranchSchema.GB_Code.Name] = "PUK";
			ukBranch[GlbBranchSchema.GB_GC.Name] = ukCompany.PK;
			ukBranch[GlbBranchSchema.GB_WebAddress] = "http://www.wisetechglobal.com/";
			Factory.Save();

			DataRegistry.Instance.WebBranch = ukBranch.PK.ToGuid();
			
			ukBranch.Delete();
			Factory.Save();

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
				AssertContains("Invalid Branch, you need to provide a valid branch in the System Registry under Web &gt; Web Branch", result.result);

				AssertEDIMessageCount(0);
			});
		}

		public void TestExceptionChangingRequestMessageStatus()
		{
			var ex = SqlExceptionBuilder.CreateSqlException(40, "Could not open a connection to SQL Server");
			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				if (factory.NameForDebugging == "Update Request Message Status")
				{
					throw ex;
				}
			});

			var inboundXml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Blah>
  </Blah>
</UniversalShipment>";

			var config = new DummyConfig()
			{
				Name = "eAdaptorTest",
				ContextSetter = eAdaptorConfig.Instance.ContextSetter,
				ResponseWriter = eAdaptorConfig.Instance.ResponseWriter,
				WebConfig = eAdaptorConfig.Instance.WebConfig,
				IsActive = false,
				ThrowIfNotActive = false,
				ThrowOnParsingError = true
			};

			AssertNoExceptionThrown(() => sender.SendRequest(inboundXml, config));
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals(ex, ErrorReporter.LastExceptionReported);
		}

		public void TestAggregateExceptionCaught()
		{
			var inboundXml =
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
					var error = SqlExceptionBuilder.CreateSqlError(111, 1, 1, "", "Test SQL Exception", "", 1);
					var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
					var exception = SqlExceptionBuilder.CreateSqlException(errorCollection, winException);
					throw exception;
				}
			});

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					AssertEquals(HttpStatusCode.ServiceUnavailable, response.StatusCode);
					AssertContains("DB is currently not responsive: if self hosted, contact your administrators; if cloud, raise an incident", response.Content.ReadAsStringAsync().Result);
					AssertEquals(1, ErrorReporter.TotalErrorCount);
				}
			}
		}

		public void TestErrorOnUserDefinedRoutineOrAggregate()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SS00011826";
			Factory.Save();

			var inboundXml = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
 <Event>
 <DataContext>
  <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>{shipment.JS_UniqueConsignRef}</Key>
        </DataTarget>
      </DataTargetCollection>
     </DataContext>
 <EventTime>2019-12-06T12:12:00</EventTime>
 <EventType>Z03</EventType>
 </Event>
</UniversalEvent>";

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
			{
				throw SqlExceptionBuilder.CreateSqlException(6522, "A.NET Framework error occurred during execution of user - defined routine or aggregate");
			});

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					AssertEquals(HttpStatusCode.GatewayTimeout, response.StatusCode);
					AssertContains("Try again later. If this problem persists, please contact your administrator", response.Content.ReadAsStringAsync().Result);
					AssertEquals(0, ErrorReporter.TotalErrorCount);
				}
			}
		}

		public void TestGetOneOffQuote()
		{
			var quotedBooking = ObjectFactory.Get<Freight.Integration.QuotedBooking.IQuotedBookingBuilder>().CreateNew(Freight.Integration.QuoteBookingType.SpotQuote, Factory);
			var quote = quotedBooking.Quote as Integration.Rating.IQuote;
			quote.TH_QuoteNumber = "QBNE00001052";

			Factory.Save();

			var xml = $@"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
			  <ShipmentRequest>
			    <DataContext>
			      <DataTargetCollection>
			        <DataTarget>
			         <Type>OneOffQuote</Type>
			          <Key>{quote.TH_QuoteNumber}</Key>
			        </DataTarget>
			      </DataTargetCollection>
			    </DataContext>
			  </ShipmentRequest>
			</UniversalShipmentRequest>";

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(xml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					var responseBody = response.Content.ReadAsStringAsync().Result;
					AssertContains($"<Type>OneOffQuote</Type>", responseBody);
					AssertContains($"<Key>{quote.TH_QuoteNumber}</Key>", responseBody);
				}
			}
		}

		public void TestCustomHeaders()
		{
			var factory = new BusinessObjectFactory();
			var inboundXml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

			var result = sender.SendRequest(inboundXml);

			var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			var responseMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));

			CombineAssertions(() =>
			{
				AssertResponseHeaderContains(result.headers, "eAdaptor-ReceiveEDIMessageNumber", requestMessage.EM_MessageNum);
				AssertResponseHeaderContains(result.headers, "eAdaptor-TransmitEDIMessageNumber", responseMessage.EM_MessageNum);
				AssertResponseHeaderContains(result.headers, "eAdaptor-MessageType", "XUS");
			});
		}

		void AssertRoutePrefixAttribute<T>(string expectedRoute)
			where T : ApiController
		{
			var route = Attribute.GetCustomAttribute(typeof(T), typeof(RoutePrefixAttribute));
			AssertNotNull(route);
			AssertEquals("Public endpoints should not be changed", expectedRoute, ((RoutePrefixAttribute)route).Prefix);
		}

		public void TestControllerRoutePrefix()
		{
			AssertRoutePrefixAttribute<eAdaptorNextController>("eAdaptorNext");
			AssertRoutePrefixAttribute<eAdaptorController>("eAdaptor");
		}

		void AssertPublicEndpoints<T>(bool isNext)
			where T : ApiController
		{
			var baseClassPublicMethods = typeof(eAdaptorControllerBase).GetMethods().Where(m => m.IsPublic);
			var publicMethods = typeof(T).GetMethods().Where(m => m.IsPublic);

			var actualCount = publicMethods.Count() - baseClassPublicMethods.Count();
			var expectedCount = isNext ? 5 : 2;

			AssertEquals("Expecting 4 public endpoints", expectedCount, actualCount);

			var getMethod = typeof(T).GetMethod("Get");
			AssertEquals(2, getMethod.GetCustomAttributes().Count());
			AssertEquals(true, getMethod.IsDefined(typeof(HttpGetAttribute), true));
			var getRoute = getMethod.GetCustomAttribute<RouteAttribute>();
			AssertEquals("Public endpoints should not be changed", "", getRoute.Template);

			var postMethod = typeof(T).GetMethod("Post");
			AssertEquals(2, postMethod.GetCustomAttributes().Count());
			AssertEquals(true, postMethod.IsDefined(typeof(HttpPostAttribute), true));
			var postRoute = postMethod.GetCustomAttribute<RouteAttribute>();
			AssertEquals("Public endpoints should not be changed", "", postRoute.Template);

			if (isNext)
			{
				var getAsyncMethod = typeof(T).GetMethod("GetAsynchronous");
				AssertEquals(2, getAsyncMethod.GetCustomAttributes().Count());
				AssertEquals(true, getAsyncMethod.IsDefined(typeof(HttpGetAttribute), true));
				var getAsyncRoute = getAsyncMethod.GetCustomAttribute<RouteAttribute>();
				AssertEquals("Public endpoints should not be changed", "Async", getAsyncRoute.Template);

				var postAsyncMethod = typeof(T).GetMethod("PostAsynchronous");
				AssertEquals(2, postAsyncMethod.GetCustomAttributes().Count());
				AssertEquals(true, postAsyncMethod.IsDefined(typeof(HttpPostAttribute), true));
				var postAsyncRoute = postAsyncMethod.GetCustomAttribute<RouteAttribute>();
				AssertEquals("Public endpoints should not be changed", "Async", postAsyncRoute.Template);
			}
		}

		public void TestAdaptorConfig()
		{
			AssertAdaptorConfig<eAdaptorConfig>(eAdaptorConfig.Instance, () => eAdaptorConfig.Instance, "eAdaptor", typeof(eAdaptorHttpResponseWriter));
			AssertAdaptorConfig<eAdaptorNextConfig>(eAdaptorNextConfig.Instance, () => eAdaptorNextConfig.Instance, "eAdaptorNext", typeof(eAdaptorNextHttpResponseWriter));
		}

		void AssertAdaptorConfig<T>(IeAdaptorConfig config,
			Func<IeAdaptorConfig> getInstance,
			string name,
			Type responseWriterType)
			where T : IeAdaptorConfig
		{
			AssertType<T>(config);

			AssertEquals(config, getInstance());

			AssertEquals(name, config.Name);
			AssertType(responseWriterType, config.ResponseWriter);
		}

		public void TestPublicEndpoints()
		{
			AssertPublicEndpoints<eAdaptorController>(false);
			AssertPublicEndpoints<eAdaptorNextController>(true);
		}

		public const string ExternalReferenceNumber = "8a64619e-9a9b-40b8-aa52-330511287dc6";

		public void TestExternalReferenceNumberInUniversalXml()
		{
			var factory = new BusinessObjectFactory();
			var inboundXml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
    <MessageNumberCollection>
      <MessageNumber Type=""External"">{ExternalReferenceNumber}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>";

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

		public void TestExternalReferenceNumberInNativeXml()
		{
			var factory = new BusinessObjectFactory();
			var inboundXml =
$@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
<Header>
  <MessageNumberCollection>
      <MessageNumber Type=""External"">{ExternalReferenceNumber}</MessageNumber>
  </MessageNumberCollection>
</Header>
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">AUSYD</Criteria>
      </CriteriaGroup>
    </UNLOCO>
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
			var factory = new BusinessObjectFactory();
			var inboundXml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
    <MessageNumberCollection>
      <MessageNumber Type=""External"">{ExternalReferenceNumber}</MessageNumber>
    </MessageNumberCollection>
  </Shipment>
</UniversalShipment>";

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

		public void TestEDIMessageTransportTypeForNativeXml()
		{
			var factory = new BusinessObjectFactory();
			var inboundXml =
$@"<Native xmlns=""http://www.cargowise.com/Schemas/Native"">
<Header>
  <MessageNumberCollection>
      <MessageNumber Type=""External"">{ExternalReferenceNumber}</MessageNumber>
  </MessageNumberCollection>
</Header>
  <Body>
    <UNLOCO>
      <CriteriaGroup Type=""Key"">
        <Criteria Entity=""RefUNLOCO"" FieldName=""Code"">AUSYD</Criteria>
      </CriteriaGroup>
    </UNLOCO>
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

		public void TestResponseReturnsDataContextCompanyWhenExceptionOccursDuringInternalPublishing()
		{
			var dataContextCompany = Factory.NewWithValidTestData<GlbCompany>();
			var shipmentBranch = Factory.NewWithValidTestData<GlbBranch>();
			shipmentBranch.GB_GC = dataContextCompany.PK;
			shipmentBranch.GB_IsActive = true;
			Factory.Save();

			var request = @$"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
	  <Company>
	    <Code>{dataContextCompany.GC_Code}</Code>
	  </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		  <Key/>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<Branch>
		<Code>{shipmentBranch.GB_Code}</Code>
	</Branch>
  </Shipment>
</UniversalShipment>";

			var exceptionThrown = false;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (f.NameForDebugging == MasterFilesIntegrationConstants.PublishUniversalXmlInternallyFactoryName)
				{
					exceptionThrown = true;
					throw new Exception();
				}
			});

			var response = sender.SendRequest(request).result;

			Assert("The exception was never thrown", exceptionThrown);
			AssertIsXml(response)
				.HavingExactlyOneChildNode("Data/UniversalEvent/Event/EventType", n => n.WithValue("DIF"))
				.HavingExactlyOneChildNode("Data/UniversalEvent/Event/DataContext/Company/Code", n => n.WithValue(dataContextCompany.GC_Code));
		}

		public void TestResponseReturnsWebBranchCompanyForInvalidDataContextCompany()
		{
			var shipmentBranch = Factory.NewWithValidTestData<GlbBranch>();
			shipmentBranch.GB_IsActive = true;
			Factory.Save();

			var request = @$"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
	  <Company>
	    <Code>AAA</Code>
	  </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		  <Key/>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<Branch>
		<Code>{shipmentBranch.GB_Code}</Code>
	</Branch>
  </Shipment>
</UniversalShipment>";

			var response = sender.SendRequest(request).result;

			var webBranch = Factory.Load<GlbBranch>(DataRegistry.Instance.WebBranch);
			AssertIsXml(response)
				.HavingExactlyOneChildNode("Data/UniversalEvent/Event/EventType", n => n.WithValue("DIF"))
				.HavingAtLeastOneChildNode("Data/UniversalEvent/Event/DataContext/Company/Code", n => n.WithValue(webBranch.Company.GC_Code));
		}

		public void TestResponseReturnsWebBranchCompanyForNoDataContextCompanyWhenExceptionOccursDuringInternalPublishing()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_IsActive = true;
			Factory.Save();

			var request = @$"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		  <Key/>
		</DataTarget>
	  </DataTargetCollection>
	</DataContext>
	<Branch>
		<Code>{branch.GB_Code}</Code>
	</Branch>
  </Shipment>
</UniversalShipment>";

			var exceptionThrown = false;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (f.NameForDebugging == MasterFilesIntegrationConstants.PublishUniversalXmlInternallyFactoryName)
				{
					exceptionThrown = true;
					throw new Exception();
				}
			});

			var response = sender.SendRequest(request).result;

			var webBranch = Factory.Load<GlbBranch>(DataRegistry.Instance.WebBranch);
			Assert("The exception was never thrown", exceptionThrown);
			AssertIsXml(response)
				.HavingExactlyOneChildNode("Data/UniversalEvent/Event/EventType", n => n.WithValue("DIF"))
				.HavingAtLeastOneChildNode("Data/UniversalEvent/Event/DataContext/Company/Code", n => n.WithValue(webBranch.Company.GC_Code));
		}

		public void TestVerifyEventTimeUtcFromCompanyWhenUniversalXmlEventTimeDoesNotContainOffset()
		{
			var dataContextCompany = Factory.NewWithValidTestData<GlbCompany>();
			var shipmentBranch = Factory.NewWithValidTestData<GlbBranch>();
			shipmentBranch.GB_GC = dataContextCompany.PK;
			shipmentBranch.GB_RL_NKHomePort = "DEFRA";
			shipmentBranch.GB_IsActive = true;

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00001001";
			shipment.JS_UniqueConsignRef = "S00001001";
			Factory.Save();

			var eventDateTimeGermany = new ZDateTime(2019, 12, 06, 16, 0, 0);
			var universalEvent = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>{shipment.JS_UniqueConsignRef}</Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
				<Code>{dataContextCompany.GC_Code}</Code>
			</Company>
		</DataContext>
		<EnterpriseID>EDI</EnterpriseID>
		<ServerID>DAT</ServerID>
		<EventTime>{eventDateTimeGermany.ToISO8601String()}</EventTime>
		<EventType>PCF</EventType>
		<EventReference>Actual Full Pick Up Completed</EventReference>
		<IsEstimate>false</IsEstimate>
	</Event>
</UniversalEvent>";
			_ = sender.SendRequest(universalEvent).result;
			var log = Factory.Load<IStmALog>(new ZQuery()).Where(log => log.SL_Parent == shipment.PK && log.SL_SE_NKEvent == "PCF").First();
			var eventTimeUTC = EnvProxy.Instance.Time.GetUtcFromUnlocoTime(shipmentBranch.GB_RL_NKHomePort, eventDateTimeGermany.ToDateTime());
			AssertEquals("SL_EventTimeUtc", eventTimeUTC, log.SL_EventTimeUtc.ToDateTime());
			AssertEquals("SL_EventTime", eventDateTimeGermany, log.SL_EventTime);
		}

		public void TestVerifyEventTimeUtcFromEventBranchWhenUniversalXmlEventTimeDoesNotContainOffset()
		{
			var shipmentBranch = Factory.NewWithValidTestData<GlbBranch>();
			shipmentBranch.GB_RL_NKHomePort = "DEFRA";
			shipmentBranch.GB_IsActive = true;

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00001001";
			shipment.JS_UniqueConsignRef = "S00001001";
			Factory.Save();

			var eventDateTimeGermany = new ZDateTime(2019, 12, 06, 16, 0, 0);
			var universalEvent = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>{shipment.JS_UniqueConsignRef}</Key>
				</DataTarget>
			</DataTargetCollection>
			<EventBranch>
				<Code>{shipmentBranch.GB_Code}</Code>
			</EventBranch>
		</DataContext>
		<EnterpriseID>EDI</EnterpriseID>
		<ServerID>DAT</ServerID>
		<EventTime>{eventDateTimeGermany.ToISO8601String()}</EventTime>
		<EventType>PCF</EventType>
		<EventReference>Actual Full Pick Up Completed</EventReference>
		<IsEstimate>false</IsEstimate>
	</Event>
</UniversalEvent>";
			_ = sender.SendRequest(universalEvent).result;
			var log = Factory.Load<IStmALog>(new ZQuery()).Where(log => log.SL_Parent == shipment.PK && log.SL_SE_NKEvent == "PCF").First();
			var eventTimeUTC = EnvProxy.Instance.Time.GetUtcFromUnlocoTime(shipmentBranch.GB_RL_NKHomePort, eventDateTimeGermany.ToDateTime());
			AssertEquals("SL_EventTimeUtc", eventTimeUTC, log.SL_EventTimeUtc.ToDateTime());
			AssertEquals("SL_EventTime", eventDateTimeGermany, log.SL_EventTime);
		}

		public void TestVerifyEventTimeUtcWhenUniversalXmlEventTimeContainsOffset()
		{
			var shipmentBranch = Factory.NewWithValidTestData<GlbBranch>();
			shipmentBranch.GB_RL_NKHomePort = "DEFRA";
			shipmentBranch.GB_IsActive = true;

			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00001001";
			shipment.JS_UniqueConsignRef = "S00001001";
			Factory.Save();

			var eventDateTimeOffsetGermany = new ZDateTimeOffset(2019, 12, 06, 16, 0, 0, new TimeSpan(1, 0, 0));
			var universalEvent = $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingShipment</Type>
					<Key>{shipment.JS_UniqueConsignRef}</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EnterpriseID>EDI</EnterpriseID>
		<ServerID>DAT</ServerID>
		<EventTime>{eventDateTimeOffsetGermany.ToISO8601String()}</EventTime>
		<EventType>PCF</EventType>
		<EventReference>Actual Full Pick Up Completed</EventReference>
		<IsEstimate>false</IsEstimate>
	</Event>
</UniversalEvent>";
			_ = sender.SendRequest(universalEvent).result;
			var log = Factory.Load<IStmALog>(new ZQuery()).Where(log => log.SL_Parent == shipment.PK && log.SL_SE_NKEvent == "PCF").First();
			var eventTimeUTC = EnvProxy.Instance.Time.GetUtcFromUnlocoTime(shipmentBranch.GB_RL_NKHomePort, eventDateTimeOffsetGermany.ToDateTime());
			AssertEquals("SL_EventTimeUtc", eventTimeUTC, log.SL_EventTimeUtc.ToDateTime());
			AssertEquals("SL_EventTime", eventDateTimeOffsetGermany.ToZDateTime(), log.SL_EventTime);
		}

		class DummyThatThrowsInConstructor : DummyBusinessObject
		{
			public DummyThatThrowsInConstructor(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
				throw new Win32Exception(10054, "An existing connection was forcibly closed by the remote host");
			}
		}

		class PaddlingPool : IConnectionPooling
		{
			public bool IsPooling => true;

			public int MaxPoolSize => 2;

			public int MinPoolSize => 1;

			public int LoadBalanceTimeout => 0;
		}

		class TestEnvironment : BaseDbEnvironment
		{
			public override int ConnectionTimeout => 1;
			public override IConnectionPooling ConnectionPooling => new PaddlingPool();
		}

		INotificationHandler notificationHandler;

		eAdaptorSenderHelper sender;

		protected override void SetUp()
		{
			var controller = new eAdaptorController();
			sender = new eAdaptorSenderHelper(controller, controller.Post);
			notificationHandler = NotificationHandler.Instance;
			NotificationHandler.Instance = null;
			var scheduleUser = Factory.New<GlbStaff>();
			scheduleUser.GS_Code = "UR2";
			scheduleUser.GS_LoginName = "User2";
			scheduleUser.GS_EmailAddress = "user2@mail.box";
			Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = 'example@example', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_CODE = 'PM'");
			Factory.Save();

			base.SetUp();
		}

		protected override void TearDown()
		{
			NotificationHandler.Instance = notificationHandler;

			base.TearDown();
		}
	}

	public class DummyConfig : IeAdaptorConfig
	{
		public string Name { get; set; }

		public IeAdaptorContextSetter ContextSetter { get; set; }

		public IWebConfig WebConfig { get; set; }

		public IeAdaptorHttpResponseWriter ResponseWriter { get; set; }

		public bool IsActive { get; set; }

		public bool ThrowIfNotActive { get; set; }

		public bool ThrowOnParsingError { get; set; }
	}

	class WrappyStream : Stream
	{
		public WrappyStream(Stream stream, Action<byte[], int, int> onRead = null)
		{
			this.stream = stream;
			this.onRead = onRead ?? new Action<byte[], int, int>((x, y, z) => { });
		}
		readonly Action<byte[], int, int> onRead;
		readonly Stream stream;
		public override bool CanRead => stream.CanRead;
		public override bool CanSeek => stream.CanSeek;
		public override bool CanWrite => stream.CanWrite;
		public override long Length => stream.Length;

		public override long Position
		{
			get => stream.Position;
			set => stream.Position = value;
		}

		public override void Flush() => stream.Flush();
		public override int Read(byte[] buffer, int offset, int count)
		{
			onRead(buffer, offset, count);
			return stream.Read(buffer, offset, count);
		}
		public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
		public override void SetLength(long value) => stream.SetLength(value);
		public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);
	}

	[UseSnapshotProtection]
	public class eAdaptorControllerTest_NonTransactioned : TestCase
	{
		INotificationHandler notificationHandler;

		eAdaptorSenderHelper sender;

		protected override void SetUp()
		{
			var controller = new eAdaptorController();
			sender = new eAdaptorSenderHelper(controller, controller.Post);

			notificationHandler = NotificationHandler.Instance;
			NotificationHandler.Instance = null;
			ExceptionReporter.Instance.TestingDoReportException.Value = true;
			Globals.IsUserInteractive = false;
			Globals.SetIsUnitTestingProductionFunctionality(true);
			Globals.IsWeb = true;
			base.SetUp();
		}

		protected override void TearDown()
		{
			NotificationHandler.Instance = notificationHandler;
			Globals.IsUserInteractive = true;
			Globals.SetIsUnitTestingProductionFunctionality(false);
			Globals.IsWeb = false;
			base.SetUp();
		}

		public void TestRetryOnDatabaseDeadlockException()
		{
			var factory = new BusinessObjectFactory();
			var inboundXml =
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
			var row = (factory.New<DummyBusinessObject>() as INeedRow).Row;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				saveCount++;
				if (saveCount == 3)
				{
					var sqlException = SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(1205, byte.MaxValue, byte.MinValue, Core.Constants.ProductName, "Transaction (Process ID 1404) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.", "", 1)
					));

					var exception = new ZSaveException(new ZDataException(sqlException, row, Db.Connection), factory);
					throw exception;
				}
			});

			var responseBody = sender.SendRequest(inboundXml).result;

			AssertEquals("No. of saves to process post request", 7, saveCount);
		}

		public void TestDoNotErrorReportWhenRetryingMessageWithTransientErrors()
		{
			var factory = new BusinessObjectFactory();
			var inboundXml = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
		  <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
      <Company>
        <Code>EDI</Code>
      </Company>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <EventTime>2020-09-08T14:03:08Z</EventTime>
    <EventType>Z00</EventType>
  </Event>
</UniversalEvent>";

			var bizo = factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			factory.Save();

			var shouldThrowError = true;
			var exceptionMessage = "Transaction (Process ID 1404) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.";

			UniversalXmlWorkflowProcessor.SetPreProcessActionHook(() =>
			{
				if (shouldThrowError)
				{
					shouldThrowError = false;
					var sqlException = SqlExceptionBuilder.CreateSqlException(1205, byte.MaxValue, byte.MinValue, null, exceptionMessage, "", 1);
					throw new ZSaveException(new ZDataException(sqlException, null, Db.Connection), null);
				}
			});

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					_ = response.Content.ReadAsStringAsync().Result;
				}
			}

			var updatedShipmentWorkflow = (IWorkflowProvider)factory.Load(ObjectFactory.GetType<Forwarding.IForwardingShipment>(), bizo.PK);

			AssertCollectionContains("Added event", updatedShipmentWorkflow.Logs.GetAllLogs(), log => ((StmALog)log).Event.SE_Code == "Z00");
			AssertCollectionNotContains("No error report", ErrorReporter.ExceptionsThrown, ex => ex.Contains(exceptionMessage));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[UseSnapshotProtection]
		public void TestImportUniversalShipment_UniqueIndexViolation()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<Forwarding.IForwardingConsol>();
			var container1 = factory.New<Forwarding.IForwardingContainer>();
			var container2 = factory.New<Forwarding.IForwardingContainer>();
			container1.JC_JK = consol.PK;
			container2.JC_JK = consol.PK;
			container2.JC_ContainerJobID = "D00001001"; //cause constraint violation by bypassing number fountain

			factory.Save();

			AssertEquals("D00001001", container2.JC_ContainerJobID);
			AssertEquals("D00001000", container1.JC_ContainerJobID);

			string responseBody = null;
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Freight\Forwarding\Forwarding.DataTransfer.Test\Universal\Consol\TestFiles\UniversalShipment.xml"));
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			var reloadFactory = new BusinessObjectFactory();
			var messages = reloadFactory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));

			AssertEquals("Expecting 1 request message", 1, messages.Length);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, messages[0].EM_Status);

			var containers = factory.Load<Forwarding.IForwardingContainer>(new ZQuery());

			AssertEquals(3, containers.Length);
			AssertEquals("Data Note Added", 1, messages[0].DataImportLogNoteCount);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "D00001000", "D00001001", "D00001002" },
				containers.Select(c => c.JC_ContainerJobID));
		}

		public void TestTransactionException_OnEverySave()
		{
			var factory = new BusinessObjectFactory();
			var bizo = factory.New<Forwarding.IForwardingShipment>();
			bizo.JS_UniqueConsignRef = "S00001001";
			factory.Save();

			var save1response = eAdaptorControllerTest.ImportEdoc_AndThrowZSaveException(factory, saveNumber: 1);
			var save2response = eAdaptorControllerTest.ImportEdoc_AndThrowZSaveException(factory, saveNumber: 2);
			var save3response = eAdaptorControllerTest.ImportEdoc_AndThrowZSaveException(factory, saveNumber: 3);
			var save4response = eAdaptorControllerTest.ImportEdoc_AndThrowZSaveException(factory, saveNumber: 4);
			var save5response = eAdaptorControllerTest.ImportEdoc_AndThrowZSaveException(factory, saveNumber: 5);

			CombineAssertions("Saves inside the delayed transaction can recover on retry)", () =>
			{
				AssertContains("Save1: eAdaptorController.Post", "Successfully Added eDoc", save1response);
				AssertContains("Save2: UniversalXmlWorkflowProcessor.SendUniversalXmlInternally (saving message)", "Successfully Added eDoc", save2response);
				AssertContains("Save3: UniversalXmlImportHandler.ProcessDataObject parent factory", "Successfully Added eDoc", save3response);
				AssertContains("Save4: UniversalXmlImportHandler.ProcessDataObject child factory", "Successfully Added eDoc", save4response);
				AssertContains("Save5: eAdaptorController.Post", "Successfully Added eDoc", save5response);
			});

			var reloadBizo = new BusinessObjectFactory().Load<Forwarding.IForwardingShipment>(bizo.PK);
			var savedEDocs = ((IDocManagerSupport)reloadBizo).DocManagerInfo.Files.Count;
			AssertEquals("5 documents were saved successfully", 5, savedEDocs);

			var saves = eAdaptorControllerTest.ImportEdoc_AndThrowZSaveException(factory, 0, returnSaveCount: true);
			AssertEquals("If you have reduced the number of saves you will need to update this test, you shouldn't be adding more saves", "5", saves);
		}

		public void TestPostHandlesConcurrencyExceptionOnRequestSave()
		{
			var factory = new BusinessObjectFactory();
			var inboundXml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

			var count = 0;
			var row = (factory.New<DummyBusinessObject>() as INeedRow).Row;

			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (f.NameForDebugging == "Request Message")
				{
					count++;
					if (count == 2)
					{
						throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new TransactionException("Concurrency can suck.", OdysseyDataErrorType.TransactionRolledBack), row, Db.Connection), factory);
					}
				}
			});

			var responseBody = sender.SendRequest(inboundXml).result;

			AssertContains("Successfully saved Shipment", responseBody);
			var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, requestMessage.EM_Status);
		}

		public void TestPostHandlesConcurrencyExceptionOnRetrySave()
		{
			var factory = new BusinessObjectFactory();
			var inboundXml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

			var count = 0;
			var row = (factory.New<DummyBusinessObject>() as INeedRow).Row;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (f.NameForDebugging == "Update Request Message Status")
				{
					// Throw a transient exception
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new TransactionException("Concurrency can suck.", OdysseyDataErrorType.TransactionRolledBack), row, Db.Connection), factory);
				}

				if (f.NameForDebugging == "Request Message")
				{
					count++;
					if (count == 2)
					{
						// Throw a transient exception on every second save.
						throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new TransactionException("Concurrency can suck.", OdysseyDataErrorType.TransactionRolledBack), row, Db.Connection), factory);
					}
				}
			});

			var responseBody = sender.SendRequest(inboundXml).result;

			AssertContains("Successfully saved Shipment", responseBody);
			var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, requestMessage.EM_Status);
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			var messages = factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(1, messages.Length);

			ErrorReporter.Clear();
		}

		public void TestPostHandlesConcurrencyExceptionOnResponseSave()
		{
			var factory = new BusinessObjectFactory();
			var inboundXml =
				$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>
  </Shipment>
</UniversalShipment>";

			var row = (factory.New<DummyBusinessObject>() as INeedRow).Row;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (f.NameForDebugging == "Response Message")
				{
					// Throw a transient exception on every second save.
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new TransactionException("Concurrency can suck.", OdysseyDataErrorType.TransactionRolledBack), row, Db.Connection), factory);
				}
			});

			var responseBody = sender.SendRequest(inboundXml).result;

			AssertContains("Save Aborted Due to Concurrency Check", responseBody);

			var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			AssertEquals(EDIMessageStatusList.Codes.Failed, requestMessage.EM_Status);
			var notes = factory.Load<StmNote>(new ZQuery()).Where(x => x.ST_Table == "EDIMessage");
			AssertEquals(1, notes.Count());
			AssertContains("Processing stream after 3 retries.", notes.First().ST_NoteDataAsText);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUniversalTransaction_IndexViolationExceptionHandling()
		{
			var factory = new BusinessObjectFactory();

			var xmlPath = @"Enterprise\Product\Operations\Accounting\Accounting.ElectronicMessaging.Testing\Turkey\EInvoiceXmlWriter\Xml\UniversalTransactionSEA.xml";
			var text = File.ReadAllText(BaseSourcePath + xmlPath);

			using (var stream = new VirtualMemoryStream())
			using (var wr = new StreamWriter(stream, Encoding.UTF8, 100, true))
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				wr.Write(text
					.Replace("<ServerID>TST</ServerID>", "")
					.Replace("<EnterpriseID>LAO</EnterpriseID>", ""));
				wr.Flush();
				stream.Position = 0;
				request.Content = new StreamContent(new WrappyStream(stream));
				controller.Request = request;
				using (var response = controller.Post())
				{
					AssertContains("<Status>PRS</Status>", response.Content.ReadAsStringAsync().Result);
				}

				Db.Connection.ExecuteNonQuery("DELETE FROM dbo.EDIMessage");

				using (var response = controller.Post())
				{
					AssertContains("<Status>PRS</Status>", response.Content.ReadAsStringAsync().Result);
					AssertContains("duplicate Transaction Pending Allocation", response.Content.ReadAsStringAsync().Result);
					var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
					AssertEquals(EDIMessageStatusList.Codes.Discarded, requestMessage.EM_Status);
				}
			}
		}

		public void TestUniversalDocumentRequestsReturnsWarningWhenNoDocumentReturned()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "S00001001";
			shipment.JS_UniqueConsignRef = "S00001001";

			factory.Save();

			var requestXml = $@"<UniversalDocumentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <DocumentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>S00001001</Key>
        </DataTarget>
      </DataTargetCollection>
      <DataProvider>EDIDATDEM</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
    <FilterCollection></FilterCollection>
  </DocumentRequest>
</UniversalDocumentRequest>";

			string responseBody = null;
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(requestXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			CombineAssertions(() =>
			{
				var messages = factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
				AssertEquals(1, messages.Length);
				AssertXMLEquals("Response Message", $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalResponse version=""1.1"" xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
  <Status>PRS</Status>
  <Data />
  <MessageNumberCollection>
    <MessageNumber Type=""MessageNumber"">{messages[0].EM_MessageNum}</MessageNumber>
  </MessageNumberCollection>
  <ProcessingLog>Warning - Data source was found, but no eDocs were found matching the filters specified.</ProcessingLog>
</UniversalResponse>", responseBody);
				var requestMessage = new BusinessObjectFactory().LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
				AssertEquals(EDIMessageStatusList.Codes.Warning, requestMessage.EM_Status);
			});
		}

		public void TestCompanyContextSwitching()
		{
			var factory = new BusinessObjectFactory();
			var company = factory.New<GlbCompany>();
			company.GC_Code = "ABC";
			company.FillWithValidTestData();
			company.Branches.AddNew().FillWithValidTestData();
			var org = factory.LoadTop1<OrgHeader>(new ZQuery());
			company.GC_OH_OrgProxy = org.PK;
			var match = company.OrgProxy.CreatePatternMatchOverrideForTest();
			match.OO_ForeignCode = "MatchDisSuka";
			match.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			match.OO_LocalCode = org.OH_Code;
			match.OO_LocalGuid = org.PK;
			factory.Save();
			var inboundXml = @"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key/>
        </DataTarget>
      </DataTargetCollection>
      <DataProvider>ULVTSTWTF</DataProvider>
      <EnterpriseID>ULV</EnterpriseID>
      <ServerID>TST</ServerID>
      <Company><Code>WTF</Code></Company>
    </DataContext>
  </Shipment>
</UniversalShipment>";
			string responseBody = null;

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			var messagesQuery = new ZQuery { ReLoadExistingRows = true };
			messagesQuery.OrderBy = EDIMessageSchema.EM_ApplicationCode.Name + ", " + EDIMessageSchema.EM_ReceiveTransmit.Name;
			var message = factory.LoadTop1<IEDIMessage>(messagesQuery);
			AssertNotNull("message reloaded", message);
			AssertEquals("Right Message Type - App|Dir|Typ|Status", "UDQ RCV XUS REJ", message.EM_ApplicationCode + " " + message.EM_ReceiveTransmit + " " + message.EM_MessageSubType + " " + message.EM_Status);

			AssertNotContains("responseBody", "<DataProvider>ULVTSTWTF</DataProvider>", responseBody);
			AssertContains("responseBody", @"DataContext ServerId was specified, however it does not match the system ServerId.", responseBody);
			AssertContains("responseBody", @"DataContext EnterpriseId was specified, however it does not match the system EnterpriseId.", responseBody);

			var newInboundXml = $@"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key/>
        </DataTarget>
      </DataTargetCollection>
      <DataProvider>{company.LicenceEnterpriseCode}{company.LicenceServerID}{company.GC_Code}</DataProvider>
      <EnterpriseID>{company.LicenceEnterpriseCode}</EnterpriseID>
      <ServerID>{company.LicenceServerID}</ServerID>
      <Company><Code>{company.GC_Code}</Code></Company>
    </DataContext>
    <OrganizationAddressCollection>
		<OrganizationAddress>
			<AddressType>ImporterDocumentaryAddress</AddressType>
			<OrganizationCode>MatchDisSuka</OrganizationCode>
		</OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(newInboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					responseBody = response.Content.ReadAsStringAsync().Result;
				}
			}

			AssertContains("responseBody", $"<DataProvider>{company.LicenceEnterpriseCode}{company.LicenceServerID}{company.GC_Code}</DataProvider>", responseBody);
			AssertContains("responseBody - Mapping", $"Mapped Organisation code 'MatchDisSuka' to '{org.OH_Code}'.", responseBody);
			AssertNotContains("responseBody", @"DataContext ServerId was specified, however it does not match the system ServerId.
DataContext EnterpriseId was specified, however it does not match the system EnterpriseId.", responseBody);
		}

		public void TestDuplicateContainerUpdatesMessageStatus()
		{
			var factory = new BusinessObjectFactory();
			var messages = factory.Load<IEDIMessage>(new ZQuery());
			AssertEquals("Precondition: No existing messages", 0, messages.Length);

			var consol = factory.New<CommonConsol>();
			consol.FillWithValidTestData();
			consol.JK_UniqueConsignRef = "C12345678";

			factory.Save();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(fac =>
			{
				if (fac.NameForDebugging == MasterFilesIntegrationConstants.PublishUniversalXmlInternallyFactoryName)
				{
					var factory2 = new BusinessObjectFactory();
					var consol2 = factory2.Load<CommonConsol>(consol.PK);
					var container1 = consol2.Containers.AddNew();
					container1.JC_ContainerNum = "CON1";
					factory2.Save();
				}
			});

			var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
   <Shipment>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>ForwardingConsol</Type>
					<Key>C12345678</Key>
				</DataTarget>
			</DataTargetCollection>
			<Company>
			<Code>DEM</Code>
			<Country>
				<Code>AU</Code>
				<Name>Australia</Name>
			</Country>
			<Name>Demo Company</Name>
			</Company>
			<DataProvider>EDIDATDEM</DataProvider>
			<EnterpriseID>EDI</EnterpriseID>
			<ServerID>DAT</ServerID>
		</DataContext>
		<ContainerCollection>
			<Container>
				<ContainerNumber>CON1</ContainerNumber>
			</Container>
		</ContainerCollection>
	</Shipment>
</UniversalShipment>";

			var responseBody = sender.SendRequest(xml).result;

			AssertContains("Error Saving", responseBody);

			var requestMessage = factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive));
			var responseMessage = factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit));
			AssertEquals(EDIMessageStatusList.Codes.Rejected, requestMessage.EM_Status);
			AssertEquals(EDIMessageStatusList.Codes.Sent, responseMessage.EM_Status);
			var notes = factory.Load<StmNote>(new ZQuery()).Where(x => x.ST_Table == "EDIMessage");
			AssertEquals(1, notes.Count());
			AssertContains("Error Saving", notes.First().ST_NoteDataAsText);
		}

		public void TestPostCorrectlyLinksRequestMessageToResponseMessage()
		{
			var requestXml = @"<UniversalShipmentRequest xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <ShipmentRequest>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
          <Key>SS00011826</Key>
        </DataTarget>
      </DataTargetCollection>
      <EnterpriseID>EDI</EnterpriseID>
      <ServerID>DAT</ServerID>
    </DataContext>
  </ShipmentRequest>
</UniversalShipmentRequest>";

			var currentDateTime = ZDateTime.UtcNow;

			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(requestXml);
				controller.Request = request;
				using (controller.Post())
				{
					var factory = new BusinessObjectFactory();
					var requestMessage = factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive).AddToFilter(new ZQuery(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, currentDateTime)));
					AssertNotNull(factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit).AddToFilter(new ZQuery(EDIMessageSchema.EM_EM_RequestMessage, requestMessage.PK))));
				}
			}
		}

		public void TestPostReturnsErrorOnUnexpectedException()
		{
			var inboundXml =
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
			var currentDateTime = ZDateTime.UtcNow;
			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (f.NameForDebugging == "Publish Universal Xml Internally")
				{
					throw new NullReferenceException();
				}
			});
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					var responseBody = response.Content.ReadAsStringAsync().Result;
					var factory = new BusinessObjectFactory();
					var requestMessage = factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive).AddToFilter(new ZQuery(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, currentDateTime)));

					AssertContains("<Status>ERR</Status>", responseBody);
					AssertContains("<Type>FailureReason</Type>", responseBody);
					AssertContains("<Value>System.NullReferenceException: Object reference not set to an instance of an object.", responseBody);
					AssertNotNull(requestMessage);
					AssertEquals(EDIMessageStatusList.Codes.Rejected, requestMessage.EM_Status);
					AssertNotNull(factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit).AddToFilter(new ZQuery(EDIMessageSchema.EM_EM_RequestMessage, requestMessage.PK))));
				}
			}

			BusinessObjectFactory.SetOnFactorySaveHookForTest(f =>
			{
				if (f.NameForDebugging == "Publish Universal Xml Internally")
				{
					throw new InvalidOperationException();
				}
			});
			using (var request = new HttpRequestMessage())
			using (var controller = new eAdaptorController())
			{
				request.Content = new StringContent(inboundXml);
				controller.Request = request;
				using (var response = controller.Post())
				{
					var responseBody = response.Content.ReadAsStringAsync().Result;
					var factory = new BusinessObjectFactory();
					var requestMessage = factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Receive).AddToFilter(new ZQuery(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, currentDateTime)));

					AssertContains("<Status>ERR</Status>", responseBody);
					AssertContains("<Type>FailureReason</Type>", responseBody);
					AssertContains("<Value>System.InvalidOperationException: Operation is not valid due to the current state of the object.", responseBody);
					AssertNotNull(requestMessage);
					AssertEquals(EDIMessageStatusList.Codes.Rejected, requestMessage.EM_Status);
					AssertNotNull(factory.LoadTop1<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit).AddToFilter(new ZQuery(EDIMessageSchema.EM_EM_RequestMessage, requestMessage.PK))));
				}
			}
		}
	}
}
