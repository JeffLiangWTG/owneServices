namespace Enterprise.ReportTesting.Customs.Shared
{
	using Enterprise.Customs.Module;
	using Enterprise.ReportTesting;

	[TemplateName("Client Declaration Job Analysis")]
	public class TestClientDeclarationJobAnalysisReport : TemplateTestCase
	{
	}

	public class TestClientDeclarationJobAnalysisReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "Client Declaration Job Analysis"; }
		}

		public override string Hint
		{
			get
			{
				return

	@"The Client - Declaration Job Analysis by Client report provides a detailed listing of declaration jobs by client. 
Use this report to summarize trading performance by client and job. It supports a level of client analysis that details information (e.g. weights, volumes, consignees, consignors) as well as job profit.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestClientDeclarationJobAnalysisReport();
		}
	}
}
