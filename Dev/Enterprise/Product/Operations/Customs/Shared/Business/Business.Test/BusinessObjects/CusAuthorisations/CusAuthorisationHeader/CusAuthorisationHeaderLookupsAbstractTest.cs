using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(CusAuthorisationHeaderLookups))]
	public abstract class CusAuthorisationHeaderLookupsAbstractTest<T> : BusinessObjectLookupsTestCase
			where T : CusAuthorisationHeaderLookups
	{
		public void TestAuthorisationTypeListAreOrderedAndCached()
		{
			CombineAssertions(() =>
			{
				var lookups = CusAuthorisationHeaderLookupsForTesting();
				var authorisationTypes = lookups.AuthorisationTypeList;
				AssertArrayEqualsByElements("Sorted by Code", authorisationTypes.GetAllCodes().OrderBy(x => x).ToArray(), authorisationTypes.GetAllCodes());
				AssertSame("Cached", lookups.AuthorisationTypeList, authorisationTypes);
			});
		}

		protected abstract T CusAuthorisationHeaderLookupsForTesting();
	}
}
