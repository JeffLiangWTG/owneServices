using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Environment.Module
{
	public class WhsCartonSizeModule : ZFilterGridModule
	{
		#region ID

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsCartonSize; }
		}

		#endregion

		#region GetNewController

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsCartonSize);
		}

		#endregion

		#region GetNewFilterControl

		protected override IFilterControl GetNewFilterControl()
		{
			return new WhsCartonSizeFilterControl(GridCollection, (WhsCartonSizeFilterBusinessObject)FilterBusinessObject);
		}

		#endregion

		#region GetNewFilterBusinessObject

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WhsCartonSizeFilterBusinessObject();
		}

		#endregion

		#region GetNewGridCollection

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsCartonSizeCollection(Factory);
		}

		#endregion

		#region LicenceCheckPointCore

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.WarehouseManagerOperationsAnd3PL; }
		}

		#endregion

		#region SecurityCheckpoint

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.WhsConfigCartonSize; }
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
