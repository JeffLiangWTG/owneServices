using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.Module
{
	public class HVLVConsignmentModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public HVLVConsignmentModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.HVLVConsignment;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) =>
			ZControllerFactory.Create(ControllerIDs.HVLVConsignment);

		protected override IFilterControl GetNewFilterControl() =>
			new HVLVConsignmentFilterControl(GridCollection, (HVLVConsignmentFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ActiveBusinessObjectCollection<HVLVConsignment>(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new HVLVConsignmentFilterBusinessObject();

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.HVLVConsignment;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Forwarder;

		public override bool AllowNew => false;

		public override bool AllowDefaultActivateDeactivate => false;

		#region Workflow

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.HVLVConsignmentWorkflowDescriptorCode;

		#endregion

		#region IOperationalActionSupportable

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new HVLVConsignmentOperationalActionSupporter();

		#endregion
	}
}
