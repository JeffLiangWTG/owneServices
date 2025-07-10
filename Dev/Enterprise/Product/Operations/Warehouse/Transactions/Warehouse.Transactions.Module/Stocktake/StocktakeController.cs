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
	public class StocktakeController : WhsControllerBase
	{
		public StocktakeController()
		{
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsStocktake;

		public override ControllerID ID => ControllerIDs.WhsStocktake;

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsStocktake);

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper)
		{
			return new StocktakeEntryForm((WhsStocktake)businessEntity, whsNotificationSubscriberGuiHelper);
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsStocktakeView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsStocktakeNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsStocktakeEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;
	}
}
