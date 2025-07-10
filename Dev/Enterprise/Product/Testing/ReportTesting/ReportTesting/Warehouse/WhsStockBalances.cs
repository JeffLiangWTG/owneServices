namespace Enterprise.ReportTesting.Warehouse
{
	using System;
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.ReportTesting;
	using Enterprise.Warehouse.Transactions.Business.Testing;
	using Enterprise.Warehouse.Transactions.Module;

	[TemplateName("Whs Stock Balances")]
	public class TestWhsStockBalancesReport : WhsTemplateTestCase
	{
		public void TestPickAreaDoesNotBreakReport()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var warehouse = data.Whs1;
			var client = data.Org1;
			var location = data.Whs1.FindLocation("A-1");
			var product = data.Part1;

			WarehouseHelper.CreateWhsReceiveWithInventory(client, warehouse, "R1", product, 100m, location, "");

			Factory.Save();

			PrepareReportForRender();
			SelectAllOptionalTemplates();

			((LookupField)Report.FilterCollection["Client"]).ValueAsStringForSerialisation = data.Org1.PK.ToString();
			((LookupField)Report.FilterCollection["Warehouse"]).ValueAsStringForSerialisation = data.Whs1.PK.ToString();
			((DateField)Report.FilterCollection["As At Date"]).ValueAsStringForSerialisation = DateTime.Now.ToShortTimeString();

			AssertNoExceptionThrown(() => RunReport());
		}

		WhsTestHelperFunctions WarehouseHelper => warehouseHelper ?? (warehouseHelper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions warehouseHelper;
	}

	public class TestWhsStockBalancesReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReportModule(); }
		}

		public override string MenuName
		{
			get { return "Stock Balances Report"; }
		}

		public override string Hint
		{
			get
			{
				return
					@"This report shows a snapshot of inventory as at a specified date.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestWhsStockBalancesReport();
		}
	}
}
