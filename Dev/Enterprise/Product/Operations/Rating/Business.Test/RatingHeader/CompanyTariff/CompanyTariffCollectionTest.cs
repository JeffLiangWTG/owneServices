using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CompanyTariffCollection))]
	public class CompanyTariffCollectionTest : RatingHeaderCollectionBaseTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CompanyTariffCollection(Factory);
		}

		protected override Type GetExpectedFindBoxListProviderType()
		{
			return typeof(RatingHeaderFindBoxListProviderForRateLevel);
		}
	}

	public class CompanyTariffCollectionWithGlobalTariffsTest : RatingTestCase
	{
		public void TestGlobalTariffsAreLoadedIntoCollection()
		{
			var companyTariff = Helper.NewCompanyTariff();
			var globalTariff = Helper.NewGlobalTariff();
			companyTariff.Factory.Save();
			globalTariff.Factory.Save();

			var collection = new CompanyTariffCollection(Factory, GlbCompany.CurrentCompany);
			collection.Load();

			AssertContainsExactElementsInAnyOrder("Should load both tariffs", new[] { companyTariff.PK, globalTariff.PK }, collection.Select(x => x.PK));
		}
	}
}
