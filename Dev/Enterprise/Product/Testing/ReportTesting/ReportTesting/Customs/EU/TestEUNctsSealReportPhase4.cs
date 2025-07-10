using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.Customs.EU
{
	[TemplateName("EU NCTS Seal Report (Phase 4)")]
	public class TestEUNctsSealReportPhase4 : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("DE");
		}
	}
}
