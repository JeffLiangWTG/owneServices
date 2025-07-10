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
	public class WhsItemReceiveASNModule : WhsTransitModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WhsItemReceiveASNFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new WhsItemReceiveASNFilterControl((WhsItemReceiveASNCollection)GridCollection, (WhsItemReceiveASNFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsItemReceiveASNCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsItemReceiveASN; }
		}

		protected override ControllerID ControllerID => ControllerIDs.WhsItemReceiveASN;

		public override string WorkflowType => WorkflowDescriptors.TransitReceiveASN;

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsItemReceiveASN;
	}
}
