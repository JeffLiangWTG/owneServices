using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.Module
{
	public class ContainerMoveModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public ContainerMoveModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AgencyContainerMove; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.ContainerMovementWorkflowDescriptorCode; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AgencyContainerMove);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ContainerMoveFilterControl(GridCollection, (ContainerMoveFilterStrip)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ContainerMovementCollection(Factory, false);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ContainerMoveFilterStrip();
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		#region Checkpoints

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ShippingManagerContainerControl; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AgencyContainerMovements; }
		}

		#endregion

		#region IOperationalActionSupportable Members

		public OperationalActionSupporter OperationalActionSupporter
		{
			get { return new ContainerMoveActionSupporter(); }
		}

		#endregion
	}
}


