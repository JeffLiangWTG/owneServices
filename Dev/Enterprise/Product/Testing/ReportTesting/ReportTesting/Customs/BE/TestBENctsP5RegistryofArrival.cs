using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.Customs.BE
{
	[TemplateName("BE NCTS P5 Registry of Arrival")]
	public class TestNCTSP5RegistryofArrivalReportTemplate : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("BE");
		}
	}
}
