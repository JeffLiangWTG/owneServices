namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Payable Order Line Report")]
	public class TestPayablesOrderLineReportTemplate : TemplateTestCase
	{
	}

	public class TestPayablesOrderLineReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.PayablesReports(); }
		}

		public override string MenuName
		{
			get { return "Payable Order Line Report"; }
		}

		public override string Hint
		{
			get
			{
				return

@"The Payables Order Line Report shows order line details for a selected status/disposition.
For each order line, the report will list selected order line and order header details.
Additionally the order line quantity and price values are sub totaled per order.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestPayablesOrderLineReportTemplate();
		}
	}
}
