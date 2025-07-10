using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class NettingPeriodHelperTest : TestCaseWithFactory
	{
		[TestDate(2015, 2, 6)]
		public void TestGetNettingPeriodForNoNettingPeriodIsFound()
		{
			var nettingSystem = CreateNettingSystem(GlbCompany.CurrentCompany, "NS1", "Test Netting System");
			Factory.Save();

			var resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2014, 12, 31), LedgerTypes.AccountsReceivable);
			AssertEquals("No period setup for this date, should return empty GUID", ZGuid.Empty, resultPeriod);
		}

		[TestDate(2015, 2, 6)]
		public void TestGetNettingPeriodForARInvoiceImport_CurrentDateIsBeforeFirstPeiodsUploadCutoffDate()
		{
			var nettingSystem = CreateNettingSystem(GlbCompany.CurrentCompany, "NS1", "Test Netting System");

			var period1 = CreatePeriod(nettingSystem, "012015", earliestInvoiceDateTime: new ZDateTime(2015, 01, 01), latestInvoiceDateTime: new ZDateTime(2015, 01, 31), latestUploadDateTime: new ZDateTime(2015, 02, 07)
				, latestApprovalDateTime: new ZDateTime(2015, 02, 10), latestFXOfferDateTime: new ZDateTime(2015, 02, 12), nettingExecutionDateTime: new ZDateTime(2015, 02, 15), valueDate: new ZDate(2015, 01, 31));

			var period2 = CreatePeriod(nettingSystem, "022015", earliestInvoiceDateTime: new ZDateTime(2015, 02, 01), latestInvoiceDateTime: new ZDateTime(2015, 02, 28), latestUploadDateTime: new ZDateTime(2015, 03, 07)
				, latestApprovalDateTime: new ZDateTime(2015, 03, 10), latestFXOfferDateTime: new ZDateTime(2015, 03, 12), nettingExecutionDateTime: new ZDateTime(2015, 03, 15), valueDate: new ZDate(2015, 02, 28));

			var period3 = CreatePeriod(nettingSystem, "032015", earliestInvoiceDateTime: new ZDateTime(2015, 03, 01), latestInvoiceDateTime: new ZDateTime(2015, 03, 31), latestUploadDateTime: new ZDateTime(2015, 04, 07)
				, latestApprovalDateTime: new ZDateTime(2015, 04, 10), latestFXOfferDateTime: new ZDateTime(2015, 04, 12), nettingExecutionDateTime: new ZDateTime(2015, 04, 15), valueDate: new ZDate(2015, 03, 28));

			Factory.Save();

			var resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2014, 12, 31), LedgerTypes.AccountsReceivable);
			AssertEquals("No period setup for this date, so the first period is returned", period1.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 01, 01), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is on the earliest due date of cycle: 012015 and current time is within of Latest Approval Date of the cycle, hence 012015 is fetched", period1.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 01, 30), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is within the earliest and latest due date of cycle: 012015 and current time is within of Latest Approval Date of the cycle, hence 012015 is fetched", period1.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 01, 31), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is on the latest due date of cycle: 012015 and current time is within of Latest Approval Date of the cycle, hence 012015 is fetched", period1.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 02, 01), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is on the earliest due date of cycle: 022015 and current time is within Latest Upload Date of the cycle", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 02, 27), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is with the earliest and latest due date of cycle: 022015 and current time is within Latest Upload Date of the cycle", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 02, 28), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is on the latest due date of cycle: 022015 and current time is within Latest Upload Date of the cycle", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 03, 01), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is on the earliest due date of cycle: 032015 and current time is within Latest Upload Date of the cycle", period3.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 03, 31), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is on the latest due date of cycle: 032015 and current time is within Latest Upload Date of the cycle", period3.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 04, 01), LedgerTypes.AccountsReceivable);
			AssertEquals("No period setup for this date, so the last period is returned", period3.PK, resultPeriod);
		}

		[TestDate(2015, 2, 8)]
		public void TestGetNettingPeriodForARInvoiceImport_CurrentDateIsAfterFirstPeiodsUploadCutoffDate()
		{
			var nettingSystem = CreateNettingSystem(GlbCompany.CurrentCompany, "NS1", "Test Netting System");

			var period1 = CreatePeriod(nettingSystem, "012015", earliestInvoiceDateTime: new ZDateTime(2015, 01, 01), latestInvoiceDateTime: new ZDateTime(2015, 01, 31), latestUploadDateTime: new ZDateTime(2015, 02, 07)
				, latestApprovalDateTime: new ZDateTime(2015, 02, 10), latestFXOfferDateTime: new ZDateTime(2015, 02, 12), nettingExecutionDateTime: new ZDateTime(2015, 02, 15), valueDate: new ZDate(2015, 01, 31));

			var period2 = CreatePeriod(nettingSystem, "022015", earliestInvoiceDateTime: new ZDateTime(2015, 02, 01), latestInvoiceDateTime: new ZDateTime(2015, 02, 28), latestUploadDateTime: new ZDateTime(2015, 03, 07)
				, latestApprovalDateTime: new ZDateTime(2015, 03, 10), latestFXOfferDateTime: new ZDateTime(2015, 03, 12), nettingExecutionDateTime: new ZDateTime(2015, 03, 15), valueDate: new ZDate(2015, 02, 28));

			var period3 = CreatePeriod(nettingSystem, "032015", earliestInvoiceDateTime: new ZDateTime(2015, 03, 01), latestInvoiceDateTime: new ZDateTime(2015, 03, 31), latestUploadDateTime: new ZDateTime(2015, 04, 07)
				, latestApprovalDateTime: new ZDateTime(2015, 04, 10), latestFXOfferDateTime: new ZDateTime(2015, 04, 12), nettingExecutionDateTime: new ZDateTime(2015, 04, 15), valueDate: new ZDate(2015, 03, 28));

			Factory.Save();

			var resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2014, 12, 31), LedgerTypes.AccountsReceivable);
			AssertEquals("No period setup for this date, so the first period is returned", period1.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 01, 01), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is on the earliest due date of cycle: 012015 but current time is outside of Latest Upload Date of the cycle, hence 022015 is fetched", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 01, 30), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is within the earliest and latest due date of cycle: 012015 but current time is outside of Latest Upload Date of the cycle, hence 022015 is fetched", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 01, 31), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is on the latest due date of cycle: 012015 but current time is outside of Latest Upload Date of the cycle, hence 022015 is fetched", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 02, 01), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is on the earliest due date of cycle: 022015 and current time is within Latest Upload Date of the cycle", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 02, 27), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is with the earliest and latest due date of cycle: 022015 and current time is within Latest Upload Date of the cycle", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 02, 28), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is on the latest due date of cycle: 022015 and current time is within Latest Upload Date of the cycle", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 03, 01), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is on the earliest due date of cycle: 032015 and current time is within Latest Upload Date of the cycle", period3.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 03, 31), LedgerTypes.AccountsReceivable);
			AssertEquals("The Date is on the latest due date of cycle: 032015 and current time is within Latest Upload Date of the cycle", period3.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 04, 01), LedgerTypes.AccountsReceivable);
			AssertEquals("No period setup for this date, so the last period is returned", period3.PK, resultPeriod);
		}

		[TestDate(2015, 2, 8)]
		public void TestGetNettingPeriodForAPInvoiceImport_CurrentDateIsBeforeFirstPeiodsApprovalCutoffDate()
		{
			var nettingSystem = CreateNettingSystem(GlbCompany.CurrentCompany, "NS1", "Test Netting System");

			var period1 = CreatePeriod(nettingSystem, "012015", earliestInvoiceDateTime: new ZDateTime(2015, 01, 01), latestInvoiceDateTime: new ZDateTime(2015, 01, 31), latestUploadDateTime: new ZDateTime(2015, 02, 07)
				, latestApprovalDateTime: new ZDateTime(2015, 02, 10), latestFXOfferDateTime: new ZDateTime(2015, 02, 12), nettingExecutionDateTime: new ZDateTime(2015, 02, 15), valueDate: new ZDate(2015, 01, 31));

			var period2 = CreatePeriod(nettingSystem, "022015", earliestInvoiceDateTime: new ZDateTime(2015, 02, 01), latestInvoiceDateTime: new ZDateTime(2015, 02, 28), latestUploadDateTime: new ZDateTime(2015, 03, 07)
				, latestApprovalDateTime: new ZDateTime(2015, 03, 10), latestFXOfferDateTime: new ZDateTime(2015, 03, 12), nettingExecutionDateTime: new ZDateTime(2015, 03, 15), valueDate: new ZDate(2015, 02, 28));

			var period3 = CreatePeriod(nettingSystem, "032015", earliestInvoiceDateTime: new ZDateTime(2015, 03, 01), latestInvoiceDateTime: new ZDateTime(2015, 03, 31), latestUploadDateTime: new ZDateTime(2015, 04, 07)
				, latestApprovalDateTime: new ZDateTime(2015, 04, 10), latestFXOfferDateTime: new ZDateTime(2015, 04, 12), nettingExecutionDateTime: new ZDateTime(2015, 04, 15), valueDate: new ZDate(2015, 03, 28));

			Factory.Save();

			var resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2014, 12, 31), LedgerTypes.AccountsPayable);
			AssertEquals("No period setup for this date, so the first period is returned", period1.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 01, 01), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is on the earliest due date of cycle: 012015 and current time is within of Latest Approval Date of the cycle, hence 012015 is fetched", period1.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 01, 30), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is within the earliest and latest due date of cycle: 012015 and current time is within of Latest Approval Date of the cycle, hence 012015 is fetched", period1.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 01, 31), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is on the latest due date of cycle: 012015 and current time is within of Latest Approval Date of the cycle, hence 012015 is fetched", period1.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 02, 01), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is on the earliest due date of cycle: 022015 and current time is within Latest Upload Date of the cycle", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 02, 27), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is with the earliest and latest due date of cycle: 022015 and current time is within Latest Upload Date of the cycle", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 02, 28), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is on the latest due date of cycle: 022015 and current time is within Latest Upload Date of the cycle", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 03, 01), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is on the earliest due date of cycle: 032015 and current time is within Latest Upload Date of the cycle", period3.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 03, 31), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is on the latest due date of cycle: 032015 and current time is within Latest Upload Date of the cycle", period3.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 04, 01), LedgerTypes.AccountsPayable);
			AssertEquals("No period setup for this date, so the last period is returned", period3.PK, resultPeriod);
		}

		[TestDate(2015, 2, 11)]
		public void TestGetNettingPeriodForAPInvoiceImport_CurrentDateIsAfterFirstPeiodsApprovalCutoffDate()
		{
			var nettingSystem = CreateNettingSystem(GlbCompany.CurrentCompany, "NS1", "Test Netting System");

			var period1 = CreatePeriod(nettingSystem, "012015", earliestInvoiceDateTime: new ZDateTime(2015, 01, 01), latestInvoiceDateTime: new ZDateTime(2015, 01, 31), latestUploadDateTime: new ZDateTime(2015, 02, 07)
				, latestApprovalDateTime: new ZDateTime(2015, 02, 10), latestFXOfferDateTime: new ZDateTime(2015, 02, 12), nettingExecutionDateTime: new ZDateTime(2015, 02, 15), valueDate: new ZDate(2015, 01, 31));

			var period2 = CreatePeriod(nettingSystem, "022015", earliestInvoiceDateTime: new ZDateTime(2015, 02, 01), latestInvoiceDateTime: new ZDateTime(2015, 02, 28), latestUploadDateTime: new ZDateTime(2015, 03, 07)
				, latestApprovalDateTime: new ZDateTime(2015, 03, 10), latestFXOfferDateTime: new ZDateTime(2015, 03, 12), nettingExecutionDateTime: new ZDateTime(2015, 03, 15), valueDate: new ZDate(2015, 02, 28));

			var period3 = CreatePeriod(nettingSystem, "032015", earliestInvoiceDateTime: new ZDateTime(2015, 03, 01), latestInvoiceDateTime: new ZDateTime(2015, 03, 31), latestUploadDateTime: new ZDateTime(2015, 04, 07)
				, latestApprovalDateTime: new ZDateTime(2015, 04, 10), latestFXOfferDateTime: new ZDateTime(2015, 04, 12), nettingExecutionDateTime: new ZDateTime(2015, 04, 15), valueDate: new ZDate(2015, 03, 28));

			Factory.Save();

			var resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2014, 12, 31), LedgerTypes.AccountsPayable);
			AssertEquals("No period setup for this date, so the first period is returned", period1.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 01, 01), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is on the earliest due date of cycle: 012015 but current time is outside of Latest Upload Date of the cycle, hence 022015 is fetched", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 01, 30), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is within the earliest and latest due date of cycle: 012015 but current time is outside of Latest Upload Date of the cycle, hence 022015 is fetched", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 01, 31), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is on the latest due date of cycle: 012015 but current time is outside of Latest Upload Date of the cycle, hence 022015 is fetched", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 02, 01), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is on the earliest due date of cycle: 022015 and current time is within Latest Upload Date of the cycle", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 02, 27), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is with the earliest and latest due date of cycle: 022015 and current time is within Latest Upload Date of the cycle", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 02, 28), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is on the latest due date of cycle: 022015 and current time is within Latest Upload Date of the cycle", period2.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 03, 01), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is on the earliest due date of cycle: 032015 and current time is within Latest Upload Date of the cycle", period3.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 03, 31), LedgerTypes.AccountsPayable);
			AssertEquals("The Date is on the latest due date of cycle: 032015 and current time is within Latest Upload Date of the cycle", period3.PK, resultPeriod);

			resultPeriod = NettingPeriodHelper.GetNettingPeriod(nettingSystem, new ZDateTime(2015, 04, 01), LedgerTypes.AccountsPayable);
			AssertEquals("No period setup for this date, so the last period is returned", period3.PK, resultPeriod);
		}

		[TestDate(2015, 2, 1, 12, 0, 0)]
		public void TestGetNextOpenPeriod()
		{
			var nettingSystem = CreateNettingSystem(GlbCompany.CurrentCompany, "NS1", "Test Netting System");

			var period1 = CreatePeriod(nettingSystem, "012015", new ZDateTime(2015, 01, 01), new ZDateTime(2015, 01, 31), new ZDateTime(2015, 02, 07)
				, new ZDateTime(2015, 02, 10), new ZDateTime(2015, 02, 12), new ZDateTime(2015, 02, 15), new ZDate(2015, 01, 31), true);

			var period2 = CreatePeriod(nettingSystem, "022015", new ZDateTime(2015, 02, 01), new ZDateTime(2015, 02, 28), new ZDateTime(2015, 03, 07)
				, new ZDateTime(2015, 03, 10), new ZDateTime(2015, 03, 12), new ZDateTime(2015, 03, 15), new ZDate(2015, 02, 28), false);

			Factory.Save();

			var resultPeriod = NettingPeriodHelper.GetNextOpenPeriod(period1, Factory);
			AssertNotNull(resultPeriod);
			AssertEquals("As current period is completed next open period is returned", period2.PK, resultPeriod.PK);

			resultPeriod = NettingPeriodHelper.GetNextOpenPeriod(period2, Factory);
			AssertNull(resultPeriod);
		}

		[TestDate(2015, 2, 1)]
		public void TestGetFirstOpenPeriod()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var nettingSystem1 = CreateNettingSystem(company, "NS1", "Test Netting System");

			var ns1_period1 = CreatePeriod(nettingSystem1, "012015", new ZDateTime(2015, 01, 01), new ZDateTime(2015, 01, 31), new ZDateTime(2015, 02, 07)
				, new ZDateTime(2015, 02, 10), new ZDateTime(2015, 02, 12), new ZDateTime(2015, 02, 15), new ZDate(2015, 01, 31), true);

			var ns1_period2 = CreatePeriod(nettingSystem1, "022015", new ZDateTime(2015, 02, 01), new ZDateTime(2015, 02, 28), new ZDateTime(2015, 03, 07)
				, new ZDateTime(2015, 03, 10), new ZDateTime(2015, 03, 12), new ZDateTime(2015, 03, 15), new ZDate(2015, 02, 28), false);

			var ns1_period3 = CreatePeriod(nettingSystem1, "032015", new ZDateTime(2015, 03, 01), new ZDateTime(2015, 03, 31), new ZDateTime(2015, 04, 07)
				, new ZDateTime(2015, 04, 10), new ZDateTime(2015, 04, 12), new ZDateTime(2015, 04, 15), new ZDate(2015, 03, 31), false);

			var nettingSystem2 = CreateNettingSystem(GlbCompany.CurrentCompany, "NS2", "Test Netting System2");

			var ns2_period1 = CreatePeriod(nettingSystem2, "012015", new ZDateTime(2015, 01, 01), new ZDateTime(2015, 01, 31), new ZDateTime(2015, 02, 07)
				, new ZDateTime(2015, 02, 10), new ZDateTime(2015, 02, 12), new ZDateTime(2015, 02, 15), new ZDate(2015, 01, 31), false);

			Factory.Save();

			var resultPeriod = NettingPeriodHelper.GetFirstOpenNettingPeriod(company.PK, Factory);
			AssertNotNull(resultPeriod);
			AssertEquals(ns1_period2.PK, resultPeriod.PK);

			resultPeriod = NettingPeriodHelper.GetFirstOpenNettingPeriod(GlbCompany.CurrentCompany.PK, Factory);
			AssertNotNull(resultPeriod);
			AssertEquals(ns2_period1.PK, resultPeriod.PK);
		}

		NettingSystem CreateNettingSystem(GlbCompany company, ZString code, ZString description)
		{
			var nettingSystem = Factory.New<NettingSystem>();
			nettingSystem.NS_GC = company.PK;
			nettingSystem.NS_Code = code;
			nettingSystem.NS_Description = description;

			return nettingSystem;
		}

		NettingSystemPeriod CreatePeriod(NettingSystem nettingSystem, ZString nsp_period, ZDateTime earliestInvoiceDateTime, ZDateTime latestInvoiceDateTime,
			ZDateTime latestUploadDateTime, ZDateTime latestApprovalDateTime, ZDateTime latestFXOfferDateTime, ZDateTime nettingExecutionDateTime, ZDate valueDate, bool isComplete = false)
		{
			var period = Factory.New<NettingSystemPeriod>();
			period.NSP_Period = nsp_period;
			period.NSP_EarliestInvoiceDateUtc = earliestInvoiceDateTime;
			period.NSP_LatestInvoiceDateUtc = latestInvoiceDateTime;
			period.NSP_NettingExecutionDateUtc = nettingExecutionDateTime;

			period.NSP_LatestUploadDateUtc = latestUploadDateTime;
			period.NSP_LatestFXOfferDateUtc = latestFXOfferDateTime;
			period.NSP_LatestApprovalDateUtc = latestApprovalDateTime;
			period.NSP_ValueDate = valueDate;
			period.NSP_OfferPrepaymentDate = valueDate;

			period.NSP_IsComplete = isComplete;

			period.NSP_NS_NettingSystem = nettingSystem.PK;

			return period;
		}
	}
}
