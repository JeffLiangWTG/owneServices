using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Opportunity Weighted Value Report")]
	public class OpportunityWeightedValueTemplate : TemplateTestCase
	{
		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			Factory.New<SalesTeam>();
			Factory.Save();
		}
	}

	public class TestOpportunityWeightedValueReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Rating.Module.SalesMgrReports(); }
		}

		public override string MenuName
		{
			get { return "Opportunity Weighted Value Report"; }
		}

		public override string Hint
		{
			get { return string.Empty; }
		}

		protected override bool ExpectedIsPublished
		{
			get { return false; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new OpportunityWeightedValueTemplate();
		}
	}
}
