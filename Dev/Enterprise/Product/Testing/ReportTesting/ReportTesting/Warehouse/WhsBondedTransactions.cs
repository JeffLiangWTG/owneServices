namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.ReportTesting;
	using Enterprise.Warehouse.Transactions.Module;

	[TemplateName("Whs Bonded Transactions")]
	public class TestWhsBondedTransactionsReport : WhsTemplateTestCase
	{
	}

	public class TestTestWhsBondedTransactions_ReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReportModule(); }
		}

		public override string MenuName
		{
			get { return "Transaction History (Bonded) Report"; }
		}

		public override string Hint
		{
			get
			{
				return

					@"This report displays details of transactions into and out of your bonded warehouses. Grouped by Entry number and product, the report shows quantities, references, date, duty and origin. Filters allow you to restrict the report to nominated Warehouse, Client,  Product, Commodity or Entry.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestWhsBondedTransactionsReport();
		}
	}
}
