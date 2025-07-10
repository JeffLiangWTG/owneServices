using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Module
{
	public class WhsItemDispatchTransportationUnitModule : WhsTransitModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WhsItemDispatchTransportationUnitFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new WhsItemDispatchTransportationUnitFilterControl((WhsItemDispatchTransportationUnitCollection)GridCollection, (WhsItemDispatchTransportationUnitFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsItemDispatchTransportationUnitCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsItemDispatchTransportationUnit; }
		}

		protected override ControllerID ControllerID => ControllerIDs.WhsItemDispatchTransportationUnit;

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override string WorkflowType => WorkflowDescriptors.TransitDispatchTransportationUnit;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsItemDispatchTransportationUnit;
	}
}
