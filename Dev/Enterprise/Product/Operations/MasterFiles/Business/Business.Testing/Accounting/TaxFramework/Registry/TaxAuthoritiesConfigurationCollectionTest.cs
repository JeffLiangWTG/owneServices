using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxAuthoritiesConfigurationCollection))]
	sealed class TaxAuthoritiesConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<TaxAuthoritiesConfigurationCollection>
	{
		#region Implementation

		protected override TaxAuthoritiesConfigurationCollection GetCollectionToTest() => new TaxAuthoritiesConfigurationCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new TaxAuthoritiesConfiguration();

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		#endregion
	}
}
