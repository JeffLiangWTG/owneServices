using System.Collections.Generic;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	internal class WhsAdjustmentRatingAdaptersProvider : RatingAdaptersProvider<WhsAdjustment>
	{
		public WhsAdjustmentRatingAdaptersProvider(WhsAdjustment docket) : base(docket) { }

		protected override List<IAutoRating> GetAdapters(WhsAdjustment parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			return new List<IAutoRating> { new WhsAdjustmentRatingAdapter(parent) };
		}
	}
}
