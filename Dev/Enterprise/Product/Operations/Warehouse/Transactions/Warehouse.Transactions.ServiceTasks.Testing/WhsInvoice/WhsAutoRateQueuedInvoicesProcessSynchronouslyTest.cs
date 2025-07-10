using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.Warehouse.Transactions.ServiceTasks;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsAutoRateQueuedInvoicesProcessSynchronouslyTransactionedTestCase : TestCaseWithFactory
	{
		#region TestContractorInvalidArgument

		public void TestContractorInvalidArgument()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new WhsAutoRateQueuedInvoicesProcessSynchronously(null));
			var dp = MockInvoiceHelperWithSetups();
			AssertNoExceptionThrown(() => new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object));
		}

		#endregion

		#region TestRunTask_NoInvoice

		public void TestRunTask_NoInvoice()
		{
			var logger = GetLogger();
			var dp = MockInvoiceHelperWithSetups();
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
			processor.Process(logger, CancellationToken.None);

			AssertMultilineASCIIEquals("Should only auto rate invoices with queue status.",
				"Information|There are no invoices in the queue to be processed.", logger.ToString());
		}

		#endregion

		#region TestRunTask_IsStillInQueue

		public void TestRunTask_IsStillInQueue_False()
		{
			TestRunTask_IsStillInQueue(isStillInQueue: false);
		}

		public void TestRunTask_IsStillInQueue_True()
		{
			TestRunTask_IsStillInQueue(isStillInQueue: true);
		}

		void TestRunTask_IsStillInQueue(bool isStillInQueue)
		{
			var client = Helper.CreateClient("C01");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse($"WHS", address, branch);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = isStillInQueue ? StorageOffBandProcessingStatus.Codes.QUE : StorageOffBandProcessingStatus.Codes.NIQ;
			Factory.Save();

			var logger = GetLogger();
			var dp = MockInvoiceHelperWithSetups(invoice);
			dp.Setup(d => d.IsStillInQueueInDB(invoice.PK)).Returns(isStillInQueue);
			dp.Setup(d => d.InvoiceSaveSafe(invoice, logger, It.IsAny<int>())).Callback(invoice.Factory.Save);
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
			processor.Process(logger, CancellationToken.None);

			if (isStillInQueue)
			{
				AssertMultilineASCIIEquals("Should be processed when still in queue.", GetLinkToInvoiceInfo("I00000001") + @"
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Information|Delivered Invoice for client C01 warehouse WHS.", logger.ToString());
			}
			else
			{
				AssertMultilineASCIIEquals("Should not process.", "", logger.ToString());
			}
		}

		#endregion

		#region TestRunTask_JobLockedInOtherProcess

		public void TestRunTask_JobLockedInOtherProcess()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var whs = data.Whs1;
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoDeliverPeriodicInvoice = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var helper = new WhsTestHelperFunctions(Factory);
			var yesterday = ZDate.Today.AddDays(-1);
			var receive = helper.CreateWhsReceiveWithInventory(client, whs, "R1", yesterday.ToZDateTime().ToOffset(), data.Part1, 10m);
			Factory.Save();

			// Setting to generate charge when autorate invoice
			var receiveHandlingFumigationCharge = helper.CreateChargeCode($"WRECFUM", "Warehouse Receive Handling Fumigation", ChargeCodeGroupList.Codes.WHSInwards, Constants.FreightServiceType.Codes.Fumigation);
			var clientRate = helper.CreateClientRate(client);
			var warehouseRate = helper.CreateRateEntry(clientRate, yesterday.AddMonths(-1), ZDate.Today);
			helper.CreateRateLine(warehouseRate, receiveHandlingFumigationCharge, "SV", 5m);

			// any charges
			var fumigationService = receive.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 30m;
			fumigationService.ES_Completed = yesterday;

			Factory.Save();

			new AccountingPeriodTestHelper().SetupPeriods();
			var invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			// Simulate other process lock the job
			using (var mutex = new ZGlobalMutex(MutexIDs.WhsInvoiceAutoRate, invoice.PK.ToString()))
			{
				mutex.Lock();

				var logger1 = GetLogger();
				var processor1 = new WhsAutoRateQueuedInvoicesProcessSynchronously(new WhsInvoiceHelper());
				processor1.Process(logger1, CancellationToken.None);

				AssertMultilineASCIIEquals("Should not start auto rate.", @"Information|[Warehouse Periodic Invoice I00000001] - Another instance of the service task is processing the invoice for client 111 warehouse 1 or it cannot be locked.", logger1.ToString());
				AssertNull("Should not contain anything about cannot lock!", invoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).SingleOrDefault());

				mutex.Unlock();

				var logger2 = GetLogger();
				var processor2 = new WhsAutoRateQueuedInvoicesProcessSynchronously(new WhsInvoiceHelper());
				processor2.Process(logger2, CancellationToken.None);

				AssertMultilineASCIIEquals("Should start auto rate.", GetLinkToInvoiceInfo("I00000001") + @"
Information|Autorated Invoice for client 111 warehouse 1.
Information|Posted Invoice for client 111 warehouse 1.
Information|Periodic Billing job status for client 111 warehouse 1 was successfully closed.
Information|Delivered Invoice for client 111 warehouse 1.", logger2.ToString());
				var note = invoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).SingleOrDefault();
				AssertStartsWith("Should log when process normally.", GetLinkToInvoiceInfo("I00000001") + @"
Information|Autorated Invoice for client 111 warehouse 1.
Information|Posted Invoice for client 111 warehouse 1.
Information|Periodic Billing job status for client 111 warehouse 1 was successfully closed.
Information|Delivered Invoice for client 111 warehouse 1.
Service task log
", note.ST_NoteText);
			}
		}

		#endregion

		#region TestCancellationToken

		public void TestCancellationToken_None()
		{
			TestCancellationTokenCore(cancelled: false);
		}

		public void TestCancellationToken_Cancel()
		{
			TestCancellationTokenCore(cancelled: true);
		}

		void TestCancellationTokenCore(bool cancelled)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client1 = Helper.CreateClient("C01");
			var client2 = Helper.CreateClient("C02");
			var invoice1 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, client1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice1.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			var invoice2 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, client2.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice2.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();
			AssertNull("Precondition", invoice1.JobHeader);

			var logger = GetLogger();
			var dp = MockInvoiceHelperWithSetups(invoice1, invoice2);
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
			var tokenSource = new CancellationTokenSource();
			var token = tokenSource.Token;
			dp.Setup(d => d.IsStillInQueueInDB(It.IsAny<ZGuid>())).Returns(() =>
			{
				if (cancelled)
				{
					tokenSource.Cancel();
				}
				return true;
			});

			try
			{
				processor.Process(logger, token);
			}
			catch (OperationCanceledException e)
			{
				AssertEquals("The operation was canceled.", e.Message);
			}

			var expectedLog = GetLinkToInvoiceInfo("I00000001") + @"
