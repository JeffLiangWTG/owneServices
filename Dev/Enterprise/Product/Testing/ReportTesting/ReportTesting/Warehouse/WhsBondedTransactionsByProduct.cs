namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.ReportTesting;
	using Enterprise.Warehouse.Transactions.Module;

	[TemplateName("Whs Bonded Transactions By Product")]
	public class TestWhsBondedTransactionsByProduct : WhsTemplateTestCase
	{
	}

	public class TestTestWhsBondedTransactionsByProduct_ReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReportModule(); }
		}

		public override string MenuName
		{
			get { return "Transaction History By MRN (Bonded) Report"; }
		}

		public override string Hint
		{
			get
			{
				return

					@"";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestWhsBondedTransactionsByProduct();
		}
	}
}
