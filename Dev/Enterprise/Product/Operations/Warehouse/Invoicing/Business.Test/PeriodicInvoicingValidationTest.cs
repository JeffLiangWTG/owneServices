using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Invoicing.Business.Test
{
	[TestedType(typeof(PeriodicInvoicingValidation))]
	public class PeriodicInvoicingValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckET_WW

		public void TestCheckET_WW()
		{
			Invoice.ET_WW = Factory.New<WhsWarehouse>().PK;
			AssertNoErrors(Invoice.ET_WWInfo);

			Invoice.Warehouse.WW_IsActive = false;
			Invoice.Validation.ValidateET_WW();
			AssertHasError(Invoice.ET_WWInfo, PeriodicInvoicingValidation.ErrorMessages.CannotSelectInactiveWarehouse);

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
			AssertEquals(PeriodicInvoicingValidation.ErrorMessages.FromDateInFuture, Invoice.ET_StorageFromDateInfo.GetErrors().GetFirstMessage());

			Invoice.IncludeInInvoicing = false;
			Invoice.ET_StorageFromDate = ZDateTime.Today.AddMonths(-1);
			Invoice.ET_StorageToDate = ZDateTime.Today.AddMonths(-2);
			AssertNoErrors(Invoice.ET_StorageFromDateInfo);
			Invoice.IncludeInInvoicing = true;
			AssertHasErrors(Invoice.ET_StorageFromDateInfo);
			AssertEquals(PeriodicInvoicingValidation.ErrorMessages.ToDateBeforeFromDate, Invoice.ET_StorageFromDateInfo.GetErrors().GetFirstMessage());

			Invoice.ET_StorageFromDate = ZDateTime.Empty;
			AssertHasErrors(Invoice.ET_StorageFromDateInfo);
			AssertEquals("Please enter a From Date.", Invoice.ET_StorageFromDateInfo.GetErrors().GetFirstMessage());

			Invoice.ET_StorageFromDate = ZDateTime.Today.AddMonths(-3);
			AssertNoErrors(Invoice.ET_StorageFromDateInfo);
		}

		#endregion

		#region TestCheckET_StorageToDate

		[TestDate(2025, 3, 1)]
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
			AssertEquals(PeriodicInvoicingValidation.ErrorMessages.ToDateBeforeFromDate, Invoice.ET_StorageToDateInfo.GetErrors().GetFirstMessage());

			Invoice.ET_StorageToDate = ZDateTime.Empty;
			AssertHasErrors(Invoice.ET_StorageToDateInfo);
			AssertEquals("Please enter a To Date.", Invoice.ET_StorageToDateInfo.GetErrors().GetFirstMessage());

			Invoice.ET_StorageToDate = ZDateTime.Today.AddDays(-1);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);
		}

		#endregion

		#region TestCheckET_StorageToDateValidatedAgainstClientStoragePeriod

		public void TestCheckET_StorageToDateValidatedAgainstClientStoragePeriod()
		{
			var year = ZDateTime.Now.Year;
			OrgHeader org = Factory.New<OrgHeader>();
			Invoice.ET_OH_Client = org.PK;
			Invoice.ET_StorageFromDate = new ZDateTime(year, 5, 1);     // Monday

			org.CompanyData.OB_ARYardStorageRatingPeriod = Core.Constants.StorageCalculationPeriods.Weekly;

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 6);       // Saturday
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 7);       // Sunday
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 8);       // Monday
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 14);      // Next sunday
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			org.CompanyData.OB_ARYardStorageRatingPeriod = Core.Constants.StorageCalculationPeriods.Fortnightly;
			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(3);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 15);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 14);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			Invoice.ET_StorageToDate = new ZDateTime(year, 5, 20);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);

			org.CompanyData.OB_ARYardStorageRatingPeriod = Core.Constants.StorageCalculationPeriods.Monthly;
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

		#endregion

		#region TestET_StorageToDateAllowFutureDate

		[TestDate(2012, 1, 1)]
		public void TestET_StorageToDateAllowFutureDate()
		{
			var org = Factory.New<OrgHeader>();
			Invoice.ET_OH_Client = org.PK;
			Invoice.ET_WW = Factory.New<WhsWarehouse>().PK;
			Invoice.ET_StorageFromDate = ZDateTime.Today.AddDays(-1);

			org.CompanyData.OB_ARYardStorageRatingPeriod = Core.Constants.StorageCalculationPeriods.Weekly;

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(+4);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);
			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(+6);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			org.CompanyData.OB_ARYardStorageRatingPeriod = Core.Constants.StorageCalculationPeriods.Fortnightly;

			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(+8);
			AssertHasErrors(Invoice.ET_StorageToDateInfo);
			Invoice.ET_StorageToDate = Invoice.ET_StorageFromDate.AddDays(+13);
			AssertNoErrors(Invoice.ET_StorageToDateInfo);

			org.CompanyData.OB_ARYardStorageRatingPeriod = Core.Constants.StorageCalculationPeriods.Monthly;

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

			PeriodicInvoicing invoice0 = Factory.NewWithValidTestData<PeriodicInvoicing>();
			invoice0.ET_OH_Client = Factory.New(typeof(OrgHeader)).PK;
			invoice0.ET_WW = whs1.PK;
			invoice0.Client.OH_Code = "TESTORG";
			invoice0.Client.CompanyData.OB_ARYardStorageRatingPeriod = Core.Constants.StorageCalculationPeriods.Daily;
			invoice0.ET_StorageFromDate = new ZDateTime(2005, 2, 1);
			invoice0.ET_StorageToDate = new ZDateTime(2005, 3, 1);
			invoice0.ET_BillingDate = new ZDateTime(2005, 3, 1);
			Factory.Save();

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

			var invoice0 = Factory.NewWithValidTestData<PeriodicInvoicing>();
			invoice0.ET_OH_Client = Factory.New(typeof(OrgHeader)).PK;
			invoice0.ET_WW = whs1.PK;
			invoice0.Client.OH_Code = "TESTORG";
			invoice0.Client.CompanyData.OB_ARYardStorageRatingPeriod = Core.Constants.StorageCalculationPeriods.Daily;
			var firstDayOf3MonthsAgo = new ZDateTime(threeMonthsAgo.Year, threeMonthsAgo.Month, 1);
			var lastDayOf3MonthsAgo = new ZDateTime(firstDayOf3MonthsAgo.Year, firstDayOf3MonthsAgo.Month + 1, 1).AddDays(-1);
			invoice0.ET_StorageFromDate = firstDayOf3MonthsAgo;
			invoice0.ET_StorageToDate = lastDayOf3MonthsAgo;
			invoice0.ET_BillingDate = new ZDateTime(twoMonthsAgo.Year, twoMonthsAgo.Month, 1);

			var invoice1 = Factory.NewWithValidTestData<PeriodicInvoicing>();
			invoice1.ET_OH_Client = invoice0.ET_OH_Client;
			invoice1.ET_WW = whs1.PK;
			var firstDayOf2MonthsAgo = new ZDateTime(twoMonthsAgo.Year, twoMonthsAgo.Month, 1);
			var lastDayOf2MonthAgo = new ZDateTime(firstDayOf2MonthsAgo.Year, firstDayOf2MonthsAgo.Month + 1, 1).AddDays(-1);
			invoice1.ET_StorageFromDate = firstDayOf2MonthsAgo;
			invoice1.ET_StorageToDate = lastDayOf2MonthAgo;
			invoice1.ET_BillingDate = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);

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
			var invoice = Factory.NewWithValidTestData<PeriodicInvoicing>();
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
			var invoice = Factory.NewWithValidTestData<PeriodicInvoicing>();
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

		void DeleteJobHeadersWithoutAnyCharges(PeriodicInvoicing invoice)
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
				AssertEquals(PeriodicInvoicingValidation.ErrorMessages.OverlappingPeriods, Invoice.ET_StorageFromDateInfo.GetErrors().GetFirstMessage());
				AssertEquals(PeriodicInvoicingValidation.ErrorMessages.OverlappingPeriods, Invoice.ET_StorageToDateInfo.GetErrors().GetFirstMessage());
			}
		}

		PeriodicInvoicing Invoice
		{
			get { return fInvoice ?? (fInvoice = Factory.NewWithValidTestData<PeriodicInvoicing>()); }
		}

		PeriodicInvoicing fInvoice;

		#endregion
	}
}
