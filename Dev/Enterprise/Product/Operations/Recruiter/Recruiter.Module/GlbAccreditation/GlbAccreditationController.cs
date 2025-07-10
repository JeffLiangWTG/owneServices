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
	public class GlbAccreditationController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GlbAccreditationForm(businessEntity as GlbAccreditation);
		}

		public override ControllerID ID => ControllerIDs.GlbAccreditation;

		public override ModuleIdentifier ModuleID => ModuleIDs.GlbAccreditation;

		public override Type TypeOfTopLevelBusinessObject => typeof(GlbAccreditation);
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.GlbAccreditationView;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.GlbAccreditationNew;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.GlbAccreditationEdit;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.GlbAccreditationDelete;
	}
}
