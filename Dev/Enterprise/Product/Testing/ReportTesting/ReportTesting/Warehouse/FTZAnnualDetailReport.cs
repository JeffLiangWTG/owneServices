namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.Warehouse.Transactions.Module;

	[TemplateName("FTZ Annual Detail")]
	public class TestFTZAnnualDetailReport : WhsTemplateTestCase
	{
	}

	public class TestFTZAnnualDetailReport_ReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReportModule(); }
		}

		public override string MenuName
		{
			get { return "FTZ Annual Report (Detail)"; }
		}

		public override string Hint => string.Empty;

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestFTZAnnualDetailReport();
		}
	}
}
