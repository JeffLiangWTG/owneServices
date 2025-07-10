namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Job Count Summary by Branch")]
	public class TestJobCountSummaryByBranch : TemplateTestCase
	{
	}

	public class TestTestJobCountSummaryByBranchMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Job Count Summary by Branch"; }
		}

		public override string Hint
		{
			get
			{
				return @"By Accounting Job Header Branch, this report provides a high level summary of job counts, TEU, KG and CBM by Branch, Department, Transport Mode and Freight Direction.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobCountSummaryByBranch();
		}
	}
}
