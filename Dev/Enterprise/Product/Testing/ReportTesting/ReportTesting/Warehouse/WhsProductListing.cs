namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.ReportTesting;
	using Enterprise.Warehouse.Transactions.Module;

	[TemplateName("Whs Product Listing")]
	public class TestWhsProductListingReport : WhsTemplateTestCase
	{
	}

	public class TestWhsProductListingReportMenuSetup : ReportTestCase
	{
		public override ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReportModule(); }
		}

		public override string MenuName
		{
			get { return "Product Listing"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report is a straightforward list of products that have been entered from the Maintain > Warehouse > Products menu. Columns reported include Product Code, Description, Commodity Code, UNDG Code, Pallet Size, Stock on Hand and ABC Category. The report allows filtering by Client, Product, ABC Category, Warehouse, Commodity code, Product Category and Product Active Status.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestWhsProductListingReport();
		}
	}
}
