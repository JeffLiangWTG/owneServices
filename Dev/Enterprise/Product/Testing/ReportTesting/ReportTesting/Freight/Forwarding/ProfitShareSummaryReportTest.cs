namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;

	[TemplateName("Profit Share Summary Report")]
	public class TestProfitShareSummaryReport : TemplateTestCase
	{
	}

	public class TestProfitShareSummaryReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Profit Share Summary"; }
		}

		public override string Hint
		{
			get
			{
				return
@"This report lists details of all profit share transactions with details of the shipment, consol and profit share amounts. 

Currently this report will only report profit share that is based on the freight amounts only.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestProfitShareSummaryReport();
		}
	}
}
