namespace Enterprise.ReportTesting.Accounting.Receivables
{
	using Enterprise.Accounting.Module;

	public class AROutstandingTransactionsDetailReportTest : ARAPOutstandingTransactionsDetailReportTest
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReceivReports(); }
		}
	}
}
