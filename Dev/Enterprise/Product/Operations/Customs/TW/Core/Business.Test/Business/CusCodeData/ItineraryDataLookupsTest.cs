using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ItineraryDataLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestCY_CodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var itinerary = declaration.Itineraries.AddNew();
			var lookups = new ItineraryDataLookups(itinerary);
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(lookups.CY_CodeList, NUnit.Framework.Is.TypeOf<RefCountryCollection>());
				NUnit.Framework.Assert.That(lookups.CY_CodeList.ToArray(), NUnit.Framework.Is.EquivalentTo(new RefCountryCollection(Factory).ToArray()));
			});
		}
	}
}
