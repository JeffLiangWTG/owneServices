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
	public class LearningCentreCampaignController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public LearningCentreCampaignController()
		{
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.LearningCentreCampaign; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.LearningCentreCampaign; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(LearningCentreCampaign); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new LearningCentreCampaignForm((LearningCentreCampaign)businessEntity);
		}

		#region Security Check Points

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.HRJobSkillExamCampaignDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.HRJobSkillExamCampaignEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.HRJobSkillExamCampaignNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.HRJobSkillExamCampaignView; }
		}

		#endregion

	}
}
