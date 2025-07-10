using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(FilteredTariffRelationshipViewCollection))]
	public class FilteredTariffRelationshipViewCollectionTest : ActiveBusinessObjectCollectionTestCase<FilteredTariffRelationshipViewCollection>
	{
		protected override FilteredTariffRelationshipViewCollection GetCollectionToTest()
		{
			s1p1TariffType = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
			return new FilteredTariffRelationshipViewCollection(CusTariff);
		}

		public void TestNewChildTariffRelationshipCollection()
		{
			var startDate = new ZDateTime(1900, 01, 01);
			var midDate = new ZDateTime(2022, 04, 01);
			var endDate = new ZDateTime(2050, 01, 01);
			var tariffType1P1 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1#");
			var tariffType12A = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A#");
			Factory.Save();
			var tariff1P1 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1P1##", startDate, endDate, description: "Parent");
			var tariff12A_1 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "12A##1", startDate, midDate, description: "Old Child");
			var tariff12A_2 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "12A##2", midDate, endDate, description: "New Child");
			var rel1 = Helper.CreateTariffRelationship(tariff12A_1.PK, tariff1P1.ZZ1_ZZI_TariffType, "1P1");
			var rel2 = Helper.CreateTariffRelationship(tariff12A_2.PK, tariff1P1.ZZ1_ZZI_TariffType, "1P1");
			var loaded1P1 = Factory.Load<TariffView>(tariff1P1.PK);
			AssertEquals("Count should be 2", 2, loaded1P1.ChildTariffs.Count);

			var wrapper = loaded1P1.Wrapper;
			wrapper.EffectiveDate = ZDate.Empty;

			var collection = FilteredTariffRelationshipViewCollection.NewChildTariffRelationshipCollection(loaded1P1);

			AssertEquals("Filtered should be 2", 2, collection.Count);

			wrapper.EffectiveDate = new ZDate(2022, 01, 01);
			AssertEquals("filteredCollection", 1, collection.Count);
			AssertNotNull("filteredCollection.12A_1", collection.FindByPK(rel1.PK));
			wrapper.EffectiveDate = new ZDate(2022, 04, 05);
			AssertEquals("filteredCollection", 1, collection.Count);
			AssertNotNull("filteredCollection.12A_2", collection.FindByPK(rel2.PK));
		}

		protected override void SetUp()
		{
			base.SetUp();
			s1p1TariffType = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
		}

		RefCusTariffType s1p1TariffType;
		TariffView CusTariff => cusTariff ?? (cusTariff = Helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0"));
		TariffView cusTariff;
		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
	}
}
