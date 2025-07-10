namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;

	[TemplateName("Commercial Invoice Lines by Product")]
	public class TestCommercialInvoiceLinesbyProduct : TemplateTestCase
	{
	}

	public class TestCommercialInvoiceLinesbyProductMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Commercial Invoice Lines by Product"; }
		}

		public override string Hint
		{
			get
			{
				return
@"The Commercial Invoice Lines by Product report produces a list of invoice lines on customs declarations matching the selection criteria.

The report shows for the nominated date range, the invoice line details including product, journey, supplier, tariff, quantity and value details. 

The report offers selectable columns to allow the user to tailor the report.  Use this report to identify products imported or exported in a particular time frame.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCommercialInvoiceLinesbyProduct();
		}
	}
}
