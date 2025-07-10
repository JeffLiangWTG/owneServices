using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.Warehouse.Transactions.Invoicing.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsInvoiceHelperTest : WhsTestCaseWithFactory
	{
		#region TestGetByPK

		public void TestGetByPK()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var yesterday = ZDate.Today.AddDays(-1);
			SetupDataToBeAbleToChargeClientInInvoice(data.Org1, data.Whs1, data.Part1, 1, yesterday);

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertEquals("Should load same PK.", invoice.PK, invoiceHelper.GetByPK(invoice.PK).PK);
			AssertNull("Should not load irrelevant PK.", invoiceHelper.GetByPK(ZGuid.NewZGuid()));
		}

		#endregion

		#region TestGetQueuedInvoiceValidToProcessPKs

		public void TestGetQueuedInvoiceValidToProcessPKs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var yesterday = ZDate.Today.AddDays(-1);
			SetupDataToBeAbleToChargeClientInInvoice(data.Org1, data.Whs1, data.Part1, 1, yesterday);
			var client2 = Helper.CreateClient("C02");

			var invoice1 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice1.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			var invoice2 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, client2.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice2.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			var invoice3 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-14), ZDateTime.Now.AddDays(-7));
			invoice3.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.NIQ;
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertContainsExactElementsInAnyOrder("Should load only invoice in queue.", new[] { invoice1.PK, invoice2.PK }, invoiceHelper.GetQueuedInvoiceValidToProcessPKs().ToArray());

			invoice3.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should load all invoices in queue.", new[] { invoice1.PK, invoice2.PK, invoice3.PK }, invoiceHelper.GetQueuedInvoiceValidToProcessPKs().ToArray());
		}

		public void TestGetQueuedInvoiceValidToProcessPKs_Future()
		{
			AssertGetQueuedInvoiceValidToProcessPKs(storageToDate: ZDateTime.Now.AddDays(1), expectedResult: false);
		}

		[TestDate(2022, 7, 12, 9, 30, 0)]
		public void TestGetQueuedInvoiceValidToProcessPKs_Today()
		{
			AssertGetQueuedInvoiceValidToProcessPKs(storageToDate: ZDateTime.Now, expectedResult: false);
		}

		[TestDate(2022, 7, 12, 15, 30, 0)]
		public void TestGetQueuedInvoiceValidToProcessPKs_TodayAfterAnHour()
		{
			AssertGetQueuedInvoiceValidToProcessPKs(storageToDate: ZDateTime.Now.AddHours(1), expectedResult: false);
		}

		public void TestGetQueuedInvoiceValidToProcessPKs_Past()
		{
			AssertGetQueuedInvoiceValidToProcessPKs(storageToDate: ZDateTime.Now.AddDays(-2), expectedResult: true);
		}

		void AssertGetQueuedInvoiceValidToProcessPKs(ZDateTime storageToDate, bool expectedResult)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var dayBefore = storageToDate.Date.AddDays(-1);
			SetupDataToBeAbleToChargeClientInInvoice(data.Org1, data.Whs1, data.Part1, 1, dayBefore);

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, storageToDate.AddDays(-6), storageToDate);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();
			AssertEquals($"Precondition - to date set based on one week after from date!", storageToDate.ToSmallDateTime(), invoice.ET_StorageToDate.ToSmallDateTime());

			var invoiceHelper = new WhsInvoiceHelper();
			var result = expectedResult ? new[] { invoice.PK } : Array.Empty<ZGuid>();
			AssertContainsExactElementsInAnyOrder($"Should {(expectedResult ? "" : "not ")}have result.", result, invoiceHelper.GetQueuedInvoiceValidToProcessPKs().ToArray());
		}

		#endregion

		#region TestLoadInNewFactory

		public void TestLoadInNewFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var yesterday = ZDate.Today.AddDays(-1);
			SetupDataToBeAbleToChargeClientInInvoice(data.Org1, data.Whs1, data.Part1, 1, yesterday);

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			var getByPKFactory1 = invoiceHelper.GetByPK(invoice.PK).Factory;
			var getByPKFactory2 = invoiceHelper.GetByPK(invoice.PK).Factory;
			AssertEquals("Should load in same factory.", getByPKFactory1, getByPKFactory2);

			var loadInNewFactory1 = invoiceHelper.LoadInNewFactory(invoice.PK).Factory;
			var loadInNewFactory2 = invoiceHelper.LoadInNewFactory(invoice.PK).Factory;
			AssertNotEquals("Should not be same factory.", getByPKFactory1, loadInNewFactory1);
			AssertNotEquals("Should not be same factory.", loadInNewFactory1, loadInNewFactory2);
		}

		#endregion

		#region TestIsStillInQueueInDB

		public void TestIsStillInQueueInDB()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var yesterday = ZDate.Today.AddDays(-1);
			SetupDataToBeAbleToChargeClientInInvoice(data.Org1, data.Whs1, data.Part1, 1, yesterday);

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertEquals("Should be in Queue.", true, invoiceHelper.IsStillInQueueInDB(invoice.PK));

			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.NIQ;
			Factory.Save();
			AssertEquals("Should not be in Queue.", false, invoiceHelper.IsStillInQueueInDB(invoice.PK));
		}

		#endregion

		#region TestAutoRateJobHeaderInUserContext

		public void TestAutoRateJobHeaderInUserContext()
		{
			var client = Helper.CreateClient("Client");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse("WHS", address, branch);
			Helper.CreateRowAndGenerateLocations(whs, "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var product = Helper.CreateProduct("Product", client);
			Factory.Save();

			var yesterday = ZDate.Today.AddDays(-1);
			SetupDataToBeAbleToChargeClientInInvoice(client, whs, product, 1, yesterday);

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var otherBranch = Helper.CreateGlbBranch("Br2");
			otherBranch.GB_GC = company.PK;
			whs.WW_GB_RelatedCompanyBranch = otherBranch.PK;
			Factory.Save();

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			Factory.Save();

			var expectedLog1 = @"Invoice does not have charges for client Client warehouse WHS.";
			AutoRateInUserContextAndAssertLog(otherBranch.PK.ToGuid(), Env.CurrentDepartment.PK, expectedLog1, string.Empty);

			var expectedLog2 = @"
Autorated Invoice for client Client warehouse WHS.";
			AutoRateInUserContextAndAssertLog(branch.PK.ToGuid(), Env.CurrentDepartment.PK, expectedLog2, BillingAutomationStatusCodes.BillingAutomationAutorated);

			void AutoRateInUserContextAndAssertLog(Guid branchPK, Guid departmentPK, string log, string expectedInvoiceStatus)
			{
				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branchPK, departmentPK))
				{
					var whsInvoiceHelper = new WhsInvoiceHelper();
					var invoiceInNewFactory = whsInvoiceHelper.LoadInNewFactory(invoice.PK);
					invoiceInNewFactory.Client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
					var logger = new TestAutoRatingServiceLogger();
					var succeed = whsInvoiceHelper.AutoRateJobHeader(logger, invoiceInNewFactory);
					if (succeed)
					{
						invoiceInNewFactory.Factory.Save();
					}
					AssertMultilineASCIIEquals(log, logger.ToString());

					AssertEquals(expectedInvoiceStatus, invoiceInNewFactory.BillingAutomationStatus);
					DeleteJobHeadersWithoutAnyCharges(invoiceInNewFactory);
				}
			}
		}

		#endregion

		#region TestPostJobHeaderInUserContext

		public void TestPostJobHeaderInUserContext()
		{
			var client = Helper.CreateClient("Client");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse("WHS", address, branch);
			Helper.CreateRowAndGenerateLocations(whs, "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var product = Helper.CreateProduct("Product", client);
			Factory.Save();

			var yesterday = ZDate.Today.AddDays(-1);
			SetupDataToBeAbleToChargeClientInInvoice(client, whs, product, 1, yesterday);

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var otherBranch = Helper.CreateGlbBranch("Br2");
			otherBranch.GB_GC = company.PK;
			whs.WW_GB_RelatedCompanyBranch = otherBranch.PK;
			Factory.Save();

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			Factory.Save();

			var expectedLog1 = @"Invoice does not have charges for client Client warehouse WHS.";
			PostInUserContextAndAssertLog(otherBranch.PK.ToGuid(), Env.CurrentDepartment.PK, expectedLog1, string.Empty);

			var expectedLog2 = @"
Autorated Invoice for client Client warehouse WHS.
Posted Invoice for client Client warehouse WHS.";
			PostInUserContextAndAssertLog(branch.PK.ToGuid(), Env.CurrentDepartment.PK, expectedLog2, BillingAutomationStatusCodes.BillingAutomationPosted);

			void PostInUserContextAndAssertLog(Guid branchPK, Guid departmentPK, string log, string expectedInvoiceStatus)
			{
				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branchPK, departmentPK))
				{
					var whsInvoiceHelper = new WhsInvoiceHelper();
					var invoiceInNewFactory = whsInvoiceHelper.LoadInNewFactory(invoice.PK);
					invoiceInNewFactory.Client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
					invoiceInNewFactory.Client.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
					var logger = new TestAutoRatingServiceLogger();
					var succeed = whsInvoiceHelper.AutoRateJobHeader(logger, invoiceInNewFactory) &&
						whsInvoiceHelper.InvoiceSaveSafe(invoiceInNewFactory, logger, 1) == InvoiceSaveSafeResult.SaveSuccessful &&
						whsInvoiceHelper.PostInvoice(logger, invoiceInNewFactory);
					AssertMultilineASCIIEquals(log, logger.ToString());

					AssertEquals(expectedInvoiceStatus, invoiceInNewFactory.BillingAutomationStatus);
					DeleteJobHeadersWithoutAnyCharges(invoiceInNewFactory);
				}
			}
		}

		#endregion

		#region TestDeliverJobHeaderInUserContext

		public void TestDeliverJobHeaderInUserContext()
		{
			var client = Helper.CreateClient("Client");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse("WHS", address, branch);
			Helper.CreateRowAndGenerateLocations(whs, "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var product = Helper.CreateProduct("Product", client);
			var debtor = Helper.CreateClient("debtor");
			var contact = debtor.Contacts.AddNew();
			contact.OC_ContactName = "me";
			contact.OC_Email = "me@wisetechglobal.com.au";
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Receivables.Code;
			document.OD_DefaultContact = true;
			Factory.Save();

			var yesterday = ZDate.Today.AddDays(-1);
			SetupDataToBeAbleToChargeClientInInvoice(client, whs, product, 1, yesterday);

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			var jobHeader = new JobHeader.Loader(invoice).TryLoadOrCreate();
			var testObjectCreator = new TestObjectCreator(Factory);
			var transactionHeader = testObjectCreator.CreateARInvoice<ARInvoice>("1", testObjectCreator.AUD, 1m, testObjectCreator.AALSHI);
			transactionHeader.AH_JH = jobHeader.PK;
			transactionHeader.AH_OH = debtor.PK;
			Factory.Save();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var otherBranch = Helper.CreateGlbBranch("Br2");
			otherBranch.GB_GC = company.PK;
			whs.WW_GB_RelatedCompanyBranch = otherBranch.PK;
			Factory.Save();

			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			Factory.Save();

			var expectedLog1 = @"Invoice does not have charges for client Client warehouse WHS.";
			DeliverInUserContextAndAssertLog(otherBranch.PK.ToGuid(), Env.CurrentDepartment.PK, expectedLog1, string.Empty);

			var expectedLog2 = @"
Autorated Invoice for client Client warehouse WHS.
Posted Invoice for client Client warehouse WHS.
Delivered Invoice for client Client warehouse WHS.";
			DeliverInUserContextAndAssertLog(branch.PK.ToGuid(), Env.CurrentDepartment.PK, expectedLog2, BillingAutomationStatusCodes.BillingAutomationDelivered);

			void DeliverInUserContextAndAssertLog(Guid branchPK, Guid departmentPK, string log, string expectedInvoiceStatus)
			{
				using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branchPK, departmentPK))
				{
					var whsInvoiceHelper = new WhsInvoiceHelper();
					var invoiceInNewFactory = whsInvoiceHelper.LoadInNewFactory(invoice.PK);
					invoiceInNewFactory.Client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
					invoiceInNewFactory.Client.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
					invoiceInNewFactory.Client.CompanyData.OB_WhsAutoDeliverPeriodicInvoice = true;
					var logger = new TestAutoRatingServiceLogger();
					var succeed = whsInvoiceHelper.AutoRateJobHeader(logger, invoiceInNewFactory) &&
						whsInvoiceHelper.InvoiceSaveSafe(invoiceInNewFactory, logger, 1) == InvoiceSaveSafeResult.SaveSuccessful &&
						whsInvoiceHelper.PostInvoice(logger, invoiceInNewFactory) &&
						whsInvoiceHelper.DeliverInvoice(logger, invoiceInNewFactory);
					AssertMultilineASCIIEquals(log, logger.ToString());

					AssertEquals(expectedInvoiceStatus, invoiceInNewFactory.BillingAutomationStatus);
					DeleteJobHeadersWithoutAnyCharges(invoiceInNewFactory);
				}
			}
		}

		#endregion

		#region TestSetUserContextForInvoice

		public void TestSetUserContextForInvoice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Helper.CreateGlbBranch("Bra");
			branch.GB_GC = company.PK;
			data.Whs1.WW_GB_RelatedCompanyBranch = branch.PK;

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var combo = branch.AllowedDepartments.AddNew();
			combo.AAB_GE_Department = department.PK;

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var currentBranchPK = GlbBranch.CurrentBranch.PK;
			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			var currentDepartmentPK = GlbDepartment.CurrentDepartment.PK;
			using (WhsInvoiceHelper.SetUserContextForInvoice(invoice))
			{
				AssertNotEquals("The context should be changed to match the invoice warehouse branch.", currentBranchPK, GlbBranch.CurrentBranch.PK);
				AssertNotEquals("The context should be changed to match the invoice warehouse company.", currentCompanyPK, GlbCompany.CurrentCompany.PK);
				AssertNotEquals("The context should be changed to match the invoice warehouse department.", currentDepartmentPK, GlbDepartment.CurrentDepartment.PK);
				AssertEquals("Should match with invoice warehouse branch.", branch.PK, GlbBranch.CurrentBranch.PK);
				AssertEquals("Should match with invoice warehouse company.", company.PK, GlbCompany.CurrentCompany.PK);
				AssertEquals("Should match with invoice warehouse department.", department.PK, GlbDepartment.CurrentDepartment.PK);
			}
		}

		public void TestSetUserContextForInvoice_NoneDepartmentAllowed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Helper.CreateGlbBranch("Bra");
			branch.GB_GC = company.PK;
			data.Whs1.WW_GB_RelatedCompanyBranch = branch.PK;

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var currentBranchPK = GlbBranch.CurrentBranch.PK;
			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			var currentDepartmentPK = GlbDepartment.CurrentDepartment.PK;
			using (WhsInvoiceHelper.SetUserContextForInvoice(invoice))
			{
				AssertNotEquals("The context should be changed to match the invoice warehouse branch.", currentBranchPK, GlbBranch.CurrentBranch.PK);
				AssertNotEquals("The context should be changed to match the invoice warehouse company.", currentCompanyPK, GlbCompany.CurrentCompany.PK);
				AssertEquals("The context should use the current department if none branch department allowed", currentDepartmentPK, GlbDepartment.CurrentDepartment.PK);
				AssertEquals("Should match with invoice warehouse branch.", branch.PK, GlbBranch.CurrentBranch.PK);
				AssertEquals("Should match with invoice warehouse company.", company.PK, GlbCompany.CurrentCompany.PK);
			}
		}

		public void TestSetUserContextForInvoice_DepartmentOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Factory.Save();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Helper.CreateGlbBranch("Bra");
			branch.GB_GC = company.PK;
			data.Whs1.WW_GB_RelatedCompanyBranch = branch.PK;

			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "CCC";
			var combo1 = branch.AllowedDepartments.AddNew();
			combo1.AAB_GE_Department = department1.PK;

			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			department2.GE_Code = "AAA";
			var combo2 = branch.AllowedDepartments.AddNew();
			combo2.AAB_GE_Department = department2.PK;

			var department3 = Factory.NewWithValidTestData<GlbDepartment>();
			department3.GE_Code = "BBB";
			var combo3 = branch.AllowedDepartments.AddNew();
			combo3.AAB_GE_Department = department3.PK;

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var currentBranchPK = GlbBranch.CurrentBranch.PK;
			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			var currentDepartmentPK = GlbDepartment.CurrentDepartment.PK;
			using (WhsInvoiceHelper.SetUserContextForInvoice(invoice))
			{
				AssertNotEquals("The context should be changed to match the invoice warehouse branch.", currentBranchPK, GlbBranch.CurrentBranch.PK);
				AssertNotEquals("The context should be changed to match the invoice warehouse company.", currentCompanyPK, GlbCompany.CurrentCompany.PK);
				AssertNotEquals("The context should be changed to match the invoice warehouse department.", currentDepartmentPK, GlbDepartment.CurrentDepartment.PK);
				AssertEquals("Should match with invoice warehouse branch.", branch.PK, GlbBranch.CurrentBranch.PK);
				AssertEquals("Should match with invoice warehouse company.", company.PK, GlbCompany.CurrentCompany.PK);
				AssertEquals("Should match with first invoice warehouse department ordered by departmentCode.", department2.PK, GlbDepartment.CurrentDepartment.PK);
			}
		}

		#endregion

		#region TestGetInvoiceBillingAutomationMutex

		public void TestGetInvoiceBillingAutomationMutex()
		{
			var invoicePK = ZGuid.NewZGuid();
			var mutex = new WhsInvoiceHelper().GetInvoiceBillingAutomationMutex(invoicePK);
			AssertEquals("MutexID should match.", MutexIDs.WhsInvoiceAutoRate, mutex.MutexID);
			AssertEquals("RecordIdentifier should match with invoice pk", invoicePK.ToString(), mutex.RecordIdentifier);
		}

		#endregion

		#region TestInvoiceSaveSafe

		public void TestInvoiceSaveSafe()
		{
			var logger = new SimpleLogger();
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			AssertEquals("Precondition - not saved", false, invoice.IsInDatabase);
			var saved = false;
			invoice.Factory.Saved += (f, s) => { saved = true; };

			var invoiceHelper = new WhsInvoiceHelper();
			invoiceHelper.InvoiceSaveSafe(invoice, logger, "No error to show this message!");

			AssertEquals("No log for exceptions.", "", logger.ToString());
			AssertEquals("Should save invoice.", true, saved);
			AssertEquals("Should save invoice.", true, invoice.IsInDatabase);
		}

		public void TestInvoiceSaveSafe_NoExceptionThrown()
		{
			var logger = new SimpleLogger();
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			var saved = false;
			invoice.Factory.Saved += (f, s) => { saved = true; };

			var invoiceHelper = new WhsInvoiceHelper();
			var invoiceSaveSafeResult = invoiceHelper.InvoiceSaveSafe(invoice, logger, "No error to show this message!");

			AssertEquals("No error should succeed", InvoiceSaveSafeResult.SaveSuccessful, invoiceSaveSafeResult);
			AssertEquals("No log for exceptions.", "", logger.ToString());
			AssertEquals("Should save invoice.", true, saved);
		}

		public void TestInvoiceSaveSafe_RandomExceptionThrown()
		{
			var logger = new SimpleLogger();
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.Factory.Saving += f => throw new InvalidOperationException("Test");

			var invoiceHelper = new WhsInvoiceHelper();
			AssertExceptionThrown<InvalidOperationException>(() => invoiceHelper.InvoiceSaveSafe(invoice, logger, ""));

			AssertEquals("Should log exceptions.", "", logger.ToString());
		}

		public void TestInvoiceSaveSafe_ZSaveException()
		{
			var logger = new SimpleLogger();
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.Factory.Saving += f => throw new ZSaveException(new ZDataException(new InvalidOperationException("Inner exception!"), null, null), invoice.Factory);

			var invoiceHelper = new WhsInvoiceHelper();
			var invoiceSaveSafeResult = invoiceHelper.InvoiceSaveSafe(invoice, logger, "Adding a message to distinguish between retries and saving in different places (e.g Second Attempt!).");
			AssertEquals("On ZSaveException should retry", InvoiceSaveSafeResult.SaveFailedRetry, invoiceSaveSafeResult);

			AssertMultilineASCIIEquals("Should add the exception message to log.", $@"Error: 
** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record
Inner Message = Inner exception!

Adding a message to distinguish between retries and saving in different places (e.g Second Attempt!).
", logger.ToString());
		}

		public void TestInvoiceSaveSafe_ZCannotSaveExceptionThrown()
		{
			var logger = new SimpleLogger();
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.Factory.Saving += f => throw new ZCannotSaveException("Test", "Test");

			var invoiceHelper = new WhsInvoiceHelper();
			var invoiceSaveSafeResult = invoiceHelper.InvoiceSaveSafe(invoice, logger, "(When attempting to change status to error).");
			AssertEquals("On ZCannotSaveException should retry", InvoiceSaveSafeResult.SaveFailedRetry, invoiceSaveSafeResult);

			AssertMultilineASCIIEquals("Should add the exception message to log.", @"
Error: Test
(When attempting to change status to error).", logger.ToString());
		}

		public void TestInvoiceSaveSafe_OnSavingCriticalCheckException()
		{
			var logger = new SimpleLogger();
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			invoice.Factory.Saving += f => throw new OnSavingCriticalCheckException<JobCharge>(jobCharge, CriticalValidationErrorType.JobChargeAmountExceedMaximumAllowedAmount, "Error Has Occurred", "Nothing");

			var invoiceHelper = new WhsInvoiceHelper();
			var invoiceSaveSafeResult = invoiceHelper.InvoiceSaveSafe(invoice, logger, "Adding a message to distinguish between retries and saving in different places (e.g First Attempt!).");
			AssertEquals("On OnSavingCriticalCheckException should not retry", InvoiceSaveSafeResult.SaveFailedNoRetry, invoiceSaveSafeResult);

			AssertMultilineASCIIEquals("Should add the exception message to log.", @"
Error: Error Has Occurred
Adding a message to distinguish between retries and saving in different places (e.g First Attempt!).
", logger.ToString());
		}

		public void TestInvoiceSaveSafe_SecondAttemptAppendMessage_AdditionalLogMessage()
		{
			TestInvoiceSaveSafe_SecondAttemptAppendMessageCore(2, @"Error: Test
Additional Log Message!", "Additional Log Message!");
		}

		public void TestInvoiceSaveSafe_SecondAttemptAppendMessage_NoAdditionalLogMessage()
		{
			TestInvoiceSaveSafe_SecondAttemptAppendMessageCore(2, "Error: Test", "");
		}

		void TestInvoiceSaveSafe_SecondAttemptAppendMessageCore(int attemp, string expectedLog, string additionalLogMessage)
		{
			var logger = new SimpleLogger();
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.Factory.Saving += f => throw new ZCannotSaveException("Test", "Test");

			var invoiceHelper = new WhsInvoiceHelper();
			invoiceHelper.InvoiceSaveSafe(invoice, logger, additionalLogMessage);

			AssertMultilineASCIIEquals("Should add the exception message to log.", expectedLog, logger.ToString());
		}

		#endregion

		#region TestDisposeLoadedJobHeaders

		public void TestDisposeLoadedJobHeaders_OnSavingCriticalCheckException()
		{
			var expectedError = @"
Error: Error Has Occurred
Testing";
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			var exception = new OnSavingCriticalCheckException<JobCharge>(jobCharge, CriticalValidationErrorType.JobChargeAmountExceedMaximumAllowedAmount, "Error Has Occurred", "Nothing");
			TestDisposeLoadedJobHeaderCore(exception, expectedError);
		}

		public void TestDisposeLoadedJobHeaders_ZCannotSaveException()
		{
			var expectedError = @"
Error: Ops
Testing";
			var exception = new ZCannotSaveException("Ops", "Ops");
			TestDisposeLoadedJobHeaderCore(exception, expectedError);
		}

		public void TestDisposeLoadedJobHeaders_ZSaveException()
		{
			var expectedError = $@"
Error: 
** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record
Inner Message = Inner exception!
Testing
";
			var exception = new ZSaveException(new ZDataException(new InvalidOperationException("Inner exception!"), null, null), new BusinessObjectFactory());
			TestDisposeLoadedJobHeaderCore(exception, expectedError);
		}

		void TestDisposeLoadedJobHeaderCore(Exception exception, string expectedError)
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
			var receiveHandlingFumigationCharge = helper.CreateChargeCode($"WRECFUM", "Warehouse Receive Handling Fumigation", ChargeCodeGroupList.Codes.WHSInwards, "FUM");
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

			var logger = new TestAutoRatingServiceLogger();
			var invoiceHelper = new WhsInvoiceHelper();
			invoiceHelper.AutoRateJobHeader(logger, invoice);
			invoice.Factory.Saving += f => throw exception;
			invoiceHelper.InvoiceSaveSafe(invoice, logger, "Testing");

			AssertMultilineASCIIEquals("Should be processed when Still in queue.", "Autorated Invoice for client 111 warehouse 1." + expectedError, logger.ToString());
		}

		#endregion

		#region TestAttemptToStrConvert

		public void TestAttemptToStrConvert()
		{
			var invoiceHelper = new WhsInvoiceHelper();
			AssertExceptionThrown<ArgumentException>("Is not valid params should have exception.", "We expected to call method always between 1 to 3.", () => invoiceHelper.AttemptToStrConvert(0));
			AssertEquals("(First attempt).", invoiceHelper.AttemptToStrConvert(1));
			AssertEquals("(Second attempt).", invoiceHelper.AttemptToStrConvert(2));
			AssertEquals("(When attempting to change status to error).", invoiceHelper.AttemptToStrConvert(3));
			AssertExceptionThrown<ArgumentException>("Is not valid params should have exception.", "We expected to call method always between 1 to 3.", () => invoiceHelper.AttemptToStrConvert(4));
		}

		#endregion

		#region TestInvalidMethodCall_InvalidOperationException

		public void TestInvalidMethodCall_InvalidOperationException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;
			data.Org1.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var yesterday = ZDate.Today.AddDays(-1);
			SetupDataToBeAbleToChargeClientInInvoice(data.Org1, data.Whs1, data.Part1, 1, yesterday);

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			invoice.TryCreateJobHeader();
			Factory.Save();
			new JobHeader.Loader(invoice).TryLoadOrCreate();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertExceptionThrown<InvalidOperationException>("To make sure we call in sequence.", "When status isn't autorated, it shouldn't be called.", () => invoiceHelper.PostInvoice(new TestAutoRatingServiceLogger(), invoice));
			invoice.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationAutorated;
			AssertNoExceptionThrown(() => invoiceHelper.PostInvoice(new TestAutoRatingServiceLogger(), invoice));

			invoice.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationAutorated;
			data.Org1.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
			AssertExceptionThrown<InvalidOperationException>("To make sure we call in sequence.", "When status isn't posted, it shouldn't be called.", () => invoiceHelper.DeliverInvoice(new TestAutoRatingServiceLogger(), invoice));
			invoice.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationPosted;
			AssertNoExceptionThrown(() => invoiceHelper.DeliverInvoice(new TestAutoRatingServiceLogger(), invoice));
		}

		#endregion

		#region GetOldestUnpostedInvoice

		public void TestGetOldestUnpostedInvoice()
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

			var beforeInvoice = ZDateTimeOffset.Now.AddMonths(-13);
			Helper.CreateWhsReceiveWithInventory(client1, whs1, "R1", beforeInvoice, part, 10m);
			Helper.CreateWhsReceiveWithInventory(client1, whs2, "R2", beforeInvoice, part, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, whs1, "R3", beforeInvoice, part, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, whs2, "R4", beforeInvoice, part, 10m);

			var lastYear = ZDateTime.Now.AddYears(-1);
			// Client1 - Whs1 - posted invoices.
			Helper.CreateWhsInvoice(client1, whs1, lastYear, isPosted: true);

			// Client1 - Whs2 - posted invoices.
			Helper.CreateWhsInvoice(client1, whs2, lastYear, isPosted: true);

			// Client2 - Whs1 - two unposted invoices.
			var invoiceClient2Whs1UnPosted1 = Helper.CreateWhsInvoice(client2, whs1, lastYear, isPosted: false);
			var invoiceClient2Whs1UnPosted2 = Helper.CreateWhsInvoice(client2, whs1, lastYear.AddDays(7), isPosted: false);

			// Client2 - Whs2 - first is unposted invoice second is posted invoice.
			var invoiceClient2Whs2Posted = Helper.CreateWhsInvoice(client2, whs2, lastYear, isPosted: true);
			var invoiceClient2Whs2UnPosted = Helper.CreateWhsInvoice(client2, whs2, lastYear.AddDays(7), isPosted: false);

			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertNull("Invoice posted and should not return.", invoiceHelper.GetOldestUnpostedInvoice(Factory, OrgWarehousePair.Create(client1.PK, whs1.PK)));
			AssertNull("Invoice posted and should not return.", invoiceHelper.GetOldestUnpostedInvoice(Factory, OrgWarehousePair.Create(client1.PK, whs2.PK)));
			AssertEquals("Should return the oldest one.", invoiceClient2Whs1UnPosted1.PK, invoiceHelper.GetOldestUnpostedInvoice(Factory, OrgWarehousePair.Create(client2.PK, whs1.PK)).PK);
			AssertNotEquals("Should return the oldest one.", invoiceClient2Whs1UnPosted2.PK, invoiceHelper.GetOldestUnpostedInvoice(Factory, OrgWarehousePair.Create(client2.PK, whs1.PK)).PK);
			AssertEquals("Should return unposted one.", invoiceClient2Whs2UnPosted.PK, invoiceHelper.GetOldestUnpostedInvoice(Factory, OrgWarehousePair.Create(client2.PK, whs2.PK)).PK);
			AssertNotEquals("Should return unposted one.", invoiceClient2Whs2Posted.PK, invoiceHelper.GetOldestUnpostedInvoice(Factory, OrgWarehousePair.Create(client2.PK, whs1.PK)).PK);
		}

		#endregion

		#region TestOldestUnpostedInvoiceExists

		public void TestOldestUnpostedInvoiceExists()
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

			var beforeInvoice = ZDateTimeOffset.Now.AddMonths(-13);
			Helper.CreateWhsReceiveWithInventory(client1, whs1, "R1", beforeInvoice, part, 10m);
			Helper.CreateWhsReceiveWithInventory(client1, whs2, "R2", beforeInvoice, part, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, whs1, "R3", beforeInvoice, part, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, whs2, "R4", beforeInvoice, part, 10m);

			var lastYear = ZDateTime.Now.AddYears(-1);
			// Whs1 - posted invoices.
			Helper.CreateWhsInvoice(client1, whs1, lastYear, isPosted: true);

			// Whs2 - inposted invoices.
			Helper.CreateWhsInvoice(client1, whs2, lastYear, isPosted: false);

			// Client2 - Whs1 - two unposted invoices.
			Helper.CreateWhsInvoice(client2, whs1, lastYear, isPosted: false);
			Helper.CreateWhsInvoice(client2, whs1, lastYear.AddDays(7), isPosted: true);

			// Client2 - Whs2 - first is unposted invoice second is posted invoice.
			Helper.CreateWhsInvoice(client2, whs2, lastYear, isPosted: true);
			Helper.CreateWhsInvoice(client2, whs2, lastYear.AddDays(7), isPosted: false);

			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertEquals("Invoice posted should return false.", false, invoiceHelper.OldestUnpostedInvoiceExists(OrgWarehousePair.Create(client1.PK, whs1.PK)));
			AssertEquals("Invoice unposted should return true.", true, invoiceHelper.OldestUnpostedInvoiceExists(OrgWarehousePair.Create(client1.PK, whs2.PK)));
			AssertEquals("There are unpost and post invoice, should return true.", true, invoiceHelper.OldestUnpostedInvoiceExists(OrgWarehousePair.Create(client2.PK, whs1.PK)));
			AssertEquals("There are unpost and post invoice, should return true.", true, invoiceHelper.OldestUnpostedInvoiceExists(OrgWarehousePair.Create(client2.PK, whs2.PK)));
		}

		#endregion

		#region GetCandidateInvoices

		public void TestGetCandidateInvoices_OB_WhsAutoCreateAndRatePeriodicInvoice()
		{
			var client = Helper.CreateClient("CLIENT1");
			client.OH_IsDebtor = true;
			var companyData = client.CompanyData;
			companyData.OB_WhsAutoCreateAndRatePeriodicInvoice = false;

			var whs = Helper.CreateWarehouse("W1", "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var part = Helper.CreateProduct(client, "P1");

			var beforeInvoice = ZDateTimeOffset.Now.AddMonths(-13);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", beforeInvoice, part, 10m);

			var lastYear = ZDateTime.Now.AddYears(-1);
			Helper.CreateWhsInvoice(client, whs, lastYear, isPosted: false);

			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertGetCandidateInvoicesResult("No result expected.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true));
			AssertGetCandidateInvoicesResult("Expect to return result when not auto created.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false), (client.PK, whs.PK, false));

			companyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			Factory.Save();

			AssertGetCandidateInvoicesResult("Expect to return result when OB_WhsAutoCreateAndRatePeriodicInvoice set to true.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true), (client.PK, whs.PK, true));
			AssertGetCandidateInvoicesResult("Expect to return result when OB_WhsAutoCreateAndRatePeriodicInvoice set to true.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false), (client.PK, whs.PK, false));
		}

		void AssertGetCandidateInvoicesResult(string message, IEnumerable<OrgWarehousePair> queryResult, params (ZGuid orgPK, ZGuid warehousePK, bool hasPreInvoice)[] expectedReturns)
		{
			var result = queryResult.Select(e => new OrgWarehousePairExact(e.OrgPK, e.WarehousePK, e.HasInvoice)).ToArray();
			AssertEquals(message, expectedReturns.Length, result.Length);
			var erPairs = expectedReturns.Select(e => new OrgWarehousePairExact(e.orgPK, e.warehousePK, e.hasPreInvoice));
			AssertContainsExactElementsInAnyOrder(message, erPairs, result);
		}

		public void TestGetCandidateInvoices_HasActivity_Receive()
		{
			var client = Helper.CreateClient("CLIENT1");
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock = false;

			var whs = Helper.CreateWarehouse("W1", "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var part = Helper.CreateProduct(client, "P1");

			var lastYear = ZDateTime.Now.AddYears(-1);
			Helper.CreateWhsInvoice(client, whs, lastYear, isPosted: false);

			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertGetCandidateInvoicesResult("No result expected when there is no activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true));
			AssertGetCandidateInvoicesResult("No result expected when there is no activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false));

			var beforeInvoice = ZDateTimeOffset.Now.AddMonths(-13);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", beforeInvoice, part, 10m);
			Factory.Save();

			AssertGetCandidateInvoicesResult("Expect to return when there is activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true), (client.PK, whs.PK, true));
			AssertGetCandidateInvoicesResult("Expect to return when there is activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false), (client.PK, whs.PK, false));
		}

		[TestDate(2023, 2, 21, 12, 30, 20)]
		public void TestGetCandidateInvoices_HasActivity_ReturnOnlyIfNotCoveredByLastInvoice()
		{
			var client = Helper.CreateClient("CLIENT1");
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock = false;

			var whs = Helper.CreateWarehouse("W1", "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var part = Helper.CreateProduct(client, "P1");

			var yesterday = ZDateTime.Now.AddDays(-1);
			var invoice = Helper.CreateWhsInvoice(client, whs, yesterday.AddDays(-7).Date, yesterday.Date, isPosted: false);
			invoice.JobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertGetCandidateInvoicesResult("No result expected when there is no activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true));
			AssertGetCandidateInvoicesResult("No result expected when there is no activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false));

			// Covered By Last Invoice
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", yesterday.ToOffset(), part, 10m);
			Factory.Save();

			var order1 = Helper.CreateWhsOrderWithOrderLine(client, whs, "O1", part, 10m);
			Helper.CreatePickNew(order1);
			order1.FinaliseDocketAlwaysFinalisingPick();
			order1.WD_FinalisedDate = yesterday.ToOffset();
			Factory.Save();

			AssertGetCandidateInvoicesResult("Activity is conver by last invoice and does not need to return to create new one.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true));
			AssertGetCandidateInvoicesResult("Activity is conver by last invoice and does not need to return to create new one.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false));

			// Not Covered By Last Invoice
			Helper.CreateWhsReceiveWithInventory(client, whs, "R2", ZDateTimeOffset.Today, part, 10m);
			Factory.Save();

			var order2 = Helper.CreateWhsOrderWithOrderLine(client, whs, "O2", part, 10m);
			Helper.CreatePickNew(order2);
			order2.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();

			AssertGetCandidateInvoicesResult("Activity is not convered by last invoice and to return to create new invoice.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true), (client.PK, whs.PK, true));
			AssertGetCandidateInvoicesResult("Activity is not convered by last invoice and to return to create new invoice.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false), (client.PK, whs.PK, false));
		}

		public void TestGetCandidateInvoices_AllowAutoCreateInvoiceWithoutCurrentTransactionsOrStock()
		{
			var whs = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("CLIENT1");
			var part = Helper.CreateProduct(client, "P1");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock = false;
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", part, 10m);
			Factory.Save();

			var order = Helper.CreateWhsOrderWithOrderLine(client, whs, "O1", part, 10m);
			Helper.CreatePickNew(order);
			order.FinaliseDocketAlwaysFinalisingPick();
			Factory.Save();

			var lastYear = ZDateTime.Now.AddYears(-1);
			var invoice = Helper.CreateWhsInvoice(client, whs, lastYear, isPosted: false);
			var datetime = DateTime.Now;
			invoice.ET_StorageToDate = datetime.AddMonths(1);
			invoice.JobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			whs.WW_IsActive = false;
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertGetCandidateInvoicesResult("Should exclude future dockets.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true));

			client.CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock = true;
			Factory.Save();

			AssertGetCandidateInvoicesResult("It should use simple docket join and not checking date.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true), (client.PK, whs.PK, true));
		}

		public void TestGetCandidateInvoices_AllowAutoCreateInvoiceWithoutCurrentTransactionsOrStock_AutoCreate_ActivateClient()
		{
			TestGetCandidateInvoices_AllowAutoCreateInvoiceWithoutCurrentTransactionsOrStock_AutoCreateCore(clientIsActive: true, expectedResult: true);
		}

		public void TestGetCandidateInvoices_AllowAutoCreateInvoiceWithoutCurrentTransactionsOrStock_AutoCreateInactivateClient()
		{
			TestGetCandidateInvoices_AllowAutoCreateInvoiceWithoutCurrentTransactionsOrStock_AutoCreateCore(clientIsActive: false, expectedResult: false);
		}

		void TestGetCandidateInvoices_AllowAutoCreateInvoiceWithoutCurrentTransactionsOrStock_AutoCreateCore(bool clientIsActive, bool expectedResult)
		{
			var whs = Helper.CreateWarehouse("W1", "A");
			var client = Helper.CreateClient("CLIENT1");
			var part = Helper.CreateProduct(client, "P1");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock = true;
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", ZDateTimeOffset.Now.AddMonths(-20), part, 10m);
			Factory.Save();

			var lastYear = ZDateTime.Now.AddYears(-1);
			Helper.CreateWhsInvoice(client, whs, lastYear, isPosted: false);
			client.OH_IsActive = clientIsActive;
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			if (expectedResult)
			{
				AssertGetCandidateInvoicesResult("Expect to return when there is no activity in warehouse but AllowAutoCreateInvoiceWithoutCurrentTransactionsOrStock is true.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true), (client.PK, whs.PK, true));
			}
			else
			{
				AssertGetCandidateInvoicesResult("No result expected when when client is not active.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true));
			}
		}

		public void TestGetCandidateInvoices_AllowAutoCreateInvoiceWithoutCurrentTransactionsOrStock_HasDocket()
		{
			var client = Helper.CreateClient("CLIENT1");
			var part = Helper.CreateProduct(client, "P1");
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAllowCreateInvoiceWithNoTransactionsOrStock = true;

			var whs = Helper.CreateWarehouse("W1", "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var lastYear = ZDateTime.Now.AddYears(-1);
			Helper.CreateWhsInvoice(client, whs, lastYear, isPosted: false);

			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertGetCandidateInvoicesResult("No result expected when there is no docket.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true));
			AssertGetCandidateInvoicesResult("No result expected when there is no docket.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false));

			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", part, 10m);
			Factory.Save();

			AssertGetCandidateInvoicesResult("Expect to return when there is a docket in warehouse and AllowAutoCreateInvoiceWithoutCurrentTransactionsOrStock is true.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true), (client.PK, whs.PK, true));
			AssertGetCandidateInvoicesResult("Expect to return when there is a docket in warehouse and AllowAutoCreateInvoiceWithoutCurrentTransactionsOrStock is true.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false), (client.PK, whs.PK, false));
		}

		public void TestGetCandidateInvoices_AdHocServiceJob_HasActivity()
		{
			var client = Helper.CreateClient("CLIENT1");
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;

			var whs = Helper.CreateWarehouse("W1", "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var lastYear = ZDateTime.Now.AddYears(-1);
			Helper.CreateWhsInvoice(client, whs, lastYear, lastYear.AddMonths(11), isPosted: false);

			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertGetCandidateInvoicesResult("No result expected when there is no activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true));
			AssertGetCandidateInvoicesResult("No result expected when there is no activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false));

			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(whs, client, ZDateTime.Today);
			adhocServiceJob.WSJ_IsFinalised = true;
			AssertEquals("IsFinalised should be true", true, adhocServiceJob.IsFinalised);

			Factory.Save();

			AssertGetCandidateInvoicesResult("Expect to return when there is activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true), (client.PK, whs.PK, true));
			AssertGetCandidateInvoicesResult("Expect to return when there is activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false), (client.PK, whs.PK, false));
		}

		[TestDate(2023, 2, 21, 12, 30, 20)]
		public void TestGetCandidateInvoices_AdHocServiceJob_ReturnOnlyIfNotCoveredByLastInvoice()
		{
			var client = Helper.CreateClient("CLIENT1");
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;

			var whs = Helper.CreateWarehouse("W1", "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var yesterday = ZDate.Today.AddDays(-1);
			Helper.CreateWhsInvoice(client, whs, yesterday.AddDays(-7), yesterday, isPosted: false);

			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertGetCandidateInvoicesResult("No result expected when there is no activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true));
			AssertGetCandidateInvoicesResult("No result expected when there is no activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false));

			var adhocServiceJobCoveredByLastInvoice = Helper.CreateWhsAdHocServiceJob(whs, client, yesterday);
			adhocServiceJobCoveredByLastInvoice.WSJ_IsFinalised = true;
			AssertEquals("IsFinalised should be true", true, adhocServiceJobCoveredByLastInvoice.IsFinalised);

			Factory.Save();

			AssertGetCandidateInvoicesResult("Activity is conver by last invoice and does not need to return to create new one.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true));
			AssertGetCandidateInvoicesResult("Activity is conver by last invoice and does not need to return to create new one.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false));

			var adhocServiceJobNotCoveredByLastInvoice = Helper.CreateWhsAdHocServiceJob(whs, client, ZDate.Today);
			adhocServiceJobNotCoveredByLastInvoice.WSJ_IsFinalised = true;
			AssertEquals("IsFinalised should be true", true, adhocServiceJobNotCoveredByLastInvoice.IsFinalised);

			Factory.Save();

			AssertGetCandidateInvoicesResult("Activity is not convered by last invoice and to return to create new invoice.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true), (client.PK, whs.PK, true));
			AssertGetCandidateInvoicesResult("Activity is not convered by last invoice and to return to create new invoice.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false), (client.PK, whs.PK, false));
		}

		public void TestGetCandidateInvoices_AdHocServiceJob_AfterLastInvoice()
		{
			var client = Helper.CreateClient("CLIENT1");
			client.OH_IsDebtor = true;
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse($"WHS", address, branch);
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var lastYear = ZDateTime.Now.AddYears(-1);
			var invoice = Helper.CreateWhsInvoice(client, whs, lastYear, lastYear.AddMonths(11), isPosted: false);
			var secondJobHeader = invoice.JobHeader;
			secondJobHeader.JH_GC = branch.Company.PK;
			secondJobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertGetCandidateInvoicesResult("No result expected when there is no activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true));
			AssertGetCandidateInvoicesResult("No result expected when there is no activity in warehouse.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false));

			var beforeInvoice = ZDateTime.Now.AddMonths(-13);
			var adhocServiceJobBefore = Helper.CreateWhsAdHocServiceJob(whs, client, beforeInvoice);
			adhocServiceJobBefore.WSJ_IsFinalised = true;
			AssertEquals("IsFinalised should be true", true, adhocServiceJobBefore.IsFinalised);
			Factory.Save();

			AssertGetCandidateInvoicesResult("No result expected when it already invoiced.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true));
			AssertGetCandidateInvoicesResult("No result expected when it already invoiced.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false));

			var adhocServiceJobAfter = Helper.CreateWhsAdHocServiceJob(whs, client, ZDateTime.Today);
			adhocServiceJobAfter.WSJ_IsFinalised = true;
			AssertEquals("IsFinalised should be true", true, adhocServiceJobAfter.IsFinalised);
			Factory.Save();

			AssertGetCandidateInvoicesResult("Expect to return when there is activity in warehouse after last invoice.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true), (client.PK, whs.PK, true));
			AssertGetCandidateInvoicesResult("Expect to return when there is activity in warehouse after last invoice.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false), (client.PK, whs.PK, false));
		}

		public void TestGetCandidateInvoices_JobClosed()
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

			AssertEquals("Predondition", branch.Company.PK, client.CompanyData.Company.PK);
			AssertEquals("Predondition", Constants.StorageCalculationPeriods.Default, client.CompanyData.OB_ARWarehouseRatingPeriod);
			var notInvoicedDateYet = ZDateTime.Now.AddDays(-40); // more than one month
			var invoice = (IJobInvoicingPlugIn)Helper.CreateWhsInvoice(whs.PK, client.PK, notInvoicedDateYet.AddDays(-7), notInvoicedDateYet.AddDays(-1));
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertGetCandidateInvoicesResult("No result expected when we have invoice which is not closed.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true));
			AssertGetCandidateInvoicesResult("No result expected when we have invoice which is not closed.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false));

			var secondJobHeader = Helper.CreateAccountingDataWithNoCharge(invoice);
			secondJobHeader.JH_GC = branch.Company.PK;
			secondJobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();
			var beforeInvoice = ZDateTimeOffset.Now.AddMonths(-13);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", beforeInvoice, product, 10m);
			Factory.Save();

			AssertGetCandidateInvoicesResult("Expect to return result when existing invoice is closed.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true), (client.PK, whs.PK, true));
			AssertGetCandidateInvoicesResult("Expect to return result when existing invoice is closed.", invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: false), (client.PK, whs.PK, false));
		}

		public void TestGetCandidateInvoices()
		{
			var client1 = Helper.CreateClient("CLIENT1");
			var client2 = Helper.CreateClient("CLIENT2");
			client1.OH_IsDebtor = true;
			client2.OH_IsDebtor = true;
			var companyData = client1.CompanyData;
			companyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			var companyData2 = client2.CompanyData;
			companyData2.OB_WhsAutoCreateAndRatePeriodicInvoice = true;

			var whs1 = Helper.CreateWarehouse("W1", "A");
			var whs2 = Helper.CreateWarehouse("W2", "B");
			whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs2.WW_UseArrivalDateForInwardsFinalisedDate = true;

			var part = Helper.CreateProduct(client1, "P1");
			Helper.CreateProductClientRelationShip(client2, part);

			var beforeInvoice = ZDateTimeOffset.Now.AddMonths(-13);
			Helper.CreateWhsReceiveWithInventory(client1, whs1, "R1", beforeInvoice, part, 10m);
			Helper.CreateWhsReceiveWithInventory(client1, whs2, "R2", beforeInvoice, part, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, whs1, "R3", beforeInvoice, part, 10m);
			Helper.CreateWhsReceiveWithInventory(client2, whs2, "R4", beforeInvoice, part, 10m);

			var lastYear = ZDateTime.Now.AddYears(-1);
			// Client1 - Whs1 - posted invoices.
			Helper.CreateWhsInvoice(client1, whs1, lastYear, isPosted: true);

			// Client1 - Whs2 - posted invoices.
			Helper.CreateWhsInvoice(client1, whs2, lastYear, isPosted: true);

			// Client2 - Whs1 - two unposted invoices.
			var invoiceClient2Whs1UnPosted1 = Helper.CreateWhsInvoice(client2, whs1, lastYear, isPosted: true);
			var invoiceClient2Whs1UnPosted2 = Helper.CreateWhsInvoice(client2, whs1, lastYear.AddDays(7), isPosted: true);

			// Client2 - Whs2 - first is unposted invoice second is posted invoice.
			var invoiceClient2Whs2Posted = Helper.CreateWhsInvoice(client2, whs2, lastYear, isPosted: true);
			var invoiceClient2Whs2UnPosted = Helper.CreateWhsInvoice(client2, whs2, lastYear.AddDays(7), isPosted: true);

			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			AssertGetCandidateInvoicesResult("Expect to return 4 results.",
				invoiceHelper.GetCandidateInvoices(Factory, isAutoCreate: true),
				(client1.PK, whs1.PK, true),
				(client1.PK, whs2.PK, true),
				(client2.PK, whs1.PK, true),
				(client2.PK, whs2.PK, true)
				);
		}

		#endregion

		#region TestCreateInvoiceFromOrgWarehousePair

		public void TestCreateInvoiceFromOrgWarehousePair()
		{
			var client = Helper.CreateClient("C01");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse($"WHS", address, branch);
			Factory.Save();
			var invoiceHelper = new WhsInvoiceHelper();
			var clientWarehouse = OrgWarehousePair.Create(client.PK, whs.PK);
			var invoiceInFactory1 = invoiceHelper.CreateInvoiceFromOrgWarehousePair(clientWarehouse);
			var invoiceInFactory2 = invoiceHelper.CreateInvoiceFromOrgWarehousePair(clientWarehouse);

			AssertEquals(client.PK, invoiceInFactory1.Client.PK);
			AssertEquals(whs.PK, invoiceInFactory1.Warehouse.PK);

			AssertEquals(client.PK, invoiceInFactory2.Client.PK);
			AssertEquals(whs.PK, invoiceInFactory2.Warehouse.PK);

			AssertNotEquals(invoiceInFactory1.PK, invoiceInFactory2.PK);
		}

		#endregion

		#region TestCreateInvoiceFromOrgWarehousePair_JobHeaderCompanyAndBranchMatchWithInvoiceWarehouseBranch

		public void TestCreateInvoiceFromOrgWarehousePair_JobHeaderCompanyAndBranchMatchWithInvoiceWarehouseBranch()
		{
			var client = Helper.CreateClient("C01");
			var branch = Helper.CreateGlbBranch("Bra");
			var company = Factory.NewWithValidTestData<GlbCompany>();
			branch.GB_GC = company.PK;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse($"WHS", address, branch);
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			var clientWarehouse = OrgWarehousePair.Create(client.PK, whs.PK);

			WhsInvoice invoiceInFactory;
			using (WarehouseUserContextHelper.SetUserContextForWarehouse(whs))
			{
				invoiceInFactory = invoiceHelper.CreateInvoiceFromOrgWarehousePair(clientWarehouse);
			}

			AssertEquals(client.PK, invoiceInFactory.Client.PK);
			AssertEquals(whs.PK, invoiceInFactory.Warehouse.PK);
			AssertNotEquals("Precondition", company.PK, GlbCompany.CurrentCompany.PK);
			AssertNotEquals("Precondition", branch.PK, GlbBranch.CurrentBranch.PK);
			AssertNull("Warehouse branch is different from the current user context.", invoiceInFactory.JobHeader);

			using (WhsInvoiceHelper.SetUserContextForInvoice(invoiceInFactory))
			{
				AssertNotNull("Should be able to load job header when login as warehouse branch.", invoiceInFactory.JobHeader);
				AssertEquals("Company should match with warehouse branch company.", company.PK, invoiceInFactory.JobHeader.Company.PK);
				AssertEquals("Branch should match with warehouse branch.", branch.PK, invoiceInFactory.JobHeader.Branch.PK);
			}
		}

		#endregion

		#region TestGetInvoiceReferNumber

		public void TestGetInvoiceReferNumber()
		{
			var client = Helper.CreateClient("C01");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse($"WHS", address, branch);
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			Factory.Save();

			var invoiceHelper = ObjectFactory.Get<IWhsInvoiceHelper>();
			AssertEquals("[Warehouse Periodic Invoice I00000001]", invoiceHelper.GetInvoiceReferNumber(invoice));
		}

		#endregion

		#region SetupDataToBeAbleToChargeClientInInvoice

		void SetupDataToBeAbleToChargeClientInInvoice(OrgHeader client, WhsWarehouse whs, OrgSupplierPart part, int i, ZDate chargeDate)
		{
			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, $"R{i}", chargeDate.ToZDateTime().ToOffset(), part, 10m);
			Factory.Save();

			// Setting to generate charge when autorate invoice
			var receiveHandlingFumigationCharge = Helper.CreateChargeCode($"WRECFUM{i}", "Warehouse Receive Handling Fumigation", ChargeCodeGroupList.Codes.WHSInwards, Constants.FreightServiceType.Codes.Fumigation);
			var clientRate = Helper.CreateClientRate(client);
			var warehouseRate = Helper.CreateRateEntry(clientRate, chargeDate.AddMonths(-1), ZDate.Today);
			Helper.CreateRateLine(warehouseRate, receiveHandlingFumigationCharge, "SV", 5m);

			// any charges
			var fumigationService = receive.Services.AddNew();
			fumigationService.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigationService.ES_ServiceCount = 30m;
			fumigationService.ES_Completed = chargeDate;

			Factory.Save();
		}

		#endregion

		#region WhsTestHelperFunctionsInvoice

		protected new WhsTestHelperFunctionsInvoice Helper => helper ?? (helper = new WhsTestHelperFunctionsInvoice(Factory));
		WhsTestHelperFunctionsInvoice helper;

		#endregion

		#region TestIsNotLockByOtherProcess

		public void TestIsNotLockByOtherProcess()
		{
			var invoicePK = ZGuid.NewZGuid();
			var invoiceHelper = ObjectFactory.Get<IWhsInvoiceHelper>();
			AssertEquals("Should be able to lock if not taken.", true, invoiceHelper.IsNotLockByOtherProcess(invoicePK));

			using (var mutex = new ZGlobalMutex(MutexIDs.WhsInvoiceAutoRate, invoicePK.ToString()))
			{
				mutex.Lock();
				AssertEquals("Should not be able to lock.", false, invoiceHelper.IsNotLockByOtherProcess(invoicePK));
			}
		}

		#endregion

		#region TestPostInvoice

		[TestDate(2022, 7, 6)]
		public void TestPostInvoice()
		{
			var client = Helper.CreateClient("Apple");
			var branch = Helper.CreateGlbBranch("Bra");
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var whs = Helper.CreateWarehouse("WHS", address, branch);
			Helper.CreateRowAndGenerateLocations(whs, "A");
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			var product = Helper.CreateProduct("Product", client);
			Factory.Save();

			var yesterday = ZDate.Today.AddDays(-1);
			SetupDataToBeAbleToChargeClientInInvoice(client, whs, product, 1, yesterday);

			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			var logger1 = new TestAutoRatingServiceLogger();
			AssertExceptionThrown<InvalidOperationException>("When status isn't autorated, it shouldn't be called.", () => invoiceHelper.PostInvoice(logger1, invoice));

			var logger2 = new TestAutoRatingServiceLogger();
			invoice.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationAutorated;
			var isValid2 = invoiceHelper.PostInvoice(logger2, invoice);
			AssertMultilineASCIIEquals("Automatically Post Periodic Invoices is not enabled for client Apple warehouse WHS.", logger2.ToString());
			AssertEquals(true, isValid2);

			var logger3 = new TestAutoRatingServiceLogger();
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
			new JobHeader.Loader(invoice).TryLoadOrCreateWithoutMutexForTestOnly();
			var isValid3 = invoiceHelper.PostInvoice(logger3, invoice);
			AssertMultilineASCIIEquals("Error: Invoice for client Apple warehouse WHS could not be posted due to following errors: Please save this form before posting costs and/or charges.", logger3.ToString());
			AssertEquals(false, isValid3);

			Factory.Save();
			var logger4 = new TestAutoRatingServiceLogger();
			var isValid4 = invoiceHelper.PostInvoice(logger4, invoice);
			AssertMultilineASCIIEquals(@"Error: Invoice for client Apple warehouse WHS could not be posted due to following errors: There is no period set up for 06-Jul-22.
Please go to General Ledger >> Period Management to setup periods.", logger4.ToString());
			AssertEquals(false, isValid4);

			invoice.AutoRateJobHeader(null);
			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			Factory.Save();
			var logger5 = new TestAutoRatingServiceLogger();
			invoice.AddRowError("Error!");
			var isValid5 = invoiceHelper.PostInvoice(logger5, invoice);
			AssertMultilineASCIIEquals("Error: Invoice for client Apple warehouse WHS could not be posted due to following errors: Error - Warehouse Periodic Invoice I00000001: Error!", logger5.ToString());
			AssertEquals(false, isValid5);

			invoice.ClearAllNotifications();
			var logger6 = new TestAutoRatingServiceLogger();
			var isValid6 = invoiceHelper.PostInvoice(logger6, invoice);
			AssertMultilineASCIIEquals("Posted Invoice for client Apple warehouse WHS.", logger6.ToString());
			AssertEquals(true, isValid6);
		}

		#endregion

		#region TestDeliverInvoice

		public void TestDeliverInvoice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var testObjectCreator = new TestObjectCreator(Factory);
			var client = data.Org1;
			var debtor = Helper.CreateClient("debtor");
			var contact = debtor.Contacts.AddNew();
			contact.OC_ContactName = "me";
			contact.OC_Email = "me@wisetechglobal.com.au";
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Receivables.Code;
			document.OD_DefaultContact = true;
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			var jobHeader = new JobHeader.Loader(invoice).TryLoadOrCreate();
			var transactionHeader = testObjectCreator.CreateARInvoice<ARInvoice>("1", testObjectCreator.AUD, 1m, testObjectCreator.AALSHI);
			transactionHeader.AH_JH = jobHeader.PK;
			transactionHeader.AH_OH = debtor.PK;
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			var logger1 = new TestAutoRatingServiceLogger();
			var isValid1 = invoiceHelper.DeliverInvoice(logger1, invoice);
			AssertMultilineASCIIEquals("Automatically Deliver Periodic Invoices is not enabled for client 111 warehouse 1.", logger1.ToString());
			AssertEquals(true, isValid1);

			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoPostPeriodicInvoice = true;
			client.CompanyData.OB_WhsAutoDeliverPeriodicInvoice = false;
			AssertExceptionThrown<InvalidOperationException>("When status isn't posted, it shouldn't be called.", () => invoiceHelper.DeliverInvoice(new TestAutoRatingServiceLogger(), invoice));

			var logger2 = new TestAutoRatingServiceLogger();
			invoice.BillingAutomationStatus = BillingAutomationStatusCodes.BillingAutomationPosted;
			data.Org1.CompanyData.OB_WhsAutoDeliverPeriodicInvoice = false;
			var isValid2 = invoiceHelper.DeliverInvoice(logger2, invoice);
			AssertMultilineASCIIEquals("Automatically Deliver Periodic Invoices is not enabled for client 111 warehouse 1.", logger2.ToString());
			AssertEquals(true, isValid2);

			client.CompanyData.OB_WhsAutoDeliverPeriodicInvoice = true;
			invoice.ClearAllNotifications();
			var logger3 = new TestAutoRatingServiceLogger();
			var isValid3 = invoiceHelper.DeliverInvoice(logger3, invoice);
			AssertMultilineASCIIEquals("Delivered Invoice for client 111 warehouse 1.", logger3.ToString());
			AssertEquals(true, isValid3);
			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("PrintJob should have been created.", 1, printJobs.Length);
		}

		#endregion

		#region TestCloseInvoice

		public void TestCloseInvoice()
		{
			var client = Helper.CreateClient("Apple");
			var whs = Helper.CreateWarehouse("WHS", "A");
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			invoice.AutoRateJobHeader(null);
			Factory.Save();

			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = false;
			var invoiceHelper = new WhsInvoiceHelper();
			var statusCalled = false;
			invoice.JobHeader.JH_StatusInfo.ValueChanged += (object s, EventArgs e) => statusCalled = true;
			var logger1 = new TestAutoRatingServiceLogger();
			var closedApplied1 = invoiceHelper.CloseBillingAndRelatedJob(logger1, invoice);
			AssertEquals("Should not change status, When OB_WhsAutoCreateAndRatePeriodicInvoice is false.", false, statusCalled);
			AssertNotEquals("Should not change status, When OB_WhsAutoCreateAndRatePeriodicInvoice is false.", JobHeaderStatus.Closed.Code, invoice.JobHeader.JH_Status);
			AssertEquals("Should not change status, When OB_WhsAutoCreateAndRatePeriodicInvoice is false.", false, closedApplied1);
			AssertMultilineASCIIEquals("", logger1.ToString());

			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			invoice.JobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			statusCalled = false;
			var logger2 = new TestAutoRatingServiceLogger();
			var closedApplied2 = invoiceHelper.CloseBillingAndRelatedJob(logger2, invoice);
			AssertEquals("Should not change status, When is already closed.", false, statusCalled);
			AssertEquals("Should not change status, When is already closed.", false, closedApplied2);
			AssertMultilineASCIIEquals("", logger2.ToString());

			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			invoice.JobHeader.JH_Status = JobHeaderStatus.Working.Code;
			statusCalled = false;
			var logger3 = new TestAutoRatingServiceLogger();
			var closedApplied3 = invoiceHelper.CloseBillingAndRelatedJob(logger3, invoice);
			AssertEquals("Should update status.", true, statusCalled);
			AssertEquals("Should update status.", true, closedApplied3);
			AssertEquals("Should update status.", JobHeaderStatus.Closed.Code, invoice.JobHeader.JH_Status);
			AssertMultilineASCIIEquals("Periodic Billing job status for client Apple warehouse WHS was successfully closed.", logger3.ToString());
			AssertEquals("Should check job header error.", true, invoice.VerifyJobHeaderNotificationErrors);
			DeleteJobHeadersWithoutAnyCharges(invoice);
		}

		public void TestCloseInvoice_NoJobHeader()
		{
			var client = Helper.CreateClient("Apple");
			var whs = Helper.CreateWarehouse("WHS", "A");
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			var logger = new TestAutoRatingServiceLogger();
			AssertExceptionThrown<InvalidOperationException>("shouldn't be called when there is no job to close.", () => invoiceHelper.CloseBillingAndRelatedJob(new TestAutoRatingServiceLogger(), invoice));

			invoice.AutoRateJobHeader(null);
			invoice.JobHeader.JH_Status = JobHeaderStatus.Working.Code;
			var closedApplied = invoiceHelper.CloseBillingAndRelatedJob(logger, invoice);
			AssertEquals("Should update status.", true, closedApplied);
			AssertEquals("Should update status.", JobHeaderStatus.Closed.Code, invoice.JobHeader.JH_Status);
			AssertMultilineASCIIEquals("Periodic Billing job status for client Apple warehouse WHS was successfully closed.", logger.ToString());
			AssertEquals("Should check job header error.", true, invoice.VerifyJobHeaderNotificationErrors);
			DeleteJobHeadersWithoutAnyCharges(invoice);
		}

		public void TestCloseInvoice_RunValidation()
		{
			var client = Helper.CreateClient("Apple");
			client.CompanyData.OB_WhsAutoCreateAndRatePeriodicInvoice = true;
			var whs = Helper.CreateWarehouse("WHS", "A");
			Factory.Save();

			var invoiceHelper = new WhsInvoiceHelper();
			var logger1 = new TestAutoRatingServiceLogger();
			var invoice1 = CreateValidInvoice();
			invoice1.ET_StorageToDate = ZDateTime.Now.AddDays(-10); //Make invoice invalid, end date before start date
			var closedApplied1 = invoiceHelper.CloseBillingAndRelatedJob(logger1, invoice1);
			AssertEquals("Should not update status, When is not valid.", false, closedApplied1);
			AssertMultilineASCIIEquals(@"Error: Invoice for client Apple warehouse WHS could not be closed. it must be manually closed due to following errors: Error - ET_StorageFromDate: To Date cannot be before From Date.
Error - ET_StorageToDate: To Date cannot be before From Date.
Error - ET_StorageToDate: The client's storage calculation period is weekly, but the specified 'To' date does not fall on a week boundary (i.e. 7 days).", logger1.ToString());
			DeleteJobHeadersWithoutAnyCharges(invoice1);
			var logger2 = new TestAutoRatingServiceLogger();
			var invoice2 = CreateValidInvoice();
			var closedApplied2 = invoiceHelper.CloseBillingAndRelatedJob(logger2, invoice2);
			AssertEquals("Should update status.", true, closedApplied2);
			AssertEquals("Should update status.", JobHeaderStatus.Closed.Code, invoice2.JobHeader.JH_Status);
			AssertMultilineASCIIEquals("Periodic Billing job status for client Apple warehouse WHS was successfully closed.", logger2.ToString());
			AssertEquals("Should check job header error.", true, invoice2.VerifyJobHeaderNotificationErrors);
			DeleteJobHeadersWithoutAnyCharges(invoice2);
			WhsInvoice CreateValidInvoice()
			{
				var invoice = (WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Now.AddDays(-7), ZDateTime.Now);
				invoice.AutoRateJobHeader(null);
				invoice.JobHeader.JH_Status = JobHeaderStatus.Working.Code;
				return invoice;
			}
		}

		#endregion

		void DeleteJobHeadersWithoutAnyCharges(WhsInvoice invoice)
		{
			if (invoice.JobHeader != null && !invoice.JobHeader.IsInDatabase && !(invoice.JobHeader.Charges.Count > 0))
			{
				invoice.JobHeader.Delete();
			}
		}

		#region OrgWarehousePairExact

		class OrgWarehousePairExact : OrgWarehousePair
		{
			public OrgWarehousePairExact(ZGuid orgPK, ZGuid warehousePK, ZBool hasPreInvoice)
			{
				OrgPK = orgPK;
				WarehousePK = warehousePK;
				HasInvoice = hasPreInvoice;
			}
			public override bool Equals(object obj) => obj != null && obj is OrgWarehousePairExact otherPair && otherPair == this;
			public static bool operator ==(OrgWarehousePairExact x, OrgWarehousePairExact y) => x.OrgPK == y.OrgPK && x.WarehousePK == y.WarehousePK && x.HasInvoice == y.HasInvoice;
			public static bool operator !=(OrgWarehousePairExact x, OrgWarehousePairExact y) => !(x == y);
			public override int GetHashCode() => OrgPK.GetHashCode() ^ WarehousePK.GetHashCode() ^ HasInvoice.GetHashCode();
		}

		#endregion
	}

	#region TestAutoRatingServiceLogger

	[Serializable]
	public class TestAutoRatingServiceLogger : SimpleLogger, IAutoRatingServiceLogger
	{
		public void Error(string message) => Log(LogType.Error, message);

		public void Information(string message) => Log(LogType.Information, message);

		public IDisposable EnableAddingNoteWhileLogging(ZGuid invoicePK) => null;

		public string OneOffStartInvoiceLog { set => throw new NotImplementedException("Not used!"); }
	}

	#endregion
}
