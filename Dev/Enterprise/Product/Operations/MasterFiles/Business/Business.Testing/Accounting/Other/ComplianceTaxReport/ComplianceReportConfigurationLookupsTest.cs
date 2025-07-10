using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using FluentAssertions;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;
using LookupsClass = Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ComplianceReportConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxRegistrationTypeList()
		{
			configuration.Country = "AU";
			Assert("TaxRegistrationTypeList contains 'ABN'", Lookups.TaxRegistrationTypeList.ContainsCode("ABN"));
		}

		public void TestRepCountryRegistrationCodeList()
		{
			configuration.Country = "AU";
			Assert("RepCountryRegistrationCodeList contains 'ABN'", Lookups.RepCountryRegistrationCodeList.ContainsCode("ABN"));
		}

		public void TestReportPeriodicityListAustralia()
		{
			configuration.Country = "AU";
			var list = Lookups.ReportPeriodicityList;
			AssertNotNull("ReportPeriodicityList", list);
			var expected = new List<string>
			{
				ReportPeriodicityCodes.AccountingPeriod,
				ReportPeriodicityCodes.CalendarMonth,
				ReportPeriodicityCodes.DateRange,
				ReportPeriodicityCodes.FinancialYear,
				ReportPeriodicityCodes.ComplianceFinancialYear,
				ReportPeriodicityCodes.RangeAccountingPeriod,
				ReportPeriodicityCodes.MonthlyQuarterlyYearly
			};
			list.Cast<CodeDescriptionPair>().Select(p => p.Code).Should().BeEquivalentTo(expected, "Equivalent to expected");
		}

		public void TestReportPeriodicityListOther()
		{
			configuration.Country = "UK";
			var list = Lookups.ReportPeriodicityList;
			AssertNotNull("ReportPeriodicityList", list);
			var expected = new List<string>
			{
				ReportPeriodicityCodes.AccountingPeriod,
				ReportPeriodicityCodes.CalendarMonth,
				ReportPeriodicityCodes.DateRange,
				ReportPeriodicityCodes.FinancialYear,
				ReportPeriodicityCodes.RangeAccountingPeriod,
				ReportPeriodicityCodes.MonthlyQuarterlyYearly
			};
			list.Cast<CodeDescriptionPair>().Select(p => p.Code).Should().BeEquivalentTo(expected, "Equivalent to expected");
		}

		public void TestReportBaseTablePrefixList()
		{
			AssertNotNull("ReportBaseTablePrefixList", Lookups.ReportBaseTablePrefixList);
			AssertEquals("ReportBaseTablePrefixList.Count", 5, Lookups.ReportBaseTablePrefixList.Count);
			Assert("The ReportBaseTablePrefixList should contain 'AH'", Lookups.ReportBaseTablePrefixList.ContainsCode(LookupsClass.ReportBaseTablePrefixListCodes.TransactionHeader));
			Assert("The ReportBaseTablePrefixList should contain 'AL'", Lookups.ReportBaseTablePrefixList.ContainsCode(LookupsClass.ReportBaseTablePrefixListCodes.TransactionLine));
			Assert("The ReportBaseTablePrefixList should contain '**'", Lookups.ReportBaseTablePrefixList.ContainsCode(LookupsClass.ReportBaseTablePrefixListCodes.AllTransactions));
			Assert("The ReportBaseTablePrefixList should contain 'ADH'", Lookups.ReportBaseTablePrefixList.ContainsCode(LookupsClass.ReportBaseTablePrefixListCodes.ComplianceDocumentHeader));
			Assert("The ReportBaseTablePrefixList should contain 'GLD'", Lookups.ReportBaseTablePrefixList.ContainsCode(LookupsClass.ReportBaseTablePrefixListCodes.GeneralLedgerData));
		}

		public void TestReportLineGroupingList()
		{
			AssertNotNull("ReportLineGroupingList", Lookups.ReportLineGroupingList);
			AssertEquals("ReportLineGroupingList.Count", 1, Lookups.ReportLineGroupingList.Count);

			configuration.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;
			AssertEquals("ReportLineGroupingList.Count", 1, Lookups.ReportLineGroupingList.Count);
			Assert("The ReportLineGroupingList should contain ''", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.NoGrouping));

			configuration.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
			AssertEquals("ReportLineGroupingList.Count", 4, Lookups.ReportLineGroupingList.Count);
			Assert("The ReportLineGroupingList should contain ''", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.NoGrouping));
			Assert("The ReportLineGroupingList should contain 'DAB'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.DayBook));
			Assert("The ReportLineGroupingList should contain 'DBW'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.DayBookWithoutGrouping));
			Assert("The ReportLineGroupingList should contain 'DBP'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.DayBookWithPresentation));

			configuration.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.GeneralLedgerData;
			AssertEquals("ReportLineGroupingList.Count", 4, Lookups.ReportLineGroupingList.Count);
			Assert("The ReportLineGroupingList should contain ''", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.NoGrouping));
			Assert("The ReportLineGroupingList should contain 'DAB'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.DayBook));
			Assert("The ReportLineGroupingList should contain 'DBW'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.DayBookWithoutGrouping));
			Assert("The ReportLineGroupingList should contain 'DBP'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.DayBookWithPresentation));

			configuration.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionHeader;
			AssertTransactionHeaderOrLine();

			configuration.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionLine;
			AssertTransactionHeaderOrLine();

			configuration.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;
			AssertEquals("ReportLineGroupingList.Count", 1, Lookups.ReportLineGroupingList.Count);
			Assert("The ReportLineGroupingList should contain ''", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.NoGrouping));
		}

		void AssertTransactionHeaderOrLine()
		{
			AssertEquals("ReportLineGroupingList.Count", 15, Lookups.ReportLineGroupingList.Count);
			Assert("The ReportLineGroupingList should contain ''", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.NoGrouping));
			Assert("The ReportLineGroupingList should contain 'TXR'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.TaxReporting));
			Assert("The ReportLineGroupingList should contain 'HDR'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.TransactionHeader));
			Assert("The ReportLineGroupingList should contain 'HDL'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.TransactionHeaderWithLines));
			Assert("The ReportLineGroupingList should contain 'ORG'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.Organisation));
			Assert("The ReportLineGroupingList should contain 'OBL'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.OrganisationBLCode));
			Assert("The ReportLineGroupingList should contain 'ORS'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.OrganisationSubCode));
			Assert("The ReportLineGroupingList should contain 'TPA'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.TransactionPayments));
			Assert("The ReportLineGroupingList should contain 'PTR'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.PaymentTimesSmallBusinessReportable));
			Assert("The ReportLineGroupingList should contain 'PTA'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.PaymentTimesAll));
			Assert("The ReportLineGroupingList should contain 'HRS'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.TransactionHeaderAndReportSubCode));
			Assert("The ReportLineGroupingList should contain 'RSH'", Lookups.ReportLineGroupingList.ContainsCode(LookupsClass.ReportLineGroupingListCodes.ReportSubCodeAndTransactionHeader));
			Assert("The ReportLineGroupingList should contain 'DAB'", Lookups.ReportLineGroupingList.ContainsCode(ReportLineGroupingListCodes.DayBook));
			Assert("The ReportLineGroupingList should contain 'DBW'", Lookups.ReportLineGroupingList.ContainsCode(ReportLineGroupingListCodes.DayBookWithoutGrouping));
			Assert("The ReportLineGroupingList should contain 'DBP'", Lookups.ReportLineGroupingList.ContainsCode(ReportLineGroupingListCodes.DayBookWithPresentation));
		}

		public void TestReportLineOrderingList()
		{
			AssertNotNull("ReportLineOrderingList", Lookups.ReportLineOrderingList);

			configuration.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionHeader;
			configuration.Country = CountryCodes.Australia;
			AssertEquals("ReportLineOrderingList.Count", 4, Lookups.ReportLineOrderingList.Count);
			Assert("The ReportLineGroupingList should contain 'LAS'", Lookups.ReportLineOrderingList.ContainsCode(LookupsClass.ReportLineOrderingListCodes.LedgerAscendinging));
			Assert("The ReportLineGroupingList should contain 'LDS'", Lookups.ReportLineOrderingList.ContainsCode(LookupsClass.ReportLineOrderingListCodes.LedgerDescendinging));
			Assert("The ReportLineGroupingList should contain 'CST'", Lookups.ReportLineOrderingList.ContainsCode(LookupsClass.ReportLineOrderingListCodes.ComplianceSubType));
			Assert("The ReportLineGroupingList should contain 'ORG'", Lookups.ReportLineOrderingList.ContainsCode(LookupsClass.ReportLineOrderingListCodes.Organisation));

			configuration.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;
			configuration.Country = CountryCodes.Australia;
			AssertEquals("ReportLineOrderingList.Count", 1, Lookups.ReportLineOrderingList.Count);
			Assert("The ReportLineGroupingList should contain 'CDN'", Lookups.ReportLineOrderingList.ContainsCode(LookupsClass.ReportLineOrderingListCodes.ComplianceDocumentNumber));

			configuration.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;
			configuration.Country = CountryCodes.Taiwan;
			AssertEquals("ReportLineOrderingList.Count", 2, Lookups.ReportLineOrderingList.Count);
			Assert("The ReportLineGroupingList should contain 'FDN'", Lookups.ReportLineOrderingList.ContainsCode(LookupsClass.ReportLineOrderingListCodes.FormatCodeAndDocumentNumber));
			Assert("The ReportLineGroupingList should contain 'CDN'", Lookups.ReportLineOrderingList.ContainsCode(LookupsClass.ReportLineOrderingListCodes.ComplianceDocumentNumber));
		}

		public void TestGoodsServiceTypeList()
		{
			AssertNotNull("GoodsServiceTypeList", Lookups.GoodsServiceTypeList);
			AssertEquals("GoodsServiceTypeList.Count", 3, Lookups.GoodsServiceTypeList.Count);
			Assert("The GoodsServiceTypeList should contain ''", Lookups.GoodsServiceTypeList.ContainsCode(LookupsClass.GoodsServiceTypeCodes.GoodsAndService));
			Assert("The GoodsServiceTypeList should contain 'GDS'", Lookups.GoodsServiceTypeList.ContainsCode(LookupsClass.GoodsServiceTypeCodes.GoodsOnly));
			Assert("The GoodsServiceTypeList should contain 'SRV'", Lookups.GoodsServiceTypeList.ContainsCode(LookupsClass.GoodsServiceTypeCodes.ServiceOnly));
		}

		public void TestReportAmountsRoundingTypeList()
		{
			AssertNotNull("ReportAmountsRoundingTypeList", Lookups.ReportAmountsRoundingTypeList);
			AssertEquals("ReportAmountsRoundingTypeList.Count", 3, Lookups.ReportAmountsRoundingTypeList.Count);
			Assert("The ReportAmountsRoundingTypeList should contain ''", Lookups.ReportAmountsRoundingTypeList.ContainsCode(LookupsClass.ReportAmountsRoundingTypeListCodes.NoRoundingOrTruncating));
			Assert("The ReportAmountsRoundingTypeList should contain 'RND'", Lookups.ReportAmountsRoundingTypeList.ContainsCode(LookupsClass.ReportAmountsRoundingTypeListCodes.Rounding));
			Assert("The ReportAmountsRoundingTypeList should contain 'TRN'", Lookups.ReportAmountsRoundingTypeList.ContainsCode(LookupsClass.ReportAmountsRoundingTypeListCodes.Truncating));
		}

		public void TestAmountThresholdLevelList()
		{
			AssertNotNull("AmountThresholdLevelList", Lookups.AmountThresholdLevelList);
			AssertEquals("AmountThresholdLevelList.Count", 3, Lookups.AmountThresholdLevelList.Count);
			Assert("The AmountThresholdLevelList should contain ''", Lookups.AmountThresholdLevelList.ContainsCode(LookupsClass.AmountThresholdLevelListCodes.NoThreshold));
			Assert("The AmountThresholdLevelList should contain 'HDR'", Lookups.AmountThresholdLevelList.ContainsCode(LookupsClass.AmountThresholdLevelListCodes.TransactionHeader));
			Assert("The AmountThresholdLevelList should contain 'ORG'", Lookups.AmountThresholdLevelList.ContainsCode(LookupsClass.AmountThresholdLevelListCodes.Organisation));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			configuration = new ComplianceReportConfiguration();
			Lookups = new LookupsClass(configuration);
		}
		LookupsClass Lookups;
		ComplianceReportConfiguration configuration;

		#endregion
	}
}
