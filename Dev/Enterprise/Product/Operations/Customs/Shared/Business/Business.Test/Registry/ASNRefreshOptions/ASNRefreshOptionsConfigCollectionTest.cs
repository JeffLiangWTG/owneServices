using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(ASNRefreshOptionsConfigCollection))]
	sealed class ASNRefreshOptionsConfigCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ASNRefreshOptionsConfigCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => false;

		protected override ASNRefreshOptionsConfigCollection GetCollectionToTest()
		{
			return new ASNRefreshOptionsConfigCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ASNRefreshOptionsConfig();
		}
	}
}
