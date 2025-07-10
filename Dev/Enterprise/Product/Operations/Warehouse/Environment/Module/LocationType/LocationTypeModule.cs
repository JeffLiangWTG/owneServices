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
	public class LocationTypeModule : ZFilterGridModule
	{
		#region ID

		public override ModuleIdentifier ID => ModuleIDs.WhsConfigLocationType;

		#endregion

		#region GetNewController

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsConfigLocationType);
		}

		#endregion

		#region GetNewFilterControl

		protected override IFilterControl GetNewFilterControl()
		{
			return new LocationTypeFilterControl((WhsLocationTypeCollection)GridCollection, (LocationTypeFilterBusinessObject)FilterBusinessObject);
		}

		#endregion

		#region GetNewFilterBusinessObject

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new LocationTypeFilterBusinessObject();
		}

		#endregion

		#region GetNewGridCollection

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsLocationTypeCollection(Factory);
		}

		#endregion

		#region LicenceCheckPointCore

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerCoreAnd4PL;

		#endregion

		#region SecurityCheckpoint

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsConfigLocationType;

		#endregion
	}
}
