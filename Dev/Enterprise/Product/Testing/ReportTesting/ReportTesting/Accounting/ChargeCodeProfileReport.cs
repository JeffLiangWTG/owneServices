using Enterprise.MasterFiles.Module;

namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Charge Code Profile")]
	public class ChargeCodeProfileTemplateTest : TemplateTestCase
	{
	}

	public class ChargeCodeProfileReportTest : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new AccountReports(); }
		}

		public override string MenuName
		{
			get { return "Charge Code Profile"; }
		}

		public override string Hint
		{
			get
			{
				return

					@"The Charge Code Profile Report will generate a detailed profile of all Charge Code set ups for the current login company.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new ChargeCodeProfileTemplateTest();
		}
	}
}
