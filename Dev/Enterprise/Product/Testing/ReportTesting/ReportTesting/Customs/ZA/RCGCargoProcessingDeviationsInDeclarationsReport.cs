using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("RCG Cargo Processing Deviations in Declarations")]
	public class TestRCGCargoProcessingDeviationsInDeclarationsReportTemplate : TemplateTestCase
	{
	}

	public class TestRCGCargoProcessingDeviationsInDeclarationsReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsReportModule();

		public override string MenuName => "RCG Cargo Processing Deviations in Declarations";

		public override string Hint => @"This report identifies Declarations with House Bills that are not cross-matched to any Manifests in the same company, i.e. it lists all Declarations with a House Bill, irrespective of whether the House Bills are matched to a Manifest job or not.

Therefore, gaps and/or discrepancies in ""Matched to Manifest"" columns on the left hand side of this report indicate potential deviations that need to be handled in order to avoid consequences due to 'RCG deviations'.

The root-cause of deviations might occur in either Declarations or Manifests so both Jobs may need to be reviewed and/or revised to eliminate potential deviations.";

		protected override TemplateTestCase GetTemplateTestCase() => new TestRCGCargoProcessingDeviationsInDeclarationsReportTemplate();
	}
}
