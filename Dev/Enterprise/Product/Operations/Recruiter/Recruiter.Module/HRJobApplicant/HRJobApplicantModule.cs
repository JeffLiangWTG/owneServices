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
	public class HRJobApplicantModule : ZFilterGridModule
	{
		public HRJobApplicantModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.HRJobApplicant; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.HRJobApplicant);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new HRJobApplicantFilterControl(GridCollection, (HRJobApplicantFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new HRJobApplicantCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new HRJobApplicantFilterBusinessObject();
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
			get { return Env.Security.HRJobApplicant; }
		}

		#endregion

		#region Workflow

		public override bool SupportsWorkflow => true;

		#endregion
	}
}
