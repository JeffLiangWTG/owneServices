using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module
{
	public class LearningCentreCampaignModule : ZFilterGridModule
	{
		public LearningCentreCampaignModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.LearningCentreCampaign; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.LearningCentreCampaign);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new LearningCentreCampaignFilterControl(GridCollection, (LearningCentreCampaignFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new LearningCentreCampaignCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new LearningCentreCampaignFilterBusinessObject();
		}

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.LearningAndDevelopment; }
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.HRJobSkillExamCampaign; }
		}

		#endregion
	}
}
