using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	public class LocationFilterControlTest : WhsGuiTestCaseWithFactory
	{
		public void TestCycleCountLastPerformedVisibility()
		{
			TestCycleCountColumnVisibilityCore(nameof(WhsLocation.WLV_CycleCountLastPerformedForBinding));
		}

		public void TestCycleCountPathSequenceVisibility()
		{
			TestCycleCountColumnVisibilityCore(WhsLocationViewSchema.Constants.WLV_CycleCountPathSequence);
		}

		void TestCycleCountColumnVisibilityCore(string columnName)
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRow(warehouse, "row", 1, 1);

			using (var control = new LocationFilterControl(row.Locations, new LocationFilterBusinessObject()))
			{
				var form = new ZForm();
				form.Controls.Add(control);
				form.Show();

				AssertEquals($"Column '{columnName}' visibility should be enabled.", true, control.Grid.Columns.Contains(columnName));

				form.Dispose();
			}
		}

		#region TestPutawayPathSequenceColumnVisibility

		public void TestPutawayPathSequenceColumnVisibility()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRow(warehouse, "row", 1, 1);

			using (var control = new LocationFilterControl(row.Locations, new LocationFilterBusinessObject()))
			{
				var form = new ZForm();
				form.Controls.Add(control);
				form.Show();

				AssertEquals($"Column '{WhsLocationViewSchema.Constants.WLV_PutawayPathSequence}' should be visible", true, control.Grid.Columns.Contains(WhsLocationViewSchema.Constants.WLV_PutawayPathSequence));

				form.Dispose();
			}
		}

		#endregion

		#region TestPickPathSequenceColumnVisibility

		public void TestPickPathSequenceColumnVisibility()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var row = Helper.CreateRow(warehouse, "row", 1, 1);

			using (var control = new LocationFilterControl(row.Locations, new LocationFilterBusinessObject()))
			{
				var form = new ZForm();
				form.Controls.Add(control);
				form.Show();

				AssertEquals($"Column '{WhsLocationViewSchema.Constants.WLV_PickPathSequence}' should be visible", true, control.Grid.Columns.Contains(WhsLocationViewSchema.Constants.WLV_PickPathSequence));

				form.Dispose();
			}
		}

		#endregion
	}
}