Information|Autorated Invoice for client C01 warehouse 1.
Information|Posted Invoice for client C01 warehouse 1.
Information|Delivered Invoice for client C01 warehouse 1.";

			if (!cancelled)
			{
				//Cancelled process should not be continued
				expectedLog += $@"
{GetLinkToInvoiceInfo("I00000002")}
Information|Autorated Invoice for client C02 warehouse 1.
Information|Posted Invoice for client C02 warehouse 1.
Information|Delivered Invoice for client C02 warehouse 1.";
			}

			AssertMultilineASCIIEquals("Should auto rate invoice until cancel process.", expectedLog, logger.ToString());
		}

		#endregion

		#region TestRunTask_EndToEnd_BillingInvoice

		[TestDate(2022, 5, 9, 10, 32, 0)]
		public void TestRunTask_EndToEnd_BillingInvoice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var whs = data.Whs1;
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoDeliverPeriodicInvoice = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var helper = new WhsTestHelperFunctions(Factory);
			var yesterday = ZDate.Today.AddDays(-1);
			var receive = helper.CreateWhsReceiveWithInventory(client, whs, "R1", yesterday.ToZDateTime().ToOffset(), data.Part1, 10m);
			Factory.Save();

			// Setting to generate charge when autorate invoice
			var receiveHandlingFumigationCharge = helper.CreateChargeCode($"WRECFUM", "Warehouse Receive Handling Fumigation", ChargeCodeGroupList.Codes.WHSInwards, Constants.FreightServiceType.Codes.Fumigation);
			var clientRate = helper.CreateClientRate(client);
			var warehouseRate = helper.CreateRateEntry(clientRate, yesterday.AddMonths(-1), ZDate.Today);
			helper.CreateRateLine(warehouseRate, receiveHandlingFumigationCharge, "SV", 5m);

			// any charges
			var fumigationService = receive.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 30m;
			fumigationService.ES_Completed = yesterday;

			Factory.Save();

			new AccountingPeriodTestHelper().SetupPeriods();
			var invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			AssertEquals("Precondition", false, NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Length > 0);

			var logger = GetLogger();
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(new WhsInvoiceHelper());
			processor.Process(logger, CancellationToken.None);

			var expectedLog = GetLinkToInvoiceInfo("I00000001") + @"
Information|Autorated Invoice for client 111 warehouse 1.
Information|Posted Invoice for client 111 warehouse 1.
Information|Periodic Billing job status for client 111 warehouse 1 was successfully closed.
Information|Delivered Invoice for client 111 warehouse 1.";
			AssertMultilineASCIIEquals("Should auto rate invoice.", expectedLog, logger.ToString());

			var whsInvoiceInDB = NewFactory().Load<WhsInvoice>(invoice.PK);
			AssertMultilineASCIIEquals("Add log to note!", expectedLog + @"
Service task log
Time: 09-May-22 10:32", whsInvoiceInDB.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Single().ST_NoteText);
			AssertEquals("Status should be set.", BillingAutomationStatusCodes.BillingAutomationDelivered, whsInvoiceInDB.BillingAutomationStatus);
		}

		#endregion

		#region TestRunTask_FactorySaveSafe

		[TestDate(2022, 5, 9, 10, 31, 0)]
		public void TestRunTask_FactorySaveSafe()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var whs = data.Whs1;
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var helper = new WhsTestHelperFunctions(Factory);
			var yesterday = ZDate.Today.AddDays(-1);
			var receive = helper.CreateWhsReceiveWithInventory(client, whs, "R1", yesterday.ToZDateTime().ToOffset(), data.Part1, 10m);
			Factory.Save();

			// Setting to generate charge when autorate invoice
			var receiveHandlingFumigationCharge = helper.CreateChargeCode($"WRECFUM", "Warehouse Receive Handling Fumigation", ChargeCodeGroupList.Codes.WHSInwards, Constants.FreightServiceType.Codes.Fumigation);
			var clientRate = helper.CreateClientRate(client);
			var warehouseRate = helper.CreateRateEntry(clientRate, yesterday.AddMonths(-1), ZDate.Today);
			helper.CreateRateLine(warehouseRate, receiveHandlingFumigationCharge, "SV", 5m);

			// any charges
			var fumigationService = receive.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 2M;
			fumigationService.ES_Completed = yesterday;

			Factory.Save();

			var invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();
			AssertEquals("Precondition", false, NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Length > 0);

			var logger = GetLogger();
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new MaximumAllowedTransactionAmount() { MaximumAllowedLineAmount = 1M, MaximumAllowedHeaderAmount = 1M }))
			{
				var invoiceHelper = new WhsInvoiceHelper();
				var dp = MockInvoiceHelperWithSetups(invoice);
				dp.Setup(d => d.LoadInNewFactory(invoice.PK)).Returns(() => invoiceHelper.LoadInNewFactory(invoice.PK));
				dp.Setup(d => d.GetInvoiceReferNumber(It.IsAny<WhsInvoice>())).Returns(() => new WhsInvoiceHelper().GetInvoiceReferNumber(invoice));
				dp.Setup(d => d.AutoRateJobHeader(logger, It.IsAny<WhsInvoice>()))
					.Callback((ILogger loggerIn, WhsInvoice invoiceIn) =>
					{
						invoiceIn.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationAutorated;
						loggerIn.Information("AutoRateJobHeaderInUserContext called!");
						invoiceIn.AutoRateJobHeader(null);
					})
					.Returns(true);
				dp.Setup(d => d.CloseBillingAndRelatedJob(logger, It.IsAny<WhsInvoice>()))
				.Callback((ILogger loggerIn, WhsInvoice invoiceIn) =>
				{
					invoice.JobStatusWithPromptlessRelatedJobUpdate = JobHeaderStatus.Closed.Code;
					loggerIn.Information("Close billing called!");
				})
				.Returns(true);
				dp.Setup(d => d.InvoiceSaveSafe(It.IsAny<WhsInvoice>(), logger, It.IsAny<int>()))
					.Returns((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) =>
					{
						return invoiceIn.BillingAutomationStatus != BillingAutomationStatusCodes.BillingAutomationAutorated
								&& invoiceIn.BillingAutomationStatus != BillingAutomationStatusCodes.BillingAutomationPosted
							? invoiceHelper.InvoiceSaveSafe(invoiceIn, loggerIn, attempt)
							: InvoiceSaveSafeResult.SaveSuccessful;
					});
				var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
				processor.Process(logger, CancellationToken.None);
			}

			var expectedMessage = GetLinkToInvoiceInfo("I00000001") + @"
