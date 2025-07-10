using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsInventoryHeldCodeModule : ZFilterGridModule
	{
		#region ID

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsInventoryHeldCodes; }
		}

		#endregion

		#region GetNewController

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsInventoryHeldCodes);
		}

		#endregion

		#region GetNewFilterControl

		protected override IFilterControl GetNewFilterControl()
		{
			return new WhsInventoryHeldCodeFilterControl(GridCollection, (WhsInventoryHeldCodeFilterBusinessObject)FilterBusinessObject);
		}

		#endregion

		#region GetNewFilterBusinessObject

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WhsInventoryHeldCodeFilterBusinessObject();
		}

		#endregion

		#region GetNewGridCollection

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsInventoryHeldCodeCollection(Factory);
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
			get { return Env.Security.WhsConfigInventoryHeldCode; }
		}

		#endregion

		#region AllowDelete

		public override bool AllowDelete
		{
			get { return true; }
		}

		#endregion
	}
}