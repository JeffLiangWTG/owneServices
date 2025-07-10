using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class WhsWorkOrderAssemblyConfirmationUserControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestBinding()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateProductBOM(data.Part1, data.Part2, 2m, "UNT");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var workOrder = Helper.CreateWhsWorkOrderWithLine(data.Org1, data.Whs1, data.Part1, 5m);
			Helper.CreatePickNew(workOrder);
			workOrder.FinaliseDocketAlwaysFinalisingPick();
			AssertIsFinalisedPrecondition(workOrder);

			using (var form = new TestForm(workOrder))
			{
				form.Show();

				var grid = form.GetAssembledInventoryGrid();
				var columnStyles = grid.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertEquals(false, columnStyles.Single(i => i.ColumnName == "IsLeftoverComponent").IsUnavailable);

				var componentColumnStyles = form.GetLinkedComponentsGrid().ColumnStyles.Cast<ZGridColumnInfo>();
				AssertEquals(false, componentColumnStyles.Single(i => i.ColumnName == nameof(ComponentLineForAssembly.UnitsToPick)).IsUnavailable);
			}
		}

		class TestForm : ZForm
		{
			public TestForm(WhsWorkOrder order)
				: base(order.Receive)
			{
			}

			public WhsWorkOrderAssemblyConfirmationUserControl UserControl;
			public ZGrid GetAssembledInventoryGrid() => GUITestHelper.FindControl<ZGrid>(UserControl.Controls, "AssemblyGrid");
			public ZGrid GetLinkedComponentsGrid() => GUITestHelper.FindControl<ZGrid>(UserControl.Controls, "LinkedComponentsGrid");

			protected override void InitializeComponent()
			{
				UserControl = new WhsWorkOrderAssemblyConfirmationUserControl();
				UserControl.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				UserControl.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsReceive";

				Controls.Add(UserControl);
				DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsReceive";
			}
		}
	}
}
