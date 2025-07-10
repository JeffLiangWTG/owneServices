using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Testing
{
	[TestedType(typeof(ShipmentProfitSharesCollection))]
	public class ShipmentProfitSharesCollectionTest : BizObjectCollectionAddDeleteTestCase
	{
		protected override BusinessObjectCollection GetCollection()
		{
			var profitShareRedistribution = Factory.New<ProfitShareRedistribution>();
			var consolidationProfit = profitShareRedistribution.ConsolProfitShares.AddNew();
			consolidationProfit.ShipmentProfitShares.AddNew();

			return new ShipmentProfitSharesCollection(consolidationProfit);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Collection.AddNew();
		}
	}
}
