using System.Collections.Generic;

namespace Enterprise.ReportTesting.Accounting.Receivables
{
	[TemplateName("AR Transactions By User")]
	public class TestARTransactionsByUserReport : TemplateTestCase
	{
		protected override List<string> GetExcludedSheetNames()
		{
			return new List<string> { "AR Transactions By User", "AR Transactions By Type User" };
		}
	}
}
