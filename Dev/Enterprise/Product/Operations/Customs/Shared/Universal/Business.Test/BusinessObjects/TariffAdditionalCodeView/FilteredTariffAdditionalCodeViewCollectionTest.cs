using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(FilteredTariffAdditionalCodeViewCollection))]
	class FilteredTariffAdditionalCodeViewCollectionTest : ActiveBusinessObjectCollectionTestCase<FilteredTariffAdditionalCodeViewCollection>
	{
		public void TestEnableEffectiveData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var er = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping("DG1", parent: er);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			Factory.Save();
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.Eritrea, "1P1", ensureDataGroupingExists: false);
			var rateType = UniversalReferenceTestDataHelper.CreateCusRateType(Factory, Core.Constants.CountryCodes.Eritrea, Constants.RateTypes.Duty, "Duty", ensureDataGroupingExists: false);
			var rateCode = helper.CreateCusRateCode(Factory, "RC1", rateType.PK);
			var category1 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.Eritrea, "CT1");
			var category2 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.SouthAfrica, "CT2");
			var category3 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, "DG1", "CT2");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			var additionalCode1 = helper.CreateTariffAdditionalCodeView(tariff, "CT1", "C11", ensureDataGroupingExists: false, ensureCategoryExists: false);
			var additionalCode2 = helper.CreateTariffAdditionalCodeView(tariff, "CT2", "C22", dataGrouping: Core.Constants.CountryCodes.SouthAfrica, ensureDataGroupingExists: false, ensureCategoryExists: false);
			var additionalCode3 = helper.CreateTariffAdditionalCodeView(tariff, "CT2", "C33", dataGrouping: "DG1", ensureDataGroupingExists: false, ensureCategoryExists: false);
			Factory.Save();
			var f = new BusinessObjectFactory();
			var tariffReloaded = f.Load<TariffView>(tariff.PK);
			var wrapper = tariffReloaded.Wrapper;
			wrapper.EffectiveDataGrouping = ZString.Empty;
			wrapper.EffectiveDate = ZDate.Empty;
			var filteredCollection = new FilteredTariffAdditionalCodeViewCollection(tariffReloaded, true);
			var collection = new TariffAdditionalCodeViewCollection(tariffReloaded);
			AssertEquals("filteredCollection", 2, filteredCollection.Count);
			AssertNotNull("filteredCollection.additionalCode1", filteredCollection.FindByPK(additionalCode1.PK));
			AssertNotNull("filteredCollection.additionalCode2", filteredCollection.FindByPK(additionalCode3.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.additionalCode1", collection.FindByPK(additionalCode1.PK));
			AssertNotNull("collection.additionalCode2", collection.FindByPK(additionalCode2.PK));
			AssertNotNull("collection.additionalCode3", collection.FindByPK(additionalCode3.PK));
			wrapper.EffectiveDataGrouping = Core.Constants.CountryCodes.Eritrea;
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.additionalCode1", filteredCollection.FindByPK(additionalCode1.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.additionalCode1", collection.FindByPK(additionalCode1.PK));
			AssertNotNull("collection.additionalCode2", collection.FindByPK(additionalCode2.PK));
			AssertNotNull("collection.additionalCode3", collection.FindByPK(additionalCode3.PK));
			wrapper.EffectiveDataGrouping = "DG1";
			AssertEquals("filteredCollection", 1, filteredCollection.Count);
			AssertNotNull("filteredCollection.additionalCode3", filteredCollection.FindByPK(additionalCode3.PK));
			AssertEquals("collection", 3, collection.Count);
			AssertNotNull("collection.additionalCode1", collection.FindByPK(additionalCode1.PK));
			AssertNotNull("collection.additionalCode2", collection.FindByPK(additionalCode2.PK));
			AssertNotNull("collection.additionalCode3", collection.FindByPK(additionalCode3.PK));
		}

		protected override FilteredTariffAdditionalCodeViewCollection GetCollectionToTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			Factory.Save();
			var cusTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			return new FilteredTariffAdditionalCodeViewCollection(cusTariff, true);
		}
	}
}
