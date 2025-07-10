using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentWrapperWithConsolAgentDocumentSupporter))]
	sealed class ForwardingShipmentWrapperWithConsolAgentDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetBODocDataProvidersNotFoundMessage_NoReceivingAgent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001";

			shipment.CreateJobHeaderWithMutex();

			using (var job = shipment.Job)
			{
				var supportable = new ForwardingShipmentWrapperWithConsolAgent(shipment, null);

				var contextArray = new[]
				{
					Constants.DataContext.ARInvoice,
					Constants.DataContext.GenericFreightJobInvoice
				};

				var menu = Factory.New<StmMenuItem>();
				var expectedMessage = "The related consol does not have a Receiving Agent.";

				AssertNotFoundMessage(supportable, menu, contextArray, true, expectedMessage);

				var consol = shipment.Consols.AddNew();
				consol.FillWithValidTestData();

				var consolAgent = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_ReceivingForwarderAddress = consolAgent.MainAddress.PK;

				var transactionHeader = Factory.New<AccTransactionHeader>();
				transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
				transactionHeader.AH_OH = consolAgent.PK;
				transactionHeader.AH_InvoiceDate = ZDateTime.Now;
				transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
				transactionHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
				transactionHeader.AH_JH = job.PK;
				transactionHeader.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

				var localCharges = Factory.NewWithValidTestData<OrgHeader>();
				job.LocalChargesPK = localCharges.PK;

				supportable = new ForwardingShipmentWrapperWithConsolAgent(shipment, consolAgent);

				AssertNotFoundMessage(supportable, menu, contextArray, false, ZString.Empty);
			}
		}

		protected override bool ShouldSkipWithContextAndMenu(Constants.DataContext context, IStmMenuItem menu)
		{
			var shipment = Factory.New<ForwardingShipment>();
			var supportable = new ForwardingShipmentWrapperWithConsolAgent(shipment, null);

			var supporter = supportable.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(context, menu);

			return wrappers != null && wrappers.Length > 0 && wrappers.All(c => c != null);
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForMessageNotPrintingTest
		{
			get
			{
				var shipment = Factory.New<ForwardingShipment>();
				return new[]
				{
					new ForwardingShipmentWrapperWithConsolAgent(shipment, null)
				};
			}
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0001";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = "JS";

			var consol = shipment.Consols.AddNew();
			consol.FillWithValidTestData();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "GBLON";

			var gbMawb = Factory.New<Enterprise.Integration.Customs.GB.CCSUK.ICusMAWB>();
			gbMawb.CM_ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes.GbCcsuk;

			var gbHawb = Factory.New<Enterprise.Integration.Customs.GB.CCSUK.ICusHAWB>();
			gbHawb.CS_JS = shipment.PK;
			gbHawb.CS_CM = ((BusinessObject)gbMawb).PK;

			var consolAgent = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_ReceivingForwarderAddress = consolAgent.MainAddress.PK;

			var transactionHeader = Factory.New<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			transactionHeader.AH_OH = consolAgent.PK;
			transactionHeader.AH_InvoiceDate = ZDateTime.Now;
			transactionHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			transactionHeader.AH_GE = GlbDepartment.CurrentDepartment.PK;
			transactionHeader.AH_JH = jobHeader.PK;
			transactionHeader.AH_RX_NKTransactionCurrency = "AUD";
			transactionHeader.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;

			return new ForwardingShipmentWrapperWithConsolAgent(shipment, consolAgent);
		}
	}
}
