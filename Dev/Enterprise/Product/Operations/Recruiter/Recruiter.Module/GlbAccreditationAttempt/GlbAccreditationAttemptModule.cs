using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module
{
	public class GlbAccreditationAttemptModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public GlbAccreditationAttemptModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.GlbAccreditationAttempt;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GlbAccreditationAttempt;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.LearningAndDevelopment;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.GlbAccreditationAttemptWorkflowDescriptorCode;

		public override bool AllowNew => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GlbAccreditationAttempt);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GlbAccreditationAttemptCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GlbAccreditationAttemptFilterControl(GridCollection, (GlbAccreditationAttemptFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GlbAccreditationAttemptFilterBusinessObject();
		}

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new GlbAccreditationAttemptOperationalActionSupporter();
	}
}
