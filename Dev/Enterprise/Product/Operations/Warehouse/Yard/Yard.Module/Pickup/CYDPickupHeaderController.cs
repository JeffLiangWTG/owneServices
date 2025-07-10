using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDPickupHeaderController : CYDController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.CYDPickupHeader; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CYDPickupHeader; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CYDPickupHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CYDPickupHeaderForm((CYDPickupHeader)businessEntity);
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("CYDPickupHeader|ShowFormForNewEntityCore", "You cannot create a Pickup on CargoWise, please go to the Container Yard portal."));

			return null;
		}
	}
}
