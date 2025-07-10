using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportCommon.Registry.Testing
{
	[TestedType(typeof(PackageVersionOverrideCollection))]
	class PackageVersionOverrideCollectionTest : RegistryBusinessObjectCollectionTestCase<PackageVersionOverrideCollection>
	{
		#region Implementation

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override PackageVersionOverrideCollection GetCollectionToTest() => new PackageVersionOverrideCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new PackageVersionOverride();

		#endregion
	}
}
