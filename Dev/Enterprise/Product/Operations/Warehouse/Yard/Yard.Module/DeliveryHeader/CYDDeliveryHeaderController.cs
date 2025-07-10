using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDDeliveryHeaderController : CYDController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.CYDDeliveryHeader; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CYDDeliveryHeader; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CYDDeliveryHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CYDDeliveryHeaderForm((CYDDeliveryHeader)businessEntity);
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("CYDDeliveryHeader|ShowFormForNewEntityCore", "You cannot create a Delivery on CargoWise, please go to the Container Yard portal."));

			return null;
		}
	}
}
