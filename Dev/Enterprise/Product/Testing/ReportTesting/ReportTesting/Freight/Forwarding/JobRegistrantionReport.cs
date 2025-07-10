namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using System.Collections.Generic;
	using Enterprise.Freight.Forwarding.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Job Registration Report")]
	public class TestJobRegistrantionReport : TemplateTestCase
	{
		protected override IEnumerable<string> SheetsNotRequiringColumnHeadings
		{
			get
			{
				return new string[]
				{
					"Template"
				};
			}
		}
	}

	public class TestJobRegistrantionReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Job Registration"; }
		}

		public override string Hint
		{
			get
			{
				return

@"The Job Registration Report provides a count of the Job Invoicing Tab's created on a particular day.  

This report is run by date.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestJobRegistrantionReport();
		}
	}
}
