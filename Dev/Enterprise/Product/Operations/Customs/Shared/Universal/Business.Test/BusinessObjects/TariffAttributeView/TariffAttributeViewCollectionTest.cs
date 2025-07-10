using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffAttributeViewCollection))]
	class TariffAttributeViewCollectionTest : ActiveBusinessObjectCollectionTestCase<TariffAttributeViewCollection>
	{
		public void TestTariffNationalCodeRelatedData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var category1 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.Eritrea, "CT1");
			var category2 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.SouthAfrica, "CT2");
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.Eritrea, "1P1", ensureDataGroupingExists: false);
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var attribute1 = helper.CreateTariffAttribute("CT1", "C11", tariff1);
			var attribute2 = helper.CreateTariffAttribute("CT2", "C22", tariff1);
			var tariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "DUMMYTRF2", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			var tariff2attribute = helper.CreateTariffAttribute("CT1", "C11", tariff2);
			var tariffNationalCode = helper.CreateTariffNationalCode(Core.Constants.CountryCodes.SouthAfrica, tariff1.PK, "TNC", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), new ZDate(2010, 12, 9), "dummy Description 0", ensureDataGroupingExists: false);
			var attribute3 = helper.CreateTariffAttribute("BT1", "B11", tariffNationalCode);
			var attribute4 = helper.CreateTariffAttribute("BT2", "B22", tariffNationalCode);
			var attributes = tariffNationalCode.Attributes;
			AssertEquals(4, attributes.Count);
			AssertCollectionContains(attribute1, attributes);
			AssertCollectionContains(attribute2, attributes);
			AssertCollectionContains(attribute3, attributes);
			AssertCollectionContains(attribute4, attributes);
		}

		protected override TariffAttributeViewCollection GetCollectionToTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var s1p1TariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
			var cusTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			return new TariffAttributeViewCollection(cusTariff);
		}
	}
}
