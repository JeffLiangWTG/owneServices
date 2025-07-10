using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public class GteGateMovementBookingController : GteCommonController
	{
		public override ControllerID ID => ControllerIDs.GteGateMovementBooking;

		public override ModuleIdentifier ModuleID => ModuleIDs.GteGateMovementBooking;

		public override Type TypeOfTopLevelBusinessObject => typeof(GteGateMovementBooking);

		protected override IZForm GetForm(IBusiness businessEntity) => new GteGateMovementBookingForm((GteGateMovementBooking)businessEntity);
	}
}
