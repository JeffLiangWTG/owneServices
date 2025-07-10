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
	public class WhsItemReceiveTransportationUnitModule : WhsTransitModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WhsItemReceiveTransportationUnitFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new WhsItemReceiveTransportationUnitFilterControl((WhsItemReceiveTransportationUnitCollection)GridCollection, (WhsItemReceiveTransportationUnitFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsItemReceiveTransportationUnitCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsItemReceiveTransportationUnit; }
		}

		protected override ControllerID ControllerID => ControllerIDs.WhsItemReceiveTransportationUnit;

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override string WorkflowType => WorkflowDescriptors.TransitReceiveTransportationUnit;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsItemReceiveTransportationUnit;
	}
}

