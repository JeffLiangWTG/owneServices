using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(QueryTariff))]
	sealed class QueryTariffTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTariffHasFormatting()
		{
			QueryTariff.FromTariff = "2203.00.0030";
			QueryTariff.ToTariff = "2203.00.0030";
			AssertEquals("2203.00.0030", QueryTariff.FromTariff);
			AssertEquals("2203.00.0030", QueryTariff.ToTariff);
		}

		public void TestFromTariffValidation()
		{
			QueryTariff.FromTariff = "";
			QueryTariff.ValidateFromTariff();
			AssertHasError(QueryTariff.FromTariffInfo, QueryTariff.FromTariffMandatory);

			QueryTariff.FromTariff = "2203000033";
			QueryTariff.ValidateFromTariff();
			AssertNoError(QueryTariff.FromTariffInfo, QueryTariff.FromTariffMandatory);
		}

		public void TestToTariffValidation()
		{
			QueryTariff.FromTariff = "7403000033";
			QueryTariff.ToTariff = "2203000030";
			QueryTariff.ValidateToTariff();
			AssertHasMessageError(QueryTariff.ToTariffInfo, QueryTariff.ToTariffShouldBeGreaterThanFromTariff);

			QueryTariff.FromTariff = "2203000030";
			QueryTariff.ToTariff = "7403000033";
			QueryTariff.ValidateToTariff();
			AssertNoMessageError(QueryTariff.ToTariffInfo, QueryTariff.ToTariffShouldBeGreaterThanFromTariff);
			AssertHasWarning(QueryTariff.ToTariffInfo, QueryTariff.RangeMayExceed100Tariffs);

			QueryTariff.ToTariff = "2203000037";
			QueryTariff.ValidateToTariff();
			AssertNoWarning(QueryTariff.ToTariffInfo, QueryTariff.RangeMayExceed100Tariffs);
		}

		public void TestToTariffMaxNumberValidationWithDateAndDistinct()
		{
			var queryTariff = new QueryTariffForTest();
			queryTariff.FromTariff = "2203000030";
			queryTariff.ToTariff = "2203000090";
			queryTariff.AsOfDate = new ZDateTime(2000, 1, 1);
			queryTariff.ValidateToTariff();
			AssertHasWarning(queryTariff.ToTariffInfo, QueryTariff.RangeMayExceed100Tariffs);

			queryTariff.AsOfDate = ZDateTime.Empty;
			queryTariff.ToTariff = "2203000060";
			queryTariff.ValidateToTariff();
			AssertNoWarning(queryTariff.ToTariffInfo, QueryTariff.RangeMayExceed100Tariffs);
		}

		public void TestAsOfDate()
		{
			var testDate = new ZDateTime(2009, 02, 18);
			QueryTariff.AsOfDate = testDate;
			AssertEquals(testDate, QueryTariff.AsOfDate);
		}

		protected override BusinessObject GetNewBusinessObject() => new QueryTariff();

		QueryTariff queryTariff;
		QueryTariff QueryTariff => queryTariff ?? (queryTariff = new QueryTariff());

		sealed class QueryTariffForTest : QueryTariff
		{
			protected override int MaxNumberOfTariffs => 2;
		}
	}
}
