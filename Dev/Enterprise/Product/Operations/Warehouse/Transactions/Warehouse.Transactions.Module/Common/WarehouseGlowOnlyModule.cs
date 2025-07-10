using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public abstract class WarehouseGlowOnlyModule : GlowOnlyModule
	{
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.Warehouse;
	}
}
