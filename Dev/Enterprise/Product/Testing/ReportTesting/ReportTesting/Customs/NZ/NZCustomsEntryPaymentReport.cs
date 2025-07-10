using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.NZ
{
	[TemplateName("NZ Customs Entry Payment Report")]
	public class TestNZCustomsEntryPaymentReportTemplate : TemplateTestCase
	{
	}

	public class TestNZCustomsEntryPaymentReportReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "NZ Customs Entry Payment Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"The report produces a listing of deferred payments on NZ Declaration Entries.
For a specified date range the report identifies declaration payment details including branch, job, broker, entry type and style,  importer, supplier and various reference and payment details.  Filter options include Branch, Type, Importer, Supplier and declaration types and a range of journey / transport filters.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestNZCustomsEntryPaymentReportTemplate();
		}
	}
}
