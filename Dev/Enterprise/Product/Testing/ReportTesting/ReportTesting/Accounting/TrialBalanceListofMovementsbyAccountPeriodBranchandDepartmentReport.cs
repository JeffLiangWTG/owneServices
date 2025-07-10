namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Trial Balance - List of Movements by Account  Period  Branch and Department")]
	public class TestTrialBalanceListofMovementsbyAccountPeriodBranchandDepartmentReport : TemplateTestCase
	{
	}

	public class TestTrialBalanceListofMovementsbyAccountPeriodBranchandDepartmentReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.GLReports(); }
		}

		public override string MenuName
		{
			get { return "Trial Balance - List of Movements by Account  Period  Branch and Department"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Trial Balance - Movements by Account, Period, Branch and Department report is designed for users who need a data file that can be integrated with their own internal business reporting tools.

The report produces a list of net movements for each branch and department, by P&L/BSH account and period. This report produces a simple data list that can be used as a base for other general ledger reporting and analysis. 

Note: Movements in your AR and AP Control accounts are only reported at a TOTAL company movement by Period. Movements in these accounts cannot be dissected at a branch / department level.

The report is designed for use as an excel file. For selected accounting periods, it produces a listing of net movements by general ledger account + period + branch + department. The report is essentially a “dump” of summary movements in your general ledger accounts. The report does not include header accounts, total accounts or any other non-transaction general ledger account.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestTrialBalanceListofMovementsbyAccountPeriodBranchandDepartmentReport();
		}
	}
}
