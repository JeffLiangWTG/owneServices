using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(QueryTariffsCollection))]
	sealed class QueryTariffsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<QueryTariffsCollection>
	{
		public void TestQueryTariffCollection()
		{
			var queryTariffsCollection = new QueryTariffsCollection(Factory);
			AssertEquals("Empty collection", 0, queryTariffsCollection.Count);
		}

		protected override QueryTariffsCollection GetCollectionToTest() => new QueryTariffsCollection(new BusinessObjectFactory());

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var queryTariff = new QueryTariff();
			queryTariff.FromTariff = "TEST";
			return queryTariff;
		}
	}
}
