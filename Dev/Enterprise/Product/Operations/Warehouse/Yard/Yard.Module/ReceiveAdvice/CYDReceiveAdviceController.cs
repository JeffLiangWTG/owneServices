using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDReceiveAdviceController : CYDController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.CYDReceiveAdvice; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CYDReceiveAdvice; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CYDReceiveAdvice); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CYDReceiveAdviceForm((CYDReceiveAdvice)businessEntity);
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("CYDReceiveAdvice|ShowFormForNewEntityCore", "You cannot create a Pre-Arrival Instruction on CargoWise, please go to the Container Yard portal."));

			return null;
		}
	}
}
