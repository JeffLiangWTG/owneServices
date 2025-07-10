namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;

	[TemplateName("Invoice Lines Report")]
	public class TestInvoiceLinesReportTemplate : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}

	public class TestInvoiceLinesReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Invoice Lines Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"Invoice Lines Report";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestInvoiceLinesReportTemplate();
		}
	}
}
