using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.Customs.DE
{
	[TemplateName("DE Logbook")]
	public class TestDELogbook : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("DE");
		}
	}

	[TemplateName("DE Logbook NonProduction")]
	public class TestDELogbookNonProduction : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("DE");
		}
	}
}
