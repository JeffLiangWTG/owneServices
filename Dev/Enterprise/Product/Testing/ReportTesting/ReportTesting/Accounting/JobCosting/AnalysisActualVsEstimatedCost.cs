using Enterprise.Accounting.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Analysis - Actual vs Estimated Cost")]
	class AnalysisActualVsEstimatedCost : TemplateTestCase
	{
	}

	public class TestAnalysisActualVsEstimatedCostMenuSetup : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new JobCostingReportModule(); }
		}

		public override string MenuName
		{
			get { return "Analysis - Actual vs. Estimated Cost"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Analysis - Actual vs. Estimated Cost Report analyses the variance between the Estimated Cost and Actual Cost of Job charge lines. Users can use this report:
1) to analyze the variances between what was quoted and eventually billed by the Creditors, or
2) to monitor the variances between what was accrued by operators (via Estimated Cost) and the actual billing by the Creditors.

In order to ensure a more accurate and stable environment in analyzing the cost variances, it is strongly recommended that this report should be generated for closed jobs only. In addition,

1) The Registry 'Accrual Reversal Behavior then Allocated to Creditor' is set to Yes;
2) The Registry 'Accrual Must have Creditor Code' set to Yes;
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new AnalysisActualVsEstimatedCost();
		}
	}
}
