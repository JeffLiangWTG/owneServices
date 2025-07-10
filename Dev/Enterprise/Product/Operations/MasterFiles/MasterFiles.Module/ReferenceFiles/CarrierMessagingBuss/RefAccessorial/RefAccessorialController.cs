using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class RefAccessorialController : ZController
	{
		public override ControllerID ID => ControllerIDs.RefAccessorial;

		public override ModuleIdentifier ModuleID => ModuleIDs.RefAccessorial;

		public override Type TypeOfTopLevelBusinessObject => typeof(RefAccessorial);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.RefAccessorialView;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
			=> new RefAccessorialForm((RefAccessorial)businessEntity);
	}
}
