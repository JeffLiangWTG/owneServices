using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	sealed class AsycudaContainerLookupsTests : BusinessObjectLookupsTestCase
	{
		public void TestEmptyFullList()
		{
			var lookup = Factory.GetCachedValue<ZaEmptyFullIndicatorList>();
			AssertEquals("Should contain 4 items", 4, lookup.Count);
			AssertSame(lookup, container.Lookups.EmptyFullList);
		}

		public void TestLandedPurposeList()
		{
			CombineAssertions(() =>
			{
				AssertEquals(4, container.Lookups.LandedPurposeList.Count);
				AssertEquals("1, 2, 3, 6", container.Lookups.LandedPurposeList.CodesAsString);
				AssertEquals("Continental / Transit", container.Lookups.LandedPurposeList.GetDescriptionFromCode("1"));
				AssertEquals("Export", container.Lookups.LandedPurposeList.GetDescriptionFromCode("2"));
				AssertEquals("Import", container.Lookups.LandedPurposeList.GetDescriptionFromCode("3"));
				AssertEquals("Transhipment", container.Lookups.LandedPurposeList.GetDescriptionFromCode("6"));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<AsycudaManifestHeader>().Containers.AddNew();
		}

		AsycudaContainer container;
	}
}