Information|AutoRateJobHeaderInUserContext called!
Information|Posted Invoice for client 111 warehouse 1.
Information|Close billing called!
Information|Delivered Invoice for client 111 warehouse 1.
Error|An error has occurred. Your unsaved work must be re-entered.
Please close the form in which you were working and re-enter the data.
Error Message: The job charge amount must be between -1.00 and 1.00 which is defined in the 'Accounting -> Maximum Allowed Transaction Amount' registry.
(First attempt).
";
			AssertMultilineASCIIEquals("Should log exception.", expectedMessage, logger.ToString());

			AssertEquals("Should change status to error.", StorageOffBandProcessingStatus.Codes.ERR, NewFactory().Load<WhsInvoice>(invoice.PK).ET_OffBandProcessingStatus);
			AssertMultilineASCIIEquals("Add log to note!", expectedMessage + @"
Service task log
Time: 09-May-22 10:31", NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Single().ST_NoteText);
		}

		#endregion

		#region TestRunTask_FactorySaveSafe_FailedEndOfProcess

		[TestDate(2022, 5, 9, 10, 31, 0)]
		public void TestRunTask_FactorySaveSafe_FailedEndOfProcess()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var whs = data.Whs1;
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var helper = new WhsTestHelperFunctions(Factory);
			var yesterday = ZDate.Today.AddDays(-1);
			var receive = helper.CreateWhsReceiveWithInventory(client, whs, "R1", yesterday.ToZDateTime().ToOffset(), data.Part1, 10m);
			Factory.Save();

			// Setting to generate charge when autorate invoice
			var receiveHandlingFumigationCharge = helper.CreateChargeCode($"WRECFUM", "Warehouse Receive Handling Fumigation", ChargeCodeGroupList.Codes.WHSInwards, Constants.FreightServiceType.Codes.Fumigation);
			var clientRate = helper.CreateClientRate(client);
			var warehouseRate = helper.CreateRateEntry(clientRate, yesterday.AddMonths(-1), ZDate.Today);
			helper.CreateRateLine(warehouseRate, receiveHandlingFumigationCharge, "SV", 5m);

			// any charges
			var fumigationService = receive.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 2M;
			fumigationService.ES_Completed = yesterday;

			Factory.Save();

			var invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();
			AssertEquals("Precondition", false, NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Length > 0);

			var logger = GetLogger();
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;
			var invoiceHelper = new WhsInvoiceHelper();
			var dp = MockInvoiceHelperWithSetups(invoice);
			dp.Setup(d => d.LoadInNewFactory(invoice.PK)).Returns(() => invoiceHelper.LoadInNewFactory(invoice.PK));
			dp.Setup(d => d.GetInvoiceReferNumber(It.IsAny<WhsInvoice>())).Returns(() => new WhsInvoiceHelper().GetInvoiceReferNumber(invoice));
			dp.Setup(d => d.AutoRateJobHeader(logger, It.IsAny<WhsInvoice>()))
				.Callback((ILogger loggerIn, WhsInvoice invoiceIn) =>
				{
					invoiceIn.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationAutorated;
					loggerIn.Information("AutoRateJobHeaderInUserContext called!");
					invoiceIn.AutoRateJobHeader(null);
				})
				.Returns(true);
			dp.Setup(d => d.CloseBillingAndRelatedJob(logger, It.IsAny<WhsInvoice>()))
				.Callback((ILogger loggerIn, WhsInvoice invoiceIn) =>
				{
					invoice.JobStatusWithPromptlessRelatedJobUpdate = JobHeaderStatus.Closed.Code;
					loggerIn.Information("CloseInvoice called!");
				})
				.Returns(true);
			dp.Setup(d => d.PostInvoice(logger, It.IsAny<WhsInvoice>()))
				.Callback((ILogger loggerIn, WhsInvoice invoiceIn) =>
				{
					invoiceIn.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationPosted;
					loggerIn.Information("PostInvoice called!");
				})
				.Returns(true);
			dp.Setup(d => d.DeliverInvoice(logger, It.IsAny<WhsInvoice>()))
				.Callback((ILogger loggerIn, WhsInvoice invoiceIn) =>
				{
					invoiceIn.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationDelivered;
					loggerIn.Information("DeliverInvoice called!");
				})
				.Returns(true);
			dp.Setup(d => d.InvoiceSaveSafe(It.IsAny<WhsInvoice>(), logger, It.IsAny<int>()))
				.Callback((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) =>
				{
					if (invoiceIn.BillingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationDelivered)
					{
						invoiceIn.Factory.Saving += (f) => throw new ZCannotSaveException("Ops!", "Test");
					}
				})
				.Returns((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) => invoiceHelper.InvoiceSaveSafe(invoiceIn, loggerIn, attempt));
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
			processor.Process(logger, CancellationToken.None);

			var expectedMessage = $@"
{GetLinkToInvoiceInfo("I00000001")}
Information|AutoRateJobHeaderInUserContext called!
Information|PostInvoice called!
Information|CloseInvoice called!
Information|DeliverInvoice called!
Error|Ops!
(First attempt).
{GetLinkToInvoiceInfo("I00000001")}
Information|CloseInvoice called!
Information|DeliverInvoice called!
Error|Ops!
(Second attempt).
";
			AssertMultilineASCIIEquals("Should log exception.", expectedMessage, logger.ToString());

			AssertEquals("Should change status to error.", StorageOffBandProcessingStatus.Codes.ERR, NewFactory().Load<WhsInvoice>(invoice.PK).ET_OffBandProcessingStatus);
			AssertMultilineASCIIEquals("Add log to note!", expectedMessage + @"
Service task log
Time: 09-May-22 10:31", NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Single().ST_NoteText);
		}

		#endregion

		#region TestRunTask_FactorySaveSafe_FailedSaveAutoRate

		[TestDate(2022, 5, 9, 10, 31, 0)]
		public void TestRunTask_FactorySaveSafe_FailedSaveAutoRate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var whs = data.Whs1;
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var helper = new WhsTestHelperFunctions(Factory);
			var yesterday = ZDate.Today.AddDays(-1);
			var receive = helper.CreateWhsReceiveWithInventory(client, whs, "R1", yesterday.ToZDateTime().ToOffset(), data.Part1, 10m);
			Factory.Save();

			// Setting to generate charge when autorate invoice
			var receiveHandlingFumigationCharge = helper.CreateChargeCode($"WRECFUM", "Warehouse Receive Handling Fumigation", ChargeCodeGroupList.Codes.WHSInwards, Constants.FreightServiceType.Codes.Fumigation);
			var clientRate = helper.CreateClientRate(client);
			var warehouseRate = helper.CreateRateEntry(clientRate, yesterday.AddMonths(-1), ZDate.Today);
			helper.CreateRateLine(warehouseRate, receiveHandlingFumigationCharge, "SV", 5m);

			// any charges
			var fumigationService = receive.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 2M;
			fumigationService.ES_Completed = yesterday;

			Factory.Save();

			var invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();
			AssertEquals("Precondition", false, NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Length > 0);

			var logger = GetLogger();
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;
			var invoiceHelper = new WhsInvoiceHelper();
			var dp = MockInvoiceHelperWithSetups(invoice);
			dp.Setup(d => d.LoadInNewFactory(invoice.PK)).Returns(() => invoiceHelper.LoadInNewFactory(invoice.PK));
			dp.Setup(d => d.GetInvoiceReferNumber(It.IsAny<WhsInvoice>())).Returns(() => new WhsInvoiceHelper().GetInvoiceReferNumber(invoice));
			dp.Setup(d => d.AutoRateJobHeader(logger, It.IsAny<WhsInvoice>()))
				.Callback((ILogger loggerIn, WhsInvoice invoiceIn) =>
				{
					invoiceIn.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationAutorated;
					loggerIn.Information("AutoRateJobHeaderInUserContext called!");
					invoiceIn.AutoRateJobHeader(null);
				})
				.Returns(true);
			dp.Setup(d => d.PostInvoice(logger, It.IsAny<WhsInvoice>()))
				.Callback((ILogger loggerIn, WhsInvoice invoiceIn) =>
				{
					invoiceIn.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationPosted;
					loggerIn.Information("PostInvoice called!");
				})
				.Returns(true);
			dp.Setup(d => d.DeliverInvoice(logger, It.IsAny<WhsInvoice>()))
				.Callback((ILogger loggerIn, WhsInvoice invoiceIn) =>
				{
					invoiceIn.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationDelivered;
					loggerIn.Information("DeliverInvoice called!");
				})
				.Returns(true);
			dp.Setup(d => d.InvoiceSaveSafe(It.IsAny<WhsInvoice>(), logger, It.IsAny<int>()))
				.Callback((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) =>
				{
					if (invoiceIn.BillingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationAutorated && attempt < 3)
					{
						invoiceIn.Factory.Saving += (f) => throw new ZCannotSaveException("Ops!", "Test");
					}
				})
				.Returns((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) => invoiceHelper.InvoiceSaveSafe(invoiceIn, loggerIn, attempt));
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
			processor.Process(logger, CancellationToken.None);

			var expectedMessage = $@"
{GetLinkToInvoiceInfo("I00000001")}
Information|AutoRateJobHeaderInUserContext called!
Error|Ops!
(First attempt).
{GetLinkToInvoiceInfo("I00000001")}
Information|AutoRateJobHeaderInUserContext called!
Error|Ops!
(Second attempt).
";
			AssertMultilineASCIIEquals("Should log exception.", expectedMessage, logger.ToString());

			AssertEquals("Should change status to error.", StorageOffBandProcessingStatus.Codes.ERR, NewFactory().Load<WhsInvoice>(invoice.PK).ET_OffBandProcessingStatus);
			AssertMultilineASCIIEquals("Add log to note!", expectedMessage + @"
Service task log
Time: 09-May-22 10:31", NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Single().ST_NoteText);
		}

		#endregion

		#region TestRunTask_FactorySaveSafe_Performance

		public void TestRunTask_FactorySaveSafe_Performance()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			Factory.Save();

			Assert("Precondition - Creating workflow from template should not be suppressed", !ProcessTask.Loader.SuppressCreatingWorkflowFromTemplate);

			var saveCalled = false;
			var invoiceHelper = new WhsInvoiceHelper();
			invoice.Factory.Saving += (f) =>
				{
					saveCalled = true;
					Assert("Creating workflow from template should be suppressed", ProcessTask.Loader.SuppressCreatingWorkflowFromTemplate);
				};

			Assert("Precondition", !saveCalled);
			invoiceHelper.InvoiceSaveSafe(invoice, GetLogger(), 1);
			Assert("Should call save.", saveCalled);
		}

		#endregion

		#region TestAutoRateJobHeaderInUserContext_HasError

		public void TestAutoRateJobHeaderInUserContext_HasError()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var whs = data.Whs1;
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var helper = new WhsTestHelperFunctions(Factory);
			var yesterday = ZDate.Today.AddDays(-1);
			var receive = helper.CreateWhsReceiveWithInventory(client, whs, "R1", yesterday.ToZDateTime().ToOffset(), data.Part1, 10m);
			Factory.Save();

			// Setting to generate charge when autorate invoice
			var receiveHandlingFumigationCharge = helper.CreateChargeCode($"WRECFUM", "Warehouse Receive Handling Fumigation", ChargeCodeGroupList.Codes.WHSInwards, Constants.FreightServiceType.Codes.Fumigation);
			var clientRate = helper.CreateClientRate(client);
			var warehouseRate = helper.CreateRateEntry(clientRate, yesterday.AddMonths(-1), ZDate.Today);
			helper.CreateRateLine(warehouseRate, receiveHandlingFumigationCharge, "SV", 5m);

			// any charges
			var fumigationService = receive.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 2M;
			fumigationService.ES_Completed = yesterday;

			Factory.Save();

			var invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var logger = GetLogger();
			var registry = AccountingMasterFilesRegistry.Instance.MaximumAllowedTransactionAmount;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new MaximumAllowedTransactionAmount() { MaximumAllowedLineAmount = 1M, MaximumAllowedHeaderAmount = 1M }))
			{
				var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(new WhsInvoiceHelper());
				processor.Process(logger, CancellationToken.None);
			}

			var expectString = GetLinkToInvoiceInfo("I00000001") + $@"
