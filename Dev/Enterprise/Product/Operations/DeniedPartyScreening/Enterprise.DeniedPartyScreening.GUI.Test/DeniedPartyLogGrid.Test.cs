using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class DeniedPartyLogGridTest : TestCaseWithDummy
	{
		public void TestGridColorsMenu()
		{
			using (var form = new ZForm())
			{
				var grid = new DeniedPartyLogGrid();
				form.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_NVarChar, 20));
				grid.BindingContext = new System.Windows.Forms.BindingContext();
				grid.SetDataBinding(Factory.New<DummyBusinessObject>(), "Collection");
				form.Show();

				Assert(!grid.ContextMenu.MenuItems.ContainsKey(ZString.Empty)); //Grid Colors menu has an empty name
			}
		}
	}
}

