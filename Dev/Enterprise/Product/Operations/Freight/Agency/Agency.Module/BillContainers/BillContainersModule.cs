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
	public class BillContainersModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public BillContainersModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AgencyBillContainers; }
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.AgencyContainerWorkflowDescriptorCode; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AgencyBillContainers);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new BillContainersFilterControl(GridCollection, (BillContainersFilterStrip)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new BillOfLadingContainerCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new BillContainersFilterStrip();
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		#region Security

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ShippingManagerBillOfLading; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AgencyBillContainers; }
		}

		#endregion

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new BillContainersActionSupporter(); }
		}

		#endregion
	}
}


