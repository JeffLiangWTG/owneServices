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
	/// <summary>
	/// This is used by the Row Form's Locations Button.
	/// </summary>
	public class LocationController : WhsControllerBase
	{
		public override ControllerID ID => ControllerIDs.WhsConfigLocation;
		public override ModuleIdentifier ModuleID => null;
		public override Type TypeOfTopLevelBusinessObject => typeof(WhsRow);
		protected override IZForm GetForm(IBusiness businessEntity) => new LocationForm((WhsRow)businessEntity);

		protected override SecurityCheckpoint CheckPointForView => Env.Security.WhsConfigLocationView;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.WhsConfigLocationEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.WhsConfigLocationNew;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.WhsConfigLocationDelete;
	}
}
