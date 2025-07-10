using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDAdHocServiceOrderController : CYDController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.CYDAdHocServiceOrder; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CYDAdHocServiceOrder; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CYDAdHocServiceOrder); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CYDAdHocServiceOrderForm((CYDAdHocServiceOrder)businessEntity);
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("CYDAdHocServiceOrder|ShowFormForNewEntityCore", "You cannot create a Service Order on CargoWise, please go to the Container Yard portal."));

			return null;
		}
	}
}
