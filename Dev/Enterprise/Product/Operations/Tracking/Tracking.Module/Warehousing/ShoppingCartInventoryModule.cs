using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Tracking.Web;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	public class ShoppingCartInventoryModule : TrackingInventoryModule
	{
		public ShoppingCartInventoryModule(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

		public override ModuleIdentifier ID
		{
			get { return WebModuleIDs.ShoppingCartInventory; }
		}

		protected override sealed ZString FilterStripLayoutContext
		{
			get { return WebModuleIDs.TrackingInventory.Name; } // We want this to be the same for Inventory & InventoryDetails
		}

		protected override GridColumnProvider GetColumnProvider()
		{
			return new ShoppingCartInventoryColumnProvider();
		}
	}
}
