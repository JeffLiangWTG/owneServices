using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MarketingManager.ServiceTask.Testing
{
	[TestedType(typeof(SalesTradeLanesSynchronisationTask))]
	class SalesTradeLanesSynchronisationTaskTest : ServiceTaskTestCase<SalesTradeLanesSynchronisationTask>
	{
		[TestDate(1971, 9, 18)]
		[TestDateIncremental(0, 0, 0, 1)]
		public void TestBatches_SilentErrorReported()
		{
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.RunJCDServiceTaskForTableCreation(Db.Connection);
			Db.Connection.ExecuteNonQuery("update dbo.orgheader set OH_IsSalesLead = 0");
			var activeOrgs = Enumerable.Range(0, 5).Select(i => GetNewOrgToSync(true)).ToList();
			var inactiveOrgs = Enumerable.Range(0, 1).Select(i => GetNewOrgToSync(false)).ToList();

			Factory.Save();

			var task = new SalesTradeLanesSynchronisationTaskThrowsExceptionForTest();
			task.ExpectedOrgs = activeOrgs.Select(org => org.PK).ToList();

			var logger = InitialiseAndRunTaskScheduleWithAnyBranchContext(task);

			AssertEquals("Error should have been reported.", "Test", ErrorReporter.LastMessageReported);
			AssertEquals("2 Exception Reports should have been reported", 2, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		[TestDate(2016, 10, 10)]
		public void TestShouldNotRunIfJCDServiceTaskNotRun_HasWarning()
		{
			var task = new SalesTradeLanesSynchronisationTaskForTest();
			var logger = InitialiseAndRunTaskScheduleWithAnyBranchContext(task);

			AssertEquals("Information|Sales Trade Lanes Synchronization Task started.", logger[0]);
			AssertEquals("Warning|Before you can synchronize sales values, you must finish processing all Transaction Lines through the Job Costing Data Queue Service Task (https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseOneUpdateNote20180319d.pdf).", logger[1]);
		}

		[TestDate(2016, 10, 10)]
		[TestDateIncremental(0, 0, 0, 1)]
		public void TestBatches()
		{
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.RunJCDServiceTaskForTableCreation(Db.Connection);
			Db.Connection.ExecuteNonQuery("update dbo.orgheader set OH_IsSalesLead = 0");
			var activeOrgs = Enumerable.Range(0, 5).Select(i => GetNewOrgToSync(true)).ToList();
			var inactiveOrgs = Enumerable.Range(0, 1).Select(i => GetNewOrgToSync(false)).ToList();
			OrganisationRegistry.Instance.FullTradeLanesSyncFromDate = new DateTime(2006, 10, 10);

			Factory.Save();

			var task = new SalesTradeLanesSynchronisationTaskForTest();
			task.ExpectedOrgs = activeOrgs.Select(org => org.PK).ToList();

			var logger = InitialiseAndRunTaskScheduleWithAnyBranchContext(task);

			AssertEquals("Information|Sales Trade Lanes Synchronization Task started.", logger[0]);
			AssertEquals("Debug|Building cache (From:01-Oct-2016 To:01-Nov-2016)", logger[1]);
			AssertEquals("Debug|Customs Brokerage Cache built", logger[2]);
			AssertEquals("Debug|Port Transport Cache built", logger[3]);
			AssertEquals("Debug|Shipments Cache built", logger[4]);
			AssertEquals("Debug|Warehouse Cache built", logger[5]);
			AssertEquals("Debug|Quotations Cache built", logger[6]);
			AssertEquals("Debug|Total organizations to Full sync: 5", logger[7]);
			AssertEquals("Debug|Processed and saved 2 Organizations", logger[8]);
			AssertEquals("Debug|Processed and saved 4 Organizations", logger[9]);
			AssertEquals("Debug|Processed and saved 5 Organizations", logger[10]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Oct-2016 To:01-Nov-2016   Synchronized organizations: 5   Skipped organizations: 0", logger[11]);
			AssertEquals("Debug|Building cache (From:01-Sep-2016 To:01-Oct-2016)", logger[12]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Sep-2016 To:01-Oct-2016   Synchronized organizations: 0   Skipped organizations: 0", logger[18]);
			AssertEquals("Debug|Building cache (From:01-Oct-2006 To:01-Nov-2006)", logger[845]);
			AssertEquals("Information|Sales Trade Lanes Synchronization Task finished.", logger[852]);
			AssertEquals(853, logger.Count);

			var factory = new BusinessObjectFactory();
			AssertSynced(activeOrgs, factory, "ALL 201610");
			AssertNotSynced(inactiveOrgs, factory);
			AssertEquals("Should have synced 10 years", new DateTime(2006, 10, 1), OrganisationRegistry.Instance.FullTradeLanesSyncLatestSyncMonth.Value);

			var newActiveOrgs = Enumerable.Range(0, 5).Select(i => GetNewOrgToSync(true)).ToList();
			Factory.Save();

			task = new SalesTradeLanesSynchronisationTaskForTest();
			task.ExpectedOrgs = activeOrgs.Select(org => org.PK).Union(newActiveOrgs.Select(org => org.PK)).ToList();

			logger = InitialiseAndRunTaskScheduleWithAnyBranchContext(task);

			AssertEquals("Information|Sales Trade Lanes Synchronization Task started.", logger[0]);
			AssertEquals("Debug|Building cache (From:01-Oct-2016 To:01-Nov-2016)", logger[1]);
			AssertEquals("Debug|Customs Brokerage Cache built", logger[2]);
			AssertEquals("Debug|Port Transport Cache built", logger[3]);
			AssertEquals("Debug|Shipments Cache built", logger[4]);
			AssertEquals("Debug|Warehouse Cache built", logger[5]);
			AssertEquals("Debug|Quotations Cache built", logger[6]);
			AssertEquals("Debug|Total organizations to Partial sync: 10", logger[7]);
			AssertEquals("Debug|Processed and saved 2 Organizations", logger[8]);
			AssertEquals("Debug|Processed and saved 4 Organizations", logger[9]);
			AssertEquals("Debug|Processed and saved 6 Organizations", logger[10]);
			AssertEquals("Debug|Processed and saved 8 Organizations", logger[11]);
			AssertEquals("Debug|Processed and saved 10 Organizations", logger[12]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Oct-2016 To:01-Nov-2016   Synchronized organizations: 10   Skipped organizations: 0", logger[13]);
			AssertEquals("Debug|Building cache (From:01-Sep-2016 To:01-Oct-2016)", logger[14]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Sep-2016 To:01-Oct-2016   Synchronized organizations: 0   Skipped organizations: 0", logger[20]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Jul-2016 To:01-Aug-2016   Synchronized organizations: 0   Skipped organizations: 0", logger[34]);
			AssertEquals("Information|Sales Trade Lanes Synchronization Task finished.", logger[35]);
			AssertEquals(36, logger.Count);

			factory = new BusinessObjectFactory();
			AssertSynced(activeOrgs, factory, "PARTIAL 201610");
			AssertSynced(newActiveOrgs, factory, "PARTIAL 201610");
			AssertNotSynced(inactiveOrgs, factory);
			AssertEquals("Should not have changed", new DateTime(2006, 10, 1), OrganisationRegistry.Instance.FullTradeLanesSyncLatestSyncMonth.Value);
		}

		[TestDate(2016, 10, 10)]
		public void TestYearly()
		{
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.RunJCDServiceTaskForTableCreation(Db.Connection);
			OrganisationRegistry.Instance.ShouldDoFullTradeLanesSyncIfRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Db.Connection.ExecuteNonQuery("update dbo.orgheader set OH_IsSalesLead = 0");
			var activeOrgs = Enumerable.Range(0, 5).Select(i => GetNewOrgToSync(true)).ToList();
			var inactiveOrgs = Enumerable.Range(0, 1).Select(i => GetNewOrgToSync(false)).ToList();

			Factory.Save();

			var task = new SalesTradeLanesSynchronisationTaskForTest();
			task.ExpectedOrgs = activeOrgs.Select(org => org.PK).ToList();

			var config = task.Config;
			config.SyncYearly = true;
			SystemDataRegistry.Instance.LastYearlySyncTLS.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.ToDateTime().AddMonths(-5));
			task.ConfigString = config.ConfigString;

			var logger = InitialiseAndRunTaskScheduleWithAnyBranchContext(task);

			AssertEquals("Information|Sales Trade Lanes Synchronization Task started.", logger[0]);
			AssertEquals("Debug|Building cache (From:01-Oct-2016 To:01-Nov-2016)", logger[1]);
			AssertEquals("Debug|Customs Brokerage Cache built", logger[2]);
			AssertEquals("Debug|Port Transport Cache built", logger[3]);
			AssertEquals("Debug|Shipments Cache built", logger[4]);
			AssertEquals("Debug|Warehouse Cache built", logger[5]);
			AssertEquals("Debug|Quotations Cache built", logger[6]);
			AssertEquals("Debug|Total organizations to Partial sync: 5", logger[7]);
			AssertEquals("Debug|Processed and saved 2 Organizations", logger[8]);
			AssertEquals("Debug|Processed and saved 4 Organizations", logger[9]);
			AssertEquals("Debug|Processed and saved 5 Organizations", logger[10]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Oct-2016 To:01-Nov-2016   Synchronized organizations: 5   Skipped organizations: 0", logger[11]);
			AssertEquals("Debug|Building cache (From:01-Sep-2016 To:01-Oct-2016)", logger[12]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Sep-2016 To:01-Oct-2016   Synchronized organizations: 0   Skipped organizations: 0", logger[18]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Oct-2015 To:01-Nov-2015   Synchronized organizations: 0   Skipped organizations: 0", logger[95]);
			AssertEquals("Information|Sales Trade Lanes Synchronization Task finished.", logger[96]);
			AssertEquals(97, logger.Count);

			var factory = new BusinessObjectFactory();
			AssertSynced(activeOrgs, factory, "PARTIAL 201610");
			AssertNotSynced(inactiveOrgs, factory);
			AssertEquals("Should not have changed", DateTime.MinValue, OrganisationRegistry.Instance.FullTradeLanesSyncLatestSyncMonth.Value);

			AssertEquals(ZDateTime.UtcToday, SystemDataRegistry.Instance.LastYearlySyncTLS.Value.Date);
		}

		public void TestDeleteOrganisationsDuringTaskRun()
		{
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.RunJCDServiceTaskForTableCreation(Db.Connection);
			Db.Connection.ExecuteNonQuery("update dbo.orgheader set OH_IsSalesLead = 0");
			var activeOrgs = Enumerable.Range(0, 5).Select(i => GetNewOrgToSync(true)).ToList();
			var deletedOrgs = Enumerable.Range(0, 2).Select(i => GetNewOrgToSync(true)).ToList();

			Factory.Save();

			var task = new SalesTradeLanesSynchronisationTaskDeletesOrgForTest(TestConnection);
			task.OrgsToDelete = deletedOrgs.Select(org => org.PK).ToList();

			var logger = InitialiseAndRunTaskScheduleWithAnyBranchContext(task);

			AssertEquals("No Exception Reports should have been reported", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			AssertOrganisationDebugInLog(logger, deletedOrgs[0].PK);
			AssertOrganisationDebugInLog(logger, deletedOrgs[1].PK);
		}

		[TestDate(2022, 1, 10)]
		public void TestDeleteObsoleteWhenNoCache()
		{
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.RunJCDServiceTaskForTableCreation(Db.Connection);
			OrganisationRegistry.Instance.ShouldDoFullTradeLanesSyncIfRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			// Ensuring OrgMiscServ records are created here as current branch is accessed by OrgMiscServ.SetDefaultValues which would result in a developer error during service task run
			_ = org1.MiscServ;
			_ = org2.MiscServ;
			_ = org3.MiscServ;

			var actualSales = org1.SalesCollection.AddNew();
			actualSales.OW_IsTraded = true;
			actualSales.OW_MP_Product = product.PK;
			actualSales.OW_OriginID = CountryGuids.Instance.Germany;
			actualSales.OW_OriginTableCode = RefCountrySchema.Constants.Prefix;
			actualSales.OW_DestinationID = CountryGuids.Instance.France;
			actualSales.OW_DestinationTableCode = RefCountrySchema.Constants.Prefix;
			actualSales.OW_OH_Buyer = org2.PK;
			actualSales.OW_OH_Supplier = org3.PK;

			var detail = actualSales.TradeDetails.AddNew();
			var period = detail.TradedPeriods.AddNew();
			period.PAS_Period = new ZDate(2022, 1, 1);
			period.PAS_LastTraded = new ZDate(2022, 1, 1);
			period.PAS_OH_Client = org1.PK;

			Factory.Save();

			var task = new SalesTradeLanesSynchronisationTask();

			var logger = InitialiseAndRunTaskScheduleWithAnyBranchContext(task);

			AssertEquals("Information|Sales Trade Lanes Synchronization Task started.", logger[0]);
			AssertEquals("Debug|Building cache (From:01-Jan-2022 To:01-Feb-2022)", logger[1]);
			AssertEquals("Debug|Customs Brokerage Cache built", logger[2]);
			AssertEquals("Debug|Port Transport Cache built", logger[3]);
			AssertEquals("Debug|Shipments Cache built", logger[4]);
			AssertEquals("Debug|Warehouse Cache built", logger[5]);
			AssertEquals("Debug|Quotations Cache built", logger[6]);
			AssertEquals("Debug|Total organizations to Partial sync: 3", logger[7]);

			var query = new ZDBOnlyQuery(typeof(OrgTradePeriod));
			query.AddToFilter(OrgTradePeriodSchema.PK, period.PK);
			var reloadPeriod = Factory.LoadTop1<OrgTradePeriod>(query);

			AssertNull("Trade Period row deleted", reloadPeriod);
		}

		// No nudging: TLS service task is a resource intensive process which targets multiple tables so should only be run once a day
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void AssertOrganisationDebugInLog(TestServiceLogger logger, ZGuid orgPK)
		{
			var found = false;
			for (int count = 0; count < logger.Count; count++)
			{
				if (logger[count].StartsWith($"Debug|Organization, PK {orgPK}"))
				{
					found = true;
					break;
				}
			}
			Assert("Organisation not found warning log exists", found);
		}

		static void AssertNotSynced(IEnumerable<OrgHeader> orgs, BusinessObjectFactory factory)
		{
			foreach (var inactiveOrg in orgs)
			{
				var reloadedOrg = factory.Load<OrgHeader>(inactiveOrg.PK);
				var log = reloadedOrg.Logs.MostRecentLogByEventTime(Events.SalesTradeLanesSynchronised);
				AssertNull("Logs should not exist for inactive orgs", log);
			}
		}

		static void AssertSynced(IEnumerable<OrgHeader> orgs, BusinessObjectFactory factory, string reference)
		{
			foreach (var activeOrg in orgs)
			{
				var reloadedOrg = factory.Load<OrgHeader>(activeOrg.PK);
				var log = reloadedOrg.Logs.MostRecentLogByEventTime(Events.SalesTradeLanesSynchronised);
				AssertNotNull("Logs should exist for active orgs", log);
				AssertStartsWith(string.Format("Should have been a '{0}' sync", reference), reference, log.SL_Reference);
			}
		}

		OrgHeader GetNewOrgToSync(bool active, OrgHeader withReferenceToOrg = null)
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = Guid.NewGuid().ToString().Replace("-", "").Substring(0, OrgHeaderSchema.OH_Code.MaxLength - 1);
			org.OH_IsActive = active;
			org.MainAddress.OA_Address1 = "test address";
			org.OH_IsSalesLead = true;

			AddNewQuoteForSync(org);

			return org;
		}

		void AddNewQuoteForSync(OrgHeader org, string origin = "AUSYD", OrgHeader consignee = null)
		{
			var newQuote = Factory.New(ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().QuoteType);
			newQuote[RatingHeaderSchema.Constants.TH_OH] = org.PK;

			var collectionHelper = new OrgSalesCollectionTestHelper(org.SalesCollection, Factory);
			collectionHelper.AddRateEntry(newQuote, "AIR", origin, "USLAX", consignee != null ? consignee.PK : ZGuid.Empty, ZGuid.Empty);
		}

		[TestDate(2016, 10, 10)]
		public void TestTaskRun()
		{
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.RunJCDServiceTaskForTableCreation(Db.Connection);
			OrganisationRegistry.Instance.FullTradeLanesSyncFromDate = new DateTime(2006, 10, 1);
			var org1 = GetNewOrgToSync(true);
			var org2 = GetNewOrgToSync(true);
			var org3 = GetNewOrgToSync(true);
			var org4 = GetNewOrgToSync(true);
			org4.OH_IsSalesLead = false;

			Factory.Save();

			SalesTradeLanesSynchronisationTask task = new SalesTradeLanesSynchronisationTask();
			TestServiceLogger log = InitialiseAndRunTaskScheduleWithAnyBranchContext(task);

			AssertEquals("Information|Sales Trade Lanes Synchronization Task started.", log[0]);
			AssertEquals("Debug|Building cache (From:01-Oct-2016 To:01-Nov-2016)", log[1]);
			AssertEquals("Debug|Customs Brokerage Cache built", log[2]);
			AssertEquals("Debug|Port Transport Cache built", log[3]);
			AssertEquals("Debug|Shipments Cache built", log[4]);
			AssertEquals("Debug|Warehouse Cache built", log[5]);
			AssertEquals("Debug|Quotations Cache built", log[6]);
			AssertEquals("Debug|Total organizations to Full sync: 4", log[7]);
			AssertEquals("Debug|Processed and saved 4 Organizations", log[8]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Oct-2016 To:01-Nov-2016   Synchronized organizations: 4   Skipped organizations: 0", log[9]);
			AssertEquals("Debug|Building cache (From:01-Sep-2016 To:01-Oct-2016)", log[10]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Sep-2016 To:01-Oct-2016   Synchronized organizations: 0   Skipped organizations: 0", log[16]);
			AssertEquals("Debug|Building cache (From:01-Oct-2006 To:01-Nov-2006)", log[843]);
			AssertEquals("Information|Sales Trade Lanes Synchronization Task finished.", log[850]);
			AssertEquals(851, log.Count);
			AssertEquals("Should have synced 10 years", new DateTime(2006, 10, 1), OrganisationRegistry.Instance.FullTradeLanesSyncLatestSyncMonth.Value);

			AssertEquals(false, org1.HasChanges);
			AssertEquals(false, org2.HasChanges);
			AssertEquals(false, org3.HasChanges);
			AssertEquals(false, org4.HasChanges);
		}

		[TestDate(2015, 9, 12)]
		public void TestContinueUnfinishedRun()
		{
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.RunJCDServiceTaskForTableCreation(Db.Connection);
			var lastActiveOrg = new List<OrgHeader> { GetNewOrgToSync(true) };
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2016, 10, 10);
			var activeOrgs = Enumerable.Range(0, 5).Select(i => GetNewOrgToSync(true)).ToList();
			OrganisationRegistry.Instance.FullTradeLanesSyncFromDate = new DateTime(2015, 10, 1);

			Factory.Save();

			var task = new SalesTradeLanesSynchronisationTaskForTest { ExpectedOrgs = activeOrgs.Select(org => org.PK).ToList() };

			var logger = InitialiseAndRunTaskScheduleWithAnyBranchContext(task);

			AssertEquals("Information|Sales Trade Lanes Synchronization Task started.", logger[0]);
			AssertEquals("Debug|Building cache (From:01-Oct-2016 To:01-Nov-2016)", logger[1]);
			AssertEquals("Debug|Customs Brokerage Cache built", logger[2]);
			AssertEquals("Debug|Port Transport Cache built", logger[3]);
			AssertEquals("Debug|Shipments Cache built", logger[4]);
			AssertEquals("Debug|Warehouse Cache built", logger[5]);
			AssertEquals("Debug|Quotations Cache built", logger[6]);
			AssertEquals("Debug|Total organizations to Full sync: 5", logger[7]);
			AssertEquals("Debug|Processed and saved 2 Organizations", logger[8]);
			AssertEquals("Debug|Processed and saved 4 Organizations", logger[9]);
			AssertEquals("Debug|Processed and saved 5 Organizations", logger[10]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Oct-2016 To:01-Nov-2016   Synchronized organizations: 5   Skipped organizations: 0", logger[11]);
			AssertEquals("Debug|Building cache (From:01-Oct-2015 To:01-Nov-2015)", logger[89]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Oct-2015 To:01-Nov-2015   Synchronized organizations: 0   Skipped organizations: 0", logger[95]);
			AssertEquals("Information|Sales Trade Lanes Synchronization Task finished.", logger[96]);
			AssertEquals(97, logger.Count);

			var factory = new BusinessObjectFactory();
			AssertSynced(activeOrgs, factory, "ALL 201610");
			AssertEquals("Should have synced 1 year", new DateTime(2015, 10, 1), OrganisationRegistry.Instance.FullTradeLanesSyncLatestSyncMonth.Value);
			AssertEquals("Should no longer require full sync", false, OrganisationRegistry.Instance.ShouldDoFullTradeLanesSyncIfRequired.Value);

			OrganisationRegistry.Instance.FullTradeLanesSyncFromDate = new DateTime(2014, 10, 2);
			OrganisationRegistry.Instance.ShouldDoFullTradeLanesSyncIfRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			task = new SalesTradeLanesSynchronisationTaskForTest { ExpectedOrgs = lastActiveOrg.Select(org => org.PK).ToList() };

			logger = InitialiseAndRunTaskScheduleWithAnyBranchContext(task);

			AssertEquals("Information|Sales Trade Lanes Synchronization Task started.", logger[0]);
			AssertEquals("Debug|Building cache (From:01-Sep-2015 To:01-Oct-2015)", logger[1]);
			AssertEquals("Debug|Customs Brokerage Cache built", logger[2]);
			AssertEquals("Debug|Port Transport Cache built", logger[3]);
			AssertEquals("Debug|Shipments Cache built", logger[4]);
			AssertEquals("Debug|Warehouse Cache built", logger[5]);
			AssertEquals("Debug|Quotations Cache built", logger[6]);
			AssertEquals("Debug|Total organizations to Full sync: 1", logger[7]);
			AssertEquals("Debug|Processed and saved 1 Organizations", logger[8]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Sep-2015 To:01-Oct-2015   Synchronized organizations: 1   Skipped organizations: 0", logger[9]);
			AssertEquals("Debug|Building cache (From:01-Oct-2014 To:01-Nov-2014)", logger[80]);
			AssertEquals("Debug|Synchronization Date Range: From:01-Oct-2014 To:01-Nov-2014   Synchronized organizations: 0   Skipped organizations: 0", logger[86]);
			AssertEquals("Information|Sales Trade Lanes Synchronization Task finished.", logger[87]);
			AssertEquals(88, logger.Count);

			factory = new BusinessObjectFactory();
			AssertSynced(lastActiveOrg, factory, "ALL 201509");
			AssertEquals("Should have synced the next year", new DateTime(2014, 10, 1), OrganisationRegistry.Instance.FullTradeLanesSyncLatestSyncMonth.Value);
			AssertEquals("Should no longer require full sync", false, OrganisationRegistry.Instance.ShouldDoFullTradeLanesSyncIfRequired.Value);
		}

		[TestDate(2021, 9, 12)]
		public void TestStopServiceTaskRunWhenBuildCacheFails()
		{
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.RunJCDServiceTaskForTableCreation(Db.Connection);
			var lastActiveOrg = new List<OrgHeader> { GetNewOrgToSync(true) };
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 10, 10);
			var activeOrgs = Enumerable.Range(0, 1).Select(i => GetNewOrgToSync(true)).ToList();
			OrganisationRegistry.Instance.FullTradeLanesSyncFromDate = new DateTime(2021, 10, 1);
			Factory.Save();

			var task = new SalesTradeLanesSynchronisationTaskThrowsCacheSqlExceptionForTest { ExpectedOrgs = activeOrgs.Select(org => org.PK).ToList(), ThrowOnBuildCache = true };
			var logger = InitialiseAndRunTaskScheduleWithAnyBranchContext(task);

			AssertEquals("Information|Sales Trade Lanes Synchronization Task started.", logger[0]);
			AssertEquals("Debug|Building cache (From:01-Oct-2022 To:01-Nov-2022)", logger[1]);
			AssertEquals("Warning|The database connection was dropped. This run has been terminated.", logger[2]);
			AssertEquals("Information|Sales Trade Lanes Synchronization Task finished.", logger[3]);
			AssertEquals(4, logger.Count);
		}

		[TestDate(2021, 9, 12)]
		public void TestStopServiceTaskRunWhenSyncInBatchesFails()
		{
			var jcdServiceTaskHelper = ObjectFactory.Get<IJCDServiceTaskTestHelper>();
			jcdServiceTaskHelper.RunJCDServiceTaskForTableCreation(Db.Connection);
			var lastActiveOrg = new List<OrgHeader> { GetNewOrgToSync(true) };
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2022, 10, 10);
			var activeOrgs = Enumerable.Range(0, 1).Select(i => GetNewOrgToSync(true)).ToList();
			OrganisationRegistry.Instance.FullTradeLanesSyncFromDate = new DateTime(2021, 10, 1);
			Factory.Save();

			var task = new SalesTradeLanesSynchronisationTaskThrowsCacheSqlExceptionForTest { ExpectedOrgs = activeOrgs.Select(org => org.PK).ToList(), ThrowOnCreateTradeLinesSynchroniser = true };
			var logger = InitialiseAndRunTaskScheduleWithAnyBranchContext(task);

			AssertEquals("Information|Sales Trade Lanes Synchronization Task started.", logger[0]);
			AssertEquals("Debug|Building cache (From:01-Oct-2022 To:01-Nov-2022)", logger[1]);
			AssertEquals("Debug|Customs Brokerage Cache built", logger[2]);
			AssertEquals("Debug|Port Transport Cache built", logger[3]);
			AssertEquals("Debug|Shipments Cache built", logger[4]);
			AssertEquals("Debug|Warehouse Cache built", logger[5]);
			AssertEquals("Debug|Quotations Cache built", logger[6]);
			AssertEquals("Debug|Total organizations to Full sync: 1", logger[7]);
			AssertEquals("Warning|The database connection was dropped. This run has been terminated.", logger[8]);
			AssertEquals("Information|Sales Trade Lanes Synchronization Task finished.", logger[9]);
			AssertEquals(10, logger.Count);
		}

		[TestDate(2016, 10, 10)]
		public void TestTaskRunForUnMatchedOrg()
		{
			var orgAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, OrgHeader.UnmatchedOrganisationPK));
			var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_OA_Address = orgAddress.PK;

			Factory.Save();

			SalesTradeLanesSynchronisationTask task = new SalesTradeLanesSynchronisationTaskForUnMatchOrgTest();
			var log = InitialiseAndRunTaskScheduleWithAnyBranchContext(task);
			for (int i = 0; i < log.Count; i++)
			{
				Assert("SystemDefined org should not be synchronised", !log[i].Contains("SystemDefined org should not be synchronised"));
			}
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "TLS", hostedServiceAttribute.Code);
				AssertEquals("Description", "Sales Trade Lanes Synchronization Task", hostedServiceAttribute.Description);
				AssertEquals("Category", "SAL", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "1day", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		TestServiceLogger InitialiseAndRunTaskScheduleWithAnyBranchContext(SalesTradeLanesSynchronisationTask serviceTask)
		{
			var log = InitialiseTaskSchedule(serviceTask);
			using (EnvProxy.Instance.TemporaryServiceTaskContext("TLS", canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}
			return log;
		}

		#region Test Classes

		class SalesTradeLanesSynchronisationTaskThrowsExceptionForTest : SalesTradeLanesSynchronisationTask
		{
			public List<ZGuid> ExpectedOrgs { get; set; }

			protected override void ThrowError_ForTesting()
			{
				if (ZDate.Today == ZDate.BrettsBirthday)
				{
					throw new Exception("Test");
				}
			}
		}

		class SalesTradeLanesSynchronisationTaskThrowsCacheSqlExceptionForTest : SalesTradeLanesSynchronisationTask
		{
			public List<ZGuid> ExpectedOrgs { get; set; }

			public bool ThrowOnBuildCache;

			public bool ThrowOnCreateTradeLinesSynchroniser;

			protected override void BuildCache(CachedTradeLinesSummaryProvider tradeLinesSummaryProvider)
			{
				if (ThrowOnBuildCache)
				{
					throw SqlExceptionBuilder.CreateSqlException(208, $"Invalid object name '{TradeLinesSummaryProviderCommon.TradeLineCacheTableName}'.");
				}

				base.BuildCache(tradeLinesSummaryProvider);
			}

			protected override TradeLinesSynchroniser CreateTradeLinesSynchroniser(ZGuid orgPk, bool isFullSync)
			{
				if (ThrowOnCreateTradeLinesSynchroniser)
				{
					throw SqlExceptionBuilder.CreateSqlException(208, $"Invalid object name '{TradeLinesSummaryProviderCommon.TradeLineCacheTableName}'.");
				}

				return base.CreateTradeLinesSynchroniser(orgPk, isFullSync);
			}
		}

		class SalesTradeLanesSynchronisationTaskDeletesOrgForTest : SalesTradeLanesSynchronisationTask
		{
			public SalesTradeLanesSynchronisationTaskDeletesOrgForTest(DbConnection connection) : base()
			{
				this.Connection = connection;
			}

			protected DbConnection Connection;

			public List<ZGuid> OrgsToDelete { get; set; }

			protected override void DeleteOrgs_ForTesting()
			{
				using (EnvProxy.Instance.SuspendBranchAccessError())
				{
					var factory = new BusinessObjectFactory(Connection);
					foreach (var pk in OrgsToDelete)
					{
						var org = factory.Load<OrgHeader>(pk);
						Connection.ExecuteNonQuery($"DELETE dbo.JobDocAddress WHERE E2_OA_Address = '{org.Addresses.First().PK}'");
						org.Delete();
					}
					factory.Save();
				}
			}
		}

		class SalesTradeLanesSynchronisationTaskForTest : SalesTradeLanesSynchronisationTask
		{
			protected override int BatchSize
			{
				get { return 2; }
			}

			public List<ZGuid> ExpectedOrgs
			{
				get { return expectedOrgs; }
				set
				{
					expectedOrgs = value;
					expectedOrgsInBatch = new List<ZGuid>(expectedOrgs);
				}
			}
			List<ZGuid> expectedOrgs;

			List<ZGuid> expectedOrgsInBatch;

			protected override void BatchComplete_ForTesting(OrgHeader[] batch)
			{
				Assert("Should be 1 or 2 orgs in a batch, but was " + batch.Length, batch.Length == 2 || batch.Length == 1);

				if (expectedOrgsInBatch != null)
				{
					foreach (var org in batch)
					{
						Assert("Processed Org was not expected", expectedOrgsInBatch.Contains(org.PK));
						expectedOrgsInBatch.Remove(org.PK);
					}
				}
			}

			protected override void ResetExpectedOrgsInBatch_ForTesting()
			{
				expectedOrgsInBatch = new List<ZGuid>(expectedOrgs);
			}
		}

		class SalesTradeLanesSynchronisationTaskForUnMatchOrgTest : SalesTradeLanesSynchronisationTask
		{
			protected override void BatchComplete_ForTesting(OrgHeader[] batch)
			{
				var unmatchedOrg = batch.Where(x => x.PK.Equals(OrgHeader.UnmatchedOrganisationPK)).First();
				if (unmatchedOrg.Logs.GetAllLogs().Cast<StmALog>().Any(x => x.SL_SE_NKEvent == Events.SalesTradeLanesSynchronised.Code))
				{
					ServiceLogger.Log(LogType.Error, "SystemDefined org should not be synchronised");
				}
			}
		}

		#endregion
	}
}
