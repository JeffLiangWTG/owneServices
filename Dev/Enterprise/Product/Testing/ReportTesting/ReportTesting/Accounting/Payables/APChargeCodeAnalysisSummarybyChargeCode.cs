namespace Enterprise.ReportTesting.Accounting.Payables
{
	using System.Collections.Generic;
	using Enterprise.Accounting.Module;

	[TemplateName("AP Charge Code Analysis - Summary by Charge Code")]
	public class APChargeCodeAnalysisSummarybyChargeCodeTemplateTest : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "Summary by Charge Code" };
		}
	}

	public class APChargeCodeAnalysisSummarybyChargeCodeReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new PayablesReports(); }
		}

		public override string MenuName
		{
			get { return "Charge Code Analysis - Summary by Charge Code"; }
		}

		public override string Hint
		{
			get
			{
				return

						@"The Payables Charge Code Analysis Summary by Charge Code report is used to analyze total turnover by charge code. For each charge code it identifies totals by payable account. The report analyses AP  Invoice, Credit Note and Adjustment Note transactions at the Transaction Line / Charge Code level.

Optional Charge Group (e.g. FRT, BRK, OBO, SDS) columns can be included.
These optional columns provide a dissection of costs by charge group.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new APChargeCodeAnalysisSummarybyChargeCodeTemplateTest();
		}
	}
}
