using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class VASOrderModule : ZFilterGridModule
	{
		#region ID

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsVASOrder; }
		}

		#endregion

		#region GetNewController

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsVASOrder);
		}

		#endregion

		#region GetNewFilterControl

		protected override IFilterControl GetNewFilterControl()
		{
			return new VASOrderFilterControl((WhsVASOrderCollection)GridCollection, (VASOrderFilterBusinessObject)FilterBusinessObject);
		}

		#endregion

		#region GetNewFilterBusinessObject

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new VASOrderFilterBusinessObject();
		}

		#endregion

		#region GetNewGridCollection

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsVASOrderCollection(Factory);
		}

		#endregion

		#region LicenceCheckPointCore

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.WarehouseManagerCoreAnd4PL; }
		}

		#endregion

		#region SecurityCheckpoint

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.WhsVASOrder; }
		}

		#endregion

		#region SupportsWorkflow

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		#endregion

		#region WorkflowType

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.WhsVASOrderWorkflowDescriptorCode; }
		}

		#endregion
	}
}