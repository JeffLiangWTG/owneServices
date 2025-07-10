using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(CostingCollection))]
	public class CostingCollectionTest : RatingHeaderCollectionBaseTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CostingCollection(Factory);
		}

		protected override Type GetExpectedFindBoxListProviderType()
		{
			return typeof(RatingHeaderFindBoxListProviderForOrganisationCode);
		}
	}

	public class CostingCollectionTestWithGlobalCosts : RatingTestCase
	{
		public void TestGlobalCostsAreLoadedIntoCollection()
		{
			var cost = Helper.NewCosting(Helper.NewOrgHeader());
			var globalCost = Helper.NewGlobalCosting(Helper.NewOrgHeader());

			var collection = new CostingCollection(Factory, GlbCompany.CurrentCompany);
			collection.Load();

			AssertContainsExactElementsInAnyOrder("Should load both costs", new[] { cost, globalCost }, collection);
		}
	}
}
