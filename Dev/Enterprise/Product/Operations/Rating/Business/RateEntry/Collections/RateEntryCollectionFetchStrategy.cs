using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateEntryCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public RateEntryCollectionFetchStrategy(RateEntryCollection entryCollection)
			: base(entryCollection)
		{
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			foreach (RateEntry entry in Collection)
			{
				Collection.Factory.AddFetchHint(RateLinesSchema.TL_TI, entry.PK);
			}
		}

		new RateEntryCollection Collection
		{
			get { return (RateEntryCollection)base.Collection; }
		}
	}
}
