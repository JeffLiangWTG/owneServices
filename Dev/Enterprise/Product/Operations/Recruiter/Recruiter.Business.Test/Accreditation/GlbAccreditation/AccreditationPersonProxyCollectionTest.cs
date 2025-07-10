using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(AccreditationPersonProxyCollection))]
	sealed class AccreditationPersonProxyCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AccreditationPersonProxyCollection>
	{
		public void TestConstructorNullRef()
		{
			var accred = Factory.New<GlbAccreditation>();
			var person = Factory.New<GlbPerson>();
			AssertNoExceptionThrown(delegate
			{ new AccreditationPersonProxyCollection(null, person); });
			AssertNoExceptionThrown(delegate
			{ new AccreditationPersonProxyCollection(accred, person); });
			AssertNoExceptionThrown(delegate
			{ new AccreditationPersonProxyCollection(null, null); });
		}

		public void TestPopulateCollectionNullRef()
		{
			var collection = new AccreditationPersonProxyCollection(null, null);
			AssertNoExceptionThrown(delegate
			{ collection.PopulateCollection(); });
		}

		protected override AccreditationPersonProxyCollection GetCollectionToTest()
		{
			return new AccreditationPersonProxyCollection(Factory.New<GlbAccreditation>(), Factory.New<GlbPerson>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AccreditationPersonProxy(Factory.New<GlbAccreditation>(), Factory.New<GlbPerson>());
		}
	}
}
