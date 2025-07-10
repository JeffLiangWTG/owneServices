namespace Enterprise.ReportTesting.Accounting
{
	using System.Collections.Generic;
	using Enterprise.Accounting.Module;

	[TemplateName("China Reports Breakdown By Categories")]
	public class TestChinaReportsBreakdownByCategories : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Balance Sheet", "Profit And Loss Monthly", "Profit And Loss Yearly", "Profit Appropriation Statement" };
		}
	}

	public class TestChinaReportsBreakdownByCategoriesMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new GLReports(); }
		}

		public override string MenuName
		{
			get { return "China Reports Breakdown By Categories"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report provides the detailed breakdown (by GL Account) of each report category as listed in these China reports.
	1. China Balance Sheet
	2. China Profit and Loss – Monthly
	3. China Profit and Loss – Yearly
	4. China Profit Appropriation Statement

For each report category, the balances of each GL Accounts assigned to that category will be listed with the total matching the balances listed in the corresponding report with the same filters values.
In this report, both the GL Mapping Account Number and the Parent Account Number will be provided.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestChinaReportsBreakdownByCategories();
		}
	}
}
