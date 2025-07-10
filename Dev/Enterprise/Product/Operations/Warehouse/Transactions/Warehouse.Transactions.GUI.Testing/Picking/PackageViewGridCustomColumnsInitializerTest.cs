using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Test
{
	public class PackageViewGridCustomColumnsInitializerTest : TestCaseWithFactory
	{
		#region TestAddCustomColumns

		public void TestAddCustomColumns()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);

			using (var form = new ZForm(pick))
			using (var grid = new ZGrid())
			{
				var column = new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" };
				grid.Columns.Add(column);

				form.Controls.Add(grid);
				grid.SetDataBinding(pick.OuterPackages, "");

				form.Show();
				AssertEquals("Precondition: New grid should have 1 columns.", 1, grid.Columns.Count);

				var container = new CustomPropertyContainer();
				container.AddCustomProperty("Test", typeof(ZString), (a) => { return "Custom Column"; });

				var columnInitializer = new PackageViewGridCustomColumnsInitializer(grid, pick.OuterPackages, null, container);
				columnInitializer.AddCustomColumns();
				System.Windows.Forms.Application.DoEvents();
				AssertEquals("Grid should have 2 column.", 2, grid.Columns.Count);
				var expectedCaption = "Test";
				AssertColumn(grid, "Test", expectedCaption);
				AssertColumnValue(grid, "Test", 0, "Custom Column");
			}
		}

		void AssertColumn(ZGrid grid, string columnName, string expectedCaption)
		{
			var column = grid.Columns[columnName];
			AssertEquals("Caption", expectedCaption, column.ColumnStyle.HeaderText);
			AssertEquals("Visible", true, column.IsVisible);
			AssertEquals("ReadOnly", true, column.ColumnStyle.ReadOnly);
		}

		void AssertColumnValue(ZGrid grid, string columnName, int rowNum, string value)
		{
			var column = grid.Columns[columnName];
			AssertEquals(value, ((ZGridColumnStyle)column.ColumnStyle).GetValueAsString(grid.ListManager, rowNum));
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}