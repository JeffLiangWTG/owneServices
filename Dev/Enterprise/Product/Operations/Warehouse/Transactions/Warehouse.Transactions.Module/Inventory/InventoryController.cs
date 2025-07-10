using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class InventoryController : WhsControllerBase
	{
		public override ControllerID ID => ControllerIDs.WhsInventory;

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsInventory;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsDocketLine);

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper)
		{
			var rec = (WhsDocketLine)businessEntity;
			rec.IsInventoryEditForm = true;
			if (rec.Inventory.Count > 0)
			{
				rec.Inventory[0].IsInventoryEditForm = true;
			}
			return new InventoryForm(rec);
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsInventoryView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsInventoryEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;
	}
}
