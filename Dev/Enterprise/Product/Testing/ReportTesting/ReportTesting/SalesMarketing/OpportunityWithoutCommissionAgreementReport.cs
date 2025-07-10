using Enterprise.Rating.Module;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Opportunity Without Commission Agreement")]
	public class OpportunityWithoutCommissionAgreementTemplateTest : TemplateTestCase
	{
	}

	public class OpportunityWithoutCommissionAgreementReportTest : ReportTestCase
	{
		public override ZEmbeddedModule ModuleToTest => new SalesMgrReports();

		public override string MenuName => "Opportunity Without Commission Agreement";

		public override string Hint => "This report shows the list of Opportunities that do not have a valid Commission Agreement associated.";

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new OpportunityWithoutCommissionAgreementTemplateTest();
		}
	}
}
