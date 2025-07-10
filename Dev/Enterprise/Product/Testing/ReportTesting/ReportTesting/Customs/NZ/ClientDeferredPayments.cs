using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.NZ
{
	[TemplateName("Client Deferred Payments")]
	public class TestClientDeferredPaymentsTemplate : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings
		{
			get { return false; }
		}
	}

	public class TestClientDeferredPaymentsReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Client Deferred Payments"; }
		}

		public override string Hint
		{
			get
			{
				return @"The report produces a listing of Client Deferred Payments on NZ Declaration Entries.

For a specified date range the report identifies declaration payment details by branch and importer.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestClientDeferredPaymentsTemplate();
		}
	}
}
