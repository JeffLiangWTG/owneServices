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
	public class HRJobOpeningsModule : ZFilterGridModule
	{
		public HRJobOpeningsModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.HRJobOpenings; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.HRJobOpenings);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new HRJobOpeningsFilterControl(GridCollection, (HRJobOpeningsFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new HRRecruitmentJobCampaignCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new HRJobOpeningsFilterBusinessObject();
		}

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Recruiter; }
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.HRJobOpenings; }
		}

		#endregion
	}
}
