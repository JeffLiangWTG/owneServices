namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;

	[TemplateName("Customs Entries Invoice Lines")]
	public class TestCustomsEntriesInvoiceLinesReport : TemplateTestCase
	{
	}

	public class TestCustomsEntriesInvoiceLinesReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Customs Entries Invoice Lines"; }
		}

		public override string Hint
		{
			get
			{
				return @"The report lists all Invoice Lines for multiple Customs Declarations & Customs Entries created within a selected date range.
It shows Product Code, Description, Reference, Quantity, Tariff, Customs and Unit Value, Duty, Other Duties, Entry No. and Date.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestCustomsEntriesInvoiceLinesReport();
		}
	}
}
