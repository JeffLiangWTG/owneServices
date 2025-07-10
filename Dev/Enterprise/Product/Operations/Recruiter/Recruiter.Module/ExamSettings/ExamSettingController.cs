using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module
{
	public class ExamSettingsController : ZController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var controller = ZControllerFactory.Create(ControllerIDs.LearningCentreCampaign);
			var internals = controller as ZControllerInternals;
			var campaign = (businessEntity as ExamSetting)?.TestCampaign ?? internals.GetNewBusinessEntityInLocalFactory();
			return internals.GetForm(campaign);
		}

		public override ControllerID ID => ControllerIDs.ExamSetting;

		public override ModuleIdentifier ModuleID => ModuleIDs.ExamSetting;

		public override Type TypeOfTopLevelBusinessObject => typeof(ExamSetting);
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ExamSettingView;
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ExamSettingNew;
		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ExamSettingEdit;
		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ExamSettingDelete;
	}
}
