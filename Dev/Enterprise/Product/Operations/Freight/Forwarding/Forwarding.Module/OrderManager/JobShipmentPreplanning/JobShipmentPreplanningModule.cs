using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class JobShipmentPreplanningModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get
			{
				return ModuleIDs.JobShipmentPreplanning;
			}
		}

		public override bool SupportsWorkflow
		{
			get
			{
				return true;
			}
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.JobShipmentPreplanningWorkflowDescriptorCode; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.OrderManager; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.OrderPreAdvice; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.JobShipmentPreplanning);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new JobShipmentPreplanningFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new JobShipmentPreplanningCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobShipmentPreplanningFilterBusinessObject();
		}
	}
}
