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
	public class TelPreDriveChecklistController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.TelematicsPreDriveChecklistHeaderView;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new TelPreDriveChecklistForm((TelPreDriveChecklistHeader)businessEntity);
		}

		public override ControllerID ID => ControllerIDs.TelPreDriveChecklist;

		public override Type TypeOfTopLevelBusinessObject => typeof(TelPreDriveChecklistHeader);

		public override ModuleIdentifier ModuleID => ModuleIDs.TelematicsPreDriveChecklists;
	}
}
