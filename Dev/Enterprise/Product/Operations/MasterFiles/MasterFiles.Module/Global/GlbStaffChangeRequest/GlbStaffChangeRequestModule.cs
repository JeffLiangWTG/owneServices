using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class GlbStaffChangeRequestModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.GlbStaffChangeRequest;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GlbStaffChangeRequest;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.GlbStaffChangeRequestWorkflowDescriptorCode;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Recruiter;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.GlbStaffChangeRequest);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new GlbStaffChangeRequestFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new GlbStaffChangeRequestFilterControl(GridCollection, (GlbStaffChangeRequestFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new GlbStaffChangeRequestCollection(Factory);

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
	}
}
