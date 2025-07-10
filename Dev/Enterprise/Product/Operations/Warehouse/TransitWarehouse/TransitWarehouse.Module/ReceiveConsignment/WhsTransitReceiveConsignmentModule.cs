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
	public class WhsTransitReceiveConsignmentModule : WhsTransitModule, IOperationalActionSupportable
	{
		public WhsTransitReceiveConsignmentModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WhsTransitReceiveConsignmentFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new WhsTransitReceiveConsignmentFilterControl((WhsItemReceiveConsignmentCollection)GridCollection, (WhsTransitReceiveConsignmentFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsItemReceiveConsignmentCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsTransitReceiveConsignment; }
		}

		protected override ControllerID ControllerID => ControllerIDs.WhsTransitReceiveConsignment;

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override string WorkflowType => WorkflowDescriptors.TransitReceiveConsignment;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsItemReceiveConsignment;

		public OperationalActionSupporter OperationalActionSupporter => new WhsTransitReceiveConsignmentOperationalActionSupporter();
	}
}
