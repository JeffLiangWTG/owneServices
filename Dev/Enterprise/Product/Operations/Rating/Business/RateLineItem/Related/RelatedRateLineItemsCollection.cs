namespace Enterprise.Rating.Business
{
	public class RelatedRateLineItemsCollection : RateLineItemsCollection
	{
		public RelatedRateLineItemsCollection(RelatedRateLine master)
			: base(master)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public new RelatedRateLineItem this[int index]
		{
			get { return (RelatedRateLineItem)Elements[index]; }
		}

		public virtual new RelatedRateLineItem AddNew()
		{
			return (RelatedRateLineItem)base.AddNew();
		}
	}
}

