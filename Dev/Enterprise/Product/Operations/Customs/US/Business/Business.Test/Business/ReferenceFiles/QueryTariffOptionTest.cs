using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(QueryTariffOption))]
	sealed class QueryTariffOptionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTariffsToQuery()
		{
			AssertEquals("Empty collection", 0, QueryTariffOption.TariffsToQuery.Count);
		}

		public void TestSendQuery()
		{
			int ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			var queryTariff = new QueryTariff();
			queryTariff.FromTariff = "2203000030";
			QueryTariffOption.TariffsToQuery.Add(queryTariff);
			QueryTariffOption.SendQuery();
			AssertEquals(++ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));

			ediMessageCount = Factory.GetDatabaseCount(typeof(EDIMessage));
			queryTariff = new QueryTariff();
			queryTariff.FromTariff = "2203000030";
			queryTariff.ToTariff = "2203000040";
			QueryTariffOption.TariffsToQuery.Add(queryTariff);
			QueryTariffOption.SendQuery();
			AssertEquals(++ediMessageCount, Factory.GetDatabaseCount(typeof(EDIMessage)));
		}

		protected override BusinessObject GetNewBusinessObject() => new QueryTariffOption(Factory);

		QueryTariffOption queryTariffOption;
		QueryTariffOption QueryTariffOption => queryTariffOption ?? (queryTariffOption = new QueryTariffOption(Factory));
	}
}
