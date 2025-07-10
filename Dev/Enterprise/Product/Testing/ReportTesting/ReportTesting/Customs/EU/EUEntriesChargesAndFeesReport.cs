using Enterprise.Customs.Module;

namespace Enterprise.ReportTesting.Customs.EU
{
	[TemplateName("EU Entries Charges And Fees Report")]
	public class TestEUEntryChargesAndFeesReportReportTemplate : TemplateTestCase
	{
	}

	class TestEUEntriesChargesAndFeesReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new CustomsReportModule(); }
		}

		public override string MenuName
		{
			get { return "EU Entries Charges And Fees Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report will show, for a mandatory country and different options, the fees and charges calculated from Declarations, Entry Lines Fees and Entries charges.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestEUEntryChargesAndFeesReportReportTemplate();
		}
	}
}
