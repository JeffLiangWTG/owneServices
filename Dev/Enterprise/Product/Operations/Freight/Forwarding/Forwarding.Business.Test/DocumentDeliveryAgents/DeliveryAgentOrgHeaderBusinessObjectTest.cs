using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(DeliveryAgentOrgHeader))]
	sealed class DeliveryAgentOrgHeaderBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var org = factory.New<DeliveryAgentOrgHeader>();
			org.OH_Code = "OrgForDelete";
			return org;
		}

		public void TestBusinessContext()
		{
			DeliveryAgentOrgHeader agentHeader = Factory.New<DeliveryAgentOrgHeader>();
			AssertEquals("DeliveryAgent Business Context", BusinessContext.DeliveryAgent, agentHeader.DocumentSupporter.BusinessContext);
		}

		public void TestSupportedDataContexts()
		{
			DeliveryAgentOrgHeader agentHeader = Factory.New<DeliveryAgentOrgHeader>();
			AssertEquals("Core.Constants.DataContext.Consol is Supported", true, agentHeader.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Consol)));
			AssertEquals("Core.Constants.DataContext.ARInvoice is Supported", true, agentHeader.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ARInvoice)));
		}

		public void TestSupportedChildBusinessContexts()
		{
			DeliveryAgentOrgHeader agentHeader = Factory.New<DeliveryAgentOrgHeader>();
			BusinessContext[] result = agentHeader.DocumentSupporter.SupportedChildBusinessContexts;
			AssertEquals("One Supported Child Context", 1, result.Length);
			AssertEquals("Supported context is shipment", BusinessContext.Shipment, result[0]);
		}

		public void TestGetDocBusinessObjectsForConsol()
		{
			DeliveryAgentOrgHeader agentHeader = Factory.New<DeliveryAgentOrgHeader>();
			AssertNull("No wrapper for consol", agentHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Consol, null));

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			agentHeader.LoadShipmentCollection(consol);

			DocumentWrapper[] wrapper = agentHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Consol, null);
			AssertNotNull("One wrapper", wrapper);
			AssertEquals("Type of wrapper is DocConsol", "DocForwardingConsol", wrapper[0].GetType().Name);
			AssertEquals("Interface Type of DocConsol wrapper is IConsolDeliveryAgent", typeof(IConsolDeliveryAgent), wrapper[0].GetType().GetInterface("IConsolDeliveryAgent"));
		}

		public void TestGetDocBusinessObjectForARInvoice()
		{
			DeliveryAgentOrgHeader agentHeader = Factory.New<DeliveryAgentOrgHeader>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C1";
			agentHeader.LoadShipmentCollection(consol);
			DocumentWrapper[] wrapper = agentHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ARInvoice, null);
			AssertNull("ARInvoice wrapper array is null", wrapper);

			AccTransactionHeader header = Factory.New<AccTransactionHeader>();
			header.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			header.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			header.AH_OH = agentHeader.PK;
			header.AH_GC = GlbCompany.CurrentCompany.PK;
			header.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
			wrapper = agentHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ARInvoice, null);
			AssertEquals("ARInvoice wrapper should be created", 1, wrapper.Length);
			AssertNotNull("ARInvoice wrapper should be created", wrapper[0]);
			AssertEquals("Wrapper type should be DocARInvoice", "DocARInvoice", wrapper[0].GetType().Name);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			AccTransactionHeader header2 = Factory.New<AccTransactionHeader>();
			header2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			header2.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			header2.AH_OH = agentHeader.PK;
			header2.AH_JH = job.PK;
			header2.AH_GC = GlbCompany.CurrentCompany.PK;
			header2.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
			agentHeader.LoadShipmentCollection(consol);
			agentHeader.Shipments.Add(shipment);
			wrapper = agentHeader.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.ARInvoice, null);
			AssertEquals("ARInvoice wrapper should be created", 2, wrapper.Length);
			AssertNotNull("ARInvoice wrapper should be created", wrapper[0]);
			AssertNotNull("ARInvoice wrapper should be created", wrapper[1]);
			AssertEquals("Wrapper type should be DocARInvoice", "DocARInvoice", wrapper[0].GetType().Name);
			AssertEquals("Wrapper type should be DocARInvoice", "DocARInvoice", wrapper[1].GetType().Name);
		}

		public void TestLoadShipmentCollection()
		{
			DeliveryAgentOrgHeader agentHeader = Factory.New<DeliveryAgentOrgHeader>();
			AssertNull("No Shipments for consol", agentHeader.Shipments);

			DeliveryAgentOrgHeader deliveryAgent = Factory.New<DeliveryAgentOrgHeader>();
			deliveryAgent.OH_Code = "CCC";
			deliveryAgent.OH_FullName = "Delivery Agent";
			deliveryAgent.MainAddress.OA_Address1 = "Address 1";

			ForwardingConsol consolBisObj = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consolBisObj.Shipments.AddNew();
			shipment1.JS_OH_DeliveryAgent = deliveryAgent.PK;
			shipment1.JS_HouseBillOfLadingType = "FIA";
			shipment1.JS_NoCopyBills = 1;
			shipment1.JS_NoOriginalBills = 1;

			deliveryAgent.LoadShipmentCollection(consolBisObj);
			AssertEquals("One shipment", 1, deliveryAgent.Shipments.Count);

			ForwardingShipment shipment2 = consolBisObj.Shipments.AddNew();
			shipment2.JS_OH_DeliveryAgent = deliveryAgent.PK;
			shipment2.JS_HouseBillOfLadingType = "FIA";
			shipment2.JS_NoCopyBills = 1;
			shipment2.JS_NoOriginalBills = 1;
			deliveryAgent.LoadShipmentCollection(consolBisObj);
			AssertEquals("Two shipments", 2, deliveryAgent.Shipments.Count);
		}

		public void TestLoadShipmentCollection_ConsolReceivingForwarderFallback()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			OrgHeader receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "Consol Receiving Forwarder";
			receivingForwarder.MainAddress.OA_Address1 = "Address 1";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			AssertEquals("prerequisite", ZGuid.Empty, shipment1.JS_OH_DeliveryAgent);

			DeliveryAgentOrgHeader deliveryAgentOrgHeader = Factory.Load<DeliveryAgentOrgHeader>(consol.ReceivingForwarderPK);
			deliveryAgentOrgHeader.LoadShipmentCollection(consol);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, deliveryAgentOrgHeader.Shipments);

			OrgHeader deliveryAgent1 = Factory.New<OrgHeader>();
			deliveryAgent1.OH_FullName = "Shipment 1 Delivery Agent";
			deliveryAgent1.MainAddress.OA_Address1 = "Address 2";

			shipment1.JS_OH_DeliveryAgent = deliveryAgent1.PK;

			deliveryAgentOrgHeader.LoadShipmentCollection(consol);
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<ForwardingShipment>(), deliveryAgentOrgHeader.Shipments);

			deliveryAgentOrgHeader = Factory.Load<DeliveryAgentOrgHeader>(deliveryAgent1.PK);
			deliveryAgentOrgHeader.LoadShipmentCollection(consol);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1 }, deliveryAgentOrgHeader.Shipments);
		}

		public void TestLoadShipmentCollection_WhenShipmentDeliveryAgentIsNull()
		{
			var agentHeader = Factory.New<DeliveryAgentOrgHeader>();
			AssertNull("No Shipments for consol", agentHeader.Shipments);

			var deliveryAgent = Factory.New<DeliveryAgentOrgHeader>();
			deliveryAgent.OH_Code = "CCC";
			deliveryAgent.OH_FullName = "Delivery Agent";
			deliveryAgent.MainAddress.OA_Address1 = "Address 1";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_ReceivingForwarderAddress = deliveryAgent.MainAddress.PK;
			consol.Shipments.AddNew();

			deliveryAgent.LoadShipmentCollection(consol);
			AssertEquals("Consol should have one shipment with no Delivery Agent", 1, deliveryAgent.Shipments.Count);
		}
	}
}
