using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class MNRWorkOrderController : CYDController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.MNRWorkOrder; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.MNRWorkOrder; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(MNRWorkOrderHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new MNRWorkOrderForm((MNRWorkOrderHeader)businessEntity);
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("MNRWorkOrder|ShowFormForNewEntityCore", "You cannot create a Work Order on CargoWise, please go to the Container Yard portal."));

			return null;
		}
	}
}
