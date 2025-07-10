using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestsSubclassesOf(typeof(WarehouseGlowOnlyModule))]
	public abstract class WarehouseGlowOnlyModuleTest<T> : GlowOnlyModuleTest<T>
		where T : WarehouseGlowOnlyModule, new()
	{
		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.WarehouseManagerOperationsAnd3PL;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.Warehouse;
	}
}
