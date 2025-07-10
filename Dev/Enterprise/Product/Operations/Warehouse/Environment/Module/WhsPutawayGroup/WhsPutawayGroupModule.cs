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
	class WhsPutawayGroupModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.WhsConfigPutawayGroup;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsConfigPutawayGroup;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsConfigPutawayGroup);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WhsPutawayGroupFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new WhsPutawayGroupFilterControl(GridCollection, (WhsPutawayGroupFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsPutawayGroupCollection(Factory);
		}

		public override bool AllowDelete => true;
	}
}
