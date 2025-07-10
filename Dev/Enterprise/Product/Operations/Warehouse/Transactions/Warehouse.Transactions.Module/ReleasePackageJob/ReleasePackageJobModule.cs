using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Packing.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ReleasePackageJobModule : PackingModule
	{
		public override ModuleIdentifier ID => ModuleIDs.WhsReleasePackageJob;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.WhsReleasePackageJob);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsRelease;
	}
}
