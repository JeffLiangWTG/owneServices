using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.OrgPatternMatching.Testing
{
	sealed class PatternMatchingOriginalCollectionsTest : TestCaseWithFactory
	{
		public void TestSetDefaultValues()
		{
			var originalCollection = new PatternMatchingOriginalCollections();
			AssertNull("originalCollection.OrganisationNamesExceptBrands", originalCollection.OrganisationNamesExceptBrands);
			AssertNull("originalCollection.OrganisationNamesFromBrands", originalCollection.OrganisationNamesFromBrands);
			AssertNull("originalCollection.Addresses", originalCollection.Addresses);
			AssertNull("originalCollection.CustomsCodes", originalCollection.CustomsCodes);

			originalCollection.SetDefaultValues();
			AssertNotNull("originalCollection.OrganisationNamesExceptBrands", originalCollection.OrganisationNamesExceptBrands);
			AssertNotNull("originalCollection.OrganisationNamesFromBrands", originalCollection.OrganisationNamesFromBrands);
			AssertNotNull("originalCollection.Addresses", originalCollection.Addresses);
			AssertNotNull("originalCollection.CustomsCodes", originalCollection.CustomsCodes);

			AssertEquals("originalCollection.OrganisationNamesExceptBrands.Count", 0, originalCollection.OrganisationNamesExceptBrands.Count());
			AssertEquals("originalCollection.OrganisationNamesFromBrands.Count", 0, originalCollection.OrganisationNamesFromBrands.Count());
			AssertEquals("originalCollection.Addresses.Count", 0, originalCollection.Addresses.Count());
			AssertEquals("originalCollection.CustomsCodes.Count", 0, originalCollection.CustomsCodes.Count());
		}
	}
}
