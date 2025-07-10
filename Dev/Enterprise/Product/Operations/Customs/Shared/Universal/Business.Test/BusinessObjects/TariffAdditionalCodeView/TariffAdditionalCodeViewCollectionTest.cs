using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffAdditionalCodeViewCollection))]
	class TariffAdditionalCodeViewCollectionTest : ActiveBusinessObjectCollectionTestCase<TariffAdditionalCodeViewCollection>
	{
		public void TestTariffNationalCodeRelatedData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var category1 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.Eritrea, "CT1");
			var category2 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.SouthAfrica, "CT2");
			var category3 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.Eritrea, "BT1");
			var category4 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.Eritrea, "BT2");
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.Eritrea, "1P1", ensureDataGroupingExists: false);
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var additionalCode1 = helper.CreateTariffAdditionalCodeView(tariff1, "CT1", "C11", ensureDataGroupingExists: false, ensureCategoryExists: false);
			var additionalCode2 = helper.CreateTariffAdditionalCodeView(tariff1, "CT2", "C22", dataGrouping: Core.Constants.CountryCodes.SouthAfrica, ensureDataGroupingExists: false, ensureCategoryExists: false);
			Factory.Save();
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF2", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var tariff2additionalCode = helper.CreateTariffAdditionalCodeView(tariff2, "CT1", "C11", ensureDataGroupingExists: false, ensureCategoryExists: false);
			var tariffNationalCode = helper.CreateTariffNationalCode(Core.Constants.CountryCodes.Eritrea, tariff1.PK, "TNC", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), new ZDate(2010, 12, 9), "dummy Description 0", ensureDataGroupingExists: false);
			var additionalCode3 = helper.CreateTariffAdditionalCodeView(tariffNationalCode, "BT1", "B11", ensureDataGroupingExists: false, ensureCategoryExists: false);
			var additionalCode4 = helper.CreateTariffAdditionalCodeView(tariffNationalCode, "BT2", "B22", ensureDataGroupingExists: false, ensureCategoryExists: false);
			var additionalCodes = new TariffAdditionalCodeViewCollection(tariffNationalCode);
			AssertEquals(4, additionalCodes.Count);
			AssertCollectionContains(additionalCode1, additionalCodes);
			AssertCollectionContains(additionalCode2, additionalCodes);
			AssertCollectionContains(additionalCode3, additionalCodes);
			AssertCollectionContains(additionalCode4, additionalCodes);
		}

		protected override TariffAdditionalCodeViewCollection GetCollectionToTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			Factory.Save();
			var cusTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			return new TariffAdditionalCodeViewCollection(cusTariff);
		}
	}
}
