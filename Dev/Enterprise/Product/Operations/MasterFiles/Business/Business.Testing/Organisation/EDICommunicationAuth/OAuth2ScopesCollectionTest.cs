using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OAuth2ScopesCollection))]
	public class OAuth2ScopesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OAuth2ScopesCollection>
	{
		protected override OAuth2ScopesCollection GetCollectionToTest()
		{
			return new OAuth2ScopesCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OAuth2Scope();
		}
	}
}
