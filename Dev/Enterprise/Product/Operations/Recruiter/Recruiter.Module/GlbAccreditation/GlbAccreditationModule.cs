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
	public class GlbAccreditationModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.GlbAccreditation;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GlbAccreditation;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.LearningAndDevelopment;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbAccreditation);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GlbAccreditationCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbAccreditationFilterControl(GridCollection,
				(GlbAccreditationFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbAccreditationFilterBusinessObject();
		}
	}
}
