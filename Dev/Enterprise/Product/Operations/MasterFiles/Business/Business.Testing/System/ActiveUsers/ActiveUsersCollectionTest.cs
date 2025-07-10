using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveUsersCollection))]
	sealed class ActiveUsersCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ActiveUsersCollection>
	{
		protected override ActiveUsersCollection GetCollectionToTest()
		{
			return new ActiveUsersCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ActiveUser(Factory);
		}

		public void TestActiveUsersCanNotBeDeleted()
		{
			Assert("We should not be allowed to remove ActiveUsers", !GetCollectionToTest().AllowRemove);
		}
	}
}
