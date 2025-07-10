using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffRelationshipViewCollection))]
	internal class TariffRelationshipViewCollectionTest : ActiveBusinessObjectCollectionTestCase<TariffRelationshipViewCollection>
	{
		protected override TariffRelationshipViewCollection GetCollectionToTest()
		{
			s1p1TariffType = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
			return new TariffRelationshipViewCollection(CusTariff);
		}

		public void TestNewChildTariffRelationshipCollection()
		{
			var startDate = new ZDateTime(1900, 01, 01);
			var endDate = new ZDateTime(2050, 01, 01);
			var tariffType1P1 = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1#");
			var tariffType12A = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A#");
			Factory.Save();
			var tariff1P1 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "1P1##", startDate, endDate, description: "long desc");
			var tariff12A = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType12A.PK, "12A##", startDate, endDate, description: "longer desc");
			Helper.CreateTariffRelationship(tariff12A.PK, tariff1P1.ZZ1_ZZI_TariffType, "1P1");
			Factory.Save();
			var tariffView = Factory.Load<TariffView>(tariff1P1.PK);

			var collection = TariffRelationshipViewCollection.NewChildTariffRelationshipCollection(tariffView);
			AssertEquals("NewChildTariffRelationshipCollection() - Count", 1, collection.Count);
			AssertEquals("NewChildTariffRelationshipCollection() - Tariff", tariff12A.PK, collection[0].ZZH_ZZ1_LinkedTariffOrNationalCode);
		}

		public void TestExcludeGeneralTariffs()
		{
			var startDate = new ZDateTime(1900, 01, 01);
			var endDate = new ZDateTime(2050, 01, 01);
			var dataGroupingCode = EnvProxy.Instance.CurrentCompany.Country.Code;
			var tariffType1P1 = Helper.CreateNewOrGetExistingTariffType(dataGroupingCode, "1P1#");
			var tariffType12A = Helper.CreateNewOrGetExistingTariffType(dataGroupingCode, "12A#");
			var tariffTypeGen = Helper.CreateNewOrGetExistingTariffType(dataGroupingCode, "5#");
			Factory.Save();
			var tariff1P1 = Helper.CreateTariff(dataGroupingCode, tariffType1P1.PK, "1P1##", startDate, endDate, description: "long desc");
			var tariff12A = Helper.CreateTariff(dataGroupingCode, tariffType12A.PK, "12A##", startDate, endDate, description: "longer desc");
			var relationship = Helper.CreateTariffRelationship(tariff12A.PK, tariff1P1.ZZ1_ZZI_TariffType, "1P1");
			var tariffGeneral = Helper.CreateTariff(dataGroupingCode, tariffTypeGen.PK, "5A##", startDate, endDate, description: "longer desc");
			var relationshipGen = Helper.CreateTariffRelationship(tariffGeneral.PK, tariff1P1.ZZ1_ZZI_TariffType, "");
			Factory.Save();
			var tariffView = Factory.Load<TariffView>(tariff1P1.PK);
			var collection = TariffRelationshipViewCollection.NewChildTariffRelationshipCollection(tariffView);
			AssertEquals("NewChildTariffRelationshipCollection() - Count", 2, collection.Count);
			AssertEquals("NewChildTariffRelationshipCollection() - Tariff", tariff12A.PK, collection[0].ZZH_ZZ1_LinkedTariffOrNationalCode);
			AssertEquals("NewChildTariffRelationshipCollection() - General Tariff", tariffGeneral.PK, collection[1].ZZH_ZZ1_LinkedTariffOrNationalCode);
			collection.ExcludeGeneralTariffs = true;
			AssertEquals("NewChildTariffRelationshipCollection() - Count", 1, collection.Count);
			AssertEquals("NewChildTariffRelationshipCollection() - Tariff", tariff12A.PK, collection[0].ZZH_ZZ1_LinkedTariffOrNationalCode);
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
