using Enterprise.Rating.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.SalesMarketing
{
	[TemplateName("Organisation Estimated Value Analysis By Opportunity Report")]
	public class OrganisationEstimatedValueAnalysisByOpportunityReportTemplateTest : TemplateTestCase
	{
	}

	public class OrganisationEstimatedValueAnalysisByOpportunityReportReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest
		{
			get { return new SalesMgrReports(); }
		}

		public override string MenuName
		{
			get { return "Organization - Estimated Value Analysis by Opportunity Report"; }
		}

		public override string Hint
		{
			get { return ""; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new OrganisationEstimatedValueAnalysisByOpportunityReportTemplateTest();
		}
	}
}
