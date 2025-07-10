namespace Enterprise.ReportTesting.Accounting
{
	[TemplateName("Job Profit - Transaction totals by Transaction Branch and Dept")]
	public class TestJobProfitTransactionTotalsbyBranchDept : TemplateTestCase
	{
		public override bool ReportUsesGeneratedSQL
		{
			get
			{
				return true;
			}
		}
	}
}
