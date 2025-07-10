namespace Enterprise.ReportTesting.Accounting.Payables
{
	using System.Collections.Generic;
	using Enterprise.Accounting.Module;

	[TemplateName("AP Outstanding Balances Currency Revaluation at Period End Rates")]
	public class APOutstandingBalancesCurrencyRevaluationAtPeriodEndRatesTemplateTest : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "AP Balances Revaluation Detail", "AP Balances Revaluation Summary" };
		}
	}

	public class APOutstandingBalancesCurrencyRevaluationAtPeriodEndRatesReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new PayablesReports(); }
		}

		public override string MenuName
		{
			get { return "Outstanding Balances Currency Revaluation at Period End Rates"; }
		}

		public override string Hint
		{
			get
			{
				return

					@"By transaction currency this report assists in the calculation of unrealized exchange gain (or loss) on outstanding payable balances as at the end of a nominated period.
Use this report to assist in calculating a provision for unrealized exchange gain or loss on payable balances at period end.
For each currency the report calculates:
a) Outstanding amounts in invoiced currency.
b) Outstanding local currency equivalent amounts held in the Payables Ledger (calculated using each transaction's historical exchange rate).
c) Outstanding local currency equivalent amounts using the Period End Rate of each currency.
d) Calculation of potential Exchange Gain / Loss on closing balances using Period End Rates.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new APOutstandingBalancesCurrencyRevaluationAtPeriodEndRatesTemplateTest();
		}
	}
}
