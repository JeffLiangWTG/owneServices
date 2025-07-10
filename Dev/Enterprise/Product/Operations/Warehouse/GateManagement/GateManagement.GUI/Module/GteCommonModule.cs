using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.GateManagement.GUI
{
	public abstract class GteCommonModule : GlowOnlyModule
	{
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GateManagement;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.GateManager;
	}
}
