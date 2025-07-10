using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("Opportunity Pipeline Report")]
	public class OpportunityPipelineReport : TemplateTestCase
	{
		protected override void FillReportWithDefaultValues()
		{
			base.FillReportWithDefaultValues();
			Factory.New<SalesTeam>();
			Factory.Save();
		}
	}

	public class TestOpportunityPipelineReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Rating.Module.SalesMgrReports(); }
		}

		public override string MenuName
		{
			get { return "Opportunity Pipeline"; }
		}

		public override string Hint
		{
			get
			{
				return @"List of Opportunities with Values grouped by Revenue Type

You are able to filter this report by Sales Rep, Sales Team, Sales Type, Closed Date, Campaign, Value, Estimated Close Date, Close Certainty";
			}
		}

		protected override bool ExpectedIsPublished
		{
			get { return false; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new OpportunityPipelineReport();
		}
	}
}
