using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.GUI.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Telematics.Module.Controllers
{
	public class TelPreDriveChecklistTemplateController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.TelematicsPreDriveChecklistHeaderTemplateEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.TelematicsPreDriveChecklistHeaderTemplateView;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new TelPreDriveChecklistTemplateForm((TelPreDriveChecklistTemplateHeader)businessEntity);
		}

		public override ControllerID ID => ControllerIDs.TelPreDriveChecklistTemplate;

		public override Type TypeOfTopLevelBusinessObject => typeof(TelPreDriveChecklistTemplateHeader);

		public override ModuleIdentifier ModuleID => ModuleIDs.TelematicsPreDriveChecklistTemplates;
	}
}
