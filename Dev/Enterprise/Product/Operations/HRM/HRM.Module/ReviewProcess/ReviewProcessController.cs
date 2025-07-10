using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.HRM.Common;
using Enterprise.HRM.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.HRM.Module
{
	public class ReviewProcessController : ZController
	{
		public override ControllerID ID => ControllerIDs.ReviewProcess;

		public override ModuleIdentifier ModuleID => ModuleIDs.ReviewProcess;

		public override Type TypeOfTopLevelBusinessObject => typeof(ReviewProcess);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
			=> new ReviewProcessForm((ReviewProcess)businessEntity);
	}
}
