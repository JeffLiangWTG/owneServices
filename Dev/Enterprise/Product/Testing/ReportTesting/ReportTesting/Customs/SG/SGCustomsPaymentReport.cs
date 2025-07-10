namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;

	[TemplateName("SG Customs Payment")]
	public class TestSGCustomsPaymentReportTemplate : TemplateTestCase
	{
	}

	public class TestSGCustomsPaymentReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "SG Customs Payment Report"; }
		}

		public override string Hint
		{
			get
			{
				return

						@"The report produces a listing of payments on Singapore Customs Declaration Entries.

For a specified date range the report identifies declaration payment details including branch, job, broker, entry type and style, importer, supplier and various reference and payment details.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestSGCustomsPaymentReportTemplate();
		}
	}
}
