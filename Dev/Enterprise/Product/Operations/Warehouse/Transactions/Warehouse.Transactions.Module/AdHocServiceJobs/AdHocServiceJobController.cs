using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class AdHocServiceJobController : WhsControllerBase
	{
		#region ID

		public override ControllerID ID => ControllerIDs.WhsAdHocServiceJob;

		#endregion

		#region ModuleID

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsAdHocServiceJob;

		#endregion

		#region GetForm

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper)
		{
			return new AdHocServiceJobEntryForm((WhsAdHocServiceJob)businessEntity, whsNotificationSubscriberGuiHelper);
		}

		#endregion

		#region TypeOfTopLevelBusinessObject

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsAdHocServiceJob);

		#endregion

		#region Security Check Points

		protected override Security.SecurityCheckpoint CheckPointForView => Env.Security.WhsAdHocServiceJobView;

		protected override Security.SecurityCheckpoint CheckPointForNew => Env.Security.WhsAdHocServiceJobNew;

		protected override Security.SecurityCheckpoint CheckPointForEdit => Env.Security.WhsAdHocServiceJobEdit;

		protected override Security.SecurityCheckpoint CheckPointForDelete => Env.Security.WhsAdHocServiceJobDelete;

		#endregion
	}
}
