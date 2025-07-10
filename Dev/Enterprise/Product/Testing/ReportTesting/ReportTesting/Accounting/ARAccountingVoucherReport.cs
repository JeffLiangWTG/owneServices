namespace Enterprise.ReportTesting.Accounting
{
	using NUnit.Framework;

	[TemplateName("CN AR Accounting Voucher Report")]
	public class TestCNARAccountingVoucherReport : TemplateTestCase
	{
		[ExpectNoExceptions]
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
