using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	public class RoutePlannerGUICustomColumnsInitializerTest : TestCaseWithFactory
	{
		#region TestAddCustomColumns

		public void TestAddCustomColumns()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();

			using (var form = new ZForm(consignment))
			using (var grid = new ZGrid())
			{
				var column = new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" };
				grid.Columns.Add(column);

				form.Controls.Add(grid);
				grid.SetDataBinding(consignment.Instructions, "");

				form.Show();
				AssertEquals("Precondition: New grid should have 1 columns.", 1, grid.Columns.Count);

				var container = new CustomPropertyContainer();
				container.AddCustomProperty("Test", typeof(ZInt), (a) => { return 1; });

				var columnInitializer = new RoutePlannerGUICustomColumnsInitializer(grid, consignment.Instructions, null, container);
				columnInitializer.AddCustomColumns();
				System.Windows.Forms.Application.DoEvents();
				AssertEquals("Grid should have 2 column.", 2, grid.Columns.Count);
				AssertColumn(grid, "Test", "Test ");
				AssertColumnValue(grid, "Test", 0, "1");
			}
		}

		void AssertColumn(ZGrid grid, string columnName, string caption)
		{
			var column = grid.Columns[columnName];
			AssertEquals(columnName + " Caption", caption, column.ColumnStyle.HeaderText);
			AssertEquals(columnName + " Visible", true, column.IsVisible);
			AssertEquals(columnName + " ReadOnly", true, column.ColumnStyle.ReadOnly);
		}

		void AssertColumnValue(ZGrid grid, string columnName, int rowNum, string value)
		{
			var column = grid.Columns[columnName];
			AssertEquals(value, ((ZGridColumnStyle)column.ColumnStyle).GetValueAsString(grid.ListManager, rowNum));
		}

		#endregion

		#region Implementation

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