Error|Invoice for client 111 warehouse 1 could not be auto rated due to following errors: Error - JR_LocalSellAmt: The job charge amount must be between -1.00 and 1.00 which is defined in the 'Accounting -> Maximum Allowed Transaction Amount' registry.";
			AssertMultilineASCIIEquals("Should log exception.", expectString, logger.ToString());

			AssertEquals("Should change status to error.", StorageOffBandProcessingStatus.Codes.ERR, NewFactory().Load<WhsInvoice>(invoice.PK).ET_OffBandProcessingStatus);
		}

		#endregion

		#region TestAutoRateJobHeaderInUserContext_NoCharge

		[TestDate(2022, 5, 9, 10, 33, 0)]
		public void TestAutoRateJobHeaderInUserContext_NoCharge()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var whs = data.Whs1;
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			Factory.Save();

			var helper = new WhsTestHelperFunctions(Factory);
			var invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();
			AssertEquals("Precondition", false, NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Length > 0);

			var logger = GetLogger();
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(new WhsInvoiceHelper());
			processor.Process(logger, CancellationToken.None);

			var expectedMessage = GetLinkToInvoiceInfo("I00000001") + $@"
Information|Invoice does not have charges for client 111 warehouse 1.";
			AssertMultilineASCIIEquals("Should log the error.", expectedMessage, logger.ToString());
			AssertEquals("Should change status to error.", StorageOffBandProcessingStatus.Codes.ERR, NewFactory().Load<WhsInvoice>(invoice.PK).ET_OffBandProcessingStatus);
			AssertMultilineASCIIEquals("Add log to note!", expectedMessage + @"
Service task log
Time: 09-May-22 10:33", NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Single().ST_NoteText);
		}

		#endregion

		#region TestAutoRateJobHeaderInUserContext_StatusPost

		[TestDate(2022, 5, 9, 10, 34, 0)]
		public void TestAutoRateJobHeaderInUserContext_StatusPost_NoPostedInvoice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoDeliverPeriodicInvoice = true;

			var helper = new WhsTestHelperFunctions(Factory);
			var invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			invoice.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationPosted;
			invoice.TryCreateJobHeader();
			Factory.Save();

			var logger = GetLogger();
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(new WhsInvoiceHelper());
			processor.Process(logger, CancellationToken.None);

			var expectedMessage = @"
Information|[Warehouse Periodic Invoice I00000001] - Service task start processing billing invoice.
Information|Periodic Billing job status for client 111 warehouse 1 was successfully closed.
Information|There are no Posted Periodic Invoices to Deliver for client 111 warehouse 1.";
			AssertMultilineASCIIEquals("Should process deliver only.", expectedMessage, logger.ToString());
			AssertEquals("Should remove from queue.", StorageOffBandProcessingStatus.Codes.NIQ, NewFactory().Load<WhsInvoice>(invoice.PK).ET_OffBandProcessingStatus);
			AssertMultilineASCIIEquals("Add log to note!", expectedMessage + @"
Service task log
Time: 09-May-22 10:34", NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Single().ST_NoteText);
		}

		[TestDate(2022, 5, 9, 10, 35, 0)]
		public void TestAutoRateJobHeaderInUserContext_StatusPost_WithPostedInvoice()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoDeliverPeriodicInvoice = true;
			var debtor = Helper.CreateClient("debtor");
			var contact = debtor.Contacts.AddNew();
			contact.OC_ContactName = "me";
			contact.OC_Email = "me@wisetechglobal.com.au";
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Receivables.Code;
			document.OD_DefaultContact = true;

			var helper = new WhsTestHelperFunctions(Factory);
			var invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			invoice.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationPosted;
			invoice.TryCreateJobHeader();
			Factory.Save();
			var transactionHeader = testObjectCreator.CreateARInvoice<ARInvoice>("1", testObjectCreator.AUD, 1m, testObjectCreator.AALSHI);
			transactionHeader.AH_JH = invoice.JobHeader.PK;
			transactionHeader.AH_OH = debtor.PK;
			Factory.Save();

			var logger = GetLogger();
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(new WhsInvoiceHelper());
			processor.Process(logger, CancellationToken.None);

			var expectedMessage = @"
Information|[Warehouse Periodic Invoice I00000001] - Service task start processing billing invoice.
Information|Periodic Billing job status for client 111 warehouse 1 was successfully closed.
Information|Delivered Invoice for client 111 warehouse 1.";
			AssertMultilineASCIIEquals("Should process deliver only.", expectedMessage, logger.ToString());
			AssertEquals("Should remove from queue.", StorageOffBandProcessingStatus.Codes.NIQ, NewFactory().Load<WhsInvoice>(invoice.PK).ET_OffBandProcessingStatus);
			AssertMultilineASCIIEquals("Add log to note!", expectedMessage + @"
Service task log
Time: 09-May-22 10:35", NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Single().ST_NoteText);
		}

		#endregion

		#region TestAutoRateJobHeaderInUserContext_StatusDeliver

		[TestDate(2022, 5, 9, 10, 35, 0)]
		public void TestAutoRateJobHeaderInUserContext_StatusDeliver()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var helper = new WhsTestHelperFunctions(Factory);
			var invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			invoice.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationDelivered;
			Factory.Save();

			var logger = GetLogger();
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(new WhsInvoiceHelper());
			processor.Process(logger, CancellationToken.None);

			var expectedMessage = @"
