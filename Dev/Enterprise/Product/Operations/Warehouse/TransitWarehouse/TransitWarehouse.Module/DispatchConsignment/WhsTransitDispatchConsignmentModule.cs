using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Module
{
	public class WhsTransitDispatchConsignmentModule : WhsTransitModule, IOperationalActionSupportable
	{
		public WhsTransitDispatchConsignmentModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WhsTransitDispatchConsignmentFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new WhsTransitDispatchConsignmentFilterControl((WhsItemDispatchConsignmentCollection)GridCollection, (WhsTransitDispatchConsignmentFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsItemDispatchConsignmentCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsTransitDispatchConsignment; }
		}

		protected override ControllerID ControllerID => ControllerIDs.WhsTransitDispatchConsignment;

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override string WorkflowType => WorkflowDescriptors.TransitDispatchConsignment;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsItemDispatchConsignment;

		public OperationalActionSupporter OperationalActionSupporter => new WhsTransitDispatchConsignmentOperationalActionSupporter();
	}
}
