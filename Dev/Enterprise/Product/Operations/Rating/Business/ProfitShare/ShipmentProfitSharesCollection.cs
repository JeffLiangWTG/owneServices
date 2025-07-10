using CargoWise.EntityFramework;

namespace Enterprise.Rating.Business
{
	public class ShipmentProfitSharesCollection : DependentBusinessObjectCollection<ShipmentProfitShares, ConsolidationProfitShare>
	{
		public ShipmentProfitSharesCollection(ConsolidationProfitShare parentConsol)
			: base(parentConsol)
		{
		}
	}
}
