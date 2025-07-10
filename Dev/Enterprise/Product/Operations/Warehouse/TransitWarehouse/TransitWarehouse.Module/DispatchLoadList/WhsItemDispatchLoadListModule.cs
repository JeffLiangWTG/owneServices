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
	public class WhsItemDispatchLoadListModule : WhsTransitModule
	{
		public override ModuleIdentifier ID => ModuleIDs.WhsItemDispatchLoadList;

		protected override ControllerID ControllerID => ControllerIDs.WhsItemDispatchLoadList;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new WhsItemDispatchLoadListFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new WhsItemDispatchLoadListFilterControl((WhsItemDispatchLoadListCollection)GridCollection, (WhsItemDispatchLoadListFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new WhsItemDispatchLoadListCollection(Factory);

		public override string WorkflowType => WorkflowDescriptors.TransitDispatchLoadList;

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsItemDispatchLoadList;
	}
}
