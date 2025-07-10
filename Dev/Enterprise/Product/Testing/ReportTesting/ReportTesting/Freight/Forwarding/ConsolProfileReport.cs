namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Consol Profile Report")]
	public class TestConsolProfileReport : TemplateTestCase
	{
	}

	public class TestConsolProfileReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Consol Profile Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Consol Profile Report produces a detailed table of data profiling each consol matching the selection criteria. It is designed to be used as an Excel file, not as a printed document. The layout is compatible with Excel’s sort, auto filter and subtotal functions.
Please note, job profit, revenue, WIP, cost and accrual columns are included in this report. For a consol these values are the sum of jobs on shipments attached to each consol.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestConsolProfileReport();
		}
	}
}
