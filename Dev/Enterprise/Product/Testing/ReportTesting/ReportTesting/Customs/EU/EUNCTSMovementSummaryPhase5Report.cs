using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.Customs.EU
{
	[TemplateName("NCTS Movement Summary Report (Phase 5)")]
	public class EUNCTSMovementSummaryPhase5Report : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("DE");
		}
	}
}
