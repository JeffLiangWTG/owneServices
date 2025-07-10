using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public class GteVehicleMovementController : GteCommonController
	{
		public override ControllerID ID => ControllerIDs.GteVehicleMovement;

		public override ModuleIdentifier ModuleID => ModuleIDs.GteVehicleMovement;

		public override Type TypeOfTopLevelBusinessObject => typeof(GteVehicleMovement);

		protected override IZForm GetForm(IBusiness businessEntity) => new GteVehicleMovementForm((GteVehicleMovement)businessEntity);
	}
}
