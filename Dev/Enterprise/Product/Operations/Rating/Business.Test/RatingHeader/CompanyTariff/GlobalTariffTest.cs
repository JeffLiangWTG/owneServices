using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class GlobalTariffTest : RatingTestCase
	{
		public void TestDescriptionAndLevel()
		{
			var globalTariffLevel1 = Factory.New<GlobalTariff>();
			var globalTariffLevel2 = Factory.New<GlobalTariff>();
			var globalTariffLevel3 = Factory.New<GlobalTariff>();

			var tariffLevel1 = Factory.New<CompanyTariff>();
			var tariffLevel2 = Factory.New<CompanyTariff>();

			AssertEquals("Should have proper description", "Global Base Tariff", globalTariffLevel1.TH_GlobalRateDescription);
			AssertEquals("Should have level 1", (byte)1, globalTariffLevel1.TH_GlobalRateLevel);

			AssertEquals("Should have proper description", "Global Tariff Level 2", globalTariffLevel2.TH_GlobalRateDescription);
			AssertEquals("Should have proper description", "Global Tariff Level 3", globalTariffLevel3.TH_GlobalRateDescription);
			AssertEquals("Should have level 2", (byte)2, globalTariffLevel2.TH_GlobalRateLevel);
			AssertEquals("Should have level 3", (byte)3, globalTariffLevel3.TH_GlobalRateLevel);

			Assert("Should be level 1 for global tariffs", globalTariffLevel1.IsLevelOneTariff());
			Assert("Should be additional global tariff", globalTariffLevel2.IsAdditionalTariff());
			Assert("Should be additional global tariff", globalTariffLevel3.IsAdditionalTariff());

			AssertEquals("Should have proper description", "Base Company Tariff", tariffLevel1.TH_GlobalRateDescription);
			AssertEquals("Should have level 1", (byte)1, tariffLevel1.TH_GlobalRateLevel);

			AssertEquals("Should have proper description", "Company Tariff Level 2", tariffLevel2.TH_GlobalRateDescription);
			AssertEquals("Should have level 2", (byte)2, tariffLevel2.TH_GlobalRateLevel);

			AssertEquals("Should have correct level 1 reference", globalTariffLevel1.PK, globalTariffLevel2.LevelOneTariff.PK);
			AssertEquals("Should have correct level 1 reference", tariffLevel1.PK, tariffLevel2.LevelOneTariff.PK);
		}
	}

	[TestedType(typeof(GlobalTariff))]
	public class GlobalTariffBizObjTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			return factory.LoadTop1<Costing>(new ZQuery());
		}
	}
}
