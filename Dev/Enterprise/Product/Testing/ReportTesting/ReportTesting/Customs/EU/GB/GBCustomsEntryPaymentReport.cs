using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.EU.GB
{
	[TemplateName("GB Customs Entry Payment Report")]
	public class TestGBCustomsEntryPaymentReportTemplate : TemplateTestCase
	{
	}

	public class TestGBCustomsEntryPaymentReportReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "GB entry summary report"; }
		}

		public override string Hint
		{
			get
			{
				return @"GB duties & VAT summary report. 

Select clients, ports, branch, broker, mode or dates. Shows entry VAT (B00) and industrial product duty (A00), entry number(s) client and transport details.  Additional tax types (A30, 411) can be optionally shown.  Taxes are referenced using their box 47 type codes. 

Limited internally to show only entries awarded a route or a probable route, or entries that have cleared.  Therefore it excludes entries that are in status 'error', 'failed from transmission' or 'cancelled'. ";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestGBCustomsEntryPaymentReportTemplate();
		}
	}
}
