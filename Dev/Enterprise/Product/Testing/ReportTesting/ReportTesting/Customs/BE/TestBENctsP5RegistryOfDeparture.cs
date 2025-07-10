using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ReportTesting.Customs.BE
{
	[TemplateName("BE NCTS P5 Registry of Departure")]
	public class TestNCTSP5RegistryOfDepartureReportTemplate : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("BE");
		}
	}

	public class TestNCTSP5RegistryOfDepartureReport : ReportTestCase
	{
		public override string MenuName => "NCTS P5 Registry of Departure";

		public override string Hint => "This report contains all NCTS P5 Departures with its previous procedure.";

		protected override TemplateTestCase GetTemplateTestCase() => new TestNCTSP5RegistryOfDepartureReportTemplate();

		public override ModuleIdentifier ModuleIdToTest => ModuleIDs.Customs.EU.NctsReportsModule;
	}
}
