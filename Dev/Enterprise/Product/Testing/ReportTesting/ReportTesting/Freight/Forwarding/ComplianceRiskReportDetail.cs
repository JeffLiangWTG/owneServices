namespace Enterprise.ReportTesting.Freight.Forwarding
{
	using Enterprise.Freight.Forwarding.Module;

	[TemplateName("Compliance Risk Report (Detail)")]
	public class TestComplianceRiskReportDetailTemplate : TemplateTestCase
	{
	}

	public class TestComplianceRiskReportDetail : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ForwardingReportsModule(); }
		}

		public override string MenuName
		{
			get { return "Compliance Risk Report (Detail)"; }
		}

		public override string Hint
		{
			get { return @"This report is designed to support the management of your organization's International Forwarding Compliance.

For the nominated date range, the report focuses on Shipment jobs which have been exported within the nominated date range. 

The report will provide insights into your Compliance Risk exposure for specific Jobs"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestComplianceRiskReportDetailTemplate();
		}
	}
}
