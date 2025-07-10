using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentWrapperWithConsolAgent))]
	sealed class ForwardingShipmentWrapperWithConsolAgentDocumentSupporterForwardingTest : ForwardingShipmentWrapperWithConsolAgentTest
	{
		public void TestGetDocBusinessObjectsForARInvoice()
		{
			ForwardingShipment forwardingShipment = Factory.New<ForwardingShipment>();
			forwardingShipment.JS_UniqueConsignRef = "S0001";
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.Parent = forwardingShipment;
			OrgHeader consolAgent = Factory.NewWithValidTestData<OrgHeader>();
			ForwardingShipmentWrapperWithConsolAgent shipmentWrapper = new ForwardingShipmentWrapperWithConsolAgent(forwardingShipment, consolAgent);

			DocumentWrapper[] wrappers = shipmentWrapper.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.ARInvoice, null);
			AssertNull("ARInvoice wrappers Null", wrappers);

			wrappers = shipmentWrapper.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.GenericFreightJob, null);
			AssertNull("No ARInvoice wrappers for GenericFreightJob either", wrappers);

			consolAgent.OH_FullName = "Consol Agent";

			AccTransactionHeader accHeader1 = Factory.New<AccTransactionHeader>();
			accHeader1.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			accHeader1.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			accHeader1.AH_OH = consolAgent.PK;
			accHeader1.AH_InvoiceDate = ZDateTime.Now;
			accHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader1.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeader1.AH_JH = job.PK;
			accHeader1.AH_ConsolidatedInvoiceRef = forwardingShipment.JS_UniqueConsignRef;

			AccTransactionHeader accHeader2 = Factory.New<AccTransactionHeader>();
			accHeader2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			accHeader2.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			accHeader2.AH_OH = consolAgent.PK;
			accHeader2.AH_InvoiceDate = ZDateTime.Now;
			accHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
			accHeader2.AH_GE = GlbDepartment.CurrentDepartment.PK;
			accHeader2.AH_JH = job.PK;
			accHeader2.AH_ConsolidatedInvoiceRef = forwardingShipment.JS_UniqueConsignRef;

			wrappers = shipmentWrapper.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.ARInvoice, null);
			AssertEquals("Wrappers created for ARInvoices with Consol Agent", 2, wrappers.Length);
			AssertContains("Wrapper 0 is a DocARInvoice", "DocARInvoice", wrappers[0].GetType().ToString());
			AssertContains("Wrapper 1 is a DocARInvoice", "DocARInvoice", wrappers[1].GetType().ToString());

			wrappers = shipmentWrapper.DocumentSupporter.GetDocumentWrappers(Enterprise.Core.Constants.DataContext.GenericFreightJob, null);
			AssertEquals("Generic Wrappers created for Consol Agent invoices", 2, wrappers.Length);
			AssertContains("Wrapper 0 is a FreightWrapperFromInvoice", "FreightWrapperFromInvoice", wrappers[0].GetType().ToString());
			AssertContains("Wrapper 1 is a FreightWrapperFromInvoice", "FreightWrapperFromInvoice", wrappers[1].GetType().ToString());
		}

		public void TestGetContactOrganisationToConsolAgent()
		{
			ForwardingShipment forwardingShipment = Factory.New<ForwardingShipment>();
			OrgHeader consolAgent = Factory.New<OrgHeader>();
			ForwardingShipmentWrapperWithConsolAgent shipmentWrapper = new ForwardingShipmentWrapperWithConsolAgent(forwardingShipment, consolAgent);

			consolAgent.OH_FullName = "Consol Agent";

			var contact = shipmentWrapper.DocumentSupporter.GetContactOrganisation("", ContactType.ExportSeaFreightAgent, DocumentDirection.ANY);
			AssertEquals("Attentioned to: goes to Consol Agent (Sea)", shipmentWrapper.ConsolAgentForARInvoice.OH_FullName, contact.OrgHeader.FullName);

			contact = shipmentWrapper.DocumentSupporter.GetContactOrganisation("", ContactType.ExportAirFreightAgent, DocumentDirection.ANY);
			AssertEquals("Attentioned to: goes to Consol Agent (Air)", shipmentWrapper.ConsolAgentForARInvoice.OH_FullName, contact.OrgHeader.FullName);
		}

		public void TestBusinessContext()
		{
			ForwardingShipment forwardingShipment = Factory.New<ForwardingShipment>();
			OrgHeader agentOrg = Factory.New<OrgHeader>();
			agentOrg.OH_FullName = "Consol Agent";

			ForwardingShipmentWrapperWithConsolAgent shipmentWrapperWithConsolAgent = new ForwardingShipmentWrapperWithConsolAgent(forwardingShipment, agentOrg);
			ForwardingShipmentWrapperWithConsolAgentDocumentSupporter docSupporter = new ForwardingShipmentWrapperWithConsolAgentDocumentSupporter(shipmentWrapperWithConsolAgent);

			AssertEquals(BusinessContext.ConsolAgent, docSupporter.BusinessContext);

			ForwardingShipmentWrapperWithConsolAgent shipmentWrapper = new ForwardingShipmentWrapperWithConsolAgent(forwardingShipment, agentOrg, BusinessContext.Shipment);
			docSupporter = new ForwardingShipmentWrapperWithConsolAgentDocumentSupporter(shipmentWrapper);

			AssertEquals(BusinessContext.Shipment, docSupporter.BusinessContext);
		}
	}
}
