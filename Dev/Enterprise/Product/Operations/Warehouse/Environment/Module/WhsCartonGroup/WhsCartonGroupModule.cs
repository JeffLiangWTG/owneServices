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
	public class WhsCartonGroupModule : ZFilterGridModule
	{
		#region ID

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsCartonGroup; }
		}

		#endregion

		#region GetNewController

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsCartonGroup);
		}

		#endregion

		#region GetNewFilterControl

		protected override IFilterControl GetNewFilterControl()
		{
			return new WhsCartonGroupFilterControl(GridCollection, (WhsCartonGroupFilterBusinessObject)FilterBusinessObject);
		}

		#endregion

		#region GetNewFilterBusinessObject

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WhsCartonGroupFilterBusinessObject();
		}

		#endregion

		#region GetNewGridCollection

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsCartonGroupCollection(Factory);
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
			get { return Env.Security.WhsConfigCartonGroup; }
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
