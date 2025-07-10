using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(ConsolidationProfitShareCollection))]
	internal class ConsolidationProfitShareCollectionTest : BizObjectCollectionAddDeleteTestCase
	{
		protected override BusinessObjectCollection GetCollection()
		{
			var profitShareRedistribution = Factory.New<ProfitShareRedistribution>();
			profitShareRedistribution.ConsolProfitShares.AddNew();

			return new ConsolidationProfitShareCollection(profitShareRedistribution);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Collection.AddNew();
		}
	}
}
