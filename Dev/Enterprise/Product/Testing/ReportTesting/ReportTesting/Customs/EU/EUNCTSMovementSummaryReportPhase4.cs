using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.Customs.EU
{
	[TemplateName("NCTS Movement Summary Report (Phase 4)")]
	public class EUNCTSMovementSummaryReportPhase4 : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("DE");
		}
	}
}
