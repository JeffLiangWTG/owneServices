namespace Enterprise.ReportTesting.MasterFiles
{
	using Enterprise.ReportTesting;

	[TemplateName("Opportunity Value Analysis Report")]
	public class TestOpportunityValueAnalysisTemplate : TemplateTestCase
	{
		protected override bool ReportRequiresColumnHeadings => false;
	}

	public class TestOpportunityValueAnalysisReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Rating.Module.SalesMgrReports(); }
		}

		public override string MenuName
		{
			get { return "Opportunity Value Analysis"; }
		}

		public override string Hint
		{
			get { return @"List of Opportunities with Values grouped by Revenue Type"; }
		}

		protected override bool ExpectedIsPublished
		{
			get { return false; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestOpportunityValueAnalysisTemplate();
		}
	}
}
