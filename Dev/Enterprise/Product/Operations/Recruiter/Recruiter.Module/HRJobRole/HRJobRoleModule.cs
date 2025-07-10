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
	public class HRJobRoleModule : ZFilterGridModule
	{
		public HRJobRoleModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.HRJobRole; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.HRJobRole);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new HRJobRoleFilterControl(GridCollection, (HRJobRoleFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new HRJobRoleCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new HRJobRoleFilterBusinessObject();
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
			get { return Env.Security.HRJobRole; }
		}

		#endregion
	}
}
