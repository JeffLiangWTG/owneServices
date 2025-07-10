using CargoWise.EntityFramework;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.LVS.Module
{
	public class CusUSLVClearanceModule : ZFilterGridModule
	{
		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.US.USLowValueEntries);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ActiveBusinessObjectCollection<CusUSLVClearance>(Factory);

		protected override IFilterControl GetNewFilterControl() => new CusUSLVClearanceFilterControl(GridCollection, (CusUSLVClearanceFilterBusinessObject)FilterBusinessObject);

		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject() => new CusUSLVClearanceFilterBusinessObject();

		public override ModuleIdentifier ID => ModuleIDs.Customs.US.USLowValueEntries;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.USLVClearance;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.CoreCustomsModule;

		public override bool AllowUniversalCopy => false;

		#region Workflow

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.CusUSLVClearanceWorkflowDescriptorCode;

		#endregion
	}
}
