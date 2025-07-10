namespace Enterprise.ReportTesting.Warehouse
{
	using Enterprise.DocumentEngine.RuntimeOptions;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ReportTesting;
	using Enterprise.Warehouse.Transactions.Business.Testing;
	using Enterprise.Warehouse.Transactions.Module;
	using NUnit.Framework;

	[TemplateName("Whs Stock On Hand Summary")]
	public class TestWhsStockOnHandSummaryReport : WhsTemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestZeroPalletSize()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			OrgPartUnit partUnit = data.Part1.PartUnits.AddNew();
			partUnit.OF_ParentPackType = "PLT";
			partUnit.OF_QuantityInParent = 0;

			var whsReceipt = WarehouseHelper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m, data.Whs1.FindLocation("A-1"), "", allocateLocations: false, finalise: true);

			Factory.Save();

			PrepareReportForRender();
			SelectAllOptionalTemplates();

			((LookupField)Report.FilterCollection["Client"]).ValueAsStringForSerialisation = data.Org1.PK.ToString();

			RunReport();
		}

		WhsTestHelperFunctions WarehouseHelper
		{
			get
			{
				if (fWarehouseHelper == null)
				{
					fWarehouseHelper = new WhsTestHelperFunctions(Factory);
				}
				return fWarehouseHelper;
			}
		}
		WhsTestHelperFunctions fWarehouseHelper;
	}

	public class TestWhsStockOnHandSummaryReportMenuSetup : ReportTestCase
	{
		public override Enterprise.ZArchitecture.Modules.ZEmbeddedModule ModuleToTest
		{
			get { return new ReportModule(); }
		}

		public override string MenuName
		{
			get { return "Stock On Hand Summary Report"; }
		}

		public override string Hint
		{
			get
			{
				return @"This report shows a snapshot of inventory at the time of printing the report. The data is sorted by Warehouse -> Client -> Product. When you need to know how much stock you currently have in your warehouses, this is the report to use.";
			}
		}

		protected override TemplateTestCase GetTemplateTestCase()
		{
			return new TestWhsStockOnHandSummaryReport();
		}
	}
}
