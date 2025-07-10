using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business.Testing;
using Urs.Api.Integration.DTOs.Request;

namespace Enterprise.Rating.Business.Test.UrsRates;

public class UrsRatesQueryBuilderTest : TestCaseWithFactory
{
	public void TestMapsQuerySource()
	{
		var queryBuilder = new UrsRatesQueryBuilder(new TestLogger());
		var query = queryBuilder.Build(new TestRatingCriteria()).ursRequest;
		AssertContainsExactElementsInAnyOrder([QuerySource.Database, QuerySource.OceanOnDemand], query.QuerySources);
	}
}
