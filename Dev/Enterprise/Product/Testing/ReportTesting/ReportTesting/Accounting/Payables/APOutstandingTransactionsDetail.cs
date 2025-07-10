namespace Enterprise.ReportTesting.Accounting.Payables
{
	using Enterprise.Accounting.Module;

	public class APOutstandingTransactionsDetailReportTest : ARAPOutstandingTransactionsDetailReportTest
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new PayablesReports(); }
		}
	}
}
