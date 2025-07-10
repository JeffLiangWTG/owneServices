namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Asset List Report")]
	class AssetList : TemplateTestCase
	{
	}

	public class AssetListTest : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.AssetManagementReports(); }
		}

		public override string Hint
		{
			get { return @"The Asset Report List, reports the lists of all information currently entered on Assets.
This report is designed for use to preview or email an XLS file to yourself.

The Asset List report lists the information recorded on the assets, including the key asset details, GL accounts and depreciation methods."; }
		}

		public override string MenuName
		{
			get { return "Asset List"; }
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new AssetList();
		}
	}
}
