using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public class GteBookingController : GteCommonController
	{
		public override ControllerID ID => ControllerIDs.GteBooking;

		public override ModuleIdentifier ModuleID => ModuleIDs.GteBooking;

		public override Type TypeOfTopLevelBusinessObject => typeof(GteBooking);

		protected override IZForm GetForm(IBusiness businessEntity) => new GteBookingForm((GteBooking)businessEntity);
	}
}
