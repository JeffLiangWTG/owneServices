using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	internal class WhsVASOrderRatingAdaptersProvider : RatingAdaptersProvider<WhsVASOrder>
	{
		public WhsVASOrderRatingAdaptersProvider(WhsVASOrder vasOrder)
			: base(vasOrder)
		{
		}

		protected override List<IAutoRating> GetAdapters(WhsVASOrder parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new WhsVASOrderRatingAdapter(parent) };
		}
	}
}