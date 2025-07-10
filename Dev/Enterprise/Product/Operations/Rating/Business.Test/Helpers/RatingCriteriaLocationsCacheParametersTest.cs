using CargoWise.Types;

namespace Enterprise.Rating.Business.Testing
{
	public class RatingCriteriaLocationsCacheParametersTest : RatingTestCase
	{
		public void TestGetCacheKey_AllEmpty()
		{
			var parameters = new RatingCriteriaLocationsCacheParameters();
			var actual = parameters.GetCacheKey();
			var expected = "~~~~~~~~~~|||||";

			AssertEquals("Cache key should match the expected value for empty parameters.", expected, actual);
		}

		public void TestGetCacheKey()
		{
			var parameters = new RatingCriteriaLocationsCacheParameters
			{
				Origin = "AUSYD",
				PlannedLoad = "USLAX",
				ZoneOwners = new[]
				{
					Helper.NewOrgHeader("Org1"),
					Helper.NewOrgHeader("Org2"),
					Helper.NewOrgHeader("Org3")
				},
				SortedOverridenPlannedLoads = new ZString[]
				{
					"CHBSL",
					"USHOU",
					"USCHS"
				}
			};

			var expectedKey = "~~~~~AUSYD~~USLAX~~~||||CHBSL~USCHS~USHOU|Org1~Org2~Org3";
			var actualKey = parameters.GetCacheKey();

			AssertEquals("The generated cache key should match the expected format.", expectedKey, actualKey);
		}

		public void TestGetNameLocationsPair_Items()
		{
			var expectedLocations = new[]
			{
				"Origin",
				"Destination",
				"Via",
				"PlannedLoad",
				"PlannedDischarge",
				"RateOrigin",
				"RateDestination",
				"FirstLoad",
				"LastDischarge",
				"FirstRouteSetLoad",
				"LastRouteSetDischarge",
				"SortedOverridenPlannedLoads",
				"SortedOverridenPlannedDischarges",
				"OriginServices",
				"DestinationServices"
			};

			var parameters = new RatingCriteriaLocationsCacheParameters();

			AssertContainsExactElementsInAnyOrder(
				"The keys from GetNameLocationsPair should match the expected locations.",
				expectedLocations,
				parameters.GetNameLocationsPair().Keys
			);
		}
	}
}
