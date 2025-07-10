using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class ActiveRateEntryCollection : ActiveBusinessObjectCollection<RateEntry>
	{
		public ActiveRateEntryCollection(RatingHeader parent)
			: base(parent.Factory, parent, null, RateEntrySchema.TI_TH)
		{
		}
	}

	public class ActiveQuoteEntryCollection : ActiveBusinessObjectCollection<QuoteEntry>
	{
		public ActiveQuoteEntryCollection(RatingHeader parent)
			: base(parent.Factory, parent, null, RateEntrySchema.TI_TH)
		{
		}
	}
}

