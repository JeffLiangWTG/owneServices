using System;

namespace Enterprise.Warehouse.Cartonisation.Integration
{
	public class ContentResult : IContentResult
	{
		public ContentResult(Guid pk, decimal qty)
		{
			cartonisableItemPK = pk;
			this.qty = qty;
		}

		readonly Guid cartonisableItemPK;
		readonly decimal qty;

		public Guid CartonisableItemPK
		{
			get { return cartonisableItemPK; }
		}

		public decimal Quantity
		{
			get { return qty; }
		}
	}
}
