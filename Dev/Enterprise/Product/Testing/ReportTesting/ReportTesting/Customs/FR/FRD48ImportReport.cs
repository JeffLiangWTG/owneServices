using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ReportTesting.Customs.FR
{
	[TemplateName("D48 Report")]
	public class TestFRD48ImportReport : TemplateTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("FR");
		}
	}

	public class TestFRD48ImportReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "D48 Report"; }
		}

		public override string Hint
		{
			get
			{
				return string.Empty;
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestFRD48ImportReport();
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("FR");
		}
	}
}
