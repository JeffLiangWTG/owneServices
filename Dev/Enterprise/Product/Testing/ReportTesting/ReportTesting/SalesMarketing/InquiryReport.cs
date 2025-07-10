using Enterprise.Rating.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Inquiry Report")]
	public class InquiryReportTemplateTest : TemplateTestCase
	{
	}

	public class InquiryReportReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new SalesMgrReports(); }
		}

		public override string MenuName
		{
			get { return "Inquiry Report"; }
		}

		public override string Hint
		{
			get { return @"The Inquiry Report lists all inquiries in the system."; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new InquiryReportTemplateTest();
		}
	}
}
