using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module
{
	public class GlbAccreditationGroupController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbAccreditationGroupForm(businessEntity as GlbAccreditationGroup);
		}

		public override ControllerID ID => ControllerIDs.GlbAccreditationGroup;

		public override ModuleIdentifier ModuleID => ModuleIDs.GlbAccreditationGroup;

		public override Type TypeOfTopLevelBusinessObject => typeof(GlbAccreditationGroup);
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GlbAccreditationGroupView;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GlbAccreditationGroupNew;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GlbAccreditationGroupEdit;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GlbAccreditationGroupDelete;
	}
}
