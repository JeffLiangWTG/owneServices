using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Module.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(InvoicingFilterBusinessObject))]
	internal class InvoicingFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		// Use the Asserter when adding new tests, and CONVERT ANY TEST YOU EDIT.

		#region Filters

		#region Filter Warehouse

		public virtual void TestFilterWarehouse()
		{
			FilterBizObj = new InvoicingFilterBusinessObject();
			Factory.New<WhsWarehouse>();
			AssertNotNull(FilterBizObj.Warehouses);
			AssertEquals(typeof(WhsWarehouseCollectionWithSecurityCheck), FilterBizObj.Warehouses.GetType());
			AssertEquals("Collection should not be loaded", 0, FilterBizObj.Warehouses.Count);
		}

		public virtual void TestFilterWarehouseVisibility()
		{
			FilterBizObj = new InvoicingFilterBusinessObject();
			AssertEquals("Precondition: WhsAllowedWarehouses", true, Env.Security.WhsAllowedWarehouses.IsAllowed);
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBizObj["Warehouse"];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);

			Env.Security.WhsAllowedWarehouses.IsAllowed = false;
			try
			{
				InvoicingFilterBusinessObject invoiceFilter1 = new InvoicingFilterBusinessObject();
				ModuleGuidFilter filter1 = (ModuleGuidFilter)invoiceFilter1["Warehouse"];
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter1.Visibility);

				Globals.IsWeb = true;
				try
				{
					InvoicingFilterBusinessObject invoiceFilter2 = new InvoicingFilterBusinessObject();
					ModuleGuidFilter filter2 = (ModuleGuidFilter)invoiceFilter2["Warehouse"];
					AssertEquals("Visibility", FilterVisibility.Visible, filter2.Visibility);
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
			finally
			{
				Env.Security.WhsAllowedWarehouses.IsAllowed = true;
			}
		}

		#endregion

		#region Filter Clients

		public void TestFilterClients()
		{
			FilterBizObj = new InvoicingFilterBusinessObject();
			Factory.New<OrgHeader>().OH_IsWarehouseClient = true;
			AssertNotNull(FilterBizObj.Clients);
			AssertEquals(typeof(WarehouseClientCollectionWithSecurityCheck), FilterBizObj.Clients.GetType());
			AssertEquals("Collection should not be loaded", 0, FilterBizObj.Clients.Count);
		}

		public virtual void TestFilterClientVisibility()
		{
			FilterBizObj = new InvoicingFilterBusinessObject();
			AssertEquals("Precondition: WhsAllowedClients", true, Env.Security.WhsAllowedClients.IsAllowed);
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBizObj["Client"];
			AssertEquals("Visibility", FilterVisibility.Visible, filter.Visibility);

			Env.Security.WhsAllowedClients.IsAllowed = false;
			try
			{
				InvoicingFilterBusinessObject invoiceFilter1 = new InvoicingFilterBusinessObject();
				ModuleGuidFilter filter1 = (ModuleGuidFilter)invoiceFilter1["Client"];
				AssertEquals("Visibility", FilterVisibility.AlwaysVisible, filter1.Visibility);

				Globals.IsWeb = true;
				try
				{
					InvoicingFilterBusinessObject invoiceFilter2 = new InvoicingFilterBusinessObject();
					ModuleGuidFilter filter2 = (ModuleGuidFilter)invoiceFilter2["Client"];
					AssertEquals("Visibility", FilterVisibility.Visible, filter2.Visibility);
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
			finally
			{
				Env.Security.WhsAllowedClients.IsAllowed = true;
			}
		}

		#endregion

		#region Filter NoJobHeader

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestNoJobHeader()
		{
			var filterBizObj = new InvoicingFilterBusinessObject();
			var invoiceCollection = new WhsInvoiceCollection(Factory);

			var company1 = GlbCompany.CurrentCompany;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var client = Helper.CreateClient("Client1");
			var warehouse = Helper.CreateWarehouse("Warehouse1");
			var today = ZDateTime.Today;

			var invoice1 = CreatePeriodicInvoice(invoiceCollection, company1, client, warehouse, today.AddDays(-10), addJobHeader: true);
			var invoice2 = CreatePeriodicInvoice(invoiceCollection, company1, client, warehouse, today.AddDays(-20), addJobHeader: false);
			var invoice3 = CreatePeriodicInvoice(invoiceCollection, company2, client, warehouse, today.AddDays(-30), addJobHeader: true);
			var invoice4 = CreatePeriodicInvoice(invoiceCollection, company2, client, warehouse, today.AddDays(-40), addJobHeader: false);
			Factory.Save();

			var filter = (ModuleFlagsFilter)filterBizObj["Missing Job Header"];
			filter.IsActive = true;
			AssertEquals("The default value should be True", true, filter.Property0);

			invoiceCollection.Load(filterBizObj.Filter);
			AssertEquals(2, invoiceCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { invoice2, invoice4 }, invoiceCollection);

			filter.Property0 = ZBool.False;
			invoiceCollection.Load(filterBizObj.Filter);
			AssertEquals(1, invoiceCollection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { invoice1 }, invoiceCollection);
		}

		WhsInvoice CreatePeriodicInvoice(WhsInvoiceCollection invoiceCollection, GlbCompany company, OrgHeader client, WhsWarehouse warehouse, ZDateTime fromDate, bool addJobHeader)
		{
			var invoice = invoiceCollection.AddNew();
			invoice.ET_OH_Client = client.PK;
			invoice.ET_WW = warehouse.PK;
			invoice.ET_StorageFromDate = fromDate;
			invoice.ET_StorageToDate = fromDate.AddDays(7);
			invoice.ET_BillingDate = fromDate.AddDays(8);

			if (addJobHeader)
			{
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
				jobHeader.JH_ParentID = invoice.PK;
				jobHeader.JH_ParentTableCode = JobStorageSchema.Constants.Prefix;
				jobHeader.JH_GC = company.PK;

				var aRHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
				aRHeader.AH_JH = jobHeader.PK;
			}

			return invoice;
		}

		#endregion

		#region Filter Status

		public void TestFilterStatus()
		{
			SetupData();

			Invoice1.JobHeader.JH_Status = JobHeaderStatus.Working.Code;
			Invoice2.JobHeader.JH_Status = JobHeaderStatus.WorkOnHold.Code;
			Invoice3.JobHeader.JH_Status = JobHeaderStatus.JobInvoiced.Code;
			Invoice4.JobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			var filter = (ModuleTextFilter)FilterBizObj["Status"];
			Asserter.AssertMatches("Filter is empty, expect all Invoices.", filter, Invoice1, Invoice2, Invoice3, Invoice4);

			filter.Property = JobHeaderStatus.Working.Code;
			Asserter.AssertMatches(string.Format("Filter is {0}, expect only Invoice1.", JobHeaderStatus.Working.Code), filter, Invoice1);

			filter.Property = JobHeaderStatus.WorkOnHold.Code;
			Asserter.AssertMatches(string.Format("Filter is {0}, expect only Invoice2.", JobHeaderStatus.WorkOnHold.Code), filter, Invoice2);

			filter.Property = JobHeaderStatus.JobInvoiced.Code;
			Asserter.AssertMatches(string.Format("Filter is {0}, expect only Invoice3.", JobHeaderStatus.JobInvoiced.Code), filter, Invoice3);

			filter.Property = JobHeaderStatus.Closed.Code;
			Asserter.AssertMatches(string.Format("Filter is {0}, expect only Invoice4.", JobHeaderStatus.Closed.Code), filter, Invoice4);

			filter.Property = JobHeaderStatus.Complete.Code;
			Asserter.AssertMatches(string.Format("Filter is {0}, expect no Invoices.", JobHeaderStatus.Complete.Code), filter);
		}

		public void TestFilterStatusList()
		{
			var filterBizObj = new InvoicingFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizObj["Status"];
			var expectedStatusList = new JobHeaderStatusList();
			expectedStatusList.AddPair("OPN", "Open");
			AssertContainsExactElementsInAnyOrder(expectedStatusList, filter.List);
		}

		#endregion

		#region Storage Off Band Processing Status

		public void TestFilterOffBandProcessingStatus()
		{
			SetupData();

			Invoice1.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			Invoice2.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.ERR;
			Invoice3.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.NIQ;
			// Invoice4 by default set to NIQ;
			Factory.Save();

			var filterBizObj = new InvoicingFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizObj["ET_OffBandProcessingStatus"];
			Asserter.AssertMatches("Filter is empty, NIQ is always applied if not specified.", filter, Invoice3, Invoice4);

			filter.Property = StorageOffBandProcessingStatus.Codes.QUE;
			Asserter.AssertMatches(string.Format("Filter is {0}, expect only Invoice1.", StorageOffBandProcessingStatus.Codes.QUE), filter, Invoice1);

			filter.Property = StorageOffBandProcessingStatus.Codes.ERR;
			Asserter.AssertMatches(string.Format("Filter is {0}, expect only Invoice2.", StorageOffBandProcessingStatus.Codes.ERR), filter, Invoice2);

			filter.Property = StorageOffBandProcessingStatus.Codes.NIQ;
			Asserter.AssertMatches(string.Format("Filter is {0}, expect Invoice3 and Invoice4.", StorageOffBandProcessingStatus.Codes.NIQ), filter, Invoice3, Invoice4);
		}
		public void TestFilterOffBandProcessingStatus_Defaults()
		{
			var filterBizObj = new InvoicingFilterBusinessObject();
			var filter = (ModuleTextFilter)filterBizObj["ET_OffBandProcessingStatus"];
			AssertEquals("Default value.", "NIQ", filter.Property);
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			AssertEquals(SQLComparisonOperator.Equal, filter.SqlComparisonOperator);
			AssertEquals(JobStorageSchema.ET_OffBandProcessingStatus, filter.FilterColumn);
			AssertEquals("Should always applied by default.", FilterVisibility.AlwaysApplied | FilterVisibility.AlwaysVisible, filter.Visibility);
			AssertContainsExactElementsInAnyOrder(new[] { ModuleTextFilter.ComparisonConstants.Exact, ModuleTextFilter.ComparisonConstants.NotEqual }, filter.AllowedComparisonOperators);
		}

		#endregion

		#region OtherFilters

		public void TestOtherFilters()
		{
			var year = ZDateTime.Now.Year;
			SetupData();
			Invoice3.ET_WW = ZGuid.Empty;

			Factory.Save();

			FilterBizObj.ET_OH_Client = Client1.PK;
			InvoiceCollection.Load(FilterBizObj.Filter);
			AssertEquals(2, InvoiceCollection.Count);
			Assert(InvoiceCollection.Contains(Invoice1));
			Assert(InvoiceCollection.Contains(Invoice2));

			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "Billing Date", new ZDateTime(year, 1, 2), new ZDateTime(year, 1, 11));
			InvoiceCollection.Load(FilterBizObj.Filter);
			AssertEquals(1, InvoiceCollection.Count);
			Assert(InvoiceCollection.Contains(Invoice1));
			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "Billing Date");

			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "From Date", new ZDateTime(year, 1, 2), new ZDateTime(year, 1, 11));
			InvoiceCollection.Load(FilterBizObj.Filter);
			AssertEquals(1, InvoiceCollection.Count);
			Assert(InvoiceCollection.Contains(Invoice2));
			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "From Date");

			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "To Date", new ZDateTime(year, 1, 2), new ZDateTime(year, 1, 11));
			InvoiceCollection.Load(FilterBizObj.Filter);
			AssertEquals(1, InvoiceCollection.Count);
			Assert(InvoiceCollection.Contains(Invoice1));
			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "To Date");

			FilterBizObj.ET_OH_Client = ZGuid.Empty;
			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "Invoice Number", (ZString)"3");
			InvoiceCollection.Load(FilterBizObj.Filter);
			AssertEquals(1, InvoiceCollection.Count);
			Assert(InvoiceCollection.Contains(Invoice3));
			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "Invoice Number");

			InvoiceCollection.Load(FilterBizObj.Filter);
			AssertEquals(4, InvoiceCollection.Count);

			FilterBizObj.ET_WW = Warehouse1.PK;
			InvoiceCollection.Load(FilterBizObj.Filter);
			AssertEquals(1, InvoiceCollection.Count);
			Assert(InvoiceCollection.Contains(Invoice1));

			FilterBizObj.ET_WW = Warehouse2.PK;
			InvoiceCollection.Load(FilterBizObj.Filter);
			AssertEquals(2, InvoiceCollection.Count);
			Assert(InvoiceCollection.Contains(Invoice2));
			Assert(InvoiceCollection.Contains(Invoice4));
		}

		#endregion

		#region BillingFiltersTest

		public void TestAPInvoiceNumberFilter()
		{
			SetupData();
			Factory.Save();

			AssertNotNull(FilterBizObj["AP Invoice #"]);

			var job = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, Invoice1.PK).AddToFilter(JobHeaderSchema.JH_GC, Env.CurrentCompany.PK));

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "00001001";

			var newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = "00001001";
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";

			var newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "AP Invoice #", (ZString)"00001001");
			InvoicingAssert(true, false, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "AP Invoice #", (ZString)"00001002");
			InvoicingAssert(false, false, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "AP Invoice #", (ZString)"");
			InvoicingAssert(true, true, true, true);
		}

		public void TestARTransactionNumberFilter()
		{
			SetupData();
			Factory.Save();

			AssertNotNull(FilterBizObj["AR Transaction #"]);

			var job = Factory.LoadTop1<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, Invoice1.PK).AddToFilter(JobHeaderSchema.JH_GC, Env.CurrentCompany.PK));

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "00001001";

			var newInvoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			newInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			newInvoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
			newInvoice.AH_OH = org.PK;
			newInvoice.AH_TransactionNum = "00001001";
			newInvoice.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			newInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			newInvoice.AH_ConsolidatedInvoiceRef = "S00009999";

			var newInvoiceLine = Factory.NewWithValidTestData<AccTransactionLines>();
			newInvoiceLine.AL_AH = newInvoice.PK;
			newInvoiceLine.AL_JH = job.PK;
			newInvoiceLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "AR Transaction #", (ZString)"00001001");
			InvoicingAssert(true, false, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "AR Transaction #", (ZString)"00001002");
			InvoicingAssert(false, false, false, false);

			FilterBusinessObjectTestHelper.SetFilterProperty(FilterBizObj, "AR Transaction #", (ZString)"");
			InvoicingAssert(true, true, true, true);
		}

		#endregion

		public void TestProfitLossReasonFilterWithOperators()
		{
			var filterBizObj = new InvoicingFilterBusinessObject();
			var invoiceCollection = new WhsInvoiceCollection(Factory);

			var company1 = GlbCompany.CurrentCompany;

			var client = Helper.CreateClient("Client1");
			var warehouse = Helper.CreateWarehouse("Warehouse1");
			var today = ZDateTime.Today;

			var invoice1 = CreatePeriodicInvoice(invoiceCollection, company1, client, warehouse, today.AddDays(-10), addJobHeader: true);
			var invoice2 = CreatePeriodicInvoice(invoiceCollection, company1, client, warehouse, today.AddDays(-20), addJobHeader: true);
			var invoice3 = CreatePeriodicInvoice(invoiceCollection, company1, client, warehouse, today.AddDays(-30), addJobHeader: true);

			invoice1.JobHeader.JH_ProfitLossReasonCode = "ND1";
			invoice2.JobHeader.JH_ProfitLossReasonCode = "CD1";
			invoice3.JobHeader.JH_ProfitLossReasonCode = string.Empty;
			Factory.Save();

			var profitLossReasonFilter = (ModuleTextFilter)filterBizObj["Profit/Loss Reason"];
			profitLossReasonFilter.IsActive = true;

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			profitLossReasonFilter.Property = "ND1";

			invoiceCollection.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { invoice1 }, invoiceCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			profitLossReasonFilter.Property = "N";
			invoiceCollection.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { invoice1 }, invoiceCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			profitLossReasonFilter.Property = "D";
			invoiceCollection.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { invoice1, invoice2 }, invoiceCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			profitLossReasonFilter.Property = "N";
			invoiceCollection.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { invoice2, invoice3 }, invoiceCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			profitLossReasonFilter.Property = "N";
			invoiceCollection.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { invoice2, invoice3 }, invoiceCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			profitLossReasonFilter.Property = "ND1";
			invoiceCollection.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { invoice2, invoice3 }, invoiceCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			profitLossReasonFilter.Property = "ND1";
			invoiceCollection.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { invoice3 }, invoiceCollection);

			profitLossReasonFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			profitLossReasonFilter.Property = "ND1";
			invoiceCollection.Load(filterBizObj.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { invoice1, invoice2 }, invoiceCollection);
		}

		#endregion

		#region Properties

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestFilter()
		{
			SetupData();

			InvoiceCollection.Load(FilterBizObj.Filter);
			AssertEquals(4, InvoiceCollection.Count);
			AssertCollectionContains(Invoice1, InvoiceCollection);
			AssertCollectionContains(Invoice2, InvoiceCollection);
			AssertCollectionContains(Invoice3, InvoiceCollection);
			AssertCollectionContains(Invoice4, InvoiceCollection);

			GlbCompany glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			GlbCompany glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			Invoice1.JobHeader.JH_GC = glbCompany1.PK;
			Invoice2.JobHeader.JH_GC = glbCompany2.PK;

			Factory.Save();

			InvoiceCollection.Load(FilterBizObj.Filter);
			AssertEquals(2, InvoiceCollection.Count);
			AssertCollectionContains(Invoice3, InvoiceCollection);
			AssertCollectionContains(Invoice4, InvoiceCollection);
		}

		#endregion

		#region SetupData

		void SetupData()
		{
			var year = ZDateTime.Now.Year;
			FilterBizObj = new InvoicingFilterBusinessObject();
			InvoiceCollection = new WhsInvoiceCollection(Factory);

			Client1 = Helper.CreateClient("Client1");
			Client2 = Helper.CreateClient("Client2");

			Warehouse1 = Helper.CreateWarehouse("Warehouse1");
			Warehouse2 = Helper.CreateWarehouse("Warehouse2");

			Invoice1 = InvoiceCollection.AddNew();
			Invoice1.ET_OH_Client = Client1.PK;
			Invoice1.ET_WW = Warehouse1.PK;
			Invoice1.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			Invoice1.ET_StorageToDate = new ZDateTime(year, 1, 10);
			Invoice1.ET_BillingDate = new ZDateTime(year, 1, 11);

			Invoice2 = InvoiceCollection.AddNew();
			Invoice2.ET_OH_Client = Client1.PK;
			Invoice2.ET_WW = Warehouse2.PK;
			Invoice2.ET_StorageFromDate = new ZDateTime(year, 1, 11);
			Invoice2.ET_StorageToDate = new ZDateTime(year, 1, 20);
			Invoice2.ET_BillingDate = new ZDateTime(year, 1, 21);

			Invoice3 = InvoiceCollection.AddNew();
			Invoice3.ET_OH_Client = Client2.PK;
			Invoice3.ET_WW = Warehouse1.PK;
			Invoice3.ET_StorageFromDate = new ZDateTime(year, 1, 1);
			Invoice3.ET_StorageToDate = new ZDateTime(year, 1, 15);
			Invoice3.ET_BillingDate = new ZDateTime(year, 1, 16);

			Invoice4 = InvoiceCollection.AddNew();
			Invoice4.ET_OH_Client = Client2.PK;
			Invoice4.ET_WW = Warehouse2.PK;
			Invoice4.ET_StorageFromDate = new ZDateTime(year, 1, 16);
			Invoice4.ET_StorageToDate = new ZDateTime(year, 1, 30);
			Invoice4.ET_BillingDate = new ZDateTime(year, 2, 1);

			Helper.CreateAccountingDataWithNoCharge(Invoice1);
			Helper.CreateAccountingDataWithNoCharge(Invoice2);
			Helper.CreateAccountingDataWithNoCharge(Invoice3);
			Helper.CreateAccountingDataWithNoCharge(Invoice4);

			Asserter.AddToScope(Invoice1);
			Asserter.AddToScope(Invoice2);
			Asserter.AddToScope(Invoice3);
			Asserter.AddToScope(Invoice4);

			Factory.Save();
		}

		#endregion

		#region Implementation

		FilterStripAsserter<WhsInvoice> Asserter
		{
			get { return asserter ?? (asserter = new FilterStripAsserter<WhsInvoice>(Factory, i => i.ET_StorageJobNumber)); }
		}

		/// <summary>
		/// Obsolete. Use the Asserter instead.
		/// </summary>
		void InvoicingAssert(bool i11, bool i12, bool i21, bool i22)
		{
			InvoiceCollection.Load(FilterBizObj.Filter);
			AssertEquals("Invoice11", i11, InvoiceCollection.Contains(Invoice1.PK));
			AssertEquals("Invoice12", i12, InvoiceCollection.Contains(Invoice2.PK));
			AssertEquals("Invoice21", i21, InvoiceCollection.Contains(Invoice3.PK));
			AssertEquals("Invoice22", i22, InvoiceCollection.Contains(Invoice4.PK));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new InvoicingFilterBusinessObject();
		}

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		InvoicingFilterBusinessObject FilterBizObj;
		WhsInvoiceCollection InvoiceCollection;

		OrgHeader Client1;
		OrgHeader Client2;
		WhsWarehouse Warehouse1;
		WhsWarehouse Warehouse2;
		WhsInvoice Invoice1;
		WhsInvoice Invoice2;
		WhsInvoice Invoice3;
		WhsInvoice Invoice4;

		WhsTestHelperFunctions helper;
		FilterStripAsserter<WhsInvoice> asserter;

		#endregion
	}

	#region TestAccountingFilter

	public class InvoicingFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<WhsInvoice>
	{
		protected override WhsInvoice GetNewBusinessObjectForFilterCollection()
		{
			return Factory.NewWithValidTestData<WhsInvoice>();
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get { return ModuleIDs.WhsInvoicing; }
		}

		protected override bool ShouldUseBillingFilters
		{
			get { return false; }
		}
	}

	#endregion
}
