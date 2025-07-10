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
	public class LoadController : WhsControllerBase
	{
		#region ID

		public override ControllerID ID => ControllerIDs.WhsLoad;

		#endregion

		#region ModuleID

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsLoad;

		#endregion

		#region TypeOfTopLevelBusinessObject

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsLoad);

		#endregion

		#region GetForm

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper) => new LoadEntryForm((WhsLoad)businessEntity);

		#endregion

		#region SecurityCheckpoints

		protected override SecurityCheckpoint CheckPointForView => Env.Security.Warehouse;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.Warehouse;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.Warehouse;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.Warehouse;

		#endregion
	}
}
