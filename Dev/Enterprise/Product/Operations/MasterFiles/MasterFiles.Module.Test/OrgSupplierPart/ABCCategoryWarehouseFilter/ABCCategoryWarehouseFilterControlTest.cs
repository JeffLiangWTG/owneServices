using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class ABCCategoryWarehouseFilterControlTest : KUserControlTest
	{
		#region TestWarehouseCodeBoxIsReadOnlyAtAppropriateTimes

		public void TestWarehouseCodeBoxIsReadOnlyAtAppropriateTimes()
		{
			using (var form = GetForm())
			{
				var control = FilterControl;
				form.Show();
				AssertEquals("", control.ABCCategoryDropEdit.CodeBox.Text);
				AssertEquals(true, control.WarehouseFindBox.ReadOnly);

				filter.WJ_Category = "A";
				control.WarehouseFindBox.Select();
				AssertEquals("A", control.ABCCategoryDropEdit.CodeBox.Text);
				AssertEquals(false, control.WarehouseFindBox.ReadOnly);

				control.ABCCategoryDropEdit.Select();
				filter.WJ_Category = "X";
				control.WarehouseFindBox.Select();
				AssertEquals("X", control.ABCCategoryDropEdit.CodeBox.Text);
				AssertEquals(false, control.WarehouseFindBox.ReadOnly);

				control.ABCCategoryDropEdit.Select();
				filter.WJ_Category = "A";
				control.WarehouseFindBox.Select();
				AssertEquals("A", control.ABCCategoryDropEdit.CodeBox.Text);
				AssertEquals(false, control.WarehouseFindBox.ReadOnly);
			}
		}

		#endregion

		#region Implementation

		ZForm GetForm()
		{
			var form = new ZForm(Filter);
			form.Controls.Add(FilterControl);
			return form;
		}

		ABCCategoryWarehouseFilterControl FilterControl
		{
			get { return filterControl ?? (filterControl = new ABCCategoryWarehouseFilterControl()); }
		}

		ABCCategoryWarehouseFilterControl filterControl;

		ABCCategoryWarehouseFilter Filter
		{
			get { return filter ?? (filter = new ABCCategoryWarehouseFilter()); }
		}

		ABCCategoryWarehouseFilter filter;

		#endregion
	}
}
