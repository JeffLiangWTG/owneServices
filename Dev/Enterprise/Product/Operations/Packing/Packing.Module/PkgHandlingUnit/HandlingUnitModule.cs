using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Packing.Module
{
	public abstract class HandlingUnitModule : GlowOnlyModule
	{
		protected override IBusinessObjectCollection GetNewGridCollection() => new PkgHandlingUnitCollection(Factory);

		public override bool AllowView => false;

		public override bool AllowEdit => false;

		public override bool SupportsWorkflow => true;
	}
}
