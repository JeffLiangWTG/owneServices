using System.Collections.Generic;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class ShoppingCartInventoryColumnProvider : TrackingInventoryColumnProvider
	{
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			var inventoriesColumn = new ZNewRowColumn((NoResString)"Inventories")    // This is a column name, may not be translated
			{
				ColumnKey = WebTracker.Grids.TrackingInventory.Inventories,
				HeaderText = details,
				Collapsable = true
			};
			inventoriesColumn.ItemTemplate = new TrackingInventoryTemplate(inventoriesColumn);
			AddToDictionaryAsRequired(inventoriesColumn);
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = base.GetOldColumnsOrder();
			result.Add((int)WebTracker.Grids.TrackingInventory.Inventories);
			return result;
		}

		static string details
		{
			get { return Res.GetString("409fa2b2-032c-4771-a71d-c802591a4e92", "Details"); }
		}
	}
}
