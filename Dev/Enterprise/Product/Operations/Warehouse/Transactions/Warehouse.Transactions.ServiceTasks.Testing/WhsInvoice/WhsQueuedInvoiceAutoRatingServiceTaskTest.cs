using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.Warehouse.Transactions.ServiceTasks;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsQueuedInvoiceAutoRatingServiceTask))]
	public class WhsQueuedInvoiceAutoRatingServiceTaskTest : ServiceTaskTestCase<WhsQueuedInvoiceAutoRatingServiceTask>
	{
		#region TestInitialiseTask

		public void TestInitialiseTask()
		{
			var attribute = GetHostedServiceAttributes().FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("1week", attribute.DefaultScheduleRunEvery);
				AssertContainsExactElementsInAnyOrder(new[] { DayOfWeek.Saturday }, attribute.DefaultScheduleDaysOfWeek);
				AssertEquals("4hours", attribute.DefaultScheduleStartAtLocal);
			});
		}

		#endregion

		#region TestServiceTask

		public void TestServiceTask()
		{
			var logger = RunServiceTask();

			AssertMultilineASCIIEquals("logger.ToString()", @"
Information|Autorating Queued Periodic Invoices Service Task started.
Information|There are no invoices in the queue to be processed.
Information|Autorating Queued Periodic Invoices Service Task completed.
".Trim(), logger.ToString());
		}

		public void TestServiceTask_EndToEnd()
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

			var logger = RunServiceTask();

			AssertMultilineASCIIEquals("logger.ToString()", @"
Information|Autorating Queued Periodic Invoices Service Task started.
Information|[Warehouse Periodic Invoice I00000001] - Service task start processing billing invoice.
Information|Autorated Invoice for client 111 warehouse 1.
Information|Posted Invoice for client 111 warehouse 1.
Information|Periodic Billing job status for client 111 warehouse 1 was successfully closed.
Information|Delivered Invoice for client 111 warehouse 1.
Information|Autorating Queued Periodic Invoices Service Task completed.
".Trim(), logger.ToString());
		}

		#endregion

		#region TestRunTask_AndPassSameToken

		public void TestRunTask_AndPassSameToken()
		{
			var mockedAutoRateQueuedInvoicesProcessSynchronously = new Mock<IWhsAutoRateQueuedInvoicesProcessSynchronously>();
			var cancellationToken = new CancellationTokenSource();
			using (ObjectFactory.Substitute(mockedAutoRateQueuedInvoicesProcessSynchronously.Object))
			{
				var task = new WhsQueuedInvoiceAutoRatingServiceTask();
				var serviceLog = InitialiseAndRunTaskSchedule(task, cancellationToken.Token);
				AssertEquals(2, serviceLog.Count);
				AssertEquals("Information|Autorating Queued Periodic Invoices Service Task started.", serviceLog[0]);
				AssertEquals("Information|Autorating Queued Periodic Invoices Service Task completed.", serviceLog[1]);
				mockedAutoRateQueuedInvoicesProcessSynchronously.Verify(wpi => wpi.Process(It.IsAny<BillingAutomationServiceLogger>(), cancellationToken.Token), Times.Once);
			}
		}

		#endregion

		#region TestRunTask_ObjectFactory_Setup

		public void TestRunTask_ObjectFactory_Setup()
		{
			var serviceLog = RunServiceTask();

			AssertMultilineASCIIEquals(@"
Information|Autorating Queued Periodic Invoices Service Task started.
Information|There are no invoices in the queue to be processed.
Information|Autorating Queued Periodic Invoices Service Task completed.
", serviceLog.ToString());
		}

		#endregion

		#region TestAllowMultipleInstances

		public void TestAllowMultipleInstances()
		{
			var thisClassAttribute = GetHostedServiceAttributes().SingleOrDefault();
			if (thisClassAttribute != null)
			{
				AssertEquals("AllowsMultipleInstances", true, thisClassAttribute.AllowsMultipleInstances);
			}
		}

		#endregion

		#region ExpectedHostedServiceBusinessObjectBindingAttributes

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						JobStorageSchema.Constants.TableName,
						"Invoice in queue",
						JobStorageSchema.Constants.ET_OffBandProcessingStatus + "=" + StorageOffBandProcessingStatus.Codes.QUE),
				};
			}
		}

		#endregion

		#region TestCloseInvoiceAndRelatedJobsWhenAutoPostEnabled

		public void TestCloseInvoiceAndRelatedJobsWhenAutoPostEnabled()
		{
			var client = TestHelper.CreateClient("Client");
			var branch = TestHelper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = TestHelper.CreateWarehouse("WHS", address, branch);
			TestHelper.CreateRowAndGenerateLocations(whs, "A");
			var product = TestHelper.CreateProduct("Product", client);
			TestHelper.CreateProductUnit(product, PkgUnit.Unit, PkgUnit.Spool, partUnitSize: 5m);
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var today = ZDate.Today;

			var yesterday = today.AddDays(-1);
			var receive = SetupDataToBeAbleToChargeClientInInvoice(client, whs, product, 1, yesterday, today);

			var storageFromDate = today.AddDays(-7);
			var invoice = (WhsInvoice)TestHelper.CreateWhsInvoice(whs.PK, client.PK, storageFromDate, today);
			var finalisedDate = new ZDateTimeOffset(today.AddDays(-6));
			var order = CreateFinalisedWhsOrder(client, whs, finalisedDate, product);

			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;

			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
			Factory.Save();

			RunServiceTask();

			AssertEquals("Precondition - ensure the Job was closed.", JobHeaderStatus.Closed.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Closing the Periodic Invoice should have also closed the Receive's JobHeader.", JobHeaderStatus.Closed.Code, receive.JobHeader.JH_Status);
			AssertEquals("Closing the Periodic Invoice should have also closed the Order's JobHeader.", JobHeaderStatus.Closed.Code, order.JobHeader.JH_Status);
		}

		#endregion

		#region TestInvoiceAndRelatedJobsNotClosedWhenAutoPostDisabled

		public void TestInvoiceAndRelatedJobsNotClosedWhenAutoPostDisabled()
		{
			var client = TestHelper.CreateClient("Client");
			var branch = TestHelper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = TestHelper.CreateWarehouse("WHS", address, branch);
			TestHelper.CreateRowAndGenerateLocations(whs, "A");
			var product = TestHelper.CreateProduct("Product", client);
			TestHelper.CreateProductUnit(product, PkgUnit.Unit, PkgUnit.Spool, partUnitSize: 5m);
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var today = ZDate.Today;

			var yesterday = today.AddDays(-1);
			var receive = SetupDataToBeAbleToChargeClientInInvoice(client, whs, product, 1, yesterday, today);

			var storageFromDate = today.AddDays(-7);
			var invoice = (WhsInvoice)TestHelper.CreateWhsInvoice(whs.PK, client.PK, storageFromDate, today);
			var finalisedDate = new ZDateTimeOffset(today.AddDays(-6));
			var order = CreateFinalisedWhsOrder(client, whs, finalisedDate, product);

			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;

			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			client.CompanyData.OB_WhsAutoPostPeriodicInvoice = false;
			Factory.Save();

			RunServiceTask();

			AssertEquals("Precondition - ensure the Job was not closed.", JobHeaderStatus.Working.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Related Receive Job not closed.", JobHeaderStatus.Working.Code, receive.JobHeader.JH_Status);
			AssertEquals("Related Order Job not closed.", JobHeaderStatus.Working.Code, order.JobHeader.JH_Status);
		}

		#endregion

		#region TestCloseInvoiceAndRelatedJobsRaiseError

		public void TestCloseInvoiceAndRelatedJobsRaiseError()
		{
			var client = TestHelper.CreateClient("Client");
			var branch = TestHelper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = TestHelper.CreateWarehouse("WHS", address, branch);
			TestHelper.CreateRowAndGenerateLocations(whs, "A");
			var product = TestHelper.CreateProduct("Product", client);
			TestHelper.CreateProductUnit(product, PkgUnit.Unit, PkgUnit.Spool, partUnitSize: 5m);
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var today = ZDate.Today;

			var yesterday = today.AddDays(-1);
			var receive = SetupDataToBeAbleToChargeClientInInvoice(client, whs, product, 1, yesterday, today);

			var storageFromDate = today.AddDays(-7);
			var invoice = (WhsInvoice)TestHelper.CreateWhsInvoice(whs.PK, client.PK, storageFromDate, today);
			var finalisedDate = new ZDateTimeOffset(today.AddDays(-6));
			var order = CreateFinalisedWhsOrder(client, whs, finalisedDate, product);

			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;

			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
			Factory.Save();

			var dp = WhsAutoRateQueuedInvoicesProcessSynchronouslyTransactionedTestCase.MockInvoiceHelperWithSetups(invoice);
			var invoiceHelper = new WhsInvoiceHelper();
			dp.Setup(d => d.AutoRateJobHeader(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()))
				.Callback((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
				{
					invoice.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationAutorated;
					invoiceHelper.AutoRateJobHeader(loggerIn, invoiceIn);
				}).Returns(true);
			dp.Setup(d => d.CloseBillingAndRelatedJob(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()))
				.Callback((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
				{
					invoiceIn.JobStatusWithPromptlessRelatedJobUpdate = JobHeaderStatus.Closed.Code;
					loggerIn.Information("Close Invoice for client Client warehouse WHS.");
				}).Returns(true);
			dp.Setup(d => d.DisposeLoadedJobHeaders(It.IsAny<BusinessObjectFactory>()))
				.Callback((BusinessObjectFactory factoryIn) =>
				{
					invoiceHelper.DisposeLoadedJobHeaders(factoryIn);
				});
			dp.Setup(d => d.InvoiceSaveSafe(It.IsAny<WhsInvoice>(), It.IsAny<ILogger>(), It.IsAny<int>()))
				.Callback((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) =>
				{
					if (invoiceIn.JobHeader != null && invoiceIn.JobHeader.JH_Status == JobHeaderStatus.Closed.Code && attempt < 3)
					{
						invoiceIn.Factory.Saving += (f) => throw new ZCannotSaveException("Ops!", "Test");
					}
					else
					{
						invoiceHelper.InvoiceSaveSafe(invoiceIn, loggerIn, attempt);
					}
				})
				.Returns((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) =>
				{
					if (invoiceIn.JobHeader != null && invoiceIn.JobHeader.JH_Status == JobHeaderStatus.Closed.Code && attempt < 3)
					{
						loggerIn.Information("Save failed.");
						return InvoiceSaveSafeResult.SaveFailedRetry;
					}
					else
					{
						loggerIn.Information("Save succeed.");
						return InvoiceSaveSafeResult.SaveSuccessful;
					}
				});
			dp.Setup(d => d.LoadInNewFactory(It.IsAny<ZGuid>()))
			.Returns((ZGuid pk) =>
			{
				return invoiceHelper.LoadInNewFactory(pk);
			});
			var mutex = new Mock<IZGlobalMutex>();
			mutex.Setup(d => d.HasLock).Returns(false);
			mutex.Setup(d => d.IsLocked).Returns(false);
			mutex.Setup(d => d.Lock()).Returns(true);
			dp.Setup(d => d.GetInvoiceBillingAutomationMutex(It.IsAny<ZGuid>())).Returns(mutex.Object);
			var serviceLog = new TestServiceLogger();
			using (ObjectFactory.Substitute(dp.Object))
			{
				serviceLog = RunServiceTask();
				invoiceHelper.DisposeLoadedJobHeaders(invoice.Factory);
			}

			invoice = new BusinessObjectFactory().Load<WhsInvoice>(invoice.PK);

			var expectedMessage = @"Information|Autorating Queued Periodic Invoices Service Task started.
Information| - Service task start processing billing invoice.
Information|Autorated Invoice for client Client warehouse WHS.
Information|Save succeed.
Information|Posted Invoice for client Client warehouse WHS.
Information|Save succeed.
Information|Close Invoice for client Client warehouse WHS.
Information|Save failed.
Information|Delivered Invoice for client Client warehouse WHS.
Information|Save succeed.
Information|Autorating Queued Periodic Invoices Service Task completed.";

			AssertMultilineASCIIEquals("Should log exception.", expectedMessage, serviceLog.ToString());
			AssertEquals("Precondition - ensure the Job was not closed.", JobHeaderStatus.Working.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Related Receive Job not closed.", JobHeaderStatus.Working.Code, receive.JobHeader.JH_Status);
			AssertEquals("Related Order Job not closed.", JobHeaderStatus.Working.Code, order.JobHeader.JH_Status);
		}

		#endregion

		#region TestCloseInvoiceAndRelatedJobsWhenInvoiceNotPosted

		public void TestCloseInvoiceAndRelatedJobs_NotCalledWhenInvoiceNotPosted()
		{
			var client = TestHelper.CreateClient("Client");
			var branch = TestHelper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = TestHelper.CreateWarehouse("WHS", address, branch);
			TestHelper.CreateRowAndGenerateLocations(whs, "A");
			var product = TestHelper.CreateProduct("Product", client);
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var today = ZDate.Today;

			var yesterday = today.AddDays(-1);
			var receive = SetupDataToBeAbleToChargeClientInInvoice(client, whs, product, 1, yesterday, today);

			var storageFromDate = today.AddDays(-7);
			var invoice = (WhsInvoice)TestHelper.CreateWhsInvoice(whs.PK, client.PK, storageFromDate, today);
			var finalisedDate = new ZDateTimeOffset(today.AddDays(-6));
			var order = CreateFinalisedWhsOrder(client, whs, finalisedDate, product);

			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;

			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
			Factory.Save();

			var dp = WhsAutoRateQueuedInvoicesProcessSynchronouslyTransactionedTestCase.MockInvoiceHelperWithSetups(invoice);
			var whsInvoiceHelper = new WhsInvoiceHelper();
			dp.Setup(d => d.PostInvoice(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>())).Callback((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
			{
				loggerIn.Log(LogType.Information, string.Format("Invoice for client {0} warehouse {1} could not be posted.", invoiceIn.ClientName, invoiceIn.WarehouseName));
			}).Returns(false);
			dp.Setup(d => d.CloseBillingAndRelatedJob(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>())).Callback((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
			{
				whsInvoiceHelper.CloseBillingAndRelatedJob(loggerIn, invoiceIn);
			});

			var serviceLog = new TestServiceLogger();
			using (ObjectFactory.Substitute(dp.Object))
			{
				serviceLog = RunServiceTask();
			}

			var expectedMessage = @"Information|Autorating Queued Periodic Invoices Service Task started.
Information|[Warehouse Periodic Invoice I00000001] - Service task start processing billing invoice.
Information|Autorated Invoice for client Client warehouse WHS.
Information|Invoice for client Client warehouse WHS could not be posted.
Information|Autorating Queued Periodic Invoices Service Task completed.";

			AssertMultilineASCIIEquals("Close should not called", expectedMessage, serviceLog.ToString());
		}

		#endregion

		#region TestCloseInvoiceAndRelatedJobsWhenInvoicePostedBefore

		public void TestCloseInvoiceAndRelatedJobs_NotCalledWhenInvoicePostedBefore()
		{
			var client = TestHelper.CreateClient("Client");
			var branch = TestHelper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = TestHelper.CreateWarehouse("WHS", address, branch);
			TestHelper.CreateRowAndGenerateLocations(whs, "A");
			var product = TestHelper.CreateProduct("Product", client);
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var today = ZDate.Today;

			var yesterday = today.AddDays(-1);
			var receive = SetupDataToBeAbleToChargeClientInInvoice(client, whs, product, 1, yesterday, today);

			var storageFromDate = today.AddDays(-7);
			var invoice = (WhsInvoice)TestHelper.CreateWhsInvoice(whs.PK, client.PK, storageFromDate, today);
			var finalisedDate = new ZDateTimeOffset(today.AddDays(-6));
			var order = CreateFinalisedWhsOrder(client, whs, finalisedDate, product);

			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;

			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
			Factory.Save();

			var dp = WhsAutoRateQueuedInvoicesProcessSynchronouslyTransactionedTestCase.MockInvoiceHelperWithSetups(invoice);
			var invoiceHelper = new WhsInvoiceHelper();
			dp.Setup(d => d.AutoRateJobHeader(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()))
				.Returns((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
				{
					return invoiceHelper.AutoRateJobHeader(loggerIn, invoiceIn);
				});
			dp.Setup(d => d.PostInvoice(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()))
			.Returns((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
			{
				return invoiceHelper.PostInvoice(loggerIn, invoiceIn);
			});
			dp.Setup(d => d.CloseBillingAndRelatedJob(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()))
				.Returns((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
				{
					return invoiceHelper.CloseBillingAndRelatedJob(loggerIn, invoiceIn);
				});
			dp.Setup(d => d.InvoiceSaveSafe(It.IsAny<WhsInvoice>(), It.IsAny<ILogger>(), It.IsAny<int>()))
				.Callback((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) =>
				{
					if (invoiceIn.BillingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationDelivered && attempt < 2)
					{
						invoiceIn.Factory.Saving += (f) => throw new ZCannotSaveException("Ops!", "Test");
					}
					else
					{
						invoiceHelper.InvoiceSaveSafe(invoiceIn, loggerIn, attempt);
					}
				})
				.Returns((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) =>
				{
					if (invoiceIn.BillingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationDelivered && attempt < 2)
					{
						loggerIn.Information("Save failed.");
						return InvoiceSaveSafeResult.SaveFailedRetry;
					}
					else
					{
						loggerIn.Information("Save succeed.");
						return InvoiceSaveSafeResult.SaveSuccessful;
					}
				});
			dp.Setup(d => d.LoadInNewFactory(It.IsAny<ZGuid>()))
				.Returns((ZGuid pk) =>
				{
					return invoiceHelper.LoadInNewFactory(pk);
				});
			var mutex = new Mock<IZGlobalMutex>();
			mutex.Setup(d => d.HasLock).Returns(false);
			mutex.Setup(d => d.IsLocked).Returns(false);
			mutex.Setup(d => d.Lock()).Returns(true);
			dp.Setup(d => d.GetInvoiceBillingAutomationMutex(It.IsAny<ZGuid>())).Returns(mutex.Object);
			var serviceLog = new TestServiceLogger();
			using (ObjectFactory.Substitute(dp.Object))
			{
				serviceLog = RunServiceTask();
				invoiceHelper.DisposeLoadedJobHeaders(invoice.Factory);
			}

			var expectedMessage = @"Information|Autorating Queued Periodic Invoices Service Task started.
Information| - Service task start processing billing invoice.
Information|Autorated Invoice for client Client warehouse WHS.
Information|Save succeed.
Information|Posted Invoice for client Client warehouse WHS.
Information|Save succeed.
Information|Periodic Billing job status for client Client warehouse WHS was successfully closed.
Information|Save succeed.
Information|Delivered Invoice for client Client warehouse WHS.
Information|Save failed.
Information| - Service task start processing billing invoice.
Information|Delivered Invoice for client Client warehouse WHS.
Information|Save succeed.
Information|Autorating Queued Periodic Invoices Service Task completed.";

			AssertMultilineASCIIEquals("autorate and post and close called once.", expectedMessage, serviceLog.ToString());
			dp.Verify(helper => helper.AutoRateJobHeader(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()), Times.Once);
			dp.Verify(helper => helper.PostInvoice(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()), Times.Once);
			// the condition is in the method, and won't be run second time
			dp.Verify(helper => helper.CloseBillingAndRelatedJob(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()), Times.Exactly(2));
		}

		#endregion

		#region TestCloseInvoiceAndRelatedJobsWhenInvoiceClosedBefore

		public void TestCloseInvoiceAndRelatedJobs_CloseSaveErrorRetry()
		{
			var client = TestHelper.CreateClient("Client");
			var branch = TestHelper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = TestHelper.CreateWarehouse("WHS", address, branch);
			TestHelper.CreateRowAndGenerateLocations(whs, "A");
			var product = TestHelper.CreateProduct("Product", client);
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var today = ZDate.Today;

			var yesterday = today.AddDays(-1);
			var receive = SetupDataToBeAbleToChargeClientInInvoice(client, whs, product, 1, yesterday, today);

			var storageFromDate = today.AddDays(-7);
			var invoice = (WhsInvoice)TestHelper.CreateWhsInvoice(whs.PK, client.PK, storageFromDate, today);
			var finalisedDate = new ZDateTimeOffset(today.AddDays(-6));
			var order = CreateFinalisedWhsOrder(client, whs, finalisedDate, product);

			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;

			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
			Factory.Save();

			var dp = WhsAutoRateQueuedInvoicesProcessSynchronouslyTransactionedTestCase.MockInvoiceHelperWithSetups(invoice);
			var invoiceHelper = new WhsInvoiceHelper();
			dp.Setup(d => d.AutoRateJobHeader(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()))
				.Returns((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
				{
					return invoiceHelper.AutoRateJobHeader(loggerIn, invoiceIn);
				});
			dp.Setup(d => d.PostInvoice(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()))
			.Returns((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
			{
				return invoiceHelper.PostInvoice(loggerIn, invoiceIn);
			});
			dp.Setup(d => d.CloseBillingAndRelatedJob(It.IsAny<IAutoRatingServiceLogger>(), It.IsAny<WhsInvoice>()))
				.Returns((IAutoRatingServiceLogger loggerIn, WhsInvoice invoiceIn) =>
				{
					return invoiceHelper.CloseBillingAndRelatedJob(loggerIn, invoiceIn);
				});
			dp.Setup(d => d.InvoiceSaveSafe(It.IsAny<WhsInvoice>(), It.IsAny<ILogger>(), It.IsAny<int>()))
				.Callback((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) =>
				{
					if ((invoiceIn.JobHeader.JH_Status == JobHeaderStatus.Closed.Code || invoiceIn.BillingAutomationStatus == BillingAutomationStatusCodes.BillingAutomationDelivered) && attempt < 2)
					{
						invoiceIn.Factory.Saving += (f) => throw new ZCannotSaveException("Ops!", "Test");
					}
				})
				.Returns((WhsInvoice invoiceIn, ILogger loggerIn, int attempt) =>
				{
					return invoiceHelper.InvoiceSaveSafe(invoiceIn, loggerIn, attempt);
				});
			dp.Setup(d => d.LoadInNewFactory(It.IsAny<ZGuid>()))
				.Returns((ZGuid pk) =>
				{
					return invoiceHelper.LoadInNewFactory(pk);
				});
			var mutex = new Mock<IZGlobalMutex>();
			mutex.Setup(d => d.HasLock).Returns(false);
			mutex.Setup(d => d.IsLocked).Returns(false);
			mutex.Setup(d => d.Lock()).Returns(true);
			dp.Setup(d => d.GetInvoiceBillingAutomationMutex(It.IsAny<ZGuid>())).Returns(mutex.Object);
			var serviceLog = new TestServiceLogger();
			using (ObjectFactory.Substitute(dp.Object))
			{
				serviceLog = RunServiceTask();
				invoiceHelper.DisposeLoadedJobHeaders(invoice.Factory);
			}

			var expectedMessage = @"Information|Autorating Queued Periodic Invoices Service Task started.
Information| - Service task start processing billing invoice.
Information|Autorated Invoice for client Client warehouse WHS.
Information|Posted Invoice for client Client warehouse WHS.
Information|Periodic Billing job status for client Client warehouse WHS was successfully closed.
Error|Ops!
(First attempt).
Information|Delivered Invoice for client Client warehouse WHS.
Error|Ops!
(First attempt).
Information| - Service task start processing billing invoice.
Information|Periodic Billing job status for client Client warehouse WHS was successfully closed.
Information|Delivered Invoice for client Client warehouse WHS.
Information|Autorating Queued Periodic Invoices Service Task completed.";

			AssertMultilineASCIIEquals("close and deliver failed first time and succeed second time.", expectedMessage, serviceLog.ToString());
		}

		#endregion

		public void TestCanRunInAnyBranch()
		{
			var attribute = typeof(WhsQueuedInvoiceAutoRatingServiceTask)
				.Assembly
				.GetCustomAttributes(true)
				.OfType<HostedServiceAttribute>()
				.Single(x => x.Code == "WPI");
			AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
		}

		WhsOrder CreateFinalisedWhsOrder(OrgHeader org, WhsWarehouse whs, ZDateTimeOffset finalisedDate, OrgSupplierPart product)
		{
			var order = TestHelper.CreateWhsOrder(org, whs, "REF0");
			order.WD_RequiredDate = finalisedDate;
			order.ConsigneePK = org.PK;
			order.ConsigneeAddressPK = org.MainAddress.PK;

			var line = TestHelper.CreateWhsOrderLine(order, product, 5);
			line.WE_F3_NKPackType = PkgUnit.Unit;
			var pick = TestHelper.CreatePickNew(order);
			pick.FinaliseOrder(order);
			pick.FinalisePick();
			order.WD_FinalisedDate = finalisedDate;

			pick.GetAllPickLines().FirstOrDefault().WZ_PickedDateTime = finalisedDate;

			Factory.Save();
			return order;
		}

		WhsReceive SetupDataToBeAbleToChargeClientInInvoice(OrgHeader client, WhsWarehouse whs, OrgSupplierPart part, int i, ZDate chargeDate, ZDate today)
		{
			var whsChargeCode = TestHelper.CreateChargeCode("WHSCHG", "Warehouse Charge", ChargeCodeGroupList.Codes.WHSOutwards, string.Empty);
			var receive = TestHelper.CreateWhsReceiveWithInventory(client, whs, $"R{i}", chargeDate.ToZDateTime().ToOffset(), part, 10m);
			Factory.Save();

			// Setting to generate charge when autorate invoice
			var receiveHandlingFumigationCharge = TestHelper.CreateChargeCode($"WRECFUM{i}", "Warehouse Receive Handling Fumigation", ChargeCodeGroupList.Codes.WHSInwards, Constants.FreightServiceType.Codes.Fumigation);
			var clientRate = TestHelper.CreateClientRate(client);
			var warehouseRate = TestHelper.CreateRateEntry(clientRate, chargeDate.AddMonths(-1), today);
			TestHelper.CreateRateLine(warehouseRate, receiveHandlingFumigationCharge, "SV", 5m);
			var rateLine = warehouseRate.AddRateLine(whsChargeCode, UnitCalculator.Code, PkgUnit.Spool);
			rateLine.Calculator.Decimal1 = 10m;
			// any charges
			var fumigationService = receive.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 30m;
			fumigationService.ES_Completed = chargeDate;

			Factory.Save();

			return receive;
		}

		protected WhsTestHelperFunctions TestHelper => testHelper ?? (testHelper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions testHelper;

		TestServiceLogger RunServiceTask()
		{
			var logger = new TestServiceLogger();
			var serviceTask = new WhsQueuedInvoiceAutoRatingServiceTask() { ServiceLogger = logger };
			InitialiseTaskSchedule(serviceTask);
			using (EnvProxy.Instance.TemporaryServiceTaskContext("WPI", canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			return logger;
		}
	}
}
