using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module
{
	public class HRHiringRequestModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.HRHiringRequest;

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.HRHiringRequest;

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Recruiter;

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.HRHiringRequest);

		protected override IBusinessObjectCollection GetNewGridCollection() => new HRHiringRequestCollection(Factory);

		protected override IFilterControl GetNewFilterControl() => new HRHiringRequestFilterControl(GridCollection, (HRHiringRequestFilterBusinessObject)FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new HRHiringRequestFilterBusinessObject();

		#region Workflow

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.HRHiringRequestDescriptorCode;

		#endregion
	}
}
