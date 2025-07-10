using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public abstract class CYDModule : GlowOnlyModule
	{
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ContainerYard;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.ContainerYard;

		public override bool AllowView => false;

		public override bool AllowEdit => false;

		public override bool SupportsWorkflow => false;
	}
}
