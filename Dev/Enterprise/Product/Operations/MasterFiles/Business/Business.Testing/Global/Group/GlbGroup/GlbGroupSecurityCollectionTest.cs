using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbGroupSecurityCollection))]
	sealed class GlbGroupSecurityCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GlbGroupSecurityCollection>
	{
		protected override GlbGroupSecurityCollection GetCollectionToTest()
		{
			return new GlbGroupSecurityCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GlbGroupSecurity();
		}
	}
}
