namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Freight Hold Status Overrides")]
	class TestFreightHoldStatusOverridesTemplate : TemplateTestCase
	{
	}

	public class TestFreightHoldStatusOverridesReport : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Freight Hold Status Overrides"; }
		}

		public override string Hint
		{
			get { return @""; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestFreightHoldStatusOverridesTemplate();
		}
	}
}