Information|[Warehouse Periodic Invoice I00000001] - Service task start processing billing invoice.
Information|The invoice has already been delivered. If you need to reprocess please do it manually.";
			AssertMultilineASCIIEquals("Should log the error.", expectedMessage, logger.ToString());
			AssertEquals("Should remove from queue.", StorageOffBandProcessingStatus.Codes.NIQ, NewFactory().Load<WhsInvoice>(invoice.PK).ET_OffBandProcessingStatus);
			AssertMultilineASCIIEquals("Add log to note!", expectedMessage + @"
Service task log
Time: 09-May-22 10:35", NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Single().ST_NoteText);
		}

		#endregion

		#region TestRunTask_ChangeOffBandProcessingStatusToError

		[TestDate(2022, 5, 9, 18, 6, 0)]
		public void TestRunTask_ChangeOffBandProcessingStatusToError()
		{
			var client = Helper.CreateClient("C01");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse($"WHS", address, branch);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();
			AssertEquals("Precondition", false, NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Length > 0);

			var logger = GetLogger();
			var helper = new WhsInvoiceHelper();
			var dp = MockInvoiceHelperWithSetups(invoice);
			dp.Setup(d => d.InvoiceSaveSafe(It.IsAny<WhsInvoice>(), It.IsAny<ILogger>(), It.IsAny<int>()))
				.Callback((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) =>
				{
					if (attempt != 3)
					{
						if (invoiceIn.BillingAutomationStatus != BillingAutomationStatusCodes.BillingAutomationAutorated
								&& invoiceIn.BillingAutomationStatus != BillingAutomationStatusCodes.BillingAutomationPosted)
						{
							var exception = new ZCannotSaveException("Ops", "Ops");
							BusinessObjectFactory.SavingEventHandler throwError = f => throw exception;
							invoice.Factory.Saving += throwError;
							helper.InvoiceSaveSafe(invoice, logger, attempt);
							invoice.Factory.Saving -= throwError;
							invoiceIn.SetSystemDefinedValue("BillingAutomationStatus", ZString.Empty);
						}
					}
					else
					{
						helper.InvoiceSaveSafe(invoice, logger, attempt);
					}
				})
				.Returns((WhsInvoice invoiceIn, ILogger loggerIn, int attemptIn)
					=> invoiceIn.BillingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationAutorated
							|| invoiceIn.BillingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationPosted
						? InvoiceSaveSafeResult.SaveSuccessful
						: InvoiceSaveSafeResult.SaveFailedRetry);

			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
			processor.Process(logger, CancellationToken.None);

			var expectedMessage = GetLinkToInvoiceInfo("I00000001") + $@"
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Information|Delivered Invoice for client C01 warehouse WHS.
Error|Ops
(First attempt).
{GetLinkToInvoiceInfo("I00000001")}
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Information|Delivered Invoice for client C01 warehouse WHS.
Error|Ops
(Second attempt).
";
			AssertMultilineASCIIEquals("Should be processed when Still in queue.", expectedMessage, logger.ToString());

			var invoiceInNewFactory = new BusinessObjectFactory().Load<WhsInvoice>(invoice.PK);
			AssertEquals("Should change status to error.", StorageOffBandProcessingStatus.Codes.ERR, invoice.ET_OffBandProcessingStatus);
			AssertMultilineASCIIEquals("Add log to note!", expectedMessage + @"
Service task log
Time: 09-May-22 18:06", NewFactory().Load<WhsInvoice>(invoice.PK).Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).Single().ST_NoteText);
		}
		#endregion

		#region TestProcess_Retry

		public void TestProcess_Retry_AlwaysFail()
		{
			// Should not try more than two times
			var alwaysFail = 100;
			var expectedLog = GetLinkToInvoiceInfo("I00000001") + $@"
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Information|Delivered Invoice for client C01 warehouse WHS.
Error|Factory save error!
(First attempt).
{GetLinkToInvoiceInfo("I00000001")}
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Information|Delivered Invoice for client C01 warehouse WHS.
Error|Factory save error!
(Second attempt).
Error|Factory save error!
(When attempting to change status to error).";
			TestProcess_RetryCore(alwaysFail, expectedLog);
		}
		public void TestProcess_Retry_AlwaysPass()
		{
			var alwaysPass = 0;
			var expectedLog = GetLinkToInvoiceInfo("I00000001") + @"
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Information|Delivered Invoice for client C01 warehouse WHS.";
			TestProcess_RetryCore(alwaysPass, expectedLog);
		}

		public void TestProcess_Retry_FirstTryFailed()
		{
			var firstTryFailed = 1;
			var expectedLog = GetLinkToInvoiceInfo("I00000001") + $@"
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Information|Delivered Invoice for client C01 warehouse WHS.
Error|Factory save error!
(First attempt).
{GetLinkToInvoiceInfo("I00000001")}
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Information|Delivered Invoice for client C01 warehouse WHS.";
			TestProcess_RetryCore(firstTryFailed, expectedLog);
		}

		public void TestProcess_Retry_FirstAndSecondTryFailed()
		{
			var firstAndSecondTryFailed = 2;
			var expectedLog = GetLinkToInvoiceInfo("I00000001") + $@"
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Information|Delivered Invoice for client C01 warehouse WHS.
Error|Factory save error!
(First attempt).
{GetLinkToInvoiceInfo("I00000001")}
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Information|Delivered Invoice for client C01 warehouse WHS.
Error|Factory save error!
(Second attempt).";
			TestProcess_RetryCore(firstAndSecondTryFailed, expectedLog);
		}

		void TestProcess_RetryCore(int successAfterAttempt, string expectedLog)
		{
			var client = Helper.CreateClient("C01");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse($"WHS", address, branch);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var lastValidSaveStatus = invoice.BillingAutomationStatus;
			var logger = GetLogger();
			var dp = MockInvoiceHelperWithSetups(invoice);
			var attempt = 0;
			dp.Setup(d => d.InvoiceSaveSafe(invoice, logger, It.IsAny<int>()))
			.Callback((WhsInvoice invoiceIn, ILogger loggerIn, int attemptIn) =>
			{
				if (successAfterAttempt > attempt)
				{
					if (invoiceIn.BillingAutomationStatus != BillingAutomationStatusCodes.BillingAutomationAutorated
							&& invoiceIn.BillingAutomationStatus != BillingAutomationStatusCodes.BillingAutomationPosted)
					{
						invoiceIn.SetSystemDefinedValue("BillingAutomationStatus", lastValidSaveStatus);
						var attempStr = new WhsInvoiceHelper().AttemptToStrConvert(attemptIn);
						var msg = new ZStringBuilder(new[] { "Factory save error!", attempStr }).ToStringWithNewLineBetweenAppends();
						loggerIn.Error(msg);
					}
				}
				else
				{
					lastValidSaveStatus = invoice.BillingAutomationStatus;
				}
			})
			.Returns((WhsInvoice invoiceIn, ILogger loggerIn, int attemptIn) =>
			{
				if (invoiceIn.BillingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationAutorated
						|| invoiceIn.BillingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationPosted)
				{
					return InvoiceSaveSafeResult.SaveSuccessful;
				}
				else
				{
					attempt++;
					var retry = successAfterAttempt >= attempt;
					return retry ? InvoiceSaveSafeResult.SaveFailedRetry : InvoiceSaveSafeResult.SaveFailedNoRetry;
				}
			});
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
			processor.Process(logger, CancellationToken.None);

			AssertMultilineASCIIEquals(expectedLog, logger.ToString());
		}

		public void TestProcess_Retry_UnHandelException()
		{
			var client = Helper.CreateClient("C01");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse($"WHS", address, branch);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var logger = GetLogger();
			var dp = MockInvoiceHelperWithSetups(invoice);
			dp.Setup(d => d.InvoiceSaveSafe(invoice, logger, It.IsAny<int>()))
			.Callback((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) =>
			{
				if (invoiceIn.BillingAutomationStatus != BillingAutomationStatusCodes.BillingAutomationAutorated
						&& invoiceIn.BillingAutomationStatus != BillingAutomationStatusCodes.BillingAutomationPosted)
				{
					throw new InvalidOperationException("Unhandle exception!");
				}
			})
			.Returns((WhsInvoice invoiceIn, ILogger loggerIn, int attemptIn)
				=> (invoiceIn.BillingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationAutorated
						|| invoiceIn.BillingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationPosted)
					? InvoiceSaveSafeResult.SaveSuccessful
					: InvoiceSaveSafeResult.SaveFailedRetry);

			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
			AssertExceptionThrown<InvalidOperationException>(() => processor.Process(logger, CancellationToken.None));

			AssertMultilineASCIIEquals("Should not log exception.", GetLinkToInvoiceInfo("I00000001") + @"
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Information|Delivered Invoice for client C01 warehouse WHS.
Error|Unhandle exception!", logger.ToString());
		}

		#endregion

		#region TestProcess_InvoiceSaveSafeResult

		public void TestProcess_InvoiceSaveSafeResult_SaveSuccessful()
		{
			var expectedLog = GetLinkToInvoiceInfo("I00000001") + @"
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Information|Delivered Invoice for client C01 warehouse WHS.";
			TestProcess_InvoiceSaveSafeResultCore(InvoiceSaveSafeResult.SaveSuccessful, expectedLog, "1 save end of process and 1 save for after autorating and 1 save for post", () => Times.Exactly(3));
		}

		public void TestProcess_InvoiceSaveSafeResult_SaveFailedRetry()
		{
			var expectedLog = GetLinkToInvoiceInfo("I00000001") + $@"
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Error|Factory save error!
(First attempt).
{GetLinkToInvoiceInfo("I00000001")}
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Error|Factory save error!
(Second attempt).
Error|Factory save error!
(When attempting to change status to error).";

			TestProcess_InvoiceSaveSafeResultCore(InvoiceSaveSafeResult.SaveFailedRetry, expectedLog, "3 save and retry and 2 save for after autorating and 2 save for after posting", () => Times.Exactly(5));
		}

		public void TestProcess_InvoiceSaveSafeResult_SaveFailedNoRetry()
		{
			var expectedLog = GetLinkToInvoiceInfo("I00000001") + @"
Information|Autorated Invoice for client C01 warehouse WHS.
Information|Posted Invoice for client C01 warehouse WHS.
Error|Factory save error!
(First attempt).
Error|Factory save error!
(When attempting to change status to error).";

			TestProcess_InvoiceSaveSafeResultCore(InvoiceSaveSafeResult.SaveFailedNoRetry, expectedLog, "2 save end of process and 1 save for after autorating and 1 save for after posting", () => Times.Exactly(3));
		}

		void TestProcess_InvoiceSaveSafeResultCore(InvoiceSaveSafeResult invoiceSaveSafeResult, string expectedLog, string verifySaveMsg, Func<Times> saveCallsTimes)
		{
			var client = Helper.CreateClient("C01");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse($"WHS", address, branch);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var lastValidSaveStatus = invoice.BillingAutomationStatus;
			var logger = GetLogger();
			var dp = MockInvoiceHelperWithSetups(invoice);
			dp.Setup(d => d.InvoiceSaveSafe(invoice, logger, It.IsAny<int>()))
				.Callback((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) =>
				{
					if (invoiceSaveSafeResult != InvoiceSaveSafeResult.SaveSuccessful && invoiceIn.BillingAutomationStatus != BillingAutomationStatusCodes.BillingAutomationAutorated)
					{
						invoiceIn.SetSystemDefinedValue("BillingAutomationStatus", lastValidSaveStatus);
						var msgAttempt = new WhsInvoiceHelper().AttemptToStrConvert(attempt);
						var msg = $@"Factory save error!
{msgAttempt}";
						loggerIn.Error(msg);
					}
					else if (invoiceSaveSafeResult == InvoiceSaveSafeResult.SaveSuccessful)
					{
						lastValidSaveStatus = invoice.BillingAutomationStatus;
					}
				})
				.Returns((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) => invoiceIn.BillingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationAutorated ? InvoiceSaveSafeResult.SaveSuccessful : invoiceSaveSafeResult);
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
			processor.Process(logger, CancellationToken.None);

			AssertMultilineASCIIEquals(expectedLog, logger.ToString());
			dp.Verify(d => d.InvoiceSaveSafe(It.IsAny<WhsInvoice>(), It.IsAny<ILogger>(), It.IsAny<int>()), saveCallsTimes, verifySaveMsg);
		}

		#endregion

		#region TestProcess_AutoRateCheckLockSuspender

		public void TestProcess_AutoRateCheckLockSuspender()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			AssertEquals("Precondition", false, invoice.IsInvoiceBillingCheckSuspended);

			var logger = GetLogger();
			var dp = MockInvoiceHelperWithSetups(invoice);
			dp.Setup(d => d.AutoRateJobHeader(It.IsAny<IAutoRatingServiceLogger>(), invoice))
				.Callback((ILogger loggerIn, WhsInvoice invoiceIn) =>
				{
					loggerIn.Information("AutoRateJobHeaderInUserContext callback called!");
					AssertEquals(true, invoiceIn.IsInvoiceBillingCheckSuspended);
				})
				.Returns(true);

			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
			processor.Process(logger, CancellationToken.None);

			AssertMultilineASCIIEquals(GetLinkToInvoiceInfo("I00000001") + @"
Information|AutoRateJobHeaderInUserContext callback called!
Information|Posted Invoice for client 111 warehouse 1.
Information|Delivered Invoice for client 111 warehouse 1.", logger.ToString());
		}

		#endregion

		#region TestProcess_FailToSaveAfterAutoRate

		public void TestProcess_FailToSaveAfterAutoRate_FirstAttemptFailed()
		{
			var expectedLog = $@"
Information|AutoRateJobHeaderInUserContext callback called!
Information|Failed to save!
{GetLinkToInvoiceInfo("I00000001")}
Information|AutoRateJobHeaderInUserContext callback called!
Information|Saved
Information|Posted Invoice for client 111 warehouse 1.
Information|Saved
Information|Delivered Invoice for client 111 warehouse 1.
Information|Saved";

			TestProcess_FailToSaveAfterAutoRateCore(failedAttempt: 1, expectedLog);
		}

		public void TestProcess_FailToSaveAfterAutoRate_NotAbleToSaveAutoRate()
		{
			var expectedLog = $@"
Information|AutoRateJobHeaderInUserContext callback called!
Information|Failed to save!
{GetLinkToInvoiceInfo("I00000001")}
Information|AutoRateJobHeaderInUserContext callback called!
Information|Failed to save!
Information|Saved";

			TestProcess_FailToSaveAfterAutoRateCore(failedAttempt: 2, expectedLog);
		}

		public void TestProcess_FailToSaveAfterAutoRate_NotAbleToSaveAnything()
		{
			var expectedLog = $@"
Information|AutoRateJobHeaderInUserContext callback called!
Information|Failed to save!
{GetLinkToInvoiceInfo("I00000001")}
Information|AutoRateJobHeaderInUserContext callback called!
Information|Failed to save!
Information|Failed to save!";

			TestProcess_FailToSaveAfterAutoRateCore(failedAttempt: 3, expectedLog);
		}

		void TestProcess_FailToSaveAfterAutoRateCore(int failedAttempt, string expectedLog)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			AssertEquals("Precondition", false, invoice.IsInvoiceBillingCheckSuspended);

			var logger = GetLogger();
			var dp = MockInvoiceHelperWithSetups(invoice);
			dp.Setup(d => d.AutoRateJobHeader(It.IsAny<IAutoRatingServiceLogger>(), invoice))
				.Callback((ILogger loggerIn, WhsInvoice invoiceIn) =>
				{
					loggerIn.Information("AutoRateJobHeaderInUserContext callback called!");
					AssertEquals(true, invoiceIn.IsInvoiceBillingCheckSuspended);
				})
				.Returns(true);
			dp.Setup(d => d.InvoiceSaveSafe(invoice, It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<int>()))
				.Callback((WhsInvoice invoiceIn, ILogger loggerIn, int attemptIn) =>
				{
					if (failedAttempt >= attemptIn)
					{
						loggerIn.Information("Failed to save!");
					}
					else
					{
						loggerIn.Information("Saved");
					}
				})
				.Returns((WhsInvoice invoiceIn, ILogger loggerIn, int attemptIn) => failedAttempt >= attemptIn ? InvoiceSaveSafeResult.SaveFailedRetry : InvoiceSaveSafeResult.SaveSuccessful);

			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
			processor.Process(logger, CancellationToken.None);

			AssertMultilineASCIIEquals(GetLinkToInvoiceInfo("I00000001") + expectedLog, logger.ToString());
		}

		#endregion

		#region TestAutoRateJobHeader_Exception

		public void TestAutoRateJobHeader_Exception_AutoRateJobHeader()
		{
			var expectedLog = @"
Information|[Warehouse Periodic Invoice I00000001] - Service task start processing billing invoice.
Error|Something wrong!";
			TestAutoRateJobHeader_Exception_Core(d => d.AutoRateJobHeader(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()), expectedLog);
		}

		public void TestAutoRateJobHeader_Exception_PostInvoice()
		{
			var expectedLog = @"
Information|[Warehouse Periodic Invoice I00000001] - Service task start processing billing invoice.
Information|Autorated Invoice for client 111 warehouse 1.
Error|Something wrong!";
			TestAutoRateJobHeader_Exception_Core(d => d.PostInvoice(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()), expectedLog);
		}

		public void TestAutoRateJobHeader_Exception_DeliverInvoice()
		{
			var expectedLog = @"
Information|[Warehouse Periodic Invoice I00000001] - Service task start processing billing invoice.
Information|Autorated Invoice for client 111 warehouse 1.
Information|Posted Invoice for client 111 warehouse 1.
Error|Something wrong!";
			TestAutoRateJobHeader_Exception_Core(d => d.DeliverInvoice(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()), expectedLog);
		}

		void TestAutoRateJobHeader_Exception_Core(Expression<Func<IWhsInvoiceHelper, bool>> exceptionThrownFrom, string expectedLog)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var logger = GetLogger();
			var dp = MockInvoiceHelperWithSetups(invoice);
			dp.Setup(exceptionThrownFrom).Callback((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
			{
				throw new Exception("Something wrong!");
			}).Returns(true);
			dp.Setup(d => d.CloseBillingAndRelatedJob(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>())).Returns(true);
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);

			AssertExceptionThrown<Exception>("Should only auto rate invoices with queue status.", "Something wrong!", () => processor.Process(logger, CancellationToken.None));
			AssertMultilineASCIIEquals(expectedLog, logger.ToString());
			dp.Verify(mcm => mcm.DisposeLoadedJobHeaders(It.IsAny<BusinessObjectFactory>()), Times.Once);
		}

		#endregion

		#region TestGetCandidateInvoices_RunProperMethodBasedOnBillingAutomationStatus

		public void TestGetCandidateInvoices_RunProperMethodBasedOnBillingAutomationStatus_Default()
		{
			TestGetCandidateInvoices_RunProperMethodBasedOnBillingAutomationStatusCore(billingAutomationPosted: "", numberOfAutorateCall: Times.Once, numberOfPostCall: Times.Once, numberOfDeliverCall: Times.Once);
		}

		public void TestGetCandidateInvoices_RunProperMethodBasedOnBillingAutomationStatus_BillingAutomationAutorated()
		{
			TestGetCandidateInvoices_RunProperMethodBasedOnBillingAutomationStatusCore(billingAutomationPosted: BillingAutomationStatusCodes.BillingAutomationAutorated, numberOfAutorateCall: Times.Once, numberOfPostCall: Times.Once, numberOfDeliverCall: Times.Once);
		}

		public void TestGetCandidateInvoices_RunProperMethodBasedOnBillingAutomationStatus_BillingAutomationPosted()
		{
			TestGetCandidateInvoices_RunProperMethodBasedOnBillingAutomationStatusCore(billingAutomationPosted: BillingAutomationStatusCodes.BillingAutomationPosted, numberOfAutorateCall: Times.Never, numberOfPostCall: Times.Never, numberOfDeliverCall: Times.Once);
		}

		public void TestGetCandidateInvoices_RunProperMethodBasedOnBillingAutomationStatus_BillingAutomationDelivered()
		{
			TestGetCandidateInvoices_RunProperMethodBasedOnBillingAutomationStatusCore(billingAutomationPosted: BillingAutomationStatusCodes.BillingAutomationDelivered, numberOfAutorateCall: Times.Never, numberOfPostCall: Times.Never, numberOfDeliverCall: Times.Never);
		}

		void TestGetCandidateInvoices_RunProperMethodBasedOnBillingAutomationStatusCore(string billingAutomationPosted, Func<Times> numberOfAutorateCall, Func<Times> numberOfPostCall, Func<Times> numberOfDeliverCall)
		{
			var client = Helper.CreateClient("C01");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse($"WHS", address, branch);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			if (!string.IsNullOrEmpty(billingAutomationPosted))
			{
				invoice.BillingAutomationStatus = billingAutomationPosted;
			}
			Factory.Save();

			AssertEquals("Precondition", invoice.BillingAutomationStatus, billingAutomationPosted);

			var logger = GetLogger();
			var dp = MockInvoiceHelperWithSetups(invoice);
			var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
			processor.Process(logger, CancellationToken.None);

			dp.Verify(d => d.AutoRateJobHeader(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()), numberOfAutorateCall);
			dp.Verify(d => d.PostInvoice(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()), numberOfPostCall);
			dp.Verify(d => d.DeliverInvoice(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()), numberOfDeliverCall);
		}

		#endregion

		#region MockInvoiceHelperWithSetups

		public static Mock<IWhsInvoiceHelper> MockInvoiceHelperWithSetups(params WhsInvoice[] invoices)
		{
			var mutex = new Mock<IZGlobalMutex>();
			mutex.Setup(d => d.HasLock).Returns(false);
			mutex.Setup(d => d.IsLocked).Returns(false);
			mutex.Setup(d => d.Lock()).Returns(true);
			var helper = new Mock<IWhsInvoiceHelper>();
			helper.Setup(d => d.GetQueuedInvoiceValidToProcessPKs()).Returns(invoices.Select(i => i.PK));
			helper.Setup(d => d.GetInvoiceBillingAutomationMutex(It.IsAny<ZGuid>())).Returns(mutex.Object);
			helper.Setup(d => d.AutoRateJobHeader(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>())).Callback((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
			{
				invoiceIn.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationAutorated;
				loggerIn.Log(LogType.Information, string.Format("Autorated Invoice for client {0} warehouse {1}.", invoiceIn.ClientName, invoiceIn.WarehouseName));
			}).Returns(true);
			helper.Setup(d => d.PostInvoice(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>())).Callback((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
			{
				invoiceIn.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationPosted;
				loggerIn.Log(LogType.Information, string.Format("Posted Invoice for client {0} warehouse {1}.", invoiceIn.ClientName, invoiceIn.WarehouseName));
			}).Returns(true);
			helper.Setup(d => d.DeliverInvoice(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>())).Callback((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
			{
				invoiceIn.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationDelivered;
				loggerIn.Log(LogType.Information, string.Format("Delivered Invoice for client {0} warehouse {1}.", invoiceIn.ClientName, invoiceIn.WarehouseName));
			}).Returns(true);
			foreach (var invoice in invoices)
			{
				helper.Setup(d => d.GetByPK(invoice.PK)).Returns(invoice);
				helper.Setup(d => d.LoadInNewFactory(invoice.PK)).Returns(invoice);
				helper.Setup(d => d.GetInvoiceReferNumber(invoice)).Returns(() => new WhsInvoiceHelper().GetInvoiceReferNumber(invoice));
				helper.Setup(d => d.IsStillInQueueInDB(invoice.PK))
					.Returns(() => invoice.ET_OffBandProcessingStatus == StorageOffBandProcessingStatus.Codes.QUE);
			}
			return helper;
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		static IAutoRatingServiceLogger GetLogger() => new BillingAutomationServiceLogger(new TestServiceLogger());

		public static string GetLinkToInvoiceInfo(string invoiceId) => string.Format(LinkToInvoiceInfo, invoiceId);

		const string LinkToInvoiceInfo = "Information|[Warehouse Periodic Invoice {0}] - Service task start processing billing invoice.";

		#endregion
	}

	public class WhsAutoRateQueuedInvoicesProcessSynchronouslyTestCase : TestCase
	{
		#region TestRunTask_OtherInstanceNotWorkOnSameJob

		[UseSnapshotProtection]
		public void TestRunTask_OtherInstanceNotWorkOnSameJob()
		{
			using (Db.DisposableActionForDbConnection())
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				var helper = new WhsTestHelperFunctions(factory);
				var data = new TestDataSimpleEnvironment(factory);
				var client1 = helper.CreateClient("C01");
				var client2 = helper.CreateClient("C02");
				var invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, client1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
				invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
				factory.Save();
			}
			var logger1 = GetLogger();
			var logger2 = GetLogger();

			var firstTaskStarted = new TaskCompletionSource<bool>();
			var task1 = Task.Factory.StartNew(() =>
			{
				RunServiceTaskWithTimeConsumingProcessesSimulation(logger1, firstTaskStarted);
			});
			var task2 = Task.Factory.StartNew(() =>
			{
				firstTaskStarted.Task.GetAwaiter().GetResult();
				RunServiceTaskWithTimeConsumingProcessesSimulation(logger2);
			});
			Parallel.ForEach(new[] { task1, task2 }, t => t.Wait());

			AssertMultilineASCIIEquals("Should auto rate.", WhsAutoRateQueuedInvoicesProcessSynchronouslyTransactionedTestCase.GetLinkToInvoiceInfo("I00000001") + @"
Information|Autorated Invoice for client C01 warehouse 1.
Information|Posted Invoice for client C01 warehouse 1.
Information|Delivered Invoice for client C01 warehouse 1.", logger1.ToString());

			AssertMultilineASCIIEquals("Should be lock when other instance start processing the invoice.",
				"Information|[Warehouse Periodic Invoice I00000001] - Another instance of the service task is processing the invoice for client C01 warehouse 1 or it cannot be locked.", logger2.ToString());
		}

		#endregion

		#region TestRunTask_ParallelProcessing

		[UseSnapshotProtection]
		[TestDate(2022, 6, 27, 7, 37, 0)]
		public void TestRunTask_ParallelProcessing()
		{
			WhsInvoice invoice;
			using (Db.DisposableActionForDbConnection())
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				var helper = new WhsTestHelperFunctions(factory);
				var data = new TestDataSimpleEnvironment(factory);
				var client1 = helper.CreateClient("C01");
				var client2 = helper.CreateClient("C02");
				invoice = (WhsInvoice)helper.CreateWhsInvoice(data.Whs1.PK, client1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
				invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
				factory.Save();
			}
			var logger1 = GetLogger();
			var logger2 = GetLogger();

			var firstTaskStarted = new TaskCompletionSource<bool>();
			var task1 = Task.Factory.StartNew(() =>
			{
				RunServiceTaskWithTimeConsumingProcessesSimulation(logger1, firstTaskStarted);
			});
			var task2 = Task.Factory.StartNew(() =>
			{
				firstTaskStarted.Task.GetAwaiter().GetResult();
				RunServiceTaskWithTimeConsumingProcessesSimulation(logger2);
			});
			Parallel.ForEach(new[] { task1, task2 }, t => t.Wait());

			AssertMultilineASCIIEquals("The instances of service tasks should be able to process jobs simultaneously.", WhsAutoRateQueuedInvoicesProcessSynchronouslyTransactionedTestCase.GetLinkToInvoiceInfo("I00000001") + @"
Information|Autorated Invoice for client C01 warehouse 1.
Information|Posted Invoice for client C01 warehouse 1.
Information|Delivered Invoice for client C01 warehouse 1.
", logger1.ToString());

			AssertMultilineASCIIEquals("The instances of service tasks should be able to process jobs simultaneously.", @"
Information|[Warehouse Periodic Invoice I00000001] - Another instance of the service task is processing the invoice for client C01 warehouse 1 or it cannot be locked.
", logger2.ToString());

			var note = invoice.Notes.FindByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description).SingleOrDefault();
			AssertMultilineASCIIEquals("Should only autorate log.", @"
Information|[Warehouse Periodic Invoice I00000001] - Service task start processing billing invoice.
Information|Autorated Invoice for client C01 warehouse 1.
Information|Posted Invoice for client C01 warehouse 1.
Information|Delivered Invoice for client C01 warehouse 1.
Service task log
Time: 27-Jun-22 07:37
", note.ST_NoteText);
		}

		#endregion

		#region RunServiceTaskWithTimeConsumingProcessesSimulation

		void RunServiceTaskWithTimeConsumingProcessesSimulation(IAutoRatingServiceLogger logger, TaskCompletionSource<bool> firstTaskStarted = null)
		{
			using (Db.DisposableActionForDbConnection())
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var factory = new BusinessObjectFactory(connection);
				var invoices = factory.Load<WhsInvoice>(new ZQuery()).OrderBy(i => i.ClientName).ToArray();
				var dp = WhsAutoRateQueuedInvoicesProcessSynchronouslyTransactionedTestCase.MockInvoiceHelperWithSetups(invoices);
				dp.Setup(d => d.InvoiceSaveSafe(It.IsAny<WhsInvoice>(), It.IsAny<ILogger>(), It.IsAny<int>())).Callback(() => Thread.Sleep(100));

				foreach (var invoice in invoices)
				{
					dp.Setup(d => d.GetInvoiceBillingAutomationMutex(invoice.PK)).Returns(new DummyMutex(invoice.PK));
					dp.Setup(d => d.LoadInNewFactory(invoice.PK)).Returns(() =>
					{
						firstTaskStarted?.TrySetResult(true);
						return invoice;
					});
				}

				var processor = new WhsAutoRateQueuedInvoicesProcessSynchronously(dp.Object);
				processor.Process(logger, CancellationToken.None);
			}
		}

		#endregion

		#region GetLogger

		static IAutoRatingServiceLogger GetLogger() => new BillingAutomationServiceLogger(new TestServiceLogger());

		#endregion

		#region DummyMutex

		public class DummyMutex : IZGlobalMutex
		{
			static readonly object locker = new object();
			static readonly Lazy<HashSet<ZGuid>> activeLocks = new Lazy<HashSet<ZGuid>>(() => new HashSet<ZGuid>());

			public DummyMutex(ZGuid pk)
			{
				PK = pk;
			}
			ZGuid PK { get; }

			static HashSet<ZGuid> ActiveLocks => activeLocks.Value;

			public ZBool HasLock => false;

			public ZBool IsLocked
			{
				get
				{
					lock (locker)
					{
						return ActiveLocks.Contains(PK);
					}
				}
			}

			public MutexID MutexID => MutexIDs.WhsInvoiceAutoRate;

			public ZString RecordIdentifier => throw new NotImplementedException();
			public LockInfo GetLockInfo() => throw new NotImplementedException();

			public bool Lock()
			{
				lock (locker)
				{
					var result = !ActiveLocks.Contains(PK);
					if (result)
					{
						ActiveLocks.Add(PK);
					}
					return result;
				}
			}

			public void Unlock()
			{
				lock (locker)
				{
					ActiveLocks.Remove(PK);
				}
			}

			public void Dispose()
			{
				lock (locker)
				{
					if (ActiveLocks.Contains(PK))
					{
						Unlock();
					}
				}
			}
		}

		#endregion
	}
}
