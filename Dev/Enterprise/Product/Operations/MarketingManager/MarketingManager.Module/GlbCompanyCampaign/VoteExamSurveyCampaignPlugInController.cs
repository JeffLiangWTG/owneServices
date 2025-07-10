using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.MarketingManager.Module
{
	public abstract class VoteExamSurveyCampaignPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CampaignManagementDelete; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CampaignManagementEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CampaignManagementNew; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CampaignManagementView; }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("");
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GlbCompanyCampaign); }
		}

		protected override sealed ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return GetVoteExamSurveyPlugIn((GlbCompanyCampaign)businessEntity);
		}

		protected abstract VoteExamSurveyPlugIn GetVoteExamSurveyPlugIn(GlbCompanyCampaign campaign);
	}
}
