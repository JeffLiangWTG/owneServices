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
	public class OrderLineModule : ZFilterGridModule
	{
		#region Security

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.OrderManager; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.OrderLineTracking; }
		}

		#endregion

		public override ModuleIdentifier ID
		{
			get
			{
				return ModuleIDs.OrderLine;
			}
		}

		public override bool SupportsWorkflow
		{
			get
			{
				return true;
			}
		}

		public override string WorkflowType => WorkflowDescriptors.OrderLineWorkflowDescriptorCode;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.OrderLine);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrderLineFilterControl((OrderLineCollection)GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrderLineCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrderLineFilterBusinessObject();
		}

		public override bool AllowDelete
		{
			get { return false; }
		}
	}
}
