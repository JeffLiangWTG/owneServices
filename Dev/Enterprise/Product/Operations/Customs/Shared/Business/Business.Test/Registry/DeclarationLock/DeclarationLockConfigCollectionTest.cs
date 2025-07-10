using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(DeclarationLockConfigCollection))]
	sealed class DeclarationLockConfigCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DeclarationLockConfigCollection>
	{
		protected override DeclarationLockConfigCollection GetCollectionToTest() => new DeclarationLockConfigCollection(CurrentFallbackLevel, Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DeclarationLockConfig(CurrentFallbackLevel, Factory);

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		FallbackLevel CurrentFallbackLevel => currentFallbackLevel ?? (currentFallbackLevel = NewFallbackLevel());
		FallbackLevel currentFallbackLevel;
	}
}
