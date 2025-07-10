namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Asset Book Report")]
	public class AssetBook : TemplateTestCase
	{
	}

	public class TestAssetBookReportMenuSetup : ReportTestCase
	{
		public override string MenuName
		{
			get { return "Asset Book"; }
		}

		public override string Hint
		{
			get { return "The Asset Book Report, reports the list of all active assets with all informations currently entered on Assets. In additional, must be noted, for each fiscal year, the purchase amount, the depreciation amount, the value of the depreciation fund and the residual value of asset."; }
		}

		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.AssetManagementReports(); }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new AssetBook();
		}
	}
}
