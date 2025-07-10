using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateCollection))]
	public class RateCollectionTest : RatingHeaderCollectionBaseTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RateCollection(Factory);
		}

		protected override Type GetExpectedFindBoxListProviderType()
		{
			return typeof(RatingHeaderFindBoxListProviderForOrganisationCode);
		}
	}

	public class RateCollectionTestWithGlobalRates : RatingTestCase
	{
		public void TestGlobalRatesAreLoadedIntoCollection()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var globalRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());

			var collection = new RateCollection(Factory, GlbCompany.CurrentCompany);
			collection.Load();

			AssertContainsExactElementsInAnyOrder("Should load both rates", new[] { rate, globalRate }, collection);
		}
	}
}
