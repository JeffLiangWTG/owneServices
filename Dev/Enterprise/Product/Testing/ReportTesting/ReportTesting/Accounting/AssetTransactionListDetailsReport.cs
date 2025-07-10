#region Tests

#if DEBUG

namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Asset Transaction List Details Report")]
	public class TestAssetTransactionListDetailsReport : TemplateTestCase
	{
	}

	public class TestAssetTransactionListDetailsReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.AssetManagementReports(); }
		}

		public override string MenuName
		{
			get { return "Asset Transaction List Details"; }
		}

		public override string Hint
		{
			get
			{
				return @"The Asset Transactions List Details Report, reports the list of all information currently entered on Assets and, in additional, the list of all transactions related to them. 
This report is designed for use to preview or email an XLS file to yourself.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestAssetTransactionListDetailsReport();
		}
	}
}
#endif

#endregion
