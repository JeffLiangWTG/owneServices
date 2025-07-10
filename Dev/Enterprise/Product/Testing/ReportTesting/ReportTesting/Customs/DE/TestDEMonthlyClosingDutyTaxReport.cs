using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.DE
{
	[TemplateName("DE Monthly Closing Duty Tax Report")]
	public class TestDEMonthlyClosingDutyTaxReportTemplate : TemplateTestCase
	{
	}

	class TestDEMonthlyClosingDutyTaxReport : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "DE Monthly Closing Duty Tax Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"DE Imports Monthly Closing - New Report Monthly Closing Duty Tax Report";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestDEMonthlyClosingDutyTaxReportTemplate();
		}
	}
}
