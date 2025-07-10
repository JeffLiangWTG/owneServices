using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDYardUnitStateController : CYDController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.CYDYardUnitState; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CYDYardUnitState; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CYDYardUnitState); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CYDYardUnitStateForm((CYDYardUnitState)businessEntity);
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("CYDYardUnitState|ShowFormForNewEntityCore", "You cannot create a Yard Unit on CargoWise, please go to the Container Yard portal."));

			return null;
		}
	}
}
