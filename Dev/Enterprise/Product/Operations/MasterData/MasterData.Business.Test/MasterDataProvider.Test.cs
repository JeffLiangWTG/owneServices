using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class MasterDataProviderTest : TestCaseWithFactory
	{
		public void TestCreatePersonDuplicationFinder_ShouldReturnCorrectTypeWhenForAdminPanel()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var provider = new MasterDataProvider();

			var personDepueFinder = provider.CreatePersonDuplicationFinder(person, forAdminPanel: true);

			AssertType<GlbPersonDuplicationFinderWithThresholdOverride>("DuplicationFinder should be for Admin Panel", personDepueFinder);
		}

		public void TestCreatePersonDuplicationFinder_ShouldReturnCorrectTypeWhenForNotAdminPanel()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			var provider = new MasterDataProvider();

			var personDepueFinder = provider.CreatePersonDuplicationFinder(person, forAdminPanel: false);

			AssertType<GlbPersonDuplicationFinderProxy>("DuplicationFinder should not be for Admin Panel", personDepueFinder);
		}
	}
}
