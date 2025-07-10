using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	sealed class CartageLegsGridTest : TestCaseWithFactory
	{
		public void TestEditCartageLegLicenceCheckpoint()
		{
			var cartage = Factory.NewWithValidTestData<CommonCartage>();
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			var leg = move.CartageLegs.AddNew();
			Factory.Save();

			using (var module = ModuleTree.Tree.FindByID(ModuleIDs.Cartage.Name).CreateZModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(cartage))
			using (var userControl = new CartageLegsGridControl())
			{
				form.Controls.Add(userControl);
				userControl.CartageLegsGrid.EditButton.PerformClick();
				using (var shownForm = (ZForm)userControl.CartageLegsGrid.LastShownZForm)
				{
					AssertEquals("ContainsCheckpoint(Env.Licence.LocalTransport)", true, shownForm.LicensedComponentManager.ContainsCheckpoint(Env.Licence.LocalTransport));
				}
			}
		}

		class CartageLegsGridForTest : ZModuleButtonGrid
		{
			public ZToolStripButton EditButton
			{
				get
				{
					var toolStrip = Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
					return toolStrip.Items.Find(Buttons.Edit, true).OfType<ZToolStripButton>().First();
				}
			}
		}

		class CartageLegsGridControl : ZUserControl
		{
			public CartageLegsGridControl()
			{
				InitializeComponent();
			}

			void InitializeComponent()
			{
				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo()
				{ ColumnName = "zTextBoxColumnStyleInfo1" };
				this.CartageLegsGrid = new CartageLegsGridForTest();
				this.BindingSource.DataSourceType = typeof(CommonCartage);
				CartageLegsGrid.BindToGridList = "CartageLegs";
				CartageLegsGrid.BindToFindBoxList = "CartageLegs";
				CartageLegsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				this.Controls.Add(CartageLegsGrid);
			}

			internal CartageLegsGridForTest CartageLegsGrid;
		}
	}
}
