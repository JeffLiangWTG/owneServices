using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Module.Testing
{
	[TestsSubclassesOf(typeof(HandlingUnitModule))]
	public abstract class HandlingUnitModuleTest<T> : GlowOnlyModuleTest<T>
		where T : HandlingUnitModule, new()
	{
		protected override bool ExpectedAllowView => false;

		protected override bool ExpectedAllowEdit => false;

		protected override bool ExpectedSupportsWorkflow => true;
	}
}
