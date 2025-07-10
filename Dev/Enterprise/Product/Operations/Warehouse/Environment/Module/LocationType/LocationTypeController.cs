using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	public class LocationTypeController : WhsControllerBase
	{
		#region ID

		public override ControllerID ID => ControllerIDs.WhsConfigLocationType;

		#endregion

		#region ModuleID

		public override ModuleIdentifier ModuleID => ModuleIDs.WhsConfigLocationType;

		#endregion

		#region TypeOfTopLevelBusinessObject

		public override Type TypeOfTopLevelBusinessObject => typeof(WhsLocationType);

		#endregion

		#region GetForm

		protected override IZForm GetForm(IBusiness locationType) => new LocationTypeEntryForm((WhsLocationType)locationType);

		#endregion

		#region SecurityCheckpoints

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsConfigLocationTypeView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsConfigLocationTypeNew;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsConfigLocationTypeEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WhsConfigLocationTypeDelete;

		#endregion
	}
}
