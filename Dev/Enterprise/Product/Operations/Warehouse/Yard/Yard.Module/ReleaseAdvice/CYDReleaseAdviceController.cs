using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class CYDReleaseAdviceController : CYDController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.CYDReleaseAdvice; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CYDReleaseAdvice; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CYDReleaseAdvice); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CYDReleaseAdviceForm((CYDReleaseAdvice)businessEntity);
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("CYDReleaseAdvice|ShowFormForNewEntityCore", "You cannot create a Release Order on CargoWise, please go to the Container Yard portal."));

			return null;
		}
	}
}
