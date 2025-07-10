using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDTransportationUnitController : CYDController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.CYDTransportationUnit; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CYDTransportationUnit; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CYDTransportationUnit); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CYDTransportationUnitForm((CYDTransportationUnit)businessEntity);
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("CYDTransportationUnit|ShowFormForNewEntityCore", "You cannot create a Transportation Unit on CargoWise, please go to the Container Yard portal."));

			return null;
		}
	}
}
