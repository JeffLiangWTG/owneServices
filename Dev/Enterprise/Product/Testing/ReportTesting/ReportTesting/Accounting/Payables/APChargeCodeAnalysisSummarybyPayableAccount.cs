namespace Enterprise.ReportTesting.Accounting.Payables
{
	using System.Collections.Generic;
	using Enterprise.Accounting.Module;

	[TemplateName("AP Charge Code Analysis - Summary by Payable Account")]
	public class APChargeCodeAnalysisSummarybyPayableAccountTemplateTest : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Summary by Payable Account" };
		}
	}

	public class APChargeCodeAnalysisSummarybyPayableAccountReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new PayablesReports(); }
		}

		public override string MenuName
		{
			get { return "Charge Code Analysis - Summary by Payable Account"; }
		}

		public override string Hint
		{
			get
			{
				return

						@"The Payables Charge Code Analysis Summary by Payable Account report is used to analyze total turnover by payable account and charge code. For each payable account, the report identifies totals by charge code. The report analyses AP  Invoice, Credit Note and Adjustment Note transactions at the Transaction Line / Charge Code level.

Optional Charge Group (e.g. FRT, BRK, OBO, SDS) columns can be included.
These optional columns provide a dissection of costs by charge group.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new APChargeCodeAnalysisSummarybyPayableAccountTemplateTest();
		}
	}
}
