using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Module
{
	public class WhsItemTransferHeaderModule : WhsTransitModule
	{
		public override ModuleIdentifier ID => ModuleIDs.WhsItemTransferHeader;

		protected override ControllerID ControllerID => ControllerIDs.WhsItemTransferHeader;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new WhsItemTransferHeaderFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new WhsItemTransferHeaderFilterControl((WhsItemTransferHeaderCollection)GridCollection, (WhsItemTransferHeaderFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new WhsItemTransferHeaderCollection(Factory);

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsItemTransferHeader;
	}
}
