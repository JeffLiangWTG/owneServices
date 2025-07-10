using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffUOMViewCollection))]
	public class TariffUOMViewCollectionTest : ActiveBusinessObjectCollectionTestCase<TariffUOMViewCollection>
	{
		public void TestConstructor_SingleParent()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var s1p1TariffType = helper.CreateTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1", nomenclatureGroupType: "ZA", ensureDataGroupingExists: false);
			Factory.Save();
			var parentTariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1), "Alpha Bravo", compositeKey: "99...99.99");
			helper.CreateTariffUOM(parentTariff, Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM", Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateTariffUOM(parentTariff, Constants.UnitOfMeasureTypes.AdditionalUOMType, "LTR", Core.Constants.CountryCodes.SouthAfrica);
			AssertContainsExactElementsInAnyOrder(new[] { "KGM", "LTR" }, new TariffUOMViewCollection(parentTariff).Select(x => x.ZZ8_UOM));
		}

		public void TestConstructor_MultipleParent()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var s1p1TariffType = helper.CreateTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1", nomenclatureGroupType: "ZA", ensureDataGroupingExists: false);
			Factory.Save();
			var parentTariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1), "Alpha Bravo", compositeKey: "99...99.99");
			var parentTariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "88888888", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1), "tariff 2", compositeKey: "88...88.88");
			parentTariff1.ZZ1_ZZ1_Tariff = parentTariff2.PK;
			parentTariff1.ZZ1_TableType = RefCusTariffNationalCodeSchema.Constants.Prefix;
			helper.CreateTariffUOM(parentTariff1, Constants.UnitOfMeasureTypes.StatisticalUOMType, "KGM", Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateTariffUOM(parentTariff1, Constants.UnitOfMeasureTypes.AdditionalUOMType, "LTR", Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateTariffUOM(parentTariff2, Constants.UnitOfMeasureTypes.StatisticalUOMType, "BG", Core.Constants.CountryCodes.SouthAfrica);
			AssertContainsExactElementsInAnyOrder(new[] { "BG", "KGM", "LTR" }, new TariffUOMViewCollection(parentTariff1).Select(x => x.ZZ8_UOM));
		}

		protected override TariffUOMViewCollection GetCollectionToTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var s1p1TariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
			var cusTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			return new TariffUOMViewCollection(cusTariff);
		}
	}
}
