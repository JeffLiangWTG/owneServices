using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Packing.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsHandlingUnitModule : HandlingUnitModule
	{
		public override ModuleIdentifier ID => ModuleIDs.WhsHandlingUnit;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsPkgHandlingUnit;

		protected override ControllerID ControllerID => ControllerIDs.WhsHandlingUnit;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new WhsHandlingUnitFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new WhsHandlingUnitFilterControl(GridCollection, (WhsHandlingUnitFilterBusinessObject)FilterBusinessObject);
	}
}
