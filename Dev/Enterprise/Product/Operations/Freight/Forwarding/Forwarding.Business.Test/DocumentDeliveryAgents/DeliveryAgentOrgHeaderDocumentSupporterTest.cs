using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(DeliveryAgentOrgHeaderDocumentSupporter))]
	sealed class DeliveryAgentOrgHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestLoadGenericWrappers()
		{
			DeliveryAgentOrgHeader deliveryAgent = Factory.New<DeliveryAgentOrgHeader>();
			deliveryAgent.LoadShipmentCollection(Factory.New<ForwardingConsol>());
			DocumentWrapper[] wrappers = deliveryAgent.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJob, null);
			AssertEquals("wrappers.Length", 1, wrappers.Length);
			AssertEquals("Wrapper is of type FreightWrapperFromConsol", "FreightWrapperFromConsol", wrappers[0].GetType().Name);
		}

		public void TestSupportedDataContext()
		{
			DeliveryAgentOrgHeader deliveryAgent = Factory.New<DeliveryAgentOrgHeader>();
			AssertEquals("DataContext.Consol is Supported", true, deliveryAgent.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Consol)));
			AssertEquals("DataContext.ARInvoice is Supported", true, deliveryAgent.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.ARInvoice)));
			AssertEquals("DataContext.GenericFreightJob is Supported", true, deliveryAgent.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJob)));
			AssertEquals("DataContext.GenericFreightJobInvoice is supported", true, deliveryAgent.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJobInvoice)));
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			DeliveryAgentOrgHeader deliveryAgent = Factory.New<DeliveryAgentOrgHeader>();
			deliveryAgent.OH_Code = "CCC";
			deliveryAgent.OH_FullName = "Delivery Agent";
			deliveryAgent.MainAddress.OA_Address1 = "Address 1";

			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "C0000001";
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			shipment1.JS_OH_DeliveryAgent = deliveryAgent.PK;
			shipment1.JS_HouseBillOfLadingType = "FIA";
			shipment1.JS_UniqueConsignRef = "S0000001";
			JobHeader job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job1.JH_GB = GlbBranch.CurrentBranch.PK;
			job1.JH_ParentID = shipment1.PK;
			ForwardingShipment shipment2 = consol1.Shipments.AddNew();
			shipment2.JS_OH_DeliveryAgent = deliveryAgent.PK;
			shipment2.JS_HouseBillOfLadingType = "FIA";
			shipment2.JS_UniqueConsignRef = "C0000002";
			JobHeader job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			job2.JH_GB = GlbBranch.CurrentBranch.PK;
			job2.JH_ParentID = shipment2.PK;

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_UniqueConsignRef = "C0000002";
			ForwardingShipment shipment3 = consol2.Shipments.AddNew();
			shipment3.JS_OH_DeliveryAgent = deliveryAgent.PK;
			shipment3.JS_HouseBillOfLadingType = "FIA";
			shipment3.JS_UniqueConsignRef = "C0000001";
			JobHeader job3 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job3.JH_GC = GlbCompany.CurrentCompany.PK;
			job3.JH_GB = GlbBranch.CurrentBranch.PK;
			job3.JH_ParentID = shipment3.PK;

			AccTransactionHeader consolInvoice1 = Factory.New<AccTransactionHeader>();
			consolInvoice1.AH_TransactionNum = "ConsolInvoice1";
			consolInvoice1.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			consolInvoice1.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			consolInvoice1.AH_OH = deliveryAgent.PK;
			consolInvoice1.AH_InvoiceDate = ZDateTime.Now;
			consolInvoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			consolInvoice1.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			consolInvoice1.AH_GE = GlbDepartment.CurrentDepartment.PK;
			consolInvoice1.AH_JH = ZGuid.Empty;
			consolInvoice1.AH_ConsolidatedInvoiceRef = "C0000001";

			AccTransactionHeader invoice1 = Factory.New<AccTransactionHeader>();
			invoice1.AH_TransactionNum = "Invoice1";
			invoice1.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice1.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			invoice1.AH_OH = deliveryAgent.PK;
			invoice1.AH_InvoiceDate = ZDateTime.Now;
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice1.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice1.AH_JH = job1.PK;
			invoice1.AH_ConsolidatedInvoiceRef = "S0000001";

			AccTransactionHeader invoice2 = Factory.New<AccTransactionHeader>();
			invoice2.AH_TransactionNum = "Invoice2";
			invoice2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice2.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			invoice2.AH_OH = deliveryAgent.PK;
			invoice2.AH_InvoiceDate = ZDateTime.Now;
			invoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice2.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice2.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice2.AH_JH = job2.PK;
			invoice2.AH_ConsolidatedInvoiceRef = "C0000002";

			AccTransactionHeader consolInvoice2 = Factory.New<AccTransactionHeader>();
			consolInvoice2.AH_TransactionNum = "ConsolInvoice2";
			consolInvoice2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			consolInvoice2.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			consolInvoice2.AH_OH = deliveryAgent.PK;
			consolInvoice2.AH_InvoiceDate = ZDateTime.Now;
			consolInvoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			consolInvoice2.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			consolInvoice2.AH_GE = GlbDepartment.CurrentDepartment.PK;
			consolInvoice2.AH_JH = ZGuid.Empty;
			consolInvoice2.AH_ConsolidatedInvoiceRef = "C0000002";

			AccTransactionHeader invoice3 = Factory.New<AccTransactionHeader>();
			invoice3.AH_TransactionNum = "Invoice3";
			invoice3.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice3.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			invoice3.AH_OH = deliveryAgent.PK;
			invoice3.AH_InvoiceDate = ZDateTime.Now;
			invoice3.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice3.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice3.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice3.AH_JH = job3.PK;
			invoice3.AH_ConsolidatedInvoiceRef = "C0000001";

			deliveryAgent.LoadShipmentCollection(consol1);

			Factory.Save();

			return deliveryAgent;
		}

		public void TestGetWrappersForARInvoice()
		{
			var deliveryAgentOrgHeader = (DeliveryAgentOrgHeader)GetDocumentSupportableBusinessObject();

			var supporter = new DeliveryAgentOrgHeaderDocumentSupporter(deliveryAgentOrgHeader);
			var wrappers = supporter.GetDocumentWrappers(Core.Constants.DataContext.ARInvoice, null);

			AssertEquals("TransactionHeader Exists", 3, wrappers.Length);

			string[] transactionHeaders = { wrappers[0].ToString(), wrappers[1].ToString(), wrappers[2].ToString() };

			AssertCollectionContains("TransactionHeader Exists", "ConsolInvoice1", transactionHeaders);
			AssertCollectionContains("TransactionHeader Exists", "Invoice1", transactionHeaders);
			AssertCollectionContains("TransactionHeader Exists", "Invoice2", transactionHeaders);
			AssertCollectionNotContains("TransactionHeader Exists", "ConsolInvoice2", transactionHeaders);
			AssertCollectionNotContains("TransactionHeader Exists", "Invoice3", transactionHeaders);
		}

		public void TestGetWrapperForGenricFreightJobInvoice()
		{
			StmMenuItem menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Delivery Agent Pack (for testing)";

			DeliveryAgentOrgHeader deliveryAgent = Factory.New<DeliveryAgentOrgHeader>();
			DocumentWrapper[] wrappers = deliveryAgent.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobInvoice, menu);

			AssertNull("No invoices to wrap", wrappers);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C0000001";
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_OH_DeliveryAgent = deliveryAgent.PK;
			shipment1.JS_HouseBillOfLadingType = "FIA";
			shipment1.JS_UniqueConsignRef = "S0000001";
			JobHeader job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job1.JH_GB = GlbBranch.CurrentBranch.PK;
			job1.JH_ParentID = shipment1.PK;

			AccTransactionHeader invoice1 = Factory.New<AccTransactionHeader>();
			invoice1.AH_TransactionNum = "Invoice1";
			invoice1.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice1.AH_TransactionType = Enterprise.ZArchitecture.Core.TransactionTypes.Invoice;
			invoice1.AH_OH = deliveryAgent.PK;
			invoice1.AH_InvoiceDate = ZDateTime.Now;
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			invoice1.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice1.AH_JH = job1.PK;
			invoice1.AH_ConsolidatedInvoiceRef = "S0000001";

			deliveryAgent.LoadShipmentCollection(consol);
			wrappers = deliveryAgent.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.GenericFreightJobInvoice, menu);
			AssertEquals("One wrapper", 1, wrappers.Length);
		}
	}
}
