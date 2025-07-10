namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("CN AP Accounting Voucher Report")]
	public class TestCNAPAccountingVoucherReport : TemplateTestCase
	{
		[NUnit.Framework.ExpectNoExceptions]
		public void TestReportOnlyRequiredFilters()
		{
			PrepareReportForRender();
			RunReport();
		}

		protected override bool ReportRequiresColumnHeadings
		{
			get
			{
				return false;
			}
		}
	}
}
