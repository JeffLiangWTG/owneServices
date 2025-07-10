using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCountryStatesDependentCollection))]
	sealed class RefCountryStatesDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new RefCountryStatesDependentCollection(Factory.NewWithValidTestData<RefCountry>(), Factory);

		public void TestAllowNewCore()
		{
			AssertEquals(false, Factory.NewWithValidTestData<RefCountry>().States.AllowNew);
		}
	}
}
