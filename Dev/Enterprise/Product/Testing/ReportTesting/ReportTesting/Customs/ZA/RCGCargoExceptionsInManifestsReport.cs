using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.ZA
{
	[TemplateName("RCG Cargo Exceptions in Manifests")]
	public class TestRCGCargoExceptionsInManifestsReportTemplate : TemplateTestCase
	{
	}

	public class TestRCGCargoExceptionsInManifestsReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest => new CustomsGlobalReportModule();

		public override string MenuName => "RCG Cargo Reporting Exceptions in Manifests";

		public override string Hint => @"This report lists all Global Manifests together with their CUSCAR-CUSRES Status so that users can identify Manifests that have not yet achieved CUSRES8- PROCEED TO BORDER.

It only displays all House Bill records recorded in your CW1 database.

CUSCAR manifest message submitted to SARS-CPS (Cargo Processing System) are displayed in the left most columns therefore negative feedback and/or gaps in feedback in columns on the right indicate exceptions that require further attention.

This report can be used proactively to ensure that various types of CUSCAR manifests have been submitted and received successful outcomes.

If a master bill is not registered in Global Manifest jobs then unfortunately it cannot appear on this report.";

		protected override TemplateTestCase GetTemplateTestCase() => new TestRCGCargoExceptionsInManifestsReportTemplate();
	}
}
