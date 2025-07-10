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
	public class HRJobOpeningsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		public HRJobOpeningsController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.HRJobOpenings; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(HRRecruitmentJobCampaign); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new HRJobOpeningsForm((HRRecruitmentJobCampaign)businessEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.HRJobOpenings; }
		}

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.HRJobOpeningsDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.HRJobOpeningsEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.HRJobOpeningsNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.HRJobOpeningsView; }
		}

		#endregion

	}
}
