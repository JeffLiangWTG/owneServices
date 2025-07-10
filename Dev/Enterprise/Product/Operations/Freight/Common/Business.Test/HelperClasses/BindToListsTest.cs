using System.Reflection;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Business.Testing
{
	sealed class BindToListsTest : TestCase
	{
		public void TestShouldNotHavePublicConstructors()
		{
			AssertEquals("Should not have public constructors", 0, typeof(BindToLists).GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length);
		}

		public void TestGetCachedLists()
		{
			var factory = new BusinessObjectFactory();
			var bindToLists1 = BindToLists.GetCachedLists(factory);
			var bindToLists2 = BindToLists.GetCachedLists(factory);
			Assert("Instance is cached against the factory", object.ReferenceEquals(bindToLists1, bindToLists2));

			var anotherFactory = new BusinessObjectFactory();
			var bindToLists3 = BindToLists.GetCachedLists(anotherFactory);
			var bindToLists4 = BindToLists.GetCachedLists(anotherFactory);
			Assert("Instance is cached against the factory", object.ReferenceEquals(bindToLists3, bindToLists4));

			Assert("Different objects for different factories", !object.ReferenceEquals(bindToLists1, bindToLists3));
		}
	}
}
