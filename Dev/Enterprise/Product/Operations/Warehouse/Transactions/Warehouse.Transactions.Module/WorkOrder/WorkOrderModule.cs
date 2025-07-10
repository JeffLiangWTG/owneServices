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
	public class WorkOrderModule : WhsComponentOrderModule, IOperationalActionSupportable
	{
		public WorkOrderModule()
			: base()
		{
		}

		public override ModuleIdentifier ID => ModuleIDs.WhsWorkOrder;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.WhsWorkOrderWorkflowDescriptorCode;

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.WhsWorkOrder };

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsWorkOrder);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new WorkOrderFilterControl(GridCollection, (WorkOrderFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WorkOrderFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsWorkOrderCollection(Factory);
		}

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsWorkOrder;

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new WorkOrderOperationalActionSupporter();

		#endregion
	}
}
