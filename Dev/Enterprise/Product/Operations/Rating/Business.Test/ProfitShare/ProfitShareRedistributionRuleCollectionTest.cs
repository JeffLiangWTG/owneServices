using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(ProfitShareRedistributionRuleCollection))]
	internal class ProfitShareRedistributionRuleCollectionTest : BizObjectCollectionAddDeleteTestCase
	{
		protected override BusinessObjectCollection GetCollection()
		{
			var profitShareRedistribution = Factory.New<ProfitShareRedistribution>();
			profitShareRedistribution.ProfitShareRules.AddNew();

			return new ProfitShareRedistributionRuleCollection(profitShareRedistribution);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Collection.AddNew();
		}
	}
}
