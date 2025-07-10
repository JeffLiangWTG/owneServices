using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	[TestedType(typeof(WhsInvoicePeriodicMultiClientInvoice))]
	public class WhsInvoicePeriodicMultiClientInvoiceTest : Transactions.Business.Testing.WhsNonPersistentBusinessObjectTestCase
	{
		#region TestInvoices

		#region TestInvoices

		[TestDate(2012, 1, 1)]
		public void TestInvoices()
		{
			var client1 = Helper.CreateClient("CLIENT1");
			var client2 = Helper.CreateClient("CLIENT2");
			client1.OH_IsDebtor = true;
			client2.OH_IsDebtor = true;

			var whs1 = Helper.CreateWarehouse("W1", "A");
			var whs2 = Helper.CreateWarehouse("W2", "B");
			whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs2.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var part = Helper.CreateProduct(client1, "P1");
			Helper.CreateProductClientRelationShip(client2, part);

			CreateAndFinaliseWhsReceiveWithInventory(client1, whs1, "R1", new ZDateTimeOffset(2010, 12, 31), Notify, part, 10m);
			CreateAndFinaliseWhsReceiveWithInventory(client1, whs2, "R2", new ZDateTimeOffset(2010, 12, 31), Notify, part, 10m);
			CreateAndFinaliseWhsReceiveWithInventory(client2, whs1, "R3", new ZDateTimeOffset(2010, 12, 31), Notify, part, 10m);
			CreateAndFinaliseWhsReceiveWithInventory(client2, whs2, "R4", new ZDateTimeOffset(2010, 12, 31), Notify, part, 10m);

			const bool IsPosted = true;
			var invoiceDate = new ZDateTime(2011, 1, 18);
			// Client1 - Whs1 - No previous periodics.

			// ...

			// Client1 - Whs2 - last periodic is posted.
			Helper.CreateWhsInvoice(client1, whs2, new ZDateTime(2011, 1, 1), IsPosted);

			// Client2 - Whs1 - two unposted periodics.
			Helper.CreateWhsInvoice(client2, whs1, new ZDateTime(2011, 1, 1), !IsPosted);
			Helper.CreateWhsInvoice(client2, whs1, new ZDateTime(2011, 1, 8), !IsPosted);

			// Client2 - Whs2 - first is unposted periodic second is posted periodic.
			Helper.CreateWhsInvoice(client2, whs2, new ZDateTime(2011, 1, 1), !IsPosted);
			Helper.CreateWhsInvoice(client2, whs2, new ZDateTime(2011, 1, 8), IsPosted);

			Factory.Save();

			var hitCounts = new Dictionary<string, int>();
			hitCounts.Add(OrgCompanyDataSchema.Constants.TableName, 1);
			hitCounts.Add(OrgHeaderSchema.Constants.TableName, 1);
			hitCounts.Add(JobStorageSchema.Constants.TableName, 14); // For each orgWarehousePairs = GetOldestUnpostedInvoiceInOtherFactory + If_invoice_found(GetOrCreateInvoiceForClientAndWarehouse) + WhsInvoice.GetDefaultStorageFromDate + CheckForOverlappingPeriods()
			hitCounts.Add(WhsWarehouseSchema.Constants.TableName, 1);
			using (AssertDbHitsForAllFactories(hitCounts, useOnlyNewFactories: true, ignoreUnspecified: true))
			{
				var multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
				multiClientInvoice.LoadOrCreateInvoices();
				multiClientInvoice.InvoiceDate = invoiceDate;

				AssertEquals("Should find/create 1 invoice per Client/Warehouse pair.", 4, multiClientInvoice.Invoices.Count);
				AssertInvoiceExists(multiClientInvoice.Invoices, client1, whs1, new ZDateTime(2010, 12, 31), invoiceDate);
				AssertInvoiceExists(multiClientInvoice.Invoices, client1, whs2, new ZDateTime(2011, 1, 8), invoiceDate);
				AssertInvoiceExists(multiClientInvoice.Invoices, client2, whs1, new ZDateTime(2011, 1, 1), invoiceDate);
				AssertInvoiceExists(multiClientInvoice.Invoices, client2, whs2, new ZDateTime(2011, 1, 1), invoiceDate);

				multiClientInvoice.Delete();
				client1.OH_IsActive = false;
				Factory.Save();

				multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
				multiClientInvoice.LoadOrCreateInvoices();
				multiClientInvoice.InvoiceDate = invoiceDate;

				AssertEquals("Should find/create 1 invoice per Client active /Warehouse pair.", 2, multiClientInvoice.Invoices.Count);
				AssertInvoiceExists(multiClientInvoice.Invoices, client2, whs1, new ZDateTime(2011, 1, 1), invoiceDate);
				AssertInvoiceExists(multiClientInvoice.Invoices, client2, whs2, new ZDateTime(2011, 1, 1), invoiceDate);
			}
		}

		#endregion

		#region TestInvoices_PostedOnChildJob

		[TestDate(2011, 6, 1)]
		public void TestInvoices_PostedOnChildJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var receive = CreateAndFinaliseWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2011, 1, 2), Notify, data.Part1, 10m);
			var jobHeader = Helper.CreateJobHeaderAndCharge(receive);
			PostJob(jobHeader);
			AssertEquals("Charge should be posted.", true, jobHeader.Charges[0].IsRevenuePosted);

			Helper.CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(2011, 1, 1), false);

			Factory.Save();

			var invoiceDate = new ZDateTime(2011, 1, 8);
			var multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice.LoadOrCreateInvoices();
			multiClientInvoice.InvoiceDate = invoiceDate;

			AssertEquals("Should find created invoice.", 1, multiClientInvoice.Invoices.Count);
			AssertInvoiceExists(multiClientInvoice.Invoices, data.Org1, data.Whs1, new ZDateTime(2011, 1, 1), invoiceDate);
		}

		void PostJob(Job jobHeader)
		{
			var postManager = new InvoicingPostManager(jobHeader);
			Factory.Save();
			postManager.CreateTransactions(JobInvoicingPostingOption.All);
		}

		#endregion

		#region TestInvoices_WithAL_JHIsNull

		[TestDate(2011, 6, 1)]
		public void TestInvoices_WithAL_JHIsNull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = data.Org1.PK;

			var receiveHandlingCharge = Helper.CreateChargeCode("INWHAN", "Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
			var warehouseEntry = Helper.CreateRateEntry(clientRate, new ZDate(2010, 1, 1), new ZDate(2011, 12, 31));
			Helper.CreateRateLine(warehouseEntry, receiveHandlingCharge, "UNT", 5m);

			CreateAndFinaliseWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2011, 1, 2), Notify, data.Part1, 10m);
			Helper.CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(2011, 1, 1), false);

			// if present posted AccTransactionLine with null AL_JH then old ZDBOnlyQuery was not returning correct result.
			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;
			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_JH = ZGuid.Empty;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue; // line is posted.
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			var invoiceDate = new ZDateTime(2011, 1, 8);
			var multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice.LoadOrCreateInvoices();
			multiClientInvoice.InvoiceDate = invoiceDate;

			AssertEquals("Should find created invoice.", 1, multiClientInvoice.Invoices.Count);
			AssertInvoiceExists(multiClientInvoice.Invoices, data.Org1, data.Whs1, new ZDateTime(2011, 1, 1), invoiceDate);
		}

		#endregion

		#region CreateAndFinaliseWhsReceiveWithInventory

		WhsReceive CreateAndFinaliseWhsReceiveWithInventory(OrgHeader client, WhsWarehouse whs, ZString reference, ZDateTimeOffset arrivalDate, NotificationBuffer notify, OrgSupplierPart part, ZDecimal units)
		{
			var receive = Helper.CreateWhsReceive(client, whs, reference, notify);
			receive.WD_ArrivalDate = arrivalDate;
			Helper.CreateWhsReceiveInventoryLine(receive, part, units);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);

			return receive;
		}

		#endregion

		#region AssertInvoiceExists

		void AssertInvoiceExists(WhsInvoiceCollection invoices, OrgHeader expectedClient, WhsWarehouse expectedWarehouse, ZDateTime expectedStorageFromDate, ZDateTime expectedInvoiceDate)
		{
			foreach (WhsInvoice invoice in invoices)
			{
				if (invoice.ET_OH_Client == expectedClient.PK && invoice.ET_WW == expectedWarehouse.PK && invoice.ET_StorageFromDate == expectedStorageFromDate)
				{
					AssertEquals("Invoice Date is incorrect.", expectedInvoiceDate, invoice.ET_BillingDate);
					return;
				}
			}

			Fail(string.Format("Periodic Invoice with set parameters is not found:\r\nClient: {0}\r\nWarehouse: {1}\r\nStorage From:{2}",
				expectedClient.OH_Code,
				expectedWarehouse.WW_WarehouseCode,
				expectedStorageFromDate));
		}

		#endregion

		#region TestInvoices_LoadOrCreateInvoices

		public void TestInvoices_LoadOrCreateInvoices()
		{
			var client = Helper.CreateClient("CLIENT1");
			client.OH_IsDebtor = true;

			var whs = Helper.CreateWarehouse("W1", "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var part = Helper.CreateProduct(client, "P1");
			Helper.CreateProductClientRelationShip(client, part);

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", part, 10m);
			Factory.Save();

			var invoiceMultiClients = new WhsInvoicePeriodicMultiClientInvoice();
			AssertEquals("Invoices should not be loaded.", 0, invoiceMultiClients.Invoices.Count);
			invoiceMultiClients.LoadOrCreateInvoices();
			AssertEquals("Invoices should be loaded.", 1, invoiceMultiClients.Invoices.Count);
		}

		#endregion

		#endregion

		#region TestWhsInvoicePeriodicMultiClientInvoiceAutoRatings

		public void TestWhsInvoicePeriodicMultiClientInvoiceAutoRating_NoInvoice()
		{
			TestWhsInvoicePeriodicMultiClientInvoiceAutoRating_Core(selectBranchFromWarehouse: true, expectToEventReturnHasInvoice: false);
		}

		public void TestWhsInvoicePeriodicMultiClientInvoiceAutoRating_HasInvoice()
		{
			TestWhsInvoicePeriodicMultiClientInvoiceAutoRating_Core(selectBranchFromWarehouse: false, expectToEventReturnHasInvoice: true);
		}

		void TestWhsInvoicePeriodicMultiClientInvoiceAutoRating_Core(bool selectBranchFromWarehouse, bool expectToEventReturnHasInvoice)
		{
			var client1 = Helper.CreateClient("CLIENT1");
			client1.OH_IsDebtor = true;
			var companyData = client1.CompanyData;
			companyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			var whs1 = Helper.CreateWarehouse("W1", "A");
			whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var part = Helper.CreateProduct(client1, "P1");
			var receive = CreateAndFinaliseWhsReceiveWithInventory(client1, whs1, "R1", ZDateTimeOffset.Now.AddDays(-7), Notify, part, 10m);
			var jobHeader = Helper.CreateJobHeaderAndCharge(receive);
			Factory.Save();

			var whsInvoicePeriodicMultiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			whsInvoicePeriodicMultiClientInvoice.LoadOrCreateInvoices(true);
			AssertEquals("Precondition", 1, whsInvoicePeriodicMultiClientInvoice.Invoices.Count);

			whsInvoicePeriodicMultiClientInvoice.Invoices[0].ET_StorageJobNumber = "1";
			whsInvoicePeriodicMultiClientInvoice.Invoices[0].IncludeInInvoicing = true;

			using (whsInvoicePeriodicMultiClientInvoice.Invoices[0].GetValidationSuspender())
			{
				var invoice = whsInvoicePeriodicMultiClientInvoice.Invoices[0];
				var eventRaised = false;
				whsInvoicePeriodicMultiClientInvoice.AutoRating += (object sender, AutoRatingEventArgs e) =>
				{
					AssertEquals(expectToEventReturnHasInvoice, e.HasExistingInvoice);
					AssertEquals(invoice, e.Invoice);
					eventRaised = true;
				};
				whsInvoicePeriodicMultiClientInvoice.AutoRateInvoices(CancellationToken.None, selectBranchFromWarehouse);
				AssertEquals("Should call event.", true, eventRaised);
			}
		}

		#endregion

		#region TestWhsInvoicePeriodicMultiClientInvoiceAutoRatings_IsAutoRateLockValidationSuspended

		public void TestWhsInvoicePeriodicMultiClientInvoiceAutoRatings_IsAutoRateLockValidationSuspended()
		{
			var client1 = Helper.CreateClient("CLIENT1");
			client1.OH_IsDebtor = true;
			var companyData = client1.CompanyData;
			companyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			var whs1 = Helper.CreateWarehouse("W1", "A");
			whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var part = Helper.CreateProduct(client1, "P1");
			var receive = CreateAndFinaliseWhsReceiveWithInventory(client1, whs1, "R1", ZDateTimeOffset.Now.AddDays(-7), Notify, part, 10m);
			var jobHeader = Helper.CreateJobHeaderAndCharge(receive);
			Factory.Save();

			var mutex = new Mock<IZGlobalMutex>();
			var invoiceHelper = new Mock<IWhsInvoiceHelper>();
			mutex.Setup(d => d.HasLock).Returns(() => false);
			mutex.Setup(d => d.Lock()).Returns(() => true);
			invoiceHelper.Setup(d => d.GetCandidateInvoices(It.IsAny<BusinessObjectFactory>(), true)).Returns(new[] { new OrgWarehousePair { OrgPK = client1.PK, WarehousePK = whs1.PK, HasInvoice = true } });
			invoiceHelper.Setup(d => d.GetInvoiceBillingAutomationMutex(It.IsAny<ZGuid>())).Returns(mutex.Object);
			invoiceHelper.Setup(d => d.IsNotLockByOtherProcess(It.IsAny<ZGuid>())).Returns(false);

			using (ObjectFactory.Substitute(invoiceHelper.Object))
			{
				var whsInvoicePeriodicMultiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
				whsInvoicePeriodicMultiClientInvoice.LoadOrCreateInvoices(true);
				AssertEquals("Precondition", 1, whsInvoicePeriodicMultiClientInvoice.Invoices.Count);
				var invoice = whsInvoicePeriodicMultiClientInvoice.Invoices[0];

				invoice.ET_StorageJobNumber = "1";
				invoice.IncludeInInvoicing = true;

				using (invoice.GetValidationSuspender())
				{
					var eventRaised = false;
					whsInvoicePeriodicMultiClientInvoice.AutoRating += (s, e) =>
					{
						eventRaised = true;
					};

					whsInvoicePeriodicMultiClientInvoice.AutoRateInvoices(CancellationToken.None, false);

					AssertNoErrors("Precondition: when there is no error it will call autorate.", invoice);
					AssertEquals("Should call event.", true, eventRaised);
					invoiceHelper.Verify(f => f.IsNotLockByOtherProcess(invoice.PK), Times.Never, "Locks should not be checked when it is locked by its process.");
					invoiceHelper.Verify(f => f.GetInvoiceBillingAutomationMutex(invoice.PK), Times.Once);
				}
			}
		}

		#endregion

		#region TestWhsInvoicePeriodicMultiClientInvoiceAutoRatings_MutexLock

		public void TestWhsInvoicePeriodicMultiClientInvoiceAutoRatings_MutexLock_AlreadyHasLock()
		{
			var hasLock = true;
			var canLock = true;
			var expectedRowError = "A service task or another user is autorating this invoice.";
			TestWhsInvoicePeriodicMultiClientInvoiceAutoRatings_MutexLockCore(hasLock, canLock, expectedRowError);
		}

		public void TestWhsInvoicePeriodicMultiClientInvoiceAutoRatings_MutexLock_ConnotLock()
		{
			var hasLock = false;
			var canLock = false;
			var expectedRowError = "Unable to acquire Lock to prevent other users Autorating this invoice at the same time.";
			TestWhsInvoicePeriodicMultiClientInvoiceAutoRatings_MutexLockCore(hasLock, canLock, expectedRowError);
		}

		void TestWhsInvoicePeriodicMultiClientInvoiceAutoRatings_MutexLockCore(bool hasLock, bool canLock, string expectedRowError)
		{
			var client1 = Helper.CreateClient("CLIENT1");
			client1.OH_IsDebtor = true;
			var companyData = client1.CompanyData;
			companyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			var whs1 = Helper.CreateWarehouse("W1", "A");
			whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var part = Helper.CreateProduct(client1, "P1");
			var receive = CreateAndFinaliseWhsReceiveWithInventory(client1, whs1, "R1", ZDateTimeOffset.Now.AddDays(-7), Notify, part, 10m);
			var jobHeader = Helper.CreateJobHeaderAndCharge(receive);
			Factory.Save();

			var mutex = new Mock<IZGlobalMutex>();
			var invoiceHelper = new Mock<IWhsInvoiceHelper>();
			mutex.Setup(d => d.HasLock).Returns(() => hasLock);
			mutex.Setup(d => d.Lock()).Returns(() => canLock);
			invoiceHelper.Setup(d => d.GetCandidateInvoices(It.IsAny<BusinessObjectFactory>(), true)).Returns(new[] { new OrgWarehousePair { OrgPK = client1.PK, WarehousePK = whs1.PK, HasInvoice = true } });
			invoiceHelper.Setup(d => d.GetInvoiceBillingAutomationMutex(It.IsAny<ZGuid>())).Returns(mutex.Object);

			using (ObjectFactory.Substitute(invoiceHelper.Object))
			{
				var whsInvoicePeriodicMultiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
				whsInvoicePeriodicMultiClientInvoice.LoadOrCreateInvoices(true);
				AssertEquals("Precondition", 1, whsInvoicePeriodicMultiClientInvoice.Invoices.Count);

				var eventRaised = false;
				whsInvoicePeriodicMultiClientInvoice.AutoRating += (s, e) =>
				{
					eventRaised = true;
				};

				whsInvoicePeriodicMultiClientInvoice.AutoRateInvoices(CancellationToken.None, false);
				var invoice = whsInvoicePeriodicMultiClientInvoice.Invoices[0];
				AssertHasRowError(invoice, expectedRowError);
				AssertEquals("Should call event.", false, eventRaised);
				invoiceHelper.Verify(f => f.IsNotLockByOtherProcess(invoice.PK), Times.Never, "Locks should not be checked when it is locked by its process.");
				invoiceHelper.Verify(f => f.GetInvoiceBillingAutomationMutex(invoice.PK), Times.Once);
			}
		}

		#endregion

		#region TestCreatesInvoiceIfZeroStock_WithUninvoicedStockMovements

		[TestDate(2021, 09, 15)]
		public void TestCreatesInvoiceIfZeroStock_WithUninvoicedStockMovements()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var today = ZDateTimeOffset.Today;

			CreateWhsReceive(data.Whs1, data.Org1, today.AddDays(-10), true, "R1", data.Part1, 10);
			CreateFinalisedWhsOrder(data.Whs1, data.Org1, today.AddDays(-10), "O1", data.Part1, 10);
			Factory.Save();

			var multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice.LoadOrCreateInvoices();
			AssertEquals("Should have 1 invoice", 1, multiClientInvoice.Invoices.Count);
		}

		#endregion

		#region TestCreatesInvoiceIfNegativeStock_WithUninvoicedStockMovements

		public void TestCreatesInvoiceIfNegativeStock_WithUninvoicedStockMovements()
		{
			// This can happen if the user sets up "Use Arrival Date for Finalised Date" - WI00093501
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var today = ZDateTimeOffset.Today;
			var receive1 = CreateWhsReceive(data.Whs1, data.Org1, today, true, "R1", data.Part1, 10);
			Factory.Save();
			CreateFinalisedWhsOrder(data.Whs1, data.Org1, today.AddDays(-10), "O1", data.Part1, 10);
			// Prior to WI00128803 it was valid to have arrival date in the future, we cannot transform old data
			receive1.WD_BookingDate = today.AddDays(9);
			receive1.WD_ArrivalDate = today.AddDays(10);
			receive1.WD_FinalisedDate = today.AddDays(10);
			Factory.Save();

			var multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice.LoadOrCreateInvoices();
			AssertEquals("Should have 1 invoice", 1, multiClientInvoice.Invoices.Count);
		}

		#endregion

		#region TestCreatesInvoiceIfZeroStock_WithUninvoicedOrderAndInvoicedReceive

		public void TestCreatesInvoiceIfZeroStock_WithUninvoicedOrderAndInvoicedReceive()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var today = ZDateTimeOffset.Today;

			var receive1 = CreateWhsReceive(data.Whs1, data.Org1, today.AddDays(-10), true, "R1", data.Part1, 10);
			var order1 = CreateFinalisedWhsOrder(data.Whs1, data.Org1, today.AddDays(-3), "O1", data.Part1, 10);
			Factory.Save();

			var receiveJobHeader = AddJobHeader(receive1);
			receiveJobHeader.JH_Status = JobHeaderStatus.Working.Code;

			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, today.AddDays(-12).Date, today.AddDays(-8).Date);
			invoice.NotificationManager.Push(Notify);
			AssertContainsExactElementsInAnyOrder(new[] { receive1 }, invoice.GetAdditionalDockets().Where(d => d.WD_DocketType == DocketType.Codes.Receive));

			invoice.AutoRateJobHeader(null);
			invoice.PostInvoice();
			invoice.JobHeader.Close(null, null);
			Factory.Save();

			var multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice.LoadOrCreateInvoices();
			AssertEquals("Should have 1 invoice", 1, multiClientInvoice.Invoices.Count);
		}

		#endregion

		#region TestDoesntCreateInvoice_IfAllStockMovementsInvoiced

		public void TestDoesntCreateInvoice_IfAllStockMovementsInvoiced()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var today = ZDateTimeOffset.Today;

			var receive1 = CreateWhsReceive(data.Whs1, data.Org1, today.AddDays(-10), true, "R1", data.Part1, 10);
			Factory.Save();
			var order1 = CreateFinalisedWhsOrder(data.Whs1, data.Org1, today.AddDays(-10), "O1", data.Part1, 10);

			var receiveJobHeader = AddJobHeader(receive1);
			receiveJobHeader.JH_Status = JobHeaderStatus.Working.Code;

			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, today.AddDays(-12).Date, today.AddDays(-8).Date);
			invoice.NotificationManager.Push(Notify);
			AssertContainsExactElementsInAnyOrder(new WhsDocket[] { receive1, order1 }, invoice.GetAdditionalDockets());

			invoice.AutoRateJobHeader(null);
			invoice.PostInvoice();
			invoice.JobHeader.Close(null, null);
			Factory.Save();

			var multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice.LoadOrCreateInvoices();
			AssertEquals("Should have 0 invoice", 0, multiClientInvoice.Invoices.Count);

			CreateWhsReceive(data.Whs1, data.Org1, today.AddDays(-3), true, "R2", data.Part1, 10);
			Factory.Save();

			multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice.LoadOrCreateInvoices();
			AssertEquals("Should have 1 invoice", 1, multiClientInvoice.Invoices.Count);
		}

		#endregion

		#region TestDoesntCreateInvoice_IfZeroUnitsAndUninvoicedStockMovementsPriorToFirstInvoice

		public void TestDoesntCreateInvoice_IfZeroUnitsAndUninvoicedStockMovementsPriorToFirstInvoice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var today = ZDateTimeOffset.Today;

			var receive1 = CreateWhsReceive(data.Whs1, data.Org1, today.AddDays(-20), true, "R1", data.Part1, 10);
			Factory.Save();
			CreateFinalisedWhsOrder(data.Whs1, data.Org1, today.AddDays(-20), "O1", data.Part1, 10);

			var receiveJobHeader = AddJobHeader(receive1);
			receiveJobHeader.JH_Status = JobHeaderStatus.Working.Code;

			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, today.AddDays(-12).Date, today.AddDays(-8).Date);
			invoice.NotificationManager.Push(Notify);
			AssertEquals("Precondition", 0, invoice.GetAdditionalDockets().Length);

			invoice.AutoRateJobHeader(null);
			invoice.PostInvoice();
			invoice.JobHeader.Close(null, null);
			Factory.Save();

			var multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			AssertEquals("Should have 0 invoice", 0, multiClientInvoice.Invoices.Count);

			CreateWhsReceive(data.Whs1, data.Org1, today.AddDays(-3), true, "R2", data.Part1, 10);
			Factory.Save();

			multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice.LoadOrCreateInvoices();
			AssertEquals("Should have 1 invoice", 1, multiClientInvoice.Invoices.Count);
		}

		#endregion

		#region TestCreatesInvoice_WhsAutoCreateAndRatePeriodicInvoice

		[TestDate(2021, 09, 15)]
		public void TestCreatesInvoice_WhsAutoCreateAndRatePeriodicInvoice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var companyData = data.Org1.CompanyData;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var today = ZDateTimeOffset.Today;

			CreateWhsReceive(data.Whs1, data.Org1, today.AddDays(-10), true, "R1", data.Part1, 10);
			CreateFinalisedWhsOrder(data.Whs1, data.Org1, today.AddDays(-10), "O1", data.Part1, 10);
			Factory.Save();

			var multiClientInvoice1 = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice1.LoadOrCreateInvoices(selectBranchFromWarehouse: true);
			AssertEquals("Precondition", false, companyData.OB_WhsAutoCreateAndRatePeriodicInvoice);
			AssertEquals("Should create invoice if AutoCreateAndRatePeriodicInvoice is true.", 0, multiClientInvoice1.Invoices.Count);

			companyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			Factory.Save();
			var multiClientInvoice2 = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice2.LoadOrCreateInvoices(selectBranchFromWarehouse: true);
			AssertEquals("Precondition", true, companyData.OB_WhsAutoCreateAndRatePeriodicInvoice);
			AssertEquals("Should create invoice if AutoCreateAndRatePeriodicInvoice is true.", 1, multiClientInvoice2.Invoices.Count);
		}

		#endregion

		#region AddJobHeader

		JobHeader AddJobHeader(WhsDocket docket)
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_JobNum = docket.WD_DocketID;
			jobHeader.JH_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			jobHeader.JH_ParentID = docket.PK;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;

			return jobHeader;
		}

		#endregion

		#region CreateWhsReceive

		WhsReceive CreateWhsReceive(WhsWarehouse whs, OrgHeader org, ZDateTimeOffset finalisedDate, bool finalise, string reference, OrgSupplierPart part, ZDecimal units)
		{
			var docket = Helper.CreateWhsReceive(org, whs, reference, new TestNotificationBuffer());
			docket.WD_BookingDate = finalisedDate.AddDays(-1);
			docket.WD_ArrivalDate = finalisedDate;
			Helper.CreateWhsReceiveInventoryLine(docket, part, units);

			docket.AllocateLocationsWithMock();
			if (finalise)
			{
				docket.FinaliseDocket();
				docket.WD_FinalisedDate = finalisedDate;
			}

			return docket;
		}

		#endregion

		#region CreateFinalisedWhsOrder

		WhsOrder CreateFinalisedWhsOrder(WhsWarehouse whs, OrgHeader org, ZDateTimeOffset finalisedDate, string reference, OrgSupplierPart part, ZDecimal units)
		{
			var order = Helper.CreateWhsOrder(org, whs, reference);
			order.WD_RequiredDate = finalisedDate;
			order.ConsigneePK = org.PK;
			order.ConsigneeAddressPK = org.MainAddress.PK;
			Helper.CreateWhsOrderLine(order, part, units);

			var pick = Helper.CreatePickNew(order);

			pick.FinaliseOrder(order);
			pick.FinalisePick();
			order.WD_FinalisedDate = finalisedDate;

			return order;
		}

		#endregion

		#region TestRunPreSaveValidationExcludeUnselectedInvoice

		public void TestRunPreSaveValidationExcludeUnselectedInvoice()
		{
			var warehouse = Helper.CreateWarehouse("Whs1", "A");
			var client1 = Helper.CreateClient("Org1");
			client1.CompanyData.OB_IsDebtor = true;
			var client2 = Helper.CreateClient("Org2");
			client2.CompanyData.OB_IsDebtor = true;

			Factory.Save();

			var part1 = Helper.CreateProduct(client1, "P1");
			var part2 = Helper.CreateProduct(client2, "P2");

			var existingInvoice_Org1 = Helper.CreateWhsInvoice(client1, warehouse, new ZDateTime(2001, 1, 1), false);
			var existingInvoice_Org2 = Helper.CreateWhsInvoice(client2, warehouse, new ZDateTime(2011, 1, 1), false);
			var docket1 = helper.CreateWhsReceiveWithInventory(client1, warehouse, "R1", part1, 1000);
			var docket2 = helper.CreateWhsReceiveWithInventory(client2, warehouse, "R2", part2, 1000);

			Factory.Save();

			var multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice.LoadOrCreateInvoices();
			AssertEquals(2, multiClientInvoice.Invoices.Count);

			var invoice1 = multiClientInvoice.Invoices.Cast<WhsInvoice>().First(i => i.ET_OH_Client == client1.PK);
			invoice1.ET_StorageJobNumber = "1";
			invoice1.ET_StorageToDate = invoice1.ET_StorageFromDate.AddDays(7);
			invoice1.IncludeInInvoicing = false;
			AssertEquals("This invoice data is invalid, but setting IncludeInInvoicing false rolls back changes made, so no errors.", false, invoice1.HasErrors);
			AssertEquals("This invoice is not included.", false, invoice1.IncludeInInvoicing);

			var invoice2 = multiClientInvoice.Invoices.Cast<WhsInvoice>().First(i => i.ET_OH_Client == client2.PK);
			invoice2.ET_StorageJobNumber = "2";
			invoice2.IncludeInInvoicing = true;

			multiClientInvoice.RunPreSaveValidation();
			Assert("Should not have validation error since invalid invoice is not included.", !multiClientInvoice.HasErrors);
		}

		#endregion

		#region TestAutoRateAndPostInvoices

		[GuiTest]
		public void TestAutoRateAndPostInvoices()
		{
			SetupWarehouseWithDockets();

			var multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice.LoadOrCreateInvoices();
			AssertEquals(4, multiClientInvoice.Invoices.Count);

			multiClientInvoice.InvoiceCreated += new EventHandler<WhsInvoicePeriodicMultiClientInvoice.InvoiceCreatedEventArgs>(AssertOnInvoiceCreated);
			multiClientInvoice.InvoiceCreationComplete += delegate
			{ OnInvoiceCreationCompleteCalled++; };

			TotalInvoicesToCreate = 1;
			OnInvoiceCreatedCalled = 0;
			OnInvoiceCreationCompleteCalled = 0;
			WhsInvoice invoiceNotIncluded = null;
			WhsInvoice invoiceShouldBeDeleted = null;

			foreach (WhsInvoice invoice in multiClientInvoice.Invoices)
			{
				invoice.SuspendValidation();
				if (invoice.ET_OH_Client == Org.PK)
				{
					invoice.ET_StorageJobNumber = "1";
					invoice.IncludeInInvoicing = true;
				}
				else if (invoice.ET_OH_Client == Org2.PK)
				{
					invoice.ET_StorageJobNumber = "2";
					invoice.IncludeInInvoicing = true;
				}
				else if (invoice.ET_OH_Client == Org3.PK)
				{
					invoice.ET_StorageJobNumber = "3";
					invoice.IncludeInInvoicing = false;
					invoiceNotIncluded = invoice;
				}
				else if (invoice.ET_OH_Client == Org4.PK)
				{
					invoice.ET_StorageJobNumber = "4";
					invoice.IncludeInInvoicing = false;
					invoiceShouldBeDeleted = invoice;
				}
			}

			multiClientInvoice.Invoices.Sort(JobStorageSchema.Constants.ET_StorageJobNumber, ListSortDirection.Descending);
			multiClientInvoice.AutoRateAndPostInvoices(CancellationToken.None);

			AssertEquals(1, OnInvoiceCreatedCalled);
			AssertEquals(1, OnInvoiceCreationCompleteCalled);

			AssertEquals("Not included existing invoice should NOT be deleted", false, invoiceNotIncluded.IsDeleted);
			AssertEquals("There should only be 1 invoice in total", 1, multiClientInvoice.Invoices.Count);
			AssertEquals("Posted job is posted", true, multiClientInvoice.Invoices[0].JobHeader.Charges[0].IsRevenuePosted);
			AssertEquals("Print Task Run", 1, multiClientInvoice.LastPrintTaskCountForTest);
			AssertEquals("Not included non-existing invoice should be deleted", true, invoiceShouldBeDeleted.IsDeleted);
		}

		void AssertOnInvoiceCreated(object sender, WhsInvoicePeriodicMultiClientInvoice.InvoiceCreatedEventArgs e)
		{
			OnInvoiceCreatedCalled++;
			AssertEquals("CountCompleted is incorrect", OnInvoiceCreatedCalled, e.CountCompleted);
			AssertEquals("TotalInvoicesToCreate is incorrect", TotalInvoicesToCreate, e.TotalCount);
		}

		int TotalInvoicesToCreate;
		int OnInvoiceCreatedCalled;
		int OnInvoiceCreationCompleteCalled;

		OrgHeader Org;
		OrgHeader Org2;
		OrgHeader Org3;
		OrgHeader Org4;
		WhsWarehouse Warehouse;

		void SetupWarehouseWithDockets()
		{
			Warehouse = Helper.CreateWarehouse("Zubin", "A");

			Org = Factory.NewWithValidTestData<OrgHeader>();
			Org.CompanyData.OB_IsDebtor = true;
			var org1Contact = Org.Contacts.AddNew();
			org1Contact.OC_ContactName = "Org1Contact";
			var doc1 = org1Contact.Documents.AddNew();
			doc1.OD_DocumentGroup = "ALL";

			Org2 = Factory.NewWithValidTestData<OrgHeader>();
			Org2.CompanyData.OB_IsDebtor = true;
			var org2Contact = Org2.Contacts.AddNew();
			org2Contact.OC_ContactName = "Org2Contact";
			var doc2 = org2Contact.Documents.AddNew();
			doc2.OD_DocumentGroup = "ALL";

			Org3 = Factory.NewWithValidTestData<OrgHeader>();
			Org3.CompanyData.OB_IsDebtor = true;
			var org3Contact = Org3.Contacts.AddNew();
			org3Contact.OC_ContactName = "Org3Contact";
			var doc3 = org3Contact.Documents.AddNew();
			doc3.OD_DocumentGroup = "ALL";

			Org4 = Factory.NewWithValidTestData<OrgHeader>();
			Org4.CompanyData.OB_IsDebtor = true;
			var org4Contact = Org4.Contacts.AddNew();
			org4Contact.OC_ContactName = "Org4Contact";
			var doc4 = org4Contact.Documents.AddNew();
			doc4.OD_DocumentGroup = "ALL";

			Factory.Save();

			var org1Part = Helper.CreateProduct(Org, "P1");
			var org2Part = Helper.CreateProduct(Org2, "P2");
			var org3Part = Helper.CreateProduct(Org3, "P3");
			var org4Part = Helper.CreateProduct(Org4, "P4");

			Helper.CreateWhsInvoice(Org2, Warehouse, new ZDateTime(2011, 1, 1), false);
			Helper.CreateWhsInvoice(Org3, Warehouse, new ZDateTime(2011, 1, 1), false);

			var docket1 = Helper.CreateWhsReceive(Org, Warehouse, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(docket1, org1Part, 1000);
			docket1.AllocateLocationsWithMock();
			docket1.FinaliseDocket();

			var docket2 = Helper.CreateWhsReceive(Org2, Warehouse, "2", Notify);
			Helper.CreateWhsReceiveInventoryLine(docket2, org2Part, 1000);
			docket2.AllocateLocationsWithMock();
			docket2.FinaliseDocket();

			var docket3 = Helper.CreateWhsReceive(Org3, Warehouse, "3", Notify);
			Helper.CreateWhsReceiveInventoryLine(docket3, org3Part, 1000);
			docket3.AllocateLocationsWithMock();
			docket3.FinaliseDocket();

			var docket4 = Helper.CreateWhsReceive(Org4, Warehouse, "4", Notify);
			Helper.CreateWhsReceiveInventoryLine(docket4, org4Part, 1000);
			docket4.AllocateLocationsWithMock();
			docket4.FinaliseDocket();

			Factory.Save();
		}

		#endregion

		#region TestAutoRateInvoices

		[GuiTest]
		public void TestAutoRateInvoice()
		{
			// Arrange
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.CompanyData.OB_IsDebtor = true;
			var orgContact = data.Org1.Contacts.AddNew();
			var doc = orgContact.Documents.AddNew();
			doc.OD_DocumentGroup = "ALL";
			Factory.Save();

			Helper.CreateWhsInvoice(data.Org1, data.Whs1, new ZDateTime(2011, 1, 1), false);
			var docket = Helper.CreateWhsReceive(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsReceiveInventoryLine(docket, data.Part1, 1000);
			docket.AllocateLocationsWithMock();
			docket.FinaliseDocket();
			Factory.Save();

			var multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice.LoadOrCreateInvoices();
			AssertEquals("Precondition", 1, multiClientInvoice.Invoices.Count);

			multiClientInvoice.Invoices[0].ET_StorageJobNumber = "1";
			multiClientInvoice.Invoices[0].IncludeInInvoicing = true;

			using (multiClientInvoice.Invoices[0].GetValidationSuspender())
			{
				multiClientInvoice.Invoices.Sort(JobStorageSchema.Constants.ET_StorageJobNumber, ListSortDirection.Descending);

				// Act
				multiClientInvoice.AutoRateInvoices(CancellationToken.None);

				// Assert
				AssertEquals("There should only be 1 invoice in total", 1, multiClientInvoice.Invoices.Count);
				AssertEquals("Job is not posted", false, multiClientInvoice.Invoices[0].JobHeader.Charges[0].IsRevenuePosted);
				AssertEquals("Print task not run", 0, multiClientInvoice.LastPrintTaskCountForTest);
			}
		}

		#endregion

		#region TestCreatesInvoice_WarehouseBranch

		[TestDate(2021, 09, 15)]
		public void TestCreatesInvoice_WarehouseBranch()
		{
			var numberOfCreateRun = 1;
			var branch1 = CreateClientAndChargeWithValidaDataToGenerateInvoice();
			var branch2 = CreateClientAndChargeWithValidaDataToGenerateInvoice();

			var multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice.LoadOrCreateInvoices(selectBranchFromWarehouse: true);
			multiClientInvoice.AutoRateInvoices(CancellationToken.None, selectBranchFromWarehouse: true);
			AssertEquals("Should create 2 invoices with charges.", 2, multiClientInvoice.Invoices.Count(i => ((WhsInvoice)i).JobHeader.Charges.Count > 0));
			AssertNotNull("Should have one invoice for branch1.", multiClientInvoice.Invoices.Single(i => ((WhsInvoice)i).JobHeader.Branch.PK == branch1.PK));
			AssertNotNull("Should have one invoice for branch2.", multiClientInvoice.Invoices.Single(i => ((WhsInvoice)i).JobHeader.Branch.PK == branch2.PK));

			GlbBranch CreateClientAndChargeWithValidaDataToGenerateInvoice()
			{
				var client = Helper.CreateClient($"Client{numberOfCreateRun}");
				var branch = Helper.CreateGlbBranch($"Br{numberOfCreateRun}");
				var address = Factory.NewWithValidTestData<OrgAddress>();
				var whs = Helper.CreateWarehouse($"WH{numberOfCreateRun}", address, branch);
				Helper.CreateRowAndGenerateLocations(whs, "A");
				whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
				whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
				client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
				var today = ZDateTimeOffset.Today;
				var product = Helper.CreateProduct($"Product{numberOfCreateRun}", client);
				Factory.Save();

				AssertEquals("Predondition", branch.Company.PK, client.CompanyData.Company.PK);
				//just to have pre existing invoice
				var invoice = (IJobInvoicingPlugIn)Helper.CreateWhsInvoice(whs.PK, client.PK, today.AddDays(-14).Date, today.AddDays(-7).Date);
				var secondJobHeader = Helper.CreateAccountingDataWithNoCharge(invoice);
				secondJobHeader.JH_GC = branch.Company.PK;
				secondJobHeader.JH_Status = JobHeaderStatus.Closed.Code;
				Factory.Save();

				var receive = CreateWhsReceive(whs, client, today.AddDays(-6), true, $"R{numberOfCreateRun}", product, 10);
				Factory.Save();

				// Setting to generate charge when autorate invoice
				var receiveHandlingFumigationCharge = Helper.CreateChargeCode($"WRECFUM{numberOfCreateRun}", "Warehouse Receive Handling Fumigation", ChargeCodeGroupList.Codes.WHSInwards, Constants.FreightServiceType.Codes.Fumigation);
				var clientRate = Helper.CreateClientRate(client);
				var warehouseRate = Helper.CreateRateEntry(clientRate, today.AddMonths(-6).Date, today.Date);
				Helper.CreateRateLine(warehouseRate, receiveHandlingFumigationCharge, "SV", 5m);

				// any charges
				var fumigationService = receive.Services.AddNew();
				fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
				fumigationService.ES_ServiceCount = 30m;
				fumigationService.ES_Completed = today.AddDays(-1).Date;
				Factory.Save();

				numberOfCreateRun++;
				return branch;
			}
		}

		#endregion

		#region TestCreatesInvoice_WarehouseDifferentBranch

		[TestDate(2021, 09, 15)]
		public void TestCreatesInvoice_WarehouseDifferentBranch()
		{
			var client = Helper.CreateClient("Client");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse($"WHS", address, branch);
			Helper.CreateRowAndGenerateLocations(whs, "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			var todayOffset = ZDateTimeOffset.Today;
			var today = todayOffset.Date;
			var product = Helper.CreateProduct($"Product", client);
			Factory.Save();

			AssertEquals("Predondition", branch.Company.PK, client.CompanyData.Company.PK);
			//just to have pre existing invoice
			var invoice = (IJobInvoicingPlugIn)Helper.CreateWhsInvoice(whs.PK, client.PK, today.AddDays(-14), today.AddDays(-7));
			var secondJobHeader = Helper.CreateAccountingDataWithNoCharge(invoice);
			secondJobHeader.JH_GC = branch.Company.PK;
			secondJobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			var receive = CreateWhsReceive(whs, client, todayOffset.AddDays(-6), true, $"R0", product, 10);
			Factory.Save();

			// Setting to generate charge when autorate invoice
			var receiveHandlingFumigationCharge = Helper.CreateChargeCode($"WRECFUM", "Warehouse Receive Handling Fumigation", ChargeCodeGroupList.Codes.WHSInwards, Constants.FreightServiceType.Codes.Fumigation);
			var clientRate = Helper.CreateClientRate(client);
			var warehouseRate = Helper.CreateRateEntry(clientRate, today.AddMonths(-6), today);
			Helper.CreateRateLine(warehouseRate, receiveHandlingFumigationCharge, "SV", 5m);

			// any charges
			var fumigationService = receive.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 30m;
			fumigationService.ES_Completed = today.AddDays(-1);
			Factory.Save();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var otherBranch = Helper.CreateGlbBranch("Br2");
			otherBranch.GB_GC = company.PK;
			whs.WW_GB_RelatedCompanyBranch = otherBranch.PK;
			Factory.Save();

			var multiClientInvoice1 = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice1.LoadOrCreateInvoices(selectBranchFromWarehouse: true);
			multiClientInvoice1.AutoRateInvoices(CancellationToken.None, selectBranchFromWarehouse: true);
			AssertEquals("Should not create invoice.", 0, multiClientInvoice1.Invoices.Count);

			whs.WW_GB_RelatedCompanyBranch = branch.PK;
			Factory.Save();
			var multiClientInvoice2 = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice2.LoadOrCreateInvoices(selectBranchFromWarehouse: true);
			multiClientInvoice2.AutoRateInvoices(CancellationToken.None, selectBranchFromWarehouse: true);
			AssertEquals("Should create one invoice with charge.", 1, multiClientInvoice2.Invoices.Count(i => ((WhsInvoice)i).JobHeader.Charges.Count > 0));
		}

		#endregion

		#region TestCreatesInvoice_CompanyWarehouseRatingPeriodRegistry

		[TestDate(2021, 09, 15)]
		public void TestCreatesInvoice_CompanyWarehouseRatingPeriodRegistry_Default()
		{
			TestCreatesInvoice_CompanyWarehouseRatingPeriodRegistry_Core(Constants.StorageCalculationPeriods.Default, ZDate.Today, expectedInvoiceToDate: (fromDate) => fromDate.AddDays(7).AddDays(-1));
		}

		[TestDate(2021, 09, 15)]
		public void TestCreatesInvoice_CompanyWarehouseRatingPeriodRegistry_Weekly()
		{
			TestCreatesInvoice_CompanyWarehouseRatingPeriodRegistry_Core(Constants.StorageCalculationPeriods.Weekly, ZDate.Today, expectedInvoiceToDate: (fromDate) => fromDate.AddDays(7).AddDays(-1));
		}

		[TestDate(2021, 09, 15)]
		public void TestCreatesInvoice_CompanyWarehouseRatingPeriodRegistry_Monthly()
		{
			TestCreatesInvoice_CompanyWarehouseRatingPeriodRegistry_Core(Constants.StorageCalculationPeriods.Monthly, ZDate.Today, expectedInvoiceToDate: (fromDate) => fromDate.AddMonths(1).AddDays(-1));
		}

		[TestDate(2021, 09, 15)]
		public void TestCreatesInvoice_CompanyWarehouseRatingPeriodRegistry_BillingPeriod()
		{
			var today = ZDate.Today;
			TestCreatesInvoice_CompanyWarehouseRatingPeriodRegistry_Core(Constants.StorageCalculationPeriods.BillingPeriod, today, expectedInvoiceToDate: (_) => today);
		}

		void TestCreatesInvoice_CompanyWarehouseRatingPeriodRegistry_Core(string warehouseRatingPeriod, ZDate today, Func<ZDate, ZDate> expectedInvoiceToDate)
		{
			var client = Helper.CreateClient("Client");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse($"WHS", address, branch);
			Helper.CreateRowAndGenerateLocations(whs, "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			var product = Helper.CreateProduct($"Product", client);
			Factory.Save();

			AssertEquals("Precondition", branch.Company.PK, client.CompanyData.Company.PK);
			AssertEquals("Precondition", Constants.StorageCalculationPeriods.Default, client.CompanyData.OB_ARWarehouseRatingPeriod);
			var notInvoicedDateYet = today.AddDays(-40); // more than one month
			var invoice = (IJobInvoicingPlugIn)Helper.CreateWhsInvoice(whs.PK, client.PK, notInvoicedDateYet.AddDays(-7), notInvoicedDateYet.AddDays(-1));
			var secondJobHeader = Helper.CreateAccountingDataWithNoCharge(invoice);
			secondJobHeader.JH_GC = branch.Company.PK;
			secondJobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();
			var chargeDateInNewPeriod = notInvoicedDateYet.AddDays(1);
			var receive = CreateWhsReceive(whs, client, whs.GetWarehouseBranchDateTimeOffset(chargeDateInNewPeriod), true, $"R0", product, 10);
			Factory.Save();

			// Setting to generate charge when autorate invoice
			var receiveHandlingFumigationCharge = Helper.CreateChargeCode($"WRECFUM", "Warehouse Receive Handling Fumigation", ChargeCodeGroupList.Codes.WHSInwards, Constants.FreightServiceType.Codes.Fumigation);
			var clientRate = Helper.CreateClientRate(client);
			var warehouseRate = Helper.CreateRateEntry(clientRate, notInvoicedDateYet, today);
			Helper.CreateRateLine(warehouseRate, receiveHandlingFumigationCharge, "SV", 5m);

			// change registry
			client.CompanyData.OB_ARWarehouseRatingPeriod = warehouseRatingPeriod;

			// any charges
			var fumigationService = receive.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 30m;
			fumigationService.ES_Completed = chargeDateInNewPeriod;
			Factory.Save();

			var multiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
			multiClientInvoice.LoadOrCreateInvoices(selectBranchFromWarehouse: true);
			multiClientInvoice.AutoRateInvoices(CancellationToken.None, selectBranchFromWarehouse: true);
			var newInvoice = multiClientInvoice.Invoices.Cast<WhsInvoice>().Single();
			AssertEquals("Should new invoice start from last invoice.", notInvoicedDateYet, newInvoice.ET_StorageFromDate.Date);
			AssertEquals("Should invoice to date set based on registry setting.", expectedInvoiceToDate(notInvoicedDateYet), newInvoice.ET_StorageToDate.Date);
		}

		#endregion

		#region Validation

		public void TestValidationType()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			var line = new WhsDocketLabelLine(order, 1, 2);
			AssertEquals(typeof(WhsDocketLabelLineValidation), line.Validation.GetType());
		}

		#endregion

		#region Implementations

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WhsInvoicePeriodicMultiClientInvoice();
		}

		protected new WhsTestHelperFunctionsInvoice Helper => helper ?? (helper = new WhsTestHelperFunctionsInvoice(Factory));
		WhsTestHelperFunctionsInvoice helper;

		#endregion
	}
}
