using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class CycleCountWaveController : WhsControllerBase
	{
		#region ID

		public override ControllerID ID => ControllerIDs.WhsCycleCountWave;

		#endregion

		#region ModuleID

		public override ModuleIdentifier ModuleID => null;

		#endregion

		#region TypeOfTopLevelBusinessObject

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsCycleCountWave);

		#endregion

		#region GetForm

		protected override IZForm GetForm(IBusiness businessEntity, INotificationSubscriberQueryUser whsNotificationSubscriberGuiHelper) => throw new ModuleGuiNotSupportedException("Not implemented yet");

		#endregion

		#region SecurityCheckpoints

		protected override SecurityCheckpoint CheckPointForView => Env.Security.Warehouse;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.Warehouse;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.Warehouse;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.Warehouse;

		#endregion
	}
}
