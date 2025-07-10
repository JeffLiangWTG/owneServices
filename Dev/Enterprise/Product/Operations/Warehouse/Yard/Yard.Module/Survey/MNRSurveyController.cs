using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.Warehouse.Yard.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public class MNRSurveyController : CYDController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.MNRSurvey; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.MNRSurvey; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(MNRSurvey); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new MNRSurveyForm((MNRSurvey)businessEntity);
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			Globals.Message.ShowError(Res.GetString("MNRSurvey|ShowFormForNewEntityCore", "You cannot create a Survey on CargoWise, please go to the Container Yard portal."));

			return null;
		}
	}
}
