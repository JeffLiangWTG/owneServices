using System.Collections.Generic;
using System.Web.UI.WebControls;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ShoppingCartInventoryColumnProvider))]
	sealed class ShoppingCartInventoryColumnProviderTest : TrackingInventoryColumnProviderTest
	{
		#region TestInventoryColumnBinding

		public void TestInventoryColumnBinding()
		{
			var inventoriesColumn = (ZNewRowColumn)TestProvider[WebTracker.Grids.TrackingInventory.Inventories];
			AssertEquals("Inventories", inventoriesColumn.BindTo);

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Russian))
			{
				var newTestProvider = GetNewTestProvider();
				newTestProvider.CustomizeDictionary();

				inventoriesColumn = (ZNewRowColumn)newTestProvider[WebTracker.Grids.TrackingInventory.Inventories];
				AssertEquals("Inventories", inventoriesColumn.BindTo);
			}
		}

		#endregion

		#region Implementation

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			List<DataGridColumn> result = new List<DataGridColumn>(base.GetColumnsForLayoutFixNoDynamicColumns());
			result.Add(TestProvider[WebTracker.Grids.TrackingInventory.Inventories]);
			return result.ToArray();
		}

		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			ZNewRowColumn inventoriesColumn = new ZNewRowColumn("Inventories")
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.Inventories,
				HeaderText = "Details",
				Collapsable = true
			};
			inventoriesColumn.ItemTemplate = new TrackingInventoryTemplate(inventoriesColumn);
			AddRequiredColumn(inventoriesColumn);
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ShoppingCartInventoryColumnProvider();
		}

		#endregion
	}
}
