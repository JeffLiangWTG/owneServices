using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonConsolFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForValidateCore()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<CommonConsol>();
			factory.ResetDatabaseLoadCount();

			var expected = new[] { JobCartageSchema.Constants.TableName };

			var fetchStrategy = new CommonConsolFetchStrategy(consol);
			fetchStrategy.FetchForValidate();

			AssertContainsExactElementsInAnyOrder("", expected, factory.GetAllFetchHintedTableNames());
		}
	}
}
