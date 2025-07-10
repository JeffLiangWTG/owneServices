using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffViewCollection))]
	public class TariffViewCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetNewCollection_FilterBusinessObjectDefaults()
		{
			var collection = TariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.Eritrea, "IMP", new ZDateTime(2019, 3, 15), "TRF");
			var tariffCode = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.TariffCode + ":Property"];
			AssertEquals("tariffCode", ZString.Empty, tariffCode.Value);
			var effectiveDate = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.EffectiveDate + ":Property1"];
			AssertEquals("effectiveDate", new ZDateTime(2019, 3, 15), effectiveDate.Value);
			var tariffTypeCountry = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.TariffType + ":Property1"];
			AssertEquals("tariffTypeCountry", (ZString)Core.Constants.CountryCodes.Eritrea, tariffTypeCountry.Value);
			var tariffTypeType = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.TariffType + ":Property2"];
			AssertEquals("tariffTypeType", (ZString)"IMP", tariffTypeType.Value);
			var tariffRestriction = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.TariffRestriction + ":Property"];
			AssertEquals("tariffRestriction", (ZString)"TRF", tariffRestriction.Value);
			collection = TariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.Eritrea, ZString.Empty, ZDateTime.Invalid, ZString.Empty);
			tariffCode = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.TariffCode + ":Property"];
			AssertEquals("tariffCode", ZString.Empty, tariffCode.Value);
			AssertEquals("effectiveDate", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.EffectiveDate + ":Property1"));
			AssertEquals("tariffTypeCountry", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.TariffType + ":Property1"));
			AssertEquals("tariffTypeType", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.TariffType + ":Property2"));
			AssertEquals("tariffRestriction", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.TariffRestriction + ":Property"));
			collection = TariffViewCollection.GetNewCollection(Factory, Core.Constants.CountryCodes.Eritrea, ZString.Empty, ZDateTime.Invalid, ZString.Empty, false);
			AssertEquals("tariffCode", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.TariffCode + ":Property"));
		}

		public void TestGetCachedCollection_FilterBusinessObjectDefaults()
		{
			var collection = TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Eritrea, "IMP", new ZDateTime(2019, 3, 15));
			var tariffCode = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.TariffCode + ":Property"];
			AssertEquals("tariffCode", ZString.Empty, tariffCode.Value);
			var effectiveDate = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.EffectiveDate + ":Property1"];
			AssertEquals("effectiveDate", new ZDateTime(2019, 3, 15), effectiveDate.Value);
			var tariffTypeCountry = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.TariffType + ":Property1"];
			AssertEquals("tariffTypeCountry", (ZString)Core.Constants.CountryCodes.Eritrea, tariffTypeCountry.Value);
			var tariffTypeType = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.TariffType + ":Property2"];
			AssertEquals("tariffTypeType", (ZString)"IMP", tariffTypeType.Value);
			collection = TariffViewCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Eritrea, ZString.Empty, ZDateTime.Invalid);
			tariffCode = collection.FilterBusinessObjectDefaults[Constants.RefCusTariffFilters.TariffCode + ":Property"];
			AssertEquals("tariffCode", ZString.Empty, tariffCode.Value);
			AssertEquals("effectiveDate", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.EffectiveDate + ":Property1"));
			AssertEquals("tariffTypeCountry", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.TariffType + ":Property1"));
			AssertEquals("tariffTypeType", false, collection.FilterBusinessObjectDefaults.ContainsDefaultFor(Constants.RefCusTariffFilters.TariffType + ":Property2"));
		}

		public void TestLoadingTypeofStartWith()
		{
			var country1 = Factory.NewWithValidTestData<RefCountry>();
			var country2 = Factory.NewWithValidTestData<RefCountry>();
			country1.RN_Code = "ZX";
			country2.RN_Code = "ZY";
			Factory.Save();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var type1 = helper.CreateNewOrGetExistingTariffType(country1.Code, "TS11");
			var type2 = helper.CreateNewOrGetExistingTariffType(country1.Code, "TS22");
			var type3 = helper.CreateNewOrGetExistingTariffType(country1.Code, "TP00");
			var type4 = helper.CreateNewOrGetExistingTariffType(country2.Code, "TS11");
			var type5 = helper.CreateNewOrGetExistingTariffType(country2.Code, "TS22");
			Factory.Save();
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var exp1 = helper.CreateTariff(country1.Code, type1.PK, "11", startDate, endDate);
			var exp2 = helper.CreateTariff(country1.Code, type2.PK, "12", startDate, endDate);
			var exp3 = helper.CreateTariff(country1.Code, type3.PK, "13", startDate, endDate);
			var exp4 = helper.CreateManualTariff(country1.Code, "TS11&", "14", startDate, endDate);
			helper.CreateTariff(country2.Code, type4.PK, "15", startDate, endDate);
			helper.CreateTariff(country2.Code, type5.PK, "16", startDate, endDate);
			CombineAssertions(() =>
			{
				var tester = TariffViewCollection.GetNewCollection(Factory, country1.Code, "TS11", ZDateTime.Today, ZString.Empty);
				tester.Load();
				AssertContainsExactElementsInAnyOrder(new[] { exp1, exp4 }, tester);
				tester = TariffViewCollection.GetNewCollection(Factory, country1.Code, "TS1", ZDateTime.Today, ZString.Empty);
				tester.Load();
				AssertContainsExactElementsInAnyOrder(new[] { exp1, exp4 }, tester);
				tester = TariffViewCollection.GetNewCollection(Factory, country1.Code, "TS", ZDateTime.Today, ZString.Empty);
				tester.Load();
				AssertContainsExactElementsInAnyOrder(new[] { exp1, exp2, exp4 }, tester);
				tester = TariffViewCollection.GetNewCollection(Factory, country1.Code, "T", ZDateTime.Today, ZString.Empty);
				tester.Load();
				AssertContainsExactElementsInAnyOrder(new[] { exp1, exp2, exp3, exp4 }, tester);
			}

			);
		}

		public void TestLoadingForMultipleTypes()
		{
			var country1 = Factory.NewWithValidTestData<RefCountry>();
			country1.RN_Code = "ZX";
			Factory.Save();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var type1 = helper.CreateNewOrGetExistingTariffType(country1.Code, "Tp11");
			var type2 = helper.CreateNewOrGetExistingTariffType(country1.Code, "Tp22");
			var type3 = helper.CreateNewOrGetExistingTariffType(country1.Code, "Tp33");
			Factory.Save();
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var tariff1 = helper.CreateTariff(country1.Code, type1.PK, "11", startDate, endDate);
			var tariff2 = helper.CreateTariff(country1.Code, type2.PK, "12", startDate, endDate);
			var tariff3 = helper.CreateTariff(country1.Code, type3.PK, "13", startDate, endDate);
			var tariff4 = helper.CreateManualTariff(country1.Code, "Tp44&", "14", startDate, endDate);
			var result = new TariffViewCollection(Factory, country1.RN_Code, ZDateTime.Now, new ZString[] { type1.ZZI_TariffType, type3.ZZI_TariffType, "Tp4", ZString.Empty });
			result.Load();
			AssertContainsExactElementsInAnyOrder(new[] { tariff1, tariff3, tariff4 }, result);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TariffViewCollection(Factory, new ZQuery(TariffViewSchema.ZZ1_ZZZ_NKDataGrouping, string.Empty));
		}
	}
}
