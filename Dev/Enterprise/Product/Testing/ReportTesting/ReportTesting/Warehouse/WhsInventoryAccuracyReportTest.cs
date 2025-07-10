using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ReportTesting.Warehouse
{
	public abstract class TestWhsInventoryAccuracyReport : WhsTemplateTestCase
	{
		[ExpectNoExceptions]
		public void TestZeroExpectedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var whsCycleCountLocation = WarehouseHelper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation, "PRD", CargoWise.Types.ZDateTimeOffset.Today.AddDays(-2), CargoWise.Types.ZDateTimeOffset.Today.AddDays(+2), "AAQ");
			var whsCycleCountLocationVar = WarehouseHelper.CreateWhsCycleCountLocationVariance(whsCycleCountLocation, "APP", "Sample Pallet ID", data.Org1, data.Part1, 3, "SampleAttr1", "SampleAttr2", "SampleAttr3", "SampleSerialNumber", null, null, 0, null, false, "");
			var whsAdjustment1 = WarehouseHelper.CreateWhsAdjustment(data.Org1, data.Whs1);
			whsCycleCountLocationVar.WCC_WD_RelatedAdjustment = whsAdjustment1.PK;
			Factory.Save();

			PrepareReportForRender();
			SelectAllOptionalTemplates();

			((LookupField)Report.FilterCollection["Client"]).ValueAsStringForSerialisation = data.Org1.PK.ToString();

			RunReport();
		}

		[ExpectNoExceptions]
		public void TestZeroCountedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var whsCycleCountLocation = WarehouseHelper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation, "PRD", CargoWise.Types.ZDateTimeOffset.Today.AddDays(-2), CargoWise.Types.ZDateTimeOffset.Today.AddDays(+2), "AAQ");
			var whsCycleCountLocationVar = WarehouseHelper.CreateWhsCycleCountLocationVariance(whsCycleCountLocation, "APP", "Sample Pallet ID", data.Org1, data.Part1, -5, "SampleAttr1", "SampleAttr2", "SampleAttr3", "SampleSerialNumber", null, null, 5, null, false, "");
			var whsAdjustment1 = WarehouseHelper.CreateWhsAdjustment(data.Org1, data.Whs1);
			whsCycleCountLocationVar.WCC_WD_RelatedAdjustment = whsAdjustment1.PK;
			Factory.Save();

			PrepareReportForRender();
			SelectAllOptionalTemplates();

			((LookupField)Report.FilterCollection["Client"]).ValueAsStringForSerialisation = data.Org1.PK.ToString();

			RunReport();
		}

		[ExpectNoExceptions]
		public void TestZeroExpectedQtyAndZeroCountedQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var whsCycleCountLocation = WarehouseHelper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation, "PRD", CargoWise.Types.ZDateTimeOffset.Today.AddDays(-2), CargoWise.Types.ZDateTimeOffset.Today.AddDays(+2), "AAQ");
			var whsCycleCountLocationVar = WarehouseHelper.CreateWhsCycleCountLocationVariance(whsCycleCountLocation, "APP", "Sample Pallet ID", data.Org1, data.Part1, 0, "SampleAttr1", "SampleAttr2", "SampleAttr3", "SampleSerialNumber", null, null, 0, null, false, "");

			Factory.Save();

			PrepareReportForRender();
			SelectAllOptionalTemplates();

			((LookupField)Report.FilterCollection["Client"]).ValueAsStringForSerialisation = data.Org1.PK.ToString();

			RunReport();
		}

		WhsTestHelperFunctions WarehouseHelper => fWarehouseHelper ?? (fWarehouseHelper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions fWarehouseHelper;
	}
}
