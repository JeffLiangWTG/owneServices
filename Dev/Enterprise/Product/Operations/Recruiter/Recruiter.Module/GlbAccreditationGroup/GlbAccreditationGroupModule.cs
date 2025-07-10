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
	public class GlbAccreditationGroupModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.GlbAccreditationGroup;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GlbAccreditationGroup;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.LearningAndDevelopment;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbAccreditationGroup);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GlbAccreditationGroupCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbAccreditationGroupFilterControl(GridCollection,
				(GlbAccreditationGroupFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbAccreditationGroupFilterBusinessObject();
		}
	}
}
