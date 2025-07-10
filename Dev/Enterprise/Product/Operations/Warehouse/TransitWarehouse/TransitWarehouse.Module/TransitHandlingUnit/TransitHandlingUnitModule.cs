using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Packing.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Module
{
	public class TransitHandlingUnitModule : HandlingUnitModule
	{
		public override ModuleIdentifier ID => ModuleIDs.TransitHandlingUnit;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.PkgHandlingUnit;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new TransitHandlingUnitFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new TransitHandlingUnitFilterControl(GridCollection, (TransitHandlingUnitFilterBusinessObject)FilterBusinessObject);

		protected override ControllerID ControllerID => ControllerIDs.TransitHandlingUnit;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.TransitWarehouse;
	}
}
