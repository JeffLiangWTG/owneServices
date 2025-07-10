using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccAlternateChartValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAAC_Code()
		{
			var chart = Factory.New<AccAlternateChart>();
			chart.Validation.ValidateAAC_Code();
			AssertHasError(chart.AAC_CodeInfo, "Please enter a Chart Code.");
			chart.AAC_Code = "1";
			AssertNoErrors(chart.AAC_CodeInfo);
			chart.AAC_Code = "11A";
			AssertNoErrors(chart.AAC_CodeInfo);

			chart.AAC_Code = "11A*&^";
			AssertHasError(chart.AAC_CodeInfo, "Invalid Chart Code. A valid code cannot include special characters.");

			chart.AAC_Code = "11A123456Q";
			AssertNoErrors(chart.AAC_CodeInfo);

			chart.AAC_Description = "desc";
			Factory.Save();

			var chart2 = new BusinessObjectFactory().New<AccAlternateChart>();
			chart2.AAC_Code = "11A123456Q";
			AssertHasError(chart2.AAC_CodeInfo, $"This Chart Code is already used by a non-Global Chart in '{GlbCompany.CurrentCompany.GC_Code}' system company. Please enter another code.");

			chart.AAC_IsGlobal = true;
			Factory.Save();
			chart2.Validation.ValidateAAC_Code();
			AssertHasError(chart2.AAC_CodeInfo, $"This Chart Code is already used by a Global Chart. Please enter another code.");

			chart2.AAC_Code = "112";
			AssertNoErrors(chart2.AAC_CodeInfo);
		}

		public void TestCheckAAC_Description()
		{
			var chart = Factory.New<AccAlternateChart>();
			chart.Validation.ValidateAAC_Description();
			AssertHasError(chart.AAC_DescriptionInfo, "Please enter a Chart Name.");
			chart.AAC_Description = "1";
			AssertNoErrors(chart.AAC_DescriptionInfo);
		}

		public void TestCheckAAC_BalanceSheetStyle()
		{
			var chart = Factory.New<AccAlternateChart>();
			chart.AAC_BalanceSheetStyle = ZString.Empty;
			AssertHasError(chart.AAC_BalanceSheetStyleInfo, "Please enter a Balance Sheet Style.");
			chart.AAC_BalanceSheetStyle = "111";
			AssertHasError(chart.AAC_BalanceSheetStyleInfo, "Enter a valid Balance Sheet Style.");
			chart.AAC_BalanceSheetStyle = AccAlternateChartLookups.BalanceSheetStyleCode.ELA;
			AssertNoErrors(chart.AAC_BalanceSheetStyleInfo);
		}

		public void TestCheckAAC_IsFixedLength()
		{
			var creator = new AccountingTestObjectCreator(Factory);
			var chart = creator.CreateAlternateChart("MGT");
			Factory.Save();
			AssertNoErrors(chart.AAC_IsFixedLengthInfo);
			var alternateGLAccount = Factory.New<AccAlternateGLAccount>();
			alternateGLAccount.AGA_AAC_AlternateChart = chart.PK;
			chart.AAC_IsFixedLength = false;
			AssertHasError(chart.AAC_IsFixedLengthInfo, "Alternate GL Account is created for this chart, you cannot change Fixed Length.");

			alternateGLAccount.Delete();
			chart.AAC_Description = "1";
			chart.RunPreSaveValidation();
			AssertNoErrors(chart.AAC_IsFixedLengthInfo);
		}

		public void TestCheckAccountFormat()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.Validation.ValidateAll();
			AssertHasRowError(chart, "The Alternate Chart of Accounts should have at least one Tier.");
		}

		public void TestCheckReportOrder()
		{
			var chart = Factory.NewWithValidTestData<AccAlternateChart>();
			chart.AAC_ReportOrder = "";
			AssertHasError(chart.AAC_ReportOrderInfo, "Please enter a Report Order.");
			chart.AAC_ReportOrder = "1";
			AssertHasError(chart.AAC_ReportOrderInfo, "Please specify a valid value. It must be 'PTB' or 'BTP'");
			chart.AAC_ReportOrder = AccAlternateChartLookups.ReportOrderCode.PTB;
			AssertNoErrors(chart.AAC_ReportOrderInfo);
		}
	}
}
