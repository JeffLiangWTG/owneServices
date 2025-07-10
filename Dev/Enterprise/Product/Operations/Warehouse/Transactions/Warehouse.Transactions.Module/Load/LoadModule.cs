using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class LoadModule : WarehouseGlowOnlyModule
	{
		#region ID

		public override ModuleIdentifier ID => ModuleIDs.WhsLoad;

		#endregion

		#region ControllerID

		protected override ControllerID ControllerID => ControllerIDs.WhsLoad;

		#endregion

		#region GetNewFilterControl

		protected override IFilterControl GetNewFilterControl() => new LoadFilterControl((WhsLoadCollection)GridCollection, (LoadFilterBusinessObject)FilterBusinessObject);

		#endregion

		#region GetNewFilterBusinessObject

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new LoadFilterBusinessObject();

		#endregion

		#region GetNewGridCollection

		protected override IBusinessObjectCollection GetNewGridCollection() => new WhsLoadCollection(Factory);

		#endregion

		#region SecurityCheckpoint

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsLoad;

		#endregion

		#region SupportsWorkflow

		public override bool SupportsWorkflow => true;

		#endregion

		public override string WorkflowType => WorkflowDescriptors.WhsLoadWorkflowDescriptorCode;
	}
}
