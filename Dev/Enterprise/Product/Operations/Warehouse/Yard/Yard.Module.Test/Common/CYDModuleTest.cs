using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Yard.Business.Test;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	public abstract class CYDModuleTest<T> : GlowOnlyModuleTest<T> where T : CYDModule, new()
	{
		protected override bool ExpectedAllowView => false;

		protected override bool ExpectedAllowEdit => false;

		protected override bool ExpectedSupportsWorkflow => false;

		protected override LicenceCheckpoint ExpectedLicenceCheckPoint => Env.Licence.ContainerYard;

		protected override SecurityCheckpoint ExpectedSecurityCheckPoint => Env.Security.ContainerYard;

		protected WhsWarehouse YardInCurrentBranch;

		protected override void SetUp()
		{
			base.SetUp();
			YardInCurrentBranch = new CYDYardTestHelper(Factory).GetOrCreateContainerYardInCurrentBranch();
			Factory.Save();
		}
	}
}
