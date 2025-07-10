using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	internal class WhsInvoiceValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckET_WW

		public void TestCheckET_WW()
		{
			Invoice.ET_WW = Factory.New<WhsWarehouse>().PK;
			AssertNoErrors(Invoice.ET_WWInfo);

			Invoice.Warehouse.WW_IsActive = false;
			Invoice.Validation.ValidateET_WW();
			AssertHasError(Invoice.ET_WWInfo, WhsInvoiceValidation.ErrorMessages.CannotSelectInactiveWarehouse);

			Invoice.ET_WW = ZGuid.Empty;
			AssertHasErrors(Invoice.ET_WWInfo);
		}

		#endregion

		#region TestCheckET_StorageFromDate

		public void TestCheckET_StorageFromDate()
		{
			Invoice.IncludeInInvoicing = false;
			Invoice.ET_StorageFromDate = ZDateTime.Today.AddDays(1);
			AssertNoErrors(Invoice.ET_StorageFromDateInfo);
			Invoice.IncludeInInvoicing = true;
			AssertHasErrors(Invoice.ET_StorageFromDateInfo);
			AssertEquals(WhsInvoiceValidation.ErrorMessages.FromDateInFuture, Invoice.ET_StorageFromDateInfo.GetErrors().GetFirstMessage());

			Invoice.IncludeInInvoicing = false;
			Invoice.ET_StorageFromDate = ZDateTime.Today.AddMonths(-1);
			Invoice.ET_StorageToDate = ZDateTime.Today.AddMonths(-2);
			AssertNoErrors(Invoice.ET_StorageFromDateInfo);
			Invoice.IncludeInInvoicing = true;
			AssertHasErrors(Invoice.ET_StorageFromDateInfo);
			AssertEquals(WhsInvoiceValidation.ErrorMessages.ToDateBeforeFromDate, Invoice.ET_StorageFromDateInfo.GetErrors().GetFirstMessage());

			Invoice.ET_StorageFromDate = ZDateTime.Empty;
			AssertHasErrors(Invoice.ET_StorageFromDateInfo);
			AssertEquals("Please enter a From Date.", Invoice.ET_StorageFromDateInfo.GetErrors().GetFirstMessage());

			Invoice.ET_StorageFromDate = ZDateTime.Today.AddMonths(-3);
			AssertNoErrors(Invoice.ET_StorageFromDateInfo);
		}

		#endregion

		#region TestCheckET_StorageToDate

		public void TestCheckET_StorageToDate()
		{
			Invoice.IncludeInInvoicing = false;
			Invoice.ET_StorageToDate = ZDateTime.Today.AddDays(1);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);
			Invoice.IncludeInInvoicing = true;
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.IncludeInInvoicing = false;
			Invoice.ET_StorageFromDate = ZDateTime.Today.AddMonths(-1);
			Invoice.ET_StorageToDate = ZDateTime.Today.AddMonths(-2);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);
			Invoice.IncludeInInvoicing = true;
			AssertHasErrors(Invoice.ET_StorageToDateInfo);
			AssertEquals(WhsInvoiceValidation.ErrorMessages.ToDateBeforeFromDate, Invoice.ET_StorageToDateInfo.GetErrors().GetFirstMessage());

			Invoice.ET_StorageToDate = ZDateTime.Empty;
			AssertHasErrors(Invoice.ET_StorageToDateInfo);
			AssertEquals("Please enter a To Date.", Invoice.ET_StorageToDateInfo.GetErrors().GetFirstMessage());

			Invoice.ET_StorageToDate = ZDateTime.Today;
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			// Day/Week/Month period validation tested in WhsInvoice
		}

		#endregion

		#region TestCheckET_StorageToDateValidatedAgainstClientStoragePeriod

		public void TestCheckET_StorageToDateValidatedAgainstClientStoragePeriod()
		{
			var year = ZDateTime.Now.Year;
			OrgHeader org = Factory.New<OrgHeader>();
			Invoice.ET_OH_Client = org.PK;
			Invoice.ET_StorageFromDate = new ZDateTime(year, 5, 1);     // Monday

			org.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Weekly;

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 6);       // Saturday
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 7);       // Sunday
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 8);       // Monday
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 14);      // Next sunday
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			org.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Fortnightly;
			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(3);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 15);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 14);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 20);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			org.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Monthly;
			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(3);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(7);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(10);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(14);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddMonths(1).AddDays(-1);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddMonths(4).AddDays(-1);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.IncludeInInvoicing = false;
			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(3);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(7);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(10);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(14);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddMonths(1);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddMonths(4);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.IncludeInInvoicing = true;
			Invoice.ET_OH_Client = ZGuid.Empty;
			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(3);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(7);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(10);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(14);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddMonths(1);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddMonths(4);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);
		}

		#region OnLoadInNewFactory()

		public void TestCheckET_StorageToDateValidatedAgainstClientStoragePeriod_OnLoadInNewFactory()
		{
			var warehouse = Helper.CreateWarehouse("Whs1");
			var org = Helper.CreateClient("Client");
			Invoice.ET_OH_Client = org.PK;

			org.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Weekly;

			Invoice.IncludeInInvoicing = true;
			Invoice.ET_OH_Client = org.PK;
			Invoice.ET_WW = warehouse.PK;

			var day = 6;
			var month = 5;
			var year = ZDateTime.Now.Year - 1;

			Invoice.ET_StorageToDate = new ZDateTime(year, month, day + 7);
			Invoice.ET_StorageFromDate = new ZDateTime(year, month, day);
			AssertNoErrors("Precondition: ", Invoice.ET_StorageToDateInfo);
			AssertNoErrors("Precondition: ", Invoice.ET_StorageFromDateInfo);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var invoiceInNewFactory = newFactory.LoadTop1<WhsInvoice>(new ZQuery(JobStorageSchema.PK, Invoice.PK));

			invoiceInNewFactory.ET_StorageToDate = new ZDateTime(year, month, day + 8);
			AssertHasErrors("Not 7 days apart", invoiceInNewFactory.ET_StorageToDateInfo);
		}

		#endregion

		#endregion

		#region TestET_StorageToDateAllowFutureDate

		[TestDate(2012, 1, 1)]
		public void TestET_StorageToDateAllowFutureDate()
		{
			var org = Factory.New<OrgHeader>();
			Invoice.ET_OH_Client = org.PK;
			Invoice.ET_WW = Factory.New<WhsWarehouse>().PK;
			Invoice.ET_StorageFromDate = ZDateTime.Today.AddDays(-1);

			org.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Weekly;

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(+4);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);
			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(+6);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			org.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Fortnightly;

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(+8);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);
			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(+13);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			org.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Monthly;

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(+15);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);
			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddMonths(1).AddDays(-1);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);
			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddMonths(3).AddDays(-1);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);
		}

		#endregion

		#region TestCheckET_BillingDate

		public void TestCheckET_BillingDate()
		{
			Invoice.ET_BillingDate = ZDateTime.Empty;
			Assert(Invoice.ET_BillingDateInfo.HasErrors());
			AssertEquals("Please enter a Billing Date.", Invoice.ET_BillingDateInfo.GetErrors().GetFirstMessage());

			Invoice.ET_BillingDate = ZDateTime.Today;
			Assert(!Invoice.ET_BillingDateInfo.HasErrors());
		}

		#endregion

		#region TestCheckForOverlappingPeriods
		[TestDate(2006, 01, 01)]
		public void TestCheckForOverlappingPeriods()
		{
			var year = ZDateTime.Now.Year;
			WhsWarehouse whs1 = Factory.NewWithValidTestData<WhsWarehouse>();

			WhsInvoice invoice0 = Factory.New<WhsInvoice>();
			invoice0.ET_StorageType = WhsInvoice.StorageType;
			invoice0.ET_OH_Client = Factory.New(typeof(OrgHeader)).PK;
			invoice0.ET_WW = whs1.PK;
			invoice0.Client.OH_Code = "TESTORG";
			invoice0.Client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Daily;
			invoice0.ET_StorageFromDate = new ZDateTime(2005, 2, 1);
			invoice0.ET_StorageToDate = new ZDateTime(2005, 3, 1);
			invoice0.ET_BillingDate = new ZDateTime(2005, 3, 1);
			Factory.Save();

			Invoice.ET_StorageType = WhsInvoice.StorageType;
			Invoice.ET_OH_Client = invoice0.Client.PK;
			Invoice.ET_WW = whs1.PK;

			AssertDates(new ZDateTime(2005, 2, 1), new ZDateTime(2005, 3, 1), ZBool.True);
			AssertDates(new ZDateTime(2005, 2, 5), new ZDateTime(2005, 2, 25), ZBool.True);
			AssertDates(new ZDateTime(2005, 1, 14), new ZDateTime(2005, 3, 15), ZBool.True);
			AssertDates(new ZDateTime(2005, 2, 5), new ZDateTime(2005, 3, 15), ZBool.True);
			AssertDates(new ZDateTime(2005, 1, 14), new ZDateTime(2005, 2, 25), ZBool.True);
			AssertDates(new ZDateTime(2005, 1, 14), new ZDateTime(2005, 1, 31), ZBool.False);
			AssertDates(new ZDateTime(2005, 3, 2), new ZDateTime(2005, 3, 15), ZBool.False);
		}

		#region TestCheckET_StorageFromDate_Overlapped

		public void TestCheckET_StorageFromDate_CreateOverlapped_MustPrevent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice1 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2017, 5, 5), new ZDateTime(2017, 5, 10));
			invoice1.RunPreSaveValidation();
			AssertNoErrors("Expected no errors.", invoice1.ET_StorageFromDateInfo);
			Factory.Save();

			var invoice2 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2017, 5, 9), new ZDateTime(2017, 5, 14));
			invoice2.RunPreSaveValidation();
			AssertHasError("Expected 'From Date' to report an error.", invoice2.ET_StorageFromDateInfo, WhsInvoiceValidation.ErrorMessages.OverlappingPeriods);
		}

		public void TestCheckET_StorageFromDate_ChangeToOverlapped_MustPrevent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice1 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2017, 5, 5), new ZDateTime(2017, 5, 11));
			var invoice2 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2017, 5, 12), new ZDateTime(2017, 5, 18));
			Factory.Save();

			AssertEquals("Precondition: Expected invoice to have the correct start date.", new ZDateTime(2017, 5, 5), invoice1.ET_StorageFromDate);
			AssertEquals("Precondition: Expected invoice to have the correct finish date.", new ZDateTime(2017, 5, 11), invoice1.ET_StorageToDate);
			AssertEquals("Precondition: Expected invoice to have the correct start date.", new ZDateTime(2017, 5, 12), invoice2.ET_StorageFromDate);
			AssertEquals("Precondition: Expected invoice to have the correct finish date.", new ZDateTime(2017, 5, 18), invoice2.ET_StorageToDate);

			invoice2.ET_StorageFromDate = new ZDateTime(2017, 5, 9);
			invoice2.RunPreSaveValidation();
			AssertHasError("Expected 'From Date' to report an error.", invoice2.ET_StorageFromDateInfo, WhsInvoiceValidation.ErrorMessages.OverlappingPeriods);
		}

		#endregion

		#region TestCheckET_StorageToDate_Overlapped

		public void TestCheckET_StorageToDate_CreateOverlapped_MustPrevent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice1 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2017, 5, 5), new ZDateTime(2017, 5, 10));
			Factory.Save();

			var invoice2 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2017, 5, 1), new ZDateTime(2017, 5, 7));
			invoice1.RunPreSaveValidation();
			invoice2.RunPreSaveValidation();
			AssertNoErrors("Expected no errors.", invoice1.ET_StorageToDateInfo);
			AssertHasError("Expected 'Finish Date' to report an error.", invoice2.ET_StorageToDateInfo, WhsInvoiceValidation.ErrorMessages.OverlappingPeriods);
		}

		public void TestCheckET_StorageToDate_ChangingToOverlapped_MustPrevent()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var invoice1 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2017, 5, 8), new ZDateTime(2017, 5, 14));
			var invoice2 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2017, 5, 1), new ZDateTime(2017, 5, 7));
			Factory.Save();

			AssertEquals("Precondition: Invoice was saved with wrong dates.", new ZDateTime(2017, 5, 8), invoice1.ET_StorageFromDate);
			AssertEquals("Precondition: Invoice was saved with wrong dates.", new ZDateTime(2017, 5, 14), invoice1.ET_StorageToDate);
			AssertEquals("Precondition: Invoice was saved with wrong dates.", new ZDateTime(2017, 5, 1), invoice2.ET_StorageFromDate);
			AssertEquals("Precondition: Invoice was saved with wrong dates.", new ZDateTime(2017, 5, 7), invoice2.ET_StorageToDate);

			invoice2.ET_StorageToDate = new ZDateTime(2017, 5, 9);
			invoice2.RunPreSaveValidation();
			AssertHasError("Expected 'Finish Date' to report an error.", invoice2.ET_StorageToDateInfo, WhsInvoiceValidation.ErrorMessages.OverlappingPeriods);
		}

		#endregion

		#region TestOverlappingDates_Warehouse

		public void TestOverlappingDates_Warehouse_ChangedToOverlapWithExistingInvoice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 1, 1);
			var invoice1 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2017, 5, 1), new ZDateTime(2017, 5, 7));
			var invoice2 = (WhsInvoice)Helper.CreateWhsInvoice(whs2.PK, data.Org1.PK, new ZDateTime(2017, 5, 1), new ZDateTime(2017, 5, 7));
			Factory.Save();

			invoice2.ET_WW = data.Whs1.PK;
			invoice2.RunPreSaveValidation();
			AssertNoErrors("Expected no errors since date is adjusted to first available date when changing the Warehouse.", invoice2.ET_StorageFromDateInfo);
			AssertNoErrors("Expected no errors since start date was adjusted.", invoice2.ET_StorageToDateInfo);
		}

		#endregion

		#region TestOverlappingDates_Client

		public void TestOverlappingDates_Client_ChangedToOverlapWithExistingInvoice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client2 = Helper.CreateClient();
			var invoice1 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, data.Org1.PK, new ZDateTime(2017, 5, 1), new ZDateTime(2017, 5, 7));
			var invoice2 = (WhsInvoice)Helper.CreateWhsInvoice(data.Whs1.PK, client2.PK, new ZDateTime(2017, 5, 1), new ZDateTime(2017, 5, 7));
			Factory.Save();

			invoice2.ET_OH_Client = data.Org1.PK;
			invoice2.RunPreSaveValidation();
			AssertNoErrors("Expected no errors since date is adjusted to first available date when changing the Client.", invoice2.ET_StorageFromDateInfo);
			AssertNoErrors("Expected no errors since start date was adjusted.", invoice2.ET_StorageToDateInfo);
		}

		#endregion

		#endregion

		#region TestCheckForOverlappingPeriodsDifferentWarehouses

		[TestDate(2015, 06, 01)]
		public void TestCheckForOverlappingPeriodsDifferentWarehouses()
		{
			var oneMonthAgo = ZDateTime.Today.AddMonths(-1);
			var twoMonthsAgo = ZDateTime.Today.AddMonths(-2);
			var threeMonthsAgo = ZDateTime.Today.AddMonths(-3);

			var whs1 = Factory.NewWithValidTestData<WhsWarehouse>();
			whs1.WW_WarehouseName = "WHS1";
			var whs2 = Factory.NewWithValidTestData<WhsWarehouse>();
			whs2.WW_WarehouseName = "WHS2";

			var invoice0 = Factory.New<WhsInvoice>();
			invoice0.ET_StorageType = WhsInvoice.StorageType;
			invoice0.ET_OH_Client = Factory.New(typeof(OrgHeader)).PK;
			invoice0.ET_WW = whs1.PK;
			invoice0.Client.OH_Code = "TESTORG";
			invoice0.Client.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.Daily;
			var firstDayOf3MonthsAgo = new ZDateTime(threeMonthsAgo.Year, threeMonthsAgo.Month, 1);
			var lastDayOf3MonthsAgo = new ZDateTime(firstDayOf3MonthsAgo.Year, firstDayOf3MonthsAgo.Month + 1, 1).AddDays(-1);
			invoice0.ET_StorageFromDate = firstDayOf3MonthsAgo;
			invoice0.ET_StorageToDate = lastDayOf3MonthsAgo;
			invoice0.ET_BillingDate = new ZDateTime(twoMonthsAgo.Year, twoMonthsAgo.Month, 1);

			var invoice1 = Factory.New<WhsInvoice>();
			invoice1.ET_StorageType = WhsInvoice.StorageType;
			invoice1.ET_OH_Client = invoice0.ET_OH_Client;
			invoice1.ET_WW = whs1.PK;
			var firstDayOf2MonthsAgo = new ZDateTime(twoMonthsAgo.Year, twoMonthsAgo.Month, 1);
			var lastDayOf2MonthAgo = new ZDateTime(firstDayOf2MonthsAgo.Year, firstDayOf2MonthsAgo.Month + 1, 1).AddDays(-1);
			invoice1.ET_StorageFromDate = firstDayOf2MonthsAgo;
			invoice1.ET_StorageToDate = lastDayOf2MonthAgo;
			invoice1.ET_BillingDate = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);

			Invoice.ET_StorageType = WhsInvoice.StorageType;
			Invoice.ET_OH_Client = invoice0.Client.PK;

			Factory.Save();

			AssertDates(new ZDateTime(threeMonthsAgo.Year, threeMonthsAgo.Month, 5), new ZDateTime(threeMonthsAgo.Year, threeMonthsAgo.Month, 10), ZBool.False);
			AssertDates(new ZDateTime(twoMonthsAgo.Year, twoMonthsAgo.Month, 5), new ZDateTime(twoMonthsAgo.Year, twoMonthsAgo.Month, 10), ZBool.False);
			AssertDates(new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 5), new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 10), ZBool.False);

			Invoice.ET_WW = whs2.PK;
			AssertDates(new ZDateTime(threeMonthsAgo.Year, threeMonthsAgo.Month, 5), new ZDateTime(threeMonthsAgo.Year, threeMonthsAgo.Month, 10), ZBool.False);
			AssertDates(new ZDateTime(twoMonthsAgo.Year, twoMonthsAgo.Month, 5), new ZDateTime(twoMonthsAgo.Year, twoMonthsAgo.Month, 10), ZBool.False);
			AssertDates(new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 5), new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 10), ZBool.False);

			Invoice.ET_WW = whs1.PK;
			AssertDates(new ZDateTime(threeMonthsAgo.Year, threeMonthsAgo.Month, 5), new ZDateTime(threeMonthsAgo.Year, threeMonthsAgo.Month, 10), ZBool.True);
			AssertDates(new ZDateTime(twoMonthsAgo.Year, twoMonthsAgo.Month, 5), new ZDateTime(twoMonthsAgo.Year, twoMonthsAgo.Month, 10), ZBool.True);
			AssertDates(new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 5), new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 10), ZBool.False);
		}

		#endregion

		#region TestCheckET_OffBandProcessingStatus

		public void TestCheckET_OffBandProcessingStatus()
		{
			TestCheckET_OffBandProcessingStatusCore(null, ""); // will populated with default 
			TestCheckET_OffBandProcessingStatusCore("", ""); // will populated with default
			TestCheckET_OffBandProcessingStatusCore("ERR", "");
			TestCheckET_OffBandProcessingStatusCore("QUE", "");
			TestCheckET_OffBandProcessingStatusCore("NIQ", "");
			TestCheckET_OffBandProcessingStatusCore("ABC", "Enter a valid selection.");
		}

		void TestCheckET_OffBandProcessingStatusCore(string status, string expectedError)
		{
			Invoice.ET_OffBandProcessingStatus = status;
			Invoice.Validation.ValidateET_OffBandProcessingStatus();
			AssertEquals($"Enter status: {status}", expectedError, Invoice.ET_OffBandProcessingStatusInfo.GetErrors().ToUniqueMessageListString());
		}

		#endregion

		#region TestCheckET_OffBandProcessingStatusStatusDesc

		public void TestCheckET_OffBandProcessingStatusStatusDesc()
		{
			var invoice = Factory.New<WhsInvoice>();
			AssertEquals("Default", StorageOffBandProcessingStatus.Descriptions.NIQ, invoice.ProcessingStatusDesc);

			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.QUE;
			AssertEquals(StorageOffBandProcessingStatus.Descriptions.QUE, invoice.ProcessingStatusDesc);

			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.NIQ;
			AssertEquals(StorageOffBandProcessingStatus.Descriptions.NIQ, invoice.ProcessingStatusDesc);

			invoice.ET_OffBandProcessingStatus = StorageOffBandProcessingStatus.Codes.ERR;
			AssertEquals(StorageOffBandProcessingStatus.Descriptions.ERR, invoice.ProcessingStatusDesc);

			invoice.ET_OffBandProcessingStatus = string.Empty;
			AssertEquals(string.Empty, invoice.ProcessingStatusDesc);
		}

		#endregion

		#region TestValidateCheckJobHeaderNotificationErrors

		public void TestValidateCheckJobHeaderNotificationErrors()
		{
			var invoice = Factory.New<WhsInvoice>();
			invoice.VerifyJobHeaderNotificationErrors = false;
			AssertNull(invoice.JobHeader);
			AssertNoErrors(invoice.VerifyJobHeaderNotificationErrorsInfo);

			invoice.VerifyJobHeaderNotificationErrors = true;
			AssertNull(invoice.JobHeader);
			AssertNoErrors(invoice.VerifyJobHeaderNotificationErrorsInfo);

			invoice.AutoRateJobHeader(null);
			invoice.VerifyJobHeaderNotificationErrors = true;
			AssertEquals(false, invoice.JobHeader.Notifications.HasErrors());
			AssertNoErrors(invoice.VerifyJobHeaderNotificationErrorsInfo);

			invoice.JobHeader.JH_ProfitLossReasonCode = "INV";
			invoice.VerifyJobHeaderNotificationErrors = true;
			AssertHasErrors(invoice.VerifyJobHeaderNotificationErrorsInfo);
			DeleteJobHeadersWithoutAnyCharges(invoice);
		}

		void DeleteJobHeadersWithoutAnyCharges(WhsInvoice invoice)
		{
			if (invoice.JobHeader != null && !invoice.JobHeader.IsInDatabase && !(invoice.JobHeader.Charges.Count > 0))
			{
				invoice.JobHeader.Delete();
			}
		}

		#endregion

		#region Implementation

		void AssertDates(ZDateTime fromDate, ZDateTime toDate, ZBool expectError)
		{
			Invoice.ET_StorageFromDate = fromDate;
			Invoice.ET_StorageToDate = toDate;
			Invoice.RunPreSaveValidation();

			AssertEquals(expectError, Invoice.ET_StorageFromDateInfo.HasErrors());
			AssertEquals(expectError, Invoice.ET_StorageToDateInfo.HasErrors());
			if (expectError)
			{
				AssertEquals(WhsInvoiceValidation.ErrorMessages.OverlappingPeriods, Invoice.ET_StorageFromDateInfo.GetErrors().GetFirstMessage());
				AssertEquals(WhsInvoiceValidation.ErrorMessages.OverlappingPeriods, Invoice.ET_StorageToDateInfo.GetErrors().GetFirstMessage());
			}
		}

		WhsInvoice Invoice
		{
			get { return fInvoice ?? (fInvoice = Factory.New<WhsInvoice>()); }
		}

		WhsInvoice fInvoice;

		#endregion
	}
}
