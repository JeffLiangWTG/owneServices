using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transit.Module
{
	public abstract class WhsTransitModule : GlowOnlyModule
	{
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.TransitWarehouse;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.TransitWarehouse;

		public override bool AllowView => false;

		public override bool AllowEdit => false;

		public override bool SupportsWorkflow => true;
	}
}
