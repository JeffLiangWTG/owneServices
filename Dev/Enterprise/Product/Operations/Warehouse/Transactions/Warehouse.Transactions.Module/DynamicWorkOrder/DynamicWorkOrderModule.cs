using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class DynamicWorkOrderModule : WhsComponentOrderModule, IOperationalActionSupportable
	{
		public DynamicWorkOrderModule()
			: base()
		{
		}

		public override ModuleIdentifier ID => ModuleIDs.WhsDynamicWorkOrder;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.WhsDynamicWorkOrderWorkflowDescriptorCode;

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.WhsDynamicWorkOrder };

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsDynamicWorkOrder);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new DynamicWorkOrderFilterControl((WhsDynamicWorkOrderCollection)GridCollection, (DynamicWorkOrderFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DynamicWorkOrderFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsDynamicWorkOrderCollection(Factory);
		}

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsDynamicWorkOrder;

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new DynamicWorkOrderOperationalActionSupporter();

		#endregion
	}
}
