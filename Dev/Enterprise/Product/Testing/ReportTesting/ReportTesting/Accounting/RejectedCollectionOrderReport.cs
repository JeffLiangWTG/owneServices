namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Rejected Collection Order Report")]
	class RejectedCollectionOrderReport : TemplateTestCase
	{
	}

	public class TestRejectedCollectionOrderReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new Enterprise.Accounting.Module.ReceivReports(); }
		}

		public override string MenuName
		{
			get { return "Rejected Collection Orders Analysis"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report analyses Collection Orders that have been refused payment by debtors.
For a selected Order Collection and/or Collection Batch Date Range, this report lists Collection Orders that have been refused payment by debtors with the rejection reasons.
A comprehensive sets of filters options assist users with the analysis of rejected collections by debtor, settlement group, credit rating, etc.
";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new RejectedCollectionOrderReport();
		}
	}
}
