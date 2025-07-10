namespace Enterprise.ReportTesting.Freight.Shared
{
	[TemplateName("Local Transport Job Profile Report")]
	public class TestLocalTransportJobProfileReport : TemplateTestCase
	{
	}

	public class TestLocalTransportJobProfileReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Freight.Module.TransportReports(); }
		}

		public override string MenuName
		{
			get { return "Port Transport Job Profile Report"; }
		}

		public override string Hint
		{
			get
			{
				return "Similar to the \"export the grid\"" + @" option from the Transport > Port Transport module.
This report lists each Port Transport job.
It details the main transport job details and Job Invoicing Status, Branch, Department, Status etc.
Report is designed for use in Excel";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestLocalTransportJobProfileReport();
		}
	}
}
