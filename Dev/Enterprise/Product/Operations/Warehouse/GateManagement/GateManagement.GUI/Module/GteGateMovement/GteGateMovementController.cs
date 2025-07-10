using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public class GteGateMovementController : GteCommonController
	{
		public override ControllerID ID => ControllerIDs.GteGateMovement;

		public override ModuleIdentifier ModuleID => ModuleIDs.GteGateMovement;

		public override Type TypeOfTopLevelBusinessObject => typeof(GteGateMovement);

		protected override IZForm GetForm(IBusiness businessEntity) => new GteGateMovementForm((GteGateMovement)businessEntity);
	}
}
