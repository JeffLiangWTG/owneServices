using Enterprise.Rating.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.SalesMarketing
{
	[TemplateName("Organisation Value Analysis Report")]
	public class OrganisationValueAnalysisReportTemplateTest : TemplateTestCase
	{
	}

	public class OrganisationValueAnalysisReportReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new SalesMgrReports(); }
		}

		public override string MenuName
		{
			get { return "Organization - Value Analysis Report"; }
		}

		public override string Hint
		{
			get { return ""; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new OrganisationValueAnalysisReportTemplateTest();
		}
	}
}
